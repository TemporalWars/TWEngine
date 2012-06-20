#region File Description
//-----------------------------------------------------------------------------
// TerrainAreaSelect.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.SceneItems;
using TWEngine.Terrain.Enums;
using TWEngine.TerrainTools;

namespace TWEngine.Terrain
{
    /// <summary>
    /// The <see cref="TerrainAreaSelect"/> class is used to create the selection lasso rectangle, which is used
    /// during game play to select on screen <see cref="SceneItem"/> units.
    /// </summary>
    public class TerrainAreaSelect : DrawableGameComponent
    {       
        // 1/8/2010 - Content Manager
        private readonly ContentManager _contentManager;

        // 4/15/2008 - Draws Selection Rectangle
        private static SpriteBatch _spriteBatch;
        private static Texture2D _mDottedLineTexture;
        private static Vector2 _startSelect = new Vector2(0, 0);
        private static Vector2 _mousePos = new Vector2(0, 0);
        private static Rectangle _mSelectionBox;
        // 8/22/2008 - Temp Rectangle used in the Drawing section
        private static Rectangle _tmpDrawRectangle;
        private static Rectangle _tmpDrawRectangle2;

        #region Properties

        ///<summary>
        /// Tracks when the <see cref="TerrainAreaSelect"/> action has started.
        ///</summary>
        public static bool AreaSelect { get; set; }

        ///<summary>
        /// Set the <see cref="Vector2"/> screen location for the start of the area select rectangle;
        /// this will be the top-left corner of the rectangle.
        ///</summary>
        public static Vector2 StartSelect
        {
            set { _startSelect = value; }
        }

        // 5/17/2010: Renamed form 'MousePos' to 'CursorPos', since Xbox uses a gamepad.
        ///<summary>
        /// Current <see cref="Vector2"/> cursor position for the area select rectangle; this will
        /// be the bottom-right corner of the rectangle.
        ///</summary>
        public static Vector2 CursorPos
        {
            set { _mousePos = value; }
        }

        ///<summary>
        /// Returns the area selection <see cref="Rectangle"/>.
        ///</summary>
        public static Rectangle SelectionBox
        {
            get { return _mSelectionBox; }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public TerrainAreaSelect(Game game)
            : base(game)
        {                  
            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            // 1/8/2010 - Content Manager
            _contentManager = new ContentManager(game.Services, TemporalWars3DEngine.ContentTexturesLoc); // was "ContentTextures"

            //Iniitliaze the Selection box's rectangle. Currently no selection box is drawn
            //so set it's x and y Position to -1 and it's height and width to 0
            _mSelectionBox = new Rectangle(-1, -1, 0, 0);

            // 8/28/2008 - Set Draw Order
            DrawOrder = 110;
        }

        // 3/14/2009
        /// <summary>
        /// Loads Graphics Content via the Content Manager.
        /// </summary>
        protected sealed override void LoadContent()
        {
            base.LoadContent();

            // 9/11/2008 - Updated to get the Global SpriteBatch from Game.Services.
            _spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));
            
            // 11/2/2009: Updated to load from game.Content, rather than terrainShape.TextureContent.
            _mDottedLineTexture = _contentManager.Load<Texture2D>(@"Textures\DottedLine");


        }

        // 1/8/2010
        /// <summary>
        /// Unloads Graphics Content via the Content Manager.
        /// </summary>
        protected override void UnloadContent()
        {
            _contentManager.Unload();

            base.UnloadContent();
        }

        /// <summary>
        /// Draws the <see cref="TerrainAreaSelect"/> rectangle.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_AreaSelect);
#endif
            // Draw Area Select Red Rectangle
            if (TerrainEditRoutines.ToolInUse == ToolType.None)
                DrawAreaSelectRectangle();

            base.Draw(gameTime);

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_AreaSelect);
#endif
        }

        // 5/30/2009: Updated to be STATIC.
        // 4/15/2008: Draws Selection Rectangle on screen  
        /// <summary>
        /// Helper method, which draws selection rectangle to screen.
        /// </summary>
        private static void DrawAreaSelectRectangle()
        {
            // Check if AreaSelect ON
            if (!AreaSelect) return;

            var recWidth = (int)(_mousePos.X - _startSelect.X);
            var recHeight = (int)(_mousePos.Y - _startSelect.Y);               
            _mSelectionBox.X = (int)_startSelect.X; _mSelectionBox.Y = (int)_startSelect.Y;
            _mSelectionBox.Width = recWidth; _mSelectionBox.Height = recHeight;

            // XNA 4.0 Updates
            // 3/21/2009
            //_spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);                

            //Draw the horizontal portions of the selection box 
            var lineAdj = _mSelectionBox.Y - 5;
            DrawHorizontalLine(lineAdj);
            lineAdj = (_mSelectionBox.Y + 5) + _mSelectionBox.Height;
            DrawHorizontalLine(lineAdj);

            //Draw the vertical portions of the selection box 
            DrawVerticalLine(_mSelectionBox.X);
            lineAdj = _mSelectionBox.X + _mSelectionBox.Width;
            DrawVerticalLine(lineAdj);

            // 3/21/2009
            _spriteBatch.End();
        }

        // 5/30/2009: Updated to be STATIC.
        /// <summary>
        /// Helper method, which draws the horizontal lines of the area select rectangle.
        /// </summary>
        /// <param name="thePositionY">Y position of line</param>
        private static void DrawHorizontalLine(int thePositionY)
        {
            //When the width is greater than 0, the user is selecting an area to the right of the starting point
            var spriteBatch = _spriteBatch; // 5/17/2010 - Cache
            var colorWhite = Color.White; // 5/17/2010 - Cache
            if (_mSelectionBox.Width > 0)
            {
                //Draw the line starting at the startring location and moving to the right
                var width = _mSelectionBox.Width - 5; // 5/17/2010 - Cache calc.
                for (var aCounter = 0; aCounter <= width; aCounter += 5)
                {
                    if (_mSelectionBox.Width - aCounter < 0) continue;

                    // 8/22/2008
                    _tmpDrawRectangle.X = _mSelectionBox.X + aCounter;
                    _tmpDrawRectangle.Y = thePositionY;
                    _tmpDrawRectangle.Width = 5;
                    _tmpDrawRectangle.Height = 2;

                    //batch.Draw(_mDottedLineTexture, new Rectangle(_mSelectionBox.X + aCounter, thePositionY, 5, 2), Color.White);
                    spriteBatch.Draw(_mDottedLineTexture, _tmpDrawRectangle, colorWhite);
                }
            }
                //When the width is less than 0, the user is selecting an area to the left of the starting point
            else if (_mSelectionBox.Width < 0)
            {
                //Draw the line starting at the starting location and moving to the left
                for (var aCounter = -5; aCounter >= _mSelectionBox.Width; aCounter -= 5)
                {
                    if (_mSelectionBox.Width - aCounter > 0) continue;

                    // 8/22/2008
                    _tmpDrawRectangle.X = _mSelectionBox.X + aCounter;
                    _tmpDrawRectangle.Y = thePositionY;
                    _tmpDrawRectangle.Width = 5;
                    _tmpDrawRectangle.Height = 2;

                    //batch.Draw(_mDottedLineTexture, new Rectangle(_mSelectionBox.X + aCounter, thePositionY, 5, 2), Color.White);
                    spriteBatch.Draw(_mDottedLineTexture, _tmpDrawRectangle, colorWhite);
                }
            }
        }

        // 5/30/2009: Updated to be STATIC.
        /// <summary>
        /// Helper method, which draws the vertical lines of the area select rectangle.
        /// </summary>
        /// <param name="thePositionX">X position of line</param>
        private static void DrawVerticalLine(int thePositionX)
        {
            // 5/17/2010 - Store const 90 degrees as radians.
            const float degrees90AsRadian = 90.0f*(6.28f/360.0f);

            //When the height is greater than 0, the user is selecting an area below the starting point
            var spriteBatch = _spriteBatch; // 5/17/2010 - Cache
            var colorWhite = Color.White; // 5/17/2010 - Cache
            var vector2Zero = Vector2.Zero; // 5/17/2010 - Cache
            if (_mSelectionBox.Height > 0)
            {
                //Draw the line starting at the starting location and moving down
                for (var aCounter = -5; aCounter <= _mSelectionBox.Height; aCounter += 5)
                {
                    if (_mSelectionBox.Height - aCounter < 0) continue;

                    // 8/22/2008
                    _tmpDrawRectangle.X = thePositionX;
                    _tmpDrawRectangle.Y = _mSelectionBox.Y + aCounter;
                    _tmpDrawRectangle.Width = 5;
                    _tmpDrawRectangle.Height = 2;

                    _tmpDrawRectangle2.X = 0;
                    _tmpDrawRectangle2.Y = 0;
                    _tmpDrawRectangle2.Width = _mDottedLineTexture.Width;
                    _tmpDrawRectangle2.Height = _mDottedLineTexture.Height;

                    //batch.Draw(_mDottedLineTexture, new Rectangle(thePositionX, _mSelectionBox.Y + aCounter, 5, 2), new Rectangle(0, 0, _mDottedLineTexture.Width, _mDottedLineTexture.Height), Color.White, MathHelper.ToRadians(90), new Vector2(0, 0), SpriteEffects.None, 0);
                    spriteBatch.Draw(_mDottedLineTexture, _tmpDrawRectangle, _tmpDrawRectangle2, colorWhite, degrees90AsRadian, vector2Zero, SpriteEffects.None, 0);
                }
            }
            //When the height is less than 0, the user is selecting an area above the starting point
            else if (_mSelectionBox.Height < 0)
            {
                //Draw the line starting at the start location and moving up
                for (var aCounter = 0; aCounter >= _mSelectionBox.Height; aCounter -= 5)
                {
                    if (_mSelectionBox.Height - aCounter > 0) continue;

                    // 8/22/2008
                    _tmpDrawRectangle.X = thePositionX;
                    _tmpDrawRectangle.Y = _mSelectionBox.Y + aCounter;
                    _tmpDrawRectangle.Width = 5;
                    _tmpDrawRectangle.Height = 2;

                    _tmpDrawRectangle2.X = 0;
                    _tmpDrawRectangle2.Y = 0;
                    _tmpDrawRectangle2.Width = _mDottedLineTexture.Width;
                    _tmpDrawRectangle2.Height = _mDottedLineTexture.Height;

                    //batch.Draw(_mDottedLineTexture, new Rectangle(thePositionX, _mSelectionBox.Y + aCounter, 5, 2), new Rectangle(0, 0, _mDottedLineTexture.Width, _mDottedLineTexture.Height), Color.White, MathHelper.ToRadians(90), new Vector2(0, 0), SpriteEffects.None, 0);
                    spriteBatch.Draw(_mDottedLineTexture, _tmpDrawRectangle, _tmpDrawRectangle2, colorWhite, degrees90AsRadian, vector2Zero, SpriteEffects.None, 0);
                }
            }
        }


        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_mDottedLineTexture != null)
                    _mDottedLineTexture.Dispose();

                _spriteBatch = null;          
                   
            }

            base.Dispose(disposing);
        }
    }
}