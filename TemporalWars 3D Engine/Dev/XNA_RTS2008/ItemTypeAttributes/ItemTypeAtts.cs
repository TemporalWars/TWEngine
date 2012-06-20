#region File Description
//-----------------------------------------------------------------------------
// ItemTypeAtts.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System.Collections.Generic;
#if !XBOX360
using System.Diagnostics;
#endif
using Microsoft.Xna.Framework;
using TWEngine.GameScreens.Generic;
using TWEngine.InstancedModels.Enums;
using TWEngine.SceneItems;
using TWEngine.Utilities;
using TWEngine.InstancedModels;
using TWEngine.Utilities.Enums;
using TWEngine.Utilities.Structs;

namespace TWEngine.ItemTypeAttributes
{
      
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ItemTypeAttributes"/> namespace contains the classes
    /// which make up the entire <see cref="ItemTypeAttributes"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    
    
    ///<summary>
    /// The <see cref="ItemTypeAtts"/> base class, provides the
    /// basic load and save functionality for the XML attributes files.
    ///</summary>
    public class ItemTypeAtts
    {
        // 8/20/2008 - Save Game Instance
        /// <summary>
        /// Instance of game.
        /// </summary>
        protected static Game GameInstance;
        

        // 9/26/2008 : Dispose
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        protected static void Dispose()
        {
            GameInstance = null;
        }

        // 8/1/2008
        // 9/26/2008: Updated to use Generics.
        /// <summary>
        /// Creates the <see cref="ItemType"/> Enum attributes for each specific <see cref="ItemType"/>,
        /// and saves the data to disk.  This file is used when loading 
        /// <see cref="SceneItem"/> into memory. This allows for changing of the attributes quickly,
        /// just by updating the XML file for a specific <see cref="ItemType"/>.
        /// </summary>
        /// <remarks>This method should only me called to create the file for the first Time,
        /// or if the file is lost or destroyed.</remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="fileName">Name of file load</param>
        /// <param name="itemTypeAtts">Dictionary instance to load data into</param>
        /// <typeparam name="T">Where <typeparamref name="T"/> is a structure.</typeparam>
        protected static void CreateItemTypeAttributesAndSave<T>(Game game, string fileName, 
                                                                Dictionary<ItemType, T> itemTypeAtts) where T : struct
        {           
            // Create the data to save; XML can only save simple values, like Ints and Floats,
            // but not some Structs, like Texture2D.
            var data = new SaveItemTypesData<T>();

            // Create Temp List Array, since Dictionaries cannot be used in the Storage Class.
            var tmpItemTypesAtts = new List<T>(InstancedItem.ItemTypeCount);

            // Get Dictionary Keys to iterate collection; avoids using the ForEach construct.
            var itemKeys = new ItemType[itemTypeAtts.Keys.Count];
            itemTypeAtts.Keys.CopyTo(itemKeys, 0);


            var length = itemKeys.Length;
            for (var i = 0; i < length; i++)
            {
                tmpItemTypesAtts.Add(itemTypeAtts[itemKeys[i]]);
            }           

            // Save List Array into Data Struct.
            data.itemAttributes = tmpItemTypesAtts;

            // Create Storage Class and pass Stuct to it
            var storageTool = new Storage();
           
#if !XBOX360
            int errorCode;
            if (!storageTool.StartSaveOperation(data, fileName, @"GameData\Misc\", out errorCode))
            {
                // 4/8/2010 - Error occured, so check which one.
                if (errorCode == 1)
                {
                    /*MessageBox.Show("Locked files detected for 'ItemTypeAtts' (" + fileName + ") save.  Unlock files, and try again.",
                                    "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);*/
                    Debug.WriteLine("Locked files detected for 'ItemTypeAtts' (" + fileName + ") save.  Unlock files, and try again.");
                    return;
                }

                if (errorCode == 2)
                {
                    /*MessageBox.Show("Directory location for 'ItemTypeAtts' (" + fileName + ") save, not found.  Verify directory exist, and try again.",
                                    "Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);*/
                    Debug.WriteLine("Directory location for 'ItemTypeAtts' (" + fileName + ") save, not found.  Verify directory exist, and try again.");
                    return;
                }

                GameInstance.Window.Title = "The Save Struct data Operation Failed.";
            }
#endif                

        }

        // 8/1/2008
        // 9/26/2008: Updated to use Generics.        
        /// <summary>
        /// Loads the <see cref="SceneItem"/> <see cref="ItemType"/> Enum attributes
        /// back into memory, from the XML file.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="fileName">Name of file load</param>
        /// <param name="itemTypeAtts">(OUT) List collection</param>
        /// <param name="itemTypeCount">Count of items in <paramref name="itemTypeAtts"/> collection.</param>
        /// <typeparam name="T">Where <typeparamref name="T"/> is a structure.</typeparam>
        /// <returns>True or False of result.</returns>
        protected static bool LoadItemTypeAttributes<T>(Game game, string fileName,
                                                        out List<T> itemTypeAtts, int itemTypeCount) where T : struct
        { 
            // Create the Struct to hold the return data
            SaveItemTypesData<T> data;

            // Create Temp List Array, since Dictionaries cannot be used in the Storage Class.
            itemTypeAtts = new List<T>(InstancedItem.ItemTypeCount);

            // Create Storage Class and pass Struct to it
            var storageTool = new Storage();
            if (storageTool.StartLoadOperation(out data, fileName, @"GameData\Misc\", StorageLocation.TitleStorage))
            {
                // 1st - Extract Data Attributes from XML file
                itemTypeAtts = data.itemAttributes;

                // Check to see if ItemCount has changed? 
                // We add 1 to first 'Count', since it is a zero-based value, and then 2nd is not.
                if ((itemTypeAtts.Count + 1) != itemTypeCount)
                {
                    game.Window.Title = "The ItemType Count Load does not match internal Count... Automatically recreating file to fix.";
                                        
                    LoadingScreen.LoadingMessage = "Recreating Atts File";
                    return false;
                }

                return true;
            }

            game.Window.Title = "The Load Struct 'ItemType Attributes' data Operation Failed... Automatically recreating file.";
                               
            return false;
        }
       
    }
}
