#region File Description
//-----------------------------------------------------------------------------
// HeightMapInfoReader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Spacewar.Terrain
{
    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    public class HeightMapInfoReader : ContentTypeReader<HeightMapInfo>
    {
        protected sealed override HeightMapInfo Read(ContentReader input,
            HeightMapInfo existingInstance)
        {
            float terrainScale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();
            var heights = new float[width, height];
            var normals = new Vector3[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heights[x, z] = input.ReadSingle();
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    normals[x, z] = input.ReadVector3();
                }
            }
            // Added Verticies Array for Pick Triangle Routines
            int verticiesCount = input.ReadInt32();
            var verticies = new Vector3[verticiesCount];
            for (int i = 0; i < verticiesCount; i++)
            {
                verticies[i] = input.ReadVector3();
            }

            return new HeightMapInfo(heights, normals, verticies, terrainScale);
        }
    }
}
