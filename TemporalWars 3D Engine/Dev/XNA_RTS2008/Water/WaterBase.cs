#region File Description
//-----------------------------------------------------------------------------
// WaterBase.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.GameCamera;
using TWEngine.GameScreens;
using TWEngine.InstancedModels;
using TWEngine.Interfaces;
using TWEngine.Shadows;
using TWEngine.Viewports;
using TWEngine.Viewports.Structs;
using TWEngine.Water.Enums;

namespace TWEngine.Water
{
    ///<summary>
    /// The <see cref="WaterBase"/> class is the base class for creating any water-type component.
    ///</summary>
    public class WaterBase : IWaterBase
    {
        // 1/8/2010 - ContentManager; 3/23/2010 - renamed to 'WaterContentManager'.
        /// <summary>
        /// Reference to <see cref="WaterBase"/> <see cref="ContentManager"/>.
        /// </summary>
        protected ContentManager WaterContentManager;

        // 1/8/2010 - Game Instance
        /// <summary>
        /// Reference to <see cref="Game"/> instance.
        /// </summary>
        protected static Game GameInstance;

        /// <summary>
        /// Reference to <see cref="ITerrainShape"/> interface.
        /// </summary>
        protected static ITerrainShape TerrainShape;
        private bool _updateTextureMaps = true; // Set True initially, to at least get one update.
        private static float _waterHeight = 5.0f;

        // RefractionMap Attributes
        /// <summary>
        /// Reference to refraction render target.
        /// </summary>
        protected static RenderTarget2D RefractionRenderTarget;
        /// <summary>
        /// Reference to refraction texture map.
        /// </summary>
        protected static Texture2D RefractionMapTexture;

        // RelectionMap Attributes
        /// <summary>
        /// Reference to reflection render target.
        /// </summary>
        protected static RenderTarget2D ReflectionRenderTarget;
        
        /// <summary>
        /// Reference to reflection texture map.
        /// </summary>
        protected static Texture2D ReflectionMapTexture;

        /// <summary>
        /// Reference to reflection view matrix.
        /// </summary>
        protected static Matrix ReflectionViewMatrix;
        private static Vector3 _reflCameraPosition;

        // GameViewPort for debugging Water maps.
        private GameViewPortItem _gameViewPortItem;
        // Enum for which Texture to show in Debug Viewport
        private ViewPortTexture _showTexture = ViewPortTexture.Refraction;

        // WaterBumpMap Attribute
        /// <summary>
        /// Reference to water 'BumpMap' texture.
        /// </summary>
        protected static Texture2D WaterBumpMapTexture;

        /// <summary>
        /// Sun light's direction given as Vector3.
        /// </summary>
        protected static Vector3 SunLightDirection = new Vector3(0, -1.0f, 0.45f);

        #region Properties

        ///<summary>
        /// Get or set the water height.
        ///</summary>
        public virtual float WaterHeight
        {
            get { return _waterHeight; }
            set{ _waterHeight = value; }
        }

        // 12/14/2009
        /// <summary>
        /// During debugging, this sets which <see cref="ViewPortTexture"/>
        /// to display in the <see cref="GameViewPort"/>.
        /// </summary>
        public ViewPortTexture ShowTexture
        {
            get { return _showTexture; }
            set { _showTexture = value; }
        }

        // 12/16/2009
        ///<summary>
        /// Allows turning on/off the drawing of the water component.
        ///</summary>
        public bool Visible { get; set; }

        #endregion

        ///<summary>
        /// <see cref="WaterBase"/> constructor, used to initialize base <see cref="ContentManager"/>, and
        /// set other misc attributes.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="inWaterHeight">Initial water height</param>
        public WaterBase(ref Game game, float inWaterHeight)
        {
            // 1/8/2010 - Create ContentManager
            WaterContentManager = new ContentManager(game.Services);

            // 1/8/2010 - Save game ref
            GameInstance = game;
           
            // Set WaterHeight
            _waterHeight = inWaterHeight;

            // 9/9/2008 - Attach CameraUpdated Event Handler
            Camera.CameraUpdated += CameraUpdated;

            // Set a Reference to the Interface for Terrain Class
            TerrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

            // Init Refraction RenderTarget
            CreateWaterBaseRenderTargets(game.GraphicsDevice);

            // Create gameViewPort for Debug Purposes of showing RenderTarget Texture           
            GameViewPort.AddNewGameViewPortItem(RefractionMapTexture, new Rectangle(0, 0, 256, 256), out _gameViewPortItem);


        }

        // 9/9/2008 - Update Texture-Maps? - Only done when CameraUpdated Event Fired.
        /// <summary>
        /// Captures the <see cref="Camera"/> 'CameraUpdated' event, and then signals
        /// the water to update the texture maps; specifically, the <see cref="ReflectionMapTexture"/> and <see cref="RefractionMapTexture"/> maps.
        /// </summary>
        protected void CameraUpdated(object sender, EventArgs e)
        {
            // Turn on Updating of Texture Maps
            _updateTextureMaps = true;
        }

        /// <summary>
        /// Loads content.
        /// </summary>
        internal virtual void LoadContent()
        {
            // 1/8/2010 - Create the Base RTs
            CreateWaterBaseRenderTargets(GameInstance.GraphicsDevice);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public virtual void Initialize()
        {
            // TODO: Add your initialization code here
           
        }

        // 1/8/2010
        /// <summary>
        /// Creates the <see cref="ReflectionMapTexture"/> and <see cref="RefractionMapTexture"/> base render targets.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void CreateWaterBaseRenderTargets(GraphicsDevice graphicsDevice)
        {
            var pp = graphicsDevice.PresentationParameters;
            if (RefractionRenderTarget == null)
            {
                // XNA 4.0 Updates
                /*RefractionRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                                        1, graphicsDevice.DisplayMode.Format, graphicsDevice.PresentationParameters.MultiSampleType,
                                                        graphicsDevice.PresentationParameters.MultiSampleQuality);*/
                RefractionRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                                            true, graphicsDevice.DisplayMode.Format,
                                                            DepthFormat.Depth24Stencil8);
            }

            // Init Reflection RenderTarget
            if (ReflectionRenderTarget == null)
            {
                // XNA 4.0 Updates
                /*ReflectionRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                                        1, graphicsDevice.DisplayMode.Format, graphicsDevice.PresentationParameters.MultiSampleType,
                                                        graphicsDevice.PresentationParameters.MultiSampleQuality);*/
                ReflectionRenderTarget = new RenderTarget2D(graphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight,
                                                            true, graphicsDevice.DisplayMode.Format,
                                                            DepthFormat.Depth24Stencil8);
            }
        }

        /// <summary>
        /// Updates the reflection view matrix, water maps, and draws the <see cref="GameViewPort"/> for debug
        /// purposes when the <see cref="_showTexture"/> flag is set.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>       
        public virtual void Update(GameTime gameTime)
        {
            // 5/20/2010 - Refactored core code into new STATIC method.
            DoUpdate(this, gameTime);
        }

        // 5/20/2010 -
        /// <summary>
        /// Method helper, which updates the reflection view matrix, water maps, and draws the <see cref="GameViewPort"/> for debug
        /// purposes when the <see cref="_showTexture"/> flag is set.
        /// </summary>
        /// <param name="waterBase">this instance of <see cref="WaterBase"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DoUpdate(WaterBase waterBase, GameTime gameTime)
        {
            // 12/14/2009
            CreateReflectionViewMatrix();

            // 1/21/2009 - Moved the code below from the 'Draw' call, to fix the Water from disappearing when the camera moved!
            waterBase.CreateWaterMaps(gameTime);

            // 7/16/2008 - Which Texture to draw in Debug GameViewPort.
            var viewPortTexture = waterBase._showTexture; // 5/20/2010 - CAche
            switch (viewPortTexture)
            {
                case ViewPortTexture.Refraction:
                    waterBase._gameViewPortItem.Texture = RefractionMapTexture;
                    GameViewPort.UpdateGameViewPortItem(ref waterBase._gameViewPortItem);
                    break;
                case ViewPortTexture.Reflection:
                    waterBase._gameViewPortItem.Texture = ReflectionMapTexture;
                    GameViewPort.UpdateGameViewPortItem(ref waterBase._gameViewPortItem);
                    break;
                case ViewPortTexture.Bump:
                    waterBase._gameViewPortItem.Texture = WaterBumpMapTexture;
                    GameViewPort.UpdateGameViewPortItem(ref waterBase._gameViewPortItem);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Allows the game component to Render itself.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public virtual void Draw(GameTime gameTime)
        {
            // TODO: Add your initialization code here
        }

        /// <summary>
        /// Set the debug <see cref="GameViewPort"/> texture to show via script
        /// </summary>
        /// <param name="textureName">Texture name to use</param>
        public void SetViewportTexture(string textureName)
        {
            switch (textureName)
            {
                case "Refraction":
                    _showTexture = ViewPortTexture.Refraction;
                    break;
                case "Reflection":
                    _showTexture = ViewPortTexture.Reflection;
                    break;                
                case "Bump":
                    _showTexture = ViewPortTexture.Bump;
                    break;
                default:                    
                    break;
            }
        }

        /// <summary>
        /// Creates the reflection view <see cref="Matrix"/>.
        /// </summary>
        private static void CreateReflectionViewMatrix()
        {
            // Camera B Position
            var reflCameraPosition = Camera.CameraPosition; // 5/20/2010 - Cache
            _reflCameraPosition = reflCameraPosition;
            _reflCameraPosition.Y = -reflCameraPosition.Y + _waterHeight * 2;
            // Camera B Target
            var cameraTarget = Camera.CameraTarget; // 5/20/2010 - Cache
            var reflTargetPos = cameraTarget;
            reflTargetPos.Y = -cameraTarget.Y + _waterHeight * 2;

            // 7/1/2009
            //var cameraRight = Vector3.Transform(new Vector3(1, 0, 0), Camera.CameraRotation);
            //var invUpVector = Vector3.Cross(cameraRight, reflTargetPos - reflCameraPosition);
            var invUpVector = Vector3.Down; // 12/14/2009 ???

            //Vector3 inUp = Vector3.Up;            
            Matrix.CreateLookAt(ref _reflCameraPosition, ref reflTargetPos, ref invUpVector, out ReflectionViewMatrix);
        }

        /// <summary>
        /// Creates both the <see cref="ReflectionMapTexture"/> and <see cref="RefractionMapTexture"/> maps for the water table.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private void CreateWaterMaps(GameTime gameTime)
        {
            // Try to get interface again.
            if (TerrainShape == null)
                TerrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));

            // 12/14/2009 - Check if either the _TerrainShape is still null.
            if (TerrainShape == null)
                return;

            // 9/9/2008 - Use to only update the Texture Maps when the CameraUpdated Event
            //            is triggered.
            if (!_updateTextureMaps) return;

            // Draw Water RefractionMap into renderTarget
            DrawRefractionMap(GameInstance.GraphicsDevice, gameTime);
            // Draw Water ReflectionMap into renderTarget
            DrawReflectionMap(GameInstance.GraphicsDevice, gameTime);

            // Set to off, until CameraUpdate Events fire again
            _updateTextureMaps = false;
        }

        /// <summary>
        /// Draws the <see cref="RefractionMapTexture"/> texture to <see cref="RefractionRenderTarget"/>.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DrawRefractionMap(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            var inPlaneNormalDirection = Vector3.Down;
            var inCameraView = Camera.View;

            var inWaterHeight = _waterHeight + 1.5f;

            // TODO: MUST fix using shader; example given in forum posting.
            // XNA 4.0 Updates - ClipPlanes are GONE!  - http://forums.xna.com/forums/t/56786.aspx
            Plane refractionPlane;
            CreatePlane(inWaterHeight, ref inPlaneNormalDirection, ref inCameraView, false, out refractionPlane);
            //graphicsDevice.ClipPlanes[0].Plane = refractionPlane;
            //graphicsDevice.ClipPlanes[0].IsEnabled = true;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, RefractionRenderTarget);
            graphicsDevice.SetRenderTarget(RefractionRenderTarget);

            Terrain.TerrainShape.Draw(ref inCameraView, gameTime, ShadowMap.IsVisibleS); // Draw Terrain           

            //graphicsDevice.ClipPlanes[0].IsEnabled = false;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //RefractionMapTexture = RefractionRenderTarget.GetTexture();
            RefractionMapTexture = RefractionRenderTarget;
            
        }

        /// <summary>
        /// Draws the <see cref="ReflectionMapTexture"/> texture to <see cref="RenderTarget"/>.
        /// </summary>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DrawReflectionMap(GraphicsDevice graphicsDevice, GameTime gameTime)
        {
            var inPlaneNormalDirection = Vector3.Down; 

            var inWaterHeight = _waterHeight - 0.5f;

            // TODO: MUST fix using shader; example given in forum posting.
            // XNA 4.0 Updates - ClipPlanes are GONE!  - http://forums.xna.com/forums/t/56786.aspx
            Plane reflectionPlane;
            CreatePlane(inWaterHeight, ref inPlaneNormalDirection, ref ReflectionViewMatrix, true, out reflectionPlane);
            //graphicsDevice.ClipPlanes[0].Plane = reflectionPlane;
            //graphicsDevice.ClipPlanes[0].IsEnabled = true;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, ReflectionRenderTarget);
            graphicsDevice.SetRenderTarget(ReflectionRenderTarget);

            graphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            // Draw SkyDome
            TerrainScreen.DrawSkyBox(ref ReflectionViewMatrix, ref _reflCameraPosition); 

            // Draw Terrain
            Terrain.TerrainShape.Draw(ref ReflectionViewMatrix, gameTime, ShadowMap.IsVisibleS); 

            // Draw ScenaryItems 'InstanceModels'            
            InstancedItem.DrawInstanceModelsForWaterRm(gameTime, ref ReflectionViewMatrix);

            //graphicsDevice.ClipPlanes[0].IsEnabled = false;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //ReflectionMapTexture = ReflectionRenderTarget.GetTexture();
            ReflectionMapTexture = ReflectionRenderTarget;
        }

        /// <summary>
        /// Creates normalized <see cref="Plane"/>.
        /// </summary>
        /// <param name="height">Height of <see cref="Plane"/></param>
        /// <param name="planeNormalDirection">Normal direction as <see cref="Vector3"/></param>
        /// <param name="currentViewMatrix">View <see cref="Matrix"/> to use</param>
        /// <param name="clipSide">Clip view at <see cref="Plane"/></param>
        /// <param name="newPlane">(OUT) new created <see cref="Plane"/></param>
        private static void CreatePlane(float height, ref Vector3 planeNormalDirection,
                                        ref Matrix currentViewMatrix, bool clipSide, out Plane newPlane)
        {
            // 12/14/2009 - Normalize plane into variable, so original is intact.
            var normalizePlane = planeNormalDirection;
            if (!normalizePlane.Equals(Vector3.Zero)) normalizePlane.Normalize();

            //planeNormalDirection.Normalize();

            var planeCoeffs = new Vector4
                                  {
                                      X = normalizePlane.X,
                                      Y = normalizePlane.Y,
                                      Z = normalizePlane.Z,
                                      W = height
                                  };

            if (clipSide)
                planeCoeffs *= -1;

            // 12/14/2009 - Updated to use Ref version of Mutiply, since faster on XBOX!
            var projection = Camera.Projection;
            Matrix worldViewProjection; // = currentViewMatrix * projection;
            Matrix.Multiply(ref currentViewMatrix, ref projection, out worldViewProjection);

            Matrix inverseWorldViewProjection;
            Matrix.Invert(ref worldViewProjection, out inverseWorldViewProjection);

            Matrix.Transpose(ref inverseWorldViewProjection, out inverseWorldViewProjection);
            Vector4.Transform(ref planeCoeffs, ref inverseWorldViewProjection, out planeCoeffs);

            newPlane = new Plane(planeCoeffs);

        }
        

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public virtual void Dispose(bool disposing)
        {

            if (WaterBumpMapTexture != null)
                WaterBumpMapTexture.Dispose();
            if (ReflectionMapTexture != null)
                ReflectionMapTexture.Dispose();
            if (RefractionMapTexture != null)
                RefractionMapTexture.Dispose();
            if (ReflectionRenderTarget != null)
                ReflectionRenderTarget.Dispose();
            if (RefractionRenderTarget != null)
                RefractionRenderTarget.Dispose();
            
            // 1/8/2010 - Unload Graphics resources
            if (WaterContentManager != null)
                WaterContentManager.Unload();
            

            // Null Interface and class References
            WaterBumpMapTexture = null;
            ReflectionMapTexture = null;
            RefractionMapTexture = null;
            ReflectionRenderTarget = null;
            RefractionRenderTarget = null;
            TerrainShape = null;

            if (!disposing) return;

            // 1/8/2010 - Dispose of ContentManager
            if (WaterContentManager != null)
                WaterContentManager.Dispose();
        }

      
    }
}