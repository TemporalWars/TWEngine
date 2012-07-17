using Microsoft.Xna.Framework;

namespace TWEngine
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