#region File Description
//-----------------------------------------------------------------------------
// Ocean.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TWEngine.GameCamera;
using TWEngine.Interfaces;
using TWEngine.Water.Structs;

namespace TWEngine.Water
{
    /// <summary>
    /// The <see cref="Ocean"/> class creates realistic waves using animated vertices and is resource intensive.
    /// </summary>
    /// <remarks>The <see cref="Lake"/> class is created using a simple flat quad rectangle where the waves are represented
    /// using a bump map texture and animated over time.</remarks>
    public class Ocean : WaterBase, IOcean
    {
        private static Effect _oceanEffect;

        private const int OceanWaterWidth = 512;
        private const int OceanWaterHeight = 512;

        private static VertexBuffer _oceanWaterVertexBuffer;
        private static IndexBuffer _oceanWaterIndexBuffer;
        private static VertexDeclaration _oceanVertexDeclaration;

        private static EffectParameter _oceanCameraViewEp;
        private static EffectParameter _oceanCameraViewInvExp; // 12/15/09
        private static EffectParameter _oceanCameraProjEp;
        private static EffectParameter _oceanBumpMapEp;
        private static EffectParameter _oceanBumpHeightEp; // 12/15/2009
        private static EffectParameter _oceanTextureScaleEp; // 12/15/2009
        private static EffectParameter _oceanCameraPosEp;
        private static EffectParameter _oceanGameTimeEp;
        private static EffectParameter _oceanWaveSpeedEp; // 12/15/2009
        private static EffectParameter _oceanFresnelBiasEp; // 12/15/2009
        private static EffectParameter _oceanFresnelPowerEp; // 12/15/2009
        private static EffectParameter _oceanHDRMultiplierEp; // 12/15/2009
        private static EffectParameter _oceanLightDirEp;
        private static EffectParameter _oceanReflectiveEp;
        private static EffectParameter _oceanRefractiveEp;
        private static EffectParameter _oceanDeepColorEp; // 12/15/2009
        private static EffectParameter _oceanShallowColorEp; // 12/15/2009
        private static EffectParameter _oceanReflectionColorEp; // 12/15/2009
        private static EffectParameter _oceanReflectionViewEp;
        private static EffectParameter _oceanReflectionAmountEp; // 12/15/2009
        private static EffectParameter _oceanReflectionSkyAmountEp; // 12/16/2009
        private static EffectParameter _oceanWaterAmountEp; // 12/15/2009
        private static EffectParameter _oceanWaveAmplitudeEp; // 12/15/2009
        private static EffectParameter _oceanWaveFrequencyEp; // 12/15/2009

        // 12/15/2009
        private float _oceanWaveAmplitude = 0.5f;
        private float _oceanWaveFrequency = 0.1f;
        private float _oceanBumpHeight = 0.1f;
        // 12/16/2009
        private Vector4 _oceanDeepColor = new Vector4(0.0f, 0.4f, 0.5f, 1.0f);
        private Vector4 _oceanShallowColor = new Vector4(0.55f, 0.75f, 0.75f, 1.0f);
        private Vector2 _oceanTextureScale = new Vector2(4, 4);
        private Vector2 _oceanWaveSpeed = new Vector2(0.1f, 0.5f);
        private float _oceanFresnelBias = 0.025f;
        private float _oceanFresnelPower = 1.0f;
        private float _oceanHDRMultiplier = 1.0f;
        private float _oceanReflectionAmt = 0.5f;
        private float _oceanReflectionSkyAmt = 0.5f;
        private float _oceanWaterAmt = 0.5f;
        private Vector4 _oceanReflectionColor = Vector4.One;

        // 1/8/2010
        private VertexMultitextured[] _waterVertices;
        private int[] _waterIndices;


        // XNA 4.0 Updates - Add new IVertexType.
        // 12/15/2009
        ///<summary>
        /// The <see cref="VertexMultitextured"/> struct holds the vertex information required
        /// to draw the <see cref="Ocean"/> waves.
        ///</summary>
        public struct VertexMultitextured : IVertexType
        {
            ///<summary>
            /// <see cref="Vector3"/> position
            ///</summary>
            public Vector3 Position;

            ///<summary>
            /// <see cref="Vector3"/> normal
            ///</summary>
            public Vector3 Normal;

            ///<summary>
            /// Packed <see cref="HalfVector2"/> for texture coordinates.
            ///</summary>
            public HalfVector2 TextureCoordinate; // 1/08/2010 - Test HalfVector2

            ///<summary>
            /// <see cref="Vector3"/> tangent
            ///</summary>
            public Vector3 Tangent;

            ///<summary>
            /// <see cref="Vector3"/> bi-normal.
            ///</summary>
            public Vector3 BiNormal;

            ///<summary>
            /// Size in bytes for data stored in this <see cref="VertexMultitextured"/>
            ///</summary>
            public const int SizeInBytes = (3 + 3 + 1 + 3 + 3) * 4;


            private static readonly VertexElement[] VertexElements = new[]
                                                               {
                                                                   new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
                                                                   new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
                                                                   new VertexElement( sizeof(float) * 6, VertexElementFormat.HalfVector2, VertexElementUsage.TextureCoordinate, 0 ),
                                                                   new VertexElement( sizeof(float) * 7, VertexElementFormat.Vector3, VertexElementUsage.Tangent, 0 ),
                                                                   new VertexElement( sizeof(float) * 10, VertexElementFormat.Vector3, VertexElementUsage.Binormal, 0 ),
                                                               };

            // 9/20/2010 - XNA 4.0 Updates - Include VertexDeclaration here now.
            ///<summary>
            /// VertexDeclaration with Collection of <see cref="VertexElement"/>
            ///</summary>
            public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(VertexElements);

            VertexDeclaration IVertexType.VertexDeclaration { get { return VertexDeclaration; } }

        }

        // 6/6/2009 - Updated by the 'V' or 'IsVisible' properties.
        ///<summary>
        /// Set to show the <see cref="Ocean"/> component.
        ///</summary>
        public static bool IsVisibleS;

       

        #region Properties

        ///<summary>
        /// Set/Get water height.
        ///</summary>
        public override float WaterHeight
        {
            get { return base.WaterHeight; }
            set
            {
                base.WaterHeight = value;

                // 1/8/2010 - Clear out data in arrays, if any.
                CreateWaterBuffers(value);
            }
        }

        ///<summary>
        /// Set to show the <see cref="Ocean"/> component.
        ///</summary>
        public bool IsVisible
        {
            get { return Visible; }
            set { Visible = value; IsVisibleS = value; }
        }

        // 1/20/2009 - shortcut version
        ///<summary>
        /// Shortcut alias 'V' for <see cref="IsVisible"/> property.
        ///</summary>
        public bool V
        {
            get { return Visible; }
            set { Visible = value; IsVisibleS = value; }
        }

        // 12/15/2009
        /// <summary>
        /// Ocean wave amplitude
        /// </summary>
        public float OceanWaveAmplitude
        {
            get { return _oceanWaveAmplitude; }
            set
            {
                _oceanWaveAmplitude = value;

                if (_oceanWaveAmplitudeEp != null) 
                    _oceanWaveAmplitudeEp.SetValue(value);
            }
        }

        // 12/15/2009
        /// <summary>
        /// Ocean wave frequency
        /// </summary>
        public float OceanWaveFrequency
        {
            get { return _oceanWaveFrequency; }
            set
            {
                _oceanWaveFrequency = value;

                if (_oceanWaveFrequencyEp != null) 
                    _oceanWaveFrequencyEp.SetValue(value);
            }
        }

        // 12/15/2009
        /// <summary>
        /// Ocean bump-map height
        /// </summary>
        public float OceanBumpHeight
        {
            get { return _oceanBumpHeight; }
            set
            {
                _oceanBumpHeight = value;

                if (_oceanBumpHeightEp != null) 
                    _oceanBumpHeightEp.SetValue(value);
            }
        }

        // 12/16/2009
        /// <summary>
        /// Ocean deep color
        /// </summary>
        public Vector4 OceanDeepColor
        {
            get { return _oceanDeepColor; }
            set
            {
                _oceanDeepColor = value;

                if (_oceanDeepColorEp != null) 
                    _oceanDeepColorEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean shallow color
        /// </summary>
        public Vector4 OceanShallowColor
        {
            get { return _oceanShallowColor; }
            set
            {
                _oceanShallowColor = value;

                if (_oceanShallowColorEp != null) 
                    _oceanShallowColorEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean texture scale
        /// </summary>
        public Vector2 OceanTextureScale
        {
            get { return _oceanTextureScale; }
            set
            {
                _oceanTextureScale = value;

                if (_oceanTextureScaleEp != null) 
                    _oceanTextureScaleEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean wave speed
        /// </summary>
        public Vector2 OceanWaveSpeed
        {
            get { return _oceanWaveSpeed; }
            set
            {
                _oceanWaveSpeed = value;

                if (_oceanWaveSpeedEp != null) 
                    _oceanWaveSpeedEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean fresnel bias
        /// </summary>
        public float OceanFresnelBias
        {
            get { return _oceanFresnelBias; }
            set
            {
                _oceanFresnelBias = value;

                if (_oceanFresnelBiasEp != null) 
                    _oceanFresnelBiasEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean fresnel power
        /// </summary>
        public float OceanFresnelPower
        {
            get { return _oceanFresnelPower; }
            set
            {
                _oceanFresnelPower = value;

                if (_oceanFresnelPowerEp != null) 
                    _oceanFresnelPowerEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean HDR multiplier
        /// </summary>
        public float OceanHDRMultiplier
        {
            get { return _oceanHDRMultiplier; }
            set
            {
                _oceanHDRMultiplier = value;

                if (_oceanHDRMultiplierEp != null) 
                    _oceanHDRMultiplierEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean reflection amount, determines the influence of the reflection
        /// shown on the water surface.
        /// </summary>
        public float OceanReflectionAmt
        {
            get { return _oceanReflectionAmt; }
            set
            {
                _oceanReflectionAmt = value;

                if (_oceanReflectionAmountEp != null) 
                    _oceanReflectionAmountEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean water amount, determines the influence of the ocean floor shown.
        /// </summary>
        public float OceanWaterAmt
        {
            get { return _oceanWaterAmt; }
            set
            {
                _oceanWaterAmt = value;

                if (_oceanWaterAmountEp != null) 
                    _oceanWaterAmountEp.SetValue(value);
            }
        }
        // 12/16/2009
        /// <summary>
        /// Ocean reflection color bias.
        /// </summary>
        public Vector4 OceanReflectionColor
        {
            get { return _oceanReflectionColor; }
            set
            {
                _oceanReflectionColor = value;

                if (_oceanReflectionColorEp != null) 
                    _oceanReflectionColorEp.SetValue(value);
            }
        }

        // 12/16/2009
        /// <summary>
        /// Ocean sky amount, determines the influence of the sky clouds shown on
        /// the water surface.
        /// </summary>
        public float OceanReflectionSkyAmt
        {
            get { return _oceanReflectionSkyAmt; }
            set
            {
                _oceanReflectionSkyAmt = value;

                if (_oceanReflectionSkyAmountEp != null) 
                    _oceanReflectionSkyAmountEp.SetValue(value);
            }
        }

        #endregion

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="inWaterHeight">Water height</param>
        public Ocean(Game game, float inWaterHeight)
            : base(ref game, inWaterHeight)
        {
            // EMTPY
        }
         
        /// <summary>
        /// Loads the <see cref="Ocean"/> content, like the <see cref="Effect"/> and waves bump map.
        /// </summary>
        internal sealed override void LoadContent()
        {
            
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var
            _oceanEffect = WaterContentManager.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\OceanWater2");

            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            // 12/15/2009 - Load WaterBumpMap
            WaterBumpMapTexture = WaterContentManager.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\waves2");

            _oceanEffect.CurrentTechnique = _oceanEffect.Techniques["OceanWater"];
            _oceanEffect.Parameters["xWorld"].SetValue(Matrix.Identity);

            _oceanCameraViewEp = _oceanEffect.Parameters["xView"];
            _oceanCameraViewInvExp = _oceanEffect.Parameters["xViewI"];
            _oceanCameraProjEp = _oceanEffect.Parameters["xProjection"];
            _oceanBumpMapEp = _oceanEffect.Parameters["xBumpMap"];
            _oceanBumpHeightEp = _oceanEffect.Parameters["fBumpHeight"];
            _oceanTextureScaleEp = _oceanEffect.Parameters["vTextureScale"];
            _oceanCameraPosEp = _oceanEffect.Parameters["xCameraPos"];
            _oceanGameTimeEp = _oceanEffect.Parameters["xTime"];
            _oceanWaveSpeedEp = _oceanEffect.Parameters["vBumpSpeed"];
            _oceanFresnelBiasEp = _oceanEffect.Parameters["fFresnelBias"];
            _oceanFresnelPowerEp = _oceanEffect.Parameters["fFresnelPower"];
            _oceanHDRMultiplierEp = _oceanEffect.Parameters["fHDRMultiplier"];
            _oceanLightDirEp = _oceanEffect.Parameters["xLightDirection"];
            _oceanReflectiveEp = _oceanEffect.Parameters["xReflectionMap"];
            _oceanRefractiveEp = _oceanEffect.Parameters["xRefractionMap"];
            _oceanDeepColorEp = _oceanEffect.Parameters["vDeepColor"]; // 12/15/2009
            _oceanShallowColorEp = _oceanEffect.Parameters["vShallowColor"]; // 12/15/2009
            _oceanReflectionColorEp = _oceanEffect.Parameters["vReflectionColor"]; // 12/15/2009
            _oceanReflectionViewEp = _oceanEffect.Parameters["xReflectionView"];
            _oceanReflectionAmountEp = _oceanEffect.Parameters["fReflectionAmount"]; // 12/15/2009
            _oceanReflectionSkyAmountEp = _oceanEffect.Parameters["fReflectionSkyAmount"]; // 12/16/2009
            _oceanWaterAmountEp = _oceanEffect.Parameters["fWaterAmount"]; // 12/15/2009
            _oceanWaveAmplitudeEp = _oceanEffect.Parameters["fWaveAmp"]; // 12/15/2009
            _oceanWaveFrequencyEp = _oceanEffect.Parameters["fWaveFreq"]; // 12/15/2009

            // 12/15/2009 - Load SkyBox texture
            _oceanEffect.Parameters["xCubeMap"].SetValue(SkyDomes.SkyDome.SkyboxTextureCube);

            // 12/15/2009 - Set Init Default Values
            _oceanDeepColorEp.SetValue(_oceanDeepColor); // 12/15/2009
            _oceanShallowColorEp.SetValue(_oceanShallowColor); // 12/15/2009
            _oceanReflectionColorEp.SetValue(_oceanReflectionColor); // 12/15/2009
            _oceanBumpHeightEp.SetValue(_oceanBumpHeight);
            _oceanTextureScaleEp.SetValue(_oceanTextureScale);
            _oceanWaveSpeedEp.SetValue(_oceanWaveSpeed);
            _oceanFresnelBiasEp.SetValue(_oceanFresnelBias);
            _oceanFresnelPowerEp.SetValue(_oceanFresnelPower);
            _oceanHDRMultiplierEp.SetValue(_oceanHDRMultiplier);
            _oceanReflectionAmountEp.SetValue(_oceanReflectionAmt); // 12/15/2009
            _oceanReflectionSkyAmountEp.SetValue(OceanReflectionSkyAmt); // 12/16/2009
            _oceanWaterAmountEp.SetValue(_oceanWaterAmt); // 12/15/2009
            _oceanWaveAmplitudeEp.SetValue(_oceanWaveAmplitude); // 12/15/2009
            _oceanWaveFrequencyEp.SetValue(_oceanWaveFrequency); // 12/15/2009

            // 1/8/2010 - Create Buffers
            CreateWaterBuffers(WaterHeight);
           
            base.LoadContent();
        }

        // 1/8/2010
        /// <summary>
        /// Creates the <see cref="Ocean"/> vertices and indices.
        /// </summary>
        /// <param name="waterHeight">Water height</param>
        private void CreateWaterBuffers(float waterHeight)
        {
            if (_waterIndices != null) Array.Clear(_waterIndices, 0, _waterIndices.Length);
            if (_waterVertices != null) Array.Clear(_waterVertices, 0, _waterVertices.Length);

            CreateWaterVertices(waterHeight, out _waterVertices);
            CreateWaterIndices(out _waterIndices);
            CreateBuffers(ref _waterVertices, ref _waterIndices);
        }

        /// <summary>
        /// Draws the <see cref="Ocean"/> component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
            RenderOceanWater(gameTime, GameInstance.GraphicsDevice);

            base.Draw(gameTime);
        }
       
        /// <summary>
        /// Gets all the internal <see cref="Ocean"/> attributes, and returns to
        /// caller in the <see cref="WaterData"/> struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Ocean"/> attributes</param>
        public void GetWaterDataAttributes(ref WaterData waterData)
        {
            waterData.OceanBumpHeight = _oceanBumpHeight;
            waterData.OceanDeepColor = _oceanDeepColor;
            waterData.OceanFresnelBias = _oceanFresnelBias;
            waterData.OceanFresnelPower = _oceanFresnelPower;
            waterData.OceanHDRMultiplier = _oceanHDRMultiplier;
            waterData.OceanReflectionAmt = _oceanReflectionAmt;
            waterData.OceanReflectionColor = _oceanReflectionColor;
            waterData.OceanReflectionSkyAmt = _oceanReflectionSkyAmt;
            waterData.OceanShallowColor = _oceanShallowColor;
            waterData.OceanTextureScale = _oceanTextureScale;
            waterData.OceanWaterAmt = _oceanWaterAmt;
            waterData.OceanWaveAmplitude = _oceanWaveAmplitude;
            waterData.OceanWaveFrequency = _oceanWaveFrequency;
            waterData.OceanWaveSpeed = _oceanWaveSpeed;
        }

        // 1/21/2009
        /// <summary>
        /// Sets all the internal <see cref="Ocean"/> attributes using the given
        /// <see cref="WaterData"/> struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Ocean"/> attributes</param>
        public void SetWaterDataAttributes(ref WaterData waterData)
        {
            // Update through Properties, so the EffectParam are updated too!
            OceanBumpHeight = waterData.OceanBumpHeight;
            OceanDeepColor = waterData.OceanDeepColor;
            OceanFresnelBias = waterData.OceanFresnelBias;
            OceanFresnelPower = waterData.OceanFresnelPower;
            OceanHDRMultiplier = waterData.OceanHDRMultiplier;
            OceanReflectionAmt = waterData.OceanReflectionAmt;
            OceanReflectionColor = waterData.OceanReflectionColor;
            OceanReflectionSkyAmt = waterData.OceanReflectionSkyAmt;
            OceanShallowColor = waterData.OceanShallowColor;
            OceanTextureScale = waterData.OceanTextureScale;
            OceanWaterAmt = waterData.OceanWaterAmt;
            OceanWaveAmplitude = waterData.OceanWaveAmplitude;
            OceanWaveFrequency = waterData.OceanWaveFrequency;
            OceanWaveSpeed = waterData.OceanWaveSpeed;

            // Set those which cannot be zero, to a default value.
            if (_oceanFresnelPower == 0) _oceanFresnelPower = 1.0f;

        }
       
        // 1/8/2010: Updated to use (OUT) param.
        /// <summary>
        /// Creates the <see cref="Ocean"/> components vertices.
        /// </summary>
        /// <param name="waterHeight">Water height</param>
        /// <param name="waterVertices">(OUT) Collection of <see cref="VertexMultitextured"/> structs.</param>
        private static void CreateWaterVertices(float waterHeight, out VertexMultitextured[] waterVertices)
        {
            waterVertices = new VertexMultitextured[OceanWaterWidth * OceanWaterHeight];

            var i = 0;
            for (var z = 0; z < OceanWaterHeight; z++)
                for (var x = 0; x < OceanWaterWidth; x++)
                {
                    var xPos = x*10;
                    var zPos = z*10;

                    var position = new Vector3(xPos, waterHeight, zPos);
                    var texCoord = new Vector2(xPos/30.0f, zPos/30.0f);

                    waterVertices[i++] = new VertexMultitextured
                                             {
                                                 Position = position,
                                                 Normal = Vector3.Up,
                                                 TextureCoordinate = new HalfVector2(texCoord)
                                             };
                }

            // 12/15/009
            // Calc Tangent and Bi Normals.
            for (var x = 0; x < OceanWaterHeight; x++)
                for (var y = 0; y < OceanWaterWidth; y++)
                {
                    // Tangent Data.
                    var index0 = x + y * OceanWaterWidth; // 5/20/2010 - Cache calc
                    if (x != 0 && x < OceanWaterWidth - 1)
                        waterVertices[index0].Tangent = waterVertices[x - 1 + y * OceanWaterWidth].Position - waterVertices[x + 1 + y * OceanWaterWidth].Position;
                    else
                        if (x == 0)
                            waterVertices[index0].Tangent = waterVertices[index0].Position - waterVertices[x + 1 + y * OceanWaterWidth].Position;
                        else
                            waterVertices[index0].Tangent = waterVertices[x - 1 + y * OceanWaterWidth].Position - waterVertices[index0].Position;

                    // Bi Normal Data.
                    if (y != 0 && y < OceanWaterHeight - 1)
                        waterVertices[index0].BiNormal = waterVertices[x + (y - 1) * OceanWaterWidth].Position - waterVertices[x + (y + 1) * OceanWaterWidth].Position;
                    else
                        if (y == 0)
                            waterVertices[index0].BiNormal = waterVertices[index0].Position - waterVertices[x + (y + 1) * OceanWaterWidth].Position;
                        else
                            waterVertices[index0].BiNormal = waterVertices[x + (y - 1) * OceanWaterWidth].Position - waterVertices[index0].Position;
                }

            // XNA 4.0 Updates - Custom VertexDeclaration now go into the Structure, which inherits from IVertexType.
            //_oceanVertexDeclaration = new VertexDeclaration(TemporalWars3DEngine.GameInstance.GraphicsDevice, VertexMultitextured.VertexElements);

            
        }

        /// <summary>
        /// Creates the <see cref="Ocean"/> component indices.
        /// </summary>
        /// <param name="waterIndices">(OUT) collection of indices</param>
        private static void CreateWaterIndices(out int[] waterIndices)
        {
            waterIndices = new int[(OceanWaterWidth) * 2 * (OceanWaterHeight - 1)];

            var i = 0;
            var z = 0;
            while (z < OceanWaterHeight - 1)
            {              
                for (var x = 0; x < OceanWaterWidth; x++)
                {
                    waterIndices[i++] = (x + z * OceanWaterWidth);
                    waterIndices[i++] = (x + (z + 1) * OceanWaterWidth);
                }
                z++;

                if (z < OceanWaterHeight - 1)
                {
                    for (var x = OceanWaterWidth - 1; x >= 0; x--)
                    {
                        waterIndices[i++] = (x + (z + 1) * OceanWaterWidth);
                        waterIndices[i++] = (x + z * OceanWaterWidth);
                    }
                }
                z++;
            }
            
        }

       
        /// <summary>
        /// Creates the <see cref="Ocean"/> <see cref="VertexBuffer"/> and <see cref="IndexBuffer"/>.
        /// </summary>
        /// <param name="waterVertices">Collection of <see cref="VertexMultitextured"/> vertices</param>
        /// <param name="waterIndices">Collection of indices</param>
        private static void CreateBuffers(ref VertexMultitextured[] waterVertices, ref int[] waterIndices)
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // 12/15/2009 - Dispose of old buffer
            if (_oceanWaterVertexBuffer != null)
                _oceanWaterVertexBuffer.Dispose();

            // XNA 4.0 Updates - VertexDeclaration now gets set with the VB.
            //_oceanWaterVertexBuffer = new VertexBuffer(graphicsDevice, VertexMultitextured.SizeInBytes * waterVertices.Length, BufferUsage.WriteOnly);
            _oceanWaterVertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexMultitextured), waterVertices.Length, BufferUsage.WriteOnly);

            _oceanWaterVertexBuffer.SetData(waterVertices);

            // 12/15/2009 - Dispose of old buffer
            if (_oceanWaterIndexBuffer != null)
                _oceanWaterIndexBuffer.Dispose();

            _oceanWaterIndexBuffer = new IndexBuffer(graphicsDevice, typeof(int), waterIndices.Length, BufferUsage.WriteOnly);
            _oceanWaterIndexBuffer.SetData(waterIndices);
        }

        /// <summary>
        /// Renders the <see cref="Ocean"/> component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        private static void RenderOceanWater(GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            //draw Water                        
            var time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            _oceanGameTimeEp.SetValue(time);

            _oceanReflectiveEp.SetValue(ReflectionMapTexture);
            _oceanRefractiveEp.SetValue(RefractionMapTexture);

            // Calc Inverse View
            var cameraView = Camera.View;
            Matrix viewI;
            Matrix.Invert(ref cameraView, out viewI);

            _oceanCameraViewEp.SetValue(cameraView);
            _oceanCameraViewInvExp.SetValue(viewI);
            _oceanCameraProjEp.SetValue(Camera.Projection);
            _oceanBumpMapEp.SetValue(WaterBumpMapTexture);
            _oceanLightDirEp.SetValue(SunLightDirection);
            _oceanCameraPosEp.SetValue(Camera.CameraPosition);
            _oceanReflectionViewEp.SetValue(ReflectionViewMatrix);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_oceanEffect.Begin();
            var effectPassCollection = _oceanEffect.CurrentTechnique.Passes; // 5/20/2010 - Cache
            foreach (var pass in effectPassCollection)
            {
                // XNA 4.0 updates - Begin() and End() obsolete.
                pass.Apply();

                // XNA 4.0 Updates
                //graphicsDevice.Vertices[0].SetSource(_oceanWaterVertexBuffer, 0, VertexMultitextured.SizeInBytes);
                graphicsDevice.SetVertexBuffer(_oceanWaterVertexBuffer);

                graphicsDevice.Indices = _oceanWaterIndexBuffer;

                // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
                //graphicsDevice.VertexDeclaration = _oceanVertexDeclaration;

                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, OceanWaterWidth * OceanWaterHeight, 0, OceanWaterWidth * 2 * (OceanWaterHeight - 1) - 2);

                // XNA 4.0 updates - Begin() and End() obsolete.
                //pass.End();
            }

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_oceanEffect.End();
        }
       

        // Dispose of resources
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public override void Dispose(bool disposing)
        {
            
            // 1/8/2010 - Dispose
            if (_oceanEffect != null)
            {
                _oceanEffect.Dispose();
                _oceanEffect = null;
            }
            if (_oceanWaterVertexBuffer != null)
            {
                _oceanWaterVertexBuffer.Dispose();
                _oceanWaterVertexBuffer = null;
            }
            if (_oceanWaterIndexBuffer != null)
            {
                _oceanWaterIndexBuffer.Dispose();
                _oceanWaterIndexBuffer = null;
            }
            if (_oceanVertexDeclaration != null)
            {
                _oceanVertexDeclaration.Dispose();
                _oceanVertexDeclaration = null;
            }

            // 1/8/2010 - Clear Arrays
            if (_waterIndices != null) Array.Clear(_waterIndices, 0, _waterIndices.Length);
            if (_waterVertices != null) Array.Clear(_waterVertices, 0, _waterVertices.Length);

            // Null Refs
            _oceanCameraViewEp = null;
            _oceanCameraViewInvExp = null;
            _oceanCameraProjEp = null;
            _oceanBumpMapEp = null;
            _oceanBumpHeightEp = null;
            _oceanTextureScaleEp = null;
            _oceanCameraPosEp = null;
            _oceanGameTimeEp = null;
            _oceanWaveSpeedEp = null;
            _oceanFresnelBiasEp = null;
            _oceanFresnelPowerEp = null;
            _oceanHDRMultiplierEp = null;
            _oceanLightDirEp = null;
            _oceanReflectiveEp = null;
            _oceanRefractiveEp = null;
            _oceanDeepColorEp = null;
            _oceanShallowColorEp = null;
            _oceanReflectionColorEp = null;
            _oceanReflectionViewEp = null;
            _oceanReflectionAmountEp = null;
            _oceanReflectionSkyAmountEp = null;
            _oceanWaterAmountEp = null;
            _oceanWaveAmplitudeEp = null;
            _oceanWaveFrequencyEp = null;
            
            base.Dispose(disposing);
        }

       
    }
}