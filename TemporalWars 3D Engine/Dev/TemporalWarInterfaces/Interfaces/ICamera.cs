using Microsoft.Xna.Framework;

namespace TWEngine
{
    // 4/22/2010
    /// <summary>
    /// The <see cref="ICamera"/> interface provides the necessary connections for use of the camera class.
    /// </summary>
    public interface ICamera 
    {
        /// <summary>
        /// Provides ICamera matrix.
        /// </summary>
        Matrix View { get; }

        /// <summary>
        /// Provides ICamera matrix.
        /// </summary>
        Matrix Projection { get; }

        /// <summary>
        /// Provides ICamera view <see cref="BoundingFrustum"/>.
        /// </summary>
        BoundingFrustum CameraFrustum { get; }

        ///<summary>
        /// Given some world <see cref="Vector3"/> position, converts from object space to screen space.  The
        /// final <see cref="Vector2"/> screen position is returned via the <paramref name="screenPosition"/>.
        ///</summary>
        ///<param name="worldPosition">World <see cref="Vector3"/> position to convert</param>
        ///<param name="screenPosition">(OUT) Converted <see cref="Vector2"/> screen position</param>
        void ProjectToScreen(ref Vector3 worldPosition, out Vector2 screenPosition);

        /// <summary>
        /// Set the ICamera in Orthogonal view; used for IFogOfWar
        /// and IMinimap.
        /// </summary>       
        void SetOrthogonalView(int mapWidth, int mapHeight);

        /// <summary>
        /// Set the ICamera in normal R.T.S. (Real Time Strategy) view
        /// </summary>
        void SetNormalRTSView();

       
    }
}