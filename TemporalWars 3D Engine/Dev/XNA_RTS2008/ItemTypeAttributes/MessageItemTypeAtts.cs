#region File Description
//-----------------------------------------------------------------------------
// MessageItemTypeAtts.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using TWEngine.IFDTiles;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Enums;
using TWEngine.ItemTypeAttributes.Structs;
using TWEngine.SceneItems;

#endregion

namespace TWEngine.ItemTypeAttributes
{
    ///<summary>
    /// The <see cref="MessageItemTypeAtts"/> class, inheriting from <see cref="ItemTypeAtts"/> base class, is 
    /// used to store the attributes displayed to the end-user, when the user hovers over some <see cref="IFDTile"/>
    /// which relates to a <see cref="SceneItem"/>.  Internally, messages are stored in a dictionary, using the 
    /// <see cref="ItemType"/> Enum as the key, with the <see cref="MessageTagDescription"/> structure containing
    /// the attributes.
    ///</summary>
    public class MessageItemTypeAtts : ItemTypeAtts
    {
        // 9/26/2008 - Specific Message ItemType Attributes
        internal static Dictionary<ItemType, MessageTagDescription> ItemTypeAtts =
            new Dictionary<ItemType, MessageTagDescription>(InstancedItem.ItemTypeCount);

        // 5/1/2009 - Create Private Constructor, per FXCop
        private MessageItemTypeAtts()
        {
            // Empty
        }

        // 9/26/2008
        /// <summary>
        /// Creates the <see cref="MessageItemTypeAtts"/> for each specific <see cref="ItemType"/>,
        /// and saves the data to disk.  This file is used when loading 
        /// items back into memory. This allows for changing of the attributes quickly,
        /// just by updating the XML file for a specific <see cref="ItemType"/>.
        /// </summary>
        /// <remarks>
        /// This method should only me called to create the file for the first Time,
        /// or if the file is lost or destroyed.
        /// </remarks>
        /// <param name="game"><see cref="Game"/> instance</param>
        private static void CreateItemTypeAttributesAndSave(Game game)
        {
            // 8/20/2008 - Save Game Ref
            GameInstance = game;

            // 1/5/2009
            PlayableItemTypeAttributes playableAtts;

            #region SciFiBuildingSet_1

            // ReSharper disable RedundantToStringCall
            // Add Building 1
            var tmpReqs = new List<string> {"Power Plant", "Refinery"};
            var tmpDesc = new List<string> {"The 1st SciFi building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda01, out playableAtts);

            AddItemTypeAttributeToArray(ItemType.sciFiBlda01, "Building 1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 2
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"The 2nd cool building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda02, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda02, "Building 2", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 3            
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Refinery", "Barracks"};
            // *Desc
            tmpDesc = new List<string> {"The 3rd cool building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda03, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda03, "Building 3", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);


            // Add Building 4
            tmpReqs = new List<string> {"Power Plant", "Refinery"};
            tmpDesc = new List<string> {"The 4th SciFi building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda04, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda04, "Building 4", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 5
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Barracks"};
            // *Desc
            tmpDesc = new List<string> {"The 5th cool Tech building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda05, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda05, "Building 5", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);
           
            // Add Building 6           
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Refinery", "War Factory", "Barracks"};
            // *Desc
            tmpDesc = new List<string> {"The best building.", "Might just be too cool!"};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda06, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda06, "Building 6", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 7
            tmpReqs = new List<string> {"Power Plant", "Refinery"};
            tmpDesc = new List<string> {"The 7tn SciFi building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda07, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda07, "Building 7", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 8
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"The 8th cool building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda08, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda08, "Building 8", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 9          
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Technology Center", "War Factory"};
            // *Desc
            tmpDesc = new List<string> {"The 9th building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBlda09, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBlda09, "Building 9", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            #endregion

            #region SciFiBuildingSet_2

            // Add Building 1
            tmpReqs = new List<string> {"HQ"};
            tmpDesc = new List<string> {"Produces vehicles."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb01, "War Factory", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 2
            // *Reqs
            tmpReqs = new List<string> {"War Factory", "Airport"};
            // *Desc
            tmpDesc = new List<string> {"Research Center"};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb02, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb02, "Tech Upgrades", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 3            
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Refinery"};
            //tmpReqs.Add("Barracks");
            // *Desc
            tmpDesc = new List<string> {"The 3rd cool building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb03, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb03, "Building 3", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);


            // Add Building 4
            tmpReqs = new List<string> {"War Factory", "Airport"};
            tmpDesc = new List<string> {"Research Center"};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb04, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb04, "Tech Upgrades", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 5
            // *Reqs
            tmpReqs = new List<string> {"HQ"};
            // *Desc
            tmpDesc = new List<string> {"Provides energy for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb05, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb05, "Power Plant", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 6           
            // *Reqs
            tmpReqs = new List<string> {"Power Plant", "Refinery", "War Factory", "Barracks"};
            // *Desc
            tmpDesc = new List<string> {"The best building.", "Might just be too cool!"};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb06, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb06, "Building 6", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 7
            tmpReqs = new List<string> {"Power Plant"};
            //tmpReqs.Add("Refinery");
            tmpDesc = new List<string> {"Produces income for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb07, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb07, "Refinery", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Supply Structure", tmpReqs, tmpDesc);

            // Add Building 8
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"The 8th cool building."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb08, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb08, "Building 8", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 9          
            // *Reqs
            tmpReqs = new List<string> {"HQ"};
            // *Desc
            tmpDesc = new List<string> {"Provides energy for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb09, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb09, "Power Plant", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Technology Structure", tmpReqs, tmpDesc);

            // Add Building 10         
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"Produces aircraft for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb10, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb10, "Airport", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 11         
            // *Reqs
            tmpReqs = new List<string> {"HQ"};
            // *Desc
            tmpDesc = new List<string> {"Produces vehicles for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb11, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb11, "War Factory", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 12         
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"Produces income for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb12, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb12, "Supply Dropoff", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Supply Structure", tmpReqs, tmpDesc);

            // Add Building 13         
            // *Reqs
            tmpReqs = new List<string> {"Power Plant"};
            // *Desc
            tmpDesc = new List<string> {"Produces aircraft for the base."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb13, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb13, "Airport", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Production Structure", tmpReqs, tmpDesc);

            // Add Building 14         
            // *Reqs
            tmpReqs = new List<string> {"None"};
            // *Desc
            tmpDesc = new List<string> {"HeadQuarters."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb14, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb14, "HQ", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "HQ Structure", tmpReqs, tmpDesc);

            // Add Building 15         
            // *Reqs
            tmpReqs = new List<string> {"None"};
            // *Desc
            tmpDesc = new List<string> {"HeadQuarters."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBldb15, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBldb15, "HQ", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "HQ Structure", tmpReqs, tmpDesc);

            #endregion

            #region Flag Marker

            // Add Flag Marker
            tmpReqs = new List<string> {"Flag Marker"};
            tmpDesc = new List<string> {"Location for unit to go to."};

            // Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.flagMarker, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.flagMarker, "Flag Marker", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Marker Location", tmpReqs, tmpDesc);

            #endregion

            #region SciFiTanks

            // Add Tank 1
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Vehicles."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank01, "Tank-1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Medium Tank", tmpReqs, tmpDesc);

            // Add Tank 2
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Quick, light tank, which", "can attack any other unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank02, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank02, "Tank-2", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Light Tank", tmpReqs, tmpDesc);

            // Add Tank 3
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Vehicles."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank03, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank03, "Tank-3", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Medium Tank", tmpReqs, tmpDesc);

            // Add Tank 4
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Quick, light tank, which", "can attack any other unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank04, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank04, "Tank-4", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Light Tank", tmpReqs, tmpDesc);

            // Add Tank 5
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Tank Desc needed."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank05, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank05, "Tank-5", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Tank", tmpReqs, tmpDesc);

            // Add Tank 6
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Buildings."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank06, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank06, "Tank-6", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Medium Tank", tmpReqs, tmpDesc);

            // Add Tank 7
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong, heavy tank, which can", "level buildings and other units."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank07, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank07, "Tank-7", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Heavy Tank", tmpReqs, tmpDesc);

            // Add Tank 8
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Buildings."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank08, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank08, "Tank-8", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Medium Tank", tmpReqs, tmpDesc);

            // Add Tank 9
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong, heavy tank, which can", "level buildings and other units."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank09, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank09, "Tank-9", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Heavy Tank", tmpReqs, tmpDesc);

            // Add Tank 10
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Artillery Tank, capable of", "long range devasting attacks."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank10, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank10, "Tank-10", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Artillery Tank", tmpReqs, tmpDesc);

            // Add Tank 11
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Tank Desc needed."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiTank11, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiTank11, "Tank-11", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Tank", tmpReqs, tmpDesc);

            // Add Artilery 1
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string>
                          {
                              "Artillery Truck, capable of",
                              "long range devasting attacks.",
                              "NOTE: Attacks when still."
                          };
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiArtilery01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiArtilery01, "Artillery-1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Artillery Truck", tmpReqs, tmpDesc);

            #endregion

            #region SciFiJeeps

            // Add Jeep 1
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"An anti-air unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiJeep01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiJeep01, "Jeep-1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Anti-Air Jeep", tmpReqs, tmpDesc);

            // Add Jeep 3
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"An anti-air unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiJeep03, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiJeep03, "Jeep-3", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Anti-Air Jeep", tmpReqs, tmpDesc);

            #endregion

            #region SciFiAircraftSet

            // Add Helicopter 1
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Vehicles."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiHeli01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiHeli01, "Helicopter-1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);

            // Add Helicopter 2
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Vehicles."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiHeli02, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiHeli02, "Helicopter-2", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);

            // Add Gunship 2
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Anti Air-unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiGunShip02, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiGunShip02, "Gunship", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);

            // Add Bomber 1
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Buildings."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBomber01, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBomber01, "Bomber-1", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);

            // Add Bomber 6
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Strong VS Buildings."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBomber06, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBomber06, "Bomber-6", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);

            // Add Bomber 7
            // *Reqs
            tmpReqs = new List<string> {"None."};
            // *Desc
            tmpDesc = new List<string> {"Anti Air-unit."};
            // 1/5/2009: Get Playable Atts for this SceneItemOwner.
            PlayableItemTypeAtts.ItemTypeAtts.TryGetValue(ItemType.sciFiBomber07, out playableAtts);
            AddItemTypeAttributeToArray(ItemType.sciFiBomber07, "Gunship", "$" + playableAtts.Cost.ToString(),
                                        playableAtts.TimeToBuild.ToString(), playableAtts.EnergyNeeded.ToString(),
                                        "Aircraft", tmpReqs, tmpDesc);
// ReSharper restore RedundantToStringCall
            #endregion

#if !XBOX360
            // Call Base Level Method to Save
            CreateItemTypeAttributesAndSave(game, "MessageItemTypeAtts.sav",
                                            ItemTypeAtts);
#endif
        }


        /// <summary>
        /// Loads the <see cref="MessageItemTypeAtts"/> back into memory, from the XML file.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public static void LoadItemTypeAttributes(Game game)
        {
            // If XBOX360, then instead of loading data, which is incredibly slow due to the slow Serializing 
            // on the Combact framework, we will simply create the attributes directly.
#if XBOX360
            CreateItemTypeAttributesAndSave(game);
#else
            // Call Base Level Method to Load
            List<MessageTagDescription> tmpItemTypeAtts;
            if (LoadItemTypeAttributes(game, "MessageItemTypeAtts.sav", out tmpItemTypeAtts, 44))
            {
                // Add each record back into the Dictionary Array
                var count = tmpItemTypeAtts.Count;
                for (var loop1 = 0; loop1 < count; loop1++)
                {
                    ItemTypeAtts.Add(tmpItemTypeAtts[loop1].ItemType, tmpItemTypeAtts[loop1]);
                }
            }
                // Load Failed, so let's recreate XML file.
            else
                CreateItemTypeAttributesAndSave(game);
#endif
        }


        // 9/26/2008 -  Helper Function to add Message ItemTypes Attributes to the List Array.
        //              This is currently called from the 'CreateItemTypeAttributesAndSave' Method.
        private static void AddItemTypeAttributeToArray(ItemType itemType, string title, string cost, string timeToBuild,
                                                        string energy, string type, List<string> reqs, List<string> desc)
        {
            var itemTag = new MessageTagDescription
                              {
                                  ItemType = itemType,
                                  Title = title,
                                  Cost = cost,
                                  TimeToBuild = timeToBuild,
                                  Energy = energy,
                                  Type = type,
                                  Reqs = reqs,
                                  Description = desc
                              };

            ItemTypeAtts.Add(itemType, itemTag);
        }

        // 9/26/2008 - Dispose 
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        public new static void Dispose()
        {
            if (ItemTypeAtts != null)
                ItemTypeAtts.Clear();

            ItemTypeAttributes.ItemTypeAtts.Dispose();
        }
    }
}