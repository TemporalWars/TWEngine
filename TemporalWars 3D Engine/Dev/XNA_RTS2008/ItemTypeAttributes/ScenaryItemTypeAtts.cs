#region File Description
//-----------------------------------------------------------------------------
// ScenaryItemTypeAtts.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes
{
    ///<summary>
    /// The <see cref="ScenaryItemTypeAtts"/> class, inheriting from <see cref="ItemTypeAtts"/> base class, is
    /// used to save and load the <see cref="ScenaryItemTypeAttributes"/> structure, for a given <see cref="ItemType"/>.
    /// This is specifically used for the <see cref="ScenaryItemScene"/> type <see cref="SceneItem"/> class.
    ///</summary>
    public class ScenaryItemTypeAtts : ItemTypeAtts
    {        
        // TODO: Maybe replace the 'ItemType' to the INT, since boxing is occuring?!
        // 8/1/2008 - Specific ItemType Attributes; the 'ScenaryItemTypeAttributes' Struct is in the 'Storage' Class.
        /// <summary>
        /// Internal Dictionary, used to store the <see cref="ScenaryItemTypeAttributes"/> structure, using the <see cref="ItemType"/> as the key.
        /// </summary>
        internal static Dictionary<ItemType, ScenaryItemTypeAttributes> ItemTypeAtts = new Dictionary<ItemType, ScenaryItemTypeAttributes>(InstancedItem.ItemTypeCount);

        // 1/6/2009 - 
        /// <summary>
        /// Set TRUE to force recreation of atts file!
        /// </summary>
        private static bool _forceRebuildOfXMLFile;

        // 5/1/2009 - Create Private Constructor, per FXCop
        ScenaryItemTypeAtts()
        {
            // Empty
            _forceRebuildOfXMLFile = false;
        }

        // 8/1/2008
        /// <summary>
        /// Creates the <see cref="ScenaryItemTypeAttributes"/> structure for each specific <see cref="ItemType"/>,
        /// and saves the data to disk.  This file is used when loading <see cref="ScenaryItemScene"/> back into memory. 
        /// This allows for changing of the attributes quickly, just by updating the XML file for a specific <see cref="ItemType"/>.
        /// </summary>
        /// <remarks>This method should only me called to create the file for the first Time,
        /// or if the file is lost or destroyed.</remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        private static void CreateItemTypeAttributesAndSave(Game game)
        {
            // 8/20/2008 - Save Game Ref
            GameInstance = game;

#if XBOX360
            const string contentInstancedModels = @"1ContentPlayable\XBox360\"; // 3/31/2011 - Update path from @"ContentPlayableModels\"
#else
            const string contentInstancedModels = @"1ContentPlayable\x86\"; // 3/31/2011 - Update path from @"ContentPlayableModels\"
#endif

            // 9/22/2009
            const string contentRTSPack = @"ContentRTSPack\";
            const string contentUrbanPack = @"ContentUrbanPack\Urban\";
            const string contentSticksNTwigg = @"ContentSticksNTwiggPack\STPack\";
            const string alleypack = @"ContentAlleyPack\AlleyPack\";
            const string downtowndistrictpack = @"DowntownDistrictPack\";
            const string warehousedistrictpack = @"ContentWarehouseDistrictPack\WarehouseDistrictPack\";

            #region DesertSet

            // Desert Tree 1
            AddItemTypeAttributeToArray(ItemType.desertTree01, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTree01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Desert Tree 2
            AddItemTypeAttributeToArray(ItemType.desertTree02, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTree02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Tree 3
            AddItemTypeAttributeToArray(ItemType.desertTree03, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTree03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Desert Tree 4
            AddItemTypeAttributeToArray(ItemType.desertTree04, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTree04", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // DesertTreeSmall1
            AddItemTypeAttributeToArray(ItemType.desertTreeSmall01, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTreeSmall01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
           
            // DesertTreeSmall2
            AddItemTypeAttributeToArray(ItemType.desertTreeSmall02, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTreeSmall02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // DesertTreeSmall3
            AddItemTypeAttributeToArray(ItemType.desertTreeSmall03, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTreeSmall03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
           
            // DesertTreeSmall4
            AddItemTypeAttributeToArray(ItemType.desertTreeSmall04, ModelType.InstanceModel, contentRTSPack + @"Trees\DesertSet\desertTreeSmall04", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Flower
            AddItemTypeAttributeToArray(ItemType.desertFlower, ModelType.InstanceModel, contentRTSPack + @"Plants\DesertSet\desertFlower", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Grass 1
            AddItemTypeAttributeToArray(ItemType.desertGrass01, ModelType.InstanceModel, contentRTSPack + @"Plants\DesertSet\desertGrass01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Grass 2
            AddItemTypeAttributeToArray(ItemType.desertGrass02, ModelType.InstanceModel, contentRTSPack + @"Plants\DesertSet\desertGrass02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Bush 1
            AddItemTypeAttributeToArray(ItemType.desertBush01, ModelType.InstanceModel, contentRTSPack + @"Plants\DesertSet\desertBush01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Desert Yuka Plant 
            AddItemTypeAttributeToArray(ItemType.yukaPlant, ModelType.InstanceModel, contentRTSPack + @"Plants\desertSet\yukaPlant", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);       
            // Desert Rock 1
            AddItemTypeAttributeToArray(ItemType.desertRock01, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Rock 2
            AddItemTypeAttributeToArray(ItemType.desertRock02, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
           
            // Desert Rock 3
            AddItemTypeAttributeToArray(ItemType.desertRock03, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Rock 4
            AddItemTypeAttributeToArray(ItemType.desertRock04, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Rock 5
            AddItemTypeAttributeToArray(ItemType.desertRock05, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            
            // Desert Rock 6
            AddItemTypeAttributeToArray(ItemType.desertRock06, ModelType.InstanceModel, contentRTSPack + @"Rocks\DesertSet\desertRock06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
           

            #endregion

            #region VolcanicSet

            // Volcanic Tree1
            AddItemTypeAttributeToArray(ItemType.volcanicTree01, ModelType.InstanceModel, contentRTSPack + @"Trees\VolcanicSet\volcanicTree01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Tree2
            AddItemTypeAttributeToArray(ItemType.volcanicTree02, ModelType.InstanceModel, contentRTSPack + @"Trees\VolcanicSet\volcanicTree02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Tree3
            AddItemTypeAttributeToArray(ItemType.volcanicTree03, ModelType.InstanceModel, contentRTSPack + @"Trees\VolcanicSet\volcanicTree03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Tree4
            AddItemTypeAttributeToArray(ItemType.volcanicTree04, ModelType.InstanceModel, contentRTSPack + @"Trees\VolcanicSet\volcanicTree04", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Tree5
            AddItemTypeAttributeToArray(ItemType.volcanicTree05, ModelType.InstanceModel, contentRTSPack + @"Trees\VolcanicSet\volcanicTree05", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Bush1
            AddItemTypeAttributeToArray(ItemType.volcanicBush01, ModelType.InstanceModel, contentRTSPack + @"Plants\VolcanicSet\volcanicBush01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            
            // Volcanic Bush2
            AddItemTypeAttributeToArray(ItemType.volcanicBush02, ModelType.InstanceModel, contentRTSPack + @"Plants\VolcanicSet\volcanicBush02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);          
            // Volcanic Bush3
            AddItemTypeAttributeToArray(ItemType.volcanicBush03, ModelType.InstanceModel, contentRTSPack + @"Plants\VolcanicSet\volcanicBush03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);            

            #endregion

            #region WinterSet

            // Winter Tree 1
            AddItemTypeAttributeToArray(ItemType.winterTree01, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree01", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 2
            AddItemTypeAttributeToArray(ItemType.winterTree02, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree02", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 3
            AddItemTypeAttributeToArray(ItemType.winterTree03, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree03", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 4
            AddItemTypeAttributeToArray(ItemType.winterTree04, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree04", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 5
            AddItemTypeAttributeToArray(ItemType.winterTree05, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree05", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 6
            AddItemTypeAttributeToArray(ItemType.winterTree06, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree06", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            //  Winter Tree 7
            AddItemTypeAttributeToArray(ItemType.winterTree07, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTree07", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 1
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow01, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow01", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 2
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow02, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow02", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 3
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow03, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow03", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 4
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow04, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow04", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 5
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow05, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow05", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 6
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow06, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow06", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Tree Snow 7
            AddItemTypeAttributeToArray(ItemType.winterTreeSnow07, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterTreeSnow07", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Bush 1
            AddItemTypeAttributeToArray(ItemType.winterBush01, ModelType.InstanceModel, contentRTSPack + @"Plants\WinterSet\winterBush01", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Bush 2
            AddItemTypeAttributeToArray(ItemType.winterBush02, ModelType.InstanceModel, contentRTSPack + @"Plants\WinterSet\winterBush02", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Dead Grass 1
            AddItemTypeAttributeToArray(ItemType.winterDeadGrass01, ModelType.InstanceModel, contentRTSPack + @"Plants\WinterSet\winterDeadGrass01", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Dead Grass 2
            AddItemTypeAttributeToArray(ItemType.winterDeadGrass02, ModelType.InstanceModel, contentRTSPack + @"Plants\WinterSet\winterDeadGrass02", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Dead Grass 3
            AddItemTypeAttributeToArray(ItemType.winterDeadGrass03, ModelType.InstanceModel, contentRTSPack + @"Plants\WinterSet\winterDeadGrass03", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Log 1
            AddItemTypeAttributeToArray(ItemType.winterLog01, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterLog01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Log 2
            AddItemTypeAttributeToArray(ItemType.winterLog02, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterLog02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Log 3
            AddItemTypeAttributeToArray(ItemType.winterLog03, ModelType.InstanceModel, contentRTSPack + @"Trees\WinterSet\winterLog03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone 1
            AddItemTypeAttributeToArray(ItemType.winterStone01, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStone01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone 2
            AddItemTypeAttributeToArray(ItemType.winterStone02, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStone02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone 3
            AddItemTypeAttributeToArray(ItemType.winterStone03, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStone03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone 4
            AddItemTypeAttributeToArray(ItemType.winterStone04, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStone04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone 5
            AddItemTypeAttributeToArray(ItemType.winterStone05, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStone05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Snow 1
            AddItemTypeAttributeToArray(ItemType.winterStoneSnow01, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneSnow01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Snow 2
            AddItemTypeAttributeToArray(ItemType.winterStoneSnow02, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneSnow02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Snow 3
            AddItemTypeAttributeToArray(ItemType.winterStoneSnow03, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneSnow03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Snow 4
            AddItemTypeAttributeToArray(ItemType.winterStoneSnow04, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneSnow04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Snow 5
            AddItemTypeAttributeToArray(ItemType.winterStoneSnow05, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneSnow05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Arch 1
            AddItemTypeAttributeToArray(ItemType.winterStoneArch01, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneArch01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Arch 2
            AddItemTypeAttributeToArray(ItemType.winterStoneArch02, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneArch02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Arch 3
            AddItemTypeAttributeToArray(ItemType.winterStoneArch03, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneArch03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Arch 4
            AddItemTypeAttributeToArray(ItemType.winterStoneArch04, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneArch04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Stone Arch 5
            AddItemTypeAttributeToArray(ItemType.winterStoneArch05, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterStoneArch05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 1
            AddItemTypeAttributeToArray(ItemType.winterRock01, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 2
            AddItemTypeAttributeToArray(ItemType.winterRock02, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 3
            AddItemTypeAttributeToArray(ItemType.winterRock03, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 4
            AddItemTypeAttributeToArray(ItemType.winterRock04, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 5
            AddItemTypeAttributeToArray(ItemType.winterRock05, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock 6
            AddItemTypeAttributeToArray(ItemType.winterRock06, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRock06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock Snow 1
            AddItemTypeAttributeToArray(ItemType.winterRockSnow01, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRockSnow01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock Snow 2
            AddItemTypeAttributeToArray(ItemType.winterRockSnow02, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRockSnow02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock Snow 3
            AddItemTypeAttributeToArray(ItemType.winterRockSnow03, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRockSnow03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock Snow 4
            AddItemTypeAttributeToArray(ItemType.winterRockSnow04, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRockSnow04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Winter Rock Snow 5
            AddItemTypeAttributeToArray(ItemType.winterRockSnow05, ModelType.InstanceModel, contentRTSPack + @"Rocks\WinterSet\winterRockSnow05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);


            #endregion

            #region SwampSet

            // Swamp Tree 1
            AddItemTypeAttributeToArray(ItemType.swampTree01, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampTree01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Tree 2
            AddItemTypeAttributeToArray(ItemType.swampTree02, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampTree02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Tree 3
            AddItemTypeAttributeToArray(ItemType.swampTree03, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampTree03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Stump 1
            AddItemTypeAttributeToArray(ItemType.swampStump01, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampStump01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Stump 2
            AddItemTypeAttributeToArray(ItemType.swampStump02, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampStump02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Stump 3
            AddItemTypeAttributeToArray(ItemType.swampStump03, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampStump03", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Stump 4
            AddItemTypeAttributeToArray(ItemType.swampStump04, ModelType.InstanceModel, contentRTSPack + @"Trees\SwampSet\swampStump04", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Fern 1 Plant
            AddItemTypeAttributeToArray(ItemType.swampFern01, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampFern01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Fern 2 Plant
            AddItemTypeAttributeToArray(ItemType.swampFern02, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampFern02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Grass 1 Plant
            AddItemTypeAttributeToArray(ItemType.swampGrass01, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampGrass01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Grass 2 Plant
            AddItemTypeAttributeToArray(ItemType.swampGrass02, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampGrass02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Grass-CatTail 1 Plant
            AddItemTypeAttributeToArray(ItemType.swampGrassCatTail01, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampGrassCattail01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Grass-CatTail 2 Plant
            AddItemTypeAttributeToArray(ItemType.swampGrassCatTail02, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampGrassCattail02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Palm 1 Plant
            AddItemTypeAttributeToArray(ItemType.swampPalm01, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampPalm01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Palm 2 Plant
            AddItemTypeAttributeToArray(ItemType.swampPalm02, ModelType.InstanceModel, contentRTSPack + @"Plants\SwampSet\swampPalm02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Swamp Temple 1
            AddItemTypeAttributeToArray(ItemType.swampTemple01, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampTemple01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Temple 2
            AddItemTypeAttributeToArray(ItemType.swampTemple02, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampTemple02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Temple 3
            AddItemTypeAttributeToArray(ItemType.swampTemple03, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampTemple03", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Temple 4
            AddItemTypeAttributeToArray(ItemType.swampTemple04, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampTemple04", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Temple 5
            AddItemTypeAttributeToArray(ItemType.swampTemple05, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampTemple05", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Pillar 1
            AddItemTypeAttributeToArray(ItemType.swampPillar01, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPillar01", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Swamp Pillar 2
            AddItemTypeAttributeToArray(ItemType.swampPillar02, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPillar02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Pillar 3
            AddItemTypeAttributeToArray(ItemType.swampPillar03, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPillar03", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Swamp Platform 1
            AddItemTypeAttributeToArray(ItemType.swampPlatform01, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPlatform01", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Swamp Platform 2
            AddItemTypeAttributeToArray(ItemType.swampPlatform02, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPlatform02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Swamp Platform 3
            AddItemTypeAttributeToArray(ItemType.swampPlatform03, ModelType.InstanceModel, contentRTSPack + @"Structures\SwampSet\swampPlatform03", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);

            #endregion

            #region FarmSet

            // Bridge 1
            AddItemTypeAttributeToArray(ItemType.farmBridge01, ModelType.InstanceModel, contentRTSPack + @"Bridges\farmBridge01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barn 1
            AddItemTypeAttributeToArray(ItemType.farmBarn01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmBarn01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Barn 2
            AddItemTypeAttributeToArray(ItemType.farmBarn02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmBarn02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Barn 3
            AddItemTypeAttributeToArray(ItemType.farmBarn03, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmBarn03", false, true,
                                        true, 2, false, 30, 30, false, string.Empty, 0);
            // Church 1
            AddItemTypeAttributeToArray(ItemType.farmChurch01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmChurch01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Out Building 1
            AddItemTypeAttributeToArray(ItemType.farmOutBuilding01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmOutBuilding01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Out Building 2
            AddItemTypeAttributeToArray(ItemType.farmOutBuilding02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmOutBuilding02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Small Barn 1
            AddItemTypeAttributeToArray(ItemType.farmSmallBarn01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmSmallBarn01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // Small Barn 2
            AddItemTypeAttributeToArray(ItemType.farmSmallBarn02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmSmallBarn02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // WindMill
            AddItemTypeAttributeToArray(ItemType.farmWindMill, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmWindMill", false, true,
                                        true, 1, false, 45, 45, true, "WindMill", 0);
            // WaterMill
            AddItemTypeAttributeToArray(ItemType.farmWaterMill, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet1\farmWaterMill", false, true,
                                        true, 2, false, 0, 0, true, "Wheel", 0);
            // Oak Tree 1
            AddItemTypeAttributeToArray(ItemType.farmOakTree01, ModelType.InstanceModel, contentRTSPack + @"Trees\FarmSet\oakTree01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak Bush 1
            AddItemTypeAttributeToArray(ItemType.farmOakBush01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\oakBush01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel 1
            AddItemTypeAttributeToArray(ItemType.farmBarrel01, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrel01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel 2
            AddItemTypeAttributeToArray(ItemType.farmBarrel02, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrel02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 1
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet01, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 2
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet02, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 3
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet03, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 4
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet04, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 5
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet05, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 6
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet06, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Barrel Set 7
            AddItemTypeAttributeToArray(ItemType.farmBarrelSet07, ModelType.InstanceModel, contentRTSPack + @"Barrels\farmBarrelSet07", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate 1
            AddItemTypeAttributeToArray(ItemType.farmSmallCrate01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrate01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate 2
            AddItemTypeAttributeToArray(ItemType.farmSmallCrate02, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrate02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate Open 1
            AddItemTypeAttributeToArray(ItemType.farmSmallCrateOpen01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrateOpen01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate Open 2
            AddItemTypeAttributeToArray(ItemType.farmSmallCrateOpen02, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrateOpen02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate Set 1
            AddItemTypeAttributeToArray(ItemType.farmSmallCrateSet01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrateSet01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Small Crate Set 2
            AddItemTypeAttributeToArray(ItemType.farmSmallCrateSet02, ModelType.InstanceModel, contentRTSPack + @"Crates\farmSmallCrateSet02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate 1
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrate01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrate01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);           
            // Wooden Crate Open 1
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateOpen01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateOpen01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Open 2
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateOpen02, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateOpen02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Set 1
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateSet01, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateSet01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Set 2
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateSet02, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateSet02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Set 3
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateSet03, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateSet03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Set 4
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateSet04, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateSet04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Wooden Crate Set 5
            AddItemTypeAttributeToArray(ItemType.farmWoodenCrateSet05, ModelType.InstanceModel, contentRTSPack + @"Crates\farmWoodenCrateSet05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // West Barn 1
            AddItemTypeAttributeToArray(ItemType.farmWestBarn01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 2
            AddItemTypeAttributeToArray(ItemType.farmWestBarn02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 3
            AddItemTypeAttributeToArray(ItemType.farmWestBarn03, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn03", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 4 
            AddItemTypeAttributeToArray(ItemType.farmWestBarn04, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn04", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 5
            AddItemTypeAttributeToArray(ItemType.farmWestBarn05, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn05", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 6
            AddItemTypeAttributeToArray(ItemType.farmWestBarn06, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn06", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Barn 7
            AddItemTypeAttributeToArray(ItemType.farmWestBarn07, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestBarn07", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Church 1
            AddItemTypeAttributeToArray(ItemType.farmWestChurch01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestChurch01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Out Building 1
            AddItemTypeAttributeToArray(ItemType.farmWestOutBuilding01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestOutBuilding01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Out Building 2
            AddItemTypeAttributeToArray(ItemType.farmWestOutBuilding02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestOutBuilding02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Silo 1
            AddItemTypeAttributeToArray(ItemType.farmWestSilo01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSilo01", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Silo 2
            AddItemTypeAttributeToArray(ItemType.farmWestSilo02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSilo02", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Silo 3
            AddItemTypeAttributeToArray(ItemType.farmWestSilo03, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSilo03", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Silo 4
            AddItemTypeAttributeToArray(ItemType.farmWestSilo04, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSilo04", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Silo 5
            AddItemTypeAttributeToArray(ItemType.farmWestSilo05, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSilo05", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Small Barn 1
            AddItemTypeAttributeToArray(ItemType.farmWestSmallBarn01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSmallBarn01", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Small Barn 2
            AddItemTypeAttributeToArray(ItemType.farmWestSmallBarn02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSmallBarn02", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Small Barn 3
            AddItemTypeAttributeToArray(ItemType.farmWestSmallBarn03, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestSmallBarn03", false, true,
                                        true, 2, false, 0, 0, false, string.Empty, 0);
            // West Wind Mill
            AddItemTypeAttributeToArray(ItemType.farmWestWindMill, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestWindMill", false, true,
                                        true, 1, false, 0, 0, true, "Fan", 0);
            // West Water Mill
            AddItemTypeAttributeToArray(ItemType.farmWestWaterMill, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestWaterMill", false, true,
                                        true, 2, false, 0, 0, true, "Wheel", 0);
            // West Water Tower 1
            AddItemTypeAttributeToArray(ItemType.farmWestWaterTower01, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestWaterTower01", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // West Water Tower 2
            AddItemTypeAttributeToArray(ItemType.farmWestWaterTower02, ModelType.InstanceModel, contentRTSPack + @"FarmSet\BuildingSet2\farmWestWaterTower02", false, true,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Brush 1
            AddItemTypeAttributeToArray(ItemType.farmBrush01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmBrush01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Brush 2
            AddItemTypeAttributeToArray(ItemType.farmBrush02, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmBrush02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Flower 1
            AddItemTypeAttributeToArray(ItemType.farmFlower01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmFlower01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Flower 2
            AddItemTypeAttributeToArray(ItemType.farmFlower02, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmFlower02", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Weed 1
            AddItemTypeAttributeToArray(ItemType.farmWeed01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmWeed01", true, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin 1
            AddItemTypeAttributeToArray(ItemType.farmPumpkin01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkin01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin 2
            AddItemTypeAttributeToArray(ItemType.farmPumpkin02, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkin02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin 3
            AddItemTypeAttributeToArray(ItemType.farmPumpkin03, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkin03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin N Vine 1
            AddItemTypeAttributeToArray(ItemType.farmPumpkinVine01, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkinVine01", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin N Vine 2
            AddItemTypeAttributeToArray(ItemType.farmPumpkinVine02, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkinVine02", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Pumpkin N Vine 3
            AddItemTypeAttributeToArray(ItemType.farmPumpkinVine03, ModelType.InstanceModel, contentRTSPack + @"Plants\FarmSet\farmPumpkinVine03", true, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // LampPost 1
            AddItemTypeAttributeToArray(ItemType.farmLampPost01, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // LampPost 2
            AddItemTypeAttributeToArray(ItemType.farmLampPost02, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // LampPost 3
            AddItemTypeAttributeToArray(ItemType.farmLampPost03, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // LampPost 4
            AddItemTypeAttributeToArray(ItemType.farmLampPost04, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // LampPost 5
            AddItemTypeAttributeToArray(ItemType.farmLampPost05, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // LampPost 6
            AddItemTypeAttributeToArray(ItemType.farmLampPost06, ModelType.InstanceModel, contentRTSPack + @"LampPosts\farmLampPost06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 23);
            // HeadStone 1
            AddItemTypeAttributeToArray(ItemType.farmHeadStone01, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 2
            AddItemTypeAttributeToArray(ItemType.farmHeadStone02, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 3
            AddItemTypeAttributeToArray(ItemType.farmHeadStone03, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 4
            AddItemTypeAttributeToArray(ItemType.farmHeadStone04, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 5
            AddItemTypeAttributeToArray(ItemType.farmHeadStone05, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 6
            AddItemTypeAttributeToArray(ItemType.farmHeadStone06, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 7
            AddItemTypeAttributeToArray(ItemType.farmHeadStone07, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone07", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 8
            AddItemTypeAttributeToArray(ItemType.farmHeadStone08, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone08", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // HeadStone 9
            AddItemTypeAttributeToArray(ItemType.farmHeadStone09, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmHeadStone09", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 22);
            // TombStone 1
            AddItemTypeAttributeToArray(ItemType.farmTombStone01, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone01", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 2
            AddItemTypeAttributeToArray(ItemType.farmTombStone02, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone02", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 3
            AddItemTypeAttributeToArray(ItemType.farmTombStone03, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone03", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 4
            AddItemTypeAttributeToArray(ItemType.farmTombStone04, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone04", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 5
            AddItemTypeAttributeToArray(ItemType.farmTombStone05, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone05", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 6
            AddItemTypeAttributeToArray(ItemType.farmTombStone06, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone06", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 7
            AddItemTypeAttributeToArray(ItemType.farmTombStone07, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone07", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 8
            AddItemTypeAttributeToArray(ItemType.farmTombStone08, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone08", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // TombStone 9
            AddItemTypeAttributeToArray(ItemType.farmTombStone09, ModelType.InstanceModel, contentRTSPack + @"GraveYard\farmTombStone09", false, true,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // BarbWireCorner
            AddItemTypeAttributeToArray(ItemType.barbWireCorner, ModelType.XNAModel, contentRTSPack + @"Fences\barbWire\barbWireCorner", false, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // BarbWireStraight1
            AddItemTypeAttributeToArray(ItemType.barbWireStraight01, ModelType.XNAModel, contentRTSPack + @"Fences\barbWire\barbWireStraight1", false, false,
                                        false, 0, false, 0, 0, false, string.Empty, 0);
            // Stone Wall Corner1
            AddItemTypeAttributeToArray(ItemType.stoneWallCorner01, ModelType.XNAModel, contentRTSPack + @"Fences\stoneWall\stoneWallCorner1", false, false,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Stone Wall Straight1
            AddItemTypeAttributeToArray(ItemType.stoneWallStraight01, ModelType.XNAModel, contentRTSPack + @"Fences\stoneWall\stoneWallStraight01", false, false,
                                        true, 1, false, 0, 0, false, string.Empty, 0);
            // Test SceneItemOwner
            AddItemTypeAttributeToArray(ItemType.testItem, ModelType.XNAModel, contentRTSPack + @"Fences\stoneWall\stoneWallStraight1", false, false,
                                        true, 1, false, 0, 0, false, string.Empty, 0); 


            #endregion

            #region UrbanPackSet

            // Cargo Container
            AddItemTypeAttributeToArray(ItemType.urbanCargoContainer, ModelType.InstanceModel, contentUrbanPack + @"Cargo\urbanCargoContainer", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Wooden Crate
            AddItemTypeAttributeToArray(ItemType.urbanWoodenCrate, ModelType.InstanceModel, contentUrbanPack + @"Crate\urbanWoodenCrate", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Traffic Cone
            AddItemTypeAttributeToArray(ItemType.urbanTrafficCone, ModelType.InstanceModel, contentUrbanPack + @"Cone\urbanTrafficCone", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Chain Link Fence
            AddItemTypeAttributeToArray(ItemType.urbanChainLinkFence, ModelType.InstanceModel, contentUrbanPack + @"Fence\urbanChainLinkFence", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Barricade
            //AddItemTypeAttributeToArray(ItemType.urbanBarricade, ModelType.InstanceModel, @"ContentInstancedModels\Models\"  + @"Barricade\urbanBarricade", false, true,
                                       //false, 0, false, 0, 0, false, string.Empty);

            // Plastic Barrel
            AddItemTypeAttributeToArray(ItemType.urbanPlasticBarrel, ModelType.InstanceModel, contentUrbanPack + @"Barrel\urbanPlasticBarrel", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Cardboard Box
            AddItemTypeAttributeToArray(ItemType.urbanCardboardBox, ModelType.InstanceModel, contentUrbanPack + @"Box\urbanCardboardBox", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Cardboard Box#2
            AddItemTypeAttributeToArray(ItemType.urbanCardboardBox2, ModelType.InstanceModel, contentUrbanPack + @"Box\urbanCardboardBox2", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Wood Pallet
            AddItemTypeAttributeToArray(ItemType.urbanWoodPallet, ModelType.InstanceModel, contentUrbanPack + @"Pallet\urbanWoodPallet", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Wood Pallet#2
            AddItemTypeAttributeToArray(ItemType.urbanWoodPallet2, ModelType.InstanceModel, contentUrbanPack + @"Pallet\urbanWoodPallet2", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Dumpster
            AddItemTypeAttributeToArray(ItemType.urbanDumpster, ModelType.InstanceModel, contentUrbanPack + @"Dumpster\urbanDumpster", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Spare Tire
            AddItemTypeAttributeToArray(ItemType.urbanSpareTire, ModelType.InstanceModel, contentUrbanPack + @"Tire\urbanSpareTire", false, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            #endregion

            #region Sticks N Twiggs Set

            #region Trees

            //
            // Aspen Set
            //

            // Aspen 001
            AddItemTypeAttributeToArray(ItemType.treeAspen001, ModelType.InstanceModel, contentSticksNTwigg + @"AspenSet\treeAspen001", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);

            // Aspen 002
            AddItemTypeAttributeToArray(ItemType.treeAspen002, ModelType.InstanceModel, contentSticksNTwigg + @"AspenSet\treeAspen002", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Green Set
            //

            // Green 001
            AddItemTypeAttributeToArray(ItemType.treeGreen001, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen001", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Green 002
            AddItemTypeAttributeToArray(ItemType.treeGreen002, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen002", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Green 003
            AddItemTypeAttributeToArray(ItemType.treeGreen003, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen003", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Green 004
            AddItemTypeAttributeToArray(ItemType.treeGreen004, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen004", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Green 005
            AddItemTypeAttributeToArray(ItemType.treeGreen005, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen005", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Green 006
            AddItemTypeAttributeToArray(ItemType.treeGreen007, ModelType.InstanceModel, contentSticksNTwigg + @"GreenSet\treeGreen007", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Oak Set
            //

            // Oak 002
            AddItemTypeAttributeToArray(ItemType.treeOak002, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak002", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak 004
            AddItemTypeAttributeToArray(ItemType.treeOak004, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak004", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak 005
            AddItemTypeAttributeToArray(ItemType.treeOak005, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak005", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak 006
            AddItemTypeAttributeToArray(ItemType.treeOak006, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak006", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak 007
            AddItemTypeAttributeToArray(ItemType.treeOak007, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak007", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Oak 008
            AddItemTypeAttributeToArray(ItemType.treeOak008, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOak008", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);

            // Old Oak 001
            AddItemTypeAttributeToArray(ItemType.treeOldOak001, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOldOak001", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Old Oak 002
            AddItemTypeAttributeToArray(ItemType.treeOldOak002, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOldOak002", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Old Oak 003
            AddItemTypeAttributeToArray(ItemType.treeOldOak003, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOldOak003", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Old Oak 004
            AddItemTypeAttributeToArray(ItemType.treeOldOak004, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOldOak004", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);
            // Old Oak 005
            AddItemTypeAttributeToArray(ItemType.treeOldOak005, ModelType.InstanceModel, contentSticksNTwigg + @"OakSet\treeOldOak005", true, true,
                                      false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Palm Set
            //

            // Palm 002
            AddItemTypeAttributeToArray(ItemType.treePalm002, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalm002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm 003
            AddItemTypeAttributeToArray(ItemType.treePalm003, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalm003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm 004
            AddItemTypeAttributeToArray(ItemType.treePalm004, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalm004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm 005
            AddItemTypeAttributeToArray(ItemType.treePalm005, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalm005", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm 006
            AddItemTypeAttributeToArray(ItemType.treePalm006, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalm006", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm New 001
            AddItemTypeAttributeToArray(ItemType.treePalmNew001, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalmNew001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm New 002a
            AddItemTypeAttributeToArray(ItemType.treePalmNew002a, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalmNew002a", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm New 002b
            AddItemTypeAttributeToArray(ItemType.treePalmNew002b, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalmNew002b", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm New 002c
            AddItemTypeAttributeToArray(ItemType.treePalmNew002c, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalmNew002c", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Palm New 003
            AddItemTypeAttributeToArray(ItemType.treePalmNew003, ModelType.InstanceModel, contentSticksNTwigg + @"PalmSet\treePalmNew003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Pine Set
            //

            // Pine 001
            AddItemTypeAttributeToArray(ItemType.treePine001, ModelType.InstanceModel, contentSticksNTwigg + @"PineSet\treePine001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Pine 002
            AddItemTypeAttributeToArray(ItemType.treePine002, ModelType.InstanceModel, contentSticksNTwigg + @"PineSet\treePine002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Pine 003
            AddItemTypeAttributeToArray(ItemType.treePine003, ModelType.InstanceModel, contentSticksNTwigg + @"PineSet\treePine003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Pine 004
            AddItemTypeAttributeToArray(ItemType.treePine004, ModelType.InstanceModel, contentSticksNTwigg + @"PineSet\treePine004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Pine 005
            AddItemTypeAttributeToArray(ItemType.treePine005, ModelType.InstanceModel, contentSticksNTwigg + @"PineSet\treePine005", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // White Set
            //

            // White 001
            AddItemTypeAttributeToArray(ItemType.treeWhite001, ModelType.InstanceModel, contentSticksNTwigg + @"WhiteSet\treeWhite001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
          
            // White 002
            AddItemTypeAttributeToArray(ItemType.treeWhite002, ModelType.InstanceModel, contentSticksNTwigg + @"WhiteSet\treeWhite002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            
            // White 003
            AddItemTypeAttributeToArray(ItemType.treeWhite003, ModelType.InstanceModel, contentSticksNTwigg + @"WhiteSet\treeWhite003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // White 004
            AddItemTypeAttributeToArray(ItemType.treeWhite004, ModelType.InstanceModel, contentSticksNTwigg + @"WhiteSet\treeWhite004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // White 005
            AddItemTypeAttributeToArray(ItemType.treeWhite005, ModelType.InstanceModel, contentSticksNTwigg + @"WhiteSet\treeWhite005", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Jungle Set
            //

            // Jungle 003
            AddItemTypeAttributeToArray(ItemType.treeJungle003, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Jungle 004
            AddItemTypeAttributeToArray(ItemType.treeJungle004, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Jungle 005
            AddItemTypeAttributeToArray(ItemType.treeJungle005, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle005", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Jungle 006
            AddItemTypeAttributeToArray(ItemType.treeJungle006, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle006", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Jungle 007
            AddItemTypeAttributeToArray(ItemType.treeJungle007, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle007", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Jungle 008
            AddItemTypeAttributeToArray(ItemType.treeJungle008, ModelType.InstanceModel, contentSticksNTwigg + @"JungleSet\treeJungle008", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            //
            // Grn Arch Set
            //

            // GrnArch 001
            AddItemTypeAttributeToArray(ItemType.treeGrnArch001, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 002
            AddItemTypeAttributeToArray(ItemType.treeGrnArch002, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 003
            AddItemTypeAttributeToArray(ItemType.treeGrnArch003, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 004
            AddItemTypeAttributeToArray(ItemType.treeGrnArch004, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 007
            AddItemTypeAttributeToArray(ItemType.treeGrnArch007, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch007", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 008
            AddItemTypeAttributeToArray(ItemType.treeGrnArch008, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch008", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnArch 009
            AddItemTypeAttributeToArray(ItemType.treeGrnArch009, ModelType.InstanceModel, contentSticksNTwigg + @"GrnArchSet\treeGrnArch009", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // DeadSplt 001
            AddItemTypeAttributeToArray(ItemType.treeDeadSplt001, ModelType.InstanceModel, contentSticksNTwigg + @"DeadSpltSet\treeDeadSplt001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // DeadSplt 002
            AddItemTypeAttributeToArray(ItemType.treeDeadSplt002, ModelType.InstanceModel, contentSticksNTwigg + @"DeadSpltSet\treeDeadSplt002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnSplt 001
            AddItemTypeAttributeToArray(ItemType.treeGrnSplt001, ModelType.InstanceModel, contentSticksNTwigg + @"GrnSpltSet\treeGrnSplt001", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnSplt 002
            AddItemTypeAttributeToArray(ItemType.treeGrnSplt002, ModelType.InstanceModel, contentSticksNTwigg + @"GrnSpltSet\treeGrnSplt002", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // GrnSplt 003
            AddItemTypeAttributeToArray(ItemType.treeGrnSplt003, ModelType.InstanceModel, contentSticksNTwigg + @"GrnSpltSet\treeGrnSplt003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            #endregion

            #region Plants

            // Cactus 001
            AddItemTypeAttributeToArray(ItemType.plantsCactus001, ModelType.InstanceModel, contentSticksNTwigg + @"Cactus\plantsCactus001", false, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Weeds 001
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds001, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Weeds 002
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds002, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Weeds 003
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds003, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds003", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Weeds 004
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds004, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds004", true, true,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Weeds 005
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds005, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds005", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Weeds 006
            AddItemTypeAttributeToArray(ItemType.plantsNewWeeds006, ModelType.InstanceModel, contentSticksNTwigg + @"Weeds\plantsNewWeeds006", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Bush 001
            AddItemTypeAttributeToArray(ItemType.plantsNewBushes001, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsNewBushes001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Bush 002
            AddItemTypeAttributeToArray(ItemType.plantsNewBushes002, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsNewBushes002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Bush 003
            AddItemTypeAttributeToArray(ItemType.plantsNewBushes003, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsNewBushes003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Bush 004
            AddItemTypeAttributeToArray(ItemType.plantsNewBushes004, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsNewBushes004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Brown Bush 001
            AddItemTypeAttributeToArray(ItemType.plantsBushesBrwn001, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsBushesBrwn001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Brown Bush 002
            AddItemTypeAttributeToArray(ItemType.plantsBushesBrwn002, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsBushesBrwn002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Brown Bush 003
            AddItemTypeAttributeToArray(ItemType.plantsBushesBrwn003, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsBushesBrwn003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Green Bush 001
            AddItemTypeAttributeToArray(ItemType.plantsBushesGrn001, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsBushesGrn001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Green Bush 002
            AddItemTypeAttributeToArray(ItemType.plantsBushesGrn002, ModelType.InstanceModel, contentSticksNTwigg + @"Bushes\plantsBushesGrn002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Fern1 001
            AddItemTypeAttributeToArray(ItemType.plantsFern1_001, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern1_001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern1 002
            AddItemTypeAttributeToArray(ItemType.plantsFern1_002, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern1_002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern1 003
            AddItemTypeAttributeToArray(ItemType.plantsFern1_003, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern1_003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Fern2 001
            AddItemTypeAttributeToArray(ItemType.plantsFern2_001, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern2_001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern2 002
            AddItemTypeAttributeToArray(ItemType.plantsFern2_002, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern2_002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern2 003
            AddItemTypeAttributeToArray(ItemType.plantsFern2_003, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern2_003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Fern3 001
            AddItemTypeAttributeToArray(ItemType.plantsFern3_001, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern3_001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern3 002
            AddItemTypeAttributeToArray(ItemType.plantsFern3_002, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern3_002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Fern3 003
            AddItemTypeAttributeToArray(ItemType.plantsFern3_003, ModelType.InstanceModel, contentSticksNTwigg + @"Ferns\plantsFern3_003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // Grass 001
            AddItemTypeAttributeToArray(ItemType.plantsGrass001, ModelType.InstanceModel, contentSticksNTwigg + @"Grass\plantsGrass001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Twig 001
            AddItemTypeAttributeToArray(ItemType.plantsTwig001, ModelType.InstanceModel, contentSticksNTwigg + @"Twigs\plantsTwig001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Twig 002
            AddItemTypeAttributeToArray(ItemType.plantsTwig002, ModelType.InstanceModel, contentSticksNTwigg + @"Twigs\plantsTwig002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
           

            #endregion

            #region Rocks

            // Asst Rocks 001
            AddItemTypeAttributeToArray(ItemType.asstRocks001, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 001a
            AddItemTypeAttributeToArray(ItemType.asstRocks001a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks001a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 002
            AddItemTypeAttributeToArray(ItemType.asstRocks002, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 00a
            AddItemTypeAttributeToArray(ItemType.asstRocks002a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks002a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 003
            AddItemTypeAttributeToArray(ItemType.asstRocks003, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 003a
            AddItemTypeAttributeToArray(ItemType.asstRocks003a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks003a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 004a
            AddItemTypeAttributeToArray(ItemType.asstRocks004a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks004a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 005a
            AddItemTypeAttributeToArray(ItemType.asstRocks005a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks005a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks 006a
            AddItemTypeAttributeToArray(ItemType.asstRocks006a, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocks006a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks Flat Bottom 001
            AddItemTypeAttributeToArray(ItemType.asstRocksFlatBottom001, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocksFlatBottom001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks Round 001
            AddItemTypeAttributeToArray(ItemType.asstRocksRound001, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocksRound001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks Round 002
            AddItemTypeAttributeToArray(ItemType.asstRocksRound002, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocksRound002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks Round 003
            AddItemTypeAttributeToArray(ItemType.asstRocksRound003, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocksRound003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // Asst Rocks Round 004
            AddItemTypeAttributeToArray(ItemType.asstRocksRound004, ModelType.InstanceModel, contentSticksNTwigg + @"AsstRocks\asstRocksRound004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            #endregion

            #endregion

            #region AlleyPack Set

            // barrel003
            AddItemTypeAttributeToArray(ItemType.barrel003, ModelType.InstanceModel, alleypack + @"Barrels\barrel003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // barrel004
            AddItemTypeAttributeToArray(ItemType.barrel004, ModelType.InstanceModel, alleypack + @"Barrels\barrel004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // barrel005
            AddItemTypeAttributeToArray(ItemType.barrel005, ModelType.InstanceModel, alleypack + @"Barrels\barrel005", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // barrel006
            AddItemTypeAttributeToArray(ItemType.barrel006, ModelType.InstanceModel, alleypack + @"Barrels\barrel006", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // barrel009
            AddItemTypeAttributeToArray(ItemType.barrel009, ModelType.InstanceModel, alleypack + @"Barrels\barrel009", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cardboardBOX
            AddItemTypeAttributeToArray(ItemType.cardboardBOX, ModelType.InstanceModel, alleypack + @"CardboardBoxes\cardboardBOX", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cardboardBoxSmallFragile
            AddItemTypeAttributeToArray(ItemType.cardboardBoxSmallFragile, ModelType.InstanceModel, alleypack + @"CardboardBoxes\cardboardBoxSmallFragile", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cardboardBOXdamaged
            AddItemTypeAttributeToArray(ItemType.cardboardBOXdamaged, ModelType.InstanceModel, alleypack + @"CardboardBoxes\cardboardBOXdamaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cinderBlockPile
            AddItemTypeAttributeToArray(ItemType.cinderBlockPile, ModelType.InstanceModel, alleypack + @"CinderBlocks\cinderBlockPile", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cinderBlockQuad
            AddItemTypeAttributeToArray(ItemType.cinderBlockQuad, ModelType.InstanceModel, alleypack + @"CinderBlocks\cinderBlockQuad", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cinderBlockSingle
            AddItemTypeAttributeToArray(ItemType.cinderBlockSingle, ModelType.InstanceModel, alleypack + @"CinderBlocks\cinderBlockSingle", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cinderBlockWall
            AddItemTypeAttributeToArray(ItemType.cinderBlockWall, ModelType.InstanceModel, alleypack + @"CinderBlocks\cinderBlockWall", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // crateFragile0051
            AddItemTypeAttributeToArray(ItemType.crateFragile0051, ModelType.InstanceModel, alleypack + @"Crates\crateFragile0051", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // crateFragileDMGED
            AddItemTypeAttributeToArray(ItemType.crateFragileDMGED, ModelType.InstanceModel, alleypack + @"Crates\crateFragileDMGED", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlain009
            AddItemTypeAttributeToArray(ItemType.cratePlain009, ModelType.InstanceModel, alleypack + @"Crates\cratePlain009", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainDMGED
            AddItemTypeAttributeToArray(ItemType.cratePlainDMGED, ModelType.InstanceModel, alleypack + @"Crates\cratePlainDMGED", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainDMGED034
            AddItemTypeAttributeToArray(ItemType.cratePlainDMGED034, ModelType.InstanceModel, alleypack + @"Crates\cratePlainDMGED034", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainDMGED035
            AddItemTypeAttributeToArray(ItemType.cratePlainDMGED035, ModelType.InstanceModel, alleypack + @"Crates\cratePlainDMGED035", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainDMGED2
            AddItemTypeAttributeToArray(ItemType.cratePlainDMGED2, ModelType.InstanceModel, alleypack + @"Crates\cratePlainDMGED2", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainPOTATOES
            AddItemTypeAttributeToArray(ItemType.cratePlainPOTATOES, ModelType.InstanceModel, alleypack + @"Crates\cratePlainPOTATOES", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cratePlainSLATS
            AddItemTypeAttributeToArray(ItemType.cratePlainSLATS, ModelType.InstanceModel, alleypack + @"Crates\cratePlainSLATS", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // fireEscape01
            AddItemTypeAttributeToArray(ItemType.fireEscape01, ModelType.InstanceModel, alleypack + @"FireEscapes\fireEscape01", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // fireEscape01down
            AddItemTypeAttributeToArray(ItemType.fireEscape01down, ModelType.InstanceModel, alleypack + @"FireEscapes\fireEscape01down", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // fireEscape01upper
            AddItemTypeAttributeToArray(ItemType.fireEscape01upper, ModelType.InstanceModel, alleypack + @"FireEscapes\fireEscape01upper", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // fireHydrantRed
            AddItemTypeAttributeToArray(ItemType.fireHydrantRed, ModelType.InstanceModel, alleypack + @"FireHydrants\fireHydrantRed", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // fireHydrantYellow01
            AddItemTypeAttributeToArray(ItemType.fireHydrantYellow01, ModelType.InstanceModel, alleypack + @"FireHydrants\fireHydrantYellow01", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // mailBoxBlue001
            AddItemTypeAttributeToArray(ItemType.mailBoxBlue001, ModelType.InstanceModel, alleypack + @"PostalMailBox\mailBoxBlue001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // plywoodSheet1
            AddItemTypeAttributeToArray(ItemType.plywoodSheet1, ModelType.InstanceModel, alleypack + @"PlyWoods\plywoodSheet1", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // plywoodSheet2leaning
            AddItemTypeAttributeToArray(ItemType.plywoodSheet2leaning, ModelType.InstanceModel, alleypack + @"PlyWoods\plywoodSheet2leaning", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // plywoodSheet3
            AddItemTypeAttributeToArray(ItemType.plywoodSheet3, ModelType.InstanceModel, alleypack + @"PlyWoods\plywoodSheet3", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // busStopBench001
            AddItemTypeAttributeToArray(ItemType.busStopBench001, ModelType.InstanceModel, alleypack + @"BusStopBench\busStopBench001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // busStopBenchAd001
            AddItemTypeAttributeToArray(ItemType.busStopBenchAd001, ModelType.InstanceModel, alleypack + @"BusStopBench\busStopBenchAd001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_20MPH
            AddItemTypeAttributeToArray(ItemType.sign_20MPH, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_20MPH", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_25MPH
            AddItemTypeAttributeToArray(ItemType.sign_25MPH, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_25MPH", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_DEADEND
            AddItemTypeAttributeToArray(ItemType.sign_DEADEND, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_DEADEND", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_DONOTENTER
            AddItemTypeAttributeToArray(ItemType.sign_DONOTENTER, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_DONOTENTER", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_INTERSECT
            AddItemTypeAttributeToArray(ItemType.sign_INTERSECT, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_INTERSECT", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_LEFT
            AddItemTypeAttributeToArray(ItemType.sign_LEFT, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_LEFT", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOOUTLET
            AddItemTypeAttributeToArray(ItemType.sign_NOOUTLET, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOOUTLET", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOPARKING
            AddItemTypeAttributeToArray(ItemType.sign_NOPARKING, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOPARKING", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOPARKING2
            AddItemTypeAttributeToArray(ItemType.sign_NOPARKING2, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOPARKING2", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOPARKING3
            AddItemTypeAttributeToArray(ItemType.sign_NOPARKING3, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOPARKING3", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOPARKING4
            AddItemTypeAttributeToArray(ItemType.sign_NOPARKING4, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOPARKING4", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_NOUTURN
            AddItemTypeAttributeToArray(ItemType.sign_NOUTURN, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_NOUTURN", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_PEDX
            AddItemTypeAttributeToArray(ItemType.sign_PEDX, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_PEDX", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_RIGHT
            AddItemTypeAttributeToArray(ItemType.sign_RIGHT, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_RIGHT", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_RR
            AddItemTypeAttributeToArray(ItemType.sign_RR, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_RR", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sign_STOP
            AddItemTypeAttributeToArray(ItemType.sign_STOP, ModelType.InstanceModel, alleypack + @"StreetSigns\sign_STOP", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tireSIDE
            AddItemTypeAttributeToArray(ItemType.tireSIDE, ModelType.InstanceModel, alleypack + @"Tires\tireSIDE", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tireSIDEx3
            AddItemTypeAttributeToArray(ItemType.tireSIDEx3, ModelType.InstanceModel, alleypack + @"Tires\tireSIDEx3", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // trashCan
            AddItemTypeAttributeToArray(ItemType.trashCan, ModelType.InstanceModel, alleypack + @"Trashcan\trashCan", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // TVantenna001
            AddItemTypeAttributeToArray(ItemType.TVantenna001, ModelType.InstanceModel, alleypack + @"TVAntennas\TVantenna001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // TVantenna002
            AddItemTypeAttributeToArray(ItemType.TVantenna002, ModelType.InstanceModel, alleypack + @"TVAntennas\TVantenna002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // TVantenna003
            AddItemTypeAttributeToArray(ItemType.TVantenna003, ModelType.InstanceModel, alleypack + @"TVAntennas\TVantenna003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // TVantenna003a
            AddItemTypeAttributeToArray(ItemType.TVantenna003a, ModelType.InstanceModel, alleypack + @"TVAntennas\TVantenna003a", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // wood2x4
            AddItemTypeAttributeToArray(ItemType.wood2x4, ModelType.InstanceModel, alleypack + @"Wood\wood2x4", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // wood2x4strewn
            AddItemTypeAttributeToArray(ItemType.wood2x4strewn, ModelType.InstanceModel, alleypack + @"Wood\wood2x4strewn", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPalette1
            AddItemTypeAttributeToArray(ItemType.woodPalette1, ModelType.InstanceModel, alleypack + @"Wood\woodPalette1", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPalette2
            AddItemTypeAttributeToArray(ItemType.woodPalette2, ModelType.InstanceModel, alleypack + @"Wood\woodPalette2", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPalette3
            AddItemTypeAttributeToArray(ItemType.woodPalette3, ModelType.InstanceModel, alleypack + @"Wood\woodPalette3", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPalette4_broken
            AddItemTypeAttributeToArray(ItemType.woodPalette4_broken, ModelType.InstanceModel, alleypack + @"Wood\woodPalette4_broken", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPlankSingle
            AddItemTypeAttributeToArray(ItemType.woodPlankSingle, ModelType.InstanceModel, alleypack + @"Wood\woodPlankSingle", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPlankTriple_leaning
            AddItemTypeAttributeToArray(ItemType.woodPlankTriple_leaning, ModelType.InstanceModel, alleypack + @"Wood\woodPlankTriple_leaning", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPlankTriple2_leaning
            AddItemTypeAttributeToArray(ItemType.woodPlankTriple2_leaning, ModelType.InstanceModel, alleypack + @"Wood\woodPlankTriple2_leaning", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // woodPlankTriple2b_leaning
            AddItemTypeAttributeToArray(ItemType.woodPlankTriple2b_leaning, ModelType.InstanceModel, alleypack + @"Wood\woodPlankTriple2b_leaning", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            #endregion

            #region DowntownDistrict Set
           

            // cgBuilding0035
            AddItemTypeAttributeToArray(ItemType.cgBuilding0035, ModelType.InstanceModel, downtowndistrictpack + @"cgBuildings\cgBuilding0035", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            // cgBuilding0050
            AddItemTypeAttributeToArray(ItemType.cgBuilding0050, ModelType.InstanceModel, downtowndistrictpack + @"cgBuildings\cgBuilding0050", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgRedBrick40sGarage
            AddItemTypeAttributeToArray(ItemType.cgRedBrick40sGarage, ModelType.InstanceModel, downtowndistrictpack + @"cgRedBrick40sGarage\cgRedBrick40sGarage", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgTrashBinRusted001
            AddItemTypeAttributeToArray(ItemType.cgTrashBinRusted001, ModelType.InstanceModel, downtowndistrictpack + @"cgTrashBinRusted001\cgTrashBinRusted001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgTrashBinRusted002wTrash
            AddItemTypeAttributeToArray(ItemType.cgTrashBinRusted002wTrash, ModelType.InstanceModel, downtowndistrictpack + @"cgTrashBinRusted002wTrash\cgTrashBinRusted002wTrash", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgTrashBinRusted003wTrashFull
            AddItemTypeAttributeToArray(ItemType.cgTrashBinRusted003wTrashFull, ModelType.InstanceModel, downtowndistrictpack + @"cgTrashBinRusted003wTrashFull\cgTrashBinRusted003wTrashFull", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cityBlock001
            AddItemTypeAttributeToArray(ItemType.cityBlock001, ModelType.InstanceModel, downtowndistrictpack + @"cityBlock001\cityBlock001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            // plazaHotel001AthreeStory
            AddItemTypeAttributeToArray(ItemType.plazaHotel001AthreeStory, ModelType.InstanceModel, downtowndistrictpack + @"plazaHotel001AthreeStory\plazaHotel001AthreeStory", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // plazaHotel002A
            AddItemTypeAttributeToArray(ItemType.plazaHotel002A, ModelType.InstanceModel, downtowndistrictpack + @"plazaHotel002A\plazaHotel002A", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // plazaHotelSign001
            AddItemTypeAttributeToArray(ItemType.plazaHotelSign001, ModelType.InstanceModel, downtowndistrictpack + @"plazaHotelSign001\plazaHotelSign001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // sidewalkSectionL
            AddItemTypeAttributeToArray(ItemType.sidewalkSectionL, ModelType.InstanceModel, downtowndistrictpack + @"sidewalkSections\sidewalkSectionL", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);
            // sidewalkSectionLshort
            AddItemTypeAttributeToArray(ItemType.sidewalkSectionLshort, ModelType.InstanceModel, downtowndistrictpack + @"sidewalkSections\sidewalkSectionLshort", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // signAdultCinema001
            AddItemTypeAttributeToArray(ItemType.signAdultCinema001, ModelType.InstanceModel, downtowndistrictpack + @"signAdultCinema001\signAdultCinema001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // stopSignal001
            AddItemTypeAttributeToArray(ItemType.stopSignal001, ModelType.InstanceModel, downtowndistrictpack + @"stopSignal001\stopSignal001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetSidewalk001
            AddItemTypeAttributeToArray(ItemType.streetSidewalk001, ModelType.InstanceModel, downtowndistrictpack + @"streetSidewalk001\streetSidewalk001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetSidewalk001damaged
            AddItemTypeAttributeToArray(ItemType.streetSidewalk001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetSidewalk001\streetSidewalk001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetBrokenMiddle001damaged1
            AddItemTypeAttributeToArray(ItemType.streetBrokenMiddle001damaged1, ModelType.InstanceModel, downtowndistrictpack + @"streetBrokenMiddle\streetBrokenMiddle001damaged1", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetCrosswalk001
            AddItemTypeAttributeToArray(ItemType.streetCrosswalk001, ModelType.InstanceModel, downtowndistrictpack + @"streetCrosswalk001\streetCrosswalk001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetCrosswalk001damaged
            AddItemTypeAttributeToArray(ItemType.streetCrosswalk001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetCrosswalk001\streetCrosswalk001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetCuldesac001
            AddItemTypeAttributeToArray(ItemType.streetCuldesac001, ModelType.InstanceModel, downtowndistrictpack + @"streetCuldesac001\streetCuldesac001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetCuldesac001damaged
            AddItemTypeAttributeToArray(ItemType.streetCuldesac001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetCuldesac001\streetCuldesac001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetEndBroken001damaged
            AddItemTypeAttributeToArray(ItemType.streetEndBroken001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetEndBroken\streetEndBroken001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetIntersection4W
            AddItemTypeAttributeToArray(ItemType.streetIntersection4W, ModelType.InstanceModel, downtowndistrictpack + @"streetIntersection4W\streetIntersection4W", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetIntersection4Wdamaged
            AddItemTypeAttributeToArray(ItemType.streetIntersection4Wdamaged, ModelType.InstanceModel, downtowndistrictpack + @"streetIntersection4W\streetIntersection4Wdamaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetLight001
            AddItemTypeAttributeToArray(ItemType.streetLight001, ModelType.InstanceModel, downtowndistrictpack + @"streetLight001\streetLight001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetLongSec001
            AddItemTypeAttributeToArray(ItemType.streetLongSec001, ModelType.InstanceModel, downtowndistrictpack + @"streetLongSec001\streetLongSec001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetLongSecSidewalk001
            AddItemTypeAttributeToArray(ItemType.streetLongSecSidewalk001, ModelType.InstanceModel, downtowndistrictpack + @"streetLongSec001\streetLongSecSidewalk001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetLongSecSidewalk001damaged
            AddItemTypeAttributeToArray(ItemType.streetLongSecSidewalk001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetLongSec001\streetLongSecSidewalk001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetThreeWayInt001
            AddItemTypeAttributeToArray(ItemType.streetThreeWayInt001, ModelType.InstanceModel, downtowndistrictpack + @"streetThreeWayInt001\streetThreeWayInt001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // streetThreeWayInt001damaged
            AddItemTypeAttributeToArray(ItemType.streetThreeWayInt001damaged, ModelType.InstanceModel, downtowndistrictpack + @"streetThreeWayInt001\streetThreeWayInt001damaged", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmApartmentSmall001
            AddItemTypeAttributeToArray(ItemType.tmApartmentSmall001, ModelType.InstanceModel, downtowndistrictpack + @"tmApartmentSmall001\tmApartmentSmall001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmApartmentSmall2x001
            AddItemTypeAttributeToArray(ItemType.tmApartmentSmall2x001, ModelType.InstanceModel, downtowndistrictpack + @"tmApartmentSmall001\tmApartmentSmall2x001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmBuilding001
            AddItemTypeAttributeToArray(ItemType.tmBuilding001, ModelType.InstanceModel, downtowndistrictpack + @"tmBuildings\tmBuilding001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmBuilding001storeFront001
            AddItemTypeAttributeToArray(ItemType.tmBuilding001storeFront001, ModelType.InstanceModel, downtowndistrictpack + @"tmBuildings\tmBuilding001storeFront001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmBuilding002A
            AddItemTypeAttributeToArray(ItemType.tmBuilding002A, ModelType.InstanceModel, downtowndistrictpack + @"tmBuildings\tmBuilding002A", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tmBuilding003Atwin2cooling
            AddItemTypeAttributeToArray(ItemType.tmBuilding003Atwin2cooling, ModelType.InstanceModel, downtowndistrictpack + @"tmBuildings\tmBuilding003Atwin2cooling", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            // tmMuseum001
            AddItemTypeAttributeToArray(ItemType.tmMuseum001, ModelType.InstanceModel, downtowndistrictpack + @"tmBuildings\tmMuseum001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);




            #endregion

            #region WarehouseDistrict Set

            // alleyWallBrick001
            AddItemTypeAttributeToArray(ItemType.alleyWallBrick001, ModelType.InstanceModel, warehousedistrictpack + @"AlleyWalls\alleyWallBrick001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // alleyWallBrick002
            AddItemTypeAttributeToArray(ItemType.alleyWallBrick002, ModelType.InstanceModel, warehousedistrictpack + @"AlleyWalls\alleyWallBrick002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // alleyWallConcrete001
            AddItemTypeAttributeToArray(ItemType.alleyWallConcrete001, ModelType.InstanceModel, warehousedistrictpack + @"AlleyWalls\alleyWallConcrete001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // alleyWallConcrete002
            AddItemTypeAttributeToArray(ItemType.alleyWallConcrete002, ModelType.InstanceModel, warehousedistrictpack + @"AlleyWalls\alleyWallConcrete002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // alleyWallConcrete003graf
            AddItemTypeAttributeToArray(ItemType.alleyWallConcrete003graf, ModelType.InstanceModel, warehousedistrictpack + @"AlleyWalls\alleyWallConcrete003graf", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // AutoGarage
            AddItemTypeAttributeToArray(ItemType.autoGarage001, ModelType.InstanceModel, warehousedistrictpack + @"AutoGarage\autoGarage001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerT001
            AddItemTypeAttributeToArray(ItemType.cargoContainerT001, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerT002
            AddItemTypeAttributeToArray(ItemType.cargoContainerT002, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);


            // cargoContainerT003
            AddItemTypeAttributeToArray(ItemType.cargoContainerT003, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerT004
            AddItemTypeAttributeToArray(ItemType.cargoContainerT004, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerT005
            AddItemTypeAttributeToArray(ItemType.cargoContainerT005, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT005", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerT006
            AddItemTypeAttributeToArray(ItemType.cargoContainerT006, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerT006", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerTopen001
            AddItemTypeAttributeToArray(ItemType.cargoContainerTopen001, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerTopen001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerTopen002Blue
            AddItemTypeAttributeToArray(ItemType.cargoContainerTopen002Blue, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerTopen002Blue", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerTopen002dirty1
            AddItemTypeAttributeToArray(ItemType.cargoContainerTopen002dirty1, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerTopen002dirty1", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cargoContainerTopen002Org
            AddItemTypeAttributeToArray(ItemType.cargoContainerTopen002Org, ModelType.InstanceModel, warehousedistrictpack + @"CargoContainers\cargoContainerTopen002Org", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgBuildingRuin002
            AddItemTypeAttributeToArray(ItemType.cgBuildingRuin002, ModelType.InstanceModel, warehousedistrictpack + @"CgBuildings\cgBuildingRuin002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgBuildingRuin002twin
            AddItemTypeAttributeToArray(ItemType.cgBuildingRuin002twin, ModelType.InstanceModel, warehousedistrictpack + @"CgBuildings\cgBuildingRuin002twin", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgWarehouse002story
            AddItemTypeAttributeToArray(ItemType.cgWarehouse002story, ModelType.InstanceModel, warehousedistrictpack + @"CgBuildings\cgWarehouse002story", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgWarehouse001enclosed
            AddItemTypeAttributeToArray(ItemType.cgWarehouse001enclosed, ModelType.InstanceModel, warehousedistrictpack + @"CgBuildings\cgWarehouse001enclosed", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // concreteBarricade001
            AddItemTypeAttributeToArray(ItemType.concreteBarricade001, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\concreteBarricade001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // concreteBarricade003
            AddItemTypeAttributeToArray(ItemType.concreteBarricade003, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\concreteBarricade003", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // concreteBarricade004
            AddItemTypeAttributeToArray(ItemType.concreteBarricade004, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\concreteBarricade004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // concreteBarricadeV2_001warning
            AddItemTypeAttributeToArray(ItemType.concreteBarricadeV2_001warning, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\concreteBarricadeV2_001warning", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // concreteBarricadeWwarning001
            AddItemTypeAttributeToArray(ItemType.concreteBarricadeWwarning001, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\concreteBarricadeWwarning001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // cgConcreteWarehouse001
            AddItemTypeAttributeToArray(ItemType.cgConcreteWarehouse001, ModelType.InstanceModel, warehousedistrictpack + @"CgBuildings\cgConcreteWarehouse001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // garageSign001
            AddItemTypeAttributeToArray(ItemType.garageSign001, ModelType.InstanceModel, warehousedistrictpack + @"Signs\garageSign001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // signTattoo001
            AddItemTypeAttributeToArray(ItemType.signTattoo001, ModelType.InstanceModel, warehousedistrictpack + @"Signs\signTattoo001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // grafittiBarricade001
            AddItemTypeAttributeToArray(ItemType.grafittiBarricade001, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\grafittiBarricade001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // grafittiBarricade002
            AddItemTypeAttributeToArray(ItemType.grafittiBarricade002, ModelType.InstanceModel, warehousedistrictpack + @"Barricades\grafittiBarricade002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // oldWarehouse001
            AddItemTypeAttributeToArray(ItemType.oldWarehouse001, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\oldWarehouse001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // oldWarehouse001worn
            AddItemTypeAttributeToArray(ItemType.oldWarehouse001worn, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\oldWarehouse001worn", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // oldWarehouse001worn2
            AddItemTypeAttributeToArray(ItemType.oldWarehouse001worn2, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\oldWarehouse001worn2", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // oldWarehouse004
            AddItemTypeAttributeToArray(ItemType.oldWarehouse004, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\oldWarehouse004", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // rustedWarehouse001
            AddItemTypeAttributeToArray(ItemType.rustedWarehouse001, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\rustedWarehouse001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tinBuilding001
            AddItemTypeAttributeToArray(ItemType.tinBuilding001, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\tinBuilding001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tinBuilding002
            AddItemTypeAttributeToArray(ItemType.tinBuilding002, ModelType.InstanceModel, warehousedistrictpack + @"OldWarehouses\tinBuilding002", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tractorTrailer001
            AddItemTypeAttributeToArray(ItemType.tractorTrailer001, ModelType.InstanceModel, warehousedistrictpack + @"TractorTrailers\tractorTrailer001", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tractorTrailer001blue
            AddItemTypeAttributeToArray(ItemType.tractorTrailer001blue, ModelType.InstanceModel, warehousedistrictpack + @"TractorTrailers\tractorTrailer001blue", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tractorTrailer001dirty01
            AddItemTypeAttributeToArray(ItemType.tractorTrailer001dirty01, ModelType.InstanceModel, warehousedistrictpack + @"TractorTrailers\tractorTrailer001dirty01", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);

            // tractorTrailer001dirty03
            AddItemTypeAttributeToArray(ItemType.tractorTrailer001dirty03, ModelType.InstanceModel, warehousedistrictpack + @"TractorTrailers\tractorTrailer001dirty03", true, false,
                                       false, 0, false, 0, 0, false, string.Empty, 0);



            #endregion

            #region SciFiBuildingSet_1

            // SciFi Building 1
            AddItemTypeAttributeToArray(ItemType.sciFiBlda01, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda01", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 2
            AddItemTypeAttributeToArray(ItemType.sciFiBlda02, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda02", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 3
            AddItemTypeAttributeToArray(ItemType.sciFiBlda03, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda03", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 4
            AddItemTypeAttributeToArray(ItemType.sciFiBlda04, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda04", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 5
            AddItemTypeAttributeToArray(ItemType.sciFiBlda05, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda05", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 6
            AddItemTypeAttributeToArray(ItemType.sciFiBlda06, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda06", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 7
            AddItemTypeAttributeToArray(ItemType.sciFiBlda07, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda07", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 8
            AddItemTypeAttributeToArray(ItemType.sciFiBlda08, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda08", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 9
            AddItemTypeAttributeToArray(ItemType.sciFiBlda09, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda09", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 10
            AddItemTypeAttributeToArray(ItemType.sciFiBlda10, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda10", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);

            // SciFi Building 11
            AddItemTypeAttributeToArray(ItemType.sciFiBlda11, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings1\sciFiBlda11", false, true,
                                        true, 1, true, 60, 60, false, string.Empty, 0);


            #endregion

            #region SciFiBuildingSet_2

            // SciFi Building 1 (Side-2 WarFactory)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb01, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb01", false, true,
                                        true, 2, true, 60, 60, true, "RadarDish", 0);

            // SciFi Building 2 (Side-1 TechCenter)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb02, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb02", false, true,
                                        true, 1, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 3 (Not Used)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb03, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb03", false, true,
                                        true, 1, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 4 (Side-2 TechCenter)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb04, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb04", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 5 (Side-2 PowerPlant)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb05, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb05", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 6 (Not Used)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb06, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb06", false, true,
                                        true, 1, true, 60, 60, true, "RadarDish", 0);

            // SciFi Building 7 (Side-2 Refinery)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb07, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb07", false, true,
                                        true, 2, true, 60, 60, true, "Wheel", 0);

            // SciFi Building 8 (Not Used)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb08, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb08", false, true,
                                        true, 1, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 9 (Side-1 PowerPlant)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb09, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb09", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 10 (Side-2 Airport)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb10, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb10", false, true,
                                        true, 1, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 11 (Side-1 WarFactory)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb11, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb11", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 12 (Side-1 SupplyPad)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb12, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb12", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 13 (Side-1 Airport)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb13, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb13", false, true,
                                        true, 2, true, 60, 60, true, string.Empty, 0);

            // SciFi Building 14 (MCF)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb14, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb14", false, true,
                                        true, 3, true, 1000, 1000, true, string.Empty, 0);

            // SciFi Building 15 (MCF)
            AddItemTypeAttributeToArray(ItemType.sciFiBldb15, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\sciFiBldb15", false, true,
                                        true, 3, true, 1000, 1000, true, string.Empty, 0);


            #endregion

            #region Flag Marker

            // Add Flag Marker
            AddItemTypeAttributeToArray(ItemType.flagMarker, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Buildings2\FlagMarker", false, true,
                                        false, 0, true, 150, 150, true, string.Empty, 0.13f);

            #endregion

            #region SciFiTankSet

            // SciFi Tank 1
            AddItemTypeAttributeToArray(ItemType.sciFiTank01, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank01", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 2
            AddItemTypeAttributeToArray(ItemType.sciFiTank02, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank02", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 3
            AddItemTypeAttributeToArray(ItemType.sciFiTank03, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank03", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 4
            AddItemTypeAttributeToArray(ItemType.sciFiTank04, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank04", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 5
            AddItemTypeAttributeToArray(ItemType.sciFiTank05, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank05", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 6
            AddItemTypeAttributeToArray(ItemType.sciFiTank06, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank06", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 7
            AddItemTypeAttributeToArray(ItemType.sciFiTank07, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank07", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 8
            AddItemTypeAttributeToArray(ItemType.sciFiTank08, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank08", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 9
            AddItemTypeAttributeToArray(ItemType.sciFiTank09, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank09", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 10
            AddItemTypeAttributeToArray(ItemType.sciFiTank10, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank10", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Tank 11
            AddItemTypeAttributeToArray(ItemType.sciFiTank11, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiTank11", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Aritilery 01
            AddItemTypeAttributeToArray(ItemType.sciFiArtilery01, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiTanks\sciFiArtilery01", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            #endregion

            #region SciFiJeeps

            // SciFi Jeep 1
            AddItemTypeAttributeToArray(ItemType.sciFiJeep01, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiJeeps\sciFiJeep01", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            // SciFi Jeep 3
            AddItemTypeAttributeToArray(ItemType.sciFiJeep03, ModelType.InstanceModel, contentInstancedModels  + @"Vehicles\SciFiJeeps\sciFiJeep03", false, true,
                                        false, 0, true, 30, 30, true, string.Empty, 0);

            #endregion

            #region SciFiAirCraftSet

            // SciFi Helicopter 1
            AddItemTypeAttributeToArray(ItemType.sciFiHeli01, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiHeli01", false, true,
                                        false, 0, true, 50, 50, true, string.Empty, 0);

            // SciFi Helicopter 2
            AddItemTypeAttributeToArray(ItemType.sciFiHeli02, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiHeli02", false, true,
                                        false, 0, true, 70, 70, true, string.Empty, 0);

            // SciFi GunShip 1
            AddItemTypeAttributeToArray(ItemType.sciFiGunShip01, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiGunShip01", false, true,
                                        false, 0, false, 0, 0, true, string.Empty, 0);

            // SciFi GunShip 2
            AddItemTypeAttributeToArray(ItemType.sciFiGunShip02, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiGunShip02", false, true,
                                        false, 0, false, 0, 0, true, string.Empty, 0);

            // SciFi Bomber 1
            AddItemTypeAttributeToArray(ItemType.sciFiBomber01, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiBomber01", false, true,
                                        false, 0, false, 0, 0, true, string.Empty, 0);

            // SciFi Bomber 6
            AddItemTypeAttributeToArray(ItemType.sciFiBomber06, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiBomber06", false, true,
                                        false, 0, false, 0, 0, true, string.Empty, 0);

            // SciFi Bomber 7
            AddItemTypeAttributeToArray(ItemType.sciFiBomber07, ModelType.InstanceModel, contentInstancedModels  + @"Aircrafts\sciFiBomber07", false, true,
                                        false, 0, false, 0, 0, true, string.Empty, 0);


            #endregion

            #region SciFiDefenseSet

            // SciFi Defense AA-gun 1
            AddItemTypeAttributeToArray(ItemType.sciFiAAGun01, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Defenses\sciFiAAGun01", false, true,
                                        true, 1, true, 30, 30, true, string.Empty, 0);

            // SciFi Defense AA-gun 2
            AddItemTypeAttributeToArray(ItemType.sciFiAAGun02, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Defenses\sciFiAAGun02", false, true,
                                        true, 1, true, 30, 30, true, string.Empty, 0);

            // SciFi Defense AA-gun 4
            AddItemTypeAttributeToArray(ItemType.sciFiAAGun04, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Defenses\sciFiAAGun04", false, true,
                                        true, 1, true, 30, 30, true, string.Empty, 0);

            // SciFi Defense AA-gun 5
            AddItemTypeAttributeToArray(ItemType.sciFiAAGun05, ModelType.InstanceModel, contentInstancedModels  + @"Structures\SciFi_Defenses\sciFiAAGun05", false, true,
                                        true, 1, true, 30, 30, true, string.Empty, 0);


            #endregion

         

#if !XBOX360
            // Call Base Level Method to Save
            CreateItemTypeAttributesAndSave(game, "ScenaryItemTypeAtts.sav", ItemTypeAtts);
#endif

        }

        // 8/1/2008
        /// <summary>
        /// Loads the <see cref="ScenaryItemScene"/> <see cref="ItemType"/> <see cref="ScenaryItemTypeAttributes"/> 
        /// structure back into memory, from the XML file.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public static void LoadItemTypeAttributes(Game game)
        {
            // If XBOX360, then instead of loading data, which is incredibly slow due to the slow Serializing 
            // on the Combact framework, we will simply create the attributes directly.
#if XBOX360
            CreateItemTypeAttributesAndSave(game);
#else

            // 1/6/2009 - Check if forced rebuild wanted?
            if (_forceRebuildOfXMLFile)
            {
                CreateItemTypeAttributesAndSave(game);
                return;
            }


            // Call Base Level Method to Load
            List<ScenaryItemTypeAttributes> tmpItemTypeAtts;
            if (LoadItemTypeAttributes(game, "ScenaryItemTypeAtts.sav", 
                                                            out tmpItemTypeAtts, InstancedItem.ItemTypeCount))
            {
                // Add each record back into the Dictionary Array
                var count = tmpItemTypeAtts.Count;
                for (var loop1 = 0; loop1 < count; loop1++)
                {
                    ItemTypeAtts.Add(tmpItemTypeAtts[loop1].itemType, tmpItemTypeAtts[loop1]);
                }
            }
                // Load Failed, so let's recreate XML file.
            else
                CreateItemTypeAttributesAndSave(game);

#endif

        }

        // 8/1/2008
        /// <summary>
        /// Helper Function to add <see cref="ItemType"/> <see cref="ScenaryItemTypeAttributes"/> structs to the dictionary.
        /// This is currently called from the 'CreateItemTypeAttributesAndSave' Method.
        /// </summary>
        /// <param name="itemType"><see cref="ItemType"/> Enum to add</param>
        /// <param name="modelType"><see cref="ModelType"/> Enum type</param>
        /// <param name="pathName">Model Location</param>
        /// <param name="useTwoDraw">Use alpha draw?</param>
        /// <param name="castShadow">Should item cast a shadow?</param>
        /// <param name="pathBlocked">A* path blocked</param>
        /// <param name="pathBlockValue">PathSize</param>
        /// <param name="useFOW">Use <see cref="IFogOfWar"/> visibility?</param>
        /// <param name="fowHeight"><see cref="IFogOfWar"/> site Height</param>
        /// <param name="fowWidth"><see cref="IFogOfWar"/> site Width</param>
        /// <param name="modelAnimates">Model has animated bone?</param>
        /// <param name="modelAnimatesBoneName">Model's animated bone name</param>
        /// <param name="scale">Model's scale (zero uses artwork scale)</param>
        private static void AddItemTypeAttributeToArray(ItemType itemType, ModelType modelType, string pathName, bool useTwoDraw, 
            bool castShadow, bool pathBlocked, int pathBlockValue, bool useFOW, int fowHeight, int fowWidth, 
            bool modelAnimates, string modelAnimatesBoneName, float scale)
        {
            var itemTypeAttsToAdd = new ScenaryItemTypeAttributes
                                        {
                                            itemType = itemType,
                                            modelType = modelType,
                                            modelLoadPathName = pathName,
                                            useTwoDrawMethod = useTwoDraw,
                                            useShadowCasting = castShadow,
                                            usePathBlocking = pathBlocked,
                                            pathBlockValue = pathBlockValue,
                                            useFogOfWar = useFOW,
                                            FogOfWarHeight = fowHeight,
                                            FogOfWarWidth = fowWidth,
                                            modelAnimates = modelAnimates,
                                            modelAnimateBoneName = modelAnimatesBoneName,
                                            Scale = scale
                                        };

            ItemTypeAtts.Add(itemType, itemTypeAttsToAdd);
        }

        // 9/26/2008 - Dispose 
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        public new static void Dispose()
        {
            if (ItemTypeAtts != null)
                ItemTypeAtts.Clear();

            ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.ItemTypeAtts.Dispose();
        }
    }
}
