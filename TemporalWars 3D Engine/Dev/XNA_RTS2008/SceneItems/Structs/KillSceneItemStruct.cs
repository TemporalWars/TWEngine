#region File Description
//-----------------------------------------------------------------------------
// KillSceneItemStruct.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Structs
{
    ///<summary>
    /// The <see cref="KillSceneItemStruct"/> structure stores the current <see cref="SceneItem"/>
    /// to kill, and the attacker's player number.
    ///</summary>
    public struct KillSceneItemStruct
    {
        ///<summary>
        /// The <see cref="SceneItem"/> to kill.
        ///</summary>
        public SceneItem ItemToKill;
        ///<summary>
        /// The <see cref="SceneItem"/> attacker's player number.
        ///</summary>
        public int AttackerPlayerNumber;
    }
}