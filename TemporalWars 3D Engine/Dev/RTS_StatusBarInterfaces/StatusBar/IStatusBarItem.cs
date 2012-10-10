using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar
{   /// <summary>
    /// Represents a single <see cref="IStatusBarItem"/> for a <see cref="IStatusBarSceneItem"/>.
    /// </summary>
    public interface IStatusBarItem
    {
        /// <summary>
        /// Enable display of <see cref="IStatusBarItem"/>.
        /// </summary>
        bool DrawStatusBar { get; set; }
        /// <summary>
        /// Current value to show in <see cref="IStatusBarItem"/>.
        /// </summary>
        float StatusBarCurrentValue { get; set; }
        /// <summary>
        /// Length of <see cref="IStatusBarItem"/> container
        /// </summary>
        int StatusBarLength { get; set; }
        /// <summary>
        /// Offset position to draw <see cref="IStatusBarItem"/>, from root position.
        /// This is because usually the root position given for a <see cref="IStatusBarSceneItem"/>, 
        /// is not the best place to draw the <see cref="IStatusBarItem"/>.
        /// </summary>
        Vector2 StatusBarOffsetPosition2D { get; set; }
        /// <summary>
        /// Starting value which defines a full <see cref="IStatusBarItem"/>.
        /// </summary>
        float StatusBarStartValue { get; set; }
        /// <summary>
        /// World 3D position to draw <see cref="IStatusBarItem"/>; 
        /// </summary>
        /// <remarks>Offset 
        /// can also be applied to this value using the <see cref="StatusBarOffsetPosition2D"/>
        /// property.</remarks>
        Vector3 StatusBarWorldPosition { get; set; }

        /// <summary>
        /// Display EnergyOff icon when power value is less than zero.
        /// </summary>
        bool ShowEnergyOffSymbol { get; set; }

        /// <summary>
        /// Identifies if the <see cref="IStatusBarItem"/> is currently in use.
        /// </summary>
        bool InUse { get; set; }

        /// <summary>
        /// Index of <see cref="IStatusBarItem"/> in internal array.
        /// </summary>
        int IndexInArray { get; set; }

        /// <summary>
        /// <see cref="IStatusBarSceneItem"/> which owns this <see cref="IStatusBarItem"/> instance.
        /// </summary>
        IStatusBarSceneItem SceneItemOwner { get; set; }

        /// <summary>
        /// Rectangle which defines this <see cref="IStatusBarItem"/>'s shape.
        /// </summary>
        Rectangle StatusBarShape { get; set; }

        /// <summary>
        /// Rectangle which defines this <see cref="IStatusBarItem"/>'s container shape.
        /// </summary>
        Rectangle StatusBarContainerShape { get; set; }
    }
}
