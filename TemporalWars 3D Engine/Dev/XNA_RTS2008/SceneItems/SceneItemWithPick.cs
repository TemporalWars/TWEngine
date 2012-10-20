#region File Description
//-----------------------------------------------------------------------------
// SceneItemWithPick.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ImageNexus.BenScharbach.TWEngine.AI;
using ImageNexus.BenScharbach.TWEngine.AI.FSMStates;
using ImageNexus.BenScharbach.TWEngine.Audio;
using ImageNexus.BenScharbach.TWEngine.Audio.Enums;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Explosions;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.Enums;
using ImageNexus.BenScharbach.TWEngine.ForceBehaviors.SteeringBehaviors;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.GameScreens;
using ImageNexus.BenScharbach.TWEngine.IFDTiles;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.MemoryPool;
using ImageNexus.BenScharbach.TWEngine.Networking;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.Players;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Enums;
using ImageNexus.BenScharbach.TWEngine.SceneItems.Structs;
using ImageNexus.BenScharbach.TWEngine.Shapes;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.rtsCommands;
using ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar;
using ImageNexus.BenScharbach.TWTools.MemoryPoolComponent;
using ImageNexus.BenScharbach.TWTools.ScreenTextDisplayer.ScreenText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

#if !XBOX360
#endif


namespace ImageNexus.BenScharbach.TWEngine.SceneItems
{
    /// <summary>
    /// The <see cref="SceneItemWithPick"/> class inherits all the functionality from <see cref="SceneItem"/> adding the ability to pick.
    /// </summary>
    public class SceneItemWithPick : SceneItem, ISceneItemWithPick, IFOWSceneItem, IMinimapSceneItem, IStatusBarSceneItem
    {
        private static int _myBit1, _myBit2, _myBit3, _myBit4;
        private static bool _myBitsCreated;
        private static int _sceneItemCount;

        // 2/28/2011

        // 7/31/2009 - FSMAIControl component.
// ReSharper disable InconsistentNaming
        /// <summary>
        /// Reference to <see cref="FSM_AIControl"/>
        /// </summary>
        protected volatile FSM_AIControl _FSMAIControl;
// ReSharper restore InconsistentNaming

        // 7/3/2008 - Bullet Projectile Particles
        // 8/15/2008 - Add Bool 'UseProjectiles', since ScenaryItems don't need these.
        /// <summary>
        /// Collection of bullet <see cref="Projectile"/> particles
        /// </summary>
        private List<Projectile> _projectiles = new List<Projectile>();
        private TimeSpan _timeToNextProjectile1 = TimeSpanZero;
        private TimeSpan _timeToNextProjectile2 = TimeSpanZero;
        private TimeSpan _timeToNextProjectile3 = TimeSpanZero;
        private TimeSpan _timeToNextProjectile4 = TimeSpanZero;

        // 6/2/2009 - 
        /// <summary>
        /// <see cref="AIOrderType"/> Enum
        /// </summary>
        private AIOrderType _aIOrderIssued = AIOrderType.None;
         
        /// <summary>
        /// Reference to <see cref="AStarItem"/> 
        /// </summary>
        public AStarItem AStarItemI;

        /// <summary>
        /// <see cref="DefenseAIStance"/> Enum stance.
        /// </summary>
        private DefenseAIStance _defenseAiStance = DefenseAIStance.Guard; // default.  
 
        /// <summary>
        /// Is this <see cref="SceneItem"/> used as bot helper?
        /// </summary>
        protected internal bool IsBotHelper;
        /// <summary>
        /// The <see cref="ItemGroupType"/> this <see cref="SceneItem"/> can attack.
        /// </summary>
        protected ItemGroupType ItemGroupTypeToAttackE = ItemGroupType.Vehicles;
        /// <summary>
        /// Is this <see cref="SceneItemWithPick"/> moveable?  
        /// </summary>
        private bool _itemMoveable = true;

        // 1/13/2011
        /// <summary>
        /// Is this <see cref="SceneItemWithPick"/> selectable?  (Scripting Purposes)
        /// </summary>
        private bool _itemSelectable = true;

        /// <summary>
        /// This <see cref="SceneItemWithPick"/> is pick selected?
        /// </summary>
        protected bool ThePickSelected;
        /// <summary>
        /// The <see cref="PlayableItemTypeAttributes"/> structure
        /// </summary>
        internal PlayableItemTypeAttributes PlayableItemAtts;
        /// <summary>
        /// This <see cref="SceneItemWithPick"/> instance unique number.
        /// </summary>
        internal readonly int SceneItemNumber;
        
        /// <summary>
        /// Collection used to visually display the selection triangles around a <see cref="SceneItemWithPick"/>
        /// </summary>
        private VertexPositionColor[] _selectionBox = new VertexPositionColor[12];
        /// <summary>
        /// The <see cref="Color"/> to use for selection box.
        /// </summary>
        private Color _selectionBoxColor = Color.Blue; // Default color.

        [Obsolete]
#pragma warning disable 612,618
#pragma warning disable 169
        private ItemMoveState _simulationState;
#pragma warning restore 169
#pragma warning restore 612,618

        /// <summary>
        /// Sprite <see cref="Texture2D"/>
        /// </summary>
        private Texture2D _spriteTexture;
        // 10/7/2008 - Add ForceBehavior Class variables
        /// <summary>
        /// Reference to the <see cref="ForceBehaviorsCalculator"/>
        /// </summary>
        public ForceBehaviorsCalculator ForceBehaviors;
        /// <summary>
        /// Reference to <see cref="ITerrainShape"/>
        /// </summary>
        internal ITerrainShape TerrainShape;

        /// <summary>
        /// Rectangle area designated safe to draw.
        /// </summary>
        protected Rectangle TitleSafe;

        internal bool UpdateInterpolation;

        /// <summary>
        /// Allow use of <see cref="Projectile"/>
        /// </summary>
        protected bool UseProjectiles;

        // 8/15/2009 -
        ///<summary>
        /// Reference to <see cref="PoolItem"/> wrapper.
        ///</summary>
        public PoolItem PoolItemWrapper;

        // 11/11/2009 - 
        /// <summary>
        /// <see cref="ScreenTextItem"/> used to display the <see cref="SpecialGroupNumber"/> of <see cref="SceneItem"/>.
        /// </summary>
        protected internal ScreenTextItem ScreenTextSpecialGroupNumber;
        // 11/11/2009 - 
        /// <summary>
        /// Stores the current <see cref="SpecialGroupNumber"/> assigned by user.
        /// </summary>
        private short _specialGroupNumber;

        // XNA 4.0 Updates
        private static readonly RasterizerState RasterizerStatePrimTriangle = new RasterizerState { FillMode = FillMode.Solid };
        private static readonly DepthStencilState DepthStencilStatePrimTriangle = new DepthStencilState { DepthBufferEnable = false };

        #region Properties

        // ShapeItem
        /// <summary>
        /// Returns the <see cref="IFogOfWarShapeItem"/> instance.
        /// </summary>
        IFogOfWarShapeItem IFOWSceneItem.ShapeItem
        {
            get { return ShapeItem; }
        }

        /// <summary>
        /// Stores the unique item number for this <see cref="IFOWSceneItem"/>. 
        /// </summary>
        /// <remarks>
        /// It should be set to either the 'SceneItemNumber' or 'NetworkItemNumber', depending if 
        /// SP or MP game type.
        /// </remarks>
        int IFOWSceneItem.UniqueItemNumber
        {
            get { return UniqueItemNumber; }
        }

        // 5/30/2011
        ///<summary>
        /// Stores the unique item number for this <see cref="SceneItem"/>.
        ///</summary>
        public int UniqueItemNumber
        {
            get { return (NetworkGameComponent.NetworkSession == null) ? SceneItemNumber : NetworkItemNumber; }
        }

        // ShapeItem
        /// <summary>
        /// Returns the <see cref="IMinimapShapeItem"/> instance.
        /// </summary>
        IMinimapShapeItem IMinimapSceneItem.ShapeItem
        {
            get { return ShapeItem; }
        }

        // 12/31/2009 - Convert to AutoProperty
        ///<summary>
        /// Is current position value the final position for this <see cref="SceneItem"/>?
        ///</summary>
        /// <remarks>Generally only applies the static items which do not move, like a <see cref="BuildingScene"/></remarks>
        public bool ItemPlacedInFinalPosition { get; set; }

        // 12/31/2009 - Convert to AutoProperty
        ///<summary>
        /// The <see cref="GameTime"/> this item was placed at.
        ///</summary>
        /// <remarks>This is used to determine <see cref="IFogOfWar"/> visibility for enemy players.</remarks>
        public double TimePlacedAt { get; set; }
       

        /// <summary>
        /// Stores the current <see cref="SpecialGroupNumber"/> assigned by user.
        /// When nothing assign, it will be set to -1.
        /// </summary>
        public short SpecialGroupNumber
        {
            get { return _specialGroupNumber; }
            set
            {
                _specialGroupNumber = value;

                // set into ScreenTextItem.
                ScreenTextSpecialGroupNumber.SbDrawText.Length = 0; // Required step, in order to start the appending at the beg!

               ScreenTextSpecialGroupNumber.SbDrawText.Append(value);

            }
        }

        // 10/10/2009 -
        /// <summary>
        /// The <see cref="AttackByLastItemType"/> stores the last <see cref="ItemType"/> 
        /// who attacked this <see cref="SceneItem"/>.  Also
        /// contains the <see cref="GameTime"/> of the attack. (Scripting purposes)
        /// </summary>
        public AttackByLastItemType? AttackBy { get; set; }

        // 10/10/2009 - 
        /// <summary>
        /// Stores the <see cref="ItemType"/> who destroyed this <see cref="SceneItem"/>. (Scripting purposes)
        /// </summary>
        public ItemType? DestroyedBy { get; set; }

        // 8/3/2009 - 
        /// <summary>
        /// Should do a self-repair?
        /// </summary>
        public bool StartSelfRepair { get; set; }

        // 12/10/2008 -
        /// <summary>
        /// Is this <see cref="SceneItemWithPick"/> currently pick hovered by cursor?
        /// </summary>
        public bool PickHovered { get; set; }

        // 10/16/2008 - 
        /// <summary>
        /// Is visible obstacle; updated in <see cref="ForceBehaviorsCalculator"/>, specifically
        /// the <see cref="ObstacleAvoidanceBehavior"/> AbstractBehavior.
        /// </summary>
        public bool IsVisibleObstacle { get; set; }

        // 9/3/2008 - 
        /// <summary>
        /// Network sync <see cref="SceneItem"/> unique number, assigned by the Server during network games.  
        /// </summary>
        /// <remarks>This unique number is unique across all player's computers, allowing reference to the same <see cref="SceneItem"/>.</remarks>
        public int NetworkItemNumber { get; set; }

        ///<summary>
        /// The path node position, used in the <see cref="AStarItem"/> class.
        ///</summary>
        public Vector3 PathNodePosition
        {
            get { return AStarItemI != null ? AStarItemI.PathNodePosition : Vector3Zero; }
            protected set
            {
                if (AStarItemI != null)
                    AStarItemI.PathNodePosition = value;
            }
        }

        /// <summary>
        /// The last node position, used in the <see cref="AStarItem"/> class.
        /// </summary>
        protected Vector3 LastPosition
        {
            get { return AStarItemI != null ? AStarItemI.LastPosition : Vector3Zero; }
            set
            {
                if (AStarItemI != null)
                    AStarItemI.LastPosition = value;
            }
        }

        ///<summary>
        /// The <see cref="IgnoreOccupiedBy"/> is used during A* searches, and a <see cref="SceneItem"/>
        /// with this set, can make other units move out the way; hence, ignoring their occupied flag!
        ///</summary>
        public IgnoreOccupiedBy IgnoreOccupiedByFlag
        {
            get { return AStarItemI != null ? AStarItemI.IgnoreOccupiedByFlag : IgnoreOccupiedBy.Off; }
            set
            {
                if (AStarItemI != null)
                    AStarItemI.IgnoreOccupiedByFlag = value;
            }
        }

        ///<summary>
        /// Is this <see cref="SceneItemWithPick"/> moveable?
        ///</summary>
        public bool ItemMoveable
        {
            get { return _itemMoveable; }
            set { _itemMoveable = value; }
        }

        // 1/13/2011
        ///<summary>
        /// Is this <see cref="SceneItemWithPick"/> selectable? (Scripting purposes)
        ///</summary>
        public bool ItemSelectable
        {
            get { return _itemSelectable; }
            set { _itemSelectable = value; }
        }

        ///<summary>
        /// The <see cref="ItemGroupType"/> this <see cref="SceneItem"/> can attack.
        ///</summary>
        public ItemGroupType ItemGroupTypeToAttack
        {
            get { return ItemGroupTypeToAttackE; }
            set { ItemGroupTypeToAttackE = value; }
        }

        // 6/1/2009
        /// <summary>
        /// <see cref="DefenseAIStance"/> Enum stance.
        /// </summary>
        public DefenseAIStance DefenseAIStance
        {
            get { return _defenseAiStance; }
            set { _defenseAiStance = value; }
        }

        // 6/2/2009
        ///<summary>
        /// <see cref="AIOrderType"/> Enum
        ///</summary>
        public AIOrderType AIOrderIssued
        {
            get { return _aIOrderIssued; }
            set { _aIOrderIssued = value; }
        }

        /// <summary>
        /// This <see cref="SceneItemWithPick"/> is pick selected?
        /// </summary>
        public bool PickSelected
        {
            get { return ThePickSelected; }
            set
            {
                ThePickSelected = value;

                // 4/11/2009 - Call Empty Virtual method
                OnPickSelected(value);
            }
        }

        ///<summary>
        /// The overall goal position to get this <see cref="SceneItemWithPick"/> to, used
        /// in the <see cref="AStarItem"/> class.
        ///</summary>
        public Vector3 GoalPosition
        {
            get { return AStarItemI != null ? AStarItemI.GoalPosition : Vector3Zero; }
            set
            {
                if (AStarItemI != null)
                    AStarItemI.GoalPosition = value;
            }
        }

        ///<summary>
        /// The current move-to position for this <see cref="SceneItemWithPick"/>, used 
        /// in the <see cref="AStarItem"/> class.
        ///</summary>
        public new Vector3 MoveToPosition
        {
            get { return AStarItemI != null ? AStarItemI.MoveToPosition : Vector3Zero; }
            set
            {
                if (AStarItemI != null)
                    AStarItemI.MoveToPosition = value;
            }
        }

        /// <summary>
        /// Controls how quickly this <see cref="SceneItemWithPick"/> can turn from side to side.
        /// </summary>
        public float ItemTurnSpeed { get; set; }

        

        /// <summary>
        /// The direction this <see cref="SceneItemWithPick"/> is facing, in radians. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value given is not with allowable range of -pi to pi.</exception>
        public virtual float FacingDirection
        {
            get { return _facingDirection; }
            set
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"Angle must be in Radian measurement; - pi to pi.");


                _facingDirection = value;
            }
        }
        private float _facingDirection;

        /// <summary>
        /// A constant offset to apply to the <see cref="FacingDirection"/> to fix
        /// shapes which might not being facing in the desired direction when created.
        /// </summary>
        /// <remarks>This is useful when you need to fix the rotation of the artwork, but do not have access to the original file to fix outside the game.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when value given is not with allowable range of -pi to pi.</exception>
        public float FacingDirectionOffset
        {
            get { return _facingDirectionOffset; }
            set
            {
                // 8/23/2009 - Make sure value given is in Radian measurement.
                const float pi = MathHelper.Pi;
                if (value < -pi || value > pi)
                    throw new ArgumentOutOfRangeException("value", @"Angle must be in Radian measurement; - pi to pi.");


                _facingDirectionOffset = value;
            }
        }
        private float _facingDirectionOffset;

        ///<summary>
        /// This <see cref="SceneItemWithPick"/> current <see cref="ItemStates"/>
        ///</summary>
        public ItemStates ItemState
        {
            get { return AStarItemI != null ? AStarItemI.ItemState : ItemStates.Resting; }
            set
            {
                if (AStarItemI != null)
                    AStarItemI.ItemState = value;
            }
        }

        /// <summary>
        /// Selection box <see cref="Color"/>
        /// </summary>
        public Color SelectionBoxColor
        {
            get { return _selectionBoxColor; }
            set { _selectionBoxColor = value; }
        }

        ///<summary>
        /// When set, the A* solution returned will be checked
        /// a 2nd time, and any redudant nodes (nodes on straight paths),
        /// will be removed.
        ///</summary>
        public bool UseSmoothingOnPath
        {
            // 11/7/2009 - Updated to check if NULL.
            get { return AStarItemI != null ?  AStarItemI.UseSmoothingOnPath : false; }
            set { if (AStarItemI != null) AStarItemI.UseSmoothingOnPath = value; }
        }

        ///<summary>
        /// Is this <see cref="SceneItemWithPick"/> currently attacking some other <see cref="SceneItem"/>
        ///</summary>
        public bool AttackOn { get; set; }

        // 10/19/2009
        /// <summary>
        /// Tracks if <see cref="AttackOn"/> order was given, which is
        /// checked by the <see cref="FSM_Machine"/> and affects the <see cref="FSM_State"/> to 
        /// transition to the <see cref="AttackMoveState"/>.
        /// </summary>
        public bool AttackMoveOrderIssued { get; set; }

        // 10/19/2009
        /// <summary>
        /// Stores the original <see cref="AttackMoveState"/> goal position, when
        /// the order was issued.  This will be used by the <see cref="FSM_Machine"/>
        /// <see cref="AttackMoveState"/>, to know when the item has
        /// reached the goal; ultimately, this is important because the
        /// regular <see cref="GoalPosition"/> value can change during attacking! 
        /// </summary>
        public Vector3 AttackMoveGoalPosition { get; set; }

        // 10/21/2009
        /// <summary>
        /// Stores a set of <see cref="AttackMoveState"/> goal positions, which are
        /// only needed for WaypointPath script orders!  This will be
        /// updated with the waypoint positions of a path, and then
        /// checked in the <see cref="AttackMoveState"/>, to know when the item
        /// has reached the final goal. (Scripting Purposes)
        /// </summary>
        public Queue<Vector3> AttackMoveQueue { get; set; }

        // 10/21/2009
        /// <summary>
        /// When set, forces the <see cref="FSM_Machine"/> to do
        /// the <see cref="RepairState"/>.  (Scripting Purposes)
        /// </summary>
        public bool DoRepair { get; set; }

        // 2/28/2011
        /// <summary>
        /// Set by the <see cref="BuildingShape"/> class when some unit
        /// is about to move to Seek position-1.
        /// </summary>
        public bool DoSeekOutOfBuildingCheck { get; set; }

        // 2/28/2011
        /// <summary>
        /// Set by the <see cref="BuildingShape"/> owner who produced the unit.
        /// </summary>
        public BuildingShape WarFactoryOwner { get; set; }

        #endregion

        #region Constructors

        private BoundingSphere _boundingSphere;
        private bool _statusBarCreated;

        /// <summary>
        /// Creates a <see cref="SceneItemWithPick"/> with a shape to be rendered at an initial Position
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="shape">The <see cref="Shape"/> to be rendered for this <see cref="SceneItemWithPick"/></param>
        /// <param name="initialPosition">The initial Position of the <see cref="SceneItemWithPick"/></param>       
        /// <param name="playerNumber">The <see cref="Player"/> number</param>
        protected SceneItemWithPick(Game game, Shape shape, ref Vector3 initialPosition, byte playerNumber)
            : base(game, shape, initialPosition)
        {
            if (game == null) return;

            // 11/11/2009 - Create ScreenTextItem to display SpecialGroup number.
            ScreenTextManager.AddNewScreenTextItem(string.Empty, Vector2.Zero, Color.White, out ScreenTextSpecialGroupNumber);
            ScreenTextSpecialGroupNumber.Visible = false;

            // 11/20/2008 - Player Number for MP games.
            PlayerNumber = playerNumber;

            // 10/21/2009 - Create Queue<Vector3>
            AttackMoveQueue = new Queue<Vector3>();

            // 1/7/2009 - Init Audio Listener & Emitter for 3D sound.
            AudioListenerI = new AudioListener();
            AudioEmitterI = new AudioEmitter();

            // 3/31/2008 - Store quadTerrain struct
            // 8/27/2008 - Updated to get TerrainShape Interface from Game.Components.
            TerrainShape = (ITerrainShape) game.Services.GetService(typeof (ITerrainShape));

            // 5/20/2008 - Update SceneItem Count
            _sceneItemCount++;
            SceneItemNumber = _sceneItemCount;

            // 9/8/2008 - Create Bit Masks for use during Network Games
            //            Only needs to be created once, since these are Static masks.
            if (!_myBitsCreated)
            {
                // Obsolete - Not Needed.
                /*_myBit1 = BitVector32.CreateMask();
                _myBit2 = BitVector32.CreateMask(_myBit1);
                _myBit3 = BitVector32.CreateMask(_myBit2);
                _myBit4 = BitVector32.CreateMask(_myBit3);
                BitVector32.CreateMask(_myBit4);*/

                _myBitsCreated = true;
            }

            // 1/22/2011 - Subscribe to the TerrainScreen Unloaded event, in order to return poolItems to the memory pool.
            TerrainScreen.UnLoading += TerrainScreen_UnLoading;


            // 9/8/2008 - Initialize the ItemMoveState Structs, used for Network Games Interpolation               
            // 12/16/2008 - Added SmoothHeading
            /*_simulationState.position = initialPosition;
            _simulationState.velocity = velocity;
            _simulationState.smoothHeading = Vector3Zero;
            _simulationState.facingDirection = 0;*/
            //_previousState = _simulationState;
        }

        // 1/22/2011
        /// <summary>
        /// EventHandler triggered when the <see cref="TerrainScreen"/> is unloading. 
        /// This will then return any <see cref="SceneItemWithPick"/> instances back to the <see cref="PoolManager"/>.
        /// </summary>
// ReSharper disable InconsistentNaming
        void TerrainScreen_UnLoading(object sender, EventArgs e)
// ReSharper restore InconsistentNaming
        {
            ReturnItemToPool(false);
        }


        // 1/30/2009
        /// <summary>
        /// Populates the <see cref="PlayableItemAtts"/> structure with the common attributes
        /// used by the given <see cref="ItemType"/>.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        /// <param name="isFinalPosition">Is <see cref="SceneItemWithPick"/> in final placement position?</param>
        public virtual void LoadPlayableAttributesForItem(ItemCreatedArgs e, bool isFinalPosition)
        {
            // 3/24/2009 - Is this the final Position for SceneItemOwner?
            ItemPlacedInFinalPosition = isFinalPosition;

            // 2/23/2009 - Make sure values are reset, since this could be reused in PoolManager.
            DeleteFromWorldTime = TimeSpan.FromSeconds(10);
            ThePickSelected = false;
            KillSceneItemStarted = false; // 10/3/2009
            KillSceneItemCalled = false; // 11/15/2009
            AttackBy = null; // 10/10/2009
            DestroyedBy = null; // 10/10/2009
            IsBelow25Percent = false; // 11/7/2009
            IsBelow50Percent = false; // 11/7/2009
            SpecialGroupNumber = -1; // 11/11/2009
            CurrentHealthState = HealthState.Alive; // 1/22/2010

            // 1/21/2011 - Reset 'OccupiedAtIndex' to Vector3.Zero
            if (AStarItemI != null) AStarItemI.OccupiedAtIndex = Vector3.Zero;

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            // 10/4/2009 - Update the PlayerStats 'Create'.
            if (player != null) PlayerStats.UpdatePlayersCreateStats(player.PlayerStats, this);

            // 7/19/2009 - Clear out any 'Projectiles'
            if (_projectiles != null)
                _projectiles.Clear();

            // 6/15/2010 - Updated to use new GetPlayer method.
            Player thisPlayer;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out thisPlayer);
            
            // 6/1/2009 - Set if MP/SP game for AIThreadManager.
            AIManager.IsNetworkGame = (thisPlayer == null || thisPlayer.NetworkSession == null) ? false : true;
            
            // Load Playable Atts for this SceneItemOwner
            if (PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(e.ItemType, out PlayableItemAtts))
            {
                // Draw StatusBar?
                CreateStatusBar();

                // 1/16/2011 - Set ItemGroupToAttack
                if (PlayableItemAtts.ItemGroupToAttack != null)
                    ItemGroupTypeToAttack = PlayableItemAtts.ItemGroupToAttack.Value;

                // Speed SceneItemOwner can turn at
                ItemTurnSpeed = PlayableItemAtts.ItemTurnSpeed;
                // FacingDirection Offset
                FacingDirectionOffset = PlayableItemAtts.FacingDirectionOffset;
                // Velocity SceneItemOwner can move at           
                MaxSpeed = PlayableItemAtts.MaxSpeed;
                // Set View Radius
                ViewRadius = PlayableItemAtts.ViewRadius;
                // Set Attack Radius
                AttackRadius = PlayableItemAtts.AttackRadius;
                // Set Health
                StartingHealth = PlayableItemAtts.StartingHealth;
                // Set Selection Box Color
                SelectionBoxColor = (player == null) ? Color.Yellow : player.PlayerColor;
                    // 4/11/2009 - Updated to Player's color
                // Set if we should Ignore the OccupiedBy Flag when A*
                if (AStarItemI != null) AStarItemI.IgnoreOccupiedByFlag = PlayableItemAtts.IgnoreOccupiedByFlag;

                // Set Collision Radius based on Model's BoundingSphere.                
                SetCollisionRadius();
            }
            else
                throw new InvalidOperationException("Unable to load the PlayableItemAtts for given SceneItem!");

            // 5/18/2009 - Fire Event Handler for 'SceneItemCreated'.                
            FireEventHandler_Created(this);
        }

        // 8/4/2009
        /// <summary>
        /// Allows <see cref="SceneItem"/> to create 'Bot' <see cref="SceneItem"/> helpers; other items, which 
        /// follow and help defend the main <see cref="SceneItem"/> parent.
        /// </summary>
        /// <param name="e"><see cref="ItemCreatedArgs"/> instance</param>
        protected internal virtual void CreateBotHelpers(ItemCreatedArgs e)
        {
            return;
        }

        // 4/15/2009
        /// <summary>
        /// Creates the <see cref="IStatusBar"/> when the <see cref="PlayableItemAtts"/> <see cref="SceneItem.DrawStatusBar"/> is true.
        /// </summary>
        protected virtual void CreateStatusBar()
        {

            if (PlayableItemAtts.DrawStatusBar)
            {
                // 4/27/2010 - Cache
                var statusBar = StatusBar;
                if (statusBar == null) return;

                if (!_statusBarCreated)
                {
                    // Add new StatusBarItem instance
                    statusBar.AddNewStatusBarItem(this, out StatusBarItem);

                    if (StatusBarItem != null) StatusBarItem.ShowEnergyOffSymbol = PlayableItemAtts.ShowEnergyOffSymbol;
                    DrawStatusBar = PlayableItemAtts.DrawStatusBar;
                    _statusBarCreated = true;
                }
                else
                {
                    // Turn 'InUse' back on.
                    if (StatusBarItem != null)
                    {
                        StatusBarItem.InUse = true;
                        StatusBarItem.DrawStatusBar = true;
                        statusBar.UpdateStatusBarItem(ref StatusBarItem);
                    }
                }
            }

            // StatusBar OffsetPosition
            StatusBarOffsetPosition2D = PlayableItemAtts.StatusBarOffsetPosition2D;
        }

        // 10/16/2008; 2/24/2009
        /// <summary>
        /// Triggers the event for <see cref="SceneItem.SceneItemCreated"/>.
        /// </summary>
        /// <param name="item"><see cref="SceneItemWithPick"/> instance</param>
        protected virtual void FireEventHandler_Created(SceneItemWithPick item)
        {
            OnSceneItemCreated();
        }

        // 10/16/2008
        /// <summary>
        /// Creates the <see cref="SceneItemWithPick"/> collision radius, using the given <see cref="BoundingSphere"/>.
        /// </summary>
        protected virtual void SetCollisionRadius()
        {
            if (((ShapeWithPick) ShapeItem).ModelInstance == null)
                return;

            // 1/22/2010: Updated to ForEach, since only called once at beg, so shouldn't harm performance on Xbox.
            // Iterate through all Model's Meshs BoundingSphere, to create
            // 1 large Sphere.
            _boundingSphere = new BoundingSphere();
            foreach (var t in ((ShapeWithPick) ShapeItem).ModelInstance.Meshes)
            {
                var boundingSphereAdd = t.BoundingSphere;
                BoundingSphere.CreateMerged(ref _boundingSphere, ref boundingSphereAdd, out _boundingSphere);
            }

            // Set Radius
            CollisionRadius = _boundingSphere.Radius;
        }

        #endregion

        #region Dispose

        // 8/15/2008 - Dispose of Resources
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {

            if (ForceBehaviors != null)
            {
                ForceBehaviors.Dispose();             
                ForceBehaviors = null;
            }

            // Stop all Particles from Drawing or Updating
            if (UseProjectiles)
            {
                // Dispose
                if (_projectiles != null)
                {
                    var count = _projectiles.Count;
                    for (var i = 0; i < count; i++)
                    {
                        if (_projectiles[i] != null)
                            _projectiles[i].Dispose();
                        _projectiles[i] = null;
                    }
                }
            } // End IF UseProjectiles

            if (AStarItemI != null)
                AStarItemI.Dispose(true);

            if (_spriteTexture != null)
                _spriteTexture.Dispose();

            // Null Refs           
            _projectiles = null;
            _spriteTexture = null;
            AStarItemI = null;
            TerrainShape = null;

            // 
            // 1/5/2010 - Note: Up to this point, no InternalDriverError will be thrown in the SpriteBatch.
            //          - Note: Discovered, the error is coming from the call to 'base' dispose!
           

            base.Dispose(finalDispose);
        }

        #endregion

        // 1/26/2009 - Calculate ForceBehavior

        #region ISceneItemWithPick Members
        /// <summary>
        /// Updates any values associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="time"><see cref="TimeSpan"/> structure for time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure for elapsed game sime since last call</param>
        /// <param name="isClientCall">Is this the client-side update in a network game?</param>
        public override void Update(GameTime gameTime, ref TimeSpan time, ref TimeSpan elapsedTime, bool isClientCall)
        {
            // 1/22/2010 - Check 'HealthState' of sceneitem.
            switch (CurrentHealthState)
            {
                case HealthState.Alive:
                    break;
                case HealthState.Dying:
                case HealthState.Dead:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 1/1/2009
            if (Delete) return;

            // 1/30/2009 - Energy is checked for both players in MP games!
            UpdateEnergyConsumption();

            // 7/3/2008 - Check If Attack On.          
            if (AttackOn)
            {
                DoAttackCheck(ref elapsedTime);
            }
            else // 5/12/2009: Stop shooting sound, if playing.
            {
                Audio_ShootProjectile(1, false);
                Audio_ShootProjectile(2, false);
                Audio_ShootProjectile(3, false);
                Audio_ShootProjectile(4, false);
            }

            if ((!isClientCall || ItemMoveable) && AStarItemI != null) 
                AStarItemI.UpdateGameTime(gameTime);

            // Update Projectiles Life Span & Attackie's Health
            if (UseProjectiles)
                UpdateProjectilesLife(gameTime);

            // 4/10/2009 - Do Animation Check
            DoAnimationCheck(gameTime);

            // 4/10/2009 - Do Particles Check
            DoParticlesCheck();

            // 8/3/2009 - Do Self-Repair Check
            DoSelfRepairCheck();

            // 5/5/2009 - Do MoveItem Check
            ItemIsMovingCheck();

            // 2/28/2011 - Do SeekOutOfBuilding Check
            SeekOutOfBuildingCheck();

            // 5/6/2009; 6/10/2012 - Moved up to the 'SceneItem' class.
            //UpdateAudioEmitters();

            base.Update(gameTime, ref time, ref elapsedTime, isClientCall);
        }

        // 2/28/2011
        /// <summary>
        /// Helper method which checks if unit has reached the first translation position.
        /// </summary>
        private void SeekOutOfBuildingCheck()
        {
            if (!DoSeekOutOfBuildingCheck) return;

            if (!HasReachedMoveToPosition()) return;

            // Have unit move to the flag marker position.
            WarFactoryOwner.DoMoveToFlagMarker(this);

            // Turn off
            DoSeekOutOfBuildingCheck = false;
        }

        /// <summary>
        /// Render any items associated with this <see cref="SceneItem"/> and its children
        /// </summary>
        public override void Render()
        {
            // 5/12/2009 - Do calc once here
            const int triangleSize = 12;
            const int triangleSizeHalfed = 6;
            var collisionRadiusMinusTriangleSize = CollisionRadius - triangleSize;

            // If SceneItemOwner Picked, then show Selection box.
            if (ThePickSelected)
                RenderSelectionBox(triangleSizeHalfed, collisionRadiusMinusTriangleSize);

            var aStarItemI = AStarItemI; // 4/27/2010
            if (aStarItemI != null)
                aStarItemI.RenderVisualDebugPaths(ref _selectionBox);

            // 11/11/09 - Set the SpecialGroupNumber postiion for display purposes.
            ScreenTextSpecialGroupNumber.DrawLocationFrom3D = Position;
            // 11/11/09 - Only show when items are selected and not -1 value.
            ScreenTextSpecialGroupNumber.Visible = PickSelected && SpecialGroupNumber != -1;

            // DEBUG: Sets in Shape for Render to show different color if pathFinding.
            //if (ShapeItem.GetType() == typeof(TankShape))
            //  ((TankShape)ShapeItem).ItemState = ItemState;

            base.Render();
        }

        // 11/11/2009
        /// <summary>
        /// Renders the current selection box for this <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="triangleSizeHalfed">Half triangle size</param>
        /// <param name="collisionRadiusMinusTriangleSize">Half collision minus triangle size</param>
        private void RenderSelectionBox(int triangleSizeHalfed, float collisionRadiusMinusTriangleSize)
        {
            // 11/11/09 - Null check
            var selectionBox = _selectionBox; // 4/27/2010 - Cache
            if (selectionBox == null) return;

            // 3/4/2008
            // Create the Selection Box around unit, using Radius of SceneItemOwner and Position.
            //      
            if (selectionBox.Length < 12)
                Array.Resize(ref selectionBox, 12);

            // 9/25/2008 - Updated the SelectinBox code, by removing the List<> Array, and the copying of the 
            //             ListArray to the simple 'VertexPositionColor' array.  Now, the simple array, which
            //             is now called 'SelectionBox', is updated directly.  Eliminates the CopyTo method, which
            //            created garbage, and the need to keep the same data in two arrays. - Ben


            // Create the four triangle corners for the Selection Box using the Radius of SceneItemOwner.
            // Top Triangle
            // Vertex 1
            selectionBox[0].Position.X = position.X;
            selectionBox[0].Position.Y = position.Y;
            selectionBox[0].Position.Z = position.Z - CollisionRadius;
            selectionBox[0].Color = _selectionBoxColor;

            // Vertex 2
            selectionBox[1].Position.X = position.X + triangleSizeHalfed;
            selectionBox[1].Position.Y = position.Y;
            selectionBox[1].Position.Z = position.Z - collisionRadiusMinusTriangleSize;
            selectionBox[1].Color = _selectionBoxColor;

            // Vertex 3
            selectionBox[2].Position.X = position.X - triangleSizeHalfed;
            selectionBox[2].Position.Y = position.Y;
            selectionBox[2].Position.Z = position.Z - collisionRadiusMinusTriangleSize;
            selectionBox[2].Color = _selectionBoxColor;


            // Bottom Triangle
            // Vertex 1
            selectionBox[3].Position.X = position.X;
            selectionBox[3].Position.Y = position.Y;
            selectionBox[3].Position.Z = position.Z + CollisionRadius;
            selectionBox[3].Color = _selectionBoxColor;

            // Vertex 2
            selectionBox[4].Position.X = position.X - triangleSizeHalfed;
            selectionBox[4].Position.Y = position.Y;
            selectionBox[4].Position.Z = position.Z + collisionRadiusMinusTriangleSize;
            selectionBox[4].Color = _selectionBoxColor;

            // Vertex 3
            selectionBox[5].Position.X = position.X + triangleSizeHalfed;
            selectionBox[5].Position.Y = position.Y;
            selectionBox[5].Position.Z = position.Z + collisionRadiusMinusTriangleSize;
            selectionBox[5].Color = _selectionBoxColor;


            // Left Triangle
            // Vertex 1
            selectionBox[6].Position.X = position.X - CollisionRadius;
            selectionBox[6].Position.Y = position.Y;
            selectionBox[6].Position.Z = position.Z;
            selectionBox[6].Color = _selectionBoxColor;

            // Vertex 2
            selectionBox[7].Position.X = position.X - collisionRadiusMinusTriangleSize;
            selectionBox[7].Position.Y = position.Y;
            selectionBox[7].Position.Z = position.Z - triangleSizeHalfed;
            selectionBox[7].Color = _selectionBoxColor;

            // Vertex 3
            selectionBox[8].Position.X = position.X - collisionRadiusMinusTriangleSize;
            selectionBox[8].Position.Y = position.Y;
            selectionBox[8].Position.Z = position.Z + triangleSizeHalfed;
            selectionBox[8].Color = _selectionBoxColor;

            // Bottom Triangle
            // Vertex 1
            selectionBox[9].Position.X = position.X + CollisionRadius;
            selectionBox[9].Position.Y = position.Y;
            selectionBox[9].Position.Z = position.Z;
            selectionBox[9].Color = _selectionBoxColor;

            // Vertex 2
            selectionBox[10].Position.X = position.X + collisionRadiusMinusTriangleSize;
            selectionBox[10].Position.Y = position.Y;
            selectionBox[10].Position.Z = position.Z + triangleSizeHalfed;
            selectionBox[10].Color = _selectionBoxColor;

            // Vertex 3
            selectionBox[11].Position.X = position.X + collisionRadiusMinusTriangleSize;
            selectionBox[11].Position.Y = position.Y;
            selectionBox[11].Position.Z = position.Z - triangleSizeHalfed;
            selectionBox[11].Color = _selectionBoxColor;

            // XNA 4.0 Updates - Final 2 params updated.
            // Draw SelectionBox
            TriangleShapeHelper.DrawPrimitiveTriangle(ref selectionBox, RasterizerStatePrimTriangle, DepthStencilStatePrimTriangle);
        }

        // 4/15/2008: 
        /// <summary>
        /// Return the current <see cref="SceneItemWithPick"/> 3D position in screen cordinates.
        /// </summary>
        /// <param name="screenPos">(OUT) position of 3D item transformed to 2D screen coordinates.</param>
        /// <returns>(OUT) Screen position as <see cref="Point"/></returns>        
        public void GetScreenPos(out Point screenPos)
        {
            var tempPos = TemporalWars3DEngine.GameInstance.GraphicsDevice.Viewport.Project(position, Camera.Projection,
                                                                                            Camera.View, Matrix.Identity);

            var tmpPoint = Point.Zero;
            tmpPoint.X = (int) tempPos.X;
            tmpPoint.Y = (int) tempPos.Y;
            screenPos = tmpPoint;
        }

        /// <summary>
        /// Issues a ground attack order
        /// </summary>       
        public virtual void AttackGroundOrder()
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;
            if (attackie == null) return;

            // 6/9/2010 - Check if Alive, before allowing order.
            if (!IsAlive)
            {
                AttackSceneItem = null;
                AttackOn = false;
                return;
            }

            // 8/27/2009 - Check if attackie is part of ItemGroupToAttack.
            if (!CheckIfProperItemGroupToAttack(attackie))
            {
                AttackSceneItem = null;
                AttackOn = false;

                return;
            }

            // Turn On Attack State
            AttackOn = true;
        }

        // 7/4/2008
        /// <summary>
        /// Issues an attack order to the attackee which must be set in the <see cref="SceneItem.AttackSceneItem"/> 
        /// Property first.  If attackee is outside range of this <see cref="SceneItemWithPick"/>, then NO attacking will commence.
        /// </summary>
        public virtual void AttackOrder()
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;
            if (attackie == null) return;

            // 6/9/2010 - Check if Alive, before allowing order.
            if (!IsAlive)
            {
                AttackSceneItem = null;
                AttackOn = false;
                return;
            }

            // 8/27/2009 - Check if attackie is part of ItemGroupToAttack.
            if (!CheckIfProperItemGroupToAttack(attackie))
            {
                AttackSceneItem = null;
                AttackOn = false;

                return;
            }

            // Turn on Attack State
            AttackOn = true;

            // 7/31/2009 - 
            // 1/1/2008: Add 'ItemMoveable' check, since items like Buildings or Defense units do not MOVE!
            // Get an attack Position near the attackie using the this unit's attacking range.            
            if (ItemMoveable && AIOrderIssued == AIOrderType.NonAIAttackOrderRequest)
            {
                PathToPositionWithinAttackingRange(attackie);
            }

            return;
        }

        // 10/18/2009
        /// <summary>
        /// Issues an attack order, and checks if this <see cref="SceneItemWithPick"/> is within attacking range; is not,
        /// then this <see cref="SceneItemWithPick"/> is moved to be within attacking range, and attacking will then commence. (Scripting Purposes)
        /// </summary>
        /// <param name="attackie"><see cref="SceneItem"/> instance of attackie</param>
        public virtual void AttackOrderToAttackieAnywhereOnMap(SceneItem attackie)
        {
            if (attackie == null)
                return;

            // if too far to attack, move closer to attackie.
            if (!IsAttackieInAttackRadius(attackie))
            {
                // tell attacker to move to position within attackie
                PathToPositionWithinAttackingRange(attackie);
            }

            // Tell Attacker to start attack on Attackie
            AttackSceneItem = attackie;
            AttackOrder();
        }

        // 8/27/2009
        /// <summary>
        /// Checks if the given attackie, is part of the <see cref="ItemGroupType"/> this unit can attack.
        /// </summary>
        /// <param name="attackie"><see cref="SceneItem"/> attackie to check <see cref="ItemGroupType"/></param>
        /// <returns>True/False of result</returns>
        private bool CheckIfProperItemGroupToAttack(SceneItem attackie)
        {
            return ((int)(attackie.ShapeItem as IInstancedItem).ItemGroupType & (int)ItemGroupTypeToAttackE) != 0;
        }

        #endregion

       

        // 1/30/2009
        /// <summary>
        /// Checks of current <see cref="SceneItemWithPick"/> generates energy.  If is
        /// does, an increase is done to player's energy card; otherwise,
        /// it checks if it is a consumer, and reduces by given amount.
        /// </summary>
        private void UpdateEnergyConsumption()
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            // 10/20/2009 - make sure not NULL
            if (player == null) return;

            // 1/30/2009 - Does Building Type generate energy?
            if (PlayableItemAtts.GeneratesEnergy)
            {
                // Increase energy by energy amount
                player.Energy += PlayableItemAtts.EnergyAmount;
            } // End If Gen Rev
            else
            {
                // This SceneItemOwner drains energy, so increase 'EnergyUsed'.
                player.EnergyUsed += PlayableItemAtts.EnergyNeeded;
            }
        }

        // 6/1/2009: Check 'DefenseAIStance', for AI Issued orders.
        // 12/19/2008 - Updated by removing PathTo adding, since this is now
        //              done in the 'AttackOrder' method above.
        // 12/11/2008 - Updated to use PathTo Queue.
        // 11/20/2008      
        /// <summary>
        /// Do an attack routine, by checking distance to target 
        /// and then shooting bullets at attackie.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure of elapsed time</param>
        private void DoAttackCheck(ref TimeSpan elapsedTime)
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;
           
            // 6/3/2009
            if (IsAttackieInAttackRadius(attackie))
            {
                // 7/4/2008
                // Start Shooting At Target                    
                ShootBullets(ref elapsedTime);
            }
        }

        // 6/3/2009
        /// <summary>
        /// Checks if attackie is within the <see cref="SceneItem.AttackRadius"/> area.
        /// </summary>
        /// <param name="attackie"><see cref="SceneItem"/> attackie to check</param>
        /// <returns>True/False</returns>
        protected internal bool IsAttackieInAttackRadius(SceneItem attackie)
        {
            // 7/7/2009
            if (attackie == null) return false;

            // 7/20/2009 Check if within Attacking Radius
            float distance;
            CalculateDistanceToSceneItem(attackie, out distance);

            return distance <= AttackRadius;
        }

        // 8/3/2009
        /// <summary>
        /// Checks if this <see cref="SceneItemWithPick"/> is within the <see cref="BuildingScene.RepairRadius"/>
        /// </summary>
        /// <param name="building"><see cref="BuildingScene"/> instance</param>
        /// <returns>True/False</returns>
        protected internal bool IsWithinRepairRadius(BuildingScene building)
        {
            if (building == null) return false;

            float distance;
            CalculateDistanceToSceneItem(building, out distance);

            return distance <= building.RepairRadius;
        }

        // 8/3/2009
        /// <summary>
        /// Checks if the <see cref="StartSelfRepair"/> flag is true, and
        /// then starts the health increases if within the HQ <see cref="BuildingScene.RepairRadius"/>.
        /// </summary>
        private void DoSelfRepairCheck()
        {
            // Check if Time to do Self-Repair
            if (!StartSelfRepair) return;

            BuildingScene buildingHQ;
            Player.GetPlayersHeadQuarters(PlayerNumber, out buildingHQ);

            // Check if within the HQ Repair Radius.
            if (!IsWithinRepairRadius(buildingHQ)) return;

            // yes, so do repairs.
            IncreaseHealth(1);

            // Is repairs done?
            if (IsHealthAtOrAbovePercentile(1.0f))
                StartSelfRepair = false;
        }

        // 7/3/2008


        // 2/24/2009
        /// <summary>
        /// <see cref="EventHandler"/> for when an attackie <see cref="SceneItem"/> is destroyed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains no event data.</param>
        protected void AttackSceneItem_SceneItemDestroyed(object sender, EventArgs e)
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;

            // Turn off attack state
            AttackOn = false;
            // 6/2/2009 - Set AIOrderIssued to None state.
            _aIOrderIssued = AIOrderType.None;

            // Remove EventHandler Reference
            if (attackie != null)
                attackie.RemoveEventHandler_SceneItemDestroyed(AttackSceneItem_SceneItemDestroyed, this); // 6/15/2009

            // Remove Attackie Reference
            AttackSceneItem = null;
           
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(TemporalWars3DEngine.SThisPlayer, out player);

            if (player == null) return;

            // 7/16/2009 - If MP Game and Host, then send Client 'CeaseAttack' order.
            if (player.NetworkSession != null && player.NetworkSession.IsHost)
            {
                SendCeaseAttackOrderToMPPlayer();
            }


#if DEBUG
            //System.Diagnostics.Debug.WriteLine("EventHandler triggered for AttackSceneItem_SceneItemDestroyed." + networkItemNumber.ToString());
#endif
        }

        // 6/3/2009; 3/4/2011 - Updated to include bool return.
        /// <summary>
        /// Moves <see cref="SceneItem"/> to a position close enough to attack the attackie.
        /// </summary>
        /// <param name="attackie"><see cref="SceneItem"/> to attack</param>
        protected internal bool PathToPositionWithinAttackingRange(SceneItem attackie)
        {
            if (attackie == null) return false;

            Debug.WriteLine(string.Format("SceneItem#{0} PathToPositionWithinAttackingRange.", UniqueItemNumber));

            var attackiePos = attackie.Position;

            // 1st - Calculate distance between current vector and center vector.
            float distance;
            CalculateDistanceToPosition(ref attackiePos, out distance);
            
            // If distance within attackRadius, then just return.
            if (distance < AttackRadius) return false;

            // Otherwise, obtain new position within attackie's radius.
            Vector3 newPosition;
            GetPositionWithinGivenCircularRadius(ref attackiePos, ref position, distance, AttackRadius,
                                                 out newPosition);

            Debug.WriteLine(string.Format("SceneItem#{0} to attack position {1}.", UniqueItemNumber, newPosition));
            
            // Queue new Goal Position to PathFind to.
            AStarItemI.AddWayPointGoalNode(ref newPosition);

            return true;
        }

        // 7/20/2009: Updated to calculate the lerpDistance, using the 'GAP' between attackie and attacker's range!
        // 12/22/2008 - 
        /// <summary>
        /// Used to get some position around a given 'center' position, at
        /// some given radius away from the 'center' position.
        /// </summary>
        /// <remarks>
        /// How: Need a distance to Lerp (Interpolate) between the current
        ///      position and the attackie's position, giving a result of
        ///      the attack position to move to.
        ///      In order to achieve this goal, do the following;
        ///      1 - Calculate distance between current vector and attackie vector.
        ///      2 - Get ratio of total distance divided by this units attacking radius distance.
        ///      3 - Using result from step 2, Lerp between the attackie vector and current units vector, to 
        ///          get the new goal-position.
        /// </remarks>
        /// <param name="centerPosition"><see cref="Vector3"/> as center position</param>
        /// <param name="currentPosition"><see cref="Vector3"/> as current position</param>
        /// <param name="distance"></param>
        /// <param name="attackRadius"><see cref="SceneItem.AttackRadius"/> value</param>
        /// <param name="newPosition">(OUT) New <see cref="Vector3"/> position</param>
        /// <returns>true/false of result</returns>
        private void GetPositionWithinGivenCircularRadius(ref Vector3 centerPosition, ref Vector3 currentPosition, float distance,
                                                           float attackRadius, out Vector3 newPosition)
        {
            // 5/30/2011 - Const gapBias
            const float gapBias = 110.0f;

#if DEBUG
            Debug.WriteLine(string.Format("SceneItem#{0} GetPositionWithinGivenCircularRadius.", UniqueItemNumber));
#endif

            // 7/20/2009 - Calculate difference between testRadius and Total distance.
            var distanceGap = distance - attackRadius;

            // 2nd - Get ratio of Total distance divided by this units attacking radius distance.
            //       Note: AttackRadius is increased by 110, to guarantee unit is within firing range!
            //
            //       Idea: *attackie* ----- ( --------------*Attacker* ---------------) <- The Attacker's AttackRadius.
            //                        ^-- Here is the distance 'Gap' between attacker's range, and the SceneItemOwner to attack.
            //              So, for example, if 'Gap' is 20 feet, and Attacker's radius is 60 feet, then the Total
            //              distance would be 80 feet between attackie and attacker.  Therefore, the 'Gap' is calculated,
            //              and a bias of 110 is added to create a 'newPosition' within the attacker's range of fire.
            // 8/2/2009: Fix: The lerpDistance must be subtracted from 1.0, in order to get the correct Lerp percentage.
            
            // 5/30/2011 - Guarantee distanceGap is not greater than the total distance with bias.
            distanceGap = (distanceGap + gapBias < distance) ? distanceGap + gapBias : distance;

            var lerpDistance = 1 - (distanceGap / distance);

            // 3rd -  Using result from step 2, Lerp between the center vector and currentPosition, to 
            //        get the newPosition!               
            Vector3.Lerp(ref centerPosition, ref currentPosition, lerpDistance, out newPosition);
            
            
        }


        // 7/3/2008; 1/15/2009
        /// <summary>
        /// Base delegate function used to <see cref="ShootBullets"/>; this can
        /// be override with an inherits classes own <see cref="ShootBullets"/>
        /// function call, to perform other activities specific to
        /// the inherited <see cref="SceneItemWithPick"/>; for example, moving the turret on 
        /// a tank before actually firing the bullets!
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        protected virtual void ShootBullets(ref TimeSpan elapsedTime)
        {
            // 6/11/2010 - Skip if not alive; done as sanity check.
            if (!IsAlive) return;

            // Add Projectiles to fire
            UpdateProjectilesCreation(ref elapsedTime);
        }
        
        // 10/14/2009
        /// <summary>
        /// Checks if the given <paramref name="angleDifference"/> value is within 1 degree.  The
        /// <paramref name="angleDifference"/> value is the calculation from the TurnToFace method.
        /// </summary>
        /// <param name="angleDifference">Angle difference to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="angleDifference"/> is not within allowable range of -pi to pi.</exception>
        /// <returns>True/False</returns>
        public static bool IsFacingTargetWithin1Degrees(float angleDifference)
        {
            // 8/23/2009 - Make sure Angle's given are all in Radians measurement.
            const float pi = MathHelper.Pi;

            if (angleDifference < -pi || angleDifference > pi)
                throw new ArgumentOutOfRangeException("angleDifference", @"Angle must be in Radian measurement; - pi to pi.");

            return (Math.Abs(MathHelper.ToDegrees(angleDifference)) <= 1.0f);
        }

        // 10/14/2009
        /// <summary>
        /// Tells the <see cref="SceneItemWithPick"/> to face the given Waypoint position. (Scripting Purposes)
        /// </summary>
        /// <param name="waypointPosition">The <see cref="Vector3"/> Waypoint position to face</param>
        public void FaceWaypointPosition(ref Vector3 waypointPosition)
        {
            // Get the 'TurnToFace' AbstractBehavior
            var turnToFaceBehavior = (TurnToFaceBehavior)ForceBehaviors.GetBehavior(BehaviorsEnum.TurnToFace);

            // make sure not NULL
            if (turnToFaceBehavior == null) return;

            // Set to use this Waypoint to turn to
            turnToFaceBehavior.FaceWaypoint = true;
            turnToFaceBehavior.WaypointPosition = waypointPosition;
        }

        // 10/3/2009 - Add 2nd param 'attackerPlayerNumber'.
        // 12/25/2008; 1/1/2009 - Add removal of SceneItemOwner from Selectable List & ItemSelected List
        /// <summary>
        /// Set the <see cref="SceneItem"/> 'isAlive' flag to false; most likely will be overriden in an inherited class with
        /// it's own version of <see cref="SceneItem.StartKillSceneItem"/> method; for example, Animation Death Clips
        /// might be played for the specific <see cref="SceneItem"/>.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="attackerPlayerNumber">Attacker's <see cref="Player"/> number</param>
        public override void StartKillSceneItem(ref TimeSpan elapsedTime, int attackerPlayerNumber)
        {
            // Added 'KillSceneItemCalled' check to make sure code is not executed twice, since during MP games,
            // the Server will make sure client kills the unit by calling this too!
            if (KillSceneItemCalled) return;

            // 5/20/2010: Moved to top, rather than bottom of method call to have 'IsAlive' applied first!
            base.StartKillSceneItem(ref elapsedTime, attackerPlayerNumber);

            // 1/7/2008 - AUDIO: Play some 'Explosion' sound.
            Audio_KillSceneItem();

            // 7/7/2009 - Remove this instance from 'ItemsSelected' array.
            Player.DeSelectSceneItem(this);

            // 2/23/2009 - Hide StatusBar
            DrawStatusBar = false;
            ThePickSelected = false;

            // 6/2/2009 - Set AIOrderIssued to 'None' state.
            _aIOrderIssued = AIOrderType.None;

            // 6/16/2009 - Set DefenseAIStance to 'Guard'.
            DefenseAIStance = DefenseAIStance.Guard;

            // 6/16/2009 - Make sure AttackSceneItem is null.                
            AttackSceneItem = null;
            AttackOn = false;

            // 6/16/2009 - Clear PathFinding.
            var aStarItemI = AStarItemI; // 4/27/2010 - Cache
            if (aStarItemI != null)
            {
                // 7/6/2009
                aStarItemI.GoalPosition = Vector3Zero;
                aStarItemI.MoveToPosition = Vector3Zero;

                //aStarItemI.SolutionFinal.Clear();
                AStarItem.ClearSolutionFinal(aStarItemI); // 6/9/2010 - Updated for LocklessQueue call.
                aStarItemI.PathToQueue.Clear();
                aStarItemI.PathToStack.Clear();
            }

            // 3/1/2010 -  Have Thread sleep a few ms.
            Thread.Sleep(0);

            // Trigger the Event;
            //             Links: Interface Class will remove the subQueue, if this a building.
            OnSceneItemDestroyed();
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            // 6/15/2010 - Check if null
            if (player != null)
            {
                // 11/11/09 - cache
                var networkSession = player.NetworkSession;

                // 1/1/2009 - (MP) Make sure SceneItemOwner is deleted on client or server side!
                if (networkSession != null)
                {
                    // Create RTS Command             
                    RTSCommKillSceneItem killSceneItem;
                    PoolManager.GetNode(out killSceneItem);

                    killSceneItem.Clear();
                    killSceneItem.NetworkCommand = NetworkCommands.KillSceneItem;
                    killSceneItem.PlayerNumber = PlayerNumber;
                    killSceneItem.NetworkItemNumber = NetworkItemNumber;
                    killSceneItem.AttackerPlayerNumber = attackerPlayerNumber; // 10/3/2009


                    // Are we host?
                    if (networkSession.IsHost)
                    {
                        // Send to Client
                        NetworkGameComponent.AddCommandsForClientG(killSceneItem);
                    }
                    else // Is Client
                    {
                        // Send to Server
                        NetworkGameComponent.AddCommandsForServerG(killSceneItem);
                    }
                } // End Is MP game
            }

            // 10/13/2012 - Obsolete
            // 1/30/2010 - Start Explosion.
            //ShapeItem.StartExplosion(this, ref elapsedTime);
            
        }

        // 1/1/2009; // 2/23/2009 - Updated to call Base first, and then call 'ReturnItemToPool'.
        /// <summary>
        /// When an item is fully dead, the <see cref="DoFinishKillSceneItemCheck"/> method will call this
        /// method. In turn, this is where the item is removed from the game world,
        /// and its <see cref="SceneItem.HealthState"/> is set to be 'Dead'.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="playerNumber">The <see cref="Player"/> number, used in MP games.</param>
        protected internal override void FinishKillSceneItem(ref TimeSpan elapsedTime, int playerNumber)
        {
           base.FinishKillSceneItem(ref elapsedTime, playerNumber);

           // 10/13/2012 - Reset KillSceneItem elapsed time variable.
           _killSceneItemElapsedTime = 0;
            
           // 4/20/2010 - Update Minimap to remove dead SceneItemOwner.
           ForceMinimapUpdate();
            
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            // 10/20/2009 - make sure not NULL
            if (player == null) return;

            // Remove from Selectable List                    
            Player.RemoveSelectableItem(player, this);

            // 2/23/2009 - Returns this SceneItemOwner to be usable again in PoolManager.
            ReturnItemToPool(false);
           
        }

        /// <summary>
        /// Helper method, which simply retrieves the <see cref="IMinimap"/> interface, and
        /// sets the <see cref="IMinimap.DoUpdateMiniMap"/> to true.
        /// </summary>
        private static void ForceMinimapUpdate()
        {
            try
            {
                var miniMap = (IMinimap)GameInstance.Services.GetService(typeof(IMinimap)); // 1/2/2010
                if (miniMap != null) miniMap.DoUpdateMiniMap = true;
            }
            // 4/27/2010 - Captures the NullRefExp, and checks if the GameInstance is null; if so, retrieves
            // updates instance directly from TWEngine class.  This avoids having to check if null every game cycle.
            catch (NullReferenceException)
            {
#if DEBUG
                Debug.WriteLine(
                    "ForceMinimapUpdate method, in SceneItemWithPick class, threw the NullReferenceExp error.",
                    "NullReferenceException");
#endif

                if (GameInstance == null)
                {
                    GameInstance = TemporalWars3DEngine.GameInstance;
#if DEBUG
                    Debug.WriteLine("The 'GameInstance' static variable was null; however, fixed by retrieving reference from TWEngine class.");
#endif
                }
            }
           
        }

        // 6/3/2009
        /// <summary>
        /// Sends the Cease-Attack order, to the proper network player during a MP game.
        /// </summary>
        internal void SendCeaseAttackOrderToMPPlayer()
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            if (player == null) return; // 10/20/2009 - make sure not NULL

            var networkSession = player.NetworkSession; // 11/11/09
            if (networkSession == null) return;

            // Check if Host or Client            
            if (networkSession.IsHost)
            {
                // Send the Cease Attack order.
                RTSCommCeaseAttackSceneItem ceaseAttack;
                PoolManager.GetNode(out ceaseAttack);

                ceaseAttack.Clear();
                ceaseAttack.NetworkCommand = NetworkCommands.CeaseAttackSceneItem;
                ceaseAttack.SceneItemAttackerPlayerNumber = PlayerNumber;
                ceaseAttack.SceneItemAttackerNetworkNumber = NetworkItemNumber;

                // Send to client
                NetworkGameComponent.AddCommandsForClientG(ceaseAttack);
            }
            else
            {
                // Send the Cease Attack order.
                RTSCommCeaseAttackSceneItem ceaseAttack;
                PoolManager.GetNode(out ceaseAttack);

                ceaseAttack.Clear();
                ceaseAttack.NetworkCommand = NetworkCommands.CeaseAttackSceneItem;
                ceaseAttack.SceneItemAttackerPlayerNumber = PlayerNumber;
                ceaseAttack.SceneItemAttackerNetworkNumber = NetworkItemNumber;

                // Send to host
                NetworkGameComponent.AddCommandsForServerG(ceaseAttack);
            }
        }

        // 6/3/2009
        /// <summary>
        /// Sends the StartAttack order, to the proper network player during a MP game.
        /// </summary>
        /// <param name="attackie"><see cref="SceneItemWithPick"/> instance</param>
        internal void SendStartAttackOrderToMPPlayer(SceneItemWithPick attackie)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            if (player == null) return; // 10/20/2009 - Make sure not NULL

            var networkSession = player.NetworkSession; // 11/11/09
            if (networkSession == null) return;

            RTSCommStartAttackSceneItem startAttackCommand;
            PoolManager.GetNode(out startAttackCommand);

            startAttackCommand.Clear();
            startAttackCommand.NetworkCommand = NetworkCommands.StartAttackSceneItem;
            startAttackCommand.SceneItemAttackerNetworkNumber = NetworkItemNumber;
            startAttackCommand.SceneItemAttackerPlayerNumber = PlayerNumber;
            startAttackCommand.SceneItemAttackieNetworkNumber = attackie.NetworkItemNumber;
            startAttackCommand.SceneItemAttackiePlayerNumber = attackie.PlayerNumber;
            startAttackCommand.AIOrderIssued = AIOrderType.AIAttackOrderRequest; // 6/3/2009

            // Check if Host or Client            
            if (networkSession.IsHost)
            {
                // Add to Queue to send to Client
                NetworkGameComponent.AddCommandsForClientG(startAttackCommand);
            }
            else
            {
                // Add to Queue to send to Client
                NetworkGameComponent.AddCommandsForServerG(startAttackCommand);
            }
        }

        // 8/3/2009
        /// <summary>
        /// Sends the start Self-Repair order, to the proper network player during a MP game.
        /// </summary>
        internal void SendStartSelfRepairToMPPlayer()
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            if (player == null) return; // 10/20/2009 - Make sure not NULL

            var networkSession = player.NetworkSession; // 11/11/09
            if (networkSession == null) return;

            RTSCommSceneItemHealth startRepairCommand;
            PoolManager.GetNode(out startRepairCommand);

            startRepairCommand.Clear();
            startRepairCommand.NetworkCommand = NetworkCommands.SceneItemHealth;
            startRepairCommand.StartSelfRepair = true;
            startRepairCommand.Health = CurrentHealth;
            startRepairCommand.PlayerNumber = PlayerNumber;
            startRepairCommand.NetworkItemNumber = NetworkItemNumber;

            // Check if Host or Client            
            if (networkSession.IsHost)
            {
                // Add to Queue to send to Client
                NetworkGameComponent.AddCommandsForClientG(startRepairCommand);
            }
            else
            {
                // Add to Queue to send to Client
                NetworkGameComponent.AddCommandsForServerG(startRepairCommand);
            }
        }

        // 4/10/2009
        /// <summary>
        /// Inheriting classes should override this method and populate with any misc
        /// animation routines.  This virtual method is called from the Update method.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected virtual void DoAnimationCheck(GameTime gameTime)
        {
            // Leaf classes should override this method and populate with any
            // misc animation routines.
            return;
        }

        // 5/5/2009
        /// <summary>
        /// Inheriting classes should override this method and populate with any logic
        /// related to when the <see cref="SceneItemWithPick"/> is moving; for example, sounds or particle effects!
        /// This virtual method is called from the Update method.
        /// </summary>
        /// <returns>True/False if item is moving.</returns>
        protected virtual bool ItemIsMovingCheck()
        {
            // Leaf classes should override this method and populate with any
            // logic related to when the SceneItemOwner is moving.

            var aStarItemI = AStarItemI; // 4/27/2010
            return aStarItemI != null && (aStarItemI.ItemState == ItemStates.Moving || aStarItemI.ItemState == ItemStates.PathFindingMoving);
        }

        // 7/16/2009
        /// <summary>
        /// Calculates the DesiredAngle the <see cref="SceneItemWithPick"/> should be facing, using its
        /// own position vs the given <see cref="SceneItemWithPick"/> position vector.
        /// </summary>
        /// <param name="itemToFace"><see cref="SceneItem"/> instance to face</param>
        /// <returns>Current desired angle</returns>
        internal float CalculateDesiredAngle(SceneItem itemToFace)
        {
            // 11/11/2009 - Check if Null
            if (itemToFace == null) return 0;

            float desiredAngle;
            CalculateDesiredAngle(itemToFace, out desiredAngle);

            return desiredAngle;
        }

        // 7/16/2009
        /// <summary>
        /// Calculates the DesiredAngle the <see cref="SceneItemWithPick"/> should be facing, using its
        /// own position vs the given <see cref="SceneItemWithPick"/> position vector.
        /// </summary>
        /// <param name="itemToFace"><see cref="SceneItem"/> instance to face</param>
        /// <param name="desiredAngle">(OUT) Current desired angle</param>
        internal void CalculateDesiredAngle(SceneItem itemToFace, out float desiredAngle)
        {
            desiredAngle = default(float);

            // 11/11/2009 - Check if Null
            if (itemToFace == null) return;

            // Get this items Position
            var tmpPos = new Vector2 { X = position.X, Y = position.Z };

            // Get itemToFace Position
            var itemToFacePosition = itemToFace.Position;
            var tmpItemToFacePos = new Vector2 { X = itemToFacePosition.X, Y = itemToFacePosition.Z };

            // calc turn Position based on itemToFace's location.               
            Vector2 direction;
            Vector2.Subtract(ref tmpPos, ref tmpItemToFacePos, out direction);
            desiredAngle = (float) Math.Atan2(-direction.Y, direction.X);
        }

        // 7/16/2009
        /// <summary>
        /// Calculates the distance between two <see cref="SceneItem"/>, using its own 'Position'
        /// vs the other <see cref="SceneItem"/> 'Position' vector.
        /// </summary>
        /// <remarks>The calculation is done using Vector2 distance formula.</remarks>
        /// <param name="otherSceneItem">Other <see cref="SceneItem"/> to measure to</param>
        /// <param name="distance">(OUT) Distance between <see cref="SceneItem"/></param>
        internal void CalculateDistanceToSceneItem(SceneItem otherSceneItem, out float distance)
        {
            distance = default(float);

            // 11/11/2009 - Check if Null
            if (otherSceneItem == null) return;

            // Get this items Position
            var tmpPos = new Vector2 { X = position.X, Y = position.Z };

            // Get otherSceneItem Position
            var otherSceneItemPosition = otherSceneItem.Position;
            var tmpOtherPos = new Vector2 { X = otherSceneItemPosition.X, Y = otherSceneItemPosition.Z };

            Vector2.Distance(ref tmpPos, ref tmpOtherPos, out distance);
        }

        /// <summary>
        /// Calculates the Distance between this <see cref="SceneItem"/> and some <paramref name="otherPosition"/> value.
        /// </summary>
        /// <remarks>The calculation is done using Vector2 distance formula.</remarks>
        /// <param name="otherPosition"><see cref="Vector3"/> position to measure to</param>
        /// <param name="distance">(OUT) Distance between items</param>
        internal void CalculateDistanceToPosition(ref Vector3 otherPosition, out float distance)
        {
            // Get this items Position
            var tmpPos = new Vector2 { X = position.X, Y = position.Z };

            // Get otherSceneItem Position
            var tmpOtherPos = new Vector2 { X = otherPosition.X, Y = otherPosition.Z };

            Vector2.Distance(ref tmpPos, ref tmpOtherPos, out distance);
        }

        // 2/28/2011
        /// <summary>
        /// Helper method, which checks if the current <see cref="SceneItem"/> position is within
        /// the 'MoveToPosition', by the current 'WaypointSeekDistSq' value.
        /// </summary>
        /// <returns>True/False</returns>
        private bool HasReachedMoveToPosition()
        {
            // Sub-goal reached? 
            var tmpPosA = new Vector2 { X = Position.X, Y = Position.Z };
            var tmpPosB = new Vector2 { X = MoveToPosition.X, Y = MoveToPosition.Z };

            float result;
            Vector2.DistanceSquared(ref tmpPosB, ref tmpPosA, out result);
            return (result < SeekDistSq);
        }

        #region Particles

        // 4/10/2009
        /// <summary>
        /// Inheriting classes should override this method and populate any misc
        /// particle routines.  This virtual method is then called from the Update method.
        /// </summary>
        protected virtual void DoParticlesCheck()
        {
            // Leaf classes should override this method and populate with any
            // misc particle routines.
            return;
        }

        // 4/11/2009
        /// <summary>
        /// When property 'PickSelected' is SET, this virtual method is called.  Inheriting classes
        /// should override this method to capture this event.  
        /// </summary>
        /// <param name="thePickSelected">The value of the <see cref="PickSelected"/>.</param>
        protected virtual void OnPickSelected(bool thePickSelected)
        {
            // Leaf classes should override this method and populate with any
            // misc routines.

            // 5/5/2009 - Play Pick Selected Sound.
            Audio_PickedSelected(thePickSelected);
        }

        // SP Game version.
        /// <summary>
        /// Helper for updating the <see cref="Projectile"/> effect, by adding new ones
        /// every few seconds.
        /// </summary>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        protected void UpdateProjectilesCreation(ref TimeSpan elapsedTime)
        {
            // 6/15/2010 - Updated to use new GetPlayer method.
            Player player;
            TemporalWars3DEngine.GetPlayer(PlayerNumber, out player);

            if (player == null) return;

            // 2/4/2009 - Perform Projectile creation for every SpawnBullet Position which exists.
            if (HasSpawnBulletPosition(1))
            {
                // 4/27/2010 - Refactored out code to new re-usable method;
                ProcessProjectileCreationForSpawnBullet(player, ref elapsedTime, 1, ref _timeToNextProjectile1);

            } // End If SpawnBullet-1

            if (HasSpawnBulletPosition(2))
            {
                // 4/27/2010 - Refactored out code to new re-usable method;
                ProcessProjectileCreationForSpawnBullet(player, ref elapsedTime, 2, ref _timeToNextProjectile2);
            } // End If SpawnBullet-2

            if (HasSpawnBulletPosition(3))
            {
                // 4/27/2010 - Refactored out code to new re-usable method;
                ProcessProjectileCreationForSpawnBullet(player, ref elapsedTime, 3, ref _timeToNextProjectile3);
            } // End If SpawnBullet-3

            if (HasSpawnBulletPosition(4))
            {
                // 4/27/2010 - Refactored out code to new re-usable method;
                ProcessProjectileCreationForSpawnBullet(player, ref elapsedTime, 4, ref _timeToNextProjectile4);
            } // End If SpawnBullet-4
        }

        // 4/27/2010
        /// <summary>
        /// Helper method, used to create new <see cref="Projectile"/> for a given <paramref name="spawnBulletNumber"/>.
        /// </summary>
        /// <param name="player"><see cref="Player"/> instance</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        /// <param name="spawnBulletNumber">The SpawnBullet number 1-4</param>
        /// <param name="timeToNextProjectile">The <see cref="TimeSpan"/> to next projectile creation</param>
        private void ProcessProjectileCreationForSpawnBullet(Player player, ref TimeSpan elapsedTime, int spawnBulletNumber, ref TimeSpan timeToNextProjectile)
        {
            timeToNextProjectile -= elapsedTime;
            if (timeToNextProjectile > TimeSpanZero) return;

            // 1/7/2009 - AUDIO: Play some 'Shooting' bullet sound.
            Audio_ShootProjectile(spawnBulletNumber, true);

            Vector3 bulletSpawnPosition;
            GetBulletStartPosition(spawnBulletNumber, out bulletSpawnPosition);

            // 4/10/2009 - Call empty method, which can be overriden by inherting classes.
            ProjectileReleased(spawnBulletNumber, ref bulletSpawnPosition);

            // Create a new projectile once per N seconds. The real work of moving
            // and creating particles is handled inside the Projectile class. 

            // 6/29/2009 - Get new Projectile from MemoryPool
            ProjectilePoolItem poolNode;
            player.PoolManager.GetNode(out poolNode);
            var projectileToUse = poolNode.ProjectileItem;

            // 4/27/2010 - Index number into array is zero based.
            var index0 = spawnBulletNumber - 1;
            projectileToUse.ProjectileInitilization(ref bulletSpawnPosition, AttackSceneItem, PlayableItemAtts, index0);


            _projectiles.Add(projectileToUse);

            timeToNextProjectile += TimeSpan.FromSeconds(PlayableItemAtts.RateOfFire[index0]);
        }

        // 4/10/2009
        /// <summary>
        /// This method is called by the <see cref="UpdateProjectilesCreation"/>  method when a new SpawnBullet is created and released.
        /// Inherting classes can override this to perform specific misc routines for this action; for example, 'Flash' particle
        /// effects.
        /// </summary>
        /// <param name="spawnBulletNumber">SpawnBullet number</param>
        /// <param name="bulletStartPosition"><see cref="Vector3"/> starting position</param>
        protected virtual void ProjectileReleased(int spawnBulletNumber, ref Vector3 bulletStartPosition)
        {
            // inherting classes can override to populate with some special routine
            // when this is activated!
            return;
        }

        // 10/23/2008
        /// <summary>
        /// Gets the SpawnBullet position for a given <paramref name="spawnBulletNumber"/>.
        /// </summary>
        /// <param name="spawnBulletNumber">SpawnBullet number to retrieve</param>
        /// <param name="bulletSpawnPosition">(OUT) World position for SpawnBullet</param>
        protected virtual void GetBulletStartPosition(int spawnBulletNumber, out Vector3 bulletSpawnPosition)
        {
            // Just sets to items Position 
            bulletSpawnPosition = Position;
        }

        // 2/4/2009
        /// <summary>
        /// Checks its own <see cref="PlayableItemTypeAttributes"/> structure to see if the given bulletPosition
        /// exists for the given <see cref="SceneItemWithPick"/>.
        /// </summary>
        /// <param name="spawnBulletNumber">SpawnBullet number to check</param>
        /// <returns>True/False</returns>
        protected bool HasSpawnBulletPosition(int spawnBulletNumber)
        {
            var index0 = spawnBulletNumber - 1;
            if (index0 < 0 || index0 > 4)
                throw new ArgumentOutOfRangeException("spawnBulletNumber", @"Value given is outside allowable range of 1-4.");

            return PlayableItemAtts.HasSpawnBullet[index0];
            
        }

        /// <summary>
        /// Helper for updating the list of active _projectiles, and for applying
        /// the damage to the attackie.
        /// </summary>
        private void UpdateProjectilesLife(GameTime gameTime)
        {
            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            var attackie = AttackSceneItem;

            var i = 0;

            // 8/15/2009 - Cache
            var projectiles = _projectiles;
           
            while (i < projectiles.Count)
            {
                // 8/14/2009 - Cache
                var projectile = projectiles[i];
                if (projectile == null) continue;

                // If Projectile Update returns False, then this means projectile
                // reached target!
                if (!projectile.Update(gameTime))
                {
                    // 1/7/2009 - AUDIO: Play 'Hit' Que                    
                    Audio_ProjectileHitTarget();

                    // 12/10/2008: Updated to also check 'Delete' flag, to stop updating when deleting SceneItemOwner.
                    // Apply Damage to SceneItem being attacked.                    
                    if (attackie != null && !attackie.Delete)
                    {
                        // 11/14/2008 - Store Projectile's velocity
                        attackie.ShapeItem.LastProjectileVelocity = projectile.Velocity;

                        // 8/14/2009 - Cache
                        var attackSceneItem = projectile.AttackSceneItem;

                        // 3/11/2009 - Apply proper AttackDamageBias, depending on what the SceneItemOwner being attack is?
                        var attackDamage = 1.0f;
                        if (attackSceneItem is SciFiTankScene)
                        {
                            attackDamage = projectile.AttackDamage*PlayableItemAtts.AttackDamageBiasVehicles;
                        }
                        else if (attackSceneItem is BuildingScene || attackSceneItem is DefenseScene)
                        {
                            attackDamage = projectile.AttackDamage*PlayableItemAtts.AttackDamageBiasBuildings;
                        }
                        else if (attackSceneItem is SciFiAircraftScene)
                        {
                            attackDamage = projectile.AttackDamage*PlayableItemAtts.AttackDamageBiasAircraft;
                        }
                                              
                        
                        // 12/30/2008 - Add the 'AttackSceneItem' parameter.
                        ApplyDamageToAttackie(gameTime, attackSceneItem, attackDamage);
                        
                    }

                    // Remove _projectiles at the end of their life.
                    projectile.ReturnItemToPool(); // 5/12/2009
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        // 12/30/2008; 10/10/2009: Updated with Gametime param, and adding AttackBy info.
        /// <summary>
        /// Reduces the <see cref="SceneItem"/> being attacked by given point value.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="attackie"><see cref="SceneItem"/> attackie instance</param>
        /// <param name="reduceBy">Damage value to apply</param>
        private void ApplyDamageToAttackie(GameTime gameTime, SceneItem attackie, float reduceBy)
        {
            // 11/11/09 - Check for Null.
            if (attackie == null) return;

            // 4/27/2010 - Cache casting
            var sceneItemWithPick = (attackie as SceneItemWithPick);
            if (sceneItemWithPick == null) return;

            // 4/27/2010 - Cache
            var shapeItem = ShapeItem;
            if (shapeItem == null) return;

            // 4/27/2010 - Cache
            var attackedByItemType = shapeItem.ItemType;

            // 10/10/2009 - Store Who the Attacker is for the 'attackie' sceneItem.
            sceneItemWithPick.AttackBy = new AttackByLastItemType
                                                           {
                                                               AttackedByItemType = attackedByItemType,
                                                               TimeOfAttack = gameTime
                                                           };


            // Apply Damage to Attackie, and check if dead yet.
            if (attackie.ReduceHealth(reduceBy, PlayerNumber) > 0) return;

            // 10/10/2009 - Item is Destroyed, so save who Destroyed the attackie. (Scripting purposes)
            sceneItemWithPick.DestroyedBy = attackedByItemType;

            AttackOn = false;
            AttackSceneItem = null;
        }

        #endregion

        #region Audio Methods

        // 5/12/2009
        /// <summary>
        /// Tracks when projectile sound has been started and stopped.
        /// </summary>
        protected bool StartShootProjectileSound;

        // 1/7/2009
        /// <summary>
        /// Plays some explosion sound.
        /// </summary>
        /// <remarks>Can be overriden to play other sounds.</remarks>
        protected virtual void Audio_KillSceneItem()
        {
            // 5/12/2009: Updated to use the SoundBanks PlayCue3D, rather than 'Play3D'; this is because when calling the 'Play3D' version,
            //            the Items shooting will still have their sounds loop forever, even though they have stopped attacking!!
            AudioManager.PlayCue(Sounds.Exp_Medium1);
        }

        // 1/7/2009; 3/27/2009: Updated to use the Cue method.
        /// <summary>
        /// Plays some hitting sound when <see cref="Projectile"/> hits target.
        /// </summary>
        /// <remarks>Can be overriden to play other sounds.</remarks>
        protected virtual void Audio_ProjectileHitTarget()
        {
            // 8/16/2009 - Direct to Dictionary version.
            AudioManager.Play3D(UniqueKey, Sounds.Exp_SmoothGroup, AudioListenerI, AudioEmitterI, false);
        }

        // 1/7/2009
        /// <summary>
        /// Plays some shooting bullet sound when <see cref="SceneItem"/> shoot <see cref="Projectile"/>.
        /// </summary>
        /// <remarks>Can be overriden to play other sounds.</remarks>
        /// <param name="spawnBullet">The SpawnBullet number.</param>
        /// <param name="playSound">Play sound?</param>
        protected virtual void Audio_ShootProjectile(int spawnBullet, bool playSound)
        {
            // 5/12/2009
            if (playSound)
            {
                if (!StartShootProjectileSound)
                {
                    AudioManager.Play3D(UniqueKey, Sounds.Cannon1, AudioListenerI, AudioEmitterI, false);
                    StartShootProjectileSound = true;
                }

            }
            else
            {
                AudioManager.Pause(UniqueKey, Sounds.Cannon1);
                StartShootProjectileSound = false;
            }
        }


        // 5/5/2009
        /// <summary>
        /// Plays some pick selected sound when <see cref="SceneItem"/> is selected.
        /// </summary>
        /// <param name="thePickSelected">Is this pick selected?</param>
        protected virtual void Audio_PickedSelected(bool thePickSelected)
        {
            // TODO: Fill with some PickSelected sound.
            return;
        }

        #endregion

        #region DefenseAI

        // 8/11/2009 - To Optimize.
        private static readonly Vector3 Vector3Zero = Vector3.Zero;
        private static readonly TimeSpan TimeSpanZero = TimeSpan.Zero;
        private float _killSceneItemElapsedTime;

        // 3/23/2009
        /// <summary>
        /// Inheriting classes should override this method and populate with proper
        /// AI logic.  This virtual method is called from the AIDefense Thread Manager.
        /// </summary>
        /// <param name="gameTime"></param>
        internal virtual void UpdateDefenseBehavior(GameTime gameTime)
        {
            // 6/17/2010
            if (_FSMAIControl == null)
                _FSMAIControl = new FSM_AIControl(this);

            // 7/31/2009 - Update FSMAIControl.
            _FSMAIControl.Update(gameTime);

            #region oldCode

            // reduce Time
            //TimeSpan ElapsedTime = TimeSpan.FromMilliseconds(timeElapsed);
            /*timeElapsed = 0;

            // 7/16/2009 - Copy 'AttackSceneItem' into local cache, to eliminate Thread Sync errors!
            SceneItem attackie = AttackSceneItem ?? null;

            // 7/6/2009 - Added 'AttakOn' check.
            // 6/2/2009 - If NonAI order given, then skip AttackSomeNeighborItem check.
            if (AIOrderIssued == AIOrderType.NonAI_AttackOrder && AttackOn)
                return;

            // 6/3/2009 - If attackie out of range, and 'Aggressive' stance, then
            //            re-path to attackie; otherwise, just stop and forget about attackie.
            if (AttackOn && attackie != null && !IsAttackieInAttackRadius(attackie))
            {
                switch (DefenseAIStance)
                {
                    case DefenseAIStance.Aggressive:
                        if (aStarItem.ItemState == itemStates.Resting)
                        {
                            // 6/9/2009 - Set to 'PathFindingAI', which prevents multiply requests!
                            aStarItem.ItemState = itemStates.PathFindingAI;

                            PathToPositionWithinAttackingRange(attackie);
                        }
                        break;
                    case DefenseAIStance.Guard:
                    case DefenseAIStance.HoldGround:
                    case DefenseAIStance.HoldFire:

                        // 6/3/2009 - Remove EventHandler Reference
                        if (attackie != null)
                            //attackie.sceneItemDestroyed -= AttackSceneItemSceneItemDestroyed;
                            attackie.RemoveEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, this); // 6/15/2009

                        // Then stop trying to attack.
                        AttackOn = false;
                        AttackSceneItem = null;
                        attackie = null; // 7/16/2009
                        AIOrderIssued = AIOrderType.None;
                        break;
                }
            }           


            // 3/23/2009: Updated to check using bitwise comparison, since enum is a bitwise enum!            
            if (ForceBehaviors != null)
            {
                switch (DefenseAIStance)
                {
                    case DefenseAIStance.Aggressive:
                        // Aggressive attack will also consider buildings/shields too.
                        if (((int)itemGroupTypeToAttack & (int)(ItemGroupType.Buildings | ItemGroupType.Shields | ItemGroupType.Vehicles)) != 0)
                            AttackSomeNeighborItemSp(ForceBehaviors.NeighborsGround);

                        if (((int)itemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                            AttackSomeNeighborItemSp(ForceBehaviors.NeighborsAir);
                        break;
                    case DefenseAIStance.Guard:
                    case DefenseAIStance.HoldGround:
                        // Guard and HoldGround only attack vehicles/aircraft.
                        if (((int)itemGroupTypeToAttack & (int)(ItemGroupType.Vehicles)) != 0)
                            AttackSomeNeighborItemSp(ForceBehaviors.NeighborsGround);

                        if (((int)itemGroupTypeToAttack & (int)ItemGroupType.Airplanes) != 0)
                            AttackSomeNeighborItemSp(ForceBehaviors.NeighborsAir);
                        break;
                    case DefenseAIStance.HoldFire:
                        if (AttackOn || attackie != null)
                        {
                            // 6/3/2009 - Remove EventHandler Reference
                            if (attackie != null)
                                //attackie.sceneItemDestroyed -= AttackSceneItemSceneItemDestroyed;
                                attackie.RemoveEventHandler_SceneItemDestroyed(AttackSceneItemSceneItemDestroyed, this); // 6/15/2009

                            // Hold Fire; stop any attacking if already started.
                            AttackOn = false;
                            AttackSceneItem = null;
                            attackie = null; // 7/16/2009
                        }

                        break;
                }

            } // End If Null*/

            #endregion
        }

        #endregion

        #region Old Interpolation methods

        // 11/12/2008 - Updated to optimize memory.
        //private float _facingForward;
        //private float _localTime;
        //private TimeSpan _oneFrame;
        //private float _smoothingDecay;
        //private float _speed;
/*
        /// <summary>
        /// Applies prediction and smoothing to client items.
        /// </summary>
        private void UpdateRemote(int framesBetweenPackets, bool enablePrediction, ref TimeSpan elapsedTime)
        {
            // Update the smoothing amount, which interpolates from the previous
            // state toward the current simultation state. The _speed of this decay
            // depends on the number of frames between packets: we want to finish
            // our smoothing interpolation at the same Time the next packet is due.
            _smoothingDecay = 1.0f/framesBetweenPackets;

            _currentSmoothing -= _smoothingDecay;

            if (_currentSmoothing < 0)
                _currentSmoothing = 0;

            if (enablePrediction)
            {
                // Predict how the remote tank will move by updating
                // our local copy of its simultation state.
                UpdateState(ref _simulationState, ref elapsedTime);

                // If both smoothing and prediction are active,
                // also apply prediction to the previous state.
                if (_currentSmoothing > 0)
                {
                    UpdateState(ref _previousState, ref elapsedTime);
                }
            }

            if (_currentSmoothing > 0)
            {
                // Interpolate the display state gradually from the
                // previous state to the current simultation state.
                ApplySmoothing();
            }
            else
            {
                // Copy the simulation state directly into the display state.
                //displayState = _simulationState;
                Position = _simulationState.Position;
                velocity = _simulationState.velocity;
                AStarItemI.SmoothHeading = _simulationState.smoothHeading; // 12/16/2008                
                FacingDirection = _simulationState.facingDirection; // 12/22/2008
            }
        }
*/

        // 12/4/2008 - Updated to optimize memory
/*
        /// <summary>
        /// Applies smoothing by interpolating the display state somewhere
        /// in between the previous state and current simulation state.
        /// </summary>
        private void ApplySmoothing()
        {
            Vector3.Lerp(ref _simulationState.Position, ref _previousState.Position, _currentSmoothing, out Position);


            Vector3.Lerp(ref _simulationState.velocity, ref _previousState.velocity, _currentSmoothing, out velocity);

            // 12/16/2008
            Vector3.Lerp(ref _simulationState.smoothHeading, ref _previousState.smoothHeading, _currentSmoothing,
                         out AStarItemI.SmoothHeading);

            // 12/22/2008
            FacingDirection = MathHelper.Lerp(_simulationState.facingDirection, _previousState.facingDirection,
                                              _currentSmoothing);
        }
*/

/*
        /// <summary>
        /// Incoming network packets tell us where the tank was at the Time the packet
        /// was sent. But packets do not arrive instantly! We want to know where the
        /// tank is now, not just where it used to be. This method attempts to guess
        /// the current state by figuring out how long the packet took to arrive, then
        /// running the appropriate number of local updates to catch up to that Time.
        /// This allows us to figure out things like "it used to be over there, and it
        /// was moving that way while turning to the left, so assuming it carried on
        /// using those same inputs, it should now be over here".
        /// </summary>
        private void ApplyPrediction(GameTime gameTime, ref TimeSpan Latency, float packetSendTime,
                                     ref TimeSpan elapsedTime)
        {
            // Work out the difference between our current local Time
            // and the remote Time at which this packet was sent.
            _localTime = (float) gameTime.TotalGameTime.TotalSeconds;

            float timeDelta = _localTime - packetSendTime;

            // Maintain a rolling Average of Time deltas from the last 100 packets.
            ClockDelta.AddValue(timeDelta);

            // The caller passed in an estimate of the Average network Latency, which
            // is provided by the XNA Framework networking layer. But not all packets
            // will take exactly that Average amount of Time to arrive! To handle
            // varying latencies per packet, we include the send Time as part of our
            // packet data. By comparing this with a rolling Average of the last 100
            // send times, we can detect packets that are later or earlier than usual,
            // even without having synchronized clocks between the two machines. We
            // then adjust our Average Latency estimate by this per-packet deviation.

            float timeDeviation = timeDelta - ClockDelta.AverageValue;

            Latency += TimeSpan.FromSeconds(timeDeviation);

            _oneFrame = TimeSpan.FromSeconds(1.0/60.0);

            // Apply prediction by updating our simulation state however
            // many times is necessary to catch up to the current Time.
            while (Latency >= _oneFrame)
            {
                UpdateState(ref _simulationState, ref elapsedTime);

                Latency -= _oneFrame;
            }
        }
*/

//        /// <summary>
//        /// Updates one of our state structures, using the current inputs to turn
//        /// the tank, and applying the velocity and inertia calculations. This
//        /// method is used directly to update locally controlled tanks, and also
//        /// indirectly to predict the motion of remote tanks.
//        /// </summary>
//        private void UpdateState(ref ItemMoveState state, ref TimeSpan elapsedTime)
//        {
//            // 1/19/2009 - Calc TankInput
//            Vector3 tmpMoveToPos = MoveToPosition;
//            Vector3 tmpDirection;
//            Vector3.Subtract(ref tmpMoveToPos, ref state.Position, out tmpDirection);
//            if (tmpDirection.X > 0)
//                _tankInput.X = 1;
//            if (tmpDirection.X < 0)
//                _tankInput.X = -1;
//            if (tmpDirection.Y > 0)
//                _tankInput.Y = 1;
//            if (tmpDirection.Y < 0)
//                _tankInput.Y = -1;
//
//            // Normalize the input vectors.
//            if (_tankInput.Length() > 1)
//                _tankInput.Normalize();
//
//            _tankInput.X = tmpDirection.X;
//            _tankInput.Y = tmpDirection.Z;
//            _tankInput.Normalize();
//
//            // 12/4/2008 - If TankInput from Server is Zero, then we know
//            //             the tank is no longer moving and should make sure
//            //             the 'SimulationState' velocity is Zero; this will insure
//            //             the interpolating updating will eventually set the SceneItemOwner's 
//            //             'Velocity' to zero too!
//            //if (_tankInput.Length() == 0)
//            if (ItemState == ItemStates.Resting)
//            {
//                // Set State to Resting.
//                //aStarItem.ItemState = itemStates.Resting;
//
//                _tankInput = Vector2.Zero; // 1/19/2009
//                _simulationState.velocity = Vector3.Zero;
//            }
//            else
//            {
//                // Tank is moving, so set ItemState on this Client side to moving.
//                AStarItemI.ItemState = ItemStates.PathFindingMoving;
//
//
//                // 1/14/2009 - Update FOW SightMatrices
//                FogOfWar.UpdateSight = true;
//                // 1/15/2009 - Tell Minimap to update the unit positions.
//                Minimap.DoUpdateMiniMap = true;
//
//                // Gradually turn the tank to face the requested direction.
//                state.facingDirection = TurnToFace(state.facingDirection, ref AStarItemI.SmoothHeading, ItemTurnSpeed,
//                                                   FacingDirectionOffset);
//            }
//
//
//            /*state.TurretRotation = TurnToFace(state.TurretRotation,
//                                              turretInput, TurretTurnRate);*/
//
//            // How close the desired direction is the tank facing?  
//            Vector2 tankForward = Vector2.Zero;
//            tankForward.X = (float) Math.Cos(state.facingDirection);
//            tankForward.Y = (float) Math.Sin(state.facingDirection);
//
//            // Calculate TargetForward using the 'Force' attribute.
//            Vector2 targetForward = Vector2.Zero;
//            targetForward.X = _tankInput.X;
//            targetForward.Y = -_tankInput.Y; // was - for y                       
//
//            Vector2.Dot(ref tankForward, ref targetForward, out _facingForward);
//
//            // If we have finished turning, also start moving forward.
//            Vector3 tmpVelocity = Vector3.Zero;
//            if (_facingForward > 0)
//            {
//                _speed = _facingForward*_facingForward*0.3f; // testing = 0.3f
//
//                // Put in Vector3 Format
//                Vector3 tmpTankForward = new Vector3 { X = tankForward.X, Y = 0, Z = tankForward.Y };
//
//                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!                
//                //state.velocity += tmpTankForward * _speed;
//                tmpVelocity = state.velocity;
//                Vector3.Multiply(ref tmpTankForward, _speed, out tmpTankForward);
//                Vector3.Add(ref tmpVelocity, ref tmpTankForward, out tmpVelocity);
//                state.velocity = tmpVelocity;
//            }
//
//            // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
//            // ***
//            // Update the Position and velocity.  
//            // **
//            //state.Position += state.velocity *(float)ElapsedTime.TotalSeconds;
//            tmpVelocity = state.velocity;
//            Vector3 tmpPositionA = state.Position;
//            if (tmpVelocity.LengthSquared() > 0.00000001) // 12/4/2008 - Does this help?
//            {
//                Vector3.Multiply(ref tmpVelocity, (float) elapsedTime.TotalSeconds, out tmpVelocity);
//                Vector3.Add(ref tmpPositionA, ref tmpVelocity, out tmpPositionA);
//                state.Position = tmpPositionA;
//
//                // 12/4/2008 - Updated to remove Overload ops, since this slows down XBOX!
//                // ***
//                // Apply Friction to slow down velocity
//                // ***
//                //state.velocity *= 0.9f; // was 'TankFriction', change to 0.9f for testing.
//                tmpVelocity = state.velocity;
//
//                Vector3.Multiply(ref tmpVelocity, 0.9f, out tmpVelocity);
//                state.velocity = tmpVelocity;
//            }
        //        
    //}
        #endregion

        #region TurnToFace Methods

        /// <summary>
        /// Calculates the angle that this <see cref="SceneItemWithPick"/> should face, given the following;
        /// position, target position, current angle, and max turning speed.
        /// </summary>
        /// <param name="desiredAngle">Enter desired angle</param>
        /// <param name="currentAngle">Enter current angle</param>
        /// <param name="turnSpeed">Enter current turn speed</param>
        /// <param name="facingDirectionOffset">Enter the facing direction offset value, if any</param>
        /// <param name="angleDifference">(OUT) the angle difference</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="currentAngle"/> is not within allowable range of -pi to pi.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="facingDirectionOffset"/> is not within allowable range of -pi to pi.</exception>
        /// <returns>New facing direction</returns>
        protected static float TurnToFace(float desiredAngle, float currentAngle, float turnSpeed, float facingDirectionOffset, out float angleDifference)
        {
            // 8/23/2009 - Make sure Angle's given are all in Radians measurement.
            const float pi = MathHelper.Pi;

            if (currentAngle < -pi || currentAngle > pi)
                throw new ArgumentOutOfRangeException("currentAngle" , @"Angle must be in Radian measurement; - pi to pi.");

            if (facingDirectionOffset < -pi || facingDirectionOffset > pi)
                throw new ArgumentOutOfRangeException("facingDirectionOffset", @"Angle must be in Radian measurement; - pi to pi.");


               
            // Ben - Added a Constant Shift in Degrees to the DesiredAngle formula
            //       to compensate for where the front of the SceneItemOwner Shape Image is.
            desiredAngle += facingDirectionOffset;

            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return DesiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            angleDifference = desiredAngle - currentAngle;
            var difference = MathHelper.WrapAngle(angleDifference);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            var currentAngleDiff = currentAngle + difference;
            var newDirection = MathHelper.WrapAngle(currentAngleDiff);

            return newDirection;
        }

        // 12/15/2008: Updated to use XNA 3.0 MathHelper new method 'WrapAngle'.
        /// <summary>
        /// Calculates the angle that this <see cref="SceneItemWithPick"/> should face, given the following;
        /// position, target position, current angle, and max turning speed.
        /// </summary>
        /// <param name="position"><see cref="Vector2"/> position</param>
        /// <param name="faceThis"><see cref="Vector3"/> as face-this value</param>
        /// <param name="currentAngle">Enter current angle</param>
        /// <param name="turnSpeed">Enter current turn speed</param>
        /// <param name="facingDirectionOffset">Enter the facing direction offset value, if any</param>
        /// <param name="desiredAngle">(OUT) desired angle</param>
        /// <param name="angleDifference">(OUT) the angle difference</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="currentAngle"/> is not within allowable range of -pi to pi.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="facingDirectionOffset"/> is not within allowable range of -pi to pi.</exception>
        /// <returns>New facing direction</returns>    
        public static float TurnToFace(ref Vector2 position, ref Vector2 faceThis, float currentAngle, float turnSpeed,
                                float facingDirectionOffset, out float desiredAngle, out float angleDifference)
        {

            // 8/23/2009 - Make sure Angle's given are all in Radians measurement.
            const float pi = MathHelper.Pi;

            if (currentAngle < -pi || currentAngle > pi)
                throw new ArgumentOutOfRangeException("currentAngle", @"Angle must be in Radian measurement; - pi to pi.");

            if (facingDirectionOffset < -pi || facingDirectionOffset > pi)
                throw new ArgumentOutOfRangeException("facingDirectionOffset", @"Angle must be in Radian measurement; - pi to pi.");


            // consider this diagram:
            //         B 
            //        /|
            //      /  |
            //    /    | y
            //  / o    |
            // A--------
            //     x
            // 
            // where A is the Position of the object, B is the Position of the target,
            // and "o" is the angle that the object should be facing in order to 
            // point at the target. we need to know what o is. using trig, we know that
            //      tan(theta)       = opposite / adjacent
            //      tan(o)           = y / x
            // if we take the arctan of both sides of this equation...
            //      arctan( tan(o) ) = arctan( y / x )
            //      o                = arctan( y / x )
            // so, we can use x and y to find o, our "desiredAngle."
            // x and y are just the differences in Position between the two objects.
            var x = faceThis.X - position.X;
            var y = faceThis.Y - position.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            desiredAngle = (float)Math.Atan2(-y, x);

            // Ben - Added a Constant Shift in Degrees to the DesiredAngle formula
            //       to compensate for where the front of the SceneItemOwner Shape Image is.
            desiredAngle += facingDirectionOffset;


            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            angleDifference = desiredAngle - currentAngle;
            var difference = MathHelper.WrapAngle(angleDifference);

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            var currentAngleDiff = currentAngle + difference;
            var newDirection = MathHelper.WrapAngle(currentAngleDiff);

            return newDirection;
        }

        // 10/14/2008 - Overload Method
        // 12/15/2008: Updated to use XNA 3.0 MathHelper new method 'WrapAngle'.
        /// <summary>
        /// Calculates the angle that this <see cref="SceneItemWithPick"/> should face, given the following;
        /// position, target position, current angle, and max turning speed.
        /// </summary>
        /// <param name="currentAngle">Enter current angle</param>
        /// <param name="turnSpeed">Enter current turn speed</param>
        /// <param name="forwardDir"><see cref="Vector2"/> as forward direction</param>
        /// <param name="facingDirectionOffset">Enter the facing direction offset value, if any</param>
        /// <param name="desiredAngle">(OUT) desired angle</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="currentAngle"/> is not within allowable range of -pi to pi.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="facingDirectionOffset"/> is not within allowable range of -pi to pi.</exception>
        /// <returns>New facing direction</returns>
        public static float TurnToFace(float currentAngle, float turnSpeed, Vector2 forwardDir, float facingDirectionOffset, out float desiredAngle)
        {

            // 8/23/2009 - Make sure Angle's given are all in Radians measurement.
            const float pi = MathHelper.Pi;

            if (currentAngle < -pi || currentAngle > pi)
                throw new ArgumentOutOfRangeException("currentAngle", @"Angle must be in Radian measurement; - pi to pi.");

            if (facingDirectionOffset < -pi || facingDirectionOffset > pi)
                throw new ArgumentOutOfRangeException("facingDirectionOffset", @"Angle must be in Radian measurement; - pi to pi.");


            // Get SceneItemOwner's smoothVelocity, as Forward Vector Direction
            var x = forwardDir.X;
            var y = forwardDir.Y;

            // we'll use the Atan2 function. Atan will calculates the arc tangent of 
            // y / x for us, and has the added benefit that it will use the signs of x
            // and y to determine what cartesian quadrant to put the result in.
            // http://msdn2.microsoft.com/en-us/library/system.math.atan2.aspx
            desiredAngle = (float)Math.Atan2(-y, x); // Return as Radians.

            // Ben - Added a Constant Shift in Degrees to the DesiredAngle formula
            //       to compensate for where the front of the SceneItemOwner Shape Image is.
            desiredAngle += facingDirectionOffset;


            // so now we know where we WANT to be facing, and where we ARE facing...
            // if we weren't constrained by turnSpeed, this would be easy: we'd just 
            // return desiredAngle.
            // instead, we have to calculate how much we WANT to turn, and then make
            // sure that's not more than turnSpeed.

            // first, figure out how much we want to turn, using WrapAngle to get our
            // result from -Pi to Pi ( -180 degrees to 180 degrees )
            var currentAngleDiff = desiredAngle - currentAngle;
            var difference = MathHelper.WrapAngle(currentAngleDiff); // Return as Radians.

            // clamp that between -turnSpeed and turnSpeed.
            difference = MathHelper.Clamp(difference, -turnSpeed, turnSpeed);

            // so, the closest we can get to our target is currentAngle + difference.
            // return that, using WrapAngle again.
            currentAngleDiff = currentAngle + difference;
            var newDirection = MathHelper.WrapAngle(currentAngleDiff); // Return as Radians.


            return newDirection;
        }

        #endregion

       
    }
}