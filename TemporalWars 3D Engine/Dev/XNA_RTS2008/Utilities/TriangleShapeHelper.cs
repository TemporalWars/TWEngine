#region File Description
//-----------------------------------------------------------------------------
// TriangleShapeHelper.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Utilities
{
    // 4/21/2009: Updated all variables to be STATIC, which reduced the instances of the 'EffectParameter' on the
    //            HEAP, from over 232,000, down to 21,000.  This was due indirectly to the instanting the 'BasicEffect' class!
    /// <summary>
    /// The <see cref="TriangleShapeHelper"/> class is used to draw triangle list.
    /// </summary>
    sealed class TriangleShapeHelper : IDisposable
    {
        private static Game _gameInstance;

        // Effect and vertex declaration for drawing the picked triangle.
        private static BasicEffect _lineEffect;

        // XNA 4.0 updates; VertexDeclartion is gone.
        //private static VertexDeclaration _lineVertexDeclaration;

        // 8/15/2008
        private static GraphicsDevice _device;

        // XNA 4.0 Updates; RenderState gone; now use BlendState, DepthStencilState, RasterizerState or SampleState.
        //private static RenderState _renderState;
        private static DepthStencilState _depthStencilState;
        private static RasterizerState _rasterizerState;
       
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public TriangleShapeHelper(ref Game game)
        {
            if (_gameInstance == null)
                _gameInstance = game;
                       

            if (_lineEffect == null)
            {
                // XNA 4.0 Updates
                //_lineEffect = new BasicEffect(game.GraphicsDevice, null) {VertexColorEnabled = true};
                _lineEffect = new BasicEffect(game.GraphicsDevice) { VertexColorEnabled = true };
            }

            // XNA 4.0 updates; VertexDeclartion is gone.
            /*if (_lineVertexDeclaration == null)
            {
                _lineVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColor.VertexElements);
                _lineEffect.VertexColorEnabled = true;
            }*/

            // 8/15/2008
            if (_device == null)
                _device = _gameInstance.GraphicsDevice;

            // XNA 4.0 Updates; RenderState gone; now use BlendState, DepthStencilState, RasterizerState or SampleState.
            //if (_renderState == null) _renderState = _device.RenderState; 
            if (_depthStencilState == null) _depthStencilState = new DepthStencilState { DepthBufferEnable = true };
            if (_rasterizerState == null) _rasterizerState = new RasterizerState { FillMode = FillMode.Solid };
  
 
        }

        /// <summary>
        /// Helper for drawing the outline of a triangle as given
        /// </summary>        
        public static void DrawPrimitiveTriangle(ref VertexPositionColor[] triangle, 
            RasterizerState rasterizerState, DepthStencilState depthStencilState)
        {
            // Call base with count using Length of array.
            DrawPrimitiveTriangle(ref triangle, triangle.Length, rasterizerState, depthStencilState);
        }

        /// <summary>
        /// Helper for drawing the outline of a triangle as given
        /// </summary>        
        public static void DrawPrimitiveTriangle(ref VertexPositionColor[] triangle, int count, 
                               RasterizerState rasterizerState, DepthStencilState depthStencilState)
        {
            try // 7/9/2010
            {
                // 7/9/2010 - If count is zero, nothing to draw.
                if (count == 0) return;

                // 7/9/2010 - cache values
                var lineEffect = _lineEffect;

                // XNA 4.0 Updates
                if (_rasterizerState == null || _depthStencilState == null || lineEffect == null) return;

                // XNA 4.0 Updates; RenderState gone; now use BlendState, DepthStencilState, RasterizerState or SampleState.
                // NOTE: http://blogs.msdn.com/b/shawnhar/archive/2010/04/02/state-objects-in-xna-game-studio-4-0.aspx
                //blendState.FillMode = fillMode;
                _device.RasterizerState = RasterizerState.CullNone;

                // 3/25/2011 -  XNA 4.0 Updates.
                //renderState.AlphaBlendEnable = true;
                //renderState.AlphaTestEnable = true;
                _device.BlendState = BlendState.Opaque;

                // XNA 4.0 Updates; RenderState gone; now use BlendState, DepthStencilState, RasterizerState or SampleState.
                //blendState.DepthBufferEnable = depthBufferEnable;
                _device.DepthStencilState = DepthStencilState.Default;

                // Activate the line drawing BasicEffect.
                lineEffect.Projection = Camera.Projection;
                lineEffect.View = Camera.View;

                // XNA 4.0 updates; Begin() and End() gone; now use Apply().
                //lineEffect.Begin();
                //lineEffect.CurrentTechnique.Passes[0].Begin();
                lineEffect.CurrentTechnique.Passes[0].Apply();

                // XNA 4.0 updates; VertexDeclartion is gone.
                // Draw the triangle.
                //_device.VertexDeclaration = _lineVertexDeclaration;

                // Calculate the # of Triangles in Array
                var triangleCount = count / 3;

                _device.DrawUserPrimitives(PrimitiveType.TriangleList,
                                          triangle, 0, triangleCount);

                // XNA 4.0 updates; Begin() and End() gone.
                //lineEffect.CurrentTechnique.Passes[0].End();
                //lineEffect.End();
                
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine("DrawPrimitiveTriangle method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            finally
            {
                // XNA 4.0 Updates; RenderState gone; now use BlendState, DepthStencilState, RasterizerState or SampleState.
                // Reset render states to their default values.
                //renderState.FillMode = FillMode.Solid;
                //renderState.DepthBufferEnable = true;
                _device.DepthStencilState = _depthStencilState;
                _device.RasterizerState = _rasterizerState;

                // XNA 4.0 Updates; these are implied now.
                //renderState.AlphaBlendEnable = false;
                //renderState.AlphaTestEnable = false;
            }
        }

        #region Dispose

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Dispose
            if (_lineEffect != null)
                _lineEffect.Dispose();

            // Null Refs
            _gameInstance = null;
            _lineEffect = null;
            _device = null;
            // free native resources
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


    }
}
