#region File Description
//-----------------------------------------------------------------------------
// InstancingTechnique.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    // 9/19/2008 - Add the new 'ShaderInstancingTwoPass' Alpha draw mode.
    // 10/28/2008 - Add Enum Numbers, which correspond to the Effect.Techniques 
    //              Position in the Array, which are determine by the order in
    //              the Shader 'InstancedModel' file.  This is to eliminate the Boxing
    //              which was occuring when using the Enum.ToString name.    
    /// <summary>
    /// <see cref="InstancingTechnique"/> Enum describes the various possible techniques
    /// that can be chosen to implement instancing.
    /// </summary>
    public enum InstancingTechnique : short
    {
// ReSharper disable InconsistentNaming
        ///<summary>
        /// Hardware instancing with Alpha draw capabilities.
        ///</summary>
        /// <remarks>
        /// Used to draw items, like plants and trees, which require the
        /// alpha channel to be used for transparency.
        /// </remarks>
        HardwareInstancingAlphaDraw = 7,
        ///<summary>
        /// Hardware instancing, which is the default choice on Pc.
        ///</summary>
        HardwareInstancing = 8,
// ReSharper restore InconsistentNaming
        ///<summary>
        /// For Test Purposes. (Non-Production)
        ///</summary>
        FogOfWarEmptyShader = 3 // 1/30/2010 - was 'NoInstancing', but now changed to FOW version.
    }
}


