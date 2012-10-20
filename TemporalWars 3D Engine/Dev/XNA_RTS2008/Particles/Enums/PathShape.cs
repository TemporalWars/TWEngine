#region File Description
//-----------------------------------------------------------------------------
// PathShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.Particles.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    /// <summary>
    /// The <see cref="PathShape"/> Enum specifies the type of path the
    /// <see cref="Projectile"/> will take.
    /// </summary>
    public enum PathShape : short
    {
        ///<summary>
        /// The <see cref="Projectile"/> will follow a straight path to its target.
        ///</summary>
        Straight,
        ///<summary>
        /// The <see cref="Projectile"/> will arch up, and then down to its target.
        ///</summary>
        ArchUp,
        ///<summary>
        /// The <see cref="Projectile"/> will zig-zag left then right to its target.
        ///</summary>
        ZigZagLeftRight
    }
}


