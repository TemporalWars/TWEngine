#region File Description
//-----------------------------------------------------------------------------
// InstanceDataParam.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012: Updated enum to inherit from short value.
    /// <summary>
    /// Used to allow updating some variable within the <see cref="ChangeRequestItem"/>. 
    /// </summary>
    public enum InstanceDataParam : short
    {
        ///<summary>
        /// <see cref="InstancedItem"/> is picked by user's mouse cursor.
        ///</summary>
        IsPicked,
        ///<summary>
        /// <see cref="InstancedItem"/> is within <see cref="Camera"/>'s frustum.
        ///</summary>
        InCameraView,
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
        ProceduralMaterialId,
        ///<summary>
        /// <see cref="InstancedItem"/> should be drawn with explosion pieces.
        ///</summary>
        DrawWithExplodePieces,
        ///<summary>
        /// <see cref="InstancedItem"/> unique key, given for each loaded <see cref="ItemType"/>.
        ///</summary>
        ItemInstanceKey,
        ///<summary>
        /// <see cref="InstancedModelPart"/> unique key.
        ///</summary>
        ModelPartIndexKey,
        ///<summary>
        /// <see cref="PlayerNumber"/> this item belongs to.
        ///</summary>
        PlayerNumber,
        ///<summary>
        /// This <see cref="InstancedItem"/> is of the <see cref="ScenaryItemScene"/> type.
        ///</summary>
        IsSceneryItem,
        ///<summary>
        /// Show this <see cref="InstancedItem"/> flashing white.
        ///</summary>
        ShowFlashWhite
    }
}