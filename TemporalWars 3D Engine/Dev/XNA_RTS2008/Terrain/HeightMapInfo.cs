#region File Description
//-----------------------------------------------------------------------------
// HeightMapInfo.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Spacewar.Terrain
{
    /// <summary>
    /// <see cref="HeightMapInfo"/> is a collection of data about the heightmap. It includes
    /// information about how high the terrain is, and how far apart each vertex is.
    /// It also has several functions to get information about the heightmap, including
    /// its height and normal at different points, and whether a point is on the 
    /// heightmap. It is the runtime equivalent of HeightMapInfoContent.
    /// </summary>
    public class HeightMapInfo
    {
        #region Private fields
                 
        /// <summary>
        /// <see cref="TerrainScale"/> is the distance between each entry in the Height property.
        /// For example, if TerrainScale is 30, Height[0,0] and Height[1,0] are 30
        /// units apart. 
        /// </summary>
        public float TerrainScale { get; private set; }
        
        /// <summary>
        /// This 2D array of floats tells us the height that each Position in the 
        /// heightmap is.
        /// </summary>
        private readonly float[,] _heights;

        private readonly Vector3[,] _normals;
         
        /// <summary>
        /// The Position of the heightmap's -x, -z corner, in worldspace.
        /// </summary>
        private readonly Vector3 _heightmapPosition;
        
        /// <summary>
        /// The total width of the heightmap, including <see cref="TerrainScale"/>.
        /// </summary>
        public float HeightMapWidth { get; private set; }
                
        /// <summary>
        /// The total height of the height map, including <see cref="TerrainScale"/>.
        /// </summary>
        public float HeightMapHeight { get; private set; }

        #endregion
        
        // Ben - Add parameter 'Verticies' for Pick Triangle Routines
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="heights">Collection of height values</param>
        /// <param name="normals">Collection of normal values</param>
        /// <param name="verticies">Collection of verticies</param>
        /// <param name="terrainScale">Terrain scale value to use</param>
        public HeightMapInfo(float[,] heights, Vector3[,] normals, Vector3[] verticies, float terrainScale)
        {
            if (heights == null)
            {
                throw new ArgumentNullException("heights");
            }
            if (normals == null)
            {
                throw new ArgumentNullException("normals");
            }
            // Ben - Added 2/27/2008
            if (verticies == null)
            {
                throw new ArgumentNullException("verticies");
            }

            TerrainScale = terrainScale;
            _heights = heights;
            _normals = normals;
            // Ben - Added 2/27/2008

            HeightMapWidth = (heights.GetLength(0) - 1) * terrainScale;
            HeightMapHeight = (heights.GetLength(1) - 1) * terrainScale;

            _heightmapPosition.X = -(heights.GetLength(0) - 1) / 2.0f * terrainScale;
            _heightmapPosition.Z = -(heights.GetLength(1) - 1) / 2.0f * terrainScale;
        }


        // This function takes in a Position, and tells whether or not the Position is 
        // on the heightmap.
        public bool IsOnHeightmap(Vector3 position)
        {
            // first we'll figure out where on the heightmap "Position" is...
            Vector3 positionOnHeightmap = position - _heightmapPosition;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            return (positionOnHeightmap.X > 0 &&
                positionOnHeightmap.X < HeightMapWidth &&
                positionOnHeightmap.Z > 0 &&
                positionOnHeightmap.Z < HeightMapHeight);
        }

        // This function takes in a Position, and has two out parameters: the 
        // heightmap's height and normal at that point. Be careful - this function will 
        // throw an IndexOutOfRangeException if Position isn't on the heightmap!        
        public void GetHeightAndNormal
            (Vector3 position, out float height, out Vector3 normal)
        {
            // the first thing we need to do is figure out where on the heightmap
            // "Position" is. This'll make the math much simpler later.
            Vector3 positionOnHeightmap = position - _heightmapPosition;

            // we'll use integer division to figure out where in the "_heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left = (int)positionOnHeightmap.X / (int)TerrainScale;
            int top = (int)positionOnHeightmap.Z / (int)TerrainScale;

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = (positionOnHeightmap.X % TerrainScale) / TerrainScale;
            float zNormalized = (positionOnHeightmap.Z % TerrainScale) / TerrainScale;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the _heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                _heights[left, top],
                _heights[left + 1, top],
                xNormalized);

            float bottomHeight = MathHelper.Lerp(
                _heights[left, top + 1],
                _heights[left + 1, top + 1],
                xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // Position.
            height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);

            // We'll repeat the same process to calculate the normal.
            Vector3 topNormal = Vector3.Lerp(
                _normals[left, top],
                _normals[left + 1, top],
                xNormalized);

            Vector3 bottomNormal = Vector3.Lerp(
                _normals[left, top + 1],
                _normals[left + 1, top + 1],
                xNormalized);

            normal = Vector3.Lerp(topNormal, bottomNormal, zNormalized);
            normal.Normalize();
        }

        // 3/14/2008
        public void RebuildNormals(IndexBuffer ib, ref VertexMultitextured_Stream1[] vertexData)
        {
            var indices = new int[(ib.SizeInBytes / sizeof(int))];
            ib.GetData(indices);

            // 1st - Zero out all _normals.
            for (int i = 0; i < vertexData.Length; i++)
                vertexData[i].Normal = new HalfVector4(0, 0, 0, 0);
                //vertexData[i].Normal = new Vector3(0, 0, 0);

            // 2nd - Update Normals in VertexData Array
            for (int i = 0; i < indices.Length / 3; i++)
            {
                // Normals
                Vector3 firstvec = vertexData[indices[i * 3 + 1]].Position - vertexData[indices[i * 3]].Position;
                Vector3 secondvec = vertexData[indices[i * 3]].Position - vertexData[indices[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertexData[indices[i * 3]].Normal += normal;
                vertexData[indices[i * 3 + 1]].Normal += normal;
                vertexData[indices[i * 3 + 2]].Normal += normal;               
                
            }
            // 3rd - A 2nd pass of Normalize required.
            for (int i = 0; i < vertexData.Length; i++)
                vertexData[i].Normal.Normalize();

            // Now update Heightmaps internal Heights & Normals arrays!
            // we'll go through each vertex....
            for (int i = 0; i < vertexData.Length; i++)
            {
                // ... and look up its Position and normal.
                Vector3 position = vertexData[i].Position;
                Vector3 normal = vertexData[i].Normal;

                // from the Position's X and Z value, we can tell what X and Y
                // coordinate of the arrays to put the height and normal into.
                var arrayX = (int)
                    ((position.X / TerrainScale) + (_heights.GetLength(0) - 1) / 2.0f);
                var arrayY = (int)
                    ((position.Z / TerrainScale) + (_heights.GetLength(1) - 1) / 2.0f);

                _heights[arrayX, arrayY] = position.Y;
                _normals[arrayX, arrayY] = normal;

            }

        }

    }   
}
