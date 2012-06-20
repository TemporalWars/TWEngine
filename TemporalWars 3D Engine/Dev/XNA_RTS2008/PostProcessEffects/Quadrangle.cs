#region File Description
//-----------------------------------------------------------------------------
// Quadrangle.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TWEngine.PostProcessEffects
{
    /// <summary>
    /// The <see cref="Quadrangle"/> class is used to draw a rectangle which 
    /// fits the entire screen, and ultimately is to draw post process effects to screen.
    /// </summary>
    internal sealed class Quadrangle
    {
        private static readonly Dictionary<GraphicsDevice, Quadrangle> Quadrangles = new Dictionary<GraphicsDevice, Quadrangle>();

        // XNA 4.0 Updates - Replaces RenderState settings.
        private static readonly RasterizerState RasterizerStateCullMode = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };

        /// <summary>
        /// Returns an instance of the <see cref="Quadrangle"/>.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <returns><see cref="Quadrangle"/> instance</returns>
        public static Quadrangle Find(GraphicsDevice graphicsDevice)
        {
            if (Quadrangles.ContainsKey(graphicsDevice))
            {
                return Quadrangles[graphicsDevice];
            }

            var quad = new Quadrangle(graphicsDevice);
            Quadrangles.Add(graphicsDevice, quad);
            return quad;
        }

        private readonly VertexPositionTexture[] _vertices = new VertexPositionTexture[4];
        private readonly VertexDeclaration _declaration;
        private readonly VertexBuffer _vertexBuffer;

        private readonly GraphicsDevice _graphicsDevice;

        private Quadrangle(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            // XNA 4.0 Updates - VertexDeclaration set at creation time.
            //_declaration = new VertexDeclaration(_graphicsDevice, VertexPositionTexture.VertexElements);
            //_vertexBuffer = new VertexBuffer(_graphicsDevice, VertexPositionTexture.SizeInBytes * 4, BufferUsage.WriteOnly);
            _vertexBuffer = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTexture), _vertices.Length, BufferUsage.WriteOnly);
            
            _vertices[0].Position = new Vector3(-1, 1, 0);
            _vertices[1].Position = new Vector3(1, 1, 0);
            _vertices[2].Position = new Vector3(-1, -1, 0);
            _vertices[3].Position = new Vector3(1, -1, 0);

            _vertices[0].TextureCoordinate = new Vector2(0, 0);
            _vertices[1].TextureCoordinate = new Vector2(1, 0);
            _vertices[2].TextureCoordinate = new Vector2(0, 1);
            _vertices[3].TextureCoordinate = new Vector2(1, 1);
            _vertexBuffer.SetData(_vertices);
        }

        private void Bind()
        {
            // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
            //_graphicsDevice.VertexDeclaration = _declaration;

            // XNA 4.0 Updates - Set ONLY VB.
            //_graphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
            _graphicsDevice.SetVertexBuffer(_vertexBuffer);

            // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
            //_graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            _graphicsDevice.RasterizerState = RasterizerStateCullMode;
        }

        private void Draw()
        {
            //_graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _vertices, 0, 2);
        }

        /// <summary>
        /// Draws the <see cref="Quadrangle"/> to screen, using the given <see cref="Effect"/> instance.
        /// </summary>
        /// <param name="effect"><see cref="Effect"/> instance to use</param>
        public void Draw(Effect effect)
        {
            Bind();

            // XNA 4.0 updates - Begin() and End() obsolete.
            //effect.Begin();

            // 8/13/2009 - Cache
            var effectPassCollection = effect.CurrentTechnique.Passes;
            var passCollection = effectPassCollection; // 4/26/2010
            var count = passCollection.Count;

            for (var i = 0; i < count; i++)
            {
                var pass = passCollection[i];

                // XNA 4.0 updates - Begin() and End() obsolete.
                pass.Apply();
                Draw();
                //pass.End();

            }

            //effect.End();
        }
    }
}


