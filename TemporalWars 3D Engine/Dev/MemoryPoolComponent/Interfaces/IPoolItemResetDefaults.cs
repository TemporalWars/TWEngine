namespace ImageNexus.BenScharbach.TWTools.MemoryPoolComponent.Interfaces
{
    // 7/9/2012
    /// <summary>
    /// Used to define the 'ResetDefault' values behavior.  This is required for the
    /// PoolItem internal wrapped object; otherwise, during re-use of the object, old values
    /// might affect the behavior of the object.
    /// </summary>
    public interface IPoolItemResetDefaults
    {
        /// <summary>
        /// Used to reset internal fields to their default values.
        /// </summary>
        void ResetToDefaultValues();
    }
}
