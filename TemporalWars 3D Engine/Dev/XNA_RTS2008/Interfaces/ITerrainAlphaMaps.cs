#region File Description
//-----------------------------------------------------------------------------
// ITerrainAlphaMaps.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Terrain;

namespace TWEngine.Interfaces
{

    ///<summary>
    /// The <see cref="TerrainAlphaMaps"/> class is used to control the texture splatting onto the <see cref="Terrain"/>.
    /// This class is specifically updated from the edit PaintTool form, and used during game play to draw the textures at
    /// the proper placement, with the proper splatting effect.
    ///</summary>
    public interface ITerrainAlphaMaps
    {
        ///<summary>
        /// Visiblity percent of texture-1.
        ///</summary>
        float AlphaLy1Percent { get; set; }
        
        ///<summary>
        /// Visiblity percent of texture-2.
        ///</summary>
        float AlphaLy2Percent { get; set; }
       
        ///<summary>
        /// Visiblity percent of texture-3.
        ///</summary>
        float AlphaLy3Percent { get; set; }
       
        ///<summary>
        /// Visiblity percent of texture-4.
        ///</summary>
        float AlphaLy4Percent { get; set; }
        
        ///<summary>
        /// Scale to apply for each texel position of the <see cref="TerrainAlphaMaps"/>.
        ///</summary>
        float AlphaScale { get; set; }
       
        ///<summary>
        /// Holds the pixel color data for layer-1/layer-2 in x/y, respectively.  Each channel, like the X channel, stores
        /// the blended pixel color for the choosen texture position (1-4).  Channel Z stores the bump data for Layer-1.
        ///</summary>
        Microsoft.Xna.Framework.Vector4 PaintTexture { get; set; }
        
        ///<summary>
        /// Sets the collection of <see cref="Texture2D"/> maps.
        ///</summary>
        ///<param name="value">Collection of <see cref="Texture2D"/></param>
        void SetTextureMaps(Texture2D value);
       
        ///<summary>
        /// Returns a collection of <see cref="Texture2D"/> maps.
        ///</summary>
        ///<returns>Collection of <see cref="Texture2D"/></returns>
        Texture2D GetTextureMaps();
    }
}