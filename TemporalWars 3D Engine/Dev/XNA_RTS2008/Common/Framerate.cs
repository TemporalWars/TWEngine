#region File Description
//-----------------------------------------------------------------------------
// FPS.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Using Statements

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText;
using Microsoft.Xna.Framework;

#endregion

namespace ImageNexus.BenScharbach.TWEngine.Common
{
    /// <summary>
    /// The <see cref="FPS"/> (Frames Per Second) component, used to show the
    /// games current FPS when running, for debug purposes.
    /// </summary>
// ReSharper disable InconsistentNaming
    public sealed class FPS : DrawableGameComponent, IFPS
// ReSharper restore InconsistentNaming
    {
        private const float UpdateInterval = 1.0f;
        private float _timeSinceLastUpdate;
        private float _framecount;

        // 6/4/2008 - Add ScreenText Class
        private readonly ScreenTextItem _screenTextHeader;
        private readonly ScreenTextItem _screenTextValue;

        // 6/16/2010 - To reduce GC on heap from strings, add int->string conversions in dictionary for resuse.
        private readonly Dictionary<int, string> _stringHelper = new Dictionary<int, string>(100);
          

        #region Properites

        ///<summary>
        /// Header draw location
        ///</summary>
        public Vector2 HeaderDrawLocation
        {
            get { return _screenTextHeader.DrawLocation; }
            set 
            {
                _screenTextHeader.DrawLocation = value;            
            }
        }

        ///<summary>
        /// The <see cref="FPS"/> value draw location
        ///</summary>
        public Vector2 FpsDrawLocation
        {
            get { return _screenTextValue.DrawLocation; }
            set 
            {
                _screenTextValue.DrawLocation = value;            
                    
            }
        }

        ///<summary>
        /// Color to draw header with
        ///</summary>
        public Color HeaderDrawColor
        {
            get { return _screenTextHeader.DrawColor; }
            set 
            {
                _screenTextHeader.DrawColor = value;
                     
            }
        }

        ///<summary>
        /// Color to draw <see cref="FPS"/> value with
        ///</summary>
        public Color FpsDrawColor
        {
            get { return _screenTextValue.DrawColor; }
            set 
            {
                _screenTextValue.DrawColor = value;
            
            }
        }    

        ///<summary>
        /// True/False to show <see cref="FPS"/> component on screen.
        ///</summary>
        public bool IsVisible
        {
            get { return Visible; }
            set { Visible = value; }
        }

        // 1/21/2009 - shortcut version
        ///<summary>
        /// True/False to show <see cref="FPS"/> component on screen.
        ///</summary>
        /// <remarks>This is shortcut for <see cref="IsVisible"/> property.</remarks>
        public bool V
        {
            get { return Visible; }
            set { Visible = value; }
        }

        // 1/19/2009
        ///<summary>
        /// Current <see cref="FPS"/> value
        ///</summary>
        public static int Fps { get; private set; }   

        #endregion
       
        ///<summary>
        /// Constructor to create the <see cref="FPS"/> class instance, using
        /// the color 'White' for default color value.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public FPS(Game game) : base(game)
        {

            ScreenTextManager.AddNewScreenTextItem(string.Empty, Vector2.Zero, Color.White, out _screenTextHeader);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, Vector2.Zero, Color.White, out _screenTextValue);

            // 5/27/2010 - Must set explicitly to draw!
            _screenTextHeader.Visible = true;
            _screenTextValue.Visible = true;

            // 8/28/2008 - Make DrawOrder higher
            DrawOrder = 490;

            // 5/11/2009 - Set Header
            _screenTextHeader.DrawText = "FPS: ";        
        
        } 
    
        /// <summary>
        /// Updates the framecounter and time elapsed value, used in the <see cref="FPS"/> calculation.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
       
            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _framecount++;
            _timeSinceLastUpdate += elapsed;
        
            base.Update(gameTime);
        }

        /// <summary>
        /// Calculates the <see cref="FPS"/> values, using the internal frameCounter
        /// and elapsedTime, and draws to screen the current value.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {        
            if (_timeSinceLastUpdate > UpdateInterval)
            {
                Fps = (int)(_framecount / _timeSinceLastUpdate);   
         
                // 6/16/2010 - Builds strings into dictionary to reduce GC on heap.
                if (!_stringHelper.ContainsKey(Fps))
                    _stringHelper.Add(Fps, Fps.ToString());

                // 8/7/2009
                _screenTextValue.SbDrawText.Length = 0;
                _screenTextValue.SbDrawText.Append(_stringHelper[Fps]); // 6/16/2010 - Updated to use Dictionary.
           

#if XBOX360           
                //System.Diagnostics.Debug.WriteLine(sb.ToString());
#else
                //Game.Window.Title = sb.ToString();           
#endif
                _framecount = 0;
                _timeSinceLastUpdate -= UpdateInterval;
           
            }  


            base.Draw(gameTime);
        }

        // 4/5/2009 - Dispose of objects

        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _screenTextHeader.Dispose();           
            }

            base.Dispose(disposing);
        }
    }
}