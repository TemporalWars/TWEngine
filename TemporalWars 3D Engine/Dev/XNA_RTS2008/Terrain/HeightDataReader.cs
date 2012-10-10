#region File Description
//-----------------------------------------------------------------------------
// HeightDataReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    /// <summary>
    /// Content pipeline support class for loading <see cref="TWEngine.Terrain"/> HeightData.
    /// </summary>
    public class HeightDataReader : ContentTypeReader<TerrainHeightData>
    {
        /// <summary>
        /// Reads HeightData from an XNB file.
        /// </summary>
        protected sealed override TerrainHeightData Read(ContentReader input,
                                               TerrainHeightData existingInstance)
        {
            return new TerrainHeightData(input);
        }
    }
}
