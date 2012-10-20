using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace ImageNexus.BenScharbach.TWEngine.TerrainTools.Structs
{
    /// <summary>
    /// Defines the group of controls which make up one <see cref="MaterialParameters"/>. 
    /// </summary>
    internal struct MaterialControls
    {
        public readonly Label ParamLabelControl;
        public readonly Button ParamAssignControl;
        public readonly List<NumericUpDown> ParamSpinnerControls;

        // constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="labelControl"><see cref="Label"/> instance</param>
        /// <param name="assignControl"><see cref="Button"/> instance</param>
        /// <param name="spinners"><see cref="NumericUpDown"/> collection</param>
        /// <returns></returns>
        public MaterialControls(Label labelControl, Button assignControl, params NumericUpDown[] spinners)
        {
            ParamLabelControl = labelControl;
            ParamAssignControl = assignControl;
            ParamSpinnerControls = new List<NumericUpDown>();

            // 6/22/2010
            AddSpinnerControl(spinners);
        }

        // 2/9/2010
        /// <summary>
        /// Use to add multiple <see cref="NumericUpDown"/> spinner controls to the internal collection.
        /// </summary>
        /// <param name="spinners"><see cref="NumericUpDown"/> spinner control</param>
        public void AddSpinnerControl(params NumericUpDown[] spinners)
        {
            try // 6/22/2010
            {
                // add each spinner control to internal list
                foreach (var spinner in spinners)
                {
                    ParamSpinnerControls.Add(spinner);
                }
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("AddSpinnerControl method threw the exception " + ex.Message ?? "No Message");
            }
        }
    }
}