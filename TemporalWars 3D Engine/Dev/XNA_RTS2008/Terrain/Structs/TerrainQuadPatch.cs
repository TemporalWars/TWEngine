#region File Description
//-----------------------------------------------------------------------------
// TerrainQuadPatch.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Terrain.Enums;

namespace TWEngine.Terrain.Structs
{
    ///<summary>
    /// The <see cref="TerrainQuadPatch"/> structure is used to hold 
    /// the <see cref="IndexBuffer"/> data for one <see cref="TerrainQuadTree"/> patch.
    ///</summary>
    public struct TerrainQuadPatch : IDisposable
    {
        //private int parent; // was - TerrainQuadTree Type
        private IndexBuffer _indexBuffers;       
        
        // 4/3/2008: Add IndexBuffer Array Size
        private int _indexBufferSize;
        private int _numTris;       
        private readonly int _width;

        // 3/3/2009
        readonly int _yStride;       

        #region Properties       

        ///<summary>
        /// Get or set the <see cref="IndexBuffer"/> collection
        ///</summary>
        public IndexBuffer IndexBuffers
        {
            get { return _indexBuffers; }
            set { _indexBuffers = value; }
        }

        ///<summary>
        /// Get or set the <see cref="IndexBuffer"/> collection size.
        ///</summary>
        public int IndexBufferSize
        {
            get { return _indexBufferSize; }
            set { _indexBufferSize = value; }
        }

        ///<summary>
        /// Get or set the number of triangles in <see cref="TerrainQuadPatch"/>.
        ///</summary>
        public int NumTris
        {
            get { return _numTris; }
            set { _numTris = value; }
        }

        #endregion


        #region Initialization

        // 4/9/2008: Overload; 3/3/2009: Updated to pass back the Indices Array via an out parameter.
        ///<summary>
        /// Constructor, which calls the internal <see cref="SetupTerrainIndices"/> method, which populates the
        /// <see cref="IndexBuffer"/> collection.
        ///</summary>
        ///<param name="device"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="width">width</param>
        ///<param name="detail"><see cref="LOD"/> Enum</param>
        ///<param name="offsetX">offsetX value</param>
        ///<param name="offsetY">offsetY value</param>
        ///<param name="indicesData">(OUT) collection of indicies</param>
        public TerrainQuadPatch(GraphicsDevice device, int width, LOD detail, int offsetX, int offsetY, out int[] indicesData)
        {
            _indexBuffers = null; // 4/15/2009
            
            _indexBufferSize = 0; // 4/15/2009
            _numTris = 0; // 4/15/2009
            _width = width;

            // 3/4/2009 - Calc Y-Stride first, since it does not change below.
            _yStride = (TerrainData.MapHeight + 1);

            // Setup patch with the highest LOD available
            SetupTerrainIndices(device, detail, ref offsetX, ref offsetY, out indicesData);

        }
       

        // 3/3/2009 - Updated to pass out the Indicies Data, to avoid having to use the GetData call on a WriteOnly buffer!
        ///<summary>
        ///  The following Overloaded SetupTerrainIndices takes the 'OffsetX' and 'OffsetY'
        ///  Quad Values in order to figure out where to start the Index Buffer Triangles.
        ///  Solution:  The original Quads all had there own Vertices created for each Quad; however,
        ///             the problem with this algorithm was each Quad had its own bounding vertices
        ///             at the edge of the Quad, and therefore, you could see cracks in the Terrain!
        /// 
        ///             To correct this, I had to create the Vertices once for the entire Terrain,
        ///             and then make the Indices share the overlapping edges.  Furthermore, in 
        ///             order to create the Indices at the proper vertices, I needed to pass in the
        ///             Quad's TopLeft Offset X&Y values.
        /// Correction:
        /// 8/11/2008: Updated the Y-Stride to by 1+ the MapHeight; this is to correct the error of the last Quad's
        ///            to the far right and bottom were incorrectly creating triangles which span accross the terrain!  
        ///            This was due to the IndexBuffer formula, which multiplies each value by the 'Detail' value.  
        ///
        ///            For example, if the 'Detail' Value is 16, then when the first Quad is made on the far Left, the
        ///            result of (x+y*mapHeight)*Detail for (1+0*512)*16 = 16, when it should have been 15, because 0-based!!!
        ///            This makes the first Quad 17 accross, while all others are 16, and thereby, pushes the last triangle 
        ///            connection back to the next row, accross the Terrain!
        ///
        ///            Furthermore, if you try adjust the formula in the IndexBuffer to be (Detail-1), even though it does
        ///            correct the spacing problem, a new problem arise; none of the BoundingBoxes match the Quads anymore.
        ///
        ///            Therefore, the best solution it to simply ADD +1 to the Y-Stride during the creation of the Verticies. - Ben
        ///</summary>
        ///<param name="device"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="detailLevel"><see cref="LOD"/> Enum</param>
        ///<param name="inOffsetX">offsetX value</param>
        ///<param name="inOffsetY">offsetY value</param>
        ///<param name="indicesData">(OUT) collection of indicies</param>
        public void SetupTerrainIndices(GraphicsDevice device, LOD? detailLevel, ref int inOffsetX, ref int inOffsetY, out int[] indicesData)
        {
            // 5/19/2010 - Refactored out the core code into new STATIC method.
            CreateIndicesData(ref this, device, detailLevel, inOffsetX, inOffsetY, out indicesData);
        }

        // 5/19/2010
        /// <summary>
        /// Helper method for <see cref="SetupTerrainIndices"/>.
        /// </summary>
        /// <param name="terrainQuadPatch">this instance of <see cref="TerrainQuadPatch"/></param>
        ///<param name="device"><see cref="GraphicsDevice"/> instance</param>
        ///<param name="detailLevel"><see cref="LOD"/> Enum</param>
        ///<param name="inOffsetX">offsetX value</param>
        ///<param name="inOffsetY">offsetY value</param>
        ///<param name="indicesData">(OUT) collection of indicies</param>
        private static void CreateIndicesData(ref TerrainQuadPatch terrainQuadPatch, GraphicsDevice device, LOD? detailLevel, 
                                              int inOffsetX, int inOffsetY, out int[] indicesData)
        {
            var offsetX = 0; var offsetY = 0; int endOffsetX; int endOffsetY;
            var ix = 0; var iy = 0;

            // 5/19/2010 - Cache
            var width = terrainQuadPatch._width;
            var yStride = terrainQuadPatch._yStride;

            if (detailLevel == null)
                detailLevel = LOD.DetailUltra1;

            var detail = (int)detailLevel;

            // If detail level is smaller than the quad patch, then move up to
            // the next highest detail level.
            var widthMinus1 = (width - 1); // 5/19/2010 - Cache
            if (detail >= widthMinus1)
                detail /= 2;

            // I Update the Loop Beginning and Ending values using the Quad's OffsetX and OffsetY
            // positions.  The Y-Stride was updated as well to follow the stride of the Vertices, which
            // happens to be the size of the entire Map.  Finally, the Indices Array formula was updated
            // to not use the Loops X&Y values, but instead update the ix & iy values manually. OffsetY = 0; endOffsetX = 0; endOffsetY = 0;
            if (inOffsetX == 0)
            {
                endOffsetX = widthMinus1 / detail;
            }
            else
            {
                offsetX = inOffsetX / detail;
                endOffsetX = (inOffsetX + widthMinus1) / detail;
                               
            }
            if (inOffsetY == 0)
            {
                endOffsetY = widthMinus1 / detail;
            }
            else
            {
                offsetY = inOffsetY / detail;
                endOffsetY = (inOffsetY + widthMinus1) / detail;
                
            }

            indicesData = new int[(widthMinus1 * widthMinus1 * 6) / (detail * detail)];
            
            for (var loopY = offsetY; loopY < endOffsetY; loopY++)
            {
                for (var loopX = offsetX; loopX < endOffsetX; loopX++)
                {
                    // 5/19/2010 - Cache calculations
                    var indexCalc = (ix + iy * (widthMinus1 / detail)) * 6;
                    var loopXPlus1 = (loopX + 1);
                    var loopYPlus1 = (loopY + 1);
                    var loopYMultyStride = loopY * yStride;
                    var loopYPlus1MultyStride = loopYPlus1 * yStride;

                    indicesData[indexCalc] = (loopX + loopYMultyStride) * detail;
                    indicesData[indexCalc + 1] = (loopXPlus1 + loopYMultyStride) * detail;
                    indicesData[indexCalc + 2] = (loopXPlus1 + loopYPlus1MultyStride) * detail;

                    indicesData[indexCalc + 3] = (loopX + loopYMultyStride) * detail;
                    indicesData[indexCalc + 4] = (loopXPlus1 + loopYPlus1MultyStride) * detail;
                    indicesData[indexCalc + 5] = (loopX + loopYPlus1MultyStride) * detail;

                    ix++;
                }
                ix = 0;
                iy++;
            }

            // 4/3/2008
            var indexBufferSize = (width - detail) * (width - detail) * 6; // 5/19/2010 - Cache calc.
            terrainQuadPatch._indexBufferSize = indexBufferSize;

            
#if EditMode // 8/14/2009 - Set 'BufferUsage', depending on if in 'EditMode'.
            terrainQuadPatch._indexBuffers = new IndexBuffer(device, typeof(int), indexBufferSize, BufferUsage.None);
#else
            terrainQuadPatch._indexBuffers = new IndexBuffer(device, typeof(int), indexBufferSize, BufferUsage.WriteOnly);
#endif

            terrainQuadPatch._indexBuffers.SetData(indicesData);

            // 4/13/2008 - Add to List
            //parent.IndexBufferData.Clear();
            //parent.IndexBufferData.InsertRange(0, indicesData);

            terrainQuadPatch._numTris = indicesData.Length / 3;
            
        }

        // 3/4/2009
/*
        /// <summary>
        /// Helper function which takes some 'Cell' value, and returns the Position the 'Cell' value
        /// is at, within an array, using the X/Y format.  
        /// The 'Cell' value is the result of X + Y * Y-Stride.  Therefore, this function simply is returning
        /// back the original X and Y value of the above formula.
        /// </summary>
        /// <param name="cellValue"></param>
        /// <param name="yStride"></param>
        /// <param name="XYPos"></param>
        private static void Get_XY_ValuesForGivenCellPosition(float cellValue, float yStride, out Point XYPos)
        {
            // Init
            XYPos = Point.Zero;

            // Step 1 - Divide CellValue by Y-Stride.
            float resultA = (cellValue / yStride);
            // Step 2 - Truncate the decimal, to get Y.
            int yValue = (int)resultA;
            // Step 3 - Take Truncate number * Stride.
            int yValueStride = yValue * (int)yStride;
            // Step 4 - Subtract new yValueStride, from CellValue, to get X.
            int xValue = ((int)cellValue - yValueStride);

            // Return result
            XYPos.X = xValue;
            XYPos.Y = yValue;
        }
*/
           
        #endregion

        // 1/8/2010
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {

            // Dispose
            if (_indexBuffers == null) return;

            _indexBuffers.Dispose();
            _indexBuffers = null;
        }
    }
}
