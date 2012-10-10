using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWLate.RTS_StatusBarComponentLibrary.StatusBars
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="StatusBars"/> namespace contains the classes
    /// which make up the entire <see cref="StatusBar"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// The <see cref="StatusBar"/> or Health Bar is a game mechanic used in computer
    /// and video games to give value to characters, enemies, NPCs, and related objects.
    /// </summary>
    public class StatusBar : DrawableGameComponent, IStatusBar
    {
        // 1/3/2010 - Inteface refs
        private static ICamera _camera; // 4/22/2010 - Updated to use 'ICamera' directly.

        // SpriteBatch made Static, since we do Batch Draw calls, and want the
        // this SpriteBatch to apply to all instances of this class!
        private static Texture2D _energyOffSymbol;
        private static SpriteBatch _spriteBatch;

        // 7/4/2008 - Status Bar Texture Colors
        private static Texture2D _statusBarContainer;
        private static Texture2D _statusBarGreen;
        private static List<StatusBarItem> _statusBarItems;
        private static Texture2D _statusBarOrange;
        private static Texture2D _statusBarRed;
        private static Texture2D _statusBarYellow;
        private static bool _texturesLoaded;
        private static Matrix _spriteScale;
        private static readonly Vector2 Vector2Zero = Vector2.Zero;
        private static readonly Matrix MatrixIdentity = Matrix.Identity;
        private static readonly Color WhiteColor = Color.White;
        private static Game _gameInstance;

        
        /// <summary>
        /// Default Parameterless contructor, required for the LateBinding on Xbox.
        /// </summary>
        public StatusBar()
            : base(null)
        {
            // XBOX will call the CommonInitilization from the game engine!
        }

        /// <summary>
        /// Creates a single <see cref="StatusBar"/> instance.
        /// </summary>
        /// <param name="game">Provides a game instance</param>
        public StatusBar(Game game)
            : base(game)
        {
            // 1/3/2010
            CommonInitilization(game);
        }

        // 1/3/2010
        /// <summary>
        /// Set to capture the NullRefExp Error, which will be thrown by the base class, since the
        /// <see cref="Game"/> instance was not able to be set for the Xbox LateBinding version.
        /// </summary>
        public override void Initialize()
        {
            // Set to capture the NullRefExp Error, which will be thrown by base, since the
            // Game instance was not able to be set for the Xbox LateBinding version!
            try
            {

                base.Initialize();
            }
            catch (Exception)
            {
                // Make sure LoadContent is called; usually called in base 'Init' method.
                LoadContent();
                return;
            }

        }

        // 1/3/2010
        /// <summary>
        /// <see cref="CommonInitilization"/> routines.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public void CommonInitilization(Game game)
        {
            // 1/3/2010
            _gameInstance = game;

            _statusBarItems = new List<StatusBarItem>(50);

            // 1/3/2010 - Get Camera Interface
            _camera = (ICamera)_gameInstance.Services.GetService(typeof(ICamera));

            // 12/5/2008 - Create Matrix Scaler for SpriteBatch
            {
                // Idea is to divide by some constant, which I believe is the Default Screen Width; therefore, when
                // the resolution is higher, then the ScreenScale > 1 and if lower, then < 1.
                //float screenScaleZ = (float)Game.GraphicsDevice.Viewport.Width / 1280f; // Assuming default width = 1280.               
                // Create the scale Transform for Draw. 
                // Do not scale the sprite depth (Z=1).
                _spriteScale = Matrix.CreateScale(1, 1, 1);
            }

            // Set Draw Order
            DrawOrder = 113;
        }

        // 12/5/2008
        /// <summary>
        /// Loads graphic resource content.
        /// </summary>
        protected override sealed void LoadContent()
        {
            // Set up SpriteBatch 
            if (_spriteBatch == null)
                _spriteBatch = (SpriteBatch) _gameInstance.Services.GetService(typeof (SpriteBatch));
            

            // 8/27/2008 - Check if Static Textures already loaded.
            if (!_texturesLoaded)
            {
                // 4/6/2010 - Updated to use 'ContentMiscLoc' engine global var.
                var contentMiscLoc = ((IStatusBarEngineRef)_gameInstance).ContentMiscLoc;

                _statusBarContainer = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\statusBarContainer");
                _statusBarGreen = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\statusBarGreen");
                _statusBarYellow = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\statusBarYellow");
                _statusBarOrange = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\statusBarOrange");
                _statusBarRed = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\statusBarRed");

                // 1/30/2009
                _energyOffSymbol = _gameInstance.Content.Load<Texture2D>(contentMiscLoc + @"\StatusBars\EnergyOff");

                _texturesLoaded = true;
            }

            base.LoadContent();
        }

        /// <summary>
        /// Drawing the <see cref="StatusBar"/>
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        public override sealed void Draw(GameTime gameTime)
        {
            // 4/21/2010 - Refactored out into its own method.
            DrawStatusBars();

            base.Draw(gameTime);
        }

        // 4/21/2010
        /// <summary>
        /// Helper method, which renders the individual <see cref="StatusBarItem"/>.
        /// </summary>
        private static void DrawStatusBars()
        {
            // 4/21/2010 - Cache
            var spriteBatch = _spriteBatch;
            var statusBarItems = _statusBarItems;
            var gameInstance = _gameInstance;
            var spriteScale = _spriteScale;

            try // 1/1/2010
            {
                // XNA 4.0 changes.
                // 12/5/2008 - Add '_spriteScale' as 4th parameter.
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.BackToFront, SaveStateMode.None, spriteScale); // 8/18/2009 - Was SaveState
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointWrap,
                    DepthStencilState.Default, null, null, spriteScale);

                // 11/24/2008 - Iterate through internal StatusBarItems array
                var count = statusBarItems.Count; // 8/18/2009
                for (var i = 0; i < count; i++)
                {
                    // 8/18/2009 - Cache
                    var statusBarItem = statusBarItems[i];

                    // Skip SceneItemOwner if not 'InUse'.
                    if (!statusBarItem.InUse) continue;
                    
                    RenderStatusBar(gameInstance, ref statusBarItem);
                    statusBarItems[i] = statusBarItem;
                }

                spriteBatch.End();

            }
                // 1/1/2010 - Capture the InvalidOpExp error, thrown when the SpriteBatch 'Begin' is called, before the 'End' was called.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(Draw) threw the 'InvalidOpExp' error, in the StatusBar class.");
            }
                // 4/21/2010 - Captures the NullRefExp error, and check if the '_spriteBatch' is null.
            catch (NullReferenceException)
            {
                Debug.WriteLine("Draw method, of StatusBar class, thread the NullReferenceException error.", "NullReferenceException");

                // 4/5/2009
                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch)gameInstance.Services.GetService(typeof(SpriteBatch));

                    Debug.WriteLine("The '_spriteBatch' was null; however, updated with new instance from services.");
                }
            }
        }

        // 11/24/2008
        /// <summary>
        /// Adds a <see cref="StatusBarItem"/> instance to display.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="IStatusBarSceneItem"/> Owner </param>
        /// <param name="statusBarItem">(OUT)<see cref="StatusBarItem"/> instance </param>
        public static void AddNewStatusBarItem(IStatusBarSceneItem sceneItemOwner, out StatusBarItem statusBarItem)
        {
            // 1st - Create new StatusBarItem instance
            statusBarItem = new StatusBarItem(sceneItemOwner);

            // 2nd - Add to List Array
            _statusBarItems.Add(statusBarItem);

            // 3rd - get index of SceneItemOwner
            statusBarItem.IndexInArray = _statusBarItems.Count - 1;
        }

        // 11/24/2008
        /// <summary>
        /// Removes a <see cref="StatusBarItem"/> instance from the display, by simply changing the
        /// internal flag to 'InUse=False'.
        /// </summary>
        /// <param name="statusBarItem"><see cref="StatusBarItem"/> instance to remove</param>
        public static void RemoveStatusBarItem(ref StatusBarItem statusBarItem)
        {
            // set 'InUse' to false.
            if ((statusBarItem.IndexInArray + 1) > _statusBarItems.Count) return;

            var item = _statusBarItems[statusBarItem.IndexInArray];
            item.InUse = false;
            _statusBarItems[statusBarItem.IndexInArray] = item;
        }

        // 4/15/2009
        /// <summary>
        /// Allows updating a <see cref="StatusBarItem"/> instance.
        /// </summary>
        /// <param name="statusBarItem"><see cref="StatusBarItem"/> to update</param>
        public static void UpdateStatusBarItem(ref StatusBarItem statusBarItem)
        {
            // directly update the array
            if (statusBarItem.IndexInArray != -1 && _statusBarItems.Count > 0)
                _statusBarItems[statusBarItem.IndexInArray] = statusBarItem;
        }

        // 11/24/2008
        /// <summary>
        /// Clears out all <see cref="StatusBarItem"/> from the internal collection.
        /// </summary>
        public static void ClearAllStatusBarItems()
        {
            // 4/21/2010 - Cache
            var statusBarItems = _statusBarItems;
            var count = statusBarItems.Count; 

            // Itereate through array, and clear
            for (var i = 0; i < count; i++)
            {
                var statusBarItem = statusBarItems[i];
                statusBarItem.SceneItemOwner = null;
                statusBarItems[i] = statusBarItem;
            }
            statusBarItems.Clear();
        }

        // 8/27/2008: Updated to optimize memory.    
        // 7/4/2008
        /// <summary>
        /// Draws the <see cref="StatusBar"/> for current <see cref="IStatusBarSceneItem"/> by using two sprite <see cref="Texture2D"/>.
        /// 1st is the container <see cref="Texture2D"/>, and the 2nd is the overlay bar; which comes
        /// in green, yellow, orange, and red.  The <see cref="SpriteBatch"/> is set to use 'BackToFront' <see cref="Effect"/>
        /// in order to see the overlay sprite over the container sprite!
        /// </summary>
        /// <remarks>This is called from the Screen Class.</remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="statusBarItem"><see cref="StatusBarItem"/> structure</param>
        private static void RenderStatusBar(Game game, ref StatusBarItem statusBarItem)
        {
            // 8/15/2008 - Don't draw if SpriteBatch was Disposed.
            var spriteBatch = _spriteBatch; // 4/21/2010 - Cache
            if (spriteBatch == null) return;

            // 4/21/2010 - Cache
            var graphicsDevice = game.GraphicsDevice;
            var camera = _camera;
            var origin = Vector2Zero;

            // Using ViewPort.Project, we will convert the SceneItemOwner's World Position to Screen Cordinates.
            var projectedPosition = graphicsDevice.Viewport.Project(statusBarItem.StatusBarWorldPosition,
                                                                    camera.Projection, camera.View,
                                                                    MatrixIdentity);

            // 4/10/2009 - Updated to use the Vector2.Add overload, to optimize on XBOX!
            var projectedPosition2D = new Vector2 {X = projectedPosition.X, Y = projectedPosition.Y};

            // Set the Screen Position for Status Bar using Project Results.
            //StatusBarItem.StatusBarPosition2D.X = StatusBarItem.StatusBarProjectPosition.X + StatusBarItem.StatusBarOffsetPosition2D.X;
            //StatusBarItem.StatusBarPosition2D.Y = StatusBarItem.StatusBarProjectPosition.Y + StatusBarItem.StatusBarOffsetPosition2D.Y;
            var tmpOffsetPosition = statusBarItem.StatusBarOffsetPosition2D;
            Vector2.Add(ref projectedPosition2D, ref tmpOffsetPosition, out statusBarItem.StatusBarPosition2D);
            var statusBarPosition2D = statusBarItem.StatusBarPosition2D; // 4/21/2010

            // 1/30/2009 - Does this SceneItemOwner show the EnergyOff Symbol?
            if (statusBarItem.ShowEnergyOffSymbol &&
                ((IStatusBarEngineRef)_gameInstance).Players[statusBarItem.SceneItemOwner.PlayerNumber].EnergyOff)
            {
                spriteBatch.Draw(_energyOffSymbol, statusBarPosition2D, null, WhiteColor
                                 , 0, origin, 1, SpriteEffects.None, 0); // Last Parameter, 0 = front.
            }

            // Check if we should draw Status bar
            // 11/17/2008: Add PickSelected check.
            // 12/10/2008: Add PickHovered check.
            if (!statusBarItem.DrawStatusBar || (!statusBarItem.SceneItemOwner.PickSelected && !statusBarItem.SceneItemOwner.PickHovered))
                return;

            // Update the StatusBarShape depending on given values
            var healthPercent = (statusBarItem.StatusBarCurrentValue/statusBarItem.StatusBarStartValue);

            var statusBarShape = statusBarItem.StatusBarShape; // 1/3/2010 - Extract StatusBar Struct
            statusBarShape.Width = (int)(statusBarItem.StatusBarContainerShape.Width * healthPercent);
            statusBarItem.StatusBarShape = statusBarShape; // 1/3/2010 - Save StatusBar Struct

            spriteBatch.Draw(_statusBarContainer, statusBarPosition2D,
                              statusBarItem.StatusBarContainerShape, WhiteColor
                              , 0, origin, 1, SpriteEffects.None, 1); // Last Parameter, 1 = back.

            // Draw the Proper Color Texture depending on Health Condition
            if (healthPercent >= 0.75f)
            {
                spriteBatch.Draw(_statusBarGreen, statusBarPosition2D, statusBarShape,
                                  WhiteColor
                                  , 0, origin, 1, SpriteEffects.None, 0); // Last Parameter, 0 = front.
            }
            else if (healthPercent >= 0.50f)
            {
                spriteBatch.Draw(_statusBarYellow, statusBarPosition2D, statusBarShape,
                                  WhiteColor
                                  , 0, origin, 1, SpriteEffects.None, 0); // Last Parameter, 0 = front.
            }
            else if (healthPercent >= 0.25f)
            {
                spriteBatch.Draw(_statusBarOrange, statusBarPosition2D, statusBarShape,
                                  WhiteColor
                                  , 0, origin, 1, SpriteEffects.None, 0); // Last Parameter, 0 = front.
            }
            else if (healthPercent >= 0)
            {
                spriteBatch.Draw(_statusBarRed, statusBarPosition2D, statusBarShape,
                                  WhiteColor
                                  , 0, origin, 1, SpriteEffects.None, 0); // Last Parameter, 0 = front.
            }
        }

        #region IStatusBar Interface
        

        // 1/3/2010 - Interface Ref
        /// <summary>
        /// Adds a <see cref="IStatusBarSceneItem"/> instance to display.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="IStatusBarSceneItem"/> Owner</param>
        /// <param name="statusBarItem">(OUT) <see cref="IStatusBarItem"/> Item</param>
        public void AddNewStatusBarItem(IStatusBarSceneItem sceneItemOwner, out IStatusBarItem statusBarItem)
        {
            StatusBarItem newStatusBarItem;
            AddNewStatusBarItem(sceneItemOwner, out newStatusBarItem);

            statusBarItem = newStatusBarItem;
        }

        /// <summary>
        /// Removes a <see cref="IStatusBarSceneItem"/> instance from the display, by simply changing the
        /// internal flag to 'InUse=False'.
        /// </summary>
        /// <param name="statusBarItem"><see cref="IStatusBarSceneItem"/> reference to remove</param>
        public void RemoveStatusBarItem(ref IStatusBarItem statusBarItem)
        {
            if (statusBarItem == null) return;

            var item = (StatusBarItem)statusBarItem;
            RemoveStatusBarItem(ref item);
        }

        // 1/3/2010 - Interface Ref
        /// <summary>
        /// Allows updating a <see cref="IStatusBarItem"/> instance.
        /// </summary>
        /// <param name="statusBarItem"><see cref="IStatusBarItem"/> to update</param>
        public void UpdateStatusBarItem(ref IStatusBarItem statusBarItem)
        {
            if (statusBarItem == null) return;

            var item = (StatusBarItem)statusBarItem;
            UpdateStatusBarItem(ref item);
        }

        // 1/3/2010 - Interface Ref
        /// <summary>
        /// Clears out all <see cref="IStatusBarItem"/> from the internal collection.
        /// </summary>
        void IStatusBar.ClearAllStatusBarItems()
        {
            ClearAllStatusBarItems();
        }

        #endregion


        // 4/5/2009 - 
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose
                if (_spriteBatch != null)
                    _spriteBatch.Dispose();
                if (_statusBarContainer != null)
                    _statusBarContainer.Dispose();
                if (_statusBarGreen != null)
                    _statusBarGreen.Dispose();
                if (_statusBarYellow != null)
                    _statusBarYellow.Dispose();
                if (_statusBarOrange != null)
                    _statusBarOrange.Dispose();
                if (_statusBarRed != null)
                    _statusBarRed.Dispose();
                if (_energyOffSymbol != null)
                    _energyOffSymbol.Dispose();

                // Null Refs           
                _spriteBatch = null;
                _statusBarContainer = null;
                _statusBarGreen = null;
                _statusBarYellow = null;
                _statusBarOrange = null;
                _statusBarRed = null;
                _energyOffSymbol = null;

                // Reset
                _texturesLoaded = false;
            }

            base.Dispose(disposing);
        }
        
    }
}