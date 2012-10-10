#region File Description
//-----------------------------------------------------------------------------
// ExplosionItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Explosions.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Explosions"/> namespace contains the common classes
    /// which make up the entire <see cref="ExplosionsManager"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    ///<summary>
    /// <see cref="ExplosionItem"/> class, which holds the data to
    /// animating a single <see cref="SceneItem"/> bone during an explosion.
    ///</summary>
    public struct ExplosionItem
    {
        internal bool Delete;
        internal bool StartDeletionCountdown; // 1/30/2010
        internal int StartExplosion; // 6/14/2010
        

        ///<summary>
        /// Reference to the <see cref="InstancedItemData"/> structure.
        ///</summary>
        public InstancedItemData InstancedItemData;
        ///<summary>
        /// Bone name, used to get absolute bone transform.
        ///</summary>
        public readonly string ItemPieceBoneName;
        ///<summary>
        /// Bone's Transform matrix
        ///</summary>
        public Matrix ItemPieceBoneTransform;
        ///<summary>
        /// Bone's current position, updated during explosion animation
        ///</summary>
        public Vector3 ItemPieceNewPosition;
        ///<summary>
        /// Bone's current velocity, updated during explosion animation
        ///</summary>
        public Vector3 ItemPieceVelocity;
        ///<summary>
        /// Concatentation of the Orientation and World matricies.
        ///</summary>
        public Matrix OrientationXWorld;
        ///<summary>
        /// Bone's rotation angle, set at initial creation.
        ///</summary>
        public Vector3 RotAngle; // z-axis       
        ///<summary>
        /// Bone's rotation speed, set a initial creation.
        ///</summary>
        public float RotSpeed;
        ///<summary>
        /// Bone's current angle, updated during explosion animation, using
        /// the <see cref="GameTime"/> and <see cref="RotSpeed"/> to calculate.
        ///</summary>
        public float RotValue;
        internal readonly Stopwatch StopWatch; // 6/8/2009
        internal TimeSpan StopWatchLimit; // 6/8/2009
        internal bool StopWatchStarted; // 6/8/2009
        ///<summary>
        /// Instance of <see cref="SceneItemWithPick"/> this ExplosionItem belongs to.
        ///</summary>
        public SceneItemWithPick SceneItemOwner; // 1/30/2010

        private static readonly Vector3 Vector3Zero = Vector3.Zero;
        private static readonly Matrix MatrixIdentity = Matrix.Identity;
        private static readonly Vector3 Vector3Backward = Vector3.Backward;

        // constructor
        ///<summary>
        /// Sets up the ExplosionItem structure, using the given
        /// attributes.
        ///</summary>
        ///<param name="sceneItemOwner"><see cref="SceneItemWithPick"/> instance owner.</param>
        ///<param name="boneName">Bone name, used to get absolute bone transform</param>
        ///<param name="velocity">Initial velocity of item</param>
        ///<param name="orientation"><see cref="SceneItemWithPick"/> orientation matrix</param>
        ///<param name="world"><see cref="SceneItemWithPick"/> world matrix</param>
        ///<param name="itemData"><see cref="SceneItemWithPick"/> InstancedItemData structure</param>
        public ExplosionItem(SceneItemWithPick sceneItemOwner, string boneName, ref Vector3 velocity, ref Matrix orientation, ref Matrix world, ref InstancedItemData itemData)
        {
            // 2/23/2009 - Set all internals to zero; required with memory pools!
            ItemPieceNewPosition = Vector3Zero;
            ItemPieceVelocity = Vector3Zero;
            ItemPieceBoneTransform = MatrixIdentity;
            OrientationXWorld = MatrixIdentity;
            RotValue = 0; // 4/3/2009
            RotAngle = Vector3Backward; // z-axis            
            RotSpeed = 5; // 4/3/2009
            StopWatch = new Stopwatch(); // 6/8/2009
            StopWatchStarted = false; // 6/8/2009
            StopWatchLimit = TimeSpan.FromSeconds(30);
            SceneItemOwner = sceneItemOwner; // 1/30/2010 - Set to Owner, so ExplosionManager can call the FinishKillSceneItem method!
              
            // 5/22/2009 - if 'FBXImport' for InstanceItem, then flip the Z/Y channels.
            RotAngle = InstancedItem.IsFBXImport(ref itemData) ? Vector3.Up : Vector3Backward;

            // Calc Combination of Orientation * World matricies.
            Matrix.Multiply(ref orientation, ref world, out OrientationXWorld);

            // Set atts
            ItemPieceBoneName = boneName;
            ItemPieceVelocity = velocity;
            InstancedItemData = itemData;
            Delete = false;
            StartDeletionCountdown = false; // 1/30/2010
            StartExplosion = 0; // 6/14/2010

            // 2/17/2009: Updated to use the 'InstancedITemTransform.
            // Get Bone Absolute Transform    
            InstancedItemTransform instancedItemTransform;
            InstancedItem.GetInstanceItemCombineAbsoluteBoneTransform(ref itemData, boneName, out instancedItemTransform);

            ItemPieceBoneTransform = instancedItemTransform.AbsoluteTransform;
        }
       
    }
}