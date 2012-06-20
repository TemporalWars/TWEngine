#region File Description
//-----------------------------------------------------------------------------
// RTSCommMoveSceneItem2.cs
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
    /// The <see cref="RTSCommMoveSceneItem2"/> is used to communicate move orders, from the
    /// server to client players.
    ///</summary>
    public sealed class RTSCommMoveSceneItem2 : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// New move-to-position for <see cref="SceneItem"/>
        ///</summary>
        public Vector2 MoveToPos = Vector2.Zero;              
        ///<summary>
        /// Game time for this request.
        ///</summary>
        public float SendTime;
        ///<summary>
        /// The network item number of <see cref="SceneItem"/> to move
        ///</summary>
        public int NetworkItemNumber;
        
        ///<summary>
        /// Solution Queue count remaining.
        ///</summary>
        [Obsolete]
        public int SolutionQueueCount;

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
        /// Constructor, which passes the <see cref="NetworkCommands.UnitMoveOrder"/> Enum to base.
        ///</summary>
        public RTSCommMoveSceneItem2()
            : base(NetworkCommands.UnitMoveOrder)
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

            // Pack Vector2 'MoveToPos'
            PackVector2(ref MoveToPos, ref packetWriter);                  
            
            // Pack Float 'SendTime'
            PackFloat(ref SendTime, ref packetWriter);

            // Write NetworkItemNumber           
            packetWriter.Write(NetworkItemNumber); 

            // Write SolutionQueue Count
            //packetWriter.Write(SolutionQueueCount);

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

                // UnPack Vector2 'MoveToPos'
                UnPackVector2(ref packetReader, out MoveToPos);

                // UnPack Float 'SendTime'
                UnPackFloat(ref packetReader, out SendTime);

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
            //PoolManager.rtsCommMoveScene2Items.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {            
            // Clear MoveToPos
            MoveToPos = Vector2.Zero;            
           
            // Clear Sendtime
            SendTime = 0;

            // Clear NetworkItemNumber
            NetworkItemNumber = 0;

            // Clear SolutionQueue Count
            //SolutionQueueCount = 0;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
