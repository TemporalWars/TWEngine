using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if XBOX
using ImageNexus.BenScharbach.TWLate.Xbox360Generics;
#endif


namespace ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText
{
    // 10/15/2008
    /// <summary>
    /// The <see cref="ScreenTextManager"/> class is used to batch draw
    /// several <see cref="ScreenTextItem"/> instance at once.
    /// </summary>
    public sealed class ScreenTextManager : DrawableGameComponent
    {
        // 4/22/2010 - Static ref of game
        private static Game _gameInstance;

        // 4/22/2010 - The camera interface
        internal static ICamera ScreenTextCamera;

        // SpriteBatch Reference
        private static SpriteBatch _spriteBatch;

        // 11/11/09: Updated to Dictionary, rather than List.
        // Holds a List of the ScreenTextItems to Display
        private static Dictionary<int, ScreenTextItem> _textItems;
        
        // 5/27/2010: SortedList collection, used to hold the items to be drawn; those which
        //            have the 'Visibility' set to true.
        private static SortedList<int, ScreenTextItem> _textItemToDraw;

        // 11/11/09: SpeedCollection unique key per item.
        private static int _textItemsKey;

        // 11/11/09 - Used in DrawScreenTextItems method.
        private static int[] _keys = new int[1];

        // 4/22/2010 - Default Font content path.
        private static string _defaultFontPath = @"Content\Fonts\Arial8";

        #region Properties

        // 4/22/2010 - General Font for all 'ScreenTextItem'
        internal static SpriteFont Font { get; private set; }


        #endregion

        ///<summary>
        /// Contructor for the <see cref="ScreenTextDisplayer"/> class, which
        /// creates the internal collection of 'ScreenTextItems'.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="defaultFontPath">(Optional) The draw <see cref="SpriteFont"/> content location to load from.</param>
        public ScreenTextManager(Game game, string defaultFontPath)
            : base(game)
        {           
            // 4/21/2010 - Save static ref.
            _gameInstance = game;
            
            // 4/22/2010 - Save defaultFontPath
            if (!string.IsNullOrEmpty(defaultFontPath))
                _defaultFontPath = defaultFontPath;

            // 5/27/2010 - Refactored into new method.
            InitializeCollections();

            // 11/7/2008
            DrawOrder = 200;

        }

        // 5/27/2010
        /// <summary>
        /// Method helper, used to initialize the internal collections.
        /// </summary>
        private static void InitializeCollections()
        {
            // 5/13/2009
            if (_textItems == null)
                _textItems = new Dictionary<int, ScreenTextItem>(300);  
         
            // 5/27/2010
            if (_textItemToDraw == null)
                _textItemToDraw = new SortedList<int, ScreenTextItem>(50);
        }

        // 4/22/2010
        /// <summary>
        /// Initializes the component. Override this method to load any non-graphics resources and query for any required services.
        /// </summary>
        public override void Initialize()
        {
            // Set Global SpriteBatch from Game.Services
            _spriteBatch = (SpriteBatch)Game.Services.GetService(typeof(SpriteBatch));

            // 4/22/2010 - Get IScreenTextCamera interface
            ScreenTextCamera = (ICamera)Game.Services.GetService(typeof(ICamera));

            base.Initialize();
        }

        // 4/22/2010
        /// <summary>
        /// Loads the general <see cref="SpriteFont"/> for use with all <see cref="ScreenTextItem"/> structures.
        /// </summary>
        protected override void LoadContent()
        {
            // 4/22/2010 - Load general 'Font' to use for screen text.
            Font = Game.Content.Load<SpriteFont>(_defaultFontPath);

            base.LoadContent();
        }
        
        /// <summary>
        /// Adds a new <see cref="ScreenTextItem"/> to the internal collection, which will be displayed
        /// onto the screen during each <see cref="SpriteBatch"/> draw call.
        /// </summary>
        /// <param name="drawText">Text to draw to screen</param>
        /// <param name="drawLocation"><see cref="Vector2"/> location to draw text</param>
        /// <param name="drawColor"><see cref="Color"/> for text</param>
        /// <param name="textItem">(OUT) <see cref="ScreenTextItem"/>, which allows for direct updating between draw calls.</param>       
        public static void AddNewScreenTextItem(string drawText, Vector2 drawLocation, Color drawColor, out ScreenTextItem textItem)
        {
            // 1st - Create new instance of ScreenTextItem.
            textItem = new ScreenTextItem(drawText, drawLocation, drawColor);

            // 5/27/2010 - Refactored into new method.
            InitializeCollections();

            // 2nd - get index of SceneItemOwner
            textItem.IndexInArray = _textItemsKey++; //_textItems.Count - 1; 

            // 3rd - Add to SpeedCollection Array
            _textItems.Add(textItem.IndexInArray, textItem);
            

        }

        // 11/11/09: Overload version.
        /// <summary>
        /// Adds a new <see cref="ScreenTextItem"/> to the internal collection, which will be displayed
        /// onto the screen during each <see cref="SpriteBatch"/> draw call.
        /// </summary>
        /// <remarks>This overload version takes the <see cref="Vector3"/> <paramref name="drawLocation"/> parameter, and converts 
        /// to the proper screen <see cref="Vector2"/> position.</remarks>
        /// <param name="drawText">Text to draw to screen</param>
        /// <param name="drawLocation"><see cref="Vector3"/> location to draw text</param>
        /// <param name="drawColor"><see cref="Color"/> for text</param>
        /// <param name="textItem">(OUT) <see cref="ScreenTextItem"/>, which allows for direct updating between draw calls.</param>       
        public static void AddNewScreenTextItem(string drawText, Vector3 drawLocation, Color drawColor, out ScreenTextItem textItem)
        {
            // 4/22/2010 - Convert 3D world position into Screen 2D position.
            Vector2 drawLocation2D;
            ScreenTextCamera.ProjectToScreen(ref drawLocation, out drawLocation2D);

            // 1st - Create new instance of ScreenTextItem.
            textItem = new ScreenTextItem(drawText, drawLocation2D, drawColor);

            // 5/27/2010 - Refactored into new method.
            InitializeCollections();

            // 2nd - get index of SceneItemOwner
            textItem.IndexInArray = _textItemsKey++; //_textItems.Count - 1; 

            // 3rd - Add to SpeedCollection Array
            _textItems.Add(textItem.IndexInArray, textItem);

        }

        // 4/21/2009
        /// <summary>
        /// Allows updating a <see cref="ScreenTextItem"/> instance.
        /// </summary>
        /// <param name="screenTextItem"><see cref="ScreenTextItem"/> instance</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="screenTextItem"/> is not valid.</exception>
        public static void UpdateScreenTextItem(ScreenTextItem screenTextItem)
        {
            // 11/11/09
            if (screenTextItem.IndexInArray < 0 && screenTextItem.IndexInArray > _textItemsKey)
                throw new ArgumentOutOfRangeException("screenTextItem", @"The given index is outside the bounds of the Dictionaires Key.");
            
            // directly update the array
            _textItems[screenTextItem.IndexInArray] = screenTextItem;
            
        }

        // 5/27/2010
        /// <summary>
        /// Checks the given <see cref="ScreenTextItem"/> 'Visible' attribute to determine
        /// if the item is to be in the Render <see cref="_textItemToDraw"/> collection.
        /// </summary>
        /// <param name="screenTextItem"><see cref="ScreenTextItem"/> instance</param>
        internal static void UpdateScreenTextItemForDrawing(ScreenTextItem screenTextItem)
        {
            // Add or remove depending on visibility setting.
            if (screenTextItem.Visible)
            {
                // visible, so add to draw list.
                if (!_textItemToDraw.ContainsKey(screenTextItem.IndexInArray))
                    _textItemToDraw.Add(screenTextItem.IndexInArray, screenTextItem);
            }
            else
            {
                // NOT visible, so remove from draw list.
                if (_textItemToDraw.ContainsKey(screenTextItem.IndexInArray))
                {
                    _textItemToDraw.Remove(screenTextItem.IndexInArray);
                }
            }
        }

        /// <summary>
        /// Removes a <see cref="ScreenTextItem"/> from the internal collection.
        /// </summary>
        /// <param name="screenTextItem"><see cref="ScreenTextItem"/> instance</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="screenTextItem"/> is not valid.</exception>
        public static void RemoveScreenTextItem(ScreenTextItem screenTextItem)
        {
            // 11/11/09
            if (screenTextItem.IndexInArray < 0 && screenTextItem.IndexInArray > _textItemsKey)
                throw new ArgumentOutOfRangeException("screenTextItem", @"The given index is outside the bounds of the Dictionaires Key.");
           
            _textItems.Remove(screenTextItem.IndexInArray);

        }

        /// <summary>
        /// The <see cref="Draw"/> method is automatically called each cycle, which in
        /// turn calls the internal <see cref="DrawScreenTextItems"/> method.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
#if DEBUG
            // 5/26/2010 - DEBUG
            //StopWatchTimers.StartStopWatchInstance(StopWatchName.GameDrawLoop_Main_ScreenText);
#endif
            // 4/21/2010 - Refactored out into STATIC method.
            DrawScreenTextItems();

            base.Draw(gameTime);


#if DEBUG
            // 5/26/2010 - DEBUG
            //StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.GameDrawLoop_Main_ScreenText);
#endif
        }

       
        // 4/21/2010; 5/27/2010: Updated to now use the SortedList dictionary for the drawing loop, which is more efficient!
        /// <summary>
        /// Helper method, which draws the <see cref="ScreenTextItem"/>, using the <see cref="SpriteBatch"/> batch draw.
        /// </summary>
        private static void DrawScreenTextItems()
        {
            // 4/21/2010 - Cache
            var spriteBatch = _spriteBatch;
            var screenTextItems = _textItemToDraw; // 5/27/2010: Updated to use new SortedList.

            try // 1/2/2010
            {
                // XNA 4.0 Changes
                //spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.None); // 1/5/2010 - Add 2nd 2 params.
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

                // Resize _keys array, if too small.
                var keyCollection = screenTextItems.Keys; // 8/13/2009
                var itemTypesKeysCount = keyCollection.Count; // 8/13/2009

                if (_keys.Length < itemTypesKeysCount)
                    Array.Resize(ref _keys, itemTypesKeysCount);

                keyCollection.CopyTo(_keys, 0);
               
                // Iterate through the List to draw the current 'ScreenTextItems'.
                for (var i = 0; i < itemTypesKeysCount; i++)
                {
                    var textItem = _textItemToDraw[_keys[i]];

                    // 11/11/09
                    if (!textItem.Visible) continue;

                    // 3/28/2009 - Check if font null.
                    if (textItem.Font == null)
                    {
                        // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
                        textItem.Font = Font; // 4/22/2010 - Use Default font.
                        screenTextItems[_keys[i]] = textItem;
                    }

                    // 5/13/2009: Updated to directly pass the StringBuilder instance into the SpriteBatch.
                    // 11/13/2008 - Updated to only draw _textItems with 'Visible' true.
                    spriteBatch.DrawString(textItem.Font, textItem.SbDrawText,
                                            textItem.DrawLocation, textItem.DrawColor);

                }

                spriteBatch.End();

            }
            // 1/2/2010 - Captures the SpriteBatch InvalidOpExp.
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(Draw) threw the 'InvalidOpExp' error, in the ScreenTextDisplayer class.");
            }
            // 4/21/2010 - Captures the NullRefExp error, and check if the _spriteBatch is Null.
            catch(NullReferenceException)
            {
                Debug.WriteLine("DrawScreenTextItems method, of the 'ScreenTextDisplay' class, threw the NullReference exception error.", "NullReferenceException");

                // 10/31/2008 - If SpriteBatch Null, try to get from Game.Services.
                if (_spriteBatch == null)
                {
                    _spriteBatch = (SpriteBatch)_gameInstance.Services.GetService(typeof(SpriteBatch));

                    Debug.WriteLine("The '_spriteBatch' instance was null; however, got new instance from services.", "NullReferenceException");
                }
            } // End Try-Catch
        }

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spriteBatch = null;
            }

            base.Dispose(disposing);
        }

    }
}