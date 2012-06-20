#region File Description
//-----------------------------------------------------------------------------
// ParticleVertex.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace TWEngine.Particles.Structs
{
    /// <summary>
    /// Custom vertex structure for drawing point sprite particles.
    /// </summary>
    struct ParticleVertex
    {
        // XNA 4.0 Updates - New field.
        // Stores which corner of the particle quad this vertex represents.
        public Short2 Corner;

        /// <summary>
        /// Stores the starting position of the particle.
        /// </summary>
        public Vector3 Position;
       
        /// <summary>
        /// Stores the starting velocity of the particle.
        /// </summary>
        public Vector3 Velocity;
       
        /// <summary>
        /// Four random values, used to make each particle look slightly different.
        /// </summary>
        public Color Random;
       
        /// <summary>
        /// The Time (in seconds) at which this particle was created.
        /// </summary>
        public float Time;

        //  XNA 4.0 Updates - Removed obsolete 'VertexElementMethod.Default'.
        #region OLDCode
        /*public static readonly VertexElement[] VertexElements =
            {
                new VertexElement(0, 0, VertexElementFormat.Vector3,
                                  VertexElementMethod.Default,
                                  VertexElementUsage.Position, 0),

                new VertexElement(0, 12, VertexElementFormat.Vector3,
                                  VertexElementMethod.Default,
                                  VertexElementUsage.Normal, 0),

                new VertexElement(0, 24, VertexElementFormat.Color,
                                  VertexElementMethod.Default,
                                  VertexElementUsage.Color, 0),

                new VertexElement(0, 28, VertexElementFormat.Single,
                                  VertexElementMethod.Default,
                                  VertexElementUsage.TextureCoordinate, 0),
            };*/
        #endregion
        /// <summary>
        /// Describe the layout of this vertex structure.
        /// </summary>
        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Short2,
                                 VertexElementUsage.Position, 0),

            new VertexElement(4, VertexElementFormat.Vector3,
                                 VertexElementUsage.Position, 1),

            new VertexElement(16, VertexElementFormat.Vector3,
                                  VertexElementUsage.Normal, 0),

            new VertexElement(28, VertexElementFormat.Color,
                                  VertexElementUsage.Color, 0),

            new VertexElement(32, VertexElementFormat.Single,
                                  VertexElementUsage.TextureCoordinate, 0)
        );

        // 9/20/2010 - XNA 4.0 Updates
        /// <summary>
        ///  Describe the size of this vertex structure.
        /// </summary>
        //public const int SizeInBytes = 32;
        public const int SizeInBytes = 36;
    }
}


