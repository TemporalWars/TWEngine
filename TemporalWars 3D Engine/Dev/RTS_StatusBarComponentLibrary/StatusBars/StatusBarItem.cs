using ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWLate.RTS_StatusBarComponentLibrary.StatusBars
{
    /// <summary>
    /// Represents a single<see cref="StatusBarItem"/> for a <see cref="IStatusBarSceneItem"/>.
    /// </summary>
    public struct StatusBarItem: IStatusBarItem
    {

        // 11/17/2008 - Add Ref to Owner SceneItem.
        private IStatusBarSceneItem _sceneItemOwner;
       
        private bool _drawStatusBar;
        private int _statusBarLength;
        internal Vector3 StatusBarProjectPosition;
        internal Vector2 StatusBarPosition2D;
        private Rectangle _statusBarContainerShape;
        private Rectangle _statusBarShape;

        // Status Bar Values
        private float _statusBarStartValue;
        private float _statusBarCurrentValue;
        private Vector3 _statusBarWorldPosition;
        private Vector2 _statusBarOffsetPosition2D;

        // 1/30/2009 - SceneItemOwner should show EnergyOff Symbol when power down
        private bool _showEnergyOffSymbol;

        // 4/15/2009 - InUse
        private bool _inUse;
        private int _indexInArray;


        #region Properties

        /// <summary>
        /// Length of <see cref="StatusBar"/> container
        /// </summary>
        public int StatusBarLength
        {
            get { return _statusBarLength; }
            set
            {
                _statusBarLength = value;
                _statusBarContainerShape.Width = value;
                _statusBarShape.Width = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            }
        }

        /// <summary>
        /// Enable display of <see cref="StatusBar"/>.
        /// </summary>
        public bool DrawStatusBar
        {
            get { return _drawStatusBar; }
            set 
            { 
                _drawStatusBar = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            }
        }

        /// <summary>
        /// World 3D Position to draw <see cref="StatusBar"/>; an offset 
        /// can also be applied to this value using the <see cref="StatusBarOffsetPosition2D"/> 
        /// property.
        /// </summary>
        public Vector3 StatusBarWorldPosition
        {
            get { return _statusBarWorldPosition; }
            set 
            { 
                _statusBarWorldPosition = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            }
        }

        /// <summary>
        /// Offset position to draw <see cref="StatusBar"/>, from root position.
        /// This is because usually the root Position given for a <see cref="IStatusBarSceneItem"/>, 
        /// is not the best place to draw the <see cref="StatusBar"/>!
        /// </summary>
        public Vector2 StatusBarOffsetPosition2D
        {
            get { return _statusBarOffsetPosition2D; }
            set 
            { 
                _statusBarOffsetPosition2D = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            
            }
        }

        /// <summary>
        /// Starting value which defines a full <see cref="StatusBar"/>.
        /// </summary>
        public float StatusBarStartValue
        {
            get { return _statusBarStartValue; }
            set 
            { 
                _statusBarStartValue = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            }
        }

        /// <summary>
        /// Current value to show in <see cref="StatusBar"/>.
        /// </summary>
        public float StatusBarCurrentValue
        {
            get { return _statusBarCurrentValue; }
            set 
            { 
                _statusBarCurrentValue = value;

                // 4/15/2009
                StatusBar.UpdateStatusBarItem(ref this);
            
            }
        }

        // 1/3/2010
        /// <summary>
        /// Display EnergyOff icon when power value is less than zero.
        /// </summary>
        public bool ShowEnergyOffSymbol
        {
            get { return _showEnergyOffSymbol; }
            set { _showEnergyOffSymbol = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Identifies if the <see cref="StatusBar"/> is currently in use.
        /// </summary>
        public bool InUse
        {
            get { return _inUse; }
            set { _inUse = value; }
        }

        // 1/3/2010
        /// <summary>
        /// Index of <see cref="StatusBar"/> in internal collection.
        /// </summary>
        public int IndexInArray
        {
            get { return _indexInArray; }
            set { _indexInArray = value; }
        }

        // 1/3/2010
        /// <summary>
        /// <see cref="IStatusBarSceneItem"/> which owns this <see cref="StatusBar"/> instance.
        /// </summary>
        public IStatusBarSceneItem SceneItemOwner
        {
            get { return _sceneItemOwner; }
            set { _sceneItemOwner = value; }
        }

        // 1/3/2010
        /// <summary>
        /// <see cref="Rectangle"/> which defines this <see cref="StatusBar"/> shape.
        /// </summary>
        public Rectangle StatusBarShape
        {
            get { return _statusBarShape; }
            set { _statusBarShape = value; }
        }

        // 1/3/2010
        /// <summary>
        /// <see cref="Rectangle"/> which defines this <see cref="StatusBar"/> container shape.
        /// </summary>
        public Rectangle StatusBarContainerShape
        {
            get { return _statusBarContainerShape; }
            set { _statusBarContainerShape = value; }
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="StatusBarItem"/>, linked to the given
        /// <see cref="IStatusBarSceneItem"/> owner.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="IStatusBarSceneItem"/> to link this instance.</param>
        public StatusBarItem(IStatusBarSceneItem sceneItemOwner)
        {
            // SceneItem Owner; used to check if selected by user.
            _sceneItemOwner = sceneItemOwner;
            _drawStatusBar = false;
            _statusBarLength = 100;
            StatusBarProjectPosition = Vector3.Zero;
            StatusBarPosition2D = Vector2.Zero;
            _statusBarContainerShape = new Rectangle(0, 0, 100, 5);
            _statusBarShape = new Rectangle(0, 0, 100, 5);
            _statusBarStartValue = 0;
            _statusBarCurrentValue = 0;
            _statusBarWorldPosition = Vector3.Zero;
            _statusBarOffsetPosition2D = Vector2.Zero;
            _showEnergyOffSymbol = false;
            _inUse = true;
            _indexInArray = -1;

        }
    }
}
