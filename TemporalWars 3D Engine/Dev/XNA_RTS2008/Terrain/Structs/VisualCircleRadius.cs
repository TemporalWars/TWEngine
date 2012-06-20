#region File Description
//-----------------------------------------------------------------------------
// VisualCircleRadius.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;

namespace TWEngine.Terrain.Structs
{
    // 4/11/2009
    ///<summary>
    /// Holds the information on where a visual 'CircleRadius' will
    /// be drawn on the <see cref="Terrain"/>.  For example, <see cref="DefenseScene"/> items will
    /// save their 'AttackRadius' here, which will show a visual 'Circle' within the Terrain shader.
    ///</summary>
    public struct VisualCircleRadius
    {
        ///<summary>
        /// constructor
        ///</summary>
        ///<param name="circlePosition"><see cref="Vector3"/> as circle position</param>
        ///<param name="circleSize">circle size</param>
        public VisualCircleRadius(Vector3 circlePosition, float circleSize)
        {
            _circlePosition = circlePosition;
            _circlePositionScaled = Vector2.Zero;
            _circleSize = circleSize;

            // divide Position by terrainScale
            TransformPositionToVector2Scaled();
        }

        /// <summary>
        /// Transforms a <see cref="Vector3"/>, by diving by <see cref="TerrainData.cScale"/>, and storing
        /// the x/z values into a <see cref="Vector2"/>.
        /// </summary>
        private void TransformPositionToVector2Scaled()
        {
            Vector3 tmpPositionScaled;
            Vector3.Divide(ref _circlePosition, TerrainData.cScale, out tmpPositionScaled);

            // store into Vector2
            _circlePositionScaled.X = tmpPositionScaled.X;
            _circlePositionScaled.Y = tmpPositionScaled.Z;
        }

        private Vector3 _circlePosition;
        ///<summary>
        /// Set or get the current circle position
        ///</summary>
        public Vector3 CirclePosition
        {
            get { return _circlePosition; }
            set
            {
                _circlePosition = value;

                // divide Position by terrainScale
                TransformPositionToVector2Scaled();
            }
        }
        private Vector2 _circlePositionScaled; // Divided by Terrain.Scale.
        ///<summary>
        /// Returns the current circle position, scaled by <see cref="TerrainData.cScale"/>.
        ///</summary>
        public Vector2 CirclePositionScaled
        {
            get { return _circlePositionScaled; }
        }
        private float _circleSize; // radius
        ///<summary>
        /// Set or get the current circle size.
        ///</summary>
        public float CircleSize
        {
            get { return _circleSize; }
            set { _circleSize = value; }
        }

    }
}
