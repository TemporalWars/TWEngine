#region File Description
//-----------------------------------------------------------------------------
// VertexMultitextured_Stream2.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace TWEngine.Terrain.Structs
{
    // XNA 4.0 Updates - Add IVertexType interface.
    // 7/8/2009 - Stream-2 contains Texture Cords data.   
    // Custom Vertex Struct, for Stream2.
    // ReSharper disable InconsistentNaming
    /*///<summary>
    /// The <see cref="VertexMultitextured_Stream2"/> structure holds the <see cref="Terrain"/> texture 
    /// coordinates in vertex stream-2.
    ///</summary>
    public struct VertexMultitextured_Stream2 : IVertexType
// ReSharper restore InconsistentNaming
    {
        ///<summary>
        /// Texture coordinates for layer-1.
        ///</summary>
        /// <remarks>Stored using the <see cref="HalfVector2"/> packed size</remarks>
        public HalfVector2 TextureCoordinate1; // (Stream-2)
        ///<summary>
        /// Texture coordinates for layer-2.
        ///</summary>
        /// <remarks>Stored using the <see cref="HalfVector2"/> packed size</remarks>
        public HalfVector2 TextureCoordinate2; // (Stream-2)  

        // 9/22/2010 - XNA 4.0 Updates - Prior to 4.0, the VertexElements were defined in the structure 'VertexMultitexturedDeclaration',
        //                               with each set of VertexElements associated to the proper VertexStream using an index number; however,
        //                               in XNA 4.0, the stream is no longer valid.  Instead, the VertexDeclaration MUST be created within
        //                               each custom structure.
        // Note: http://blogs.msdn.com/b/shawnhar/archive/2010/04/19/vertex-data-in-xna-game-studio-4-0.aspx?PageIndex=2
        private static readonly VertexElement[] VertexElements = new[]
                                                           {
                                                               //  Stream-2
                                                               new VertexElement( 0, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0 ),
                                                               new VertexElement( sizeof(uint) * 1, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 1 ),
                                                           };

        // 9/22/2010 - XNA 4.0 updates - Declare VertexDec here.
        ///<summary>
        /// <see cref="VertexDeclaration"/> with collection of <see cref="VertexElement"/> structs.
        ///</summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        ///<summary>
        /// Size in bytes for <see cref="TextureCoordinate1"/> and <see cref="TextureCoordinate2"/> data.
        ///</summary>
        public const int SizeInBytes = ((1 + 1) * sizeof(uint));
    }*/
}


