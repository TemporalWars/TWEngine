#region File Description
//-----------------------------------------------------------------------------
// MaterialParamDef.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;

namespace TWEngine.InstancedModels.Structs
{
    /// <summary>
    /// Defines a single Material Parameter
    /// within the PropertiesTool Form window.
    /// </summary>
    [Serializable]
    public struct MaterialParamDef
    {
        ///<summary>
        /// Enter display name for this <see cref="MaterialParamDef"/>
        ///</summary>
        public string DisplayName;

        ///<summary>
        /// Is <see cref="MaterialParamDef"/> enabled?
        ///</summary>
        public bool Enabled;

        ///<summary>
        /// Collection of <see cref="MaterialSpinnerDef"/> structs.
        ///</summary>
        public List<MaterialSpinnerDef> Spinners; // 2/9/2010

        // 2/9/2010 - constructor
        ///<summary>
        /// Constructor, which creates the collection of <see cref="MaterialSpinnerDef"/>.
        ///</summary>
        ///<param name="displayName">Display name for this <see cref="MaterialParamDef"/></param>
        ///<param name="enabled">Is enabled?</param>
        public MaterialParamDef(string displayName, bool enabled)
        {
            DisplayName = displayName;
            Enabled = enabled;
            Spinners = new List<MaterialSpinnerDef>();
        }

        // 2/9/2010
        /// <summary>
        /// To add a spinner definition to the internal list.
        /// </summary>
        /// <param name="materialSpinnerDef">Material spinner definition node</param>
        public void AddMaterialSpinnerDef(MaterialSpinnerDef materialSpinnerDef)
        {
            // check if null
            if (Spinners == null)
                Spinners = new List<MaterialSpinnerDef>();

            // add new spinner def
            Spinners.Add(materialSpinnerDef);
        }
    }
}