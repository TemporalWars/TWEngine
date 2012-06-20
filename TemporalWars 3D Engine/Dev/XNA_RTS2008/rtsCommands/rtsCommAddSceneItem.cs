#region File Description
//-----------------------------------------------------------------------------
// RTSCommAddSceneItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels.Enums;
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;

namespace TWEngine.rtsCommands
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.rtsCommands"/> namespace contains the classes
    /// which make up the entire <see cref="rtsCommands"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    ///<summary>
    /// The <see cref="RTSCommAddSceneItem"/> is used to send a <see cref="SceneItem"/> 
    /// add request to other network player.
    ///</summary>
    public sealed class RTSCommAddSceneItem : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// Position to place <see cref="SceneItem"/>.
        ///</summary>
        public Vector3 AtPosition = Vector3.Zero;
        ///<summary>
        /// Network item number for given <see cref="SceneItem"/>.
        ///</summary>
        public int NetworkItemNumber;
        ///<summary>
        /// <see cref="ItemType"/> Enum of type to create
        ///</summary>
        public ItemType ItemType;
        ///<summary>
        /// <see cref="ItemGroupType"/> Enum group this <see cref="SceneItem"/> can attack. (Optional)
        ///</summary>
        public ItemGroupType? ItemGroupToAttack; // 12/30/2008
        ///<summary>
        /// <see cref="ItemGroupType"/> Enum group this <see cref="SceneItem"/> belongs to.
        ///</summary>
        public ItemGroupType BuildingType;
        ///<summary>
        /// <see cref="ItemGroupType"/> Enum group this <see cref="SceneItem"/> can produce. (Optional)
        ///</summary>
        public ItemGroupType? ProductionType;
        ///<summary>
        /// This <see cref="SceneItem"/> parent <see cref="BuildingScene"/> network item number.
        ///</summary>
        public int BuildingNetworkItemNumber;
        ///<summary>
        /// Is this <see cref="SceneItem"/> a bot helper for some other <see cref="SceneItem"/>?
        ///</summary>
        public bool IsBotHelper; // 8/4/2009
        ///<summary>
        /// The Bot's parent <see cref="SceneItem"/> network item number.
        ///</summary>
        /// <remarks>This attribute requires the <see cref="IsBotHelper"/> to be set to true.</remarks>
        public int LeaderNetworkItemNumber; // 8/4/2009 - The Bot's leader.
        ///<summary>
        /// The Bot's parent player number.
        ///</summary>
        ///<remarks>This attribute requires the <see cref="IsBotHelper"/> to be set to true.</remarks>
        public int LeaderPlayerNumber; // 8/5/2009 - The Bot's Leader PlayerNumber.

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
        /// Constructor, which passes the <see cref="NetworkCommands.AddSceneItem"/> Enum to base.
        ///</summary>
        public RTSCommAddSceneItem()
            : base(NetworkCommands.AddSceneItem)
        {
            
        }

        /// <summary>
        /// Creates the network packet, by writing into the given <see cref="PacketWriter"/>.
        /// </summary>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        public override void CreateNetworkPacket(ref PacketWriter packetWriter)
        {
            // 1/12/2009
            base.CreateNetworkPacket(ref packetWriter);
            
            // Pack Vector3 'AtPosition'
            PackVector3(ref AtPosition, ref packetWriter);
           
            // Write NetworkItemNumber            
            packetWriter.Write(NetworkItemNumber); 
          
            // 11/5/2008 - Write ItemType & ProductionType
            packetWriter.Write((int)ItemType);
            packetWriter.Write((int)BuildingType);

            // 12/30/2008 - Write ItemGroupType; if null, then (-1) value.
            if (ItemGroupToAttack != null)
                packetWriter.Write((int)ItemGroupToAttack);
            else
                packetWriter.Write(-1);

            // 11/11/2008 - Write Production Type; if null, then (-1) value.
            if (ProductionType != null)
                packetWriter.Write((int)ProductionType);
            else
                packetWriter.Write(-1);

            // 11/11/2008 - Write BuildingNetworkItemNumber
            packetWriter.Write(BuildingNetworkItemNumber);

            // 8/4/2009 - Write IsBotHelper
            packetWriter.Write(IsBotHelper);

            // 8/4/2009 - Write LeaderNetworkItemNumber
            packetWriter.Write(LeaderNetworkItemNumber);

            // 8/5/2009 - Write Leader PlayerNumber
            packetWriter.Write(LeaderPlayerNumber);

        }

        /// <summary>
        /// Read the network packet, from the given <see cref="PacketReader"/>, and saves the
        /// information into the class.
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        /// <returns>True/False of success.</returns>
        public override bool ReadNetworkPacket(ref PacketReader packetReader)
        {   
            // 6/16/2010 - Try-Catch
            try
            {
                // 1/12/2009
                base.ReadNetworkPacket(ref packetReader);

                // UnPack Vector3
                UnPackVector3(ref packetReader, out AtPosition);

                // NetworkItemNumber            
                NetworkItemNumber = packetReader.ReadInt32();

                // 11/5/2008 - UnPack ItemType & ProductionType
                ItemType = (ItemType)packetReader.ReadInt32();
                BuildingType = (ItemGroupType)packetReader.ReadInt32();

                // 12/30/2008 - UnPack ItemGroupType; if (-1), then set to Null
                var tmpAttackType = packetReader.ReadInt32();
                if (tmpAttackType == -1)
                    ItemGroupToAttack = null;
                else
                    ItemGroupToAttack = (ItemGroupType?)tmpAttackType;

                // 11/11/2008 - UnPack Production Type; if (-1), then set to Null
                var tmpProdType = packetReader.ReadInt32();
                if (tmpProdType == -1)
                    ProductionType = null;
                else
                    ProductionType = (ItemGroupType?)tmpProdType;

                // 11/11/2008 - Unpack BuildingNetworkItemNumber
                BuildingNetworkItemNumber = packetReader.ReadInt32();

                // 8/4/2009 - Read IsBotHelper
                IsBotHelper = packetReader.ReadBoolean();

                // 8/4/2009 - Read LeaderNetworkItemNumber
                LeaderNetworkItemNumber = packetReader.ReadInt32();

                // 8/5/2009 - Read Leader's PlayerNumber
                LeaderPlayerNumber = packetReader.ReadInt32();
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
        /// Return this instance to the PoolManager.
        /// </summary>
        public override void ReturnItemToPool()
        {
            // Return this instance to the PoolManager 
            //PoolManager.rtsCommAddSceneItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        // 9/9/2008
        /// <summary>
        /// Clears all internal fields to thier default values.
        /// </summary>
        public override void Clear()
        {
            // Clear Position
            AtPosition.X = 0;
            AtPosition.Y = 0;
            AtPosition.Z = 0;

            // Clear Network Number
            NetworkItemNumber = 0;

            // 11/5/2008 - Clear ItemType & ProductionType
            ItemType = 0;
            BuildingType = 0;
            BuildingNetworkItemNumber = 0;
            // 12/30/2008
            ItemGroupToAttack = 0;

            BuildingNetworkItemNumber = 0;
            IsBotHelper = false;
            LeaderNetworkItemNumber = 0;
            LeaderPlayerNumber = 0;

            // 5/13/2009 - Call Base
            base.Clear();

        }
    }
}
