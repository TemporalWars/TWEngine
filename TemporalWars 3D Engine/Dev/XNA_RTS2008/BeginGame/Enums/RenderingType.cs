#region File Description
//-----------------------------------------------------------------------------
// RenderingType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.BeginGame.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.BeginGame.Enums"/> namespace contains the common enumerations
    /// which make up the entire <see cref="TWEngine.BeginGame"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="RenderingType"/> Enum is used to set the type of 
    /// rendering method to use; for example, 'Deferred' rendering.
    ///</summary>
    public enum RenderingType : short
    {
        ///<summary>
        /// Renders game using the Deferred rendering method.
        ///</summary>
        DeferredRendering,
        ///<summary>
        /// Renders game using the normal Forward rendering method.
        ///</summary>
        NormalRendering,
        ///<summary>
        /// Renders game using the normal Forward rendering method, 
        /// with the 2nd render pass for PostProcess effects.
        ///</summary>
        NormalRenderingWithPostProcessEffects, // 6/6/2009
       
    }
}