#region File Description
//-----------------------------------------------------------------------------
// InstancedModelExtra.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels
{
    ///<summary>
    ///</summary>
    public class InstancedModelExtra
    {
        // 10/17/2008 - Offset-Rotation values
        internal float RotX;
        internal float RotY;
        internal float RotZ;

        // 1/1/2009 - Scale Value
        internal float Scale = 1;

        // 11/15/2008 - Store BakeTransforms Flag, to know if AbsoluteTransforms need to be calc during runtime.
        internal bool UseBakeTransforms;

        // 7/22/2009- Store IsStaticItem Flag; used for items which will never move in Game World.
        internal bool IsStaticItem;

        // 3/14/2011 - XNA 4.0
        internal bool HasSpawnBulletMarkers; // 2/10/2009 - SpawnBullet Marker Positions extracted
        internal bool IsFbxFormat; // 5/27/2009 - IsFBXFormat?
       
        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input"></param>
        public InstancedModelExtra(InstancedModelExtraLoader input)
        {
            RotX = input.RotX;
            RotY = input.RotY;
            RotZ = input.RotZ;
            Scale = input.Scale;
            UseBakeTransforms = input.UseBakeTransforms;
            IsStaticItem = input.IsStaticItem;
            HasSpawnBulletMarkers = input.HasSpawnBulletMarkers;
            IsFbxFormat = input.IsFbxFormat;
        }
    }
}
