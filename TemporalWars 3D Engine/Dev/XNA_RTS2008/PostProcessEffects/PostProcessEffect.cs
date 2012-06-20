#region File Description
//-----------------------------------------------------------------------------
// PostProcessEffect.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.PostProcessEffects.BloomEffect;

namespace TWEngine.PostProcessEffects
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PostProcessEffects"/> namespace contains the classes
    /// which make up the entire <see cref="PostProcessEffects"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
  
    ///<summary>
    /// The <see cref="PostProcessEffect"/> base abstract class provides 
    /// the foundation for creating some post process effect, like the <see cref="Bloom"/> effect.
    /// Internally, a <see cref="Quadrangle"/> class is used to draw the post process effect to screen.
    ///</summary>
    public abstract class PostProcessEffect : IDisposable
    {
        private readonly Quadrangle _quadrangle;
// ReSharper disable InconsistentNaming
        internal GraphicsDevice _GraphicsDevice;
        private static readonly Vector2 Vector2Zero = Vector2.Zero;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// Retrieves the <see cref="GraphicsDevice"/> instance.
        /// </summary>
        protected GraphicsDevice GraphicsDevice
        {
            get
            {
                return _GraphicsDevice;
            }
        }

        /// <summary>
        /// Constructor, which saves the given <see cref="GraphicsDevice"/> instance, and
        /// creates the <see cref="Quadrangle"/> instance.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        protected PostProcessEffect(GraphicsDevice graphicsDevice)
        {
            _GraphicsDevice = graphicsDevice;
            _quadrangle = Quadrangle.Find(graphicsDevice);
        }

        ///<summary>
        /// Abstract method, used by inherting classes to provide the source <see cref="Texture2D"/>
        /// to affect, and the <see cref="RenderTarget2D"/> to use to draw the post process effect.
        ///</summary>
        ///<param name="sourceTexture"><see cref="Texture2D"/> instance</param>
        ///<param name="result"><see cref="RenderTarget2D"/> to return the final post process effect</param>
        public abstract void PostProcess(Texture2D sourceTexture, RenderTarget2D result);

        /// <summary>
        /// Helper method, used to return the given <see cref="RenderTarget2D"/> width and height, via
        /// the OUT <paramref name="dimentions"/> parameter.
        /// </summary>
        /// <remarks>If given <paramref name="target"/> is null, then the <see cref="GraphicsDevice"/> back-buffer width and height is returned.</remarks>
        /// <param name="target"><see cref="RenderTarget2D"/> instance</param>
        /// <param name="dimentions">(OUT) <see cref="Vector2"/> structure</param>
        protected void GetTargetDimensions(RenderTarget2D target, out Vector2 dimentions)
        {
            dimentions = Vector2Zero;

            if (target == null)
            {
                dimentions.X = _GraphicsDevice.PresentationParameters.BackBufferWidth;
                dimentions.Y = _GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
            else
            {
                dimentions.X = target.Width;
                dimentions.Y = target.Height;
            }
        }

        /// <summary>
        /// Call the internal <see cref="Quadrangle"/> classes <see cref="Quadrangle.Draw"/> method.
        /// </summary>
        /// <param name="effect"><see cref="Effect"/> instance to use</param>
        protected void DrawQuad(Effect effect)
        {
            _quadrangle.Draw(effect);
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            return;
        }

        #endregion
    }
}


