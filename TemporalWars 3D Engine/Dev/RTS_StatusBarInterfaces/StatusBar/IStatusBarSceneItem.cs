namespace TWEngine
{
    /// <summary>
    /// The <see cref="IStatusBarSceneItem"/> is the base class, which provides the primary funtions
    /// for any <see cref="IStatusBarSceneItem"/>.  This includes updating the transforms for position data, 
    /// updating attributes, like current health, etc.
    /// This class inherts from a collection of <see cref="IStatusBarSceneItem"/>, allowing a single item to have
    /// multiple children of <see cref="IStatusBarSceneItem"/>.
    /// </summary>
    public interface IStatusBarSceneItem
    {
        /// <summary>
        /// The <see cref="IStatusBarPlayer"/> number this <see cref="IStatusBarSceneItem"/> belongs to.
        /// </summary>
        byte PlayerNumber { get; }
        /// <summary>
        /// This <see cref="IStatusBarSceneItem"/> is pick selected?
        /// </summary>
        bool PickSelected { get; }
        /// <summary>
        /// Is this <see cref="IStatusBarSceneItem"/> currently pick hovered by cursor?
        /// </summary>
        bool PickHovered { get; }
    }
}