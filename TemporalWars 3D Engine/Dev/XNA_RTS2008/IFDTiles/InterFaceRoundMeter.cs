#region File Description
//-----------------------------------------------------------------------------
// InterFaceRoundMeter.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TWEngine.IFDTiles
{
    // 9/28/2008: Created
    /// <summary>
    /// The <see cref="InterFaceRoundMeter"/> overlay, which draws the 
    /// Round Meter counter, used to show build-progress.
    /// </summary>
    class InterFaceRoundMeter : IDisposable
    {
        private static SpriteBatch _spriteBatch;

        private static Texture2D _meterTexture;
        private static Effect _meterEffect;

        private readonly Vector2 _meterPosition;
        private readonly Vector4 _meterColor = new Vector4(125, 125, 125, 125); // Default
        private const float MeterScale = 1;
        private float _fullMeterValue = 4; // Default to 4 seconds     
        private float _accumMeterValue;

        internal bool RunCountdown;
        private static readonly Vector2 Vector2Zero = Vector2.Zero;
        private static readonly Color ColorWhite = Color.White;

        /// <summary>
        /// Value which represents a full meter
        /// </summary>
        public float FullMeterValue
        {
            get { return _fullMeterValue; }
            set { _fullMeterValue = value; }
        }

        /// <summary>
        /// Current Meter Percent (0 - 1)
        /// </summary>
        public float CurrentMeterValue { get; set; }

        /// <summary>
        /// Constructor for <see cref="InterFaceRoundMeter"/>, loading the required round meter textures.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="contentManager"><see cref="ContentManager"/> used to load textures</param>
        /// <param name="position"><see cref="Vector2"/> meter position</param>
        public InterFaceRoundMeter(Game game, ContentManager contentManager, Vector2 position)
        {
            _spriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            if (_meterEffect == null)
                _meterEffect = contentManager.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\RoundMeterEffect");
            if (_meterTexture == null)
                _meterTexture = TemporalWars3DEngine.ContentResourceManager.Load<Texture2D>("RoundMeterClock_75");

            _meterPosition = position;
            
        }

        /// <summary>
        /// Default empty constructor for <see cref="InterFaceRoundMeter"/>.
        /// </summary>
        public InterFaceRoundMeter() : this(TemporalWars3DEngine.GameInstance, TemporalWars3DEngine.GameInstance.Content, Vector2Zero)
        {
            return;
        }      

        
        /// <summary>
        /// Starts the meter Countdown
        /// </summary>
        public void StartCountdown()
        {
            // Set to zero
            _accumMeterValue = 0;

        }

        /// <summary>
        /// Updates the meter value percentage
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {
            if (!RunCountdown)
                return;

            // 1st - Get Increase Accum Meter by the elapsedtime
            _accumMeterValue += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 2nd - Get meter percentage, by dividing accumMeter by FullMeter value.
            CurrentMeterValue = _accumMeterValue / _fullMeterValue;
            
            CurrentMeterValue = MathHelper.Clamp(CurrentMeterValue, 0f, 1f);
           
        }

        /// <summary>
        /// Draws the <see cref="InterFaceRoundMeter"/>.
        /// </summary>
        public void Draw()
        {
            if (!RunCountdown)
                return;

            //Draw Meter
            _meterEffect.Parameters["meterValue"].SetValue(CurrentMeterValue);
            _meterEffect.Parameters["mColor"].SetValue(_meterColor);

            // 8/14/2009
            var effectPass = _meterEffect.CurrentTechnique.Passes[0];

            // XNA 4.0 Updates - Updated SpriteBatch.Begin() signature.
            //_spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_meterEffect.Begin();
            effectPass.Apply();
            _spriteBatch.Draw(_meterTexture, _meterPosition, null, ColorWhite, 0, Vector2Zero, MeterScale, SpriteEffects.None, 0);
            //effectPass.End();
            //_meterEffect.End();
            _spriteBatch.End();
            
        }


        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="finalDispose">Is this final dispose?</param>
        public static void Dispose(bool finalDispose)
        {
            if (finalDispose)
            {
                _spriteBatch = null;

                if (_meterEffect != null)
                    _meterEffect.Dispose();

                if (_meterTexture != null)
                    _meterTexture.Dispose();

                _meterEffect = null;
                _meterTexture = null;
            }
        }


        #endregion
    }
}
