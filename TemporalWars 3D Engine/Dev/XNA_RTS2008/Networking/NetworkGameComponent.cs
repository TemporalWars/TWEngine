#region File Description
//-----------------------------------------------------------------------------
// NetworkGameComponent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;
using ParallelTasksComponent.LocklessQueue;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using ScreenTextDisplayer.ScreenText;
using TWEngine.GameScreens;
using TWEngine.IFDTiles;
using TWEngine.MemoryPool;
using TWEngine.Players;
using TWEngine.rtsCommands;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.Shapes;

namespace TWEngine.Networking
{
    ///<summary>
    /// The <see cref="NetworkGameComponent"/>, is the main manager in network games, responsible
    /// for managing the communciation between the network players.  This is accomplished by the use
    ///  of two internal <see cref="Thread"/>s; 1st thread checks for new <see cref="RTSCommand"/> request
    /// records from the network, and queues them up for processing for the 2nd thread.
    /// The 2nd thread, which is pumped each game-cycle, does the actual processing of the <see cref="RTSCommand"/> requests.
    /// Finally, to keep the player games in sync, this class uses the <see cref="NetworkGameSyncer"/> class.
    ///</summary>
    public sealed class NetworkGameComponent : GameComponent
    {
        /// <summary>
        /// The <see cref="PacketWriter"/> is used to write data to be sent accross the wire.
        /// </summary>
        private static PacketWriter _packetWriter = new PacketWriter();
        /// <summary>
        /// The <see cref="PacketReader"/> is used to read data from the wire.
        /// </summary>
        private static PacketReader _packetReader = new PacketReader();

#if DEBUG

        // 12/16/2008 - TODO: Debug - ScreenTextItems; shows the Network Queues Count.
        private readonly ScreenTextItem _screenText1;
        private readonly ScreenTextItem _screenText2;
        // 12/16/2008 - TODO: Debug - Show CommunicationTurn Delay.
        private readonly ScreenTextItem _screenText3;
        private readonly ScreenTextItem _screenText4; // Show CommunicationTurnNumber
        private readonly ScreenTextItem _screenText5; // Show ClientCommunicationTurnNumber on Host        

#endif

        // 9/14/2008 - Thread members
        private static volatile GameTime _gameTime;
        private static Thread _gameNetworkingThread;
        private static Thread _gameNetworkingProcessThread; // 8/10/2009
        private static volatile bool _isStopping;

        // 8/10/2009 - AutoResetEvent Prims;
        //             To use, simply call 'Set()', which activates thread method, and 'WaitOne()' in thread,
        //             which makes thread go to sleep and wait for 'Set()' call!
        private static readonly AutoResetEvent ProcessCommandsThreadStart = new AutoResetEvent(false);
        private static readonly AutoResetEvent ProcessCommandsThreadEnd = new AutoResetEvent(false);

        // 8/26/2009
        private static readonly Stopwatch TimerToSleep1 = new Stopwatch();
        private static readonly Stopwatch TimerToSleep2 = new Stopwatch();
        private static readonly Stopwatch StopWatchClientProcessBatch = new Stopwatch();

        // 9/8/2008
        // 
        /// <summary>
        /// How often should we send network packets?
        /// </summary>
        private const int FramesBetweenMoveToPackets = 8;

        /// <summary>
        /// Set by the <see cref="NetworkGameSyncer"/> class, which tells this class to
        /// send a netork packet this next game frame.
        /// </summary>
        internal static volatile bool SendPacketThisFrame; // Update by NetworkGameSyncer class.       

        
        /// <summary>
        /// Packet Send Time
        /// </summary>
        private static float _packetSendTime;
         
        /// <summary>
        /// how long a packet took to arrive; set in Client Receive.
        /// </summary>
        private static TimeSpan _latency;
        // Is prediction and/or smoothing enabled?
        private static bool _enablePrediction = true;
        private static bool _enableSmoothing = true;

        // 9/3/2008; 6/7/2010: Updated to new LocklessQueue.
        /// <summary>
        /// Queue of RTS Commands for Client
        /// </summary>
        private static readonly LocklessQueue<RTSCommand> CommandsForClient = new LocklessQueue<RTSCommand>();
        // 12/2/2008; 6/7/2010: Updated to new LocklessQueue.
        /// <summary>
        ///  Queue of RTS Commands (ReliableInOrder) for Client. 
        /// </summary>
        private static readonly LocklessQueue<RTSCommand> CommandsForClientG = new LocklessQueue<RTSCommand>();
        // 6/7/2010: Updated to new LocklessQueue.
        private static readonly LocklessQueue<RTSCommand> CommandsForClientToProcess = new LocklessQueue<RTSCommand>();

        // 9/3/2008; // 6/7/2010: Updated to new LocklessQueue.
        /// <summary>
        /// Queue of RTS Commands for Server
        /// </summary>
        private static readonly LocklessQueue<RTSCommand> CommandsForServer = new LocklessQueue<RTSCommand>();
        // 12/2/2008; // 6/7/2010: Updated to new LocklessQueue.
        /// <summary>
        ///  Queue of RTS Commands (ReliableInOrder) for Server.
        /// </summary>
        private static readonly LocklessQueue<RTSCommand> CommandsForServerG = new LocklessQueue<RTSCommand>();
        // 6/7/2010: Updated to new LocklessQueue.
        private static readonly LocklessQueue<RTSCommand> CommandsForServerToProcess = new LocklessQueue<RTSCommand>();

        #region Properties

        // 9/9/2008
        ///<summary>
        /// Used to enable the prediction algorithm.
        ///</summary>
        [Obsolete]
        public static bool EnablePrediction
        {
            get { return _enablePrediction; }
            set { _enablePrediction = value; }
        }

        ///<summary>
        /// Used to enable the smoothing algorithm.
        ///</summary>
        [Obsolete]
        public static bool EnableSmoothing
        {
            get { return _enableSmoothing; }
            set { _enableSmoothing = value; }
        }

        // 9/4/2008 - 
        /// <summary>
        /// During network games, the server side will increase this <see cref="NetworkItemCount"/> counter
        /// for every <see cref="SceneItem"/> added to world.  The ItemNumber is then passed
        /// back to the client who requested a new <see cref="SceneItem"/>.  This network <see cref="SceneItem"/> owner number
        /// is then used for all communications between the server and client, so they know what object they are talking about.
        /// </summary>
        private static int NetworkItemCount { get; set; }

        ///<summary>
        /// Get or Set the current <see cref="NetworkSession"/> instance.
        ///</summary>
        public static NetworkSession NetworkSession { get; set; }

        #endregion

        // Constructor
        ///<summary>
        /// Constructor for the <see cref="NetworkGameComponent"/> manager class.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="inNetworkSession"><see cref="NetworkSession"/> instance</param>
        public NetworkGameComponent(Game game, NetworkSession inNetworkSession) : base(game)
        {
            NetworkSession = inNetworkSession;

#if DEBUG

            // 12/16/2008 - TODO: Debug only - Init ScreenText Class
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 40), Color.Red, out _screenText1);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 55), Color.Red, out _screenText2);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 70), Color.Red, out _screenText3);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 85), Color.Red, out _screenText4);
            ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(10, 100), Color.Red, out _screenText5);
            // 6/10/2010 - Display on screen
            _screenText1.Visible = true;
            _screenText2.Visible = true;
            _screenText3.Visible = true;
            _screenText4.Visible = true;
            _screenText5.Visible = true;
#endif
        }

        // 8/5/2009
        /// <summary>
        /// Increases the internal <see cref="NetworkItemCount"/> counter, used
        /// to track items over the network.
        /// </summary>
        /// <returns>Returns the new unique <see cref="NetworkItemCount"/> value.</returns>
        public static int IncreaseNetworkItemCount()
        {
            NetworkItemCount++;

            return NetworkItemCount;
        }

        // 9/14/2008
        /// <summary>
        /// Starts the network <see cref="Thread"/> agents at beginning of game.  Then
        /// calls the method <see cref="UpdateNetworkSession"/> each cycle for processing.
        /// </summary>
        /// <param name="inGameTime"><see cref="GameTime"/> instance.</param>
        public override void Update(GameTime inGameTime)
        {
            StopWatchTimers.StartStopWatchInstance(StopWatchName.NetworkGCUpdate);//"NetworkGC-Update"

            // Save GameTime for Thread to use


            // 5/12/2009 - Since this class is created before the 'NetworkSession' is
            //             available, this Null check is now needed.
            if (NetworkSession != null)
            {
#if DEBUG
                // Build #1
                _screenText1.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!
                _screenText1.SbDrawText.Append("Client Queue Count: ");
                _screenText1.SbDrawText.Append(CommandsForClient.Count);
                _screenText1.SbDrawText.Append(", ClientP Queue Count: ");
                _screenText1.SbDrawText.Append(CommandsForClientToProcess.Count);
                _screenText1.SbDrawText.Append(", Client Batch Time: ");
                _screenText1.SbDrawText.Append(NetworkGameSyncer.ClientProcessingTimeAvg);
                _screenText1.SbDrawText.Append(", Client FrameTime: ");
                _screenText1.SbDrawText.Append(NetworkGameSyncer.ClientFrameTimeAvg);

                
                /*_screenText1.SbDrawText.AppendFormat(
                    "Client Queue Count: {0}, ClientP Queue Count: {1}, Client Batch Time: {2},  Client FrameTime: {3}",
                    CommandsForClient.Count, CommandsForClientToProcess.Count, NetworkGameSyncer.ClientProcessingTimeAvg,
                    NetworkGameSyncer.ClientFrameTimeAvg);*/

                // Build #2
                _screenText2.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!
                _screenText2.SbDrawText.Append("Server Queue Count: ");
                _screenText2.SbDrawText.Append(CommandsForServer.Count);
                _screenText2.SbDrawText.Append(", ServerP Queue Count: ");
                _screenText2.SbDrawText.Append(CommandsForServerToProcess.Count);
                _screenText2.SbDrawText.Append(", Server FrameTime: ");
                _screenText2.SbDrawText.Append(NetworkGameSyncer.ServerFrameTimeAvg);
                
                /*_screenText2.SbDrawText.AppendFormat(
                    "Server Queue Count: {0}, ServerP Queue Count: {1}, Server Batch Time: {2}, Server FrameTime: {3}",
                    CommandsForServer.Count, CommandsForServerToProcess.Count, NetworkGameSyncer.ServerProcessingTimeAvg,
                    NetworkGameSyncer.ServerFrameTimeAvg);*/

                // Build
                _screenText3.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!
                _screenText3.SbDrawText.Append("Communication Turn Time: ");
                _screenText3.SbDrawText.Append(NetworkGameSyncer.CommunicationTurnTime);
                _screenText3.SbDrawText.Append(", Latency: ");
                _screenText3.SbDrawText.Append(NetworkGameSyncer.ClientLatency);
                
                /*_screenText3.SbDrawText.AppendFormat(
                    "Communication Turn Time: {0}, Latency: {1}",
                    NetworkGameSyncer.CommunicationTurnTime, NetworkGameSyncer.ClientLatency);*/

                // Build #4
                _screenText4.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!
                _screenText4.SbDrawText.Append("Communication Turn Number: ");
                _screenText4.SbDrawText.Append(NetworkGameSyncer.CommunicationTurnNumber);
                _screenText4.SbDrawText.Append(", FrameDelay: ");               
                _screenText4.SbDrawText.Append(NetworkGameSyncer.FrameSyncDelay);
                
                /*_screenText4.SbDrawText.AppendFormat(
                    "Communication Turn Number: {0}, FrameDelay: {1}",
                    NetworkGameSyncer.CommunicationTurnNumber, NetworkGameSyncer.FrameSyncDelay);*/

                // 3/12/2009 - If running slowly, show for debug purposes.
                if (inGameTime.IsRunningSlowly)
                    _screenText4.SbDrawText.Append(" IsRunningSlowly: True");


                // Build #5
                if (NetworkSession.IsHost)
                {
                    _screenText5.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!
                    _screenText5.SbDrawText.Append("Client Communication Turn Number: ");
                    _screenText5.SbDrawText.Append(NetworkGameSyncer.ClientCommunicationTurnNumber);
                    _screenText5.SbDrawText.Append(", Client CommTurn Avg: ");
                    _screenText5.SbDrawText.Append(NetworkGameSyncer.ClientCompletedGameTurnAvg);

                    
                    /*_screenText5.SbDrawText.AppendFormat(
                        "Client Communication Turn Number: {0}, Client CommTurn Avg: {1}",
                        NetworkGameSyncer.ClientCommunicationTurnNumber, NetworkGameSyncer.ClientCompletedGameTurnAvg);*/
                }
#endif


                // 9/14/2008 - Start GameNetworking Thread
                if (_gameNetworkingThread == null)
                {
                    _gameNetworkingThread = new Thread(NetworkingThreadMethod)
                                               {
                                                   Name = "_gameNetworkingThread",
                                                   IsBackground = true
                                               };
                    _gameNetworkingThread.Start();
                }

                // 8/10/2009 - Start NetworkThread#2
                if (_gameNetworkingProcessThread == null)
                {
                    _gameNetworkingProcessThread = new Thread(NetworkingProcessingThreadMethod)
                                                      {
                                                          Name = "GameNetworkingProcessingThread",
                                                          IsBackground = true
                                                      };
                    _gameNetworkingProcessThread.Start();
                }

                // Process NetworkSession
                UpdateNetworkSession(inGameTime);
            }


            base.Update(inGameTime);

            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.NetworkGCUpdate); //"NetworkGC-Update"
        }

        // 8/10/2009
        // Check if any Thread need to be woken up.
        ///<summary>
        /// Wakes up the processing internal <see cref="Thread"/>, to check
        /// for any current frame processing.
        ///</summary>
        public static void PumpUpdateThreads()
        {
            ProcessCommandsThreadStart.Set();
        }

        // 8/10/2009
        ///<summary>
        /// Waits for each <see cref="Thread"/> <see cref="AutoResetEvent"/> to signal its finished
        /// working for the current frame.
        ///</summary>
        public static void WaitForThreadsToFinishCurrentFrame()
        {
            if (_gameNetworkingProcessThread == null)
                return;

            // 5/27/2010: Updated to now have 'WaitOne' in a while loop, while calling Thread.Sleep when waiting.
            // Wait Thread to end current frame.
            while(!ProcessCommandsThreadEnd.WaitOne(5)) //, false)) 
            {
                Thread.Sleep(1);
            }
        }


        // 9/14/2008
        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the queue for server to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        public static void AddCommandsForServer(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForServer.Enqueue(command);
            
        }

        // 12/2/2008
        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the 'ReliableInOrder' queue for server to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        public static void AddCommandsForServerG(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForServerG.Enqueue(command);
            
        }

        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the queue for server to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        private static void AddCommandsForServerToProcess(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForServerToProcess.Enqueue(command);
            
        }

        // 9/14/2008
        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the queue for client to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        public static void AddCommandsForClient(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForClient.Enqueue(command);
            
        }

        // 12/2/2008
        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the 'ReliableInOrder' queue for client to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        public static void AddCommandsForClientG(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForClientG.Enqueue(command);
            
        }

        /// <summary>
        /// Adds a <see cref="RTSCommand"/> to the queue for client to process
        /// </summary>
        /// <param name="command"><see cref="RTSCommand"/> instance</param>
        private static void AddCommandsForClientToProcess(RTSCommand command)
        {
            // 6/7/2010: Removed lock.
            // Enqueuing a record.
            CommandsForClientToProcess.Enqueue(command);
            
        }


        // 9/14/2008
        /// <summary>
        /// The main networking <see cref="Thread"/> for Server/Client reading game input from network, and
        /// storing into queues for processing, which the 2nd thread will take care of.
        /// </summary>
        /// <remarks>This <see cref="Thread"/> method runs continously.</remarks>
        private static void NetworkingThreadMethod()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(5);
#endif
            
                while (!_isStopping)
                {
                    // 1/2/2009 - Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                    //            thrown periodically when exiting the level!
                    //try
                    {
                        // 4/20/2010 - Cache
                        var networkSession = NetworkSession;
                        if (networkSession == null) continue;

                        // Run only if NetworkSession is not disposed of.
                        if (!networkSession.IsDisposed)
                        {
                            // Read inputs for locally controlled items, and send them to the server.
                            var localGamers = networkSession.LocalGamers; // 8/25/2009
                            var localGamersCount = localGamers.Count; // 8/25/2009
                            for (var i = 0; i < localGamersCount; i++)
                            {
                                // 4/20/2010 - Cache
                                LocalNetworkGamer localNetworkGamer;
                                try // 6/18/2010 - Handle the ArgOutOfRange error.
                                {
                                    localNetworkGamer = localGamers[i];
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                   break; 
                                }

                                if (localNetworkGamer == null) continue;

                                UpdateLocalGamer_ProcessCommands(localNetworkGamer);
                            }

                            // If we are the server, update all the items and transmit
                            // their latest positions back out over the network.
                            if (networkSession.IsHost)
                            {
                                UpdateServer_ProcessCommands();
                            }

                            // Pump the underlying session object.
                            networkSession.Update();

                            // Read any incoming network packets.
                            for (var i = 0; i < localGamersCount; i++)
                            {
                                // 8/25/2009 - Cache
                                LocalNetworkGamer localNetworkGamer;
                                try  // 6/18/2010 - Handle the ArgOutOfRange error.
                                {
                                    localNetworkGamer = localGamers[i];
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                   break;
                                }

                                if (localNetworkGamer == null) continue;

                                if (localNetworkGamer.IsHost)
                                {
                                    ServerReadInputFromClients(localNetworkGamer);
                                    //ServerProcessInputFromClient(NetworkSession.LocalGamers[loop1]);
                                }
                                else
                                {
                                    ClientReadGameStateFromServer(localNetworkGamer);
                                    //ClientProcessInputFromServer(NetworkSession.LocalGamers[loop1]);
                                }
                            } // End Forloop
                        } // End IF IsDisposed

                        // Have thread sleep a few millaseconds.
                        Thread.Sleep(1);
                    }
                    /*catch (NullReferenceException)
                    {
                        // Simply end thread when this occurs; seems to only occur when exiting level.
                        Debug.WriteLine("Network Component Thread threw NullRefExp.");
                    }
                    catch (InvalidOperationException) // 7/16/2009 - Occurs when the NetworkSession.Update() no longer works.
                    {
                        Debug.WriteLine("Network Component Thread threw InvalidOpExp.");
                    }
                    catch (ArgumentException) // 8/26/2009
                    {
                        // Error occurs when other player leaves, and the 'localGamers' indexing is off, due to Thread sync.
                        Debug.WriteLine(
                            "Network Component Thread threw ArgOpExp, caused by LocalGamer array indexing incorrectly.");
                    }*/

                } // End While
           
        }

        // 8/31/2008
        /// <summary>
        /// Updates the state of the network session, moving the items
        /// around and synchronizing their state over the network.
        /// </summary>
        private static void UpdateNetworkSession(GameTime gameTime)
        {
            // 8/10/2009 - Store GameTime.
            _gameTime = gameTime;

            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            // Make sure the session has not ended.
            if (networkSession == null) return;

            // Run only if NetworkSession is not disposed of.
            if (networkSession.IsDisposed) return;

            // Read inputs for locally controlled items, and send them to the server.
            var count = networkSession.LocalGamers.Count;
            for (var i = 0; i < count; i++)
            {
                // 4/20/2010 - Cache
                var localNetworkGamer = networkSession.LocalGamers[i];
                if (localNetworkGamer == null) continue;

                UpdateLocalGamer(gameTime, localNetworkGamer);
            }

            // If we are the server, update all the items and transmit
            // their latest positions back out over the network.
            if (networkSession.IsHost)
            {
                UpdateServer(gameTime);
            }

            // Read any incoming network packets.
            /*for (int loop1 = 0; loop1 < NetworkSession.LocalGamers.Count; loop1++)
                {
                    if (NetworkSession.LocalGamers[loop1].IsHost)
                    {
                        //ServerReadInputFromClients(NetworkSession.LocalGamers[loop1]);
                        ServerProcessInputFromClient(gameTime);
                    }
                    else
                    {
                        //ClientReadGameStateFromServer(NetworkSession.LocalGamers[loop1]);
                        ClientProcessInputFromServer(gameTime);
                    }
                } // End Forloop*/
        }

        /// <summary>
        /// The networking <see cref="Thread"/> method for Server/Client processing of commands.
        /// </summary>
        /// <remarks>This thread runs in sync with each game frame, using the Pumping action.</remarks>
        private static void NetworkingProcessingThreadMethod()
        {
            // Set XBOX-360 CPU Core for thread            
#if XBOX360
            Thread.CurrentThread.SetProcessorAffinity(3);
#endif

            while (!_isStopping)
            {
                // 1/2/2009 - Add Try-Catch construct to capture the 'System.InvalidOperationException' Exception,
                //            thrown periodically when exiting the level!
               // try
                {
                    // 8/10/2009 - Wait for Set() call to start.
                    ProcessCommandsThreadStart.WaitOne();

                    // 4/20/2010 - cache
                    var networkSession = NetworkSession;
                    // Make sure the session has not ended.
                    if (networkSession == null) continue;

                    // Read any incoming network packets.
                    var localGamersCount = networkSession.LocalGamers.Count; // 11/6/2009
                    for (var i = 0; i < localGamersCount; i++)
                    {
                        // 11/6/2009 - Cache
                        LocalNetworkGamer localGamer;
                        try // 6/18/2010 - Handle the ArgOutOfRange error.
                        {
                            localGamer = networkSession.LocalGamers[i];
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            break;
                        }
                        

                        // check if null.
                        if (localGamer == null) continue;

                        if (localGamer.IsHost)
                        {
                            ServerProcessInputFromClient(_gameTime);
                        }
                        else
                        {
                            ClientProcessInputFromServer(_gameTime);
                        }
                    } // End Forloop


                    // 8/10/2009 - Signal end of thread frame.
                    ProcessCommandsThreadEnd.Set();


                }
/*#pragma warning disable 168
                catch (NullReferenceException err)
#pragma warning restore 168
                {
                    // Simply end thread when this occurs; seems to only occur when exiting level.
                    Debug.WriteLine("Network Component Thread threw NullRefExp.");
                    Debugger.Break();
                }
#pragma warning disable 168
                catch (InvalidOperationException err)
#pragma warning restore 168
                    // 7/16/2009 - Occurs when the NetworkSession.Update() no longer works.
                {
                    Debug.WriteLine("Network Component Thread threw InvalidOpExp.");
                }*/

            } // End While
        }


        /// <summary>
        /// Helper for updating a locally controlled gamer.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="gamer"><see cref="Gamer"/> instance</param>
        private static void UpdateLocalGamer(GameTime gameTime, Gamer gamer)
        {
            // Look up what Player class is associated with this local player,
            // and read the latest user inputs for it. The server will
            // later use these values to control the sceneItems movement.
            var localPlayer = gamer.Tag as Player;

            // 2/21/2009 - Skip if localPlayer is Null; will occur within the Lobby screens,
            //             since the Player class has not been created yet.
            if (localPlayer == null) return;

            // Handle Input for localPlayer
            localPlayer.DoHandleInput(gameTime);

            // 4/20/2010 - check if null.
            if (NetworkSession == null) return;

            // If Server, then return; server calls this from 'UpdateServer' method.
            if (NetworkSession.IsHost) return;

            // Update localPlayer            
            localPlayer.Update(gameTime);

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player serverPlayer;
            TemporalWars3DEngine.GetPlayer(0, out serverPlayer);
            
            if (serverPlayer == null) return;

            // Update our copy of Server Player. - Currently, server is always set to be player zero.
            serverPlayer.Update(gameTime);
        }

        // 9/14/2008 - Called From thread
        // 12/2/2008 - 
        /// <summary>
        /// Processes all <see cref="RTSCommand"/> instances queued for the client, creating the network packets to be
        /// sent across the wire.
        /// </summary>
        /// <param name="localNetworkGamer"><see cref="LocalNetworkGamer"/> instance</param>
        private static void UpdateLocalGamer_ProcessCommands(LocalNetworkGamer localNetworkGamer)
        {
            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            // Only send if we are not the server. There is no point sending packets
            // to ourselves, because we already know what they will contain!
            if (networkSession.IsHost) return;

            // 11/5/2009 - Send Packet this frame?
            if (!SendPacketThisFrame) return;
            SendPacketThisFrame = false;

            // 6/10/2010 - Updated to use new refactored method.
            SendBatchOfCommands(localNetworkGamer, CommandsForServer, SendDataOptions.InOrder, false);

            // 6/10/2010 - Updated to use new refactored method.
            SendBatchOfCommands(localNetworkGamer, CommandsForServerG, SendDataOptions.ReliableInOrder, false);
        }

        /// <summary>
        /// This method only runs on the server. It calls the <see cref="Player"/> Update on all the
        /// <see cref="SceneItem"/> instances, both local and remote, using inputs that have
        /// been received over the network. It then sends the resulting <see cref="SceneItem"/>
        ///  position data to everyone in the session.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>     
        private static void UpdateServer(GameTime gameTime)
        {
            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            // 4/20/2010 - Cache
            var gamerCollection = networkSession.AllGamers;
            if (gamerCollection == null) return;

            // Loop over all the players in the session, not just the local ones!
            var count = gamerCollection.Count;
            for (var i = 0; i < count; i++)
            {
                // 4/20/2010 - Cache
                var networkGamer = gamerCollection[i];
                if (networkGamer == null) continue;

                // Look up what Player Class is associated with this player.
                var player = networkGamer.Tag as Player;

                // 2/21/2009 - Skip if localPlayer is Null; will occur within the Lobby screens,
                //             since the Player class has not been created yet.
                if (player == null) continue;

                // Update the Player's SelectableItems.
                player.Update(gameTime);
            } // END For Loop all Network Players          
        }


        // 9/14/2008 - Called by the thread
        // 12/2/2008 - Updated to now iterate the 2nd Queue - 'ReliableInOrder'. 
        /// <summary>
        /// Processes all <see cref="RTSCommand"/> instances queued for the server, creating the network packets to be
        /// sent across the wire.
        /// </summary>
        private static void UpdateServer_ProcessCommands()
        {
            // 12/16/2008 - Send Packet this frame?
            if (!SendPacketThisFrame) return;
            SendPacketThisFrame = false;

            // 4/20/2010 - Cache
            var networkSession = NetworkSession;
            if (networkSession == null) return;

            // Send the combined data for all Items to everyone in the session
            var server = (LocalNetworkGamer) networkSession.Host;
           
            // 6/10/2010
            SendBatchOfCommands(server, CommandsForClient, SendDataOptions.InOrder, true);

            // 6/10/2010 - Updated to use new refactored method.
            SendBatchOfCommands(server, CommandsForClientG, SendDataOptions.ReliableInOrder, true);
        }

        // 6/10/2010
        /// <summary>
        /// Iterates a given <see cref="LocklessQueue{T}"/> of <see cref="RTSCommand"/>, creating a new <see cref="PacketWriter"/>, and
        /// then sending it to the other players using the given <see cref="SendDataOptions"/>.
        /// </summary>
        /// <param name="localNetworkGamer"><see cref="LocalNetworkGamer"/> instance</param>
        /// <param name="commandsToSend"><see cref="LocklessQueue{T}"/> of commands</param>
        /// <param name="sendDataOptions"><see cref="SendDataOptions"/> Enum to use</param>
        /// <param name="isHost">Is this Host?</param>
        private static void SendBatchOfCommands(LocalNetworkGamer localNetworkGamer, LocklessQueue<RTSCommand> commandsToSend,
                                                SendDataOptions sendDataOptions, bool isHost)
        {
            // Make sure networkSession not Null.
            var networkSession = NetworkSession;
            if (networkSession == null) return;
            // Make sure commandsToSend not Null.
            if (commandsToSend == null) return;
            // Return if nothing to process
            if (commandsToSend.IsEmpty) return;

            // Get count.
            var commandCount = commandsToSend.Count;

            // First off, our packet will indicate how many players it has data for.
            _packetWriter.Write(networkSession.AllGamers.Count);

            // Write placeholder for command count.
            _packetWriter.Write(0); 
            
            // 6/9/2010 - Changed to Loop rather than While; this is because the While method 
            //            could iterate MORE records than the Count value written above!
            // 2nd - Iterate through 'CommandsForClientG' Queue for special 'ReliableInOrder' commands.
            RTSCommand command;
            var actualCommandCount = 0; // 6/10/2010
            //while (commandsForClientG.Count > 0)
            for (var i = 0; i < commandCount; i++)
            {
                // 6/7/2010 - Updated to use TryDequeue.
                if (!commandsToSend.TryDequeue(out command)) break;

                // 6/10/2010 - Tracks true count, incase it differs from 'commandCount'.
                actualCommandCount++;

                // Call the CreateNetworkPacket from Base Abstract class,
                // which through polymorphism, the proper inherited version should
                // be run.
                command.CreateNetworkPacket(ref _packetWriter);

                // 6/16/2010 - All RTS commands are now returned in Validator
                // 5/13/2009 - Return to MemoryPool
                //command.ReturnItemToPool(); 
            }

            const int countSeekPos = sizeof(int) / sizeof(byte); // 6/10/2010
            _packetWriter.Seek(countSeekPos, SeekOrigin.Begin); // 6/10/2010
            _packetWriter.Write(actualCommandCount);

            // Now Write how many actual Commands to expect, and send data.
            if (isHost)
                localNetworkGamer.SendData(_packetWriter, sendDataOptions);
            else
                localNetworkGamer.SendData(_packetWriter, sendDataOptions, networkSession.Host);
        }

        
        // 9/3/2008 - called from thread
        /// <summary>
        /// This method only runs on the server. It reads player inputs that
        /// have been sent over the network by a client machine, which is then
        /// read and stored into the <see cref="CommandsForServerToProcess"/> queue.
        /// </summary>    
        /// <param name="localNetworkGamer"><see cref="LocalNetworkGamer"/> instance</param>
        private static void ServerReadInputFromClients(LocalNetworkGamer localNetworkGamer)
        {
            // Keep reading as long as incoming packets are available.
            while (localNetworkGamer.IsDataAvailable)
            {
                NetworkGamer sender;

                // 4/20/2010 - Cache
                var packetReader = _packetReader;
                if (packetReader == null) break;

                // Read a single packet from the network.
                localNetworkGamer.ReceiveData(packetReader, out sender);

                // Make sure we are reading Remote Data
                if (sender.IsLocal) continue;

                // 6/10/2010 - Reads packet and throw away; however, used with client calls.
                //             Kept the writing of this packet for both client and server, regardless if server doesn't use.
                packetReader.ReadInt32(); 

                // Now Read Quantity of Commands to expect
                var numberOfCommands = packetReader.ReadInt32();

                for (var i = 0; i < numberOfCommands; i++)
                {
                    // Read the networkCommand Header
                    var command = (NetworkCommands)packetReader.ReadByte(); // 6/16/2010 - Updated to ReadByte.

                    // Create the proper RTSCommand Class to Read the networkPacket
                    switch (command)
                    {
                        case NetworkCommands.ReqAddSceneItem:
                            // Create Command Class, and retrieve command                               
                            RTSCommAddSceneItem commAddItem;
                            PoolManager.GetNode(out commAddItem);

                            commAddItem.Clear();
                            commAddItem.NetworkCommand = NetworkCommands.ReqAddSceneItem;
                            // 6/16/2010 - Validation
                            if (commAddItem.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(commAddItem);
                            break;
                        case NetworkCommands.ReqUnitMoveOrder:
                            // Create Command Class, and retrieve command
                            RTSCommMoveSceneItem commMoveItem;
                            PoolManager.GetNode(out commMoveItem);

                            commMoveItem.Clear();
                            commMoveItem.NetworkCommand = NetworkCommands.ReqUnitMoveOrder;
                            // 6/16/2010 - Validation
                            if (commMoveItem.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(commMoveItem);
                            break;
                        case NetworkCommands.ReqStartAttackSceneItem:
                            RTSCommStartAttackSceneItem startAttackItem;
                            PoolManager.GetNode(out startAttackItem);

                            startAttackItem.Clear();
                            startAttackItem.NetworkCommand = NetworkCommands.ReqStartAttackSceneItem;
                            // 6/16/2010 - Validation
                            if (startAttackItem.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(startAttackItem);
                            break;
                        case NetworkCommands.SyncTime: // 12/2/2008
                            RTSCommSyncTime syncTime;
                            PoolManager.GetNode(out syncTime);

                            syncTime.Clear();
                            syncTime.NetworkCommand = NetworkCommands.SyncTime;
                            // 6/16/2010 - Validation
                            if (syncTime.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(syncTime);
                            break;
                        case NetworkCommands.IsReady: // 12/2/2008
                            RTSCommIsReady isReady;
                            PoolManager.GetNode(out isReady);

                            isReady.Clear();
                            isReady.NetworkCommand = NetworkCommands.IsReady;
                            // 6/16/2010 - Validation
                            if (isReady.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(isReady);
                            break;
                        case NetworkCommands.GameTurn: // 12/3/2008
                            RTSCommGameTurn gameTurn;
                            PoolManager.GetNode(out gameTurn);

                            gameTurn.Clear();
                            gameTurn.NetworkCommand = NetworkCommands.GameTurn;
                            // 6/16/2010 - Validation
                            if (gameTurn.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(gameTurn);
                            break;
                        case NetworkCommands.QueueMarker: // 12/9/2008
                            RTSCommQueueMarker queueMarker;
                            PoolManager.GetNode(out queueMarker);

                            queueMarker.Clear();
                            queueMarker.NetworkCommand = NetworkCommands.QueueMarker;
                            // 6/16/2010 - Validation
                            if (queueMarker.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(queueMarker);
                            break;
                        case NetworkCommands.KillSceneItem: // 1/2/2009
                            RTSCommKillSceneItem killSceneItem;
                            PoolManager.GetNode(out killSceneItem);

                            killSceneItem.Clear();
                            killSceneItem.NetworkCommand = NetworkCommands.KillSceneItem;
                            // 6/16/2010 - Validation
                            if (killSceneItem.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(killSceneItem);
                            break;
                        case NetworkCommands.GameSlow: // 3/12/2009 
                            RTSCommGameSlow gameSlowItem;
                            PoolManager.GetNode(out gameSlowItem);

                            gameSlowItem.Clear();
                            gameSlowItem.NetworkCommand = NetworkCommands.GameSlow;
                            // 6/16/2010 - Validation
                            if (gameSlowItem.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(gameSlowItem);
                            break;
                        case NetworkCommands.LobbyData: // 4/9/2009  
                            RTSCommLobbyData lobbyData;
                            PoolManager.GetNode(out lobbyData);

                            lobbyData.Clear();
                            lobbyData.NetworkCommand = NetworkCommands.LobbyData;
                            // 6/16/2010 - Validation
                            if (lobbyData.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(lobbyData);
                            break;
                        case NetworkCommands.LobbyData_UserNotReady: // 9/1/2009
                            RTSCommLobbyData lobbyData2;
                            PoolManager.GetNode(out lobbyData2);

                            lobbyData2.Clear();
                            lobbyData2.NetworkCommand = NetworkCommands.LobbyData_UserNotReady;
                            // 6/16/2010 - Validation
                            if (lobbyData2.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(lobbyData2);
                            break;
                        case NetworkCommands.SceneItemStance: // 6/2/2009
                            RTSCommSceneItemStance sceneItemStance;
                            PoolManager.GetNode(out sceneItemStance);

                            sceneItemStance.Clear();
                            sceneItemStance.NetworkCommand = NetworkCommands.SceneItemStance;
                            // 6/16/2010 - Validation
                            if (sceneItemStance.ReadNetworkPacket(ref packetReader))
                                AddCommandsForServerToProcess(sceneItemStance);
                            break;
                        case NetworkCommands.Validator: // 6/16/2010
                            RTSCommValidator rtsCommValidator;
                            PoolManager.GetNode(out rtsCommValidator);

                            // Note: Only need to read command, which takes care of validation.
                            rtsCommValidator.Clear();
                            rtsCommValidator.NetworkCommand = NetworkCommands.Validator;
                            rtsCommValidator.ReadNetworkPacket(ref packetReader);

                            break;
                        default:
                            break;
                    } // End Switch commands 
                } // End ForLoop  

                // Add Quantity of Commands Server expects to Process
                /*lock (_queueDataThreadLock)
                    {
                        quantityForServerToProcess.Enqueue(numberOfCommands);
                    }*/
            }
        }

        private static readonly Stopwatch StopWatchServerProcessBatch = new Stopwatch();

        // 9/14/2008: 4/6/2009: Updated to include the 'GameTime' parameter.
        /// <summary>
        /// Iterates through the <see cref="CommandsForServerToProcess"/> queue, and processes
        /// the <see cref="RTSCommand"/> instance stored by the <see cref="Thread"/>, 
        /// via the method <see cref="ServerReadInputFromClients"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        internal static void ServerProcessInputFromClient(GameTime gameTime)
        {
            try // 11/6/2009
            {
                // 4/20/2010 - Cache
                var commandsForServerToProcess = CommandsForServerToProcess;
                if (commandsForServerToProcess == null) return;

                // 8/26/2009
                var count = commandsForServerToProcess.Count; 
                if (count == 0) return;

                // 1/2/2009 - Part of syncing the network games is to measure how long
                //            it takes the server computer to process the batch of commands
                //            below for this gameturn!
                StopWatchServerProcessBatch.Reset();
                StopWatchServerProcessBatch.Start();

                // 8/26/2009 - Start StopWatch timer
                TimerToSleep2.Reset();
                TimerToSleep2.Start();

                // Process Commands sent from Client
                RTSCommand command;
                for (var i = 0; i < count; i++)
                {
                    // 6/7/2010 - Updated to use TryDequeue.
                   if (!commandsForServerToProcess.TryDequeue(out command)) continue;

                    switch (command.NetworkCommand)
                    {
                        case NetworkCommands.ReqAddSceneItem:
                            // Cast to proper command
                            var commAddItem = (RTSCommAddSceneItem) command;
                            
                            // Increase NetworkItemNumber Counter
                            var newNetworkNumber = IncreaseNetworkItemCount();

                            // 3rd - Add Command to be sent for clients to create the SceneItemOwner
                            commAddItem.NetworkItemNumber = newNetworkNumber;
                            commAddItem.NetworkCommand = NetworkCommands.AddSceneItem;
                            AddCommandsForClientG(commAddItem); // 12/2/2008 - Updated to 'ReliableInOrder' Queue.

                            // 2nd - Create new SceneItemOwner on Server's copy of Client Player
                            var e = new ItemCreatedArgs(commAddItem.BuildingType, commAddItem.ProductionType,
                                                        commAddItem.ItemType, commAddItem.ItemGroupToAttack,
                                                        commAddItem.AtPosition,
                                                        commAddItem.BuildingNetworkItemNumber, null,
                                                        gameTime.TotalGameTime.TotalSeconds)
                                        {
                                            IsBotHelper = commAddItem.IsBotHelper,
                                            LeaderUniqueNumber = commAddItem.LeaderNetworkItemNumber,
                                            LeaderPlayerNumber = commAddItem.LeaderPlayerNumber,
                                        };

                           
                            // 6/15/2010 - Updated to use new GetPlayer method.
                            Player player1;
                            if (!TemporalWars3DEngine.GetPlayer(commAddItem.PlayerNumber, out player1))
                                break;

                            Player.AddSceneItem(player1, newNetworkNumber, e);
                            break;
                        case NetworkCommands.ReqUnitMoveOrder:
                            // Cast to proper command
                            var commMoveItem = (RTSCommMoveSceneItem)command;
                            {
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(commMoveItem.PlayerNumber, out player))
                                    break;

                                // 2nd - Send Move order for SceneItem on Server's copy of Client Player
                                if (player != null)
                                    player.UnitsMoveOrder(ref commMoveItem.NetworkItemNumber, ref commMoveItem.Position,
                                                          commMoveItem.IsAttackMoveOrder);
                                // 5/13/2009 - Return to PoolManager
                                commMoveItem.ReturnItemToPool();
                            }
                            break;
                        case NetworkCommands.ReqStartAttackSceneItem:
                            // Cast to proper command
                            var startAttackItem = (RTSCommStartAttackSceneItem)command;
                            {
                                // 2nd - Send Attack Order for SceneItem on Server's copy of Client Player
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(startAttackItem.SceneItemAttackerPlayerNumber, out player))
                                    break;

                                if (player != null) Player.UnitsStartAttackItemOrder(player, startAttackItem);

                                // 5/13/2009 - Return to PoolManager
                                startAttackItem.ReturnItemToPool();
                            }
                            break;
                        case NetworkCommands.SyncTime: // 12/2/2008
                            // Cast to proper command
                            var syncTime = (RTSCommSyncTime) command;
                            // 2nd - Store syncTime value from client into the 'NetworkGameSyncer' class.
                            NetworkGameSyncer.ClientAvgTurnSpeed = syncTime.AvgTurnSpeedTime;
                            // 5/13/2009 - Return to PoolManager
                            syncTime.ReturnItemToPool();
                            break;
                        case NetworkCommands.IsReady: // 12/2/2008
                            // Cast to proper command
                            var isReady = (RTSCommIsReady) command;
                            // 2nd - Store 'IsReady' value into TerrainScreen class.
                            TerrainScreen.ClientIsReady = isReady.IsReadyToStart;
                            // 5/13/2009 - Return to PoolManager
                            isReady.ReturnItemToPool();
                            break;
                        case NetworkCommands.GameTurn: // 12/3/2008
                            var gameTurn = (RTSCommGameTurn) command;
                            // 2nd - Store 'FrameForTurn' value into NetworkGameSyncer class.
                            NetworkGameSyncer.SetClientCompletedGameTurn(gameTurn);
                            // 5/13/2009 - Return to PoolManager
                            gameTurn.ReturnItemToPool();
                            break;
                        case NetworkCommands.QueueMarker: // 12/9/2008
                            var queueMarker = (RTSCommQueueMarker) command;

                            // 2nd - Store new 'Queue' translation into proper SceneItemOwner.                      

                            // 12/17/2008 - Search SelectableItems Dict   
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(queueMarker.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                        queueMarker.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // 4/20/2010 - Cache
                                        var buildingShape = ((BuildingShape)sceneItemToUpdate.ShapeItem);
                                        
                                        // Update
                                        if (buildingShape != null)
                                            buildingShape.SetMarkerPosition(
                                                ref queueMarker.NewQueuePosition, queueMarker.NetworkItemNumber);
                                    }
                            }

                            // 5/13/2009 - Return to PoolManager
                            queueMarker.ReturnItemToPool();

                            break;
                        case NetworkCommands.KillSceneItem: // 1/2/2009
                            var killSceneItem = (RTSCommKillSceneItem) command;

                            // 1/2/2009 - Search SelectableItems Dict
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(killSceneItem.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                        killSceneItem.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // Found SceneItemOwner, so Kill SceneItemOwner and break out of loop
                                        var elapsedTime = gameTime.ElapsedGameTime;
                                        if (sceneItemToUpdate != null)
                                            sceneItemToUpdate.StartKillSceneItem(ref elapsedTime, killSceneItem.AttackerPlayerNumber);
                                    }
                            }

                            // 5/13/2009 - Return to PoolManager
                            killSceneItem.ReturnItemToPool();
                            break;
                        case NetworkCommands.GameSlow: // 3/12/2009
                            var gameSlowItem = (RTSCommGameSlow) command;
                            // Store result into NetworkSyncerClass
                            NetworkGameSyncer.GameRunningSlowlyOnClient();

                            // 5/13/2009 - Return to PoolManager
                            gameSlowItem.ReturnItemToPool();
                            break;
                        case NetworkCommands.LobbyData: // 4/9/2009
                            var lobbyData = (RTSCommLobbyData) command;

                            Lobby2Screen.Network_SetGamerInfo(lobbyData, NetworkSession);

                            // 5/13/2009 - Return to MemoryPool
                            lobbyData.ReturnItemToPool();

                            break;
                        case NetworkCommands.LobbyData_UserNotReady: // 9/1/2009

                            var lobbyData2 = (RTSCommLobbyData) command;

                            LobbyScreen.SetLocalGamersToNotReady(NetworkSession);

                            // Return to MemoryPool
                            lobbyData2.ReturnItemToPool();


                            break;
                        case NetworkCommands.SceneItemStance: // 6/2/2009
                            var sceneItemStance = (RTSCommSceneItemStance) command;

                            // Search SelectableItems Dict 
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(sceneItemStance.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                        sceneItemStance.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // 4/20/2010
                                        if (sceneItemToUpdate != null) 
                                        {
                                            // Found SceneItemOwner, so set Stance
                                            sceneItemToUpdate.DefenseAIStance = sceneItemStance.DefenseAIStance;
                                            // 6/2/2009 - Set the AIOrderIssued enum to 'None'.
                                            sceneItemToUpdate.AIOrderIssued = AIOrderType.None;
                                        }
                                    }
                            }

                            // Return to PoolManager
                            sceneItemStance.ReturnItemToPool();

                            break;
                        default:
                            break;
                    } // End Switch

                    // 8/26/2009 - Sleep every few ms.
                    if (TimerToSleep2.Elapsed.TotalMilliseconds >= 40)
                    {
                        Thread.Sleep(0);
                        TimerToSleep2.Reset();
                        TimerToSleep2.Start();
                    }

                } // end For loop

                // 11/7/2009: Discovered the 'ElapsedMS' is always zero in the StopWatch; therefore, the 'TotalMilliseconds' is used.
                // 1/2/2009 - Done processing this Gameturns batch of commands, so calc Time it took.
                StopWatchServerProcessBatch.Stop();
                NetworkGameSyncer.SetServerCommandProcessingTime((float)StopWatchServerProcessBatch.Elapsed.TotalMilliseconds);

               
            }
#pragma warning disable 168
            catch (NullReferenceException err)
#pragma warning restore 168
            {
                Debug.WriteLine("ServerProcessInputFromClient method Thread threw NullRefExp.");
                Debugger.Break();
            }
        }


        // 9/3/2008 - called from thread
        /// <summary>
        /// This method only runs on client machines. It reads
        /// player data that has been computed by the server.
        /// </summary>
        private static void ClientReadGameStateFromServer(LocalNetworkGamer gamer)
        {
            // 4/20/2010 - Check if null
            if (gamer == null) return;

            // Keep reading as long as incoming packets are available.
            while (gamer.IsDataAvailable)
            {
                NetworkGamer sender;
                
                // 4/20/2010 - Cache
                var networkSession = NetworkSession; // 4/20/2010
                if (networkSession == null) break;

                // Read a single packet from the network.
                gamer.ReceiveData(_packetReader, out sender);

                // 4/20/2010 - Cache
                var packetReader = _packetReader;
                if (packetReader == null) break;


                // If a player has recently joined or left, it is possible the server
                // might have sent information about a different number of players
                // than the client currently knows about. If so, we will be unable
                // to match up which data refers to which player. The solution is
                // just to ignore the packet for now: this situation will resolve
                // itself as soon as the client gets the join/leave notification.
                var playersCount = packetReader.ReadInt32(); // 6/10/2010
                if (networkSession.AllGamers.Count != playersCount)
                {
#if DEBUG
                    Debug.WriteLine("Player Count was incorrect during ClientRead.");
#endif
                    continue;
                }

#if DEBUG
                // Set Estimate of how long this packet took to be received.
                _latency = networkSession.SimulatedLatency +
                          TimeSpan.FromTicks(sender.RoundtripTime.Ticks/2);
#else
                // Set Estimate of how long this packet took to be received.
                _latency = TimeSpan.FromTicks(sender.RoundtripTime.Ticks/2);
#endif

                // 12/16/2008 - Set ClientLatency into NetworkGameSyncer.
                NetworkGameSyncer.ClientLatency = _latency.Milliseconds;

                // Now Read Quantity of Commands to expect
                var numberOfCommands = packetReader.ReadInt32();

                // Iterate through commands sent to client by server
                for (var i = 0; i < numberOfCommands; i++)
                {
                    // Read the networkCommand Header
                    var command = (NetworkCommands) packetReader.ReadByte(); // 6/16/2010 - Updated to ReadByte.

                    // Create the proper RTSCommand Class to Read the networkPacket
                    switch (command)
                    {
                        case NetworkCommands.AddSceneItem:
                            RTSCommAddSceneItem commAddItem;
                            PoolManager.GetNode(out commAddItem);

                            // 4/20/2010 - Check if null
                            if (commAddItem != null)
                            {
                                commAddItem.Clear();
                                commAddItem.NetworkCommand = NetworkCommands.AddSceneItem;
                                // 6/16/2010 - Validation
                                if (commAddItem.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(commAddItem);
                            }

                            break;
                        case NetworkCommands.UnitMoveOrder:
                            RTSCommMoveSceneItem2 commMoveItem;
                            PoolManager.GetNode(out commMoveItem);

                            // 4/20/2010 - Check if null
                            if (commMoveItem != null)
                            {
                                commMoveItem.Clear();
                                commMoveItem.NetworkCommand = NetworkCommands.UnitMoveOrder;
                                // 6/16/2010 - Validation
                                if (commMoveItem.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(commMoveItem);
                            }

                            break;
                        case NetworkCommands.StartAttackSceneItem: // 11/20/2008
                            RTSCommStartAttackSceneItem startAttackItem;
                            PoolManager.GetNode(out startAttackItem);

                            // 4/20/2010 - Check if null
                            if (startAttackItem != null)
                            {
                                startAttackItem.Clear();
                                startAttackItem.NetworkCommand = NetworkCommands.StartAttackSceneItem;
                                // 6/16/2010 - Validation
                                if (startAttackItem.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(startAttackItem);
                            }

                            break;
                        case NetworkCommands.SyncTime: // 12/2/2008
                            RTSCommSyncTime syncTime;
                            PoolManager.GetNode(out syncTime);

                            // 4/20/2010 - Check if null
                            if (syncTime != null)
                            {
                                syncTime.Clear();
                                syncTime.NetworkCommand = NetworkCommands.SyncTime;
                                // 6/16/2010 - Validation
                                if (syncTime.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(syncTime);
                            }

                            break;
                        case NetworkCommands.DelayTime: // 12/2/2008 (Client only)
                            RTSCommDelayTime delayTime;
                            PoolManager.GetNode(out delayTime);

                            // 4/20/2010 - Check if null
                            if (delayTime != null)
                            {
                                delayTime.Clear();
                                delayTime.NetworkCommand = NetworkCommands.DelayTime;
                                // 6/16/2010 - Validation
                                if (delayTime.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(delayTime);
                            }

                            break;
                        case NetworkCommands.GameTurn: // 12/17/2008
                            RTSCommGameTurn gameTurn;
                            PoolManager.GetNode(out gameTurn);

                            // 4/20/2010 - Check if null
                            if (gameTurn != null)
                            {
                                gameTurn.Clear();
                                gameTurn.NetworkCommand = NetworkCommands.GameTurn;
                                // 6/16/2010 - Validation
                                if (gameTurn.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(gameTurn);
                            }

                            break;
                        case NetworkCommands.IsReady: // 12/2/2008
                            RTSCommIsReady isReady;
                            PoolManager.GetNode(out isReady);

                            // 4/20/2010 - Check if null
                            if (isReady != null)
                            {
                                isReady.Clear();
                                isReady.NetworkCommand = NetworkCommands.IsReady;
                                // 6/16/2010 - Validation
                                if (isReady.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(isReady);
                            }

                            break;
                        case NetworkCommands.KillSceneItem: // 12/10/2008                            
                            RTSCommKillSceneItem killSceneItem;
                            PoolManager.GetNode(out killSceneItem);

                            // 4/20/2010 - Check if null
                            if (killSceneItem != null)
                            {
                                killSceneItem.Clear();
                                killSceneItem.NetworkCommand = NetworkCommands.KillSceneItem;
                                // 6/16/2010 - Validation
                                if (killSceneItem.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(killSceneItem);
                            }

                            break;
                        case NetworkCommands.LobbyData: // 4/9/2009
                            RTSCommLobbyData lobbyData;
                            PoolManager.GetNode(out lobbyData);

                            // 4/20/2010 - Check if null
                            if (lobbyData != null)
                            {
                                lobbyData.Clear();
                                lobbyData.NetworkCommand = NetworkCommands.LobbyData;
                                // 6/16/2010 - Validation
                                if (lobbyData.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(lobbyData);
                            }

                            break;
                        case NetworkCommands.LobbyData_UserNotReady: // 9/1/2009
                            RTSCommLobbyData lobbyData2;
                            PoolManager.GetNode(out lobbyData2);

                            // 4/20/2010 - Check if null
                            if (lobbyData2 != null)
                            {
                                lobbyData2.Clear();
                                lobbyData2.NetworkCommand = NetworkCommands.LobbyData_UserNotReady;
                                // 6/16/2010 - Validation
                                if (lobbyData2.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(lobbyData2);
                            }
                            break;
                        case NetworkCommands.SceneItemStance: // 6/2/2009
                            RTSCommSceneItemStance sceneItemStance;
                            PoolManager.GetNode(out sceneItemStance);

                            // 4/20/2010 - Check if null
                            if (sceneItemStance != null)
                            {
                                sceneItemStance.Clear();
                                sceneItemStance.NetworkCommand = NetworkCommands.SceneItemStance;
                                // 6/16/2010 - Validation
                                if (sceneItemStance.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(sceneItemStance);
                            }

                            break;
                        case NetworkCommands.CeaseAttackSceneItem: // 6/2/2009
                            RTSCommCeaseAttackSceneItem ceaseAttack;
                            PoolManager.GetNode(out ceaseAttack);

                            // 4/20/2010 - Check if null
                            if (ceaseAttack != null)
                            {
                                ceaseAttack.Clear();
                                ceaseAttack.NetworkCommand = NetworkCommands.CeaseAttackSceneItem;
                                // 6/16/2010 - Validation
                                if (ceaseAttack.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(ceaseAttack);
                            }

                            break;
                        case NetworkCommands.SceneItemHealth: // 8/3/2009
                            RTSCommSceneItemHealth sceneItemHealth;
                            PoolManager.GetNode(out sceneItemHealth);

                            // 4/20/2010 - Check if null
                            if (sceneItemHealth != null)
                            {
                                sceneItemHealth.Clear();
                                sceneItemHealth.NetworkCommand = NetworkCommands.SceneItemHealth;
                                // 6/16/2010 - Validation
                                if (sceneItemHealth.ReadNetworkPacket(ref packetReader))
                                    AddCommandsForClientToProcess(sceneItemHealth);
                            }

                            break;
                        case NetworkCommands.Validator: // 6/16/2010
                            RTSCommValidator rtsCommValidator;
                            PoolManager.GetNode(out rtsCommValidator);

                            // Note: Only need to read command, which takes care of validation.
                            if (rtsCommValidator != null)
                            {
                                rtsCommValidator.Clear();
                                rtsCommValidator.NetworkCommand = NetworkCommands.Validator;
                                rtsCommValidator.ReadNetworkPacket(ref packetReader);
                            }

                            break;
                    } // End Switch
                } // End ForLoop
                
            }
        }

       

        // 9/14/2008 -  4/6/2009: Updated to include the 'GameTime' parameter.  
        /// <summary>
        /// Iterates through the <see cref="CommandsForClientToProcess"/> queue, and processes
        /// the <see cref="RTSCommand"/> instance stored by the <see cref="Thread"/>, 
        /// via the method <see cref="ClientReadGameStateFromServer"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        internal static void ClientProcessInputFromServer(GameTime gameTime)
        {
            //try // 11/6/2009
            {
                // 4/20/2010 - Cache
                var commandsForClientToProcess = CommandsForClientToProcess;
                if (commandsForClientToProcess == null) return;

                var clientToProcessCount = commandsForClientToProcess.Count; // 8/26/2009
                if (clientToProcessCount == 0) return;

                // 6/7/2010 - Updated to use TryPeek.
                // 1/12/2009 - Peek at top SceneItemOwner to see if command is part of next turn number.
                RTSCommand command;
                if(!commandsForClientToProcess.TryPeek(out command)) return;
                if (command.CommunicationTurnNumber > NetworkGameSyncer.CommunicationTurnNumber)
                {
                    // then let server know done with prior gameTurn!

                    // End of GameTurn
                    // 12/22/2008 - Set Client completed processing.
                    NetworkGameSyncer.ClientCompletedGameTurn = true;
                }

                // 12/22/2008 - Part of syncing the network games is to measure how long
                //              it takes the client computer to process the batch of commands
                //              below for this gameturn!
                StopWatchClientProcessBatch.Reset();
                StopWatchClientProcessBatch.Start();

                // 8/26/2009 - Start StopWatch timer
                TimerToSleep1.Reset();
                TimerToSleep1.Start();

                // Process Commands sent from Client
                for (var i = 0; i < clientToProcessCount; i++)
                {
                    // 6/7/2010 - Updated to use TryDequeue.
                    if(!commandsForClientToProcess.TryDequeue(out command)) continue;

                    switch (command.NetworkCommand)
                    {
                        case NetworkCommands.AddSceneItem:
                            var commAddSceneItem = (RTSCommAddSceneItem)command;
                            {
                                // TODO: Why does this particular command become corrupt from Xbox as server?
                                // 6/9/2010 - Check if Playernumber outside valid range.
                                if (commAddSceneItem.PlayerNumber > 1)
                                {
                                    Debug.WriteLine("Client process of 'AddSceneItem' has an invalid Playernumber value of " + commAddSceneItem.PlayerNumber);
                                    continue;
                                }
                                // Add SceneItem    
                                var e = new ItemCreatedArgs(commAddSceneItem.BuildingType,
                                                            commAddSceneItem.ProductionType,
                                                            commAddSceneItem.ItemType,
                                                            commAddSceneItem.ItemGroupToAttack,
                                                            commAddSceneItem.AtPosition,
                                                            commAddSceneItem.BuildingNetworkItemNumber, null,
                                                            gameTime.TotalGameTime.TotalSeconds)
                                            {
                                                IsBotHelper = commAddSceneItem.IsBotHelper,
                                                LeaderUniqueNumber = commAddSceneItem.LeaderNetworkItemNumber,
                                                LeaderPlayerNumber = commAddSceneItem.LeaderPlayerNumber,
                                            };

                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(commAddSceneItem.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    Player.AddSceneItem(player, commAddSceneItem.NetworkItemNumber, e);

                                // 5/13/2009 - Return to MemoryPool
                                commAddSceneItem.ReturnItemToPool();
                            }

                            break;
                        case NetworkCommands.UnitMoveOrder:
                            var commMoveItem = (RTSCommMoveSceneItem2) command;

                            // Set Move Data
                            // 12/17/2008 - Search SelectableItems Dict 
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(commMoveItem.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                        commMoveItem.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // 12/15/2008 - Update Interpolation.
                                        if (sceneItemToUpdate != null) sceneItemToUpdate.UpdateInterpolation = true;

                                        // 1/19/2009 - Push MoveTo Node into SolutionQueue.                    
                                        var moveToPos = new Vector3
                                                            {
                                                                X = commMoveItem.MoveToPos.X,
                                                                Y = 0,
                                                                Z = commMoveItem.MoveToPos.Y
                                                            };
                                        // 4/6/2009 - Threadlock
                                        if (sceneItemToUpdate != null)
                                            sceneItemToUpdate.AStarItemI.SolutionFinal.Enqueue(moveToPos); // 6/9/2010 - LocklessQueue

                                        _packetSendTime = commMoveItem.SendTime;
                                        // Used for Prediction in client Update.                            
                                    }
                            }

                            // 5/13/2009 - Return to MemoryPool
                            commMoveItem.ReturnItemToPool();


                            break;
                        case NetworkCommands.StartAttackSceneItem: // 11/20/2008; 1/15/2009- Updated to start attack
                            var startAttackItem = (RTSCommStartAttackSceneItem)command;
                            {
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(startAttackItem.SceneItemAttackerPlayerNumber, out player))
                                    break;
                                
                                // Set Attack Order
                                if (player != null)
                                    Player.UnitsStartAttackItemOrder(player, startAttackItem);

                                // 5/13/2009 - Return to MemoryPool
                                startAttackItem.ReturnItemToPool();
                            }

                            break;

                        case NetworkCommands.SyncTime: // 12/2/2008
                            var syncTime = (RTSCommSyncTime) command;
                            // 2nd - Store syncTime value from Server into the 'NetworkGameSyncer' class.
                            NetworkGameSyncer.ServerAvgTurnSpeed = syncTime.AvgTurnSpeedTime;
                            // 5/13/2009 - Return to MemoryPool
                            syncTime.ReturnItemToPool();
                            break;
                        case NetworkCommands.DelayTime: // 12/2/2008 (Client only)
                            var delayTime = (RTSCommDelayTime) command;
                            // 2nd - Store DelayTime value from Server into the 'NetworkGameSyncer' class.
                            NetworkGameSyncer.FrameSyncDelay = delayTime.DelayTime;
                            NetworkGameSyncer.CommunicationTurnTime = delayTime.CommunicationTime;
                            // 5/13/2009 - Return to MemoryPool
                            delayTime.ReturnItemToPool();
                            break;
                        case NetworkCommands.GameTurn: // 12/17/2008
                            var gameTurn = (RTSCommGameTurn) command;
                            // Tell NetworkGameSyncer GameTurn arrived.
                            NetworkGameSyncer.SetServerStartedGameTurn(gameTurn.CommunicationTurnNumber);
                            // 5/13/2009 - Return to MemoryPool
                            gameTurn.ReturnItemToPool();
                            break;
                        case NetworkCommands.IsReady: // 12/2/2008
                            var isReady = (RTSCommIsReady) command;
                            // 2nd - Store 'IsReady' value into TerrainScreen class.
                            TerrainScreen.ServerIsReady = isReady.IsReadyToStart;
                            // 5/13/2009 - Return to MemoryPool
                            isReady.ReturnItemToPool();
                            break;
                        case NetworkCommands.KillSceneItem: // 12/10/2008
                            var killSceneItem = (RTSCommKillSceneItem) command;
                            // 2nd - Find SceneItemOwner and call KillSceneItem method.                      

                            // 12/17/2008 - Search SelectableItems Dict   
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(killSceneItem.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.
                                        TryGetValue(killSceneItem.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // Found SceneItemOwner, so Kill SceneItemOwner and break out of loop
                                        var elapsedGameTime = gameTime.ElapsedGameTime;
                                        if (sceneItemToUpdate != null)
                                            sceneItemToUpdate.StartKillSceneItem(ref elapsedGameTime,
                                                                                 killSceneItem.AttackerPlayerNumber);
                                    }
                            }

                            // 5/13/2009 - Return to MemoryPool
                            killSceneItem.ReturnItemToPool();

                            break;
                        case NetworkCommands.LobbyData: // 4/9/2009
                            var lobbyData = (RTSCommLobbyData) command;

                            Lobby2Screen.Network_SetGamerInfo(lobbyData, NetworkSession);

                            // 5/13/2009 - Return to MemoryPool
                            lobbyData.ReturnItemToPool();

                            break;
                        case NetworkCommands.LobbyData_UserNotReady: // 9/1/2009

                            var lobbyData2 = (RTSCommLobbyData) command;

                            LobbyScreen.SetLocalGamersToNotReady(NetworkSession);

                            // Return to MemoryPool
                            lobbyData2.ReturnItemToPool();


                            break;
                        case NetworkCommands.SceneItemStance: // 6/2/2009
                            var sceneItemStance = (RTSCommSceneItemStance) command;

                            // Search SelectableItems Dict 
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(sceneItemStance.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                        sceneItemStance.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // 4/20/2010 - Cache
                                        if (sceneItemToUpdate != null)
                                        {
                                            // Found SceneItemOwner, so set Stance
                                            sceneItemToUpdate.DefenseAIStance = sceneItemStance.DefenseAIStance;
                                            // 6/2/2009 - Set the AIOrderIssued enum to 'None'.
                                            sceneItemToUpdate.AIOrderIssued = AIOrderType.None;
                                        }
                                    }
                            }

                            // Return to PoolManager
                            sceneItemStance.ReturnItemToPool();

                            break;
                        case NetworkCommands.CeaseAttackSceneItem: // 6/2/2009
                            var ceaseAttackSceneItem = (RTSCommCeaseAttackSceneItem) command;

                            // Search SelectableItems Dict
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(ceaseAttackSceneItem.SceneItemAttackerPlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.TryGetValue(
                                            ceaseAttackSceneItem.SceneItemAttackerNetworkNumber, out sceneItemToUpdate))
                                    {
                                        // 4/20/2010 - Cache
                                        if (sceneItemToUpdate != null)
                                        {
                                            // Found SceneItemOwner, so stop any attack orders!
                                            sceneItemToUpdate.AttackOn = false;
                                            sceneItemToUpdate.AttackSceneItem = null;
                                        }
                                    }
                            }

                            // Return to MemoryPool
                            ceaseAttackSceneItem.ReturnItemToPool();

                            break;
                        case NetworkCommands.SceneItemHealth: // 8/3/2009
                            var sceneItemHealth = (RTSCommSceneItemHealth) command;

                            // Search SelectableItems Dict  
                            {
                                SceneItemWithPick sceneItemToUpdate; // 10/19/2009
                                
                                // 6/15/2010 - Updated to use new GetPlayer method.
                                Player player;
                                if (!TemporalWars3DEngine.GetPlayer(sceneItemHealth.PlayerNumber, out player))
                                    break;

                                if (player != null)
                                    if (player.SelectableItemsDict.
                                        TryGetValue(sceneItemHealth.NetworkItemNumber, out sceneItemToUpdate))
                                    {
                                        // 4/20/2010
                                        if (sceneItemToUpdate != null)
                                        {
                                            // Found SceneItemOwner, so set health request.
                                            sceneItemToUpdate.StartSelfRepair = sceneItemHealth.StartSelfRepair;
                                            sceneItemToUpdate.CurrentHealth = sceneItemHealth.Health;
                                        }
                                    }
                            }

                            // Return to MemoryPool
                            sceneItemHealth.ReturnItemToPool();

                            break;
                        default:
                            break;
                    } // End Switch

                    // 8/26/2009 - Sleep every few ms.
                    if (TimerToSleep1.Elapsed.TotalMilliseconds >= 40)
                    {
                        Thread.Sleep(0);
                        TimerToSleep1.Reset();
                        TimerToSleep1.Start();
                    }

                } // End For Loop           

                // 11/7/2009: Discovered the 'ElapsedMS' is always zero in the StopWatch; therefore, the 'TotalMilliseconds' is used.
                // 12/22/2008 - Done processing this Gameturns batch of commands, so calc Time it took.
                StopWatchClientProcessBatch.Stop();
                NetworkGameSyncer.SetClientCommandProcessingTime((float)StopWatchClientProcessBatch.Elapsed.TotalMilliseconds);

                
            }
//#pragma warning disable 168
            /*catch (NullReferenceException err)
#pragma warning restore 168
            {
                Debug.WriteLine("ClientProcessInputFromServer method, of NetworkGameComponent, threw NullRefExp.");
                Debugger.Break();
            }*/
        }

        #region Dispose

        // 9/14/2008 - Dispose of objects
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the GameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 5/3/2009
                if (_packetReader != null)
                    _packetReader.Close();

                if (_packetWriter != null)
                    _packetWriter.Close();

                // let's shutdown our thread if it hasn't
                // shutdown already
                _isStopping = true;
                if (_gameNetworkingThread != null)
                {
                    _gameNetworkingThread.Join(); // wait for the thread to shutdown
                    _gameNetworkingThread.Abort(); // Terminate the Thread.                    
                    _gameNetworkingThread = null;
                }
                if (_gameNetworkingProcessThread != null)
                {
                    ProcessCommandsThreadStart.Set();
                    _gameNetworkingProcessThread.Join(); // wait for the thread to shutdown
                    _gameNetworkingProcessThread.Abort(); // Terminate the Thread.                    
                    _gameNetworkingProcessThread = null;
                }

                // Dispose
                _packetReader = null;
                _packetWriter = null;

                // 6/7/2010 - LocklessQueue does NOT have a Clear; therefore, need to check if 'IsEmpty' and
                //            safely try to dequeue remaining items.
                RTSCommand command;
                if (CommandsForClient != null)
                {
                    //CommandsForClient.Clear();
                    while(!CommandsForClient.IsEmpty)
                    {
                        CommandsForClient.TryDequeue(out command);
                    }
                }
                if (CommandsForServer != null)
                {
                    //CommandsForServer.Clear();
                    while (!CommandsForServer.IsEmpty)
                    {
                        CommandsForServer.TryDequeue(out command);
                    }
                }
                if (CommandsForClientToProcess != null)
                {
                    //CommandsForClientToProcess.Clear();
                    while (!CommandsForClientToProcess.IsEmpty)
                    {
                        CommandsForClientToProcess.TryDequeue(out command);
                    }
                }
                if (CommandsForServerToProcess != null)
                {
                    //CommandsForServerToProcess.Clear();
                    while (!CommandsForServerToProcess.IsEmpty)
                    {
                        CommandsForServerToProcess.TryDequeue(out command);
                    }
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}