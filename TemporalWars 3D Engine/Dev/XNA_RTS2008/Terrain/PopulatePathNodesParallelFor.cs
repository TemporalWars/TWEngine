#region File Description
//-----------------------------------------------------------------------------
// PopulatePathNodesParallelFor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWTools.ParallelTasksComponent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    /// <summary>
    /// Populates the PathNodes collection, which is used to show the PathNodes in the A* class
    /// on the terrain; for debug purposes only. 
    /// </summary>
    /// <remarks> This class inherits from the <see cref="AbstractParallelFor"/> thread class.</remarks>
    class PopulatePathNodesParallelFor : AbstractParallelFor
    {
        private static List<VertexPositionColor> _tmpPathNodesThread;

        private volatile int _yStride;
        private volatile int _yIndex;
        const int TemporalWarsGamepathNodeStride = TemporalWars3DEngine._pathNodeStride;

        private readonly Color _whiteColor = Color.White;
        private readonly Color _redColor = Color.Red;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        /// <summary>
        /// Used to Parallize a For-Loop, to run on 4 separate processors.
        /// </summary>
        /// <param name="pathNodes">(OUT) collection of <see cref="VertexPositionColor"/> structs</param>
        public void ParallelFor(out List<VertexPositionColor> pathNodes)
        {
            // 5/24/2010: Refactored core code to new STATIC method.
             GetPathNodes(this, out pathNodes);
        }

        // 5/24/2010
        /// <summary>
        /// Method helper, which returns a collection of <see cref="VertexPositionColor"/>, used
        /// to draw the A* path nodes.
        /// </summary>
        /// <param name="populatePathNodesParallelFor">this instance of <see cref="PopulatePathNodesParallelFor"/></param>
        /// <param name="pathNodes">(OUT) collection of <see cref="VertexPositionColor"/> structs</param>
        private static void GetPathNodes(PopulatePathNodesParallelFor populatePathNodesParallelFor, out List<VertexPositionColor> pathNodes)
        {
            var size = TemporalWars3DEngine.SPathNodeSize;

            // init temp array.
            var capacity = (size * size * 3) + 2;
            _tmpPathNodesThread = new List<VertexPositionColor>(capacity);
            
            // Populate with empty positions.
            for (var i = 0; i < capacity; i++)
            {
                _tmpPathNodesThread.Add(default(VertexPositionColor));
            }

            pathNodes = new List<VertexPositionColor>(capacity);

            for (var loopY = 0; loopY < size; loopY++)
            {
                // Set YStride for this iteration
                populatePathNodesParallelFor._yStride = loopY * TemporalWarsGamepathNodeStride;
                populatePathNodesParallelFor._yIndex = loopY * size * 3;

                // Parallize the X for-loop
                ParallelFor(populatePathNodesParallelFor, 0, size);
            }

            // copy final results
            pathNodes.AddRange(_tmpPathNodesThread);
            // clear temp array
            _tmpPathNodesThread.Clear();
            
        }

        /// <summary>
        /// Core method for the Body of the For-Loop.  Inheriting classes
        /// MUST override and provide the core 'Body' to the For-Loop logic.
        /// </summary>
        protected override void LoopBody(int index)
        {
            // 2/17/2010 - Multiply by PathNodeStride
            PopulatePathNodeArray(this, index);
        }

        // 2/17/2010; 5/24/2010: Updated to be STATIC method.
        /// <summary>
        /// Populates the PathNode array, at the specified index value in the array.
        /// </summary>
        /// <param name="populatePathNodesParallelFor">this instance of <see cref="PopulatePathNodesParallelFor"/></param>
        /// <param name="index">Index position to populate</param>
        private static void PopulatePathNodeArray(PopulatePathNodesParallelFor populatePathNodesParallelFor, int index)
        {
            var xStride = index * TemporalWarsGamepathNodeStride;
            var yStride = populatePathNodesParallelFor._yStride;

            // ReSharper disable UseObjectOrCollectionInitializer
            var pathNodeTriangle = new VertexPositionColor(Vector3Zero, populatePathNodesParallelFor._whiteColor);
            // ReSharper restore UseObjectOrCollectionInitializer

            // 11/28/2008 - Updated to use the Static 'ContainsKey'.
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 5/17/2010 - Cache
            pathNodeTriangle.Color = aStarGraph != null // 1/13/2010
                                         ? (aStarGraph.ContainsKey(xStride, yStride)
                                                ? populatePathNodesParallelFor._redColor
                                                : populatePathNodesParallelFor._whiteColor)
                                         : populatePathNodesParallelFor._whiteColor;

            // Triangle - Point 1
            pathNodeTriangle.Position.X = xStride;
            pathNodeTriangle.Position.Y = TerrainData.GetTerrainHeight(ref xStride, ref yStride);
            pathNodeTriangle.Position.Z = yStride;
            //_tmpPathNodesThread.Add(pathNodeTriangle);
            _tmpPathNodesThread[populatePathNodesParallelFor._yIndex + index * 3] = pathNodeTriangle;

            // Triangle - Point 2
            pathNodeTriangle.Position.X = xStride + 5;
            pathNodeTriangle.Position.Y = TerrainData.GetTerrainHeight(ref xStride, ref yStride);
            pathNodeTriangle.Position.Z = yStride + 5;
            //_tmpPathNodesThread.Add(pathNodeTriangle);
            _tmpPathNodesThread[populatePathNodesParallelFor._yIndex + (index * 3 + 1)] = pathNodeTriangle;

            // Triangle - Point 3
            pathNodeTriangle.Position.X = xStride - 5;
            pathNodeTriangle.Position.Y = TerrainData.GetTerrainHeight(ref xStride, ref yStride);
            pathNodeTriangle.Position.Z = yStride + 5;
            //_tmpPathNodesThread.Add(pathNodeTriangle);
            _tmpPathNodesThread[populatePathNodesParallelFor._yIndex + (index * 3 + 2)] = pathNodeTriangle;
        }
       
      
    }
}