#region File Description
//-----------------------------------------------------------------------------
// RTSCommand.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace ImageNexus.BenScharbach.TWEngine.rtsCommands
{

    ///<summary>
    /// The <see cref="RTSCommand"/> abstract base class is the foundation for
    /// all RTS commands, which stores the fundamental pieces of information, like the
    /// <see cref="Player"/> number and communication turn number. It also provides compression
    /// methods, used to compress the <see cref="Vector3"/>, <see cref="Vector2"/>, and <see cref="float"/> value 
    /// types.
    ///</summary>
    public abstract class RTSCommand
    {        
        // 9/3/2008 - 
        ///<summary>
        /// Tracks the current <see cref="NetworkCommands"/> for the server.
        ///</summary>
        public NetworkCommands NetworkCommand = NetworkCommands.None;

        // 9/7/2008; 6/16/2010: Updated from 'int' to 'byte' type. 
        private byte _playerNumber; // 4/27/2010
        ///<summary>
        /// The Network <see cref="Player"/> number.
        ///</summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <see cref="PlayerNumber"/> is outside the allowable range of 0 - 
        /// <see cref="TemporalWars3DEngine._maxAllowablePlayers"/>.</exception>
        public byte PlayerNumber
        {
            get
            {
                return _playerNumber;
            }
            set
            {
                // 4/27/2010 - Check if valid player number
                if (_playerNumber < 0 || _playerNumber >= TemporalWars3DEngine._maxAllowablePlayers)
                    throw new ArgumentOutOfRangeException("value",@"The PlayerNumber value is outside the allowable rangeof 0 - MaxAllowablePlayers.");

                _playerNumber = value;
            }
        }  
  
        // 1/12/2009 - Communication Turn Number
        ///<summary>
        /// Current communication turn number, updated via the <see cref="NetworkGameSyncer"/> class.
        ///</summary>
        public int CommunicationTurnNumber;

        // 6/16/2010
        /// <summary>
        /// Unique number for each <see cref="RTSCommand"/> Enum when created, for use with the <see cref="RTSCommandValidator"/>.
        /// </summary>
        /// <remarks>Stored as <see cref="uint"/>, which gives range of 0 to 4,294,967,295.</remarks>
        protected uint RTSCommandNumber;

        // 6/16/2010
        /// <summary>
        /// Unique number incremented for each sent <see cref="RTSCommand"/> Enum, stored into the <see cref="RTSCommandNumber"/> field.
        /// </summary>
        /// <remarks>Stored as <see cref="uint"/>, which gives range of 0 to 4,294,967,295.</remarks>
        private uint _rtsCommandNumberIncrementer;

        /// <summary>
        /// Constructor, which sets the current <see cref="NetworkCommands"/> enum.
        /// </summary>
        /// <param name="command"><see cref="NetworkCommands"/> enum</param>
        protected RTSCommand(NetworkCommands command)
        {
            NetworkCommand = command;
        }

        // 9/7/2008; 6/9/2010: Updated to now Round to 2 digit. 
        /// <summary>
        /// Packs the <see cref="Vector3"/> and writes into given <see cref="PacketWriter"/>.
        /// </summary>
        /// <param name="inVector3"><see cref="Vector3"/> structure</param>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        protected static void PackVector3(ref Vector3 inVector3, ref PacketWriter packetWriter)
        {
            // Store 'inVector3' using 3 halfSingle (50%)
            var posX = new HalfSingle((float) Math.Round(inVector3.X,2));            
            var posY = new HalfSingle((float) Math.Round(inVector3.Y,2));
            var posZ = new HalfSingle((float) Math.Round(inVector3.Z,2));
            packetWriter.Write(posX.PackedValue);
            packetWriter.Write(posY.PackedValue);
            packetWriter.Write(posZ.PackedValue);           

        }

        // 9/7/2008 -   
        /// <summary>
        /// UnPacks the <see cref="Vector3"/> read from given <see cref="PacketReader"/>
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        /// <param name="outVector3">(OUT) <see cref="Vector3"/> structure</param>
        protected static void UnPackVector3(ref PacketReader packetReader, out Vector3 outVector3)
        {
            // Read 'Vector3'
            var posX = new HalfSingle();
            var posY = new HalfSingle();
            var posZ = new HalfSingle();
            // Convert back to PacketValues
            posX.PackedValue = packetReader.ReadUInt16();
            posY.PackedValue = packetReader.ReadUInt16();
            posZ.PackedValue = packetReader.ReadUInt16();
            // Store into Out 'outVector3'
            var tempVector3 = new Vector3
                {
                    X = posX.ToSingle(), Y = posY.ToSingle(), Z = posZ.ToSingle()
                };            
            
            // Store into Out 'outVector3'
            outVector3 = tempVector3;
        }

        // 12/4/2008; 6/9/2010: Updated to now Round to 2 digit. 
        /// <summary>
        /// Packs the <see cref="Vector2"/> and writes into given <see cref="PacketWriter"/>
        /// </summary>
        /// <param name="inVector2"><see cref="Vector2"/> structure</param>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        protected static void PackVector2(ref Vector2 inVector2, ref PacketWriter packetWriter)
        {
            // Store 'inVector3' using 3 halfSingle (50%)
            var posX = new HalfSingle((float) Math.Round(inVector2.X,2));
            var posY = new HalfSingle((float) Math.Round(inVector2.Y,2));            
            packetWriter.Write(posX.PackedValue);
            packetWriter.Write(posY.PackedValue);
            
        }

        // 12/4/2008 - 
        /// <summary>
        /// UnPacks the <see cref="Vector2"/> read from given <see cref="PacketReader"/>  
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        /// <param name="outVector2">(OUT) <see cref="Vector2"/> structure</param>
        protected static void UnPackVector2(ref PacketReader packetReader, out Vector2 outVector2)
        {
            // Read 'Vector3'
            var posX = new HalfSingle();
            var posY = new HalfSingle();           
            // Convert back to PacketValues
            posX.PackedValue = packetReader.ReadUInt16();
            posY.PackedValue = packetReader.ReadUInt16();          
            // Store into Out 'outVector2'
            var tempVector2 = new Vector2
            {
                X = posX.ToSingle(),
                Y = posY.ToSingle()
            };            
           
            // Store into Out 'outVector2'
            outVector2 = tempVector2;
        }

        // 9/8/2008 - 
        /// <summary>
        ///  Packs the <see cref="float"/> and writes into given <see cref="PacketWriter"/>
        /// </summary>
        /// <param name="inFloat"><see cref="float"/> value</param>
        /// <param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        protected static void PackFloat(ref float inFloat, ref PacketWriter packetWriter)
        {
            // Store 'inFloat' using a HalfSingle (50%)
            var nfloat = new HalfSingle(inFloat);
            packetWriter.Write(nfloat.PackedValue);

        }

        // 9/8/2008 - 
        /// <summary>
        /// UnPacks the <see cref="float"/> read from given <see cref="PacketReader"/>
        /// </summary>
        /// <param name="packetReader"><see cref="PacketReader"/> instance</param>
        /// <param name="outFloat">(OUT) <see cref="float"/> value</param>
        protected static void UnPackFloat(ref PacketReader packetReader, out float outFloat)
        {
            // Read 'Float'
            var nfloat = new HalfSingle {PackedValue = packetReader.ReadUInt16()};
            // Convert back to PacketValues
            // Store into Out 'outFloat'
            outFloat = nfloat.ToSingle();
            
        }

        ///<summary>
        /// Starts the creation of the given network packet, using the <see cref="PacketWriter"/>,
        /// by first writing the <see cref="NetworkCommands"/>, then <see cref="Player"/> number, followed
        /// by the <see cref="NetworkGameSyncer.CommunicationTurnNumber"/>, leaving  
        /// inherting classes to provide the remaining data.
        ///</summary>
        ///<param name="packetWriter"><see cref="PacketWriter"/> instance</param>
        public virtual void CreateNetworkPacket(ref PacketWriter packetWriter)
        {
            // Add Header RTS Command
            packetWriter.Write((byte)NetworkCommand);

            // Write Player Number
            packetWriter.Write(PlayerNumber);

            // Write CommunicationTurn Number
            CommunicationTurnNumber = NetworkGameSyncer.CommunicationTurnNumber;
            packetWriter.Write(CommunicationTurnNumber);

            // 6/16/2010
            // Write RTSCommand Number
            _rtsCommandNumberIncrementer++;
            RTSCommandNumber = _rtsCommandNumberIncrementer;
            packetWriter.Write(RTSCommandNumber);

            // 6/16/2010
            // Add command to validator.
            RTSCommandValidator.AddRecord(this);

        }

        // 6/16/2010: Updated to return a bool.
        ///<summary>
        /// Reads back the <see cref="Player"/> number and <see cref="NetworkGameSyncer.CommunicationTurnNumber"/>, leaving
        /// the inherting classes to read back the remaining data.
        ///</summary>
        ///<param name="packetReader">Instance of <see cref="PacketReader"/>.</param>
        /// <returns>True/False of result.</returns>
        public virtual bool ReadNetworkPacket(ref PacketReader packetReader)
        {
            // UnPack Player Number
            PlayerNumber = packetReader.ReadByte();

            // UnPack CommuncationTurn Number
            CommunicationTurnNumber = packetReader.ReadInt32();

            // 6/16/2010
            // UnPack RTSCommand Number
            RTSCommandNumber = packetReader.ReadUInt32();

            // 6/16/2010
            return true;
        }

        // 5/13/2009
        /// <summary>
        /// Returns this instance back into the <see cref="PoolManager"/>, setting 'Active' to false.
        /// </summary>
        public abstract void ReturnItemToPool();


        // 5/13/2009
        /// <summary>
        /// Sets the internal values back to their default; for example,
        /// the <see cref="NetworkCommands"/> Enum is set back to 'None'.
        /// </summary>
        public virtual void Clear()
        {
            NetworkCommand = NetworkCommands.None;
            PlayerNumber = 0;
            CommunicationTurnNumber = 0;
        }
        

        // 6/16/2010
        /// <summary>
        /// The <see cref="RTSCommandValidator"/> is used to store some history of each <see cref="RTSCommand"/> sent
        /// to the other players.  When a <see cref="RTSCommand"/> is corrupt, the other players will send a validation request, with
        /// the original <see cref="NetworkCommands"/> type and unique number.  This information is then used to search the internal
        /// collection to re-send the <see cref="RTSCommand"/>.
        /// </summary>
        /// <remarks>Currently, history is kept for the last <see cref="MaxPositions"/>, which is group by each <see cref="NetworkCommands"/> Enum.</remarks>
        protected static class RTSCommandValidator
        {
            // number of command turns to keep groups of RTSCommands.
            private const byte MaxPositions = 50;

            // Collection for NetworkCommands current position into internal List.
            private static readonly byte[] CurrentPosition = new byte[MaxPositions];

            // Dictionary, set with Key = (byte)NetworkCommand Enum, Value = List of RTSCommands sent for that Enum.
            private static readonly Dictionary<byte, List<RTSCommand>> SentRTSCommands = new Dictionary<byte, List<RTSCommand>>(25);
           

            /// <summary>
            /// Adds an <see cref="RTSCommand"/> to the collection.
            /// </summary>
            /// <param name="newCommand"><see cref="RTSCommand"/> to add</param>
            public static void AddRecord(RTSCommand newCommand)
            {
                // skip adding any validation records
                if (newCommand is RTSCommValidator) return;

                // cache network command as number
                var networkCommand = (byte)newCommand.NetworkCommand;

                // increase position for given NetworkCommand enum.
                CurrentPosition[networkCommand]++;
                if (CurrentPosition[networkCommand] >= MaxPositions)
                    CurrentPosition[networkCommand] = 0;

                var newPosition = CurrentPosition[networkCommand];

                // Check if list exist
                List<RTSCommand> commands;
                if (SentRTSCommands.TryGetValue(networkCommand, out commands))
                {
                    // cache
                    var command = commands[newPosition];

                    // if old command, return to memory pool.
                    if (command != null)
                        command.ReturnItemToPool();

                    // then add to list
                    commands[newPosition] = newCommand;
                }
                else
                {
                    // need to initialize list and add to dictionary.
                    var newList = new List<RTSCommand>(MaxPositions); 
                    for (var i = 0; i < MaxPositions; i++)
                    {
                        newList.Add(null);
                    }

                    // populate new command
                    newList[newPosition] = newCommand;

                    // Add list to dictionary
                    SentRTSCommands.Add(networkCommand, newList);
                }
               
            }

            /// <summary>
            /// Processes a failed <see cref="RTSCommand"/> by looking up the failed command in the
            /// internal collection, and resending to other player for re-processing.
            /// </summary>
            /// <remarks><see cref="RTSCommand"/> are only kept for so many generations; so if not found, it is noted in the log.</remarks>
            /// <param name="commandValidator"><see cref="RTSCommValidator"/> instance</param>
            /// <exception cref="ArgumentNullException">Thrown when the <paramref name="commandValidator"/> is Null.</exception>
            public static void ProcessFailedRTSCommand(RTSCommValidator commandValidator)
            {
                // check if null.
                if (commandValidator == null) 
                    throw new ArgumentNullException("commandValidator", @"The given command cannot be null!");

                //
                // Failed command;
                //

                // Extract command to send
                var rtsCommand = GetRTSCommand(commandValidator);

                // If Null, then Command is no longer availble to be resent.
                if (rtsCommand == null)
                {
                    Debug.WriteLine("RTSCommand # " + commandValidator.RTSCommandNumberToValidate +
                                    ", is no longer available for resending.");
                    return;
                }

                // failed, so resend command.
                SendRTSCommand(commandValidator, rtsCommand);
            }

            /// <summary>
            /// Resends out the failed RTSCommand to the other player, for reprocessing.
            /// </summary>
            /// <param name="commandValidator"><see cref="RTSCommValidator"/> instance</param>
            /// <param name="rtsCommand"><see cref="RTSCommand"/> to send</param>
            private static void SendRTSCommand(RTSCommValidator commandValidator, RTSCommand rtsCommand)
            {
                if (NetworkGameComponent.NetworkSession.IsHost)
                {
#if DEBUG
                    Debug.WriteLine("Resending to client the RTSCommand #" + commandValidator.RTSCommandNumberToValidate);
#endif
                    // send to client
                    NetworkGameComponent.AddCommandsForClientG(rtsCommand);
                }
                else
                {
#if DEBUG
                    Debug.WriteLine("Resending to server the RTSCommand #" + commandValidator.RTSCommandNumberToValidate);
#endif
                    // send to server
                    NetworkGameComponent.AddCommandsForServerG(rtsCommand);
                }
            }

            /// <summary>
            /// Searches the internal <see cref="SentRTSCommands"/> collection for the
            /// given <see cref="RTSCommand"/> which matches the <see cref="RTSCommValidator.RTSCommandNumberToValidate"/>.
            /// </summary>
            /// <param name="commandValidator"></param>
            /// <returns></returns>
            private static RTSCommand GetRTSCommand(RTSCommValidator commandValidator)
            {
                // iterate through each generation.
                var numberToSearchFor = commandValidator.RTSCommandNumberToValidate;
                List<RTSCommand> commands;
                if (SentRTSCommands.TryGetValue(commandValidator.RTSNetworkCommandToValidate, out commands))
                {
                    // iterate inner list.
                    var count = commands.Count;
                    for (var i = 0; i < count; i++)
                    {
                        var command = commands[i];
                        if (command == null) continue;

                        // check if correct number
                        if (numberToSearchFor == command.RTSCommandNumber)
                            return command;
                    } // End Loop

                } // End TryGetValue

                // Nothing found, so return null
                return null;
            }
            

            // 6/16/2010
            /// <summary>
            /// Used to send a request, back to original server or client who sent it, 
            /// for a new copy of the Failed <see cref="RTSCommand"/>.
            /// </summary>
            /// <param name="rtsCommandNumber">The unique command number</param>
            /// <param name="networkCommand"><see cref="NetworkCommands"/> Enum group</param>
            public static void SendRTSCommandValidator(uint rtsCommandNumber, NetworkCommands networkCommand)
            {
                // create RTSCommand Validator
                RTSCommValidator rtsCommValidator;
                PoolManager.GetNode(out rtsCommValidator);

                if (rtsCommValidator == null)
                {
#if DEBUG
                    Debug.WriteLine("SendRTSCommandValidator retrieval of RTSCommValidator came back null!");
#endif
                    return;
                }

                rtsCommValidator.Clear();
                rtsCommValidator.NetworkCommand = NetworkCommands.Validator;
                rtsCommValidator.RTSCommandNumberToValidate = rtsCommandNumber;
                rtsCommValidator.RTSNetworkCommandToValidate = (byte) networkCommand;

                // add to proper guaranttee queue.
                if (NetworkGameComponent.NetworkSession.IsHost)
                {
                    // send to client
                    NetworkGameComponent.AddCommandsForClientG(rtsCommValidator);
                }
                else
                {
                    // send to server
                    NetworkGameComponent.AddCommandsForServerG(rtsCommValidator);
                }

#if DEBUG
                Debug.WriteLine("SendRTSCommandValidator successfully sent a new request for RTSCommand # " + rtsCommandNumber);
#endif
            }

        } // End RTSCommValidator class

    }
}
