#region File Description
//-----------------------------------------------------------------------------
// AIOrderType.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace ImageNexus.BenScharbach.TWEngine.SceneItems.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.SceneItems.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="TWEngine.TWEngine.SceneItems.Enumsnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    // 10/16/2012: Updated enum to inherit from short value.
    // 6/2/2009 - AI Order Types
    ///<summary>
    /// The <see cref="AIOrderType"/> Enum tracks if attack order was
    /// requested by player or computer AI.
    ///</summary>
    public enum AIOrderType : short
    {
        ///<summary>
        /// None - default state.
        ///</summary>
        None,
        ///<summary>
        /// Attack order requested by computer AI.
        ///</summary>
        NonAIAttackOrderRequest,
        ///<summary>
        /// Attack order requested by player.
        ///</summary>
        AIAttackOrderRequest
    }
}
