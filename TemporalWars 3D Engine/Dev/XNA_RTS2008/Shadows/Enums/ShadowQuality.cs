#region File Description
//-----------------------------------------------------------------------------
// ShadowQuality.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.Shadows.Enums
{
    // NOTE: (8/17/2010) Anything changed or added to this ShadowQuality, 
    // NOTE:  MUST be done to Enum ShadowQuality in TWTerrainToolsWPF.
    // 12/6/2009
    /// <summary>
    /// The <see cref="ShadowQuality"/> Enum controls the level
    /// of detail used for shadows; specifically, the size of the
    /// <see cref="RenderTarget"/> used when creating the shadow maps.
    /// </summary>
    public enum ShadowQuality
    {
        ///<summary>
        /// Low = 1024x for <see cref="RenderTarget"/>
        ///</summary>
        Low,
        ///<summary>
        /// Med = 2048x for <see cref="RenderTarget"/>
        ///</summary>
        Medium,
        ///<summary>
        /// High = 4096x for <see cref="RenderTarget"/>
        ///</summary>
        High
    }
}