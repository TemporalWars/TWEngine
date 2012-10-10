#region File Description
//-----------------------------------------------------------------------------
// AveragerForVector3.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Utilities
{
    /// <summary>
    /// The <see cref="AveragerForVector3"/> class is used to average <see cref="Vector3"/>, keeping only the 'SampleSize'
    /// as the total Count in the internal collection.
    /// </summary>
    /// <remarks> Example: Used to smooth frame rate calculations.</remarks>
    class AveragerForVector3
    {
        //this holds the _history       
        private readonly List<Vector3> _history = new List<Vector3>();

        private int _iNextUpdateSlot;

        //an example of the 'zero' value of the type to be smoothed. This
        //would be something like Vector2D(0,0)
        private readonly Vector3 _zeroValue;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sampleSize">Sample size</param>
        public AveragerForVector3(int sampleSize)
        {
            _zeroValue = Vector3.Zero;
            _history.Capacity = sampleSize;

            // Loop to create placeholders in array
            for (var i = 0; i < sampleSize; i++)
            {
                _history.Add(_zeroValue);  
            }
        }
        
        // 5/30/2009: Updated to return using an Out param.
        /// <summary>
        /// Each time you want to get a new Average, feed it the most recent value
        /// and this method will return an Average over the last total count in collection.
        /// </summary>
        /// <param name="mostRecentValue">New <see cref="Vector3"/> to add to overall average</param>
        /// <param name="average">(OUT) <see cref="Vector3"/> average</param>
        public void Update(ref Vector3 mostRecentValue, out Vector3 average)
        {
            // 5/20/2010 - Refactored out the core code to new STATIC method.
            GetAverage(this, mostRecentValue, out average);
        }

        // 5/20/2010
        /// <summary>
        /// Helper method, which does the average calculations.
        /// </summary>
        /// <param name="avgForVector3"></param>
        /// <param name="mostRecentValue">New <see cref="Vector3"/> to add to overall average</param>
        /// <param name="average">(OUT) <see cref="Vector3"/> average</param>
        private static void GetAverage(AveragerForVector3 avgForVector3, Vector3 mostRecentValue, out Vector3 average)
        {
            //overwrite the oldest value with the newest
            var history = avgForVector3._history; // 5/20/2010 - Cache
            history[avgForVector3._iNextUpdateSlot++] = mostRecentValue;

            //make sure m_iNextUpdateSlot wraps around. 
            if (avgForVector3._iNextUpdateSlot == history.Capacity) avgForVector3._iNextUpdateSlot = 0;

            //now to calculate the Average of the _history list
            var sum = avgForVector3._zeroValue;

            //std::vector<T>::iterator it = m_History.begin();  

            // 1/12/2009 - Updated to remove Ops overload on Vector3, since slow on XBOX!
            var historyCount = history.Count;
            for (var i = 0; i < historyCount; i++)
            {
                //sum = sum + _history[i];
                var tmpHistory = history[i];
                Vector3.Add(ref sum, ref tmpHistory, out sum);
            }

            // 5/30/2009
            //return sum / (float)_history.Count;
            Vector3.Divide(ref sum, historyCount, out average);
           
        }
    }
}
