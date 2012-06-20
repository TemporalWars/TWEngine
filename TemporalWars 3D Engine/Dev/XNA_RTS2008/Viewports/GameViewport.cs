#region File Description
//-----------------------------------------------------------------------------
// GameViewPort.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Interfaces;
using TWEngine.Shadows;
using TWEngine.Viewports.Structs;

namespace TWEngine.Viewports
{

    /// <summary>
    /// The <see cref="GameViewPort"/> class is used to draw some game view port on screen.  This is
    /// useful to draw textures for debug purposes, like the <see cref="ShadowMap"/> texture.
    /// </summary>
    public class GameViewPort : DrawableGameComponent, IGameViewPort
    {
        private static SpriteBatch _spriteBatch;

        // 6/26/2009 - Init List, staring with space for 5.
        private static readonly List<GameViewPortItem> GVPItems = new List<GameViewPortItem>(5);
        
        
        #region Properties
        /// <summary>
        /// Returns a reference to <see cref="GameViewPort"/> 
        /// </summary>
        public GameViewPort GVP
        {
            get { return this; }
        }
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="GameViewPort"/> instances.
        ///</summary>
        public bool IsVisible
        {
            get { return Visible; }
            set { Visible = value; }
        }
        ///<summary>
        /// Set to turn On/Off rendering of all <see cref="GameViewPort"/> instances.
        ///</summary>
        public bool V
        {
            get { return Visible; }
            set { Visible = value; }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public GameViewPort(Game game)
            : base(game)
        {   
           
            // Default to not showing gameViewPort; needs to be explicitly turned on.
            Visible = false;

            // 4/2/2009 - Set Drawing Order
            DrawOrder = 500;
        }

        // 4/9/2009
        protected sealed override void LoadContent()
        {
            // 9/11/2008 - Set Global SpriteBatch from Game.Services
            _spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            base.LoadContent();
        }

        // 8/14/2008
        protected sealed override void UnloadContent()
        {            

            base.UnloadContent();
        }
       

        // 6/26/2009
        /// <summary>
        /// Adds a new <see cref="GameViewPortItem"/>, with the given texture and rectangle viewport size to use.  Returns the
        /// GameViewPortItem populated with the given information, and adds to the internal collection.
        /// </summary>
        /// <param name="texture"><see cref="Texture2D"/> to draw.</param>
        /// <param name="rectangle"><see cref="GameViewPortItem"/> size to draw texture in.</param>
        /// <param name="gameViewPortItem">(OUT) <see cref="GameViewPortItem"/></param>
        public static void AddNewGameViewPortItem(Texture2D texture, Rectangle rectangle, out GameViewPortItem gameViewPortItem)
        {
            // 1st - Create new GameViewPortItem
            gameViewPortItem = new GameViewPortItem
                                   {
                                       RectSize = rectangle,
                                       Texture = texture,
                                       InUse = false
                                       // 3/23/2010 - Should be defaulted to False; otherwise, all items will try to draw.
                                   };

            // 2nd - Add to List Array
            GVPItems.Add(gameViewPortItem);

            // 3rd - get index of SceneItemOwner
            gameViewPortItem.IndexInArray = GVPItems.Count - 1;   

            
        }

        // 6/26/2009
        /// <summary>
        /// Removes a <see cref="GameViewPortItem"/> struct from the display, by simply changing the
        /// internal flag to 'InUse=False'.
        /// </summary>
        /// <param name="gameViewPortItem"><see cref="GameViewPortItem"/> to remove</param>
        public static void RemoveGameViewPortItem(ref GameViewPortItem gameViewPortItem)
        {
            // set 'InUse' to false.
            if ((gameViewPortItem.IndexInArray + 1) > GVPItems.Count) return;

            var item = GVPItems[gameViewPortItem.IndexInArray];
            item.InUse = false;
            GVPItems[gameViewPortItem.IndexInArray] = item;
        }

        // 6/26/2009
        /// <summary>
        /// Allows updating a <see cref="GameViewPortItem"/> struct.
        /// </summary>
        /// <param name="gameViewPortItem"><see cref="GameViewPortItem"/> to update</param>
        public static void UpdateGameViewPortItem(ref GameViewPortItem gameViewPortItem)
        {
            // directly update the array
            if (gameViewPortItem.IndexInArray != -1 && GVPItems.Count > 0)
                GVPItems[gameViewPortItem.IndexInArray] = gameViewPortItem;
        }

        /// <summary>
        /// Renders the <see cref="GameViewPortItem"/> structs which have the 'InUse' set to TRUE.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public sealed override void Draw(GameTime gameTime)
        {
            if (_spriteBatch == null)
                _spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            // 5/20/2010 - Refactored out core draw code to new STATIC method.
            DrawGameViewPorts();
        }

        // 5/20/2010
        /// <summary>
        /// Method helper, which iterates the internal collection of <see cref="GameViewPortItem"/> structs, drawing the instances which
        /// have the 'InUse' flag set to TRUE.
        /// </summary>
        private static void DrawGameViewPorts()
        {
            var spriteBatch = _spriteBatch; // 5/20/2010 - Cache
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 updates - Note: XNA Framework HiDef profile requires TextureFilter to be Point when using texture format Single.
            //spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Deferred, SaveStateMode.SaveState);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null);
            
            // Iterate through internal GameViewPortItems array
            var gameViewPortItems = GVPItems; // 5/20/2010 - Cache
            var count = gameViewPortItems.Count;
            for (var i = 0; i < count; i++)
            {
                var gvpItem = gameViewPortItems[i]; // 12/6/2009

                // Skip SceneItemOwner if not 'InUse'.
                if (!gvpItem.InUse) continue;

                // 7/6/20009 - make sure not null; 3/23/2010 - make sure 'Texture' not disposed.
                if (gvpItem.Texture != null && gvpItem.Texture.IsDisposed != true)
                    spriteBatch.Draw(gvpItem.Texture, gvpItem.RectSize, Color.White);

                gameViewPortItems[i] = gvpItem;
            }

            spriteBatch.End();

            graphicsDevice.Textures[0] = null;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
          
        }

        // 6/26/2009 - Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose Resource           
                if (GVPItems != null)
                {
                    foreach (var gvpItem in GVPItems)
                    {
                        if (gvpItem.Texture != null)
                            gvpItem.Texture.Dispose();
                    }
                    // 3/23/2010 - Clear Array
                    GVPItems.Clear();
                }

                // Null Refs
                _spriteBatch = null;
                
            }

            base.Dispose(disposing);
        }
    }
}