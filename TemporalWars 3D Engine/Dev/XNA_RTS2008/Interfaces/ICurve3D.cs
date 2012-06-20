#region File Description
//-----------------------------------------------------------------------------
// ICurve3D.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace TWEngine.Interfaces
{
    /// <summary>
    /// Curve3D class provides the ability to do smooth Vector3 curves, using the XNA frameworks
    /// Curve instances.
    /// </summary>
    public interface ICurve3D
    {
        /// <summary>
        /// Add Points to create the Curves at a given Time interval.
        /// </summary>
        /// <param name="point">3D Point for Curve</param>
        /// <param name="time">Time at which we should be at this point</param>
        void AddPoint(ref Vector3 point, float time);
        /// <summary>
        /// Get/Set the Curve's X axis.
        /// </summary>
        Curve CurveX { get; set; }
        /// <summary>
        /// Get/Set the Curve's Y axis.
        /// </summary>
        Curve CurveY { get; set; }
        /// <summary>
        /// Get/Set the Curve's Z axis.
        /// </summary>
        Curve CurveZ { get; set; }
        /// <summary>
        /// Retrieves the Points, Interpolated between the Added Points, at a given Time.
        /// </summary>
        /// <param name="time">The Time to use</param>
        /// <param name="point">(OUT) returns the interpolation point as a <see cref="Vector3"/>.</param>
        /// <returns>Vector3 Point on the Curve at the given Time.</returns>   
        bool GetPointOnCurve(float time, out Vector3 point);
        /// <summary>
        /// Sets up the Tangents between the Points on the Curve.
        /// </summary>        
        void SetTangents();
    }
}