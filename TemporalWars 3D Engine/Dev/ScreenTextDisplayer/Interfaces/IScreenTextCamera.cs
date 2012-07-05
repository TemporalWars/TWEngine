using Microsoft.Xna.Framework;

namespace ScreenTextDisplayer.Interfaces
{
    // 4/22/2010
    /// <summary>
    /// This <see cref="IScreenTextCamera"/> interface, defines the required interfaces that some 'Camera'
    /// interface must provide, in order to function properly.
    /// </summary>
    public interface IScreenTextCamera
    {
        void ProjectToScreen(ref Vector3 worldPosition, out Vector2 screenPosition);
    }
}
