#region File Description
//-----------------------------------------------------------------------------
// ScriptingActions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#if !XBOX360
using System.Data;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using AStarInterfaces.AStarAlgorithm;
using AStarInterfaces.AStarAlgorithm.Enums;
using AStarInterfaces.AStarAlgorithm.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Particles3DComponentLibrary;
using TWEngine.Audio;
using TWEngine.Audio.Enums;
using TWEngine.Common;
using TWEngine.Explosions;
using TWEngine.GameCamera;
using TWEngine.GameCamera.Enums;
using TWEngine.GameCamera.Structs;
using TWEngine.GameLevels.ChangeRequests;
using TWEngine.GameLevels.ChangeRequests.Enums;
using TWEngine.GameLevels.ChangeRequests.Structs;
using TWEngine.GameLevels.Enums;
using TWEngine.GameLevels.Structs;
using TWEngine.GameScreens;
using TWEngine.GameScreens.Generic;
using TWEngine.IFDTiles;
using TWEngine.IFDTiles.Enums;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.ItemTypeAttributes;
using TWEngine.MemoryPool;
using TWEngine.ParallelTasks.Structs;
using TWEngine.Particles;
using TWEngine.Particles.Enums;
using TWEngine.Particles.ParticleSystems;
using TWEngine.Particles.Structs;
using TWEngine.Players;
using TWEngine.PostProcessEffects.BloomEffect.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.ScreenManagerC;
using TWEngine.Shadows;
using TWEngine.Shadows.Enums;
using TWEngine.SkyDomes;
using TWEngine.SkyDomes.Enums;
using TWEngine.Terrain;
using TWEngine.ParallelTasks;
using TWEngine.Utilities;

namespace TWEngine.GameLevels
{
    // 10/11/2009
    ///<summary>
    /// The <see cref="ScriptingActions"/> class, provides actionable real-time
    /// in game effects, like camera movement, <see cref="SceneItem"/> spawning, <see cref="ParticleSystem"/>
    ///  effects, and more, while abstracting the user from the lower engine level details in achieving
    /// these goals.  Ideally, this class should be used within the <see cref="GameLevel"/>s and <see cref="GameLevelPart"/>s, 
    /// along with its companion <see cref="ScriptingConditions"/> class, to create fully scripted game levels.
    ///</summary>
    public static class ScriptingActions
    {
        // Saves current A* blocking data when new camera bound set.
        private static List<PathNodeForSaving> _currentBlockingCollection;
        // 2/24/2011 - Tracks if blocking data set.
        private static volatile bool _blockingDataSet;

        // 2/28/2011 - Thread Manager
        private static readonly CameraBoundThreadManager<Rectangle> MiscThreadManager;
        private static readonly ManualResetEvent RestoreBlockingDataEvent = new ManualResetEvent(false);

        // 5/25/2012 - Dictionary to track IFD messages by name
        private static readonly Dictionary<string, IFDTile> IFDTileByNames = new Dictionary<string, IFDTile>(); 
        // 5/25/2012 - Dictionary to track IFD key to name relationship
        private static readonly Dictionary<int, string> IFDTileKeytoNames = new Dictionary<int, string>(); 

        // 6/11/2012 - Dictionary to track Sounds by name
        private static readonly Dictionary<string, SoundNameStruct> SoundNames = new Dictionary<string, SoundNameStruct>(); 

#if WithLicense
#if !XBOX
        // 5/10/2012 - License
        private static readonly LicenseHelper LicenseInstance;
#endif
#endif

        #region Constructor

        /// <summary>
        /// constructor
        /// </summary>
        static ScriptingActions()
        {
            var gameInstance = TemporalWars3DEngine.GameInstance;
            MiscThreadManager = new CameraBoundThreadManager<Rectangle>(gameInstance);

#if WithLicense
#if !XBOX
            // 5/10/2012 Check for Valid License.
            LicenseInstance = new LicenseHelper();
#endif
#endif
        }

        #endregion

        #region MemoryPool Methods

        // 4/24/2011
        ///<summary>
        /// Sets the given <see cref="IFDGroupControlType"/> maximum population count.
        ///</summary>
        ///<param name="ifdGroupControlType"><see cref="IFDGroupControlType"/> to set</param>
        ///<param name="maxCount">Maximum population value to set</param>
        public static void SetPlayableItemMaxPopulation(IFDGroupControlType ifdGroupControlType, int maxCount)
        {
            // Check if value is within range
            if (maxCount < 1 || maxCount > 100)
                throw new ArgumentOutOfRangeException("maxCount", @"Range MUSt be with 1-100.");

            switch (ifdGroupControlType)
            {
                case IFDGroupControlType.ControlGroup1:
                    break;
                case IFDGroupControlType.Buildings:
                    PoolManager.BuildingItemsMaxPopulation = maxCount;
                    break;
                case IFDGroupControlType.Shields:
                    PoolManager.DefenseItemsMaxPopulation = maxCount;
                    break;
                case IFDGroupControlType.People:
                    break;
                case IFDGroupControlType.Vehicles:
                    PoolManager.TankItemsMaxPopulation = maxCount;
                    break;
                case IFDGroupControlType.Airplanes:
                    PoolManager.AircraftItemsMaxPopulation = maxCount;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("ifdGroupControlType");
            }
        }

        #endregion

        #region Misc

        // 6/16/2012
        /// <summary>
        /// Overrides the loading screen's default background.
        /// </summary>
        /// <param name="textureToLoad"><see cref="Texture2D"/> to set as loading screen background.</param>
        public static void SetTheLoadScreenBackground(Texture2D textureToLoad)
        {
            if (textureToLoad == null)
                throw new ArgumentNullException("textureToLoad");

            LoadingScreen.BackgroundTexture = textureToLoad;
        }

        // 6/16/2012
        /// <summary>
        /// Overrides the MainMenu screen's default background.
        /// </summary>
        /// <param name="textureToLoad"><see cref="Texture2D"/> to set as loading screen background.</param>
        public static void SetTheMainMenuScreenBackground(Texture2D textureToLoad)
        {
            if (textureToLoad == null)
                throw new ArgumentNullException("textureToLoad");

            BackgroundScreen.DrawBackgroundAnimated = false;
            BackgroundScreen.BackgroundTexture = textureToLoad;
        }

        // 6/3/2012
        /// <summary>
        /// Set to tell the game engine to preload all playable R.T.S. items into 
        /// memory at the start of the game engine.
        /// </summary>
        /// <param name="reloadArtwork">Load R.T.S. playable items?</param>
        public static void SetToPreloadRtsItems(bool reloadArtwork)
        {
            InstancedItemLoader.DoPreloadPlayableRtsItems = reloadArtwork;
        }

        // 4/23/2011
        ///<summary>
        /// Returns a reference to the <see cref="InputState"/> component.
        ///</summary>
        ///<returns>Instance of <see cref="InputState"/>.</returns>
        public static InputState GetInputStateComponent()
        {
            // Get ScreenManager component
            var screenManager = (ScreenManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ScreenManager));

            // Return InputState component
            return screenManager == null ? null : screenManager.Input;

        }

        // 1/19/2011
        /// <summary>
        /// Calls the framework <see cref="Microsoft.Xna.Framework.GamerServices.Guide.ShowMarketplace"/>.
        /// </summary>
        public static void DisplayGuideMarketplaceMenu()
        {
            TemporalWars3DEngine.ShowMarketplace();
        }

        // 1/19/2011
        /// <summary>
        /// Call to set the game in trial over mode.  This will lock
        /// the game camera, pause the game play and display the 
        /// <see cref="Guide.ShowMarketplace"/>.
        /// </summary>
        public static void SetGameTrialIsOver()
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SetGameTrialIsOver' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            TemporalWars3DEngine.IsGameTrialOver = true;

            // Retrieve ScreenManager service
            var screenManager = (ScreenManager) 
                TemporalWars3DEngine.GameInstance.Services.GetService(typeof (ScreenManager));

            if (screenManager != null)
            {
                // If they pressed pause, bring up the pause menu screen.
                screenManager.AddScreen(new PauseMenuScreen(null), false);

                // Display Marketplace screen to allow user to purchase game.
                TemporalWars3DEngine.ShowMarketplace();

                return;
            }

            throw new InvalidOperationException("ScreenManager is null.");

        }

        // 6/2/2012
        /// <summary>
        /// Draw the picked terrain triangles?
        /// </summary>
        /// <param name="drawTriangles">Draw the triangles?</param>
        public static void SetDebugDrawPickedTriangles(bool drawTriangles)
        {
            TerrainPickingRoutines.DrawDebugPickedTriangles = drawTriangles;
        }

        // 1/14/2011
        /// <summary>
        /// Helper method which validates the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        private static void ValidateSceneItemName(string sceneItemName)
        {
            // 1/14/2010 - check if null
            if (string.IsNullOrEmpty(sceneItemName))
                throw new ArgumentNullException("sceneItemName", @"Name given CANNOT be empty or null.");
#if !XBOX360
            // check if 'Name' exist in the Player's Dictionary
            if (Player.SceneItemsByName.ContainsKey(sceneItemName))
                throw new DuplicateNameException("Name given must be unique.");
#endif
        }
       

#endregion

        #region Terrain Rendering Methods

        // 5/28/2012
        /// <summary>
        /// Sets terrain's specular color and power for texture layer <see cref="TextureLayerEnum"/>.
        /// </summary>
        /// <param name="layerEnum">Apply to terrain's texture layer?</param>
        /// <param name="specularColor"><see cref="Color"/> as specular color to apply.</param>
        /// <param name="specularPower">Specular power to apply. (0 - 255)</param>
        public static void SetTerrainSpecularColor(TextureLayerEnum layerEnum, Color specularColor, float specularPower)
        {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif
            if (specularPower < 0 || specularPower > 255)
            {
                throw new ArgumentOutOfRangeException("specularPower", "Specular power MUST be in range of 0 - 255.");
            }

            // Convert and normalize color to a range of 0.00 - 1.00.
            Vector3 vectorColor = specularColor.ToVector3();
            float specularPowerN = specularPower / 255.0f;

            // Get TerrainShape Interface
            var terrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));
            if (terrainShape != null)
            {
                switch (layerEnum)
                {
                    case TextureLayerEnum.Layer1:
                        terrainShape.SpecularColorLayer1 = vectorColor;
                        terrainShape.SpecularPowerLayer1 = specularPowerN;
                        break;
                    case TextureLayerEnum.Layer2:
                        terrainShape.SpecularColorLayer2 = vectorColor;
                        terrainShape.SpecularPowerLayer2 = specularPowerN;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("layerEnum");
                }
            }
        }

        // 5/28/2012
        /// <summary>
        /// Sets terrain's ambient color and power for texture layer <see cref="TextureLayerEnum"/>.
        /// </summary>
        /// <param name="layerEnum">Apply to terrain's texture layer?</param>
        /// <param name="ambientColor"><see cref="Color"/> as ambient color to apply.</param>
        /// <param name="ambientPower">Ambient power to apply. (0 - 255)</param>
        public static void SetTerrainAmbientColor(TextureLayerEnum layerEnum, Color ambientColor, float ambientPower)
        {
#if WithLicense
                // 5/10/2012 - Return if trial.
                if (LicenseInstance.IsTrial)
                {
                    MessageBox.Show("Valid ONLY in FULL PAID Version!", "License Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
#endif

            if (ambientPower < 0 || ambientPower > 255)
            {
                throw new ArgumentOutOfRangeException("ambientPower", "Ambient power MUST be in range of 0 - 255.");
            }

            // Convert and normalize color to a range of 0.00 - 1.00.
            Vector3 vectorColor = ambientColor.ToVector3();
            float ambientPowerN = ambientPower/255.0f;

            // Get TerrainShape Interface
            var terrainShape = (ITerrainShape)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ITerrainShape));
            if (terrainShape != null)
            {
                switch (layerEnum)
                {
                    case TextureLayerEnum.Layer1:
                        terrainShape.AmbientColorLayer1 = vectorColor;
                        terrainShape.AmbientPowerLayer1 = ambientPowerN;
                        break;
                    case TextureLayerEnum.Layer2:
                        terrainShape.AmbientColorLayer2 = vectorColor;
                        terrainShape.AmbientPowerLayer2 = ambientPowerN;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("layerEnum");
                }
            }
        }

        // 5/28/2012
        /// <summary>
        /// Sets the use of the debug shape renderer; useful to show debug
        /// information like 'Collision-Spheres'.
        /// </summary>
        /// <param name="isVisible">Show Debug Information?</param>
        public static void UseDebugShapeRenderer(bool isVisible)
        {
#if !XBOX360
            DebugShapeRenderer.IsVisible = isVisible;
#endif
        }

        // 5/28/2012
        /// <summary>
        /// Draw 'Collision-Spheres' debug shapes for <see cref="ScenaryItemScene"/>.
        /// </summary>
        /// <param name="isVisible">Draw spheres?</param>
        public static void DrawCollisionSpheresForScenaryItems(bool isVisible)
        {
#if !XBOX360
            DebugShapeRenderer.DrawCollisionSpheresForScenaryItems = isVisible;
#endif
        }

        // 5/28/2012
        /// <summary>
        /// Draw 'Collision-Spheres' debug shapes for <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="isVisible">Draw spheres?</param>
        public static void DrawCollisionSpheresForPlayableItems(bool isVisible)
        {
#if !XBOX360
            DebugShapeRenderer.DrawCollisionSpheresForPlayableItems = isVisible;
#endif
        }

        // 3/1/2011
        /// <summary>
        /// Turn on/off the raining particles effect.
        /// </summary>
        /// <param name="isRaining">Enable rain effect?</param>
        public static void IsRaining(bool isRaining)
        {
            TerrainScreen.IsRaining = isRaining;
        }

        // 3/1/2011
        /// <summary>
        /// Turn on/off the snowing particles effect.
        /// </summary>
        /// <param name="isSnowing">Enable snow effect?</param>
        public static void IsSnowing(bool isSnowing)
        {
            TerrainScreen.IsSnowing = isSnowing;
        }

        // 1/23/2010
        /// <summary>
        /// Update the visibility of the <see cref="IFogOfWar"/> component.
        /// </summary>
        /// <param name="visible">True/False to show <see cref="IFogOfWar"/>.</param>
        public static void DisplayFogOfWarComponent(bool visible)
        {
            // retrieve Visibility interface.
            var fow = (IFogOfWar)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IFogOfWar));

            // if FOW Interface not present, then just return.
            if (fow == null) return;

            // otherwise, set visiblitity setting.
            fow.IsVisible = visible;
        }

        // 2/23/2011; 6/1/2012 - Add 'SkyDomeTextureEnum' param.
        /// <summary>
        /// Update the visibility of the SkyDome component.
        /// </summary>
        /// <param name="visible">True/False to show SkyDome.</param>
        /// <param name="skydomeTextureEnum"><see cref="SkyDomeTextureEnum"/> texture to use.</param>
        public static void DisplaySkyDomeComponent(bool visible, SkyDomeTextureEnum skydomeTextureEnum)
        {
            // retrieve Visibility interface.
            var skyDome = (SkyDome)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(SkyDome));

            // if SkyDome not present, then just return.
            if (skyDome == null) return;

            // otherwise, set visiblitity setting.
            skyDome.Visible = visible;

            // set the Skydome texture to use
            skyDome.SkyboxTextureToUse = skydomeTextureEnum;
        }

        // 5/25/2012
        /// <summary>
        /// Sets to turn On/Off the drawing of the 'PerlinNoise' cloud effect on the terrain.
        /// </summary>
        /// <param name="drawClouds">Draw perlin clouds?</param>
        public static void DisplayPerlinNoiseClouds(bool drawClouds)
        {
            TerrainPerlinClouds.EnableClouds = drawClouds;
        }

        // 3/2/2011
        ///<summary>
        /// Displays the ground Directional Icon.
        ///</summary>
        ///<param name="name">Name of icon to reference.</param>
        ///<param name="visible">Is Visible?</param>
        ///<param name="position">Position of Icon</param>
        ///<param name="size">Size of Icon</param>
        ///<param name="rotation">Angle of Icon; like 90 degrees.</param>
        ///<param name="color">Color of Icon.</param>
        public static void DisplayDirectionalIcon(string name, bool visible, Vector3 position, int size, float rotation, Color color)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (position.Z > TerrainData.MapHeight)
                throw new ArgumentOutOfRangeException(string.Format("Value given for Position Z-axis cannot be larger than the MapHeight of {0}.", TerrainData.MapHeight));

            if (position.X > TerrainData.MapWidth)
                throw new ArgumentOutOfRangeException(string.Format("Value given for Position X-axis cannot be larger than the MapWidth of {0}.", TerrainData.MapWidth));

            //var terrainDirectionalIcon = TerrainShape.TerrainDirectionalIcon; // Cache

            // 6/5/2012
            var terrainDirectionalIcon = TerrainDirectionalIconManager.CreateDirectionalIcon(name); // Cache

            terrainDirectionalIcon.Visible = visible;
            terrainDirectionalIcon.DirectionalIconPosition = position;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
            terrainDirectionalIcon.DirectionalIconColor = color;
        }

        // 6/6/2012
        ///<summary>
        /// Displays the ground Directional Icon and sets a rotation animation.
        ///</summary>
        ///<param name="name">Name of icon to reference.</param>
        ///<param name="visible">Is Visible?</param>
        ///<param name="position">Position of Icon</param>
        ///<param name="size">Size of Icon</param>
        ///<param name="rotation">Angle of Icon; like 90 degrees.</param>
        ///<param name="color">Color of Icon.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        public static void DisplayDirectionalIcon(string name, bool visible, Vector3 position, int size, float rotation, Color color, float deltaMagnitude, int rotationTimeMax)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (position.Z > TerrainData.MapHeight)
                throw new ArgumentOutOfRangeException(string.Format("Value given for Position Z-axis cannot be larger than the MapHeight of {0}.", TerrainData.MapHeight));

            if (position.X > TerrainData.MapWidth)
                throw new ArgumentOutOfRangeException(string.Format("Value given for Position X-axis cannot be larger than the MapWidth of {0}.", TerrainData.MapWidth));

            //var terrainDirectionalIcon = TerrainShape.TerrainDirectionalIcon; // Cache

            // 6/5/2012
            var terrainDirectionalIcon = TerrainDirectionalIconManager.CreateDirectionalIcon(name); // Cache

            terrainDirectionalIcon.Visible = visible;
            terrainDirectionalIcon.DirectionalIconPosition = position;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
            terrainDirectionalIcon.DirectionalIconColor = color;

            // 6/6/2012 - set rotation animation
            terrainDirectionalIcon.StartRotation(deltaMagnitude, rotationTimeMax);
        }

        // 3/3/2011 - Overload#1
        ///<summary>
        /// Shows the ground Directional Icon.
        ///</summary>
        ///<param name="name">Name of icon to reference.</param>
        ///<param name="visible">Is Visible?</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        ///<param name="size">Size of Icon</param>
        ///<param name="rotation">Angle of Icon; like 90 degrees.</param>
        ///<param name="color">Color of Icon.</param>
        public static void DisplayDirectionalIcon(string name, bool visible, int waypointIndex, int size, float rotation, Color color)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Remove TerrainScale
            Vector3.Multiply(ref goalPosition, 0.10f, out goalPosition);

            //var terrainDirectionalIcon = TerrainShape.TerrainDirectionalIcon; // Cache
            // 6/5/2012
            var terrainDirectionalIcon = TerrainDirectionalIconManager.CreateDirectionalIcon(name); // Cache

            terrainDirectionalIcon.Visible = visible;
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
            terrainDirectionalIcon.DirectionalIconColor = color;
        }

        // 6/6/2012
        ///<summary>
        /// Shows the ground Directional Icon and sets a rotation animation.
        ///</summary>
        ///<param name="name">Name of icon to reference.</param>
        ///<param name="visible">Is Visible?</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        ///<param name="size">Size of Icon</param>
        ///<param name="rotation">Angle of Icon; like 90 degrees.</param>
        ///<param name="color">Color of Icon.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        public static void DisplayDirectionalIcon(string name, bool visible, int waypointIndex, int size, float rotation, Color color, float deltaMagnitude, int rotationTimeMax)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Remove TerrainScale
            Vector3.Multiply(ref goalPosition, 0.10f, out goalPosition);

            //var terrainDirectionalIcon = TerrainShape.TerrainDirectionalIcon; // Cache
            // 6/5/2012
            var terrainDirectionalIcon = TerrainDirectionalIconManager.CreateDirectionalIcon(name); // Cache

            terrainDirectionalIcon.Visible = visible;
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
            terrainDirectionalIcon.DirectionalIconColor = color;

            // 6/6/2012 - set rotation animation
            terrainDirectionalIcon.StartRotation(deltaMagnitude, rotationTimeMax);
        }

        // 6/6/2012
        /// <summary>
        /// Sets the movement bound area for the Directional Icon.
        /// </summary>
        /// <param name="name">Name of icon to adjust.</param>
        /// <param name="boundArea"><see cref="Rectangle"/> as bound area to set.</param>
        public static void SetDirectionalIconBoundArea(string name, Rectangle boundArea)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            var minBound = new Vector3(boundArea.Left, 0, boundArea.Top);
            var maxBound = new Vector3(boundArea.Right, 0, boundArea.Bottom);

            // Set new bound area.
            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.SetMovementBoundArea(ref minBound, ref maxBound);
        }

        // 6/6/2012
        /// <summary>
        /// Sets the movement bound area for the Directional Icon.
        /// </summary>
        /// <param name="name">Name of icon to adjust.</param>
        /// <param name="triggerAreaName">TriggerArea name as bound area to set.</param>
        public static void SetDirectionalIconBoundArea(string name, string triggerAreaName)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Rectangle boundArea;
            TriggerAreas_GetVisualRectangle(triggerAreaName, out boundArea);

            var minBound = new Vector3(boundArea.Left, 0, boundArea.Top);
            var maxBound = new Vector3(boundArea.Right, 0, boundArea.Bottom);

            // Set new bound area.
            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.SetMovementBoundArea(ref minBound, ref maxBound);
        }

        // 6/6/2012
        /// <summary>
        /// Sets an animated rotation for the given Directional Icon <see cref="name"/>.
        /// </summary>
        ///<param name="name">Name of icon to reference.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        public static void SetDirectionalIconRotation(string name, float deltaMagnitude, int rotationTimeMax)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            TerrainDirectionalIconManager.StartRotation(name, deltaMagnitude, rotationTimeMax);
        }

        // 6/6/2012
        /// <summary>
        /// Updates the Directional Icon's postion, based on the requested <see cref="MovementDirectionEnum"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        /// <param name="movementDirection"><see cref="MovementDirectionEnum"/> request.</param>
        /// <param name="deltaMagnitude">The Position's delta magnitude change value. (Rate of Change)</param>
        /// <returns>Current position of Directional Icon.</returns>
        public static Vector3 UpdateDirectionalIcon(string name, MovementDirectionEnum movementDirection, float deltaMagnitude)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            Vector3 currentPosition;

            switch (movementDirection)
            {
                case MovementDirectionEnum.Still:
                    currentPosition = TerrainDirectionalIconManager.SetMovementDirection(name,
                                                                       TerrainDirectionalIcon.MovementDirection.Still, deltaMagnitude);
                    break;
                case MovementDirectionEnum.Right:
                    currentPosition = TerrainDirectionalIconManager.SetMovementDirection(name,
                                                                       TerrainDirectionalIcon.MovementDirection.Right, deltaMagnitude);
                    break;
                case MovementDirectionEnum.Left:
                    currentPosition = TerrainDirectionalIconManager.SetMovementDirection(name,
                                                                       TerrainDirectionalIcon.MovementDirection.Left, deltaMagnitude);
                    break;
                case MovementDirectionEnum.Up:
                    currentPosition = TerrainDirectionalIconManager.SetMovementDirection(name,
                                                                                         TerrainDirectionalIcon.
                                                                                             MovementDirection.Up,
                                                                                         deltaMagnitude);
                    break;
                case MovementDirectionEnum.Down:
                    currentPosition = TerrainDirectionalIconManager.SetMovementDirection(name,
                                                                                         TerrainDirectionalIcon.
                                                                                             MovementDirection.Down,
                                                                                         deltaMagnitude);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("movementDirection");
            }

            // 6/13/2012 - Get height at current position.
            currentPosition.Y = TerrainData.GetTerrainHeight(currentPosition.X, currentPosition.Z);

            return currentPosition;
        }

        // 6/14/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        public static void UpdateDirectionalIcon(string name, int waypointIndex)
        {
            // get location for given waypoint index
            Vector3 position = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            UpdateDirectionalIcon(name, ref position);
        }

        // 6/14/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="waypointIndex"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        /// <param name="size">New size for icon.</param>
        public static void UpdateDirectionalIcon(string name, int waypointIndex, int size)
        {
            // get location for given waypoint index
            Vector3 position = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            UpdateDirectionalIcon(name, ref position, size);
        }

        // 6/14/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="waypointIndex"/>, <paramref name="size"/> and <paramref name="rotation"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        /// <param name="size">New size for icon.</param>
        /// <param name="rotation">Angle of Icon; like 90 degrees.</param>
        public static void UpdateDirectionalIcon(string name, int waypointIndex, int size, float rotation)
        {
            // get location for given waypoint index
            Vector3 position = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            UpdateDirectionalIcon(name, ref position, size, rotation);
        }

        // 6/14/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="waypointIndex"/>, <paramref name="size"/> and <paramref name="rotation"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        ///<param name="waypointIndex">Waypoint index position to use.</param>
        /// <param name="size">New size for icon.</param>
        /// <param name="rotation">Angle of Icon; like 90 degrees.</param>
        /// <param name="color">Color of Icon.</param>
        public static void UpdateDirectionalIcon(string name, int waypointIndex, int size, float rotation, Color color)
        {
            // get location for given waypoint index
            Vector3 position = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            UpdateDirectionalIcon(name, ref position, size, rotation, color);
        }

        // 6/5/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="position"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        /// <param name="position"><see cref="Vector3"/> as position</param>
        public static void UpdateDirectionalIcon(string name, ref Vector3 position)
        {
            // Remove TerrainScale
            Vector3 goalPosition;
            Vector3.Multiply(ref position, 0.10f, out goalPosition);
          
            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
        }

        // 6/6/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="position"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        /// <param name="position"><see cref="Vector3"/> as position</param>
        /// <param name="size">New size for icon.</param>
        public static void UpdateDirectionalIcon(string name, ref Vector3 position, int size)
        {
            // Remove TerrainScale
            Vector3 goalPosition;
            Vector3.Multiply(ref position, 0.10f, out goalPosition);

            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
            terrainDirectionalIcon.DirectionalIconSize = size;
        }

        // 6/6/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="position"/>, <paramref name="size"/> and <paramref name="rotation"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        /// <param name="position"><see cref="Vector3"/> as position</param>
        /// <param name="size">New size for icon.</param>
        /// <param name="rotation">Angle of Icon; like 90 degrees.</param>
        public static void UpdateDirectionalIcon(string name, ref Vector3 position, int size, float rotation)
        {
            // Remove TerrainScale
            Vector3 goalPosition;
            Vector3.Multiply(ref position, 0.10f, out goalPosition);

            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
        }

        // 6/6/2012
        /// <summary>
        /// Updates the Directional Icon to the given <paramref name="position"/>, <paramref name="size"/> and <paramref name="rotation"/>.
        /// </summary>
        /// <param name="name">Name of icon to reference.</param>
        /// <param name="position"><see cref="Vector3"/> as position</param>
        /// <param name="size">New size for icon.</param>
        /// <param name="rotation">Angle of Icon; like 90 degrees.</param>
        /// <param name="color">Color of Icon.</param>
        public static void UpdateDirectionalIcon(string name, ref Vector3 position, int size, float rotation, Color color)
        {
            // Remove TerrainScale
            Vector3 goalPosition;
            Vector3.Multiply(ref position, 0.10f, out goalPosition);

            var terrainDirectionalIcon = TerrainDirectionalIconManager.GetDirectionalIcon(name);
            terrainDirectionalIcon.DirectionalIconPosition = goalPosition;
            terrainDirectionalIcon.DirectionalIconSize = size;
            terrainDirectionalIcon.DirectionalIconRotation = rotation;
            terrainDirectionalIcon.DirectionalIconColor = color;
        }

        // 5/26/2012 - 
        /// <summary>
        /// Sets the Bloom effect to a specific <see cref="BloomType"/>.
        /// </summary>
        /// <param name="bloomType">Set to <see cref="BloomType"/> to use.</param>
        public static void SetBloomType(BloomType bloomType)
        {
            var screenManager = (ScreenManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(ScreenManager));
            if (screenManager != null) screenManager.BloomSetting = bloomType;
        }


#endregion

        #region SpawnItem Methods

        // 10/21/2009
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="newScale">Set Scale value.</param>
        public static void SpawnItemTypeForPlayerAtWaypoint(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, float newScale)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // 5/17/2012 - Spawn the ItemType
            ScriptingHelper.SpawnItemType(playerNumber, itemTypeToSpawn, player, goalPosition, newScale, "E$"); 
        }

        // 1/14/2011 - Overload#1
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <param name="newScale">Set Scale value.</param>
        public static void SpawnItemTypeForPlayerAtWaypoint(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, string sceneItemName, float newScale)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // 1/14/2011 - Validates given name.
            ValidateSceneItemName(sceneItemName);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // 5/17/2012 - Spawn the ItemType
            ScriptingHelper.SpawnItemType(playerNumber, itemTypeToSpawn, player, goalPosition, newScale, sceneItemName);
        }

        // 1/14/2011
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position
        ///  with the given <paramref name="relativeOffset"/>, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="relativeOffset"><see cref="Vector3"/> position offset relative to waypoint position.</param>
        /// <param name="newScale">Set Scale value.</param>
        public static void SpawnItemTypeForPlayerAtWaypointWithOffset(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, Vector3 relativeOffset, float newScale)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SpawnItemTypeForPlayerAtWaypointWithOffset' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif

            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Add given offset value to create final position
            Vector3.Add(ref goalPosition, ref relativeOffset, out goalPosition);

            // 5/17/2012 - Spawn the ItemType
            ScriptingHelper.SpawnItemType(playerNumber, itemTypeToSpawn, player, goalPosition, newScale, "E$");
        }

        // 1/14/2011 - Overload#1
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position
        ///  with the given <paramref name="relativeOffset"/>, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="relativeOffset"><see cref="Vector3"/> position offset relative to waypoint position.</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <param name="newScale">Set Scale value.</param>
        public static void SpawnItemTypeForPlayerAtWaypointWithOffset(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, 
                                                                      Vector3 relativeOffset, string sceneItemName, float newScale)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SpawnItemTypeForPlayerAtWaypointWithOffset' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif

            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // 1/14/2011 - Validates given name.
            ValidateSceneItemName(sceneItemName);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Add given offset value to create final position
            Vector3.Add(ref goalPosition, ref relativeOffset, out goalPosition);

            // 5/17/2012 - Spawn the ItemType
            ScriptingHelper.SpawnItemType(playerNumber, itemTypeToSpawn, player, goalPosition, newScale, sceneItemName);
        }

        // 1/13/2011
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position, 
        /// for the given <see cref="Player"/>, set to follow the given <paramref name="waypointPathName"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        public static void SpawnItemTypeForPlayerAtWaypointFollowingWaypointPath(byte playerNumber, ItemType itemTypeToSpawn,
                                                                                 int waypointIndex, string waypointPathName)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Create SceneItem
            var sceneItemNumber = Player.AddSceneItem(player, itemTypeToSpawn, goalPosition);

            // Get Instance just created, to finish populating with data
            SceneItemWithPick newSceneItem;
            Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

            // 5/17/2012
            if (newSceneItem == null)
            {
                throw new NullReferenceException("Not a valid Selectable Item.");
            }

            // Update playerNumber
            newSceneItem.PlayerNumber = playerNumber;

            // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
            newSceneItem.SpawnByScriptingAction = true;

            if (DoFollowWaypointPath(newSceneItem, waypointPathName)) return;

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        // 1/14/2011 - Overload#1
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position, 
        /// for the given <see cref="Player"/>, set to follow the given <paramref name="waypointPathName"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for <see cref="TerrainWaypoints"/>#3.</param>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        public static void SpawnItemTypeForPlayerAtWaypointFollowingWaypointPath(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, 
                                                                                 string waypointPathName, string sceneItemName)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // 1/14/2011 - Validates given name.
            ValidateSceneItemName(sceneItemName);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Create SceneItem
            var sceneItemNumber = Player.AddSceneItem(player, itemTypeToSpawn, goalPosition);

            // Get Instance just created, to finish populating with data
            SceneItemWithPick newSceneItem;
            Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

            // 5/17/2012
            if (newSceneItem == null)
            {
                throw new NullReferenceException("Not a valid Selectable Item.");
            }

            // Update playerNumber
            newSceneItem.PlayerNumber = playerNumber;

            // 1/14/2011 - Set SceneItemName
            newSceneItem.Name = sceneItemName;

            // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
            newSceneItem.SpawnByScriptingAction = true;

            if (DoFollowWaypointPath(newSceneItem, waypointPathName)) return;

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        // 10/21/2009
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position with rotation, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="rotationToSet">Rotation to set item to (degrees)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="rotationToSet"/> is not within the range 0-360.</exception>
        public static void SpawnItemTypeForPlayerAtWaypointRotated(byte playerNumber, ItemType itemTypeToSpawn, int waypointIndex, int rotationToSet)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SpawnItemTypeForPlayerAtWaypointRotated' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // verify rotation value given falls within 0-360 range.
            if (rotationToSet < 0 || rotationToSet > 360)
                throw new ArgumentOutOfRangeException("rotationToSet", @"Rotation value given MUST be in degrees; range 0-360.");

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Create SceneItem
            var sceneItemNumber = Player.AddSceneItem(player, itemTypeToSpawn, goalPosition);

            // Get Instance just created, to finish populating with data
            SceneItemWithPick newSceneItem;
            Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

            // 5/17/2012
            if (newSceneItem == null)
            {
                throw new NullReferenceException("Not a valid Selectable Item.");
            }
           
            // Update playerNumber
            newSceneItem.PlayerNumber = playerNumber;

            // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
            newSceneItem.SpawnByScriptingAction = true;

            // 2/6/2011 - AStarItem is ONLY empty for buildings...
            // Rotation needs to be updated using the SmoothHeading; this is what the 'TurnToFaceAbstractBehavior' uses!
            if (newSceneItem.AStarItemI != null)
            {
                newSceneItem.AStarItemI.SmoothHeading.X = (float) Math.Cos(MathHelper.ToRadians(rotationToSet));
                newSceneItem.AStarItemI.SmoothHeading.Z = (float) Math.Sin(MathHelper.ToRadians(rotationToSet));
            }
            else
            {
                // Set Into Rotation 
                var rotationAxis = Matrix.CreateRotationY(MathHelper.ToRadians(rotationToSet));
                Quaternion newRotation;
                Quaternion.CreateFromRotationMatrix(ref rotationAxis, out newRotation);

                newSceneItem.Rotation = newRotation;
            }
        }

        // 1/14/2011 - Overload#1
        /// <summary>
        /// Spawns the given <see cref="ItemType"/> at the given <see cref="TerrainWaypoints"/> position with rotation, for the given <see cref="Player"/>.
        /// </summary>
        /// <param name="playerNumber"><see cref="Player"/> to add <see cref="ItemType"/> to</param>
        /// <param name="itemTypeToSpawn"><see cref="ItemType"/> enum to spawn; for example 'SciFiTank01' enum.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="rotationToSet">Rotation to set item to (degrees)</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="rotationToSet"/> is not within the range 0-360.</exception>
        public static void SpawnItemTypeForPlayerAtWaypointRotated(byte playerNumber, ItemType itemTypeToSpawn, 
                                                                int waypointIndex, int rotationToSet, string sceneItemName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SpawnItemTypeForPlayerAtWaypointRotated' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // verify rotation value given falls within 0-360 range.
            if (rotationToSet < 0 || rotationToSet > 360)
                throw new ArgumentOutOfRangeException("rotationToSet", @"Rotation value given MUST be in degrees; range 0-360.");

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // 1/14/2011 - Validates given name.
            ValidateSceneItemName(sceneItemName);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Create SceneItem
            var sceneItemNumber = Player.AddSceneItem(player, itemTypeToSpawn, goalPosition);

            // Get Instance just created, to finish populating with data
            SceneItemWithPick newSceneItem;
            Player.GetSelectableItem(player, sceneItemNumber, out newSceneItem);

            // 5/17/2012
            if (newSceneItem == null)
            {
                throw new NullReferenceException("Not a valid Selectable Item.");
            }

            // Update playerNumber
            newSceneItem.PlayerNumber = playerNumber;

            // 5/31/2012 - set flag to designate this item was created dynamically with a scripting call.
            newSceneItem.SpawnByScriptingAction = true;

            // 1/14/2011 - Set SceneItemName
            newSceneItem.Name = sceneItemName;

            // 2/6/2011 - AStarItem is ONLY empty for buildings...
            // Rotation needs to be updated using the SmoothHeading; this is what the 'TurnToFaceAbstractBehavior' uses!
            if (newSceneItem.AStarItemI != null)
            {
                newSceneItem.AStarItemI.SmoothHeading.X = (float) Math.Cos(MathHelper.ToRadians(rotationToSet));
                newSceneItem.AStarItemI.SmoothHeading.Z = (float) Math.Sin(MathHelper.ToRadians(rotationToSet));
            }
            else
            {
                // Set Into Rotation 
                var rotationAxis = Matrix.CreateRotationY(MathHelper.ToRadians(rotationToSet));
                Quaternion newRotation;
                Quaternion.CreateFromRotationMatrix(ref rotationAxis, out newRotation);

                newSceneItem.Rotation = newRotation;
            }
        }

        #endregion

        #region SpawnSmoke Methods

        // 6/2/2012
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            // Note: DO NOT set the 'Settings' before the creation of the ParticleSytem; otherwise, the settings are lost.
            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);
            //particleSystem.SetColors(minColor, maxColor);
            //particleSystem.Settings.ColorMultiplier = colorMultiplier;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            return instanceKey;
        }

        // 6/2/2012
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="height">Adjust waypoint position's height value. (set as zero to use ground's height.)</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex, float height)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set height value
            if (height != 0.0f)
            {
                goalPosition.Y += height;
            }

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            // Note: DO NOT set the 'Settings' before the creation of the ParticleSytem; otherwise, the settings are lost.
            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);
            //particleSystem.SetColors(minColor, maxColor);
            //particleSystem.Settings.ColorMultiplier = colorMultiplier;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            return instanceKey;
        }

        // 1/16/2010
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="height">Adjust waypoint position's height value. (set as zero to use ground's height.)</param>
        /// <param name="minColor">Set Min Color of Smoke</param>
        /// <param name="maxColor">Set Max Color of Smoke</param>
        /// <param name="colorMultiplier">Used to increase or reduce the color range.</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex, float height, Color minColor, Color maxColor, Vector4 colorMultiplier)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set height value
            if (height != 0.0f)
            {
                goalPosition.Y += height;
            }

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            // Note: DO NOT set the 'Settings' before the creation of the ParticleSytem; otherwise, the settings are lost.
            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);
            particleSystem.SetColors(minColor, maxColor);
            particleSystem.Settings.ColorMultiplier = colorMultiplier;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            return instanceKey;
        }

        // 6/2/2012
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="height">Adjust waypoint position's height value. (set as zero to use ground's height.)</param>
        /// <param name="minColor">Set Min Color of Smoke</param>
        /// <param name="maxColor">Set Max Color of Smoke</param>
        /// <param name="colorMultiplier">Used to increase or reduce the color range.</param>
        /// <param name="duration">How long these particles will last.</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex, float height, Color minColor, Color maxColor, Vector4 colorMultiplier, TimeSpan duration)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set height value
            if (height != 0.0f)
            {
                goalPosition.Y += height;
            }

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            // Note: DO NOT set the 'Settings' before the creation of the ParticleSytem; otherwise, the settings are lost.
            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);

            // update the attributes.
            particleSystem.SetColors(minColor, maxColor);
            particleSystem.Settings.ColorMultiplier = colorMultiplier;
            particleSystem.Settings.Duration = duration;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            return instanceKey;
        }

        // 6/1/2012
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="height">Adjust waypoint position's height value. (set as zero to use ground's height.)</param>
        /// <param name="minColor">Set Min Color of Smoke</param>
        /// <param name="maxColor">Set Max Color of Smoke</param>
        /// <param name="colorMultiplier">Used to increase or reduce the color range.</param>
        /// <param name="duration">How long these particles will last.</param>
        /// <param name="directionVelocity">Direction and velocity of wind. (To make particles rise, set negative values on Y-axis.  To apply gravity, set positive values on Y-axis.)</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex, float height, Color minColor, Color maxColor, 
                                     Vector4 colorMultiplier, TimeSpan duration, Vector3 directionVelocity)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set height value
            if (height != 0.0f)
            {
                goalPosition.Y += height;
            }

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            // Note: DO NOT set the 'Settings' before the creation of the ParticleSytem; otherwise, the settings are lost.
            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);

            // update the attributes.
            particleSystem.SetColors(minColor, maxColor);
            particleSystem.Settings.ColorMultiplier = colorMultiplier;
            particleSystem.Settings.Duration = duration;
            particleSystem.Settings.Gravity = directionVelocity;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            return instanceKey;
        }

        // 6/2/2012
        /// <summary>
        /// Spawns smoke, at the given <see cref="TerrainWaypoints"/> position.  Smoke
        /// can be of any color.
        /// </summary>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="height">Adjust waypoint position's height value. (set as zero to use ground's height.)</param>
        /// <param name="minColor">Set Min Color of Smoke</param>
        /// <param name="maxColor">Set Max Color of Smoke</param>
        /// <param name="colorMultiplier">Used to increase or reduce the color range.</param>
        /// <param name="duration">How long these particles will last.</param>
        /// <param name="directionVelocity">Direction and velocity of wind. (To make particles rise, set negative values on Y-axis.  To apply gravity, set positive values on Y-axis.)</param>
        /// <param name="particlesPerSecond">particles per second to emit</param>
        /// <param name="frequency"><see cref="TimeSpan"/> of frequecy to emit</param>
        /// <returns>InstanceKey</returns>
        public static int SpawnSmoke(int waypointIndex, float height, Color minColor, Color maxColor, Vector4 colorMultiplier, 
                                     TimeSpan duration, Vector3 directionVelocity, int particlesPerSecond, TimeSpan frequency)
        {
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set height value
            if (height != 0.0f)
            {
                goalPosition.Y += height;
            }

            // Create new ParticleSystem
            var emitVelocity = Vector3.Left;
            var smokePlume = new SmokePlumeParticleSystem(TemporalWars3DEngine.GameInstance,
                                                              TemporalWars3DEngine.GameInstance.Content);

            var instanceKey = ParticlesManager.AddNewParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem,
                                                                        smokePlume,
                                                                        true, ref goalPosition, ref emitVelocity);

            // retrieve new Instance, and set color of smoke.
            ParticleSystem particleSystem;
            ParticlesManager.GetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystem);

            // update the attributes.
            particleSystem.SetColors(minColor, maxColor);
            particleSystem.Settings.ColorMultiplier = colorMultiplier;
            particleSystem.Settings.Duration = duration;
            particleSystem.Settings.Gravity = directionVelocity;

            // Store changes back into list
            ParticlesManager.SetParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystem);

            // retrieve structure to update emitters.
            ParticleSystemItem particleSystemItem;
            ParticlesManager.GetParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, out particleSystemItem);

            // update the freq attributes
            particleSystemItem.ParticleEmitters.SetFrequencyAttributes(particlesPerSecond, frequency);

            // store changes back into list
            ParticlesManager.SetParticleSystemItem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey, particleSystemItem);

            return instanceKey;
        }

        // 1/16/2010
        /// <summary>
        /// Removes an instance of Smoke <see cref="ParticleSystem"/>, using the given InstanceKey.  
        /// </summary>
        /// <remarks>The InstanceKey is returned back from the <see cref="SpawnSmoke"/> script call.</remarks>
        /// <param name="instanceKey">InstanceKey of <see cref="ParticleSystem"/> to remove</param>
        public static void RemoveSmoke(int instanceKey)
        {
            // Remove ParticleSystem
            ParticlesManager.RemoveParticleSystem(ParticleSystemTypes.SmokePlumeParticleSystem, instanceKey);
        }

        #endregion

        #region SpawnExplosion Methods

        // 6/9/2012
        /// <summary>
        /// Spawns a new explosion at the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="explosionSize">Size of explosion defined as <see cref="ExplosionSizeEnum"/>.</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="explosionVelocity"><see cref="Vector3"/> as velocity of explosion cloud.</param>
        public static void SpawnExplosion(ExplosionSizeEnum explosionSize, int waypointIndex, Vector3 explosionVelocity)
        {
            // Default velocity
            if (explosionVelocity.Equals(Vector3.Zero))
                explosionVelocity = new Vector3(25);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            switch (explosionSize)
            {
                case ExplosionSizeEnum.Small:
                    ExplosionsManager.DoParticles_SmallExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Medium:
                    ExplosionsManager.DoParticles_MediumExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Large:
                    ExplosionsManager.DoParticles_LargeExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("explosionSize");
            }
        }

        // 6/9/2012
        /// <summary>
        /// Spawns a new explosion at the given <paramref name="goalPosition"/>.
        /// </summary>
        /// <param name="explosionSize">Size of explosion defined as <see cref="ExplosionSizeEnum"/>.</param>
        /// <param name="goalPosition"><see cref="Vector3"/> as location of explosion.</param>
        /// <param name="explosionVelocity"><see cref="Vector3"/> as velocity of explosion cloud.</param>
        /// <param name="onGround">Get ground height for given <paramref name="goalPosition"/>.</param>
        public static void SpawnExplosion(ExplosionSizeEnum explosionSize, Vector3 goalPosition, Vector3 explosionVelocity, bool onGround)
        {
            // Default velocity
            if (explosionVelocity.Equals(Vector3.Zero))
                explosionVelocity = new Vector3(25);

            // get correct height for given location
            if (onGround)
            {
                goalPosition.Y = TerrainData.GetTerrainHeight(goalPosition.X, goalPosition.Z);
            }

            switch (explosionSize)
            {
                case ExplosionSizeEnum.Small:
                    ExplosionsManager.DoParticles_SmallExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Medium:
                    ExplosionsManager.DoParticles_MediumExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Large:
                    ExplosionsManager.DoParticles_LargeExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("explosionSize");
            }
        }

        // 6/9/2012
        /// <summary>
        /// Spawns a new explosion at the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="explosionSize">Size of explosion defined as <see cref="ExplosionSizeEnum"/>.</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to spawn explosion at.</param>
        /// <param name="explosionVelocity"><see cref="Vector3"/> as velocity of explosion cloud.</param>
        /// <param name="heightAdjustment">Enter height adjustment, relative to the <see cref="sceneItemName"/> position; leave zero for no adjustment.</param>
        public static void SpawnExplosion(ExplosionSizeEnum explosionSize, string sceneItemName, Vector3 explosionVelocity, float heightAdjustment)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
            var goalPosition = namedSceneItem.Position;

            // Default velocity
            if (explosionVelocity.Equals(Vector3.Zero))
                explosionVelocity = new Vector3(25);

            // height adjustment
            goalPosition.Y += heightAdjustment;

            // get correct height for given location
            //goalPosition.Y = TerrainData.GetTerrainHeight(goalPosition.X, goalPosition.Z);

            switch (explosionSize)
            {
                case ExplosionSizeEnum.Small:
                    ExplosionsManager.DoParticles_SmallExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Medium:
                    ExplosionsManager.DoParticles_MediumExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                case ExplosionSizeEnum.Large:
                    ExplosionsManager.DoParticles_LargeExplosion(ref goalPosition, ref explosionVelocity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("explosionSize");
            }
        }

        #endregion

        #region Player

        ///<summary>
        /// Sets the player's maximum population limit.
        ///</summary>
        ///<param name="maxCount">Maximum population value to set</param>
        public static void SetPlayerMaxPopulation(int maxCount)
        {
            // Check if value is within range
            if (maxCount < 1 || maxCount > 100)
                throw new ArgumentOutOfRangeException("maxCount", @"Range MUSt be with 1-100.");

            Player.PopulationMax = maxCount;
        }

        // 10/4/2009
        /// <summary>
        /// Helper method, which checks if the given PlayerNumber is valid within the
        /// Players array, and if the <see cref="Player"/> is not NULL!
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to verify</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="playerNumber"/> is invalid.</exception>
        /// <returns>True/False</returns>
        private static bool DoPlayerArgCheck(int playerNumber)
        {
            // 6/15/2010 - Use new GetPlayers method.
            Player[] players; 
            TemporalWars3DEngine.GetPlayers(out players);

            if (playerNumber >= players.Length)
                throw new ArgumentOutOfRangeException("playerNumber", @"Invalid PlayerNumber given!");

            // make sure not null
            return players[playerNumber] != null;
        }

        // 10/11/2009
        /// <summary>
        /// Enables the ability for the <see cref="Player"/> to build the given
        /// <see cref="IFDGroupControlType"/> itemType; for example, Buildings.
        /// </summary>
        /// <param name="ifdGroupControlType"><see cref="IFDGroupControlType"/> to enable</param>
        public static void PlayerIsAllowedToBuild(IFDGroupControlType ifdGroupControlType)
        {
            // Enable all 'Tiles' for the given groupType.
            IFDTileManager.SetAbilityToBuildGroupControlType(ifdGroupControlType, true);
        }

        // 10/11/2009
        /// <summary>
        /// Disables the ability for the <see cref="Player"/> to build the given
        /// <see cref="IFDGroupControlType"/> itemType; for example, Buildings.
        /// </summary>
        /// <param name="ifdGroupControlType"><see cref="IFDGroupControlType"/> to disable</param>
        public static void PlayerIsNotAllowedToBuild(IFDGroupControlType ifdGroupControlType)
        {
            // Disable all 'Tiles' for the given groupType.
            IFDTileManager.SetAbilityToBuildGroupControlType(ifdGroupControlType, false);
        }

        // 10/11/2009
        /// <summary>
        /// Enables the ability for the <see cref="Player"/> to build the specific
        /// <see cref="ItemType"/> given; for example, 'Tank-04'.
        /// </summary>
        /// <param name="ifdGroupControlType">Enum <see cref="IFDGroupControlType"/> the ItemType belongs to</param>
        /// <param name="itemType"><see cref="ItemType"/> to affect</param>
        public static void PlayerIsAllowedToBuildItemType(IFDGroupControlType ifdGroupControlType, ItemType itemType)
        {
            // Enable all 'Tiles' which fit this match.
            IFDTileManager.SetAbilityToBuildSpecificItemType(ifdGroupControlType, itemType, true);
        }

        // 10/11/2009
        /// <summary>
        /// Disables the ability for the <see cref="Player"/> to build the specific
        /// <see cref="ItemType"/> given; for example, 'Tank-04'.
        /// </summary>
        /// <param name="ifdGroupControlType">Enum <see cref="IFDGroupControlType"/> the ItemType belongs to</param>
        /// <param name="itemType"><see cref="ItemType"/> to affect</param>
        public static void PlayerIsNotAllowedToBuildItemType(IFDGroupControlType ifdGroupControlType, ItemType itemType)
        {
            // Enable all 'Tiles which fit this match.
            IFDTileManager.SetAbilityToBuildSpecificItemType(ifdGroupControlType, itemType, false);
        }

        // 10/12/2009
        /// <summary>
        /// Increases or Decreases a <see cref="Player"/>'s <see cref="Player.Cash"/> value.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to affect</param>
        /// <param name="change">Value to increase or decrease</param>
        public static void PlayersMoneyIsIncreaseOrDecreaseBy(int playerNumber, int change)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // do money value change
            if (player != null) player.Cash += change;
        }

        // 10/12/2009
        /// <summary>
        /// Sets the <see cref="Player"/>'s <see cref="Player.Cash"/> value to the given number.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to affect</param>
        /// <param name="moneyValue">new money value</param>
        public static void PlayersMoneyIsSetAt(int playerNumber, int moneyValue)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // set new money value
            if (player != null) player.Cash = moneyValue;
        }

        // 10/12/2009
        /// <summary>
        /// Kills the given <see cref="Player"/>, by iterating all of its <see cref="Player._selectableItems"/>, and
        /// dealing a lethal amount of damage.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to affect</param>
        public static void KillGivenPlayer(int playerNumber)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player)) 
                return;

            // Kill Player
            Player.KillAllPlayersSelectableItems(player);
        }

        // 2/25/2011
        ///<summary>
        /// Sets the <see cref="DefenseAIStance"/> for all selectableItems of the
        /// given <paramref name="playerNumber"/>.
        ///</summary>
        ///<param name="playerNumber">PlayerNumber to affect</param>
        ///<param name="defenseAIStance">The <see cref="DefenseAIStance"/> to set.</param>
        public static void SetDefenseAIStance_AllItems_GivenPlayer(int playerNumber, DefenseAIStance defenseAIStance)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            if (!TemporalWars3DEngine.GetPlayer(playerNumber, out player))
                return;

            // Retrieve selectableItems
            ReadOnlyCollection<SceneItemWithPick> selectableItems;
            Player.GetSelectableItems(player, out selectableItems);

            if (selectableItems == null) return;

            // Iterate items and update
            for (var i = 0; i < selectableItems.Count; i++)
            {
                // Retrieve item
                var selectableItem = selectableItems[i];
                if (selectableItem == null) continue;

                // Set DefenseAIStance
                selectableItem.DefenseAIStance = defenseAIStance;
            }

        }
       

        #endregion

        #region NamedItems

        // 5/27/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> 'Collision' radius.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="collisionRadius">Set the collision radius for this item.</param>
        public static void SetCollisionForNamedItem(string sceneItemName, float collisionRadius)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
            namedSceneItem.CollisionRadius = collisionRadius;
        }

        // 5/27/2012
        /// <summary>
        /// Gets the given <paramref name="sceneItemName"/> 'Collision' radius.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to retrieve collision.</param>
        /// <returns>Collision radius as float.</returns>
        public static float GetCollisionForNamedItem(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
            return namedSceneItem.CollisionRadius;
        }

        // 6/9/2012
        /// <summary>
        /// Terminates all ScriptingActions for the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">>Named <see cref="SceneItem"/> to terminate all scripting actions.</param>
        public static void TerminateAllScriptingActions(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Call Manager to terminate actions
            ScriptingActionChangeRequestManager.TerminateAllScriptingActions(namedSceneItem.UniqueKey);
        }

        // 5/28/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a simple movement.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="startPosition">Set to position item should start from as <see cref="Vector3"/>; if Zero, then the <see cref="SceneItem"/> current position will be used.</param>
        /// <param name="goalPosition">Set to position item should move to as <see cref="Vector3"/>.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="keepOnGround">Keep <see cref="SceneItem"/> on ground during movement?</param>
        public static void SetMovementForNamedItem(string sceneItemName, Vector3 startPosition, Vector3 goalPosition, float maxVelocity, Vector3 rotationForce, bool keepOnGround)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Set goalPosition into sceneItem
            namedSceneItem.MoveToWayPosition = goalPosition;

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.StartPosition = startPosition;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.KeepOnGround = keepOnGround;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.MovementRequest);
        }

        // 5/29/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a simple movement.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="startWaypointIndex">Set to starting waypoint index.</param>
        /// <param name="goalWaypointIndex">Set to goal waypoint index.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="keepOnGround">Keep <see cref="SceneItem"/> on ground during movement?</param>
        public static void SetMovementForNamedItem(string sceneItemName, int startWaypointIndex, int goalWaypointIndex, float maxVelocity, Vector3 rotationForce, bool keepOnGround)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // get location for given waypoint index
            Vector3 startPosition = ScriptingHelper.GetExistingWaypoint(startWaypointIndex);
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(goalWaypointIndex);

            // Set goalPosition into sceneItem
            namedSceneItem.MoveToWayPosition = goalPosition;

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.StartPosition = startPosition;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.KeepOnGround = keepOnGround;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.MovementRequest);
        }

        // 6/8/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a simple movement following the given waypoint path.  
        /// All edges are given the same settings for <paramref name="maxVelocity"/>, <paramref name="rotationForce"/> and <paramref name="keepOnGround"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="waypointPathName">Name for waypoint path to follow.</param>
        /// <param name="repeat">Set to repeat the overall operation. ( -1 = Continous; 0 = No-Repeat; 1+ = Repetition )</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="keepOnGround">Keep <see cref="SceneItem"/> on ground during movement?</param>
        public static void SetMovementOnPathForNamedItem(string sceneItemName, string waypointPathName, int repeat, float maxVelocity, Vector3 rotationForce, bool keepOnGround)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            List<MovementOnPathAttributes> movements;
            Waypoints_GetEdgesForWaypointPath(waypointPathName, out movements);

            // check if collection given is null
            if (movements == null)
            {
                throw new ArgumentNullException("movements");
            }

            // check if collection is empty
            var movementsCount = movements.Count;
            if (movementsCount == 0)
            {
                throw new ArgumentOutOfRangeException("movements", "The movement collection MUST have a count greater than zero.  Verify a call was made to the 'Waypoints_GetEdgesForWaypointPath' ScriptingAction.");
            }

            // iterate movements and set with default values
            for (var i = 0; i < movementsCount; i++)
            {
                SetMovementOnPathForNamedItem_AddEdgeMovement(sceneItemName, maxVelocity, rotationForce, keepOnGround, movements, i);
            }

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            var changeRequest = (ScriptingActionMovementOnPathsRequest)ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.PathMovementRequest);

            // Add ROC movements to the request
            changeRequest.PopulateEdgeMovements(movements);

            // Set 'Repeat'
            changeRequest.Repeat = repeat;

        }

        // 5/29/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a simple movement following the given waypoint path.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="waypointPathName">Name for waypoint path to follow.</param>
        /// <param name="repeat">Set to repeat the overall operation. ( -1 = Continous; 0 = No-Repeat; 1+ = Repetition )</param>
        /// <param name="movements">A Collection with the movements for each edge; use the method <see cref="Waypoints_GetEdgesForWaypointPath"/> to get collection.</param>
        /// <remarks>Call the ScriptingAction <see cref="Waypoints_GetEdgesForWaypointPath"/> to retrieve the required <paramref name="movements"/> collection for a given waypoint-path name.</remarks>
        public static void SetMovementOnPathForNamedItem(string sceneItemName, string waypointPathName, int repeat, List<MovementOnPathAttributes> movements)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if collection given is null
            if (movements == null)
            {
                throw new ArgumentNullException("movements");
            }

            // check if collection is empty
            if (movements.Count == 0)
            {
                throw new ArgumentOutOfRangeException("movements", "The movement collection MUST have a count greater than zero.  Verify a call was made to the 'Waypoints_GetEdgesForWaypointPath' ScriptingAction.");
            }

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            var changeRequest = (ScriptingActionMovementOnPathsRequest)ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.PathMovementRequest);

            // Add ROC movements to the request
            changeRequest.PopulateEdgeMovements(movements);

            // Set 'Repeat'
            changeRequest.Repeat = repeat;

        }

        // 5/29/2012
        /// <summary>
        /// Adds a ScriptingAction 'Edge' rotation request to the internal collection
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        /// <param name="rotationType">Set to the rotation type to use.</param>
        /// <param name="rotationDirection">Set to the rotation direction to use.</param>
        /// <param name="movements">Collection of <see cref="MovementOnPathAttributes"/> retrieved from ScriptingAction call 'Waypoints_GetEdgesForWaypointPath'.</param>
        /// <param name="edgeIndex">Edge index to update in collection.</param>
        public static void SetMovementOnPathForNamedItem_AddEdgeRotation(string sceneItemName, float deltaMagnitude, int rotationTimeMax,
                                                                         RotationTypeEnum rotationType, RotationDirectionEnum rotationDirection,
                                                                         List<MovementOnPathAttributes> movements, int edgeIndex)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if collection given is null
            if (movements == null)
            {
                throw new ArgumentNullException("movements");
            }

            // check if collection is empty
            if (movements.Count == 0)
            {
                throw new ArgumentOutOfRangeException("movements", "The movement collection MUST have a count greater than zero.  Verify a call was made to the 'Waypoints_GetEdgesForWaypointPath' ScriptingAction.");
            }

            // create proper rotation attributes
            var scriptingActionAtts = new ScriptingActionAttributes
                                          {ChangeRequestEnum = ScriptingActionChangeRequestEnum.RotationRequest};
            var attributes = scriptingActionAtts.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.RotationType = rotationType;
            attributes.RotationDirection = rotationDirection;
            attributes.RotationTimeMax = rotationTimeMax;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.DeltaMagnitude = deltaMagnitude;
            scriptingActionAtts.ChangeRequestAttributes = attributes;

            // verify 'EdgeIndex' is valid
            if (edgeIndex >= movements.Count)
            {
                throw new ArgumentOutOfRangeException("edgeIndex", "Index given is not valid index in the 'movements' collection!");
            }

            // add to structure's MovementOnPathAttributes collection
            var movementToUpdate = movements[edgeIndex];
            movementToUpdate.ScriptingActions.Add(scriptingActionAtts);
            movements[edgeIndex] = movementToUpdate;
        }

        // 5/29/2012
        /// <summary>
        /// Adds a ScriptingAction 'Edge' scale request to the internal collection.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <param name="newScale">Scale value in the range of 0.01f to 10.</param>
        /// <param name="deltaMagnitude">The Scale's delta magnitude change value. (Rate of Change)</param>
        /// <param name="scaleTypeEnum">Scale type to use.</param>
        /// <param name="movements">Collection of <see cref="MovementOnPathAttributes"/> retrieved from ScriptingAction call 'Waypoints_GetEdgesForWaypointPath'.</param>
        /// <param name="edgeIndex">Edge index to update in collection.</param>
        public static void SetMovementOnPathForNamedItem_AddEdgeScale(string sceneItemName, float newScale, float deltaMagnitude, 
                                                                      ScaleTypeEnum scaleTypeEnum, List<MovementOnPathAttributes> movements, int edgeIndex)
        {
            MathHelper.Clamp(newScale, 0.01f, 10);

            if (deltaMagnitude < 0)
                throw new ArgumentOutOfRangeException("deltaMagnitude", "Value MUST be zero or greater.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // create proper scale attributes
            var scriptingActionAtts = new ScriptingActionAttributes { ChangeRequestEnum = ScriptingActionChangeRequestEnum.ScaleRequest };
            var attributes = scriptingActionAtts.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;

            attributes.Scale = newScale;
            attributes.ScaleType = scaleTypeEnum;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.DeltaMagnitude = deltaMagnitude;
            scriptingActionAtts.ChangeRequestAttributes = attributes;

            // verify 'EdgeIndex' is valid
            if (edgeIndex >= movements.Count)
            {
                throw new ArgumentOutOfRangeException("edgeIndex", "Index given is not valid index in the 'movements' collection!");
            }

            // add to structure's MovementOnPathAttributes collection
            var movementToUpdate = movements[edgeIndex];
            movementToUpdate.ScriptingActions.Add(scriptingActionAtts);
            movements[edgeIndex] = movementToUpdate;
            
        }

        // 5/29/2012
        /// <summary>
        /// Adds a ScriptingAction simple movement request for the specified <paramref name="edgeIndex"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="keepOnGround">Keep <see cref="SceneItem"/> on ground during movement?</param>
        /// <param name="movements">Collection of <see cref="MovementOnPathAttributes"/> retrieved from ScriptingAction call 'Waypoints_GetEdgesForWaypointPath'.</param>
        /// <param name="edgeIndex">Edge index to update in collection.</param>
        public static void SetMovementOnPathForNamedItem_AddEdgeMovement(string sceneItemName, float maxVelocity, Vector3 rotationForce,
                                                                         bool keepOnGround, List<MovementOnPathAttributes> movements, int edgeIndex)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // verify 'EdgeIndex' is valid
            if (edgeIndex >= movements.Count)
            {
                throw new ArgumentOutOfRangeException("edgeIndex", "Index given is not valid index in the 'movements' collection!");
            }

            // retrieve edge
            var movementToUpdate = movements[edgeIndex];

            // create proper scale attributes
            var scriptingActionAtts = new ScriptingActionAttributes { ChangeRequestEnum = ScriptingActionChangeRequestEnum.MovementRequest };
            var attributes = scriptingActionAtts.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;

            attributes.StartPosition = movementToUpdate.StartPosition;
            attributes.GoalPosition = movementToUpdate.GoalPosition;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.KeepOnGround = keepOnGround;
            scriptingActionAtts.ChangeRequestAttributes = attributes;

            // add to structure's MovementOnPathAttributes collection
            movementToUpdate.ScriptingActions.Add(scriptingActionAtts);
            movements[edgeIndex] = movementToUpdate;
        }

        // 5/29/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a simple movement from its current position.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="goalWaypointIndex">Set to goal waypoint index.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="keepOnGround">Keep <see cref="SceneItem"/> on ground during movement?</param>
        public static void SetMovementForNamedItem(string sceneItemName, int goalWaypointIndex, float maxVelocity, Vector3 rotationForce, bool keepOnGround)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
           
            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(goalWaypointIndex);

            // Set goalPosition into sceneItem
            namedSceneItem.MoveToWayPosition = goalPosition;

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.StartPosition = Vector3.Zero;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.KeepOnGround = keepOnGround;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.MovementRequest);
        }

        // 5/20/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a 'Toss' movement at the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="upForce">Set to the maximum UP force</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="accuracyPercent">Set to some accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Set to some error distance offset from goalPosition.</param>
        /// <param name="objectWeight">Weight of the given object, which will affect the gravity pull.</param>
        /// <remarks>
        /// Toss movements have a default life-span of 10 seconds.  Once this time is reached, the action will be terminated!  To override 
        /// this settings, use one of the overloads to set a different time-span.
        /// </remarks>
        public static void SetTossMovementForNamedItem(string sceneItemName, int waypointIndex, float maxVelocity, float upForce, 
                                                       Vector3 rotationForce, int accuracyPercent, float errorDistanceOffset, float objectWeight)
        {
            if (accuracyPercent < 0 || accuracyPercent > 100)
                throw new ArgumentOutOfRangeException("accuracyPercent", "Accuracy percent MUST be in the range of 0 - 100.");

            if (errorDistanceOffset < 0)
                throw new ArgumentOutOfRangeException("errorDistanceOffset", "Error distance offset circle MUST be greater than zero.");

            if (objectWeight < 1)
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.WaypointIndex = waypointIndex;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.AccuracyPercent = accuracyPercent;
            attributes.ErrorDistanceOffset = errorDistanceOffset;
            attributes.UpForce = upForce;
            attributes.ObjectWeight = objectWeight; // 6/4/2012
            attributes.UseLifeSpanCheck = true; // 6/13/2012
            attributes.MaxLifeSpan = 10000; // 6/13/2012
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.TossMovementRequest);
        }

        // 6/13/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a 'Toss' movement at the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="upForce">Set to the maximum UP force</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="accuracyPercent">Set to some accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Set to some error distance offset from goalPosition.</param>
        /// <param name="objectWeight">Weight of the given object, which will affect the gravity pull.</param>
        /// <param name="maxLifeSpan">Gets or sets the total life span for this instance.  Once the life span is reached, the item is terminated.</param>
        public static void SetTossMovementForNamedItem(string sceneItemName, int waypointIndex, float maxVelocity, float upForce,
                                                       Vector3 rotationForce, int accuracyPercent, float errorDistanceOffset, 
                                                       float objectWeight, int maxLifeSpan)
        {
            if (accuracyPercent < 0 || accuracyPercent > 100)
                throw new ArgumentOutOfRangeException("accuracyPercent", "Accuracy percent MUST be in the range of 0 - 100.");

            if (errorDistanceOffset < 0)
                throw new ArgumentOutOfRangeException("errorDistanceOffset", "Error distance offset circle MUST be greater than zero.");

            if (objectWeight < 1)
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");

            if (maxLifeSpan <= 0)
                throw new ArgumentOutOfRangeException("maxLifeSpan", "Value given for Max life span MUST be greater than zero.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.WaypointIndex = waypointIndex;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.AccuracyPercent = accuracyPercent;
            attributes.ErrorDistanceOffset = errorDistanceOffset;
            attributes.UpForce = upForce;
            attributes.ObjectWeight = objectWeight; // 6/4/2012
            attributes.UseLifeSpanCheck = true; // 6/13/2012
            attributes.MaxLifeSpan = maxLifeSpan; // 6/13/2012
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.TossMovementRequest);
        }

        // 6/6/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a 'Toss' movement at the given <paramref name="goalPosition"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="goalPosition"><see cref="Vector3"/> as goal position.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="upForce">Set to the maximum UP force</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="accuracyPercent">Set to some accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Set to some error distance offset from goalPosition.</param>
        /// <param name="objectWeight">Weight of the given object, which will affect the gravity pull.</param>
        /// <remarks>
        /// Toss movements have a default life-span of 10 seconds.  Once this time is reached, the action will be terminated!  To override 
        /// this settings, use one of the overloads to set a different time-span.
        /// </remarks>
        public static void SetTossMovementForNamedItem(string sceneItemName, Vector3 goalPosition, float maxVelocity, float upForce,
                                                       Vector3 rotationForce, int accuracyPercent, float errorDistanceOffset, float objectWeight)
        {
            if (accuracyPercent < 0 || accuracyPercent > 100)
                throw new ArgumentOutOfRangeException("accuracyPercent", "Accuracy percent MUST be in the range of 0 - 100.");

            if (errorDistanceOffset < 0)
                throw new ArgumentOutOfRangeException("errorDistanceOffset", "Error distance offset circle MUST be greater than zero.");

            if (objectWeight < 1)
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.WaypointIndex = -1; // set to -1 to tell manager to use the proper constructor call.
            attributes.GoalPosition = goalPosition;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.AccuracyPercent = accuracyPercent;
            attributes.ErrorDistanceOffset = errorDistanceOffset;
            attributes.UpForce = upForce;
            attributes.ObjectWeight = objectWeight; // 6/4/2012
            attributes.UseLifeSpanCheck = true; // 6/13/2012
            attributes.MaxLifeSpan = 10000; // 6/13/2012
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.TossMovementRequest);
        }

        // 6/13/2012
        /// <summary>
        /// Sets the given <paramref name="sceneItemName"/> to do a 'Toss' movement at the given <paramref name="goalPosition"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update</param>
        /// <param name="goalPosition"><see cref="Vector3"/> as goal position.</param>
        /// <param name="maxVelocity">Set maximum forward 'seek' velocity</param>
        /// <param name="upForce">Set to the maximum UP force</param>
        /// <param name="rotationForce">Set the 3-axis rotation force; zero is off.</param>
        /// <param name="accuracyPercent">Set to some accuracy percent 0 - 100.</param>
        /// <param name="errorDistanceOffset">Set to some error distance offset from goalPosition.</param>
        /// <param name="objectWeight">Weight of the given object, which will affect the gravity pull.</param>
        /// <param name="maxLifeSpan">Gets or sets the total life span for this instance.  Once the life span is reached, the item is terminated.</param>
        public static void SetTossMovementForNamedItem(string sceneItemName, Vector3 goalPosition, float maxVelocity, float upForce,
                                                       Vector3 rotationForce, int accuracyPercent, float errorDistanceOffset, float objectWeight, int maxLifeSpan)
        {
            if (accuracyPercent < 0 || accuracyPercent > 100)
                throw new ArgumentOutOfRangeException("accuracyPercent", "Accuracy percent MUST be in the range of 0 - 100.");

            if (errorDistanceOffset < 0)
                throw new ArgumentOutOfRangeException("errorDistanceOffset", "Error distance offset circle MUST be greater than zero.");

            if (objectWeight < 1)
                throw new ArgumentOutOfRangeException("objectWeight", "Weight of object CANNOT be less than zero.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.WaypointIndex = -1; // set to -1 to tell manager to use the proper constructor call.
            attributes.GoalPosition = goalPosition;
            attributes.MaxVelocity = maxVelocity;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.AccuracyPercent = accuracyPercent;
            attributes.ErrorDistanceOffset = errorDistanceOffset;
            attributes.UpForce = upForce;
            attributes.ObjectWeight = objectWeight; // 6/4/2012
            attributes.UseLifeSpanCheck = true; // 6/13/2012
            attributes.MaxLifeSpan = maxLifeSpan; // 6/13/2012
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.TossMovementRequest);
        }

        // 5/19/2012
        /// <summary>
        /// Sets the growth Rotation for the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        /// <param name="rotationType">Set to the rotation type to use.</param>
        /// <param name="rotationDirection">Set to the rotation direction to use.</param>
        public static void SetRotationForNamedItem(string sceneItemName, float deltaMagnitude, int rotationTimeMax, 
                                                   RotationTypeEnum rotationType, RotationDirectionEnum rotationDirection)
        {
            if (deltaMagnitude < 0)
                throw new ArgumentOutOfRangeException("deltaMagnitude", "Value MUST be zero or greater.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.RotationType = rotationType;
            attributes.RotationDirection = rotationDirection;
            attributes.RotationTimeMax = rotationTimeMax;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.DeltaMagnitude = deltaMagnitude;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.RotationRequest);
        }

        // 5/17/2012
        /// <summary>
        /// Gets the Scale for the given <see cref="sceneItemName"/>
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <returns>Scale as <see cref="Vector3"/></returns>
        public static Vector3 GetScaleForNamedItem(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
            return namedSceneItem.Scale;
        }

        // 5/25/2012
        /// <summary>
        /// Gets the Position for the given <see cref="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <returns>Scale as <see cref="Vector3"/></returns>
        public static Vector3 GetPositionForNamedItem(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);
            return namedSceneItem.Position;
        }

        // 6/14/2012
        /// <summary>
        /// Gets the relative height above the terrain for the given <see cref="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <returns>Relative height value above the terrain.</returns>
        public static float GetHeightAboveTerrainForNamedItem(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // get the current terrain's height at this position
            var terrainHeight = TerrainData.GetTerrainHeight(namedSceneItem.Position.X, namedSceneItem.Position.Z);

            // return the height difference between the sceneItem and terrain.
            return namedSceneItem.Position.Y - terrainHeight;
        }

        // 5/17/2012
        /// <summary>
        /// Sets the Scale for the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to update.</param>
        /// <param name="newScale">Scale value in the range of 0.01f to 10.</param>
        /// <param name="deltaMagnitude">The Scale's delta magnitude change value. (Rate of Change)</param>
        /// <param name="scaleTypeEnum">Scale type to use.</param>
        public static void SetScaleForNamedItem(string sceneItemName, float newScale, float deltaMagnitude, ScaleTypeEnum scaleTypeEnum)
        {
            MathHelper.Clamp(newScale, 0.01f, 10);

            if (deltaMagnitude < 0)
                throw new ArgumentOutOfRangeException("deltaMagnitude", "Value MUST be zero or greater.");

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // Populate attributes for given creation request.
            var attributes = ScriptingActionChangeRequestManager.ChangeRequestAttributes;
            attributes.SceneItem = namedSceneItem;
            attributes.Scale = newScale;
            attributes.ScaleType = scaleTypeEnum;
            attributes.InstancedItemPickedIndex = instancedItemPickedIndex;
            attributes.DeltaMagnitude = deltaMagnitude;
            ScriptingActionChangeRequestManager.ChangeRequestAttributes = attributes;

            // Create new ScriptingActionRotationRequest instance and queue.
            ScriptingActionChangeRequestManager.CreateActionChangeRequest(ScriptingActionChangeRequestEnum.ScaleRequest);
        }

        // 2/25/2011
        ///<summary>
        /// Sets the <see cref="DefenseAIStance"/> for the given <paramref name="sceneItemName"/>.
        ///</summary>
        ///<param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        ///<param name="defenseAIStance">The <see cref="DefenseAIStance"/> to set.</param>
        public static void SetDefenseAIStanceForNamedItem(string sceneItemName, DefenseAIStance defenseAIStance)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SetDefenseAIStanceForNamedItem' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif

            Player.SetDefenseAIStance(sceneItemName, defenseAIStance);
        }
       
        // 10/12/2009
        /// <summary>
        /// Reduces <see cref="SceneItem.CurrentHealth"/> of a given Named <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="damageToApply">Damage to apply</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        public static void DealDamageToANamedItem(string sceneItemName, int damageToApply)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this method!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // Reduce item's health by given value
            namedSceneItem.ReduceHealth(damageToApply, 0);
        }

        // 10/12/2009
        /// <summary>
        /// Kills a Named <see cref="SceneItem"/>, by dealing a lethal amount of damage.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        public static void KillANamedItem(string sceneItemName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'KillANamedItem' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this method!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // get current health of item
            var currentHealth = namedSceneItem.CurrentHealth;

            // Reduce item's health by currentHealth + 100 to make sure it is dead!
            namedSceneItem.ReduceHealth(currentHealth + 100, 0);
        }

        // 10/12/2009
        /// <summary>
        /// Kills all <see cref="Player._selectableItems"/> belonging to the given <see cref="Player"/>, 
        /// in the given <see cref="TerrainTriggerAreas"/> name. (Scripting purposes)
        /// </summary>
        /// <param name="playerNumber">PlayerNumber of <see cref="Player"/> to check</param>
        /// <param name="triggerAreaName"><see cref="TerrainTriggerAreas"/> name to check</param>
        public static void KillAllUnitsBelongingToPlayerInTriggerArea(int playerNumber, string triggerAreaName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'KillAllUnitsBelongingToPlayerInTriggerArea' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif

            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;
 
            // CAll Kill method in TerrainTriggerAreas class.
            TerrainTriggerAreas.KillAllUnitsBelongingToPlayerInTriggerArea(playerNumber, triggerAreaName);
        }

        // 10/12/2009
        /// <summary>
        /// Sets the given Named <see cref="SceneItem"/> to the given <see cref="SceneItem.CurrentHealthPercent"/> value.  
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="healthPercent"><see cref="SceneItem.CurrentHealthPercent"/> value to set (0 - 100)</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        public static void SetHealthOfNamedItemToGivenPercent(string sceneItemName, int healthPercent)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this method!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // set new health percent
            namedSceneItem.SetHealthToGivenPercentage(healthPercent);
        }

        // 10/12/2009
        /// <summary>
        /// Flashes a given Named <see cref="SceneItem"/> 'White' for a specified amount of time in seconds.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check; can be both a 'Selectable' or 'Scenary' item.</param>
        /// <param name="timeInSeconds">How long to flash item (Seconds)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        public static void FlashNamedItemWhiteForSpecifiedAmountOfTime(string sceneItemName, int timeInSeconds)
        {
            // Call Player method to start Flash
            Player.FlashNamedSceneItemWhiteForSpecifiedAmountOfTime(sceneItemName, timeInSeconds);
        }

        // 10/14/2009
        /// <summary>
        /// The Named <see cref="SceneItem"/> will begin the process of turning toward the <see cref="TerrainWaypoints"/> position.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to check (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        public static void NamedItemBeginFacingWaypoint(string sceneItemName, int waypointIndex)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this method!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast to SceneItemWithPick.
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            // Tell sceneItem to Face the position.
            if (sceneItemWithPick != null) TerrainWaypoints.SceneItemBeginFacingWaypoint(sceneItemWithPick, waypointIndex);
        }

        // 10/14/2009
        /// <summary>
        /// The Named <see cref="SceneItem"/> will begin the process of turning toward the 2nd Named <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to turn (Only Selectable SceneItems allowed)</param>
        /// <param name="sceneItemNameToFace">Named <see cref="SceneItem"/> to face; can be both a 'Selectable' or 'Scenary' sceneItem.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemNameToFace"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemNameToFace"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemBeginFacingNamedItem(string sceneItemName, string sceneItemNameToFace)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemBeginFacingNamedItem' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // 2nd - Now try to get 'Named' sceneItem to face.
            int instancedItemPickedIndex2;
            SceneItem namedSceneItemToFace = ScriptingHelper.GetNamedItem(sceneItemNameToFace, out instancedItemPickedIndex2);

            // cast 1st item to SceneItemWithPick
            var itemToTurn = (namedSceneItem as SceneItemWithPick);

            // make sure not null
            if (itemToTurn == null)
                throw new InvalidOperationException("Given sceneItem Name was not able to cast to a SceneItemWithPick class!");

            // check if 2nd item is Scenary item; if so, then MUST get proper instance within item.
            var scenaryItem = (namedSceneItemToFace as ScenaryItemScene);
            if (scenaryItem != null)
            {
                // Begin Turn process
                var facePosition = scenaryItem.Position;
                itemToTurn.FaceWaypointPosition(ref facePosition);

                return;

            } // End If ScenaryItem

            // cast 2nd item to SceneItemWithPick
            var itemToFace = (namedSceneItemToFace as SceneItemWithPick);

            // make sure not null
            if (itemToFace == null)
                throw new InvalidOperationException("Given sceneItem Name was not able to cast to a SceneItemWithPick class!");

            // Begin Turn process
            var facePosition2 = itemToFace.Position;
            itemToTurn.FaceWaypointPosition(ref facePosition2);
        }

        // 10/15/2009
        /// <summary>
        /// Moves a given Named <see cref="SceneItem"/>, to the given <see cref="TerrainWaypoints"/> location.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to move (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void MoveNamedItemToWaypoint(string sceneItemName, int waypointIndex)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast item to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            // make sure not null
            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Given sceneItem Name was not able to cast to a SceneItemWithPick class!");

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // Get unit to start moving
            sceneItemWithPick.AStarItemI.AddWayPointGoalNode(ref goalPosition);
        }

        // 10/15/2009
        /// <summary>
        /// Teleports Named <see cref="SceneItem"/> to given <see cref="TerrainWaypoints"/> location.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to teleport (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void TeleportNamedItemToWaypoint(string sceneItemName, int waypointIndex)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'TeleportNamedItemToWaypoint' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // If ScenaryItem, then call the 'SearchByName' which sets the proper PickedIndex instance.
            var scenaryItemScene = namedSceneItem as ScenaryItemScene;
            if (scenaryItemScene != null)
            {
                // 6/8/2012 - Terminate any movement requests


                // Directly update position and MoveToPosition values
                scenaryItemScene.Position = goalPosition;
                return;
            }

            // cast item to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            // make sure not null
            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Given sceneItem Name was not able to cast to a SceneItemWithPick class!");

            // Directly update position and MoveToPosition values
            namedSceneItem.Position = goalPosition;

            // cache AStarItem
            var astarItem = sceneItemWithPick.AStarItemI;

            // Update, only if AStarItem is not NULL!
            if (astarItem != null)
            {
                astarItem.GoalPosition = goalPosition;

                // Save to PathNodeStride value
                Vector3 pathNodePosition;
                Vector3.Divide(ref goalPosition, TemporalWars3DEngine._pathNodeStride, out pathNodePosition);
                pathNodePosition.Y = goalPosition.Y;
                astarItem.PathNodePosition = pathNodePosition;

                // remove item from AStar at old position.
                AStarItem.RemoveOccupiedByAtOldPosition(astarItem);
                // Set AStar at current position
                AStarItem.SetOccupiedByAtCurrentPosition(astarItem);

            } // End AstarItem
        }

        // 10/16/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> will follow the given <see cref="TerrainWaypoints"/> path.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to teleport (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> is not valid.</exception>
        public static void NamedItemToFollowWaypointPath(string sceneItemName, string waypointPathName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

            // 1/13/2011 - Refactored the follow waypoint path code.
            if (DoFollowWaypointPath(sceneItemWithPick, waypointPathName)) return;

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        // 1/13/2011
        /// <summary>
        /// Helper method to do the follow the waypointPath logic for the given <paramref name="sceneItemWithPick"/>.
        /// </summary>
        /// <param name="sceneItemWithPick">Instance of <see cref="SceneItemWithPick"/>.</param>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        /// <returns>True/False</returns>
        private static bool DoFollowWaypointPath(SceneItemWithPick sceneItemWithPick, string waypointPathName)
        {
            // try get 'PathName' from TerrainWaypoints class
            LinkedList<int> linkedList;
            if (TerrainWaypoints.WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // iterate list, and add positions for sceneItem to follow.
                // NOTE: The FORACH construct could be used here; however this causes garbage on the XBOX!

                // 1st - get first & last items in linkedList.
                var currentWaypoint = linkedList.First;
                var lastWaypoint = linkedList.Last;

                // loop until linked item is last item in list.
                var isLast = false;
                while (!isLast)
                {
                    // get location for given waypoint index
                    Vector3 position = ScriptingHelper.GetExistingWaypoint(currentWaypoint.Value);

                    // enque waypoint position into item for pathfinding.
                    sceneItemWithPick.AStarItemI.PathToQueue.Enqueue(position);

                    // check if currentWaypoint was last one
                    if (currentWaypoint == lastWaypoint)
                        isLast = true;

                    // move to next item in linkedList
                    currentWaypoint = currentWaypoint.Next;

                } // End while loop
                    

                return true;
            }

            return false;
        }

        // 10/18/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> to Attack another Named <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="attackerName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <param name="attackieName">Named <see cref="SceneItem"/> of attackie (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="attackerName"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="attackieName"/> is not valid.</exception>
        public static void NamedItemToAttackNamedItem(string attackerName, string attackieName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemToAttackNamedItem' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 1st - try get 'Attacker' sceneItem from Player class
            int instancedItemPickedIndex;
            var sceneItemAttacker = ScriptingHelper.GetNamedItem(attackerName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(sceneItemAttacker);

            // cast to SceneItemWithPick
            var attacker = (SceneItemWithPick)sceneItemAttacker;

            // 2nd - try get 'Attackie'
            SceneItem sceneItemAttackie;
            if (Player.SceneItemsByName.TryGetValue(attackieName, out sceneItemAttackie))
            {
                // check if this is a ScenaryItem, which is not allowed for this 1st param!
                SceneItem.DoCheckIfSceneItemIsScenaryItemType(sceneItemAttackie);

                // Issue special attack order, which will move attacker close enough if attackie too far away!
                attacker.AttackOrderToAttackieAnywhereOnMap(sceneItemAttackie);

                return;
            }

            throw new ArgumentException(@"Given sceneItem Name is NOT VALID!", "attackieName");
        }

        // 10/18/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> to Attack some random selectableItem within the given <see cref=" TerrainTriggerAreas"/> name.
        /// </summary>
        /// <param name="playerNumber">PlayerNumber to check selectables</param>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <param name="triggerAreaName">Name of <see cref=" TerrainTriggerAreas"/> to check</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        public static void NamedItemToAttackAnythingInTriggerArea(int playerNumber, string sceneItemName, string triggerAreaName)
        {
            // verify proper PlayerNumber given, and not NULL instance!
            if (!DoPlayerArgCheck(playerNumber)) return;

            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var sceneItemAttacker = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(sceneItemAttacker);

            // cast to SceneItemWithPick
            var attacker = (SceneItemWithPick)sceneItemAttacker;

            // Get some random item for given TriggerArea.
            SceneItemWithPick selectableItem;
            if (TerrainTriggerAreas.GetSomeSelectableWithinTriggerArea(playerNumber, triggerAreaName, out selectableItem))
            {
                // Issue special attack order, which will move attacker close enough if attackie too far away!
                attacker.AttackOrderToAttackieAnywhereOnMap(selectableItem);

            } // End If Some selectable found.
        }
       

        // 10/20/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> to AttackMove to given <see cref="TerrainWaypoints"/> number.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> of attacker (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemAttackMoveToWaypoint(string sceneItemName, int waypointIndex)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemAttackMoveToWaypoint' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // get location for given waypoint index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

            // start AttacMove order
            Player.UnitMoveOrder(sceneItemWithPick, ref goalPosition, true, 0, false);
        }

        // 10/21/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> to AttackMove while following the given <see cref="TerrainWaypoints"/> Path name.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to teleport (Only Selectable SceneItems allowed)</param>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemAttackMoveFollowingWaypointPath(string sceneItemName, string waypointPathName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemAttackMoveFollowingWaypointPath' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");


            // try get 'PathName' from TerrainWaypoints class
            LinkedList<int> linkedList;
            if (TerrainWaypoints.WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // iterate list, and add positions for sceneItem to follow.
                // NOTE: The FORACH construct could be used here; however this causes garbage on the XBOX!

                // 1st - get first & last items in linkedList.
                var currentWaypoint = linkedList.First;
                var lastWaypoint = linkedList.Last;

                // loop until linked item is last item in list.
                var isLast = false;
                while (!isLast)
                {
                    // get location for given waypoint index
                    Vector3 position = ScriptingHelper.GetExistingWaypoint(currentWaypoint.Value);

                    // Enqueue waypoint AttackMove position
                    sceneItemWithPick.AttackMoveQueue.Enqueue(position);

                    // check if currentWaypoint was last one
                    if (currentWaypoint == lastWaypoint)
                        isLast = true;

                    // move to next item in linkedList
                    currentWaypoint = currentWaypoint.Next;

                } // End while loop

                // Now dequeue 1st AttackMove goal position to start.
                var startWaypoint = sceneItemWithPick.AttackMoveQueue.Dequeue();
                Player.UnitMoveOrder(sceneItemWithPick, ref startWaypoint, true, 0, false);

                return;
            }

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        // 10/21/2009 
        /// <summary>
        /// Named <see cref="SceneItem"/> to stop moving.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to stop (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemToStopMoving(string sceneItemName)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

            // stop item from moving by putting into Rest state, and clearing all pathing queues.
            //sceneItemWithPick.AStarItemI.SolutionFinal.Clear();
            AStarItem.ClearSolutionFinal(sceneItemWithPick.AStarItemI); // 6/9/2010 - Updated for LocklessQueue call.
            sceneItemWithPick.AStarItemI.PathToQueue.Clear();
            sceneItemWithPick.AStarItemI.PathToStack.Clear();
            sceneItemWithPick.ItemState = ItemStates.Resting;
        }

        // 10/21/2009
        /// <summary>
        /// Named <see cref="SceneItem"/> to start repairing itself.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to stop (Only Selectable SceneItems allowed)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemToRepairItself(string sceneItemName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'NamedItemToRepairItself' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

            // Start Repair Op
            sceneItemWithPick.DoRepair = true;
        }

        // 1/13/2011
        ///<summary>
        /// Named <see cref="SceneItem"/> to set either as selectable or non-selectable.
        ///</summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to stop (Only Selectable SceneItems allowed)</param>
        ///<param name="isSelectable">Set as selectable or non-selectable.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sceneItemName"/> is not valid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <paramref name="sceneItemName"/> is unable to cast to <see cref="SceneItemWithPick"/> type.</exception>
        public static void NamedItemIsSelectable(string sceneItemName, bool isSelectable)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a ScenaryItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsScenaryItemType(namedSceneItem);

            // cast SceneItem to SceneItemWithPick
            var sceneItemWithPick = (namedSceneItem as SceneItemWithPick);

            if (sceneItemWithPick == null)
                throw new InvalidOperationException("Cast of SceneItem to SceneItemWithPick failed!");

            Player player;
            TemporalWars3DEngine.GetPlayer(sceneItemWithPick.PlayerNumber, out player);

            // Set Selectable condition
            sceneItemWithPick.ItemMoveable = sceneItemWithPick.ItemSelectable = isSelectable;
        }

        #endregion

        #region Tutorial

        // 10/21/2009
        /// <summary>
        /// Iterates all <see cref="Player"/>'s <see cref="Player._itemsSelected"/> internal arrays, to verify all <see cref="SceneItem"/>s are
        /// deselected.  (Tutorial Use)
        /// </summary>
        public static void DeSelectsEverything()
        {
            // 6/15/2010 - get Players array
            Player[] players;
            TemporalWars3DEngine.GetPlayers(out players);

            // make sure not Null
            if (players == null) return;

            // itereate each player
            var length = players.Length; // 4/30/2010
            for (var i = 0; i < length; i++)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                if (!TemporalWars3DEngine.GetPlayer(i, out player))
                    break;

                // De-Select all items
                Player.DeSelectAll(player);

            } // End For Loop
        }

       

        // 10/22/2009
        /// <summary>
        /// Sets the given <see cref="IFDGroupControlType"/> as the production queue tab
        /// to display.  (Tutorial Use)
        /// </summary>
        /// <param name="ifdGroupControlType">Production Queue tab (Buildings for example)</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ifdGroupControlType"/> is not valid.</exception>
        public static void SwitchToProductionQueueTab(IFDGroupControlType ifdGroupControlType)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SwitchToProductionQueueTab' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // make sure not 'ControlGroup1' enum.
            if (ifdGroupControlType == IFDGroupControlType.ControlGroup1)
                throw new ArgumentException(@"The given GroupControlType is not valid for this method.", "ifdGroupControlType");

            // Set given GroupControlType to display.
            IFDTileManager.SetAsCurrentGroupToDisplay(ifdGroupControlType);
        }

        // 10/22/2009
        /// <summary>
        /// Sets the given <see cref="IFDGroupControlType"/> Tile (<see cref="ItemType"/> specific) to Flash. (Tutorial Use)
        /// </summary>
        /// <param name="ifdGroupControlType">Production Queue tab (Buildings for example)</param>
        /// <param name="itemType">Specific <see cref="ItemType"/> to Flash (Tank-03, for example)</param>
        /// <param name="flashTile">Enable or Disable flashing</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="ifdGroupControlType"/> is not valid.</exception>
        public static void FlashUIBuildButton(IFDGroupControlType ifdGroupControlType, ItemType itemType, bool flashTile)
        {
            // make sure not 'ControlGroup1' enum.
            if (ifdGroupControlType == IFDGroupControlType.ControlGroup1)
                throw new ArgumentException(@"The given GroupControlType is not valid for this method.", "ifdGroupControlType");

            // Set given GroupControlType to flash.
            IFDTileManager.SetToFlashSpecificItemType(ifdGroupControlType, itemType, flashTile);
        }

        #endregion

        #region Cinematics

        // 10/21/2009
        /// <summary>
        /// Locks or Unlocks the ability to rotate the <see cref="Camera"/>. (Tutorial Use)
        /// </summary>
        /// <param name="lockRotation">True/False</param>
        public static void CameraRotation_LockOrUnlock(bool lockRotation)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraRotation_LockOrUnlock' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // set the new Lock value into Camera
            Camera.LockRotation = lockRotation;
        }

        // 10/21/2009
        /// <summary>
        /// Locks or Unlocks the ability to scroll the <see cref="Camera"/>. (Tutorial Use)
        /// </summary>
        /// <param name="lockScroll">True/False</param>
        public static void CameraScroll_LockOrUnlock(bool lockScroll)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraScroll_LockOrUnlock' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // set the new Lock value into Camera
            Camera.LockScroll = lockScroll;
        }

        // 10/21/2009
        /// <summary>
        /// Locks or Unlocks the ability to zoom the <see cref="Camera"/>. (Tutorial Use)
        /// </summary>
        /// <param name="lockZoom">True/False</param>
        public static void CameraZoom_LockOrUnlock(bool lockZoom)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CameraZoom_LockOrUnlock' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // set the new Lock value into Camera
            Camera.LockZoom = lockZoom;
        }

        // 10/21/2009
        /// <summary>
        /// Locks or Unlocks the ability to reset the <see cref="Camera"/>. (Tutorial Use)
        /// </summary>
        /// <param name="lockReset">True/False</param>
        public static void CameraReset_LockOrUnlock(bool lockReset)
        {
            // set the new Lock value into Camera
            Camera.LockReset = lockReset;
        }

        // 10/21/2009
        /// <summary>
        /// Locks or Unlocks the ability to adjust the <see cref="Camera"/>. 
        /// In all, this will lock the 'Zoom', 'Scroll', 
        /// 'Rotation' and 'Reset' abilities. (Tutorial Use)
        /// </summary>
        /// <param name="lockAll">True/False</param>
        public static void Camera_LockOrUnlock(bool lockAll)
        {
            // set the new Lock value into Camera
            Camera.LockAll = lockAll;
        }

        
        // 10/22/2009
        /// <summary>
        /// Sets the <see cref="Camera"/> at given <see cref="TerrainWaypoints"/> position, to look at the other given <see cref="TerrainWaypoints"/> position.
        /// </summary>
        /// <param name="waypointPosition"><see cref="TerrainWaypoints"/> position to set  <see cref="Camera"/> at.</param>
        /// <param name="waypointToLookat"><see cref="TerrainWaypoints"/> position  <see cref="Camera"/> is looking at.</param>
        /// <param name="zoomHeight">Zoom height value (0.0 to 1.0)</param>
        public static void Camera_PositionCameraAtWaypointLookingAtWaypoint(int waypointPosition, int waypointToLookat, float zoomHeight)
        {
            // get location for given waypoint Position index
            Vector3 cameraPosition = ScriptingHelper.GetExistingWaypoint(waypointPosition);

            // get location for given waypoint LookAt index
            Vector3 cameraLookAt = ScriptingHelper.GetExistingWaypoint(waypointToLookat);

            // Position Camera
            Camera.SetUpCamera(ref cameraPosition, ref cameraLookAt, zoomHeight);
           
        }

        // 10/22/2009
        /// <summary>
        /// Sets the  <see cref="Camera"/> to move from the starting <see cref="TerrainWaypoints"/> position, to the given
        /// ending <see cref="TerrainWaypoints"/> position.  Also interpolates the height (Zoom) between the
        /// <see cref="TerrainWaypoints"/>s, using the given start and end values.  Finally, the amount of
        /// time to do this should be given in milliseconds.
        /// </summary>
        /// <remarks>
        /// The CameraMoveType affects what to update; either  <see cref="Camera"/> position or
        ///  <see cref="Camera"/> target (LookAt).
        /// </remarks>
        /// <param name="startWaypoint">Starting <see cref="TerrainWaypoints"/> index</param>
        /// <param name="endWaypoint">Ending <see cref="TerrainWaypoints"/> index</param>
        /// <param name="startZoomHeight">Staring Zoom height value (0.0 to 1.0)</param>
        /// <param name="endZoomHeight">Ending Zoom height value (0.0 to 1.0)</param>
        /// <param name="totalTime">Total time to complete the movement in milliseconds.</param>
        /// <param name="cameraMoveType">Affect either <see cref="Camera"/> position or <see cref="Camera"/> target (LookAt).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startZoomHeight"/> is not within range of 0.0-1.0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endZoomHeight"/> is not within range of 0.0-1.0.</exception>
        public static void Camera_MoveCameraFromWaypointToWaypoint(int startWaypoint, int endWaypoint, float startZoomHeight, 
                                                                 float endZoomHeight, int totalTime, CameraMoveType cameraMoveType)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_MoveCameraFromWaypointToWaypoint' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify Zoom height values are between 0.0 - 1.0
            if (startZoomHeight < 0.0f || startZoomHeight > 1.0f)
                throw new ArgumentOutOfRangeException("startZoomHeight", @"Value given is outside the allowable range of 0.0 - 1.0.");

            if (endZoomHeight < 0.0f || endZoomHeight > 1.0f)
                throw new ArgumentOutOfRangeException("endZoomHeight", @"Value given is outside the allowable range of 0.0 - 1.0.");


            // get location for given waypoint Position index
            Vector3 startPosition = ScriptingHelper.GetExistingWaypoint(startWaypoint);

            // get location for given waypoint Position index
            Vector3 endPosition = ScriptingHelper.GetExistingWaypoint(endWaypoint);


            // Create new CinematicNode struct for Camera.
            var cinematicNodeStart = new CinematicNode
                                    {
                                        Position = startPosition,
                                        HeightZoom = startZoomHeight,
                                        TimeToCompleteInMilliSeconds = 0
                                    };

            var cinematicNodeEnd = new CinematicNode
                                       {
                                           Position = endPosition,
                                           HeightZoom = endZoomHeight,
                                           TimeToCompleteInMilliSeconds = totalTime
                                       };

            // Add node to Camera Queue
            CameraCinematics.AddNewCinematicNodePair(ref cinematicNodeStart, ref cinematicNodeEnd, cameraMoveType);
        }

        // 10/24/2009
        /// <summary>
        /// Sets the <see cref="Camera"/> to follow some <see cref="TerrainWaypoints"/> Path name.  Also interpolates the height (Zoom)
        /// between the <see cref="TerrainWaypoints"/>s, using the given start and end values.  Finally, the amount of
        /// time to do this should be given in milliseconds.
        /// </summary>
        /// <remarks>
        /// The CameraMoveType affects what to update; either  <see cref="Camera"/> position or
        ///  <see cref="Camera"/> target (LookAt).
        /// </remarks>
        /// <param name="waypointPathName">A <see cref="TerrainWaypoints"/> Path to follow</param>
        /// <param name="startZoomHeight">Staring Zoom height value (0.0 to 1.0)</param>
        /// <param name="endZoomHeight">Ending Zoom height value (0.0 to 1.0)</param>
        /// <param name="totalTime">Total time to complete the movement in milliseconds.</param>
        /// <param name="cameraMoveType">Affect either <see cref="Camera"/> position or <see cref="Camera"/> target (LookAt).</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="startZoomHeight"/> is not within range of 0.0-1.0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="endZoomHeight"/> is not within range of 0.0-1.0.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="waypointPathName"/> is not valid.</exception>
        public static void Camera_MoveCameraFollowingWaypointPath(string waypointPathName, float startZoomHeight,
                                                                float endZoomHeight, int totalTime, CameraMoveType cameraMoveType)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_MoveCameraFollowingWaypointPath' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify Zoom height values are between 0.0 - 1.0
            if (startZoomHeight < 0.0f || startZoomHeight > 1.0f)
                throw new ArgumentOutOfRangeException("startZoomHeight", @"Value given is outside the allowable range of 0.0 - 1.0.");

            if (endZoomHeight < 0.0f || endZoomHeight > 1.0f)
                throw new ArgumentOutOfRangeException("endZoomHeight", @"Value given is outside the allowable range of 0.0 - 1.0.");

            // try get 'PathName' from TerrainWaypoints class
            LinkedList<int> linkedList;
            if (TerrainWaypoints.WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // Add linkedList path to Camera, to create a cinematic spline.
                CameraCinematics.AddNewCinematicSplineFromLinkedList(waypointPathName, linkedList, startZoomHeight, endZoomHeight, totalTime, cameraMoveType);
               
                return;
            }

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        // 10/26/2009
        /// <summary>
        /// Sets the <see cref="Camera"/> to follow a specific Named <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to follow (Only Selectable SceneItems allowed)</param>
        /// <param name="zoomHeight">Zoom height value (0.0 to 1.0)</param>
        /// <remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="zoomHeight"/> is not within range of 0.0-1.0.</exception>
        public static void Camera_SetCameraToFollowSpecificSceneItem(string sceneItemName, float zoomHeight)
        {
            // verify Zoom height values are between 0.0 - 1.0
            if (zoomHeight < 0.0f || zoomHeight > 1.0f)
                throw new ArgumentOutOfRangeException("zoomHeight", @"Value given is outside the allowable range of 0.0 - 1.0.");

            // Set into Camera Cinematics
            CameraCinematics.SetCameraToFollowSpecificSceneItem(sceneItemName, zoomHeight);
        }

        // 10/26/2009
        /// <summary>
        /// Stops the <see cref="Camera"/> from following any <see cref="SceneItem"/>s.
        /// </summary>
        public static void Camera_StopFollowingAnySceneItems()
        {
            // Tell Camera Cinematics to stop following any items.
            CameraCinematics.StopFollowingAnySceneItems();
        }

        // 1/15/2010
        /// <summary>
        /// Stops the <see cref="Camera"/> from following any current Cinematic <see cref="TerrainWaypoints"/> paths.
        /// </summary>
        public static void Camera_StopFollowingWaypointPaths()
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_StopFollowingWaypointPaths' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // Tell Camera Cinematics to stop following any splines.
            CameraCinematics.StopAllCinematicSplines();
        }

        // 10/26/2009
        /// <summary>
        /// Updates the <see cref="Camera"/>, by smoothly adjusting the <see cref="Camera"/>'s current roll value, to
        /// be the new roll value, interpolated by the given time value.
        /// </summary>
        /// <param name="newRoll">New roll value to adjust to. (Degrees)</param>
        /// <param name="totalTime">Total time to complete the movement in milliseconds.</param>
        /// <param name="useSmoothStep">Default is to use Linear Interpolation; however if this is TRUE, smoothStep is used, which
        /// Interpolates smoothly by easing in and out.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="newRoll"/> is not within range of -360 to 360.</exception>
        public static void Camera_ChangeCameraRoll(int newRoll, int totalTime, bool useSmoothStep)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_ChangeCameraRoll' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // verify rotation value given falls within 0-360 range.
            if (newRoll < -360 || newRoll > 360)
                throw new ArgumentOutOfRangeException("newRoll", @"Roll value given MUST be in degrees; range -360 to 360.");

            // Convert to radians and wrap angle
            var radians = MathHelper.WrapAngle(MathHelper.ToRadians(newRoll));

            // Tell Camera Cinematics to start the 'Roll' adjustment.
            CameraCinematics.AdjustCameraRollTo(radians, totalTime, useSmoothStep);
        }

        // 2/1/2010
        /// <summary>
        /// Enumeration for the Camera angle type to affect. 
        /// </summary>
        public enum CameraAngle
        {
            ///<summary>
            /// <see cref="Camera"/>'s <see cref="Camera.AlphaAngle"/> (rotation);
            ///</summary>
            AlphaAngle,
            ///<summary>
            /// <see cref="Camera"/>'s <see cref="Camera.BetaAngle"/> (Height).
            ///</summary>
            BetaAngle,
            ///<summary>
            /// Both <see cref="Camera.AlphaAngle"/> and <see cref="Camera.BetaAngle"/>.
            ///</summary>
            Both
        }

        // 2/1/2010
        /// <summary>
        /// Use to reset either the <see cref="Camera"/>'s <see cref="Camera.AlphaAngle"/>, or <see cref="Camera.BetaAngle"/> to original
        /// values.
        /// </summary>
        /// <remarks>
        /// <see cref="Camera.AlphaAngle"/> = <see cref="Camera"/>'s Rotation around the look-at point; Y-axis (UP).
        /// <see cref="Camera.BetaAngle"/>  = <see cref="Camera"/>'s Rotation around the look-at point; X-axis. 
        /// </remarks>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        /// <param name="cameraAngle"><see cref="CameraAngle"/> to set</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="cameraAngle"/> is invalid.</exception>
        public static void Camera_ResetAngle(GameTime gameTime, CameraAngle cameraAngle)
        {
            switch (cameraAngle)
            {
                case CameraAngle.AlphaAngle:
                    Camera.ResetCameraAlphaAngle();
                    break;
                case CameraAngle.BetaAngle:
                    Camera.ResetCameraBetaAngle();
                    break;
                case CameraAngle.Both:
                    Camera.ResetCameraPosition();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("cameraAngle");
            }

            // Small Hack: slight movement to force camera to pop in to position.
            Camera.CameraDirection = CameraDirectionEnum.ScrollBackward; // 6/15/2012
        }

        // 1/13/2011
        ///<summary>
        /// Sets the area which the camera can move in.
        ///</summary>
        ///<param name="cameraBounds"><see cref="Rectangle"/> as camera bounds area.</param>
        public static void Camera_SetCameraBoundArea(Rectangle cameraBounds)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_SetCameraBoundArea' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            var minBound = new Vector3(cameraBounds.Left, 0, cameraBounds.Top);
            var maxBound = new Vector3(cameraBounds.Right, 0, cameraBounds.Bottom);
            Camera.SetCameraBoundArea(ref minBound, ref maxBound);

            MiscThreadManager.AddItemRequest(new CameraBoundItemStruct<Rectangle>()
                                                  {
                                                      ActionMethod = DoSetCameraBoundArea,
                                                      MethodParam = cameraBounds
                                                  });
            
        }

        // 6/6/2012
        ///<summary>
        /// Sets the area which the camera can move in.
        ///</summary>
        ///<param name="triggerAreaName">TriggerArea name as bound area to set.</param>
        public static void Camera_SetCameraBoundArea(string triggerAreaName)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'Camera_SetCameraBoundArea' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            Rectangle cameraBounds;
            TriggerAreas_GetVisualRectangle(triggerAreaName, out cameraBounds);

            var minBound = new Vector3(cameraBounds.Left, 0, cameraBounds.Top);
            var maxBound = new Vector3(cameraBounds.Right, 0, cameraBounds.Bottom);
            Camera.SetCameraBoundArea(ref minBound, ref maxBound);

            MiscThreadManager.AddItemRequest(new CameraBoundItemStruct<Rectangle>()
            {
                ActionMethod = DoSetCameraBoundArea,
                MethodParam = cameraBounds
            });

        }

        // 2/28/2011
        /// <summary>
        /// Method helper which sets the area which the camera can move in.
        /// </summary>
        /// <param name="cameraBounds"><see cref="Rectangle"/> as camera bounds area.</param>
        private static void DoSetCameraBoundArea(Rectangle cameraBounds)
        {
            // Retrieve IAStarManager interface
            var iAStarManager = (IAStarManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IAStarManager));

            if (iAStarManager == null) return;
            
            // Retrieve IAStarGraph
            var iAStarGraph = iAStarManager.IAStarGraph;

            if (iAStarGraph == null) return;
            
            // Reset to original blocking data set.
            RestoreBlockingDataEvent.Reset();
            RestoreOriginalBlockingDataSet();

            // Wait until Restore complete
            RestoreBlockingDataEvent.WaitOne();

            // Retrieve current blocking collection
            _currentBlockingCollection = iAStarGraph.GetPathfindingGraph();
            _blockingDataSet = true;

            // Iterate entire A* NodeArraySize
            var nodeArraySize = iAStarGraph.NodeArraySize; // cache
            var nodeStride = iAStarGraph.NodeStride; // cache

            for (var x = 0; x < nodeArraySize; x++)
                for (var y = 0; y < nodeArraySize; y++)
                {
                    var node = new Point(x * nodeStride, y * nodeStride);

                    // check if outside rectangle bounds
                    if (!cameraBounds.Contains(node))
                    {
                        iAStarGraph.SetCostToPos(node.X, node.Y, -1, 1);
                    }

                    Thread.Sleep(1);
                }
           
            // Since new Blocking data loaded, need to update the AStar Neighbors lists!
            iAStarManager.ReInitAStarArrays(iAStarGraph.NodeArraySize);

            TerrainShape.PopulatePathNodesArray();
        }

        // 1/13/2011
        ///<summary>
        /// Sets the camera's bound area to the size of the terrain map.
        ///</summary>
        public static void Camera_SetDefaultCameraBoundArea()
        {
            Camera.SetDefaultCameraBoundArea();

            // 2/24/2011 - Restore original blocking data set.
            RestoreOriginalBlockingDataSet();
        }

        // 2/27/2011 - Note: Called automatically by the GameLevelManager
        /// <summary>
        /// Clears the prior levels blocking collection data set.
        /// </summary>
        internal static void Camera_ClearPriorBlockingDataSet()
        {
            _currentBlockingCollection.Clear();
            _blockingDataSet = false;
        }

        // 2/24/2011
        /// <summary>
        /// Updates the blocking data set.
        /// </summary>
        internal static void RestoreOriginalBlockingDataSet()
        {
            // check if blocking data set
            if (!_blockingDataSet) return;

            MiscThreadManager.AddItemRequest(new CameraBoundItemStruct<Rectangle>()
            {
                ActionMethod = DoRestoreOriginalBlockingDataSet,
                MethodParam = Rectangle.Empty
            });
            
        }

        // 2/28/2011
        /// <summary>
        /// Helper method which updates the blocking data set.
        /// </summary>
        private static void DoRestoreOriginalBlockingDataSet(Rectangle temp)
        {
            var iAStarManager = (IAStarManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IAStarManager));

            if (iAStarManager == null) return;
            
            // Retrieve IAStarGraph
            var iAStarGraph = iAStarManager.IAStarGraph;

            if (iAStarGraph == null) return;

            var nodeArraySize = iAStarGraph.NodeArraySize; // cache
            var nodeStride = iAStarGraph.NodeStride; // cache

            // Reset all costs to zero.
            for (var x = 0; x < nodeArraySize; x++)
                for (var y = 0; y < nodeArraySize; y++)
                {
                    var node = new Point(x * nodeStride, y * nodeStride);

                    if (!iAStarGraph.IsOccupied(node.X, node.Y, PathNodeType.GroundItem))
                        iAStarGraph.RemoveCostAtPos(node.X, node.Y, 1);

                    Thread.Sleep(1);
                }

            // Since new Blocking data loaded, need to update the AStar Neighbors lists!
            iAStarManager.ReInitAStarArrays(iAStarGraph.NodeArraySize);

            // restore original blocking data
            iAStarGraph.LoadAStarGraphBlockingData(_currentBlockingCollection, iAStarGraph.NodeStride);

            TerrainShape.PopulatePathNodesArray();

            // Signal completed
            RestoreBlockingDataEvent.Set();
        }

        #endregion

        #region Interface

        // 2/6/2011
        /// <summary>
        /// Loads a <see cref="Texture2D"/> production set, like the 'WarFactory-Tank' set, into memory.
        /// </summary>
        /// <param name="productionType">The <see cref="ItemGroupType"/> Enum production set to load</param>
        /// <param name="assetSide">The player's asset side to load; either side 1 or 2.</param>
// ReSharper disable InconsistentNaming
        public static void PreLoadIFDTileSet(ItemGroupType productionType, int assetSide)
// ReSharper restore InconsistentNaming
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'PreLoadIFDTileSet' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            IFDTileTextureLoader.PreLoadIFDTileSet(productionType, assetSide);
        }

        // 1/15/2010
        /// <summary>
        /// Update the visibility of the <see cref="IMinimap"/> component.
        /// </summary>
        /// <param name="visible">True/False to show <see cref="IMinimap"/>.</param>
        public static void DisplayMinimapComponent(bool visible)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'DisplayMinimapComponent' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // retrieve Minimap interface.
            var miniMap = (IMinimap) TemporalWars3DEngine.GameInstance.Services.GetService(typeof (IMinimap));

            // if miniMap Interface not present, then just return.
            if (miniMap == null) return;

            // otherwise, set visiblitity setting.
            miniMap.IsVisible = visible;
                
        }

        // 1/15/2010
        /// <summary>
        /// Update the visibility of the <see cref="IIFDTileManager"/> component.
        /// </summary>
        /// <param name="visible">True/False to show <see cref="IIFDTileManager"/>.</param>
        public static void DisplayInterfaceDisplayComponent(bool visible)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'DisplayInterfaceDisplayComponent' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // retrieve Visibility interface.
            var ifd = (IIFDTileManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IIFDTileManager));

            // if IFD Interface not present, then just return.
            if (ifd == null) return;

            // otherwise, set visiblitity setting.
            ifd.IsVisible = visible;
        }

        // 5/25/2012
        /// <summary>
        /// Creates a new <see cref="IFDTileMessage"/> instance for multi-timed messages, and returns the instance key.
        /// </summary>
        /// <param name="name">Set to a name to use for reference later.</param>
        /// <param name="tileLocation">Tile location defined as <see cref="Rectangle"/>.</param>
        /// <param name="messageOrigin">Message orgin set as <see cref="Vector2"/>.</param>
        /// <param name="fontColor">Set to a Font <see cref="Color"/>.</param>
        /// <param name="backgroundTexture">Texture name to use, which MUST be located in the '1ContentTextures' folder; example "\Textures\scoreBoard". (Empty string for no background texture.)</param>
        /// <param name="fontType">Font name to use, which MUST be located in the '1ContentMisc' folder; example "\Fonts\Arial18". (Empty string for default of Arial18)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDTileMultiTimedMessage(string name, ref Rectangle tileLocation, ref Vector2 messageOrigin, ref Color fontColor, string backgroundTexture, string fontType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name already exist for another IFD instance!");
            }

            // set default FontType, is empty.
            if (string.IsNullOrEmpty(fontType))
            {
                fontType = @"\Fonts\Arial18";
            }

            // Create new IFDMessage box.
            var multiTimedMessageBox = new IFDTileMessage(TemporalWars3DEngine.GameInstance, tileLocation, false)
            {
                MessageOrigin = messageOrigin, // -50, -65
                FontColor = fontColor,
                MessageFont = IFDTile.ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + fontType)
            };

            // Set background texture, if set.
            if (!string.IsNullOrEmpty(backgroundTexture))
            {
                multiTimedMessageBox.DrawBackground = true;
                multiTimedMessageBox.BackgroundTexture =
                   TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(
                   TemporalWars3DEngine.ContentTexturesLoc + backgroundTexture); // @"\Textures\scoreBoard"
            }

            // Add to Manager
            IFDTileManager.AddInterFaceDisplayTile(multiTimedMessageBox);

            // Add to Dictionaries
            IFDTileByNames.Add(name, multiTimedMessageBox);
            IFDTileKeytoNames.Add(multiTimedMessageBox.TileInstanceKey, name);

            return multiTimedMessageBox.TileInstanceKey;
        }

        // 5/25/2012
        /// <summary>
        /// Creates a new <see cref="IFDTileMessage"/> instance, and returns the instance key.
        /// </summary>
        /// <param name="name">Set to a name to use for reference later.</param>
        /// <param name="tileLocation">Tile location defined as <see cref="Rectangle"/>.</param>
        /// <param name="messageOrigin">Message orgin set as <see cref="Vector2"/>.</param>
        /// <param name="fontColor">Set to a Font <see cref="Color"/>.</param>
        /// <param name="backgroundTexture">Texture name to use, which MUST be located in the '1ContentTextures' folder; example "\Textures\scoreBoard". (Empty string for no background texture.)</param>
        /// <param name="fontType">Font name to use, which MUST be located in the '1ContentMisc' folder; example "\Fonts\Arial18". (Empty string for default of Arial18)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDTileMessage(string name, ref Rectangle tileLocation, ref Vector2 messageOrigin, ref Color fontColor, string backgroundTexture, string fontType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name already exist for another IFD instance!");
            }

            // set default FontType, is empty.
            if (string.IsNullOrEmpty(fontType))
            {
                fontType = @"\Fonts\Arial18";
            }

            // Create new IFDMessage box.
            var messageBox = new IFDTileMessage(TemporalWars3DEngine.GameInstance, "0", tileLocation, true)
            {
                SbMessageToDisplay = { Capacity = 10 },
                MessageOrigin = messageOrigin, // -50, -65
                FontColor = fontColor,
                MessageFont = IFDTile.ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + fontType) //  @"\Fonts\Arial18"
            };

            // Set background texture, if set.
            if (!string.IsNullOrEmpty(backgroundTexture))
            {
                messageBox.DrawBackground = true;
                messageBox.BackgroundTexture =
                   TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(
                   TemporalWars3DEngine.ContentTexturesLoc + backgroundTexture); // @"\Textures\scoreBoard"
            }

            // Add to Manager
            IFDTileManager.AddInterFaceDisplayTile(messageBox);

            // Add to Dictionaries
            IFDTileByNames.Add(name, messageBox);
            IFDTileKeytoNames.Add(messageBox.TileInstanceKey, name);

            return messageBox.TileInstanceKey;
        }

        // 5/25/2012
        /// <summary>
        /// Add a new message to display to the given <paramref name="name"/> <see cref="IFDTileMessage"/> instance.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to affect.</param>
        /// <param name="messageToQueue">Message to display</param>
        /// <param name="messageTimer">Amount of time to display the message</param>
        /// <param name="triggerEvent">Should trigger event, to signal message displayed?</param>
        public static void AddMultiTimedMessageToQueue(string name, string messageToQueue, int messageTimer, bool triggerEvent)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            // retrieve from name dictionary
            var messageTile = (IFDTileMessage)IFDTileByNames[name];

            // add new message to internal queue.
            messageTile.AddMultiTimedMessageToQueue(messageToQueue, messageTimer, triggerEvent);
        }

        // 6/14/2012
        /// <summary>
        /// Add a new message to display to the given <paramref name="name"/> <see cref="IFDTileMessage"/> instance.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to affect.</param>
        public static void ClearMultiTimedMessagesInQueue(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            // retrieve from name dictionary
            var messageTile = (IFDTileMessage)IFDTileByNames[name];

            // clear out all messages in queue
            messageTile.ClearMultiTimedMessagesInQueue();
        }

        // 5/25/2012
        /// <summary>
        /// Updates the message to display for the given <paramref name="name"/> <see cref="IFDTileMessage"/> instance.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to affect.</param>
        /// <param name="messageToDisplay">Message to display</param>
        public static void UpdateIFDTileMessageToDisplay(string name, string messageToDisplay)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            // retrieve from name dictionary
            var messageTile = (IFDTileMessage)IFDTileByNames[name];

            // Populate StringBuilder
            messageTile.SbMessageToDisplay.Length = 0;
            messageTile.SbMessageToDisplay.Append(messageToDisplay);
        }

        // 6/12/2012
        /// <summary>
        /// Updates the message to display's font color for the given <paramref name="name"/> <see cref="IFDTileMessage"/> instance.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to affect.</param>
        /// <param name="fontColor"><see cref="Color"/> to update font with.</param>
        public static void UpdateIFDTileMessageColor(string name, Color fontColor)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            // retrieve from name dictionary
            var messageTile = (IFDTileMessage)IFDTileByNames[name];

            messageTile.FontColor = fontColor;
        }

        // 5/25/2012
        /// <summary>
        /// Updates the 'Scoreboard' display cash amount.
        /// </summary>
        /// <param name="cash">New cash value to display</param>
        public static void UpdateScoreboardCashValue(int cash)
        {
            IFDTileManager.DoUpdateCashValueTile(cash);
        }

        // 5/25/2012
        /// <summary>
        /// Updates the 'Scoreboard' display energy amount.
        /// </summary>
        /// <param name="energy"></param>
        public static void UpdateScoreboardEneryValue(int energy)
        {
            IFDTileManager.DoUpdateEnergyValueTile(energy);
        }

        // 5/25/2012
        /// <summary>
        /// Set to turn On/Off the drawing of the 'Scoreboard' tile.
        /// </summary>
        /// <param name="drawTile">Draw scoreboard?</param>
        public static void DisplayScoreboard(bool drawTile)
        {
            IFDTileManager.DisplayScoreboard = drawTile;
        }

        // 5/25/2012
        /// <summary>
        /// Sets to turn On/Off the drawing of the 'IFDGroup' control tile.  This tile
        /// allows for the creation of RTS units, like tanks.
        /// </summary>
        /// <param name="drawTile">Draw IFDGroup control?</param>
        public static void DisplayIFDGroupControlTile(bool drawTile)
        {
            IFDTileManager.DisplayIFDGroupTileControl = drawTile;
        }

        // 5/25/2012
        /// <summary>
        /// Gets an instance of <see cref="IFDTileMessage"/> based on the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to retrieve.</param>
        /// <returns>Instance of <see cref="IFDTileMessage"/></returns>
        public static IFDTileMessage GetIFDMessageTile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            return (IFDTileMessage)IFDTileByNames[name];
        }

        // 2/23/2011
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="texture">Instance of <see cref="Texture2D"/>.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(Texture2D texture, ref Rectangle tileLocation, 
                                                ref Rectangle textureRectangleSize, float textureScale)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, texture, tileLocation)
                                     {
                                         TextureRectangleSize = textureRectangleSize,
                                         MainImageScale = textureScale
                                     };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 2/23/2011 - overload#1
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="texture">Instance of <see cref="Texture2D"/>.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        ///<param name="displayTime">Amount of time to display the texture in ms.</param>
        ///<returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(Texture2D texture, ref Rectangle tileLocation,
                                                ref Rectangle textureRectangleSize, float textureScale, float displayTime)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, texture, tileLocation)
            {
                TextureRectangleSize = textureRectangleSize,
                MainImageScale = textureScale,
                DisplayTime = displayTime
            };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 3/5/2011
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="texture">Instance of <see cref="Texture2D"/>.</param>
        ///<param name="backgroundTexture">Instance of <see cref="Texture2D"/> for background.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        ///<param name="displayTime">Amount of time to display the texture in ms.</param>
        ///<returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(Texture2D texture, Texture2D backgroundTexture, ref Rectangle tileLocation,
                                                ref Rectangle textureRectangleSize, float textureScale, float displayTime)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, texture, tileLocation)
            {
                TextureRectangleSize = textureRectangleSize,
                MainImageScale = textureScale,
                DisplayTime = displayTime,
                DrawBackground = true,
                BackgroundTexture = backgroundTexture
            };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 2/23/2011 - overload#2
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="textureName">Tile texture name.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(string textureName, ref Rectangle tileLocation,
                                                ref Rectangle textureRectangleSize, float textureScale)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, textureName, tileLocation)
            {
                TextureRectangleSize = textureRectangleSize,
                MainImageScale = textureScale
            };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 2/23/2011 - overload#3
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="textureName">Tile texture name.</param>
        ///<param name="backgroundTexture">Instance of <see cref="Texture2D"/> for background.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(string textureName, Texture2D backgroundTexture, ref Rectangle tileLocation,
                                                ref Rectangle textureRectangleSize, float textureScale, float displayTime)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, textureName, tileLocation)
            {
                TextureRectangleSize = textureRectangleSize,
                MainImageScale = textureScale,
                DisplayTime = displayTime,
                DrawBackground = true,
                BackgroundTexture = backgroundTexture
            };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 3/5/2011
        ///<summary>
        /// Creates a new <see cref="IFDTileOverlay"/> instance, returning the instance key.
        ///</summary>
        ///<param name="textureName">Tile texture name.</param>
        ///<param name="tileLocation"><see cref="Rectangle"/> location to display tile.</param>
        ///<param name="textureRectangleSize">Size of texture, given as a <see cref="Rectangle"/>.</param>
        ///<param name="textureScale">Tile's scale (0-1)</param>
        /// <returns>Tile's instance key</returns>
        public static int CreateIFDOverlayTile(string textureName, ref Rectangle tileLocation,
                                                ref Rectangle textureRectangleSize, float textureScale, float displayTime)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'CreateIFDOverlayTile' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // 2/23/2011 - Create IFDOverlay tile
            var ifdTileOverlay = new IFDTileOverlay(TemporalWars3DEngine.GameInstance, textureName, tileLocation)
            {
                TextureRectangleSize = textureRectangleSize,
                MainImageScale = textureScale,
                DisplayTime = displayTime
            };

            // add to tile manager.
            IFDTileManager.AddInterFaceDisplayTile(ifdTileOverlay);

            // return instance key to allow access to original instance.
            return ifdTileOverlay.TileInstanceKey;
        }

        // 2/23/2011
        /// <summary>
        /// Sets the visibility for the given <paramref name="tileInstanceKey"/>.
        /// </summary>
        /// <param name="tileInstanceKey">Tile's instance key.</param>
        /// <param name="isVisible">Visibility setting.</param>
// ReSharper disable InconsistentNaming
        public static void DisplayIFDOverlayTile(int tileInstanceKey, bool isVisible)
// ReSharper restore InconsistentNaming
        {
            IFDTileManager.SetVisibility(tileInstanceKey, isVisible);
        }

        // 2/23/2011
        /// <summary>
        /// Removes an <see cref="IFDTile"/>, using the
        /// given <paramref name="tileInstanceKey"/> as the search criteria.
        /// </summary>
        /// <param name="tileInstanceKey"><see cref="IFDTile"/> instance Key</param>  
// ReSharper disable InconsistentNaming
        public static void RemoveIFDTile(int tileInstanceKey)
// ReSharper restore InconsistentNaming
        {
            IFDTileManager.RemoveInterFaceDisplayTile(tileInstanceKey);

            // 5/25/2012 - Check to remove from Dictionaries
            string name;
            if (!IFDTileKeytoNames.TryGetValue(tileInstanceKey, out name))
            {
                return;
            }

            // remove from IfdTileKeyToName
            IFDTileKeytoNames.Remove(tileInstanceKey);
            // remove from IfdTileNames
            if (IFDTileByNames.ContainsKey(name))
            {
                IFDTileByNames.Remove(name);
            }
        }

        // 6/13/2012
        /// <summary>
        /// Removes an <see cref="IFDTileMessage"/> using the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of <see cref="IFDTileMessage"/> instance to remove.</param>
        public static void RemoveIFDMessageTile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!IFDTileByNames.ContainsKey(name))
            {
                throw new InvalidOperationException("The given name MUST be a valid name which already exist.");
            }

            // retrieve from name dictionary
            var messageTile = (IFDTileMessage)IFDTileByNames[name];
            if (messageTile == null) return;
           
            // Remove IFDTile
            RemoveIFDTile(messageTile.TileInstanceKey);
        }

        // 2/25/2011
        ///<summary>
        /// Updates the default wrapper texture to the given <paramref name="wrapperTexture"/>.
        ///</summary>
        ///<param name="wrapperTexture">Instance of <see cref="Texture2D"/>.</param>
        public static void UpdateWrapperTexture(Texture2D wrapperTexture)
        {
            // retrieve IMinimap
            var iMiniMap = (IMinimap)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IMinimap));

            // check if null
            if (iMiniMap == null) return;
            
            // Set new wrapper texture
            iMiniMap.UpdateWrapperTexture(wrapperTexture);

        }

        #endregion

        #region TriggerAreas

        // 1/13/2011
        ///<summary>
        /// Gets the rectangle area for a given <paramref name="triggerAreaName"/>.
        ///</summary>
        ///<param name="triggerAreaName">TriggerArea name</param>
        ///<param name="rectangleArea">(OUT) <see cref="TerrainTriggerAreas"/> visual rectangle.</param>
        ///<returns>True/False</returns>
        ///<exception cref="ArgumentNullException">Thrown when <paramref name="triggerAreaName"/> is not valid.</exception>
// ReSharper disable InconsistentNaming
        public static bool TriggerAreas_GetVisualRectangle(string triggerAreaName, out Rectangle rectangleArea)
// ReSharper restore InconsistentNaming
        {
            if (string.IsNullOrEmpty(triggerAreaName))
            {
                throw new ArgumentNullException("triggerAreaName");
            }

            return TerrainTriggerAreas.GetTriggerAreaRectangle(triggerAreaName, out rectangleArea);
        }

        // 5/25/2012
        /// <summary>
        /// Creates a new trigger area around the given waypoint index.
        /// </summary>
        /// <param name="triggerAreaName">Set to a new TriggerArea name</param>
        /// <param name="waypointIndex"><see cref="TerrainWaypoints"/> index; for example 3 is for Waypoint#3.</param>
        /// <param name="width">Width of TriggerArea</param>
        /// <param name="height">Height of TriggerArea</param>
        public static void TriggerAreas_CreateAtWaypoint(string triggerAreaName, int waypointIndex, int width, int height)
        {
            if (string.IsNullOrEmpty(triggerAreaName))
            {
                throw new ArgumentNullException("triggerAreaName");
            }

            // get location for given waypoint Position index
            Vector3 goalPosition = ScriptingHelper.GetExistingWaypoint(waypointIndex);

            // set starting positions
            var startX = goalPosition.X - (width/2.0f);
            var startY = goalPosition.Z - (height/2.0f);

            // create rectangleArea based on given waypoint location
            var rectangleArea = new Rectangle((int)startX, (int) startY, width, height);

            if (!TerrainTriggerAreas.AddNewTriggerArea(triggerAreaName, ref rectangleArea))
            {
                throw new InvalidOperationException("TriggerArea name given MUST be unique.");
            }
        }

        #endregion

        #region Waypoints

        // 6/5/2012
        /// <summary>
        /// Gets the current <see cref="Vector3"/> position for the given <paramref name="waypointIndex"/>.
        /// </summary>
        /// <param name="waypointIndex">Index key of waypoint to remove.</param>
        /// <returns><see cref="Vector3"/> as current waypoint position.</returns>
        public static Vector3 Waypoints_GetWaypointAt(int waypointIndex)
        {
            // get location for given waypoint index
            return ScriptingHelper.GetExistingWaypoint(waypointIndex);
        }

        // 5/28/2012
        /// <summary>
        /// Creates a new waypoint at the given <see cref="waypointLocation"/>, and returns the index key.
        /// </summary>
        /// <param name="waypointLocation"><see cref="Vector2"/> as waypoint location.</param>
        /// <returns>Index key to new waypoint.</returns>
        public static int Waypoints_AddWaypointAt(Vector2 waypointLocation)
        {
            // Create new waypoint Vector3, which includes correct height
            var height = TerrainData.GetTerrainHeight(waypointLocation.X, waypointLocation.Y);
            var waypoint = new Vector3(waypointLocation.X, height, waypointLocation.Y); 

            // Add to waypoints class
            return TerrainWaypoints.AddWaypoint(ref waypoint);
        }

        // 5/28/2012
        /// <summary>
        /// Removes the waypoint using the given <paramref name="waypointIndex"/>, and deletes any paths using
        /// this waypoint.
        /// </summary>
        /// <param name="waypointIndex">Index key of waypoint to remove.</param>
        public static void Waypoints_RemoveWaypoint(int waypointIndex)
        {
            // check if index exist.
            ScriptingHelper.DoesWaypointExist(waypointIndex);

            // remove from internal dictionary
            TerrainWaypoints.Waypoints.Remove(waypointIndex);

            // 2nd - delete from WaypointPath linked list, if exist in any. 
            TerrainWaypoints.DeleteWaypointFromAllWaypointPaths(waypointIndex);
        }

        // 5/29/2012
        /// <summary>
        /// Gets a collection of <see cref="MovementOnPathAttributes"/> for the given <paramref name="waypointPathName"/>, where
        /// each <see cref="MovementOnPathAttributes"/> represents an edge to the path.
        /// </summary>
        /// <param name="waypointPathName"><see cref="TerrainWaypoints"/> Path name to follow</param>
        /// <param name="edges">(OUT) Collection of <see cref="MovementOnPathAttributes"/></param>
        /// <remarks>This scripting call is a requirement to call before calling the <see cref="SetMovementOnPathForNamedItem"/>.</remarks>
        public static void Waypoints_GetEdgesForWaypointPath(string waypointPathName, out List<MovementOnPathAttributes> edges)
        {
            edges = new List<MovementOnPathAttributes>();

            // try get 'PathName' from TerrainWaypoints class
            LinkedList<int> linkedList;
            if (TerrainWaypoints.WaypointPaths.TryGetValue(waypointPathName, out linkedList))
            {
                // iterate list, and add positions for sceneItem to follow.
                // NOTE: The FORACH construct could be used here; however this causes garbage on the XBOX!

                // 1st - get first & last items in linkedList.
                var firstWaypoint = linkedList.First;
                var currentWaypoint = linkedList.First;
                var lastWaypoint = linkedList.Last;

                // check if open/close path loop
                var isCloseLoop = (firstWaypoint.Value == lastWaypoint.Value);

                // loop until linked item is last item in list.
                var isLast = false;
                int edgeCount = 0;
                while (!isLast)
                {
                    // get location for given waypoint index
                    Vector3 positionA = ScriptingHelper.GetExistingWaypoint(currentWaypoint.Value);

                    // check if currentWaypoint was last one
                    if (currentWaypoint == lastWaypoint)
                    {
                        isLast = true;
                        continue;
                    }

                    // move to next item in linkedList
                    currentWaypoint = currentWaypoint.Next;

                    // get location for given waypoint index
                    Vector3 positionB = ScriptingHelper.GetExistingWaypoint(currentWaypoint.Value);

                    // add waypoint position into collection
                    var edge = new MovementOnPathAttributes(positionA, positionB, string.Format("Edge {0}", ++edgeCount)) { IsCloseLoop = isCloseLoop };
                    edges.Add(edge);

                } // End while loop

                return;
            }

            throw new ArgumentException(@"Given waypointPath Name is NOT VALID!", "waypointPathName");
        }

        #endregion

        #region Shadows

        // 5/28/2012
        /// <summary>
        /// Sets the Shadow-Mapping's light position.  Generally, this would be the position
        /// of the sun.
        /// </summary>
        /// <param name="lightPosition"><see cref="Vector3"/> as light position.</param>
        public static void SetShadowsLightPosition(Vector3 lightPosition)
        {
            ShadowMap.DebugIsFor = DebugIsFor.LightPosition;
            TerrainShape.LightPosition = lightPosition;
        }

        // 5/28/2012
        /// <summary>
        /// Sets the Shadow-Mapping's target position.  Generally, this would be the a position
        /// in the middle of the terrain.
        /// </summary>
        /// <param name="lightTarget"><see cref="Vector3"/> as light target.</param>
        public static void SetShadowsTargetPosition(Vector3 lightTarget)
        {
            ShadowMap.DebugIsFor = DebugIsFor.LightTarget;
            ShadowMap.LightTarget = lightTarget;
        }

        // 4/24/2011
        ///<summary>
        /// Used to set the ShadowDarkness level.
        ///</summary>
        ///<param name="darkness">Enter shadow darkness value of 0-100, where 100 represents a solid black shadow.</param>
        public static void SetShadowDarkness(int darkness)
        {
#if WithLicense
#if !XBOX
            // 5/10/2012 - Return if trial.
            if (LicenseInstance.IsTrial)
            {
                throw new InvalidOperationException("'SetShadowDarkness' Valid ONLY in FULL PAID Version!");
            }
#endif
#endif
            // Verify within 0-100 range
            if (darkness < 0 || darkness > 100)
                throw new ArgumentOutOfRangeException("darkness", @"Shadow darkness MUST be in the range of 0 - 100.");

            // Inverse value before setting into ShadowMap.
            var invDarkness = 100 - darkness;

            // Update into ShadowMap component.
            ShadowMap.ShadowMapDarkness = MathHelper.Clamp((float)invDarkness / 100.0f, 0, 1.0f);
        }

        #endregion

        #region Audio Methods

        // 6/11/2012
        /// <summary>
        /// Stops playing all <see cref="Sounds"/> instances.
        /// </summary>
        public static void StopAllAudio()
        {
            AudioManager.StopAll();
        }

        // 6/11/2012
        /// <summary>
        /// Plays given <see cref="Sounds"/>, which can be accessed using the given <paramref name="soundName"/>.
        /// </summary>
        /// <param name="soundName">Unique name to reference sound by.</param>
        /// <param name="soundToPlay"><see cref="Sounds"/> to play.</param>
        public static void PlayAudio(string soundName, Sounds soundToPlay)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException("soundName");

            SoundNameStruct soundNameStruct;
            if (SoundNames.TryGetValue(soundName, out soundNameStruct))
            {
                AudioManager.Play(soundNameStruct.UniqueKey, soundToPlay);
                return;
            }

            // create new soundName structure
            soundNameStruct = new SoundNameStruct(soundNameStruct.SoundName, soundToPlay);

            // else add new soundName to dictionary
            SoundNames.Add(soundName, soundNameStruct);
            AudioManager.Play(soundNameStruct.UniqueKey, soundToPlay);
        }

        // 6/11/2012
        /// <summary>
        /// Pauses the <see cref="Sounds"/> associated by the given <paramref name="soundName"/>.
        /// </summary>
        /// <param name="soundName">Unique name to reference sound by.</param>
        public static void PauseAudio(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException("soundName");

            SoundNameStruct soundNameStruct;
            if (SoundNames.TryGetValue(soundName, out soundNameStruct))
            {
                AudioManager.Pause(soundNameStruct.UniqueKey, soundNameStruct.Sound);
                return;
            }

            throw new ArgumentException("Sound name given does NOT exist!", "soundName");
        }

        // 6/11/2012
        /// <summary>
        /// Resumes the <see cref="Sounds"/> associated by the given <paramref name="soundName"/>.
        /// </summary>
        /// <param name="soundName">Unique name to reference sound by.</param>
        public static void ResumeAudio(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException("soundName");

            SoundNameStruct soundNameStruct;
            if (SoundNames.TryGetValue(soundName, out soundNameStruct))
            {
                AudioManager.Resume(soundNameStruct.UniqueKey, soundNameStruct.Sound);
                return;
            }

            throw new ArgumentException("Sound name given does NOT exist!", "soundName");
        }

        // 6/11/2012
        /// <summary>
        /// Stops the <see cref="Sounds"/> associated by the given <paramref name="soundName"/>.
        /// </summary>
        /// <param name="soundName">Unique name to reference sound by.</param>
        public static void StopAudio(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException("soundName");

            SoundNameStruct soundNameStruct;
            if (SoundNames.TryGetValue(soundName, out soundNameStruct))
            {
                AudioManager.Stop(soundNameStruct.UniqueKey, soundNameStruct.Sound);
                return;
            }

            throw new ArgumentException("Sound name given does NOT exist!", "soundName");
        }

        // 6/11/2012
        /// <summary>
        /// Removes the <see cref="Sounds"/> associated by the given <paramref name="soundName"/>.
        /// </summary>
        /// <param name="soundName">Unique name to reference sound by.</param>
        public static void RemoveAudio(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
                throw new ArgumentNullException("soundName");

            SoundNameStruct soundNameStruct;
            if (SoundNames.TryGetValue(soundName, out soundNameStruct))
            {
                // Remove from AudioManager
                AudioManager.Remove(soundNameStruct.UniqueKey, soundNameStruct.Sound);

                // Remove from SoundNames Dictionary
                SoundNames.Remove(soundName);

                return;
            }

            throw new ArgumentException("Sound name given does NOT exist!", "soundName");
        }

        // 6/10/2012
        /// <summary>
        /// Queues up <see cref="Sounds"/> to play, attached to the given <paramref name="sceneItemName"/>.
        /// </summary>
        /// <param name="sceneItemName">Named <see cref="SceneItem"/> to stop (Only Scenary SceneItems allowed)</param>
        /// <param name="soundToPlay"><see cref="Sounds"/> to play</param>
        public static void Play3DAudioAtSceneItem(string sceneItemName, Sounds soundToPlay)
        {
            // try get 'Named' sceneItem from Player class
            int instancedItemPickedIndex;
            var namedSceneItem = ScriptingHelper.GetNamedItem(sceneItemName, out instancedItemPickedIndex);

            // check if this is a PlayableItem, which is not allowed for this 1st param!
            SceneItem.DoCheckIfSceneItemIsPlayableItemType(namedSceneItem);

            // cast SceneItem to ScenaryItemScene
            var scenaryItemScene = (namedSceneItem as ScenaryItemScene);

            if (scenaryItemScene == null)
                throw new InvalidOperationException("Cast of SceneItem to ScenaryItemScene failed!");

            // Add sound to play.
            scenaryItemScene.Play3DAudio(soundToPlay);
        }


        #endregion

        // 6/6/2012
        /// <summary>
        /// Used to unload resources during level loads.
        /// </summary>
        public static void UnloadContent()
        {
           if (IFDTileByNames != null)
               IFDTileByNames.Clear();

            if (IFDTileKeytoNames != null)
                IFDTileKeytoNames.Clear();

            if (SoundNames != null)
                SoundNames.Clear();
        }
    }
}
