#region File Description
//-----------------------------------------------------------------------------
// VertexMultitexturedDeclaration.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Terrain.Structs
{
    // 9/22/2010 - XNA 4.0 Updates - Removed ALL VertexElements from this structure, and placed in appropriate custom structures 'VertexMultitextured_Steam1..3'.
    // 7/8/2009 - 
    /*///<summary>
    /// Streams 1/2/3 <see cref="VertexElementFormat"/> information, for the <see cref="Terrain"/>.
    ///</summary>
    public struct VertexMultitexturedDeclaration 
    {

        // 9/22/2010 - XNA 4.0 UPdates - VertexElements no longer require the Stream setting.
        // Note: http://blogs.msdn.com/b/shawnhar/archive/2010/04/19/vertex-data-in-xna-game-studio-4-0.aspx?PageIndex=2
        #region OLDcode
        public static VertexElement[] VertexElements = new[]
                                                           {
                                                               // 5/26/2010: Test using streams 4-6.
                                                               // Stream-1
                                                               new VertexElement( 3, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),                             
                                                               new VertexElement( 3, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
                                                               //  Stream-2
                                                               new VertexElement( 4, 0, VertexElementFormat.HalfVector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
                                                               new VertexElement( 4, sizeof(uint) * 1, VertexElementFormat.HalfVector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 ),
                                                               //  Stream-3
                                                               new VertexElement( 5, 0, VertexElementFormat.HalfVector4, VertexElementMethod.Default, VertexElementUsage.Tangent, 0 ),                             
                                                               new VertexElement( 5, sizeof(float) * 2, VertexElementFormat.HalfVector4, VertexElementMethod.Default, VertexElementUsage.Binormal, 0 ),
                                                           };
        #endregion

        // 9/22/2010 - XNA 4.0 Updates - Since STREAMS are no longer allowed, each set of VertexElements shown below, are now moved
        //                               to their appropriate structure owners; 'VertexMultitextured_Steam1..3'.
        private static readonly VertexElement[] VertexElements = new[]
                                                           {
                                                               // 5/26/2010: Test using streams 4-6.
                                                               // Stream-1
                                                               new VertexElement( 0, VertexElementFormat.Vector3,  VertexElementUsage.Position, 0 ),                             
                                                               new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                                                               //  Stream-2
                                                               new VertexElement( 0, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0 ),
                                                               new VertexElement( sizeof(uint) * 1, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 1 ),
                                                               //  Stream-3
                                                               new VertexElement( 0, VertexElementFormat.HalfVector4, VertexElementUsage.Tangent, 0 ),                             
                                                               new VertexElement( sizeof(float) * 2, VertexElementFormat.HalfVector4, VertexElementUsage.Binormal, 0 ),
                                                           };

        

    }*/
}


