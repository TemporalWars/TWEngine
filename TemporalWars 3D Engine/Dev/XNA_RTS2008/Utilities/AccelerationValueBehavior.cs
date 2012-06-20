#region File Description
//-----------------------------------------------------------------------------
// AccelerationValueBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TWEngine.Utilities
{
    // 6/15/2012
    /// <summary>
    /// The <see cref="AccelerationValueBehavior"/> class returns a value which accelerates over-time.  This is useful
    /// for scenarios like a game Cursor speeding up the longer the user presses some button.
    /// </summary>
    internal class AccelerationValueBehavior
    {
        private float _delta;
        private float _acceleration;
        private readonly float _accelerationMagnitude; // 0.25f
        private bool _isStartState; // used to determine if in the start or stop state.

        #region Properties

        /// <summary>
        /// Gets or sets the current Max delta.
        /// </summary>
        public float DeltaMax { get; set; }

        /// <summary>
        /// Gets or sets to start/stop the acceleration behavior.
        /// </summary>
        internal bool IsStartState
        {
            get { return _isStartState; }
            set
            {
                // if transitioning from the off state, then reset the acceleration and delta.
                if (_isStartState == false)
                {
                    _acceleration = 0;
                    _delta = 0;
                }

                _isStartState = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes an instance of <see cref="AccelerationValueBehavior"/>.
        /// </summary>
        internal AccelerationValueBehavior(float deltaMax, float accelerationMagnitude)
        {
            DeltaMax = deltaMax;
            _accelerationMagnitude = accelerationMagnitude;
        }

        /// <summary>
        /// Returns the current delta value for the acceleration behavior.
        /// </summary>
        /// <returns><see cref="float"/> as current delta change.</returns>
        internal float GetDelta(GameTime gameTime)
        {
            // calc the current acceleration based on time.
            _acceleration += _accelerationMagnitude * (float)gameTime.ElapsedGameTime.TotalSeconds;
            // clamp the value between 0 - 100%
            MathHelper.Clamp(_acceleration, 0, 1);

            _delta = (DeltaMax * _acceleration) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            return _delta;
        }

    }
}
