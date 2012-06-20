#region File Description
//-----------------------------------------------------------------------------
// RTSCommLobbyData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics;
using TWEngine.IFDTiles;
using TWEngine.MemoryPool;
using TWEngine.MemoryPool.Interfaces;
using TWEngine.rtsCommands.Enums;

namespace TWEngine.rtsCommands
{
    ///<summary>
    /// The <see cref="RTSCommLobbyData"/> is used to send the network players lobby
    /// choices to the other network players.
    ///</summary>
    public sealed class RTSCommLobbyData : RTSCommand, IPoolNodeItem
    {
        ///<summary>
        /// The player's map name choice.
        ///</summary>
        public string MapName = "Empty";
        ///<summary>
        /// The player's side choice.
        ///</summary>
        public int PlayerSide = 1; // 4/7/2009
        ///<summary>
        /// The player's color choice.
        ///</summary>
        public Color PlayerColor = Color.White; // 4/7/2009
        ///<summary>
        /// Unique player's gamer ID.
        ///</summary>
        public byte GamerID; // 4/7/2009
        ///<summary>
        /// The player's location choice.
        ///</summary>
        public int PlayerLocation = 1; // 4/8/2009

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
        /// Constructor, which passes the <see cref="NetworkCommands.LobbyData"/> Enum to base.
        ///</summary>
        public RTSCommLobbyData() 
            : base (NetworkCommands.LobbyData)
        {
            
        }

        /// <summary>
        /// Creates the network packet, by writing into the given <see cref="PacketWriter"/>
        /// </summary>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        public override void CreateNetworkPacket(ref PacketWriter packetWriter)
        {
            // 2/21/2009
            base.CreateNetworkPacket(ref packetWriter);            
            
            // 9/1/2009 - Skip writing values, if 'UserNotReady' enum.
            if (NetworkCommand == NetworkCommands.LobbyData_UserNotReady)
                return;

            // Pack string
            packetWriter.Write(MapName);  
            // Pack int
            packetWriter.Write(PlayerSide);
            // Pack Color
            packetWriter.Write(PlayerColor);
            // Pack byte
            packetWriter.Write(GamerID);
            // Pack int
            packetWriter.Write(PlayerLocation);
          

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
                // 2/21/2009
                base.ReadNetworkPacket(ref packetReader);

                // 9/1/2009 - Skip writing values, if 'UserNotReady' enum.
                if (NetworkCommand == NetworkCommands.LobbyData_UserNotReady)
                    return true;

                // UnPack string
                MapName = packetReader.ReadString();
                // UnPack int
                PlayerSide = packetReader.ReadInt32();
                // UnPack Color
                PlayerColor = packetReader.ReadColor();
                // UnPack byte
                GamerID = packetReader.ReadByte();
                // UnPack int
                PlayerLocation = packetReader.ReadInt32();
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
            //PoolManager.rtsCommLobbyDataItems.Return(PoolNode);
            if (PoolNode != null) PoolNode.ReturnToPool();
        }

        /// <summary>
        /// Clears out all data in class, for re-use again.
        /// </summary>
        public override void Clear()
        {
            MapName = string.Empty;
            PlayerSide = 1;
            PlayerColor = Color.White;
            GamerID = 0;
            PlayerLocation = 1;

            // 5/13/2009 - Call Base
            base.Clear();
        }
    }
}
