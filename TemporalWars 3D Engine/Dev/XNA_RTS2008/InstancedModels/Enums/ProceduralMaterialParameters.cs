#region File Description
//-----------------------------------------------------------------------------
// ProceduralMaterialParameters.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    // 2/4/2010
    /// <summary>
    /// The ProceduralMaterial's optional shared parameters, used
    /// within the different materials; for example, the 'Blinn'
    /// material.
    /// </summary>
    /// <remarks>
    /// Depending on the material used, each parameter represents
    /// different functionality.
    /// </remarks>
    public enum ProceduralMaterialParameters : short
    {
        ///<summary>
        /// Diffuse color parameter
        ///</summary>
        DiffuseColor,
        ///<summary>
        /// Specular color parameter
        ///</summary>
        SpecularColor,
        ///<summary>
        /// Misc color parameter
        ///</summary>
        MiscColor,
        ///<summary>
        /// Misc float#1 parameter
        ///</summary>
        MiscFloat1,
        ///<summary>
        /// Misc float#2 parameter
        ///</summary>
        MiscFloat2,
        ///<summary>
        /// Misc float#3 parameter
        ///</summary>
        MiscFloat3,
        ///<summary>
        /// Misc float#4 parameter
        ///</summary>
        MiscFloat4,
// ReSharper disable InconsistentNaming
        ///<summary>
        /// Misc Float#5 parameter, which
        /// can store floats in X and Y channels.
        ///</summary>
        MiscFloatx2_5,
        ///<summary>
        /// Misc Float#6 parameter, which
        /// can store floats in X, Y, Z, W channels.
        ///</summary>
        MiscFloatx4_6,
        ///<summary>
        /// Misc Float#7 parameter, which
        /// can store floats in X, Y, Z, W channels.
        ///</summary>
        MiscFloatx4_7,
// ReSharper restore InconsistentNaming
    }
}