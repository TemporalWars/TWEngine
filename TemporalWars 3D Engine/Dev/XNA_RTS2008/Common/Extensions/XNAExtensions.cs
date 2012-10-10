#region File Description
//-----------------------------------------------------------------------------
// XNAExtensions.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Structs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Collections.Generic;

namespace ImageNexus.BenScharbach.TWEngine.Common.Extensions
{
    ///<summary>
    /// The <see cref="XNAExtensions"/> is used to extend other pre-made classes with new
    /// abilities.  For example, the <see cref="BoundingBox"/> has been extended with a new
    /// version of <see cref="CreateFromPointsV2"/>, which eliminates garbage for Xbox.
    ///</summary>
    public static class XNAExtensions
    {
        // 3/31/2011
        ///<summary>
        /// Copies the <paramref name="textureSource"/> data to the <paramref name="textureDest"/>.
        ///</summary>
        ///<param name="textureSource">Instance of <see cref="Texture2D"/> as source.</param>
        ///<param name="textureDest">Instance of <see cref="Texture2D"/> as destination.</param>
        ///<exception cref="InvalidOperationException">Thrown when formats between the source and destination do not match.</exception>
        ///<exception cref="ArgumentOutOfRangeException">Thrown when surface format cannot be handled by extension.</exception>
        public static void CopyTo(this Texture2D textureSource, Texture2D textureDest) 
        {
            if (textureDest == null) throw new ArgumentNullException("textureDest");

            // Verify dest has same format as source
            if (textureDest.Format != textureSource.Format)
                throw new InvalidOperationException("Destination Texture2D format MUST match Source Texture2D.");

            switch (textureSource.Format)
            {
                case SurfaceFormat.Color:
                    {
                        // Get Texture Source data
                        var tmpData = new Color[textureSource.Height*textureSource.Width];
                        textureSource.GetData(tmpData);
                        // Save Texture data
                        textureDest.SetData(tmpData);
                    }
                    break;
                case SurfaceFormat.Single:
                    {
                        // Get Texture Source data
                        var tmpData = new float[textureSource.Height*textureSource.Width];
                        textureSource.GetData(tmpData);
                        // Save Texture data
                        textureDest.SetData(tmpData);
                    }
                    break;
                case SurfaceFormat.Vector2:
                    {
                        // Get Texture Source data
                        var tmpData = new Vector2[textureSource.Height*textureSource.Width];
                        textureSource.GetData(tmpData);
                        // Save Texture data
                        textureDest.SetData(tmpData);
                    }
                    break;
                case SurfaceFormat.Vector4:
                    {
                        // Get Texture Source data
                        var tmpData = new Vector4[textureSource.Height*textureSource.Width];
                        textureSource.GetData(tmpData);
                        // Save Texture data
                        textureDest.SetData(tmpData);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

           
        }

        // 6/12/2010
        /// <summary>
        /// Extension method for the <see cref="TimeSpan"/>, which adds the ability to
        /// subtract the light-weight <see cref="BehaviorsTimeSpan"/> version.
        /// </summary>
        /// <param name="t1">Automatic pass of this <see cref="TimeSpan"/></param>
        /// <param name="t2"><see cref="BehaviorsTimeSpan"/> to subtract</param>
        /// <returns><see cref="TimeSpan"/> result</returns>
        public static TimeSpan Subtract(this TimeSpan t1, BehaviorsTimeSpan t2)
        {
            // Create TimeSpan version to subtract
            var timeSpan = new TimeSpan(t2.Ticks);

            return t1.Subtract(timeSpan);
        }


        // 8/25/2009
        /// <summary>
        /// Creates the smallest BoundingBox that will contain a group of points.
        /// <remarks>This version overrides the XNA version, which used the ForEach construct creating garbage on the HEAP!</remarks>  
        /// </summary>
        /// <param name="boundingBox">This instance of <see cref="BoundingBox"/>.</param>
        /// <param name="points">Collection of <see cref="Vector3"/> points.</param>
        /// <returns>Instance of <see cref="BoundingBox"/>.</returns>
        public static BoundingBox CreateFromPointsV2(this BoundingBox boundingBox, Vector3[] points)
        {
            var flag = false;
            var vector3 = new Vector3(float.MaxValue);
            var vector2 = new Vector3(float.MinValue);

            var length = points.Length;
            for (int index = 0; index < length; index++)
            {
                var vector = points[index];
                var vector4 = vector;
                Vector3.Min(ref vector3, ref vector4, out vector3);
                Vector3.Max(ref vector2, ref vector4, out vector2);
                flag = true;
            }
            if (!flag)
            {
                throw new ArgumentException("BoundingBox has Zero Points!");
            }
            return new BoundingBox(vector3, vector2);

        }

        // 1/29/2010
        /// <summary>
        /// Extension method, used to add a Vector3 to a HalfVector4.
        /// </summary>
        /// <param name="leftArg">(This) auto-param.</param>
        /// <param name="rightArg">Enter Vector3 to add</param>
        /// <returns>Result as HalfVector4</returns>
        public static HalfVector4 AddVector3(this HalfVector4 leftArg, Vector3 rightArg)
        {
            // Convert to Vector3
            var leftAsVector3 = leftArg.ToVector3();

            // Add arguments together
            Vector3 result;
            Vector3.Add(ref leftAsVector3, ref rightArg, out result);
            
            // Save result pack into packed format.
            return new HalfVector4(result.X, result.Y, result.Z, 0);
        }

        // 1/29/2010
        /// <summary>
        /// Extension method, used to normalize the internal value; the W
        /// component is ignored for this operation.
        /// </summary>
        /// <param name="leftArg">(This) auto-param.</param>
        /// <returns>Result as HalfVector4</returns>
        public static HalfVector4 Normalize(this HalfVector4 leftArg)
        {
            // Convert to Vector3
            var leftAsVector3 = leftArg.ToVector3();

            // Normalize Vector3
            leftAsVector3.Normalize();

            // Save result pack into packed format.
            return new HalfVector4(leftAsVector3.X, leftAsVector3.Y, leftAsVector3.Z, 0);
        }

        // 1/29/2010
        /// <summary>
        /// Extension method, used to convert HalfVector4 to Vector3; the W
        /// component is ignored for this operation.
        /// </summary>
        /// <param name="leftArg">(This) auto-param.</param>
        /// <returns>Vector3 struct</returns>
        public static Vector3 ToVector3(this HalfVector4 leftArg)
        {
            // Unpack left Argument, to Vector3, to perform operation
            var leftAsVector4 = leftArg.ToVector4();
            return new Vector3(leftAsVector4.X, leftAsVector4.Y, leftAsVector4.Z);
        }

#if XBOX360

        // 3/12/2011 - New method for XNA 4.0.
        public static int FindIndex<T>(this List<T> collection, Predicate<T> match)
        {
            return collection.FindIndex(0, collection.Count, match);
        }

        // 3/12/2011 - New method for XNA 4.0.
        public static int FindIndex<T>(this List<T> collection, int startIndex, Predicate<T> match)
        {
            return collection.FindIndex(startIndex, collection.Count - startIndex, match);
        }

        // 3/12/2011 - New method for XNA 4.0.
        public static int FindIndex<T>(this List<T> collection, int startIndex, int count, Predicate<T> match)
        {
            if (startIndex > collection.Count)
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
                throw new ArgumentOutOfRangeException("startIndex");
            }
            if ((count < 0) || (startIndex > (collection.Count - count)))
            {
                //ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.count, ExceptionResource.ArgumentOutOfRange_Count);
                throw new ArgumentOutOfRangeException("count");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }
            var num = startIndex + count;
            for (var i = startIndex; i < num; i++)
            {
                if (match(collection[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        // 3/12/2011 - New method for XNA 4.0.
        /// <summary>
        /// Extension method, used to replace the RemoveAll method call of the generic List{T} collection which
        /// is unavailble for the Xbox framework in XNA 4.0.
        /// </summary>
        /// <typeparam name="T">Generic type to use.</typeparam>
        /// <param name="collection">Extension This param.</param>
        /// <param name="match">Predicate Match</param>
        public static int RemoveAll<T>(this List<T> collection, Predicate<T> match)
        {
            if (match == null) throw new ArgumentNullException("match");

            var index = 0;
            var removeCount = 0;
            var count = collection.Count;
            while ((index < count))
            {
                // check for match
                if (match(collection[index]))
                {
                    collection.RemoveAt(index);
                    removeCount++;
                    // 3/24/2011 - Fix: Need to decrease count value.
                    count--;
                }
                else
                    index++;
            }

            return removeCount;

            /*if (index >= collection.Count)
            {
                return 0;
            }
            int num2 = index + 1;
            while (num2 < collection.Count)
            {
                while ((num2 < collection.Count) && match(collection[num2]))
                {
                    num2++;
                }
                if (num2 < collection.Count)
                {
                    collection[index++] = collection[num2++];
                }
            }
            Array.Clear(collection.ToArray(), index, collection.Count - index);
            int num3 = collection.Count - index;
            this._size = index;
            this._version++;
            return num3;*/
        }

#endif

        // 8/25/2009
        /*public static void CopyReverseTo(this ICollection<int> collection, int[] array, int index, int length)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if ((index < 0) || (index > array.Length))
            {
                throw new ArgumentOutOfRangeException("index", "Index out of range; need non-negative number.");
            }
            if ((array.Length - index) < collection.Count)
            {
                throw new ArgumentException("Array given too small to fit requested range.");
            }

            IEnumerator<int> enumerator = collection.GetEnumerator();
            int orgIndex = index;
            
            // Copy collection to array
            while(enumerator.MoveNext())
            {
                array[orgIndex++] = enumerator.Current;
            }

            // Reverse order
            int num = index;
            int num2 = (index + length) - 1;
           
            while (num < num2)
            {
                int obj2 = array[num];
                array[num] = array[num2];
                array[num2] = obj2;
                num++;
                num2--;
            }

        }*/
        
    }
}