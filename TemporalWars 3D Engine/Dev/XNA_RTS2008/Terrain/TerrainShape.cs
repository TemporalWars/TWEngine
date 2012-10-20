#region File Description
//-----------------------------------------------------------------------------
// TerrainShape.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.PostProcessEffects.BloomEffect;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWEngine.Shadows;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.TerrainTools;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.Utilities.Enums;
using ImageNexus.BenScharbach.TWEngine.Utilities.Structs;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DrawMode = ImageNexus.BenScharbach.TWEngine.Terrain.Enums.DrawMode;
using System.Diagnostics;
#if !XBOX360
using TWEngine.TerrainTools;
using System.Windows.Forms;
#endif


namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    // 8/12/2008 - Updated the 'ITerrainShape' Interface to inherit from IWater, ITerrainStorageRoutines, and IWater; this
    //             allowed me to remove these references here, and remove the redudant code in ITerrainShape.
    ///<summary>
    /// The <see cref="TerrainShape"/> class is a manager, which uses the other terrain classes to create and manage
    /// the <see cref="TWEngine.Terrain"/>.  For example, the drawing of the terrain is initialized in this class, but the actual drawing is
    /// done in the <see cref="TerrainQuadTree"/> class.  This class also loads the <see cref="SceneItem"/> into memory at the
    /// beginning of a level load.  This class also used the <see cref="TerrainAlphaMaps"/>, <see cref="TerrainPickingRoutines"/>, and
    /// the <see cref="TerrainEditRoutines"/> classes.
    ///</summary>
    public class TerrainShape : ShapeWithPick, ITerrainShape
    {        

        #region Fields

        // 3/24/2010
        private static readonly Matrix MatrixIdenitityInvert = Matrix.Invert(Matrix.Identity);

        // 2/17/2010
        private static PopulatePathNodesParallelFor _pathNodesParallelFor;

        // 3/31/30211 - Controls when the Effect.Set occurs, which is now on the Update cycle.
        private static bool _updateDiffuseTextures;
        private static bool _updateBumpmapTextures;

        // 10/31/2008 - 
        ///<summary>
        /// Occurs when the <see cref="TerrainShape"/> is first created.
        ///</summary>
        public static event EventHandler TerrainShapeCreated;

        // 1/2/2010
        event EventHandler IMinimapTerrainShape.TerrainShapeCreated
        {
            add { TerrainShapeCreated += value; }
            remove { TerrainShapeCreated -= value; }
        }

        // 1/7/2010 - Content Manager
        private static ContentManager _contentManager;

        // 4/11/2009 - VisualCircles
        ///<summary>
        /// Reference for the <see cref="TerrainVisualCircles"/>.
        ///</summary>
        public static TerrainVisualCircles TerrainVisualCircles;

        // 3/2/2011 - Directional Icon
        /// <summary>
        /// Reference for the <see cref="TerrainDirectionalIcon"/>.
        /// </summary>
        public static TerrainDirectionalIcon TerrainDirectionalIcon;
                
        // 7/22/2008 - Add Game Instance
        private Game _gameInstance;        
        // 7/8/2008 - Add IShadowMap Interface
        ///<summary>
        /// Reference to the <see cref="IShadowMap"/> service.
        ///</summary>
        public static IShadowMap ShadowMapInterface;

        // 7/28/2008 - Add TerrainStorageRoutine Class for Indirect Inheritance
        private static TerrainStorageRoutines _terrainStorage;

        // 7/28/2008 - AlphaMaps Interface for indirect inheritance
        private static TerrainAlphaMaps _terrainAlphaMaps; 

        // 7/28/2008 - TerrainPickingRoutines Interface for indirect inheritance
        private static TerrainPickingRoutines _terrainPicking;

        // 7/29/2008 - TerrainEditingRoutines Class
        private static TerrainEditRoutines _terrainEditing;    
    
        // 7/29/2008 - TerrainAreaSelect Class
        private static TerrainAreaSelect _terrainAreaSelect; 

        // 5/31/2010 - Stores lightPosition.
        private static Vector3 _lightPosition = new Vector3(20,2300,0);
        
        private static DrawMode _drawMode = DrawMode.Solid;            
        
        // 7/30/2008 - Enable BumpMap
        private static bool _enableNormalMap;

        private List<VertexMultitextured_Stream1> _terrainVertices;      // Nullified after terrain patches are initialized.         

        // 4/30/2008; 4/9/2009: Updated to be static.
        // Array of Scene Items; for example, walls, buildings, houses, trees, bushes, etc
        // Note: This list is also used to search for Line-Of-Sight collisions in the Smoothing Algorithm.

        // 5/15/2009 - Store PerlinNoiseData for the Texture splatting.
        internal static PerlinNoiseData PerlinNoiseDataTexture1To2MixLayer1;
        internal static PerlinNoiseData PerlinNoiseDataTexture1To2MixLayer2;

        // 5/8/2009
        private Vector3 _ambientColorLayer1 = new Vector3(0.3f, 0.3f, 0.3f);       
        private Vector3 _specularColorLayer1 = new Vector3(1.0f, 1.0f, 1.0f);
        private Vector3 _ambientColorLayer2 = new Vector3(0.3f, 0.3f, 0.3f);
        private Vector3 _specularColorLayer2 = new Vector3(1.0f, 1.0f, 1.0f); 
        private Vector3 _diffuseColor = new Vector3(1.0f, 1.0f, 1.0f);       
        private float _ambientPowerLayer1 = 0.6f;        
        private float _specularPowerLayer1 = 128.0f;
        private float _ambientPowerLayer2 = 0.6f;
        private float _specularPowerLayer2 = 128.0f;  

        // Debug Settings
        static bool _drawTerrain = true;            // Default: true
        static bool _drawBoundingBoxes;       // Default: false 
       

        // 7/15/2008 - Add IGameConsole Interface
#if !XBOX360
        ///<summary>
        /// Reference to the <see cref="IGameConsole"/> service.
        ///</summary>
        public static IGameConsole GameConsole;
#endif

        // 5/12/2008 - DEBUG Purposes: Drawing white triangles for all _pathNodes
        private static TriangleShapeHelper _tShapeHelper;
        private static List<VertexPositionColor> _pathNodes;        
        internal static bool DisplayPathNodes; // DEBUG Purposes.                   

        // To keep things efficient, the picking works by first applying a bounding
        // sphere test, and then only bothering to test each individual triangle
        // if the ray intersects the bounding sphere. This allows us to quickly
        // reject many models without even needing to bother looking at their triangle
        // data. This field keeps track of which models passed the bounding sphere
        // test, so you can see the difference between this approximation and the more
        // accurate triangle picking.
        internal List<string> InsideBoundingSpheres;

        private static Effect _multiTerrainEffect;
  
        // 11/17/2008 - Add EffectParams & EffectTechniques
        private static EffectTechnique _multiTextured2Technique;
        private static EffectTechnique _multiTexturedAdditionalEffectsAll; // 3/22/2011 - XNA 4.0 Updates
        private static EffectTechnique _multiTerrainAdditionalEffectsShadowFow; // 5/26/2012 - Shadow-Mapping and Fog-Of-War
        private static EffectTechnique _multiTerrainAdditionalEffectsShadowPerlinNoise; // 5/26/2012 - Shadow-Mapping and PerlinNoise clouds.
        private static EffectTechnique _multiTerrainAdditionalEffectsShadow; // 5/26/2012 - Shadow-Mapping.
        private static EffectTechnique _multiTexturedDeferredTechnique; // 3/15/2009
        private static EffectTechnique _miniMapTechnique;
        private static EffectTechnique _multiTextureWireTechnique;
        private static EffectTechnique _editModeTechnique;
        private static EffectParameter _viewEParam;
        private static EffectParameter _viewInverseEParam; // 1/22/2010
        private static EffectParameter _projectionEParam;
        private static EffectParameter _worldEParam;
        private static EffectParameter _cameraPositionEParam;
        private static EffectParameter _gameTimeEParam; // 6/13/2010
        private static EffectParameter _shaderIndexEParam; // 3/30/2011

        // 1/23/2009 - Add ShadowMap EffectParams
        private static EffectParameter _lightPositionEParam;
        private static EffectParameter _enableShadowsEParam;
        private static EffectParameter _lightDiffuseEParam;
        private static EffectParameter _lightViewProjection; // 6/14/2010
        private static EffectParameter _lightViewProjectionStatic; // 6/14/2010


        // 1/23/2009 -Add FOW EffectParams
        private static EffectParameter _fowTextureEParam;

        // 12/12/2009 - Terrain's LightingType to use during rendering.
        private static TerrainLightingType _lightingType = TerrainLightingType.Blinn;

        // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
        private static RasterizerState _rasterizerState1;
        private static RasterizerState _rasterizerState2;
        private static DepthStencilState _depthStencilState1;
        private static DepthStencilState _depthStencilState2;

        #endregion

        #region Properties

        // 4/28/2009; 1/10/2011 - Rename from EnableBumpMap.
        ///<summary>
        /// Gets or Sets the use of the Normal-Mapping effect.
        ///</summary>
        public static bool EnableNormalMap
        {
            get { return _enableNormalMap; }
            set
            {
                _enableNormalMap = value;

                // Set PS via ShaderIndex
                if (_shaderIndexEParam != null)
                    _shaderIndexEParam.SetValue((value) ? 2 : 1);

                // Enable NormalMap
                //if (_multiTerrainEffect != null) 
                    //_multiTerrainEffect.Parameters["xEnableBumpMapping"].SetValue(value);
            }
        }

        // 4/8/2009
        ///<summary>
        /// Get or Set the <see cref="MapMarkerPositions"/> structure.
        ///</summary>
        public MapMarkerPositions MapMarkerPositions { get; set; }
         

        #region IAlphaMaps Interface Property Wrappers

        void ITerrainAlphaMaps.SetTextureMaps(Texture2D value)
        {
            _terrainAlphaMaps.SetTextureMaps(value);
        }

        Texture2D ITerrainAlphaMaps.GetTextureMaps()
        {
            return _terrainAlphaMaps.GetTextureMaps();
        }

        ///<summary>
        /// Scale to apply for each texel position of the <see cref="TerrainAlphaMaps"/>.
        ///</summary>
        public float AlphaScale
        {
            get { return _terrainAlphaMaps.AlphaScale; }
            set { _terrainAlphaMaps.AlphaScale = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-1.
        ///</summary>
        public float AlphaLy1Percent
        {
            get { return _terrainAlphaMaps.AlphaLy1Percent; }
            set { _terrainAlphaMaps.AlphaLy1Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-2.
        ///</summary>
        public float AlphaLy2Percent
        {
            get { return _terrainAlphaMaps.AlphaLy2Percent; }
            set { _terrainAlphaMaps.AlphaLy2Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-3.
        ///</summary>
        public float AlphaLy3Percent
        {
            get { return _terrainAlphaMaps.AlphaLy3Percent; }
            set { _terrainAlphaMaps.AlphaLy3Percent = value; }
        }

        ///<summary>
        /// Visiblity percent of texture-4.
        ///</summary>
        public float AlphaLy4Percent
        {
            get { return _terrainAlphaMaps.AlphaLy4Percent; }
            set { _terrainAlphaMaps.AlphaLy4Percent = value; }
        }

        ///<summary>
        /// Holds the pixel color data for layer-1/layer-2 in x/y, respectively.  Each channel, like the X channel, stores
        /// the blended pixel color for the choosen texture position (1-4).  Channel Z stores the bump data for Layer-1.
        ///</summary>
        public Vector4 PaintTexture
        {
            get { return _terrainAlphaMaps.PaintTexture; }
            set { _terrainAlphaMaps.PaintTexture = value; }
        }

        #endregion     

        #region IShadowMap Wrapper

        bool IShadowMap.IsVisible
        {
            get { return ShadowMapInterface.IsVisible; }
            set { ShadowMapInterface.IsVisible = value; }
        }

#if !XBOX360
        bool IShadowMap.DebugValues
        {
            get { return ShadowMapInterface.DebugValues; }
            set { ShadowMapInterface.DebugValues = value; }
        }
#endif
        #endregion

        ///<summary>
        /// Get or Set the bounding boxes used to determine which
        /// <see cref="TerrainQuadPatch"/> are within the camera frustum.
        ///</summary>
        public bool DrawBoundingBoxes
        {
            get { return _drawBoundingBoxes; }
            set { _drawBoundingBoxes = value; }
        }
        
        ///<summary>
        /// Get or Set reference for the <see cref="TerrainAreaSelect"/>.
        ///</summary>
        public TerrainAreaSelect AreaSelect
        {
            get { return _terrainAreaSelect; }
            set { _terrainAreaSelect = value; }
        }
        // 4/25/2008
        /// <summary>
        /// This is just a simple listing of the Parent Quad's which were Tessellated to LOD-2,
        /// which will be saved and used during the Load to duplicate the exact Tessellation for
        /// the entire Terrain.
        /// </summary>
        public List<int> QuadParentsTessellated { get; private set; }

        ///<summary>
        /// Get or Set the <see cref="Effect"/> used to draw the <see cref="TWEngine.Terrain"/>
        ///</summary>
        public Effect Effect
        {
            get { return _multiTerrainEffect; }
            set { _multiTerrainEffect = value; }
        }

        // 9/28/2009 -
        /// <summary>
        /// <see cref="TerrainTriggerAreas"/> which are setup using the 'PropertiesTools' form's 'Area' tab.
        /// </summary>
        public static TerrainTriggerAreas TriggerAreas { get; set; }

        // 10/13/2009
        /// <summary>
        /// Get or Set reference to the <see cref="TerrainWaypoints"/>.
        /// </summary>
        public static TerrainWaypoints Waypoints { get; set; }

        // 3/31/2008 
        /// <summary>
        /// Use BoundingBox dictionary to track all <see cref="TerrainQuadTree"/> individual BoundingBox values.
        /// These values are used to check which BoundingBox the cursor is over for Picking purposes.        
        /// </summary>
        public Dictionary<int, BoundingBox> TerrainBoundingBoxes { get; private set; }

        // 5/7/2008        
        /// <summary>
        /// Collection of texture names for default group 1; updated from the PaintTool Form.
        /// This is used to know which textures are being used as the 8 default textures; also used to
        /// add them back to the PaintTool containers when the Form loads.     
        /// </summary>
        public Dictionary<int, TexturesGroupData> TextureGroupData1 { get; private set; }

        /// <summary>
        /// Collectoin of Texture names for default group 2; updated from the PaintTool Form.
        /// This is used to know which textures are being used as the 8 default textures, also used to
        /// add them back to the PaintTool containers when the Form loads.    
        /// </summary>
        public Dictionary<int, TexturesGroupData> TextureGroupData2 { get; private set; }

        /// <summary>
        /// Used to track the first 10 draw calls of game.  This is currently used to
        /// FORCE the scenary items to draw when the game level starts; otherwise, they are
        /// only drawn when the camera frustum is updated.
        /// </summary>
        public static int FirstTenFramesOfGame { get; set; }
        

        private static List<Texture2D> _terrainTextures; // 8
        ///<summary>
        /// Collection of <see cref="Texture2D"/> used for the current <see cref="TWEngine.Terrain"/> map.
        ///</summary>
        public static List<Texture2D> TerrainTextures
        {
            get { return _terrainTextures; }
        }

        private static List<Texture2D> _terrainTextureNormals; // 4
        ///<summary>
        /// Collection of <see cref="Texture2D"/> normal-maps, used for the current <see cref="TWEngine.Terrain"/> map.
        ///</summary>
        public static List<Texture2D> TerrainTextureNormals
        {
            get { return _terrainTextureNormals; }
        }

        private static List<Texture3D> _terrainTextureVolumes; // 2

        /// <summary>
        /// Volume <see cref="Texture3D"/>; a stack of <see cref="Texture2D"/> in one Volume. 
        /// Collection is only for 2; layer-1 and layer-2.
        /// </summary>
        public static List<Texture3D> TerrainTextureVolumes
        {
            get { return _terrainTextureVolumes; }
        }

        private static List<string> _terrainTextureVolumeNames; // 2

        ///<summary>
        /// Collection of the names for the Volume <see cref="Texture3D"/>.
        ///</summary>
        public static List<string> TerrainTextureVolumeNames
        {
            get { return _terrainTextureVolumeNames; }
        }


        ///<summary>
        /// Get or set reference for the <see cref="TerrainEditRoutines"/>.
        ///</summary>
        public TerrainEditRoutines TerrainEditing
        {
            get { return _terrainEditing; }
            set { _terrainEditing = value; }
        }       

        ///<summary>
        /// Get or set reference for the <see cref="TerrainAlphaMaps"/>.
        ///</summary>
        public TerrainAlphaMaps AlphaMaps
        {
            get { return _terrainAlphaMaps; }
            set { _terrainAlphaMaps = value; }
        }

        ///<summary>
        /// Get or set reference for the ROOT <see cref="TerrainQuadTree"/>.
        ///</summary>
        public static TerrainQuadTree RootQuadTree { get; set; }

        ///<summary>
        /// Returns a collection of <see cref="ScenaryItemScene"/>.
        ///</summary>
        public List<ScenaryItemScene> ScenaryItems { get; protected set; }

        ///<summary>
        /// Get or set reference to the <see cref="GraphicsDevice"/>.
        ///</summary>
        public GraphicsDevice Device { get; set; }

       

        ///<summary>
        /// Get or set the <see cref="DrawMode"/> Enum.
        ///</summary>
        public static DrawMode DrawMode
        {
            get { return _drawMode; }
            set { _drawMode = value; }

        }         

        ///<summary>
        /// Ambient color for texture group layer-1.
        ///</summary>
        public Vector3 AmbientColorLayer1
        {
            get { return _ambientColorLayer1; }
            set 
            { 
                _ambientColorLayer1 = value;
                InitEffectParameters();
            }
        }

        /// <summary>
        /// Ambient color for texture group layer-2.
        /// </summary>
        public Vector3 AmbientColorLayer2
        {
            get { return _ambientColorLayer2; }
            set
            {
                _ambientColorLayer2 = value;
                InitEffectParameters();
            }
        }

        /// <summary>
        /// Ambient power for texture group layer-1.
        /// </summary>
        public float AmbientPowerLayer1
        {
            get { return _ambientPowerLayer1; }
            set 
            { 
                _ambientPowerLayer1 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Ambient power for texture group layer-2.
        ///</summary>
        public float AmbientPowerLayer2
        {
            get { return _ambientPowerLayer2; }
            set
            {
                _ambientPowerLayer2 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Ambient specular color for texture group layer-1.
        ///</summary>
        public Vector3 SpecularColorLayer1
        {
            get { return _specularColorLayer1; }
            set 
            { 
                _specularColorLayer1 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Ambient specular color for texture group layer-2.
        ///</summary>
        public Vector3 SpecularColorLayer2
        {
            get { return _specularColorLayer2; }
            set
            {
                _specularColorLayer2 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Ambient specular power for texture group layer-1.
        ///</summary>
        public float SpecularPowerLayer1
        {
            get { return _specularPowerLayer1; }
            set 
            { 
                _specularPowerLayer1 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Ambient specular power for texture group layer-2.
        ///</summary>
        public float SpecularPowerLayer2
        {
            get { return _specularPowerLayer2; }
            set
            {
                _specularPowerLayer2 = value;
                InitEffectParameters();
            }
        }

        ///<summary>
        /// Extra diffuse color to emit from texture groups.
        ///</summary>
        public Vector3 DiffuseColor
        {
            get { return _diffuseColor; }
            set 
            { 
                _diffuseColor = value;
                InitEffectParameters();
            }
        }

        // 2/13/2009 
        /// <summary>
        /// Sunlight position, used for <see cref="ShadowMap"/> and shader calculations.
        /// </summary>
        public static Vector3 LightPosition
        {
            get { return _lightPosition; }
            set
            {
                _lightPosition = value;

                // 5/31/2010
                if (_lightPositionEParam != null) 
                    _lightPositionEParam.SetValue(LightPosition);

                // 6/18/2010 - Update the InstancedModels effectParam.
                InstancedItem.SetSpecificEffectParam("xLightPos", value, null);
            }
        }

        // 5/19/2008
        /// <summary>
        /// This collection is used to store the 49 goalNode Transformations for Pathfinding.
        /// It is called upon when a group of units are selected and 'right-clicked' on
        /// the terrain to be moved, triggered from the <see cref="Player"/> class.
        /// </summary>
        public static List<Vector2> GoalNodeTransformations { get; private set; }  

        // 1/16/2011 - Updated to be static.
        /// <summary>
        /// The <see cref="TerrainIsIn"/> Enum.
        /// </summary>
        public static TerrainIsIn TerrainIsIn { get; internal set; }

        // 12/12/2009
        /// <summary>
        /// Get or set the <see cref="TerrainLightingType"/> Enum to use for rendering.
        /// </summary>
        public static TerrainLightingType LightingType
        {
            get { return _lightingType; }
            set
            {
                _lightingType = value;

                if (_multiTerrainEffect != null)
                    _multiTerrainEffect.Parameters["xLightingType"].SetValue((int)value);
            }
        }

        #endregion
        

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="terrainMode"><see cref="TerrainIsIn"/> Enum</param>
        public TerrainShape(Game game, TerrainIsIn terrainMode)
            : base(game)
        {
            // 7/22/2008 - Save Game Instance
            _gameInstance = game;
            var graphicsDevice = _gameInstance.GraphicsDevice; 
            var width = graphicsDevice.Viewport.Width;
            var height = graphicsDevice.Viewport.Height;

            // 4/6/2010: Updated to use 'ContenMiscLoc' global var.
            // 1/7/2010 - Create ContentManager
            _contentManager = new ContentManager(game.Services, TemporalWars3DEngine.ContentMiscLoc); // was "Content"

            // 4/10/2009 - Init Arrays
            TerrainBoundingBoxes = new Dictionary<int, BoundingBox>();
            QuadParentsTessellated = new List<int>();
            ScenaryItems = new List<ScenaryItemScene>();
            TextureGroupData1 = new Dictionary<int, TexturesGroupData>();
            TextureGroupData2 = new Dictionary<int, TexturesGroupData>();
            _pathNodes = new List<VertexPositionColor>();
            GoalNodeTransformations = new List<Vector2>(50);
            InsideBoundingSpheres = new List<string>();
            // 11/18/2009
            _terrainTextures = new List<Texture2D>(8); // 8
            _terrainTextureNormals = new List<Texture2D>(4); // 4
            _terrainTextureVolumes = new List<Texture3D>(2); // 2
            _terrainTextureVolumeNames = new List<string>(2); // 2

            // 9/28/2009 - Init Trigger Areas
            TriggerAreas = new TerrainTriggerAreas(game);
            game.Components.Add(TriggerAreas);

            // 10/13/2009 - Init Trigger Waypoints
            Waypoints = new TerrainWaypoints(game);
            game.Components.Add(Waypoints);

            // 8/12/2009 - Init the arrays
            for (var i = 0; i < 8; i++)
            {
                _terrainTextures.Add(null);

                if (i < 4)
                    _terrainTextureNormals.Add(null);

                if (i >= 2) continue;

                _terrainTextureVolumes.Add(null);
                _terrainTextureVolumeNames.Add(null);
            }

            // 4/11/2009 - Init TerrainVisualCircles class.
            TerrainVisualCircles = new TerrainVisualCircles(TemporalWars3DEngine.ContentGroundTextures); 

            // 7/28/2008 - Create TerrainStorage Class
            _terrainStorage = new TerrainStorageRoutines(game);

            // 7/28/2008 - Create TerrainPicking DrawableGame Component Class
            _terrainPicking = new TerrainPickingRoutines(ref game, this);
            //_terrainPicking.IsVisible = false;
            game.Components.Add(_terrainPicking);

            // 7/29/2008 - Create TerrainEditing Game Component Class
            _terrainEditing = new TerrainEditRoutines(game, this);
            game.Components.Add(_terrainEditing);

            // 7/29/2008 - Create TerrainAreaSelect DrawableGame Component Class
            _terrainAreaSelect = new TerrainAreaSelect(game);
            game.Components.Add(_terrainAreaSelect); 

            // 5/1/2008 - Set TerrainIsIn Mode
            TerrainIsIn = terrainMode;

            // 5/19/2008 - Populate the goalNodesTranformation Array for use with pathFinding.
            PopulateGoalNodesTransformationArray();

            // 7/31/2008 - Add ITerrainShape Interface to Services
            game.Services.AddService(typeof(ITerrainShape), this);

            // 12/31/2009 - Add IFOWTerrainShape Interface to Services
            game.Services.AddService(typeof(IFOWTerrainShape), this);
            // 1/2/2010 - Add IMiniTerrainShape Interface to Services
            game.Services.AddService(typeof(IMinimapTerrainShape), this);

            LoadingScreen.LoadingMessage = "Create Init Quads";
            // Creates Quad Terrain and Height data.
            CreateQuadTerrain();

            // Rebuild Normals for proper lighting!
            //terrainData.RebuildNormals(RootQuadTree); - Already done in the loading method.          

            // 7/8/2008 - Add IShadowMap Interface
            ShadowMapInterface = (IShadowMap)game.Services.GetService(typeof(IShadowMap));
            // 7/9/2008 - Add IFogOfWar Interface

            // 9/10/2008 - Initialize Effect Settings
            InitEffectParameters();

            // 3/2/2011 - Init Directional Icon class.
            //TerrainDirectionalIcon = new TerrainDirectionalIcon(_multiTerrainEffect, TemporalWars3DEngine.ContentGroundTextures);
            TerrainDirectionalIconManager.Initialize(_multiTerrainEffect); // 6/5/2012

            // NOTE: DEBUG
            //ScriptingActions.DisplayDirectionalIcon("DebugDirectionIcon", true, new Vector3(314.5f, 0, 56.8f), 50, 90f, Color.Blue);

            // 8/29/2008 - Enable BumpMapping for Terrain
            //_enableNormalMap = true;
            _multiTerrainEffect.Parameters["xEnableBumpMapping"].SetValue(_enableNormalMap);

            // 8/13/2008 - Add IGameConsole Interface
#if !XBOX360
            GameConsole = (IGameConsole)game.Services.GetService(typeof(IGameConsole));
#endif
            _tShapeHelper = new TriangleShapeHelper(ref game);

            // 2/16/2009 - Set the PCF Samples
            _multiTerrainEffect.Parameters["PCFSamples"].SetValue(ShadowMap.PcfSamples);

            // 11/17/2008 - Set EffectParams & EffectTechniques
            _multiTextured2Technique = _multiTerrainEffect.Techniques["MultiTextured2"];
            _multiTexturedAdditionalEffectsAll = _multiTerrainEffect.Techniques["MultiTexturedAdditionalTechAll"]; // 3/22/2011 - XNA 4.0 Updates
            _multiTerrainAdditionalEffectsShadowFow = _multiTerrainEffect.Techniques["MultiTexturedAdditionalTechShadowFow"]; // 5/26/2012
            _multiTerrainAdditionalEffectsShadowPerlinNoise = _multiTerrainEffect.Techniques["MultiTexturedAdditionalTechShadowPerlinNoise"]; // 5/26/2012
            _multiTerrainAdditionalEffectsShadow = _multiTerrainEffect.Techniques["MultiTerrainAdditionalTechShadow"]; // 5/26/2012
            _multiTexturedDeferredTechnique = _multiTerrainEffect.Techniques["MultiTextured_Deferred"]; // 3/15/2009
            _miniMapTechnique = _multiTerrainEffect.Techniques["MiniMapShader"];
            _multiTextureWireTechnique = _multiTerrainEffect.Techniques["MultiTextured2WireFrame"];
            _editModeTechnique = _multiTerrainEffect.Techniques["MultiTextured2_EditMode"];
            _worldEParam = _multiTerrainEffect.Parameters["xWorld"];
            _viewEParam = _multiTerrainEffect.Parameters["xView"];
            _viewInverseEParam = _multiTerrainEffect.Parameters["xViewInverse"]; // 1/22/2010
            _projectionEParam = _multiTerrainEffect.Parameters["xProjection"];
            _cameraPositionEParam = _multiTerrainEffect.Parameters["xCameraPosition"];
            _gameTimeEParam = _multiTerrainEffect.Parameters["xTime"]; // 6/13/2010
            _shaderIndexEParam = _multiTerrainEffect.Parameters["ShaderIndex"]; // 3/30/2011

            // 1/23/2009 - ShadowMap EffectParams
            _lightPositionEParam = _multiTerrainEffect.Parameters["xLightPos"];
            _enableShadowsEParam = _multiTerrainEffect.Parameters["xEnableShadows"];
            _lightDiffuseEParam = _multiTerrainEffect.Parameters["xLightDiffuse"];
            _lightViewProjection = _multiTerrainEffect.Parameters["xLightViewProjection"]; // 6/14/2010
            _lightViewProjectionStatic = _multiTerrainEffect.Parameters["xLightViewProjection_Static"]; // 6/14/2010

            // 5/30/2010 - Moved from 'SetShadowMapSettings' method, and put here instead.  Why? 
            //             Because if the 'ShadowMap' class is not created, then the 'LightPosition', 'World' and 'IWorld' was
            //             not being updated correctly, which consquently, was causing a pitch-black terrain!
            _lightDiffuseEParam.SetValue(Color.LightGray.ToVector4());
            _lightPositionEParam.SetValue(LightPosition);
            _worldEParam.SetValue(Matrix.Identity);
            _multiTerrainEffect.Parameters["xWorldI"].SetValue(MatrixIdenitityInvert); // 1/19/2010
       
            // 12/12/2009 - Set LightingType for Terrain to use during rendering.
            _multiTerrainEffect.Parameters["xLightingType"].SetValue((int) _lightingType);
                     

            // 1/23/2009 - FOW EffectParams
            _fowTextureEParam = _multiTerrainEffect.Parameters["TextureFog"];

            // 5/21/2010 - Generate TerrainPerlinClouds data.
            TerrainPerlinClouds.GenerateTextures(_multiTerrainEffect);

            // 1/10/2011 - Set Bloom Settings
            Bloom.SetBloomSettingToEffect(this);

            // 10/31/2008 - Fire EventHandler
            if (TerrainShapeCreated != null)
                TerrainShapeCreated(this, EventArgs.Empty);

            // XNA 4.0 Updates
            _rasterizerState1 = new RasterizerState { FillMode = FillMode.WireFrame, CullMode = CullMode.None };
            _rasterizerState2 = new RasterizerState { FillMode = FillMode.Solid, CullMode = CullMode.CullClockwiseFace };
            _depthStencilState1 = new DepthStencilState { DepthBufferEnable = false };
            _depthStencilState2 = new DepthStencilState { DepthBufferEnable = true };

        }

        ///<summary>
        /// Default empty constructor, which calls the main constructor.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public TerrainShape(Game game)
            : this(game, TerrainIsIn.PlayableMode)
        {
            return;
        }
       

        #region Initialization

        /// <summary>
        /// Creates the <see cref="TerrainQuadTree"/> instance used to draw the terrain.
        /// </summary>
        private void CreateQuadTerrain()
        {                    
            Device = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            //Content = base.GameInstance.Content;
            TerrainData.DetailDefaultLevel1 = LOD.DetailHigh2; // Was Minimum
            TerrainData.DetailDefaultLevel2 = LOD.DetailUltra1; // 5/18/2009
            TerrainData.Detail = TerrainData.DetailDefaultLevel1;

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // 1/7/2010 - Content Manager
            if (_contentManager == null)
                _contentManager = new ContentManager(_gameInstance.Services, TemporalWars3DEngine.ContentMiscLoc); // "Content"
            
            // MultiTerrain Shader Effect
            _multiTerrainEffect = _contentManager.Load<Effect>(@"Shaders\multiTerrainEffect");                
            _multiTerrainEffect.Parameters["xTexScale"].SetValue(0.1f);

            _terrainVertices = new List<VertexMultitextured_Stream1>();

            // XNA 4.0 Updates - VD obsolete here.
            //TerrainData.TerrainVertDeclaration = new VertexDeclaration(Device, VertexMultitexturedDeclaration.VertexElements);

            SetElevationStrength(30); // Changes elevation strength               

            // heightmap
            Initialize();


        }       

        /// <summary>
        /// Triggers the <see cref="TerrainData"/> class to load the HeightData collection into memory for the current map.
        /// Also creates the instance of the <see cref="TerrainAlphaMaps"/> class.
        /// </summary>        
        private void Initialize()
        {            
            LoadingScreen.LoadingMessage = "Create Init Quads: Height Data";
            TerrainData.LoadHeightData(TemporalWars3DEngine.ContentMaps, TerrainScreen.TerrainMapToLoad, TerrainScreen.TerrainMapGameType);

            LoadingScreen.LoadingMessage = "Create Init Quads: Create AlphaMap";
            // 7/28/2008 - Create AlphaMaps Interface Instance
            _terrainAlphaMaps = new TerrainAlphaMaps(_gameInstance);

            _terrainVertices.Clear();
            
        }

        // 6/30/2009
        /// <summary>
        /// Creates a new empty <see cref="TWEngine.Terrain"/>, with given <paramref name="mapWidth"/> 
        /// and <paramref name="mapHeight"/> size.
        /// </summary>
        /// <param name="mapWidth">Map width to use</param>
        /// <param name="mapHeight">Mpa height to use</param>
        public static void CreateNewEmptyTerrain(int mapWidth, int mapHeight)
        {
            // 1st - Create empty HeightData array
            TerrainData.CreateNewHeightData(mapWidth, mapHeight);

            // 2nd - Init AlphaMaps
            if (_terrainAlphaMaps != null)
                _terrainAlphaMaps.Dispose();

            _terrainAlphaMaps = new TerrainAlphaMaps(Game);
            TerrainAlphaMaps.CreateAlphaMaps();

            TerrainData.SetupTerrainVertexBuffer();
#if !XBOX360
            TerrainData.SetupVertexDataAndVertexLookup();
#endif

            // 3rd - Init new QuadTree
            RootQuadTree.ClearQuadTree();
            RootQuadTree = new TerrainQuadTree(Game, TerrainData.TerrainNormals.Count);
            // Store the Total Count 'TreeLeafList' Array, used in the Draw method.
            TerrainQuadTree.TreeLeafCount = TerrainQuadTree.TreeLeafList.Count;

            // 4th - Rebuild Normals
            var rootQuadTree = RootQuadTree;
            TerrainData.RebuildNormals(ref rootQuadTree);

            // 5th - Reset CameraBounds
            Camera.DoCameraBoundInit = true;

            // 6th - Update the AStarManager's internal neighbor arrays
            var aStarManager = (IAStarManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IAStarManager)); // 1/13/2010
            if (aStarManager != null) aStarManager.ReInitAStarArrays(TemporalWars3DEngine.SPathNodeSize);
        }

        // 9/10/2008
        /// <summary>
        /// Sets the internal <see cref="Effect"/> parameters.  
        /// </summary>
        /// <remarks>
        /// All effect parameters which do not need
        /// to be updated each draw cycle, should be placed into this method.
        /// </remarks>
        private void InitEffectParameters()
        {
            _multiTerrainEffect.Parameters["xAmbientColorLayer1"].SetValue(_ambientColorLayer1 * _ambientPowerLayer1);
            _multiTerrainEffect.Parameters["xAmbientColorLayer2"].SetValue(_ambientColorLayer2 * _ambientPowerLayer2);    
            _multiTerrainEffect.Parameters["xSpecularColorLayer1"].SetValue(_specularColorLayer1);
            _multiTerrainEffect.Parameters["xSpecularPowerLayer1"].SetValue(_specularPowerLayer1);                  
            _multiTerrainEffect.Parameters["xSpecularColorLayer2"].SetValue(_specularColorLayer2);
            _multiTerrainEffect.Parameters["xSpecularPowerLayer2"].SetValue(_specularPowerLayer2);
            _multiTerrainEffect.Parameters["xDiffuseColor"].SetValue(_diffuseColor);            
           
            // 7/30/2008 - Enable BumpMap
            _multiTerrainEffect.Parameters["xEnableBumpMapping"].SetValue(_enableNormalMap);

            // XNA 4.0 updates.
            //_multiTerrainEffect.CommitChanges();

        }
       

        /// <summary>
        /// Sets up 8 default textures that the terrain can use.
        /// </summary>        
        /*public void SetDefaultTerrainTextures()
        {
            // Load 8 Default Textures.
            terrainTextures[0] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\dryCrackedGround");
            terrainTextures[1] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\rockyBeach");
            terrainTextures[2] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\Mud02");
            terrainTextures[3] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\ConcreteRough");

            terrainTextures[4] = ContentManager.Load<Texture2D>(@"Terrain\texturesDesertSet\DesertRoughGround01");
            terrainTextures[5] = ContentManager.Load<Texture2D>(@"Terrain\texturesDesertSet\DesertRoughGround02");
            terrainTextures[6] = ContentManager.Load<Texture2D>(@"Terrain\texturesDesertSet\DesertRock01");
            terrainTextures[7] = ContentManager.Load<Texture2D>(@"Terrain\texturesDesertSet\DesertRock02");

            // 8/7/2008 - Add to TextureGroup Dictionaries.
            {
                TextureGroupData_AddRecord(0, "MaPZone[Dry_Cracked_Ground_diffuse].bmp", "MaPZone[Dry_Cracked_Ground_diffuse].bmp", @"Terrain\texturesGroundSet\dryCrackedGround", 1);
                TextureGroupData_AddRecord(1, "MaPZone[Rocky_Beach_diffuse].bmp", "MaPZone[Rocky_Beach_diffuse].bmp", @"Terrain\texturesGroundSet\rockyBeach", 1);
                TextureGroupData_AddRecord(2, "MaPZone[Mud_02_diffuse].bmp", "MaPZone[Mud_02_diffuse].bmp", @"Terrain\texturesGroundSet\Mud02", 1);
                TextureGroupData_AddRecord(3, "MaPZone[Concrete_Rough_diffuse].bmp", "MaPZone[Concrete_Rough_diffuse].bmp", @"Terrain\texturesGroundSet\ConcreteRough", 1);

                TextureGroupData_AddRecord(0, "DesertRoughGround01.png", "DesertRoughGround01.png", @"Terrain\texturesDesertSet\DesertRoughGround01", 2);
                TextureGroupData_AddRecord(1, "DesertRoughGround02.png", "DesertRoughGround02.png", @"Terrain\texturesDesertSet\DesertRoughGround02", 2);
                TextureGroupData_AddRecord(2, "DesertRock01.png", "DesertRock01.png", @"Terrain\texturesDesertSet\DesertRock01", 2);
                TextureGroupData_AddRecord(3, "DesertRock02.png", "DesertRock02.png", @"Terrain\texturesDesertSet\DesertRock02", 2);
            }

            // 7/23/2008
            UpdateEffectDiffuseTextures();
        }*/

        /// <summary>
        /// Sets up 4 Default BumpMapping Textures.
        /// </summary>
        /*public void SetDefaultTerrainNormalsTextures()
        {           

            terrainTextureNormals[0] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\dryCrackedGroundNormal");
            terrainTextureNormals[1] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\rockyBeachNormal");
            terrainTextureNormals[2] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\Mud02Normal");
            terrainTextureNormals[3] = ContentManager.Load<Texture2D>(@"Terrain\texturesGroundSet\ConcreteRoughNormal");

            // 7/29/2008
            UpdateEffectBumpMapTextures();
        }
        */

        // 9/10/2008; 1/23/2009- Updated to use EffectParam
        /// <summary>
        /// Sets the <see cref="IFogOfWar"/> texture into <see cref="Effect"/>.  
        /// </summary>
        /// <remarks>Called from the <see cref="IFogOfWar"/> component.</remarks>
        /// <param name="fowTexture"><see cref="Texture2D"/> instance</param>
        public void SetFogOfWarTextureEffect(Texture2D fowTexture)
        {
            if (_fowTextureEParam != null)
                _fowTextureEParam.SetValue(fowTexture);            

        }

        // 9/10/2008
        /// <summary>
        /// Sets the <see cref="IFogOfWar"/> texture into <see cref="Effect"/>. 
        /// </summary>
        /// <remarks>Called from the <see cref="IFogOfWar"/> component.</remarks>
        /// <param name="isVisible">Sets the isVisible flag</param>
        public void SetFogOfWarSettings(bool isVisible)
        {
            // 5/28/2008 - Enable FOW
            if (_multiTerrainEffect == null) return;

            _multiTerrainEffect.Parameters["xEnableFogOfWar"].SetValue(isVisible);

            // XNA 4.0 updates
            //_multiTerrainEffect.CommitChanges();
        }

        
        // 9/10/2008; 
        /// <summary>
        /// Sets the 'Dynamic' <see cref="ShadowMap"/> texture into <see cref="Effect"/>.         
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component. </remarks>
        /// <param name="dynamicShadowMapTexture"><see cref="Texture2D"/> instance</param>
        public void SetDynamicShadowMap(Texture2D dynamicShadowMapTexture)
        {            
            // Dynamic ShadowMap 
            if (_multiTerrainEffect != null)
                _multiTerrainEffect.Parameters["DynamicShadowMap"].SetValue(dynamicShadowMapTexture);
            
        }

        // 12/12/2009
        /// <summary>
        /// Sets the Static <see cref="ShadowMap"/> texture into <see cref="Effect"/>.          
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component. </remarks>
        /// <param name="staticShadowMapTexture"><see cref="Texture2D"/> instance</param>
        public void SetStaticShadowMap(Texture2D staticShadowMapTexture)
        {
            if (_multiTerrainEffect != null)
                _multiTerrainEffect.Parameters["StaticShadowMap"].SetValue(staticShadowMapTexture);
        }

        // 9/10/2008 - 7/9/2010: Updated to pass one AlphaMap and not an array.
        /// <summary>
        /// Sets the <see cref="TerrainAlphaMaps"/> texture into <see cref="Effect"/>.  
        /// </summary>
        /// <remarks>Called from the <see cref="TerrainAlphaMaps"/> class.</remarks>
        /// <param name="alphaScale">Alpha scale</param>
        /// <param name="textureMapToUse"><see cref="Texture2D"/> instance to use</param>
        public static void SetAlphaMapsTextureEffect(float alphaScale, Texture2D textureMapToUse)
        {
            // 4/21/2008 - Added AlphaMaps which now do the MultiTexturing!
            if (_multiTerrainEffect == null) return;

            _multiTerrainEffect.Parameters["xAlphaMap1"].SetValue(textureMapToUse);                
            _multiTerrainEffect.Parameters["xAlphaScale"].SetValue(alphaScale);

            // XNA 4.0 updates
            //_multiTerrainEffect.CommitChanges();
        }

        // 5/30/2010: Removed the setting of the 'LightPosition', and moved to this Init method.
        // 9/10/2008 ; 6/4/2009 - Added lightView2, lightView3, lightView4 params.
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> settings into <see cref="Effect"/>.  
        /// </summary>
        /// <remarks>Called from the <see cref="ShadowMap"/> component.</remarks>
        /// <param name="isVisible">Sets the isVisible flag</param>       
        /// <param name="lightView"><see cref="Matrix"/> light view</param>       
        /// <param name="lightProj"><see cref="Matrix"/> light projection</param>
        /// <param name="lightViewStatic"><see cref="Matrix"/> light view static</param>
        /// <param name="lightProjStatic"><see cref="Matrix"/> light projection static</param>
        public void SetShadowMapSettings(bool isVisible, ref Matrix lightView, ref Matrix lightProj, 
                                          ref Matrix lightViewStatic, ref Matrix lightProjStatic)
        {
            try
            {
                // 6/3/2008 - Shadow Map Settings
                if (_multiTerrainEffect == null) return;

                // 4/2/2009 - Calc the Light/View projection
                Matrix lightViewProj;
                Matrix.Multiply(ref lightView, ref lightProj, out lightViewProj);

                // 7/10/2009 - Calc the Light/View projection for STATIC
                Matrix lightViewProjStatic;
                Matrix.Multiply(ref lightViewStatic, ref lightProjStatic, out lightViewProjStatic);

                // Set EffectParams
                _enableShadowsEParam.SetValue(isVisible);

                if (_lightViewProjection != null) _lightViewProjection.SetValue(lightViewProj); // 3/24/2010
                if (_lightViewProjectionStatic != null) _lightViewProjectionStatic.SetValue(lightViewProjStatic); // 3/24/2010

                // 4/28/2010 - If 'DeferredRendering' style, then also set shadow atts into Deferred2lights shader.
                if (ScreenManager.RenderingType == RenderingType.DeferredRendering)
                {
                    var lightPosition = LightPosition;
                    DeferredRenderingStyle.SetShadowMapSettings(isVisible, ref lightView, ref lightProj, ref lightViewStatic, ref lightProjStatic, ref lightPosition);
                }

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                //_multiTerrainEffect.CommitChanges();
            }
            catch (Exception)
            {
                System.Console.WriteLine(@"Method 'SetShadowMapSettings' threw an exception.");
            }
           
        }

        // 12/12/2009
        /// <summary>
        /// Sets the proper <see cref="ShadowMap.ShadowType"/> Enum to use into the <see cref="Effect"/>;
        /// 1) Simple
        /// 2) Percentage-Close-Filter#1 (Technique 1).
        /// 3) Percentage-Close-Filter#2 (Technique 2).
        /// 4) Variance.
        /// </summary>
        /// <param name="shadowType"><see cref="ShadowMap.ShadowType"/> Enum to use</param>
        public void SetShadowMapType(ShadowMap.ShadowType shadowType)
        {
            if (_multiTerrainEffect != null) 
                _multiTerrainEffect.Parameters["xShadowType"].SetValue((int)shadowType);
        }

        // 12/12/2009
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> darkness, using a value between 0-1.0, with
        /// 1.0 being completely white with no shadow.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="shadowDarkness"/> not within allowable range 0.0 - 1.0.</exception>
        /// <param name="shadowDarkness"><see cref="ShadowMap"/> Darkness level (0-1.0f)</param>
        public void SetShadowMapDarkness(float shadowDarkness)
        {
            // check if outside allowable range
            if (shadowDarkness < 0 || shadowDarkness > 1.0f)
                throw new ArgumentOutOfRangeException("shadowDarkness", @"ShadowMap darkness value MUST be in 0.0 - 1.0 range.");

            if (_multiTerrainEffect != null) 
                _multiTerrainEffect.Parameters["xShadowDarkness"].SetValue(shadowDarkness);
        }

        // 12/12/2009
        /// <summary>
        /// Sets the <see cref="ShadowMap"/> HalfPixel correction setting.
        /// </summary>
        /// <param name="halfPixel">HalfPixel correction setting</param>
        public void SetShadowMapHalfPixel(ref Vector2 halfPixel)
        {
            // Set ShadowMap HalfPixel
            if (_multiTerrainEffect != null) 
                _multiTerrainEffect.Parameters["xHalfPixel"].SetValue(halfPixel);
        }

        #endregion

        // 8/25/2009
        static VertexPositionColor[] _trianglePathNodes = new VertexPositionColor[1];
        


        // 4/15/2009: Updated to be STATIC.              
        ///<summary>
        /// Draws the A* solutions path nodes flooding values, when the <see cref="DisplayPathNodes"/> is set
        /// to TRUE. 
        ///</summary>
        ///<param name="gameTime"><see cref="GameTime"/> instance</param>
        ///<param name="additionalEffects"></param>
        ///<remarks>Used for DEBUG purposes only.</remarks>
        public static void Render(GameTime gameTime, bool additionalEffects)
        {
            var cameraView = Camera.View;

            // 3/22/2011 - XNA 4.0 Updates - New 2-pass RT method.
            // Draws Quad Terrain
            switch (_drawMode)
            {
                case DrawMode.EditMode:
                   Draw(ref cameraView, gameTime, false);
                    break;
                case DrawMode.Solid:
                case DrawMode.WireFrame:
                case DrawMode.MiniMap:
                    Draw(ref cameraView, gameTime, additionalEffects);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // DEBUG: Draw PathNodes on Terrain
            DrawPathNodes();
        }

        /// <summary>
        /// Helper method which draws the path nodes onto the terrain.
        /// </summary>
        private static void DrawPathNodes()
        {
            if (!DisplayPathNodes) return;

            // 9/25/2008 - Removed the 'ToArray' call below, since creates lots of garbage!
            var count = _pathNodes.Count;
            if (_trianglePathNodes.Length < count)
                Array.Resize(ref _trianglePathNodes, count);
                
            _pathNodes.CopyTo(_trianglePathNodes);

            // XNA 4.0 Updates - Final 2 params updated.
            TriangleShapeHelper.DrawPrimitiveTriangle(ref _trianglePathNodes, count, _rasterizerState2, _depthStencilState1);
        }


        // 1/1/2010 - Add Non-Static draw method, used to draw Minimap for Minimap library.
        /// <summary>
        /// Draws the <see cref="TWEngine.Terrain"/> landscape, using a top-down-view, 
        /// for the <see cref="IMinimap"/> component.
        /// </summary>
        public void DrawMiniMap()
        {
            // Render the Terrain to renderTarget
            DrawMode = DrawMode.MiniMap;
            var cameraView = Camera.View;
            Draw(ref cameraView, null, ShadowMap.IsVisibleS);
            DrawMode = DrawMode.Solid;
        }
       

        // 4/15/2009: Updated to be STATIC.
        // 1/28/2009 - Removed Ops Overload of Matrix Multi, since this is slow on XBOX!            
        /// <summary>
        /// Draws the <see cref="TWEngine.Terrain"/>.
        /// </summary>
        /// <param name="currentViewMatrix"><see cref="Matrix"/> view</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="additionalEffects"></param>
        public static void Draw(ref Matrix currentViewMatrix, GameTime gameTime, bool additionalEffects)
        {
            try // 7/9/2010
            {
                // 8/26/2009 - Cache
                var rootQuadTree = RootQuadTree;
                var drawMode = _drawMode; // 5/19/2010 - Cache
                var projection = Camera.Projection; // 5/19/2010 - Cache
                var cameraPosition = Camera.CameraPosition; // /5/19/2010 - Cache
                var boundingFrustum = Camera.CameraFrustum; // 5/19/2010 - Cache
                var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
               
                if (_drawTerrain)
                {
                    // XNA 4.0 Updates - VertexBuffers now set in one Atomic operation.
                    #region OLDCode
                    // 5/26/2010: Test using streams 4-6
                    // Set Stream-1
                    /*vertexStreamCollection[3].SetSource(terrainVertexBufferStream1, 0,
                                                        VertexMultitextured_Stream1.SizeInBytes);
                    // 7/8/2009 - Set Stream-2; TextureCords data
                    vertexStreamCollection[4].SetSource(terrainVertexBufferStream2, 0,
                                                        VertexMultitextured_Stream2.SizeInBytes);
                    // 7/8/2009 - Set Stream-3; Tangent data
                    vertexStreamCollection[5].SetSource(terrainVertexBufferStream3, 0,
                                                        VertexMultitextured_Stream3.SizeInBytes);*/
                    #endregion

                    // XNA 4.0 Updates - VertexDeclaration Only set at creation time with VertexBuffer.
                    //graphicsDevice.VertexDeclaration = TerrainData.TerrainVertDeclaration;

                    // 4/10/2008 - Choose which Shader Technique to use for DrawMode.
                    // 11/20/2008 - Updated to include 'EditMode' check.
                    var multiTerrainEffect = _multiTerrainEffect; // 5/19/2010 - Cache

                    // 5/26/2012 - Updated to a new method call.
                    SetCurrentTechnique(multiTerrainEffect, additionalEffects);

                    switch (drawMode)
                    {
                        case DrawMode.Solid:
                        
                            switch (ScreenManager.RenderingType)
                            {
                                case RenderingType.DeferredRendering:
                                case RenderingType.NormalRendering:
                                case RenderingType.NormalRenderingWithPostProcessEffects:

                                    graphicsDevice.SetVertexBuffer(TerrainData.TerrainVertexBufferStream1);

                                    // 4/11/2009 - Set VisualCircles
                                    TerrainVisualCircles.SetEffectParameters(multiTerrainEffect);

                                    // 6/6/2012 - Set DirectionIcons
                                    TerrainDirectionalIconManager.DrawIcons();
                                   
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case DrawMode.MiniMap:
                            graphicsDevice.SetVertexBuffer(TerrainData.TerrainVertexBufferStream1);
                            multiTerrainEffect.CurrentTechnique = _miniMapTechnique;
                            break;
                        case DrawMode.EditMode:
#if EditMode
                            graphicsDevice.SetVertexBuffer(!TerrainEditRoutines.CurrentVertexBuffer
                                                                ? TerrainData.TerrainVertexBufferStream1
                                                                : TerrainData.TerrainVertexBufferStream1A);
                           

                            TerrainEditRoutines.GroundCursorTextureParam.SetValue(TerrainEditRoutines.GroundCursorTex);
                            TerrainEditRoutines.PaintCursorTextureParam.SetValue(TerrainEditRoutines.PaintCursorTex);
                            TerrainEditRoutines.GroundCursorPositionParam.SetValue(TerrainEditRoutines.GroundCursorPosition);
                            TerrainEditRoutines.GroundCursorSizeParam.SetValue(TerrainEditRoutines.GroundCursorSize);
                            TerrainEditRoutines.PaintCursorSizeParam.SetValue(TerrainEditRoutines.PaintCursorSize);
                            TerrainEditRoutines.ShowHeightCursorParam.SetValue(TerrainEditRoutines.ShowHeightCursor);
                            TerrainEditRoutines.ShowPaintCursorParam.SetValue(TerrainEditRoutines.ShowPaintCursor);
#endif
                            break;
                        default:
                            graphicsDevice.SetVertexBuffer(TerrainData.TerrainVertexBufferStream1);
                            // XNA 4.0 - Now set RasterizerState, rather than use Technique.
                            multiTerrainEffect.CurrentTechnique = _multiTextureWireTechnique;
                            //graphicsDevice.RasterizerState = _rasterizerStateWire;
                            break;
                    }

                    //
                    // 11/17/2008 - Updated to use EffectParams                 
                    _viewEParam.SetValue(currentViewMatrix);
                    _viewInverseEParam.SetValue(Matrix.Invert(currentViewMatrix)); // 1/22/2010

                    // 5/21/2010 - Update gameTime; 6/13/2010 - Updated to use EffectParam.
                    if (_gameTimeEParam != null)
                        _gameTimeEParam.SetValue((gameTime == null) ? 0 : (float)gameTime.TotalGameTime.TotalMilliseconds);

                    _projectionEParam.SetValue(projection);
                    _cameraPositionEParam.SetValue(cameraPosition);
                    //

                    // 8/28/2008 - Draws Terrain
                    switch (drawMode)
                    {
                        case DrawMode.MiniMap:
                            TerrainQuadTree.Draw(ref rootQuadTree, multiTerrainEffect);  // 7/8/2009
                            break;
                        default:
                            // 6/13/2010 - At beg of game level, this forces the scenary items to 
                            //             update; otherwise, they won't be seen until camera movement.
                            if (FirstTenFramesOfGame < 10)
                            {
                                FirstTenFramesOfGame++;
                                TerrainQuadTree.UpdateSceneryCulledList = true;
                                ShadowMap.DoPreShadowMapTextures = true; // Also Force Static redraws.
                            }

                            // Clear ScenaryItems
                            if (TerrainQuadTree.UpdateSceneryCulledList)
                                InstancedItem.ClearSceneryInstancesCulledList();
                            // Draw Terrain
                            TerrainQuadTree.Draw(ref rootQuadTree, boundingFrustum, multiTerrainEffect); // 7/8/2009
                            // Reset Flag
                            if (TerrainQuadTree.UpdateSceneryCulledList)
                                TerrainQuadTree.UpdateSceneryCulledList = false;

                            break;
                    }

                }

#if DEBUG && !XBOX360
                // 7/15/2009
                // Debugging Purposes Only
                if (_drawBoundingBoxes)
                {
                    // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
                    /*graphicsDevice.RenderState.FillMode = FillMode.WireFrame;
                    graphicsDevice.RenderState.CullMode = CullMode.None;
                    graphicsDevice.RenderState.DepthBufferEnable = false;*/
                    graphicsDevice.RasterizerState = _rasterizerState1;
                    graphicsDevice.DepthStencilState = _depthStencilState1;

                    TerrainQuadTree.DrawBoundingBox(rootQuadTree, boundingFrustum);

                    // XNA 4.0 Updates - RenderState replaced with 4 new states; BlendState, RasterizerState, DepthStencilState, or SampleState.
                    //graphicsDevice.RenderState.FillMode = FillMode.Solid;
                    graphicsDevice.RasterizerState = _rasterizerState2;
                    graphicsDevice.DepthStencilState = _depthStencilState2;
                }

#endif
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Draw method in TerrainShape class threw the exception " + ex.Message ?? "No Message");
            }
        }

        // 5/26/2012
        /// <summary>
        /// Helper method which sets the technique depending on the effects required.
        /// </summary>
        /// <param name="multiTerrainEffect">Instance of <see cref="Effect"/>.</param>
        /// <param name="additionalEffects">Use addtional effects like Shadows, FOW and PerlinNoise clouds?</param>
        private static void SetCurrentTechnique(Effect multiTerrainEffect, bool additionalEffects)
        {
            // retrieve Visibility interface.
            var fow = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar)); // 5/26/2012

            var additionalEffectsShadows = ShadowMap.IsVisibleS; // 5/26/2012
            var additionalEffectsFogOfWar = fow != null && fow.IsVisible; // 5/26/2012; 10/7/2012 - Check if null.
            var additionalEffectsPerlinNoise = TerrainPerlinClouds.EnableClouds; // 5/26/2012
            var multiTextured2Technique = _multiTextured2Technique;
            var multiTexturedAdditionalEffectsAll = _multiTexturedAdditionalEffectsAll; // 5/26/2012
            var multiTerrainAdditionalEffectsShadow = _multiTerrainAdditionalEffectsShadow; // 5/26/2012
            var multiTerrainAdditionalEffectsShadowFow = _multiTerrainAdditionalEffectsShadowFow; // 5/26/2012
            var multiTerrainAdditionalEffectsShadowPerlinNoise = _multiTerrainAdditionalEffectsShadowPerlinNoise; // 5/26/2012

            // set to regular technique?
            if (!additionalEffects)
            {
                multiTerrainEffect.CurrentTechnique = multiTextured2Technique;
                return;
            }
          
            //
            // No, then set to which special effect?
            //

            // Set to Shadows only?
            if (additionalEffectsShadows && !additionalEffectsFogOfWar && !additionalEffectsPerlinNoise)
            {
                multiTerrainEffect.CurrentTechnique = multiTerrainAdditionalEffectsShadow;
                return;
            }

            // Set to Shadows and Fog-Of-War?
            if (additionalEffectsShadows && additionalEffectsFogOfWar && !additionalEffectsPerlinNoise)
            {
                multiTerrainEffect.CurrentTechnique = multiTerrainAdditionalEffectsShadowFow;
                return;
            }

            // Set to Shadows and PerlinNoise?
            if (additionalEffectsShadows && !additionalEffectsFogOfWar)
            {
                multiTerrainEffect.CurrentTechnique = multiTerrainAdditionalEffectsShadowPerlinNoise;
                return;
            }

            // Set All Effect
            multiTerrainEffect.CurrentTechnique = multiTexturedAdditionalEffectsAll;
            
        }

        // 1/28/2009 - Removed Ops Overload of Matrix Multi, since this is slow on XBOX!
        // 7/23/2008 - Updated the 'CameraFrustum' to use the Shadows View & Light Matricies.
        /// <summary>
        /// Draws the <see cref="ShadowMap"/> for the Static terrain items.
        /// </summary>
        /// <param name="lightPos"><see cref="Vector3"/> as light position</param>
        /// <param name="lightView"><see cref="Matrix"/> light view</param>
        /// <param name="lightProj"><see cref="Matrix"/> light projection</param>
        public void DrawForShadowMap(ref Vector3 lightPos, ref Matrix lightView, ref Matrix lightProj)
        {
            if (!_drawTerrain) return;
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 Updates - Streams removed; VertexBuffers set in one atomic operation.
            //                               Also, VertexDeclarations now created with VertexBuffers.
            /*var vertexStreamCollection = graphicsDevice.Vertices;
            vertexStreamCollection[0].SetSource(TerrainData.TerrainVertexBufferStream1, 0, VertexMultitextured_Stream1.SizeInBytes);
            // 7/8/2009 - Set Stream-2; TextureCords data
            vertexStreamCollection[1].SetSource(TerrainData.TerrainVertexBufferStream2, 0, VertexMultitextured_Stream2.SizeInBytes);
            graphicsDevice.VertexDeclaration = TerrainData.TerrainVertDeclaration;*/
            graphicsDevice.SetVertexBuffer(TerrainData.TerrainVertexBufferStream1);

            // 8/29/2008: Updated to use the 'TerrainShadowMapRender'.                
            ShadowMap.ShadowMapEffect.CurrentTechnique = ShadowMap.ShadowMapEffect.Techniques["TerrainShadowMapRender"];

            ShadowMap.ShadowMapEffect.Parameters["xWorld"].SetValue(Matrix.Identity);
            ShadowMap.ShadowMapEffect.Parameters["xLightView"].SetValue(lightView);
            ShadowMap.ShadowMapEffect.Parameters["xLightProjection"].SetValue(lightProj);
            ShadowMap.ShadowMapEffect.Parameters["xLightPos"].SetValue(lightPos); // 2/6/2009 

            // XNA 4.0 Updates - Obsolete CommiteChanges()
            //ShadowMap.ShadowMapEffect.CommitChanges();

            //rootQuadTree.Draw(cameraFrustum, ShadowMap.ShadowMapEffect); // 7/8/2009
            var rootQuadTree = RootQuadTree; // 8/26/2009
            TerrainQuadTree.Draw(ref rootQuadTree, ShadowMap.ShadowMapEffect); // 7/10/2009
        }

        
        ///<summary>
        /// Sets the negate of the internal <see cref="_drawTerrain"/> boolean value.
        ///</summary>
        public void ToggleTerrainDraw()
        {
            _drawTerrain = !_drawTerrain;
        }

        ///<summary>
        /// Sets the negate of the internal <see cref="_drawBoundingBoxes"/> boolean value.
        ///</summary>
        public void ToggleBoundingBoxDraw()
        {
            _drawBoundingBoxes = !_drawBoundingBoxes;
        }

        ///<summary>
        /// Sets the strength into the <see cref="TerrainData.ElevationStrength"/> property.
        ///</summary>
        ///<param name="strength">Enter a value between 0 through 100.</param>
        /// <remarks>Any value entered into the <paramref name="strength"/> parameter will always be multiplied by the
        /// constant value of ten.</remarks>
        /// <exception cref="strength">This exception is thrown when <paramref name="strength"/> is outside the allowable
        /// range of 0 through 100.</exception>
        public void SetElevationStrength(float strength)
        {
            if (strength < 0 || strength > 100)
                throw new ArgumentOutOfRangeException("strength",@"Value must be in between the range of 0 through 100.");

            TerrainData.ElevationStrength = strength;
        }

        /// <summary>
        /// Sets the minimum leaf size for <see cref="TerrainQuadPatch"/>. Must be a power of two.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> is not a power of 2.</exception>
        /// <param name="width">Minimum leaf size width (also sets height to match)</param>
        public void SetLeafSize(int width)
        {
            if (!MathUtils.IsPowerOf2(width))
                throw new ArgumentOutOfRangeException("width",@"Leaf size must be set to a power of two");

            TerrainData.MinimumLeafSize = width*width; // All maps must be square, so only width is needed
        }

        // 3/31/2011
        /// <summary>
        /// Updates the <see cref="Shape"/>. 
        /// </summary>
        /// <remarks>Base class does nothing.</remarks>
        /// <param name="time"><see cref="TimeSpan"/> struct with time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> struct with elapsed time since last call</param>
        public override void Update(ref TimeSpan time, ref TimeSpan elapsedTime)
        {
            // 3/31/2011
            if (_updateDiffuseTextures)
            {
                DoDiffuseTextureUpdateToEffect();
                _updateDiffuseTextures = false;
            }

            // 3/31/2011
            if (_updateBumpmapTextures)
            {
                DoBumpMapTexturesUpdateToEffect();
                _updateBumpmapTextures = false;
            }

            base.Update(ref time, ref elapsedTime);
        }

        
        /// <summary>
        /// Sets the <see cref="_updateDiffuseTextures"/> flag to TRUE, which
        /// causes an update of the volume textures on the next 'Update' cycle.
        /// </summary>
        public static void UpdateEffectDiffuseTextures()
        {
            // Set Flag to update on next cycle.
            _updateDiffuseTextures = true;

        }

        // 3/31/3011
        /// <summary>
        /// Sets the <see cref="TWEngine.Terrain"/> texture Volumes into the <see cref="Effect"/> for the terrain.
        /// When no texture volumes exist, this method attempts to recreate the texture volumes, by using the
        /// collection <see cref="TerrainTextures"/>; index 0-3 = Volume#1, and index 4-7 = Volume#2.
        /// </summary>
        private static void DoDiffuseTextureUpdateToEffect()
        {
            var textureSize = (TemporalWars3DEngine.TerrainTexturesQuality == ImageNexus.BenScharbach.TWEngine.BeginGame.Enums.TerrainTextures.Tex128X) ? 128 
                                  : (TemporalWars3DEngine.TerrainTexturesQuality == ImageNexus.BenScharbach.TWEngine.BeginGame.Enums.TerrainTextures.Tex256X) ? 256 : 512;
          
            // 1/21/2009 - Volume Texture creation; a stack of Texture2D are put into one Texture3D volume!
            if (TerrainTextureVolumes[0] == null)
            {
                var elementCount = textureSize * textureSize;
                const int numberOfTextures = 4;

                // XNA 4.0 Updates
                //var volumeTexture = new Texture3D(Game.GraphicsDevice, textureSize, textureSize, numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                var volumeTexture = new Texture3D(Game.GraphicsDevice, textureSize, textureSize, numberOfTextures, true, SurfaceFormat.Color);
                
                var tmpData = new Color[elementCount * numberOfTextures];
                TerrainTextures[0].GetData(0, null, tmpData, 0, elementCount);
                TerrainTextures[1].GetData(0, null, tmpData, elementCount * 1, elementCount);
                TerrainTextures[2].GetData(0, null, tmpData, elementCount * 2, elementCount);
                TerrainTextures[3].GetData(0, null, tmpData, elementCount * 3, elementCount);
                volumeTexture.SetData(tmpData);
                TerrainTextureVolumes[0] = volumeTexture;
            }

            if (TerrainTextureVolumes[1] == null)
            {
                var elementCount = textureSize * textureSize;
                const int numberOfTextures = 4;

                // XNA 4.0 Updates
                //var volumeTexture = new Texture3D(Game.GraphicsDevice, textureSize, textureSize, numberOfTextures, 1, TextureUsage.None, SurfaceFormat.Color);
                var volumeTexture = new Texture3D(Game.GraphicsDevice, textureSize, textureSize, numberOfTextures, true, SurfaceFormat.Color);
                
                var tmpData = new Color[elementCount * numberOfTextures];
                TerrainTextures[4].GetData(0, null, tmpData, 0, elementCount);
                TerrainTextures[5].GetData(0, null, tmpData, elementCount * 1, elementCount);
                TerrainTextures[6].GetData(0, null, tmpData, elementCount * 2, elementCount);
                TerrainTextures[7].GetData(0, null, tmpData, elementCount * 3, elementCount);
                volumeTexture.SetData(tmpData);
                TerrainTextureVolumes[1] = volumeTexture;
            }           

            // Update Volume Textures           
            if (_multiTerrainEffect != null)
            {
                _multiTerrainEffect.Parameters["TextureVolume1"].SetValue(TerrainTextureVolumes[0]);
                _multiTerrainEffect.Parameters["TextureVolume2"].SetValue(TerrainTextureVolumes[1]);
            }

            // XNA 4.0 Updates - Obsolete CommiteChanges()
            // 11/18/09
            //_multiTerrainEffect.CommitChanges();
        }

        /// <summary>
        /// Sets the <see cref="_updateBumpmapTextures"/> flag to TRUE, which
        /// causes an update of the volume textures on the next 'Update' cycle.
        /// </summary>
        public static void UpdateEffectBumpMapTextures()
        {
            _updateBumpmapTextures = true;
        }

        // 3/31/2011
        /// <summary>
        /// Sets the <see cref="TWEngine.Terrain"/> collection of <see cref="TerrainTextureNormals"/> into the <see cref="Effect"/>.
        /// </summary>
        private static void DoBumpMapTexturesUpdateToEffect()
        {
            if (_multiTerrainEffect == null) return;

            _multiTerrainEffect.Parameters["TextureBump1"].SetValue(TerrainTextureNormals[0]);
            _multiTerrainEffect.Parameters["TextureBump2"].SetValue(TerrainTextureNormals[1]);
            _multiTerrainEffect.Parameters["TextureBump3"].SetValue(TerrainTextureNormals[2]);
            _multiTerrainEffect.Parameters["TextureBump4"].SetValue(TerrainTextureNormals[3]);

            // XNA 4.0 Updates - Obsolete CommiteChanges()
            // 11/18/09
            //_multiTerrainEffect.CommitChanges();
        }

        // 8/7/2008
        /// <summary>
        /// Adds a <see cref="TexturesGroupData"/> record to one of the two dictionaries.
        /// Primarily called from the PaintTool Form Class when adding a new texture.
        /// </summary>
        /// <param name="index">Value 1-4</param>
        /// <param name="imageKey">ImageKey name</param>
        /// <param name="selectedImageKey">SelectedImageKey name</param>
        /// <param name="textureImagePath">Texture image path</param>
        /// <param name="groupNumber">Dictionary texture group to use</param>
        public void TextureGroupData_AddRecord(int index, string imageKey,
                                    string selectedImageKey, string textureImagePath, int groupNumber)
        {
            var data = new TexturesGroupData { imageKey = imageKey, selectedImageKey = selectedImageKey, textureImagePath = textureImagePath };

            switch (groupNumber)
            {
                case 1:
                    // Check if key already there
                    if (TextureGroupData1.ContainsKey(index))
                    {
                        TextureGroupData1.Remove(index);
                        TextureGroupData1.Add(index, data);
                    }
                    else
                        TextureGroupData1.Add(index, data);
                    break;
                case 2:
                    // Check if key already there
                    if (TextureGroupData2.ContainsKey(index + 4))
                    {
                        TextureGroupData2.Remove(index + 4);
                        TextureGroupData2.Add(index + 4, data);
                    }
                    else
                        TextureGroupData2.Add(index + 4, data);
                    break;
                default:
                    break;
            }

        }   

       

        // 5/19/2008
        
        /// <summary>
        /// Populates the goalNodesTransformation Array, which is used for the pathFinding.
        /// </summary>
        /// <remarks>
        /// The Following Array uses the grid for unit placement as follows;
        ///
        ///     26  27  28  29  30  31  32
        ///     33  10  11  12  13  14  34
        ///     35  15  04  05  06  16  36
        ///     37  17  02  01  03  18  38
        ///     39  19  07  08  09  20  40
        ///     41  21  22  23  24  25  42
        ///     43  44  45  46  47  48  49
        /// 
        /// </remarks>
        private static void PopulateGoalNodesTransformationArray()
        {
            Vector2[] tmpData = { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(-1, -1), new Vector2(0, -1), new Vector2(1, -1), new Vector2(-1,1),
                                new Vector2(0, 1), new Vector2(1, 1), new Vector2(-2, -2), new Vector2(-1, -2), new Vector2(0, -2), new Vector2(1, -2), new Vector2(2,-2),
                                new Vector2(-2, -1), new Vector2(2, -1), new Vector2(-2, 0), new Vector2(2, 0), new Vector2(-2, 1), new Vector2(2, 1), new Vector2(-2,2),
                                new Vector2(-1, 2), new Vector2(0, 2), new Vector2(1, 2), new Vector2(2, 2), new Vector2(-3, -3), new Vector2(-2, -3), new Vector2(-1,-3),
                                new Vector2(0, -3), new Vector2(1, -3), new Vector2(2, -3), new Vector2(3, -3), new Vector2(-3, -2), new Vector2(3, -2), new Vector2(-3,-1),
                                new Vector2(3, -1), new Vector2(-3, 0), new Vector2(3, 0), new Vector2(-3, 1), new Vector2(3, 1), new Vector2(-3, 2), new Vector2(3,2),
                                new Vector2(-3, 3), new Vector2(-2, 3), new Vector2(-1, 3), new Vector2(0, 3), new Vector2(1, 3), new Vector2(2, 3), new Vector2(3,3)};

            GoalNodeTransformations.AddRange(tmpData);
            
        }


       

        // DEBUG Purposes
        // 5/12/2008 - 
        ///<summary>
        /// Populates the _pathNodes collection used to show the PathNodes in the A* Class
        /// onto the terrain.   
        ///</summary>
        public static void PopulatePathNodesArray()
        {
            // 2/17/2010 - Redirect to the ParallelFor version.
            _pathNodes.Clear();
            PopulatePathNodesArray_Threaded();
        }

        // 4/9/2009; 2/17/2010: Updated to use new ParallelFor construct.
        /// <summary>
        /// Starts the Threaded version of populating the PathNodes collection. 
        /// </summary>
        public static void PopulatePathNodesArray_Threaded()
        {
            // 2/17/2010 - Create and call ParallelFor
            if (_pathNodesParallelFor == null)
                _pathNodesParallelFor = new PopulatePathNodesParallelFor();

            _pathNodesParallelFor.ParallelFor(out _pathNodes);
           
        }

        // 10/7/2009
        /// <summary>
        /// Returns the internal VolumeTextures, as a <see cref="List{TexturesAtlasData}"/> structs.
        /// </summary>
        /// <returns><see cref="List{TexturesAtlasData}"/> collection</returns>
        public static List<TexturesAtlasData> GetVolumeTextureDataAsList()
        {
            var count = TerrainTextureVolumeNames.Count; // 11/20/09
            var tmpGroupData3 = new List<TexturesAtlasData>(count);
            for (var i = 0; i < count; i++)
            {
                var atlasItem = new TexturesAtlasData { textureAtlasName = TerrainTextureVolumeNames[i] };

                tmpGroupData3.Add(atlasItem);
            }
            return tmpGroupData3;
        }

        // 10/7/2009
        /// <summary>
        /// Loads the maps three <see cref="TexturesGroupData"/> structs into memory.
        /// </summary>
        /// <param name="terrainShape"><see cref="ITerrainShape"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType"></param>
        /// <param name="tmpGroupData1">List of <see cref="TexturesGroupData"/> struct (Layer-1)</param>
        /// <param name="tmpGroupData2">List of <see cref="TexturesGroupData"/> struct (Layer-2)</param>
        /// <param name="tmpGroupData3">List of <see cref="TexturesAtlasData"/> struct (Volume Textures)</param>
        public static void LoadMapTextureData(ITerrainShape terrainShape, string mapName, string mapType, 
            List<TexturesGroupData> tmpGroupData1, List<TexturesGroupData> tmpGroupData2, List<TexturesAtlasData> tmpGroupData3)
        {
            var count = tmpGroupData1.Count; // 11/2/2009
            var terrainTextures = TerrainTextures; // 5/19/2010 - Cache
            var terrainTextureNormals = TerrainTextureNormals; // 5/19/2010 - Cache
            var contentTextures = TemporalWars3DEngine.ContentGroundTextures; // 5/19/2010 - Cache
            var texturesGroupDatas = terrainShape.TextureGroupData1; // 5/19/2010 - Cache
            for (var i = 0; i < count; i++)
            {
                // 8/12/2009 - Cache
                var groupData = tmpGroupData1[i];
                texturesGroupDatas[i] = groupData;
 
                // 2nd - load textures back into memory
                terrainTextures[i] =
                    contentTextures.Load<Texture2D>(groupData.textureImagePath);

                // 7/31/2008
                // 3rd - load Bumpmap textures back into memory
                terrainTextureNormals[i] =
                    contentTextures.Load<Texture2D>(String.Format("{0}Normal", groupData.textureImagePath));
            }

            // Add texturesGroupData2 back into Dictionary

            var count1 = tmpGroupData2.Count; // 11/2/2009
            var textureGroupData2 = terrainShape.TextureGroupData2; // 5/19/2010 - Cache
            for (var i = 0; i < count1; i++)
            {
                // 8/12/2009
                var groupData = tmpGroupData2[i];

                // 1st - Add Into Dictionary
                textureGroupData2[i + 4] = groupData;

                // 2nd - load textures back into memory
                terrainTextures[i + 4] =
                    contentTextures.Load<Texture2D>(groupData.textureImagePath);

            }

            // 3/31/2011 - Create VolumeTextures dynamically
            UpdateEffectDiffuseTextures();
            
            /*try
            {
                // 1/21/2009 - Load Volume Texture back into memory.
                var count2 = tmpGroupData3.Count; // 11/2/2009
                var terrainTextureVolumeNames = TerrainTextureVolumeNames; // 5/19/2010 - Cache
                var terrainTexturesQuality = TemporalWars3DEngine.TerrainTexturesQuality; // 5/19/2010 - Cache
                var contentManager = TemporalWars3DEngine.ContentMaps; // 5/19/2010 - Cache
                for (var i = 0; i < count2; i++)
                {
                    // 8/12/2009
                    var texturesAtlasData = tmpGroupData3[i];

                    // Load Texture Atlas back into memory.
                    terrainTextureVolumeNames[i] = texturesAtlasData.textureAtlasName; // 11/20/09 - Only original AtlasName; not with TextureQual.

                    // 11/3/2009 - Update to include Texture version name.
                    var atlasName = texturesAtlasData.textureAtlasName + terrainTexturesQuality;

                    TerrainTextureVolumes[i] = contentManager.Load<Texture3D>(String.Format(@"{0}\{1}\{2}", mapType, mapName, atlasName));
                }
            }
            catch (Exception)
            {
                // 11/19/2009 - Not a problem if load fails for VolumeTextures, since they will be recreated in the 'UpdateEffectDiffuseTextures' method
                //              call.
                Debug.WriteLine("Unable to load the Volume Textures");
                  
            }*/


        }

       
        // 10/7/2009
        /// <summary>
        /// Populates the <see cref="SaveTerrainData"/> struct given with the data from the 
        /// internal PerlinNoise struct; this should be called from the TerrainStorageRoutines class.
        /// </summary>
        /// <param name="data"><see cref="SaveTerrainData"/> struct</param>
        public static void GetPerlinNoiseData(ref SaveTerrainData data)
        {
            // Layer 1
            data.perlinNoiseDataTexture1To2Mix_Layer1.noiseSize =
                PerlinNoiseDataTexture1To2MixLayer1.noiseSize;
            data.perlinNoiseDataTexture1To2Mix_Layer1.octaves =
                PerlinNoiseDataTexture1To2MixLayer1.octaves;
            data.perlinNoiseDataTexture1To2Mix_Layer1.persistence =
                PerlinNoiseDataTexture1To2MixLayer1.persistence;
            data.perlinNoiseDataTexture1To2Mix_Layer1.seed = PerlinNoiseDataTexture1To2MixLayer1.seed;
            // Layer 2
            data.perlinNoiseDataTexture1To2Mix_Layer2.noiseSize =
                PerlinNoiseDataTexture1To2MixLayer2.noiseSize;
            data.perlinNoiseDataTexture1To2Mix_Layer2.octaves =
                PerlinNoiseDataTexture1To2MixLayer2.octaves;
            data.perlinNoiseDataTexture1To2Mix_Layer2.persistence =
                PerlinNoiseDataTexture1To2MixLayer2.persistence;
            data.perlinNoiseDataTexture1To2Mix_Layer2.seed = PerlinNoiseDataTexture1To2MixLayer2.seed;

        }
#if !XBOX360
        // 10/7/2009
        /// <summary>
        /// Saves the <see cref="MapMarkerPositions"/> data.
        /// </summary>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP</param>
        public static void SaveMapMarkersPositionData(Storage storageTool, string mapName, string mapType)
        {
            // 4/6/2010: Updated to use 'ContentMapsLoc' global var.
            int errorCode;
            if (storageTool.StartSaveOperation(TerrainScreen.TerrainShapeInterface.MapMarkerPositions,
                                               "tdMapMarkerPositions.tmd",
                                               String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc,
                                                             mapType, mapName), out errorCode)) return;

            // 4/7/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'MapMarkers' (tdMapMarkerPositions.tmd) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'MapMarkers' (tdMapMarkerPositions.tmd) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            throw new InvalidOperationException("The Save Struct MapMarkerPositions Operation Failed.");
        }
#endif

        // 10/7/2009
        /// <summary>
        /// Loads the <see cref="MapMarkerPositions"/> data back into memory.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapType">Map type; either SP or MP</param>
        /// <param name="markerPositions">(OUT) <see cref="MapMarkerPositions"/> struct</param>
        public static bool LoadMapMarkerPositionsData(Storage storageTool, string mapName, string mapType, out MapMarkerPositions markerPositions)
        {
            // 4/6/2010: Updated to use 'ContentMapsLoc' global var.
            // 4/8/2009 - Load MapMarkerPositions Struct data
            MapMarkerPositions mapMarkerData;
            if (storageTool.StartLoadOperation(out mapMarkerData, "tdMapMarkerPositions.tmd",
                                               String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc, mapType, mapName), StorageLocation.TitleStorage))
            {
                // set with values loaded back into memory.
                markerPositions = mapMarkerData;
                return true;
            }
            
            // set to defaults, since Out requires so!
            markerPositions = mapMarkerData;
            return false;
        }

        // 10/7/2009
        /// <summary>
        /// Extracts all the <see cref="TWEngine.Terrain"/> <see cref="ScenaryDataProperties"/>, and populates into collection return via Out param, while
        /// returning a collection of <see cref="ItemType"/> Enums too.
        /// </summary>
        /// <param name="tmpItemProperties">(OUT) Collection of <see cref="ScenaryDataProperties"/> struct</param>
        /// <returns>Collection of <see cref="ItemType"/> Enums</returns>
        public static List<ItemType> GetScenaryItemsData(out List<ScenaryDataProperties> tmpItemProperties)
        {
            // cache values
            var scenaryItems = TerrainScreen.TerrainShapeInterface.ScenaryItems;
            var scenaryItemsCount = scenaryItems.Count;

            // Iterate through Array 'ScenaryItems' and extract Location, and Enum Type of SceneItemOwner.
            var tmpItemTypes = new List<ItemType>(scenaryItemsCount);
            tmpItemProperties = new List<ScenaryDataProperties>(scenaryItemsCount);
            var tmpScenaryDataProperties = new ScenaryDataProperties();
            
            for (var i = 0; i < scenaryItemsCount; i++)
            {
                // 5/19/2010 - Cache
                var scenaryItemScene = scenaryItems[i];
                if (scenaryItemScene == null) continue;

                var shape = scenaryItemScene.ShapeItem;
                var itemType = shape.InstancedItemData.ItemType; // 5/19/2010 - Cache
                var isPathBlocked = shape.IsPathBlocked; // 5/19/2010 - Cache
                var pathBlockSize = shape.PathBlockSize; // 5/19/2010 - Cache

                // 4/14/2009 - Iterate through internal ScenaryItemData array
                var scenaryItemDatas = scenaryItemScene.ScenaryItems; // 5/19/2010 - Cache
                var count = scenaryItemDatas.Count;
                for (var j = 0; j < count; j++)
                {
                    // 5/31/2012 - skip if added with a script action
                    var scenaryItemData = scenaryItemDatas[j]; // cache
                    if (scenaryItemData.SpawnByScriptingAction)
                        continue;

                    // Extract Scenary SceneItemOwner Enum Type                    
                    tmpItemTypes.Add(itemType); // 11/20/2009 - Updated to get 'ItemType' from 'InstancedItemData'.

                    // Extract Scenary SceneItemOwner Properties we want to save
                    tmpScenaryDataProperties.position = scenaryItemData.position;
                    tmpScenaryDataProperties.rotation = scenaryItemData.rotation;
                    tmpScenaryDataProperties.scale = scenaryItemData.scale; // 5/31/2012
                    tmpScenaryDataProperties.isPathBlocked = isPathBlocked;
                    tmpScenaryDataProperties.pathBlockSize = pathBlockSize;

                    // 10/6/2009 - Save 'Name'
                    var name = scenaryItemData.name;
                    tmpScenaryDataProperties.name = string.IsNullOrEmpty(name) ? "$E" : name;

                    tmpItemProperties.Add(tmpScenaryDataProperties);
                }
            }
            return tmpItemTypes;
        }

        // 10/7/2009
        /// <summary>
        /// Extracts all the <see cref="TWEngine.Terrain"/> <see cref="SelectablesDataProperties"/>, and populates into collection return via Out param, while
        /// returning a collection of <see cref="ItemType"/> Enums too.
        /// </summary>
        /// <param name="tmpItemProperties">(OUT) Collection of <see cref="SelectablesDataProperties"/> struct</param>
        /// <returns>Collection of <see cref="ItemType"/> Enums</returns>
        public static List<ItemType> GetSelectableItemsData(out List<SelectablesDataProperties> tmpItemProperties)
        {
            var tmpItemTypes = new List<ItemType>();
            tmpItemProperties = new List<SelectablesDataProperties>();
            var tmpSelectablesDataProperties = new SelectablesDataProperties();
            
            // 1/15/2011 - Iterate Player collection
            const int maxAllowablePlayers = TemporalWars3DEngine._maxAllowablePlayers;
            for (var i = 0; i < maxAllowablePlayers; i++)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(i, out player);

                if (player == null) continue;

                // 6/15/2010 - Updated to retrieve the ROC collection.
                // Iterate through all 'Selectable' items
                ReadOnlyCollection<SceneItemWithPick> selectableItems;
                Player.GetSelectableItems(player, out selectableItems);

                var playerSelectableItemsCount = selectableItems.Count; // Cache
                for (var j = 0; j < playerSelectableItemsCount; j++)
                {
                    // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                    SceneItemWithPick selectableItem;
                    if (!Player.GetSelectableItemByIndex(player, j, out selectableItem))
                        break;

                    if (selectableItem == null) continue; // Make sure not null!

                    // 5/31/2012 - skip if added with a script action
                    if (selectableItem.SpawnByScriptingAction) continue;

                    // cache
                    var shapeItem = selectableItem.ShapeItem;

                    // Extract Scenary SceneItemOwner Enum Type
                    tmpItemTypes.Add(shapeItem.ItemType);

                    // Extract Scenary SceneItemOwner Properties we want to save
                    tmpSelectablesDataProperties.playerNumber = selectableItem.PlayerNumber; // 10/20/2009
                    tmpSelectablesDataProperties.position = selectableItem.Position;
                    tmpSelectablesDataProperties.rotation = selectableItem.Rotation;
                    tmpSelectablesDataProperties.isPathBlocked = shapeItem.IsPathBlocked;
                    tmpSelectablesDataProperties.pathBlockSize = shapeItem.PathBlockSize;

                    // Save 'Name', used for scripting.
                    var name = selectableItems[j].Name;
                    tmpSelectablesDataProperties.name = string.IsNullOrEmpty(name) ? "$E" : name;

                    tmpItemProperties.Add(tmpSelectablesDataProperties);

                } // End For Loop  
            } // End For Player collection

            return tmpItemTypes;
        }


        #region ITerrainStorage Interface Wrapper Methods

#if !XBOX360
        /// <summary>
        /// Saves the <see cref="TWEngine.Terrain"/> meta-data, like heights, ground textures, waypoints, quads, etc.
        /// </summary>
        /// <param name="mapName">MapName</param>
        /// <param name="mapType">MapType; either SP or MP.</param>
        public void SaveTerrainData(string mapName, string mapType)
        {
            _terrainStorage.SaveTerrainData(mapName, mapType);
        }      
#endif

        #endregion

        

        #region Dispose

        // 1/5/2010
        ///<summary>
        ///Disposes of unmanaged resources.
        ///</summary>
        void ITerrainShape.Dispose()
        {
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            base.Dispose();
        }

        // 4/5/2009 - Dispose of Resources
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="all">Is this final dispose?</param>
        private void Dispose(bool all)
        {
            if (!all) return;

            // 1/5/2010
            if (TerrainTextures != null)
            {
                // 1/7/2010 - Dispose of Textures.
                foreach (var texture in TerrainTextures)
                {
                    if (texture != null)
                        texture.Dispose();
                }
                TerrainTextures.Clear();
            }
            if (TerrainTextureNormals != null)
            {
                // 1/7/2010 - Dispose of Textures.
                foreach (var textureNormal in TerrainTextureNormals)
                {
                    if (textureNormal != null)
                        textureNormal.Dispose();
                }
                TerrainTextureNormals.Clear();
            }
            if (TerrainTextureVolumes != null)
            {
                // 1/7/2010 - Dispose of Textures.
                foreach (var textureVolume in TerrainTextureVolumes)
                {
                    if (textureVolume != null)
                        textureVolume.Dispose();
                }
                TerrainTextureVolumes.Clear();
            }

            // Stop components from drawing               
            if (_terrainPicking != null)
                _terrainPicking.IsVisible = false;

            // Remove components                
            _gameInstance.Components.Remove(_terrainEditing);
            _gameInstance.Components.Remove(_terrainPicking);
            _gameInstance.Components.Remove(_terrainAreaSelect);
            _gameInstance.Components.Remove(TriggerAreas); // 10/13/2009
            _gameInstance.Components.Remove(Waypoints); // 10/13/2009

            // Remove Service Intefaces          
            _gameInstance.Services.RemoveService(typeof(ITerrainShape));
            _gameInstance.Services.RemoveService((typeof(IFOWTerrainShape))); // 1/2/2010
            _gameInstance.Services.RemoveService((typeof(IMinimapTerrainShape))); // 1/2/2010

            // Call Dispose                       
            TerrainData.Dispose(true);
            if (_terrainPicking != null)
                _terrainPicking.Dispose();
            //return;
            if (_terrainEditing != null)
                _terrainEditing.Dispose();
            if (_terrainAreaSelect != null)
                _terrainAreaSelect.Dispose();
            if (_terrainAlphaMaps != null)
                _terrainAlphaMaps.Dispose();
            if (_multiTerrainEffect != null)
                _multiTerrainEffect.Dispose();
            if (_terrainStorage != null)
                _terrainStorage.Dispose();
            if (_tShapeHelper != null)
                _tShapeHelper.Dispose();
            if (TerrainVisualCircles != null) // 1/8/2010
                TerrainVisualCircles.Dispose();

            // 10/13/2009 - TriggerAreas
            if (TriggerAreas != null)
                TriggerAreas.Dispose();
            if (Waypoints != null)
                Waypoints.Dispose();

            // Clear Arrays
            if (_terrainVertices != null)
                _terrainVertices.Clear();
            if (TerrainBoundingBoxes != null)
                TerrainBoundingBoxes.Clear();
            if (QuadParentsTessellated != null)
                QuadParentsTessellated.Clear();
            if (ScenaryItems != null)
                ScenaryItems.Clear();
            if (TextureGroupData1 != null)
                TextureGroupData1.Clear();
            if (TextureGroupData2 != null)
                TextureGroupData2.Clear();
            if (_pathNodes != null)
                _pathNodes.Clear();
            if (InsideBoundingSpheres != null)
                InsideBoundingSpheres.Clear();
            if (GoalNodeTransformations != null) // 1/8/2010
                GoalNodeTransformations.Clear();
            Array.Clear(_trianglePathNodes, 0, _trianglePathNodes.Length); // 1/8/2010

            // 11/17/09 - clear volume names.
            if (TerrainTextureVolumeNames != null)
                TerrainTextureVolumeNames.Clear();

            // 1/7/2010 - Unload ContentManager
            if (_contentManager != null)
                _contentManager.Unload();

            RootQuadTree.ClearQuadTree();

            // Release References
            Cursor = null;
            ShadowMapInterface = null;
            _terrainVertices = null;
            TerrainBoundingBoxes = null;
            QuadParentsTessellated = null;
            ScenaryItems = null;
            TextureGroupData1 = null;
            TextureGroupData2 = null;
            _pathNodes = null;
            InsideBoundingSpheres = null;
            _terrainStorage = null;
            _terrainPicking = null;
            _terrainEditing = null;
            _terrainAreaSelect = null;
            _terrainAlphaMaps = null;
            TerrainVisualCircles = null;
            _pathNodesParallelFor = null; // 2/17/2010
#if !XBOX360
            GameConsole = null;
#endif
            _multiTerrainEffect = null;
            _tShapeHelper = null;
            _gameInstance = null;
        }

        #endregion
       
    }
}
