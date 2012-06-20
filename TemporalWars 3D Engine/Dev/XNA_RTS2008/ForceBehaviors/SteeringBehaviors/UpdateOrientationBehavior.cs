#region File Description
//-----------------------------------------------------------------------------
// UpdateOrientationBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.Terrain;

namespace TWEngine.ForceBehaviors.SteeringBehaviors
{
    ///<summary>
    /// The <see cref="UpdateOrientationBehavior"/> class is used to orientate some <see cref="SceneItem"/> to match the ground's angle, like going
    /// up a steep incline.  This is accomplished by reading the <see cref="TerrainShape"/> 'Normal' angle at some given position.
    ///</summary>
    public sealed class UpdateOrientationBehavior : AbstractBehavior
    {
        // 11/14/2008 - Thread Lock for TerrainShape Ref
        private static object _terrainShapeRefThreadLock = new object();

        private bool _isOnHeightMap;
        // 2/3/2009 - HeightAdjustment variable
        private float _heightAdjustment;
        // 2/3/2009 - Aircraft's desired height above ground.
        private float _desiredAircraftHeight = 100;  
        
        // Ref to the SceneItemOwner's Orientation Matrix       
        private Matrix _orientation;
        private Matrix _oldOrientation; // 10/14/2009
        private Vector3 _oldPosition;
        private static readonly Vector3 Vector3Zero = Vector3.Zero; // 8/11/2009

        // 11/28/2009 - Used for the Animations.
        private float _totalGameTime;

        #region Properties

        ///<summary>
        /// Thread lock for <see cref="TerrainShape"/> access
        ///</summary>
        public static object TerrainShapeRefThreadLock
        {
            get
            {
                return _terrainShapeRefThreadLock;
            }
            set
            {
                _terrainShapeRefThreadLock = value;
            }
        }

        /// <summary>
        /// Use Aircraft update?
        /// </summary>
        public bool UseAircraftUpdate { get; set; }

        // 11/28/2009
        /// <summary>
        /// When set, the aircraft will hover slightly up and down in mid-air.
        /// </summary>
        public bool UseAircraftUpDownAnimation { get; set; }

        // 11/28/2009
        /// <summary>
        /// When setm the unit will rock its up axis slightly left, then right.
        /// </summary>
        public bool UseRockLeftRightAnimation { get; set; }

        ///<summary>
        /// Stores summation of current ground height with given <see cref="DesiredAircraftHeight"/> value.
        ///</summary>
        public float HeightAdjustment
        {
            get { return _heightAdjustment; }
            set { _heightAdjustment = value; }
        }

        ///<summary>
        /// Value to use to affect the current aircraft height above the ground.
        ///</summary>
        public float DesiredAircraftHeight
        {
            get { return _desiredAircraftHeight; }
            set { _desiredAircraftHeight = value; }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        public UpdateOrientationBehavior()
            : base((int)Enums.BehaviorsEnum.UpdateOrientation, 0.0f)
        {
            
        }

        /// <summary>
        /// Given a target, this AbstractBehavior returns a steering force which will 
        /// direct the <see cref="SceneItem"/> towards the target
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public override void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force)
        {
            // 5/20/2012 - Throw expection if not SceneItemWithPick.
            var sceneItemWithPick = (SceneItemWithPick)item;
            if (sceneItemWithPick == null)
            {
                throw new InvalidOperationException("TurnTurretAbstractBehavior can ONLY be used with SceneItemWithPick types.");
            }

            // Just set to Zero Force, since this AbstractBehavior only update orientation
            DoUpdate(this, sceneItemWithPick, ref elapsedTime, out force);
        }

        // 5/17/2010; 6/12/2010: Updated to BehaviorsTimeSpan.
        /// <summary>
        /// Helper method, for the Update method.
        /// </summary>
        /// <param name="updateOrientationBehavior">Instance of <see cref="UpdateOrientationBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        /// <param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        private static void DoUpdate(UpdateOrientationBehavior updateOrientationBehavior, SceneItemWithPick item, 
                                    ref BehaviorsTimeSpan elapsedTime, out Vector3 force)
        {
            force = Vector3Zero;
            
            // Rotate Orientation based on 'FacingDirection' angle.
            Matrix.CreateRotationY(item.FacingDirection, out updateOrientationBehavior._orientation);
            
            // 11/28/2009 - Update GameTime used in Animations
            if (updateOrientationBehavior.UseAircraftUpDownAnimation || updateOrientationBehavior.UseRockLeftRightAnimation)
                updateOrientationBehavior._totalGameTime += (float)elapsedTime.TotalSeconds;

            // 2/3/2009 - Call proper UpdateOrientation verison if ground/aircraft SceneItemOwner.
            if (updateOrientationBehavior.UseAircraftUpdate)
                UpdateOrientation_Air(updateOrientationBehavior, item);
            else
                UpdateOrientation_Ground(updateOrientationBehavior, item);

            // 10/14/2009 - Update 'oldOrientation'.
            updateOrientationBehavior._oldOrientation = updateOrientationBehavior._orientation;
            
        }

        // 2/3/2009
        /// <summary>
        /// Updates the orienation of ground <see cref="SceneItem"/>, using the HeightMaps 'Normals', to position
        /// the <see cref="SceneItem"/> at the right angles.
        /// </summary>
        /// <param name="updateOrientationBehavior">Instance of <see cref="UpdateOrientationBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        private static void UpdateOrientation_Ground(UpdateOrientationBehavior updateOrientationBehavior, SceneItemWithPick item)
        {
            var newPosition = item.Position;      
     
            // 10/14/2009: Updated to include the Orientation check too; otherwise, rotations in place will not show up.
            // 7/7/2009 - Optimization - If 'Newposition' is same as 'OldPosition', then no orientation updating is necessary!
            if (updateOrientationBehavior._oldPosition.Equals(newPosition) && updateOrientationBehavior._oldOrientation.Equals(updateOrientationBehavior._orientation))
                return;

            // 5/18/2010: Updated to use overload version#2 of IsOnHeightMap method call.
            updateOrientationBehavior._isOnHeightMap = TerrainData.IsOnHeightmap(ref newPosition);

            if (updateOrientationBehavior._isOnHeightMap)
            {
                // now that we know we're on the heightmap, we need to know the correct
                // height and normal at this Position.             
                newPosition.Y = TerrainData.GetTerrainHeight(newPosition.X, newPosition.Z); // +heightAdjustment;

                // 5/18/2010: Updated to use overload version#2 of GetNormal method call.
                Vector3 normal;
                TerrainData.GetNormal(ref newPosition, out normal);

                // As discussed in the doc, we'll use the normal of the heightmap
                // and our desired forward direction to recalculate our orientation
                // matrix. It's important to normalize, as well.
                updateOrientationBehavior._orientation.Up = normal;

                // 7/16/2009 - If calculation successful, then store orientation back into SceneItemOwner.
                if (CalculateMatrixOrientation(updateOrientationBehavior, item))
                {
                    // Store Orientation Matrix back to ItemShape
                    if (item.ShapeItem != null) // 2/15/2009
                        item.ShapeItem.Orientation = updateOrientationBehavior._orientation;
                }

            } // End Is On Heightmap
           

            // Store Position with Height Adj
            item.Position = newPosition;

            // 7/7/2009 - Optimization use.
            updateOrientationBehavior._oldPosition = newPosition;
        }      

       
       
        // 2/3/2009; 11/28/2009: Updated with 'ElapsedTime' param.
        /// <summary>
        /// Updates the orienation of Aircraft items.
        /// </summary>
        /// <param name="updateOrientationBehavior">Instance of <see cref="UpdateOrientationBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        private static void UpdateOrientation_Air(UpdateOrientationBehavior updateOrientationBehavior, SceneItemWithPick item)
        {
            var newPosition = item.Position;

            // 5/18/2010: Updated to use overload version#2 of IsOnHeightMap method call.
            updateOrientationBehavior._isOnHeightMap = TerrainData.IsOnHeightmap(ref newPosition);

            if (updateOrientationBehavior._isOnHeightMap)
            {
                // 11/28/2009 - Animation: Adjusting the desiredHeight up & down.
                if (updateOrientationBehavior.UseAircraftUpDownAnimation && item.AStarItemI.ItemState == ItemStates.Resting)
                {
                    var heightAdjustment = (float)Math.Sin(updateOrientationBehavior._totalGameTime * 1.25f) * 0.333f;
                    updateOrientationBehavior._desiredAircraftHeight += heightAdjustment;
                }
               
                // With Aircraft, the height is adjusted to be at a specific spot, relative to ground movement.
                // 1st - get groundHeight
                var groundHeight = TerrainData.GetTerrainHeight(newPosition.X, newPosition.Z);
                // 2nd - calc max height adjustment using desired aircraft height
                updateOrientationBehavior._heightAdjustment = (groundHeight + updateOrientationBehavior._desiredAircraftHeight);

                // 3rd - adjust height slowly, using a controlled increase.
                if (Math.Abs(newPosition.Y - updateOrientationBehavior._heightAdjustment) > 0.5f)
                    if (newPosition.Y < updateOrientationBehavior._heightAdjustment)
                        newPosition.Y += 0.5f;
                    else if (newPosition.Y > updateOrientationBehavior._heightAdjustment)
                        newPosition.Y -= 0.5f;


                // 7/16/2009 - If calculation successful, then store orientation back into SceneItemOwner.
                if (CalculateMatrixOrientation(updateOrientationBehavior, item))
                {
                    // Store Orientation Matrix back to ItemShape
                    if (item.ShapeItem != null) // 2/15/2009
                        item.ShapeItem.Orientation = updateOrientationBehavior._orientation;
                }
                
            } // End Is On Heightmap
           

            // Store Position with Height Adj
            item.Position = newPosition;

            // 7/7/2009 - Optimization use.
            updateOrientationBehavior._oldPosition = newPosition;
        }

        // 7/16/2009
        /// <summary>
        /// Calculates the Matrix orientation
        /// </summary>
        /// <param name="updateOrientationBehavior">Instance of <see cref="UpdateOrientationBehavior"/>.</param>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        private static bool CalculateMatrixOrientation(UpdateOrientationBehavior updateOrientationBehavior, SceneItemWithPick item)
        {
            // 7/16/2009 - Use temp copy for calculations
            var tmpOrientation = updateOrientationBehavior._orientation;

            var inForward = tmpOrientation.Forward;
            var inUp = tmpOrientation.Up;
            
            // 11/28/2009 - Animation: Making the top move left and right slightly!
            if (updateOrientationBehavior.UseRockLeftRightAnimation)
            {
                if (item.AStarItemI.ItemState == ItemStates.Resting)
                {
                    var upAdjustment = (float)Math.Sin(updateOrientationBehavior._totalGameTime * 0.75f) * 0.333f;
                    Matrix angleAdj;
                    Matrix.CreateRotationZ(upAdjustment, out angleAdj);
                    Vector3.Transform(ref inUp, ref angleAdj, out inUp);
                    if (!inUp.Equals(Vector3Zero)) inUp.Normalize();
                } // End If Resting
                else
                    inUp = Vector3.Up;

            }// End if RockLeftRight

            //orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);           
            Vector3 inRight;
            Vector3.Cross(ref inForward, ref inUp, out inRight);
            //orientation.Right = Vector3.Normalize(orientation.Right);
            if (!inRight.Equals(Vector3Zero)) Vector3.Normalize(ref inRight, out inRight);  // 8/5/2009: Avoid NaN errors, by not normalizing Zero values!
            tmpOrientation.Right = inRight;
            //orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
            Vector3.Cross(ref inUp, ref inRight, out inForward);
            //orientation.Forward = Vector3.Normalize(orientation.Forward);
            if (!inForward.Equals(Vector3Zero)) Vector3.Normalize(ref inForward, out inForward); // 8/5/2009: Avoid NaN errors, by not normalizing Zero values!
            tmpOrientation.Forward = inForward;

#if DEBUG
            // 7/16/2009 - Now check if calculation failed, by locating any NaN values
            //             within any of the channels.
            var success = !float.IsNaN(tmpOrientation.M11) && !float.IsNaN(tmpOrientation.M12) && !float.IsNaN(tmpOrientation.M13) && !float.IsNaN(tmpOrientation.M14)
                           && !float.IsNaN(tmpOrientation.M21) && !float.IsNaN(tmpOrientation.M22) && !float.IsNaN(tmpOrientation.M23) && !float.IsNaN(tmpOrientation.M24)
                           && !float.IsNaN(tmpOrientation.M31) && !float.IsNaN(tmpOrientation.M32) && !float.IsNaN(tmpOrientation.M33) && !float.IsNaN(tmpOrientation.M34)
                           && !float.IsNaN(tmpOrientation.M41) && !float.IsNaN(tmpOrientation.M42) && !float.IsNaN(tmpOrientation.M43) && !float.IsNaN(tmpOrientation.M44);

            // 7/16/2009 - Copy value back, if successful!
            if (success)
                updateOrientationBehavior._orientation = tmpOrientation;
            else
                System.Diagnostics.Debugger.Break();

            return success;
#else
             updateOrientationBehavior._orientation = tmpOrientation;
            return true;
#endif
        }
       
        
    }
}
