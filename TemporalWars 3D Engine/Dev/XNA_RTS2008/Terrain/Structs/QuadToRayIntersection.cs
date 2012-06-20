#region File Description
//-----------------------------------------------------------------------------
// QuadToRayIntersection.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Terrain.Structs
{
    ///<summary>
    /// The <see cref="QuadToRayIntersection"/> structure is used to store the
    /// 2 closest <see cref="TerrainQuadPatch"/> items found, when checking for
    /// cursor ray hits from the <see cref="TerrainPickingRoutines"/> class.
    ///</summary>
    /// <remarks>Items are stored in order of distance, where index 0 is the closest hit.</remarks>
    public struct QuadToRayIntersection
    {
        ///<summary>
        /// Stores the distance of the closest intersection.
        ///</summary>
        public float? Intersection;
        ///<summary>
        /// <see cref="TerrainQuadTree"/> instance.
        ///</summary>
        public TerrainQuadTree Quad;
    }
}