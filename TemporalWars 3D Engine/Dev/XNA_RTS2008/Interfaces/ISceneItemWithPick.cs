#region File Description
//-----------------------------------------------------------------------------
// ISceneItemWithPick.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// SceneItem is any SceneItemOwner that can be in a scenegraph
    /// </summary>
    public interface ISceneItemWithPick
    {
        /// <summary>
        /// Issues a ground attack order
        /// </summary>       
        void AttackGroundOrder();
        ///<summary>
        /// Is this <see cref="SceneItemWithPick"/> currently attacking some other <see cref="SceneItem"/>
        ///</summary>
        bool AttackOn { get; set; }
        /// <summary>
        /// Issues an attack order to the attackee which must be set in the <see cref="SceneItem.AttackSceneItem"/> 
        /// Property first.  If attackee is outside range of this <see cref="SceneItemWithPick"/>, then NO attacking will commence.
        /// </summary>
        void AttackOrder();
        /// <summary>
        /// The direction this <see cref="SceneItemWithPick"/> is facing, in radians. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value given is not with allowable range of -pi to pi.</exception>
        float FacingDirection { get; set; }
        /// <summary>
        /// A constant offset to apply to the <see cref="FacingDirection"/> to fix
        /// shapes which might not being facing in the desired direction when created.
        /// </summary>
        /// <remarks>This is useful when you need to fix the rotation of the artwork, but do not have access to the original file to fix outside the game.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value given is not with allowable range of -pi to pi.</exception>
        float FacingDirectionOffset { get; set; }
        /// <summary>
        /// Return the current <see cref="SceneItemWithPick"/> 3D position in screen cordinates.
        /// </summary>
        /// <param name="screenPos">(OUT) Screen position as <see cref="Point"/></param>
        void GetScreenPos(out Point screenPos);
        ///<summary>
        /// The overall goal position to get this <see cref="SceneItemWithPick"/> to, used
        /// in the <see cref="AStarItem"/> class.
        ///</summary>
        Vector3 GoalPosition { get; set; }
        /// <summary>
        /// Is this <see cref="SceneItem"/> alive?
        /// </summary>
        bool IsAlive { get; }
        ///<summary>
        /// This <see cref="SceneItemWithPick"/> current <see cref="ItemStates"/>
        ///</summary>
        ItemStates ItemState { get; set; }
        /// <summary>
        /// Controls how quickly this <see cref="SceneItemWithPick"/> can turn from side to side.
        /// </summary>
        float ItemTurnSpeed { get; set; }
        ///<summary>
        /// The current move-to position for this <see cref="SceneItemWithPick"/>, used 
        /// in the <see cref="AStarItem"/> class.
        ///</summary>
        Vector3 MoveToPosition { get; set; }
        //void OnCreateDevice();
        /// <summary>
        /// This <see cref="SceneItemWithPick"/> is pick selected?
        /// </summary>
        bool PickSelected { get; set; }
        /// <summary>
        /// This method just updates the <see cref="DefenseScene"/> items 'turret' rotation values.
        /// </summary>
        void Render();
        /// <summary>
        /// Selection box <see cref="Color"/>
        /// </summary>
        Color SelectionBoxColor { get; set; }
        /// <summary>
        /// Updates any values associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time"><see cref="TimeSpan"/> structure for time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed game sime since last call</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall); // 4/26/2010 - Add 4th param.   
        ///<summary>
        /// When set, the A* solution returned will be checked
        /// a 2nd time, and any redudant nodes (nodes on straight paths),
        /// will be removed.
        ///</summary>
        bool UseSmoothingOnPath { get; set; }
    }
}