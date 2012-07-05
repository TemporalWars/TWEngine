using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace ScreenTextDisplayer.ScreenText
{
    // NOTE: MUST remain as class, since referenced now in two collections!
    // 5/27/2010: Converted back to class, rather than struct, so it can be used in 2 collections at once!
    // 4/21/2009: Updated to be a struct, rather than a class.
    ///<summary>
    /// The <see cref="ScreenTextItem"/> structure stores the text to be displayed
    /// on screen, using the <see cref="ScreenTextDisplayer"/> class to batch draw.  
    ///</summary>
    /// <remarks>
    /// Internally, this structure stores the text using a <see cref="StringBuilder"/> for efficiency of text updates,
    /// which reduced Heap garbage; thereby, increasing performance on the Xbox-360.
    /// </remarks>
    public class ScreenTextItem : IDisposable
    {
        private SpriteFont _font;
        private StringBuilder _sbDrawText; // 5/13/2009
        private Vector2 _drawLocation;
        private Color _drawColor;
        private bool _visible;
        internal int IndexInArray; // 4/21/2009

        #region Properties

        /// <summary>
        /// <see cref="SpriteFont"/> to use for current <see cref="ScreenTextItem"/>.
        /// </summary>
        public SpriteFont Font
        {
            get { return _font; }
            set 
            { 
                _font = value;
                ScreenTextManager.UpdateScreenTextItem(this);
            }
        }

        /// <summary>
        /// Text to draw to screen, using the <see cref="StringBuilder"/> class for storage.
        /// </summary>
        public StringBuilder SbDrawText
        {
            get { return _sbDrawText; }
            
        }

        /// <summary>
        /// Text to draw to screen.
        /// </summary>
        public string DrawText
        {
            get { return _sbDrawText.ToString(); }
            set 
            { 
                
                // 8/24/2009
                _sbDrawText.Length = 0;
                _sbDrawText.Append(value);
                //_sbDrawText.Insert(0, value);

                ScreenTextManager.UpdateScreenTextItem(this);
            }
        }

        /// <summary>
        /// Screen <see cref="Vector2"/> location to draw item.
        /// </summary>
        public Vector2 DrawLocation
        {
            get { return _drawLocation; }
            set 
            { 
                _drawLocation = value;
                ScreenTextManager.UpdateScreenTextItem(this);
            }

        }

        // 11/11/2009
        /// <summary>
        /// World <see cref="Vector3"/> position to draw, which will be
        /// converted automatically to screen <see cref="Vector2"/> position.
        /// </summary>
        public Vector3 DrawLocationFrom3D
        {
            set
            {
                // 4/22/2010 - Convert 3D world position into Screen 2D position.
                ScreenTextManager.ScreenTextCamera.ProjectToScreen(ref value, out _drawLocation);

                ScreenTextManager.UpdateScreenTextItem(this);
            }
        }
       
        /// <summary>
        /// <see cref="Color"/> to use for text.
        /// </summary>
        public Color DrawColor
        {
            get { return _drawColor; }
            set 
            { 
                _drawColor = value;
                ScreenTextManager.UpdateScreenTextItem(this);
            }
        }
       
        /// <summary>
        /// Draw this text <see cref="ScreenTextItem"/>?  Default=true.
        /// </summary>
        public bool Visible
        {
            get { return _visible; }
            set 
            { 
                _visible = value;
                ScreenTextManager.UpdateScreenTextItem(this);
            
                // 5/27/2010 - Update the SortedList for drawing.
                ScreenTextManager.UpdateScreenTextItemForDrawing(this);
            }
        }

        #endregion


        ///<summary>
        /// Constructor for <see cref="ScreenTextItem"/>, which sets the given values.
        ///</summary>
        ///<param name="drawText">Text to draw to screen</param>
        ///<param name="drawLocation"><see cref="Vector2"/> location to draw text</param>
        ///<param name="drawColor"><see cref="Color"/> to use for text</param>
        public ScreenTextItem(string drawText, Vector2 drawLocation, Color drawColor)
        {
            // 5/13/2009 - Create StringBuilder 
            _sbDrawText = new StringBuilder(drawText);

            // Save ScreenText data
            _font = ScreenTextManager.Font; // 4/22/2010 - Set default font.                   
            _drawLocation = drawLocation;
            _drawColor = drawColor;
            IndexInArray = 0;
            _visible = true;

        }        
        

        // 8/15/2008 - Dispose of Resources
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {   
            // First remove Ref from List Array
            ScreenTextManager.RemoveScreenTextItem(this);                       
 
            // Null Refs
            _font = null;
            _sbDrawText = null;
        }               
        
    }
}
