using System;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWLate.RTS_StatusBarInterfaces.StatusBar
{
    // NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine"/> namespace contains the enum
    ///
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// The <see cref="IStatusBar"/> or (Health Bar) is a game mechanic used in computer
    /// and video games to give value to characters, enemies, NPCs, and related objects.
    /// </summary>
    public interface IStatusBar : IDisposable, ICommonInitilization
    {
        /// <summary>
        /// Adds a <see cref="IStatusBarItem"/> instance to display.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="IStatusBarSceneItem"/> instance </param>
        /// <param name="statusBarItem">(OUT) <see cref="IStatusBarItem"/></param>
        void AddNewStatusBarItem(IStatusBarSceneItem sceneItemOwner, out IStatusBarItem statusBarItem);
        /// <summary>
        /// Removes a <see cref="IStatusBarItem"/> instance from the display, by simply changing the
        /// internal flag to 'InUse=False'.
        /// </summary>
        /// <param name="statusBarItem"><see cref="IStatusBarItem"/> reference to remove </param>
        void RemoveStatusBarItem(ref IStatusBarItem statusBarItem);
        /// <summary>
        /// Allows updating a <see cref="IStatusBarItem"/> instance.
        /// </summary>
        /// <param name="statusBarItem"><see cref="IStatusBarItem"/> to update </param>
        void UpdateStatusBarItem(ref IStatusBarItem statusBarItem);
        /// <summary>
        /// Clears out all <see cref="IStatusBarItem"/> from the internal collection.
        /// </summary>
        void ClearAllStatusBarItems();
        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime">Provides the GameTime instance</param>
        void Draw(GameTime gameTime);
    }
}
