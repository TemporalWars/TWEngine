#region File Description
//-----------------------------------------------------------------------------
// NetworkGameSyncer.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.rtsCommands;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace ImageNexus.BenScharbach.TWEngine.Networking
{
    // 12/2/2008
    /// <summary>
    /// The <see cref="NetworkGameSyncer"/> primary goal is to keep two network gamers in sync.
    /// This is accomplished by keeping an average 'ElapsedTime', over so many 'GameTurns'. 
    /// Then to stay in sync, either the server or client computers, which ever is the fastest,
    /// is slowed down with calculated delays.
    /// </summary>
    internal class NetworkGameSyncer
    {
        // 2/27/2009 - Game Ref

        // 12/22/2008 - Client Batch Processing Commands ElapsedTime Avg     
        private static bool _clientCompletedGameTurn = true;
        private static AveragerForFloats _clientProcessingTimeAvg;
        private static int _communicationTurnNumber = 1;
        private static float _communicationTurnTime = 20.0f;
        private static float _oldCommunicationTurnTime = 20.0f; // 8/11/2009
        private static int _fpsFrameTargetValue = 40; // 40ms = 25 FPS for MP games as default.
        private static int _oldFrameSyncDelay; // 1/19/2009
        private const float CommunicationTurnTimeMinimum = 200.0f; // 11/11/09

        // 1/2/2008 - Server Batch Processing Commands ElapsedTime Avg        
        private static AveragerForFloats _serverProcessingTimeAvg;
        
        // 12/17/2008 - Server started GameTurn
// ReSharper disable UnaccessedField.Local
        private static bool _serverStartedGameTurn = true;
// ReSharper restore UnaccessedField.Local
        private static AveragerForFloats _clientCompletedGameTurnAvg;

        // 12/16/2008 - 
        /// <summary>
        /// Client latency
        /// </summary>
        internal static float ClientLatency;
        private static int _counter;
        private static bool _enabled;
        private static AveragerForFloats _frameTimeWithoutDelayAvg;
        private static readonly Stopwatch StopWatchClientCompletedGameTurn = new Stopwatch();

        private readonly AveragerForFloats _frameTimeAvg;
        private readonly Game _game;
        private readonly LocalNetworkGamer _hostNetworkGamer;
        private readonly NetworkSession _networkSession;
        private float _accumElapsedTime;
// ReSharper disable UnaccessedField.Local
        private LocalNetworkGamer _clientNetworkGamer;// 
// ReSharper restore UnaccessedField.Local

        private float _communicationTurnElapsedTime;
// ReSharper disable UnaccessedField.Local
        private bool _gamerLeft;
// ReSharper restore UnaccessedField.Local
// ReSharper disable UnaccessedField.Local
        private NetworkGameComponent _networkGameComponent;
// ReSharper restore UnaccessedField.Local


        private int _framesSinceLastTurn; // # of Frames between _game turns.
        private readonly Stopwatch _gameSlowStopWatch = new Stopwatch();

        // wait 45 seconds before testing for GameSlow check!
        private TimeSpan _startGameSlowCheck = TimeSpan.FromSeconds(45);
        private static readonly TimeSpan TimeSpanZero = TimeSpan.Zero; // 6/18/2010

        #region Properties

        /// <summary>
        /// Returns the host <see cref="LocalNetworkGamer"/>.
        /// </summary>
        public LocalNetworkGamer HostNetworkGamer
        {
            get { return _hostNetworkGamer; }
        }

        /// <summary>
        /// Get or Set the clients average turn speed.
        /// </summary>
        public static float ClientAvgTurnSpeed { get; set; }

        /// <summary>
        /// Get or Set the servers average turn speed.
        /// </summary>
        public static float ServerAvgTurnSpeed { get; set; }

        /// <summary>
        /// Delay set to fastest computer, to keep it in sync with slower computer.
        /// </summary>
        public static int FrameSyncDelay { get; set; }

        /// <summary>
        /// Client completed GameTurn?
        /// </summary>
        public static bool ClientCompletedGameTurn
        {
            get { return _clientCompletedGameTurn; }
            set { _clientCompletedGameTurn = value; }
        }

        /// <summary>
        /// The time elapsed for each game turn.
        /// </summary>
        public static float CommunicationTurnTime
        {
            get { return _communicationTurnTime; }
            set { _communicationTurnTime = value; }
        }

        /// <summary>
        /// The turn number currently being processed!
        /// </summary>
        public static int CommunicationTurnNumber
        {
            get { return _communicationTurnNumber; }
        }

        /// <summary>
        /// Communication turns for client
        /// </summary>
        public static int ClientCommunicationTurnNumber { get; set; }

        /// <summary>
        /// Client batch processing commands ElapsedTime average. 
        /// </summary>
        public static float ClientProcessingTimeAvg
        {
            get
            {
                // 2/27/2009
                return _clientProcessingTimeAvg != null ? _clientProcessingTimeAvg.CurrentAverage : 0;
            }
        }

        // 2/27/2009
        /// <summary>
        /// Client completed game turn average.
        /// </summary>
        public static float ClientCompletedGameTurnAvg
        {
            get 
            {
                return _clientCompletedGameTurnAvg != null ? _clientCompletedGameTurnAvg.CurrentAverage : 0;
            }
        }

        /// <summary>
        /// Clients frame time average
        /// </summary>
        public static float ClientFrameTimeAvg { get; private set; }

        /// <summary>
        /// Servers frame time average
        /// </summary>
        public static float ServerFrameTimeAvg { get; private set; }

        /// <summary>
        /// Set to enable use of this <see cref="NetworkGameSyncer"/>.
        /// </summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;

                // 12/10/2008
                ClientFrameTimeAvg = 0;
                ServerFrameTimeAvg = 0;
            }
        }

        /// <summary>
        /// Server processing average time.
        /// </summary>
        public static float ServerProcessingTimeAvg
        {
            get { return _serverProcessingTimeAvg != null ? _serverProcessingTimeAvg.CurrentAverage : 0; }
        }

        #endregion

        // 12/10/2008 - Enable/Disable Updating

        // Constructor
        /// <summary>
        /// Constructor for the <see cref="NetworkGameSyncer"/> class, which
        /// initializes the internal <see cref="AveragerForFloats"/> instances.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="networkSession"><see cref="NetworkSession"/> instance</param>
        public NetworkGameSyncer(Game game, NetworkSession networkSession)
        {
            // 2/27/2009 - Store Game Ref
            _game = game;

            _networkSession = networkSession;

            // 2/27/2009 - Attach EventHandler for the GamerLeft Event.
            _networkSession.GamerLeft += NetworkSessionGamerLeft;

            // Set _game to use Fix Lock frame method.
            game.IsFixedTimeStep = true;

            // 1/12/2009 - Instantiate the AveragerForFloats instances
            _clientProcessingTimeAvg = new AveragerForFloats(100);
            _serverProcessingTimeAvg = new AveragerForFloats(100);
            _clientCompletedGameTurnAvg = new AveragerForFloats(100);
            _frameTimeWithoutDelayAvg = new AveragerForFloats(100);
            _frameTimeAvg = new AveragerForFloats(100);

            // 12/12/2008 - Get Ref to NetworkGameComponent Service
            _networkGameComponent = (NetworkGameComponent) game.Services.GetService(typeof (NetworkGameComponent));

            // 12/12/2008 - Store Ref to Server/Client LocalGamers
            var count = networkSession.LocalGamers.Count;
            for (var loop1 = 0; loop1 < count; loop1++)
            {
                // 4/20/2010 - Cache
                var clientNetworkGamer = networkSession.LocalGamers[loop1];
                if (clientNetworkGamer == null) continue;

                if (clientNetworkGamer.IsHost)
                {
                    _hostNetworkGamer = clientNetworkGamer;
                }
                else
                    _clientNetworkGamer = clientNetworkGamer;
            }
        }

        /// <summary>
        /// Default empty constructor for <see cref="NetworkGameSyncer"/>.
        /// </summary>
        public NetworkGameSyncer() : this(TemporalWars3DEngine.GameInstance, null)
        {
            return;
        }


        // 2/27/2009
        /// <summary>
        /// Captures if GamerLeft, and if so, calls the <see cref="NetworkSessionComponent.LeaveSession"/> method, to allow host to return to menus.
        /// </summary>
        private void NetworkSessionGamerLeft(object sender, GamerLeftEventArgs e)
        {
            // Get ScreenManager
            var screenManager = (ScreenManager) _game.Services.GetService(typeof (ScreenManager));

            // Set gameLeft
            _gamerLeft = true;

            // Leave Network Session
            NetworkSessionComponent.LeaveSession(screenManager);
        }


        /// <summary>
        /// Updates average elapsed <see cref="GameTime"/> for both Client/Server.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {
            // 12/10/2008 - Is it _enabled?
            DoUpdate(this, gameTime);
        }

        // 6/18/2010
        /// <summary>
        /// Method helper, which updates average elapsed <see cref="GameTime"/> for both Client/Server.
        /// </summary>
        /// <param name="networkGameSyncer">this instance of <see cref="NetworkGameSyncer"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DoUpdate(NetworkGameSyncer networkGameSyncer, GameTime gameTime)
        {
            if (!_enabled) return;

            // Increase 'AccumElapsedTime' by current ElapsedTime.
            var elapsedGameTime = gameTime.ElapsedGameTime; // 6/18/2010 - Cache
            networkGameSyncer._accumElapsedTime += (float)elapsedGameTime.TotalMilliseconds;

            // 12/12/2008
            networkGameSyncer._communicationTurnElapsedTime += (float)elapsedGameTime.TotalMilliseconds;

            networkGameSyncer._framesSinceLastTurn++;

            // 3/12/2009 - Check if Game RunningSlowly?
            if (networkGameSyncer._startGameSlowCheck > TimeSpanZero)
            {
                networkGameSyncer._startGameSlowCheck -= elapsedGameTime;
            }
            else
            {
                CheckIfGameRunningSlowly(networkGameSyncer, gameTime);
            }

            // 12/17/2008
            //CommunicationTurnCheck_V1();
            CommunicationTurnCheck_V2(networkGameSyncer, gameTime);

            // 1/19/2009
#if DEBUG
            UpdateFrameTimeWithoutDelayAvg(FPS.Fps);
#endif
            if (FrameSyncDelay != _oldFrameSyncDelay)
            {
                TemporalWars3DEngine.GameInstance.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, FrameSyncDelay);
            }
            _oldFrameSyncDelay = FrameSyncDelay;
        }

        // 1/2/2009; 1/12/2009: Updated to use Averager class.
        private static void UpdateFrameTimeWithoutDelayAvg(float frameTimeWithoutDelay)
        {
            _frameTimeWithoutDelayAvg.Update(frameTimeWithoutDelay);
        }


        // 12/17/2008
        // This version uses Avg times, and Frame Delays, in order to stay in Sync.
/*
        private void CommunicationTurnCheck_V1()
        {
            // 12/12/2008: Updated to use the communicationTurn elapsed Time method
            // If max frames hit per _game-turn, then update Average.
            //if (framesSinceLastSend >= framesBetweenTurn)
            if (_communicationTurnElapsedTime >= _communicationTurnTime)
            {
                // 12/12/2008 - Reset Time
                _communicationTurnElapsedTime = 0;

                // Calc Avg by dividing by the framesSinceLastSend.
                _accumElapsedTime = _accumElapsedTime/_framesSinceLastTurn; // Was framesBetweenTurn

                // Update Averager                
                _frameTimeAvg.Update(_accumElapsedTime);


                // Update the static Time variables
                if (NetworkSession.IsHost)
                {
                    // this is server, so determine delay
                    ServerAvgTurnSpeed = _frameTimeAvg.CurrentAverage; // Store our own Time for ref            

                    // 1/2/2009
                    SetServerCompletedGameTurn(_frameTimeWithoutDelayAvg.CurrentAverage);

                    // 12/12/2008 - Tell NetworkGameComponent to start next GameTurn
                    NetworkGameComponent.SendPacketThisFrame = true;
                }
                else
                {
                    // 6/29/2009 - Send GameTurn done to server.            
                    RTSCommGameTurn gameTurn = null;
                    PoolManager.GetNode(out gameTurn);

                    gameTurn.Clear();
                    gameTurn.NetworkCommands = NetworkCommands.GameTurn;
                    gameTurn.FrameTimeAvg = _frameTimeAvg.CurrentAverage; // 12/17/2008
                    gameTurn.FrameTimeWithoutDelayAvg = _frameTimeWithoutDelayAvg.CurrentAverage;
                    gameTurn.CommunicationTurnNumber = _communicationTurnNumber; // 12/17/2008
                    gameTurn.ClientProcessingTime = _clientProcessingTimeAvg.CurrentAverage; // 12/22/2008
                    gameTurn.Latency = ClientLatency; // 12/22/2008
                    NetworkGameComponent.AddCommandsForServerG(gameTurn);
                }

                // Clear Values
                _accumElapsedTime = 0;
                _framesSinceLastTurn = 0;

                // 12/16/2008 - Increase CommunicationTurn
                _communicationTurnNumber++;
            } // End If GameTurn done.
        }
*/


        // 12/17/2008; 6/18/2010: Updated to be STATIC method.
        // This version only advances when GameTurn is received
        // from other computer.
        /// <summary>
        /// Checks if time to advance to next communication turn.
        /// </summary>
        /// <param name="networkGameSyncer">this instance of <see cref="NetworkGameSyncer"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
// ReSharper disable UnusedParameter.Local
        private static void CommunicationTurnCheck_V2(NetworkGameSyncer networkGameSyncer, GameTime gameTime)
// ReSharper restore UnusedParameter.Local
        {
            
            // 11/10/2009 - Let's track the avg Time between calls, to know how
            //              long it is taking the client to finish each turn.
            {
                StopWatchClientCompletedGameTurn.Stop();
                var mostRecentValue = StopWatchClientCompletedGameTurn.Elapsed.Milliseconds; // 6/10/2010
                if (mostRecentValue > 0) // was ElpaedMilliseconds
                {
                    // 11/10/2009 - Updated to use 'Elapsed.Milliseconds', and not 'ElapsedMilliseconds'.
                    _clientCompletedGameTurnAvg.Update(mostRecentValue);
                }
                // Start measuring Time for new gameTurn
                StopWatchClientCompletedGameTurn.Reset();
                StopWatchClientCompletedGameTurn.Start();
            }

            // If max frames hit per _game-turn, then update Average.          
            if (networkGameSyncer._communicationTurnElapsedTime < _communicationTurnTime) return;

            // Reset Time
            networkGameSyncer._communicationTurnElapsedTime = 0;

            // Calc Avg by dividing by the framesSinceLastSend.
            networkGameSyncer._accumElapsedTime = networkGameSyncer._accumElapsedTime 
                / networkGameSyncer._framesSinceLastTurn; // Was framesBetweenTurn

            // Update Averager               
            networkGameSyncer._frameTimeAvg.Update(networkGameSyncer._accumElapsedTime);

            // Update the static Time variables
            if (networkGameSyncer._networkSession != null)
                if (networkGameSyncer._networkSession.IsHost)
                {
                    // this is server, so determine delay
                    ServerAvgTurnSpeed = networkGameSyncer._frameTimeAvg.CurrentAverage; // Store our own Time for ref 

                    // Wait until Client finish GameTurn.
                    /*while (!_clientCompletedGameTurn)
                    {
                        // Pump Network Process Queue
                        _networkGameComponent.ServerProcessInputFromClient(_hostNetworkGamer);

                        NetworkSession.Update();
                        
                        // 2/27/2009 - keeps from being in an endless loop if gamer leaves session!
                        if (_gamerLeft)
                            break;
                        
                    }
                    _clientCompletedGameTurn = false;*/

                    // Adjust Frame Delay
                    SetServerCompletedGameTurn(_frameTimeWithoutDelayAvg.CurrentAverage);

                    // 12/16/2008 - Increase CommunicationTurn
                    _communicationTurnNumber++;
                    

                    // 6/29/2009 - Send Start GameTurn to client; simply guarantees the client will process at
                    //              least 1 command to trigger client completed _game turn message for server!                                
                    RTSCommGameTurn gameTurn;
                    PoolManager.GetNode(out gameTurn);

                    // 4/20/2010 - Check if null.
                    if (gameTurn != null)
                    {
                        gameTurn.Clear();
                        gameTurn.NetworkCommand = NetworkCommands.GameTurn;
                        gameTurn.ClientCompletedGameTurnAvg = 0;
                        gameTurn.FrameTimeWithoutDelayAvg = 0;
                        gameTurn.CommunicationTurnNumber = _communicationTurnNumber;
                        // Let client know new commTurnNumber.                   
                        NetworkGameComponent.AddCommandsForClientG(gameTurn);
                    }

                    // 12/12/2008 - Tell NetworkGameComponent to start next GameTurn and send next 
                    //              batch of commands to client.
                    NetworkGameComponent.SendPacketThisFrame = true;
                }
                else
                {
                    // Wait until Client completes processing of commands from server.
                    /*while (!_clientCompletedGameTurn)
                    {
                        // Pump Network Process Queue
                        NetworkGameComponent.ClientProcessInputFromServer(gameTime);

                        // sleep a ms
                        Thread.Sleep(1);
                    }
                    _clientCompletedGameTurn = false;*/



                    // 6/29/2009 - Send GameTurn done to server.                   
                    RTSCommGameTurn gameTurn;
                    PoolManager.GetNode(out gameTurn);

                    // 4/20/2010 - Check if null
                    if (gameTurn != null)
                    {
                        gameTurn.Clear();
                        gameTurn.NetworkCommand = NetworkCommands.GameTurn;
                        gameTurn.ClientCompletedGameTurnAvg = _clientCompletedGameTurnAvg.CurrentAverage; // 11/10/09
                        gameTurn.FrameTimeWithoutDelayAvg = _frameTimeWithoutDelayAvg.CurrentAverage;
                        gameTurn.CommunicationTurnNumber = _communicationTurnNumber; // 12/17/2008
                        gameTurn.ClientProcessingTime = _clientProcessingTimeAvg.CurrentAverage; // 12/22/2008
                        gameTurn.Latency = ClientLatency; // 12/22/2008
                        NetworkGameComponent.AddCommandsForServerG(gameTurn);
                    }

                    // 11/5/2009 - Tell NetworkGameComponent to start next GameTurn and send next 
                    //              batch of commands to server.
                    NetworkGameComponent.SendPacketThisFrame = true;

                    //_serverStartedGameTurn = false;                   
                }

            // Clear Values
            networkGameSyncer._accumElapsedTime = 0;
            networkGameSyncer._framesSinceLastTurn = 0;
        }

        // 12/3/2008
        /// <summary>
        /// Calculates the client's <paramref name="gameTurn"/> average, using the
        /// differences between each <paramref name="gameTurn"/>.  This is used as an
        /// additional delay for the server to stay in sync.
        /// </summary>
        /// <param name="gameTurn"><see cref="RTSCommGameTurn"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameTurn"/> is null.</exception>
        public static void SetClientCompletedGameTurn(RTSCommGameTurn gameTurn)
        {
            // 4/20/2010 - Check if given param is null
            if (gameTurn == null)
                throw new ArgumentNullException("gameTurn", @"Given parameter cannot be null!");

            // Set Completed GameTurn
            _clientCompletedGameTurn = true;

            // Store Client's Avg FrameTime Without Delay.
            ClientFrameTimeAvg = gameTurn.FrameTimeWithoutDelayAvg;

            // Store Client's CommunicationTurn
            ClientCommunicationTurnNumber = gameTurn.CommunicationTurnNumber;
            // Store Client's Processing Batch Time
            _clientProcessingTimeAvg.Update(gameTurn.ClientProcessingTime);
            // Store Client's Latency
            ClientLatency = gameTurn.Latency;

            // 11/10/2009 - Store client GameTurn avg.
            _clientCompletedGameTurnAvg.Update(gameTurn.ClientCompletedGameTurnAvg);
           
        }

        // 12/22/2008
        /// <summary>
        /// Calculates the client's processing batch commands max <see cref="GameTime"/>.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time</param>
        public static void SetClientCommandProcessingTime(float elapsedTime)
        {
            // Update the ClientProcessingTime Averages
            if (_clientProcessingTimeAvg != null) // 2/21/2009
                _clientProcessingTimeAvg.Update(elapsedTime);
        }

        // 1/2/2009
        /// <summary>
        /// Calculates the server's processing batch commands Max <see cref="GameTime"/>.
        /// </summary>
        /// <param name="elapsedTime">Elapsed time</param>
        public static void SetServerCommandProcessingTime(float elapsedTime)
        {
            // Update the serverProcessingTime Averages
            if (_serverProcessingTimeAvg != null) // 2/21/2009
                _serverProcessingTimeAvg.Update(elapsedTime);
        }

        // 12/17/2008
        /// <summary>
        /// Server set when new communication turn started.
        /// </summary>
        /// <param name="commTurnNumber">New communication number</param>
        public static void SetServerStartedGameTurn(int commTurnNumber)
        {
            // Set Server Start GameTurn
            _serverStartedGameTurn = true;
            // Set new CommunicationTurn number
            _communicationTurnNumber = commTurnNumber;
        }
       
        // 12/3/2008
        /// <summary>
        /// Calculates the server's game turn average, using the
        /// differences between each 'gameTime'.  This is used as an
        /// additional delay for the Server to stay in Sync.
        /// </summary>
        /// <param name="frameTimeWithoutDelay">Enter frame time, minus the delay</param>
        public static void SetServerCompletedGameTurn(float frameTimeWithoutDelay)
        {
            // Reset Client flag to false.
            _clientCompletedGameTurn = false;
            // Store the Server's FrameTime Without Delay.
            ServerFrameTimeAvg = frameTimeWithoutDelay;
            // Set Server Delay
            FrameSyncDelay = _fpsFrameTargetValue;

            // 12/22/2008 - Calc current Communication Turn Length, and send to client if necessary.
            SetCommunicationTurnLength();

            // 2/27/2009 - Compare CommunicationTurns between Client/Server to see if extra delay needed?
            if (ClientCommunicationTurnNumber < _communicationTurnNumber)
            {
                // Then server going too fast, need to slow down a bit; using 5 ms for every 1 turn diff!
                var additionalDelay = (5*(_communicationTurnNumber - ClientCommunicationTurnNumber));

                FrameSyncDelay += additionalDelay;

                // 2/27/2009 - No more than 200 ms; which would be 5 FPS!
                if (FrameSyncDelay > 200)
                    FrameSyncDelay = 200;
            }
            else if (_communicationTurnNumber < ClientCommunicationTurnNumber)
            {
                // Then client going too fast, need to slow down a bit; using 5 ms for every 1 turn diff!
                var additionalDelay = (5*(ClientCommunicationTurnNumber - _communicationTurnNumber));

                // Create Client command   
                // 2/27/2009 - No more than 200 ms; which would be 5 FPS!
                SendClientDelayTime(_fpsFrameTargetValue + additionalDelay > 200 ? 200 : _fpsFrameTargetValue + additionalDelay);

            }
            
        }

        // 12/22/2008; 1/2/2009: Updated to use the 'serverProcessingTime'.
        /// <summary>
        /// Using the client latency and client's batch processing time,
        /// this <see cref="SetCommunicationTurnLength"/> method on the server 
        /// side will determine what the communication turn delay should be.
        /// </summary>
        private static void SetCommunicationTurnLength()
        {
            // Set Comm Turn Time to client's Latency Time with
            // the greater processing Time of the client or server.
            if (_clientProcessingTimeAvg.CurrentAverage > _serverProcessingTimeAvg.CurrentAverage)
            {
                _communicationTurnTime = ClientLatency + _clientProcessingTimeAvg.CurrentAverage*2;
            }
            else
                _communicationTurnTime = ClientLatency + _serverProcessingTimeAvg.CurrentAverage*2;

            // Make 250ms min.
            if (_communicationTurnTime < CommunicationTurnTimeMinimum)
                _communicationTurnTime = CommunicationTurnTimeMinimum;


            // 12/30/2008 - If clientCompletedGameTurnAvgTime value is larger, then use it.
            if (_communicationTurnTime < _clientCompletedGameTurnAvg.CurrentAverage)
                _communicationTurnTime = _clientCompletedGameTurnAvg.CurrentAverage;

            // 8/11/2009 - ONLY send update to client, when value changes!
            if (_oldCommunicationTurnTime == _communicationTurnTime) return;

            // Create Client command         
            SendClientDelayTime(_fpsFrameTargetValue);

            // Set old value
            _oldCommunicationTurnTime = _communicationTurnTime;
        }

        // 8/11/2009
        /// <summary>
        /// Send the <see cref="RTSCommDelayTime"/>' command to the client, with the current
        /// <paramref name="frameTargetValue"/> given and the <see cref="_communicationTurnTime"/> in use.
        /// </summary>
        /// <param name="frameTargetValue">Frame target value</param>
        private static void SendClientDelayTime(int frameTargetValue)
        {
            RTSCommDelayTime delayTime;
            PoolManager.GetNode(out delayTime);

            // 4/20/2010
            if (delayTime == null) return;

            delayTime.Clear();
            delayTime.NetworkCommand = NetworkCommands.DelayTime;
            delayTime.CommunicationTime = _communicationTurnTime;

            // Set Client Delay
            delayTime.DelayTime = frameTargetValue;

            // Send new Delay command to Client
            NetworkGameComponent.AddCommandsForClientG(delayTime);
        }

        // 12/17/2008
        /// <summary>
        /// Compares ranges to determine the best FPS FrameTarget value
        /// for all computers.
        /// </summary>       
        private static void SetFpsFrameTargetValue()
        {
            // Sets new value to one of 6 values.            
            switch (_fpsFrameTargetValue)
            {
                case 40:
                    _fpsFrameTargetValue = 50; // 50ms = 20 fps              
                    break;
                case 50:
                    _fpsFrameTargetValue = 66; // 66ms = 15 fps
                    break;
            }

            // 8/11/2009 - Send Delay Command to client
            // Create Client command         
            SendClientDelayTime(_fpsFrameTargetValue);

            // If FPS between 55-60
            /*if (frameTurnAvg >= 55 && frameTurnAvg < 60)
                _fpsFrameTargetValue = 18; // Set to 55 fps (1000/55 = 18ms)

            // If FPS between 50-55
            if (frameTurnAvg >= 50 && frameTurnAvg < 55)
                _fpsFrameTargetValue = 20; // Set to 50 fps (1000/50 = 20ms)

            // If FPS between 45-50
            if (frameTurnAvg >= 45 && frameTurnAvg < 50)
                _fpsFrameTargetValue = 22; // Set to 45 fps (1000/45 = 22ms)

            // If FPS between 40-45
            if (frameTurnAvg >= 40 && frameTurnAvg < 45)
                _fpsFrameTargetValue = 25; // Set to 40 fps (1000/40 = 25ms)

            // If FPS between 35-40
            if (frameTurnAvg >= 35 && frameTurnAvg < 40)
                _fpsFrameTargetValue = 28; // Set to 35 fps (1000/35 = 28ms)

            // If FPS between 30-35
            if (frameTurnAvg >= 30 && frameTurnAvg < 35)
                _fpsFrameTargetValue = 33; // Set to 30 fps (1000/30 = 33ms)

            // If FPS between 25-30
            if (frameTurnAvg >= 25 && frameTurnAvg < 30)
                _fpsFrameTargetValue = 40; // Set to 25 fps (1000/25 = 40ms)

            // If FPS between 20-25
            if (frameTurnAvg >= 20 && frameTurnAvg < 25)
                _fpsFrameTargetValue = 50; // Set to 20 fps (1000/20 = 50ms)

            // If FPS between 15-20
            if (frameTurnAvg >= 15 && frameTurnAvg < 20)
                _fpsFrameTargetValue = 66; // Set to 15 fps (1000/15 = 66ms)

            // If FPS between 10-15
            if (frameTurnAvg >= 10 && frameTurnAvg < 15)
                _fpsFrameTargetValue = 100; // Set to 10 fps (1000/10 = 100ms)*/
        }

        // 3/12/2009
        /// <summary>
        /// Called for every update cycle, which then checks if the 
        /// <see cref="GameTime.IsRunningSlowly"/> flag is true.
        /// </summary>
        /// <param name="networkGameSyncer">this instance of <see cref="NetworkGameSyncer"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void CheckIfGameRunningSlowly(NetworkGameSyncer networkGameSyncer, GameTime gameTime)
        {
            // set to 10 seconds.
            const int gameSlowMaxTime = 10000;
            if (networkGameSyncer._networkSession == null) return;

            // 6/18/2010 - Try/Finally
            try
            {
                // Is Host?
                if (networkGameSyncer._networkSession.IsHost)
                {
                    if (gameTime.IsRunningSlowly)
                    {
                        // Start Stopwatch timer
                        if (!networkGameSyncer._gameSlowStopWatch.IsRunning)
                        {
                            networkGameSyncer._gameSlowStopWatch.Start();
                        }
                        else
                        {
                            // if running slowly for more than 10 seconds.
                            if (networkGameSyncer._gameSlowStopWatch.ElapsedMilliseconds > gameSlowMaxTime)
                            {
                                // since server, lets immediately slow both games down!
                                SetFpsFrameTargetValue();
                            }
                        } // Is stopwatch running?
                    }

                    return;
                }

                //
                // else, client;
                //

                if (gameTime.IsRunningSlowly)
                {
                    // Start Stopwatch timer
                    if (!networkGameSyncer._gameSlowStopWatch.IsRunning)
                    {
                        networkGameSyncer._gameSlowStopWatch.Start();
                    }
                    else
                    {
                        // if running slowly for more than 10 seconds.
                        if (networkGameSyncer._gameSlowStopWatch.ElapsedMilliseconds > gameSlowMaxTime)
                        {
                            // send rtsGameSlow command to server                       
                            RTSCommGameSlow gameSlowItem;
                            PoolManager.GetNode(out gameSlowItem);

                            // 4/20/2010
                            if (gameSlowItem != null)
                            {
                                gameSlowItem.Clear();
                                gameSlowItem.NetworkCommand = NetworkCommands.GameSlow;
                                gameSlowItem.IsGameRunningSlow = true;

                                NetworkGameComponent.AddCommandsForServerG(gameSlowItem);
                            }
                           
                        }
                    } // Is stopwatch running?
                }
            }
            finally
            {
                // reset timer
                if (!gameTime.IsRunningSlowly)
                {
                    networkGameSyncer._gameSlowStopWatch.Stop();
                    networkGameSyncer._gameSlowStopWatch.Reset();
                }
                
            }
            
        }

        // 3/12/2009
        /// <summary>
        /// Checks if game running slowly, which is done if client
        /// calls this more than 10 times.
        /// </summary>
        public static void GameRunningSlowlyOnClient()
        {
            _counter++;

            // if called 10 times, then let's set new _game speed.
            if (_counter <= 10) return;

            SetFpsFrameTargetValue();
            _counter = 0;
        }
    }
}