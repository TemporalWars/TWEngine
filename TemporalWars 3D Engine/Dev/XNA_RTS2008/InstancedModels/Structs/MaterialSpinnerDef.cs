#region File Description
//-----------------------------------------------------------------------------
// MaterialSpinnerDef.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    /// <summary>
    /// Defines a single Numeric-Up-Down spinner's
    /// parameters within the PropertiesTool Form window.
    /// </summary>
    [Serializable]
    public struct MaterialSpinnerDef
    {
        ///<summary>
        /// Spinner's minimum value allowed.
        ///</summary>
        public float SpinnerMinValue;

        ///<summary>
        /// Spinner's maximum value allowed.
        ///</summary>
        public float SpinnerMaxValue;

        ///<summary>
        /// Spinner's step value (incremental value).
        ///</summary>
        public float SpinnerStepValue;

        // 2/9/2010 - constructor
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="min">Set spinner's minimum value allowed.</param>
        ///<param name="max">Set spinner's maximum value allowed.</param>
        ///<param name="step">Set spinner's step value (incremental value).</param>
        public MaterialSpinnerDef(float min, float max, float step)
        {
            SpinnerMinValue = min;
            SpinnerMaxValue = max;
            SpinnerStepValue = step;
        }
    }
}