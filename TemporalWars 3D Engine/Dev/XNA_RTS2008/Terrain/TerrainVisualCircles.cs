#region File Description
//-----------------------------------------------------------------------------
// TerrainVisualCircles.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.Terrain.Structs;

namespace TWEngine.Terrain
{

    ///<summary>
    /// The <see cref="TerrainVisualCircles"/> class is used to draw 
    /// visual circles into the game world, visible on the <see cref="Terrain"/>.
    ///</summary>
    public class TerrainVisualCircles : IDisposable
    {       
        // 4/11/2009 - 'VisualCircleRadius' array
        private bool _visualCirclesUpdated;        
        private readonly Dictionary<int, VisualCircleRadius> _visualCircles;
        private int _indexKey;
        private int _indexToDisplay = -1;

        private bool _effectParamsSet;
        private EffectParameter _circlePositionEP;
        private EffectParameter _circleSizeEP;
        private EffectParameter _circleOnEP;
        private EffectParameter _groundCursorTextureEP;
        private readonly Texture2D _groundCursorTex;

        // 11/2/2009: Updated to pass in the 'ZippedContent'.
        // constructor
        ///<summary>
        /// Constructor, which loads the 'visualDottedCircle' texture into memory.
        ///</summary>
        ///<param name="zippedContent"><see cref="ContentManager"/> used to load texture</param>
        public TerrainVisualCircles(ContentManager zippedContent)
        {
            // Init Dictionary array
            _visualCircles = new Dictionary<int, VisualCircleRadius>();

            // 11/2/2009: Updated to texture Name only.
            _groundCursorTex = zippedContent.Load<Texture2D>("visualDottedCircle"); // @"ContentTextures\Terrain\visualDottedCircle"
        }

        /// <summary>
        /// Adds a new <see cref="VisualCircleRadius"/> into the internal dictionary.
        /// </summary>
        /// <param name="visualCircle"><see cref="VisualCircleRadius"/> Struct to add</param>
        /// <returns>Index to new entry</returns>
        public int AddVisualCircle(ref VisualCircleRadius visualCircle)
        {   
            // will update shader with new values
            _visualCirclesUpdated = true;

            // create new unique key for dictionary
            _indexKey++;

            // add new entry into dictionary
            _visualCircles.Add(_indexKey, visualCircle);

            return _indexKey;
           
        }

        /// <summary>
        /// Updates an existing <see cref="VisualCircleRadius"/> in the dictionary.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> given is not valid.</exception>
        /// <param name="index">Index of <see cref="VisualCircleRadius"/> to update</param>
        /// <param name="visualCircle">Updated <see cref="VisualCircleRadius"/></param>
        public void UpdateVisualCircle(int index, ref VisualCircleRadius visualCircle)
        {
            // check if index given is valid            
            if (!_visualCircles.ContainsKey(index))
                throw new ArgumentOutOfRangeException("index", @"Index given is not valid.");

            // will update shader with new values
            _visualCirclesUpdated = true;

            // Update entry using index given.
            _visualCircles[index] = visualCircle;
        }

        /// <summary>
        /// Removes an existing <see cref="VisualCircleRadius"/> entry from internal array.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> given is not valid.</exception>
        /// <param name="index">Index of <see cref="VisualCircleRadius"/> to remove</param>
        public void RemoveVisualCircle(int index)
        {
            // check if index given is valid            
            if (!_visualCircles.ContainsKey(index))
                throw new ArgumentOutOfRangeException("index", @"Index given is not valid.");

            // 8/27/2009 - Make sure circle is not visible anymore
            ShowVisualCircle(index, false);

            // Remove entry using index given
            _visualCircles.Remove(index);
        }

        /// <summary>
        /// Updates <see cref="Effect"/> visibility flag for drawing the <see cref="VisualCircleRadius"/>.
        /// </summary>
        /// <param name="index">Index of <see cref="VisualCircleRadius"/></param>
        /// <param name="displayCircle">true/false to display circle</param>
        public void ShowVisualCircle(int index, bool displayCircle)
        {
            // check if index given is valid
            if (!_visualCircles.ContainsKey(index)) return;

            // will update shader with new values
            _visualCirclesUpdated = true;

            _indexToDisplay = displayCircle ? index : -1;
        }        

        /// <summary>
        /// Sets the <see cref="VisualCircleRadius"/> values into <see cref="Terrain"/> <see cref="Effect"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="effect"/> given is null.</exception>
        /// <param name="effect"><see cref="Effect"/> instance</param>
        public void SetEffectParameters(Effect effect)
        {
            // 5/19/2010 - Check if effect null.
            if (effect == null)
                throw new ArgumentNullException("effect", @"Effect given cannot be null!");

            // Init EffectParams the first Time.
            if (!_effectParamsSet)
            {
                _circlePositionEP = effect.Parameters["xCirclePositions"];
                _circleSizeEP = effect.Parameters["xCircleSizes"];
                _circleOnEP = effect.Parameters["xShowVisualCircle"];                 
                _groundCursorTextureEP = effect.Parameters["xGroundCursorTex"];

                _effectParamsSet = true;
            }

            // only update when values change.
            if (!_visualCirclesUpdated) return;

            if (_indexToDisplay != -1)
            {
                // 4/12/2009
                // check if index given is valid
                if (_visualCircles.ContainsKey(_indexToDisplay))
                {
                    var visualCircle = _visualCircles[_indexToDisplay];

                    _circlePositionEP.SetValue(visualCircle.CirclePositionScaled);
                    _circleSizeEP.SetValue(visualCircle.CircleSize);
                    _circleOnEP.SetValue(true);
                    _groundCursorTextureEP.SetValue(_groundCursorTex);
                }
            }
            else
            {
                _circleOnEP.SetValue(false);
            }

            _visualCirclesUpdated = false;
        }

        // 1/8/2010
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose
            if (_groundCursorTex != null)
                _groundCursorTex.Dispose();

            // Clear Arrays
            _visualCircles.Clear();
        }
    }
}
