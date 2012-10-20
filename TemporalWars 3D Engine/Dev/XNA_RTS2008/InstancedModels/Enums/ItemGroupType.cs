#region File Description
//-----------------------------------------------------------------------------
// ItemGroupType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.IFDTiles.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// <see cref="ItemGroupType"/> Enum, used to determine
    /// the group <see cref="SceneItem"/> can belong to, or
    /// the group a <see cref="SceneItem"/> can attack, etc.
    ///</summary>
    /// <remarks>This Enum is decorated with the <see cref="FlagsAttribute"/>, allowing for combinations.</remarks>
    [Flags]
    public enum ItemGroupType : short
    {
        ///<summary>
        /// Represents items which can be built, like the <see cref="BuildingScene"/>.
        ///</summary>
        Buildings = IFDGroupControlType.Buildings,
        ///<summary>
        /// Represents items which are of defense, like the <see cref="DefenseScene"/>.
        ///</summary>
        Shields = IFDGroupControlType.Shields,
        ///<summary>
        /// Testing pursoses only. (Not production use)
        ///</summary>
        People = IFDGroupControlType.People,
        ///<summary>
        /// Represents items which belong to the vehicles, like the <see cref="SciFiTankScene"/>.
        ///</summary>
        Vehicles = IFDGroupControlType.Vehicles,
        ///<summary>
        /// Represents items which belong to the aircraft, like the <see cref="SciFiAircraftScene"/>.
        ///</summary>
        Airplanes = IFDGroupControlType.Airplanes
    }
}