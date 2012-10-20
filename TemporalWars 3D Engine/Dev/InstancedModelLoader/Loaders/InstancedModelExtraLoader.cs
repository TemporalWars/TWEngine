#region File Description
//-----------------------------------------------------------------------------
// InstancedModelExtraLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework.Content;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModelLoader.Loaders
{
    /// <summary>
    /// The <see cref="InstancedModelExtraLoader"/> is used to abtract loading of the InstancedModel artwork, separate from the game engine.
    /// This allows changes to the main Game Engine's versions number and strong name, without breaking the loading of the arwork.
    /// </summary>
    public class InstancedModelExtraLoader
    {
         // 10/17/2008 - Offset-Rotation values
        public float RotX;
        public float RotY;
        public float RotZ;

        // 1/1/2009 - Scale Value
        public float Scale = 1;

        // 11/15/2008 - Store BakeTransforms Flag, to know if AbsoluteTransforms need to be calc during runtime.
        public bool UseBakeTransforms;

        // 7/22/2009- Store IsStaticItem Flag; used for items which will never move in Game World.
        public bool IsStaticItem;

        // 3/14/2011 - XNA 4.0
        public bool HasSpawnBulletMarkers; // 2/10/2009 - SpawnBullet Marker Positions extracted
        public bool IsFbxFormat; // 5/27/2009 - IsFBXFormat?

        ///<summary>
        /// Constructor
        ///</summary>
        ///<param name="input"></param>
        public InstancedModelExtraLoader(ContentReader input)
        {
            RotX = input.ReadSingle();
            RotY = input.ReadSingle();
            RotZ = input.ReadSingle();
            Scale = input.ReadSingle();
            UseBakeTransforms = input.ReadBoolean();
            IsStaticItem = input.ReadBoolean();
            HasSpawnBulletMarkers = input.ReadBoolean();
            IsFbxFormat = input.ReadBoolean();
        }
    }
}
