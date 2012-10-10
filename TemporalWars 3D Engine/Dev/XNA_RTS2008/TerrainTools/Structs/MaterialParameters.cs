using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools.Structs
{
    /// <summary>
    /// Defines a group of parameters, which defines one Material.
    /// </summary>
    internal struct MaterialParameters
    {
        private Dictionary<int, MaterialControls> _materialParameters;

        // 2/9/2010
        /// <summary>
        /// Used to add a group <see cref="MaterialControls"/> node, for a given <see cref="MaterialParameters"/> name, to
        /// the internal dictionary.
        /// </summary>
        /// <param name="materialParamName"><see cref="MaterialParameters"/> name</param>
        /// <param name="materialControls"><see cref="MaterialControls"/> group node</param>
        public void AddMaterialControlsGroup(ProceduralMaterialParameters materialParamName, MaterialControls materialControls)
        {
            try // 6/22/2010
            {
                // check if null
                if (_materialParameters == null)
                    _materialParameters = new Dictionary<int, MaterialControls>();

                // add new item to dictionary
                _materialParameters.Add((int)materialParamName, materialControls);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("AddMaterialControlsGroup method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 2/9/2010
        /// <summary>
        /// Updates a given <see cref="MaterialControls"/> node, for a given <see cref="MaterialParameters"/> name, using
        /// the given <see cref="MaterialParamDef"/> node.
        /// </summary>
        /// <param name="materialParamName"><see cref="MaterialParameters"/> name</param>
        /// <param name="materialParamDef"><see cref="MaterialParamDef"/> node</param>
        public void UpdateMaterialControl(ProceduralMaterialParameters materialParamName, ref MaterialParamDef materialParamDef)
        {
            try // 6/22/2010
            {
                // retrieve and update given material parameter
                MaterialControls materialControls;
                if (!_materialParameters.TryGetValue((int)materialParamName, out materialControls)) return;

                // update main controls
                var enabled = materialParamDef.Enabled;
                {
                    var paramLabelControl = materialControls.ParamLabelControl;
                    paramLabelControl.Text = materialParamDef.DisplayName;
                    paramLabelControl.Enabled = enabled;
                    paramLabelControl.ForeColor = enabled ? System.Drawing.Color.Blue : System.Drawing.Color.Black;
                }
                {
                    var paramAssignControl = materialControls.ParamAssignControl;
                    paramAssignControl.Enabled = enabled;
                    paramAssignControl.ForeColor = enabled ? System.Drawing.Color.Blue : System.Drawing.Color.Black;
                }

                // update spinners
                var count = materialParamDef.Spinners.Count;
                for (int index = 0; index < count; index++)
                {
                    // cache
                    var spinnerDef = materialParamDef.Spinners[index];

                    // update spinner
                    var spinner = materialControls.ParamSpinnerControls[index];
                    if (spinner == null) continue;

                    spinner.Minimum = (decimal)spinnerDef.SpinnerMinValue;
                    spinner.Maximum = (decimal)spinnerDef.SpinnerMaxValue;
                    spinner.Increment = (decimal)spinnerDef.SpinnerStepValue;
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("UpdateMaterialControl method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 2/10/2010
        /// <summary>
        /// Iterates all the <see cref="MaterialControls"/> in the internal dictionary, to reset all values
        /// to zero, and description font colors to black.
        /// </summary>
        public void ResetMaterialControls()
        {
            try // 6/22/2010
            {
                // iterate dictionary
                foreach (var materialControls in _materialParameters)
                {
                    var controls = materialControls.Value;

                    controls.ParamAssignControl.ForeColor = System.Drawing.Color.Black;
                    controls.ParamLabelControl.ForeColor = System.Drawing.Color.Black;

                    // iterate spinners to reset values
                    foreach (var spinnerControl in controls.ParamSpinnerControls)
                    {
                        // check if null
                        if (spinnerControl == null) continue;
                        spinnerControl.Value = (spinnerControl.Minimum > 0) ? spinnerControl.Minimum : 0;
                    }
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ResetMaterialControls method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 2/10/2010
        /// <summary>
        /// Used to iterate the internal dictionary, returning each '<see cref="MaterialControls"/> node to caller.
        /// </summary>
        /// <returns><see cref="MaterialControls"/> node</returns>
        public IEnumerable<KeyValuePair<int, MaterialControls>> IterateMaterialParameters()
        {           
            // iterate internal dictionary
            foreach (var materialParameter in _materialParameters)
            {
                yield return materialParameter;
            }      
        }
    }
}