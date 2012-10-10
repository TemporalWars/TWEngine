#region File Description
//-----------------------------------------------------------------------------
// RTSCommDelayTime.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommDelayTime"/> is used to send the delay time between each communication turn, calculated
    /// by the <see cref="NetworkGameSyncer"/> class.
    ///</summary>
    public sealed class RTSCommDelayTime : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Delay time to use
        ///</summary>
        public int DelayTime;
        ///<summary>
        /// Communication time to use.
        ///</summary>
        public float CommunicationTime;

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
        /// Constructor, which passes the <see cref="NetworkCommands.CeaseAttackSceneItem"/> Enum to base.
        ///</summary>
        public RTSCommDelayTime()
            : base(NetworkCommands.DelayTime)
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
            
            // Pack Int
            packetWriter.Write(DelayTime); 
          
            // Pack float
            PackFloat(ref CommunicationTime, ref packetWriter);          

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

                // UnPack Int
                DelayTime = packetReader.ReadInt32();

                // UnPack Float
                UnPackFloat(ref packetReader, out CommunicationTime);
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
            //PoolManager.rtsCommDelayTimeItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            DelayTime = 0;
            CommunicationTime = 0;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
