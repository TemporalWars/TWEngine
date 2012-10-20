#region File Description
//-----------------------------------------------------------------------------
// Cursor.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common.Enums;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers;
using ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ImageNexus.BenScharbach.TWEngine.Common
{
    /// <summary>
    /// Cursor is a DrawableGameComponent that draws a Cursor on the screen. It works
    /// differently on Xbox and Windows. On windows, this will be a Cursor that is
    /// controlled using both the mouse and the gamepad. On Xbox, the Cursor will be
    /// controlled using only the gamepad.
    /// </summary>
    public sealed class Cursor : DrawableGameComponent, ICursor
    {
        #region Constants

        /// <summary>
        /// This constant controls how fast the gamepad moves the Cursor. This constant
        /// is in pixels per second.
        /// </summary>
        internal const float CursorSpeed = 750.0f; // was 250
      
        #endregion

        #region Fields and properties
       
        // used to draw the Cursor.
        private static SpriteBatch _spriteBatch; 
       
        // 5/1/2009 - Enum of CursorTexture to display
        /// <summary>
        /// Curser icon displayed relative to location.
        /// </summary>
        internal static CursorTextureEnum CursorTextureToDisplay = CursorTextureEnum.Normal;
        
        // this is the sprite that is drawn at the current Cursor Position.
        // textureCenter is used to center the sprite when drawing.
        private static Texture2D _cursorTextureNormal;
       
        // 5/1/2009 - Blocked Cursor Texture.
        private static Texture2D _cursorTextureBlocked;
        
        // Position is the Cursor Position, and is in screen space. 
// ReSharper disable InconsistentNaming
        /// <summary>
        /// Cursor position in screen space.
        /// </summary>
        private static Vector2 _position;
// ReSharper restore InconsistentNaming

        private static readonly Matrix MatrixIdentity = Matrix.Identity;
        private static Viewport _viewPort; // 8/18/2009
        private static Matrix _cameraProjection;
        private static Matrix _cameraView;
        private static readonly Vector2 Vector2Zero = Vector2.Zero;
        private static Color _colorTintingForNormalCursor = Color.White;
        private static Color _colorTintingForBlockingCursor = Color.White;

        #endregion

        // 7/10/2012
        /// <summary>
        /// Gets or sets to use the cursor. (Scripting Purposes)
        /// </summary>
        public bool UseCursor { get; set; }

        ///<summary>
        /// Cursor position in screen space.
        ///</summary>
        public static Vector2 Position
        {
            get { return _position;}
            set { _position = value; }
        }

        // 9-25-2010 - Interface implementation
        Vector2 ICursor.Position
        {
            get { return Position; }
            set { Position = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Sets the <see cref="Texture2D"/> to use for the Cursor.
        /// </summary>
        public static Texture2D CursorTextureNormal
        {
            set { _cursorTextureNormal = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Sets the <see cref="Texture2D"/> to use for the Cursor.
        /// </summary>
        Texture2D ICursor.CursorTextureNormal
        {
            set { _cursorTextureNormal = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Sets the <see cref="Texture2D"/> to use for the Blocked-Cursor.
        /// </summary>
        public static Texture2D CursorTextureBlocked
        {
            set { _cursorTextureBlocked = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Sets the <see cref="Texture2D"/> to use for the Blocked-Cursor.
        /// </summary>
        Texture2D ICursor.CursorTextureBlocked
        {
            set { _cursorTextureBlocked = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Gets or sets the <see cref="Color"/> TINT for the Cursor.
        /// </summary>
        public static Color ColorTintingForNormalCursor
        {
            get { return _colorTintingForNormalCursor; }
            set { _colorTintingForNormalCursor = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Gets or sets the <see cref="Color"/> TINT for the Cursor.
        /// </summary>
        Color ICursor.ColorTintingForNormalCursor
        {
            get { return _colorTintingForNormalCursor; }
            set { _colorTintingForNormalCursor = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Gets or sets the <see cref="Color"/> TINT for the Blocking-Cursor.
        /// </summary>
        public static Color ColorTintingForBlockingCursor
        {
            get { return _colorTintingForBlockingCursor; }
            set { _colorTintingForBlockingCursor = value; }
        }

        // 6/28/2012
        /// <summary>
        /// Gets or sets the <see cref="Color"/> TINT for the Blocking-Cursor.
        /// </summary>
        Color ICursor.ColorTintingForBlockingCursor
        {
            get { return _colorTintingForBlockingCursor; }
            set { _colorTintingForBlockingCursor = value; }
        }

        #region Creation and initialization
        
        ///<summary>
        /// Constructor for Cursor class, which simply sets
        /// drawOrder to 500, and saves the Camera's projection and 
        /// view matricies.
        ///</summary>
        ///<param name="game">Instance of <see cref="Game"/>.</param>
        public Cursor(Game game)
            : base(game) 
        {
            // 8/21/2008 - Make Drawing one of the last items, so over everything.
            DrawOrder = 500;
            
            // 8/20/2009
            _cameraProjection = Camera.Projection;
            _cameraView = Camera.View;

            // 8/20/2009 - Capture the Camera move event, which will update the view/proj varibles, thereby, eliminating
            //             the need to retrieve them every frame.
            Camera.CameraUpdated += CameraUpdated;
           
        }
        

        // 8/20/2009
        /// <summary>
        /// Captures the Camera move event, thereby, updating the internal View & Projection attributes.
        /// This eliminates the need to constanly retrieve the values from the Camera class.
        /// </summary>
        static void CameraUpdated(object sender, EventArgs e)
        {
            // 8/20/2009 - Update the internal Camera View/Proj atts.
            _cameraProjection = Camera.Projection;
            _cameraView = Camera.View;

        }
        
        /// <summary>
        /// Loads the Cursors 'Arrow' and 'Block' textures.
        /// </summary>
        protected override void LoadContent()
        {
            // 9/11/2008 - Set Global SpriteBatch from Game.Services
            _spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            if (_cursorTextureNormal == null) // 6/28/2012
                _cursorTextureNormal = TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\cursorArrow");

            if (_cursorTextureBlocked == null) // 6/28/2012
                CursorTextureBlocked = TemporalWars3DEngine.GameInstance.Content.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\BlockedArea");

            base.LoadContent();
        }

        // 8/18/2009
        /// <summary>
        /// Initializes the component; specifically, saves the GraphicDevice's ViewPort,
        /// and centers the cursor on the XBOX.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // 8/18/2009 - Store Viewport for quick access in CalculateCursorRay method.
            _viewPort = GraphicsDevice.Viewport;

            // on Xbox360, initialize is overriden so that we can center the Cursor once we
            // know how big the viewport will be.
#if XBOX360
            _position.X = _viewPort.X + (_viewPort.Width / 2);
            _position.Y = _viewPort.Y + (_viewPort.Height / 2);
#endif
        }

        #endregion
                
        #region Draw
        
        /// <summary>
        /// Draw is pretty straightforward: we'll Begin the SpriteBatch, Draw the Cursor,
        /// and then End.
        /// </summary>
        /// <param name="gameTime">GameTime instance</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            // 7/10/2012 - check usability (Scripting Purposes)
            //if (!UseCursor) return;

            // 7/10/2012 - check visibility
            if (!Visible) return;

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_Cursor);
#endif

            try // 1/2/2010
            {
                // 10/31/2008
                if (_spriteBatch == null)
                    _spriteBatch = (SpriteBatch) Game.Services.GetService(typeof (SpriteBatch));

                // XNA 4.0 Updates
                //_spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState); // 1/5/2010 - Add 2nd 2 params.
                _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend); // 1/5/2010 - Add 2nd 2 params.

                // 1/7/2009: Updated Origin to be Zero, instead of textureCenter; this corrects the problem
                //           of having to have the almost the entire arrow placed into an IFD tile, in order
                //           to activate it!  Now, since the texture is placed so the tip of the arrow is the
                //           Cursor 'Position', the player can more easily select their tiles!

                // 5/1/2009 - Display CursorTexture, depending on enum setting
                switch (CursorTextureToDisplay)
                {
                    case CursorTextureEnum.Normal:
                        _spriteBatch.Draw(_cursorTextureNormal, _position, null, ColorTintingForNormalCursor, 0.0f,
                                          Vector2Zero, 1.0f, SpriteEffects.None, 0.0f);
                        break;
                    case CursorTextureEnum.Blocked:
                        _spriteBatch.Draw(_cursorTextureBlocked, _position, null, ColorTintingForBlockingCursor, 0.0f,
                                          Vector2Zero, 1.0f, SpriteEffects.None, 0.0f);
                        break;
                    default:
                        break;
                }

                _spriteBatch.End();
            }
                // 1/2/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(Draw) threw the 'InvalidOpExp' error, in the ScreenTextDisplayer class.");
            }

#if DEBUG
            // 5/26/2010 - DEBUG
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_Cursor);
#endif

        }

        #endregion

        // 6/15/2012
        /// <summary>
        /// Called when the GameComponent needs to be updated. Override this method with component-specific update code.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to Update</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 7/10/2012 - check usability (Scripting Purposes)
            //if (!UseCursor) return;

            // 6/15/2012
            var inputState = HandleInput.InputState;
            if (inputState == null)
                return;

            // 11/27/2009 - When IFDTile displaying, then skip updating Camera position.  This should fix the
            //              error of the user selecting an item on XBOX, and then exiting menu and experiencing
            //              the jump off screen position; since the cursor position below, was continously being
            //              updated as the user held the stick to choose a menu item!
            if (IFDTileManager.IFDTileSetIsDisplaying)
                return;

            // 10/19/2009 - Cache cursor position.
            var cursorPosition = Position;

#if XBOX360
            // 12/17/2008: Removed Overload Ops, since slow on XBOX!
            // modify Position using delta, the CursorSpeed constant defined above, and
            // the elapsed game Time.            
            //Position += delta * (CursorSpeed * _accelerate) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //float cursorSpeedElapsed = (Cursor.CursorSpeed * _accelerate) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            //Vector2 tmpDelta; 
            //Vector2.Multiply(ref delta, cursorSpeedElapsed, out tmpDelta);
            //Vector2.Add(ref cursorPosition, ref tmpDelta, out cursorPosition);

            // clamp the _cursor Position to the viewport, so that it can't move off the
            // screen.
            Viewport vp = TemporalWars3DEngine.GameInstance.GraphicsDevice.Viewport;
            cursorPosition.X = MathHelper.Clamp(cursorPosition.X, vp.X, vp.X + vp.Width);
            cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, vp.Y, vp.Y + vp.Height);
#else

            cursorPosition.X = inputState.CurrentMouseState.X;
            cursorPosition.Y = inputState.CurrentMouseState.Y;

            if (TemporalWars3DEngine.GameInstance.IsActive)
            {
                // modify Position using delta, the CursorSpeed constant defined above,
                // and the elapsed game Time, only if the _cursor is on the screen
                /*var vp = TemporalWars3DEngine.GameInstance.GraphicsDevice.Viewport;
                if ((vp.X <= cursorPosition.X) && (cursorPosition.X <= (vp.X + vp.Width)) &&
                    (vp.Y <= cursorPosition.Y) && (cursorPosition.Y <= (vp.Y + vp.Height)))
                {
                    // 12/17/2008: Removed Overload Ops, since slow on XBOX!
                    //Position += delta * CursorSpeed *(float)gameTime.ElapsedGameTime.TotalSeconds;
                    var cursorSpeedElapsed = Cursor.CursorSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    Vector2 tmpDelta;
                    Vector2.Multiply(ref delta, cursorSpeedElapsed, out tmpDelta);

                    Vector2.Add(ref cursorPosition, ref tmpDelta, out cursorPosition);

                    cursorPosition.X = MathHelper.Clamp(cursorPosition.X, vp.X, vp.X + vp.Width);
                    cursorPosition.Y = MathHelper.Clamp(cursorPosition.Y, vp.Y, vp.Y + vp.Height);
                }
                else if (delta.LengthSquared() > 0f)
                {
                    cursorPosition.X = vp.X + vp.Width / 2;
                    cursorPosition.Y = vp.Y + vp.Height / 2;
                }*/

                // set the new mouse Position using the combination of mouse and gamepad
                // data.
                Mouse.SetPosition((int)cursorPosition.X, (int)cursorPosition.Y);
            }

#endif
            // 10/19/2009 - Store changes back into original
            Position = cursorPosition;
        }
       
        // 8/13/2009: Updated to Optimize memory.
        // 8/26/2008: Updated to optimize memory.
        ///<summary>
        /// Calculates a world space ray starting at the camera's
        /// "eye" and pointing in the direction of the cursor; Viewport.Unproject() is used
        /// to accomplish this. 
        ///</summary>
        ///<param name="cursorRay">(OUT) New cursor ray struct.</param>
        public static void CalculateCursorRay(out Ray cursorRay)
        {
            // create 2 positions in screenspace using the Cursor Position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.  
            var nearSource = new Vector3 {X = _position.X, Y = _position.Y, Z = 0f};

            var farSource = new Vector3 {X = _position.X, Y = _position.Y, Z = 1f};
            
            // use Viewport.Unproject to tell what those two screen space positions
            // would be in World space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a World
            // matrix, which can just be identity.
            var nearPoint = _viewPort.Unproject(nearSource, _cameraProjection, _cameraView, MatrixIdentity);

            var farPoint = _viewPort.Unproject(farSource, _cameraProjection, _cameraView, MatrixIdentity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....

            // 4/10/2009 - Optimize by using Vector.Subtract to optimize on XBOX!
            //direction = farPoint - nearPoint;
            Vector3 direction;     
            Vector3.Subtract(ref farPoint, ref nearPoint, out direction);
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.            
            cursorRay.Position = nearPoint;
            cursorRay.Direction = direction;
            
        }

        // 4/5/2009 - Dipose of objects
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of Resources            
                if (_cursorTextureNormal != null)
                    _cursorTextureNormal.Dispose();

                // 5/1/2009
                if (_cursorTextureBlocked != null)
                    _cursorTextureBlocked.Dispose();
                
                // Null Refs
                _spriteBatch = null;
                CursorTextureNormal = null;         

               
            }

            base.Dispose(disposing);
        }

       
    }
}