#region File Description
//-----------------------------------------------------------------------------
// PickTriangles.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.Terrain.Structs
{
    // 3/13/2008
    ///<summary>
    /// Custom Picked triangle structure, which holds the data for
    /// the current picked triangle at the user's cursor.
    ///</summary>
    public struct PickTriangles
    {
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="corner1Pos"><see cref="Vector3"/> for corner position 1</param>
        ///<param name="corner2Pos"><see cref="Vector3"/> for corner position 2</param>
        ///<param name="corner3Pos"><see cref="Vector3"/> for corner position 3</param>
        ///<param name="color">Wireframe color to use for triangle</param>
        public PickTriangles(Vector3 corner1Pos, Vector3 corner2Pos, Vector3 corner3Pos, Color color)
        {
            Triangle = new VertexPositionColor[3];
            Triangle[0] = new VertexPositionColor(corner1Pos, color);
            Triangle[1] = new VertexPositionColor(corner2Pos, color);
            Triangle[2] = new VertexPositionColor(corner3Pos, color);
            RayPosition = new Vector3(0, 0, 0);
            RayDirection = new Vector3(0, 0, 0);
            RayDistance = 0;
            VertexArrayValue = new int[3]; // All 3 VertexData Array Position
            QuadInstanceKey = -1;
        }

        ///<summary>
        /// Collection of <see cref="VertexPositionColor"/> for a triangle.
        ///</summary>
        public VertexPositionColor[] Triangle;
        ///<summary>
        /// Position <see cref="Ray"/> hit terrain.
        ///</summary>
        public Vector3 RayPosition;
        ///<summary>
        /// Direction of <see cref="Ray"/>
        ///</summary>
        public Vector3 RayDirection;
        ///<summary>
        /// Distance of <see cref="Ray"/> from cursor to hit postion
        ///</summary>
        public float RayDistance;
        ///<summary>
        /// Collection of integer values
        ///</summary>
        public int[] VertexArrayValue;
        ///<summary>
        /// Unique instance key for <see cref="TerrainQuadPatch"/> which ray hit.
        ///</summary>
        public int QuadInstanceKey;

        // 5/18/2010
        /// <summary>
        /// Allows updating the triangle's color.
        /// </summary>
        public void SetTriangleColor(ref Color colorToUse)
        {
            if (Triangle == null || Triangle.Length == 0) return;

            // iterate Triangle[] collection and set color
            var length = Triangle.Length;
            for (var i = 0; i < length; i++)
            {
                Triangle[i].Color = colorToUse;
            }
        }

        // 5/18/2010
        /// <summary>
        /// Used to add additional triangles to the internal collection.
        /// </summary>
        /// <param name="triangleNumber">Triangle number to be added, which is used as index into internal collection.</param>
        /// <param name="corner1Pos">see cref="Vector3"/> for corner position 1</param>
        /// <param name="corner2Pos">see cref="Vector3"/> for corner position 2</param>
        /// <param name="corner3Pos">see cref="Vector3"/> for corner position 3</param>
        /// <param name="color"><see cref="Color"/> to apply for triangle</param>
        public void AddTriangle(int triangleNumber, ref Vector3 corner1Pos, ref Vector3 corner2Pos, ref Vector3 corner3Pos, Color color)
        {
            // Create array index
            var arrayIndex = triangleNumber * 3;

            // Check if array large enough
            if (Triangle.Length < arrayIndex + 3)
                Array.Resize(ref Triangle, arrayIndex + 3);

            // Set Position
            Triangle[arrayIndex].Position = corner1Pos;
            Triangle[arrayIndex + 1].Position = corner2Pos;
            Triangle[arrayIndex + 2].Position = corner3Pos;

            // Set Color
            Triangle[arrayIndex].Color = color;
            Triangle[arrayIndex + 1].Color = color;
            Triangle[arrayIndex + 2].Color = color;
        }
    }
}
