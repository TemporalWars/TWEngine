#region File Description
//-----------------------------------------------------------------------------
// WaterAtts.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Windows.Forms;
using TWEngine.Interfaces;
using TWEngine.Water;
using TWEngine.Water.Enums;
using Color = Microsoft.Xna.Framework.Color;
using System.Diagnostics;

namespace TWEngine.TerrainTools
{
    ///<summary>
    /// The <see cref="WaterAtts"/> class is used for updating the attributes 
    /// the <see cref="Ocean"/> and <see cref="Lake"/> water components.
    ///</summary>
    public partial class WaterAtts : Form
    {
        private readonly IWaterManager _waterManager = TemporalWars3DEngine.EngineGameConsole.Water;

        ///<summary>
        /// Constructor
        ///</summary>
        public WaterAtts()
        {
            try // 6/22/2010
            {
                InitializeComponent();

                // 12/14/2009
                SetAllControlValueStatesToDefaults();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("WaterAtts constructor threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        /// Captures the close event, and cancels the close action; instead, the form is simply hidden
        /// from view by setting the 'Visible' flag to False.  This fixes the issue of the HEAP crash 
        /// when trying to re-instantiate a form, after it was already created in the 'TerrainEditRoutines' class.
        /// </summary>
        private void WaterAtts_FormClosing(object sender, FormClosingEventArgs e)
        {
            try // 6/22/2010
            {
                e.Cancel = true;
                Visible = false;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("WaterAtts_FormClosing method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        /// Starts the ColorDialog component, so user can select the Water's DullColor.
        /// </summary>
// ReSharper disable InconsistentNaming
        private void btnSetDullColor_Click(object sender, EventArgs e)

        {
            try // 6/22/2010
            {
                if (colorDialog1.ShowDialog() != DialogResult.OK) return;

                var newColor = colorDialog1.Color;
                btnSetDullColor.BackColor = newColor;

                var dullColor = new Color(newColor.R, newColor.G, newColor.B, newColor.A);
                _waterManager.DullColor = dullColor.ToVector4();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnSetDullColor_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        /// Retrieves all current Water attributes, and updates the
        /// forms controls to reflects the correct values.
        /// </summary>
        private void SetAllControlValueStatesToDefaults()
        {
            try // 6/22/2010
            {
                // Set WindDirection values
                var windDir = _waterManager.WindDirection;
                nupWindDir_X.Value = (decimal)windDir.X;
                nupWindDir_Y.Value = (decimal)windDir.Y;
                nupWindDir_Z.Value = (decimal)windDir.Z;

                // Set SunDirection values
                var sunDir = _waterManager.SunlightDirection;
                nupSunDir_X.Value = (decimal)sunDir.X;
                nupSunDir_Y.Value = (decimal)sunDir.Y;
                nupSunDir_Z.Value = (decimal)sunDir.Z;

                // Set WindForce
                nupWindForce.Value = (decimal)_waterManager.WindForce;

                // Set WaveSpeed
                nupWaveSpeed.Value = (decimal)_waterManager.WaveSpeed;

                // Set WaveLength
                nupWaveLength.Value = (decimal)_waterManager.WaveLength;

                // Set WaveHeight
                nupWaveHeight.Value = (decimal)_waterManager.WaveHeight;

                // Set WaterTable Height
                nupWaterTblHeight.Value = (WaterManager.WaterTypeToUse == WaterType.Lake) ?
                    (decimal)((ILake)_waterManager).WaterHeight :
                    // ReSharper disable RedundantCast
                    (decimal)((IOcean)_waterManager).WaterHeight;
                // ReSharper restore RedundantCast

                // 12/15/2009 - Set Ocean's Atts
                nupWaveAmp.Value = (decimal)_waterManager.OceanWaveAmplitude;
                nupWaveFreq.Value = (decimal)_waterManager.OceanWaveFrequency;
                nupBumpHeight.Value = (decimal)_waterManager.OceanBumpHeight;
                // 12/16/2009 - Set additional Ocean's Atts
                var waveSpeed = _waterManager.OceanWaveSpeed;
                nupWaveSpeed_X.Value = (decimal)waveSpeed.X;
                nupWaveSpeed_Y.Value = (decimal)waveSpeed.Y;
                var texScale = _waterManager.OceanTextureScale;
                nupTextureScale_X.Value = (decimal)texScale.X;
                nupTextureScale_Y.Value = (decimal)texScale.Y;
                nupFresnelBias.Value = (decimal)_waterManager.OceanFresnelBias;
                nupFresnelPower.Value = (decimal)_waterManager.OceanFresnelPower;
                nupHDRMultiplier.Value = (decimal)_waterManager.OceanHDRMultiplier;
                nupReflcAmt.Value = (decimal)_waterManager.OceanReflectionAmt;
                nupWaterAmt.Value = (decimal)_waterManager.OceanWaterAmt;
                nupReflectionSkyAmt.Value = (decimal)_waterManager.OceanReflectionSkyAmt;

                // Restore DeepColor
                var color = new Color(_waterManager.OceanDeepColor);
                btnOceanDeepColor.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                // Restore ShallowColor
                color = new Color(_waterManager.OceanShallowColor);
                btnOceanShallowColor.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                // Restore ReflectiveColor
                color = new Color(_waterManager.OceanReflectionColor);
                btnOceanReflectiveColor.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                // Restore DullColor
                color = new Color(_waterManager.DullColor);
                btnSetDullColor.BackColor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("SetAllControlValueStatesToDefaults method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Wind Direction's X value.
        /// </summary>
        private void nupWindDir_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var windDir = _waterManager.WindDirection;
                windDir.X = (float)nupWindDir_X.Value;
                _waterManager.WindDirection = windDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWindDir_X_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Wind Direction's Y value.
        /// </summary>
        private void nupWindDir_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var windDir = _waterManager.WindDirection;
                windDir.Y = (float)nupWindDir_Y.Value;
                _waterManager.WindDirection = windDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWindDir_Y_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Wind Direction's Z value.
        /// </summary>
        private void nupWindDir_Z_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var windDir = _waterManager.WindDirection;
                windDir.Z = (float)nupWindDir_Z.Value;
                _waterManager.WindDirection = windDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWindDir_Z_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Sun Direction's X value.
        /// </summary>
        private void nupSunDir_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var sunDir = _waterManager.SunlightDirection;
                sunDir.X = (float)nupWindDir_X.Value;
                _waterManager.SunlightDirection = sunDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupSunDir_X_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Sun Direction's Y value.
        /// </summary>
        private void nupSunDir_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var sunDir = _waterManager.SunlightDirection;
                sunDir.Y = (float)nupWindDir_Y.Value;
                _waterManager.SunlightDirection = sunDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupSunDir_Y_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the Sun Direction's Z value.
        /// </summary>
        private void nupSunDir_Z_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var sunDir = _waterManager.SunlightDirection;
                sunDir.Z = (float)nupWindDir_Z.Value;
                _waterManager.SunlightDirection = sunDir;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupSunDir_Z_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the WindForce value.
        /// </summary>
        private void nupWindForce_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.WindForce = (float)nupWindForce.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWindForce_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the WaveSpeed value.
        /// </summary>
        private void nupWaveSpeed_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.WaveSpeed = (float)nupWaveSpeed.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveSpeed_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the WaveLength value.
        /// </summary>
        private void nupWaveLength_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.WaveLength = (float)nupWaveLength.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveLength_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the WaveHeight value.
        /// </summary>
        private void nupWaveHeight_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                _waterManager.WaveHeight = (float)nupWaveHeight.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveHeight_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/14/2009
        /// <summary>
        ///  Updates the WaterTblHeight
        /// </summary>
        private void nupWaterTblHeight_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                switch (WaterManager.WaterTypeToUse)
                {
                    case WaterType.None: // 6/1/2010
                        break;
                    case WaterType.Lake:
                        ((ILake)_waterManager).WaterHeight = (float)nupWaterTblHeight.Value;
                        break;
                    case WaterType.Ocean:
                        // ReSharper disable RedundantCast
                        ((IOcean)_waterManager).WaterHeight = (float)nupWaterTblHeight.Value;
                        // ReSharper restore RedundantCast
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaterTblHeight_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
            
        }

        // 12/15/2009
        /// <summary>
        /// Updates the Ocean's Wave Amplitude
        /// </summary>
        private void nupWaveAmp_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanWaveAmplitude = (float) nupWaveAmp.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveAmp_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/15/2009
        /// <summary>
        /// Updates the Ocean's Wave Frequency
        /// </summary>
        private void nupWaveFreq_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanWaveFrequency = (float)nupWaveFreq.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveFreq_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/15/2009
        /// <summary>
        /// Updates the Ocean's Bump Height
        /// </summary>
        private void nupBumpHeight_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanBumpHeight = (float)nupBumpHeight.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupBumpHeight_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Ocean's Deep Color
        /// </summary>
        private void btnOceanDeepColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (colorDialog1.ShowDialog() != DialogResult.OK) return;

                var newColor = colorDialog1.Color;
                btnOceanDeepColor.BackColor = newColor;

                var deepColor = new Color(newColor.R, newColor.G, newColor.B, newColor.A);
                _waterManager.OceanDeepColor = deepColor.ToVector4();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnOceanDeepColor_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Ocean's Shallow Color
        /// </summary>
        private void btnOceanShallowColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (colorDialog1.ShowDialog() != DialogResult.OK) return;

                var newColor = colorDialog1.Color;
                btnOceanShallowColor.BackColor = newColor;

                var shallowColor = new Color(newColor.R, newColor.G, newColor.B, newColor.A);
                _waterManager.OceanShallowColor = shallowColor.ToVector4();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnOceanShallowColor_Click method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }


        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' Reflective Color
        /// </summary>
        private void btnOceanReflectiveColor_Click(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                if (colorDialog1.ShowDialog() != DialogResult.OK) return;

                var newColor = colorDialog1.Color;
                btnOceanReflectiveColor.BackColor = newColor;

                var reflectiveColor = new Color(newColor.R, newColor.G, newColor.B, newColor.A);
                _waterManager.OceanReflectionColor = reflectiveColor.ToVector4();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("btnOceanReflectiveColor_Click method threw the exception " + ex.Message ?? "No Message");
#endif

            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' WaveSpeed X value
        /// </summary>
        private void nupWaveSpeed_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var waveSpeed = _waterManager.OceanWaveSpeed;
                waveSpeed.X = (float)nupWaveSpeed_X.Value;
                _waterManager.OceanWaveSpeed = waveSpeed;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveSpeed_X_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' WaveSpeed Y value
        /// </summary>
        private void nupWaveSpeed_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var waveSpeed = _waterManager.OceanWaveSpeed;
                waveSpeed.Y = (float)nupWaveSpeed_Y.Value;
                _waterManager.OceanWaveSpeed = waveSpeed;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaveSpeed_Y_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' TextureScale X value
        /// </summary>
        private void nupTextureScale_X_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var textureScale = _waterManager.OceanTextureScale;
                textureScale.X = (float)nupTextureScale_X.Value;
                _waterManager.OceanTextureScale = textureScale;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupTextureScale_X_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' TextureScale Y value
        /// </summary>
        private void nupTextureScale_Y_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                var textureScale = _waterManager.OceanTextureScale;
                textureScale.Y = (float)nupTextureScale_Y.Value;
                _waterManager.OceanTextureScale = textureScale;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupTextureScale_Y_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' Fresnel Bias value
        /// </summary>
        private void nupFresnelBias_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanFresnelBias = (float)nupFresnelBias.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupFresnelBias_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' Fresnel Power value
        /// </summary>
        private void nupFresnelPower_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanFresnelPower = (float)nupFresnelPower.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupFresnelPower_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' HDRMultiplier value
        /// </summary>
        private void nupHDRMultiplier_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanHDRMultiplier = (float)nupHDRMultiplier.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupHDRMultiplier_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' ReflcAmt value
        /// </summary>
        private void nupReflcAmt_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanReflectionAmt = (float)nupReflcAmt.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupReflcAmt_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Oceans' WaterAmt value
        /// </summary>
        private void nupWaterAmt_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
                _waterManager.OceanWaterAmt = (float)nupWaterAmt.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupWaterAmt_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }

        // 12/16/2009
        /// <summary>
        /// Updates the Ocean's ReflcSkyAmt value
        /// </summary>
        private void nupReflectionSkyAmt_ValueChanged(object sender, EventArgs e)
        {
            try // 6/22/2010
            {
            	_waterManager.OceanReflectionSkyAmt = (float) nupReflectionSkyAmt.Value;
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("nupReflectionSkyAmt_ValueChanged method threw the exception " + ex.Message ?? "No Message");
#endif
            }
        }
// ReSharper restore InconsistentNaming

    }
}
