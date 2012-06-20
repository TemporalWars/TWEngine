#region File Description
//-----------------------------------------------------------------------------
// IFDGroupControlType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.IFDTiles.Enums
{
    // 7/24/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.IFDTiles.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="IFDTiles.Enums"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    ///<summary>
    /// <see cref="InterfaceDisplay"/> tile group to use.
    ///</summary>
    public enum IFDGroupControlType
    {
        ///<summary>
        /// Control group
        ///</summary>
        ControlGroup1 = 0, // Perm Tile Selector Group
        ///<summary>
        /// Buildings group
        ///</summary>
        Buildings = 1,
        ///<summary>
        /// Defense shield group
        ///</summary>
        Shields = 2,
        ///<summary>
        /// Infantry group
        ///</summary>
        People = 4,
        ///<summary>
        /// Vehicles group
        ///</summary>
        Vehicles = 8,
        ///<summary>
        /// Aircraft group
        ///</summary>
        Airplanes = 16
    }
}
