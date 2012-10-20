#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPartExtraLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders
{
    /// <summary>
    /// The <see cref="InstancedModelPartExtraLoader"/> is used to abtract loading of the InstancedModel artwork, separate from the game engine.
    /// This allows changes to the main Game Engine's versions number and strong name, without breaking the loading of the arwork.
    /// </summary>
    public class InstancedModelPartExtraLoader
    {
        public InstancedModelTexturesLoader InstancedModelTexturesLoader;
        public int ProceduralMaterialId = 2;
        public string AssetName;
        // Bone rotations on the GPU.
        public readonly bool BoneRotates;
        public readonly Vector3 BoneRotationData = Vector3.Zero;
        public Effect MpEffect;

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input">Instance of <see cref="ContentReader"/></param>
        internal InstancedModelPartExtraLoader(ContentReader input)
        {
            // Read Textures
            InstancedModelTexturesLoader = new InstancedModelTexturesLoader(input);

            // Read in the "ProceduralMaterial' ID, used for material lighting type.
            var proceduralMaterialId = input.ReadInt32();
            ProceduralMaterialId = (proceduralMaterialId != -1) ? proceduralMaterialId : 2;

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
        /// for this specific InstancedModelPart.  Finally, all <see cref="EffectParameter"/>
        /// references are set here.
        /// </summary>
        /// <param name="effect"><see cref="MpEffect"/> instance</param>
        internal void InitializeEffectAndEffectParams(Effect effect)
        {
            // 1/17/2010 - Note: Updated to clone the effect, so atts changes don't stomp on the other models effect instances!
            MpEffect = effect.Clone();
        }
    }
}
