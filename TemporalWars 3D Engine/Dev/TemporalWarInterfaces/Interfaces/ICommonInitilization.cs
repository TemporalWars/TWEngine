using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces
{
    /// <summary>
    /// Required common initialization interface as the XBox does not allow passing of
    /// game instance through the constructor during late binding.
    /// </summary>
    public interface ICommonInitilization
    {
        void CommonInitilization(Game game);
    }
}