#region File Description
//-----------------------------------------------------------------------------
// MessageTagDescription.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;
using TWEngine.InstancedModels.Enums;

namespace TWEngine.ItemTypeAttributes.Structs
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ItemTypeAttributes.Structs"/> namespace contains the structures
    /// which make up the entire <see cref="ItemTypeAttributes.Structs"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

#pragma warning disable 1587
    ///<summary>
    /// The <see cref="MessageTagDescription"/> structure stores the
    /// <see cref="ItemType"/> item's attributes for on-screen display.
    ///</summary>
#pragma warning restore 1587
#if !XBOX360
    [Serializable] 
#endif
    public struct MessageTagDescription
    {
        private ItemType _itemType;
        internal StringBuilder SbTitle;
        internal StringBuilder SbCost;
        internal StringBuilder SbTimeToBuild;
        internal StringBuilder SbEnergy;        
        ///<summary>
        /// Collection of strings, as 'Requirements'.
        ///</summary>
        public List<string> Reqs;
        internal StringBuilder SbType; // ex: Prod Structure or Tech Structure.        
        ///<summary>
        /// Collection of strings, as 'Description'.
        ///</summary>
        public List<string> Description;
        ///<summary>
        /// Collection of strings, as 'Abilities'.
        ///</summary>
        public List<string> Abilities;        

        #region Properities

        ///<summary>
        /// The <see cref="ItemType"/> enum this structure belongs to
        ///</summary>
        public ItemType ItemType
        {
            get { return _itemType; }
            set { _itemType = value; }
        }

        ///<summary>
        /// Title to show for this <see cref="ItemType"/>.
        ///</summary>
        public string Title
        {
            get { return SbTitle.ToString(); }
            set 
            {
                if (SbTitle == null)
                    SbTitle = new StringBuilder(value);

                // 5/14/2009
                SbTitle.Remove(0, SbTitle.Length); // remove all previous chars.
                SbTitle.Insert(0, value);        
            
            }
        }

        ///<summary>
        /// Cost value to show for this <see cref="ItemType"/>.
        ///</summary>
        public string Cost
        {
            get { return SbCost.ToString(); }
            set 
            {
                if (SbCost == null)
                    SbCost = new StringBuilder(value);

                // 5/14/2009
                SbCost.Remove(0, SbCost.Length); // remove all previous chars.
                SbCost.Insert(0, value);       
            
            }
        }

        /// <summary>
        /// Time to build for this <see cref="ItemType"/>.
        /// </summary>
        public string TimeToBuild
        {
            get { return SbTimeToBuild.ToString(); }
            set 
            {
                if (SbTimeToBuild == null)
                    SbTimeToBuild = new StringBuilder(value);

                // 5/14/2009
                SbTimeToBuild.Remove(0, SbTimeToBuild.Length); // remove all previous chars.
                SbTimeToBuild.Insert(0, value);                    
            }
        }

        ///<summary>
        /// Energy necessary to use this <see cref="ItemType"/>.
        ///</summary>
        public string Energy
        {
            get { return SbEnergy.ToString(); }
            set 
            {
                if (SbEnergy == null)
                    SbEnergy = new StringBuilder(value);

                // 5/14/2009
                SbEnergy.Remove(0, SbEnergy.Length); // remove all previous chars.
                SbEnergy.Insert(0, value);                    
            }
        }

        ///<summary>
        /// The 'Type' this <see cref="ItemType"/> is.
        ///</summary>
        public string Type
        {
            get { return SbType.ToString(); }
            set 
            {
                if (SbType == null)
                    SbType = new StringBuilder(value);

                // 5/14/2009
                SbType.Remove(0, SbType.Length); // remove all previous chars.
                SbType.Insert(0, value);   
            
            }
        }


        #endregion

    }
}