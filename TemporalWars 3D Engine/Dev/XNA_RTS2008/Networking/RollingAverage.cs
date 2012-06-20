#region File Description
//-----------------------------------------------------------------------------
// RollingAverage.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace TWEngine.Networking
{
    /// <summary>
    /// To compensate for network Latency, we need to know exactly how late each
    /// packet is. Trouble is, there is no guarantee that the clock will be set the
    /// same on every machine! The sender can include packet data indicating what
    /// Time their clock showed when they sent the packet, but this is meaningless
    /// unless our local clock is in sync with theirs. To compensate for any clock
    /// skew, we maintain a rolling Average of the send times from the last 100
    /// incoming packets. If this Average is, say, 50 milliseconds, but one specific
    /// packet arrives with a Time difference of 70 milliseconds, we can deduce this
    /// particular packet was delivered 20 milliseconds later than usual.
    /// </summary>
    class RollingAverage
    {
        #region Fields

        // Array holding the N most recent sample values.
        readonly float[] _sampleValues;

        // Counter indicating how many of the _sampleValues have been filled up.
        int _sampleCount;

        // Cached sum of all the valid _sampleValues.
        float _valueSum;

        // Write Position in the _sampleValues array. When this reaches the end,
        // it wraps around, so we overwrite the oldest samples with newer data.
        int _currentPosition;

        #endregion


        /// <summary>
        /// Constructs a new <see cref="RollingAverage"/> object that will track
        /// the specified number of sample values.
        /// </summary>
        /// <param name="sampleCount">Sample count</param>
        public RollingAverage(int sampleCount)
        {
            _sampleValues = new float[sampleCount];
        }

        // 10/2/2008: Updated to use ForLoop, rather than ForEach.
        /// <summary>
        /// Adds a new value to the <see cref="RollingAverage"/>, automatically
        /// replacing the oldest existing entry.
        /// </summary>
        public void AddValue(float newValue)
        {
            // To avoid having to recompute the sum from scratch every Time
            // we add a new sample value, we just subtract out the value that
            // we are replacing, then add in the new value.
            _valueSum -= _sampleValues[_currentPosition];
            _valueSum += newValue;

            // Store the new sample value.
            _sampleValues[_currentPosition] = newValue;

            // Increment the write Position.
            _currentPosition++;

            // Track how many of the _sampleValues elements are filled with valid data.
            if (_currentPosition > _sampleCount)
                _sampleCount = _currentPosition;

            // If we reached the end of the array, wrap back to the beginning.
            var length = _sampleValues.Length;
            if (_currentPosition < length) return;
            _currentPosition = 0;

            // The trick we used at the top of this method to update the sum
            // without having to recompute it from scratch works pretty well to
            // keep the Average efficient, but over Time, floating point rounding
            // errors could accumulate enough to cause problems. To prevent that,
            // we recalculate from scratch each Time the counter wraps.
            _valueSum = 0;

            // 10/2/2008: Ben - Updated to use ForLoop, rather than ForEach.
            for (var i = 0; i < length; i++)
            {
                _valueSum += _sampleValues[i];
            }
            
        }


        /// <summary>
        /// Gets the current value of the rolling Average.
        /// </summary>
        public float AverageValue
        {
            get
            {
                if (_sampleCount == 0)
                    return 0;
                
                return _valueSum / _sampleCount;
            }
        }
    }
}
