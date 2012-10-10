#region File Description
//-----------------------------------------------------------------------------
// BlueTexCube.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Structs
{
    // 4/22/2008
    ///<summary>
    /// This is the <see cref="BlueTexCube"/> Struct, which will draw a blue cube on the Terrain using two triangles
    /// internally using the given corners positions.  This is used specifically to show the blue
    /// cubes when the PaintTool is activated. 
    ///</summary>
    public struct BlueTexCube
    {
        // Effect and vertex declaration for drawing the picked triangle.
        private VertexPositionColor[] _triangle1;
        private VertexPositionColor[] _triangle2;

        // XNA 4.0 Updates
        private static readonly RasterizerState RasterizerState = new RasterizerState { FillMode = FillMode.Solid };
        private static readonly DepthStencilState DepthStencilState = new DepthStencilState { DepthBufferEnable = true };

        ///<summary>
        /// Constructor, which initializes the 2 internal <see cref="VertexPositionColor"/> collections.
        ///</summary>
        ///<param name="gameInstance"><see cref="Game"/> instance</param>
        ///<param name="corner1Pos"><see cref="Vector3"/> corner position 1</param>
        ///<param name="corner2Pos"><see cref="Vector3"/> corner position 2</param>
        ///<param name="corner3Pos"><see cref="Vector3"/> corner position 3</param>
        public BlueTexCube(Game gameInstance, Vector3 corner1Pos, Vector3 corner2Pos, Vector3 corner3Pos)
        {
            _triangle1 = new VertexPositionColor[3];
            _triangle1[0] = new VertexPositionColor(corner1Pos, Color.LightBlue);
            _triangle1[1] = new VertexPositionColor(corner2Pos, Color.LightBlue);
            _triangle1[2] = new VertexPositionColor(corner3Pos, Color.LightBlue);

            _triangle2 = new VertexPositionColor[3];
            _triangle2[0] = new VertexPositionColor(corner1Pos, Color.LightBlue);
            _triangle2[1] = new VertexPositionColor(corner2Pos, Color.LightBlue);
            _triangle2[2] = new VertexPositionColor(corner3Pos, Color.LightBlue);
            
        }

        ///<summary>
        /// Use to update a given texture cube with new position data.
        ///</summary>
        ///<param name="topLeftCorner"><see cref="Vector3"/> as top left corner</param>
        ///<param name="topRightCorner"><see cref="Vector3"/> as top right corner</param>
        ///<param name="bottomRightCorner"><see cref="Vector3"/> as bottom right corner</param>
        ///<param name="bottomLeftCorner"><see cref="Vector3"/> as bottom left corner</param>
        public void UpdateTexCube(Vector3 topLeftCorner, Vector3 topRightCorner, Vector3 bottomRightCorner,
                                  Vector3 bottomLeftCorner)
        {
            _triangle1[0].Position = topLeftCorner; // TL
            _triangle1[1].Position = bottomRightCorner; // BR
            _triangle1[2].Position = bottomLeftCorner; // BL

            _triangle2[0].Position = topLeftCorner; // TL
            _triangle2[1].Position = topRightCorner; // TR
            _triangle2[2].Position = bottomRightCorner; // BR
        }

        ///<summary>
        /// Draws the texture blue cube, using the helper class <see cref="TriangleShapeHelper"/>.
        ///</summary>
        public void Draw()
        {
            // XNA 4.0 Updates - Final 2 params updated.
            TriangleShapeHelper.DrawPrimitiveTriangle(ref _triangle1, RasterizerState, DepthStencilState);
            TriangleShapeHelper.DrawPrimitiveTriangle(ref _triangle2, RasterizerState, DepthStencilState);
        }
    }
}