#region File Description
//-----------------------------------------------------------------------------
// IFDTile.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TWEngine.Audio;
using TWEngine.Audio.Enums;
using TWEngine.IFDTiles.Enums;
using TWEngine.Interfaces;
using TWEngine.HandleGameInput;

namespace TWEngine.IFDTiles
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.IFDTiles"/> namespace contains the common classes
    /// which make up the entire <see cref="IFDTiles"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 7/9/2008 - IFDTileManager Tile Class
    ///<summary>
    /// The <see cref="IFDTile"/> is the base class, providing the
    /// main functionality for a display tile.
    ///</summary>
    public class IFDTile
    {
        // Save Game Instance
       /// <summary>
        /// The instance of <see cref="Game"/>.
       /// </summary>
        protected static Game GameInstance;
        /// <summary>
        /// Instance of the <see cref="SpriteBatch"/>.
        /// </summary>
        protected static SpriteBatch SpriteBatch;

        // 11/4/2008 - Add Reference to _interfaceDisplay
        private static IFDTileManager _interfaceDisplay;

        // 11/4/2008 - Add Ref to Cursor Interface
        private static ICursor _cursor;

        // 11/4/2008 - Add EventHandler
        ///<summary>
        /// Occurs when a tile is selected.
        ///</summary>
        public event EventHandler TileSelectedEvent;

        // 10/6/2008 - IFDTile Unique Instance Key; allows for deleting.
        private static int _instanceKeyCounter;
   
        // 5/1/2009 - Is _cursor inside IFD Tile?
        /// <summary>
        /// Is screen cursor inside this tile?
        /// </summary>
        internal bool CursorInsideIFDTile;
      
        // 9/28/2008 - 
        /// <summary>
        /// Current <see cref="TileState"/> Enum State.
        /// </summary>
        private TileState _tileState = TileState.None;      

        // 9/23/2008 - Background Tiles, shown behind image.
        private static Texture2D _backgroundImageOff;
        private static Texture2D _backgroundImageDisabled; // 3/25/2009
        private static Texture2D _backgroundImageOn;
        private static Texture2D _backgroundImageRdy; 
      
        /// <summary>
        /// Uniform multiple by which to scale width and height.
        /// </summary>
        protected float BackgroundImageOnScale = 1;
        /// <summary>
        /// Uniform multiple by which to scale width and height.
        /// </summary>
        protected float BackgroundImageOffScale = 1;
        private const float BackgroundImageRdyScale = 1;

        /// <summary>
        /// The origon of the sprite.  Specify (0,0) for the upper left hand corner.
        /// </summary>
        protected Vector2 Origin = Vector2.Zero;

        // 8/13/2008 - Add ContentManager Instance
        // 11/17/2008 - Updated to be 'STATIC' variable, so textures won't get reloaded every Time
        //              a new tile is created!  This fixed huge 7-second lag seen every Time a new
        //              buildingScene was created and placed in XBOX World.
        public static ContentManager ContentManager;  
        
        // 10/6/2008 - Draw Background
        private bool _drawBackground = true;

        //      
        /// <summary>
        /// Tile Placement Type Attributes  
        /// </summary>
        private float _mainImageScale = 1;
        /// <summary>
        /// The screen location of the this <see cref="IFDTile"/>.
        /// </summary>
        protected Vector2 IFDTileLocation;
        /// <summary>
        /// The size of the <see cref="IFDTile"/>.
        /// </summary>
        protected Rectangle TextureRectSize;
        /// <summary>
        /// The size of the background texture.
        /// </summary>
        protected Rectangle BackgroundTextureRectSize;

        // 10/22/2009 - Flash On/Off - 
        /// <summary>
        /// This will show one of two colors, depending on state; true or false. (Scripting Purposes)
        /// Note: This only works when the Property 'FlashTile' is set to TRUE.
        /// </summary>
        private bool _flashState;

        // 10/22/2009 - Flash Timer & timer duration.
        private const float FlashTimerDuration = 400; // in millseconds.
        private float _flashTimer;
        private readonly Guid _uniqueKey = Guid.NewGuid(); // 6/10/2012

        #region Properties

        /// <summary>
        /// <see cref="IFDTile"/> unique instance key; allows for deleting.
        /// </summary>
        public int TileInstanceKey { get; private set; }

        /// <summary>
        /// Turn On/Off ability to draw this <see cref="IFDTile"/>.
        /// </summary>
        public bool DrawTile { protected get; set; }

        /// <summary>
        /// <see cref="Texture2D"/> image to use as the main image for this tile.
        /// </summary>
        protected Texture2D MainImage { get; set; }

        /// <summary>
        /// The <see cref="Vector2"/> position for this tile.
        /// </summary>
        public Vector2 IFDTileLocationP
        {
            get { return IFDTileLocation; }
        }

        /// <summary>
        /// The <see cref="Rectangle"/> area to use for the cursor selection checks.
        /// </summary>
        public Rectangle TileRectCheck { get; protected set; }

        /// <summary>
        /// Size of texture, given as <see cref="Rectangle"/>.
        /// </summary>
        public Rectangle TextureRectangleSize
        {
            private get { return TextureRectSize; }
            set { TextureRectSize = value; }
        }

        /// <summary>
        /// The current <see cref="TileState"/> for this tile.
        /// </summary>
        internal TileState TileState
        {
            get { return _tileState; }
            set { _tileState = value; }
        }    
  
        // 10/11/2009
        /// <summary>
        /// When set to FALSE, the <see cref="IFDTile"/> will not be useable, even
        /// if the 'TileState' is not set to 'Disabled'.  (Scripting purposes)
        /// </summary>
        public bool TileIsUseable { get; set; }

        // 10/22/2009
        /// <summary>
        /// When set, will cause the <see cref="IFDTile"/> to Flash yellow. (Scripting purposes)
        /// </summary>
        public bool FlashTile { get; set; }

        // 1/15/2010
        /// <summary>
        /// Set to turn On/Off the drawing of the <see cref="IFDTile"/> texture background.
        /// </summary>
        public bool DrawBackground
        {
            get { return _drawBackground; }
            set { _drawBackground = value; }
        }

        // 2/23/2011
        /// <summary>
        /// Tile's Scale. (0-1)
        /// </summary>
        public float MainImageScale
        {
            get { return _mainImageScale; }
            set
            {
                // Clamp value in range of 0-1.
                MathHelper.Clamp(value, 0, 1);
                _mainImageScale = value;
            }
        }

        /// <summary>
        /// The <see cref="Texture2D"/> to use for the background of message.
        /// </summary>
        public Texture2D BackgroundTexture { get; set; }

        #endregion
      
        /// <summary>
        /// Constructor for creating the generic <see cref="IFDTile"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>       
        protected IFDTile(Game game)
        {
            DrawTile = true;
            // 11/4/2008
            if (_interfaceDisplay == null)
                _interfaceDisplay = (IFDTileManager)game.Services.GetService(typeof(IFDTileManager));

            CommonInitialization(game);
           
            // 10/6/2008 - Create Unique Key for IFD Tile
            _instanceKeyCounter++;
            TileInstanceKey = _instanceKeyCounter;

            // 10/11/2009 - Set the new Property to TRUE - (Scripting purposes)
            TileIsUseable = true;

            // 10/22/2009 - Set the FlashTimer - (Scripting purposes)
            _flashTimer = FlashTimerDuration;

        }

        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="finalDispose">Is this final dispose?</param>
        public virtual void Dispose(bool finalDispose)
        {
            // 12/10/2008 - Tiles can be removed during gameplay, and
            //              therefore, the static variables should not
            //              be diposed of, unless it is a 'FinalDispose'!
            if (!finalDispose) return;

            // Dispose of Resources            
            if (_backgroundImageOff != null)
                _backgroundImageOff.Dispose();

            if (_backgroundImageOn != null)
                _backgroundImageOn.Dispose();

            if (_backgroundImageRdy != null)
                _backgroundImageRdy.Dispose();

            if (MainImage != null)
                MainImage.Dispose();

            // Null Refs
            GameInstance = null;               
            _backgroundImageOff = null;
            _backgroundImageOn = null;
            _backgroundImageRdy = null;
            MainImage = null;
            //SpriteBatch = null;

            if (ContentManager != null)
            {
                ContentManager.Unload();
                ContentManager.Dispose();
                ContentManager = null;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.  Currently does not perform any
        /// processes at the base level.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public virtual void Update(GameTime gameTime)
        {
            return;
        }

        /// <summary>
        /// Renders the <see cref="IFDTile"/>; also does the game input check here.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>       
        public virtual void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;

            // 11/4/2008 - Check for Input; this check could be put in 'Update', but then it would be checked for all Tiles, 
            //             but it only needs to be checked when visible; which is why 'HERE', in the 'Render' call.
           
            // 4/29/2009 - Call the proper IFDInputCheck, depending if PC or XBOX
#if XBOX360
            HandleInput.IFDInputCheckForXbox(this);
#else
            HandleInput.IFDInputCheckForPc(this);
            //HandleGameInput.IFDInputCheckForXbox(this);
#endif
           
            DrawPlacementTileType(gameTime);

            // 3/5/2011 - Draw Background Texture
            RenderBackgroundTexture();

        }

        // 3/5/2011
        ///<summary>
        /// Draws the IFDTile background, if any.
        ///</summary>
        public virtual void RenderBackgroundTexture()
        {
            //if (!DrawBackground) return;

            if (BackgroundTexture == null) return;

            // Draw IFDTile Background   
            // 3rd param was = TextureRectSize
            SpriteBatch.Draw(BackgroundTexture, IFDTileLocationP, null, Color.White
                   , 0, Vector2.Zero, 1, SpriteEffects.None, 1); // Last Parameter, 1 = back.
            
        }

        // 7/23/2008
        // 9/23/2008 - Updated to use AlphaBlend drawing, to draw a background tile and then
        //             the Image tile on top.
        // 9/28/2008 - Updated to use the 'TileState' Enum to know which background image to use.
        /// <summary>
        /// Draws the Generic <see cref="IFDTile"/> Type to screen.
        /// </summary> 
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private void DrawPlacementTileType(GameTime gameTime)
        {
            //  9/25/2008 - Only draw if Texture not Null
            //              This is can occur from the MessageTile, since it calls
            //              this base Draw automatically, even though it doesn't need it.
            if (MainImage == null)
                return;

            // 10/11/2009 - Check 'TileIsUseable' flag; if FALSE, then Tile will be OFF, regardless of
            //              actual 'TileState' settings!  (Scripting Purposes)
            if (!TileIsUseable)
            {
                // Then set to Grey for OFF
                SpriteBatch.Draw(_backgroundImageDisabled, IFDTileLocationP, BackgroundTextureRectSize, Color.DarkGray
                     , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.

                // Draw Image with Grey effect.            
                SpriteBatch.Draw(MainImage, IFDTileLocationP, TextureRectangleSize, Color.DarkGray
                        , 0, Origin, _mainImageScale, SpriteEffects.None, 0.5f); // Last Parameter, 0 = front. 

                return;
            }

            // 1st - Draw Background tiles depending on state.
            if (DrawBackground)
            {
                // 10/22/2009 - Check if Flash enabled. (Scripting Purposes)
                if (FlashTile)
                {
                    //System.Diagnostics.Debugger.Break();

                    // reduce flashTimer by gameTime.
                    _flashTimer -= gameTime.ElapsedGameTime.Milliseconds;

                    // check if ready to change flash color.
                    if (_flashTimer <= 0)
                    {
                        _flashState = !_flashState;
                        _flashTimer = FlashTimerDuration;
                    }

                    // Set Flash background color.
                    SpriteBatch.Draw(_backgroundImageOff, IFDTileLocationP, BackgroundTextureRectSize, (_flashState) ? Color.DarkBlue : Color.Yellow
                                            , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1);
                }
                else
                {
                    switch (_tileState)
                    {
                        case TileState.None:
                        case TileState.Countdown:
                        case TileState.WaitingForClearance:
                        case TileState.Paused:
                            SpriteBatch.Draw(_backgroundImageOff, IFDTileLocationP, BackgroundTextureRectSize, Color.White
                                             , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        case TileState.Queued:
                            SpriteBatch.Draw(_backgroundImageOff, IFDTileLocationP, BackgroundTextureRectSize, Color.DarkBlue
                                             , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        case TileState.Hovered:
                            SpriteBatch.Draw(_backgroundImageOn, IFDTileLocationP, BackgroundTextureRectSize, Color.White
                                             , 0, Origin, BackgroundImageOnScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        case TileState.Ready:
                            SpriteBatch.Draw(_backgroundImageRdy, IFDTileLocationP, BackgroundTextureRectSize, Color.White
                                             , 0, Origin, BackgroundImageRdyScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        case TileState.InsufficientFunds: // 1/5/2009
                            SpriteBatch.Draw(_backgroundImageOff, IFDTileLocationP, BackgroundTextureRectSize, Color.DarkRed
                                             , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        case TileState.Disabled: // 3/25/2009
                            SpriteBatch.Draw(_backgroundImageDisabled, IFDTileLocationP, BackgroundTextureRectSize, Color.DarkGray
                                             , 0, Origin, BackgroundImageOffScale, SpriteEffects.None, 1); // Last Parameter, 1 = back.
                            break;
                        default:
                            break;
                    } // End Switch

                } // End If Flash Enabled

            } // End IF Background

            // ***
            // 2nd - Draw Image tile
            // ***
            // Check if 'Queued' state.
            switch (TileState)
            {
                case TileState.Queued:
                    SpriteBatch.Draw(MainImage, IFDTileLocationP, TextureRectangleSize, Color.Gray
                                     , 0, Origin, _mainImageScale, SpriteEffects.None, 0.5f); // Last Parameter, 0 = front. 
                    break;
                case TileState.Disabled:
                    SpriteBatch.Draw(MainImage, IFDTileLocationP, TextureRectangleSize, Color.DarkGray
                                     , 0, Origin, _mainImageScale, SpriteEffects.None, 0.5f); // Last Parameter, 0 = front. 
                    break;
                default:
                    SpriteBatch.Draw(MainImage, IFDTileLocationP, TextureRectangleSize, Color.White
                                     , 0, Origin, _mainImageScale, SpriteEffects.None, 0.5f); // Last Parameter, 0 = front.  
                    break;
            }

        }

        // 9/23/2008 -
        /// <summary>
        /// Starts SriteBatch using the Alpha Spritemode method to overlay the IFD tiles.
        /// </summary>
        internal static void SpriteBatchBegin()
        {
            // 4/28/2010 - Add Try-Catch
            try
            {
                // XNA 4.0 Updates
                //SpriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None);
                SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("SpriteBatchBegin method, in IFDTile class, threw the NullRefExp error.");

                if (SpriteBatch == null)
                {
                    SpriteBatch = (SpriteBatch) GameInstance.Services.GetService(typeof (SpriteBatch));

                    Debug.WriteLine("The 'SpriteBatch' was null; however, got new instance from services.");
                }
                
            }

        }

        /// <summary>
        /// Ends the SpriteBatch draw call.
        /// </summary>
        internal static void SpriteBatchEnd()
        {
            // 4/28/2010 - Add Try-Catch
            try
            {
                SpriteBatch.End();
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("SpriteBatchEnd method, in IFDTile class, threw the NullRefExp error.");

                if (SpriteBatch == null)
                {
                    SpriteBatch = (SpriteBatch)GameInstance.Services.GetService(typeof(SpriteBatch));

                    Debug.WriteLine("The 'SpriteBatch' was null; however, got new instance from services.");
                }

            }
        }

        // 9/24/2008 - Tile Selected Event. 
        /// <summary>
        /// <see cref="IFDTile"/> selected event, which triggers the event <see cref="TileSelectedEvent"/>.
        /// </summary>
        internal virtual void TileSelected()
        {
            // 5/6/2009 - Play Menu_Click sound.
            AudioManager.Play(_uniqueKey, Sounds.Menu_Click);

            if (TileSelectedEvent != null)
                TileSelectedEvent(this, EventArgs.Empty);
        }

        // 9/28/2008 - Tile Canceled Event.
        /// <summary>
        /// <see cref="IFDTile"/> canceled event.  Inherting classes should
        /// override with their own logic.
        /// </summary>
        internal virtual void TileCanceled()
        {
            return;
        }

        // 9/25/2008 - Tile Hovered by Mouse/Gamepad Event.
        //             When tile is hovered, let's show the 
        //             Message Tile, if any.
        /// <summary>
        /// Tile Hovered by Mouse/Gamepad Event.
        /// </summary>
        /// <param name="isTileHovered">Is tile hovered?</param>
        internal virtual void TileHovered(bool isTileHovered)
        {
            // 4/2/2009 -Keeps the state from being changed.
            if (TileState == TileState.Disabled)
                return;

            // 10/11/2009 - Check 'TileIsUseable' flag; if FALSE, then Tile will be OFF, regardless of
            //              actual 'TileState' settings!  (Scripting Purposes)
            if (!TileIsUseable)
                return;

            // Make sure not in Countdown, Queued, Paused, waiting, InsufficentFunds or Ready modes.
            if (TileState != TileState.Countdown && TileState != TileState.Ready
                && TileState != TileState.Paused && TileState != TileState.Queued
                && TileState != TileState.WaitingForClearance && TileState != TileState.InsufficientFunds)
            {
                // 9/28/2008 - Set tile state
                TileState = isTileHovered ? TileState.Hovered : TileState.None;
               
            }
        }

        // 7/23/2008
        /// <summary>
        /// The <see cref="CommonInitialization"/> method creates the <see cref="ContentManager"/> for loading
        /// content into memory, as well as loading the default background textures.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        private static void CommonInitialization(Game game)
        {
            if (GameInstance == null)
                GameInstance = game;

            // 8/13/2008
            if (ContentManager == null)
                ContentManager = new ContentManager(game.Services, "");

            // 11/4/42008 - Set a Reference to the Interface for Cursor Class
            if (_cursor == null)                
                _cursor = (ICursor)game.Services.GetService(typeof(ICursor));

            // Set up SpriteBatch
            if (SpriteBatch == null)
            {
                // 9/11/20008 - Get Global SpriteBatch from Game.Services.
                SpriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));
                //SpriteBatch = new SpriteBatch(game.GraphicsDevice);
            }

            // 9/23/2008 - Load Static Background Tiles
            var contentResourceManager = TemporalWars3DEngine.ContentResourceManager;
            if (_backgroundImageOff == null)
                _backgroundImageOff = contentResourceManager.Load<Texture2D>(@"IFDTileBlueOff"); // @"ContentIFDTiles\InterfaceTiles\IFDTileBlueOff"
            if (_backgroundImageOn == null)
                _backgroundImageOn = contentResourceManager.Load<Texture2D>(@"IFDTileBlueOn");
            if (_backgroundImageRdy == null)
                _backgroundImageRdy = contentResourceManager.Load<Texture2D>(@"IFDTileBlueRdy");
            if (_backgroundImageDisabled == null) // 3/25/2009
                _backgroundImageDisabled = contentResourceManager.Load<Texture2D>(@"IFDTileGreyOff");


            
        }  
  
       
    }
}
