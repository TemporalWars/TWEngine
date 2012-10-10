namespace ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar
{
    /// <summary>
    /// Provides the energy value from the defined <see cref="IStatusBarPlayer"/> instance.
    /// </summary>
    public interface IStatusBarPlayer
    {
        /// <summary>
        /// Allows the <see cref="IStatusBarPlayer"/> class instance to check the EnergyOff status.
        /// </summary>
        bool EnergyOff { get; } 
    }
}