#region File Description
//-----------------------------------------------------------------------------
// TerrainQuadTessellater.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using TWEngine.Terrain.Enums;
using TWEngine.Terrain.Structs;
using TWEngine.TerrainTools;

namespace TWEngine.Terrain
{
    /// <summary>
    /// The <see cref="TerrainQuadTessellater"/> class is used to tessellate 
    /// some given <see cref="TerrainQuadPatch"/> down to a lower level detail.
    /// </summary>
    public static class TerrainQuadTessellater
    {
        /// <summary>
        /// Tessellate given <paramref name="terrainQuadTree"/> patch one <see cref="TessellateLevel"/> deeper.
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="terrainQuadTree"/> instance</param>
        /// <param name="quadInstance">Quad instance key</param>
        /// <param name="changeToLevel"><see cref="TessellateLevel"/> Enum</param>
        public static void TessellateCurrentQuad(TerrainQuadTree terrainQuadTree, int quadInstance, TessellateLevel changeToLevel)
        {
            // Check if current Quad is the one to Tessellate
            if (terrainQuadTree.QuadKey != quadInstance) return;

            // Check if we can Tessellate deeper
            if (terrainQuadTree.LODLevel == TessellateLevel.Level1 && changeToLevel == TessellateLevel.Level2)
            {
                // Remove Current Bounding Box from List
                //string Key = "Quad#" + _quadKeyInstance.ToString();
                TerrainQuadTree.TerrainShapeInterface.TerrainBoundingBoxes.Remove(terrainQuadTree.QuadKey);

#pragma warning disable 168
                var topLeft = new TerrainQuadTree(terrainQuadTree.WidthXHeight, terrainQuadTree.OffsetX, terrainQuadTree.OffsetY, terrainQuadTree.RootWidth, QuadSection.TopLeft, quadInstance);
#pragma warning restore 168
                                                  
                var bottomLeft = new TerrainQuadTree(terrainQuadTree.WidthXHeight, terrainQuadTree.OffsetX, (terrainQuadTree.Height - 1) / 2 + terrainQuadTree.OffsetY, terrainQuadTree.RootWidth,
                                                     QuadSection.BottomLeft, quadInstance);
                var topRight = new TerrainQuadTree(terrainQuadTree.WidthXHeight, (terrainQuadTree.Width - 1) / 2 + terrainQuadTree.OffsetX, terrainQuadTree.OffsetY, terrainQuadTree.RootWidth,
                                                   QuadSection.TopRight, quadInstance);
                var bottomRight = new TerrainQuadTree(terrainQuadTree.WidthXHeight, (terrainQuadTree.Width - 1) / 2 + terrainQuadTree.OffsetX,
                                                      (terrainQuadTree.Height - 1) / 2 + terrainQuadTree.OffsetY, terrainQuadTree.RootWidth,
                                                      QuadSection.BottomRight, quadInstance);

                terrainQuadTree.TreeList = new List<TerrainQuadTree>();
                terrainQuadTree.TopLeftIndex = terrainQuadTree.TreeList.Count - 1;
                terrainQuadTree.TreeList.Add(topRight);
                terrainQuadTree.TopRightIndex = terrainQuadTree.TreeList.Count - 1;
                terrainQuadTree.TreeList.Add(bottomLeft);
                terrainQuadTree.BottomLeftIndex = terrainQuadTree.TreeList.Count - 1;
                terrainQuadTree.TreeList.Add(bottomRight);
                terrainQuadTree.BottomRightIndex = terrainQuadTree.TreeList.Count - 1;

                // Change Current Quad Instance to Branch
                terrainQuadTree.Leaf = false;
            }
                // At LOD Level 2, instead of creating smaller Quads, we simply change
                // the Detail level down and re-build the Leaf.
            else if (terrainQuadTree.LODLevel == TessellateLevel.Level2 && changeToLevel == TessellateLevel.Level3)
            {
                terrainQuadTree.LODLevel = TessellateLevel.Level3;
                terrainQuadTree.Detail = LOD.DetailHigh2;

                // 3/3/2009 - Updated to get the Indices Array, via the 'Out' parameter.
                // Create LeafPatch
                //_leafPatch = null;
                terrainQuadTree.LeafPatch = new TerrainQuadPatch(TerrainQuadTree.TerrainShapeInterface.Device, terrainQuadTree.Width, terrainQuadTree.Detail, terrainQuadTree.OffsetX,
                                                                  terrainQuadTree.OffsetY, out terrainQuadTree.TmpIndexBufferData);
                // 4/21/2009
                terrainQuadTree.IndexBufferData.Clear();
                terrainQuadTree.IndexBufferData.InsertRange(0, terrainQuadTree.TmpIndexBufferData);


                // 1st - Check for Adjacent Neighbors to Crack Fix.
                // Eliminate Cracks for Quad - Top
                var parentQuadKeyInstance = terrainQuadTree.ParentQuadKeyInstance; // 5/19/2010
                if (parentQuadKeyInstance != null)
                {
                    var quadAdjacentTopKey = TerrainData.GetAdjacentQuadInstanceKey((int) parentQuadKeyInstance,
                                                                                    QuadAdjacent.Top,
                                                                                    terrainQuadTree.QuadSection);
                    if (quadAdjacentTopKey != null)
                        TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentTopKey, QuadAdjacent.Top,
                                                                terrainQuadTree.QuadSection,
                                                                terrainQuadTree.Detail);

                    // Eliminate Cracks for Quad - Bottom
                    var quadAdjacentBottomKey = TerrainData.GetAdjacentQuadInstanceKey((int) parentQuadKeyInstance,
                                                                                       QuadAdjacent.Bottom,
                                                                                       terrainQuadTree.QuadSection);
                    if (quadAdjacentBottomKey != null)
                        TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentBottomKey, QuadAdjacent.Bottom,
                                                                terrainQuadTree.QuadSection, terrainQuadTree.Detail);

                    // Eliminate Cracks for Quad - Left
                    var quadAdjacentLeftKey = TerrainData.GetAdjacentQuadInstanceKey((int) parentQuadKeyInstance,
                                                                                     QuadAdjacent.Left,
                                                                                     terrainQuadTree.QuadSection);
                    if (quadAdjacentLeftKey != null)
                        TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentLeftKey, QuadAdjacent.Left,
                                                                terrainQuadTree.QuadSection, terrainQuadTree.Detail);

                    // Eliminate Cracks for Quad - Right
                    var quadAdjacentRightKey = TerrainData.GetAdjacentQuadInstanceKey((int) parentQuadKeyInstance,
                                                                                      QuadAdjacent.Right,
                                                                                      terrainQuadTree.QuadSection);
                    if (quadAdjacentRightKey != null)
                        TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentRightKey, QuadAdjacent.Right,
                                                                terrainQuadTree.QuadSection, terrainQuadTree.Detail);
                }

                // 2nd - Now Crack Fix Internal TWO Adjacent Siblings, if necessary.
                // Each QuadSection will always have TWO inner Adjacent Siblings
                //
                // Note: Quad Sibilings Keys are numbering in following order:
                //       |---|---|
                //       | 1 | 3 |
                //       |---|---|
                //       | 2 | 4 |
                //       |---|---
                switch (terrainQuadTree.QuadSection)
                {
                    case QuadSection.TopLeft:
                        // Sibilings are: Bottom/Right
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey + 2, QuadAdjacent.Right,
                                                                QuadSection.TopLeft, terrainQuadTree.Detail);
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey + 1, QuadAdjacent.Bottom,
                                                                QuadSection.TopLeft, terrainQuadTree.Detail);

                        break;
                    case QuadSection.TopRight:
                        // Sibilings are: Bottom/Left
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey - 2, QuadAdjacent.Left,
                                                                QuadSection.TopRight, terrainQuadTree.Detail);
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey + 1, QuadAdjacent.Bottom,
                                                                QuadSection.TopRight, terrainQuadTree.Detail);

                        break;
                    case QuadSection.BottomLeft:
                        // Sibilings are: Top/Right
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey - 1, QuadAdjacent.Top,
                                                                QuadSection.BottomLeft, terrainQuadTree.Detail);
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey + 2, QuadAdjacent.Right,
                                                                QuadSection.BottomLeft, terrainQuadTree.Detail);

                        break;
                    case QuadSection.BottomRight:
                        // Sibilings are: Top/Left
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey - 1, QuadAdjacent.Top,
                                                                QuadSection.BottomRight, terrainQuadTree.Detail);
                        TerrainEditRoutines.CrackFixCurrentQuad(terrainQuadTree.QuadKey - 2, QuadAdjacent.Left,
                                                                QuadSection.BottomRight, terrainQuadTree.Detail);

                        break;
                    default:
                        break;
                }
            }
            
        }

        ///<summary>
        /// Adds Triangles to <see cref="TerrainQuadPatch"/> to eliminate terrain cracks, caused by neighbors 
        /// having a different <see cref="LOD"/> setting.
        ///</summary>
        ///<param name="terrainQuadTree">this instance of <see cref="TerrainQuadTree"/></param>
        ///<param name="quadInstance">Quad instance key</param>
        ///<param name="quadAdjacent"><see cref="QuadAdjacent"/> Enum</param>
        ///<param name="section"><see cref="QuadSection"/> Enum</param>
        ///<param name="detailLevel"><see cref="LOD"/> Enum</param>
        public static void CrackFixCurrentQuad(TerrainQuadTree terrainQuadTree, int quadInstance, QuadAdjacent quadAdjacent, QuadSection section, LOD detailLevel)
        {
            // Check if current Quad is the one to eliminate cracks.
            if (terrainQuadTree.QuadKey != quadInstance) return;

            // Crack Fix: LOD 1 to LOD 2
            if (terrainQuadTree.Detail == LOD.DetailMinimum16 && detailLevel == LOD.DetailLow8)
                CrackFixQuad_LODMinToLODLow(quadAdjacent, section, terrainQuadTree.IndexBufferData, terrainQuadTree.OffsetX, terrainQuadTree.OffsetY,
                                            ref terrainQuadTree.LeafPatch);

            // Crack Fix: LOD 1 to LOD 3
            if (terrainQuadTree.Detail == LOD.DetailMinimum16 && detailLevel == LOD.DetailHigh2)
                CrackFixQuad_LODMinToLODMed(quadAdjacent, section, terrainQuadTree.IndexBufferData, terrainQuadTree.OffsetX, terrainQuadTree.OffsetY,
                                            ref terrainQuadTree.LeafPatch);

            // Crack Fix: LOD 2 to LOD 3
            if (terrainQuadTree.Detail == LOD.DetailLow8 && detailLevel == LOD.DetailHigh2)
                CrackFixQuad_LODLowToLODMed(quadAdjacent, section, terrainQuadTree.IndexBufferData, terrainQuadTree.OffsetX, terrainQuadTree.OffsetY,
                                            ref terrainQuadTree.LeafPatch);
        }

        /// <summary>
        /// Helper Function: Crack Fix between Level 2 to Level 3 tessellation.
        /// </summary>
        private static void CrackFixQuad_LODLowToLODMed(QuadAdjacent quadAdjacent, QuadSection section,
                                                        List<int> indexBufferData, int offsetX, int offsetY,
                                                        ref TerrainQuadPatch leafPatch)
        {
            switch (quadAdjacent)
            {
                case QuadAdjacent.Top:
                    switch (section)
                    {
                        case QuadSection.TopLeft:
                        case QuadSection.TopRight:
                            CrackFixQuad_TessBottomOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                            //
                            // Following are the TWO Internal Sibiling Crack Fixes
                            //
                        case QuadSection.BottomLeft:
                        case QuadSection.BottomRight:
                            CrackFixQuad_TessBottomOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Bottom:
                    switch (section)
                    {
                        case QuadSection.BottomLeft:
                        case QuadSection.BottomRight:
                            CrackFixQuad_TessTopOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                            //
                            // Following are the TWO Internal Sibiling Crack Fixes
                            //
                        case QuadSection.TopLeft:
                        case QuadSection.TopRight:
                            CrackFixQuad_TessTopOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Left:
                    switch (section)
                    {
                        case QuadSection.TopLeft:
                        case QuadSection.BottomLeft:
                            CrackFixQuad_TessRightOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                            //
                            // Following are the TWO Internal Sibiling Crack Fixes
                            //
                        case QuadSection.TopRight:
                        case QuadSection.BottomRight:
                            CrackFixQuad_TessRightOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Right:
                    switch (section)
                    {
                        case QuadSection.TopRight:
                        case QuadSection.BottomRight:
                            CrackFixQuad_TessLeftOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                            //
                            // Following are the TWO Internal Sibiling Crack Fixes
                            //
                        case QuadSection.TopLeft:
                        case QuadSection.BottomLeft:
                            CrackFixQuad_TessLeftOfLODLow(indexBufferData, offsetX, offsetY, ref leafPatch);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper Fn: Tessellate Left of LOD Low to Match LOD Med.
        /// </summary>
        private static void CrackFixQuad_TessLeftOfLODLow(List<int> indexBufferData, int offsetX, int offsetY,
                                                          ref TerrainQuadPatch leafPatch)
        {
            // Remove Triangle
            indexBufferData.RemoveRange(3, 3);
            leafPatch.NumTris -= 1;

            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
            //
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            indexBufferData.Insert(3, offsetX + offsetY*mapHeightPlus1);
            indexBufferData.Insert(4, (offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
            indexBufferData.Insert(5, offsetX + (offsetY + 2)*mapHeightPlus1);

            var x1 = 4;
            var x2 = 2;
            for (var i = 1; i < 4; i++)
            {
                indexBufferData.Add(offsetX + (offsetY + x2)*mapHeightPlus1);
                indexBufferData.Add((offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add(offsetX + (offsetY + x1)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            x1 = 10;
            x2 = 8;
            for (var i = 1; i < 5; i++)
            {
                indexBufferData.Add(offsetX + (offsetY + x2)*mapHeightPlus1);
                indexBufferData.Add((offsetX + 8) + (offsetY + 16)*mapHeightPlus1);
                indexBufferData.Add(offsetX + (offsetY + x1)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            // Update Triangle Count
            leafPatch.NumTris += 8;

            // Copy Array in to IndexBuffer
            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
        }

        /// <summary>
        /// Helper Fn: Tessellate Right of LOD Low to Match LOD Med.
        /// </summary>
        private static void CrackFixQuad_TessRightOfLODLow(List<int> indexBufferData, int offsetX, int offsetY,
                                                           ref TerrainQuadPatch leafPatch)
        {
            // Remove Triangle
            indexBufferData.RemoveRange(6, 3);
            leafPatch.NumTris -= 1;

            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
            //
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            indexBufferData.Insert(6, (offsetX + 8) + offsetY*mapHeightPlus1);
            indexBufferData.Insert(7, (offsetX + 16) + offsetY*mapHeightPlus1);
            indexBufferData.Insert(8, (offsetX + 16) + (offsetY + 2)*mapHeightPlus1);

            var x1 = 4;
            var x2 = 2;
            for (var i = 1; i < 4; i++)
            {
                indexBufferData.Add((offsetX + 8) + offsetY*mapHeightPlus1);
                indexBufferData.Add((offsetX + 16) + (offsetY + x2)*mapHeightPlus1);
                indexBufferData.Add((offsetX + 16) + (offsetY + x1)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            x1 = 10;
            x2 = 8;
            for (var i = 1; i < 5; i++)
            {
                indexBufferData.Add((offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add((offsetX + 16) + (offsetY + x2)*mapHeightPlus1);
                indexBufferData.Add((offsetX + 16) + (offsetY + x1)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            // Update Triangle Count
            leafPatch.NumTris += 8;

            // Copy Array in to IndexBuffer
            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
        }

        /// <summary>
        /// Helper Fn: Tessellate Top of LOD Low to Match LOD Med.
        /// </summary>
        private static void CrackFixQuad_TessTopOfLODLow(List<int> indexBufferData, int offsetX, int offsetY,
                                                         ref TerrainQuadPatch leafPatch)
        {
            // Remove Triangle
            indexBufferData.RemoveRange(0, 3);
            leafPatch.NumTris -= 1;

            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
            //
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            indexBufferData.Insert(0, (offsetX + 2) + offsetY*mapHeightPlus1);
            indexBufferData.Insert(1, (offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
            indexBufferData.Insert(2, offsetX + offsetY*mapHeightPlus1);

            var x1 = 4;
            var x2 = 2;
            for (var i = 1; i < 4; i++)
            {
                indexBufferData.Add((offsetX + x1) + offsetY*mapHeightPlus1);
                indexBufferData.Add((offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x2) + offsetY*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            x1 = 10;
            x2 = 8;
            for (var i = 1; i < 5; i++)
            {
                indexBufferData.Add((offsetX + x1) + offsetY*mapHeightPlus1);
                indexBufferData.Add((offsetX + 16) + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x2) + offsetY*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            // Update Triangle Count
            leafPatch.NumTris += 8;

            // Copy Array in to IndexBuffer
            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
        }

        /// <summary>
        /// Helper Fn: Tessellate Bottom of LOD Low to Match LOD Med.
        /// </summary>
        private static void CrackFixQuad_TessBottomOfLODLow(List<int> indexBufferData, int offsetX, int offsetY,
                                                            ref TerrainQuadPatch leafPatch)
        {
            // Remove Triangle
            indexBufferData.RemoveRange(15, 3);
            leafPatch.NumTris -= 1;

            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
            //
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            indexBufferData.Insert(15, offsetX + (offsetY + 8)*mapHeightPlus1);
            indexBufferData.Insert(16, (offsetX + 2) + (offsetY + 16)*mapHeightPlus1);
            indexBufferData.Insert(17, offsetX + (offsetY + 16)*mapHeightPlus1);

            var x1 = 4;
            var x2 = 2;
            for (var i = 1; i < 4; i++)
            {
                indexBufferData.Add(offsetX + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x1) + (offsetY + 16)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x2) + (offsetY + 16)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            x1 = 10;
            x2 = 8;
            for (var i = 1; i < 5; i++)
            {
                indexBufferData.Add((offsetX + 8) + (offsetY + 8)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x1) + (offsetY + 16)*mapHeightPlus1);
                indexBufferData.Add((offsetX + x2) + (offsetY + 16)*mapHeightPlus1);
                x1 += 2;
                x2 += 2;
            }

            // Update Triangle Count
            leafPatch.NumTris += 8;

            // Copy Array in to IndexBuffer
            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
        }

        /// <summary>
        /// Helper Function: Crack Fix between Level 1 to Level 3 tessellation.
        /// </summary>
        private static void CrackFixQuad_LODMinToLODMed(QuadAdjacent quadAdjacent, QuadSection section,
                                                        List<int> indexBufferData, int offsetX, int offsetY,
                                                        ref TerrainQuadPatch leafPatch)
        {
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            switch (quadAdjacent)
            {
                case QuadAdjacent.Top:
                    switch (section)
                    {
                        case QuadSection.TopLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(15, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(15, offsetX + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(16, (offsetX + 2) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(17, offsetX + (offsetY + 32)*mapHeightPlus1);

                            var x1 = 4;
                            var x2 = 2;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add(offsetX + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x1) + (offsetY + 32)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x2) + (offsetY + 32)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        case QuadSection.TopRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(21, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(21, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(22, (offsetX + 18) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(23, (offsetX + 16) + (offsetY + 32)*mapHeightPlus1);

                            x1 = 20;
                            x2 = 18;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x1) + (offsetY + 32)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x2) + (offsetY + 32)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Bottom:
                    switch (section)
                    {
                        case QuadSection.BottomLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(0, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(0, (offsetX + 2) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(1, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(2, offsetX + offsetY*mapHeightPlus1);

                            var x1 = 4;
                            var x2 = 2;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add((offsetX + x1) + offsetY*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x2) + offsetY*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        case QuadSection.BottomRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(6, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(6, (offsetX + 18) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(7, (offsetX + 32) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(8, (offsetX + 16) + offsetY*mapHeightPlus1);

                            x1 = 20;
                            x2 = 18;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add((offsetX + x1) + offsetY*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 32) + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + x2) + offsetY*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Left:
                    switch (section)
                    {
                        case QuadSection.TopLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(6, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(6, (offsetX + 16) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(7, (offsetX + 32) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(8, (offsetX + 32) + (offsetY + 2)*mapHeightPlus1);

                            var x1 = 4;
                            var x2 = 2;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add((offsetX + 16) + offsetY*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 32) + (offsetY + x2)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 32) + (offsetY + x1)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        case QuadSection.BottomLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(18, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(18, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(19, (offsetX + 32) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(20, (offsetX + 32) + (offsetY + 18)*mapHeightPlus1);

                            x1 = 20;
                            x2 = 18;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 32) + (offsetY + x2)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 32) + (offsetY + x1)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Right:
                    switch (section)
                    {
                        case QuadSection.TopRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(3, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(3, offsetX + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(4, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(5, offsetX + (offsetY + 2)*mapHeightPlus1);

                            var x1 = 4;
                            var x2 = 2;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add(offsetX + (offsetY + x2)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                                indexBufferData.Add(offsetX + (offsetY + x1)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        case QuadSection.BottomRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(15, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(15, offsetX + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(16, (offsetX + 16) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(17, offsetX + (offsetY + 18)*mapHeightPlus1);

                            x1 = 20;
                            x2 = 18;
                            for (var i = 1; i < 8; i++)
                            {
                                indexBufferData.Add(offsetX + (offsetY + x2)*mapHeightPlus1);
                                indexBufferData.Add((offsetX + 16) + (offsetY + 32)*mapHeightPlus1);
                                indexBufferData.Add(offsetX + (offsetY + x1)*mapHeightPlus1);
                                x1 += 2;
                                x2 += 2;
                            }

                            // Update Triangle Count
                            leafPatch.NumTris += 8;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Helper Function: Crack Fix between Level 1 to Level 2 tessellation.
        /// </summary>
        private static void CrackFixQuad_LODMinToLODLow(QuadAdjacent quadAdjacent, QuadSection section,
                                                        List<int> indexBufferData, int offsetX, int offsetY,
                                                        ref TerrainQuadPatch leafPatch)
        {
            // Quad Adjacent Value is from the Tesselated Quad's point of view; in other words,
            // the current quad to fix, needs to address the top vertices if the quadAdjacent value
            // is Bottom!
            var mapHeightPlus1 = TerrainData.MapHeight + 1; // 5/19/2010 - Cache
            switch (quadAdjacent)
            {
                case QuadAdjacent.Top:
                    switch (section)
                    {
                        case QuadSection.TopLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(15, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(15, offsetX + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(16, (offsetX + 8) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(17, offsetX + (offsetY + 32)*mapHeightPlus1);

                            indexBufferData.Add(offsetX + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 16) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 8) + (offsetY + 32)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        case QuadSection.TopRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(21, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.
                            // 
                            indexBufferData.Insert(21, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(22, (offsetX + 24) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(23, (offsetX + 16) + (offsetY + 32)*mapHeightPlus1);

                            indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 24) + (offsetY + 32)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Bottom:
                    switch (section)
                    {
                        case QuadSection.BottomLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(0, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.
                            //                                
                            indexBufferData.Insert(0, offsetX + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(1, (offsetX + 8) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(2, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);

                            indexBufferData.Add((offsetX + 8) + offsetY*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 16) + offsetY*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());

                            break;
                        case QuadSection.BottomRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(6, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.
                            // 
                            indexBufferData.Insert(6, (offsetX + 16) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(7, (offsetX + 24) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(8, (offsetX + 32) + (offsetY + 16)*mapHeightPlus1);

                            indexBufferData.Add((offsetX + 24) + offsetY*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + offsetY*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 16)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Left:
                    switch (section)
                    {
                        case QuadSection.TopLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(6, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(6, (offsetX + 16) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(7, (offsetX + 32) + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(8, (offsetX + 32) + (offsetY + 8)*mapHeightPlus1);

                            indexBufferData.Add((offsetX + 16) + offsetY*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 8)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 16)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        case QuadSection.BottomLeft:

                            // Remove Triangle
                            indexBufferData.RemoveRange(18, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(18, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(19, (offsetX + 32) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(20, (offsetX + 32) + (offsetY + 24)*mapHeightPlus1);

                            indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 24)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 32) + (offsetY + 32)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Right:
                    switch (section)
                    {
                        case QuadSection.TopRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(3, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(3, offsetX + offsetY*mapHeightPlus1);
                            indexBufferData.Insert(4, (offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(5, offsetX + (offsetY + 8)*mapHeightPlus1);

                            indexBufferData.Add(offsetX + (offsetY + 8)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 16) + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Add(offsetX + (offsetY + 16)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        case QuadSection.BottomRight:

                            // Remove Triangle
                            indexBufferData.RemoveRange(15, 3);
                            leafPatch.NumTris -= 1;

                            // Update IndicesData Array to include new Triangles to fill up the cracks.                                
                            //
                            indexBufferData.Insert(15, offsetX + (offsetY + 16)*mapHeightPlus1);
                            indexBufferData.Insert(16, (offsetX + 16) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Insert(17, offsetX + (offsetY + 24)*mapHeightPlus1);

                            indexBufferData.Add(offsetX + (offsetY + 24)*mapHeightPlus1);
                            indexBufferData.Add((offsetX + 16) + (offsetY + 32)*mapHeightPlus1);
                            indexBufferData.Add(offsetX + (offsetY + 32)*mapHeightPlus1);

                            // Update Triangle Count
                            leafPatch.NumTris += 2;

                            // Copy Array in to IndexBuffer
                            leafPatch.IndexBuffers.SetData(indexBufferData.ToArray());
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
