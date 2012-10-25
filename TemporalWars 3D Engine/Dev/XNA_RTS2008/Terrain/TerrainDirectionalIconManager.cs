#region File Description
//-----------------------------------------------------------------------------
// TerrainDirectionalIconManager.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    // 6/5/2012
    /// <summary>
    /// The <see cref="TerrainDirectionalIconManager"/> class is used to draw multiple 
    /// instances of the decal icons <see cref="TerrainDirectionalIcon"/>.
    /// </summary>
    public class TerrainDirectionalIconManager : GameComponent
    {
        // This value MUST match the value set in the 'MultiTerrainEffect.fx' shader.
        private const int MaxIcons = 2;

        // holds a collection of decal icons.
        private static readonly Dictionary<string, TerrainDirectionalIcon> TerrainDirectionalIcons = new Dictionary<string, TerrainDirectionalIcon>();
        private static string[] _dictionaryKeys = new string[1];
        private static TerrainDirectionalIcon[] _dictionaryValues = new TerrainDirectionalIcon[1];
        
        
        private static int _arrayIndexCounter;
        private static Effect _multiTerrainEffect;
        private static Texture2D _directionalIcon;
        private static readonly ContentManager ContentTextures;

        private static EffectParameter _directionalIconPositionEp;
        private static EffectParameter _directionalIconSizeEp;
        private static EffectParameter _directionalIconRotationEp;
        private static EffectParameter _directionalIconIsVisibleEp;
        private static EffectParameter _directionalIconTextureEp;
        private static EffectParameter _directionalIconColorEp;

        // Array of attributes
        internal static readonly Vector3[] IconPositions = new Vector3[MaxIcons] { new Vector3(250, 0, 250), new Vector3(250, 0, 250) };
        internal static readonly int[] IconSizes = new int[MaxIcons] { 50, 50 };
        internal static readonly float[] IconRotations = new float[MaxIcons];
        internal static readonly bool[] IconVisibles = new bool[MaxIcons];
        internal static readonly Vector4[] IconColors = new Vector4[MaxIcons] { Color.White.ToVector4(), Color.White.ToVector4() };
        internal static readonly bool[] ApplyCameraRotation = new bool[MaxIcons]; // 10/24/2012

        /// <summary>
        /// Constructor
        /// </summary>
        static TerrainDirectionalIconManager()
        {
            ContentTextures = TemporalWars3DEngine.ContentGroundTextures;
            _arrayIndexCounter = 0;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="game"></param>
        public TerrainDirectionalIconManager(Game game) : base(game)
        {
            _arrayIndexCounter = 0;
        }

        /// <summary>
        /// Initializes the internal effect and effect parameters.
        /// </summary>
        /// <param name="effect">Instance of <see cref="Effect"/></param>
        public static void Initialize(Effect effect)
        {
            _multiTerrainEffect = effect;

            // Set EffectParams
            _directionalIconTextureEp = effect.Parameters["xDirectionalIconTex"];

            _directionalIconPositionEp = effect.Parameters["xDirectionalIconPosition"];
            _directionalIconSizeEp = effect.Parameters["xDirectionalIconSize"];
            _directionalIconRotationEp = effect.Parameters["xDirectionalIconRotation"];
            _directionalIconIsVisibleEp = effect.Parameters["xDirectionIconIsVisible"];
            _directionalIconColorEp = effect.Parameters["xDirectionalIconColor"];

            // Load DirectionalIcon Texture
            _directionalIcon = ContentTextures.Load<Texture2D>(@"Terrain\directionalIcon");

            // Set Default values
            _directionalIconTextureEp.SetValue(_directionalIcon);

            // Set Default Arrays
            _directionalIconPositionEp.SetValue(IconPositions);
            _directionalIconSizeEp.SetValue(IconSizes);
            _directionalIconRotationEp.SetValue(IconRotations);
            _directionalIconIsVisibleEp.SetValue(IconVisibles);
            _directionalIconColorEp.SetValue(IconColors);

            // XNA 4.0 updates
            //effect.CommitChanges();
        }
        
        /// <summary>
        /// Draws the Decal Icons onto the terrain.
        /// </summary>
        internal static void DrawIcons()
        {
            if (_multiTerrainEffect == null)
                return;

            // Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;

            //Vector3 cursorPos;
            //TerrainPickingRoutines.GetCursorPosByPickedRay(PickRayScale.DivideByTerrainScale, out cursorPos);
            // _directionalIconTextureEp.SetValue(_directionalIcon);

            // Update EffectParams
            _directionalIconPositionEp.SetValue(IconPositions); 
            _directionalIconSizeEp.SetValue(IconSizes);
            _directionalIconRotationEp.SetValue(IconRotations);
            _directionalIconIsVisibleEp.SetValue(IconVisibles);
            _directionalIconColorEp.SetValue(IconColors);
        }

        /// <summary>
        /// Called when the GameComponent needs to be updated. Override this method with component-specific update code.
        /// </summary>
        /// <param name="gameTime">Time elapsed since the last call to Update</param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Enter Pause check here.
            if (TemporalWars3DEngine.GamePaused)
                return;
            
            // Copy ChangeRequest keys
            var keysCount = TerrainDirectionalIcons.Keys.Count;
            if (_dictionaryKeys.Length != keysCount)
                Array.Resize(ref _dictionaryKeys, keysCount);
            TerrainDirectionalIcons.Keys.CopyTo(_dictionaryKeys, 0);

            // Copy ChangeRequest values
            var valuesCount = TerrainDirectionalIcons.Values.Count;
            if (_dictionaryValues.Length != valuesCount)
                Array.Resize(ref _dictionaryValues, valuesCount);
            TerrainDirectionalIcons.Values.CopyTo(_dictionaryValues, 0);

            // iterate collection and update
            // iterate outer keys collection
            var keysLength = _dictionaryKeys.Length;
            for (var i = 0; i < keysLength; i++)
            {
                var terrainDirectionalIcon = _dictionaryValues[i];
                if (terrainDirectionalIcon == null)
                    continue;

                terrainDirectionalIcon.Update(gameTime);
            }
        }

        /// <summary>
        /// Creates a new <see cref="TerrainDirectionalIcon"/> saved to the internal dictionary
        /// with the given key <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name to use as reference.</param>
        /// <returns>Instance of <see cref="TerrainDirectionalIcon"/></returns>
        public static TerrainDirectionalIcon CreateDirectionalIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            if (TerrainDirectionalIcons.ContainsKey(name))
            {
                throw new ArgumentException("Name given MUST be unique!","name");
            }

            // create new icon
            var directionalIcon = new TerrainDirectionalIcon(_arrayIndexCounter++);

            // add to dictionary
            TerrainDirectionalIcons.Add(name, directionalIcon);

            return directionalIcon;
        }

        /// <summary>
        /// Gets an instance of <see cref="TerrainDirectionalIcon"/> for the given name.
        /// </summary>
        /// <param name="name">Name of icon to retrieve.</param>
        /// <returns>Instance of <see cref="TerrainDirectionalIcon"/></returns>
        public static TerrainDirectionalIcon GetDirectionalIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            TerrainDirectionalIcon directionalIcon;
            if (TerrainDirectionalIcons.TryGetValue(name, out directionalIcon))
            {
                return directionalIcon;
            }

            throw new ArgumentException("Name given does not exist!", "name");
        }

        // 6/6/2012
        /// <summary>
        /// Sets to move Directional Icon in <see cref="TerrainDirectionalIcon.MovementDirection"/>.
        /// </summary>
        /// <param name="name">Name of icon to set movement request.</param>
        /// <param name="movementDirection"><see cref="TerrainDirectionalIcon.MovementDirection"/> to move in.</param>
        /// <param name="deltaMagnitude">The Position's delta magnitude change value. (Rate of Change)</param>
        /// <returns>Current position of Directional Icon.</returns>
        public static Vector3 SetMovementDirection(string name, TerrainDirectionalIcon.MovementDirection movementDirection, float deltaMagnitude)
        {
            var directionalIcon = GetDirectionalIcon(name);
            return directionalIcon.SetMovementDirection(movementDirection, deltaMagnitude);
        }

        // 6/6/2012
        /// <summary>
        /// Starts a rotation request for the given name.
        /// </summary>
        /// <param name="name">Name of icon to set rotation request.</param>
        /// <param name="deltaMagnitude">The Rotation's delta magnitude change value. (Rate of Change)</param>
        /// <param name="rotationTimeMax">Set to length of given rotation in milliseconds; 0 implies infinite.</param>
        public static void StartRotation(string name, float deltaMagnitude, int rotationTimeMax)
        {
            var directionalIcon = GetDirectionalIcon(name);
            directionalIcon.StartRotation(deltaMagnitude, rotationTimeMax);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_directionalIcon != null)
                _directionalIcon.Dispose();

            UnloadContent();
        }

        /// <summary>
        /// Used to unload resources during level loads.
        /// </summary>
        internal static void UnloadContent()
        {
            _arrayIndexCounter = 0;

            if (TerrainDirectionalIcons != null)
                TerrainDirectionalIcons.Clear();

            if (_dictionaryKeys != null)
                Array.Clear(_dictionaryKeys, 0, _dictionaryKeys.Length);

            if (_dictionaryValues != null)
                Array.Clear(_dictionaryValues, 0, _dictionaryValues.Length);
        }
       
    }
}