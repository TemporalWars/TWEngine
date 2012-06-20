#region File Description
//-----------------------------------------------------------------------------
// PhysXHeightField.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
#if !XBOX360 // 6/22/2009
using StillDesign.PhysX;
using TWEngine.Terrain;
using TWEngine.Terrain.Enums;

#endif

namespace TWEngine.PhysX
{
    // 6/23/2009
    class PhysXHeightField
    {

        // Terrain HeightField
        public static Actor TerrainHeightField;

        // Creates a heightField for the given height data.
        public static void CreateHeightField(float[] heightData, int width, int height, int scale, LOD detail)
        {
            var detailLevel = (int)detail;

            width /= detailLevel;
            height /= detailLevel;

            var samples = new HeightFieldSample[width * height];
            for (int r = 0; r < width; r++)
            {
                for (int c = 0; c < height; c++)
                {                   

                    var sample = new HeightFieldSample
                                     {
                                         Height = (short) heightData[c + r*height],
                                         MaterialIndex0 = 0,
                                         MaterialIndex1 = 1,
                                         TessellationFlag = 0
                                     };

                    samples[c + r * height] = sample;
                }
            }

            var heightFieldDesc = new HeightFieldDescription
                                      {
                NumberOfRows = width,
                NumberOfColumns = height,
                //Samples = samples
            };


            HeightField heightField = PhysXEngine.PhysXCore.CreateHeightField(heightFieldDesc);

            //

            var heightFieldShapeDesc = new HeightFieldShapeDescription
                                           {
                HeightField = heightField,
                HoleMaterial = 2,
                // The max height of our samples is short.MaxValue and we want it to be 1
                HeightScale = 1.0f / 255.0f,
                RowScale = scale,
                ColumnScale = scale
            };
            heightFieldShapeDesc.LocalPosition = new Vector3(-0.5f * width * 1 * heightFieldShapeDesc.RowScale, 0, -0.5f * height * 1 * heightFieldShapeDesc.ColumnScale);
            

            var actorDesc = new ActorDescription
                                {
                GlobalPose = Matrix.CreateTranslation(100, 0, 0),
                Shapes = { heightFieldShapeDesc }
            };

            TerrainHeightField = PhysXEngine.PhysXScene.CreateActor(actorDesc);
           
        }

    }
}
