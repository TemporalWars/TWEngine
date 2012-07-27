namespace TWEngine
{
    /// <summary>
    /// The <see cref="IMinimapShapeItem"/> is the base class of any object that is renderable
    /// </summary>
    public interface IMinimapShapeItem
    {
        ///<summary>
        /// Path block size area to affect?
        ///</summary>
        /// <remarks>Requires the IsPathBlocked to be TRUE.</remarks>
        int PathBlockSize { get; set; }
    }
}