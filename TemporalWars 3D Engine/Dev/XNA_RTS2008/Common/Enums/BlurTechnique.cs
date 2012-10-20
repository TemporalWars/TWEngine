#region File Description
//-----------------------------------------------------------------------------
// BlurTechnique.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Common.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Common.Enums"/> namespace contains the common enumerations
    /// which make up the entire <see cref="TWEngine.Common"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// Enumeration of Blur Technique to use.
    ///</summary>
    public enum BlurTechnique : short
    {
        ///<summary>
        /// plain color
        ///</summary>
        Color = 0,              // plain color
        ///<summary>
        /// plain texture mapping
        ///</summary>
        ColorTexture,           // plain texture mapping
        ///<summary>
        /// horizontal blur
        ///</summary>
        BlurHorizontal,         // horizontal blur
        ///<summary>
        /// vertical blur
        ///</summary>
        BlurVertical,           // vertical blur
        ///<summary>
        /// horizontal split screen blur
        ///</summary>
        BlurHorizontalSplit,    // horizontal split screen blur
       
    }
}