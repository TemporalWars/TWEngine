#region File Description
//-----------------------------------------------------------------------------
// BehaviorsTimeSpan.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;

namespace TWEngine.ForceBehaviors.Structs
{
    // 6/12/2010
    /// <summary>
    /// A Light-Version of the <see cref="TimeSpan"/> specifically created
    /// for the <see cref="TWEngine.ForceBehaviors.SteeringBehaviors"/> classes.
    /// </summary>
    public struct BehaviorsTimeSpan
    {
// ReSharper disable InconsistentNaming

        /// <summary>
        /// Game's elapsed time in ticks.
        /// </summary>
        public long Ticks;

        /// <summary>
        /// Returns the internal ticks as milliseconds.
        /// </summary>
        public int Milliseconds
        {
            get
            {
                return (int)((Ticks / 10000) % 1000);
            }
        }

        ///<summary>
        /// Returns the internal ticks as total seconds.
        ///</summary>
        public double TotalSeconds
        {
            get
            {
                return (Ticks * 1E-07);
            }
        }

        /// <summary>
        /// Returns the internal ticks as total milliseconds.
        /// </summary>
        public double TotalMilliseconds
        {
            get
            {
                var num = Ticks * 0.0001;
                if (num > 922337203685477)
                {
                    return 922337203685477;
                }
                if (num < -922337203685477)
                {
                    return -922337203685477;
                }
                return num;
            }
        }

// ReSharper restore InconsistentNaming
    }
}
