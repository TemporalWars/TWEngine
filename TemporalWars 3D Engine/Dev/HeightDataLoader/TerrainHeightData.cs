#region File Description
//-----------------------------------------------------------------------------
// TerrainHeightData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.HeightDataLoader
{
    ///<summary>
    /// Stores the Terrain maps <see cref="HeightData"/> and <see cref="NormalData"/> collections.
    ///</summary>
    public class TerrainHeightData : IDisposable
    {
        ///<summary>
        /// Heights for the given Terrain map.
        ///</summary>
        public List<float> HeightData;
        ///<summary>
        /// Normals for the given Terrain map.
        ///</summary>
        public Vector3[] NormalData; // 7/10/2009

        // 5/14/2009
        ///<summary>
        /// Terrain map height value.
        ///</summary>
        public int MapHeight { get; private set; }

        // 5/14/2009
        ///<summary>
        /// Terrain map width value.
        ///</summary>
        public int MapWidth { get; private set; }

        /// <summary>
        /// Constructor reads HeightData from the custom XNB format.
        /// </summary>
        /// <param name="input"><see cref="Microsoft.Xna.Framework.Content.ContentReader"/> instance</param>
        public TerrainHeightData(ContentReader input)
        {
            // Get Maps Dimensions
            MapWidth = input.ReadInt32();
            MapHeight = input.ReadInt32();
           
            // Set Array size
            HeightData = new List<float>(MapWidth * MapHeight);

            // 8/12/2009 - Init array
            for (var loopY = 0; loopY < MapHeight; loopY++)
                for (var loopX = 0; loopX < MapWidth; loopX++)
                {
                    HeightData.Add(0);
                }

            // 7/10/2009
            NormalData = new Vector3[MapWidth * MapHeight];

            // Load HeightData
            for (var loopX = 0; loopX < MapWidth; loopX++)
                for (var loopY = 0; loopY < MapHeight; loopY++)
                {
                    HeightData[loopX + loopY*MapHeight] = input.ReadSingle();
                }

            // 7/10/2009 - Load NormalData
            for (var loopX = 0; loopX < MapWidth; loopX++)
                for (var loopY = 0; loopY < MapHeight; loopY++)
                {
                    NormalData[loopX + loopY*MapHeight] = input.ReadVector3();
                }
        }

        // 1/8/2010
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            // Clear Arrays
            if (HeightData != null)
                HeightData.Clear();

            if (NormalData != null)
                Array.Clear(NormalData, 0, NormalData.Length);
        }
    }
}
