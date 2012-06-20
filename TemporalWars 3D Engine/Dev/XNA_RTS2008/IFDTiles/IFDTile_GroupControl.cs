#region File Description
//-----------------------------------------------------------------------------
// IFDTileGroupControl.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.IFDTiles.Enums;
using TWEngine.IFDTiles.Structs;
using TWEngine.SceneItems;

namespace TWEngine.IFDTiles
{
    // 9/25/2008: Created
    /// <summary>
    /// An <see cref="IFDTileManager"/> tile, inherited from the base <see cref="IFDTile"/>,
    /// used to show a texture on screen; when clicked, will set what is the current display group.  
    /// For example, 9 tiles could be grouped into a <see cref="IFDGroupControlType"/> called 'Buildings', 
    /// which would be set as the current display group.
    /// </summary>
    public sealed class IFDTileGroupControl : IFDTile
    {

        // 9/24/2008 - 
        /// <summary>
        /// The <see cref="IFDGroupControlType"/> this belongs to.
        /// </summary>
        internal IFDGroupControlType GroupControlType;

        // 11/4/2008 - 
        /// <summary>
        /// Collection of <see cref="IFDTileSubGroupControl"/> building queue tabs.
        /// </summary>
        private readonly List<IFDTileSubGroupControl> _buildingQueues = new List<IFDTileSubGroupControl>();
        
        /// <summary>
        /// Stores the <see cref="SubQueueKey"/>, for the last queue used.
        /// </summary>
        internal SubQueueKey LastQueueUsed;

        // 11/4/2008 - Active Group - 
        /// <summary>
        /// There can only be one active <see cref="IFDGroupControlType"/> selected at a time.
        /// This setting controls which subGroup is drawn.
        /// </summary>
        public static IFDGroupControlType ActiveGroup;

        // 11/4/2008
        /// <summary>
        /// Sets Reference for all <see cref="IFDTile"/> Positions.
        /// </summary>
        private static Point _tilePlacementStartPoint = new Point(1035, 245); // 1/9/2010 - Set Default Values (1035,245)

        #region Properties

        ///<summary>
        /// Tile placement start <see cref="Point"/>.
        ///</summary>
        public static Point TilePlacementStartPoint
        {
            get { return _tilePlacementStartPoint; }
            set { _tilePlacementStartPoint = value; }
        }

        #endregion

         // 9/24/2008
        /// <summary>
        /// Constructor for creating a <see cref="IFDTileGroupControl"/> tile, which is used
        /// to set the current <see cref="IFDGroupControlType"/> group to render.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="tileName">Tile texture to show</param>
        /// <param name="tileGroup"><see cref="IFDGroupControlType"/> to make current</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> as location to display tile</param>
        public IFDTileGroupControl(Game game, string tileName, IFDGroupControlType tileGroup, Rectangle tileLocation)
            : base(game)
        {  
            // Set Last SubQueueKey to (-1)
            LastQueueUsed.InstanceKey = -1;

            // Load Texture Tile Given            
            MainImage = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>(tileName);

            // TextureRectSize should match actual pic size.
            TextureRectSize = new Rectangle(0, 0, MainImage.Width, MainImage.Height);
            BackgroundTextureRectSize = new Rectangle(0, 0, 75, 75);           

            // Set TileGroup
            GroupControlType = tileGroup;

            IFDTileLocation.X = _tilePlacementStartPoint.X + tileLocation.X;
            IFDTileLocation.Y = _tilePlacementStartPoint.Y + tileLocation.Y;

            var tmpRectangle = tileLocation;
            tmpRectangle.X += _tilePlacementStartPoint.X;
            tmpRectangle.Y += TilePlacementStartPoint.Y;
            TileRectCheck = tmpRectangle;

            // Scale for Background to match these smaller tiles.
            BackgroundImageOffScale = 0.45f;
            BackgroundImageOnScale = 0.45f;

        }
        /// <summary>
        /// Constructor for creating the generic <see cref="IFDTileGroupControl"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public IFDTileGroupControl(Game game)
            : base(game)
        {
            return; 
        }
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public IFDTileGroupControl() : this(TemporalWars3DEngine.GameInstance)
        {
            return;
            
        }
         

        // 11/4/2008
        /// <summary>
        /// If this is active <see cref="GroupControlType"/>, then Call 'Update' on
        /// <see cref="_buildingQueues"/> tiles.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            // Are we the ActiveGroup Control selected?
            if (ActiveGroup == GroupControlType)
            {
                // Then Update subGroup Building Queues
                var count = _buildingQueues.Count; // 11/9/2009
                for (var i = 0; i < count; i++)
                {
                    // 11/9/2009 - cache
                    var buildingQueue = _buildingQueues[i];

                    if (buildingQueue != null) buildingQueue.Update(gameTime);
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// If this is active  <see cref="GroupControlType"/>, then Call <see cref="RenderInterFaceTile"/> for
        /// all <see cref="_buildingQueues"/> tiles.
        /// </summary>     
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>    
        public override void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;      
 
            // Are we the ActiveGroup Control selected?
            if (ActiveGroup == GroupControlType)
            {
                // Then Draw subGroup Building Queues
                var count = _buildingQueues.Count; // 11/9/2009
                for (var i = 0; i < count; i++)
                {
                    // 11/9/2009 - cache
                    var buildingQueue = _buildingQueues[i];

                    if (buildingQueue != null) buildingQueue.RenderInterFaceTile(gameTime);
                }
            }

            base.RenderInterFaceTile(gameTime);
        }

        // 11/4/2008
        /// <summary>
        /// Adds a new SubControl tile to the <see cref="_buildingQueues"/> collection.
        /// </summary>
        /// <param name="owner"><see cref="BuildingScene"/> owner</param>
        /// <param name="subQueueKey">(OUT) <see cref="SubQueueKey"/> structure</param>
        internal void AddNewBuildingQueueTab(BuildingScene owner, out SubQueueKey subQueueKey)
        {
            var subTileCount = _buildingQueues.Count;
            subTileCount++;
            var textureToUse = String.Empty;
            switch (GroupControlType)
            {                
                case IFDGroupControlType.Buildings:
                    textureToUse = "IFDTileGC_Buildings";
                    break;
                case IFDGroupControlType.Shields:
                    textureToUse = "IFDTileGC_Shields";
                    break;
                case IFDGroupControlType.People:
                    break;
                case IFDGroupControlType.Vehicles:
                    textureToUse = "IFDTileGC_Vehicles";
                    break;
                case IFDGroupControlType.Airplanes:
                    textureToUse = "IFDTileGC_Airplanes"; // 2/3/2009
                    break;
                default:
                    break;
            }

            var subBuildingTab = new IFDTileSubGroupControl(GameInstance, textureToUse, owner,
                                                        (IFDSubGroupPosition)subTileCount, GroupControlType, subTileCount);

            // Set EventHandler for event
            subBuildingTab.TileSelectedEvent += SubBuildingTabTileClicked;            

            _buildingQueues.Add(subBuildingTab);

            subQueueKey = subBuildingTab.SubQueueKey;
        }

        // 12/9/2008
        /// <summary>
        /// Removes a SubControl from the <see cref="_buildingQueues"/> collection.
        /// </summary>
        /// <param name="subQueueKey"><see cref="SubQueueKey"/> structure</param>
        internal void RemoveBuildingQueueTab(ref SubQueueKey subQueueKey)
        {
            // 11/9/2009: NOTE: Do not optimize 'Count' here, because 'RemoveAt' inside loop is changing count!
            // Find SubQueue to delete from List
            for (var i = 0; i < _buildingQueues.Count; i++)
            {
                // 11/9/2009 - Cache
                var buildingQueue = _buildingQueues[i];
                
                // 11/9/2009 - Null check.
                if (buildingQueue == null)
                    continue;

                if (buildingQueue.SubQueueKey.InstanceKey != subQueueKey.InstanceKey) continue;

                // Found Queue, so let's delete it.
                _buildingQueues.RemoveAt(i);
                break;
            }

            // Since we removed a Queue, we must renumber them again.
            for (var i = 0; i < _buildingQueues.Count; i++)
            {
                // 11/9/2009 - Cache
                var buildingQueue = _buildingQueues[i];

                // 11/9/2009 - Null check.
                if (buildingQueue == null)
                    continue;

                buildingQueue.TilePosition = (IFDSubGroupPosition)(i + 1);

                // Reset the Tilecheck Draw Position
                Rectangle tileCheck;
                buildingQueue.SetTileCheckPosition((IFDSubGroupPosition)(i + 1), (i + 1), out tileCheck);
            }
        }      

        // 11/4/2008
        /// <summary>
        /// Event Handler for <see cref="IFDTileSubGroupControl"/>, which stores 
        /// the <see cref="SubQueueKey"/> structure into the <see cref="LastQueueUsed"/>.
        /// </summary>
        private void SubBuildingTabTileClicked(object sender, EventArgs e)
        {
            var tile = (IFDTileSubGroupControl)sender;

            LastQueueUsed = tile.SubQueueKey;
        }      

        // 9/25/2008 - Tile Clicked Event.
        /// <summary>
        /// Tile clicked event, calling the method <see cref="SetAsCurrentGroupToDisplay"/>,
        /// and then proceeding to the base <see cref="IFDTile.TileSelected"/> call.
        /// </summary>
        internal override void TileSelected()
        {
            // Show subGroup by setting 'ActiveGroup' Selected.
            ActiveGroup = GroupControlType;

            // Set Proper SubQueue Group to display
            SetAsCurrentGroupToDisplay();

            base.TileSelected();
        }

        // 11/4/2008
        /// <summary>
        /// Checks if the <see cref="LastQueueUsed"/> is valid to display, otherwise
        /// the 1st item in the <see cref="_buildingQueues"/> collection is set to display.
        /// </summary>
        public void SetAsCurrentGroupToDisplay()
        {
            // Get SubQueue to Display
            if (LastQueueUsed.InstanceKey != -1)
            {
                // Set to Last HasFocus SubQueue
                IFDTileManager.SetAsCurrentGroupToDisplay(ref LastQueueUsed);
            }
            else
            {
                // Set to First in buildingQueue, if any.
                if (_buildingQueues.Count > 0)
                {
                    var subQueueKey = _buildingQueues[0].SubQueueKey;
                    IFDTileManager.SetAsCurrentGroupToDisplay(ref subQueueKey);
                    LastQueueUsed = _buildingQueues[0].SubQueueKey;
                }
            }

        }
    }
}
