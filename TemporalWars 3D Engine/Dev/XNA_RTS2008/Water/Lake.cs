#region File Description
//-----------------------------------------------------------------------------
// Lake.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.SkyDomes;
using ImageNexus.BenScharbach.TWEngine.Terrain;
using ImageNexus.BenScharbach.TWEngine.Water.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Water
{
    /// <summary>
    /// The <see cref="Lake"/> class is created using a simple flat quad rectangle, where the waves are represented
    /// using a bump map texture and animated over time. The <see cref="Lake"/> class is not resource intensive.
    /// </summary>
    /// <remarks>Use the <see cref="Ocean"/>class to create realistic waves using animated vertices.</remarks>
    public class Lake : WaterBase, ILake
    {
        // 11/17/2008 - Add EffectParameters & EffectTechniques
        private static EffectTechnique _waterTechnique;
        private static EffectParameter _worldEParam;
        private static EffectParameter _viewEParam;
        private static EffectParameter _projEParam;
        private static EffectParameter _reflectionViewEParam;
        private static EffectParameter _reflectionMapEParam;
        private static EffectParameter _refractionMapEParam;
        private static EffectParameter _waterBumpMapEParam;
        private static EffectParameter _waveLengthEParam;
        private static EffectParameter _waveHeightEParam;
        private static EffectParameter _camPosEParam;
        private static EffectParameter _timeEParam;
        private static EffectParameter _windForceEParam;
        private static EffectParameter _windDirectionEParam;
        private static EffectParameter _lightDirectionEParam;
        private static EffectParameter _dullColorEParam;

        // Water Attributes
        private static Vector4 _dullColor = new Vector4(0.3f, 0.3f, 0.5f, 1.0f);
        private static Vector3 _windDirection = new Vector3(-1, 0, 1);
        private static float _windForce = 0.002f;
        private static float _waveSpeed = 800.0f;
        private static float _waveLength = 0.1f;
        private static float _waveHeight = 0.1f;

        // 7/1/2009 - Distort Vertices Variables
        private static Vector4 _waveFreqs;     
        private static Vector4 _waveHeights;       
        private static Vector4 _waveLengths;        
        private static Vector2[] _waveDirs = new Vector2[4];        
        private static bool _useDistortVertices;

        private static VertexPositionTexture[] _waterVertices;

        // Water Attributes
        private static Effect _waterEffect;
        private static VertexBuffer _waterVertexBuffer;
        private static VertexDeclaration _waterVertexDeclaration;

       

        #region Properties

        /// <summary>
        /// Set water table height.
        /// </summary>
        public override float WaterHeight
        {
            get { return base.WaterHeight; }
            set 
            {
                base.WaterHeight = value;

                // 12/15/2009
                // Create Water Mesh
                SetUpWaterVertices(TerrainData.MapWidthToScale, TerrainData.MapHeightToScale, value);
            }
        }
       
        /// <summary>
        /// Wave length
        /// </summary>
        public float WaveLength
        {
            get { return _waveLength; }
            set { _waveLength = value; }
        }

        /// <summary>
        /// Wave height 
        /// </summary>
        public float WaveHeight
        {
            get { return _waveHeight; }
            set { _waveHeight = value; }
        }      

        /// <summary>
        ///  Wave speed
        /// </summary>
        public float WaveSpeed
        {
            get { return _waveSpeed; }
            set { _waveSpeed = value; }
        }

        /// <summary>
        /// Wind direction as Vector3.
        /// </summary>
        public Vector3 WindDirection
        {
            get { return _windDirection; }
            set { _windDirection = value; }
        }


        /// <summary>
        /// Wind force as float.
        /// </summary>
        public float WindForce
        {
            get { return _windForce; }
            set { _windForce = value; }
        }

        /// <summary>
        /// Sunlight direction as Vector3.
        /// </summary>
        public Vector3 SunlightDirection
        {
            get { return SunLightDirection; }
            set { SunLightDirection = value; }
        }

        /// <summary>
        /// Color tone of water.
        /// </summary>
        public Vector4 DullColor
        {
            get { return _dullColor; }
            set { _dullColor = value; }
        }
        
        // 7/1/2009
        /// <summary>
        /// Use distort vertices option when drawing water.
        /// </summary>
        public bool UseDistortVertices
        {
            get { return _useDistortVertices; }
            set 
            { 
                _useDistortVertices = value;
                SetWaterEffectParams();        
            }
        }
        // 7/1/2009
        /// <summary>
        /// Frequency of waves
        /// </summary>
        public Vector4 WaveFreqs
        {
            get { return _waveFreqs; }
            set 
            { 
                _waveFreqs = value;
                SetWaterEffectParams();             
            }
        }

        // 7/1/2009
        /// <summary>
        /// Wave height
        /// </summary>
        public Vector4 WaveHeights
        {
            get { return _waveHeights; }
            set 
            { 
                _waveHeights = value;
                SetWaterEffectParams(); 
            }
        }

        // 7/1/2009
        /// <summary>
        /// Wave length
        /// </summary>
        public Vector4 WaveLengths
        {
            get { return _waveLengths; }
            set 
            { 
                _waveLengths = value;
                SetWaterEffectParams(); 
            }
        }

        // 8/12/2009
        ///<summary>
        /// Sets direction of waves.
        ///</summary>
        ///<param name="value">Array of Vector2</param>
        public void SetWaveDirs(Vector2[] value)
        {
            _waveDirs = value;
            SetWaterEffectParams();
        }

        // 8/12/2009 - Convert to method, per FXCop.
        /// <summary>
        /// Returns collectoin of <see cref="Vector2"/> for wave directions.
        /// </summary>
        /// <returns>Array of Vector2</returns>
        public Vector2[] GetWaveDirs()
        {
            return _waveDirs;
        }

        #endregion

        ///<summary>
        /// Constructor for creating the <see cref="Lake"/> water component.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="inWaterHeight">Set water table height</param>
        public Lake(Game game, float inWaterHeight)
            : base(ref game, inWaterHeight)
        {
            // Set default values
            const int mapWidth = 5120;
            const int mapHeight = 5120;

            // Create Water Mesh
            SetUpWaterVertices(mapWidth, mapHeight, inWaterHeight);

            // XNA 4.0 Updates - VertexDeclaration unncessary if using the built-int 'VertexPositionTexture'.
            //_waterVertexDeclaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionTexture.VertexElements);

        }
       
        // 9/9/2008 
        /// <summary>
        /// Loads the <see cref="Lake"/> content, like the <see cref="Effect"/> and water bump-map texture.
        /// </summary>
        internal sealed override void LoadContent()
        {
            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // Set Water Shader Effect 
            _waterEffect = WaterContentManager.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Water");
            
            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            // Load WaterBumpMap
            WaterBumpMapTexture = WaterContentManager.Load<Texture2D>(TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\waterbump");


            // 11/17/2008 - Set EffectParameters & EffectTechniques
            _waterTechnique = _waterEffect.Techniques["Water"];
            _worldEParam = _waterEffect.Parameters["xWorld"];
            _viewEParam = _waterEffect.Parameters["xView"];
            _reflectionViewEParam = _waterEffect.Parameters["xReflectionView"];
            _projEParam = _waterEffect.Parameters["xProjection"];
            _reflectionMapEParam = _waterEffect.Parameters["xReflectionMap"];
            _refractionMapEParam = _waterEffect.Parameters["xRefractionMap"];
            _waterBumpMapEParam = _waterEffect.Parameters["xWaterBumpMap"];
            _waveLengthEParam = _waterEffect.Parameters["xWaveLength"];
            _waveHeightEParam = _waterEffect.Parameters["xWaveHeight"];
            _camPosEParam = _waterEffect.Parameters["xCamPos"];
            _timeEParam = _waterEffect.Parameters["xTime"];
            _windForceEParam = _waterEffect.Parameters["xWindForce"];
            _windDirectionEParam = _waterEffect.Parameters["xWindDirection"];
            _lightDirectionEParam = _waterEffect.Parameters["xLightDirection"];
            _dullColorEParam = _waterEffect.Parameters["xDullColor"];

            // 12/14/2009 - Load SkyBox texture
            _waterEffect.Parameters["xCubeMap"].SetValue(SkyDome.SkyboxTextureCube);

            // 12/10/2009 - Use Distort vertices
            UseDistortVertices = true;

            // 7/1/2009 - Used for either the Lake/Ocean methods.
            PopulateWaveData();

            // 7/1/2009 - Set Wave Data
            SetWaterEffectParams();

            _worldEParam.SetValue(Matrix.Identity);


            base.LoadContent();
        }

        // 7/1/2009
        /// <summary>
        /// Sets all effect parameters, for the <see cref="Lake"/> effect, which only need
        /// to be set once.
        /// </summary>
        private static void SetWaterEffectParams()
        {
            _waterEffect.Parameters["xDistortVertices"].SetValue(_useDistortVertices); // Was 'true'
            _waterEffect.Parameters["xWaveSpeeds"].SetValue(_waveFreqs);
            _waterEffect.Parameters["xWaveHeights"].SetValue(_waveHeights);
            _waterEffect.Parameters["xWaveLengths"].SetValue(_waveLengths);
            _waterEffect.Parameters["xWaveDir0"].SetValue(_waveDirs[0]);
            _waterEffect.Parameters["xWaveDir1"].SetValue(_waveDirs[1]);
            _waterEffect.Parameters["xWaveDir2"].SetValue(_waveDirs[2]);
            _waterEffect.Parameters["xWaveDir3"].SetValue(_waveDirs[3]);
        }

        /// <summary>
        /// Draws the <see cref="Lake"/> water component.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Draw(GameTime gameTime)
        {
            RenderLakeWater((float)gameTime.TotalGameTime.TotalMilliseconds);

            base.Draw(gameTime);
        }

        // 12/14/2009
        /// <summary>
        /// Renders the <see cref="Lake"/> to screen
        /// </summary>
        /// <param name="time">time value</param>        
        private static void RenderLakeWater(float time)
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;
            
            // Adjust Speed of Water
            time /= _waveSpeed;

            // 11/17/2008 - Updated to use EffectParameters           
            _waterEffect.CurrentTechnique = _waterTechnique;
            _viewEParam.SetValue(Camera.View);
            _reflectionViewEParam.SetValue(ReflectionViewMatrix);
            _projEParam.SetValue(Camera.Projection);
            _reflectionMapEParam.SetValue(ReflectionMapTexture);
            _refractionMapEParam.SetValue(RefractionMapTexture);
            _waterBumpMapEParam.SetValue(WaterBumpMapTexture);
            _waveLengthEParam.SetValue(_waveLength);
            _waveHeightEParam.SetValue(_waveHeight);
            _camPosEParam.SetValue(Camera.CameraPosition);
            _timeEParam.SetValue(time);
            _windForceEParam.SetValue(_windForce);
            _windDirectionEParam.SetValue(_windDirection);
            _lightDirectionEParam.SetValue(SunLightDirection);
            _dullColorEParam.SetValue(_dullColor);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_waterEffect.Begin();
            // 8/26/2008: Updated to ForLoop, rather than ForEach.
            var effectPassCollection = _waterEffect.CurrentTechnique.Passes; // 5/20/2010 - Cache
            var count = effectPassCollection.Count; // 5/20/2010 - Cache
            for (var loop1 = 0; loop1 < count; loop1++)
            {
                // XNA 4.0 updates - Begin() and End() obsolete.
                effectPassCollection[loop1].Apply();

                // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
                //graphicsDevice.Vertices[0].SetSource(_waterVertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                //graphicsDevice.VertexDeclaration = _waterVertexDeclaration;
                graphicsDevice.SetVertexBuffer(_waterVertexBuffer);

                // XNA 4.0 Updates - VertexCount built in!
                //var noVertices = _waterVertexBuffer.SizeInBytes / VertexPositionTexture.SizeInBytes;
                var noVertices = _waterVertexBuffer.VertexCount;
                graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, noVertices / 3);

                // XNA 4.0 updates - Begin() and End() obsolete.
                //effectPassCollection[loop1].End();
            }

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_waterEffect.End();
        }

        /// <summary>
        /// Creates two huge triangles that span the whole terrain.
        /// </summary>   
        /// <param name="mapWidth">Map width</param>
        /// <param name="mapHeight">Map height</param>
        /// <param name="waterHeight">Water height</param>  
        private static void SetUpWaterVertices(int mapWidth, int mapHeight, float waterHeight)
        {
            _waterVertices = new VertexPositionTexture[6];

            var tmpPosition = Vector3.Zero;

            tmpPosition.X = 0; tmpPosition.Y = waterHeight; tmpPosition.Z = 0; var tmpTextCord = Vector2.Zero;
            _waterVertices[0] = new VertexPositionTexture(tmpPosition, tmpTextCord);
            tmpPosition.X = mapWidth; tmpPosition.Y = waterHeight; tmpPosition.Z = mapHeight; tmpTextCord = Vector2.One;
            _waterVertices[1] = new VertexPositionTexture(tmpPosition, tmpTextCord);
            tmpPosition.X = 0; tmpPosition.Y = waterHeight; tmpPosition.Z = mapHeight; tmpTextCord.X = 0; tmpTextCord.Y = 1;
            _waterVertices[2] = new VertexPositionTexture(tmpPosition, tmpTextCord);

            tmpPosition.X = 0; tmpPosition.Y = waterHeight; tmpPosition.Z = 0; tmpTextCord = Vector2.Zero;
            _waterVertices[3] = new VertexPositionTexture(tmpPosition, tmpTextCord);
            tmpPosition.X = mapWidth; tmpPosition.Y = waterHeight; tmpPosition.Z = 0; tmpTextCord.X = 1; tmpTextCord.Y = 0;
            _waterVertices[4] = new VertexPositionTexture(tmpPosition, tmpTextCord);
            tmpPosition.X = mapWidth; tmpPosition.Y = waterHeight; tmpPosition.Z = mapHeight; tmpTextCord = Vector2.One;
            _waterVertices[5] = new VertexPositionTexture(tmpPosition, tmpTextCord);

            // 6/30/2009 - Dispose of old resource.
            if (_waterVertexBuffer != null)
                _waterVertexBuffer.Dispose();

            // XNA 4.0 Updates - VertexDeclaration set at creation of VB.
            /*_waterVertexBuffer = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, 
                                                  _waterVertices.Length * VertexPositionTexture.SizeInBytes, BufferUsage.WriteOnly);*/
            _waterVertexBuffer = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice,
                                                  typeof(VertexPositionTexture),
                                                  _waterVertices.Length, BufferUsage.WriteOnly);

            _waterVertexBuffer.SetData(_waterVertices);
        }

        // 7/1/2009
        /// <summary>
        /// Populates the <see cref="Lake"/> wave attributes, like <see cref="WaveFreqs"/>, <see cref="WaveHeights"/>, and <see cref="WaveLengths"/>.
        /// </summary>
        private static void PopulateWaveData()
        {
            _waveFreqs = new Vector4(1, 2, 0.5f, 1.5f);
            _waveHeights = new Vector4(1.3f, 1.4f, 1.2f, 1.3f);
            _waveLengths = new Vector4(10, 5, 15, 7);

        }

        // 1/21/2009
        /// <summary>
        /// Gets all <see cref="Lake"/> components attributes, and returns to
        /// caller in the <see cref="WaterData"/> struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Lake"/> attributes</param>
        internal void GetWaterDataAttributes(ref WaterData waterData)
        {
            waterData.DullColor = _dullColor;
            waterData.SunlightDirection = SunLightDirection;
            waterData.WaterHeight = WaterHeight;
            waterData.WaveHeight = _waveHeight;
            waterData.Wavelength = _waveLength;
            waterData.WaveSpeed = _waveSpeed;
            waterData.WindDirection = _windDirection;
            waterData.WindForce = _windForce;
        }

        // 1/21/2009
        /// <summary>
        /// Sets all the <see cref="Lake"/> attributes using the given
        /// <see cref="WaterData"/>  struct.
        /// </summary>
        /// <param name="waterData"><see cref="WaterData"/> with <see cref="Lake"/> attributes</param>
        internal void SetWaterDataAttributes(ref WaterData waterData)
        {
            _dullColor = waterData.DullColor;
            SunLightDirection = waterData.SunlightDirection;
            WaterHeight = waterData.WaterHeight;
            _waveHeight = waterData.WaveHeight;
            _waveLength = waterData.Wavelength;
            _waveSpeed = waterData.WaveSpeed;
            _windDirection = waterData.WindDirection;
            _windForce = waterData.WindForce;

            // 12/14/2009 - Set default values if zero
            if (_dullColor.Equals(Vector3.Zero)) _dullColor = new Vector4(0.3f, 0.3f, 0.5f, 1.0f);
            if (SunLightDirection.Equals(Vector3.Zero)) SunLightDirection = new Vector3(0, -1.0f, 0.45f);
            if (WaterHeight == 0) WaterHeight = 50;
            if (_waveHeight == 0) _waveHeight = 0.1f;
            if (_waveLength == 0) _waveLength = 0.1f;
            if (_waveSpeed == 0) _waveSpeed = 800.0f;
            if (_windDirection.Equals(Vector3.Zero)) _windDirection = new Vector3(-1, 0, 1);
            if (_windForce == 0) _windForce = 0.002f;

            // 6/30/2009 - Update the WaterVertices Heights
            var length = _waterVertices.Length; // /5/20/2010 - Cache
            for (var i = 0; i < length; i++)
            {
                _waterVertices[i].Position.Y = WaterHeight;
            }
            _waterVertexBuffer.SetData(_waterVertices);

        }
        

        // 4/5/2009 - Dispose of resources
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public override void Dispose(bool disposing)
        {
            if (_waterEffect != null)
                _waterEffect.Dispose();
            if (_waterEffect != null)
                _waterEffect.Dispose();
            if (_waterVertexBuffer != null)
                _waterVertexBuffer.Dispose();
            if (_waterVertexDeclaration != null)
                _waterVertexDeclaration.Dispose();
            

            // Null Interface and class References
            _waterEffect = null;
            WaterBumpMapTexture = null;
            _waterVertexBuffer = null;
            _waterVertexDeclaration = null;
            _waterTechnique = null;
            // 1/8/2010 - Null EffectParams
            _worldEParam = null;
            _viewEParam = null;
            _projEParam = null;
            _reflectionViewEParam = null;
            _reflectionMapEParam = null;
            _refractionMapEParam = null;
            _waterBumpMapEParam = null;
            _waveLengthEParam = null;
            _waveHeightEParam = null;
            _camPosEParam = null;
            _timeEParam = null;
            _windForceEParam = null;
            _windDirectionEParam = null;
            _lightDirectionEParam = null;
            _dullColorEParam = null;

            // 1/8/2010 - Clear Arrays
            if (_waveDirs != null) Array.Clear(_waveDirs, 0, _waveDirs.Length);
            if (_waterVertices != null) Array.Clear(_waterVertices, 0, _waterVertices.Length);
           

            base.Dispose(disposing);
        }
    }
}