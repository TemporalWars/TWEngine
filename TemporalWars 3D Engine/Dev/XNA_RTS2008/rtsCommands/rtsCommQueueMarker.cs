#region File Description
//-----------------------------------------------------------------------------
// RTSCommQueueMarker.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using TWEngine.IFDTiles;
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;

namespace TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommQueueMarker"/> is used to send the current <see cref="BuildingScene"/> output queue marker, set
    /// by the current player, to the other players.
    ///</summary>
    public sealed class RTSCommQueueMarker : RTSCommand, IPoolNodeItem
    {        
        ///<summary>
        /// New <see cref="Vector3"/> output queue position.
        ///</summary>
        public Vector3 NewQueuePosition = Vector3.Zero;
        ///<summary>
        /// The <see cref="BuildingScene"/> network item number this belongs to.
        ///</summary>
        public int NetworkItemNumber;

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
        /// Constructor, which passes the <see cref="NetworkCommands.QueueMarker"/> Enum to base.
        ///</summary>
        public RTSCommQueueMarker()
            : base(NetworkCommands.QueueMarker)
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
            
            // Pack Vector3 'NewQueuePosition'
            PackVector3(ref NewQueuePosition, ref packetWriter);
           
            // Write NetworkItemNumber           
            packetWriter.Write(NetworkItemNumber);     
          

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

                // UnPack Vector3
                UnPackVector3(ref packetReader, out NewQueuePosition);

                // UnPack NetworkItemNumber            
                NetworkItemNumber = packetReader.ReadInt32();
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
            //PoolManager.rtsCommQueueMarkerItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            // Clear Position
            NewQueuePosition.X = 0;
            NewQueuePosition.Y = 0;
            NewQueuePosition.Z = 0;

            // Clear Network Number
            NetworkItemNumber = 0;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}