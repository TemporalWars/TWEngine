#region File Description
//-----------------------------------------------------------------------------
// VertexMultitextured_Stream3.cs
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
    // 1/29/2010 - Updated to use the PackedVector format.
    // 7/8/2009 - Stream-3 contains BumpMapping 'Tangent' data.   
    // Custom Vertex Struct, for Stream2.
    // ReSharper disable InconsistentNaming
    /*///<summary>
    /// The <see cref="VertexMultitextured_Stream3"/> structure holds the <see cref="Terrain"/> bump mapping 
    /// tanget and binormal data in vertex stream-3.
    ///</summary>
    public struct VertexMultitextured_Stream3 : IVertexType
// ReSharper restore InconsistentNaming
    {
        ///<summary>
        /// Bumpmap's Tangent channel
        ///</summary>
        /// <remarks>Stored using the <see cref="HalfVector4"/> packed size</remarks>
        public HalfVector4 Tangent; // (Stream-3)
        ///<summary>
        /// Bumpmap's BiNormal channel
        ///</summary>
        /// <remarks>Stored using the <see cref="HalfVector4"/> packed size</remarks>
        public HalfVector4 BiNormal; // (Stream-3)  


        // 9/22/2010 - XNA 4.0 Updates - Prior to 4.0, the VertexElements were defined in the structure 'VertexMultitexturedDeclaration',
        //                               with each set of VertexElements associated to the proper VertexStream using an index number; however,
        //                               in XNA 4.0, the stream is no longer valid.  Instead, the VertexDeclaration MUST be created within
        //                               each custom structure.
        // Note: http://blogs.msdn.com/b/shawnhar/archive/2010/04/19/vertex-data-in-xna-game-studio-4-0.aspx?PageIndex=2
        private static readonly VertexElement[] VertexElements = new[]
                                                           {
                                                               //  Stream-3
                                                               new VertexElement( 0, VertexElementFormat.HalfVector4, VertexElementUsage.Tangent, 0 ),                             
                                                               new VertexElement( sizeof(float) * 2, VertexElementFormat.HalfVector4, VertexElementUsage.Binormal, 0 ),
                                                           };

        // 9/22/2010 - XNA 4.0 updates - Declare VertexDec here.
        ///<summary>
        /// <see cref="VertexDeclaration"/> with collection of <see cref="VertexElement"/> structs.
        ///</summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }


        ///<summary>
        /// Size in bytes for <see cref="Tangent"/> and <see cref="BiNormal"/> data.
        ///</summary>
        public const int SizeInBytes = ((2 + 2) * sizeof(float)); // 1/29/2010 - was (3+3)
    }*/
}


