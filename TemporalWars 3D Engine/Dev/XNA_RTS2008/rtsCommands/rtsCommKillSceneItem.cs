#region File Description
//-----------------------------------------------------------------------------
// RTSCommKillSceneItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework.Net;
using TWEngine.IFDTiles;
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;

namespace TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommKillSceneItem"/> is used to send a kill command for some
    /// <see cref="SceneItem"/>.
    ///</summary>
    public sealed class RTSCommKillSceneItem : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Network item number of <see cref="SceneItem"/> to kill.
        ///</summary>
        public int NetworkItemNumber;

        ///<summary>
        /// Attacker requesting the kill command; used to update the player
        /// stats correctly.
        ///</summary>
        public int AttackerPlayerNumber; // 10/3/2009 - Req to update stats correctly.

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
        public PoolManager PoolManager { get; set; }

        ///<summary>
        /// Set or Get if <see cref="IPoolNodeItem.PoolNode"/> is in use.
        ///</summary>
        public bool InUse { get; set; }

        ///<summary>
        /// Set or Set if this <see cref="IPoolNodeItem.PoolNode"/> instance reduces the <see cref="IFDTile"/> counter.
        ///</summary>
        public bool ReduceIFDCounter { get; set; }

        ///<summary>
        /// Constructor, which passes the <see cref="NetworkCommands.KillSceneItem"/> Enum to base.
        ///</summary>
        public RTSCommKillSceneItem()
            : base(NetworkCommands.KillSceneItem)
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
            packetWriter.Write(NetworkItemNumber);   
    
            // 10/3/2009 - Pack Attacker's PlayerNumber
            packetWriter.Write(AttackerPlayerNumber);

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
                NetworkItemNumber = packetReader.ReadInt32();

                // 10/3/2009 - UnPack attacker's PlayerNumber
                AttackerPlayerNumber = packetReader.ReadInt32();
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
        /// Returns this instance back into the <see cref="MemoryPool.PoolManager"/>, setting 'Active' to false.
        /// </summary>
        public override void ReturnItemToPool()
        {
            // Return this instance to the PoolManager 
            //PoolManager.rtsCommKillSceneItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            // Clear
            NetworkItemNumber = 0;
            AttackerPlayerNumber = 0;

            // 5/13/2009 - Call Base
            base.Clear();           
        }
    }
}
