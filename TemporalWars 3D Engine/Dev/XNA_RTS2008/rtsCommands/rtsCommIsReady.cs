#region File Description
//-----------------------------------------------------------------------------
// RTSCommIsReady.cs
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

namespace TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommIsReady"/> is used to notify the other network players if
    /// they are ready to start the current game session.
    ///</summary>
    public sealed class RTSCommIsReady : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Is player ready to start?
        ///</summary>
        public bool IsReadyToStart;

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
        /// Constructor, which passes the <see cref="NetworkCommands.IsReady"/> Enum to base.
        ///</summary>
        public RTSCommIsReady()
            : base(NetworkCommands.IsReady)
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
            
            // Pack boolean
            packetWriter.Write(IsReadyToStart);           
          

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

                // UnPack boolean
                IsReadyToStart = packetReader.ReadBoolean();
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
            //PoolManager.rtsCommIsReadyItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            IsReadyToStart = false;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
