#region File Description
//-----------------------------------------------------------------------------
// RTSCommGameTurn.cs
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
    /// The <see cref="RTSCommGameTurn"/> is used to send if the current network player is
    /// done with current game turn batch.  This is used by the <see cref="NetworkGameSyncer"/> class.
    ///</summary>
    public sealed class RTSCommGameTurn : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Clients game turn average.
        ///</summary>
        public float ClientCompletedGameTurnAvg; // 11/10/2009
        ///<summary>
        /// Frame time without the delay average.
        ///</summary>
        public float FrameTimeWithoutDelayAvg; // 1/2/2009        
        ///<summary>
        /// Clients processing batch time.
        ///</summary>
        public float ClientProcessingTime;
        ///<summary>
        /// Network latency.
        ///</summary>
        public float Latency;

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
        /// Constructor, which passes the <see cref="NetworkCommands.GameTurn"/> Enum to base.
        ///</summary>
        public RTSCommGameTurn()
            : base(NetworkCommands.GameTurn)
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
            
            // Pack Float
            PackFloat(ref ClientCompletedGameTurnAvg, ref packetWriter); 
            
            // Write Float           
            PackFloat(ref FrameTimeWithoutDelayAvg, ref packetWriter);           

            // Pack Float
            PackFloat(ref ClientProcessingTime, ref packetWriter);

            // Pack Float
            PackFloat(ref Latency, ref packetWriter); 

        }

        /// <summary>
        /// Read the network packet, from the given <see cref="PacketReader"/>, and saves the
        /// information into the class.
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        public override bool ReadNetworkPacket(ref PacketReader packetReader)
        {
            // 6/16/2010 - Try-Catch
            try
            {
                // 1/12/2009
                base.ReadNetworkPacket(ref packetReader);

                // UnPack Float            
                UnPackFloat(ref packetReader, out ClientCompletedGameTurnAvg);

                // UnPack Float            
                UnPackFloat(ref packetReader, out FrameTimeWithoutDelayAvg);

                // UnPack Float
                UnPackFloat(ref packetReader, out ClientProcessingTime);

                // UnPack Float
                UnPackFloat(ref packetReader, out Latency);
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
            //PoolManager.rtsCommGameTurnItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            ClientCompletedGameTurnAvg = 0;
            FrameTimeWithoutDelayAvg = 0;            
            ClientProcessingTime = 0;
            Latency = 0;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
