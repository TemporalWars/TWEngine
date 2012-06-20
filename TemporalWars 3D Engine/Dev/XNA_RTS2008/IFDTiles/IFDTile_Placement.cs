#region File Description
//-----------------------------------------------------------------------------
// IFDTilePlacement.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.IFDTiles.Delegates;
using TWEngine.IFDTiles.Enums;
using TWEngine.IFDTiles.Structs;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.MemoryPool;
using TWEngine.Players;
using TWEngine.SceneItems;
using TWEngine.ItemTypeAttributes;
using TWEngine.Terrain;

namespace TWEngine.IFDTiles
{
    // 9/25/2008: Created
    /// <summary>
    /// The <see cref="IFDTilePlacement"/> tile is used to display a <see cref="Texture2D"/> on screen; when clicked,
    /// allows a user to place the <see cref="SceneItem"/> owner seen in the texture, into the game engine world.
    /// </summary>
    public sealed class IFDTilePlacement : IFDTile
    {
        // 9/28/2008 -
        /// <summary>
        ///  Reference to <see cref="InterFaceRoundMeter"/>.
        /// </summary>
        internal InterFaceRoundMeter RoundMeter;        
        
        // 9/25/2008 -
        /// <summary>
        ///  Saves ref to a <see cref="IFDTileMessage"/>, shown when
        ///  user's mouse hovers over this <see cref="IFDTile"/>.
        /// </summary>
        private IFDTileMessage _messageTile;

        // 9/23/2008 - Delegate to call for 'CreateItem' tiles.
        ///<summary>
        /// Delegate to call for redirection to proper creation method, depending
        /// on <see cref="IFDTilePlacement"/> caller.
        ///</summary>
        public CreateItemToPlace CreateItemToPlace;

        // 11/5/2008 - Event for when tile requests creation of some sceneItem.
        ///<summary>
        /// Occurs when <see cref="IFDTilePlacement"/> requests creation for some <see cref="SceneItem"/>.
        ///</summary>
        public event ItemCreateRequestEventHandler ItemCreateRequest;

        // 3/25/2009 -
        ///<summary>
        /// Occurs when a <see cref="SceneItem"/> is placed.
        ///</summary>
        public event EventHandler ItemPlaced;

        // 3/26/2009 - SpecialBuilding1 name Identifier; used when some tile is attached
        //             to the BuildingScene 'SpecialBuildingCreated' event!  
        ///<summary>
        /// When some <see cref="IFDTile"/> is attached to the <see cref="BuildingScene"/> classes event 
        /// <see cref="BuildingScene.SpecialBuildingCreated"/>, this name is used to identify if the given
        /// <see cref="BuildingScene"/> is of 'Special' bulding type#1.  Ultimately, this is used to
        /// unlock some other game feature, like additional technology buildings.
        ///</summary>
        public string SpecialBuildingName1 = string.Empty;

        ///<summary>
        /// When some <see cref="IFDTile"/> is attached to the <see cref="BuildingScene"/> classes event 
        /// <see cref="BuildingScene.SpecialBuildingCreated"/>, this name is used to identify if the given
        /// <see cref="BuildingScene"/> is of 'Special' bulding type#2.  Ultimately, this is used to
        /// unlock some other game feature, like additional technology buildings.
        ///</summary>
        public string SpecialBuildingName2 = string.Empty;
        
        ///<summary>
        /// Designates that the two special enablers, <see cref="SpecialBuildingName1"/> and <see cref="SpecialBuildingName2"/>,
        /// are required to get access to the current <see cref="SceneItem"/>.
        ///</summary>
        public bool RequiresTwoEnablers;
        
        /// <summary>
        /// Set to 'TRUE' when current <see cref="SpecialBuildingName1"/> was placed.
        /// </summary>
        private bool _specialBuilding1Placed;

        /// <summary>
        /// Set to 'TRUE' when current <see cref="SpecialBuildingName2"/> was placed.
        /// </summary>
        private bool _specialBuilding2Placed;

        // 10/6/2008 - Sets Ref for all Tiles Pos 1-9.
        private static Point _tilePlacementStartPoint = new Point(1035, 330);  // 1/9/2010 - Set Default Values (1035, 330)       
        // 12/26/2008 - ItemGroup to Attack for Defense items.
        private readonly ItemGroupType? _itemGroupToAttack;

        // 11/4/2008 - Production Type - Used to add Sub Queue to Proper GroupControl Tab.        
        private readonly ItemGroupType? _productionType;
        // 11/11/2008 - Production Building Ref pointer
        internal BuildingScene ProductionBuilding;

        // 12/8/2008 - Tracks the SubQueue this Tile belongs to; for example, "Building", "Airplanes", "Tanks", etc.
        //             This is used during the 'Clicked' event to add to the InterfaceDisplay's 'IfdTilesClicked' Dictionary,
        //             which needs this key to access the internal 'Queue'.
        private SubQueueKey _subQueueKey;       

        // 12/8/2008 
        private string _tileClickCountString = string.Empty; // 4/21/2009 - Stores quantity for display, and avoids high GC!
        private readonly bool _allowMultipleClicks; // Can we Queue up Multiple items for this tile?
        private readonly SpriteFont _numberFont;
        private readonly Color _numberColor;
        private readonly Vector2 _numberPos;

        // 5/18/2009 - Tracks the TOTAL Count for all tiles Queued, by BuildingType.
        private static int _totalTileClicksCountBuildings;
        private static int _totalTileClicksCountDefenses; // 6/29/2009
        private static int _totalTileClicksCountVehicles;
        private static int _totalTileClicksCountAircraft;

        // 1/5/2009 - Used to reduce the Cash amount of player for current SceneItemOwner being built
        private readonly float _itemTotalCost; // Cost to build SceneItemOwner
        private int _itemInProdCost; // Amount spent so far during production for building current SceneItemOwner
        private int _calcInProdCost; // The current calc of production cost for building
        
        
        #region Properties

        /// <summary>
        /// Is Placement Flag Icon?
        /// </summary>
        public bool IsPlacementFlag { get; set; }

        ///<summary>
        /// Start <see cref="Point"/> for this <see cref="IFDTilePlacement"/>.
        ///</summary>
        public static Point TilePlacementStartPoint
        {
            get { return _tilePlacementStartPoint; }
            set { _tilePlacementStartPoint = value; }
        }

        /// <summary>
        /// <see cref="ItemType"/> to place
        /// </summary>
        public ItemType ItemTypeToUse { get; set; }

        /// <summary>
        /// <see cref="ItemGroupType"/> Enum - Used to know what class type to instantiate
        /// </summary>
        public ItemGroupType BuildingType { get; private set; }

        ///<summary>
        /// Unique key to retrieve some <see cref="IFDTile"/>.
        ///</summary>
        public SubQueueKey SubQueueKey
        {
            get { return _subQueueKey; }            
        }

        /// <summary>
        /// Quantity of tile clicks queued - if clicked 5 times, then 5 items will be created.  
        /// </summary>
        public int TileClicksCount { get; private set; }

        #endregion

        // 11/6/2009: Updated by removing the params; 'ItemGroupToAttack', 'BuildingType', & 'ProductionType'.
        /// <summary>
        /// Constructor for creating a Texture <see cref="IFDTilePlacement"/>, which is used
        /// to show an <see cref="ItemType"/>, which user can click and place on <see cref="TerrainShape"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>        
        /// <param name="tileName">Name of tile texture to show</param>
        /// <param name="tileCheck"><see cref="Rectangle"/> area used as cursor check</param>
        /// <param name="itemToUse"><see cref="ItemType"/>, of <see cref="ScenaryItemScene"/>, to place on <see cref="TerrainShape"/></param>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> for the group control</param>
        /// <param name="allowMultipleClicks">Allow for multiple clicks?</param>
        public IFDTilePlacement(Game game, string tileName, Rectangle tileCheck, ItemType itemToUse, SubQueueKey subQueueKey, bool allowMultipleClicks)
            : base(game)
        {

            // 11/6/2009 - Get ItemType atts.
            PlayableItemTypeAttributes itemAtts;
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemToUse, out itemAtts);

            // 12/26/2008 - ItemGroupType to Attack for defense structures
            _itemGroupToAttack = itemAtts.ItemGroupToAttack; // 11/6/2009
            // Store GroupControl Production Type
            _productionType = itemAtts.ProductionType; // 11/6/2009
            // Store Building Type
            BuildingType = itemAtts.ItemGroupType; // 11/6/2009
            // 12/8/2008 - Store SubQueueKey
            _subQueueKey = subQueueKey;
            // 12/8/2008 - Allow Multiple Clicks
            _allowMultipleClicks = allowMultipleClicks;

            // Load Texture Tiles Given            
            MainImage = ContentManager.Load<Texture2D>(String.Format(@"ContentIFDTiles\{0}", tileName));

            // TextureRectSize should match actual pic size.
            TextureRectSize = new Rectangle(0, 0, MainImage.Width, MainImage.Height);
            BackgroundTextureRectSize = TextureRectSize;
            
            IFDTileLocation.X = tileCheck.X; IFDTileLocation.Y = tileCheck.Y;
            TileRectCheck = tileCheck;
            ItemTypeToUse = itemToUse;

            // 12/8/2008
            _numberFont = ContentManager.Load<SpriteFont>(@"Content\Fonts\ConsoleFont");
            _numberColor = Color.White;
            _numberPos = new Vector2(tileCheck.X + 15, tileCheck.Y + 5);

            // 1/5/2009 - Get SceneItemOwner's Total Cost
            PlayableItemTypeAttributes playableAtts;
            if (PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemToUse, out playableAtts))
                _itemTotalCost = playableAtts.Cost;

            // Create Round Meter Countdown bar
            RoundMeter = new InterFaceRoundMeter(game, ContentManager, IFDTileLocationP);

        }

        // 11/6/2009: Updated by removing the params; 'ItemGroupToAttack', 'BuildingType', & 'ProductionType'.
        // 9/23/2008 - Overload 2
        /// <summary>
        /// Constructor for creating a Texture <see cref="IFDTilePlacement"/>, which is used
        /// to show an <see cref="ItemType"/>, which user can click and place on <see cref="TerrainShape"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>        
        /// <param name="tileName">Name of tile texture to show</param>
        /// <param name="tilePlacement"><see cref="TilePlacement"/> Enum</param>
        /// <param name="itemToUse"><see cref="ItemType"/>, of <see cref="ScenaryItemScene"/>, to place on <see cref="TerrainShape"/></param>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> for the group control</param>
        /// <param name="allowMultipleClicks">Allow for multiple clicks?</param>
        public IFDTilePlacement(Game game, string tileName, TilePlacement tilePlacement, ItemType itemToUse, SubQueueKey subQueueKey, bool allowMultipleClicks)
            : base(game)
        {

            // 11/6/2009 - Get ItemType atts.
            PlayableItemTypeAttributes itemAtts;
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemToUse, out itemAtts);

            // 12/26/2008 - ItemGroupType to Attack for defense structures
            _itemGroupToAttack = itemAtts.ItemGroupToAttack; // 11/6/2009
            // Store GroupControl Production Type
            _productionType = itemAtts.ProductionType; // 11/6/2009
            // Store Building Type
            BuildingType = itemAtts.ItemGroupType; // 11/6/2009
            // 12/8/2008 - Store SubQueueKey
            _subQueueKey = subQueueKey;
            // 12/8/2008 - Allow Multiple Clicks
            _allowMultipleClicks = allowMultipleClicks;

            // Load Texture Tiles Given 
            MainImage = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>(tileName);

            // TextureRectSize should match actual pic size.
            TextureRectSize = new Rectangle(0, 0, MainImage.Width, MainImage.Height);
            BackgroundTextureRectSize = TextureRectSize;                      

            // Get Tile Placement Position using given Enum
            Rectangle tileCheck; 

            // 4/29/2009 - Calcualte proper TilePlacement, depending of PC or XBOX
#if XBOX360

             // 4/29/2009 - Set Start of Circle in middle of screen.
            IFDTileManager.MiddleScreenX = TemporalWars3DEngine.GameInstance.GraphicsDevice.PresentationParameters.BackBufferWidth/2;
            IFDTileManager.MiddleScreenY = TemporalWars3DEngine.GameInstance.GraphicsDevice.PresentationParameters.BackBufferHeight/2;

            // Set IFD Placement Tiles Starting Point
            TilePlacementStartPoint = new Point(IFDTileManager.MiddleScreenX, IFDTileManager.MiddleScreenY);

            CalculateTilePlacementForXbox(tilePlacement, out tileCheck);
#else
            CalculateTilePlacementForPc(tilePlacement, out tileCheck);
           
#endif
            

            IFDTileLocation.X = tileCheck.X; IFDTileLocation.Y = tileCheck.Y;
            TileRectCheck = tileCheck;
            ItemTypeToUse = itemToUse;

            // 12/8/2008; 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _numberFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\ConsoleFont");
            _numberColor = Color.White;
            _numberPos = new Vector2(tileCheck.X + 15, tileCheck.Y + 5);

            // 1/5/2009 - Get SceneItemOwner's Total Cost
            PlayableItemTypeAttributes playableAtts;
            if (PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(itemToUse, out playableAtts))
                _itemTotalCost = playableAtts.Cost;

            // Create Round Meter Countdown bar
            RoundMeter = new InterFaceRoundMeter(game, ContentManager, IFDTileLocation);
        }
        /// <summary>
        /// Constructor for creating the generic <see cref="IFDTilePlacement"/>.
        /// </summary>
        /// <param name="game">Game Instance</param>
        public IFDTilePlacement(Game game)
            : base(game)
        {
            return;
        }
        /// <summary>
        /// Default Constructor for <see cref="IFDTilePlacement"/>
        /// </summary>
        public IFDTilePlacement() : this(TemporalWars3DEngine.GameInstance)
        {
            return;
        }
         

        // 4/29/2009
        /// <summary>
        /// Calculates the tile location for the PC, using the given <see cref="TilePlacement"/> Enum.
        /// </summary>
        /// <param name="tilePlacement"><see cref="TilePlacement"/> Enum</param>
        /// <param name="tileCheck">(OUT) <see cref="Rectangle"/> area to use as cursor check</param>
        private static void CalculateTilePlacementForPc(TilePlacement tilePlacement, out Rectangle tileCheck)
        {
            tileCheck = new Rectangle {Width = 75, Height = 75};
            switch (tilePlacement)
            {
                case TilePlacement.Pos1:
                    tileCheck.X = _tilePlacementStartPoint.X;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case TilePlacement.Pos2:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case TilePlacement.Pos3:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case TilePlacement.Pos4:
                    tileCheck.X = _tilePlacementStartPoint.X;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                case TilePlacement.Pos5:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                case TilePlacement.Pos6:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                case TilePlacement.Pos7:
                    tileCheck.X = _tilePlacementStartPoint.X ;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 150;
                    break;
                case TilePlacement.Pos8:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 150;
                    break;
                case TilePlacement.Pos9:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 150;
                    break;
                case TilePlacement.Pos10:
                    tileCheck.X = _tilePlacementStartPoint.X;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;
                case TilePlacement.Pos11:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;
                case TilePlacement.Pos12:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;
                default:
                    break;
            }
            
        }

        // 4/29/2009
        /// <summary>
        /// Calculates the tile location for the XBOX, using the given <see cref="TilePlacement"/> Enum.
        /// </summary>
        /// <param name="tilePlacement"><see cref="TilePlacement"/> Enum</param>
        /// <param name="tileCheck">(OUT) <see cref="Rectangle"/> area to use as cursor check</param>
// ReSharper disable UnusedMember.Local
        private static void CalculateTilePlacementForXbox(TilePlacement tilePlacement, out Rectangle tileCheck)
// ReSharper restore UnusedMember.Local
        {
            tileCheck = new Rectangle {Width = 75, Height = 75};
            switch (tilePlacement)
            {
                case TilePlacement.Pos1:
                    tileCheck.X = _tilePlacementStartPoint.X - 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y - 150;
                    break;
                case TilePlacement.Pos2:
                    tileCheck.X = _tilePlacementStartPoint.X - 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                case TilePlacement.Pos3:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y - 150;
                    break;
                case TilePlacement.Pos4:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                case TilePlacement.Pos5:
                    tileCheck.X = _tilePlacementStartPoint.X - 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y - 37;
                    break;
                case TilePlacement.Pos6:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y - 37;
                    break;
                case TilePlacement.Pos7:
                    tileCheck.X = _tilePlacementStartPoint.X - 37;
                    tileCheck.Y = _tilePlacementStartPoint.Y - 150;
                    break;
                case TilePlacement.Pos8:
                    tileCheck.X = _tilePlacementStartPoint.X - 37;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 75;
                    break;
                /*case TilePlacement.Pos9:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 150;
                    break;
                case TilePlacement.Pos10:
                    tileCheck.X = _tilePlacementStartPoint.X;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;
                case TilePlacement.Pos11:
                    tileCheck.X = _tilePlacementStartPoint.X + 75;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;
                case TilePlacement.Pos12:
                    tileCheck.X = _tilePlacementStartPoint.X + 150;
                    tileCheck.Y = _tilePlacementStartPoint.Y + 225;
                    break;*/
                default:
                    break;
            }

        }

        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {
            if (finalDispose)
            {
                if (RoundMeter != null)
                    InterFaceRoundMeter.Dispose(true);

                if (_messageTile != null)
                    _messageTile.Dispose(true);

                CreateItemToPlace = null;
                RoundMeter = null;
                _messageTile = null;
            }

            base.Dispose(finalDispose);
        }

        /// <summary>
        /// Processes round meter countdown, as well as update the <see cref="Player"/> cash value.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // 3/25/2009 - Skip if TileState is 'Disabled'
            if (TileState == TileState.Disabled)
                return;

            // 9/28/2008 - Update Round Meter Countdown
            if (TileState == TileState.Countdown)
                RoundMeter.Update(gameTime);

            // 5/11/2009 - Skip if in Paused state.
            if (RoundMeter.RunCountdown && TileState != TileState.Paused)
            {
                // 6/15/2010 - Updated to use new GetPlayer method.
                Player thisPlayer;
                TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer);

                // 1/5/2009 - Calculate the 'In-Production' partial cost for SceneItemOwner being built
                // 1st - Add back ProdCost from last cycle
                thisPlayer.Cash += _itemInProdCost;
                // 2nd - Calc the current ProdCost for given RoundMeter percent.
                _calcInProdCost = (int)(_itemTotalCost * RoundMeter.CurrentMeterValue);
                // 3rd - Verify if sufficient funds
                if (_calcInProdCost <= thisPlayer.Cash)
                {
                    // 4th - Subtract ProdCost for this cycle.
                    thisPlayer.Cash -= _calcInProdCost;
                    _itemInProdCost = _calcInProdCost;

                    // 5th - Check if last state was 'InsufficientFunds'.
                    if (TileState == TileState.InsufficientFunds)
                        TileState = TileState.Countdown;
                }
                else
                {
                    // 4th - Subtract ProdCost from last cycle, and stop production!
                    thisPlayer.Cash -= _itemInProdCost;
                    TileState = TileState.InsufficientFunds;
                }


                // Check if done
                if (RoundMeter.CurrentMeterValue == 1)
                {
                    CreatePlacementItem(); 

                } // end CurrentMeterValue = 1
            } // end RunCountDown
                 

            base.Update(gameTime);
        }

        // 12/8/2008
        /// <summary>
        /// Helper method to create the proper <see cref="IFDTilePlacement"/> <see cref="SceneItem"/> owner.
        /// </summary>
        private void CreatePlacementItem()
        {
            RoundMeter.RunCountdown = false;

            // Set Tile State
            TileState = TileState.Ready;

            // 12/8/2008 - Reduce counter
            if (_allowMultipleClicks)
            {
                TileClicksCount--;
                _tileClickCountString = TileClicksCount.ToString(); // 4/21/2009
            }

            // 11/5/2008
            // ******
            // If ItemCreated is not Null, then this is a Unit which does not need placement, and should be created
            // immediately!
            // ******
            // Fire ItemCreated Event
            if (ItemCreateRequest == null) return;

            _itemInProdCost = 0;
            // 6/10/2010 - Updated to use Vector3.Add() for placeItemAt.
            Vector3 placeItemAt;
            var position = ProductionBuilding.Position;
            var itemSpawnOffset = ProductionBuilding.ItemSpawnOffset;
            Vector3.Add(ref position, ref itemSpawnOffset, out placeItemAt);
            // 2/4/2009 - Updated to include the 'ItemSpawnOffset' adjustement for PlaceItemAt parameter.
            var itemArgs = new ItemCreatedArgs(BuildingType, null, ItemTypeToUse, _itemGroupToAttack,
                                                           placeItemAt, ProductionBuilding.NetworkItemNumber, ProductionBuilding, 0);
                
            ItemCreateRequest(this, itemArgs);

            // 4/18/2009 - Fixed: Fixed the error where a user could hover over the tile, which has multiple items 
            //                    Queued up, but when an SceneItemOwner finished building, the next SceneItemOwner would not start building automatically, since
            //                    the hovering got in the way!  The solution is to set the TileState to 'Queued' here, if additional items are 
            //                    queued!
            TileState = TileClicksCount > 0 ? TileState.Queued : TileState.None;
        }

        /// <summary>
        /// Renders the <see cref="IFDTilePlacement"/> tile.
        /// </summary>
        /// <param name="gameTime">Instance of game time.</param>         
        public override void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;

            // 4/21/2009: Updated to use the new 'TileClicksCountString', rather than recreating is every cycle, which creates garabage on heap!
            // 12/8/2008 - Show Items Clicked Counter
            if (TileClicksCount > 0)
                SpriteBatch.DrawString(_numberFont, _tileClickCountString, _numberPos, _numberColor, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

            // 9/25/2008 - Does this tile have a '_messageTile'?
            if (_messageTile != null) 
                _messageTile.RenderInterFaceTile(gameTime);
                

            base.RenderInterFaceTile(gameTime);
        }

       

        // 9/25/2008
        /// <summary>
        /// Adds a <see cref="IFDTileMessage"/> tile, which is displayed when the cursor hovers over
        /// the current tile.
        /// </summary>
        /// <param name="tile">IFD Message tile to display</param>
        public void AddMessageTile(IFDTileMessage tile)
        {
            // Save Ref to message tile.
            _messageTile = tile;           
        }

        // 9/25/2008 - Tile Clicked Event.
        // 12/8/2008 - Updated to add a tile clicked to the Queue in the 'Hovered' case.
        /// <summary>
        /// <see cref="IFDTilePlacement"/> selected event, which checks current <see cref="TileState"/>
        /// and processes accordingly.
        /// </summary>
        internal override void TileSelected()
        {
            // Check tile State
            switch (TileState)
            {                
                case TileState.Paused:
                    TileState = TileState.Countdown; // Resume Countdown
                    break;
                case TileState.Hovered:  
                    // 5/19/2009 - Check if placement flag.
                    if (IsPlacementFlag && !IFDTileManager.AttemptingItemPlacement)
                    {
                        // Call Tile's Delegate Function to create SceneItemOwner for placement. 
                        if (CreateItemToPlace != null)
                        {
                            _itemInProdCost = 0;
                            CreateItemToPlace(BuildingType, _productionType, ItemTypeToUse, _itemGroupToAttack, ref IFDTileManager.PlaceItemAt, this);
                            IFDTileManager.AttemptingItemPlacement = true;

                            // 11/9/2009 - DeActivate current group
                            IFDTileManager.DeActivateCurrentDisplayGroup();
                        }
                        TileState = TileState.None;
                        return;
                    }

                    // 5/29/2009 - Call the PreLoad InstancedItem method.
                    //InstancedItem.PreLoadPlayableInstancedItem(ItemTypeToUse);
                   

                    // 6/29/2009 - Check if at Unit Cap.
                    if (IsBuildTypeAtUnitCap())
                        return;    
                    
                    // 12/8/2008 - Add tile clicked to Queue.                    
                    IFDTileManager.AddTileClickedToQueue(ref _subQueueKey, this);
                    if (_allowMultipleClicks)
                    {                                          
                        
                        TileClicksCount++;                        
                        _tileClickCountString = TileClicksCount.ToString(); // 4/21/2009
                    }
                    TileState = TileState.Queued;
                    break;
                case TileState.Queued:
                case TileState.Countdown:
                    // 6/29/2009 - Check if at Unit Cap.
                    if (IsBuildTypeAtUnitCap())
                        return;

                    // 12/8/2008 - Only need to increase the tiles click counter.                   
                    if (_allowMultipleClicks)
                    {                        
                        TileClicksCount++;                       
                        _tileClickCountString = TileClicksCount.ToString(); // 4/21/2009
                    }
                    break;
                case TileState.Ready:
                    // 9/24/2008
                    // Call Tile's Delegate Function to create SceneItemOwner for placement. 
                    if (CreateItemToPlace != null)
                    {
                        _itemInProdCost = 0;
                        CreateItemToPlace(BuildingType, _productionType, ItemTypeToUse, _itemGroupToAttack, ref IFDTileManager.PlaceItemAt, this);
                        IFDTileManager.AttemptingItemPlacement = true;
                    }
                    TileState = TileState.None;

#if XBOX
                    // 11/9/2009: Updated to also check 'Defense' items. Note: Flag check is done above in hovering.
                    // 5/19/2009 - On XBOX, close menu when placing BuildingType of buildings!
                    if (BuildingType == ItemGroupType.Buildings || BuildingType == ItemGroupType.Shields)
                    {
                        // 11/9/2009 - Updated to use this DeActivate method.
                        IFDTileManager.DeActivateCurrentDisplayGroup();
                    }
#endif

                    break;
                default:
                    break;
            }           
            
            base.TileSelected();
        }

       

        // 9/28/2008 - Tile Right-Click Event.
        /// <summary>
        /// <see cref="IFDTilePlacement"/> canceled event, which checks current <see cref="TileState"/>
        /// and processes accordingly.
        /// </summary>
        internal override void TileCanceled()
        {
            // check tile State
            switch (TileState)
            {                
                case TileState.Countdown:
                    TileState = TileState.Paused; // Pause order
                    break;
                case TileState.Paused:
                case TileState.Ready:
                case TileState.Queued:
                case TileState.InsufficientFunds:

                    // 8/17/2009 - Cache
                    var thisPlayerNumber = TemporalWars3DEngine.SThisPlayer;

                    // 6/15/2010 - Updated to use new GetPlayer method.
                    Player thisPlayer;
                    TemporalWars3DEngine.GetPlayer(thisPlayerNumber, out thisPlayer);
                  
                    // 12/8/2008
                    if (_allowMultipleClicks)
                    {
                        // 5/18/2009 - Reduce TOTAL queued counter for given BuildingType.
                        ReduceTotalQueuedCountForBuildingType(BuildingType, thisPlayerNumber);

                        TileClicksCount--;                        
                        _tileClickCountString = TileClicksCount.ToString(); // 4/21/2009

                        if (TileClicksCount == 0)
                        {
                            // 1/5/2009 - Add back Production Cost since canceled!
                            thisPlayer.Cash += _itemInProdCost;
                            _itemInProdCost = 0;
                            TileState = TileState.None; // Cancel order
                            RoundMeter.RunCountdown = false;
                        }
                    }
                    else
                    {
                        // 1/5/2009 - Add back Production Cost since canceled!
                        thisPlayer.Cash += _itemInProdCost;
                        _itemInProdCost = 0;
                        TileState = TileState.None; // Cancel order
                        RoundMeter.RunCountdown = false;
                    }
                    
                    break;                
                default:
                    break;
            }

            base.TileCanceled();
        }


       

        // 9/25/2008 - Tile Hovered by Mouse/Gamepad Event.
        //             When tile is hovered, let's show the 
        //             Message Tile, if any.
        /// <summary>
        /// <see cref="IFDTilePlacement"/> hovered event, which will
        /// show the <see cref="IFDTileMessage"/>, if any.
        /// </summary>
        /// <param name="isTileHovered">Is tiled hovered?</param>
        internal override void TileHovered(bool isTileHovered)
        {
            if (_messageTile != null)
            {
                _messageTile.DrawTile = isTileHovered;                

            }

            base.TileHovered(isTileHovered);
        }

        // 3/25/2009 - EventHandler for the 'ItemPlaced' Event.
        //             This allows for cross talking between different
        //             instances of this IFDPlacement class.
        /// <summary>
        /// <see cref="IFDTilePlacement"/> placed event handler, which will
        /// turn off the 'Disabled' <see cref="TileState"/> of this instance.  
        /// </summary>
        /// <remarks>
        /// Connect this event handler to the <see cref="IFDTile"/> which will be the 'Enabler'.
        /// </remarks>
        /// <param name="e">An EventArgs that contains no event data.</param>
        /// <param name="sender">The source of the event.</param>
        public void ItemPlaced_EventHandler(object sender, EventArgs e)
        {
            // cast to IFDPlacement tile
            var tile = (IFDTilePlacement)sender;

            // 4/1/2009 - Check if required 2 enablers?
            if (RequiresTwoEnablers)
            {
                // check if proper 'Special' building# 1
                if (SpecialBuildingName1 == tile.SpecialBuildingName1)
                {
                    // yes, so mark SceneItemOwner placed
                    _specialBuilding1Placed = true;
                }

                // check if proper 'Special' building# 2
                if (SpecialBuildingName2 == tile.SpecialBuildingName2)
                {
                    // yes, so mark SceneItemOwner placed
                    _specialBuilding2Placed = true;
                }

                // have both buildings been placed?
                if (_specialBuilding1Placed && _specialBuilding2Placed)
                {
                    // yes, then remove the 'Disabled' TileState.
                    if (TileState == TileState.Disabled) // 5/20/2009
                        TileState = TileState.None;
                }
            }
            else
            {
                // just remove 'Disabled' TileState.
                if (TileState == TileState.Disabled) // 5/20/2009
                    TileState = TileState.None;
            }
        }

        // 3/26/2009
        /// <summary>
        /// <see cref="IFDTilePlacement"/> 'Special-Building' created event handler,
        /// which will turn off the 'Disabled' <see cref="TileState"/> of this instance.  
        /// </summary>
        /// <remarks>
        /// Connect this eventhandler to the BuildingScene global event.</remarks>
        /// <param name="e">An EventArgs that contains no event data.</param>
        /// <param name="sender">The source of the event.</param>
        public void SpecialBuildingCreated_EventHandler(object sender, EventArgs e)
        {
            // cast to buildingScene
            var building = (BuildingScene)sender;

            // 5/4/2009 - If NOT this player, then return.
            if (building.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                return;

            // 8/14/2009 - Cache
            var buildingName = building.PlayableItemAtts.SpecialBuildingName; 

            // 4/1/2009 - Check if required 2 enablers?
            if (RequiresTwoEnablers)
            {
                // check if proper 'Special' building# 1
                if (SpecialBuildingName1 == buildingName)
                {
                    // yes, so mark SceneItemOwner placed
                    _specialBuilding1Placed = true;
                }

                // check if proper 'Special' building# 2
                if (SpecialBuildingName2 == buildingName)
                {
                    // yes, so mark SceneItemOwner placed
                    _specialBuilding2Placed = true;
                }

                // have both buildings been placed?
                if (_specialBuilding1Placed && _specialBuilding2Placed)
                {
                    // yes, then remove the 'Disabled' TileState.
                    if (TileState == TileState.Disabled) // 5/20/2009
                        TileState = TileState.None;
                }

            }
            else
            {
                // check if proper 'Special' building.
                if (SpecialBuildingName1 == buildingName)
                {
                    // yes, then remove the 'Disabled' TileState.
                    if (TileState == TileState.Disabled) // 5/20/2009
                        TileState = TileState.None;
                }
            }
        }

        // 5/18/2009
        /// <summary>
        /// <see cref="IFDTilePlacement"/> 'Special-Building' destroyed event handler, which
        /// will turn ON the 'Disabled' <see cref="TileState"/> of this instance, since
        /// the 'Enabler' <see cref="BuildingScene"/> was destroyed!
        /// </summary>
        /// <param name="e">An EventArgs that contains no event data.</param>
        /// <param name="sender">The source of the event.</param>
        public void SpecialBuildingDestroyed_EventHandler(object sender, EventArgs e)
        {
            // cast to buildingScene
            var building = (BuildingScene)sender;

            // 5/4/2009 - If NOT this player, then return.
            var thisPlayer = TemporalWars3DEngine.SThisPlayer; // 8/14/2009
            if (building.PlayerNumber != thisPlayer)
                return;

            // 8/14/2009 - Cache
            var buildingName = building.PlayableItemAtts.SpecialBuildingName;
           
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(thisPlayer, out player);

            // check if proper 'Special' building# 1
            if (SpecialBuildingName1 == buildingName)
            {
                // now check if any others exist; if TRUE, then just return without doing anything.
                if (Player.IsSpecialBuildingPlaced(player, building, SpecialBuildingName1))
                    return;

                // yes, so mark SceneItemOwner gone
                _specialBuilding1Placed = false;
                // Set TileState back to Disabled.
                TileState = TileState.Disabled;
            }

            // check if proper 'Special' building# 2
            if (SpecialBuildingName2 == buildingName)
            {
                // now check if any others exist; if TRUE, then just return without doing anything.
                if (Player.IsSpecialBuildingPlaced(player, building, SpecialBuildingName2))
                    return;

                // yes, so mark SceneItemOwner gone
                _specialBuilding2Placed = false;
                // Set TileState back to Disabled.
                TileState = TileState.Disabled;
            }

        }

        // 5/18/2009
        /// <summary>
        /// Helper Method which checks if the total queued count, for the given <see cref="ItemGroupType"/> Enum, is at
        /// the Max.  If so, the method will not increase the total queued count for given <see cref="ItemGroupType"/> Enum.
        /// </summary>
        /// <returns>True/False of result</returns>
        private bool IsBuildTypeAtUnitCap()
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer);

            var poolManager = thisPlayer.PoolManager;

            // 8/17/2009
            //
            switch (BuildingType)
            {
                case ItemGroupType.Buildings:
                    if (poolManager.GetPoolItemAvailableCount(typeof(BuildingScenePoolItem)) == 0 ||
                        PoolManager.BuildingItemsMaxPopulation -  _totalTileClicksCountBuildings <= 0 )
                        return true;
                    _totalTileClicksCountBuildings += 1; // 5/18/2009 - Track TOTAL tiles Queued for Buildings
                    break;
                case ItemGroupType.Shields:
                    if (poolManager.GetPoolItemAvailableCount(typeof(DefenseScenePoolItem)) == 0 ||
                        PoolManager.DefenseItemsMaxPopulation - _totalTileClicksCountDefenses <= 0)
                        return true;
                    _totalTileClicksCountDefenses += 1;
                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:
                    if (poolManager.GetPoolItemAvailableCount(typeof(SciFiTankScenePoolItem)) == 0 ||
                        PoolManager.TankItemsMaxPopulation - _totalTileClicksCountVehicles <= 0 ||
                         Player.IsAtPopulationMax(thisPlayer))
                        return true;
                    _totalTileClicksCountVehicles += 1; // 5/18/2009 - Track TOTAL tiles Queued for Vehicles
                    // 8/14/2009 - Increase Population
                    thisPlayer.Population += 1;
                    break;
                case ItemGroupType.Airplanes:
                    if (poolManager.GetPoolItemAvailableCount(typeof(SciFiAircraftScenePoolItem)) == 0 ||
                        PoolManager.AircraftItemsMaxPopulation - _totalTileClicksCountAircraft <= 0 ||
                         Player.IsAtPopulationMax(thisPlayer))
                        return true;
                    _totalTileClicksCountAircraft += 1; // 5/18/2009 - Track TOTAL tiles Queued for Airplanes
                    // 8/14/2009 - Increase Population
                    thisPlayer.Population += 1;
                    break;
            }

            return false;
        }

        // 5/18/2009; 7/27/2009: Updated to be STATIC, and internal, so PoolMAnager can reduce correctly, when an SceneItemOwner is destroyed!
        /// <summary>
        /// Helper method, which reduces the total queued count, for the given <see cref="ItemGroupType"/> Enum.
        /// </summary>
        /// <param name="buildingType"><see cref="ItemGroupType"/> Enum</param>
        /// <param name="playerNumber"><see cref="Player"/> number</param>
        internal static void ReduceTotalQueuedCountForBuildingType(ItemGroupType buildingType, int playerNumber)
        {
            // 8/17/2009 - Make sure reduction request is ONLY for current player.
            var thisPlayerNumber = TemporalWars3DEngine.SThisPlayer;
            if (thisPlayerNumber != playerNumber)
                return;
           
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(thisPlayerNumber, out thisPlayer);

            switch (buildingType)
            {
                case ItemGroupType.Buildings:
                    _totalTileClicksCountBuildings -= 1; // 5/18/2009 - Track TOTAL tiles Queued for BuildingType.
                    break;
                case ItemGroupType.Shields:
                    _totalTileClicksCountDefenses -= 1; // 7/27/2009 - Track TOTAL tiles Queued for DefenseType.
                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:
                    _totalTileClicksCountVehicles -= 1; // 5/18/2009 - Track TOTAL tiles Queued for Vehicles
                    // 8/14/2009 - Decrease Population
                    thisPlayer.Population -= 1;
                    break;
                case ItemGroupType.Airplanes:
                    _totalTileClicksCountAircraft -= 1; // 5/18/2009 - Track TOTAL tiles Queued for Airplanes
                    // 8/14/2009 - Decrease Population
                    thisPlayer.Population -= 1;
                    break;
            }
            
        }

        // 7/27/2009: Overload version.
        /// <summary>
        /// Helper method, which reduces the total queued count by the
        /// <paramref name="quantityToRemove"/> value, for the given <see cref="ItemGroupType"/> Enum.
        /// enum.
        /// </summary>
        /// <param name="buildingType"><see cref="ItemGroupType"/> Enum</param>
        /// <param name="quantityToRemove">quantity to remove value</param>
        /// <param name="playerNumber"><see cref="Player"/> number</param>
        internal static void ReduceTotalQueuedCountForBuildingType(ItemGroupType buildingType, int quantityToRemove, int playerNumber)
        {
            // 8/17/2009 - Make sure reduction request is ONLY for current player.
            var thisPlayerNumber = TemporalWars3DEngine.SThisPlayer;
            if (thisPlayerNumber != playerNumber)
                return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(thisPlayerNumber, out thisPlayer);

            switch (buildingType)
            {
                case ItemGroupType.Buildings:
                    _totalTileClicksCountBuildings -= quantityToRemove; // Track TOTAL tiles Queued for BuildingType.
                    break;
                case ItemGroupType.Shields:
                    _totalTileClicksCountDefenses -= quantityToRemove; // Track TOTAL tiles Queued for DefenseType.
                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:
                    _totalTileClicksCountVehicles -= quantityToRemove; // Track TOTAL tiles Queued for Vehicles
                    // 8/14/2009 - Decrease Population
                    thisPlayer.Population -= quantityToRemove;
                    break;
                case ItemGroupType.Airplanes:
                    _totalTileClicksCountAircraft -= quantityToRemove; // Track TOTAL tiles Queued for Airplanes
                    // 8/14/2009 - Decrease Population
                    thisPlayer.Population -= quantityToRemove;
                    break;
            }
            
        }


        // 3/25/2009
        /// <summary>
        /// Triggers the event <see cref="ItemPlaced"/>.
        /// </summary>
        public void OnItemPlaced()
        {
            if (ItemPlaced != null)
                ItemPlaced(this, EventArgs.Empty);
        }
    }
}
