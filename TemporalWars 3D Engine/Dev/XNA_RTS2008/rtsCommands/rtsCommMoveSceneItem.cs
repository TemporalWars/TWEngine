#region File Description
//-----------------------------------------------------------------------------
// RTSCommMoveSceneItem.cs
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
using TWEngine.SceneItems.Enums;

namespace TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommMoveSceneItem"/> is used by some client player to request
    /// a <see cref="SceneItem"/> pathfinding order, which will be processed by the server.
    ///</summary>
    public sealed class RTSCommMoveSceneItem : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Current position of <see cref="SceneItem"/>
        ///</summary>
        public Vector3 Position = Vector3.Zero;
        ///<summary>
        /// Current velocity of <see cref="SceneItem"/>
        ///</summary>
        public Vector3 Velocity = Vector3.Zero;        
        ///<summary>
        /// New move-to-position for <see cref="SceneItem"/>
        ///</summary>
        public Vector3 MoveToPos = Vector3.Zero;        
        ///<summary>
        /// The smooth heading <see cref="Vector3"/> value
        ///</summary>
        public Vector3 SmoothHeading = Vector3.Zero; // 12/16/2008       
        ///<summary>
        /// Game time for this request.
        ///</summary>
        public float SendTime;
        ///<summary>
        /// The network item number of <see cref="SceneItem"/> to move
        ///</summary>
        public int NetworkItemNumber;
        ///<summary>
        /// The <see cref="ItemStates"/> of given <see cref="SceneItem"/>
        ///</summary>
        public ItemStates ItemState; // 1/19/2009
        ///<summary>
        /// When doing move request, should do an 'AttackMove' order?
        ///</summary>
        /// <remarks>AttackMove order is when a unit stops to attack an enemy unit, before reaching it goal.</remarks>
        public bool IsAttackMoveOrder; // 10/19/2009

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
        public RTSCommMoveSceneItem()
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

            // Pack Vector3 'Position'
            PackVector3(ref Position, ref packetWriter);

            // Pack Vector3 'Velocity'
            PackVector3(ref Velocity, ref packetWriter);           

            // Pack Vector3 'MoveToPos'
            PackVector3(ref MoveToPos, ref packetWriter);
            
            // Pack Vecotr3 'SmoothHeading'
            PackVector3(ref SmoothHeading, ref packetWriter);          
            
            // Pack Float 'SendTime'
            PackFloat(ref SendTime, ref packetWriter);

            // Write NetworkItemNumber           
            packetWriter.Write(NetworkItemNumber); 

            // Write ItemState
            packetWriter.Write((int)ItemState);

            // 10/19/2009 - Write 'Boolean' IsAttackMoveOrder
            packetWriter.Write(IsAttackMoveOrder);

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

                // UnPack Vector3 'Position'
                UnPackVector3(ref packetReader, out Position);

                // UnPack Vector3 'Velocity'
                UnPackVector3(ref packetReader, out Velocity);

                // UnPack Vector3 'MoveToPos'
                UnPackVector3(ref packetReader, out MoveToPos);

                // UnPack Vector3 'SmoothHeading'
                UnPackVector3(ref packetReader, out SmoothHeading);

                // UnPack Float 'SendTime'
                UnPackFloat(ref packetReader, out SendTime);

                // UnPack NetworkItemNumber           
                NetworkItemNumber = packetReader.ReadInt32();

                // UnPack ItemState
                ItemState = (ItemStates)packetReader.ReadInt32();

                // Unpack 'Boolean' IsAttackMoveOrder
                IsAttackMoveOrder = packetReader.ReadBoolean();
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
            //PoolManager.rtsCommMoveSceneItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            // Clear Position
            Position = Vector3.Zero;

            // Clear Velocity
            Velocity = Vector3.Zero;
            
            // Clear MoveToPos
            MoveToPos = Vector3.Zero;

            // Clear SmoothHeading
            SmoothHeading = Vector3.Zero;
           
            // Clear Sendtime
            SendTime = 0;

            // Clear NetworkItemNumber
            NetworkItemNumber = 0;

            // Clear IsAttackMoveOrder
            IsAttackMoveOrder = false;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
