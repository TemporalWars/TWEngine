#region File Description
//-----------------------------------------------------------------------------
// VertexBufferComponent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Terrain.Enums
{
    ///<summary>
    /// The <see cref="VertexBufferComponent"/> Enum is used to determine the vertex type to affect or use.
    ///</summary>
    public enum VertexBufferComponent
    {
        ///<summary>
        /// Affect position attribute.
        ///</summary>
        Position,
        ///<summary>
        /// Affect texture coordinate-1 attribute.
        ///</summary>
        TexCoordinate1,
        ///<summary>
        /// Affect texture coordinate-2 attribute.
        ///</summary>
        TexCoordinate2,
        ///<summary>
        /// Affect normal attribute.
        ///</summary>
        Normal
    }
}
