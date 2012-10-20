#region File Description
//-----------------------------------------------------------------------------
// BuildingScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.Audio;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.IFDTiles.Structs;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.MemoryPool.PoolItems;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.Particles.Enums;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar;
using ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.SceneItems"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.SceneItems"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }


    ///<summary>
    /// The <see cref="BuildingScene"/> is used for static structures, which do not move and can produce
    /// other <see cref="SceneItem"/>.
    ///</summary>
    public sealed class BuildingScene : SceneItemWithPick, IFOWPlaceableItem
    {
        // 11/7/2009
        private Player _thisPlayer;

        // 3/2/2009 - Supply Dropoff Position - used for revenue buildings supply depots.
        private Vector3 _supplyDropPosition = Vector3Zero;
        private Vector3 _supplyDropPadPosition = Vector3Zero; // 7/14/2009
        private const float GunShipHeight = 1000;
        private bool _gunShipDescending = true;
        private SciFiAircraftScene _gunShipForSupplyDrops;

        // 3/26/2009 -
        //             Note: 
        ///<summary>
        ///  Occurs when a special <see cref="BuildingScene"/> was placed; for example,
        ///  a 'Research Center' technology building, which when placed would unlock other
        ///  features in the game!  This event is only attached to some <see cref="BuildingScene"/> instance, when
        ///  the 'PlayableAtts' flag 'SpecialBuilding' is set to true, which is checked within
        ///  the <see cref="CommonInitilization"/> method internally.
        ///</summary>
        /// <remarks>
        /// The primary reason this was created was to capture this event within the
        /// <see cref="IFDTile"/> section, and activate some tile for use, like a special tank, for example.
        /// </remarks>
        public static event EventHandler SpecialBuildingCreated;     
  
        // 5/18/2009
        ///<summary>
        /// Occurs when a special <see cref="BuildingScene"/> was destroyed; for example,
        ///  a 'Research Center' technology building.
        ///</summary>
        public static event EventHandler SpecialBuildingDestroyed;

        // 12/9/2008 - Position of damage fire particles, when health falls below 50%
        private Vector3 _damageFirePosition = Vector3Zero; 

        // 1/6/2008 - Smoke Stack Spawn points for SciFi-7_set2.
        private bool _spawnSmokeBonesSetup;
        private Vector3 _spawnSmoke1, _spawnSmoke2;
        private Vector3 _smokeStackVelocity1 = Vector3.Up;
        private Vector3 _smokeStackVelocity2 = Vector3.Up;

        // 3/10/2009 - Init Marker Position Setup completed.
        private bool _initMarkerPositionSetup;

        // 2/24/2009 - 
        ///<summary>
        /// Reference to <see cref="BuildingScenePoolItem"/> wrapper class.
        ///</summary>
        public new BuildingScenePoolItem PoolItemWrapper;

        // 4/30/2009 -
        ///<summary>
        /// Save reference to the <see cref="IFDTile"/> <see cref="SubQueueKey"/>, needed to display the
        /// building's group of <see cref="IFDTile"/>.
        ///</summary>
        public SubQueueKey SubQueueKeyIFDTiles;

        #region Properties

        // 11/11/2008 - Override Delete so we can also delete Transform from InstanceItem.
        /// <summary>
        /// Should this <see cref="BuildingScene"/> be deleted?
        /// </summary>
        public override bool Delete
        {
            get
            {
                return base.Delete;
            }
            set
            {
                // Delete Transform for InstanceItem, if True
                if (value)
                {
                    // 1/18/2011
                    if (ShapeItem != null)
                        InstancedItem.RemoveInstanceTransform(ref ((Shape) ShapeItem).InstancedItemData);
                }

                base.Delete = value;
            }
        }

        // 2/4/2009
        ///<summary>
        /// Returns the current spawn offset, stored in the <see cref="SceneItemWithPick.PlayableItemAtts"/> structure.
        ///</summary>
        public Vector3 ItemSpawnOffset
        {
            get
            {
                return PlayableItemAtts.ItemSpawnOffset;
            }
        }

        // 2/5/2009
        ///<summary>
        /// Set or Get reference to the <see cref="BuildingShape"/> instance.
        ///</summary>
        public new BuildingShape ShapeItem
        {
            get
            {
                return (base.ShapeItem as BuildingShape);
            }
            set
            {
                ShapeItem = value;
            }
           
        }

        // 8/3/2009
        private float _repairRadius = 800.0f;
        ///<summary>
        /// The radius around this <see cref="BuildingScene"/> that some <see cref="SceneItem"/>
        /// must be in for a repair to occur.
        ///</summary>
        public float RepairRadius
        {
            get
            {
                return _repairRadius;
            }
            set
            {
                _repairRadius = value;
            }
        }

        // 10/9/2009
        /// <summary>
        /// The <see cref="ItemGroupType"/> this <see cref="BuildingScene"/> can produce; 
        /// for example, a War-Factory might produce <see cref="SciFiTankScene"/> items.
        /// </summary>
        public ItemGroupType? ProductionType { get; set; }
        

        #endregion

        ///<summary>
        /// Constructor, which initializes a new <see cref="BuildingScene"/>.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="itemType"><see cref="ItemType"/> to use</param>
        ///<param name="initialPosition"><see cref="Vector3"/> position to place item</param>
        ///<param name="playerNumber">The <see cref="Player"/> number this item belongs to</param>
        public BuildingScene(Game game, ItemType itemType, ref Vector3 initialPosition, byte playerNumber)
            : base(game, new BuildingShape(game, itemType, playerNumber), ref initialPosition, playerNumber)
        {
            
            // 11/12/2008 - Set ItemMovable to False
            ItemMoveable = false;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // 11/11/2009 - Create ScreenTextItem for revenue text animation.
            ScreenTextManager.AddNewScreenTextItem(string.Empty, Vector2.Zero, Color.Green, out _revenueTextItem);
            _revenueTextItem.Font = game.Content.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\Arial18");
           
        }

        // 1/30/2009; 3/24/2009: Updated with 2nd param; finalPosition.
        /// <summary>
        /// Populates the <see cref="SceneItemWithPick.PlayableItemAtts"/> structure with the common attributes
        /// used by the given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="e">The <see cref="ItemCreatedArgs"/> attributes</param>
        /// <param name="isFinalPosition">When item is first being placed, the <see cref="IFDTile"/> passes the current position
        /// the item is at, while user moves item around game world; however, until the player actually places 
        /// the item down, the position is not considered 'Final'.</param>
        public override void LoadPlayableAttributesForItem(ItemCreatedArgs e, bool isFinalPosition)
        {           
            base.LoadPlayableAttributesForItem(e, isFinalPosition);

            // 2/24/2009
            CommonInitilization(GameInstance, e.PlaceItemAt);

#if !XBOX360
            // 6/22/2009 - Set 'PhysX' CLOTH for Building-1, 10, 11 flag.
            if (e.ItemType == ItemType.sciFiBldb01 || e.ItemType == ItemType.sciFiBldb10
                || e.ItemType == ItemType.sciFiBldb11 || e.ItemType == ItemType.sciFiBldb13)
            {
                //InstancedItem.SetPhysXClothForBoneTransform(ref ((Shape) ShapeItem).InstancedItemData, "Flag");
            }
#endif
        }

        // 4/15/2009
        /// <summary>
        /// When item is placed in final position, this method creates the <see cref="IStatusBar"/>.
        /// </summary>
        protected override void CreateStatusBar()
        {
            // If SceneItemOwner placed in final Position, then allow creation of statusbar!
            if (ItemPlacedInFinalPosition)
                base.CreateStatusBar();
        }

        /// <summary>
        /// The <see cref="CommonInitilization"/> method sets internal tweakable flags
        /// back to there defaults, retrieves the current rotation value, updates the proper
        /// <see cref="IFogOfWar"/> settings if required, and obtains the current <see cref="TWEngine.Terrain"/>
        /// height for the given position.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="initialPosition"><see cref="Vector3"/> position to place item</param>
        private void CommonInitilization(Game game, Vector3 initialPosition)
        {          

            // 2/24/2009 - Reset flags correctly
            IsAlive = true;
            Delete = false;
            _spawnSmokeBonesSetup = false;
            ShapeItem.ExplodeAnimStarted = false;
            ThePickSelected = false; // 7/12/2009

            // 6/15/2010 - Updated to use new GetPlayer method.
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);

            // 10/13/2012 - Obsolete.
            // 3/28/2009 - Tell InstanceModel to draw using Normal pieces, and not explosion pieces!
            //InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData, PlayerNumber, false);

            // 3/26/2009 - Is this building marked as a 'SpecialTechBuilding'?
            if (PlayableItemAtts.IsSpecialEnablerBuilding && ItemPlacedInFinalPosition)
            {
                // then trigger event
                if (SpecialBuildingCreated != null)
                    SpecialBuildingCreated(this, EventArgs.Empty);
                
            }            

            // 1/1/2009 - Get the default Scale value, contain in the ItemType's content pipeline file.
            float useScale;
            InstancedItem.GetScale(ref ((Shape) ShapeItem).InstancedItemData, out useScale);
            scale = Vector3Zero;
            scale.X = scale.Y = scale.Z = useScale;

            // 12/9/2008 - Get the default Rotation values, contain in the ItemType's content pipeline file.
            float rotX, rotY, rotZ;
            InstancedItem.GetRotationX(ref ((Shape) ShapeItem).InstancedItemData, out rotX);
            InstancedItem.GetRotationY(ref ((Shape) ShapeItem).InstancedItemData, out rotY);
            InstancedItem.GetRotationZ(ref ((Shape) ShapeItem).InstancedItemData, out rotZ);
            Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(rotY), MathHelper.ToRadians(rotX), MathHelper.ToRadians(rotZ), out rotation);

            // Set FogOfWar Radius, based on SceneItem ViewRadius Property
            if (ShapeItem.UseFogOfWar)
            {
                ShapeItem.FogOfWarHeight = (int)ViewRadius / TerrainData.cScale;
                ShapeItem.FogOfWarWidth = (int)ViewRadius / TerrainData.cScale;
                
                // 1/14/2009 - Make sure it can be seen immediately!
                ShapeItem.IsFOWVisible = true;

                var fogOfWar = (IFogOfWar)game.Services.GetService(typeof(IFogOfWar));
                if (fogOfWar != null) fogOfWar.UpdateSight = true;
            }

            // Retrieve the ITerrainShape Interface
            TerrainShape = (ITerrainShape)game.Services.GetService(typeof(ITerrainShape));

            // Set to approriate height on map.
            if (TerrainData.IsOnHeightmap(initialPosition.X, initialPosition.Z))
            {
                initialPosition.Y = TerrainData.GetTerrainHeight(initialPosition.X, initialPosition.Z);
                Position = initialPosition;
            }

            // 3/10/2009 - Marker Position setup
            _initMarkerPositionSetup = false;

            // 12/9/2008 - Add Particles Class

            // 1/15/2009 - Tell Minimap to update the unit positions.
            var miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap)); // 1/2/2010
            if (miniMap != null) miniMap.DoUpdateMiniMap = true;
        }


        /// <summary>
        /// Updates the current <see cref="BuildingScene"/>, by initially calling the base.  Also
        /// handles updates for network games, and calls the <see cref="GeneratesRevenueCheck"/> method.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time">The <see cref="TimeSpan"/> structure as game time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public override void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {
            base.Update(gameTime, ref time, ref elapsedTime, isClientCall);

            try // 4/26/2010
            {
                // 1/30/2009 - Make sure isAlive first.
                if (!IsAlive) return;

                // 3/10/2009 - Init Marker Position check
                if (!_initMarkerPositionSetup)
                    InitialMarkerPositionSetup();

                // Is Network Game?
                var thisPlayer = _thisPlayer; // 4/26/2010
                if (thisPlayer.NetworkSession != null)
                {
                    // 12/8/2008 - HandleInput - (MP games only check their own player)
                    if (TemporalWars3DEngine.SThisPlayer == PlayerNumber || thisPlayer.PlayerNumber == PlayerNumber)
                    {
                        HandleInput.BuildingSceneInputCheck(this);
                        // 1/5/2009
                        GeneratesRevenueCheck(this, ref elapsedTime);
                    }
                }
                else
                {
                    HandleInput.BuildingSceneInputCheck(this);
                    // 1/5/2009
                    GeneratesRevenueCheck(this, ref elapsedTime);
                }  
            }
            // 4/26/2010 - Capture the NullRef exception, and check if '_thisPlayer' is null; is true, then
            // update with reference from player class. This avoid having to check if '_thisPlayer' is null ever call cycle!
            catch (NullReferenceException)
            {
                Debug.WriteLine("Update method, in 'BuildingScene' class, threw the NullReferenceExp error.", "NullReferenceException");

                // 11/7/2009 - Check Null
                if (_thisPlayer == null)
                {
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out _thisPlayer);

                    Debug.WriteLine("The '_thisPlayer' var was null; however, retrieved new instance from the TWEngine.SPlayers collection.", "NullReferenceException");
                }
            } // End Try-Catch
            
        }    
       
       
        // 3/10/2009
        /// <summary>
        /// Checks if a <see cref="BuildingScene"/> has the 'ItemMarkerOffset' set, and if so,
        /// will set the initial marker flag position.
        /// </summary>
        private void InitialMarkerPositionSetup()
        {
            // 3/10/2009 - Does building have Marker to place.
            if (!PlayableItemAtts.ItemMarkerOffset.Equals(Vector3Zero))
            {
                // set Marker Position
                Vector3 markerPosition;
                Vector3.Add(ref position, ref PlayableItemAtts.ItemMarkerOffset, out markerPosition);
                ShapeItem.SetMarkerPosition(ref position, ref markerPosition, NetworkItemNumber);

            }

            _initMarkerPositionSetup = true;
        }

        // 11/16/2009 - Revenue Time to next deposit, for Revenue buildings.
        private int _revenueAdditionTimer = 10000; // 11/16/09 - 10 sec default.

        // 5/20/2010: Updated to be a STATIC method.
        // 1/5/2009; 3/11/2009: Updated to use Stopwatch class.
        /// <summary>
        /// Checks if current <see cref="BuildingScene"/> generates revenue.  If it
        /// does, then a deposit is made every 'TimeToNextDeposit' into the player's 'Cash' card.
        /// </summary>
        /// <param name="buildingScene">this instance of <see cref="BuildingScene"/></param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> instance</param>
        private static void GeneratesRevenueCheck(BuildingScene buildingScene, ref TimeSpan elapsedTime)
        {
            // 3/24/2009 - Skip if SceneItemOwner not placed in final Position yet.
            if (!buildingScene.ItemPlacedInFinalPosition) return;

            // 1/5/2009 - Does Building Type generate revenue?
            if (!buildingScene.PlayableItemAtts.GeneratesRevenue) return;

            // 11/11/2009 - Do Revenue Text Animation
            DoRevenueTextAnimation(buildingScene, ref elapsedTime);

            // 3/2/2009 - Does building require a Supply Ship?
            if (buildingScene.PlayableItemAtts.RevenueComesFromSupplyShip)
                DoSupplyDropAnimation(buildingScene, ref elapsedTime); // yes, then show ship landing with supplies

            // is it Time to add next deposit?
            buildingScene._revenueAdditionTimer -= elapsedTime.Milliseconds; // 11/16/09
            if (buildingScene._revenueAdditionTimer > 0)
                return;

            // Reset Timer
            buildingScene._revenueAdditionTimer = buildingScene.PlayableItemAtts.TimeToNextDeposit * 1000; // 11/16/2009
           
            // 11/11/09 - Start Text animation
            buildingScene._startRevenueTextAnimation = true;
            

            // Increase cash by deposit amount; if energyOff, then increase by reduced amount.
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(buildingScene.PlayerNumber, out player);

            if (player == null) return;

            if (player.EnergyOff)
            {
                // 11/11/09 - Updated the Revenue Text value.
                buildingScene._revenueTextItem.SbDrawText.Length = 0;
                buildingScene._revenueTextItem.SbDrawText.Append(buildingScene.PlayableItemAtts.ReducedDepositAmount);
                player.Cash += buildingScene.PlayableItemAtts.ReducedDepositAmount;
            }
            else
            {
                // 11/11/09 - Updated the Revenue Text value.
                buildingScene._revenueTextItem.SbDrawText.Length = 0;
                buildingScene._revenueTextItem.SbDrawText.Append(buildingScene.PlayableItemAtts.DepositAmount);
                player.Cash += buildingScene.PlayableItemAtts.DepositAmount;
            }
        }

        // 11/11/2009 - ScreenTextItem Revenue Animation vars
        private bool _startRevenueTextAnimation;
        private const int RevenueTimerStartAt = 3000; // set as 3 secs
        private int _revenueTextAnimationTimer = RevenueTimerStartAt;
        private ScreenTextItem _revenueTextItem;
        private float _scrollHeightOffset;

        // 11/11/2009; // 5/20/2010: Updated to be a STATIC method.
        /// <summary>
        /// For each Revenue deposit, the amount will be scrolled up 
        /// the screen, over the current building.
        /// </summary>
        /// <param name="buildingScene">this instance of <see cref="BuildingScene"/></param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure</param>
        private static void DoRevenueTextAnimation(BuildingScene buildingScene, ref TimeSpan elapsedTime)
        {
            if (!buildingScene._startRevenueTextAnimation)
                return;

            // do timer countdown
            buildingScene._revenueTextAnimationTimer -= elapsedTime.Milliseconds;
            if (buildingScene._revenueTextAnimationTimer <= 0)
            {
                // then animation done, so stop.
                buildingScene._scrollHeightOffset = 0;
                buildingScene._revenueTextItem.Visible = false;
                buildingScene._startRevenueTextAnimation = false;
                buildingScene._revenueTextAnimationTimer = RevenueTimerStartAt;
                return;
            }

            // else, do text animation
            buildingScene._revenueTextItem.Visible = true;

            // Update Text position to scroll up.
            buildingScene._revenueTextItem.DrawLocationFrom3D = buildingScene.Position;
            var currentPosition = buildingScene._revenueTextItem.DrawLocation;
            // increase height of text.
            buildingScene._scrollHeightOffset -= 25.0f * (float)elapsedTime.TotalSeconds;
            currentPosition.Y += buildingScene._scrollHeightOffset;
            buildingScene._revenueTextItem.DrawLocation = currentPosition;

        }

        // 5/6/2009 - SupplyShipDelayBeforeTakeoff
        TimeSpan _supplyShipDelayBeforeTakeOff = TimeSpan.FromSeconds(4);

        // 3/2/2009; 5/20/2010: Updated to be STATIC method.
        /// <summary>
        /// Does the supply drop animation, using the GunShip model.  The
        /// animation simply decends the gunship down to the base, and then
        /// it go backs up off the screen.
        /// </summary>
        /// <param name="buildingScene">this instance of <see cref="BuildingScene"/></param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> as elapsed time</param>
        private static void DoSupplyDropAnimation(BuildingScene buildingScene, ref TimeSpan elapsedTime)
        {
                      
            // 1st - make sure we have a gunship to use.
            if (buildingScene._gunShipForSupplyDrops == null)
                buildingScene.CreateSupplyDropGunShip();

            // 2nd - move to supply drop off Position
            if (buildingScene._supplyDropPosition.Y > buildingScene._supplyDropPadPosition.Y && buildingScene._gunShipDescending)
            {
                //
                // reduce height of gunship
                //

                buildingScene._supplyDropPosition.Y -= 105.0f * (float)elapsedTime.TotalSeconds;
                if (buildingScene._gunShipForSupplyDrops != null) buildingScene._gunShipForSupplyDrops.Position = buildingScene._supplyDropPosition;
            }
            else if (buildingScene._supplyDropPosition.Y <= buildingScene._supplyDropPadPosition.Y && buildingScene._gunShipDescending)
            {
                // 11/11/09 - Note: Revenue was here; but now it is not tied to animation anymore!
                
                // Set to ship to leave
                buildingScene._gunShipDescending = false;               

            }
            else if (buildingScene._supplyShipDelayBeforeTakeOff > TimeSpan.Zero)
            {
                // 5/6/2009 - Let's have ship wait a few seconds, before leaving!
                buildingScene._supplyShipDelayBeforeTakeOff -= elapsedTime;

                // Lock Position onto pad; otherwise will drift!
                buildingScene._supplyDropPosition.Y = buildingScene._supplyDropPadPosition.Y;
                if (buildingScene._gunShipForSupplyDrops != null) buildingScene._gunShipForSupplyDrops.Position = buildingScene._supplyDropPosition;
            }
            else
            {
                // 
                // increase height of gunship
                //

                if (buildingScene._supplyDropPosition.Y < GunShipHeight)
                {
                    buildingScene._supplyDropPosition.Y += 135.0f * (float)elapsedTime.TotalSeconds;
                    if (buildingScene._gunShipForSupplyDrops != null) buildingScene._gunShipForSupplyDrops.Position = buildingScene._supplyDropPosition;
                }
                else
                {
                    buildingScene._supplyDropPosition.Y = GunShipHeight;
                    if (buildingScene._gunShipForSupplyDrops != null) buildingScene._gunShipForSupplyDrops.Position = buildingScene._supplyDropPosition;
                    buildingScene._gunShipDescending = true;

                    // Reset Timer  
                    buildingScene._revenueAdditionTimer = buildingScene.PlayableItemAtts.TimeToNextDeposit * 1000; // 11/16/09

                    // Reset timer
                    buildingScene._supplyShipDelayBeforeTakeOff = TimeSpan.FromSeconds(4);
                }

            }
            
        }

        // 8/14/2009 - MP games require a Unique number.
        private static int _supplyDropShipNetworkItemNumber = 99990;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;
        

        // 3/2/2009 - Creates the GunShip used for the supply drops.
        /// <summary>
        /// Creates the GunShip model, both for SP & MP games.
        /// </summary>
        private void CreateSupplyDropGunShip()
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(ShapeItem.PlayerNumber, out player);

            if (player == null) return;

            // 1st - Get ItemToPosition1 for supply drop spot
            Matrix tmpMatrix;
            InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape) ShapeItem).InstancedItemData, "ItemToPosition1", out tmpMatrix);
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref tmpMatrix, out _supplyDropPosition);

            // 7/14/2009 - Set SupplyDropPadPosition, for landing
            _supplyDropPadPosition = _supplyDropPosition;
            _supplyDropPosition.Y = TerrainData.GetTerrainHeight(_supplyDropPadPosition.X, _supplyDropPadPosition.Z);
            _supplyDropPadPosition.Y += 25;

            // Set Position to be higher than ItemToPosition, using 'GunShipHeight'.
            _supplyDropPosition.Y += GunShipHeight;

            // 2nd - Create Gunship (sciFiGunShip1)
            SciFiAircraftScenePoolItem poolItem;
            player.PoolManager.GetNode(out poolItem);
            _gunShipForSupplyDrops = (SciFiAircraftScene) poolItem.SceneItemInstance;
            _gunShipForSupplyDrops.ShapeItem.SetInstancedItemTypeToUse(ItemType.sciFiGunShip01);
            _gunShipForSupplyDrops.LoadPlayableAttributesForItem(new ItemCreatedArgs { ItemType = ItemType.sciFiGunShip01, PlaceItemAt = _supplyDropPosition }, false);
            _gunShipForSupplyDrops.Position = _supplyDropPosition;
            _gunShipForSupplyDrops.ShapeItem.PlayerNumber = ShapeItem.PlayerNumber;
            _gunShipForSupplyDrops.PlayerNumber = ShapeItem.PlayerNumber;
            _gunShipForSupplyDrops.NetworkItemNumber = _supplyDropShipNetworkItemNumber++;
            
           
            // 7/14/2009
            // Add to the Selectable Items
            Player.AddSelectableItem(player, _gunShipForSupplyDrops, true);

            // 2nd - Create Gunship (sciFiGunShip1)
            /*Spacewar.IFDTiles.ItemCreatedArgs itemCreatedArgs = new Spacewar.IFDTiles.ItemCreatedArgs(ItemGroupType.Airplanes, null, ItemType.sciFiGunShip2, null, _supplyDropPosition, 0, null, 0);
            
            // Is Network Game?
            if (ImageNexusRTSGameEngine.Players[ImageNexusRTSGameEngine._thisPlayer].NetworkSession == null)
            {
                // No, SP Game

                // Add GunShip
                SciFiAircraftScenePoolItem poolItem;
                ImageNexusRTSGameEngine.Players[ShapeItem.PlayerNumber].PoolManager.GetNode(out poolItem);
                _gunShipForSupplyDrops = poolItem.SciFiAircraftSceneItem;
               

                _gunShipForSupplyDrops.ShapeItem.SetInstancedItemTypeToUse(itemCreatedArgs.ItemType);
                _gunShipForSupplyDrops.LoadPlayableAttributesForItem(itemCreatedArgs.ItemType, ref _supplyDropPosition);
                _gunShipForSupplyDrops.Position = _supplyDropPosition;
                _gunShipForSupplyDrops.NetworkItemNumber = 0;

                // Add to the Selectable Items                    
                ImageNexusRTSGameEngine.Players[ShapeItem.PlayerNumber].AddSelectableItem(_gunShipForSupplyDrops);

            }
            else // Yes, MP game.
            {

                // Add Host's GunShip                  
                
                SciFiAircraftScenePoolItem poolItem;
                ImageNexusRTSGameEngine.Players[ShapeItem.PlayerNumber].PoolManager.GetNode(out poolItem);
                _gunShipForSupplyDrops = poolItem.SciFiAircraftSceneItem;


                _gunShipForSupplyDrops.ShapeItem.SetInstancedItemTypeToUse(itemCreatedArgs.ItemType);
                _gunShipForSupplyDrops.LoadPlayableAttributesForItem(itemCreatedArgs.ItemType, ref _supplyDropPosition);
                _gunShipForSupplyDrops.Position = _supplyDropPosition;
                _gunShipForSupplyDrops.ShapeItem.PlayerNumber = ShapeItem.PlayerNumber;
                _gunShipForSupplyDrops.PlayerNumber = ShapeItem.PlayerNumber;
                _gunShipForSupplyDrops.NetworkItemNumber = 0; // since not shared with other player, leave as 0.
                

                // Add to the Selectable Items diretly, since not sharing with other MP player!                  
                //ImageNexusRTSGameEngine.Players[ShapeItem.PlayerNumber].AddSelectableItem(_gunShipForSupplyDrops);
                ImageNexusRTSGameEngine.Players[ShapeItem.PlayerNumber].SelectableItems.Add(_gunShipForSupplyDrops);


            }// End If network game*/
            

        }       

        // 9/23/2008 - Called when an SceneItemOwner is placed on the screen, via the IFD Display.       
        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="TWEngine.Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called in order to do specific <see cref="SceneItem"/> placement checks; for example, if the <see cref="SceneItem"/>
        /// requires A* blocking updated.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <returns>true/false of result</returns>
        public override bool RunPlacementCheck(ref Vector3 placementPosition)
        {
            // 1/13/2010
            if (TemporalWars3DEngine.AStarGraph == null)
                return true;

            // 1/5/2009 - Check if SceneItemOwner can be placed at the current location, given the 'PathBlockSize'.            
            return (!TemporalWars3DEngine.AStarGraph.IsPathNodeSectionBlocked((int)placementPosition.X, (int)placementPosition.Z, ShapeItem.PathBlockSize, BlockedType.Any));
        }

        // 6/8/2009  - Called when an SceneItemOwner is placed on the screen, via the IFD Display. 
        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="TWEngine.Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called to check if the x/y values given, are within this sceneItem's <paramref name="pathBlockSize"/> zone.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="pathBlockSize">The scene items <paramref name="pathBlockSize"/>.</param>
        /// <returns>true/false of result</returns>
        public override bool IsInPlacementZone(ref Vector3 placementPosition, int x, int y, int pathBlockSize)
        {
            // call base version, passing in the 'pathBlockSize' from the ShapeItem.
            return base.IsInPlacementZone(ref placementPosition, x, y, ShapeItem.PathBlockSize);
        }

        // 3/2/2009 - Once SceneItemOwner is placed on terrain, this should be called to set the PathBlockSize of this SceneItemOwner
        //            into the AStarGraph.
        /// <summary>
        /// Once a <see cref="SceneItem"/> is placed on the <see cref="TWEngine.Terrain"/>, this method is called
        /// in order to set its placement in the AStarGraph component, using the PathBlockSize.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        public override void SetPlacement(ref Vector3 placementPosition)
        {
            // Get Items Path Blocking Size and set in AStarGraph.
            if (!ShapeItem.IsPathBlocked) return;

            // 6/26/2012 - Call base
            base.SetPlacement(ref placementPosition);

            //  5/18/2009 - Updated to set the cost to a value of -2, rather than -1, which is reserved for 'Blocked'
            //              sections of the map!  The difference is important, because it affects how the Cursor 'Blocked'
            //              image is displayed, which is only done when it reads the -1 cost value!
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
            if (aStarGraph != null) // 1/13/2010
                aStarGraph.SetCostToPos((int)placementPosition.X, (int)placementPosition.Z, -2, ShapeItem.PathBlockSize);

            global::ImageNexus.BenScharbach.TWEngine.Terrain.TerrainShape.PopulatePathNodesArray();
        }

        // 10/16/2008
        /// <summary>
        /// This overrides the base method by setting the <see cref="SceneItem.CollisionRadius"/>
        /// using the <see cref="InstancedItem"/> model <see cref="BoundingSphere"/>, rather than the
        /// XNA model.
        /// </summary>
        protected override void SetCollisionRadius()
        {
            CollisionRadius = InstancedItem.GetInstanceItemCollisionRadius(ref ((Shape) ShapeItem).InstancedItemData, PlayableItemAtts.AdjustCollisionRadius);            
           
        }

        // 12/9/2008
        /// <summary>
        /// Removes <see cref="SceneItem"/> from Game, while calling some random death animation.
        /// </summary>
        /// <param name="elapsedTime">The <see cref="TimeSpan"/> structure</param>
        /// <param name="attackerPlayerNumber">The attacker's player number</param>
        public override void StartKillSceneItem(ref TimeSpan elapsedTime, int attackerPlayerNumber)
        {
            // Added 'KillSceneItemCalled' check to make sure code is not executed twice, since during MP games,
            // the Server will make sure client kills the unit by calling this too!
            if (KillSceneItemCalled) return;

            // 5/20/2010: Moved to top, rather than bottom of method call to have 'IsAlive' applied first!
            base.StartKillSceneItem(ref elapsedTime, attackerPlayerNumber);

            // 5/20/2010 - Turn off display of _revenueTextItem, otherwise will remain on screen when building gone!
            _revenueTextItem.Visible = false;
            
            // Remove AStar Costs at our Position          
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/26/2010
            if (aStarGraph != null) // 1/13/2010
                aStarGraph.RemoveCostAtPos((int)position.X, (int)position.Z, ShapeItem.PathBlockSize);

            // 10/13/2012 - Obsolete.
            // 3/28/2009 - Tell InstanceModel to draw using Explosion Pieces!
            //InstancedItem.UpdateInstanceModelToDrawExplosionPieces(ref ((Shape) ShapeItem).InstancedItemData,PlayerNumber, true);

            // 10/13/2012 - Draw Explosion smoke
            var currentPosition = ShapeItem.World.Translation;
            var lastProjectileVelocity = ShapeItem.LastProjectileVelocity;
            ParticlesManager.DoParticles_LargeExplosion(ref currentPosition, ref lastProjectileVelocity);

            // 4/3/2009 - Check if SupplyShip exits?
            if (PlayableItemAtts.RevenueComesFromSupplyShip)
            {
                if (_gunShipForSupplyDrops != null)
                {
                    // remove gunship from selectableitems
                    
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    Player player;
                    TemporalWars3DEngine.GetPlayer(ShapeItem.PlayerNumber, out player);

                    Player.RemoveSelectableItem(player, _gunShipForSupplyDrops);

                    // 6/30/2009 - Return Gunship to PoolManager
                    _gunShipForSupplyDrops.ReturnItemToPool(false);

                    // release gunship
                    _gunShipForSupplyDrops = null;
                }
            }

            // 5/18/2009 - if SpecialBuilding, then trigger Event.  This is used to Turn OFF Tiles in IFD.
            if (PlayableItemAtts.IsSpecialEnablerBuilding && SpecialBuildingDestroyed != null)
                SpecialBuildingDestroyed(this, EventArgs.Empty);
        }

        // 2/23/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting 'Active' to false.
        /// </summary>
        /// <param name="isInterfaceDisplayNode">Was called from <see cref="IFDTile"/></param>
        public override void ReturnItemToPool(bool isInterfaceDisplayNode)
        {
            // 6/29/2009
            if (PoolItemWrapper != null && PoolItemWrapper.PoolNode != null) 
                PoolItemWrapper.PoolNode.ReturnToPool();
            else
                Debug.WriteLine("(ReturnItemToPool) failed to return pool node.");
        }


        // 12/9/2008
        /// <summary>
        /// When <see cref="BuildingScene"/> health falls below 50%, this method will draw the smoke <see cref="ParticleSystem"/>
        /// on the <see cref="BuildingScene"/>.
        /// </summary>
        /// <param name="elapsedTime">The <see cref="TimeSpan"/> structure</param>
        protected override void Below50HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            var tmpZero = Vector3Zero;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlackSmokePlumeParticleSystem, ref position, ref tmpZero);
        }

        // 12/9/2008
        /// <summary>
        /// When <see cref="BuildingScene"/> health falls below 25%, this will draw the fire <see cref="ParticleSystem"/>
        /// on the <see cref="BuildingScene"/>.
        /// </summary>
        /// <param name="elapsedTime">The <see cref="TimeSpan"/> structure</param>
        protected override void Below25HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // 1/14/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            // Set Position of damage fire
            _damageFirePosition = position; _damageFirePosition.Y += 25;
            var tmpZero = Vector3Zero;
            ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.FireParticleSystem, ref _damageFirePosition, ref tmpZero);
        }


        // 5/18/2009
        /// <summary>
        /// Triggers the event for 'SceneItemCreated', passing 'This' as the <see cref="BuildingScene"/> instance.
        /// </summary>
        /// <param name="item">The <see cref="SceneItemWithPick"/> instance</param>
        protected override void FireEventHandler_Created(SceneItemWithPick item)
        {
            // TODO: What was I doing here?  It was never finsihed!
            // 5/18/2009 - Check if BuildingType created, is the 'War Factory', to  see if 
            //             if the 'Tech' center is already created!  (The 'Enabler' for this SceneItemOwner).
            if (item is BuildingScene)
            {
                
            }

            base.FireEventHandler_Created(this);
        }
       
       
        // 4/10/2009
        /// <summary>
        /// Processes <see cref="ParticleSystem"/> for the current <see cref="BuildingScene"/>.
        /// </summary>
        protected override void DoParticlesCheck()
        {
            // 1/15/2009: Updated to show only when in FOW view and in Camera view!
            if (!ShapeItem.IsFOWVisible || !InstancedItem.IsInCameraView(ref ((Shape) ShapeItem).InstancedItemData))
                return;

            // TODO: Add this as attribute!
            // If Scifi-Bld7 / SciFi-Blda8 / SciFi-BldB11, do Smoke Stack particle effect.
            if ((ShapeItem.ItemType != ItemType.sciFiBldb07 && ShapeItem.ItemType != ItemType.sciFiBlda08) &&
                ShapeItem.ItemType != ItemType.sciFiBldb11) return;

            if (!_spawnSmokeBonesSetup)
                InitBuildingAnimation();
            else
            {
                // Smoke Stacks
                ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlueSmokePlumeParticleSystem, ref _spawnSmoke1, ref _smokeStackVelocity1);
                ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.BlueSmokePlumeParticleSystem, ref _spawnSmoke2, ref _smokeStackVelocity2);

                // Torch in Stacks
                ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.TorchParticleSystem, ref _spawnSmoke1, ref _smokeStackVelocity1);
                ParticlesManager.UpdateParticleSystem(ParticleSystemTypes.TorchParticleSystem, ref _spawnSmoke2, ref _smokeStackVelocity2);
            }
        }

        // 2/24/2009
        /// <summary>
        /// Initializes the <see cref="BuildingScene"/> animation transforms, which are used in the <see cref="ParticleSystem"/> update calls.
        /// </summary>
        private void InitBuildingAnimation()
        {
            // TODO: Add this as attribute!
            // If Scifi-Bld7, do Smoke Stack particle effect.
            if ((ShapeItem.ItemType != ItemType.sciFiBldb07 && ShapeItem.ItemType != ItemType.sciFiBlda08) &&
                ShapeItem.ItemType != ItemType.sciFiBldb11) return;

            // Get SpawnSmoke-1
            Matrix tmpMatrix; _spawnSmoke1 = _spawnSmoke2 = Vector3Zero; 
            InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape) ShapeItem).InstancedItemData, "SpawnSmoke1", out tmpMatrix);
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref tmpMatrix, out _spawnSmoke1);

            // Get SpawnSmoke-2
            InstancedItem.GetInstanceItemAbsoluteBoneTransform(ref ((Shape) ShapeItem).InstancedItemData, "SpawnSmoke2", out tmpMatrix);
            InstancedItem.GetWorldPositionFromBoneTransform(this, ref tmpMatrix, out _spawnSmoke2);

            _spawnSmokeBonesSetup = !_spawnSmoke1.Equals(Vector3Zero) && !_spawnSmoke2.Equals(Vector3Zero);
        }       

        #region Audio Methods

        // 5/5/2009
        /// <summary>
        /// Plays some pick selected sound when a <see cref="BuildingScene"/> is selected.
        /// </summary>
        /// <param name="pickSelected">Is item picked?</param>
        protected override void Audio_PickedSelected(bool pickSelected)
        {
            if (!pickSelected) return;

            // 5/5/2009
            UpdateAudioEmitters();

            // TODO: Place given sound to play in the PlayableItemAtts file.
            // If War Factory, then play given sound.
            if (ShapeItem.ItemType == ItemType.sciFiBldb01 || ShapeItem.ItemType == ItemType.sciFiBldb11)
                AudioManager.Play3D(UniqueKey, Sounds.WF_Selected, AudioListenerI, AudioEmitterI, false);
        }

        #endregion

    }
}
