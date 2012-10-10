#region File Description
//-----------------------------------------------------------------------------
// RTSCommSceneItemHealth.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommSceneItemHealth"/> is used to communicate the current
    /// health status for a given <see cref="SceneItem"/>.
    ///</summary>
    public sealed class RTSCommSceneItemHealth : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Current health value for <see cref="SceneItem"/>
        ///</summary>
        public float Health;
        ///<summary>
        /// Should start self repair option?
        ///</summary>
        public bool StartSelfRepair;
        ///<summary>
        /// Network item number of <see cref="SceneItem"/> to update.
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
        /// Constructor, which passes the <see cref="NetworkCommands.SceneItemHealth"/> Enum to base.
        ///</summary>
        public RTSCommSceneItemHealth()
            : base(NetworkCommands.SceneItemHealth)
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

            // Write Health float
            packetWriter.Write(Health);

            // Write StartSelfRepair boolean
            packetWriter.Write(StartSelfRepair);

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

                // Read Health float
                Health = packetReader.ReadSingle();

                // Read StartSelfRepair boolean
                StartSelfRepair = packetReader.ReadBoolean();

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
        /// Returns this instance back into the <see cref="ImageNexus.BenScharbach.TWEngine.MemoryPool.PoolManager"/>, setting 'Active' to false.
        /// </summary>
        public override void ReturnItemToPool()
        {
            // Return this instance to the PoolManager 
            //PoolManager.rtsCommSceneItemStanceItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            // reset
            Health = 0;

            // clear startSelfRepair
            StartSelfRepair = false;

            // Clear NetworkItemNumber
            NetworkItemNumber = 0;

            // 5/13/2009 - Call Base
            base.Clear();
        }

       
    }
}
