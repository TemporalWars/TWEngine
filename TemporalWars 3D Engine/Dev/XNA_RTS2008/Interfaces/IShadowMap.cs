#region File Description
//-----------------------------------------------------------------------------
// IShadowMap.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.SceneItems;
using TWEngine.Shadows;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="ShadowMap"/> is used to create the shadow maps of the <see cref="SceneItem"/>
    /// and the <see cref="Terrain"/>.  These are then passed to the objects which request them;  
    /// for example, the <see cref="Terrain"/>.
    /// </summary>
    public interface IShadowMap
    {
        ///<summary>
        /// Draws the <see cref="Terrain"/> using the <see cref="ShadowMap"/> shader <see cref="Effect"/>, which essentially populates the
        /// <see cref="ShadowMap"/> texture with the depth information and is then passed into the normal Draw call later on.
        ///</summary>
        ///<param name="lightPos"><see cref="Vector3"/> as light position</param>
        ///<param name="lightView"><see cref="Matrix"/> as light view</param>
        ///<param name="lightProj"><see cref="Matrix"/> as light projection</param>
        void DrawForShadowMap(ref Vector3 lightPos,
                              ref Matrix lightView,
                              ref Matrix lightProj);
        ///<summary>
        /// Show shadows?
        ///</summary>
        bool IsVisible { get; set; }

#if !XBOX360
        ///<summary>
        /// Allows debuging creation of <see cref="ShadowMap"/>, and updating
        /// of the light source.
        ///</summary>
        bool DebugValues { get; set; }
#endif

    }
}