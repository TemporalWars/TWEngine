#region File Description
//-----------------------------------------------------------------------------
// InstancedModelBone.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    // 1/27/2010
    /// <summary>
    /// The <see cref="InstancedModelBone"/> class, holds the 
    /// Bone information for a given <see cref="InstancedModel"/> class. 
    /// </summary>
    public class InstancedModelBone
    {
        // Fields
        private readonly int _index;
        private readonly int _parentIndex; // 3/21/2011
        private readonly string _name;
        private InstancedModelBone _parent;
        private Matrix _transform;

        #region Properties

        // Properties
        /// <summary>
        /// Bone index
        /// </summary>
        public int Index
        {
            get
            {
                return _index;
            }
        }

        /// <summary>
        /// Bone name
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Parent <see cref="InstancedModelBone"/> instance reference
        /// </summary>
// ReSharper disable ConvertToAutoProperty
        public InstancedModelBone Parent
// ReSharper restore ConvertToAutoProperty
        {
            get
            {
                return _parent;
            }
            set { _parent = value; }
        }

        /// <summary>
        /// <see cref="Matrix"/> transform
        /// </summary>
// ReSharper disable ConvertToAutoProperty
        public Matrix Transform
// ReSharper restore ConvertToAutoProperty
        {
            get
            {
                return _transform;
            }
            set
            {
                _transform = value;
            }
        }

        ///<summary>
        /// Parent's bone index.
        ///</summary>
        public int ParentIndex
        {
            get { return _parentIndex; }
        }

        #endregion

        // Methods
        /// <summary>
        /// Constructor for <see cref="InstancedModelBone"/>, which sets the internal
        /// variables to the given attributes.
        /// </summary>
        /// <param name="name">Bone name</param>
        /// <param name="transform"><see cref="Matrix"/> transform</param>
        /// <param name="index">Bone index</param>
        /// <param name="parentIndex">Parent bone index</param>
        internal InstancedModelBone(string name, Matrix transform, int index, int parentIndex)
        {
            _name = name;
            _parentIndex = parentIndex; // 3/21/2011
            _transform = transform;
            _index = index;
        }

    }
}
