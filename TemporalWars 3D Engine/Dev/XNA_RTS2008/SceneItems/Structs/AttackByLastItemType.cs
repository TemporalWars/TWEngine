#region File Description
//-----------------------------------------------------------------------------
// AttackByLastItemType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Structs
{
    // 7/24/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.SceneItems.Structs"/> namespace contains the structures
    /// which make up the entire <see cref="SceneItems.Structs"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 10/10/2009 - AttackByLastItemType Struct; used for the AttackBy (Scripting purposes)
    ///<summary>
    /// The <see cref="AttackByLastItemType"/> structure is used for the <see cref="SceneItemWithPick.AttackBy"/> method
    /// call. (Scripting purposes)
    ///</summary>
    public struct AttackByLastItemType
    {
        ///<summary>
        /// The <see cref="ItemType"/> Enum of attacker
        ///</summary>
        public ItemType AttackedByItemType;
        ///<summary>
        /// The <see cref="GameTime"/> attack occured
        ///</summary>
        public GameTime TimeOfAttack;
    }
}