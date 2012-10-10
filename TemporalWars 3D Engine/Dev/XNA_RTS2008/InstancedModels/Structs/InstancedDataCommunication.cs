#region File Description
//-----------------------------------------------------------------------------
// InstancedDataCommunication.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using Microsoft.Xna.Framework;

#if !XBOX360
using System.Windows.Forms;
#endif

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs
{
    // 4/15/2010: Renamed from 'InstancedData' to now be 'InstancedDataCommunication'.
    // 9/12/2008
    ///<summary>
    /// The <see cref="InstancedDataCommunication"/> structure is used to communicate updates, between
    /// the <see cref="InstancedItem"/> class, and the <see cref="InstancedModel"/> instances.  The updates
    /// are processed via the <see cref="InstancedModelChangeRequests"/> system each draw cycle; required
    /// since double-buffering is used.
    ///</summary>
    public struct InstancedDataCommunication
    {
        ///<summary>
        /// Reference to the owner's <see cref="ShapeItem"/>.
        ///</summary>
        public Shape ShapeItem; // 8/27/2009 - Ref to the Owner's shapeItem.
        ///<summary>
        /// <see cref="InstancedItem"/> is picked by user's mouse cursor.
        ///</summary>
        public bool IsPicked;
        ///<summary>
        /// <see cref="InstancedItem"/> is within <see cref="Camera"/>'s frustum.
        ///</summary>
        public bool InCameraView;
        ///<summary>
        /// Enter Material Id to use;
        ///  0 = Blinn Lighting
        ///  1 = Metal Lighting
        ///  2 = Plastic Lighting
        ///  3 = Glossy Lighting
        ///  4 = Phong Lighting
        ///  5 = PhongRed Lighting
        ///  6 = Flash-White
        ///  7 = Color Blend
        ///  8 = Fresnel Blend
        ///  9 = Saturation
        /// 10 = Custom Metal
        /// 11 = Reflective Metal
        /// 12 = Velvety
        /// 13 = Wood
        ///</summary>
        public int ProceduralMaterialId; // 2/3/2010
        ///<summary>
        /// <see cref="InstancedItem"/> should be drawn with explosion pieces.
        ///</summary>
        public bool DrawWithExplodePieces; // 3/28/2009 - Draws using the Explosion 'Pieces'!
        ///<summary>
        /// <see cref="InstancedItem"/> unique key, given for each loaded <see cref="ItemType"/>.
        ///</summary>
        public int ItemInstanceKey;
// ReSharper disable InconsistentNaming
        /// <summary>
        /// <see cref="PlayerNumber"/> this item belongs to.
        /// </summary>
        /// <remarks>Allows checking via the UpdateInstanceTransforms method, since need to bypass the Property 'Get' in this case.</remarks>
        internal int _modelPartIndexKey; // 2/3/2010 - Allows checking via the UpdateInstanceTransforms method, since need to bypass the Property 'Get' in this case.
// ReSharper restore InconsistentNaming
        ///<summary>
        /// <see cref="PlayerNumber"/> this item belongs to.
        ///</summary>
        public int PlayerNumber; // 11/19/2008 (MP)
        ///<summary>
        /// This <see cref="InstancedItem"/> is of the <see cref="ScenaryItemScene"/> type.
        ///</summary>
        public bool IsSceneryItem; // 7/10/2009
        ///<summary>
        /// Show this <see cref="InstancedItem"/> flashing white.
        ///</summary>
        public bool ShowFlashWhite; // 10/12/2009 (Scripting purposes)

        // 2/3/2010 - Note: Internally, this property adjusts the index by a factor of 1+ when set.
        //                  This allows value zero, to be checked and used as the 'All' model parts factor,
        //                  even though a value zero 'ModelPartIndex' could be entered!
        /// <summary>
        /// Used to apply changes to a specific modelPart!
        /// </summary>
        public int ModelPartIndexKey
        {
            get
            {
                return _modelPartIndexKey - 1;
            }
            set
            {
                _modelPartIndexKey = value + 1;
            }
        }

        // 8/27/2009 - This is needed, since scenery items have batches of 'Transforms' in one 'SceneItem' instance!  
        //             Therefore, if you try to retrieve the 'World' matrix from the 'ShapeItem', it will not reflect
        //             all the differente instances the one 'SceneryItemScene' class can have.  Currently, the 'World'
        //             instances are stored in the 'SceneryItemScene' classes 'ScenaryItems' LIST!
        ///<summary>
        /// Matrix transform, specifically just for the <see cref="ScenaryItemScene"/> type.
        ///</summary>
        /// <remarks>
        /// This is needed, since <see cref="ScenaryItemScene"/> items have batches of 'Transforms' in one 'SceneItem' instance!  
        /// Therefore, if you try to retrieve the 'World' matrix from the <see cref="ShapeItem"/>, it will not reflect
        /// all the different instances the one <see cref="ScenaryItemScene"/> class can have.  Currently, the 'World'
        /// instances are stored in the <see cref="ScenaryItemScene"/> classes 'ScenaryItems' LIST!
        /// </remarks>
        public Matrix SceneryTransform; 

        ///<summary>
        /// <see cref="InstancedItem"/> Matrix tranform, which already includes
        /// the contatention of the <see cref="Orientation"/> matrix.
        ///</summary>
        /// <remarks>If no transform exist, then the Identity matrix is returned.</remarks>
        public Matrix Transform
        {
            get
            {
                if (ShapeItem != null)
                {
                    if (IsSceneryItem)
                    {
                        return SceneryTransform;
                    }

                    Matrix tmpValue;
                    var tmpOrientation = ShapeItem.Orientation;
                    var tmpWorld = ShapeItem.WorldP;
                    Matrix.Multiply(ref tmpOrientation, ref tmpWorld, out tmpValue);
                    return tmpValue;
                }

                return Matrix.Identity;
            }
        }

       
    }
}


