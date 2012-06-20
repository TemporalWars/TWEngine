#region File Description
//-----------------------------------------------------------------------------
// AbstractBehavior.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using TWEngine.ForceBehaviors.Enums;
using TWEngine.ForceBehaviors.Structs;
using TWEngine.SceneItems;

namespace TWEngine.ForceBehaviors
{
    /// <summary>
    /// Base abstract class to inherit from to create a steering behavior. 
    /// </summary>
    public abstract class AbstractBehavior
    {
        // Used to set behavior On or Off
        private bool _useBehavior = true;       
        
        /// <summary>
        /// References steering behavior manager.
        /// </summary>
        public ForceBehaviorsCalculator ForceBehaviorManager;

        //use these values to tweak the amount that each steering force
        //contributes to the Total steering force
        private float _behaviorWeight = 1.0f;

        #region Properties

        /// <summary>
        /// Use behavior.
        /// </summary>
        public bool UseBehavior
        {
            get { return _useBehavior; }
            set { _useBehavior = value; }
        }

        ///<summary>
        /// Influence level of the behavior. 
        ///</summary>
        public float BehaviorWeight
        {
            get { return _behaviorWeight; }
            set { _behaviorWeight = value; }
        }

        // 10/14/2009 - AutoProperty of Behaviors Enum type.
        /// <summary>
        /// Behaviors, where the value given is the sortOrder key.
        /// </summary>
        public BehaviorsEnum BehaviorType { get; set; }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="priority">Priority is the Enum <see cref="BehaviorsEnum"/> type.</param>
        /// <param name="weight">Importance assigned to a behavior.</param>
        protected AbstractBehavior(int priority, float weight)
        {    
            // 10/14/2009 - Priority IS the Enum 'ForceBehaviors' type.
            BehaviorType = (BehaviorsEnum)priority;

            _behaviorWeight = weight;
        }

        // 11/14/2008
        ///<summary>
        /// Release of unmanaged resources.
        ///</summary>
        public virtual void Dispose()
        {
            // null refs
            ForceBehaviorManager = null;
        }

        // 6/12/2010 - Updated 1st param from TimeSpan, to new lightwight BehaviorTimeSpan version.
        ///<summary>
        /// The <see cref="Update"/> method is used to calculate the proper steering force, for
        /// the given behavior.
        ///</summary>
        ///<param name="elapsedTime"><see cref="TimeSpan"/> as elapsed game time.</param>
        ///<param name="item"><see cref="SceneItemWithPick"/> instance</param>
        ///<param name="force">(OUT) calculated force as <see cref="Vector3"/></param>
        public abstract void Update(ref BehaviorsTimeSpan elapsedTime, SceneItem item, out Vector3 force);
        
    }
}
