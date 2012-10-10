#region File Description
//-----------------------------------------------------------------------------
// VertexPosition.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.SkyDomes.Structs
{
    /// <summary>
    /// Used to hold the vertex information used to create the <see cref="SkyDome"/>
    /// </summary>
    public struct VertexPosition : IVertexType
    {
        ///<summary>
        /// The <see cref="Vector3"/> position
        ///</summary>
        public Vector3 Position;

        // XNA 4.0 Updates
        VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        /// <summary>
        /// Constructor for <see cref="VertexPosition"/>.
        /// </summary>
        /// <param name="position"></param>
        public VertexPosition(Vector3 position)
        {
            Position = position;
        }

        // XNA 4.0 Updates
        ///<summary>
        /// VertexDeclaration with collection of <see cref="VertexElement"/>.
        ///</summary>
        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0)
        );


        /*/// <summary>
        /// Collection of <see cref="VertexElement"/>.
        /// </summary>
        public static readonly VertexElement[] VertexElements = 
            {
                // 9/20/2010 - XNA 4.0 Updates
                new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 )
            };*/

        /// <summary>
        /// Size in bytes of the given structure.
        /// </summary>
        public static readonly int SizeInBytes = sizeof(float) * 3;
    }
}