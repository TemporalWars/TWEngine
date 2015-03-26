namespace ImageNexus.BenScharbach.TWEngine.BeginGame.Enums
{
    // 12/2/2013
    /// <summary>
    /// The <see cref="SetGameSettingTypeEnum"/> enum is used to set Game's setting in the <see cref="TemporalWars3DEngine.DoSetGameSetting"/> method call.
    /// </summary>
    public enum SetGameSettingTypeEnum
    {
        /// <summary>
        /// Set ScreenResolution to 1024X768, 1280X720, 1280X1024, 1440X900.
        /// </summary>
        ScreenResolution,
        /// <summary>
        /// Set to use Window-Mode or Full-Screen.
        /// </summary>
        WindowMode,
        /// <summary>
        /// Set ShadowQuality to Low (1024x1024), Med (2048x2048), High (4096x4096).
        /// </summary>
        ShadowQuality,
        /// <summary>
        /// Set the Terrain's texture quality.
        /// </summary>
        TerrainTexturesQuality,
        /// <summary>
        /// Set to TRUE to turn off Normal-Mapping.
        /// </summary>
        TurnOffNormalMap,
        /// <summary>
        ///  Set to TRUE to turn off use of Cloud-Shadows.
        /// </summary>
        TurnOffCloudShadows,
        /// <summary>
        /// Set to TRUE to turn off use of Post-Processing Effects. (Shadows/FogOfWar/Perlin-Clouds) 
        /// </summary>
        TurnOffPPEffects,
    }
}