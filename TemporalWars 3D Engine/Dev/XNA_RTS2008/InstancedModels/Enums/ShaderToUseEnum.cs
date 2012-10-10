#region File Description
//-----------------------------------------------------------------------------
// ShaderToUseEnum.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 2/12/2010 - Enum of Shaders
    // Note: Any changes made here, also need to be done in the 'InstancedModelProcessors' version!
    ///<summary>
    /// Procedural Material type to use.
    ///</summary>
    public enum ShaderToUseEnum
    {
        ///<summary>
        /// Blinn material
        ///</summary>
        Blinn,
        ///<summary>
        /// Metal material
        ///</summary>
        Metal,
        ///<summary>
        /// Plastic material
        ///</summary>
        Plastic,
        ///<summary>
        /// Glossy material
        ///</summary>
        Glossy,
        ///<summary>
        /// Phong material
        ///</summary>
        Phong,
        ///<summary>
        /// Phong-Red material
        ///</summary>
        PhongRed,
        ///<summary>
        /// Phong-Flash-White material
        ///</summary>
        PhongFlashWhite,
        ///<summary>
        /// Color blend material
        ///</summary>
        ColorBlend,
        ///<summary>
        /// FresnelTerm material
        ///</summary>
        FresnelTerm,
        ///<summary>
        /// Saturation material
        ///</summary>
        Saturation,
        ///<summary>
        /// Custom metal material
        ///</summary>
        CustomMetal,
        ///<summary>
        /// Reflective metal material
        ///</summary>
        ReflectiveMetal,
        ///<summary>
        /// Velvety material
        ///</summary>
        Velvety,
        ///<summary>
        /// Wood material
        ///</summary>
        Wood
    }
}