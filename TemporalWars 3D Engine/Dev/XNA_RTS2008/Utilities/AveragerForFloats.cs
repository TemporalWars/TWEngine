#region File Description
//-----------------------------------------------------------------------------
// AveragerForFloats.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;

namespace TWEngine.Utilities
{
    /// <summary>
    /// The <see cref="AveragerForFloats"/> class is used to average floats, keeping only the 'SampleSize'
    /// as the total Count in the internal collection.
    /// </summary>
    /// <remarks>Example: Used to Average Time values.</remarks>
    class AveragerForFloats
    {
        //this holds the _history       
        private readonly List<float> _history = new List<float>();
        private readonly int _maxSampleSize;
        
        private int _iNextUpdateSlot;

        private const float ZeroValue = 0;

        #region properties

        public float CurrentAverage { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize">Sample size</param>
        public AveragerForFloats(int sampleSize)
        {           
            _history.Capacity = sampleSize;

            // 8/11/2009
            _maxSampleSize = sampleSize;
            
            // Loop to create placeholders in array
            for (var i = 0; i < sampleSize; i++)
            {
                _history.Add(ZeroValue); 
               
            }
        }
        
        
        /// <summary>
        /// Each time you want to get a new Average, feed it the most recent value
        /// and this method will return an Average over the last <see cref="_maxSampleSize"/> updates.
        /// </summary>
        /// <param name="mostRecentValue">New value to add to overall average</param>
        /// <returns>average</returns>
        public float Update(float mostRecentValue)
        {
            // 5/20/2010 - Refactored out core code into new STATIC method.
            return DoAverageUpdate(this, mostRecentValue);
        }

        // 5/20/2010
        /// <summary>
        /// Method helper, which does the average calculation.
        /// </summary>
        /// <param name="avgForFloats">this instance of <see cref="AveragerForFloats"/></param>
        /// <param name="mostRecentValue">New value to add to overall average</param>
        /// <returns>average</returns>
        private static float DoAverageUpdate(AveragerForFloats avgForFloats, float mostRecentValue)
        {
            //overwrite the oldest value with the newest
            var history = avgForFloats._history; // 5/20/2010 - Cache
            history[avgForFloats._iNextUpdateSlot++] = mostRecentValue;

            //make sure m_iNextUpdateSlot wraps around. 
            var maxSampleSize = avgForFloats._maxSampleSize; // 5/20/2010
            if (avgForFloats._iNextUpdateSlot == maxSampleSize) avgForFloats._iNextUpdateSlot = 0;

            // 5/12/2009 - CAN NOT USE the Linq 'Average', since this use an IEnumerator, and causes 
            //             Garbage on HEAP!
            //currentAverage = _history.Average();
            var totalValue = 0.0f;
            for (var i = 0; i < maxSampleSize; i++)
            {
                totalValue += history[i];
            }

            avgForFloats.CurrentAverage = totalValue / maxSampleSize;

            return avgForFloats.CurrentAverage;
        }
    }
}
