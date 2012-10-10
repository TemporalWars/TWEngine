namespace ImageNexus.BenScharbach.TWTools.PerfTimersComponent.Timers.Enums
{
    // 8/20/2009 - StopWatch Enum Names.
    //             CurrentCount = 63.
    ///<summary>
    /// The <see cref="StopWatchName"/> Enum.
    ///</summary>
    public enum StopWatchName
    {
        // ReSharper disable InconsistentNaming       
#pragma warning disable 1591
        AIDefenseThread,
        AStarItemThread,
        AStarThread1,
        AStarThread2,
        AStarThread3,
        FOWUpdate,
        FOWUpdate_SightMatrices,
        FOWUpdate_ObjectsVisible,
        FOWRender,
        IFDUpdate,
        IMDraw,
        InstancedItemDraw,
        InstancedItemUpdate,
        InstancedItemCameraCulling,
        UpdateTransforms,
        MinimapUpdate,
        MinimapDraw,
        ParticleSystemUpdate,
        ParticleSystemDraw,
        PlayerUpdate,
        TerrainUpdate,
        TerrainDraw, // 5/26/2010
        TerrainDrawSelectables,
        TerrainDrawSelectablesA,
        TerrainDrawSelectablesB,
        TerrainDrawAlpha,
        StartLoad,
        CreateShadowMap,
        SciFiTankShapeUpdate,
        SoundManagerUpdate,
        GameDrawLoop,
        GameDrawLoop_Begin, // 5/26/2010
        GameDrawLoop_Main, // 5/26/2010
        GameDrawLoop_Main_Cursor, // 5/25/2010
        GameDrawLoop_Main_Messages, // 5/26/2010
        GameDrawLoop_Main_IFDTiles, // 5/26/2010
        GameDrawLoop_Main_ScreenText, // 5/26/2010
        GameDrawLoop_Main_Waypoints, // 5/26/2010
        GameDrawLoop_Main_TriggerAreas, // 5/26/2010
        GameDrawLoop_Main_AreaSelect, // 5/26/2010
        GameDrawLoop_Main_Picking, // 5/26/2010
        GameDrawLoop_Main_ShadowDraw, // 5/26/2010
        GameDrawLoop_Main_ScreenDraw,
        GameDrawLoop_Main_ScreenDrawGlow, // 2/16/2010
        GameDrawLoop_Main_ScreenDrawGBlur, // 2/16/2010
        GameDrawLoop_End, // 5/26/2010
        GameUpdateLoop,
        GameEndDrawWaitForThreads,
        GameEndDrawWaitForThreads_AI,
        GameEndDrawWaitForThreads_AStar,
        GameEndDrawWaitForThreads_Steer,
        GameEndDrawWaitForThreads_IM,
        Name,
        NetworkGCUpdate,
        SteeringAIThread,
        PreLoadThread,
        PreLoadSceneryThread,
        KillSceneItemManager,
        DoChangeRequests, // 2/16/2010
        IMPSetRt, // 4/21/2010 - InstancedModelPart Set RenderStates
        IMPProcBuffs, // 4/21/2010 - InstancedModelPart Process Double Buffers
        IMPDraw // 4/21/2010 - InstancedModelPart draw
        ,
        
    }
#pragma warning restore 1591
    // ReSharper restore InconsistentNaming
}