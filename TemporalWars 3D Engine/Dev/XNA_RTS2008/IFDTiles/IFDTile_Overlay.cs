#region File Description
//-----------------------------------------------------------------------------
// IFDTileOverlay.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.BeginGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles
{    

    // 10/06/2008: Created
    /// <summary>
    /// This <see cref="IFDTileOverlay"/> tile, inherited from the base <see cref="IFDTile"/>,
    /// is used to only show some <see cref="Texture2D"/> overlay.   
    /// </summary>
    public sealed class IFDTileOverlay : IFDTile
    {
        // 2/23/2011 - DisplayTime of overlay.
        private float _displayTime;

        private bool _useDisplayTime;

        #region Properties

        ///<summary>
        /// Amount of time to display the <see cref="IFDTileOverlay"/>.
        ///</summary>
        public float DisplayTime
        {
            get { return _displayTime; }
            set 
            {
                _useDisplayTime = true;
                _displayTime = value;
            }
        }

        #endregion

        /// <summary>
        /// Constructor for creating a <see cref="IFDTileOverlay"/> tile.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="tileTextureName">Name of <see cref="Texture2D"/> to show</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> location to display tile</param>
        public IFDTileOverlay(Game game, string tileTextureName, Rectangle tileLocation)
            : base(game)
        {           

            // Load Texture Tile Given            
            MainImage = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>(tileTextureName);

            // We do not need the background drawn
            DrawBackground = false;

            // Default size for most Control Tiles
            TextureRectSize = new Rectangle(0, 0, 275, 275);
            
            IFDTileLocation.X = tileLocation.X; IFDTileLocation.Y = tileLocation.Y;
            TileRectCheck = tileLocation;           

        }

        // 2/23/2011 - Overload#1
        /// <summary>
        /// Constructor for creating a <see cref="IFDTileOverlay"/> tile.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="tileTexture">Instance of <see cref="Texture2D"/> to show</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> location to display tile</param>
        public IFDTileOverlay(Game game, Texture2D tileTexture, Rectangle tileLocation)
            : base(game)
        {

            // Set Texture Tile Given            
            MainImage = tileTexture;

            // We do not need the background drawn
            DrawBackground = false;

            // Default size for most Control Tiles
            TextureRectSize = new Rectangle(0, 0, 275, 275);

            IFDTileLocation.X = tileLocation.X; IFDTileLocation.Y = tileLocation.Y;
            TileRectCheck = tileLocation;

        }


        /// <summary>
        /// Renders this <see cref="IFDTileOverlay"/>.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>       
        public override void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;

            base.RenderInterFaceTile(gameTime);

            // 2/23/2011 - Check if doing timer countdown.
            if (!_useDisplayTime) return;

            // reduce displayTime
            _displayTime -= (float) gameTime.ElapsedGameTime.TotalMilliseconds;

            // check if zero
            if (DisplayTime > 0) return;

            DrawTile = false;
            _useDisplayTime = false;
        }

        
    }
}
