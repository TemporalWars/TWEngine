#region File Description
//-----------------------------------------------------------------------------
// VertexBufferComponent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.Terrain.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="VertexBufferComponent"/> Enum is used to determine the vertex type to affect or use.
    ///</summary>
    public enum VertexBufferComponent : short
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
