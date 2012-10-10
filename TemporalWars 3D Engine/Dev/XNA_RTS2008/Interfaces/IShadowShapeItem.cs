#region File Description
//-----------------------------------------------------------------------------
// IShadowShapeItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{
    /// <summary>
    /// The <see cref="IShadowShapeItem"/> Interface provides the necessary
    /// properties and methods for creating a <see cref="ShadowMap"/> for any <see cref="Shape"/> item.
    /// </summary>
    public interface IShadowShapeItem
    {               
        ///<summary>
        /// Draws the <see cref="IShadowShapeItem"/> using the <see cref="ShadowMap"/> shader, 
        /// which will project the shadow for this <see cref="SceneItem"/> onto the <see cref="ShadowMap"/>.        
        ///</summary>
        ///<param name="lightView">Light view <see cref="Matrix"/></param>
        ///<param name="lightProjection">Light projection <see cref="Matrix"/></param>
        void DrawForShadowMap(ref Matrix lightView, ref Matrix lightProjection);
        ///<summary>
        /// Set or get reference to XNA <see cref="Model"/>.
        ///</summary>
        Model Model { get; set; }

        ///<summary>
        /// Call the <see cref="ShadowItem"/> method <see cref="StoreModelEffect"/>.
        ///</summary>
        ///<param name="model">XNA <see cref="Model"/> instance</param>
        ///<param name="isBasicEffect">Is <see cref="BasicEffect"/>?</param>
        void StoreModelEffect(ref Model model, bool isBasicEffect);

        /// <summary>
        /// The World <see cref="Matrix"/>
        /// </summary>
        Matrix WorldP { get; set; }

        ///<summary>
        /// Item cast shadow?
        ///</summary>
        bool ModelCastShadow { get; set; }

        /// <summary>
        /// Gets or Sets if model animates?
        /// </summary>
        bool ModelAnimates { get; set; }
        ///<summary>
        /// Item in <see cref="Camera"/> frustrum?
        ///</summary>
        bool InCameraFrustrum { get; set; }
    }
}