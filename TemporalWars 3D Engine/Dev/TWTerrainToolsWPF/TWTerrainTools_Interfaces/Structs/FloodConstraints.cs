using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TWTerrainTools_Interfaces.Structs
{
    // 7/2/2010
    /// <summary>
    /// Stores the current Flood Constraint attributes
    /// settings, used to communicate to outside callers.
    /// </summary>
    public struct FloodConstraints
    {
// ReSharper disable InconsistentNaming
        public int HeightMin;
        public int HeightMax;
        public float NoiseGreater_Lv3;
        public float NoiseGreater_Lv2;
        public float NoiseGreater_Lv1;
        public int Density_Lv3;
        public int Density_Lv2;
        public int Density_lv1;
        public int Spacing;
        public int DensitySpacing;
// ReSharper restore InconsistentNaming
    }
}
