#region File Description
//-----------------------------------------------------------------------------
// IWaterManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.Viewports;
using ImageNexus.BenScharbach.TWEngine.Water;
using ImageNexus.BenScharbach.TWEngine.Water.Enums;

namespace ImageNexus.BenScharbach.TWEngine.Interfaces
{

    /// <summary>
    /// The <see cref="WaterManager"/> class keeps copies of the water components, like the <see cref="Ocean"/> or <see cref="Lake"/>, 
    /// and calls the appropriate methods to setup vertices, intialize content, and render and update the components each game cycle.
    /// </summary>
    public interface IWaterManager : ILake, IOcean
    {
        // 1/8/2010
        ///<summary>
        ///Disposes of unmanaged resources.
        ///</summary>
        ///<param name="disposing">Is this final dispose?</param>
        void Dispose(bool disposing);
               
        /// <summary>
        /// Set the debug <see cref="GameViewPort"/> texture to show via script
        /// </summary>
        /// <param name="textureName">Texture name to use</param>
        void SetViewportTexture(string textureName);

        /// <summary>
        /// During debugging, this sets which WaterMap
        /// to display in the GameViewPort.
        /// </summary>
        ViewPortTexture ShowTexture { get; set; }

        ///<summary>
        /// Turn on/off drawing of water component.
        ///</summary>
        bool IsVisible { get; set; }

        // 6/1/2010
        /// <summary>
        /// The <see cref="WaterType"/> Component to use.
        /// </summary>
        WaterType WaterTypeToUse { get; set; }
    }
}