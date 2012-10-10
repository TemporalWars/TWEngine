#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemTypeAttributes.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;

namespace ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs
{
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="ScenaryItemTypeAttributes"/> structure, stores all
    /// the necessary attributes used specifically for the <see cref="ScenaryItemScene"/>.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct ScenaryItemTypeAttributes
    {
        ///<summary>
        /// The <see cref="ItemType"/> these <see cref="ScenaryItemTypeAttributes"/> are for.
        ///</summary>
        /// <remarks>Should ONLY be of the <see cref="ScenaryItemScene"/> type.</remarks>
// ReSharper disable InconsistentNaming
        public ItemType itemType;
        ///<summary>
        /// The <see cref="ModelType"/> Enum.
        ///</summary>
        public ModelType modelType;
        ///<summary>
        /// Path name where to load the model from.
        ///</summary>
        public string modelLoadPathName;
        ///<summary>
        /// Should alpha draw?
        ///</summary>
        public bool useTwoDrawMethod;
        ///<summary>
        /// Does this <see cref="ItemType"/> cast a shadow?
        ///</summary>
        public bool useShadowCasting;
        ///<summary>
        /// Should this item block path section during A*?
        ///</summary>
        public bool usePathBlocking;
        ///<summary>
        /// Size of path block area.
        ///</summary>
        /// <remarks>Must answer TRUE to <see cref="usePathBlocking"/>, in order to use.</remarks>
        public int pathBlockValue;
        ///<summary>
        /// Does this item affect <see cref="IFogOfWar"/> visibility?
        ///</summary>
        public bool useFogOfWar;
        ///<summary>
        /// The width area of <see cref="IFogOfWar"/> visibility.
        ///</summary>
        /// <remarks>Must answer TRUE to <see cref="useFogOfWar"/>, in order to use.</remarks>
        public int FogOfWarWidth;
        ///<summary>
        /// The height area of <see cref="IFogOfWar"/> visibility.
        ///</summary>
        /// <remarks>Must answer TRUE to <see cref="useFogOfWar"/>, in order to use.</remarks>
        public int FogOfWarHeight;
        ///<summary>
        /// Does this model have moving bones?
        ///</summary>
        public bool modelAnimates;
        ///<summary>
        /// Bone name which moves.
        ///</summary>
        /// <remarks>Must answer TRUE to <see cref="modelAnimates"/>, in order to use.</remarks>
        public string modelAnimateBoneName;
        // ReSharper restore InconsistentNaming

        // 2/25/2011 - Set Scale
        ///<summary>
        /// Scale or size of the item in the game world.
        ///</summary>
        public float Scale;
    }
}