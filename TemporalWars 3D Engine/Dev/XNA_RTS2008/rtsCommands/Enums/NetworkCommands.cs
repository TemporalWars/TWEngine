#region File Description
//-----------------------------------------------------------------------------
// NetworkCommands.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ImageNexus.BenScharbach.TWEngine.rtsCommands.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.rtsCommands.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="TWEngine.rtsCommands"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 9/3/2008
    // ReSharper disable InconsistentNaming
    ///<summary>
    /// The <see cref="NetworkCommands"/> represents the different types of <see cref="RTSCommand"/> instances to send
    /// accross wire, during network games.
    ///</summary>
    public enum NetworkCommands
    {
        ///<summary>
        /// None choosen
        ///</summary>
        None,
        ///<summary>
        /// Client requests the <see cref="RTSCommAddSceneItem"/>
        ///</summary>
        ReqAddSceneItem,
        ///<summary>
        /// Use the <see cref="RTSCommAddSceneItem"/>
        ///</summary>
        AddSceneItem,
        ///<summary>
        /// Client requests the <see cref="RTSCommMoveSceneItem"/>
        ///</summary>
        ReqUnitMoveOrder,
        ///<summary>
        /// Use the <see cref="RTSCommMoveSceneItem2"/>
        ///</summary>
        UnitMoveOrder,
        ///<summary>
        /// Use the <see cref="RTSCommStartAttackSceneItem"/>
        ///</summary>
        StartAttackSceneItem,
        ///<summary>
        /// Use the <see cref="RTSCommCeaseAttackSceneItem"/>
        ///</summary>
        CeaseAttackSceneItem,
        ///<summary>
        /// Client requests the <see cref="RTSCommStartAttackSceneItem"/>
        ///</summary>
        ReqStartAttackSceneItem,
        ///<summary>
        /// Use the <see cref="RTSCommAttackSceneItem"/>
        ///</summary>
        AttackSceneItem,
        ///<summary>
        /// Use the <see cref="RTSCommSceneItemHealth"/>
        ///</summary>
        ReduceItemHealth,
        ///<summary>
        /// Use the <see cref="RTSCommKillSceneItem"/>
        ///</summary>
        KillSceneItem,
        ///<summary>
        /// Use the <see cref="RTSCommSyncTime"/>
        ///</summary>
        SyncTime,
        ///<summary>
        /// Use the <see cref="RTSCommDelayTime"/>
        ///</summary>
        DelayTime,
        ///<summary>
        /// Use the <see cref="RTSCommIsReady"/>
        ///</summary>
        IsReady,
        ///<summary>
        /// Use the <see cref="RTSCommGameTurn"/>
        ///</summary>
        GameTurn,
        ///<summary>
        /// Use the <see cref="RTSCommQueueMarker"/>
        ///</summary>
        QueueMarker,
        ///<summary>
        /// Use the <see cref="RTSCommLobbyData"/>
        ///</summary>
        LobbyData,
        ///<summary>
        /// Use the <see cref="RTSCommLobbyData"/>
        ///</summary>
        LobbyData_UserNotReady,
        ///<summary>
        /// Use the <see cref="RTSCommGameSlow"/>
        ///</summary>
        GameSlow,
        ///<summary>
        /// Use the <see cref="RTSCommSceneItemStance"/>
        ///</summary>
        SceneItemStance,
        ///<summary>
        /// Use the <see cref="RTSCommSceneItemHealth"/>
        ///</summary>
        SceneItemHealth,
        ///<summary>
        /// Use the <see cref="RTSCommValidator"/>
        ///</summary>
        Validator,

    }
    // ReSharper restore InconsistentNaming
}
