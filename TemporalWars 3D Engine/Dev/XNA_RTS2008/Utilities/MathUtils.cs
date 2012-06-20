#region File Description
//-----------------------------------------------------------------------------
// MathUtils.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace TWEngine.Utilities
{
    /// <summary>
    /// Singleton <see cref="MathUtils"/> class for helping with common math functions
    /// </summary>
    public class MathUtils
    {
        private static readonly Random Random = new Random();

        // 12/17/2009: Updated to use Ref/Out.
        // 5/9/2008 -
        ///<summary>
        /// Helper function to convert <see cref="Quaternion"/> values to a euler rotation <see cref="Vector3"/>.
        ///</summary>
        ///<param name="rotation"><see cref="Quaternion"/> struct</param>
        ///<param name="eulerRotation">(OUT) <see cref="Vector3"/> as euler rotation</param>
        public static void QuaternionToEuler(ref Quaternion rotation, out Vector3 eulerRotation)
        {
            eulerRotation = default(Vector3); // 6/22/2010

            try // 6/22/2010
            {
                var q0 = rotation.W;
                var q1 = rotation.Y;
                var q2 = rotation.X;
                var q3 = rotation.Z;

                var radAngles = new Vector3
                {
                    X = (float)Math.Asin(2 * (q0 * q2 - q3 * q1)),
                    Y = (float)Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - 2 * (Math.Pow(q1, 2) + Math.Pow(q2, 2))),
                    Z = (float)Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - 2 * (Math.Pow(q2, 2) + Math.Pow(q3, 2)))
                };

                eulerRotation = new Vector3
                {
                    X = MathHelper.ToDegrees(radAngles.X),
                    Y = MathHelper.ToDegrees(radAngles.Y),
                    Z = MathHelper.ToDegrees(radAngles.Z)
                };
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("QuaternionToEuler method threw the exception " + ex.Message ?? "No Message");
#endif
            }

        }

        ///<summary>
        /// Returns direction vector from <paramref name="posFirst"/> to <see cref="posSecond"/>.
        ///</summary>
        ///<param name="posFirst"><see cref="Vector3"/> first position</param>
        ///<param name="posSecond"><see cref="Vector3"/> second position</param>
        ///<param name="finalResult">(OUT) <see cref="Vector3"/> direction</param>
        public static void DirectionFirstToSecond(Vector3 posFirst, Vector3 posSecond, out Vector3 finalResult)
        {
            // 5/20/2010 - Updated calcs.
            Vector3 result;
            Vector3.Subtract(ref posSecond, ref posFirst, out result);

            // Normalize
            Vector3.Normalize(ref result, out finalResult);

            //return Vector3.Normalize(posSecond - posFirst);
        }

        ///<summary>
        /// Distance between vector <paramref name="posFirst"/> to <paramref name="posSecond"/>.
        ///</summary>
        ///<param name="posFirst"><see cref="Vector3"/> first position</param>
        ///<param name="posSecond"><see cref="Vector3"/> second position</param>
        ///<returns>distance between vectors</returns>
        public static float DistanceVectToVect(Vector3 posFirst, Vector3 posSecond)
        {
            // 5/20/2010 - Updated calcs.
            Vector3 result;
            Vector3.Subtract(ref posFirst, ref posSecond, out result);

            return result.Length();
        }

        ///<summary>
        /// Distance between vector <paramref name="posFirst"/> to <paramref name="posSecond"/>.
        ///</summary>
        ///<param name="posFirst"><see cref="Vector3"/> first position</param>
        ///<param name="posSecond"><see cref="Vector3"/> second position</param>
        ///<returns>distance between vectors</returns>
        public static float DistanceVectToVect(Vector3 posFirst, ref Vector3 posSecond)
        {
            // 5/20/2010 - Updated calcs.
            Vector3 result;
            Vector3.Subtract(ref posFirst, ref posSecond, out result);

            return result.Length();
        }

        ///<summary>
        /// Distance between vector <paramref name="posFirst"/> to <paramref name="posSecond"/>.
        ///</summary>
        ///<param name="posFirst"><see cref="Vector3"/> first position</param>
        ///<param name="posSecond"><see cref="Vector3"/> second position</param>
        ///<returns>distance between vectors</returns>
        public static float DistanceVectToVect(ref Vector3 posFirst, ref Vector3 posSecond)
        {
            // 5/20/2010 - Updated calcs.
            Vector3 result;
            Vector3.Subtract(ref posFirst, ref posSecond, out result);

            return result.Length();
        }

        ///<summary>
        /// Returns if given value is a power of 2.
        ///</summary>
        ///<param name="value">Value to check</param>
        ///<returns>true/false of result</returns>
        public static bool IsPowerOf2(int value)
        {
            if (value < 2)
                return false;

            return (value & (value - 1)) == 0;
        }

        ///<summary>
        /// Returns a random number between the given <paramref name="min"/> and <paramref name="max"/> values.
        ///</summary>
        ///<param name="min">Minimum range value</param>
        ///<param name="max">Maximum range value</param>
        ///<returns>Some random number between given range</returns>
        public static float RandomBetween(double min, double max)
        {
            return (float)(min + (float)Random.NextDouble() * (max - min));
        }

        /// <summary>
        /// 50/50 chance of returning either -1 or 1
        /// </summary>
        public static int Random5050
        {
            get { return RandomBetween(0, 2) >= 1 ? 1 : -1; }
        }
       
    }
}
