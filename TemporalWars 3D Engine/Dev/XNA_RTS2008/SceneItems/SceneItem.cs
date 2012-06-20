#region File Description
//-----------------------------------------------------------------------------
// SceneItem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using TWEngine.Explosions;
using TWEngine.ForceBehaviors;
using TWEngine.GameCamera;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels;
using TWEngine.MemoryPool;
using TWEngine.Players;
using TWEngine.SceneItems.Enums;
using TWEngine.SceneItems.Structs;
using TWEngine.Shapes;
using TWEngine.Utilities;

namespace TWEngine.SceneItems
{
    /// <summary>
    /// The <see cref="SceneItem"/> is the base class, which provides the primary funtions
    /// for any <see cref="SceneItem"/>.  This includes updating the transforms for position data, 
    /// updating attributes, like current health, etc.
    /// This class inherts from a collection of <see cref="SceneItem"/>, allowing a single item to have
    /// multiple children of <see cref="SceneItem"/>.
    /// </summary>
    public class SceneItem : List<SceneItem>, IStatusBarItem
    {
        // 5/22/2012 - Unique GUID for this class instance - (Scripting Purposes)
        private readonly Guid _uniqueKey = Guid.NewGuid();

        protected const float SeekDistSq = 30*30;
        // 2/24/2009 -
        ///<summary>
        /// Occurs when a <see cref="SceneItem"/> is created.
        ///</summary>
        public static event EventHandler SceneItemCreated; 
        // 2/24/2009 - 
        /// <summary>
        /// Occurs when a <see cref="SceneItem"/> is destroyed.
        /// </summary>
        private event EventHandler SceneItemDestroyed;  
        
        // 7/16/2009 - 
        /// <summary>
        /// Was <see cref="SceneItem"/> already assigned to <see cref="SceneItemDestroyed"/> event.
        /// </summary>
        private bool _wasAssignedToDestroyedEventHandler;
 
        // 6/15/2009 - General ThreadLock, used for Properties access by other threads.
        private readonly object _threadLock = new object();

        // 1/22/2010 -
        /// <summary>
        /// The <see cref="HealthState"/> Enum
        /// </summary>
        protected enum HealthState
        {
            /// <summary>
            /// The <see cref="SceneItem"/> is alive.
            /// </summary>
            Alive,
            /// <summary>
            /// The <see cref="SceneItem"/> is in process of being killed.
            /// </summary>
            Dying,
            /// <summary>
            /// The <see cref="SceneItem"/> id dead.
            /// </summary>
            Dead
        }
        // 1/22/2010
        /// <summary>
        /// <see cref="SceneItem"/> current <see cref="HealthState"/> status.
        /// </summary>
        protected HealthState CurrentHealthState;

        // 1/3/2010 - Add IStatusBar/IStatusBarItem Interfaces
        /// <summary>
        /// Reference to the <see cref="IStatusBar"/> instance for this <see cref="SceneItem"/>
        /// </summary>
        protected IStatusBar StatusBar;
        /// <summary>
        /// Reference to the <see cref="IStatusBarItem"/> instance for this <see cref="SceneItem"/>
        /// </summary>
        protected IStatusBarItem StatusBarItem;

        /// <summary>
        /// A <see cref="TimeSpan"/> structure set to a default value of 10 seconds.
        /// </summary>
        protected internal TimeSpan DeleteFromWorldTime = new TimeSpan(0, 0, 10);

        // 3/28/2009 - 
        /// <summary>
        /// Has World matrix been initiliazed yet?
        /// </summary>
        private bool _isWorldMatrixInit;

        // 7/1/2008 - // TODO: Probably should remove, since could now use Enum HealthState!
        private volatile bool _isAlive; // 11/15/09 - Updated with 'volatile'. 
       
        /// <summary>
        /// Reference to <see cref="SceneItem"/> to attack.
        /// </summary>
        private volatile SceneItem _attackSceneItem; // 11/15/09 - Updated with 'volatile'.      
      
        // 4/15/2009
        private float _currentHealthPercent = 1.0f;
       
        // 4/30/2009 - Tracks if below 50/25 percents
        /// <summary>
        /// If current health value below 50%?
        /// </summary>
        protected bool IsBelow50Percent;
        /// <summary>
        /// Is current health value below 25%?
        /// </summary>
        protected bool IsBelow25Percent;

        // 10/12/2009 - 
        /// <summary>
        /// ShowFlashWhite (Scripting Purposes)
        /// </summary>
        private bool _showFlashWhiteOn;
        private int _flashForGivenMilliSeconds;
       
        private float _startingHealth;
        private float _currentHealth;

        /// <summary>
        /// The root <see cref="SceneItem"/>
        /// </summary>
        private SceneItem _root;

// ReSharper disable InconsistentNaming
        /// <summary>
        /// The position for this <see cref="SceneItem"/>.
        /// </summary>
        protected Vector3 position; // (MP)
        /// <summary>
        /// The velocity for this <see cref="SceneItem"/>.
        /// </summary>
        protected Vector3 velocity; // (MP)

// ReSharper disable UnaccessedField.Local
        private Vector3 force;
// ReSharper restore UnaccessedField.Local
        /// <summary>
        /// The current rotation for this <see cref="SceneItem"/>.
        /// </summary>
        protected Quaternion rotation;

        private Vector3 _center;
        /// <summary>
        /// The scaling transformation for this <see cref="SceneItem"/>.
        /// </summary>
        protected Vector3 scale = new Vector3(1f, 1f, 1f);
// ReSharper restore InconsistentNaming

        // 10/6/2009
        private string _name;
        
        /// <summary>
        /// Reference to current <see cref="Game"/> instance
        /// </summary>
        protected static Game GameInstance; // 4/27/2010 - Updated to be static.

        // 3/28/2009
        private Matrix _rotMatrix = Matrix.Identity;
        private Matrix _oldOrientation = Matrix.Identity;
        private Quaternion _oldRotation;
        private Vector3 _oldPosition, _oldScale;
        

        // 11/15/2009 - KillSceneItem is called check.
        /// <summary>
        /// The <see cref="KillSceneItemCalled"/> check is used to make sure code is not executed twice, since during MP games
        /// the Server will make sure client kills the unit by calling this, too.
        /// </summary>
        protected bool KillSceneItemCalled;

        // 5/31/2012
        protected readonly Color BoundingSphereDefaultColor = Color.Magenta;

        // 6/10/2012
        /// <summary>
        /// <see cref="AudioEmitter"/> instance
        /// </summary>
        protected AudioEmitter AudioEmitterI;

        // 6/10/2012
        /// <summary>
        /// <see cref="AudioListener"/> instance
        /// </summary>
        protected AudioListener AudioListenerI;

        #region Properties 

        // 5/31/2012
        /// <summary>
        /// Gets or sets if the <see cref="SceneItem"/> was spawned with some scripting action.  
        /// </summary>
        /// <remarks>This flag is used to remove item spawned dynamically when saving map data.</remarks>
        public virtual bool SpawnByScriptingAction { get; set; }

        // 5/22/2012; 6/6/2012 - Made virtual.
        /// <summary>
        /// Gets the current <see cref="SceneItem"/> unique key GUID.
        /// </summary>
        public virtual Guid UniqueKey
        {
            get { return _uniqueKey; }
        }

        // 12/31/2009 - Updated to AutoProperty
        ///<summary>
        /// Set to force an update to World <see cref="Matrix"/>.
        ///</summary>
        public bool UpdateWorldMatrix { get; set; }

       
        // 10/6/2009
        /// <summary>
        /// User defined name, used to itentify this instance 
        /// within the Scripting conditions.
        /// </summary>
        public virtual string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                // 1/14/2011 - Set into Player's named dictionary
                if (!string.IsNullOrEmpty(value))
                    // Store new 'Name' into Player Dictionary
                    Player.AddSceneItemToNamesDictionary(value, this);
            }
        }

        // 10/5/2009
        /// <summary>
        /// The <see cref="Player"/> number this <see cref="SceneItem"/> belongs to.
        /// </summary>
        public byte PlayerNumber { get; set; }

        /// <summary>
        /// Is this <see cref="SceneItem"/> alive?
        /// </summary>
        public bool IsAlive
        {
            get { return _isAlive; }
            protected set { _isAlive = value; }
        }

        // 10/3/2009 - 
        ///<summary>
        /// Tracks if kill item order started in <see cref="ReduceHealth"/> method.
        ///</summary>
        public bool KillSceneItemStarted { get; protected set; }

        /// <summary>
        /// Should this <see cref="SceneItem"/> be deleted?
        /// </summary>
        public virtual bool Delete { get; set; }

        /// <summary>
        /// Collision radius for this <see cref="SceneItem"/>
        /// </summary>
        public float CollisionRadius { get; set; }

        // 10/14/2008
        /// <summary>
        /// View radius for this <see cref="SceneItem"/>
        /// </summary>
        public float ViewRadius { get; protected set; }

        /// <summary>
        /// Attacking radius for this <see cref="SceneItem"/>
        /// </summary>
        public float AttackRadius { get; protected set; }

        /// <summary>
        /// Starting health value for this <see cref="SceneItem"/>
        /// </summary>
        protected float StartingHealth
        {
            set
            {
                // Internal SceneItem Values
                _startingHealth = value;
                CurrentHealth = value;
                             
                // 4/27/2010 - Cache
                var statusBarItem = StatusBarItem;
                if (statusBarItem == null) return;

                // Set IStatusBar Properties too  
                statusBarItem.StatusBarStartValue = value;
                statusBarItem.StatusBarCurrentValue = value;
            }
        }

       

        /// <summary>
        /// Current health value for this <see cref="SceneItem"/>, after taking any damage.
        /// </summary>
        public float CurrentHealth
        {
            get { return _currentHealth; }
            set
            {
                _currentHealth = value;

                // 4/27/2010 - Cache
                var statusBarItem = StatusBarItem;
                if (statusBarItem == null) return;

                // 10/12/2009 - Update the StatusBar's value too
                statusBarItem.StatusBarCurrentValue = _currentHealth;
            }
        }

        /// <summary>
        /// Current health value as percent for this <see cref="SceneItem"/>.
        /// </summary>
        public float CurrentHealthPercent
        {
            get { return _currentHealthPercent; }
        }

        /// <summary>
        /// Reference to <see cref="SceneItem"/> to attack
        /// </summary>
        public SceneItem AttackSceneItem
        {
            get 
            {
                // 6/7/2010 - Updated and removed the Lock call.
                return _attackSceneItem;
            }
            set 
            {
                // 6/7/2010 - Updated and removed the Lock call.
                Interlocked.Exchange(ref _attackSceneItem, value);
            }
        }

        /// <summary>
        /// The <see cref="Shape"/> is the actual renderable object
        /// </summary>
        public Shape ShapeItem { get; private set; }

        /// <summary>
        /// Simulation paused for this <see cref="SceneItem"/>, nothing will update
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// The max speed for the <see cref="SceneItem"/>
        /// </summary>
        public float MaxSpeed { get; set; }

        /// <summary>
        ///  The maximum force this <see cref="SceneItem"/> can produce to power itself 
        /// (think rockets and thrust)
        /// </summary>
        public float MaxForce { get; set; }

        /// <summary>
        /// The force applies to the <see cref="SceneItem"/>, via <see cref="AbstractBehavior"/> classes.
        /// </summary>
        public Vector3 Force
        {
            set { force = value; }
        }

        /// <summary>
        /// The velocity for this <see cref="SceneItem"/>
        /// </summary>
        public Vector3 Velocity
        {
            get{ return velocity; }
            set{ velocity = value; }
        }

        /// <summary>
        /// The position for this <see cref="SceneItem"/>
        /// </summary>
        public virtual Vector3 Position
        {
            get{ return position; }
            set{ position = value; }
        }

        /// <summary>
        /// The current rotation for this <see cref="SceneItem"/>
        /// </summary>
        public virtual Quaternion Rotation
        {
            get{ return rotation; }
            set{ rotation = value; }
        }

        /// <summary>
        /// The center of rotation and scaling for this <see cref="SceneItem"/>
        /// </summary>
// ReSharper disable ConvertToAutoProperty
        protected Vector3 Center
// ReSharper restore ConvertToAutoProperty
        {
            get { return _center; }
            set { _center = value; }
        }

        // 4/3/2011 - Updated to virtual.
        /// <summary>
        /// The scaling transformation for this <see cref="SceneItem"/>
        /// </summary>
        public virtual Vector3 Scale
        {
            get{ return scale; }
            set{ scale = value; }
        }
       

        #region IStatusBar Interface Properties

        /// <summary>
        /// Length of StatusBar Container
        /// </summary>
        public int StatusBarLength
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarLength : 0;
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarLength = value; }
        }

        /// <summary>
        /// Should draw Status Bar?
        /// </summary>
        public bool DrawStatusBar
        {
            get
            {
                return StatusBarItem != null && StatusBarItem.DrawStatusBar;
            }
            set { if (StatusBarItem != null) StatusBarItem.DrawStatusBar = value; }
        }

        /// <summary>
        /// World 3D Position to draw StatusBar; an Offset 
        /// can also be applied to this value using the 'StatusBarOffsetPosition2D'
        /// property.
        /// </summary>
        public Vector3 StatusBarWorldPosition
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarWorldPosition : new Vector3();
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarWorldPosition = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Display EnergyOff icon when power value is less than zero.
        /// </summary>
        public bool ShowEnergyOffSymbol
        {
            get
            {
                return StatusBarItem != null && StatusBarItem.ShowEnergyOffSymbol;
            }
            set { if (StatusBarItem != null) StatusBarItem.ShowEnergyOffSymbol = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Identifies if the StatusBar is currently in use.
        /// </summary>
        public bool InUse
        {
            get
            {
                return StatusBarItem != null && StatusBarItem.InUse;
            }
            set { if (StatusBarItem != null) StatusBarItem.InUse = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Index of StatusBar in internal array.
        /// </summary>
        public int IndexInArray
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.IndexInArray : 0;
            }
            set { if (StatusBarItem != null) StatusBarItem.IndexInArray = value; }
        }

        // 1/3/2010
        /// <summary>
        /// <see cref="SceneItem"/> which owns this StatusBar instance.
        /// </summary>
        public IStatusBarSceneItem SceneItemOwner
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.SceneItemOwner : null;
            }
            set { if (StatusBarItem != null) StatusBarItem.SceneItemOwner = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Rectangle which defines this StatusBar's shape.
        /// </summary>
        public Rectangle StatusBarShape
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarShape : new Rectangle();
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarShape = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Rectangle which defines this StatusBar's Container shape.
        /// </summary>
        public Rectangle StatusBarContainerShape
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarContainerShape : new Rectangle();
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarContainerShape = value; }
        }

        /// <summary>
        /// Offset Position to draw statusBar, from root position.
        /// This is because usually the root position given for a <see cref="SceneItem"/>, 
        /// is not the best place to draw the statusBar!
        /// </summary>
        public Vector2 StatusBarOffsetPosition2D
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarOffsetPosition2D : new Vector2();
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarOffsetPosition2D = value; }
        }

        /// <summary>
        /// Starting value which defines a full Status Bar.
        /// </summary>
        public float StatusBarStartValue
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarStartValue : 0f;
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarStartValue = value; }
        }

        /// <summary>
        /// Current value to show in Status Bar.
        /// </summary>
        public float StatusBarCurrentValue
        {
            get
            {
                return StatusBarItem != null ? StatusBarItem.StatusBarCurrentValue : 0f;
            }
            set { if (StatusBarItem != null) StatusBarItem.StatusBarCurrentValue = value; }
        }

        // 5/20/2012
        /// <summary>
        /// Gets or sets the position to move to. (Scripting Purposes)
        /// </summary>
        /// <remarks>
        /// Normally, the MoveToPosition in the SceneItemWithPick will be used.  However, for ScenaryItems
        /// which use the ForceBehaviors model, via the ScriptingActions class, this property will then
        /// become the focus point, and can be set to a Waypoint position.
        /// </remarks>
        public Vector3 MoveToWayPosition { get; set; }

        #endregion

        #endregion

        #region Constructors

        // 1/22/2010: Updated to call the 3-param constructor.
        /// <summary>
        /// Default constructor, does nothing special
        /// </summary>
        /// <param name="game">Instance of game.</param>
        public SceneItem(Game game) 
            : this (game, null, Vector3.Zero)
        {
           // Emtpy
        }

        // 1/22/2010: Updated to call the 3-param constructor.
        /// <summary>
        /// Default constructor, does nothing special
        /// </summary>
        /// <param name="game">Instance of game.</param>
        /// <param name="shape">The shape to be rendered for this SceneItemOwner</param>       
        protected SceneItem(Game game, Shape shape)
            : this(game, shape, Vector3.Zero)
        {
            // Empty
        }

        /// <summary>
        /// Creates a <see cref="SceneItem"/> with a <see cref="Shape"/> to be rendered at an initial position
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="shape">
        ///   The <see cref="Shape"/> to be rendered for this <see cref="SceneItem"/>
        /// </param>
        /// <param name="initialPosition">
        ///   The initial Position of the <see cref="SceneItem"/>
        /// </param>
        protected SceneItem(Game game, Shape shape, Vector3 initialPosition)
        {
            IsAlive = true;
            GameInstance = game;

            ShapeItem = shape;
            position = initialPosition;

            // 10/7/2008 - Set Default MaxForce
            MaxForce = 300.0f;

            // 1/3/2010 - Set StatusBar Interface Ref
            StatusBar = (IStatusBar)game.Services.GetService(typeof(IStatusBar));

            // 1/22/2010 - HealthState
            CurrentHealthState = HealthState.Alive;
            
        }
        

        #endregion

        // 8/15/2008 - Dispose of resources.
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="finalDispose">Is this final dispose?</param>
        public virtual void Dispose(bool finalDispose)
        {
            // 
            // 1/5/2010 - Note: Up to this point, no InternalDriverError will be thrown in the SpriteBatch.
            //          - Note: Discovered, the error is coming from the call to 'ShapeItem' dispose!

            // Dispose of Resources           
            var shapeItem = ShapeItem; // 4/27/2010
            if (shapeItem != null)
                shapeItem.Dispose();

            // Remove StatusBarItem Instance
            var statusBar = StatusBar; // 4/27/2010
            if (statusBar != null) 
                statusBar.RemoveStatusBarItem(ref StatusBarItem);

            // Null Refs            
            ShapeItem = null;
            _attackSceneItem = null;
            SceneItemDestroyed = null;  // 2/24/2009 

            // 12/10/2008 - Final Dispose also kills Static items.
            if (!finalDispose) return;

            // Null Refs                                
            _root = null;
            GameInstance = null;
           
        }

        /// <summary>
        /// Adds a <see cref="SceneItem"/> to the scene node
        /// </summary>
        /// <param name="childItem">The <see cref="SceneItem"/> to add</param>
        public new void Add(SceneItem childItem)
        {
            //A new custom 'add' that sets the parent and the root properties
            //on the child SceneItemOwner
            childItem._root = _root ?? this;

            //Call the 'real' add method on the dictionary
            ((List<SceneItem>)this).Add(childItem);
        }


        /// <summary>
        /// Updates any values associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time"><see cref="TimeSpan"/> structure for time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed game sime since last call</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public virtual void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {
            // 11/13/2008 - Call virtual methods, depending on currentHealth
            // 4/30/2009: Update to check isBelow booleans, to improve CPI in Vtune!
            if (IsBelow50Percent)
            {
                Below50HealthParticleEffects(ref elapsedTime);

                if (IsBelow25Percent)
                    Below25HealthParticleEffects(ref elapsedTime);
            }

            // 10/12/2009 - Check if item is Flashing White! (Scripting Purposes)
            DoFlashingWhiteCheck(ref elapsedTime);
            

            //If this SceneItemOwner has something to draw then update it
            var shapeItem = ShapeItem; // 4/27/2010 - Cache
            if (shapeItem != null)
            {
                // 3/28/2009 - Optimization: Only create World Matrix once!
                if (!_isWorldMatrixInit)
                {
                    // 4/27/2010 - Refactored code into new STATIC method.
                    UpdateTransforms(this, shapeItem);
                }
                else // Update the part of World Matrix, as needed!
                {
                    // 4/27/2010 - Refactored code into new STATIC method.
                    UpdateTransformsAsNeeded(this, shapeItem);
                }

                // 11/24/2008 - Update StatusBarItem's WorldPosition
                var statusBarItem = StatusBarItem; // 4/27/2010 - Cache
                if (statusBarItem != null) 
                    statusBarItem.StatusBarWorldPosition = position;


                if (!Paused)
                {
                    shapeItem.Update(ref time, ref elapsedTime);
                }
            }
            
            // 9/7/2008: Updated to ForLoop, rather than ForEach.
            // Update each child SceneItem
            for (var i = 0; i < Count; i++)
            {
                this[i].Update(gameTime, ref time, ref elapsedTime, isClientCall);
            }           
            
            // 1/29/2009: TODO: Is this needed?  This is for any child List items.
            //Remove any items that need deletion  
            //RemoveAll(IsDeleted);

            // 6/10/2012
            UpdateAudioEmitters();
        }

        // 4/27/2010
        /// <summary>
        /// Helper method, which updates the given <see cref="SceneItem"/> matrix 
        /// transforms, for position, rotation, scale, etc.
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance to update</param>
        /// <param name="shapeItem"><see cref="Shape"/> instance</param>
        private static void UpdateTransforms(SceneItem sceneItem, Shape shapeItem)
        {
            #region OldCode
            // 2/28/2008 - Ben
            // Updated method to use Quaternion Rotation method.
            /*shape.World = Matrix.CreateTranslation(-center) *
                          Matrix.CreateScale(scale) *
                          Matrix.CreateRotationX(rotation.X) *
                          Matrix.CreateRotationY(rotation.Y) *
                          Matrix.CreateRotationZ(rotation.Z) *
                          Matrix.CreateTranslation(Position + center);*/
            #endregion

            // 11/3/2008 - Updated to Optimize memory!
            var inCenter = (-sceneItem.Center);
            Matrix inMatrix1;
            Matrix.CreateTranslation(ref inCenter, out inMatrix1);
            Matrix inMatrix2;
            Matrix.CreateScale(ref sceneItem.scale, out inMatrix2);
            Matrix inMatrix3;
            Matrix.CreateFromQuaternion(ref sceneItem.rotation, out inMatrix3);                   
            //Vector3.Add(ref Position, ref center, out inTranslate); //inTranslate = Position + center;
            Matrix inMatrix4;
            Matrix.CreateTranslation(ref sceneItem.position, out inMatrix4);

            // 11/19/2008 - Optimize by removing Matrix Overload operations, which are slow on XBOX!
            //shape.World = inMatrix1 * inMatrix2 * inMatrix3 * inMatrix4;
            Matrix.Multiply(ref inMatrix1, ref inMatrix2, out inMatrix1);
            Matrix.Multiply(ref inMatrix1, ref inMatrix3, out inMatrix1);
            Matrix.Multiply(ref inMatrix1, ref inMatrix4, out inMatrix1);
            shapeItem.WorldP = inMatrix1;

            // 3/28/2009
            sceneItem._isWorldMatrixInit = true;
            sceneItem._oldRotation = sceneItem.rotation;
            sceneItem._oldPosition = sceneItem.position;
            sceneItem._oldScale = sceneItem.scale;
            sceneItem._oldOrientation = shapeItem.Orientation; // 4/10/2009
        }

        // 4/27/2010
        /// <summary>
        /// Helper method, which updates the given <see cref="SceneItem"/> matrix 
        /// transforms, for position, rotation, scale, etc.  However, for each section, 
        /// like rotation, the previous rotation value is check with the current, and if the
        /// same, will be skipped to eliminate redudant computations!
        /// </summary>
        /// <param name="sceneItem"><see cref="SceneItem"/> instance to update</param>
        /// <param name="shapeItem"><see cref="Shape"/> instance</param>
        private static void UpdateTransformsAsNeeded(SceneItem sceneItem, Shape shapeItem)
        {
            var tmpWorld = shapeItem.WorldP;

            // Apply Scaling / Rotation
            var result = sceneItem._oldScale.Equals(sceneItem.scale); // 4/13/2009
            if (!result)
            {
                Matrix inMatrix2;
                Matrix.CreateScale(ref sceneItem.scale, out inMatrix2);
                Matrix.CreateFromQuaternion(ref sceneItem.rotation, out sceneItem._rotMatrix);
                Matrix.Multiply(ref inMatrix2, ref sceneItem._rotMatrix, out tmpWorld);
                tmpWorld.Translation = shapeItem.WorldP.Translation;

                sceneItem._oldScale = sceneItem.scale;
                sceneItem._oldRotation = sceneItem.rotation;
                sceneItem.UpdateWorldMatrix = true;
            }  // Apply Rotation Only                   
            else if (!sceneItem._oldRotation.Equals(sceneItem.rotation))
            {
                Matrix.CreateFromQuaternion(ref sceneItem.rotation, out sceneItem._rotMatrix);
                Matrix.Multiply(ref tmpWorld, ref sceneItem._rotMatrix, out tmpWorld);

                sceneItem._oldRotation = sceneItem.rotation;
                sceneItem.UpdateWorldMatrix = true;
            }              

            // Apply Translation 
            var resultPosition = sceneItem._oldPosition.Equals(sceneItem.position); // 4/13/2009
                    
            if (!resultPosition)
            {
                //Vector3.Add(ref Position, ref center, out inTranslate); //inTranslate = Position + center;
                tmpWorld.Translation = sceneItem.position; // was Position                        

                sceneItem._oldPosition = sceneItem.position;
                sceneItem.UpdateWorldMatrix = true;
            }

            // 4/10/2009
            // Orientation changed?
            var resultOrientation = sceneItem._oldOrientation.Equals(shapeItem.Orientation); // 4/13/2009
            if (!resultOrientation)
            {
                sceneItem.UpdateWorldMatrix = true;
                sceneItem._oldOrientation = shapeItem.Orientation;
            }

            // Apply updated 'World' matrix.
            //if (UpdateWorldMatrix)
            {
                shapeItem.WorldP = tmpWorld;
                sceneItem.UpdateWorldMatrix = false;
            }
        }


        /// <summary>
        /// Render any items associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        public virtual void Render()
        {
            #region OldCode
            //If this SceneItemOwner has something to draw then draw it
            /*if (shape != null)
            {
                shape.Render();
            }

            // 9/7/2008: Updated to ForLoop, rather than ForEach.
            //Then render all of the child nodes
            for (loop1 = 0; loop1 < this.Count; loop1++)
            {
                this[loop1].Render();
            }*/
            #endregion
        }

        // 5/27/2012
        /// <summary>
        /// Helper method which renders all the Collision spheres for debug purposes.
        /// </summary>
        /// <param name="gameTime"></param>
        [Conditional("WINDOWS")]
        public virtual void RenderDebug(GameTime gameTime)
        {
            Vector3 itemPosition = Position;

            // 5/27/2012 - Draw Debug Collision Spheres
            var sphere = new BoundingSphere(itemPosition, 75.0f); // Test - new Vector3(3461, 50, 967)
            DebugShapeRenderer.AddBoundingSphere(sphere, BoundingSphereDefaultColor);
            DebugShapeRenderer.Draw(gameTime);
        }

        /// <summary>
        /// Checks if there is a collision between the this and the passed in <see cref="SceneItem"/>
        /// </summary>
        /// <param name="item">A scene <see cref="SceneItem"/> to check</param>
        /// <returns>True if there is a collision</returns>
        public virtual bool Collide(SceneItem item)
        {
            // 3/12/2009
            if (item == null)
                return false;

            //Until we get collision meshes sorted just do a simple sphere (well circle!) check
            return (position - item.position).Length() < CollisionRadius + item.CollisionRadius;
        }

        // 5/18/2009
        /// <summary>
        /// Checks if the given <see cref="Vector3"/> position, is within the <see cref="CollisionRadius"/> of this <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="checkPosition"><see cref="Vector3"/> position to check</param>
        /// <returns>true/false if within collision radius</returns>
        public virtual bool WithinCollision(ref Vector3 checkPosition)
        {
            //Until we get collision meshes sorted just do a simple sphere (well circle!) check
            return (position - checkPosition).Length() < CollisionRadius;
        }

        // 2/27/2009; 10/8/2009: Updated to use the Generic 'TType'.
        /// <summary>
        /// Checks if current <see cref="SceneItem"/> is within view radius of given <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="item"><see cref="SceneItem"/> to check</param>
        /// <returns>True if <see cref="SceneItem"/> is within view radius</returns>
        /// <typeparam name="TType">Generic type with base type of <see cref="SceneItem"/>.</typeparam>
        public virtual bool WithinView<TType>(TType item) where TType : SceneItem
        {
            // 3/12/2009
            if (item == null)
                return false;

            // 10/8/2009 - Get property 'Position', since it can differ for 'ScenaryItems'.
            var itemPosition = item.Position;

            // 4/14/2009 - Optimized by using the Ref Overload of Vector3, which is faster on XBOX!
            Vector3 difference;
            Vector3.Subtract(ref position, ref itemPosition, out difference);
            var length = difference.Length();

            return length < (ViewRadius + item.CollisionRadius);
        }

        // 10/12/2009
        /// <summary>
        /// Sets the given <see cref="SceneItem"/> <see cref="CurrentHealth"/> to the given
        /// percent value. (0 - 100).
        /// </summary>
        /// <param name="healthPercent">New health percent (0-100)</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="healthPercent"/> is not within allowable range of (0-100).</exception>
        public void SetHealthToGivenPercentage(int healthPercent)
        {
            // make sure value given is correct
            if (healthPercent < 0 || healthPercent > 100)
                throw new ArgumentOutOfRangeException("healthPercent", @"Value given for the health percent is outside the allowable range. (0 - 100).");

            // convert given integer into actual float percent value
            var percentValue = (float)healthPercent/100;

            // calculate what the actual health value is, by
            // taking this items 'StartingHealth' value, and mutiply by given percent.
            CurrentHealth = _startingHealth*percentValue;
            
        }

       
        // 7/4/2008
        /// <summary>
        /// Called by attacking <see cref="SceneItem"/>, which will cause this <see cref="SceneItem"/> health
        /// to be reduce by the given <paramref name="damage"/> value.
        /// </summary>
        /// <param name="damage">Damage to apply</param>
        /// <param name="attackerPlayerNumber">Attacker's <see cref="Player"/> number</param>
        /// <returns name="health">Returns the <see cref="CurrentHealth"/></returns>        
        public float ReduceHealth(float damage, int attackerPlayerNumber)
        {
            // Apply Damage Value to this SceneItemOwner
            if (CurrentHealth > 0)
            {
                CurrentHealth -= damage;
                //StatusBarItem.StatusBarCurrentValue -= damage; // 10/12/2009 - Now set in Property

                // 4/15/2009 - Calc the Current Health %
                _currentHealthPercent = CurrentHealth / _startingHealth;

                // 4/30/2009 - Check if below 50 percent
                if (_currentHealthPercent < 0.50f)
                    IsBelow50Percent = true;

                // 4/30/2009 - Check if below 25 percent
                if (_currentHealthPercent < 0.25f)
                    IsBelow25Percent = true;
            }

            // 10/3/2009: Updated to check '_killSceneItemStarted', which keeps it to only
            //            one possible instance call, since multiple units could kill at same time.
            // Kill SceneItemOwner if health <= 0
            if (CurrentHealth <= 0 && !KillSceneItemStarted)
            {
                // 10/3/2009 - Keep this area from being called more than once!
                KillSceneItemStarted = true;

                // 8/21/2009 - Updated to now Queue up the request into
                //             the KillSceneItem Manager thread.

                // 10/3/2009 - Create KillSceneItem struct
                var killSceneItem = new KillSceneItemStruct
                                        {
                                            ItemToKill = this,
                                            AttackerPlayerNumber = attackerPlayerNumber
                                        };

                // 4/27/2010 - Check if null
                if (TemporalWars3DEngine.KillSceneItemManager != null)
                    TemporalWars3DEngine.KillSceneItemManager.AddItemRequest(killSceneItem);

                // 10/3/2009 - Increase attakies Player's Death kill stat.

                // 6/15/2010 - Updated to use new GetPlayer method.
                Player attackerPlayer;
                TemporalWars3DEngine.GetPlayer(attackerPlayerNumber, out attackerPlayer);

                if (attackerPlayer != null) PlayerStats.UpdatePlayersKillStats(attackerPlayer.PlayerStats, this);

                // 6/15/2010 - Updated to use new GetPlayer method.
                Player player;
                TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

                // 10/5/2009 - Increase this Player's Destroyed stat.
                if (player != null) PlayerStats.UpdatePlayersDestroyedStats(player.PlayerStats, this);

                // 1/22/2010 - Set HealthState
                CurrentHealthState = HealthState.Dying;
            }

            return CurrentHealth;

        }

        // 8/2/2009
        /// <summary>
        /// This will increase the <see cref="CurrentHealth"/>, for example to repair buildings or units.
        /// </summary>
        /// <param name="value">Value to increase health by</param>
        /// <returns>Returns the <see cref="CurrentHealth"/></returns>
        protected float IncreaseHealth(float value)
        {
            // Apply Health value to this SceneItemOwner
            if (CurrentHealth > 0)
            {
                CurrentHealth += value;
                //StatusBarItem.StatusBarCurrentValue += value; // 10/12/2009 - Now set in Property

                // Calc the Current Health %
                _currentHealthPercent = CurrentHealth / _startingHealth;

                // Check if above 50 percent
                if (_currentHealthPercent >= 0.50f)
                    IsBelow50Percent = false;

                // Check if above 25 percent
                if (_currentHealthPercent >= 0.25f)
                    IsBelow25Percent = false;
            }

            return CurrentHealth;
        }

        // 8/2/2009
        /// <summary>
        /// Checks if the <see cref="CurrentHealth"/> percentile is above or at the
        /// given percentile value.
        /// </summary>
        /// <param name="value">Percentile value to use from 0 - 1.0f</param>
        /// <returns>true/false of result</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> does not fall into allowable range of 0 - 1.0.</exception>
        public bool IsHealthAtOrAbovePercentile(float value)
        {
            // make sure value is between 0-1.0
            if (value < 0 || value > 1.0f)
                throw new ArgumentOutOfRangeException("value", @"Given percentile value does not fall into allowable range of 0 - 1.0.");

            // Calc the Current Health %
            _currentHealthPercent = CurrentHealth / _startingHealth;

            // Check if at or above given percentile value
            return _currentHealthPercent >= value;
        }

        // 1/22/2010: Renamed to 'StartKillSceneItem', to make intent more clear!
        // 7/4/2008; // 10/3/2009 - Add 2nd param 'attackerPlayerNumber'.
        /// <summary>
        /// Set the <see cref="SceneItem"/> 'isAlive' flag to false; most likely will be overriden in an inherited class with
        /// it's own version of <see cref="StartKillSceneItem"/> method; for example, Animation Death Clips
        /// might be played for the specific <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="attackerPlayerNumber">Attacker's <see cref="Player"/> number</param>
        public virtual void StartKillSceneItem(ref TimeSpan elapsedTime, int attackerPlayerNumber)
        {
            IsAlive = false;
            KillSceneItemCalled = true; // 11/15/09
            _attackSceneItem = null; // 11/16/09
            Position = Vector3.Zero; // 2/28/2011 - Zero position.
            
            // 4/24/2009 - Reset CurrentHealthPercent back to 100%
            _currentHealthPercent = 1.0f;
            // 4/30/2009 - Reset isBelow
            IsBelow50Percent = false;
            IsBelow25Percent = false;

            // 4/8/2009
            // Remove StatusBarItem Instance
            var statusBar = StatusBar; // 4/27/2010 - Cache
            if (statusBar != null) statusBar.RemoveStatusBarItem(ref StatusBarItem);

            // 11/16/2009 - Remove 'Name' from Players Dictionary.
            if (!string.IsNullOrEmpty(Name)) 
                if (!Name.Equals("$E")) Player.SceneItemsByName.Remove(Name);
        }

       

        // 11/13/2008; 1/30/2010 - Updated to now be called 'FinishKillSceneITem'.
        /// <summary>
        /// When an item is fully dead, the <see cref="ExplosionsManager"/> will call this
        /// method. In turn, this is where the item is removed from the game world,
        /// and its <see cref="HealthState"/> is set to be 'Dead'.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="playerNumber">The <see cref="Player"/> number, used in MP games.</param>
        protected internal virtual void FinishKillSceneItem(ref TimeSpan elapsedTime, int playerNumber)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(playerNumber, out player);

            // 10/20/2009 - make sure not NULL
            if (player == null) return;
            
            Delete = true;

            // 1/29/2009 - Tell Player class to perform RemoveAll check.
            player.DoRemoveAllCheck = true;

            // 1/30/2010 - Set Current HealthState to Dead.
            CurrentHealthState = HealthState.Dead;
            
        }       


        // 11/13/2008
        /// <summary>
        /// When <see cref="SceneItem"/> <see cref="CurrentHealth"/> falls below 50%, this method will automatically be
        /// called.  It is empty in this base class; however, inheriting <see cref="SceneItem"/> classes
        /// can use this to display appropriate particle effects for given class.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed time</param>
        protected virtual void Below50HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // Inherting Classes will populate with code.
            return;
        }

        // 11/13/2008
        /// <summary>
        /// When <see cref="SceneItem"/> <see cref="CurrentHealth"/> falls below 25%, this method will automatically be
        /// called.  It is empty in this base class; however, inheriting <see cref="SceneItem"/> classes
        /// can use this to display appropriate particle effects for given class.
        /// </summary>
        ///  <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed time</param>
        protected virtual void Below25HealthParticleEffects(ref TimeSpan elapsedTime)
        {
            // Inherting Classes will populate with code.
            return;
        }

        // 9/23/2008
        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, via the <see cref="IFDTileManager"/>, this virtual method
        /// is called in order to do specific <see cref="SceneItem"/> placement checks; for example, if the <see cref="SceneItem"/>
        /// requires A* Blocking updated.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at.</param>
        /// <returns>True/False of result.</returns>
        public virtual bool RunPlacementCheck(ref Vector3 placementPosition)
        {
            // Should be filled in by inherited classes
            return false;
        }

        // 6/8/2009
        /// <summary>
        /// When a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, via the <see cref="IFDTileManager"/>, this method
        /// is called to check if the x/y values given, are within this sceneItem's <paramref name="pathBlockSize"/> zone.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at</param>
        /// <param name="x">X-value</param>
        /// <param name="y">Y-value</param>
        /// <param name="pathBlockSize">Path block size area to affect?</param>
        /// <returns>true/false of result</returns>
        public virtual bool IsInPlacementZone(ref Vector3 placementPosition, int x, int y, int pathBlockSize)
        {
            // 1/13/2010
            var aStarGraph = TemporalWars3DEngine.AStarGraph; // 4/27/2010
            if (aStarGraph == null) return true;

            var size = pathBlockSize;
            var nodeStride = aStarGraph.NodeStride;

            // Convert World Cords to Graph Stride Cords
            var xPos = (int)(placementPosition.X / nodeStride);
            var yPos = (int)(placementPosition.Z / nodeStride);

            // 3/25/2009 - Updated to * -1 by the (size - 1).  This is so the MCV units
            //             will have the blocking done correctly.
            // 1/5/2009 - Set path nodes using the size given on the X and Y directions.                
            var xRowStart = -1 * (size - 1); // 4/27/2010
            for (var xRow = xRowStart; xRow < size; xRow++)
                for (var yRow = xRowStart; yRow < size; yRow++)
                {
                    var xValue = (xPos + xRow) * nodeStride;
                    var yValue = (yPos + yRow) * nodeStride;

                    if (x == xValue && y == yValue)
                        return true;
                }

            return false;
        }

        // 3/2/2009
        /// <summary>
        /// Once a <see cref="SceneItem"/> is placed on the <see cref="Terrain"/>, this virtual method is called
        /// in order to set its placement in the AStarGraph component, using the PathBlockSize.
        /// </summary>
        /// <param name="placementPosition">The <see cref="Vector3"/> position to place item at.</param>
        public virtual void SetPlacement(ref Vector3 placementPosition)
        {
            // Should be filled in by inherited classes
            return;
        }
        
        /*public virtual void OnCreateDevice()
        {
            return;
        }*/

        // 2/24/2009
        /// <summary>
        /// Triggers the event <see cref="SceneItemCreated"/>
        /// </summary>
        protected void OnSceneItemCreated()
        {
            if (SceneItemCreated != null)
            {
                SceneItemCreated(this, EventArgs.Empty);
            }
        }

        // 2/24/2009
        /// <summary>
        /// Triggers the event <see cref="SceneItemDestroyed"/>
        /// </summary>
        protected void OnSceneItemDestroyed()
        {
            if (SceneItemDestroyed != null)
            {
                SceneItemDestroyed(this, EventArgs.Empty);
            }
        }


        // 7/27/2009: Updated to allow Buildings assinged on client side, so the Queues are correctly
        //            removed when buildings destroyed!
        // 6/15/2009; 7/16/2009 - Updated to ONLY have Host set, and use the 'wasAssigned' flag.
        /// <summary>
        /// Assigns the <paramref name="eventHandler"/> given to the <see cref="SceneItemDestroyed"/> event.  Only one
        /// is allowed to be added at any one time, which is automatically checked.
        /// </summary>
        /// <param name="eventHandler">The <see cref="EventHandler"/> instance to assign</param>
        /// <param name="attacker"><see cref="SceneItem"/> as attacker</param>
        public void AssignEventHandler_SceneItemDestroyed(EventHandler eventHandler, SceneItem attacker)
        {
            // 7/27/2009: Updated to allow Buildings assinged on client side, so the Queues are correctly
            //            removed when buildings destroyed!
            // 7/16/2009 - For MP games, only allow Host to assign.

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            var networkSession = player.NetworkSession;

            if (networkSession != null && !networkSession.IsHost && !(attacker is BuildingScene))
                return;

            // 7/16/2009 - Check if already assigned
            if (attacker == null || attacker._wasAssignedToDestroyedEventHandler) return;

            // Set Assigned                
            attacker._wasAssignedToDestroyedEventHandler = true;
                
            // Assign EventHandler
            SceneItemDestroyed += eventHandler;
        }

        // 6/15/2009; 7/16/2009 - Updated to ONLY have Host set, and use the 'wasAssigned' flag.
        /// <summary>
        /// Removes the <paramref name="eventHandler"/> given from the event <see cref="SceneItemDestroyed"/>.
        /// </summary>
        /// <param name="eventHandler">The <see cref="EventHandler"/> instance to remove</param>
        /// <param name="attacker"><see cref="SceneItem"/> as attacker</param>
        public void RemoveEventHandler_SceneItemDestroyed(EventHandler eventHandler, SceneItem attacker)
        {
             // 7/16/2009 - For MP games, only allow Host to assign.
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            var networkSession = player.NetworkSession;
            if (networkSession != null && !networkSession.IsHost)
                return;

            if (SceneItemDestroyed != null)
                SceneItemDestroyed -= eventHandler;

            // 7/16/2009 - Remove Assignie flag
            if (attacker != null)
                attacker._wasAssignedToDestroyedEventHandler = false;
            
        }

        // 10/12/2009
        /// <summary>
        /// Flashes the given <see cref="SceneItem"/> 'White' for the 
        /// specified amount of time given in seconds.
        /// </summary>
        /// <param name="timeInSeconds">Time in seconds to flash white</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="timeInSeconds"/> is less than 1.</exception>
        public virtual void FlashItemWhite(int timeInSeconds)
        {
            // verify seconds are at least 1 or greater.
            if (timeInSeconds < 1)
                throw new ArgumentOutOfRangeException("timeInSeconds", @"Time in seconds must be at least 1 or greater.");

            // set internal values
            _showFlashWhiteOn = true;
            _flashForGivenMilliSeconds = timeInSeconds * 1000; // convert to milliseconds.

            // Set FlashState to TRUE.
            SetFlashWhiteState(true);
        }

      

        // 11/21/2009
        /// <summary>
        /// Checks if this <see cref="SceneItem"/> has the 'FlashWhite' ON, which a 
        /// countdown is done to turn off the flashing.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        internal void DoFlashingWhiteCheck(ref TimeSpan elapsedTime)
        {
            if (!_showFlashWhiteOn) return;

            // Is time up for flashing?
            _flashForGivenMilliSeconds -= elapsedTime.Milliseconds;
            if (_flashForGivenMilliSeconds >= 0) return;

            _showFlashWhiteOn = false; // Turn off

            // Set FlashState to FALSE.
            SetFlashWhiteState(false);
            
        }

        // 11/21/2009
        /// <summary>
        /// Directly sets to turn On/Off the flash state.
        /// </summary>
        /// <param name="setFlashState">Set flash state?</param>
        private void SetFlashWhiteState(bool setFlashState)
        {
            // 4/27/2010 - Cache
            var shapeItem = ShapeItem;
            if (shapeItem == null) return;

            // check if shapeItem is ScenaryShape
            var scenaryShapeItem = (shapeItem as ScenaryItemShape);
            if (scenaryShapeItem != null)
            {
                // Get proper InstancedItemData struct
                var instancedItemData = scenaryShapeItem.InstancedItemDatas[scenaryShapeItem.InstancedItemPickedIndex];

                // Start Flash White!
                InstancedItem.UpdateInstanceModelToFlashWhite(ref instancedItemData, PlayerNumber, setFlashState);
            }
            else // Start Flash White!
                InstancedItem.UpdateInstanceModelToFlashWhite(ref shapeItem.InstancedItemData, PlayerNumber,
                                                              setFlashState);
        }

        // 2/23/2009
        /// <summary>
        /// Leaf classes should call this to return their instance to <see cref="PoolManager"/>.
        /// </summary>
        /// <remarks>By 'Leaf', it is meant the final inherting <see cref="SceneItem"/> of the given hierachy.</remarks>
        /// <param name="isInterfaceDisplayNode">Is this called by an <see cref="IFDTile"/></param>
        public virtual void ReturnItemToPool(bool isInterfaceDisplayNode)
        {
            // Leaf classes should call this to return their instance to PoolManager.
            return;
        }

        // 10/26/2009
        /// <summary>
        /// Helper method, which checks if given <see cref="SceneItem"/> is of type <see cref="ScenaryItemScene"/>,
        /// and throws an error if TRUE. Used primarily in the Scripting classes.
        /// </summary>
        /// <param name="sceneItemToCheck"><see cref="SceneItem"/> instance to check</param>
        public static void DoCheckIfSceneItemIsScenaryItemType(SceneItem sceneItemToCheck)
        {
            var scenaryItemCheck = (sceneItemToCheck as ScenaryItemScene);
            if (scenaryItemCheck != null)
                throw new ArgumentException(@"The given Name refers to a ScenaryItem, which is not allowed for this method.", "sceneItemToCheck");
        }

        // 6/10/2012
        /// <summary>
        /// Helper method, which checks if given <see cref="SceneItem"/> is of type <see cref="SceneItemWithPick"/>,
        /// and throws an error if TRUE. Used primarily in the Scripting classes.
        /// </summary>
        /// <param name="sceneItemToCheck"><see cref="SceneItem"/> instance to check</param>
        public static void DoCheckIfSceneItemIsPlayableItemType(SceneItem sceneItemToCheck)
        {
            var playableItemCheck = (sceneItemToCheck as SceneItemWithPick);
            if (playableItemCheck != null)
                throw new ArgumentException(@"The given Name refers to a PlayableItem, which is not allowed for this method.", "sceneItemToCheck");
        }

        // 2/28/2011; 5/20/2012 - Moved down from SceneItemWithPick
        /// <summary>
        /// Helper method, which checks if the current <see cref="SceneItem"/> position is within
        /// the 'MoveToPosition', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <returns>True/False</returns>
        internal virtual bool HasReachedMoveToPosition(Vector3 moveToPosition)
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = moveToPosition.X, Y = moveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        // 2/28/2011; 5/20/2012 - Moved down from SceneItemWithPick
        /// <summary>
        /// Helper method, which checks if the current <see cref="SceneItem"/> position is within
        /// the 'MoveToPosition', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <returns>True/False</returns>
        public virtual bool HasReachedMoveToPosition(ref Vector3 moveToPosition)
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = moveToPosition.X, Y = moveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        // 2/28/2011; 5/20/2012 - Moved down from SceneItemWithPick
        /// <summary>
        /// Helper method, which checks of the current <see cref="SceneItem"/> position is within
        /// the given 'Position', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <param name="moveToPosition">Vector3 position</param>
        /// <returns>True/False of result</returns>
        internal bool HasReachedGivenPosition(ref Vector3 moveToPosition)
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = moveToPosition.X, Y = moveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        // 6/11/2012
        /// <summary>
        /// Creates a new rotation <see cref="Quaternion"/> on the requested <paramref name="rotationAxis"/> with
        /// the given <paramref name="rotationValue"/>.
        /// </summary>
        /// <param name="rotationAxis"><see cref="RotationAxisEnum"/> to affect.</param>
        /// <param name="rotationValue">Rotation value to use</param>
        protected internal virtual void SetRotationByValue(RotationAxisEnum rotationAxis, float rotationValue)
        {
            Matrix rotationOnAxis;

            switch (rotationAxis)
            {
                case RotationAxisEnum.RotationOnX:
                    rotationOnAxis = Matrix.CreateRotationX(MathHelper.ToRadians(rotationValue));
                    break;
                case RotationAxisEnum.RotationOnY:
                    rotationOnAxis = Matrix.CreateRotationY(MathHelper.ToRadians(rotationValue));
                    break;
                case RotationAxisEnum.RotationOnZ:
                    rotationOnAxis = Matrix.CreateRotationZ(MathHelper.ToRadians(rotationValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("rotationAxis");
            }

            Quaternion newQuaternion;
            Quaternion.CreateFromRotationMatrix(ref rotationOnAxis, out newQuaternion);

            // Save new Quaternion
            Rotation = newQuaternion;
        }

        // 5/5/2009; 6/10/2012 - Pushed up to SceneItem level.
        /// <summary>
        /// Helper method to initalize the internal audio emitter and listner variables.
        /// </summary>
        protected virtual void UpdateAudioEmitters()
        {
            // 5/24/2009 - Verify NaN is not present.
            if (float.IsNaN(position.X))
                position.X = 0;

            if (float.IsNaN(position.Y))
                position.Y = 0;

            if (float.IsNaN(position.Z))
                position.Z = 0;

            // skip null items
            if (AudioEmitterI == null || AudioListenerI == null)
                return;

            // Init 3D Position
            AudioEmitterI.Position = position;
            AudioEmitterI.Up = Vector3.Up;
            AudioEmitterI.Forward = Vector3.Forward;
            AudioListenerI.Position = Camera.CameraPosition;
        }
    }
}
