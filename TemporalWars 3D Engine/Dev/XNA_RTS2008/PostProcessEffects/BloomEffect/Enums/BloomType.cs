#region File Description
//-----------------------------------------------------------------------------
// BloomType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.PostProcessEffects.BloomEffect.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PostProcessEffects.BloomEffect.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="PostProcessEffects.BloomEffect.Enums"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 12/7/2009
    ///<summary>
    /// The <see cref="BloomType"/> Enum allows setting the type
    /// of <see cref="Bloom"/> effect.
    ///</summary>
    public enum BloomType
    {
        ///<summary>
        /// Standard <see cref="Bloom"/> effect.
        ///</summary>
        Default = 0,
        ///<summary>
        /// Soft <see cref="Bloom"/> effect.
        ///</summary>
        Soft = 1,
        ///<summary>
        /// Desaturated <see cref="Bloom"/> effect.
        ///</summary>
        DeSaturated = 2,
        ///<summary>
        /// Saturated <see cref="Bloom"/> effect.
        ///</summary>
        Saturated = 3,
        ///<summary>
        /// Blurry <see cref="Bloom"/> effect.
        ///</summary>
        Blurry = 4,
        ///<summary>
        /// Subtle <see cref="Bloom"/> effect.
        ///</summary>
        Subtle = 5
    }
}