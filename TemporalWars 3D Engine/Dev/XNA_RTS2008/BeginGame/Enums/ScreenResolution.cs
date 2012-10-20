#region File Description
//-----------------------------------------------------------------------------
// ScreenResolution.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.BeginGame.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    ///<summary>
    /// The <see cref="ScreenResolution"/> Enum is used to set different
    /// game screen resolutions.
    ///</summary>
    public enum ScreenResolution : short
    {
        ///<summary>
        /// Set 1024x768 resolution.
        ///</summary>
        Type1024X768, 
        ///<summary>
        ///  Set 1280x760 resolution.
        ///</summary>
        Type1280X720, 
        ///<summary>
        ///  Set 1024x1024 resolution.
        ///</summary>
        Type1280X1024, 
        ///<summary>
        ///  Set 1440x900 resolution.
        ///</summary>
        Type1440X900  
    }
}