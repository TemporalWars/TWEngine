#region File Description
//-----------------------------------------------------------------------------
// PartType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Explosions.Structs;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    ///<summary>
    /// <see cref="PartType"/> Enum used to diffrentiate between
    /// parts used in the <see cref="ExplosionItem"/> animation, versus
    /// parts used to draw the item normally.
    ///</summary>
    public enum PartType
    {
        ///<summary>
        /// <see cref="InstancedModelPart"/> used to draw the <see cref="InstancedItem"/> normally.
        ///</summary>
        NormalPart,
        ///<summary>
        /// <see cref="InstancedModelPart"/> uesd to draw ONLY the explosion pieces of the <see cref="InstancedItem"/>.
        ///</summary>
        ExplosionPart
    }
}