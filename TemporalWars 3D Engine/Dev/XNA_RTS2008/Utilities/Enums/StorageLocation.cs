#region File Description
//-----------------------------------------------------------------------------
// StorageLocation.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
namespace TWEngine.Utilities.Enums
{
    // 8/1/2008 - Used to Save ScenaryItemType Attributes.
    // 9/26/2008: Becomes nested in the 'SaveItemTypeData' Struct via Generics.

    ///<summary>
    /// The <see cref="StorageLocation"/> Enum is used to identify the desired
    /// storage location to use; either the users game folder or the game title folder.
    ///</summary>
    public enum StorageLocation
    {
        ///<summary>
        /// Uses location the game is stored in.  For example, on the PC platform, this would
        /// be in the 'Program Files' section.
        ///</summary>
        TitleStorage,
        ///<summary>
        /// Uses location designated as users games save folder.  For example, on the PC platform, 
        /// this would be in the users 'MyDocuments' section.
        ///</summary>
        UserStorage
    }
}