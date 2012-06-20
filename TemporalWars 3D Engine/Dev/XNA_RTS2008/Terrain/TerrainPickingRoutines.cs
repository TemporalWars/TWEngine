#region File Description
//-----------------------------------------------------------------------------
// TerrainPickingRoutines.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using ScreenTextDisplayer.ScreenText;
using TWEngine.Common;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools;
using TWEngine.Utilities;
using TWEngine.Terrain.Structs;

namespace TWEngine.Terrain
{

    ///<summary>
    /// The <see cref="TerrainPickingRoutines"/> is used for cursor picking detection on the <see cref="TerrainQuadTree"/>, where each
    /// <see cref="TerrainQuadPatch"/> is checked for a pick, at the current cursor position.  Cursor picked hits are stored into the
    /// <see cref="PickTriangles"/> structure.
    ///</summary>
    public class TerrainPickingRoutines : DrawableGameComponent
    {
        // Ref to ITerrainShape Class
        private static TerrainShape _terrainShape;

        // 8/11/2008 - Writes Text to Screen
        private readonly ScreenTextItem _screenText;

        private TriangleShapeHelper _tShapeHelper;
        private static PickTriangles _pickedTriangle;
       
#if DEBUG
        // 5/18/2010 - Stores test triangles during picking for debuging.
        private static PickTriangles _testTriangles;
#endif

        // XNA 4.0 Updates
        private static readonly RasterizerState RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
        private static readonly DepthStencilState DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        #region Properties

        ///<summary>
        /// Returns the <see cref="PickTriangles"/> structure.
        ///</summary>
        public static PickTriangles PickedTriangle
        {
            get { return _pickedTriangle; }
        }
       
        /// <summary>
        /// Show Debug Values?
        /// </summary>
        private bool DebugValues { get; set; }

        /// <summary>
        /// Draw the testing picked triangles? 
        /// </summary>
        public static bool ShowDebugTestTriangles { get; set; }

        /// <summary>
        /// Draw the picked terrain triangles?
        /// </summary>
        public static bool DrawDebugPickedTriangles { get; set; }

        ///<summary>
        /// Show picked triangles?
        ///</summary>
        public bool IsVisible
        {
            set { Visible = value; }
        }

        #endregion   
   
       
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="terrainShape"><see cref="TerrainShape"/> instance</param>
        public TerrainPickingRoutines(ref Game game, TerrainShape terrainShape) : base(game)
        {
            // Save Ref to TerrainShape
            _terrainShape = terrainShape;

            // 8/13/2008
            _tShapeHelper = new TriangleShapeHelper(ref game);

            // 8/11/2008 - Add Ref of TerrainPicking class to Game Services.
            game.Services.AddService(typeof(TerrainPickingRoutines), this);

            // 8/11/2008 - Init ScreenText Class
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(550, 530), Color.Red, out _screenText);

            // 1/15/2009
            _pickedTriangle =  new PickTriangles(Vector3.Zero, Vector3.Zero, Vector3.Zero, Color.Magenta);

#if DEBUG
            // 5/18/2010
            _testTriangles = new PickTriangles(Vector3.Zero, Vector3.Zero, Vector3.Zero, Color.White);
            ShowDebugTestTriangles = false;
#endif

            DebugValues = false;

            // 9/3/2008 - Set DrawOrder
            DrawOrder = 105;
        }   

       

        /// <summary>
        /// Updates the internal picking calculations.
        /// </summary>
        /// <param name="gameTime"></param>
        public sealed override void Update(GameTime gameTime)
        {
            if (!Visible)
                return;

            // 8/13/2009 - ONLY do when in TerrainEdit mode; otherwise, this is called ONLY
            //             when needed in the 'GetCursorPosByPickedRay' method below.
#if EditMode
            
            // Check for Terrain Picking hits.
            UpdatePicking();
            
#endif

            base.Update(gameTime);
        }

        /// <summary>
        /// For debug purposes, allows drawing of the picked triangle, as magenta triangle on screen.
        /// </summary>
        /// <param name="gameTime"></param>             
        public sealed override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_Picking);
#endif

#if DEBUG
            // DEBUG: Draw Picked Triangle in Magenta
            if (TerrainEditRoutines.ToolInUse == ToolType.None)
                DrawPickedTriangle(Color.Magenta);
#endif

            if (DebugValues)
            {
// ReSharper disable RedundantToStringCall
                _screenText.DrawText ="Picked Quad: {0}" + _pickedTriangle.QuadInstanceKey.ToString();  // boxing using String.Format.              
// ReSharper restore RedundantToStringCall
            }

            base.Draw(gameTime);

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_Picking);
#endif
        }

#if DEBUG

        /// <summary>
        /// Shows Triangle currently 'Picked' by cursor as given <paramref name="colorToUse"/> triangle
        /// </summary>
        /// <param name="colorToUse"><see cref="Color"/> to draw triangle with</param>
        private static void DrawPickedTriangle(Color colorToUse)
        {
            // 6/2/2012 - Flag updated by the ScriptingAction method call.
            if (DrawDebugPickedTriangles)
            {
                // 5/18/2010 - Set triangle color
                _pickedTriangle.SetTriangleColor(ref colorToUse);
                // XNA 4.0 Updates - Final 2 params updated.
                TriangleShapeHelper.DrawPrimitiveTriangle(ref _pickedTriangle.Triangle, RasterizerState, DepthStencilState);
            }

            // 5/18/2010 - Draw Test triangles
            if (ShowDebugTestTriangles)
                // XNA 4.0 Updates - Final 2 params updated.
                TriangleShapeHelper.DrawPrimitiveTriangle(ref _testTriangles.Triangle, RasterizerState, DepthStencilState);
            
        }


#endif
        // 8/13/2009 - Updated to optimize memory.
        // 4/22/2008 - Returns the Cursor Position using the Picked Ray
        // 1/15/2009: Change to Static method to optimize memory.
        // 1/15/2009: Optimize by removing Ops Vector3 Overloads, which are slow on XBOX!
        ///<summary>
        /// Returns the <see cref="Cursor"/> position using the Picked <see cref="Ray"/>.
        ///</summary>
        ///<param name="convertTo"><see cref="PickRayScale"/> Enum conversion</param>
        ///<param name="cursorPos">(OUT) cursor position</param>
        public static void GetCursorPosByPickedRay(PickRayScale convertTo, out Vector3 cursorPos)
        {
            // 8/13/2009 - Cache
            const int scale = TerrainData.cScale;
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
            var pickedTriangle = PickedTriangle; // 8/13/2009

            // When in EditMode, the 'UpdatePicking' is already called in the 'Update' method above.
#if !EditMode
            // 8/13/2009 - Check for Terrain Picking hits.
            UpdatePicking();
#endif

            // 1/15/2009 - Optimize by removing Ops Vector3 Overloads, which are slow on XBOX!
            //Vector3 rayTarget = TerrainShape.PickedTriangle.rayPosition + TerrainShape.PickedTriangle.rayDirection * TerrainShape.PickedTriangle.rayDistance;
            var pickedRayDirection = pickedTriangle.RayDirection;
            var pickedRayPosition = pickedTriangle.RayPosition;
            Vector3 rayTarget, tmpResult;
            Vector3.Multiply(ref pickedRayDirection, pickedTriangle.RayDistance, out tmpResult);
            Vector3.Add(ref pickedRayPosition, ref tmpResult, out rayTarget);
            
            switch (convertTo)
            {
                case PickRayScale.NoChange:
                    cursorPos = new Vector3 {X = rayTarget.X, Y = rayTarget.Y, Z = rayTarget.Z};
                    return;
                case PickRayScale.DivideByTerrainScale:
                    cursorPos = new Vector3
                                    {
                                        X = rayTarget.X/scale,
                                        Y = rayTarget.Y,
                                        Z = rayTarget.Z/scale
                                    };
                    return;
                case PickRayScale.DivideByAStarPathScale:
                    cursorPos = new Vector3
                                    {
                                        X = (int) (rayTarget.X/pathNodeStride),
                                        Y = rayTarget.Y,
                                        Z = (int) (rayTarget.Z/pathNodeStride)
                                    };
                    return;
                default:
                    cursorPos = Vector3.Zero;
                    return;
            }            

        }

        // 6/11/2009: Optimized to now copy into the 'IndiciesPick' using the 'CopyTo' method, and NOT the 'ToArray' method.
        // 1/29/2009: Moved outside of Call, since 'New' waste memory!
        static int[] _indicesPick = new int[1];
        /// <summary>
        /// Runs a per-triangle picking algorithm over all the models in the scene,
        /// storing which triangle is currently under the Cursor.
        /// </summary>
        // 8/26/2008: Updated to optimize memory 
        // 1/15/2009: Change to Static method to optimize memory.
        private static void UpdatePicking()
        {
            // Look up a collision ray based on the current Cursor Position. 
            Ray cursorRay;
            Cursor.CalculateCursorRay(out cursorRay);           
            
            // Clear the previous picking results.
            _terrainShape.InsideBoundingSpheres.Clear();
          
            // 3/28/2008: Ben - Loop through Quad Terrain IndexBuffer Dictionary and check if 
            //            mouse Ray is in the Current Quad's BoundingBox; this is to only check
            //            the Quad the mouse Ray is actually in!
            int quadKey;

            var tmpWorldMatrix = _terrainShape.WorldP; 
            //Matrix inverseTransform;      
            //Matrix.Invert(ref tmpWorldMatrix, out inverseTransform);

            var ray = cursorRay;
            Vector3.Transform(ref ray.Position, ref tmpWorldMatrix, out ray.Position); // 5/18/2010: Was inverseTransform
            Vector3.TransformNormal(ref ray.Direction, ref tmpWorldMatrix, out ray.Direction); // 5/18/2010: Was inverseTransform

            // 6/11/20009
            var indicesPickCount = GetTerrainQuadTreeKeyInCameraFrustum(ref ray, out quadKey);

            // Check to make sure a QuadKey was found before doing any Pick Checks!  
            if (quadKey == -1) return;

            // Perform the ray to model intersection test. 
            bool insideBoundingSphere;
            tmpWorldMatrix = _terrainShape.WorldP;

            int arrayIndexPos1, arrayIndexPos2, arrayIndexPos3;
            Vector3 vertex1, vertex2, vertex3;

            var intersection = RayIntersectsModel(ref cursorRay, ref TerrainData.VertexBufferDataStream1,
                                                  ref _indicesPick, indicesPickCount, ref tmpWorldMatrix,
                                                  out insideBoundingSphere, out vertex1, out vertex2, out vertex3,
                                                  out arrayIndexPos1, out arrayIndexPos2, out arrayIndexPos3);

            // Do we have a per-triangle intersection with this model?
            if (intersection == null)
            {
                // 5/18/2010 - Then check if 2nd closest Quad has intersection?
                indicesPickCount = GetQuadIndices(_terrainQuad[1], out quadKey);

                // Check to make sure a QuadKey was found before doing any Pick Checks!  
                if (quadKey == -1) return;


                intersection = RayIntersectsModel(ref cursorRay, ref TerrainData.VertexBufferDataStream1,
                                                  ref _indicesPick, indicesPickCount, ref tmpWorldMatrix,
                                                  out insideBoundingSphere, out vertex1, out vertex2, out vertex3,
                                                  out arrayIndexPos1, out arrayIndexPos2, out arrayIndexPos3);

                // 5/18/2010 - Any intersection on 2nd try?
                if (intersection == null)
                {

                    //Debug.WriteLine("No... intersection (UpdatePicking)!");

                    return;
                }
            }

            // Store vertex positions so we can display the picked triangle.
            _pickedTriangle.Triangle[0].Position = vertex1;
            _pickedTriangle.Triangle[1].Position = vertex2;
            _pickedTriangle.Triangle[2].Position = vertex3;
            _pickedTriangle.RayPosition = cursorRay.Position;
            _pickedTriangle.RayDirection = cursorRay.Direction;
            _pickedTriangle.RayDistance = (float)intersection;
            _pickedTriangle.VertexArrayValue[0] = arrayIndexPos1;
            _pickedTriangle.VertexArrayValue[1] = arrayIndexPos2;
            _pickedTriangle.VertexArrayValue[2] = arrayIndexPos3;
            _pickedTriangle.QuadInstanceKey = quadKey;

            //Debug.WriteLine("YES... intersection (UpdatePicking)!");
        }

        // 5/18/2010 - Static Quads
        private static QuadToRayIntersection[] _terrainQuad = new QuadToRayIntersection[2];

        // 7/8/2009
        /// <summary>
        /// Searches the Terrain QuadTree, and returns the QuadTree 'Key' which
        /// the given 'Ray' intersects.  Also populates the 'IndicesPick' array,
        /// from the Quad's IndexBufferData.
        /// </summary>
        /// <param name="quadKey">QuadKey value if found</param>
        /// <param name="ray">Ray to check for intersect</param>
        /// <returns>IndexBufferData Count</returns>
        private static int GetTerrainQuadTreeKeyInCameraFrustum(ref Ray ray, out int quadKey)
        {
            var terrainQuadTree = TerrainShape.RootQuadTree; // 5/18/2010 - Cache

            // 5/18/2010 - Clear the 2 array positions
            _terrainQuad[0].Intersection = null;
            _terrainQuad[0].Quad = null;
            _terrainQuad[1].Intersection = null;
            _terrainQuad[1].Quad = null;

            // If key found, then get IndexBufferData from quad.
            TerrainQuadTree.GetQuadForGivenRayIntersectionInCameraFrustum(terrainQuadTree, ref ray, ref _terrainQuad);

            // 5/18/2010 - Closest Quad found, so get indices for this quad at index 0.
            var indicesPickCount = GetQuadIndices(_terrainQuad[0], out quadKey);

            return indicesPickCount;
        }

        // 5/18/2010
        /// <summary>
        /// Method helper, which retrieves the indices for the given Quad
        /// </summary>
        /// <param name="quadToRayIntersection"><see cref="QuadToRayIntersection"/> struct with closest quad to check</param>
        /// <param name="quadKey">(OUT) QuadKey value if found</param>
        /// <returns>IndexBufferData Count</returns>
        private static int GetQuadIndices(QuadToRayIntersection quadToRayIntersection, out int quadKey)
        {
            quadKey = -1;
            var indicesPickCount = 0;

            if (quadToRayIntersection.Intersection != null)
            {
                if (quadToRayIntersection.Quad != null)
                {
                    // Resize Array, if too small for given 'IndexBufferData'.
                    indicesPickCount = quadToRayIntersection.Quad.IndexBufferData.Count;
                    if (_indicesPick.Length < indicesPickCount)
                        Array.Resize(ref _indicesPick, indicesPickCount);

                    // Updated to use the 'CopyTo' method, rather than the 'ToArray' method, since
                    // the 'ToArray' method creates an entire new array no the HEAP every Time!!
                    //_indicesPick = quadTree.IndexBufferData.ToArray();  
                    quadToRayIntersection.Quad.IndexBufferData.CopyTo(_indicesPick);

                    // Set quadKey
                    quadKey = quadToRayIntersection.Quad.QuadKeyInstance;

                } // If null
            }

            return indicesPickCount;
        }


        /// <summary>
        /// Checks whether a ray intersects a model. This method needs to access
        /// the model vertex data, so the model must have been built using the
        /// custom TrianglePickingProcessor provided as part of this sample.
        /// Returns the distance along the ray to the point of intersection, or null
        /// if there is no intersection.
        /// </summary>
        private static float? RayIntersectsModel(ref Ray ray, ref  VertexMultitextured_Stream1[] quadVertexData, ref int[] quadIndicesData, int quadIndicesCount,
                                         ref Matrix modelTransform, out bool insideBoundingSphere,
                                         out Vector3 vertex1, out Vector3 vertex2,
                                         out Vector3 vertex3, out int arrayIndexPos1,
                                         out int arrayIndexPos2, out int arrayIndexPos3) // Ben - Added arrayIndexPos
        {
            
            arrayIndexPos1 = arrayIndexPos2 = arrayIndexPos3 = 0;
            vertex1 = vertex2 = vertex3 = Vector3.Zero;

            // The input ray is in World space, but our model data is stored in object
            // space. We would normally have to Transform all the model data by the
            // modelTransform matrix, moving it into World space before we test it
            // against the ray. That Transform can be slow if there are a lot of
            // triangles in the model, however, so instead we do the opposite.
            // Transforming our ray by the inverse modelTransform moves it into object
            // space, where we can test it directly against our model data. Since there
            // is only one ray but typically many triangles, doing things this way
            // around can be much faster.

            //Matrix inverseTransform; // = Matrix.Invert(modelTransform);
            //Matrix.Invert(ref modelTransform, out inverseTransform);

            //ray.Position = Vector3.Transform(ray.Position, inverseTransform);
            Vector3.Transform(ref ray.Position, ref modelTransform, out ray.Position); // 5/18/2010: Was inverseTransform
            //ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransform);
            Vector3.TransformNormal(ref ray.Direction, ref modelTransform, out ray.Direction); // 5/18/2010: Was inverseTransform


            // The bounding sphere test passed, so we need to do a full
            // triangle picking test.
            insideBoundingSphere = true;

            // Keep track of the closest triangle we found so far,
            // so we can always return the closest one.
            float? closestIntersection = null;

            // 6/11/2009: Updated to use the 'quadIndicesCount', rather than the direct 'Length' value of the
            //            given 'quadIndicesData'; this is because this temp array is only resized upwards, and
            //            can have more indices than needs to be check for the given call!
            for (var loop1 = 0; loop1 < quadIndicesCount; loop1 += 3)
            {
                // Perform a ray to triangle intersection test.
                float? intersection;

                // 4/10/2009 - Indexes calculated here, to reduce CPI in VTune!
                arrayIndexPos1 = quadIndicesData[loop1];
                arrayIndexPos2 = quadIndicesData[loop1 + 1];
                arrayIndexPos3 = quadIndicesData[loop1 + 2];

                // Add Quad's VertexBufferOffset value to Pick the proper Quad section of the Terrain!
                var vertexPos1 = quadVertexData[arrayIndexPos1].Position;
                var vertexPos2 = quadVertexData[arrayIndexPos2].Position;
                var vertexPos3 = quadVertexData[arrayIndexPos3].Position;

#if DEBUG
                // 5/18/2010 - Store test triangles for debug purposes
                if (ShowDebugTestTriangles)
                    _testTriangles.AddTriangle(loop1, ref vertexPos1, ref vertexPos2, ref vertexPos3, Color.White);
#endif

                RayIntersectsTriangle(ref ray,
                                      ref vertexPos1,
                                      ref vertexPos2,
                                      ref vertexPos3,
                                      out intersection);

                // Does the ray intersect this triangle?
                if (intersection == null) continue;
                

                // If so, is it closer than any other previous triangle?
                if ((closestIntersection != null) && (intersection >= closestIntersection)) continue;

                // Store the distance to this triangle.
                closestIntersection = intersection;


                Vector3.Transform(ref vertexPos1,
                                  ref modelTransform, out vertex1);

                Vector3.Transform(ref vertexPos2,
                                  ref modelTransform, out vertex2);

                Vector3.Transform(ref vertexPos3,
                                  ref modelTransform, out vertex3);

               
            } // End For Indices Loop

            return closestIntersection;

        }


        /// <summary>
        /// Checks whether a ray intersects a triangle. This uses the algorithm
        /// developed by Tomas Moller and Ben Trumbore, which was published in the
        /// Journal of Graphics Tools, volume 2, "Fast, Minimum Storage Ray-Triangle
        /// Intersection".
        /// 
        /// This method is implemented using the pass-by-reference versions of the
        /// XNA math functions. Using these overloads is generally not recommended,
        /// because they make the code less readable than the normal pass-by-value
        /// versions. This method can be called very frequently in a tight inner loop,
        /// however, so in this particular case the performance benefits from passing
        /// everything by reference outweigh the loss of readability.
        /// </summary>
        private static void RayIntersectsTriangle(ref Ray ray,
                                          ref Vector3 vertex1,
                                          ref Vector3 vertex2,
                                          ref Vector3 vertex3, out float? result)
        {
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;
            Vector3.Subtract(ref vertex2, ref vertex1, out edge1);
            Vector3.Subtract(ref vertex3, ref vertex1, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                result = null;
                return;
            }

            var inverseDeterminant = 1.0f / determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref vertex1, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0.0f || triangleU > 1.0f)
            {
                result = null;
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0.0f || triangleU + triangleV > 1.0f)
            {
                result = null;
                return;
            }

            // Compute the distance along the ray to the triangle.
            float rayDistance;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out rayDistance);
            rayDistance *= inverseDeterminant;

            // Is the triangle behind the ray Origin?
            if (rayDistance < 0.0f)
            {
                result = null;
                return;
            }

            result = rayDistance;
        }  
      
        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!disposing) return;

            // 1/5/2010
            Game.Services.RemoveService(typeof(TerrainPickingRoutines));  

            // Dispose                
            _screenText.Dispose();
            if (_tShapeHelper != null)
                _tShapeHelper.Dispose();

            // Clear Arrays - 1/8/2010
            Array.Clear(_indicesPick, 0, _indicesPick.Length);

            // Null references
            _terrainShape = null;                
            _tShapeHelper = null;
        }

    }
}
