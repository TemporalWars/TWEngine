#region File Description
//-----------------------------------------------------------------------------
// BlurManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common.Enums;
using ImageNexus.BenScharbach.TWTools.SimpleQuadDrawer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Common
{
    ///<summary>
    /// Manager of the Blur PostProcess effect.
    ///</summary>
    public static class BlurManager
    {
        // blur effect
        static Effect _blurEffect;
        // 2/19/2009
        static Effect _combineEffect;

        // screen quad vertex declaration and buffer
        static VertexDeclaration _vertexDeclaration;
        internal static VertexBuffer VertexBuffer;
        
        // render target resolution
        static float _sizeX;
        static float _sizeY;

        // normalized pixel size (1.0/size)
        internal static Vector2 PixelSize;

        // 2D ortho view projection matrix
        internal static Matrix ViewProjection;

        // parameters
        static EffectParameter _paramWorldViewProjection;  // World * view * proj matrix
        static EffectParameter _paramColorMap;             // color texture
        static EffectParameter _paramColor;                // color 
        static EffectParameter _paramPixelSize;            // pixel size       


        /// <summary>
        /// Create a new blur manager
        /// </summary>
        /// <param name="graphicsDevice">Instance of <see cref="GraphicsDevice"/>.</param>
        /// <param name="effect">Instance of <see cref="Effect"/>.</param>
        /// <param name="sizeX">Horizontal buffer size.</param>
        /// <param name="sizeY">Verical buffer size.</param>
        public static void SetBlurManager(GraphicsDevice graphicsDevice, Effect effect, float sizeX, float sizeY)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }
            if (effect == null)
            {
                throw new ArgumentNullException("effect");
            }

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _combineEffect = TemporalWars3DEngine.GameInstance.Content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\CombineEffect");
            _combineEffect.CurrentTechnique = _combineEffect.Techniques["CombineTwoTextures"];

            _blurEffect = effect;    // save effect
            _sizeX = sizeY;      // save horizontal buffer size
            _sizeY = sizeX;      // save verical buffer size

            // get effect parameters
            _paramWorldViewProjection = _blurEffect.Parameters["g_WorldViewProj"];
            _paramColorMap = _blurEffect.Parameters["g_ColorMap"];
            _paramColor = _blurEffect.Parameters["g_Color"];
            _paramPixelSize = _blurEffect.Parameters["g_PixelSize"];

            PixelSize = new Vector2(1.0f / _sizeX, 1.0f / _sizeY);
            ViewProjection = Matrix.CreateOrthographicOffCenter(0, _sizeX, 0, _sizeY, -1, 1);

            // create vertex buffer
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionTexture),
                                             6, BufferUsage.WriteOnly);

            // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
            // create vertex declaration
            //_vertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionTexture.VertexElements);

            // create vertex data
            SetVertexData();
        }

        /// <summary>
        /// Set vertex data with cube vertex normals (used for cubemap blur option only)
        /// </summary>
        public static void SetVertexData()
        {
            var data = new VertexPositionTexture[6];

            data[0] = new VertexPositionTexture(
                new Vector3(0, 0, 0), new Vector2(0, 1));
            data[1] = new VertexPositionTexture(
                new Vector3(_sizeX, _sizeY, 0), new Vector2(1, 0));
            data[2] = new VertexPositionTexture(
                new Vector3(_sizeX, 0, 0), new Vector2(1, 1));
            data[3] = new VertexPositionTexture(
                new Vector3(0, 0, 0), new Vector2(0, 1));
            data[4] = new VertexPositionTexture(
                new Vector3(0, _sizeY, 0), new Vector2(0, 0));
            data[5] = new VertexPositionTexture(
                new Vector3(_sizeX, _sizeY, 0), new Vector2(1, 0));

            VertexBuffer.SetData(data);
        }

        // 2/19/2009
        /// <summary>
        /// Renders 2 textures into 1, and returns this to the caller.
        /// </summary>
        /// <param name="gd">GraphicsDevice</param>
        /// <param name="texture1">Texture 1</param>
        /// <param name="texture2">Texture 2</param>
        /// <param name="combineTextureRT">(OUT) Combine Result Texture</param>
        public static void RenderTwoTexturesAsOne(GraphicsDevice gd, Texture2D texture1, 
                                                  Texture2D texture2,  ref RenderTarget2D combineTextureRT)
        {
            // XNA 4.0 Updates - Index obsolete for SetRenderTarget.
            // Set render target to miniMap Render target
            //gd.SetRenderTarget(0, combineTextureRT);
            gd.SetRenderTarget(combineTextureRT);

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            // draw the scene using our pixel shader.   
            _combineEffect.Parameters["xTextureMap1"].SetValue(texture1);
            _combineEffect.Parameters["xTextureMap2"].SetValue(texture2);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_combineEffect.Begin();
            _combineEffect.CurrentTechnique.Passes[0].Apply();

            SimpleQuadDraw.DrawSimpleQuad(gd);

            // XNA 4.0 Updates - Updated SpriteBatch.Begin() signature.
            //_combineEffect.CurrentTechnique.Passes[0].End();
            //_combineEffect.End();

            // XNA 4.0 Updates - Index obsolete for SetRenderTarget.
            // Set render target back to the back buffer
            //gd.SetRenderTarget(0, null);
            gd.SetRenderTarget(null);  
           
        }

        /// <summary>
        /// Render a screen aligned quad used to process 
        /// the horizontal and vertical blur operations
        /// </summary>
        /// <param name="graphicsDevice">Instance of <see cref="GraphicsDevice"/>.</param>
        /// <param name="technique">Instance of <see cref="BlurTechnique"/>.</param>
        /// <param name="texture">Instance of <see cref="Texture2D"/>.</param>
        /// <param name="color">Instance of <see cref="Vector4"/>.</param>
        public static void RenderScreenQuad(GraphicsDevice graphicsDevice, BlurTechnique technique,
                                            Texture2D texture, Vector4 color)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
            //graphicsDevice.VertexDeclaration = _vertexDeclaration;
            //graphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);

            _blurEffect.CurrentTechnique = _blurEffect.Techniques[(int)technique];

            _paramWorldViewProjection.SetValue(ViewProjection);
            _paramPixelSize.SetValue(PixelSize);
            _paramColorMap.SetValue(texture);
            _paramColor.SetValue(color);

            // XNA 4.0 updates - CommitChanges() obsolete.
            //_blurEffect.CommitChanges();

            // XNA 4.0 updates.
            graphicsDevice.SetVertexBuffer(VertexBuffer);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_blurEffect.Begin(); // was SaveStateMode.SaveState
            _blurEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            //_blurEffect.CurrentTechnique.Passes[0].End();
            //_blurEffect.End();

            // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
            //graphicsDevice.VertexDeclaration = null;
            //graphicsDevice.Vertices[0].SetSource(null, 0, 0);
        }


        /// <summary>
        /// Render a screen aligned quad used to process 
        /// the horizontal and vertical blur operations
        /// </summary>
        /// /// <param name="graphicsDevice">Instance of <see cref="GraphicsDevice"/>.</param>
        /// <param name="technique">Instance of <see cref="BlurTechnique"/>.</param>
        /// <param name="texture">Instance of <see cref="Texture2D"/>.</param>
        /// <param name="color">Instance of <see cref="Vector4"/>.</param>
        /// <param name="scale">Enter the scale value.</param>
        public static void RenderScreenQuad(GraphicsDevice graphicsDevice, BlurTechnique technique,
                                            Texture2D texture, Vector4 color, float scale)
        {
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphicsDevice");
            }

            // XNA 4.0 Updates - VertexDeclaration obsolete.
            //graphicsDevice.VertexDeclaration = _vertexDeclaration;
            //graphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);

            _blurEffect.CurrentTechnique = _blurEffect.Techniques[(int)technique];

            var m = Matrix.CreateTranslation(-_sizeX / 2, -_sizeY / 2, 0) *
                       Matrix.CreateScale(scale, scale, 1) *
                       Matrix.CreateTranslation(_sizeX / 2, _sizeY / 2, 0);

            _paramWorldViewProjection.SetValue(m * ViewProjection);
            _paramPixelSize.SetValue(PixelSize);
            _paramColorMap.SetValue(texture);
            _paramColor.SetValue(color);

            // XNA 4.0 updates - CommitChanges() obsolete.
            //_blurEffect.CommitChanges();

            // XNA 4.0 updates.
            graphicsDevice.SetVertexBuffer(VertexBuffer);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_blurEffect.Begin(SaveStateMode.SaveState);
            _blurEffect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            //_blurEffect.CurrentTechnique.Passes[0].End();
            //_blurEffect.End();

            // XNA 4.0 Updates - VertexDeclaration obsolete.
            //graphicsDevice.VertexDeclaration = null;
            //graphicsDevice.Vertices[0].SetSource(null, 0, 0);
        }

        #region IDisposable Members

        ///<summary>
        /// Disposal status of this class.
        ///</summary>
        public static bool IsDisposed;  
      
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public static void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private static void Dispose(bool disposing)
        {
            if (!disposing || IsDisposed) return;

            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }

            if (_vertexDeclaration == null) return;

            _vertexDeclaration.Dispose();
            _vertexDeclaration = null;
        }

        #endregion
    }
}