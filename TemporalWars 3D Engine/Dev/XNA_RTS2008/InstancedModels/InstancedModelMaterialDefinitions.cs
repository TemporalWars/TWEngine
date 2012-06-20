#region File Description
//-----------------------------------------------------------------------------
// InstancedModelMaterialDefinitions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;

namespace TWEngine.InstancedModels
{
    // 2/8/2010
    /// <summary>
    /// Stores the mapping between the materials in the 'MaterialShaders.hlsl' file
    /// to the parameters in the 'Material' tab of the Properties Tool Form.
    /// </summary>
    public static class InstancedModelMaterialDefinitions
    {
        // 
        /// <summary>
        /// stores a reference to the ProceduralMaterialId's <see cref="MaterialDefinition"/>.
        /// </summary>
        private static readonly Dictionary<int, MaterialDefinition> MaterialDefinitions = new Dictionary<int, MaterialDefinition>();

        // static constructor
        /// <summary>
        /// Static constructor, which calls the <see cref="CreateDefaultMaterialDefinitionsAndSave"/> method to initialize
        /// the default materials.
        /// </summary>
        static InstancedModelMaterialDefinitions()
        {
            CreateDefaultMaterialDefinitionsAndSave();
        }

        /// <summary>
        /// Creates the initial 12 default procedural materials, and populates 
        /// into the <see cref="MaterialDefinition"/> dictionary.
        /// </summary>
        public static void CreateDefaultMaterialDefinitionsAndSave()
        {
            //
            // Recreate all default procedural materials (0->)
            //
            // MaterialId#0
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 0,
                    MaterialName = "Blinn Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Specular %", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Highlight Eccentricity", true,
                                             new MaterialSpinnerDef(0, 1, 0.0001f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(0, material);
            }

           
            // MaterialId#1
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 1,
                    MaterialName = "Metal Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Diffuse %", true,
                                             new MaterialSpinnerDef(0, 1, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Reflection %", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(1, material);
            }

            // MaterialId#2
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 2,
                    MaterialName = "Plastic Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Surface Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Diffuse %", true,
                                             new MaterialSpinnerDef(0, 1, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Specular %", true,
                                             new MaterialSpinnerDef(0, 1, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "Reflection %", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "Fresnel Reflection Scale", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f), new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "Misc Floatx4-6", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "Misc Floatx4-7", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(2, material);
            }


           
            // MaterialId#3
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 3,
                    MaterialName = "Glossy Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Specular %", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "GlossDrop", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "Gloss Top/Bot", true,
                     new MaterialSpinnerDef(0.2f, 1, 0.005f), new MaterialSpinnerDef(0.05f, 0.95f, 0.005f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "Surface Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(3, material);
            }

            // MaterialId#4
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 4,
                    MaterialName = "Phong Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Shininess", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(4, material);
            }


            // MaterialId#5
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 5,
                    MaterialName = "PhongRed Lighting",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Shininess", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(5, material);
            }
            // MaterialId#6
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 6,
                    MaterialName = "Flash-White",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambience Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Shininess", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(6, material);
            }
            // MaterialId#7
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 7,
                    MaterialName = "Color Blend",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Diffuse Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Gloss Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Blend Type", true,
                                             new MaterialSpinnerDef(1, 10, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(7, material);
            }
            // MaterialId#8
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 8,
                    MaterialName = "Fresnel Blend",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Diffuse Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Edge Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "SpecularLevel", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "FresnelPower", true,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "FresnelScale", true,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "FresnelBias", true,
                                             new MaterialSpinnerDef(-100, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(8, material);
            }

            // MaterialId#9
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 9,
                    MaterialName = "Saturation",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Diffuse Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambient Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Power of x", true,
                                             new MaterialSpinnerDef(1, 25, 0.5f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(-100, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(9, material);
            }

            // MaterialId#10
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 10,
                    MaterialName = "Custom Metal",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Diffuse Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Ambient Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "POW Constant", true,
                                             new MaterialSpinnerDef(1, 25, 0.5f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(-100, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(10, material);
            }

            // MaterialId#11
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 11,
                    MaterialName = "Reflective Metal",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "None", false,
                                             new MaterialSpinnerDef(1, 25, 0.5f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(-100, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(11, material);
            }

            // MaterialId#12
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 12,
                    MaterialName = "Velvety",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "Surface Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Fuzzy Spec Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "SubColor", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "RollOff", true,
                                             new MaterialSpinnerDef(0, 1, 0.05f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "None", false,
                                             new MaterialSpinnerDef(0, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "None", false,
                                             new MaterialSpinnerDef(-100, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "None", false);

                // Add Material to dictionary.
                MaterialDefinitions.Add(12, material);
            }

            // 2/12/2010
            // MaterialId#13
            {
                var material = new MaterialDefinition
                {
                    ProceduralMaterialId = 13,
                    MaterialName = "Wood",

                };
                material.AddMaterialParamDef(ProceduralMaterialParameters.DiffuseColor, "None", false);
                material.AddMaterialParamDef(ProceduralMaterialParameters.SpecularColor, "Specular Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscColor, "Amibent Color", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat1, "Lighter Wood Spec", true,
                                             new MaterialSpinnerDef(0, 2, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat2, "Darker Wood Spec", true,
                                             new MaterialSpinnerDef(0, 2, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat3, "Specular Exp", true,
                                             new MaterialSpinnerDef(1, 128, 1));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloat4, "Ring Scale", true,
                                             new MaterialSpinnerDef(0, 10, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx2_5, "Wobbliness/SizeofNoise", true,
                    new MaterialSpinnerDef(0.01f, 2, 0.01f), new MaterialSpinnerDef(0.01f, 100, 0.01f));
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_6, "WoodColor1", true);
                material.AddMaterialParamDef(ProceduralMaterialParameters.MiscFloatx4_7, "WoodColor2", true);

                // Add Material to dictionary.
                MaterialDefinitions.Add(13, material);
            }
        }


        // 2/8/2010
        /// <summary>
        /// Retrieves a given Procedural <see cref="MaterialDefinition"/> by its <paramref name="materialId"/>.
        /// </summary>
        /// <param name="materialId">Material Id to retrieve</param>
        /// <param name="materialDefinition">(OUT) <see cref="MaterialDefinition"/> structure</param>
        /// <returns>True/False of success</returns>
        public static bool TryGetProceduralMaterialDef(int materialId, out MaterialDefinition materialDefinition)
        {
            // try to retrieve given 'MaterialId'.
            return (MaterialDefinitions.TryGetValue(materialId, out materialDefinition));
        }

    }
}
