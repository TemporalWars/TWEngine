using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 10/16/2012
    ///<summary>
    /// <see cref="BufferRequest"/> messages Enumeration
    ///</summary>
    ///<remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
    public enum ChangeRequestEnum : short
    {
        ///<summary>
        /// Adds or updates the current transform.
        ///</summary>
        AddOrUpdateTransform,
        /// <summary>
        /// Add or updates to set the FlashWhite.
        /// </summary>
        AddOrUpdateFlashWhite,
        /// <summary>
        /// Add or updates to set the procedure material ID.
        /// </summary>
        AddOrUpdateProcedureId,
        // 10/18/2012
        /// <summary>
        /// Removes the current transform.
        /// </summary>
        RemoveTransform,
    }
}