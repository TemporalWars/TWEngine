#region File Description
//-----------------------------------------------------------------------------
// AStarItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using AStarInterfaces.AStarAlgorithm;
using AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParallelTasksComponent.LocklessQueue;
using TWEngine.HandleGameInput;
using TWEngine.InstancedModels.Enums;
using TWEngine.Interfaces;
using TWEngine.MemoryPool;
using TWEngine.Networking;
using TWEngine.Players;
using TWEngine.rtsCommands;
using TWEngine.rtsCommands.Enums;
using TWEngine.SceneItems;
using TWEngine.SceneItems.Enums;
using TWEngine.Shapes;
using TWEngine.ForceBehaviors;
using TWEngine.Terrain;
using TWEngine.Utilities;


namespace TWEngine.Common
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.Common"/> namespace contains the common components
    /// which make up the entire <see cref="TWEngine"/>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    ///<summary>
    /// Pathfinding State arguments, used to pass important
    /// attributes to the event handler.
    ///</summary>
    public class UpdatePathFindingStateArgs : EventArgs
    {
        ///<summary>
        /// TimeSpan struct with current game elapsed time.
        ///</summary>
        public TimeSpan ElapsedTime;
        ///<summary>
        /// Current ItemStates enum.
        ///</summary>
        public ItemStates ItemState;
        ///<summary>
        /// TimeSpan struct with current game time.
        ///</summary>
        public TimeSpan Time;
    }

    ///<summary>
    /// PathFinding delegate, used for pathfinding events.
    ///</summary>
    public delegate void PathFindingEventHandler(object sender, UpdatePathFindingStateArgs e);

    /// <summary>
    /// The <see cref="AStarItem"/> item is an extension for the <see cref="SceneItem"/>, which provides
    /// pathfinding capabilities by using the A* component for searches.  
    /// Furthermore, the actual processing and lower level work for moving
    /// the units in the game world is accomplished in this class.
    /// </summary>
    public class AStarItem : IAStarItem
    {
        
        // 2/29/2008 - Add A* Manager Class
        private const float Friction = 1.0f;
        private static IAStarManager _aStarManager;

        // 3/1/2011 - Optimization timers, used to reduce the # of times a method is called.
        private const float BlockNodeCheckTimeReset = 5000f;
        private float _blockNodeCheckTime = BlockNodeCheckTimeReset;
        private const float FindAlternativeGoalCheckTimeReset = 3000f;
        private float _findAlternativeGoalCheckTime = FindAlternativeGoalCheckTimeReset;
        
        // 3/1/2011 - Set when some AltGoal position for attacking was found.
        private bool _foundAlternativeGoalAttackPosition;
        
        ///<summary>
        /// For Debug purposes, allows showing the PathNodes in the game world.
        ///</summary>
        public static bool ShowVisualPathNodes;
        
        ///<summary>
        /// For Debug purposes, allows showing the TestNodes in the game world.
        ///</summary>
        public static bool ShowVisualTestedNodes;

        // 10/10/2008
        private readonly AveragerForVector3 _forwardAveragerForVector3S;
        private readonly UpdatePathFindingStateArgs _pathFindingStateArgs;

        // 10/20/2009 - Used to measure the distance between 2 nodes.
        private const int PathNodeStrideX3 = TemporalWars3DEngine._pathNodeStride * 3;
        private readonly Vector2 _vector2Zero = Vector2.Zero;
        private readonly Vector2 _distanceToUse = new Vector2(PathNodeStrideX3, PathNodeStrideX3);
        private static float _distanceWithin3Nodes;

        //
        // 2/27/2008 - Add A* Parameters
        //
        // The destination to move the SceneItemOwner to.
        private Vector3 _goalPosition = Vector3Zero; // (MP) 
        private Vector3 _flagGoalPosition = Vector3Zero; // 5/29/2011
        private ItemStates _itemState = ItemStates.Resting;
        // The _tempGoal when Collision avoiding
        // Used when following Nodes set by A* pathfinding.
        private Vector3 _moveToPosition = Vector3Zero;
        /// <summary>
        /// Current PathNode Index unit is at in AstarGraph.
        /// </summary>
        internal Vector3 OccupiedAtIndex; // 1/21/2011 - Removed ? 'Null' setting.
        private Vector3? _oldTempGoal;
        private Vector3 _pathNodePosition = Vector3Zero;
        private float _pauseTime = 0.5f; // half-second default
        private const float UnpatientTimeMax = 50.0f; // 11/10/09
        private float _unpatientTime = UnpatientTimeMax; // 11/10/09 - 50 ms 'unpatient' time, until unit repaths!
// ReSharper disable UnaccessedField.Local
        private Vector3 _side = Vector3Zero;
// ReSharper restore UnaccessedField.Local
        private Vector3 _tempGoal = Vector3Zero;

        // Vertex array that stores exactly which triangle was picked.

        // DEBUG: List Array used to Visually Display A* Path.
        private TriangleShapeHelper _tShapeHelper;
        private List<VertexPositionColor> _visualPath;

        // DEBUG: List Array used to Visually Display the TestedNodes of A* Pass.
        private List<VertexPositionColor> _visualTestedNodes;
        private VertexPositionColor _visualTriangle;
        private Vector3 _vMaxSpeed;
        private Vector3 _vMaxSpeedN;
        private volatile GameTime _gameTime;
        /// <summary>
        /// The forward direction expressed as a <see cref="Vector3"/>.
        /// </summary>
        internal Vector3 Heading;
        /// <summary>
        /// Queue of path locations for the unit to move to, expressed as <see cref="Vector3"/>.
        /// </summary>
        public readonly Queue<Vector3> PathToQueue;
        // 12/14/2008 - Add new PathTo_Stack, to give ability to re-path to half-way waypoints, when
        //              the original location could not be AStar calc within the given max cycles!
        /// <summary>
        /// Temporary redirection path for unit to move to, expressed as <see cref="Vector3"/>.
        /// </summary>
        public readonly Stack<Vector3> PathToStack;
        /// <summary>
        /// Displays a terrain picked location using a magenta colored triangle for debug purposes.
        /// </summary>
        protected VertexPositionColor[] PickedTriangle =
            {
                new VertexPositionColor(Vector3Zero, Color.Magenta),
                new VertexPositionColor(Vector3Zero, Color.Magenta),
                new VertexPositionColor(Vector3Zero, Color.Magenta),
            };

        /// <summary>
        /// The <see cref="SceneItemWithPick"/> is the owner to this <see cref="AStarItem"/> class.
        /// </summary>
        internal readonly SceneItemWithPick SceneItemOwner;
        /// <summary>
        /// Average of the <see cref="Heading"/> vector.
        /// </summary>
        public Vector3 SmoothHeading;
        private LocklessQueue<Vector3> _solutionFinal; // 6/9/2010 - Updated to new LocklessQueue.
        private readonly Queue<Vector3> _solutionRepath; // 11/10/2009 - Hold original 'SolutionFinal' during a repath.
        private bool _doingRepathAroundItem; // 11/10/2009
        // 2/4/2009: Use PathNodeType 'Ground' or 'Air'.
        private readonly PathNodeType _usePathNodeType = PathNodeType.GroundItem; // Ground is default.

        // XNA 4.0 Updates
        private static RasterizerState _rasterizerState = new RasterizerState { FillMode = FillMode.Solid };
        private static DepthStencilState _depthStencilState = new DepthStencilState { DepthBufferEnable = false };


        #region Properties

        ///<summary>
        /// Stores the final validated A-Star solution, returned from
        /// the A-Star engine.
        ///</summary>
        /// <remarks>This Property is Thread-Safe.</remarks>
        public LocklessQueue<Vector3> SolutionFinal
        {
            get { return _solutionFinal; }
            set { _solutionFinal = value; }
        }

        // 9/22/2009 - Used in pathfinding AStar engine.
        /// <summary>
        /// Allow <see cref="SceneItem"/> to pass over blocked areas? 
        /// </summary>
        public bool CanPassOverBlockedAreas{ get; set; }

        // 5/29/2011
        /// <summary>
        /// Set when the <see cref="SceneItem"/> is in transition to the flag marker position upon
        /// exiting the building (aka. War Factory).
        /// </summary>
        public bool InTransitionToFlagMarker { get; set; }

        // 5/29/2011
        ///<summary>
        /// Vector3 FlagMarker goal position for the given unit.
        ///</summary>
        public Vector3 FlagGoalPosition
        {
            get { return _flagGoalPosition; }
            set { _flagGoalPosition = value; }
        }

        ///<summary>
        /// Vector3 goal position for the given unit.
        ///</summary>
        public Vector3 GoalPosition
        {
            get { return _goalPosition; }
            set { _goalPosition = value; }
        }

        ///<summary>
        /// Vector3 'Move-To' position for the given unit.
        ///</summary>
        public Vector3 MoveToPosition
        {
            get { return _moveToPosition; }
            set { _moveToPosition = value; }
        }

        // 11/15/2009
        ///<summary>
        /// Current A-Star position the unit is occuping
        /// in the game world.
        ///</summary>
        public Vector3 OccupiedByPosition
        {
            get
            {
                const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;

               
                /*Vector3 occupiedAtPosition = new Vector3
                                             {
                                                 X = OccupiedAtIndex == null ? 0 : OccupiedAtIndex.Value.X * pathNodeStride,
                                                 Y = 0,
                                                 Z = OccupiedAtIndex == null ? 0 : OccupiedAtIndex.Value.Z * pathNodeStride
                                             };*/

                // 1/21/2011
                OccupiedAtIndex.Y = 0;
                Vector3 occupiedAtPosition;
                Vector3.Multiply(ref OccupiedAtIndex, pathNodeStride, out occupiedAtPosition);

                return occupiedAtPosition;
            }
        }

        ///<summary>
        /// Current A-Star node position in game world.
        ///</summary>
        public Vector3 PathNodePosition
        {
            get { return _pathNodePosition; }
            set { _pathNodePosition = value; }
        }

        /// <summary>
        /// Last Vector3 position current unit was at.
        /// </summary>
        public Vector3 LastPosition { get; set; }

        ///<summary>
        /// Enumeration for the ItemState of this
        /// sceneItem; for example, item is in 'Resting' state
        /// or 'Moving' state.
        ///</summary>
        public ItemStates ItemState
        {
            get { return _itemState; }
            set { _itemState = value; }
        }

        // 5/23/2008 
        /// <summary>
        /// IgnoreOccupiedBy for PathFinding? 
        /// </summary>
        /// <remarks>If a unit has this flag set On,
        /// then the A* will ignore the 'occupiedBy' Status when creating a valid path.
        /// As the unit moves along the path, any blocking units will move out of the way! </remarks>
        public IgnoreOccupiedBy IgnoreOccupiedByFlag { get; set; }

        ///<summary>
        /// When set, the A-Star solution returned will be checked
        /// a 2nd time, and any redudant nodes (nodes on straight paths),
        /// will be removed.
        ///</summary>
        public bool UseSmoothingOnPath { get; set; }

        /// <summary>
        /// Enum to signify a Ground SceneItemOwner or Air SceneItemOwner, for
        /// the given SceneItem.
        /// </summary>
        public PathNodeType UsePathNodeType
        {
            get { return _usePathNodeType; }
        }

        ///<summary>
        /// During A-Star solutions, this is checked to determine if the 
        /// original 'Start' and 'End' nodes should be used; otherwise, if set 'TRUE', 
        /// the closest node to the 'End' node, from the 'Start' node, will be used.
        ///</summary>
        public AdjToClosestNode SetAdjToClosestNode
        {
            get { return _setAdjToClosestNode; }
            set { _setAdjToClosestNode = value; }
        }

        #endregion

// ReSharper disable UnaccessedField.Local

        ///<summary>
        /// Constructor, which initializes all required internal collections, like the
        /// <see cref="Queue{T}"/> and <see cref="Stack{T}"/>, used during pathfind moves.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="item"><see cref="SceneItemWithPick"/> instance this A* item extends</param>
        public AStarItem(Game game, SceneItemWithPick item)
        {
            // 10/20/2009 - Calculate distance between 3 points, to be use as estimate in 'GetNexPathNodeChecked' method.
            Vector2.Distance(ref _vector2Zero, ref _distanceToUse, out _distanceWithin3Nodes);

            // 8/13/2008
            _tShapeHelper = new TriangleShapeHelper(ref game);

            // 8/21/2008 - Get AStar Interface Ref
            if (_aStarManager == null)
                _aStarManager = (IAStarManager) game.Services.GetService(typeof (IAStarManager));

            // Create Queues/Stacks
            if (_visualPath == null)
                _visualPath = new List<VertexPositionColor>(25);

            if (_visualTestedNodes == null)
                _visualTestedNodes = new List<VertexPositionColor>(25);

            if (PathToQueue == null)
                PathToQueue = new Queue<Vector3>(25);

            if (PathToStack == null)
                PathToStack = new Stack<Vector3>(25);

            if (_solutionFinal == null)
                _solutionFinal = new LocklessQueue<Vector3>(); // 6/9/2010 - Updated to LocklessQueue
            
            // 11/10/09
            if (_solutionRepath == null)
                _solutionRepath = new Queue<Vector3>(100);

            if (_forwardAveragerForVector3S == null)
                _forwardAveragerForVector3S = new AveragerForVector3(10);

            if (_pathFindingStateArgs == null)
                _pathFindingStateArgs = new UpdatePathFindingStateArgs();

            // 6/17/2010 - Populate _possibleGoals with 8 empty placeholders.
            for (var i = 0; i < 8; i++)
            {
                _possibleGoals.Add(Vector2.Zero);
            }
            
            _visualTriangle = new VertexPositionColor(Vector3Zero, Color.OrangeRed);

            // 11/13/2008
            _moveToPosition = item.Position;

            // 2/4/2009 - Is this a GroundItem or AirItem?
            if (item is SciFiAircraftScene)
            {
                _usePathNodeType = PathNodeType.AirItem;
                
                // 9/22/2009
                CanPassOverBlockedAreas = true;
            }
            else
                _usePathNodeType = PathNodeType.GroundItem;

            SceneItemOwner = item;
        }

        // 3/4/2011
        ///<summary>
        /// Occurs when the pathfinding operation has completed.
        ///</summary>
        public event EventHandler PathfindingGoalCompleted;

        ///<summary>
        /// Occurs before the pathfinding state is updated.
        ///</summary>
        public event PathFindingEventHandler PathStateUpdating;
        ///<summary>
        /// Occurs after the pathfinding state was updated.
        ///</summary>
        public event PathFindingEventHandler PathStateUpdated;
        ///<summary>
        /// Occurs when the SceneItem has reached it intended 'Move-To' position.
        ///</summary>
        public event PathFindingEventHandler PathMoveToCompleted; // 11/5/2008     
        ///<summary>
        /// Occurs when the SceneItem has reached it intended 'Move-To' position. (Global version)
        ///</summary>
        public static event PathFindingEventHandler PathMoveToCompletedG; // 1/19/2009

        // 3/23/2009 - Update Gametime
        ///<summary>
        /// Saves the given <see cref="GameTime"/> instance locally, and sets into the 
        /// <see cref="ForceBehaviorsCalculator"/> manager.
        ///</summary>
        ///<param name="inGameTime"><see cref="GameTime"/> instance</param>
        public void UpdateGameTime(GameTime inGameTime)
        {
            _gameTime = inGameTime;

            // 10/5/2009 - Check 'ForceBehaviors' is not NULL!
            if (SceneItemOwner.ForceBehaviors != null)
                SceneItemOwner.ForceBehaviors.ThreadElapsedTime = _gameTime.ElapsedGameTime;
        }

        /// <summary>
        /// Updates the PathFindingState, as well as checking the PathTo Queue for next 
        /// PathFinding request.  Currently, this method is called from the AIThreadManager.
        /// </summary>        
        public void Update()
        {
            if (_gameTime == null) return;

            // 1/13/2010 - Skip ALL pathfinding, when the Interface 'IAStarGraph' is not present!
            if (TemporalWars3DEngine.AStarGraph == null)
                return;              

            // 8/4/2009 - Check if BotHelper.
            if (SceneItemOwner.IsBotHelper)
                ItemState = ItemStates.BotHelper;

            // 6/9/2010 - Check if sceneItem owner is alive, before allowing any pathfinding.
            if (!SceneItemOwner.IsAlive) return;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            // 3/23/2009 - Is Network Game?
            if (player.NetworkSession == null)
            {
                // No, this is a SP game.
                UpdatePathFinding(this);
            }
            else // Yes, Network game.
            {
                // then, is this the Host?
                if (player.NetworkSession.IsHost)
                {
                    // This is the Host, so call normal UpdatePathFinding.
                    UpdatePathFinding(this);
                }
                else
                {
                    // No, this is client
                    Update_MPClient(this);
                }
            }
        }

        // 6/8/2010 - Stores references to Neighbor collections.
        private SceneItemWithPick[] _neighborsGround;

        // 3/23/2009; 5/30/2011 - cache values
        /// <summary>
        /// Updates the PathfindingState for the <see cref="SceneItem"/>, by checking the PathTo Queue for new orders, and
        /// updating the items velocity and position.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="astarItem"/> is Null.</exception>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        private static void UpdatePathFinding(AStarItem astarItem)
        {
            // 11/27/2009 - Null check
            if (astarItem == null)
                throw new ArgumentNullException("astarItem", @"(UpdatePathFinding) method threw the NullRefExp.");

            // 5/30/2011 - Cache values
            var sceneItemWithPick = astarItem.SceneItemOwner;
            var itemState = astarItem._itemState;
            var pathToStack = astarItem.PathToStack;
            var pathToQueue = astarItem.PathToQueue;

            // 3/1/2011 - Check if errenously on a -1 blocked node.
            CheckIfUnitOnBlockedNode(astarItem);

            // 3/27/2009 - Only Update when not in Resting state!
            if (itemState != ItemStates.Resting)
                UpdatePathFindingState(astarItem, astarItem._gameTime);

            // 11/16/2009 - Updated to check BuildingScene's only and not the vehicles anymore.
            // 3/1/2009 - NonPenetration; added the check for only items which are in the 'Resting' state.
            if (itemState == ItemStates.Resting &&
                (sceneItemWithPick is BuildingScene || sceneItemWithPick is SciFiAircraftScene)
                && sceneItemWithPick.ForceBehaviors != null)
            {
                // 6/8/2010
                sceneItemWithPick.ForceBehaviors.GetNeighborsGround(ref astarItem._neighborsGround);
                EnforceNonPenetrationConstraint(astarItem, astarItem._neighborsGround, sceneItemWithPick.ForceBehaviors.NeighborsGroundKeysCount);
            }
                       
            
            // 4/27/2009: Or 'PathFindingTempGoal' state; // 6/9/2009: Or 'PathFindingAI'.
            // 12/11/2008 - If State is Resting, let's check PathToQueue 
            //              to see if any FindPath() need to be started.
            if ((itemState != ItemStates.Resting && itemState != ItemStates.PathFindingTempGoal) &&
                itemState != ItemStates.PathFindingAI) return;

            // 12/14/2008 - First check the 'PathToStack', to see if any 
            //              temporary waypoint goals need to be done first!
            if (pathToStack.Count > 0)
            {
                // Pop off the top waypoint
                astarItem._goalPosition = pathToStack.Pop();

                // Start AStar FindPath Request
                FindPath(astarItem, false);

                // 12/14/2008 - Set to PathFindingCalc
                astarItem._itemState = ItemStates.PathFindingCalc;
            } // Check pathTo Queue.               
            else
            {
                if (pathToQueue.Count > 0)
                {
                    // Dequeue waypoint
                    astarItem._goalPosition = pathToQueue.Dequeue();

                    // Start AStar FindPath Request
                    FindPath(astarItem, false);

                    // 12/14/2008 - Set to PathFindingCalc
                    astarItem._itemState = ItemStates.PathFindingCalc;
                }
            }
        }

        // 12/4/2008 - MP Version, used only for the Client player.
        /// <summary>
        /// Updates the PathFindingState, as well as checking the PathTo Queue for next 
        /// PathFinding request.  Currently, this method is called from the AIThreadManager.
        /// </summary> 
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="astarItem"/> is Null.</exception>
        /// <remarks> MP Version, used only for the Client player.</remarks>  
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
// ReSharper disable InconsistentNaming
        private static void Update_MPClient(AStarItem astarItem)
// ReSharper restore InconsistentNaming
        {
            // 11/27/2009 - Null check
            if (astarItem == null)
                throw new ArgumentNullException("astarItem", @"(UpdatePathFinding) method threw the NullRefExp.");

            // 5/30/2011 - Cache values
            var itemState = astarItem._itemState;
            var pathToQueue = astarItem.PathToQueue;
            var pathToStack = astarItem.PathToStack;

            // 3/1/2011 - Check if errenously on a -1 blocked node.
            CheckIfUnitOnBlockedNode(astarItem);

            // Check if node to process
            if (!astarItem.SolutionFinal.IsEmpty) // 6/9/2010 - Change to use IseEmpty.
                astarItem._itemState = ItemStates.PathFindingReady;

            // 11/13/2009 - If 'Resting' state, Check if Queue or Stack is not empty.
            if (itemState == ItemStates.Resting)
            {
                if (pathToQueue != null && pathToStack != null) // 11/27/2009
                    if (pathToQueue.Count > 0 || pathToStack.Count > 0)
                        astarItem._itemState = ItemStates.PathFindingReady;
            }

            // 3/27/2009 - Only Update when not in Resting state!
            if (itemState != ItemStates.Resting)
                UpdatePathFindingState_MPClient(astarItem, astarItem._gameTime);

            // 11/16/2009 - Updated to check BuildingScene's only and not the vehicles anymore.
            // 3/1/2009 - NonPenetration; added the check for only items which are in the 'Resting' state.            
            if (itemState == ItemStates.Resting &&
                (astarItem.SceneItemOwner is BuildingScene || astarItem.SceneItemOwner is SciFiAircraftScene))
            {
                // 6/8/2010
                astarItem.SceneItemOwner.ForceBehaviors.GetNeighborsGround(ref astarItem._neighborsGround);
                EnforceNonPenetrationConstraint(astarItem, astarItem._neighborsGround,
                                                astarItem.SceneItemOwner.ForceBehaviors.NeighborsGroundKeysCount);
            }
        }

        // 12/11/2008
        /// <summary>
        /// Used to add a new GoalPosition Node to request a path for; each call
        /// to this method adds the GoalPosition into the 'pathToQueue', if the 'Left-Shift'
        /// key is pressed down; otherwise, it starts the path request immediately.
        /// </summary>
        /// <param name="wayPointGoal"><see cref="Vector3"/> waypoint goal</param>
        public void AddWayPointGoalNode(ref Vector3 wayPointGoal)
        {
            // 8/13/2009: Updated to use the InputState.LeftShift property.
            // Check if Shift-key pressed, which indicates WayPoint Queuing!
            //KeyboardState keyState = Keyboard.GetState();
            if (HandleInput.InputState.LeftShift)
            {
                // Queue up new waypoint goal node.
                PathToQueue.Enqueue(wayPointGoal);
            }
            else
            {
                // Start Path request immeditely, clearing out any other current pathfinding.
                PathToQueue.Clear();
                _goalPosition = wayPointGoal;
                _itemState = ItemStates.Resting; // 3/6/2011
                FindPath(this, false);
            }
        }
       
        ///<summary>
        /// Renders the visual debug paths, to show the A* nodes and
        /// test nodes returned from the last solution.
        ///</summary>
        /// <remarks>Currently called from <see cref="SceneItemWithPick"/> Render method.</remarks>
        ///<param name="selectionBox">Collection of <see cref="VertexPositionColor"/> structs</param>
        public void RenderVisualDebugPaths(ref VertexPositionColor[] selectionBox)
        {
            // DEBUG: Show Visual A* Path
            if (_visualPath.Count > 0 && ShowVisualPathNodes)
            {
                selectionBox = _visualPath.ToArray();

                // XNA 4.0 Updates - Final 2 params updated.
                TriangleShapeHelper.DrawPrimitiveTriangle(ref selectionBox, _rasterizerState, _depthStencilState);
            }

            // DEBUG: Show the A* TestedNodes
            if (_visualTestedNodes.Count > 0 && ShowVisualTestedNodes)
            {
                selectionBox = _visualTestedNodes.ToArray();

                // XNA 4.0 Updates - Final 2 params updated.
                TriangleShapeHelper.DrawPrimitiveTriangle(ref selectionBox, _rasterizerState, _depthStencilState);
            }
        }

        #region A* Pathfinding Methods

        private Vector3[] _copyOfSolutionPath = new Vector3[1];

        private bool _giveChanceToMoveFirst;
        private readonly List<Vector2> _pastTries = new List<Vector2>(10); // 12/24/2008
        private readonly List<Vector2> _possibleGoals = new List<Vector2>(8);
        private static readonly Random RndGenerator = new Random();

        private AdjToClosestNode _setAdjToClosestNode = AdjToClosestNode.Off;
        private int _tempGoalTries;
        private static readonly Vector3 Vector3One = Vector3.One;
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        /// <summary>
        /// Find Path for <see cref="SceneItem"/> using A* algorithm calls.
        /// </summary>     
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="useTempGoal">use temp goal?</param>
        private static void FindPath(AStarItem astarItem, bool useTempGoal)
        {
            // 1/13/2010
            if (TemporalWars3DEngine.AStarGraph == null) return;

            // Set LastPosition to current Pos
            astarItem.LastPosition = astarItem.SceneItemOwner.Position;

            // 10/16/2009: Updated to try to find an alternative Goal node, if the current node is blocked!
            // 3/22/2009: Updated to skip AirCraft, since they can fly anywhere!
            // 12/10/2008 - Verify 'Goal' Position can be moved to, otherwise no pathfinding needed!
            if (TemporalWars3DEngine.AStarGraph.IsNodeBlocked(NodeScale.TerrainScale, (int)astarItem._goalPosition.X, (int)astarItem._goalPosition.Z) &&
                !(astarItem.SceneItemOwner is SciFiAircraftScene))
            {
                // Try to find an alternative Goal node to use.
                Vector3 newNode;
                if (GetClosestFreeNode(NodeScale.TerrainScale, astarItem.UsePathNodeType, 
                    ref astarItem._goalPosition, out newNode))
                {
                    // set as new Goal node to use.
                    astarItem._goalPosition = newNode;
                }
                else
                {
                    // Else, no Alternative node, so stop search.
                    astarItem._itemState = ItemStates.Resting;
                    return;
                } // End if Alt Goal node.
            }
            
            // Make sure clear before getting new solution path.
            ClearSolutionFinal(astarItem); // 6/9/2010

            // Start A* Path Algorithm 
            // A* see X and Y as Width & Height, therefore need to place Z-Axis into A* Y-Axis.
            if (!useTempGoal)
            {
                astarItem.SetAdjToClosestNode = AdjToClosestNode.On; // 5/2/2009
                _aStarManager.FindPath_Init(astarItem);
            }
            else
            {
                astarItem.SetAdjToClosestNode = AdjToClosestNode.Off; // 5/2/2009
                _aStarManager.FindPath_Init(astarItem);
            }


        }



#if EditMode && !XBOX360

        // 8/13/2009
        /// <summary>
        /// Search complete method, called by the 'CycleOnce' A* engine when search completes; however,
        /// this method overload is ONLY used in Debug mode, to also pass back the 'TestedNodes'.
        /// </summary>
        /// <param name="astarItemInterfaceInterface"><see cref="IAStarItem"/> instance</param>
        /// <param name="solutionFound">Solution found?</param>
        /// <param name="testedNodes">Collection of <see cref="Vector3"/> as test nodes.</param>
        public void AStarInstanceSearchComplete(IAStarItem astarItemInterfaceInterface, bool solutionFound, IDictionary<int, Vector3> testedNodes)
        {
            var astarItem = (AStarItem)astarItemInterfaceInterface; // 9/22/2009

            // DEBUG: Create List Array '_visualTestedNodes'; this will be used to show A* Nodes on screen.
            if (ShowVisualTestedNodes)
                PopulateVisualTestedNodesArray(astarItem, testedNodes);

            // 3/6/2008
            // DEBUG: Create List Array '_visualPath'; this will be used to show A* Paths on screen.
            if (ShowVisualPathNodes)
                PopulateVisualPathArray(astarItem, astarItem.SolutionFinal.GetEnumerator());

            // Pass to Search complete method.
            AStarInstanceSearchComplete(astarItem, solutionFound);
        }
#endif

        // 6/16/2009
        /// <summary>
        /// Search Complete method, called by the 'CycleOnce' AStar engine, when search complete.
        /// </summary>     
        /// <param name="astarItemInterfaceInterface"><see cref="IAStarItem"/> instance</param>
        /// <param name="solutionFound">Solution found?</param>  
        public void AStarInstanceSearchComplete(IAStarItem astarItemInterfaceInterface, bool solutionFound)
        {
            Vector3 firstNode;
            var astarItem = (AStarItem) astarItemInterfaceInterface; // 9/22/2009

            if (solutionFound)
            {
                // 11/10/2009 - Check if 'Repath' occured.
                if (_doingRepathAroundItem)
                {
                    _doingRepathAroundItem = false;

                    // Copy rest of solution into Queue.
                    while (_solutionRepath.Count > 0)
                    {
                        var currentNode = _solutionRepath.Dequeue();
                        _solutionFinal.Enqueue(currentNode);
                    }
                }

                // Apply Smoothing Algorithm - 3/3/2008                
                // 4/28/2008 - Add Ability to Not Use Smoothing
                if (astarItem.UseSmoothingOnPath)
                    firstNode = SmoothingPathAlgorithm(astarItem, astarItem.SolutionFinal);
                else
                {
                    // DeQueue first node
                    if (astarItem.SolutionFinal.Count > 0)
                    {
                        if (!SolutionTryDequeue(astarItem, out firstNode))
                        {
                            // Then NO solution found
                            astarItem._itemState = ItemStates.Resting;
                            return;
                        }
                    }
                    else
                    {
                        // Then NO solution found
                        astarItem._itemState = ItemStates.Resting;
                        return;
                    }
                }

                astarItem._moveToPosition.X = firstNode.X;
                astarItem._moveToPosition.Y = 0; // Height will be adjusted by tank movement algorithm  
                astarItem._moveToPosition.Z = firstNode.Z;

                // 12/24/2008 - Iterate solution path to make any units on path to move out of the way!
                //              This will help avoid unnecessary alt-temp pathfinding.
                CheckForOtherUnitsOnSolutionPath(astarItem);


                // isSolutionReady = true;
                astarItem._itemState = ItemStates.PathFindingReady;

                return; // 5/29/2011
            }

            // Then NO Solution found
            astarItem._itemState = ItemStates.Resting;
        }

        // 6/9/2010
        /// <summary>
        /// Method helper, which tries to dequeue the next valid node
        /// from the <see cref="SolutionFinal"/>.
        /// </summary>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        /// <param name="pathNode"></param>
        /// <returns>true/false of result</returns>
        private static bool SolutionTryDequeue(AStarItem astarItem, out Vector3 pathNode)
        {
            while (!astarItem.SolutionFinal.TryDequeue(out pathNode)) // 6/9/2010 - Updated to TryDequeue.
            {
                // Check if empty
                if (astarItem.SolutionFinal.IsEmpty)
                {
                    // Then NO solution found
                    astarItem._itemState = ItemStates.Resting;
                    return false;
                }
                Thread.Sleep(0);
            }
            return true;
        }

        // 6/9/2010
        /// <summary>
        /// Method helper, which iterates the <see cref="SolutionFinal"/> queue, calling
        /// the tryDequeue method until all nodes are removed.
        /// </summary>
        /// <param name="astarItem"><see cref="IAStarItem"/> instance</param>
        public static void ClearSolutionFinal(IAStarItem astarItem)
        {
            Vector3 result;
            while (!astarItem.SolutionFinal.IsEmpty)
                astarItem.SolutionFinal.TryDequeue(out result);
        }

        // 7/7/2009 - Used for the 'CheckForOtherUnitsOnSoltionPath' method.
        // 12/24/2008; 
        // 4/17/2009: Updated to use the 'IEnumerator' of the Queue, to eliminate need for 2nd array, which creates garabage!
        /// <summary>
        /// Iterates the current solution path, checking for any of our own units, and when found, makes
        /// them move out of the way!
        /// </summary>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        private static void CheckForOtherUnitsOnSolutionPath(AStarItem astarItem)
        {
            // 6/17/2010 - Cache
            var solutionPath = astarItem.SolutionFinal;
            var aStarGraph = TemporalWars3DEngine.AStarGraph;
            var sceneItemOwner = astarItem.SceneItemOwner;

            try
            {
                // 6/17/2010 - Check if null
                if (sceneItemOwner == null) return;

                // 2/3/2009 - Skip if Aircraft type
                if (sceneItemOwner.ShapeItem is SciFiAircraftShape)
                    return;

                // 7/7/2009 - Copy Queue into local copy, from thread copy.
                var indexCount = solutionPath.Count;
                if (astarItem._copyOfSolutionPath.Length < indexCount)
                    Array.Resize(ref astarItem._copyOfSolutionPath, indexCount);

                solutionPath.CopyTo(astarItem._copyOfSolutionPath, 0);


                // 2nd - Iterate through temp array, checking for any items currently
                //       occuping the solution path, and force them to move out of the way.               
                //IEnumerator tmpEnum = solutionPath.GetEnumerator(); // Source Pointer                                
                //while (tmpEnum.MoveNext())
               
                for (var i = 0; i < indexCount; i++)
                {
                    var currentNode = astarItem._copyOfSolutionPath[i];

                    // 11/15/2009 - Updated to use the new 'GetOccupiedByAtIndex'.
                    // is there anyone occuping the current node pos?
                    object occupiedBy; // Was SceneItemWithPick
                    var index = new Point { X = (int)currentNode.X, Y = (int)currentNode.Z };
                    if (aStarGraph != null) // 1/13/2010
                        if (aStarGraph.GetOccupiedByAtIndex(ref index, NodeScale.TerrainScale, PathNodeType.GroundItem,
                                                                                     out occupiedBy))
                        {
                            // 6/17/2010 - Cache
                            var sceneItemWithPick = ((SceneItemWithPick) occupiedBy);
                            
                            // Is it one of our own units, and not enemy unit?
                            if (sceneItemWithPick != null)
                                if (sceneItemWithPick.PlayerNumber == sceneItemOwner.PlayerNumber &&
                                    sceneItemWithPick.ItemState == ItemStates.Resting)
                                {
                                    // yes, then make them move out of way!
                                    MoveOutOfTheWay(sceneItemWithPick.AStarItemI);
                                } // End If PlayerNumber
                        } // End if GetOccupiedBy

                } // End For QueueNodes
            }
            catch (Exception)
            {
                Debug.WriteLine("Method Error in CheckForOtherUnitsOnSolutionPath:");
            }
        }

        // 3/1/2011
        /// <summary>
        /// Helper method which checks for any units which are errenously on
        /// some blocked -1 node position.  If true, it will move the unit to the
        /// closest free node.
        /// </summary>
        /// <param name="astarItem">Instance of <see cref="AStarItem"/>.</param>
        private static void CheckIfUnitOnBlockedNode(AStarItem astarItem)
        {
            // Do check every few seconds
            astarItem._blockNodeCheckTime -= (float)astarItem._gameTime.ElapsedGameTime.TotalMilliseconds;
            if (astarItem._blockNodeCheckTime >= 0) return;
            astarItem._blockNodeCheckTime = BlockNodeCheckTimeReset;

            // NOTE: Using the IsNodeBlockedForCursor because only want to check the -1 state, not the building -2 state.
            if (!TemporalWars3DEngine.AStarGraph.IsNodeBlockedForCursor(NodeScale.TerrainScale, (int) astarItem.SceneItemOwner.Position.X,
                                                          (int)astarItem.SceneItemOwner.Position.Z))
            return;

            // return if still in PathFindingMoving state.
            if (astarItem.ItemState == ItemStates.PathFindingMoving) return;

            // Search for nearby valid node to move unit to
            Vector3 newPosition;
            var itemPosition = astarItem.SceneItemOwner.Position;
            if (!GetClosestFreeNode(NodeScale.TerrainScale, PathNodeType.GroundItem, ref itemPosition, out newPosition)) return;

            // Move unit to given location
            astarItem.MoveToPosition = newPosition;
            astarItem.ItemState = ItemStates.PathFindingMoving;

        }

        // 6/24/2008: 
        /// <summary>
        /// Checks the <see cref="ItemStates"/> for pathfinding, and updates accordingly.
        /// </summary>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdatePathFindingState(AStarItem astarItem, GameTime gameTime)
        {
            var totalRealTime = gameTime.TotalGameTime;
            var elapsedGameTime = gameTime.ElapsedGameTime;
            var sceneItemOwner = astarItem.SceneItemOwner; // 6/17/2010 - Cache
            var sceneItemOwnerPosition = sceneItemOwner.Position; // 8/13/2009

            // 10/9/2008 - Fire Updating Event             
            DoPathStateUpdatingEvent(astarItem, ref totalRealTime, ref elapsedGameTime);


            // 2/29/2008; 6/17/2010: Converted if statements to Switch.
            // A* Pathfinding State
            switch (astarItem._itemState)
            {
                case ItemStates.PathFindingReady:
                    {
                        // Call A* SceneItemWithPick.GetNextPathNode method
                        var ready = GetNextPathNodeChecked(astarItem, gameTime); // 10/20/2009 - Updated to use new 'Checked' version.
                        // Found Node, then set to "Moving" state.
                        if (ready)
                        {
                            astarItem._itemState = ItemStates.PathFindingMoving;

                            // 1/19/2009 - If Host of MP game, send the MoveToPos command to client.
                            MPGame_SendMoveCommandToClient(astarItem);
                        } // End If Ready
                    }
                    break;
                case ItemStates.PausePathfinding:
                    if (astarItem._pauseTime > 0)
                    {
                        astarItem._pauseTime -= (float)elapsedGameTime.TotalSeconds;
                        return;
                    }
                    astarItem._itemState = ItemStates.PathFindingMoving; // PathFindingReady
                    break;
                case ItemStates.PathFindingMoving:
                    if (astarItem.SceneItemOwner.HasReachedMoveToPosition(astarItem.MoveToPosition))
                    {
                        // 5/29/2011
                        // check if unit has reached the final FlagMarkerPosition.
                        DoInTransitionToFlagMarkerCheck(astarItem);

                        // 5/30/2011
                        // to check if unit is attacking a dead attackie, then stop current pathfinding.
                        DoAttackingDeadItemCheck(astarItem);

                        // 6/17/2010 - Refactored.
                        UpdatePathNodePosition(astarItem, ref totalRealTime, ref elapsedGameTime, ItemStates.PathFindingReady);
                    } // End If MoveToPosition Reach                             
                    break;
                case ItemStates.Moving:
                    if (astarItem.SceneItemOwner.HasReachedMoveToPosition(astarItem.MoveToPosition))
                    {
                        // 5/29/2011
                        // check if unit has reached the final FlagMarkerPosition.
                        DoInTransitionToFlagMarkerCheck(astarItem);

                        // 5/30/2011
                        // to check if unit is attacking a dead attackie, then stop current pathfinding.
                        DoAttackingDeadItemCheck(astarItem);

                        // Check if it was currently not finished getting to a goal?
                        UpdatePathNodePosition(astarItem, ref totalRealTime, ref elapsedGameTime,
                                               astarItem.SolutionFinal.Count > 0
                                                   ? ItemStates.PathFindingReady
                                                   : ItemStates.Resting);
                    } // End If Goal Reach
                    break;
            }

            // 3/23/2009 - Check if ForceBehaviors is null; occurs within the Thread now, since
            //             the ForceBehaviors is added in the Main thread.
            if (sceneItemOwner.ForceBehaviors == null)
                return;

            // 10/7/2008 - Call Calculate to get Total Steering force acceleration           
            Vector3 tmpVelocity;
            UpdateVelocityWithSteeringForce(astarItem, sceneItemOwner, ref elapsedGameTime, out tmpVelocity);

            // 6/23/2009 - Test 'PhysX' Vehicle Force
/*#if !XBOX360
            if (SceneItemOwner.ShapeItem.ItemType == Spacewar.InstancedModels.ItemType.sciFiTank1)
            {
                // Get new Position
                SceneItemOwner.Position = PhysX.PhysXVehicle.GetNewVehiclePositionData();

                // Update Force
                PhysX.PhysXVehicle.ApplyForceToVehicle(tmpVelocity);  
              
                return;
            }
#endif*/

            // 12/23/2008
            if (tmpVelocity.LengthSquared() > 0.1) // removed two zeros; was 0.001
            {
                // update Position using given velocity           
                UpdatePositionUsingVelocity(sceneItemOwner, ref sceneItemOwnerPosition, ref elapsedGameTime,
                                            ref tmpVelocity);

                // 10/14/2008
                // Store the 'Forward' Velocity in to 'smoothVelocity', which the 'TurnToFace' AbstractBehavior
                // uses to determine the 'DesiredAngle'.                    
                UpdateSmoothHeading(astarItem, ref tmpVelocity);

                // 1/26/2009 - Calculate ForceBehavior
            } // End If Vel > .1


            // 10/9/2008 - Fire Updated Event
            DoPathStateUpdatedEvent(astarItem, ref totalRealTime, ref elapsedGameTime);
        }

        // 5/30/2011
        /// <summary>
        /// Method helper to check if unit is attacking a dead attackie, then stop current
        /// pathfinding.
        /// </summary>
        private static void DoAttackingDeadItemCheck(AStarItem astarItem)
        {
            var sceneItemOwner = astarItem.SceneItemOwner;
            if (!sceneItemOwner.AttackOn) return;

            var attackie = sceneItemOwner.AttackSceneItem;
            if (attackie == null || !attackie.IsAlive)
            {
                // Clear SolutionFinal Queue
                ClearSolutionFinal(astarItem);
            }

        }

        // 5/30/2011
        /// <summary>
        /// Helper method which checks if a <see cref="SceneItem"/>, which was just built from a war factory,
        /// as reached its final destination at the current FlagMarker position.
        /// </summary>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        private static void DoInTransitionToFlagMarkerCheck(AStarItem astarItem)
        {
            if (!astarItem.InTransitionToFlagMarker ||
                !astarItem.SceneItemOwner.HasReachedMoveToPosition(astarItem.FlagGoalPosition)) return;

            astarItem.InTransitionToFlagMarker = false;

            // 5/29/2011 - for GroundItem units the flag 'CanPassOverBlockedAreas' is used ONLY when the unit first
            //             is pathfinding from the building; therefore, it reset to false here.
            if (astarItem.CanPassOverBlockedAreas && astarItem._usePathNodeType == PathNodeType.GroundItem)
                astarItem.CanPassOverBlockedAreas = false;
        }


        // 1/19/2009
        /// <summary>
        /// Checks the <see cref="ItemStates"/> for pathfinding, and updates accordingly on the client side Only
        /// during a MP game.
        /// </summary>
        /// <param name="astarItem">this instance of <see cref="AStarItem"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
// ReSharper disable InconsistentNaming
        private static void UpdatePathFindingState_MPClient(AStarItem astarItem, GameTime gameTime)
// ReSharper restore InconsistentNaming
        {
            var totalRealTime = gameTime.TotalGameTime;
            var elapsedGameTime = gameTime.ElapsedGameTime;
            var sceneItemOwner = astarItem.SceneItemOwner; // 6/17/2010
            var sceneItemOwnerPosition = sceneItemOwner.Position; // 8/13/2009

            // 2/29/2008; 6/17/2010: Converted if statements to Switch.
            // A* Pathfinding State
            switch (astarItem.ItemState)
            {
                case ItemStates.PathFindingReady:
                    if (astarItem.SolutionFinal.Count > 0)
                    {
                        Vector3 destNode;
                        if (!SolutionTryDequeue(astarItem, out destNode))
                        {
                            // Then no solution found.
                            astarItem._itemState = ItemStates.Resting;
                            return;
                        }

                        astarItem._moveToPosition.X = destNode.X;
                        astarItem._moveToPosition.Y = TerrainData.GetTerrainHeight(astarItem._moveToPosition.X,
                                                                                   astarItem._moveToPosition.Z);
                        astarItem._moveToPosition.Z = destNode.Z;

                        astarItem.ItemState = ItemStates.PathFindingMoving;
                    }
                    else
                        astarItem._itemState = ItemStates.Resting;
                    break;
                case ItemStates.PathFindingMoving:
                    if (astarItem.SceneItemOwner.HasReachedMoveToPosition(astarItem.MoveToPosition))
                    {
                        // 6/17/2010 - Refactored.
                        UpdatePathNodePosition(astarItem, ref totalRealTime, ref elapsedGameTime, ItemStates.PathFindingReady);

                    } // End If Goal Reach                             
                    break;
            }

            // 3/23/2009 - Check if ForceBehaviors is null; occurs within the Thread now, since
            //             the ForceBehaviors is added in the Main thread.
            if (sceneItemOwner.ForceBehaviors == null)
                return;

            // 10/7/2008 - Call Calculate to get Total Steering force acceleration
            Vector3 tmpVelocity;
            UpdateVelocityWithSteeringForce(astarItem, sceneItemOwner, ref elapsedGameTime, out tmpVelocity);

            // 12/23/2008
            if (tmpVelocity.LengthSquared() <= 0.1) return;

            // update Position using given velocity 
            UpdatePositionUsingVelocity(sceneItemOwner, ref sceneItemOwnerPosition, ref elapsedGameTime,
                                        ref tmpVelocity);

            // 10/14/2008
            // Store the 'Forward' Velocity in to 'smoothVelocity', which the 'TurnToFace' AbstractBehavior
            // uses to determine the 'DesiredAngle'.
            UpdateSmoothHeading(astarItem, ref tmpVelocity);

            // 1/26/2009
        }

        // 6/17/2010
        /// <summary>
        /// Updates the internal <see cref="_pathNodePosition"/> using the new <see cref="_moveToPosition"/>.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="totalRealTime"><see cref="TimeSpan"/> as total real time</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elasped game time</param>
        /// <param name="pathFindingReady"><see cref="ItemStates"/> Enum to set as new ItemState.</param>
        private static void UpdatePathNodePosition(AStarItem astarItem, ref TimeSpan totalRealTime, 
                                                    ref TimeSpan elapsedGameTime, ItemStates pathFindingReady)
        {
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
            var sceneItemOwnerPosition = astarItem.SceneItemOwner.Position;

            astarItem.ItemState = pathFindingReady;
            astarItem.LastPosition = sceneItemOwnerPosition;
            // Set Current _pathNodePosition

// ReSharper disable PossibleLossOfFraction
            astarItem._pathNodePosition.X = (int) astarItem._moveToPosition.X/pathNodeStride;
// ReSharper restore PossibleLossOfFraction
            astarItem._pathNodePosition.Y = astarItem._moveToPosition.Y;
// ReSharper disable PossibleLossOfFraction
            astarItem._pathNodePosition.Z = (int) astarItem._moveToPosition.Z/pathNodeStride;
// ReSharper restore PossibleLossOfFraction

            // 11/5/2008 - Fire Event
            DoPathMoveToCompletedEvent(astarItem, ref totalRealTime, ref elapsedGameTime);

            // 1/19/2009 - Fire Global Event
            DoPathMoveToCompletedGEvent(astarItem, ref totalRealTime, ref elapsedGameTime);
        }

        // 6/17/2010
        /// <summary>
        /// Updates the velocity using the steering force result.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="sceneItemOwner"><see cref="SceneItemWithPick"/> instance</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elapsed game time</param>
        /// <param name="velocity">(OUT) updated velocity</param>
        private static void UpdateVelocityWithSteeringForce(AStarItem astarItem, SceneItemWithPick sceneItemOwner, 
                                                           ref TimeSpan elapsedGameTime, out Vector3 velocity)
        {
            velocity = sceneItemOwner.Velocity;
            Vector3 tmpForceWithElapsedTime;

            // 11/14/2008 - Update to use Thread Method
            //SceneItemOwner.ForceBehaviors.ThreadElapsedTime = elapsedGameTime;
            var tmpForce = sceneItemOwner.ForceBehaviors.ThreadForceResult;

            // 11/19/2008 - Optimize by removing Vector Overload operations, which are slow on XBOX!
            sceneItemOwner.Force = tmpForce;
            //tmpVelocity = tmpVelocity * (1 - Friction * (float)ElapsedTime.TotalSeconds) +
            //    (SceneItemOwner.Force * (float)ElapsedTime.TotalSeconds);
            var tmpFrictionResult = (1 - (Friction*(float) elapsedGameTime.TotalSeconds));
            Vector3.Multiply(ref tmpForce, (float) elapsedGameTime.TotalSeconds, out tmpForceWithElapsedTime);
            Vector3.Multiply(ref velocity, tmpFrictionResult, out velocity);
            Vector3.Add(ref velocity, ref tmpForceWithElapsedTime, out velocity);

            //make sure vehicle does not exceed maximum velocity            
            //_vMaxSpeed = Vector3.One * SceneItemOwner.MaxSpeed;
            var tmpVector3One = Vector3One;
            Vector3.Multiply(ref tmpVector3One, sceneItemOwner.MaxSpeed, out astarItem._vMaxSpeed);
            //_vMaxSpeedN = (Vector3.One * SceneItemOwner.MaxSpeed) * -1;
            Vector3.Multiply(ref astarItem._vMaxSpeed, -1, out astarItem._vMaxSpeedN);
            Vector3.Clamp(ref velocity, ref astarItem._vMaxSpeedN, ref astarItem._vMaxSpeed, out velocity);
            sceneItemOwner.Velocity = velocity;
        }

        // 6/17/2010
        /// <summary>
        /// Updates the 'Forward' smooth heading vector using the given <paramref name="velocity"/>.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="velocity"><see cref="Vector3"/> as velocity</param>
        private static void UpdateSmoothHeading(AStarItem astarItem, ref Vector3 velocity)
        {
            if (!velocity.Equals(Vector3Zero))
                velocity.Normalize(); // 8/13/2009 - Avoid NaN, by not normalizing zero values.
            astarItem.Heading = velocity;
            // 10/17/2008 - Store Side of Heading.
            astarItem._side.X = -astarItem.Heading.Z;
            astarItem._side.Y = astarItem.Heading.Y;
            astarItem._side.Z = astarItem.Heading.X;

            // Add to AveragerForVector3s to remove Forward Jitter!                
            astarItem._forwardAveragerForVector3S.Update(ref velocity, out astarItem.SmoothHeading);
            astarItem.SmoothHeading.Normalize();

            // 7/19/2009
            if (float.IsNaN(astarItem.SmoothHeading.X))
                astarItem.SmoothHeading.X = 0.0f;

            if (float.IsNaN(astarItem.SmoothHeading.Y))
                astarItem.SmoothHeading.Y = 0.0f;

            if (float.IsNaN(astarItem.SmoothHeading.Z))
                astarItem.SmoothHeading.Z = 0.0f;
        }

        // 6/17/2010
        /// <summary>
        /// Updates the current position with the given velocity.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="SceneItem"/> instance</param>
        /// <param name="sceneItemOwnerPosition"><see cref="Vector3"/> position to update</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elapsed game time</param>
        /// <param name="velocity"><see cref="Vector3"/> velocity to use</param>
        private static void UpdatePositionUsingVelocity(SceneItem sceneItemOwner, ref Vector3 sceneItemOwnerPosition, 
                                                        ref TimeSpan elapsedGameTime, ref Vector3 velocity)
        {
            //SceneItemOwner.Position += SceneItemOwner.Velocity * (float)ElapsedTime.TotalSeconds;
            Vector3 tmpVelocityWithElapsedTime;
            Vector3.Multiply(ref velocity, (float) elapsedGameTime.TotalSeconds, out tmpVelocityWithElapsedTime);
               
            Vector3.Add(ref sceneItemOwnerPosition, ref tmpVelocityWithElapsedTime, out sceneItemOwnerPosition);
            sceneItemOwner.Position = sceneItemOwnerPosition;
            
        }

        /// <summary>
        /// Given a pointer to an 'SceneItem' and an Array of pointers to nearby
        //  neighbors, this function checks to see if there is an overlap between
        //  entities. If there is, then the entities are moved away from each
        //  other.
        /// </summary>
        private static void EnforceNonPenetrationConstraint(AStarItem astarItem, SceneItemWithPick[] neighbors, int keysCount)
        {
            try
            {
                var item = astarItem.SceneItemOwner; // 6/8/2010

                // 5/12/2009: 'IndexCount' is used, instead of 'defenseCopy' Count, since this array is a temporary
                //            array which can grow, but doesn't shrink, due to memory managmenet!  Therefore, it's size
                //            will not always be correct, which is why we use the current 'IndexCount' size.
                //iterate through all entities checking for any overlap of bounding radii
                for (var i = 0; i < keysCount; i++)
                {
                    // 6/8/2010 - Lockless Dictionary
                    var neighbor = neighbors[i];
                    // 3/1/2009
                    if (neighbor == null) continue;
                    
                    // 4/14/2009 - Cache local values to improve CPI in VTUNE!
                    var neighborSceneItem = neighbor;

                    //make sure we don't check against the individual
                    if (neighborSceneItem == item)
                        continue;

                    // 3/1/2009 - make sure shapeitem not null, to avoid crashes
                    if (neighborSceneItem.ShapeItem == null)
                        continue;

                    // 3/1/2009 - make sure we don't compare aircraft with ground vehciles!                
                    if (((neighborSceneItem.ShapeItem as IInstancedItem).ItemGroupType == ItemGroupType.Airplanes &&
                         (item.ShapeItem as IInstancedItem).ItemGroupType == ItemGroupType.Vehicles))
                        continue;

                    // 12/18/2008 - Remove Overload Ops, since slows down XBOX!
                    //calculate the distance between the positions of the entities
                    //toEntity = SceneItemOwner.Position - neighbors[i].Position;
                    var itemPosition = item.Position; // 8/13/2009
                    var tmpPos = new Vector2 {X = itemPosition.X, Y = itemPosition.Z};
                    var neighborPosition = neighborSceneItem.Position; // 8/13/2009
                    var tmpPos2 = new Vector2 {X = neighborPosition.X, Y = neighborPosition.Z};

                    Vector2 toEntityA;
                    Vector2.Subtract(ref tmpPos, ref tmpPos2, out toEntityA);

                    // 2/3/2009
                    var toEntityB = new Vector3 {X = toEntityA.X, Y = 0, Z = toEntityA.Y};

                    var distFromEachOther = toEntityB.Length();

                    //if this distance is smaller than the sum of their radii then this
                    //entity must be moved away in the direction parallel to the
                    //ToEntity vector   
                    var amountOfOverLap = neighborSceneItem.CollisionRadius + item.CollisionRadius - distFromEachOther;

                    if (amountOfOverLap < 0) continue;

                    // 12/18/2008 - Remove Overload Ops, since slows down XBOX!
                    //move the entity a distance away equivalent to the amount of overlap.
                    //SceneItemOwner.Position += (toEntity / distFromEachOther) * amountOfOverLap;
                    Vector3.Divide(ref toEntityB, distFromEachOther, out toEntityB);
                    Vector3.Multiply(ref toEntityB, amountOfOverLap, out toEntityB);
                    Vector3.Add(ref itemPosition, ref toEntityB, out itemPosition);
                    item.Position = itemPosition;
                } // End Loop Neighbors
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                {
                    Debug.WriteLine("Method Error in EnforceNonPentration: {0}", err.InnerException.Message);
                }
                else
                    Debug.WriteLine("Method Error in EnforceNonPentration.");
            }
        }

        // 10/20/2009
        /// <summary>
        /// Gets the next node in the FinalSolution array, using the 'GetNextPathNode' method, and
        /// then checking if the given MoveToPosition is a valid position by checking if the distance
        /// between the current item's position and MoveToPosition is too far!  
        /// </summary>
        /// <param name="astarItem">AStarItem instance</param>
        /// <param name="gameTime">GameTime</param>
        /// <returns>True/False</returns>
        private static bool GetNextPathNodeChecked(AStarItem astarItem, GameTime gameTime)
        {
            // Is there a MoveToPosition node to go to?
            if (GetNextPathNode(astarItem, gameTime))
            {
                // Lets verify the 'MoveToPosition' is valid, by making sure
                // it is a reasonable distance away from the current position!
                float distanceBetweenNodes;
                var moveToPosition = astarItem.MoveToPosition;
                astarItem.SceneItemOwner.CalculateDistanceToPosition(ref moveToPosition, out distanceBetweenNodes);
             
                // Is distance too far?  Is this a valid node to moveTo?
                return distanceBetweenNodes <= _distanceWithin3Nodes;
            }

            // 3/1/2011 - Reset AltGoalAttack flag to false
            astarItem._foundAlternativeGoalAttackPosition = false;

            return false;
        }

        /// <summary>
        /// Gets the next node in the FinalSolution array, and set as the
        /// current 'MoveToPosition'.  
        /// </summary>
        /// <param name="astarItem">AStarItem instance</param>
        /// <param name="gameTime">GameTime</param>
        /// <returns>True/False</returns>       
        private static bool GetNextPathNode(AStarItem astarItem, GameTime gameTime)
        {
            // Check to see if Queue Empty
            if (astarItem.SolutionFinal.Count > 0)
            {
                // Peek at first Node without removing it yet
                Vector3 destNode;
                astarItem.SolutionFinal.TryPeek(out destNode); // 6/9/2010 - Updated To TryPeek.

// ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (destNode != null)
// ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    astarItem._moveToPosition.X = destNode.X;
                    astarItem._moveToPosition.Y = 0;
                    astarItem._moveToPosition.Z = destNode.Z;

                    // 11/15/2009 - Updated to use the new 'GetOccupiedByAtIndex'.
                    // 2/4/2009: Updated to pass in the '_usePathNodeType'.
                    // Check for Collision with another unit. (NextPathNodeCollisionCheck)
                    var index = new Point {X = (int) destNode.X, Y = (int) destNode.Z};
                    object occupiedByOut;
                    if (TemporalWars3DEngine.AStarGraph.GetOccupiedByAtIndex(ref index, NodeScale.TerrainScale,
                                                                             astarItem.UsePathNodeType,
                                                                             out occupiedByOut))
                    {
                        // 9/22/2009
                        var occupiedBy = occupiedByOut as SceneItemWithPick;

                        // Make sure it is not ourselves?
                        if (occupiedBy != null)
                            if (occupiedBy.SceneItemNumber != astarItem.SceneItemOwner.SceneItemNumber)
                            {
                                // Is Other Unit Moving?
                                switch (occupiedBy.ItemState)
                                {
                                    case ItemStates.Moving:
                                    case ItemStates.PathFindingReady:
                                    case ItemStates.PausePathfinding:
                                    case ItemStates.PathFindingMoving:
                                        // does occupied item want our position?
                                        if (occupiedBy.MoveToPosition == astarItem.OccupiedByPosition)
                                        {
                                            // 3/6/2011 - Check if enemy player
                                            if(occupiedBy.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                                                return false;

                                            // yes, so will move out of its way!
                                            MoveOutOfTheWay(astarItem);
                                            return false;
                                        }
                                        // 11/10/2009
                                        // else, is occupied unit just in a 'Pause' mode; if so, let's repath because
                                        // we are unpatient unit!
                                        if (occupiedBy.ItemState == ItemStates.PausePathfinding)
                                        {
                                            // reduce our patience timer!
                                            astarItem._unpatientTime -= gameTime.ElapsedGameTime.Milliseconds;
                                            if (astarItem._unpatientTime > 0)
                                            {
                                                astarItem._itemState = ItemStates.PausePathfinding;
                                                astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                                                astarItem._pauseTime = 0.5f;
                                                return false;
                                            }
                                            // reset timer
                                            astarItem._unpatientTime = UnpatientTimeMax;

                                            // if same tempGoal as last, and 5 attempts made, then stop.
                                            if (astarItem._oldTempGoal == astarItem._tempGoal)
                                            {
                                                astarItem._tempGoalTries++;
                                                if (astarItem._tempGoalTries >= 5)
                                                {
                                                    // Clear solution Queue, and return to Pool
                                                    ClearSolutionFinal(astarItem); // 6/9/2010
                                                    astarItem._itemState = ItemStates.Resting;
                                                    return false;
                                                }
                                            }

                                            // 11/10/2009 - try repath solution.
                                            TrySomeRepathSolution(astarItem);
                                            return false;
                                        }

                                        astarItem._itemState = ItemStates.PausePathfinding;
                                        astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                                        astarItem._pauseTime = 0.5f;
                                        return false;
                                    default:
                                        {
                                            // 5/23/2008; 12/19/2008 - Add check for 'AttackOn' to be false for 'occupiedBy' unit.
                                            // If IgnoreOccupiedByFlag is On, then other units must move out of our way!!
                                            if (astarItem.IgnoreOccupiedByFlag == IgnoreOccupiedBy.On &&
                                                !occupiedBy.AttackOn)
                                            {
                                                // 3/6/2011 - Check if enemy player
                                                if (occupiedBy.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                                                    return false;

                                                // Was Unit able to move out of way?
                                                if (MoveOutOfTheWay(occupiedBy.AStarItemI))
                                                {
                                                    // Yes, so let's pause for sec to let unit move out of way
                                                    astarItem._itemState = ItemStates.PausePathfinding;
                                                    astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                                                    astarItem._pauseTime = 0.6f;
                                                    return false;
                                                }
                                            }

                                            // 12/23/2008 - Check if this unit has faster velocity than occupiedBy unit; if so,
                                            //              then ask them to move.
                                            if (astarItem.SceneItemOwner.MaxSpeed > occupiedBy.MaxSpeed)
                                            {
                                                // 3/6/2011 - Check if enemy player
                                                if (occupiedBy.PlayerNumber != TemporalWars3DEngine.SThisPlayer)
                                                    return false;

                                                // Was Unit able to move out of way?
                                                if (MoveOutOfTheWay(occupiedBy.AStarItemI))
                                                {
                                                    // Yes, so let's pause for sec to let unit move out of way
                                                    astarItem._itemState = ItemStates.PausePathfinding;
                                                    astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                                                    astarItem._pauseTime = 0.6f;
                                                    return false;
                                                }
                                            }

                                            // 3/1/2011 - Updated to track when an AltGoal is found.
                                            // 12/22/2008 - If this unit in AttackMode, then use different method to 
                                            //              find next suitable goalNode.
                                            if (astarItem.SceneItemOwner.AttackOn &&
                                                 !astarItem._foundAlternativeGoalAttackPosition)
                                            {
                                                // 3/1/2011 - Do check every few seconds
                                                astarItem._findAlternativeGoalCheckTime -= (float)astarItem._gameTime.ElapsedGameTime.TotalMilliseconds;
                                                if (astarItem._findAlternativeGoalCheckTime >= 0) return false;
                                                astarItem._findAlternativeGoalCheckTime = FindAlternativeGoalCheckTimeReset;

                                                // 3/1/2011
                                                Vector3 newGoal;
                                                astarItem._foundAlternativeGoalAttackPosition =
                                                    FindAlternativeGoalNodeForAttacking(astarItem, out newGoal);
                                                
                                                // 12/23/2008 - Is there alt goal to move to for attacking?
                                                if (astarItem._foundAlternativeGoalAttackPosition)
                                                {
                                                    // Clear solution Queue
                                                    ClearSolutionFinal(astarItem); // 6/9/2010

#if DEBUG
                                                    System.Console.WriteLine(@"PathtoStack");
#endif

                                                    astarItem.PathToStack.Push(newGoal);
                                                    astarItem._itemState = ItemStates.PathFindingTempGoal;
                                                    return false;
                                                }

                                                // no, so just give up.                                            
                                                // Clear solution Queue
                                                ClearSolutionFinal(astarItem); // 6/9/2010

                                                astarItem._itemState = ItemStates.Resting;
                                                return false;
                                            }

                                            // 12/23/2008 - Let's pause for 1/2 second to see if other SceneItemOwner will move?
                                            //              if not, then the 2nd Time here will trigger a temp path solution.
                                            if (!astarItem._giveChanceToMoveFirst)
                                            {
                                                astarItem._itemState = ItemStates.PausePathfinding;
                                                astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                                                astarItem._pauseTime = 0.5f;
                                                astarItem._giveChanceToMoveFirst = true;
                                                return false;
                                            }
                                            astarItem._giveChanceToMoveFirst = false;


                                            // 11/10/2009 - try some repath solution.
                                            //TrySomeRepathSolution(astarItem);

                                            return false;
                                        } // If Other Unit Moving/Resting
                                }
                            } // If Not ourselves check
                    }

                    // ELSE: We are fine to move to next Node  

                    // Remove OccupiedBy at our Old Position
                    RemoveOccupiedByAtOldPosition(astarItem);

                    // Set A* OccupiedBy for Current MoveToPosition PathNode, while checking
                    // for failure! If it fails, we will have the unit wait.
                    if (!SetOccupiedByAtMoveToPosition(astarItem))
                    {
                        /*SetOccupiedByAtCurrentPosition(astarItem);

                            // Let's pause for sec to let unit move out of way
                            astarItem._itemState = ItemStates.PausePathfinding;
                            astarItem.SceneItemOwner.Velocity = Vector3Zero; // 12/19/2008
                            astarItem._pauseTime = 0.5f;
                            return false;*/
                        throw new InvalidOperationException(
                            "(GetNextPathNode) method failed at 'SetOccupiedByAtMoveToPosition'");
                    }

                    // Now Officially DeQueue the Next Node from Queue
                    if (astarItem.SolutionFinal.Count > 0)
                    {
                        //astarItem.SolutionFinal.Dequeue();
                        Vector3 pathNode;
                        SolutionTryDequeue(astarItem, out pathNode);
                    }

                    astarItem._tempGoalTries = 0; // reset
                    return true;
                } // End If destNode is Null
            }

            astarItem._itemState = ItemStates.Resting;
            astarItem._tempGoalTries = 0; // reset

            // 3/4/2011 - Signal pathfinding operation has completed.
            if (astarItem.PathfindingGoalCompleted != null)
                astarItem.PathfindingGoalCompleted(astarItem, EventArgs.Empty);

            return false;
        }

        // 11/10/2009
        /// <summary>
        /// The given <see cref="SceneItem"/> will first try to repath around the blocked node.  If this fails,
        /// then it will try to repath to a new 'Goal' position.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        private static void TrySomeRepathSolution(AStarItem astarItem)
        {
#if DEBUG
            System.Console.WriteLine(@"TrySomeRepathSolution()");
#endif

            //Find next unoccupied walkable node in _solutionFinal Queue
            /*if (FindNextUnoccupiedWalkableNode(astarItem))
            {
                // ReCalc new Path to Temp Goal to avoid the blocking unit,
                // then continue onwards to the finalGoal.                         
                                                    
                // copy solutionFinal into solutionRePath Queue.
                IEnumerator<Vector3> tmpEnum = astarItem.SolutionFinal.GetEnumerator(); // Source Pointer                                
                while (tmpEnum.MoveNext())
                {
                    var currentNode = tmpEnum.Current;
                    astarItem._solutionRepath.Enqueue(currentNode);
                }
                ClearSolutionFinal(astarItem); // 6/9/2010

                astarItem._doingRepathAroundItem = true;
                astarItem._oldTempGoal = astarItem._tempGoal;
                astarItem.PathToStack.Push(astarItem._tempGoal); 
                astarItem._itemState = ItemStates.PathFindingTempGoal;
                return;
            }*/

            // Else there was no Unoccupied Walkable Node, so find new goalNode to move to!
            Vector3 newGoal;
            if (GetClosestFreeNode(NodeScale.TerrainScale, astarItem.UsePathNodeType, 
                ref astarItem._goalPosition, out newGoal))
            {
                // 8/25/2008: Updated to use Reference version of Distance method.
                // Close enough to the final goal or not?
                float result;
                Vector3.Distance(ref newGoal, ref astarItem._goalPosition, out result);
                if (result < 1.0f)
                {
                    // 4/16/2009
                    // Clear solution Queue, and return to Pool
                    ClearSolutionFinal(astarItem); // 6/9/2010
                    astarItem._itemState = ItemStates.Resting;
                    return;
                }

                // Clear current solution, and set _goalPosition to newGoal Position   
                ClearSolutionFinal(astarItem); // 6/9/2010
                astarItem._itemState = ItemStates.Resting; // 3/6/2011
                astarItem.PathToQueue.Enqueue(newGoal);

                return;
            } // End if newNode

            // Clear solution Queue
            ClearSolutionFinal(astarItem); // 6/9/2010
            astarItem._itemState = ItemStates.Resting;
            return;
        }


        // 4/17/2009
        /// <summary>
        /// Iterates through the <see cref="SolutionFinal"/> Queue, looking for the next
        /// unoccupied node to move to.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        private static bool FindNextUnoccupiedWalkableNode(AStarItem astarItem)
        {
            var foundAltNode = false;
            var index = 0;
            IEnumerator<Vector3> tmpEnum = astarItem.SolutionFinal.GetEnumerator(); // Source Pointer                                
            while (tmpEnum.MoveNext())
            {
                var currentNode = tmpEnum.Current;

                // 11/15/2009 - Updated to use the new 'GetOccupiedByAtIndex'.
                var index2 = new Point {X = (int) currentNode.X, Y = (int) currentNode.Z};
                object occupiedByOut;
                if (!TemporalWars3DEngine.AStarGraph.GetOccupiedByAtIndex(ref index2, NodeScale.TerrainScale, astarItem.UsePathNodeType, out occupiedByOut))
                {
                    astarItem._tempGoal.X = currentNode.X;
                    astarItem._tempGoal.Z = currentNode.Z;
                    foundAltNode = true;

                    // 1/2/2009 - Dequeue all nodes from current pos to temp, since
                    //            skipping these all together!  
                    //      Note: If these nodes aren't removed, the units would then reach their new           
                    //            Temp Position, but instead of continuing forward, they would
                    //            then go backwards, since the old nodes would still be
                    //            on the '_solutionFinal' queue!   
                    
                    for (var j = 0; j < index; j++)
                    {
                        //astarItem.SolutionFinal.Dequeue();
                        Vector3 pathNode;
                        SolutionTryDequeue(astarItem, out pathNode);
                    }

                    break;
                }

                index++;
            } // While
            return foundAltNode;
        }

        // 12/22/2008; 3/1/2011 - Updated to iterate angles.
        /// <summary>
        /// In order to find the next alternative attack Position,
        /// I am going to use the current original goal node, but 
        /// then adjust it to be translated 45 degrees (left or right)
        /// around the radius of the attackie!
        /// 
        /// How it is done;
        /// 1 - Calc direction to translate from attackie's Position to
        ///     current MoveToPosition, then Normalize direction.
        /// 2 - Get angle of current moveToPos Position, using the Atan2 function.
        /// 3 - Rotate Matrix, on the Y-axis, by 45+/45- degrees using the angle calc
        ///     in step 2. (Left or Right)     
        /// 4 - Apply Rotation Matrix to 'direction' Vector.
        /// 5 - Apply AttackRadius distance to 'direction' vector.
        /// 6 - Add 'direction' vector to attack Position to get new Position.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="newPosition">(out) New position</param>
        internal static bool FindAlternativeGoalNodeForAttacking(AStarItem astarItem, out Vector3 newPosition)
        {

#if DEBUG
            System.Console.WriteLine(@"FindAlternativeGoalNodeForAttacking()");
#endif

            // 8/13/2009 - Set Radians of 45 = 3.14 /4.
            const float adjAngle = 3.14f / 4;
            const float adjAngleNeg = adjAngle * -1;
            var rndNumber = RndGenerator.Next(1, 2);


            // 3/1/2011 - Fix: Updated to set to current unit position, rather than zero;
            //                 otherwise, the unit may move to top left corner if no solution is found.
            var attackerPos = astarItem.SceneItemOwner.Position; // 5/30/2011 - Cache
            newPosition = attackerPos;
            // 8/5/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = astarItem.SceneItemOwner.AttackSceneItem;
            if (attackie == null) return false;

            // 5/30/2011 - Optimized: Removed from iteration below.
            // 1 - Calc direction to translate from attackie's Position to
            //     current MoveToPosition, then Normalize direction.      
            Vector3 direction;
            var attackiePos = attackie.Position;
            var moveToPosition = astarItem._moveToPosition; // 5/30/2011 - Cache
            Vector3.Subtract(ref moveToPosition, ref attackiePos, out direction);
            if (!direction.Equals(Vector3Zero)) direction.Normalize(); // 11/11/09 - never normalize zero vectors.

            // 5/30/2011 - Optimized: Removed from iteration below.
            // 2 - Get angle of current moveToPos Position, using the Atan2 function.
            var angle = (float)Math.Atan2(-moveToPosition.Z, moveToPosition.X);

            // 3/1/2011 - Iterate through all 8 possible angle positions.
            for (var i = 1; i < 9; i++)
            {
                // 3 - Rotate Matrix, on the Y-axis, by 45+/45- degrees of the angle calc
                //     in step 2. (Left or Right) 
                var newAngle = rndNumber == 1 ? angle * i + adjAngle : angle * i + adjAngleNeg;

                Matrix rotTrans;
                Matrix.CreateRotationY(newAngle, out rotTrans);

                // 4 - Apply Rotation Matrix to Vector direction.
                Vector3.Transform(ref direction, ref rotTrans, out direction);

                // 11/11/2009 - Updated to apply the FULL AttackRadius distance, ONLY when the unit is outside this distance; otherwise,
                //              if the current distance is closer than the full attackRadius, it should stay at the closer distance!
                // 5 - Apply AttackRadius distance to 'direction' vector.
                {
                    // Calculate current distance from target
                    float distance;
                    Vector3.Distance(ref attackerPos, ref attackiePos, out distance);

                    // Check if distance is closer than using full AttackRadius distance?
                    var attackRadius = astarItem.SceneItemOwner.AttackRadius;
                    var distanceToUse = (distance < attackRadius) ? distance : attackRadius;

                    Vector3.Multiply(ref direction, distanceToUse, out direction);

                }

                // 6 - Add 'direction' vector to attack Position to get new Position.           
                Vector3.Add(ref attackiePos, ref direction, out newPosition);

                // 7- (4/24/2011) Verify newPosition is not already occupied.
                var isOccupied = TemporalWars3DEngine.AStarGraph.IsOccupied((int) newPosition.X, (int) newPosition.Z,
                                                                            PathNodeType.GroundItem);

                // 8 - Verify newPosition is not already blocked; has a cost of -1 or -2.
                var isBlocked = TemporalWars3DEngine.AStarGraph.IsNodeBlocked(NodeScale.TerrainScale,
                                                                              (int) newPosition.X, (int) newPosition.Z);
                // 4/24/2011
                if (!isOccupied && !isBlocked) return true;

                // since blocked, is there another close by node we can use?
                /*return GetClosestFreeNode(NodeScale.TerrainScale, astarItem.UsePathNodeType,
                                          ref newPosition, out newPosition);*/
                    

            } // End ForLoop
            
            return false;
        }

        // 8/24/2009
        private readonly Vector3[] _solutionFinalCopy = new Vector3[1];

        // 5/23/208
        /// <summary>
        /// When called, this unit will find an alternative node to reside on, so calling
        /// unit can pass through on its path.  
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance of item to move.</param>
        /// <returns> The Status of Finding Alt Goal is return to caller.</returns>
        private static bool MoveOutOfTheWay(AStarItem astarItem)
        {
            IList<Vector2> possibleGoals = astarItem._possibleGoals;
            var solutionFinalCopy = astarItem._solutionFinalCopy;
            var callingUnit = astarItem.SceneItemOwner; // 6/17/2010
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 6/17/2010

            // Get Current PathNode Position
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride; // 8/13/2009
            var pathPosition = new Vector2 { X = astarItem._pathNodePosition.X, Y = astarItem._pathNodePosition.Z };
            var goalNodeTransformations = TerrainShape.GoalNodeTransformations; // 8/13/2009

            // 6/17/2010 - Rather than Clear the List, just set the count back to zero!
            var possibleGoalsCount = 0;
            

            // 10/2/2008 - Ben: Updated to use ForLoop, rather than ForEach.
            // 8/24/2009: Changed back to copying to an array, since using an 'IEnumerator' causes Boxing/UnBoxing
            //           , thereby causing garabage on the HEAP!
            // 4/17/2009: Updated to using the 'IEnumerator', to eliminate need to copy to another array!                        
            //IEnumerator tmpEnum = callingUnit.AStarItemI._solutionFinal.GetEnumerator();
            // 8/24/2009
            var solutionFinalCount = callingUnit.AStarItemI.SolutionFinal.Count;
            if (solutionFinalCopy.Length < solutionFinalCount)
                Array.Resize(ref solutionFinalCopy, solutionFinalCount);

            callingUnit.AStarItemI.SolutionFinal.CopyTo(solutionFinalCopy, 0);
            
            for (var i = 1; i < 9; i++)
            {
                // 4/24/2009 - Optimize to use Vector2.Add to speed up on XBOX!
                Vector2 tryGoal;
                //tryGoal = pathPosition + TerrainShape.GoalNodeTransformations[loop1];
                var tmpGoalNode = goalNodeTransformations[i];
                Vector2.Add(ref pathPosition, ref tmpGoalNode, out tryGoal);

                // 11/15/2009: Updated to use new 'GetOccupiedByAtIndex'.
                // 5/18/2009: Updated to also make sure Node is not Blocked!
                // 2/4/2009: Updated to use the '_usePathNodeType'.
                // Check if tryGoal Node is Occupied
                var foundGoal = false;
                //int inTmpX = *pathNodeStride;
                //int inTmpY = *pathNodeStride;
                var index = new Point {X = (int) tryGoal.X, Y = (int) tryGoal.Y};
                object occupiedByOut;
                if (!aStarGraph.GetOccupiedByAtIndex(ref index, NodeScale.AStarPathScale, astarItem.UsePathNodeType, out occupiedByOut) &&
                    callingUnit.PathNodePosition.X != tryGoal.X && callingUnit.PathNodePosition.Z != tryGoal.Y &&
                    !aStarGraph.IsNodeBlocked(NodeScale.AStarPathScale, index.X, index.Y))
                {
                    // Now let's make sure it's not a SolutionPath Node?  
                    // To save Time, we will only check the first 4 nodes, which is
                    // in the immediate area of where we can move to.
                    var count = 0;
                    foundGoal = true;

                    // loop through solutionFinal                              
                    for (var j = 0; j < solutionFinalCount; j++)
                    {
                        var currentNode = solutionFinalCopy[j];

                        count++;
                        // If it equals a node on _solutionFinal, then we need to try again!
                        if (currentNode.X/pathNodeStride == tryGoal.X
                            && currentNode.Z/pathNodeStride == tryGoal.Y)
                            foundGoal = false;

                        if (count == 4)
                            break;
                    }
                } // End If OccupiedBy

                // Did we Find a Goal?
                if (!foundGoal) continue;

                // 6/17/2010 - Rather than use Add, just set directly into collection.
                possibleGoals[possibleGoalsCount] = tryGoal;
                possibleGoalsCount++;
                
            } // End for Loop            

            // Did we find some _possibleGoals to move to?

            // 12/24/2008 - If only one possibleGoal, then track goal node, so we avoid
            //              the possibility of getting stuck in and end-less loop of going
            //              back and forth between the same two possible goal nodes!
            var pastTries = (ICollection<Vector2>) astarItem._pastTries; // 11/10/2009
            
            if (possibleGoalsCount == 1)
            {
                // 11/28/2009 - Lock
                // Check if already tried this possible goal.
                var possibleGoal = possibleGoals[0];
                if (pastTries.Contains(possibleGoal))
                {
                    // Then stuck in endless loop, so return no possible moveTo solution!
                    pastTries.Clear();
                    return false;
                }

                // else, add to PastTries List
                pastTries.Add(possibleGoal);

                var tryGoal = possibleGoal;

                astarItem._moveToPosition.X = tryGoal.X*pathNodeStride;
                astarItem._moveToPosition.Y = callingUnit.Position.Y;
                astarItem._moveToPosition.Z = tryGoal.Y*pathNodeStride;

                // Remove OccupiedBy at our Old Position
                RemoveOccupiedByAtOldPosition(astarItem);

                // Set A* OccupiedBy for Current MoveToPosition PathNode
                if (!SetOccupiedByAtMoveToPosition(astarItem))
                    return false;

                // 1/20/2009 - If Host of MP game, send the MoveToPos command to client.
                MPGame_SendMoveCommandToClient(astarItem);

                astarItem._itemState = ItemStates.Moving;
                return true;
            }

            if (possibleGoalsCount > 1)
            {
                // 12/24/2008
                pastTries.Clear();

                // Yes, we did, so let's pick one of them randomly to use                
                var rndNumber = RndGenerator.Next(possibleGoalsCount);

                // 11/28/2009 
                var tryGoal = possibleGoals[rndNumber];

                astarItem._moveToPosition.X = tryGoal.X*pathNodeStride;
                astarItem._moveToPosition.Y = callingUnit.Position.Y;
                astarItem._moveToPosition.Z = tryGoal.Y*pathNodeStride;

                // Remove OccupiedBy at our Old Position
                RemoveOccupiedByAtOldPosition(astarItem);

                // Set A* OccupiedBy for Current MoveToPosition PathNode
                if (!SetOccupiedByAtMoveToPosition(astarItem))
                    return false;

                // 1/20/2009 - If Host of MP game, send the MoveToPos command to client.
                MPGame_SendMoveCommandToClient(astarItem);

                astarItem._itemState = ItemStates.Moving;
                return true;
            }
            return false;
        }

        // 1/20/2009
        /// <summary>
        /// Sends a MoveTo Command to the client computer during a MP game.
        /// </summary>
// ReSharper disable InconsistentNaming
        private static void MPGame_SendMoveCommandToClient(AStarItem astarItem)
// ReSharper restore InconsistentNaming
        {
            // 8/24/2009 - Cache
            var sceneItemOwner = astarItem.SceneItemOwner;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(sceneItemOwner.PlayerNumber, out player);

            // 10/20/2009 - make sure not NULL
            if (player == null) return;

            if (player.NetworkSession == null) return;

            // Are we host?
            if (!player.NetworkSession.IsHost) return;

            // Create new Command                    
            RTSCommMoveSceneItem2 moveItem;
            PoolManager.GetNode(out moveItem);

            // 8/13/2009 - Cache
            var sceneItemOwnerMoveToPosition = sceneItemOwner.MoveToPosition;

            moveItem.Clear();
            moveItem.NetworkCommand = NetworkCommands.UnitMoveOrder;
            moveItem.PlayerNumber = sceneItemOwner.PlayerNumber;
            moveItem.NetworkItemNumber = sceneItemOwner.NetworkItemNumber;
            moveItem.MoveToPos.X = sceneItemOwnerMoveToPosition.X;
            moveItem.MoveToPos.Y = sceneItemOwnerMoveToPosition.Z; // Y = Z is correct.
            moveItem.SendTime = 0;

            // Add to Queue to be sent out in next cycle                       
            NetworkGameComponent.AddCommandsForClientG(moveItem);
        }

        // 3/1/2011: Updated to include new param 'IsNodeScale'.
        // 12/23/08: Updated to also check if PossibleGoal is blocked, and return true/false of result.  
        // 5/19/2008
        /// <summary>
        /// Helper method, used to find an alternative goal node to move to, by searching
        /// the tiles around the blockedNode, using the 'goalNodeTransformtion' array.
        /// </summary>
        /// <param name="isNodeScale">Current scale of the given blocked node.</param>
        /// <param name="usePathNodeType">Node type</param>
        /// <param name="blockedNode">Node which is blocked.</param>
        /// <param name="newNode">Alternative node to use</param>
        /// <returns>True/False</returns>
        internal static bool GetClosestFreeNode(NodeScale isNodeScale, PathNodeType usePathNodeType, 
                                                ref Vector3 blockedNode, out Vector3 newNode)
        {
            // 8/13/2009 - Cache
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;
            var goalNodeTransformations = TerrainShape.GoalNodeTransformations; // 8/13/2009
            var goalNodeTransformationsCount = goalNodeTransformations.Count; // 8/13/2009

            // 3/1/2011 - Convert when in TerrainScale
            Vector2 goalPosTrans;
            if (isNodeScale == NodeScale.TerrainScale)
            {
                // Convert _goalPosition into PathNode Coordinates    
                goalPosTrans = new Vector2
                                   {
                                       X = (int) (blockedNode.X/pathNodeStride),
                                       Y = (int) (blockedNode.Z/pathNodeStride)
                                   };
            }
            else
            {
                // Set PathNode coordinates
                goalPosTrans = new Vector2
                                   {
                                      X = (int)blockedNode.X ,
                                      Y = (int)blockedNode.Z
                                   };
            }
           

            // 4/27/2009: Updated to search starting at 1, since 0 gives back same 'BlockedNode' Position!
            // Search for alt node not occupied. 
            for (var loop1 = 1; loop1 < goalNodeTransformationsCount; loop1++)
            {
                Vector2 possibleGoal;
                var tmpNodeTrans = goalNodeTransformations[loop1];
                //possibleGoal = goalPosTrans + TerrainShape.GoalNodeTransformations[loop1];
                Vector2.Add(ref goalPosTrans, ref tmpNodeTrans, out possibleGoal);

                // 11/15/2009: Updated to use new 'GetOccupiedByAtIndex'.
                // 2/4/2009: Updated to use the '_usePathNodeType'.
                // Check if possible goal node is occupied
                // 12/23/2008: Also check if possible goal node is blocked.
                var index = new Point {X = (int) possibleGoal.X, Y = (int) possibleGoal.Y};
                object occupiedByOut;
                if (TemporalWars3DEngine.AStarGraph.GetOccupiedByAtIndex(ref index, NodeScale.AStarPathScale, usePathNodeType, out occupiedByOut) ||
                    TemporalWars3DEngine.AStarGraph.IsNodeBlocked(NodeScale.AStarPathScale, (int)possibleGoal.X, (int)possibleGoal.Y))
                    continue;

                // found new goal, so let's return to user
                newNode = new Vector3
                              {
                                  X = possibleGoal.X*pathNodeStride,
                                  Y = blockedNode.Y,
                                  Z = possibleGoal.Y*pathNodeStride
                              };

                return true;
            }
            // else nothing found, so return original Position.           
            newNode = blockedNode;
            return false;
        }

#if EditMode

        // 4/24/2011: Updated 2nd param to IEnumerator<Vector3>
        // 4/17/2009: Updated to use the 'IEnumerator' of the Queue, to eliminate need for 2nd array, which creates garabage!
        /// <summary>
        /// Helper Function: Takes A* Path Nodes and populates the 'visaulPath' Array, which
        /// is used to show the Nodes onto the Terrain for Debugging purposes.
        /// </summary>  
        private static void PopulateVisualPathArray(AStarItem astarItem, IEnumerator<Vector3> tmpEnum)
        {
            astarItem._visualPath.Clear();

            // 8/25/2009 - Updated to use IEnumerator<> generic, which eliminates Unboxing for the retrieval
            //             of nodes below.
            //IEnumerator<Vector3> tmpEnum = solutionQueue.GetEnumerator();

                // Source Pointer                                
            var orangeRed = Color.OrangeRed; // 8/24/2009
            while (tmpEnum.MoveNext())
            {
                var currentNode = tmpEnum.Current;

                var nodePos = new Vector3 {X = currentNode.X, Z = currentNode.Z};
                nodePos.Y = TerrainData.GetTerrainHeight(nodePos.X, nodePos.Z);

                // Set color of Triangle
                astarItem._visualTriangle.Color = orangeRed;

                // Triangle - Point 1
                astarItem._visualTriangle.Position.X = nodePos.X;
                astarItem._visualTriangle.Position.Y = nodePos.Y;
                astarItem._visualTriangle.Position.Z = nodePos.Z;
                astarItem._visualPath.Add(astarItem._visualTriangle);

                // Triangle - Point 2
                astarItem._visualTriangle.Position.X = nodePos.X + 6;
                astarItem._visualTriangle.Position.Y = nodePos.Y;
                astarItem._visualTriangle.Position.Z = nodePos.Z + 12;
                astarItem._visualPath.Add(astarItem._visualTriangle);

                // Triangle - Point 3
                astarItem._visualTriangle.Position.X = nodePos.X - 6;
                astarItem._visualTriangle.Position.Y = nodePos.Y;
                astarItem._visualTriangle.Position.Z = nodePos.Z + 12;
                astarItem._visualPath.Add(astarItem._visualTriangle);
            }
        }

        // 8/24/2009
        private int[] _keys = new int[1];

        /// <summary>
        /// Helper Function: Takes A* Tested Nodes and populates the '_visualTestedNodes' Array, which
        /// is used to show the Nodes onto the Terrain for Debugging purposes.
        /// </summary>  
        private static void PopulateVisualTestedNodesArray(AStarItem astarItem, IDictionary<int, Vector3> testedNodes)
        {
            astarItem._visualTestedNodes.Clear();

            // 8/24/2009 - get dictionary _keys
            var keysCount = testedNodes.Keys.Count;
            if (astarItem._keys.Length < keysCount)
            {
                Array.Resize(ref astarItem._keys, keysCount);
                testedNodes.Keys.CopyTo(astarItem._keys, 0);
            }

            // 8/25/2008 - Updated to For-Loop, rather than ForEach statement
            var yellow = Color.Yellow; // 8/24/2009
            for (var loop1 = 0; loop1 < keysCount; loop1++)
            {
                // 8/24/2009 - Cache
                var testedNode = testedNodes[astarItem._keys[loop1]];

                // Set color of Triangle
                astarItem._visualTriangle.Color = yellow;

                // Triangle - Point 1
                astarItem._visualTriangle.Position = testedNode;
                astarItem._visualTestedNodes.Add(astarItem._visualTriangle);

                // Triangle - Point 2
                astarItem._visualTriangle.Position.X = testedNode.X + 6;
                astarItem._visualTriangle.Position.Y = testedNode.Y;
                astarItem._visualTriangle.Position.Z = testedNode.Z + 12;
                astarItem._visualTestedNodes.Add(astarItem._visualTriangle);

                // Triangle - Point 3
                astarItem._visualTriangle.Position.X = testedNode.X - 6;
                astarItem._visualTriangle.Position.Y = testedNode.Y;
                astarItem._visualTriangle.Position.Z = testedNode.Z + 12;
                astarItem._visualTestedNodes.Add(astarItem._visualTriangle);
            }
        }

#endif

       
        /// <summary>
        /// Helper Function: Takes given A* solution and checks all solution Nodes to see 
        /// if all are necessary, by checking if you can move from N1 to N3 unobstructed; if so,
        /// we then remove the middle Node, and start the check over again on next set of Nodes.       
        /// </summary> 
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param> 
        /// <param name="solutionQueue"><see cref="Vector3"/> solution queue.</param>
        private static Vector3 SmoothingPathAlgorithm(AStarItem astarItem, LocklessQueue<Vector3> solutionQueue)
        {
            // Create 2nd & 3rd copy of Queue in order to iterate through it at different node positions.
            var solutionQueue2 = new Queue<Vector3>(solutionQueue);

            // Get three pointers to Queue's
            IEnumerator<Vector3> tmpEnum = solutionQueue.GetEnumerator(); // Source Pointer
            IEnumerator<Vector3> tmpEnum2 = solutionQueue2.GetEnumerator(); // Middle Pointer
            IEnumerator<Vector3> tmpEnum3 = solutionQueue2.GetEnumerator(); // Dest Pointer

            // Move 2nd & 3rd pointers forward 
            tmpEnum2.MoveNext(); // set at 1st Node
            tmpEnum3.MoveNext();
            tmpEnum3.MoveNext(); // set at 2nd Node

            //
            // Get First Node & Insert into _solutionFinal Queue
            //
            // 2nd pointer is used first because it has already at the 1st node Position, while
            // the 1st pointer is not pointing at anything, and will be initiated at the While() fn.
            var firstNode = tmpEnum2.Current;
            astarItem.SolutionFinal.Enqueue(firstNode);

            Vector3 sourceNode2;
            var destNode2 = Vector3Zero;

            // For Smoothing, we are comparing between the 1st pointer Node and 3rd pointer Node
            // to see if anything is in between; if not, we throw away the 2nd Node, and advance 
            // the 2nd pointer forward to be the 3rd, and the 3rd pointer gets advance to compare
            // the next 'Destination' node.
            tmpEnum.MoveNext();
            tmpEnum2.MoveNext();
            while (tmpEnum3.MoveNext())
            {
                destNode2 = tmpEnum3.Current; // Destination Node 
                //destNode2 = new AStarNode2D(null, null, 0, ((destNode.X * PathNodeStride) - widthOffset),
                //((destNode.Y * PathNodeStride) - heightOffset), 0);
                var middleNode2 = tmpEnum2.Current;
                //middleNode2 = new AStarNode2D(null, null, 0, ((middleNode.X * PathNodeStride) - widthOffset),
                //((middleNode.Y * PathNodeStride) - heightOffset), 0);
                sourceNode2 = tmpEnum.Current; // Source Node  
                //sourceNode2 = new AStarNode2D(null, null, 0, ((sourceNode.X * PathNodeStride) - widthOffset),
                //((sourceNode.Y * PathNodeStride) - heightOffset), 0);

                //
                // Create Ray from Source Node to Destination Node
                //

                // 1st - Get Height for source Node Position.
                Vector3 normal;
                var sourcePos = new Vector3(sourceNode2.X, 0, sourceNode2.Z);
                //terrainData.GetHeightAndNormal(sourcePos, out sourcePos.Y, out normal);
                sourcePos.Y = TerrainData.GetTerrainHeight(sourcePos.X, sourcePos.Z);
                // 5/18/2010: Updated to use overload version#2 of GetNormal method call.
                TerrainData.GetNormal(ref sourcePos, out normal);

                // 2nd - Get Height for dest Node Position.
                var destPos = new Vector3(destNode2.X, 0, destNode2.Z);
                //terrainData.GetHeightAndNormal(destPos, out destPos.Y, out normal);
                destPos.Y = TerrainData.GetTerrainHeight(destPos.X, destPos.Z);
                // 5/18/2010: Updated to use overload version#2 of GetNormal method call.
                TerrainData.GetNormal(ref destPos, out normal);

                // 3rd - Create Ray                
                var direction = destPos - sourcePos;
                direction.Normalize();
                var testRay = new Ray(sourcePos, direction);

                // 4th - Get all Scene Items contain in Terrain Scene & iterate checking
                //       if Path clear between Source Node & Dest Node.               
                var items = astarItem.SceneItemOwner.TerrainShape.ScenaryItems;
                var isClearBetweenNodes = true;
                var count = items.Count; // 6/3/2010
                for (var j = 0; j < count; j++)
                {
                    // Check if Model in Scene was Picked              
                    var shapeWithPick = (ShapeWithPick) items[j].ShapeItem; // Cast up

                    float? distance; // 2/2/2010
                    if (shapeWithPick.IsMeshPicked(ref testRay, out distance))
                    {
                        // Ray between Source and Dest is not clear, therefore we keep Middle Node
                        isClearBetweenNodes = false;
                    }
                }

                // 5th - Check outcome; if not Clear, Add Middle Node to _solutionFinal Queue
                //       and advance Middle Node Pointer.
                if (isClearBetweenNodes)
                {
                    tmpEnum2.MoveNext();
                }
                else
                {
                    astarItem.SolutionFinal.Enqueue(middleNode2);
                    tmpEnum.MoveNext();
                    tmpEnum2.MoveNext();
                }
            }

            // Insert Final Node into _solutionFinal Queue
            astarItem.SolutionFinal.Enqueue(destNode2);

            // DeQueue first node
            astarItem.SolutionFinal.TryDequeue(out firstNode); // 6/9/2010 - Updated to TryDequeue.

            return firstNode;
        }

        ///<summary>
        /// Sets a Cost Value in the A* Algorithm at the given MapNode.
        ///</summary>
        ///<param name="astarItem">AStarItem instance</param>
        ///<param name="cost">Cost value to set</param>
        ///<param name="size">Size to affect</param>
        public static void SetCostAtCurrentPosition(AStarItem astarItem, int cost, int size)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return;

            // Add new Point with Cost and Size Affected to A* Node Array
            var sceneItemOwnerPosition = astarItem.SceneItemOwner.Position; // 8/13/2009
            var inTmpX = (int) sceneItemOwnerPosition.X;
            var inTmpY = (int) sceneItemOwnerPosition.Z;

            // 1/13/2010
            if (TemporalWars3DEngine.AStarGraph != null)
                TemporalWars3DEngine.AStarGraph.SetCostToPos(inTmpX, inTmpY, cost, size);
        }

        // 5/13/2008
        ///<summary>
        /// Remove Cost Value from the A* Graph at the current Position.
        ///</summary>
        ///<param name="astarItem">AStarItem instance</param>
        public static void RemoveCostAtCurrentPosition(AStarItem astarItem)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return;

            var sceneItemOwnerPosition = astarItem.SceneItemOwner.Position; // 8/13/2009
            var inTmpX = (int) sceneItemOwnerPosition.X;
            var inTmpY = (int) sceneItemOwnerPosition.Z;

            var pathBlockSize = (astarItem.SceneItemOwner.ShapeItem as IInstancedItem).PathBlockSize;

            // 1/13/2010
            if (TemporalWars3DEngine.AStarGraph != null)
                TemporalWars3DEngine.AStarGraph.RemoveCostAtPos(inTmpX, inTmpY, pathBlockSize);
        }


        // 4/28/2008
        // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
        // 2/4/2009: Updated to use the new '_usePathNodeType' enum.
        ///<summary>
        /// Sets the Current SceneItemOwner as OccupiedBy in the A* algorithm
        ///</summary>
        ///<param name="astarItem">AStarItem instance</param>
        ///<returns>True/False of result</returns>
        public static bool SetOccupiedByAtCurrentPosition(AStarItem astarItem)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return false;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return true;
           
            astarItem.OccupiedAtIndex = astarItem.PathNodePosition;
           
            // 11/15/2009 - Updated to use the new 'SetOccupiedByToIndex'.
            var index = new Point {X = (int)astarItem.OccupiedAtIndex.X, Y = (int)astarItem.OccupiedAtIndex.Z};

            // 1/13/2010
            return TemporalWars3DEngine.AStarGraph == null || 
                TemporalWars3DEngine.AStarGraph.SetOccupiedByToIndex(ref index, NodeScale.AStarPathScale, astarItem.UsePathNodeType, astarItem.SceneItemOwner);
        }

        // 5/18/2008; 8/24/2009: Updated to be STATIC, and only pass in 'AstarItem'.
        // 2/4/2009: Updated to use the new '_usePathNodeType' enum.
        /// <summary>
        /// Sets the Current SceneItemOwner as OccupiedBy in the A* algorithm
        /// </summary>
        /// <param name="astarItem">AStarItem instance</param>
        /// <returns>True/False result</returns>
        private static bool SetOccupiedByAtMoveToPosition(AStarItem astarItem)
        {
            // 8/13/2009 - Cache
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return false;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return true;

            var moveToPosition1 = astarItem.MoveToPosition;

            // 11/15/2009 - Convert into AStarScale index value.
            astarItem.OccupiedAtIndex = new Vector3
                                            {
                                                X = (int)(moveToPosition1.X / pathNodeStride), 
                                                Y = 0,
                                                Z = (int)(moveToPosition1.Z / pathNodeStride)
                                            };

            // TODO: Crash for Nullable 'Value' below.
            // 11/15/2009 - Updated to use the new 'SetOccupiedByToIndex'.
            var index = new Point { X = (int)astarItem.OccupiedAtIndex.X, 
                Y = (int)astarItem.OccupiedAtIndex.Z };

            // 1/13/2010
            return TemporalWars3DEngine.AStarGraph == null || 
                TemporalWars3DEngine.AStarGraph.SetOccupiedByToIndex(ref index, NodeScale.AStarPathScale, astarItem.UsePathNodeType, astarItem.SceneItemOwner);
        }

        // 11/16/2009
        /// <summary>
        /// Sets the OccupiedBy to the given goal position.  Should be used when an item
        /// is 1st coming out of a building, and the goal position needs to be locked down
        /// immediately!
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="goalPosition"><see cref="Vector3"/> position to set</param>
        /// <returns>True/False</returns>
        internal static bool SetOccupiedByToGivenPosition(AStarItem astarItem, ref Vector3 goalPosition)
        {
            // 8/13/2009 - Cache
            const int pathNodeStride = TemporalWars3DEngine._pathNodeStride;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return false;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return true;
            
            // 11/15/2009 - Convert into AStarScale index value.
            astarItem.OccupiedAtIndex = new Vector3
                                            {
                                                X = (int) (goalPosition.X/pathNodeStride),
                                                Y = 0,
                                                Z = (int) (goalPosition.Z/pathNodeStride)
                                            };

            // 11/15/2009 - Updated to use the new 'SetOccupiedByToIndex'.
            var index = new Point { X = (int)astarItem.OccupiedAtIndex.X, Y = (int)astarItem.OccupiedAtIndex.Z };

            // 1/13/2010
            return TemporalWars3DEngine.AStarGraph == null || 
                TemporalWars3DEngine.AStarGraph.SetOccupiedByToIndex(ref index, NodeScale.AStarPathScale, astarItem.UsePathNodeType, astarItem.SceneItemOwner);
        }

        // 8/24/2009: Updated to be STATIC, and only pass in 'AstarItem'.
        // 2/4/2009: Updated to use the new '_usePathNodeType' enum.
        // 4/28/2008
        ///<summary>
        /// Remove the current <see cref="SceneItem"/> as OccupiedBy in the A* algorithm
        ///</summary>
        ///<param name="astarItem"><see cref="AStarItem"/> instance</param>
        ///<returns>True/False of result</returns>
        public static bool RemoveOccupiedByAtOldPosition(AStarItem astarItem)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            if (!TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer))
                return false;

            // 7/6/2009 - If ClientSide of MP-game, then skip operation.
            if (thisPlayer.NetworkSession != null && !thisPlayer.NetworkSession.IsHost)
                return true;

            // 11/15/2009 - Updated to use the new 'RemoveOccupiedAtIndex'.
            var index = new Point
                            {
                                X = (int) astarItem.OccupiedAtIndex.X,
                                Y = (int) astarItem.OccupiedAtIndex.Z
                            };

            var result = TemporalWars3DEngine.AStarGraph == null ||
                         TemporalWars3DEngine.AStarGraph.RemoveOccupiedByAtIndex(ref index, NodeScale.AStarPathScale,
                                                                                 astarItem.UsePathNodeType);

            astarItem.OccupiedAtIndex = Vector3.Zero;
            return result;
           
        }

        #endregion

        // 6/17/2010
        #region TriggerEvent methods

        // 6/17/2010
        /// <summary>
        /// Triggers the <see cref="PathStateUpdating"/> event.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="totalRealTime"><see cref="TimeSpan"/> as total real time</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elasped game time</param>
        private static void DoPathStateUpdatingEvent(AStarItem astarItem, ref TimeSpan totalRealTime, ref TimeSpan elapsedGameTime)
        {
            if (astarItem.PathStateUpdating == null) return;

            // 6/17/2010 - Cache
            var pathFindingStateArgs = astarItem._pathFindingStateArgs;
            if (pathFindingStateArgs == null) return;

            pathFindingStateArgs.ItemState = astarItem.ItemState;
            pathFindingStateArgs.Time = totalRealTime;
            pathFindingStateArgs.ElapsedTime = elapsedGameTime;

            astarItem.PathStateUpdating(astarItem, pathFindingStateArgs);
        }

        // 6/17/2010
        /// <summary>
        /// Triggers the <see cref="PathStateUpdated"/> event.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="totalRealTime"><see cref="TimeSpan"/> as total real time</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elasped game time</param>
        private static void DoPathStateUpdatedEvent(AStarItem astarItem, ref TimeSpan totalRealTime, ref TimeSpan elapsedGameTime)
        {
            if (astarItem.PathStateUpdated == null) return;

            // 6/17/2010 - Cache
            var pathFindingStateArgs = astarItem._pathFindingStateArgs;
            if (pathFindingStateArgs == null) return;

            pathFindingStateArgs.ItemState = astarItem.ItemState;
            pathFindingStateArgs.Time = totalRealTime;
            pathFindingStateArgs.ElapsedTime = elapsedGameTime;

            astarItem.PathStateUpdated(astarItem, pathFindingStateArgs);
        }

        // 6/17/2010
        /// <summary>
        /// Triggers the <see cref="PathMoveToCompletedG"/> event.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="totalRealTime"><see cref="TimeSpan"/> as total real time</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elasped game time</param>
        private static void DoPathMoveToCompletedGEvent(AStarItem astarItem, ref TimeSpan totalRealTime, ref TimeSpan elapsedGameTime)
        {
            if (PathMoveToCompletedG == null) return;

            // 6/17/2010 - Cache
            var pathFindingStateArgs = astarItem._pathFindingStateArgs;
            if (pathFindingStateArgs == null) return;

            pathFindingStateArgs.ItemState = astarItem.ItemState;
            pathFindingStateArgs.Time = totalRealTime;
            pathFindingStateArgs.ElapsedTime = elapsedGameTime;

            PathMoveToCompletedG(astarItem, pathFindingStateArgs);
        }

        // 6/17/2010
        /// <summary>
        /// Triggers the <see cref="PathMoveToCompleted"/> event.
        /// </summary>
        /// <param name="astarItem"><see cref="AStarItem"/> instance</param>
        /// <param name="totalRealTime"><see cref="TimeSpan"/> as total real time</param>
        /// <param name="elapsedGameTime"><see cref="TimeSpan"/> as elasped game time</param>
        private static void DoPathMoveToCompletedEvent(AStarItem astarItem, ref TimeSpan totalRealTime, ref TimeSpan elapsedGameTime)
        {
            if (astarItem.PathMoveToCompleted == null) return;

            // 6/17/2010 - Cache
            var pathFindingStateArgs = astarItem._pathFindingStateArgs;
            if (pathFindingStateArgs == null) return;

            pathFindingStateArgs.ItemState = astarItem.ItemState;
            pathFindingStateArgs.Time = totalRealTime;
            pathFindingStateArgs.ElapsedTime = elapsedGameTime;

            astarItem.PathMoveToCompleted(astarItem, pathFindingStateArgs);
        }

        #endregion

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (_tShapeHelper != null)
                _tShapeHelper.Dispose();

            // Clear Arrays                
            if (SolutionFinal != null)
                ClearSolutionFinal(this); // 6/9/2010
            if (_solutionRepath != null) // 11/10/09
                _solutionRepath.Clear();
            if (_visualPath != null)
                _visualPath.Clear();
            if (_visualTestedNodes != null)
                _visualTestedNodes.Clear();

            // Null Refs                
            SolutionFinal = null;
            _tShapeHelper = null;
            _visualPath = null;
            _visualTestedNodes = null;
        }
    }
}