#region File Description
//-----------------------------------------------------------------------------
// InstancedModelBoneCollection.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace TWEngine.InstancedModels
{
    // 1/27/2010
    /// <summary>
    /// The <see cref="InstancedModelBoneCollection"/> class, is a <see cref="ReadOnlyCollection{TValue}"/> of
    /// type <see cref="InstancedModelBone"/>, providing an <see cref="Enumerator"/> to iterate the collection.
    /// </summary>
    public class InstancedModelBoneCollection : ReadOnlyCollection<InstancedModelBone>
    {
        // Fields
        /// <summary>
        /// Internal array of <see cref="InstancedModelBone"/>.
        /// </summary>
        private readonly InstancedModelBone[] _wrappedArray;

        // 1/28/2010 - 
        /// <summary>
        /// Root <see cref="InstancedModelBone"/> reference.
        /// </summary>
        public InstancedModelBone Root { get; set; }

        // Methods
        /// <summary>
        /// Constructor for the <see cref="InstancedModelBoneCollection"/>, which saves
        /// the given <paramref name="bones"/> collection, as the <see cref="_wrappedArray"/>
        /// array.
        /// </summary>
        /// <param name="bones">Array of <see cref="InstancedModelBone"/>.</param>
        internal InstancedModelBoneCollection(InstancedModelBone[] bones)
            : base(bones)
        {
            _wrappedArray = bones;
        }

        /// <summary>
        /// Returns a reference to the <see cref="InstancedModelBone"/>
        /// with the given <paramref name="boneName"/> value.
        /// </summary>
        /// <param name="boneName">Bone name to retrieve</param>
        /// <returns><see cref="InstancedModelBone"/> reference</returns>
        public InstancedModelBone this[string boneName]
        {
            get
            {
                InstancedModelBone bone;
                if (!TryGetValue(boneName, out bone))
                {
                    throw new KeyNotFoundException();
                }
                return bone;
            }
        }

        /// <summary>
        /// Returns an <see cref="Enumerator"/> for the given collection.
        /// </summary>
        /// <returns>Returns the <see cref="Enumerator"/>.</returns>
        public new Enumerator GetEnumerator()
        {
            return new Enumerator(_wrappedArray);
        }
        
        /// <summary>
        /// Attempts to return an <see cref="InstancedModelBone"/> for the
        /// given <paramref name="boneName"/>.
        /// </summary>
        /// <param name="boneName">Bone name to attempt retrieval</param>
        /// <param name="value">(OUT) <see cref="InstancedModelBone"/> reference</param>
        /// <returns>True/False of result</returns>
        public bool TryGetValue(string boneName, out InstancedModelBone value)
        {
            if (string.IsNullOrEmpty(boneName))
            {
                throw new ArgumentNullException("boneName");
            }
            var count = Items.Count;
            for (var i = 0; i < count; i++)
            {
                var bone = Items[i];
                if (string.Compare(bone.Name, boneName, StringComparison.Ordinal) == 0)
                {
                    value = bone;
                    return true;
                }
            }
            value = null;
            return false;
        }

        // Properties

        // Nested Types
        #region Nested type: Enumerator

        /// <summary>
        /// An <see cref="Enumerator"/> for the <see cref="InstancedModelBone"/> type.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<InstancedModelBone>
        {
            private readonly InstancedModelBone[] wrappedArray;
            private int position;

            internal Enumerator(InstancedModelBone[] wrappedArray)
            {
                this.wrappedArray = wrappedArray;
                position = -1;
            }
           
            /// <summary>
            /// The current position in the collection.
            /// </summary>
            public InstancedModelBone Current
            {
                get { return wrappedArray[position]; }
            }

            /// <summary>
            /// Move to the next <see cref="InstancedModelBone"/> in the collection.
            /// </summary>
            /// <returns>True or False result.</returns>
            public bool MoveNext()
            {
                position++;
                if (position >= wrappedArray.Length)
                {
                    position = wrappedArray.Length;
                    return false;
                }
                return true;
            }

            void IEnumerator.Reset()
            {
                position = -1;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <filterpriority>2</filterpriority>
            public void Dispose()
            {
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

        #endregion
    }
}