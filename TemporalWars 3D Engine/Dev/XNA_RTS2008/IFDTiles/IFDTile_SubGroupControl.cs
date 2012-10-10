#region File Description
//-----------------------------------------------------------------------------
// IFDTileSubGroupControl.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.IFDTiles.Enums;
using ImageNexus.BenScharbach.TWEngine.IFDTiles.Structs;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles
{
    // 11/04/2008: Created
    /// <summary>
    /// The <see cref="IFDTileSubGroupControl"/> building queue tile, which shows each sub-queue for a particular
    /// <see cref="IFDGroupControlType"/> Enum.  For example, the <see cref="BuildingScene"/> tile, would have several
    /// sub-queues, where each instance is associated to some <see cref="BuildingScene"/> producing structure in the game.
    /// </summary>
    public sealed class IFDTileSubGroupControl : IFDTile
    {
        // Reference to Building Owner
        private BuildingScene _buildingOwner;     
   
        // SubGroup Static Key Counter
        private static int _instanceKeyCounter;
        private SubQueueKey _subQueueKey;    
   
        // This SubQueue's Number in Group
        private string _subGroupQueueNumber;
        private readonly SpriteFont _numberFont;
        private Color _numberColor;
        private Vector2 _numberPos;

        // Additional Texture for Select State.
        private static Texture2D _backgroundImageSlt;
        private readonly float _backgroundImageSltScale = 1;

        // Tile 'Group' Position
        /// <summary>
        /// <see cref="IFDSubGroupPosition"/> tile position.
        /// </summary>
        internal IFDSubGroupPosition TilePosition;

        // Parent GroupControl Tile Type
        /// <summary>
        /// <see cref="IFDGroupControlType"/> parent tile.
        /// </summary>
        internal IFDGroupControlType TileGroupToSetCurrent;

        // Sets Ref for all Tiles Pos.
        private static Point _tilePlacementStartPoint = new Point(1055, 305);  // 1/9/2010 - Set Default Values (1055, 305)

        // Has Focus - Set when tile clicked.
        private static IFDTileSubGroupControl _wasClicked;       

        #region Properties

        ///<summary>
        /// <see cref="IFDSubGroupPosition"/> placement start <see cref="Point"/>.
        ///</summary>
        public static Point TilePlacementStartPoint
        {
            get { return _tilePlacementStartPoint; }
            set { _tilePlacementStartPoint = value; }
        }
       
        ///<summary>
        /// Returns the unique key for this <see cref="IFDSubGroupPosition"/>.
        ///</summary>
        public SubQueueKey SubQueueKey
        {
            get { return _subQueueKey; }           
        }              

        #endregion

        
        /// <summary>
        /// <see cref="IFDSubGroupPosition"/> constructor, for creating this <see cref="IFDSubGroupPosition"/> tile, which is used
        /// to set the current <see cref="IFDSubGroupPosition"/> Enum to render.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="textureName">Name of <see cref="Texture2D"/> image to load</param>
        /// <param name="owner"><see cref="BuildingScene"/> instance as owner</param>
        /// <param name="tilePosition"><see cref="IFDSubGroupPosition"/> Enum as tile position</param>
        /// <param name="parentGroupControlType">This tile's parent <see cref="IFDGroupControlType"/> Enum.</param>
        /// <param name="subGroupQueueNumber">The visual number displayed for this <see cref="IFDTileSubGroupControl"/> tile.</param>
        public IFDTileSubGroupControl(Game game, string textureName, BuildingScene owner,
            IFDSubGroupPosition tilePosition, IFDGroupControlType parentGroupControlType, int subGroupQueueNumber)
            : base(game)
        {     
            // Set Unique Key for SubGroup.
            // Key is used for InterfaceDisplay classes 'ifdTileGroups' Dictionary.
            _instanceKeyCounter++;
            _subQueueKey.InstanceKey = _instanceKeyCounter;
           
            // Load MainImage Texture Tile Given
            MainImage = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>(textureName);
            MainImageScale = .50f;  // Set MainImage to be smaller
            
            // Set Different Background Image for Ready
            _backgroundImageSlt = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>("IFDTileGC_Selected");
            _backgroundImageSltScale = 0.33f;            
            
            // TextureRectSize should match actual pic size.
            TextureRectSize = new Rectangle(0, 0, MainImage.Width, MainImage.Height);
            BackgroundTextureRectSize = new Rectangle(0, 0, 75, 75);    
       
            // Set Tile Owner Ref
            _buildingOwner = owner;

            // Set TileGroup
            TilePosition = tilePosition;

            // Set Parent Tile GroupControl Type
            TileGroupToSetCurrent = parentGroupControlType;

            // Set Tile Placement Position using given Enum
            Rectangle tileCheck;
            SetTileCheckPosition(tilePosition, subGroupQueueNumber, out tileCheck);
            TileRectCheck = tileCheck;

            // Scale for Background to match these smaller tiles.
            BackgroundImageOffScale = 0.33f;
            BackgroundImageOnScale = 0.33f;

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _numberFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\ConsoleFont");

            // 12/9/2008 - Set EventHandler for event of building killed!
            if (owner != null)
                //owner.sceneItemDestroyed += new EventHandler(OwnerBuildingSceneKilled);
                owner.AssignEventHandler_SceneItemDestroyed(OwnerBuildingSceneKilled, owner); // 6/15/2009

        }
        /// <summary>
        /// Constructor for creating the generic <see cref="IFDTileSubGroupControl"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public IFDTileSubGroupControl(Game game)
            : base(game)
        {
            return;
        }
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public IFDTileSubGroupControl() : this(TemporalWars3DEngine.GameInstance)
        {
            return;
        }
         

        // 12/9/2008
        /// <summary>
        /// Sets the <see cref="IFDTileSubGroupControl"/> tile draw position, using the <see cref="IFDSubGroupPosition"/> Enum as position.
        /// </summary>
        /// <param name="tilePosition"><see cref="IFDSubGroupPosition"/> Enum as tile position.</param>
        /// <param name="subGroupQueueNumber">The visual number displayed for this <see cref="IFDTileSubGroupControl"/> tile.</param>
        /// <param name="tileCheck">(OUT) </param>
        /// <returns></returns>
        internal void SetTileCheckPosition(IFDSubGroupPosition tilePosition, int subGroupQueueNumber, out Rectangle tileCheck)
        {
            tileCheck = new Rectangle {Width = 25, Height = 25};
            switch (tilePosition)
            {
                case IFDSubGroupPosition.Pos1:
                    tileCheck.X = _tilePlacementStartPoint.X;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case IFDSubGroupPosition.Pos2:
                    tileCheck.X = _tilePlacementStartPoint.X + 30;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case IFDSubGroupPosition.Pos3:
                    tileCheck.X = _tilePlacementStartPoint.X + 60;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case IFDSubGroupPosition.Pos4:
                    tileCheck.X = _tilePlacementStartPoint.X + 90;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                case IFDSubGroupPosition.Pos5:
                    tileCheck.X = _tilePlacementStartPoint.X + 120;
                    tileCheck.Y = _tilePlacementStartPoint.Y;
                    break;
                default:
                    break;
            }

            // Set SubGroupControl Queue number.
            _subGroupQueueNumber = subGroupQueueNumber.ToString();           
            _numberColor = Color.White;
            _numberPos = new Vector2(tileCheck.X + 15, tileCheck.Y + 5);

            IFDTileLocation.X = tileCheck.X; IFDTileLocation.Y = tileCheck.Y;
            
        }

        // 12/9/2008
        /// <summary>
        /// Event Handler for <see cref="BuildingScene"/> killed event, which tells this <see cref="IFDTileSubGroupControl"/>
        /// instance to remove the <see cref="SubQueueKey"/> used by the <see cref="BuildingScene"/>.
        /// </summary>
        private void OwnerBuildingSceneKilled(object sender, EventArgs e)
        {
            var buildingToDelete = (BuildingScene)sender;

            // 12/10/2008 - Since all eventHandler will be called, we need to verify
            //              this is the proper subGroupControl to delete.
            if (_buildingOwner != buildingToDelete) return;

            // Release Ref to BuildingScene
            _buildingOwner = null;

            // Call InterfaceDisplay to remove this SubQueue.
            IFDTileManager.RemoveBuildingQueueTab(ref _subQueueKey, (ItemGroupType)TileGroupToSetCurrent);
        }

        /// <summary>
        /// Renders this <see cref="IFDTileSubGroupControl"/> sub group queue number.
        /// </summary>   
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>      
        public override void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;     
  
            // Draw SubGroup Queue Number
            SpriteBatch.DrawString(_numberFont, _subGroupQueueNumber, _numberPos, _numberColor);

            // 11/5/2008 - Draw Special 'Select' Texture, when Active Selection, and set
            //             TileState to 'Select', so base class does not override background.
            if (_wasClicked == this)
            {
                SpriteBatch.Draw(_backgroundImageSlt, IFDTileLocationP, BackgroundTextureRectSize, Color.White
               , 0, Origin, _backgroundImageSltScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.

                DrawBackground = false;
            }
            else
                DrawBackground = true;
            
            
            base.RenderInterFaceTile(gameTime);
        }

       
        // 11/04/2008 - Tile Clicked Event.
        /// <summary>
        /// <see cref="IFDTileSubGroupControl"/> selected event handler, which
        /// calls sets the current <see cref="IFDGroupControlType"/> as group to display.
        /// </summary>
        /// <remarks>A double-click will move <see cref="Camera"/> view to given <see cref="BuildingScene"/> item.</remarks>
        internal override void TileSelected()
        {
            // Set Group to Display           
            IFDTileManager.SetAsCurrentGroupToDisplay(ref _subQueueKey);
            
           
            // If tile already hasFocus, then 2nd click will move Camera View
            // to be over the 'BuildingScene' owner.
            if (_wasClicked == this && _buildingOwner != null)
            {               

                var tmpCameraTarget = Vector3.Zero;
                tmpCameraTarget.X = _buildingOwner.Position.X; 
                tmpCameraTarget.Y = 0; 
                tmpCameraTarget.Z = _buildingOwner.Position.Z;

                Camera.CameraTarget = tmpCameraTarget;
            }
            else
                _wasClicked = this;


            base.TileSelected();
        }

        
    }
}
