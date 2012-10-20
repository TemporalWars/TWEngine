#region File Description
//-----------------------------------------------------------------------------
// BufferRequest.cs
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
    /// which make up the entire.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 10/16/2012: Updated enum to inherit from short value.
    // 10/16/2012: Renamed from 'ChangeRequest' to now be 'BufferRequest'
    ///<summary>
    /// <see cref="BufferRequest"/> messages Enumeration
    ///</summary>
    ///<remarks>'Selectable' items are of type <see cref="SceneItemWithPick"/>, while 'Scenary' items are of type <see cref="ScenaryItemScene"/>.</remarks>
    public enum BufferRequest : short
    {
        /// <summary>
        /// Removes all draw transforms for all <see cref="InstancedModelPart"/> for a given <see cref="InstancedModel"/>.
        /// </summary>
        ClearAllDrawTransformsForInstancedModel, // 10/18/2012
        ///<summary>
        /// To add or update a single <see cref="InstancedModelPart"/> for a given <see cref="InstancedModel"/>.
        ///</summary>
        AddOrUpdateInstancedModelPart,
        ///<summary>
        /// Removes a single <see cref="InstancedModelPart"/> for the given <see cref="InstancedModel"/>.
        ///</summary>
        RemoveInstancedModelPart,
    }
}