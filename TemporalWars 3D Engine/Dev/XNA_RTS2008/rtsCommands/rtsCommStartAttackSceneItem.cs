#region File Description
//-----------------------------------------------------------------------------
// RTSCommStartAttackSceneItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommStartAttackSceneItem"/> is used to send a start attack order
    /// to some network player.
    ///</summary>
    public sealed class RTSCommStartAttackSceneItem : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// The network number for the <see cref="SceneItem"/> doing the attack (attacker).
        ///</summary>
        public int SceneItemAttackerNetworkNumber;
        ///<summary>
        /// The network number for the <see cref="SceneItem"/> being attacked (attackie).
        ///</summary>
        public int SceneItemAttackieNetworkNumber;
        ///<summary>
        /// The player number for the <see cref="SceneItem"/> doing the attack (attacker).
        ///</summary>
        public int SceneItemAttackerPlayerNumber;
        ///<summary>
        /// The player number for the <see cref="SceneItem"/> being attacked (attackie).
        ///</summary>
        public int SceneItemAttackiePlayerNumber;
        ///<summary>
        /// The <see cref="AIOrderType"/> Enum issued.
        ///</summary>
        public AIOrderType AIOrderIssued = AIOrderType.None; // 6/3/2009

        // 5/13/2009
        // Ref to Memory PoolNode parent.
        ///<summary>
        /// Set or Get a reference to the <see cref="IPoolNodeItem.PoolNode"/> instance.
        ///</summary>
        public PoolNode PoolNode { get; set; }
        // Ref to PoolManager instance
        ///<summary>
        /// Set or Get a reference to the <see cref="IPoolNodeItem.PoolManager"/> instance.
        ///</summary>
        public PoolManagerBase PoolManager { get; set; }

        ///<summary>
        /// Set or Get if <see cref="IPoolNodeItem.PoolNode"/> is in use.
        ///</summary>
        public bool InUse { get; set; }

        ///<summary>
        /// Set or Set if this <see cref="IPoolNodeItem.PoolNode"/> instance reduces the <see cref="IFDTile"/> counter.
        ///</summary>
        public bool ReduceIFDCounter { get; set; }

        ///<summary>
        /// Constructor, which passes the <see cref="NetworkCommands.StartAttackSceneItem"/> Enum to base.
        ///</summary>
        public RTSCommStartAttackSceneItem()
            : base(NetworkCommands.StartAttackSceneItem)
        {
            
        }

        /// <summary>
        /// Creates the network packet, by writing into the given <see cref="PacketWriter"/>
        /// </summary>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        public override void CreateNetworkPacket(ref PacketWriter packetWriter)
        {
            // 1/12/2009
            base.CreateNetworkPacket(ref packetWriter);
            
            // Pack attacker's network number
            packetWriter.Write(SceneItemAttackerNetworkNumber);

            // Pack attackie's network number
            packetWriter.Write(SceneItemAttackieNetworkNumber);

            // Pack attacker's player number
            packetWriter.Write(SceneItemAttackerPlayerNumber);

            // Pack attackie's player number
            packetWriter.Write(SceneItemAttackiePlayerNumber);

            // Write 'AIOrderType' int.
            packetWriter.Write((int)AIOrderIssued);

        }

        /// <summary>
        /// Read the network packet, from the given <see cref="PacketReader"/>, and saves the
        /// information into the class.
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        /// <returns>True/False of result.</returns>
        public override bool ReadNetworkPacket(ref PacketReader packetReader)
        {
            // 6/16/2010 - Try-Catch
            try
            {
                // 1/12/2009
                base.ReadNetworkPacket(ref packetReader);

                // UnPack attacker's network number
                SceneItemAttackerNetworkNumber = packetReader.ReadInt32();

                // UnPack attackie's network number
                SceneItemAttackieNetworkNumber = packetReader.ReadInt32();

                // UnPack attacker's player number
                SceneItemAttackerPlayerNumber = packetReader.ReadInt32();

                // UnPack attackie's player number
                SceneItemAttackiePlayerNumber = packetReader.ReadInt32();

                // Read 'AIOrderType' int.
                AIOrderIssued = (AIOrderType)packetReader.ReadInt32();
            }
            catch (Exception)
            {
                RTSCommandValidator.SendRTSCommandValidator(RTSCommandNumber, NetworkCommand);
                return false;
            }
            
            return true;
        }

        // 5/13/2009
        /// <summary>
        /// Returns this instance back into the <see cref="ImageNexus.BenScharbach.TWEngine.MemoryPool.PoolManager"/>, setting 'Active' to false.
        /// </summary>
        public override void ReturnItemToPool()
        {
            // Return this instance to the PoolManager 
            //PoolManager.rtsCommStartAttackSceneItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            // Clear
            SceneItemAttackerNetworkNumber = 0;
            SceneItemAttackieNetworkNumber = 0;
            SceneItemAttackerPlayerNumber = 0;
            SceneItemAttackiePlayerNumber = 0;
            AIOrderIssued = AIOrderType.None;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
