#region File Description
//-----------------------------------------------------------------------------
// ChangeRequest.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using ImageNexus.BenScharbach.TWEngine.SceneItems;

namespace ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.InstancedModels.Enums"/> namespace contains the enumerations
    /// which make up the entire <see cref="TWEngine.TWEngine.InstancedModels.Enumsnent.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    ///<summary>
    /// <see cref="ChangeRequest"/> messages Enumeration
    ///</summary>
    ///<remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
    public enum ChangeRequest
    {
// ReSharper disable InconsistentNaming
        ///<summary>
        /// To add or update a normal <see cref="SceneItemWithPick"/> 'selectable' <see cref="InstancedItem"/>.
        ///</summary>
        AddUpdatePart_InstanceItem,
        ///<summary>
        /// To add or update a <see cref="ScenaryItemScene"/> 'scenary' <see cref="InstancedItem"/>.
        ///</summary>
        /// <remarks>Will clear all prior Culled Parts, while adding new culled part.</remarks>
        AddUpdateSceneryPart_InstanceItem, // 8/19/2009 - 
        ///<summary>
        /// Deletes all current <see cref="InstancedItem"/> parts.
        ///</summary>
        DeleteAllParts_InstanceItem, // 8/27/2009
        ///<summary>
        /// Deletes ONLY culled parts for 'selectable' <see cref="InstancedItem"/> parts.
        ///</summary>
        DeleteAllCulledParts_InstanceItem, //8/27/2009
        ///<summary>
        /// Deletes ONLY culled parts for both 'selectable' and 'scenary' <see cref="InstancedItem"/> parts.
        ///</summary>
        DeleteAllCulledParts_AllItems,
// ReSharper restore InconsistentNaming
       
    }
}