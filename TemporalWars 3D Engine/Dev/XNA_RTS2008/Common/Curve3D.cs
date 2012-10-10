#region File Description
//-----------------------------------------------------------------------------
// Curve3D.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Common
{
    /// <summary>
    /// Curve3D class provides the ability to do smooth Vector3 curves, using the XNA frameworks
    /// Curve instances.
    /// </summary>
    class Curve3D : ICurve3D
    {

        private Curve _curveX = new Curve();
        private Curve _curveY = new Curve();
        private Curve _curveZ = new Curve();

        // 8/7/2009 - StraightPath CurveKeysVector3 struct
        private struct CurveKeyVector3
        {
            private readonly CurveKey _x;
            private readonly CurveKey _y;
            private readonly CurveKey _z;

            /// <summary>
            /// Constructor: Used to intialize a new CurveKeyVector3.  Once the
            /// Time is set, is can not be changed!
            /// </summary>
            /// <param name="time"></param>
            /// <param name="point"></param>
            public CurveKeyVector3(float time, ref Vector3 point)
            {
                _x = new CurveKey(time, point.X);
                _y = new CurveKey(time, point.Y);
                _z = new CurveKey(time, point.Z);
            }

            /// <summary>
            /// Allows updating the Point of the CurveKey.
            /// </summary>
            /// <param name="point"></param>
            public void UpdatePoint(ref Vector3 point)
            {
                _x.Value = point.X;
                _y.Value = point.Y;
                _z.Value = point.Z;
            }

            /// <summary>
            /// Returns the Curve3D X axis.
            /// </summary>
            public CurveKey X
            {
                get { return _x; }
            }

            /// <summary>
            /// Returns the Curve3D Z axis.
            /// </summary>
            public CurveKey Z
            {
                get { return _z; }
            }

            /// <summary>
            /// Returns the Curve3D Y axis.
            /// </summary>
            public CurveKey Y
            {
                get { return _y; }
            }
           
        }

        // 8/7/2009
        private CurveKeyVector3 _straightPathStartPoint;
        private CurveKeyVector3 _straightPathEndPoint;
        private bool _straightPathPointsCreated;

        #region Properties

        /// <summary>
        /// Get/Set the Curve's X axis.
        /// </summary>
        public Curve CurveX
        {
            get { return _curveX; }
            set { _curveX = value; }
        }

        /// <summary>
        /// Get/Set the Curve's Y axis.
        /// </summary>
        public Curve CurveY
        {
            get { return _curveY; }
            set { _curveY = value; }
        }

        /// <summary>
        /// Get/Set the Curve's Z axis.
        /// </summary>
        public Curve CurveZ
        {
            get { return _curveZ; }
            set { _curveZ = value; }
        }

        // 10/22/2009
        /// <summary>
        /// Used to know when the Curve3D as passed the max
        /// amount of time allowed.
        /// </summary>
        public double MaxTimeAllowed { get; set; }

        // 6/7/2012
        /// <summary>
        /// Gets or sets this Curve3D name.
        /// </summary>
        public string Curve3DName { get; set; }

        #endregion

        public Curve3D()
        {            
            _curveX.PostLoop = CurveLoopType.Constant; // Was Oscillate
            _curveY.PostLoop = CurveLoopType.Constant;
            _curveZ.PostLoop = CurveLoopType.Constant;

            _curveX.PreLoop = CurveLoopType.Constant;
            _curveY.PreLoop = CurveLoopType.Constant;
            _curveZ.PreLoop = CurveLoopType.Constant;
        }

        /// <summary>
        /// Sets up the Tangents between the Points on the Curve.
        /// </summary>        
        public void SetTangents()
        {             
            for (var i = 0; i < _curveX.Keys.Count; i++)
            {
                var prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                var nextIndex = i + 1;
                if (nextIndex == _curveX.Keys.Count) nextIndex = i;

                var prev = _curveX.Keys[prevIndex];
                var next = _curveX.Keys[nextIndex];
                var current = _curveX.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                _curveX.Keys[i] = current;
                prev = _curveY.Keys[prevIndex];
                next = _curveY.Keys[nextIndex];
                current = _curveY.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                _curveY.Keys[i] = current;

                prev = _curveZ.Keys[prevIndex];
                next = _curveZ.Keys[nextIndex];
                current = _curveZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                _curveZ.Keys[i] = current;
            }
        }

        /// <summary>
        /// Sets up the Curves Key Tangent's for Slopes
        /// </summary>
        /// <param name="prev">Previous CurveKey</param>
        /// <param name="cur">Current CurveKey</param>
        /// <param name="next">Next CurveKey</param>        
        static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, ref CurveKey next)
        {
            var dt = next.Position - prev.Position;
            var dv = next.Value - prev.Value;
            if (Math.Abs(dv) < float.Epsilon)
            {
                cur.TangentIn = 0;
                cur.TangentOut = 0;
            }
            else
            {
                // The in and out tangents should be equal to the slope between the adjacent keys.
                cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
                cur.TangentOut = dv * (next.Position - cur.Position) / dt;
            }
        }

        /// <summary>
        /// Add Points to create the Curves at a given Time interval.
        /// </summary>
        /// <param name="point">3D Point for Curve</param>
        /// <param name="time">Time at which we should be at this point</param>
        public void AddPoint(ref Vector3 point, float time)
        {
            _curveX.Keys.Add(new CurveKey(time, point.X));
            _curveY.Keys.Add(new CurveKey(time, point.Y));
            _curveZ.Keys.Add(new CurveKey(time, point.Z));
           
        }

        // 8/7/2009
        /// <summary>
        /// Adds 2 CurveKey points to the internal Curve instance.  
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        private void AddPoints(ref CurveKeyVector3 startPoint, ref CurveKeyVector3 endPoint)
        {
            // Add StartPoint
            _curveX.Keys.Add(startPoint.X);
            _curveY.Keys.Add(startPoint.Y);
            _curveZ.Keys.Add(startPoint.Z);

            // Add EndPoint
            _curveX.Keys.Add(endPoint.X);
            _curveY.Keys.Add(endPoint.Y);
            _curveZ.Keys.Add(endPoint.Z);

        }

        // 8/7/2009
        /// <summary>
        /// This will add 2 points, which define a StraightPath over Time.  The Time is set
        /// only once, and cannot be adjusted!  This is due to how the CurveKey is created by XNA
        /// framework, and does not allow changes to the Time value.  However, the positions can
        /// be updated, by calling this method again.
        /// </summary>
        /// <param name="endPoint">The end point of a straight line expresssed as a <see cref="Vector3"/>.</param>
        /// <param name="startPoint">The start point of a straight line expressed as a <see cref="Vector3"/>.</param>
        /// <param name="timeMultiplier">The object speed is interpolated based on the given time between the <paramref name="endPoint"/>
        /// and the <paramref name="startPoint"/>.</param>
        public void AddStraightPath(float timeMultiplier, ref Vector3 startPoint, ref Vector3 endPoint)
        {
            // Create the CurveKeyVector3 structs only once.
            if (!_straightPathPointsCreated)
            {
                // create structs first
                _straightPathStartPoint = new CurveKeyVector3(0, ref startPoint);
                _straightPathEndPoint = new CurveKeyVector3(1000 * timeMultiplier, ref endPoint);

                // Add Points to Curve
                AddPoints(ref _straightPathStartPoint, ref _straightPathEndPoint);

                _straightPathPointsCreated = true;
                return;
            }

            // 10/22/2009 - Set MaxAllowedtime
            MaxTimeAllowed = 1000*timeMultiplier;

            // Else change current points and update
            _straightPathStartPoint.UpdatePoint(ref startPoint);
            _straightPathEndPoint.UpdatePoint(ref endPoint);

            // Add Points to Curve
            AddPoints(ref _straightPathStartPoint, ref _straightPathEndPoint);
        }

        // 5/18/2009
        /// <summary>
        /// Clears all 'Keys' from the internal Curve X/Y/Z instances.
        /// </summary>
        public void ClearAll()
        {
            _curveX.Keys.Clear();
            _curveY.Keys.Clear();
            _curveZ.Keys.Clear();
        }

        /// <summary>
        /// Retrieves the Points, Interpolated between the Added Points, at a given Time.
        /// </summary>
        /// <param name="time">The Time to use</param>
        /// <param name="point">(OUT) <see cref="Vector3"/> Point on the Curve at the given Time.</param>
        /// <returns>True/False if the time is greater than the maximum time allowed.</returns>       
        public bool GetPointOnCurve(float time, out Vector3 point)
        {
            var tmpPoint = Vector3.Zero;
            tmpPoint.X = _curveX.Evaluate(time);
            tmpPoint.Y = _curveY.Evaluate(time);
            tmpPoint.Z = _curveZ.Evaluate(time);
           
            point = tmpPoint;

            return IsSplineComplete(time);
        }

        // 6/7/2012
        /// <summary>
        /// Checks if the time given is greater than the <see cref="MaxTimeAllowed"/>.
        /// </summary>
        /// <param name="time">Time value to check</param>
        /// <returns>true/false of result.</returns>
        public bool IsSplineComplete(float time)
        {
            return time > MaxTimeAllowed;
        }
    }
}