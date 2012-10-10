#region File Description
//-----------------------------------------------------------------------------
// ForceBehaviorsCalculator.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Structs;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.TurretBehaviors;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

#if XBOX360
using ImageNexus.BenScharbach.TWLate.Xbox360Generics;
#endif

namespace ImageNexus.BenScharbach.TWEngine.ForceBehaviors
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ForceBehaviors"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.ForceBehaviors"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 5/22/2012 - Renamed from SteeringBehavior to ForceBehavior
    /// <summary>
    /// The <see cref="ForceBehaviorsCalculator"/> class is where the actual workload is done
    /// to call and calculate any forces applies to some <see cref="SceneItem"/>, and
    /// return the current calculated force to use for the given game cycle.  Every <see cref="SceneItem"/> has
    /// its own instance of the <see cref="ForceBehaviorsCalculator"/>, which is called and updated by the 
    /// <see cref="ForceBehaviorsManager"/>.
    /// </summary>
    public class ForceBehaviorsCalculator
    {
        // 12/30/2008
        internal NetworkSession NetworkSession;

        // Thread Locks
        internal static readonly object BehaviorsNeighborsThreadLock = new object();
        
        // 11/14/2008 - Thread Atts
        private Vector3 _threadForceResult;

        // 6/12/2010 - Split up the Vector3 steering values into 3 ints; allows for use of Interlocked.Exchange!
        private int _threadSteeringForceX;
        private int _threadSteeringForceY;
        private int _threadSteeringForceZ;

        // 6/12/2010 - Updated to light-weight TimeSpan.
        private BehaviorsTimeSpan _threadElapsedTime;

#if XBOX360
        // List of current Behaviors for given instance.      
        internal volatile SortedList<int, AbstractBehavior> Behaviors = new SortedList<int, AbstractBehavior>(5);
#else
        // List of current Behaviors for given instance.      
        internal volatile SortedList<int, AbstractBehavior> Behaviors = new SortedList<int, AbstractBehavior>(5);
#endif

        // The owner of this instance
        internal volatile SceneItem SceneItemOwner;
        //this is used to multiply the steering force AND all the multipliers
        //found in ForceBehavior
        private const float SteeringForceTweaker = 200.0f;

        //these can be used to keep track of friends, pursuers, or prey
        internal SceneItem TargetItem1;
        internal SceneItem TargetItem2;

        // 10/14/2008 - Neighbors
        ///<summary>
        /// Neighbors collection beginning capacity value.
        ///</summary>
        public const int NeighborsBegCapacity = 100;

        // 6/8/2010 - Updated from List<T> to now use a simple array of SceneItem[].
        private readonly SceneItemWithPick[] _neighborsGround = new SceneItemWithPick[NeighborsBegCapacity];
        private readonly SceneItemWithPick[] _neighborsAir = new SceneItemWithPick[NeighborsBegCapacity];

        // 6/8/2010 - New Keys count will store the valid count for the LocklessDictionaries, thereby eliminating the
        //            need to remove neighbors from the list; the cause of extra garbage on Xbox.
        private int _neighborsGroundKeysCount;
        private int _neighborsAirKeysCount;

        

        // 10/16/2008 - Obstacles - 
        /// <summary>
        /// Will include <see cref="SceneItem"/> from both the 'Selectable' and 'Scenary' collections,
        /// which have the Interface 'Obstacle'.
        /// </summary>
        internal static List<SceneItem> Obstacles;
       

        #region Properties

        /// <summary>
        /// When set, this will populate the Neighbors for Air units.
        /// </summary>
        public bool PopulateNeighborsAir { get; set; }

        /// <summary>
        /// When set, this will populate the Neighbors for Ground units.
        /// </summary>
        public bool PopulateNeighborsGround { get; set; }

        // 8/4/2009
        /// <summary>
        /// When set, this will force the <see cref="ForceBehaviorsManager"/> to
        /// use Version-2 of GetNeighbors method.  This version uses the
        /// collision view test to retrieve neighbors when the A* graph cannot!
        /// </summary>
        public bool UseGetNeighborsVersion2 { get; set; }

        /// <summary>
        /// Returns the current force result.
        /// </summary> 
        internal Vector3 ThreadForceResult
        {
            get
            {
                // 6/12/2010 - Assemble values
                _threadForceResult.X = _threadSteeringForceX;
                _threadForceResult.Y = _threadSteeringForceY;
                _threadForceResult.Z = _threadSteeringForceZ;

                return _threadForceResult; 
            }

        }

        /// <summary>
        /// Set the internal Elapsed game time.
        /// </summary> 
        internal TimeSpan ThreadElapsedTime
        {
            set
            {
                // 6/12/2010 - Store in light-weight BehaviorTimeSpan version.
                _threadElapsedTime.Ticks = value.Ticks;
            }
        }

        // 6/8/2010; 6/9/2010 - Updated to only Interlocked on Set.
        /// <summary>
        /// Retrieves the current iteration count for ground neighbors.
        /// </summary>
        /// <remarks>This property is Thread-Safe.</remarks>
        public int NeighborsGroundKeysCount
        {
            get
            {
                return _neighborsGroundKeysCount;
            }
            set
            {
                Interlocked.Exchange(ref _neighborsGroundKeysCount, value);
            }
        }

        // 6/8/2010; 6/9/2010 - Updated to only Interlocked on Set.
        /// <summary>
        /// Retrieve the current iteration count for air neighbors.
        /// </summary>
        /// <remarks>This property is Thread-Safe.</remarks>
        public int NeighborsAirKeysCount
        {
            get
            {
                return _neighborsAirKeysCount;
            }
            set
            {
                Interlocked.Exchange(ref _neighborsAirKeysCount, value);
            }
        }

        #endregion

        ///<summary>
        /// Constructor, which checks if attached <see cref="SceneItemWithPick"/> parent is of ground or air
        /// <see cref="ItemGroupType"/>, to set the proper flags; <see cref="PopulateNeighborsGround"/> or <see cref="PopulateNeighborsAir"/>.
        /// Also, the <see cref="SceneItem.SceneItemCreated"/> <see cref="EventHandler"/> is attached to the <see cref="BuildingCreated"/> event.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        ///<param name="item"><see cref="SceneItemWithPick"/> instance</param>
        public ForceBehaviorsCalculator(Game game, SceneItem item)
        {
            SceneItemOwner = item;

            // Populate Obstacles
            PopulateObstacles();

            // 10/16/2008 - Add Event Handler for the 'BuildingScene' Created.           
            SceneItem.SceneItemCreated += BuildingCreated;

            // 4/15/2009 - Set if Populate 'Ground' Neighbors.
            // 3/23/2009: Updated to check using bitwise comparison, since enum is a bitwise enum!
            var sceneItemWithPick = item as SceneItemWithPick; // 5/20/2012
            if (sceneItemWithPick != null)
            {
                if (((int)sceneItemWithPick.ItemGroupTypeToAttack & (int)(ItemGroupType.Buildings | ItemGroupType.Shields | ItemGroupType.Vehicles)) != 0)
                    PopulateNeighborsGround = true;

                // 4/15/2009 - Set if Populate 'Air' Neighbors.
                if (((int)sceneItemWithPick.ItemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                    PopulateNeighborsAir = true;
            }

            // 12/30/2008 - Get NetworkSession from NetworkGameComponent service.
            var networkGameComponent = (NetworkGameComponent)game.Services.GetService(typeof(NetworkGameComponent));
            if (networkGameComponent != null)
                NetworkSession = NetworkGameComponent.NetworkSession;
        }

        // 6/8/2009 - 
        /// <summary>
        /// The 'Neighbors_Ground' collection
        /// </summary>
        /// <param name="neighbors">Returns reference to the ground neighbors</param>
        internal void GetNeighborsGround(ref SceneItemWithPick[] neighbors)
        {
            // Set reference to internal collection.
            neighbors = _neighborsGround;
        }

        // 6/8/2009 -

        /// <summary>
        /// The 'Neighbors_Air' List
        /// </summary>
        /// <param name="neighbors">Return reference to the air neigbors</param>
        internal void GetNeighborsAir(ref SceneItemWithPick[] neighbors)
        {
            // Set reference to internal collection.
            neighbors = _neighborsAir;
        }

        // 11/14/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Clear List Arrays
            if (_neighborsGround != null) Array.Clear(_neighborsGround, 0, _neighborsGround.Length);
            if (_neighborsAir != null) Array.Clear(_neighborsAir, 0, _neighborsAir.Length); // 6/8/2010
            if (Obstacles != null) Obstacles.Clear();

            // 11/15/2008 - Remove EventHandler
            SceneItem.SceneItemCreated -= BuildingCreated;

            // Iterate and dispose of resources.
            foreach (var behavior in Behaviors)
            {
                behavior.Value.Dispose();
            }
            Behaviors.Clear();

            // Null refs
            Behaviors = null;
            SceneItemOwner = null;
            TargetItem1 = null;
            TargetItem2 = null;
            Obstacles = null;

            NetworkSession = null;
        }

        #region Calculate Accum Steering Force Methods

        

        // 6/7/2010: Updated to now use the new LocklessQueue for storing the force value.
        /// <summary>
        /// Calculates the accumulated steering force of all
        /// <see cref="Behaviors"/> contain in the collection.
        /// </summary>
        public void Calculate()
        {
            Vector3 newSteeringForceValue;
            Calculate(this, out newSteeringForceValue);

            // 6/12/2010 - Enqueue new force value.
            Interlocked.Exchange(ref _threadSteeringForceX, (int)newSteeringForceValue.X);
            Interlocked.Exchange(ref _threadSteeringForceY, (int)newSteeringForceValue.Y);
            Interlocked.Exchange(ref _threadSteeringForceZ, (int)newSteeringForceValue.Z);

        }

        // 2/16/2010 - Static members are shared with Threads, so made this one NOT static.
        private volatile AbstractBehavior[] _abstractBehaviorSortValues = new AbstractBehavior[100];
        
        // 6/1/2010: Removed the behaviorSortValues param, since can access from first param.
        // 5/11/2009; 8/25/2009: Updated to be STATIC.
        /// <summary>
        /// calculates the accumulated steering force 
        /// </summary>
        private static void Calculate(ForceBehaviorsCalculator forceBehavior, out Vector3 threadSteeringForce)
        {
            /*if (ForceBehavior.SceneItemOwner.ShapeItem.ItemType == ItemType.sciFiAAGun05)
                Debugger.Break();*/

            var item = forceBehavior.SceneItemOwner;
            var neighborsGround = forceBehavior._neighborsGround;
            var neighborsAir = forceBehavior._neighborsAir;
            var populateNeighborsGround = forceBehavior.PopulateNeighborsGround;
            var populateNeighborsAir = forceBehavior.PopulateNeighborsAir;
            var useGetNeighborsV2 = forceBehavior.UseGetNeighborsVersion2;
            var behaviors = forceBehavior.Behaviors;


            //reset the steering force  
            var steeringForce = Vector3Zero;

            // Update Local Thread variables  
            var threadTmpElapsedTime = forceBehavior._threadElapsedTime;

            // 8/4/2009 - Which GetNeighbors version to use?
            if (forceBehavior.SceneItemOwner is SceneItemWithPick) // 5/20/2012
            {
                if (useGetNeighborsV2)
                {
                    if (populateNeighborsGround)
                        GetNeighborsV2(forceBehavior, neighborsGround, false);

                    if (populateNeighborsAir)
                        GetNeighborsV2(forceBehavior, neighborsAir, true);
                }
                else
                {
                    if (populateNeighborsGround)
                        GetNeighbors(forceBehavior, PathNodeType.GroundItem, neighborsGround);

                    if (populateNeighborsAir)
                        GetNeighbors(forceBehavior, PathNodeType.AirItem, neighborsAir);
                }
            }

            var count = behaviors.Count;


            // Copy Sort-Values
            if (forceBehavior._abstractBehaviorSortValues.Length < count)
#pragma warning disable 420
                Array.Resize(ref forceBehavior._abstractBehaviorSortValues, count);
#pragma warning restore 420
            behaviors.Values.CopyTo(forceBehavior._abstractBehaviorSortValues, 0);

            // 6/1/2010 - Cache
            var behaviorSortValues = forceBehavior._abstractBehaviorSortValues;

            // Iterate through Array of Behaviors
            for (var i = 0; i < count; i++)
            {
                // 4/15/2009
                var behavior = behaviorSortValues[i];
                if (behavior == null) continue; // 11/28/2009

                // First check to see if AbstractBehavior set to use
                if (!behavior.UseBehavior)
                    continue;

                Vector3 force;
                behavior.Update(ref threadTmpElapsedTime, item, out force);

                // 11/19/2008 - Optimize by removing Vector Overload operations, which are slow on XBOX!                    
                var threadTmpBehaviorWeightResult = (behavior.BehaviorWeight*SteeringForceTweaker);
                Vector3.Multiply(ref force, threadTmpBehaviorWeightResult, out force);

                // If Total Force Reached, then break out of loop
                if (!AccumulateForce_Thread(item, ref steeringForce, ref force))
                    break;
            } // End For Loop Behaviors

            // 8/4/2009 - Check for NaN!
            {
                if (float.IsNaN(steeringForce.X))
                    steeringForce.X = 0.0f;

                if (float.IsNaN(steeringForce.Y))
                    steeringForce.Y = 0.0f;

                if (float.IsNaN(steeringForce.Z))
                    steeringForce.Z = 0.0f;
            }

            // Update AStarItem's copy of steeringForce.
            threadSteeringForce = steeringForce;
        }

        #endregion

        /// <summary>
        /// Adds a new <see cref="AbstractBehavior"/> to the Prioritized AbstractBehavior list
        /// </summary>
        /// <param name="behaviorToAdd">AbstractBehavior to add.</param>
        /// <returns>Returns an instance of <see cref="AbstractBehavior"/>.</returns>
        public AbstractBehavior Add(BehaviorsEnum behaviorToAdd)
        {
            // Create requested AbstractBehavior
            AbstractBehavior abstractBehavior = null;
            switch (behaviorToAdd)
            {
                case BehaviorsEnum.Alignment:
                    abstractBehavior = new AlignmentBehavior();
                    break;
                case BehaviorsEnum.Arrive:
                    abstractBehavior = new ArriveBehavior();
                    break;
                case BehaviorsEnum.Cohesion:
                    abstractBehavior = new CohesionBehavior();
                    break;
                case BehaviorsEnum.Flee:
                    abstractBehavior = new FleeBehavior();
                    break;
                case BehaviorsEnum.FollowPath:
                    abstractBehavior = new FollowPathBehavior();
                    break;
                case BehaviorsEnum.OffsetPursuit:
                    abstractBehavior = new OffsetPursuitBehavior();
                    break;
                case BehaviorsEnum.ObstacleAvoidance:
                    abstractBehavior = new ObstacleAvoidanceBehavior();
                    break;
                case BehaviorsEnum.Seek:
                    abstractBehavior = new SeekBehavior();
                    break;
                case BehaviorsEnum.Separation:
                    abstractBehavior = new SeparationBehavior();
                    break;
                case BehaviorsEnum.TurnToFace:
                    abstractBehavior = new TurnToFaceBehavior();
                    break;
                case BehaviorsEnum.TurnTurret:
                    abstractBehavior = new TurnTurretBehavior();
                    break;
                case BehaviorsEnum.UpdateOrientation:
                    abstractBehavior = new UpdateOrientationBehavior();
                    break;
                case BehaviorsEnum.Wander:
                    abstractBehavior = new WanderBehavior();
                    break;
                case BehaviorsEnum.DefenseTurretBehavior:
                    abstractBehavior = new DefenseTurretBehavior();
                    break;
                default:
                    break;
            }

            if (abstractBehavior != null)
            {
                abstractBehavior.ForceBehaviorManager = this;
                // 8/9/2009 - Make sure does not already contain the AbstractBehavior.
                if (!Behaviors.ContainsKey((int) behaviorToAdd))
                    Behaviors.Add((int) behaviorToAdd, abstractBehavior);

                return abstractBehavior;
            }
            return null;
        }

        // 10/14/2009
        /// <summary>
        /// Searches the internal <see cref="Behaviors"/> collection for the given <see cref="BehaviorsEnum"/> to
        /// retrieve; if found, it is returned to the caller.
        /// </summary>
        /// <param name="behaviorToRetrieve">Behaviors Enum to search for</param>
        /// <returns>Instance of AbstractBehavior</returns>
        public AbstractBehavior GetBehavior(BehaviorsEnum behaviorToRetrieve)
        {
            // Get AbstractBehavior
            var behavior = Behaviors[(int)behaviorToRetrieve];

            // check if AbstractBehavior is what caller looking for
            return behavior.BehaviorType == behaviorToRetrieve ? behavior : null;
        }

        /// <summary>
        /// Removes a <see cref="BehaviorsEnum"/> from the Prioritized <see cref="Behaviors"/> collection
        /// </summary>
        /// <param name="behaviorToRemove">Instance of <see cref="behaviorToRemove"/>.</param>
        public void Remove(BehaviorsEnum behaviorToRemove)
        {
            // Removes the AbstractBehavior using the Enum Value as sortKey.
            Behaviors.Remove((int)behaviorToRemove);
        }

        
        /// <summary>
        /// This function calculates how much of its max steering force the 
        /// <see cref="SceneItem"/> has left to apply and then applies that amount of the
        /// force to add.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance</param>
        /// <param name="runningTot">Running total</param>
        /// <param name="forceToAdd">Force value to add</param>
        /// <returns>true/false of result</returns>        
        private static bool AccumulateForce_Thread(SceneItem sceneItem, ref Vector3 runningTot, ref Vector3 forceToAdd)
        {
            //calculate how much steering force the vehicle has used so far
            var magnitudeSoFar = runningTot.Length();

            //calculate how much steering force remains to be used by this vehicle
            var magnitudeRemaining = sceneItem.MaxForce - magnitudeSoFar;

            //return false if there is no more force left to use
            if (magnitudeRemaining <= 0.0) return false;

            //calculate the magnitude of the force we want to add
            var magnitudeToAdd = forceToAdd.Length();

            //if the magnitude of the sum of ForceToAdd and the running Total
            //does not exceed the maximum force available to this vehicle, just
            //add together. Otherwise add as much of the ForceToAdd vector is
            //possible without going over the max.
            if (magnitudeToAdd < magnitudeRemaining)
            {
                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
                //runningTot += forceToAdd;
                Vector3.Add(ref runningTot, ref forceToAdd, out runningTot);
            }

            else
            {
                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
                // ***
                // add it to the steering force
                // ***
                Vector3.Normalize(ref forceToAdd, out forceToAdd);
                //runningTot += forceToAdd * magnitudeRemaining;
                var tmpForceToAdd = forceToAdd;
                Vector3.Multiply(ref tmpForceToAdd, magnitudeRemaining, out tmpForceToAdd);
                Vector3.Add(ref runningTot, ref tmpForceToAdd, out runningTot);
            }

            return true;
        }

        #region GetNeighbors Methods


        // Method 'GetNeighbors' variables; 
        static object[] _tmpNeighborsList = new object[1];
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        // 10/14/2008; 6/8/2010: Updated to pass in 'ForceBehavior' instance; also updated to use LocklessDictionary.        
        /// <summary>
        /// Gets the <see cref="SceneItem"/> neighbors for the given <see cref="SceneItem"/>, using its 'ViewRadius' as the
        /// distance to search around the given <see cref="SceneItem"/>, for other <see cref="SceneItem"/>.  Any neighbors found
        /// are added to the collection <paramref name="neighbors"/>.
        /// </summary>
        /// <param name="forceBehavior"></param>
        /// <param name="usePathNodeType"><see cref="PathNodeType"/> Enum</param>
        /// <param name="neighbors">Collection of <see cref="SceneItemWithPick"/> as neighbors</param>
        private static void GetNeighbors(ForceBehaviorsCalculator forceBehavior, PathNodeType usePathNodeType, SceneItem[] neighbors)
        {
            try
            {
                // Cache values to improve performance.
                var item = forceBehavior.SceneItemOwner; // 6/8/2010
                var viewRadius = item.ViewRadius;
                var viewRadiusX2 = (int)viewRadius * 2; // 8/12/2009
                var position = item.Position; // 8/6/2009   
                var isNetworkGame = Player.IsNetworkGame; // 6/8/2010
                
                // Create QueryRectangle using ViewRadius
                var queryRect = new Rectangle
                                          {
                                              X = (int) (position.X - viewRadius),
                                              Y = (int) (position.Z - viewRadius),
                                              Width = viewRadiusX2,
                                              Height = viewRadiusX2
                                          };
               

                // 1/13/2010
                if (TemporalWars3DEngine.AStarGraph == null) return;

                // 8/6/2009 - Get Occupiants from AGraph, using Rectangle as search area.
                int occupiantsFound;

                var avoidSceneItem = isNetworkGame ? ((SceneItemWithPick)item).NetworkItemNumber : ((SceneItemWithPick)item).SceneItemNumber; // 6/8/2010
                if (!TemporalWars3DEngine.AStarGraph.GetOccupantsForRectangle(ref queryRect, avoidSceneItem, usePathNodeType, 
                                                                                    ref _tmpNeighborsList, out occupiantsFound))
                    return;
                
                var indexCount = 0;
                //var neighborsCount = neighbors.Count; // 8/25/2009
                for (var i = 0; i < occupiantsFound; i++)
                {
                    // 1/21/2011 - Add Lock to avoid ArgOutOfRange exception for _tmpNeighborsList.
                    SceneItemWithPick neighbor;
                    lock (_tmpNeighborsList)
                    {
                        // Yes, so add to Neighbors list
                        // - Is Index in List?  Yes, then reuse
                        neighbor = _tmpNeighborsList[i] as SceneItemWithPick; // 8/12/2009
                    }
                   
                    if (neighbor == null) continue; // 9/22/2009

                    // 6/8/2010 - Updated to use simple array.
                    neighbors[indexCount] = neighbor;

                    indexCount++;
                }

                // 6/8/2010 - Save current neighbors count; this is used by the outside callers, which are iterating
                //            the neighbors collection, to iterate ONLY the new neighbors in the LocklessDictionary.
                switch (usePathNodeType)
                {
                    case PathNodeType.GroundItem:
                        forceBehavior.NeighborsGroundKeysCount = indexCount;
                        break;
                    case PathNodeType.AirItem:
                        forceBehavior.NeighborsAirKeysCount = indexCount;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("usePathNodeType");
                }


                // 3/6/2009 - Remove Items which are after new IndexCount!
               /*if (indexCount == 0)
                   neighbors.Clear();
               else
               {
                   
                   // Note: Since IndexCount is increased at the end of the loop, 
                   // then the # indicated is already at the start of the
                   // range needed to be deleted!
                   if (indexCount <= neighborsCount)
                   {
                       //neighbors.RemoveRange(indexCount, (neighborsCount - indexCount));

                       // 6/8/2010 - Updated to use LocklessDictionary
                       for (var i = indexCount; i < (neighborsCount - indexCount); i++)
                       {
                           SceneItemWithPick sceneItem;
                           neighbors.TryRemove(i, out sceneItem);
                       }
                   } // End IndexCount<=NeighborsCount
                   
               } // End indexCount==0*/

            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Method Error: GetNeighbors_Thread1, threw Null error.");
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("Method Error: GetNeighbors_Thread1, threw ArgOutOfRange error.");
            }

        }

        // 6/8/2010: Updated to pass in 'ForceBehavior' instance; also updated to use LocklessDictionary.  
        // 2/27/2009 - This version uses the collision test to determine if within viewRadius!
        /// <summary>
        /// Gets the <see cref="SceneItem"/> neighbors for the given <see cref="SceneItem"/>, using its 'ViewRadius' as the
        /// distance to search around the given <see cref="SceneItem"/>, for other <see cref="SceneItem"/>.  Any neighbors found
        /// are added the to the collection <paramref name="neighbors"/>.
        /// </summary>
        private static void GetNeighborsV2(ForceBehaviorsCalculator forceBehavior, SceneItem[] neighbors, bool filterForAir)
        {
            try // 11/26/2009
            {
                var item = forceBehavior.SceneItemOwner; // 6/8/2010

                // iterate through all selectable items, checking to see who is inside 
                // ViewRadius.
                var indexCount = 0;
                const int playersLength = TemporalWars3DEngine._maxAllowablePlayers;
                for (var playerIndex = 0; playerIndex < playersLength; playerIndex++)
                {
                    // 6/15/2010 - Updated to use new GetPlayer method.
                    Player player;
                    if (!TemporalWars3DEngine.GetPlayer(playerIndex, out player))
                        break;

                    if (player == null) continue; // 8/12/2009

                    // 6/15/2010 - Updated to retrieve the ROC collection.
                    // iterate selectable items of given player index 
                    ReadOnlyCollection<SceneItemWithPick> selectableItems;
                    Player.GetSelectableItems(player, out selectableItems);

                    var count = selectableItems.Count; // 8/25/2009
                    SceneItemWithPick itemToCheck; // 6/15/2010
                    for (var i = 0; i < count; i++)
                    {
                        // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                        // Get a selectableItem to check
                        //var itemToCheck = selectableItems[i];
                        if (!Player.GetSelectableItemByIndex(player, i, out itemToCheck))
                            break;
                        
                        if (itemToCheck == null) continue; // 3/1/2009

                        // Make sure is Alive.
                        if (!itemToCheck.IsAlive) continue;

                        // Make sure not BuildingScene model
                        if (itemToCheck is BuildingScene) continue;

                        // 8/4/2009 - Filter for Air units?
                        if (filterForAir && itemToCheck is SciFiTankScene) continue;

                        if (!filterForAir && itemToCheck is SciFiAircraftScene) continue;

                        // Make sure its not ourselves
                        if ((itemToCheck.SceneItemNumber == ((SceneItemWithPick)item).SceneItemNumber)) continue;

                        // is SceneItemOwner with view?
                        if (!item.WithinView(itemToCheck)) continue;

                        // 1/14/2011 - Capture IndexOutOfRange error
                        try
                        {
                            // yes, so add to neighbors array.                        
                            // - Is Index in List?  Yes, then reuse
                            // 6/8/2010 - Updated to use simple array.
                            neighbors[indexCount] = itemToCheck;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Debug.WriteLine("(GetNeighborsV2_ThreadMethod1) threw the 'IndexOutOfRangeException' error.");
                        }
                        

                        indexCount++;
                    } // End Loop selectables
                } // End Loop players

                // 6/8/2010 - Save current neighbors count; this is used by the outside callers, which are iterating
                //            the neighbors collection, to iterate ONLY the new neighbors in the LocklessDictionary.
                if (filterForAir)
                {
                    forceBehavior.NeighborsAirKeysCount = indexCount;
                }
                else
                {
                    forceBehavior.NeighborsGroundKeysCount = indexCount;
                }

                // Resize Array if necessary
                /*if (indexCount >= neighbors.Count) return;

                if (indexCount == 0)
                    neighbors.Clear();
                else
                {
                    // 3/6/2009 - Remove Items which are after new IndexCount!
                    //            Note: Since IndexCount is increased at the end of the loop, 
                    //                  then the # indicated is already at the start of the
                    //                  range needed to be deleted!
                    if (indexCount <= neighbors.Count)
                    {
                        //neighbors.RemoveRange(indexCount, (neighbors.Count - indexCount));

                        // 6/8/2010 - Updated to use LocklessDictionary
                        for (var i = (neighbors.Count - indexCount); i < indexCount; i++)
                        {
                            SceneItemWithPick sceneItem;
                            neighbors.TryRemove(i, out sceneItem);
                        }
                    }

                    // resize array
                    //neighbors.TrimExcess();

                }*/
            }
            catch (ArgumentException)
            {
                Debug.WriteLine("(GetNeighborsV2_ThreadMethod1) threw the 'ArgumentException' error.");
            }
        }

        #endregion


        // 10/16/2008 - 
        /// <summary>
        /// <see cref="EventHandler"/> for the 'SceneItemCreated' Event from <see cref="BuildingScene"/> class,
        ///  which adds the given <see cref="BuildingScene"/> as an obstacle.
        /// </summary>
        static void BuildingCreated(object sender, EventArgs e)
        {
            var building = sender as BuildingScene;
            if (building == null) return;
           
            // Add SceneItemOwner to Obstacle Array
            if (Obstacles != null) Obstacles.Add(building);
        }       

        // 10/16/2008
        /// <summary>
        /// Populates the collection <see cref="Obstacles"/>, with <see cref="SceneItem"/> from the 
        /// 'SelectableItems' collection, which are of type <see cref="BuildingScene"/> classes marked as 'Obstacles'.
        /// </summary>
        private static void PopulateObstacles()
        {
            // 12/18/2009 - Only populate once!
            if (Obstacles != null) return;
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            // 6/15/2010 - Updated to retrieve the ROC collection.
            ReadOnlyCollection<SceneItemWithPick> selectableItems;
            Player.GetSelectableItems(player, out selectableItems);
            var count = selectableItems.Count;

            // 9/23/2009 - Init
            Obstacles = new List<SceneItem>();

            // 1st - Iterate through all 'SelectableItems'
            for (var i = 0; i < count; i++)
            {
                // 6/15/2010 - To avoid ArgOutOfRange error, use the Player new 'Get' method.
                //selectableItem = selectableItems[i];
                SceneItemWithPick selectableItem; // 6/15/2010
                if (!Player.GetSelectableItemByIndex(player, i, out selectableItem))
                    break;

                if (selectableItem == null) continue;

                // Add if 'BuildingScene' type, since all buildings should be considered obstacles
                if (selectableItem is BuildingScene)
                {
                    Obstacles.Add(selectableItem);
                }
            }
        }

        /// <summary>
        /// Converts a given point from world space into local space
        /// </summary>
        /// <param name="point"><see cref="Vector3"/> point to convert</param>
        /// <param name="sceneItemPosition"><see cref="Vector3"/> position of <see cref="SceneItem"/></param>
        /// <param name="transform"><see cref="Matrix"/> transform</param>
        /// <param name="transPoint">(OUT) new <see cref="Vector3"/> point in local space</param>
        internal static void PointToLocalSpace(ref Vector3 point, ref Vector3 sceneItemPosition, ref Matrix transform, out Vector3 transPoint)
        {
            transform.Translation = sceneItemPosition;
            Matrix invTransMat; 
            Matrix.Invert(ref transform, out invTransMat);            
            Vector3.Transform(ref point, ref invTransMat, out transPoint);            
        }

        // 10/17/2008 
        /// <summary>
        /// Converts a local point to world space.
        /// </summary>
        /// <param name="point"><see cref="Vector3"/> point to convert</param>
        /// <param name="matTransform"><see cref="Matrix"/> transform</param>
        /// <param name="transPoint">(OUT) new <see cref="Vector3"/> as point in world space</param>
        internal static void VectorToWorldSpace(ref Vector3 point, ref Matrix matTransform, out Vector3 transPoint)
        {
            // Transform Point using Matrix            
            Vector3.Transform(ref point, ref matTransform, out transPoint);
            
        }
    }
}
