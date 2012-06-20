#region File Description
//-----------------------------------------------------------------------------
// DefenseAIStance.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.SceneItems.Enums
{
    // 6/1/2009 - Defense AI Stances
    ///<summary>
    /// The <see cref="SceneItem"/> defensive AI stance.
    ///</summary>
    public enum DefenseAIStance
    {
        ///<summary>
        /// In aggressive stance, the <see cref="SceneItem"/> will continuously to pursue
        /// an enemy player until the enemy is dead, or itself.
        ///</summary>
        Aggressive,
        ///<summary>
        /// In guard stance, the <see cref="SceneItem"/> will attack an enemy unit only
        /// when the enemy is within the attacker's view range.  Attacker will pursue
        /// unit, if within its view range, but not outside of range.
        ///</summary>
        Guard,
        ///<summary>
        /// In hold-ground stance, the <see cref="SceneItem"/> will attack an enemy unit
        /// only when the enemy is within the attacker's attack range, but not the view range.
        /// Will not pursue any enemy unit, but instead will hold its current position.
        ///</summary>
        HoldGround,
        ///<summary>
        /// In hold-fire stance, the <see cref="SceneItem"/> will not attack!  An enemy unit
        /// could approach this item, both within its view and attack range, and no action will occur.
        ///</summary>
        HoldFire
    }
}
