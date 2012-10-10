using System;
using ImageNexus.BenScharbach.TWEngine.TemporalWarInterfaces.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW
{
    // 7/3/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine"/> namespace contains the classes
    /// which make up the entire <see cref="IFogOfWar"/> Interface.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// The <see cref="IFogOfWar"/> class is a shroud used to hide places, buildings, and enemy units that a player hasn't yet seen.  This means any
    ///	areas which aren't within the sight range of a friendly unit are hidden from the player's view by the fog-of-war.
    /// </summary>
    public interface IFogOfWar : IDisposable, ICommonInitilization
    {
        ///<summary>
        /// <see cref="Game"/> instance
        ///</summary>
        Game GameInstance { set; }

        ///<summary>
        /// The <see cref="Texture2D"/> with fog-of-war <see cref="RenderTarget"/> result.
        ///</summary>
        Texture2D FogOfWarTexture { get; set; }

        ///<summary>
        /// Show <see cref="IFogOfWar"/>?
        ///</summary>
        bool IsVisible { get; set; }

        ///<summary>
        /// Set to force SightMatrices to update themselves, which is 
        /// indirectly done via the <see cref="UpdateSightMatrices"/> method.
        ///</summary>
        bool UpdateSight { get; set; }

        /// <summary>
        /// Call to set the interface references for the <see cref="IFOWTerrainShape"/> and
        /// <see cref="IFOWTerrainData"/>. 
        /// </summary>
        void InitFogOfWarSettings();

        /// <summary>
        /// <see cref="EventHandler"/> for forcing the SightMatrices to update themselves, which is 
        /// indirectly done via the <see cref="UpdateSightMatrices"/> method.
        /// </summary>
        /// <param name="sender">Provides a reference to the object that raised the event.</param>
        /// <param name="e">Passes an object specific to the event that is being handled.</param>
        void UpdateSightMatrices(object sender, EventArgs e);

        // 6/10/2010
        ///<summary>
        /// Add a <see cref="IFOWSceneItem"/> selectable item to the
        /// internal collection, for use when doing the sight calculations.
        ///</summary>
        ///<param name="selectableItem"><see cref="IFOWSceneItem"/> instance</param>
        void AddSelectableItem(IFOWSceneItem selectableItem);

        // 6/10/2010
        /// <summary>
        /// Removes all <see cref="IFOWSceneItem"/> selectable items where the
        /// 'Delete' property is set to TRUE.
        /// </summary>
        /// <param name="selectableItem"><see cref="IFOWSceneItem"/> instance</param>
        void RemoveSelectableItem(IFOWSceneItem selectableItem);
    }
}
