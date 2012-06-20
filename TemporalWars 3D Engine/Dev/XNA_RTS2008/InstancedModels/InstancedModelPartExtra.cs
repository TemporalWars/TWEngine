#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPartExtra.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Shadows;
using TWEngine.Terrain;
using TWEngine.Utilities;
using TWEngine.Utilities.Enums;

namespace TWEngine.InstancedModels
{
    ///<summary>
    ///</summary>
    public class InstancedModelPartExtra
    {
        // Stores meshPart texture refs.
        internal InstancedModelTextures InstancedModelTextures;

        // Stores Asset game from ContentReader.
        internal string AssetName;

        /// <summary>
        /// Procedural Material ID, used to know which material lighting type to use.
        /// </summary>
        private int _proceduralMaterialId = 2; // default to 'Blinn'.

        // Bone rotations on the GPU.
        internal readonly bool BoneRotates;
        internal readonly Vector3 BoneRotationData = Vector3.Zero;

        internal Effect MpEffect;


        internal EffectTechnique ShadowMapHwTechnique; // 12/1/2008
        internal EffectTechnique ShadowMapHwAlphaTechnique; // 6/4/2009
        internal EffectTechnique ShadowMapTechnique;
        // 6/11/2010
        internal EffectTechnique HwTechnique;
        internal EffectTechnique HwAlphaTechnique;
        // 11/17/2008 - Added EffectParameters & EffectTechniques
        // ReSharper disable UnaccessedField.Local
        internal EffectParameter NormalMapTextureParam;
        internal EffectParameter SpecularMapTextureParam; // 2/10/2009
        internal EffectParameter IllumMapTextureParam; // 2/1/2009
        // ReSharper restore UnaccessedField.Local
        internal EffectParameter OscillateIllumParam; // 2/2/2009
        internal EffectParameter OscillateSpeedParam; // 2/2/2009
        internal EffectParameter IllumColorParam; // 2/2/2009
        internal EffectParameter BoneRotatesParam; // 2/10/2009
        internal EffectParameter BoneRotationDataParam; // 2/10/2009 
        internal EffectParameter IsExplosionPieceEp; // 6/11/2010
        internal EffectParameter TimeEp; // 6/12/2010
        internal EffectParameter AccumElapsedTimeEp; // 6/12/2010
        internal EffectParameter InstanceTransformsEp; // 6/12/2010
        internal EffectParameter InstancedTransformsPlayerNumbersEp; // 6/12/2010
        internal EffectParameter InstancedTransformsPVelocityEp; // 6/12/2010
        internal EffectParameter LightViewProj22Ep; // 6/13/2010
        internal EffectParameter ShadowMapTextureEp; // 6/13/2010
        internal EffectParameter TerrainShadowMapEp; // 6/13/2010
        internal EffectParameter LightViewProjEp; // 6/14/2010
        internal EffectParameter LightViewProjStaticEp; // 6/14/2010

        // 2/12/2010 -
        /// <summary>
        ///  Wood Noise texture, used for the 'Wood' material.
        /// </summary>
        private static Texture3D _woodNoiseTexture;
        // 2/14/2010 - 
        /// <summary>
        /// Reflective texture, used for some of the 'Metal' materials.
        /// </summary>
        private static TextureCube _reflectiveCubeTexture;
        // 2/14/2010 -
        /// <summary>
        ///  Ambient texture, used for some of the materials.
        /// </summary>
        private static TextureCube _ambientCubeTexture;

        // 1/25/2010
        internal bool UseIllumMap;
        internal bool UseNormalMap;
        internal bool UseSpecularMap;
        internal bool UseWind; // 3/26/2011

        internal bool EffectParamsSet;
        internal bool EffectStaticParamsSet; // 7/22/2009
        // Track whether effect.CurrentTechnique is dirty.
        internal bool TechniqueChanged;

        #region Properties

        /// <summary>
        /// Used to apply a specific Procedural MaterialId to the given model part.
        /// Reference the 'LightingShader.HLSL' file for specific material Ids.
        /// </summary>
        public int ProceduralMaterialId
        {
            get { return _proceduralMaterialId; }
            set
            {
                _proceduralMaterialId = value;

#if !XBOX360
                // 2/12/2010 - Update 'Effect' to use.
                InstancedModelCustomMaterials.UpdateShaderToUse((ShaderToUseEnum)value, MpEffect);

                // Force Rebinding to new Effect
                TechniqueChanged = true;
                EffectParamsSet = false;
                EffectStaticParamsSet = false;

                // 6/18/2010 - Init Effect
                InitializeEffectAndEffectParams(MpEffect);

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                // Commit changes
                // Effect.CommitChanges();
#endif
            }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input">Instance of <see cref="ContentReader"/></param>
        internal InstancedModelPartExtra(ContentReader input)
        {
            // Read Textures
            InstancedModelTextures = new InstancedModelTextures(input);

            // Read in the "ProceduralMaterial' ID, used for material lighting type.
            var proceduralMaterialId = input.ReadInt32();
            _proceduralMaterialId = (proceduralMaterialId != -1) ? proceduralMaterialId : 2;

            // Read the BoneAnimationAtts
            {
                var boneRotates1 = input.ReadBoolean();
                if (boneRotates1) // BoneAtts-1
                {
                    var packedVector = input.ReadObject<PackedVector3>();
                    packedVector.UnPackVector3(out BoneRotationData);
                }

                var boneRotates2 = input.ReadBoolean();
                if (boneRotates2) // BoneAtts-2
                {
                    var packedVector = input.ReadObject<PackedVector3>();
                    packedVector.UnPackVector3(out BoneRotationData);
                }

                if (boneRotates1 || boneRotates2)
                    BoneRotates = true;
            }     

            // Store AssetName
            AssetName = input.AssetName;

            // Read Effect
            input.ReadSharedResource<Effect>(InitializeEffectAndEffectParams);
        }

        // 6/14/2010
        /// <summary>
        /// Named delegate for the ReadSharedResource callback, which waits for the
        /// creation of the <see cref="MpEffect"/> shader file, and then makes a clone copy
        /// for this specific <see cref="InstancedModelPart"/>.  Finally, all <see cref="EffectParameter"/>
        /// references are set here.
        /// </summary>
        /// <param name="effect"><see cref="MpEffect"/> instance</param>
        internal void InitializeEffectAndEffectParams(Effect effect)
        {
            // 1/17/2010 - Note: Updated to clone the effect, so atts changes don't stomp on the other models effect instances!
            MpEffect = effect.Clone();

            // 6/18/2010 - Initizalies the STATIC render states.
            MpEffect.Parameters["xWorld"].SetValue(Matrix.Identity); // 1/16/2009 
            InstancedModelPart.ViewParam = MpEffect.Parameters["View"];
            InstancedModelPart.ProjectionParam = MpEffect.Parameters["Projection"];
           
            // 3/19/2011 - Iterate InstancedTextures collection and apply 'Texture'.
            Texture textureToApply;
            if (InstancedModelTextures.TexturesDictionary.TryGetValue("Texture", out textureToApply))
            {
                /*var testRedTexture = new Texture2D(_graphicsDevice, ((Texture2D) textureToApply).Width,
                                                   ((Texture2D) textureToApply).Height);

                var tmpData = new Color[testRedTexture.Height * testRedTexture.Width];
                testRedTexture.GetData(tmpData);

                for (int i = 0; i < tmpData.Length; i++)
                {
                    tmpData[i].R = 255;
                }

                testRedTexture.SetData(tmpData);*/

                // TODO: DEBUG
                /*if (_assetName == "sciFiHeli01" || _assetName == "sciFiHeli02")
                {
                    Storage.SaveTexture(((Texture2D)textureToApply), ImageFileFormat.Jpeg, string.Format(@"C:\Downloads\{0}", _assetName));
                }*/
               
                //MpEffect.Parameters["Texture"].SetValue(textureToApply);
            }

      

            // Regular
            HwTechnique = MpEffect.Techniques["HardwareInstancing"];
            HwAlphaTechnique = MpEffect.Techniques["HardwareInstancingAlphaDraw"];

            // Shadows
            ShadowMapTechnique = MpEffect.Techniques["ShadowMapRender"];
            ShadowMapHwTechnique = MpEffect.Techniques["ShadowMapHWRender"]; // 12/1/2008
            ShadowMapHwAlphaTechnique = MpEffect.Techniques["ShadowMapHWAlphaRender"]; // 6/4/2009

            // 6/12/2010 - Set Time EffectParams
            TimeEp = MpEffect.Parameters["xTime"];
            AccumElapsedTimeEp = MpEffect.Parameters["xAccumElapsedTime"];
            IsExplosionPieceEp = MpEffect.Parameters["xIsExplosionPiece"]; // 6/14/2010

            // 6/12/2010 - Set Xbox EffectParams
            InstanceTransformsEp = MpEffect.Parameters["InstanceTransforms"];
            InstancedTransformsPlayerNumbersEp = MpEffect.Parameters["InstanceTransforms_PlayerNTime"];
            InstancedTransformsPVelocityEp = MpEffect.Parameters["InstanceTransforms_PVelocity"];
            // 6/13/2010 - Additional EffectParams
            LightViewProj22Ep = MpEffect.Parameters["xLightViewProjection22"];
            ShadowMapTextureEp = MpEffect.Parameters["ShadowMapTexture"];
            TerrainShadowMapEp = MpEffect.Parameters["TerrainShadowMap"];
            // 6/14/2010 - Additional EffectParams
            LightViewProjEp = MpEffect.Parameters["xLightViewProjection"];
            LightViewProjStaticEp = MpEffect.Parameters["xLightViewProjection_Static"];

            //if (_assetName.StartsWith("plantsNewWeeds"))
              //  Debugger.Break();

            UseIllumMap = MpEffect.Parameters["oUseIllumMap"].GetValueBoolean(); // 1/25/2010
            UseNormalMap = MpEffect.Parameters["oUseNormalMap"].GetValueBoolean(); // 3/20/2011
            UseSpecularMap = MpEffect.Parameters["oUseSpecularMap"].GetValueBoolean(); // 3/20/2011
            UseWind = MpEffect.Parameters["oUseWind"].GetValueBoolean(); // 3/26/2011

            // 6/14/2010 - Moved the following code from the SetRender
            MpEffect.Parameters["xWorldI"].SetValue(Matrix.Invert(Matrix.Identity)); // 1/19/2010  
            NormalMapTextureParam = MpEffect.Parameters["NormalMapTexture"];
            IllumMapTextureParam = MpEffect.Parameters["IllumMapTexture"]; // 2/1/2009

            // 8/28/2009
            OscillateIllumParam = MpEffect.Parameters["xOscillateIllum"]; // 2/2/2009
            OscillateSpeedParam = MpEffect.Parameters["xOscillateSpeed"]; // 2/2/2009
            IllumColorParam = MpEffect.Parameters["xIllumColor"]; // 2/2/2009
            BoneRotatesParam = MpEffect.Parameters["xBoneRotates"]; // 2/10/2009
            BoneRotationDataParam = MpEffect.Parameters["xBoneRotationData"]; // 2/10/2009           
            SpecularMapTextureParam = MpEffect.Parameters["SpecularMapTexture"]; // 2/10/2009 


            // 6/26/2009 - Set the PCF Samples
            MpEffect.Parameters["PCFSamples"].SetValue(ShadowMap.PcfSamples);

            //_oscillateSpeedParam.SetValue(AttsData.oscillateSpeed);
            //_oscillateIllumParam.SetValue(AttsData.oscillateIllum); // 2/2/2009 
            //_illumColorParam.SetValue(AttsData.illumColor.ToVector4()); // 2/2/2009

            // 2/10/2009 - Set Bone RotationData onto the GPU
            BoneRotatesParam.SetValue(BoneRotates);
            BoneRotationDataParam.SetValue(BoneRotationData);

            // 4/20/2010 - Cache values
            var lightPosEp = MpEffect.Parameters["xLightPos"];
            var depthBiasEp = MpEffect.Parameters["xDepthBias"];
            var enableShadowsEp = MpEffect.Parameters["xEnableShadows"];
            var halfPixelEp = MpEffect.Parameters["xHalfPixel"];
            var lightPosition = TerrainShape.LightPosition;
            var shadowMapDepthBias = ShadowMap.ShadowMapDepthBias;
            var isVisible = ShadowMap.IsVisibleS;
            var halfPixel = ShadowMap.HalfPixel;

            if (lightPosEp != null) lightPosEp.SetValue(lightPosition); // 2/11/2010
            if (depthBiasEp != null) depthBiasEp.SetValue(shadowMapDepthBias); // 6/4/2009
            if (enableShadowsEp != null) enableShadowsEp.SetValue(isVisible); // 2/1/2009
            if (halfPixelEp != null) halfPixelEp.SetValue(halfPixel);
        }

        // 3/20/2011
        internal class InstancedModelCustomMaterials
        {
#if !XBOX360
            private const string ShaderToUseLocation = @"Shaders\InstancedModel\"; // 2/12/2010 - Shader's path location


            // 2/12/2010; 2/14/2010; 6/18/2010
            /// <summary>
            /// Updates the current <see cref="MpEffect"/>, by loading a new one using the given <see cref="ShaderToUseEnum"/> Enum.
            /// Also, any resources needed for the particular material, like 'Reflective' texture, will be loaded
            /// and set to the new <see cref="MpEffect"/>.
            /// </summary>
            /// <param name="proceduralMaterialId"><see cref="ShaderToUseEnum"/> Enumerator</param>
            /// <param name="effect"></param>
            internal static void UpdateShaderToUse(ShaderToUseEnum proceduralMaterialId, Effect effect)
            {
                // Retrieve file 'Shader' name to load
                var shaderToUseName = GetShaderToUseName(proceduralMaterialId);

                // Retrieve current 'Diffuse', 'Normal', 'Illum' & 'Specular' textures from effect.
                var diffuseTexture = effect.Parameters.GetParameterBySemantic("DIFFUSEMAP").GetValueTexture2D();
                var normalTexture = effect.Parameters.GetParameterBySemantic("NORMAL").GetValueTexture2D();
                var illumTexture = effect.Parameters.GetParameterBySemantic("ENVMAP").GetValueTexture2D();
                var specularTexture = effect.Parameters.GetParameterBySemantic("SPECULARMAP").GetValueTexture2D();

                // Load new 'Shader' material
                //var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
                effect = TemporalWars3DEngine.ContentMisc.Load<Effect>(ShaderToUseLocation + shaderToUseName);

                // Reset the 4 main Texture Maps!
                if (diffuseTexture != null) effect.Parameters.GetParameterBySemantic("DIFFUSEMAP").SetValue(diffuseTexture);
                if (normalTexture != null) effect.Parameters.GetParameterBySemantic("NORMAL").SetValue(normalTexture);
                if (illumTexture != null) effect.Parameters.GetParameterBySemantic("ENVMAP").SetValue(illumTexture);
                if (specularTexture != null) effect.Parameters.GetParameterBySemantic("SPECULARMAP").SetValue(specularTexture);

                // Update specific resources, depending on material
                switch (proceduralMaterialId)
                {
                    case ShaderToUseEnum.Blinn:
                        break;
                    case ShaderToUseEnum.Metal:
                        break;
                    case ShaderToUseEnum.Plastic:
                        break;
                    case ShaderToUseEnum.Glossy:
                        break;
                    case ShaderToUseEnum.Phong:
                        break;
                    case ShaderToUseEnum.PhongRed:
                        break;
                    case ShaderToUseEnum.PhongFlashWhite:
                        break;
                    case ShaderToUseEnum.ColorBlend:
                        break;
                    case ShaderToUseEnum.FresnelTerm:
                        break;
                    case ShaderToUseEnum.Saturation:
                        break;
                    case ShaderToUseEnum.CustomMetal:
                        // Load Ambient cube texture required for this material
                        _ambientCubeTexture = TemporalWars3DEngine.ContentMisc.Load<TextureCube>(@"Shaders\c_bensBackyard16D");
                        if (_ambientCubeTexture != null) effect.Parameters["_mpAmbientCubeTexture"].SetValue(_ambientCubeTexture);
                        // Load Reflective cube texture required for this material
                        _reflectiveCubeTexture = TemporalWars3DEngine.ContentMisc.Load<TextureCube>(@"Shaders\sunol_cubemap");
                        if (_reflectiveCubeTexture != null) effect.Parameters["_mpReflectCubeTexture"].SetValue(_reflectiveCubeTexture);
                        break;
                    case ShaderToUseEnum.ReflectiveMetal:
                        // Load Reflective cube texture required for this material
                        _reflectiveCubeTexture = TemporalWars3DEngine.ContentMisc.Load<TextureCube>(@"Shaders\sunol_cubemap");
                        if (_reflectiveCubeTexture != null) effect.Parameters["_mpReflectCubeTexture"].SetValue(_reflectiveCubeTexture);
                        break;
                    case ShaderToUseEnum.Velvety:
                        break;
                    case ShaderToUseEnum.Wood:
                        _woodNoiseTexture = TemporalWars3DEngine.ContentMisc.Load<Texture3D>(@"Shaders\noiseL8");
                        // If 'Wood' material, then update Noise 
                        if (_woodNoiseTexture != null) effect.Parameters["_mpNoise3DTex"].SetValue(_woodNoiseTexture);
                        break;
                    default:
                        break;
                }


               

            }

            // 2/14/2010 
            // Note: Any changes made here, also need to be done in the 'InstancedModelProcessors' version!
            /// <summary>
            /// Specifically returns the Name of the shader to load, depending on the <see cref="ShaderToUseEnum"/> enumeration.
            /// </summary>
            /// <param name="proceduralMaterialId"><see cref="ShaderToUseEnum"/> Enumerator</param>
            /// <returns>String name of shader to load</returns>
            private static string GetShaderToUseName(ShaderToUseEnum proceduralMaterialId)
            {
                string shaderToUseName;
                switch (proceduralMaterialId)
                {
                    case ShaderToUseEnum.Plastic:
                        shaderToUseName = "InstancedModel_Plastic";
                        break;
                    case ShaderToUseEnum.Metal:
                        shaderToUseName = "InstancedModel_Metal";
                        break;
                    case ShaderToUseEnum.Blinn:
                        shaderToUseName = "InstancedModel_Blinn";
                        break;
                    case ShaderToUseEnum.Glossy:
                        shaderToUseName = "InstancedModel_Glossy";
                        break;
                    case ShaderToUseEnum.Phong:
                        shaderToUseName = "InstancedModel_Phong";
                        break;
                    case ShaderToUseEnum.PhongRed:
                        shaderToUseName = "InstancedModel_PhongRed";
                        break;
                    case ShaderToUseEnum.PhongFlashWhite:
                        shaderToUseName = "InstancedModel_PhongFlashWhite";
                        break;
                    case ShaderToUseEnum.ColorBlend:
                        shaderToUseName = "InstancedModel_ColorBlend";
                        break;
                    case ShaderToUseEnum.FresnelTerm:
                        shaderToUseName = "InstancedModel_FresnelTerm";
                        break;
                    case ShaderToUseEnum.Saturation:
                        shaderToUseName = "InstancedModel_Saturation";
                        break;
                    case ShaderToUseEnum.CustomMetal:
                        shaderToUseName = "InstancedModel_CustomMetal";
                        break;
                    case ShaderToUseEnum.ReflectiveMetal:
                        shaderToUseName = "InstancedModel_ReflectiveMetal";
                        break;
                    case ShaderToUseEnum.Velvety:
                        shaderToUseName = "InstancedModel_Velvety";
                        break;
                    case ShaderToUseEnum.Wood:
                        shaderToUseName = "InstancedModel_Wood";
                        break;
                    default:
                        shaderToUseName = "InstancedModel_Blinn";
                        break;
                }
                return shaderToUseName;
            }

            // 2/4/2010
            /// <summary>
            ///  Sets the given <see cref="ProceduralMaterialParameters"/> parameter, to the given new value.
            /// </summary>
            /// <param name="parameterToUpdate"><see cref="ProceduralMaterialParameters"/> parameter to update</param>
            /// <param name="effect"></param>
            /// <param name="newValue">New value to set</param>
            internal static void SetProceduralMaterialParameter(ProceduralMaterialParameters parameterToUpdate, Effect effect, object newValue)
            {
                // update the requested parameter.
                
                switch (parameterToUpdate)
                {
                    case ProceduralMaterialParameters.DiffuseColor:
                        {
                            var valueVector4 = ((Color)newValue).ToVector4();
                            effect.Parameters["_mpDiffuseColor"].SetValue(valueVector4);
                        }
                        break;
                    case ProceduralMaterialParameters.SpecularColor:
                        {
                            var valueVector4 = ((Color)newValue).ToVector4();
                            effect.Parameters["_mpSpecularColor"].SetValue(valueVector4);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscColor:
                        {
                            var valueVector4 = ((Color)newValue).ToVector4();
                            effect.Parameters["_mpMiscColor"].SetValue(valueVector4);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat1:
                        {
                            var valueFloat = (float)newValue;
                            effect.Parameters["_mpMiscFloat1"].SetValue(valueFloat);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat2:
                        {
                            var valueFloat = (float)newValue;
                            effect.Parameters["_mpMiscFloat2"].SetValue(valueFloat);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat3:
                        {
                            var valueFloat = (float)newValue;
                            effect.Parameters["_mpMiscFloat3"].SetValue(valueFloat);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat4:
                        {
                            var valueFloat = (float)newValue;
                            effect.Parameters["_mpMiscFloat4"].SetValue(valueFloat);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx2_5:
                        {
                            var valueVector2 = (Vector2)newValue;
                            effect.Parameters["_mpMiscFloatx2_5"].SetValue(valueVector2);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx4_6:
                        {
                            var valueVector4 = (Vector4)newValue;
                            effect.Parameters["_mpMiscFloatx4_6"].SetValue(valueVector4);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx4_7:
                        {
                            var valueVector4 = (Vector4)newValue;
                            effect.Parameters["_mpMiscFloatx4_7"].SetValue(valueVector4);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("parameterToUpdate");
                }

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                // Commit changes
                //effect.CommitChanges();
            }

            // 2/10/2010
            /// <summary>
            /// Gets the given <see cref="ProceduralMaterialParameters"/> parameter.
            /// </summary>
            /// <param name="parameterToRetrieve"><see cref="ProceduralMaterialParameters"/> parameter to retrieve</param>
            /// <param name="effect"></param>
            /// <param name="value">(OUT) Value retrieved</param>
            /// <param name="type">(OUT) value Type</param>
            internal static void GetProceduralMaterialParameter(ProceduralMaterialParameters parameterToRetrieve, Effect effect, out object value, out Type type)
            {
                switch (parameterToRetrieve)
                {
                    case ProceduralMaterialParameters.DiffuseColor:
                        {
                            value = effect.Parameters["_mpDiffuseColor"].GetValueVector4();
                            type = typeof(Color);
                        }
                        break;
                    case ProceduralMaterialParameters.SpecularColor:
                        {
                            value = effect.Parameters["_mpSpecularColor"].GetValueVector4();
                            type = typeof(Color);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscColor:
                        {
                            value = effect.Parameters["_mpMiscColor"].GetValueVector4();
                            type = typeof(Color);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat1:
                        {
                            value = effect.Parameters["_mpMiscFloat1"].GetValueSingle();
                            type = typeof(float);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat2:
                        {
                            value = effect.Parameters["_mpMiscFloat2"].GetValueSingle();
                            type = typeof(float);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat3:
                        {
                            value = effect.Parameters["_mpMiscFloat3"].GetValueSingle();
                            type = typeof(float);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloat4:
                        {
                            value = effect.Parameters["_mpMiscFloat4"].GetValueSingle();
                            type = typeof(float);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx2_5:
                        {
                            value = effect.Parameters["_mpMiscFloatx2_5"].GetValueVector2();
                            type = typeof(Vector2);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx4_6:
                        {
                            value = effect.Parameters["_mpMiscFloatx4_6"].GetValueVector4();
                            type = typeof(Vector4);
                        }
                        break;
                    case ProceduralMaterialParameters.MiscFloatx4_7:
                        {
                            value = effect.Parameters["_mpMiscFloatx4_7"].GetValueVector4();
                            type = typeof(Vector4);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("parameterToRetrieve");
                }
            }

#endif
        } // End Internal Class

    }
   
   
}
