#region File Description
//-----------------------------------------------------------------------------
// VertexMultitextured_Stream1.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ImageNexus.BenScharbach.TWEngine.Terrain.Structs
{
    // XNA 4.0 Updates - Add IVertexType interface.
    // 7/8/2009 - Stream-1 contains Geometry data 
    // Custom Vertex Struct to get Vertex Data from Terrain Model
    // ReSharper disable InconsistentNaming
    ///<summary>
    /// The <see cref="VertexMultitextured_Stream1"/> structure holds the <see cref="TWEngine.Terrain"/> position
    /// and normals in vertex stream-1.
    ///</summary>
    public struct VertexMultitextured_Stream1 : IVertexType
    // ReSharper restore InconsistentNaming
    {
        ///<summary>
        /// Vertex position
        ///</summary>
        public Vector3 Position; // (Stream-1)     
        ///<summary>
        /// Vertex normal
        ///</summary>
        public Vector3 Normal; // (Stream-1) 

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


        // XNA 4.0 Updates - Prior to 4.0, the VertexElements were defined in the structure 'VertexMultitexturedDeclaration',
        //                   with each set of VertexElements associated to the proper VertexStream using an index number; however,
        //                   in XNA 4.0, the stream is no longer valid.  Instead, the VertexDeclaration MUST be created within
        //                   each custom structure.
        // Note: http://blogs.msdn.com/b/shawnhar/archive/2010/04/19/vertex-data-in-xna-game-studio-4-0.aspx?PageIndex=2
        private static readonly VertexElement[] VertexElements = new[]
                                                           {
                                                               // 10/3/2010 - Was Stream-1 in XNA 3.1
                                                               new VertexElement( 0, VertexElementFormat.Vector3,  VertexElementUsage.Position, 0 ),                             
                                                               new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                                                               // 10/3/2010 - Was Stream-2 in XNA 3.1
                                                               new VertexElement( sizeof(float) * 6, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0 ),
                                                               new VertexElement( sizeof(float) * 6 + sizeof(uint) * 1, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 1 ),
                                                                // 10/3/2010 - Was Stream-3 in XNA 3.1
                                                               new VertexElement( sizeof(float) * 6 + sizeof(uint) * 2, VertexElementFormat.HalfVector4, VertexElementUsage.Tangent, 0 ),                             
                                                               new VertexElement( sizeof(float) * 6 + sizeof(uint) * 4, VertexElementFormat.HalfVector4, VertexElementUsage.Binormal, 0 ),
                                                           };

        // XNA 4.0 updates - Declare VertexDec here.
        ///<summary>
        /// <see cref="VertexDeclaration"/> with collection of <see cref="VertexElement"/> structs.
        ///</summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        //public const int SizeInBytes = ((3 + 3) * sizeof(float)); 
    }
}


