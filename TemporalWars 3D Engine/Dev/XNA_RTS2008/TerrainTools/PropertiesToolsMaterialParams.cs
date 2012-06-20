#region File Description
//-----------------------------------------------------------------------------
// Extensions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Interfaces;
using TWEngine.TerrainTools.Structs;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    // 2/10/2010 - 
    /// <summary>
    /// Extensions class for NumericUpDown control
    /// </summary>
    static class Extensions
    {
        // 2/10/2010 - Extension method
        /// <summary>
        /// Extension method, for the NumericUpDown control, which checks the given
        /// value to set into the control, to verify it is within range of the Min/Max
        /// settings on the control.
        /// </summary>
        /// <param name="nup">(THIS) empty param.</param>
        /// <param name="newValue">value to set</param>
        /// <returns>True/False if value was adjusted</returns>
        public static bool SafeUpdateValue(this NumericUpDown nup, decimal newValue)
        {
            // was value adjusted to be within range?
            var valueAdjusted = false;

            try // 6/22/2010
            {
                // verify value given is within the Min/Max ranges defined on NUP control.
                if (newValue < nup.Minimum)
                {
                    // too low, so set to min
                    newValue = nup.Minimum;
                    valueAdjusted = true;
                }
                else if (newValue > nup.Maximum)
                {
                    // too high, so set to max
                    newValue = nup.Maximum;
                    valueAdjusted = true;
                }

                // set value
                nup.Value = newValue;
                
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SafeUpdateValue method threw the exception " + ex.Message ?? "No Message");
#endif
            }

            return valueAdjusted;
        }
    }

    // 2/9/2010
    /// <summary>
    /// Stores the group of Material parameters used on the 'Materials' tab.
    /// </summary>
    class PropertiesToolsMaterialParams
    {
        /// <summary>
        /// Stores the group of Material parameters used on the 'Materials' tab.
        /// </summary>
        internal static MaterialParameters ProceduralMaterialParams;
        

        // 2/9/2010
        /// <summary>
        /// Sets the PropertiesTools 'Material' tab control's attributes, like the DisplayName, to 
        /// have the proper values for a given MaterialParameter, using the MaterialDefinitions node; which 
        /// is retrieved by the given MaterialId number from the 'ProceduralMaterialParams' class.
        /// </summary>
        /// <param name="propertiesTools">Instance of PropertiesTool Form</param>
        /// <param name="materialId">Material ID to set</param>
        internal static void SetMaterialDefinitionParamsAtts(PropertiesTools propertiesTools, int materialId)
        {
            try // 6/22/2010
            {
                MaterialDefinition materialDefinition;
                if (!InstancedModelMaterialDefinitions.TryGetProceduralMaterialDef(materialId, out materialDefinition)) return;

                // update material name
                propertiesTools.grpMaterialParams.Text = materialDefinition.MaterialName;

                // reset all controls
                ProceduralMaterialParams.ResetMaterialControls();

                // iterate and update control groups
                foreach (var materialParamDef in materialDefinition.MaterialParams)
                {
                    // Update this material control group
                    var materialParamName = (ProceduralMaterialParameters)materialParamDef.Key;
                    var paramDef = materialParamDef.Value;
                    ProceduralMaterialParams.UpdateMaterialControl(materialParamName, ref paramDef);
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetMaterialDefinitionParamsAtts method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 2/10/2010
        /// <summary>
        /// Sets the PropertiesTools 'Material' tab control's values, like the mpMiscFloat1 value, to
        /// have the proper value obtained from the 'picked' instanced model's material.
        /// </summary>
        /// <param name="propertiesTools">Instance of PropertiesTool Form</param>
        /// <param name="instancedModel">Instance of Picked InstancedModel</param>
        private static void SetMaterialDefinitionParamsValues(PropertiesTools propertiesTools, InstancedModel instancedModel)
        {
            try // 6/22/2010
            {
                // Get ModelPart index key
                var modelPartIndexKey = (int)propertiesTools.cmbPickByPart.SelectedValue;

                // The inner Dictionary of 'ProceduralMaterialParams', which holds the references to each of the controls on the PropertiesTool
                // form, are iterated and updated with the proper values obtain from the InstancedModel picked.  Depending on the type value
                // return, either 1,2 or 4 spinner controls are updated per material parameter.
                foreach (var materialControls in ProceduralMaterialParams.IterateMaterialParameters())
                {
                    // retrieve for given MaterialControls key (which is ProceduralMaterialParameters enum)
                    object value;
                    Type valueType;
                    instancedModel.GetProceduralMaterialParameter((ProceduralMaterialParameters)materialControls.Key, modelPartIndexKey, out value, out valueType);

                    // Update spinners
                    UpdateSpinnerControlsValues(instancedModel, modelPartIndexKey, materialControls, value, valueType);


                } // End Foreach
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetMaterialDefinitionParamsValues method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 2/10/2010
        /// <summary>
        /// Helper method, to directly update a given SpinnerControl's value, using the extension method 'SafeUpdateValue' to 
        /// verify the new value is within range of Min/Max settings.
        /// </summary>
        /// <param name="modelPartIndexKey">Specific modelPart to affect</param>
        /// <param name="materialControls">MaterialControls node</param>
        /// <param name="value">new value</param>
        /// <param name="valueType">value Type</param>
        /// <param name="instancedModel">Instance of Picked InstancedModel</param>
        private static void UpdateSpinnerControlsValues(InstancedModel instancedModel, int modelPartIndexKey, KeyValuePair<int, MaterialControls> materialControls,
                                                     object value, Type valueType) 
        {
            try // 6/22/2010
            {
                // verify param array is not null, and specific index is exist first.
                if (materialControls.Value.ParamSpinnerControls == null) return;

                // value adjusted?
                var valueAdjusted = false;

                // update spinner values, depending on base type
                if (valueType == typeof(float))
                {
                    // cast to float value
                    var valueFloat = (float)value;

                    // update value
                    if (materialControls.Value.ParamSpinnerControls.Count > 0)
                        valueAdjusted = materialControls.Value.ParamSpinnerControls[0].SafeUpdateValue((decimal)valueFloat);
                }
                else if (valueType == typeof(Vector2))
                {
                    // cast to Vector2 value
                    var valueVector2 = ((Vector2)value);

                    if (materialControls.Value.ParamSpinnerControls.Count != 2)
                        throw new IndexOutOfRangeException("ParamSpinnerControls should have a count of 2!");

                    valueAdjusted = materialControls.Value.ParamSpinnerControls[0].SafeUpdateValue((decimal)valueVector2.X);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[1].SafeUpdateValue((decimal)valueVector2.Y);

                }
                else if (valueType == typeof(Vector4))
                {
                    // cast to Vector4 value
                    var valueVector4 = ((Vector4)value);

                    if (materialControls.Value.ParamSpinnerControls.Count != 4)
                        throw new IndexOutOfRangeException("ParamSpinnerControls should have a count of 4!");

                    valueAdjusted = materialControls.Value.ParamSpinnerControls[0].SafeUpdateValue((decimal)valueVector4.X);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[1].SafeUpdateValue((decimal)valueVector4.Y);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[2].SafeUpdateValue((decimal)valueVector4.Z);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[3].SafeUpdateValue((decimal)valueVector4.W);
                }
                else if (valueType == typeof(Color))
                {
                    // Color, therefore 4 spinners to update
                    var colorValue = new Color((Vector4)value);

                    if (materialControls.Value.ParamSpinnerControls.Count != 4)
                        throw new IndexOutOfRangeException("ParamSpinnerControls should have a count of 4!");

                    valueAdjusted = materialControls.Value.ParamSpinnerControls[0].SafeUpdateValue(colorValue.R);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[1].SafeUpdateValue(colorValue.G);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[2].SafeUpdateValue(colorValue.B);
                    valueAdjusted &= materialControls.Value.ParamSpinnerControls[3].SafeUpdateValue(colorValue.A);
                }

                // If value adjusted, update the Effect param, since incorrect.
                if (valueAdjusted)
                {
                    // value was adjusted, so update effect
                    instancedModel.SetProceduralMaterialParameter((ProceduralMaterialParameters)
                                                                  materialControls.Key,
                                                                  value,
                                                                  modelPartIndexKey);

                } // End if Value adjusted
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateSpinnerControlsValues method threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }


        // 2/10/2010
        /// <summary>
        /// Sets the PropertiesTools 'Material' tab control's values, like the mpMiscFloat1 value, to
        /// have the proper value obtained from the 'picked' instanced model's material.
        /// </summary>
        /// <param name="propertiesTools">Instance of PropertiesTool Form</param>
        public static void UpdateMaterialDefinitionParams(PropertiesTools propertiesTools)
        {
            try // 6/22/2010
            {               
                // Check if anything picked yet.
                InstancedModel instancedModel;
                IInstancedItem instancedItem;
                if (!propertiesTools.GetInstancedModelAndInstancedItem(out instancedModel, out instancedItem))
                    return;

                // Get ModelPart index key
                var modelPartIndexKey = (int) propertiesTools.cmbPickByPart.SelectedValue;
                // Retrieve MaterialId currently used for given modelPart.
                int materialId;
                if (instancedModel.GetProceduralMaterialId(modelPartIndexKey, out materialId))
                {
                    // set control to value retrieved
                    propertiesTools.nupMaterialId.Value = materialId;

                    // update the 'Materials' tab parameter attributes
                    SetMaterialDefinitionParamsAtts(propertiesTools, materialId);

                    // update the 'Materials' tab parameter values
                    SetMaterialDefinitionParamsValues(propertiesTools, instancedModel);

                    return;
                } // End if retrieved MaterialId

                //
                // Failed to retrieve MaterialId, so just use default value in nupMaterialId control.
                //
                // update the 'Materials' tab parameter attributes
                SetMaterialDefinitionParamsAtts(propertiesTools, (int) propertiesTools.nupMaterialId.Value);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateSpinnerControlsValues method threw the exception " + ex.Message ?? "No Message");
#endif
            }           
           
        }
       
    }
}
