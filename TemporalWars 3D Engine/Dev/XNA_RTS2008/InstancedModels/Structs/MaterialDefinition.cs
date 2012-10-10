#region File Description
//-----------------------------------------------------------------------------
// MaterialDefinition.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    // 2/8/2010
    /// <summary>
    /// Defines the 10 Material Parameters for 
    /// a single ProceduralMaterialId.
    /// </summary>
    [Serializable]
    public struct MaterialDefinition
    {
        ///<summary>
        /// Unique ID which indentifies procedural material to use.
        ///</summary>
        public int ProceduralMaterialId;

        ///<summary>
        /// Procedural material name
        ///</summary>
        public string MaterialName;

        ///<summary>
        /// Dictionary of <see cref="MaterialParamDef"/> structs, where the key is the material ID.
        ///</summary>
        public Dictionary<int, MaterialParamDef> MaterialParams; // 2/9/2010
       
        // 2/9/2010 - constructor
        ///<summary>
        /// Constructor, which creates the internal <see cref="MaterialParams"/> Dictionary.
        ///</summary>
        ///<param name="materialId">Enter unique material ID</param>
        ///<param name="materialName">Enter material's name</param>
        public MaterialDefinition(int materialId, string materialName)
        {
            ProceduralMaterialId = materialId;
            MaterialName = materialName;
            MaterialParams = new Dictionary<int, MaterialParamDef>();
            
        }

        // 2/9/2010
        /// <summary>
        /// Adds a new MaterialParam definition to the internal dictionary.
        /// </summary>
        /// <param name="materialParamName">Material Parameter to add</param>
        /// <param name="displayName">Material's DisplayName, shown on properties material's tab</param>
        /// <param name="enabled">Allow use of this parameter on properties material's tab</param>
        /// <param name="materialSpinnerDef">Define spinner parameters, like MinValue or MaxValue; can enter more than one definition here.</param>
        public void AddMaterialParamDef(ProceduralMaterialParameters materialParamName, string displayName, bool enabled, params MaterialSpinnerDef[] materialSpinnerDef)
        {
            // check if null
            if (MaterialParams == null)
                MaterialParams = new Dictionary<int, MaterialParamDef>();

            // create new material parameter definition
            var materialDef = new MaterialParamDef(displayName, enabled);

            // add Spinner Defs
            foreach (var spinnerDef in materialSpinnerDef)
            {
                materialDef.AddMaterialSpinnerDef(spinnerDef);
            }

            // add new material definition to dictionary
            MaterialParams.Add((int)materialParamName, materialDef);

        }
    }
}