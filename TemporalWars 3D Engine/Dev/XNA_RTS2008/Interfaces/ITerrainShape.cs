#region File Description
//-----------------------------------------------------------------------------
// ITerrainShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Interfaces;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.Terrain;
using TWEngine.Terrain.Structs;
using TWEngine.TerrainTools;

namespace TWEngine
{
    ///<summary>
    /// The <see cref="TerrainShape"/> class is a manager, which uses the other terrain classes to create and manage
    /// the <see cref="Terrain"/>.  For example, the drawing of the terrain is intiated in this class, but the actual drawing is
    /// done in the <see cref="TerrainQuadTree"/> class.  This class also loads the <see cref="SceneItem"/> into memory at the
    /// beginning of a level load.  This class also used the <see cref="TerrainAlphaMaps"/>, <see cref="TerrainPickingRoutines"/>, and
    /// the <see cref="TerrainEditRoutines"/> classes.
    ///</summary>
    public interface ITerrainShape : ITerrainStorageRoutines, IShadowMap, ITerrainAlphaMaps, IFOWTerrainShape, IMinimapTerrainShape 
    {
        ///<summary>
        /// Get or set reference for the <see cref="TerrainAlphaMaps"/>.
        ///</summary>
        TerrainAlphaMaps AlphaMaps { get; set; }
       
        ///<summary>
        /// Ambient color for texture group layer-1.
        ///</summary>
        Vector3 AmbientColorLayer1 { get; set; }
        
        /// <summary>
        /// Ambient color for texture group layer-2.
        /// </summary>
        Vector3 AmbientColorLayer2 { get; set; }
        
        /// <summary>
        /// Ambient power for texture group layer-1.
        /// </summary>
        float AmbientPowerLayer1 { get; set; }
        
        ///<summary>
        /// Ambient power for texture group layer-2.
        ///</summary>
        float AmbientPowerLayer2 { get; set; }
        
        ///<summary>
        /// Get or Set reference for the <see cref="TerrainAreaSelect"/>.
        ///</summary>
        TerrainAreaSelect AreaSelect { get; set; }
        
        ///<summary>
        /// Get or set reference to the <see cref="GraphicsDevice"/>.
        ///</summary>
        GraphicsDevice Device { get; set; }
        
        ///<summary>
        /// Extra diffuse color to emit from texture groups.
        ///</summary>
        Vector3 DiffuseColor { get; set; }
        
        ///<summary>
        ///Disposes of unmanaged resources.
        ///</summary>
        void Dispose();
        
        ///<summary>
        /// Get or Set the bounding boxes used to determine which
        /// <see cref="TerrainQuadPatch"/> are within the camera frustum.
        ///</summary>
        bool DrawBoundingBoxes { get; set; }
        
        ///<summary>
        /// Get or Set the <see cref="Effect"/> used to draw the <see cref="Terrain"/>
        ///</summary>
        Effect Effect { get; set; }
        
        // 4/8/2009
        ///<summary>
        /// Get or Set the <see cref="MapMarkerPositions"/> structure.
        ///</summary>
        MapMarkerPositions MapMarkerPositions { get; set; }
        
        /// <summary>
        /// This is just a simple listing of the Parent Quad's which were Tessellated to LOD-2,
        /// which will be saved and used during the Load to duplicate the exact Tessellation for
        /// the entire Terrain.
        /// </summary>
        System.Collections.Generic.List<int> QuadParentsTessellated { get; }
        
        ///<summary>
        /// Renders the <see cref="ITerrainShape"/>. 
        ///</summary>
        void Render();

        ///<summary>
        /// Returns a collection of <see cref="ScenaryItemScene"/>.
        ///</summary>
        System.Collections.Generic.List<ScenaryItemScene> ScenaryItems { get; }

        ///<summary>
        /// Sets the strength into the <see cref="TerrainData.ElevationStrength"/> property.
        ///</summary>
        ///<param name="strength">Enter a value between 0 through 100.</param>
        /// <remarks>Any value entered into the <paramref name="strength"/> parameter will always be multiplied by the
        /// constant value of ten.</remarks>
        /// <exception cref="strength">This exception is thrown when <paramref name="strength"/> is outside the allowable
        /// range of 0 through 100.</exception>
        void SetElevationStrength(float strength);
       
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> settings into <see cref="Effect"/>.  
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component.</remarks>
        /// <param name="isVisible">Sets the isVisible flag</param>       
        /// <param name="lightView"><see cref="Matrix"/> light view</param>       
        /// <param name="lightProj"><see cref="Matrix"/> light projection</param>
        /// <param name="lightViewStatic"><see cref="Matrix"/> light view static</param>
        /// <param name="lightProjStatic"><see cref="Matrix"/> light projection static</param>
        void SetShadowMapSettings(bool isVisible, ref Matrix lightView, ref Matrix lightProj,
                                  ref Matrix lightViewStatic, ref Matrix lightProjStatic);
      
        /// <summary>
        /// Sets the 'Dynamic' <see cref="ShadowMap"/> texture into <see cref="Effect"/>.         
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component. </remarks>
        /// <param name="shadowTexture"><see cref="Texture2D"/> instance</param>
        void SetDynamicShadowMap(Texture2D shadowTexture);

        /// <summary>
        /// Sets the minimum leaf size for <see cref="TerrainQuadPatch"/>. Must be a power of two.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> is not a power of 2.</exception>
        /// <param name="width">Minimum leaf size width (also sets height to match)</param>
        void SetLeafSize(int width);

        ///<summary>
        /// Ambient specular color for texture group layer-1.
        ///</summary>
        Vector3 SpecularColorLayer1 { get; set; }
        
        ///<summary>
        /// Ambient specular color for texture group layer-2.
        ///</summary>
        Vector3 SpecularColorLayer2 { get; set; }
        
        ///<summary>
        /// Ambient specular power for texture group layer-1.
        ///</summary>
        float SpecularPowerLayer1 { get; set; }
        
        ///<summary>
        /// Ambient specular power for texture group layer-2.
        ///</summary>
        float SpecularPowerLayer2 { get; set; }
        
        /// <summary>
        /// Use BoundingBox dictionary to track all <see cref="TerrainQuadTree"/> individual BoundingBox values.
        /// These values are used to check which BoundingBox the cursor is over for Picking purposes.        
        /// </summary>
        System.Collections.Generic.Dictionary<int, BoundingBox> TerrainBoundingBoxes { get; }
        
        ///<summary>
        /// Get or set reference for the <see cref="TerrainEditRoutines"/>.
        ///</summary>
        TerrainEditRoutines TerrainEditing { get; set; }
        
        /// <summary>
        /// Collection of texture names for default group 1; updated from the PaintTool Form.
        /// This is used to know which textures are being used as the 8 default textures; also used to
        /// add them back to the PaintTool containers when the Form loads.     
        /// </summary>
        System.Collections.Generic.Dictionary<int, TexturesGroupData> TextureGroupData1 { get; }

        /// <summary>
        /// Collection of texture names for default group 1; updated from the PaintTool Form.
        /// This is used to know which textures are being used as the 8 default textures; also used to
        /// add them back to the PaintTool containers when the Form loads.     
        /// </summary>
        System.Collections.Generic.Dictionary<int, TexturesGroupData> TextureGroupData2 { get; }

        /// <summary>
        /// Adds a <see cref="TexturesGroupData"/> record to one of the two dictionaries.
        /// Primarily called from the PaintTool Form Class when adding a new texture.
        /// </summary>
        /// <param name="index">Value 1-4</param>
        /// <param name="imageKey">ImageKey name</param>
        /// <param name="selectedImageKey">SelectedImageKey name</param>
        /// <param name="textureImagePath">Texture image path</param>
        /// <param name="groupNumber">Dictionary texture group to use</param>
        void TextureGroupData_AddRecord(int index, string imageKey, string selectedImageKey, string textureImagePath, int groupNumber);

        ///<summary>
        /// Sets the negate of the internal <see cref="TerrainShape._drawBoundingBoxes"/> boolean value.
        ///</summary>
        void ToggleBoundingBoxDraw();

        ///<summary>
        /// Sets the negate of the internal <see cref="TerrainShape._drawTerrain"/> boolean value.
        ///</summary>
        void ToggleTerrainDraw(); 
      
        /// <summary>
        /// Sets the proper ShadowType method to use into the shader;
        /// 1) Simple
        /// 2) PercentageCloseFilter_1
        /// 3) Variance.
        /// </summary>
        /// <param name="shadowType">ShadowType to use</param>
        void SetShadowMapType(ShadowMap.ShadowType shadowType);

        /// <summary>
        /// Sets the <see cref="ShadowMap"/> darkness, using a value between 0-1.0, with
        /// 1.0 being completely white with no shadow.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="shadowDarkness"/> not within allowable range 0.0 - 1.0.</exception>
        /// <param name="shadowDarkness"><see cref="ShadowMap"/> Darkness level (0-1.0f)</param>
        void SetShadowMapDarkness(float shadowDarkness);

        /// <summary>
        /// Sets the ShadowMap's HalfPixel correction setting.
        /// </summary>
        /// <param name="halfPixel">HalfPixel correction setting</param>
        void SetShadowMapHalfPixel(ref Vector2 halfPixel);

        /// <summary>
        /// Sets the Static <see cref="ShadowMap"/> texture into <see cref="Effect"/>.          
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component. </remarks>
        /// <param name="staticShadowMapTexture"><see cref="Texture2D"/> instance</param>
        void SetStaticShadowMap(Texture2D staticShadowMapTexture);
    }
}