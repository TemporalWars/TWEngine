using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWTools.SimpleQuadDrawer
{
    /// <summary>
    /// The <see cref="SimpleQuadDraw"/> class was created to draw the results of Post-Processing effects back
    /// to the screen, without the need to use the <see cref="SpriteBatch"/>.
    /// </summary>
    public static class SimpleQuadDraw
    {
        // 1/28/2009
        private static VertexPositionTexture[] _quadVertices;
        // private static VertexDeclaration _quadVertexDeclaration; // XNA 4.0 Changes
        private static short[] _indexBuffer;        
       
        /// <summary>
        /// Creates a basic Quad struct, which can be used to render a texture to the 
        /// entire screen!  This eliminates having to use the <see cref="SpriteBatch"/> to do the same.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        public static void CreateQuadVertices(GraphicsDevice graphicsDevice)
        {
            _quadVertices = new VertexPositionTexture[6];

            _quadVertices[0] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 1));
            _quadVertices[1] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 1));
            _quadVertices[2] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(0, 0));
            _quadVertices[3] = new VertexPositionTexture(new Vector3(0, 0, 0), new Vector2(1, 0));

            _indexBuffer = new short[] { 0, 1, 2, 2, 3, 0 };

            _quadVertices[0].Position.X = 1.0f;
            _quadVertices[0].Position.Y = -1.0f;

            _quadVertices[1].Position.X = -1.0f;
            _quadVertices[1].Position.Y = -1.0f;

            _quadVertices[2].Position.X = -1.0f;
            _quadVertices[2].Position.Y = 1.0f;

            _quadVertices[3].Position.X = 1.0f;
            _quadVertices[3].Position.Y = 1.0f;

            // XNA 4.0 Changes
            //_quadVertexDeclaration = new VertexDeclaration(graphicsDevice, VertexPositionTexture.VertexElements);

        }

        /// <summary>
        /// Draws a simple Quad of two vertices, which fill the entire screen.  Before calling
        /// this method, an effect must be started.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        public static void DrawSimpleQuad(GraphicsDevice graphicsDevice)
        {
            if (_quadVertices == null)
                CreateQuadVertices(graphicsDevice);
            
            // XNA 4.0 Changes
            //graphicsDevice.VertexDeclaration = _quadVertexDeclaration; 

            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,
                                        _quadVertices, 0, 4, _indexBuffer, 0, 2);
        }
    }
}
