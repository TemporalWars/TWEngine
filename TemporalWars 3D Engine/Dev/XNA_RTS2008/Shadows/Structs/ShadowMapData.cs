#region File Description
//-----------------------------------------------------------------------------
// ShadowMapData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWEngine.Shadows.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Shadows.Structs"/> namespace contains the common classes
    /// which make up the entire <see cref="TWEngine.TWEngine.Shadows.Structsnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }



    // 6/5/2009
#pragma warning disable 1587
    ///<summary>
    /// The <see cref="ShadowMapData"/> structure is used t\
    /// store <see cref="ShadowMap"/> data, like 'DepthBias'.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable]
#endif
    public struct ShadowMapData
    {
        // 1/21/2011
        ///<summary>
        /// Show shadows?
        ///</summary>
        public bool IsVisible;

        ///<summary>
        /// The <see cref="ShadowMap"/> depth bias value.
        ///</summary>
// ReSharper disable InconsistentNaming
        public float shadowMapDepthBias; // 4/30/2010: If rename, this will affect the load/save.
// ReSharper restore InconsistentNaming
    }
}
