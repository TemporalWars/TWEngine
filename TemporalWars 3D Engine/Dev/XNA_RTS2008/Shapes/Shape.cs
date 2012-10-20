#region File Description
//-----------------------------------------------------------------------------
// Shape.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.Common;
using ImageNexus.BenScharbach.TWEngine.Explosions;
using ImageNexus.BenScharbach.TWEngine.Explosions.Structs;
using ImageNexus.BenScharbach.TWEngine.InstancedModels;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Structs;
using ImageNexus.BenScharbach.TWEngine.Interfaces;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.Particles;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWEngine.Shapes.Enums;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.Shapes
{
    /// <summary>
    /// The <see cref="Shape"/> is the base class of any object that is renderable
    /// </summary>
    public abstract class Shape : IDisposable, IInstancedItem, IFogOfWarShapeItem, IMinimapShapeItem
    {
        // 12/31/2009 - 
        /// <summary>
        /// The <see cref="FogOfWarItem"/> structure
        /// </summary>
// ReSharper disable InconsistentNaming
        internal FogOfWarItem _fogOfWarItem;
// ReSharper restore InconsistentNaming

        /// <summary>
        /// The current World <see cref="Matrix"/> used to render this shape
        /// </summary>
        protected internal Matrix World;
        private Vector3 _position;  
    
        // 11/14/2008 - Explode SceneItemOwner
        // 11/14/2008 - 

        /// <summary>
        /// How is the <see cref="SceneItem"/> oriented? We'll calculate this based on the user's input and
        /// the heightmap's normals, and then use it when drawing.
        /// </summary>
        private Matrix _orientation = Matrix.Identity;

        ///<summary>
        /// Set or Get the <see cref="InstancedItemData"/> structure.
        ///</summary>
        /// <remarks>Currently defaults to ItemType.sciFiTank01.</remarks>
        internal InstancedItemData InstancedItemData = new InstancedItemData(ItemType.sciFiTank01);

        /// <summary>
        /// Stores the <see cref="Game"/> reference
        /// </summary>
        protected static Game Game;
       
        #region Properties

        ///<summary>
        /// How is the <see cref="SceneItem"/> oriented? We'll calculate this based on the user's input and
        /// the heightmap's normals, and then use it when drawing.
        ///</summary>
        public Matrix Orientation
        {
            get{ return _orientation; }
            set
            {
                _orientation = value; 
            }
        }

        ///<summary>
        /// World <see cref="Matrix"/>
        ///</summary>
        public virtual Matrix WorldP
        {
            get{ return World; }
            set{ World = value; }
        }

        ///<summary>
        /// <see cref="Vector3"/> position
        ///</summary>
// ReSharper disable ConvertToAutoProperty
        public Vector3 Position
// ReSharper restore ConvertToAutoProperty
        {
            get{ return _position; }
            set{ _position = value; }
        }

        // 1/17/2011 - Updated to AutoProp
        ///<summary>
        /// <see cref="Vector3"/> as last <see cref="Projectile"/> velocity.
        ///</summary>
        public Vector3 LastProjectileVelocity { get; set; }
        

        #region IInstancedItem Interface Properties

        /// <summary>
        /// The <see cref="ItemType"/> Enum to use
        /// </summary>
        public virtual ItemType ItemType
        {
            get { return InstancedItemData.ItemType; }
        }

        /// <summary>
        /// The <see cref="ItemGroupType"/> Enum this item belongs to.
        /// </summary>
        public virtual ItemGroupType ItemGroupType
        {
            get { return InstancedItemData.ItemGroupType; }
        }

        ///<summary>
        /// Item picked in edit mode?
        ///</summary>
        public virtual bool IsPickedInEditMode { get; set; }

        ///<summary>
        /// <see cref="ModelType"/> Enum.
        ///</summary>
        public virtual ModelType IsModelType { get; set; }

        ///<summary>
        /// Does item contribute to path blocking for A*.
        ///</summary>
        public virtual bool IsPathBlocked { get; set; }

        ///<summary>
        /// Path block size area to affect?
        ///</summary>
        /// <remarks>Requires the <see cref="IsPathBlocked"/> to be TRUE.</remarks>
        public virtual int PathBlockSize { get; set; }
        
        /// <summary>
        /// The <see cref="InstancedItem"/> unique instance item key,
        /// stored in the <see cref="InstancedItemData"/> structure.
        /// </summary>
        public virtual int ItemInstanceKey
        {
            get { return InstancedItemData.ItemInstanceKey; }
        }
        

        #endregion

        #region IFogOfWarItem Interface Properties

        ///<summary>
        /// The <see cref="Rectangle"/> structure as <see cref="IFogOfWarShapeItem"/> destination.
        ///</summary>
        public Rectangle FogOfWarDestination
        {
            get { return _fogOfWarItem.FogOfWarDestination; }
            set { _fogOfWarItem.FogOfWarDestination = value; }
        }

        /// <summary>
        /// Is this <see cref="IFogOfWarShapeItem"/> in visible location?
        /// (Can be seen by enemy player)
        /// </summary>
        public bool IsFOWVisible
        {
            get { return _fogOfWarItem.IsFOWVisible; }
            set
            {
                _fogOfWarItem.IsFOWVisible = value;
                
            }
        }

        /// <summary>
        /// Use the <see cref="IFogOfWar"/> with the <see cref="IFogOfWarShapeItem"/>.
        /// </summary>
        public bool UseFogOfWar { get; set; }

        /// <summary>
        /// The <see cref="IFogOfWarShapeItem"/> visibility width.
        /// </summary>
        public int FogOfWarWidth
        {
            get { return _fogOfWarItem.FogOfWarWidth; }
            set { _fogOfWarItem.FogOfWarWidth = value; }
        }

        /// <summary>
        /// The <see cref="IFogOfWarShapeItem"/> visibility height.
        /// </summary>
        public int FogOfWarHeight
        {
            get { return _fogOfWarItem.FogOfWarHeight; }
            set { _fogOfWarItem.FogOfWarHeight = value; }
        }

        #endregion

        #endregion

        /// <summary>
        /// Creates a new <see cref="Shape"/>. Calls the virtual Create method to generate any vertex buffers etc
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        protected Shape(Game game)
        {           

            Game = game;
            
        }

        /// <summary>
        /// Creates the vertex buffers etc. This routine is called on object creation and on Device reset etc
        /// </summary>
        abstract public void Create();

        /// <summary>
        /// Renders the <see cref="Shape"/>. 
        /// </summary>
        /// <remarks>Base class does nothing.</remarks>
        public virtual void Render()
        {
            return;
        }

        /// <summary>
        /// Updates the <see cref="Shape"/>. 
        /// </summary>
        /// <remarks>Base class does nothing.</remarks>
        /// <param name="time"><see cref="TimeSpan"/> struct with time</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> struct with elapsed time since last call</param>
        public virtual void Update(ref TimeSpan time, ref TimeSpan elapsedTime)
        {
            return;
        }       
 
        // 1/30/2010: Renamed from 'DoExplodeItem', to 'StartExplosion'; also added 'SceneItem' ref param.
        // 11/14/2008 - Explode SceneItemOwner Virtual method
        /// <summary>
        /// When a <see cref="SceneItem"/> is killed, the base class <see cref="Shape"/> will automatically call
        /// this method.  This overriding method will provide an Exploding animation
        /// depending on the <see cref="ItemType"/>.  The <see cref="SceneItem"/> owner MUST be set to have at least one 
        /// <see cref="ExplosionItem"/> part being; this is so the <see cref="ExplosionsManager"/> can call the <see cref="SceneItemWithPick.FinishKillSceneItem"/>
        /// metohd to complete the death process.
        /// </summary>
        /// <param name="sceneItemOwner"><see cref="SceneItemWithPick"/> owner of this shape</param>
        /// <param name="elapsedTime"><see cref="TimeSpan"/> structure as elapsed time</param>
        [Obsolete]
        public virtual void StartExplosion(SceneItemWithPick sceneItemOwner, ref TimeSpan elapsedTime)
        {
            // 1/30/2010
            // NOTE: MUST set at least one ExplosionItem to have the 'SceneItemOwner' reference; but ONLY one, otherwise multiply calls will occur.

            return;
        }

        /// <summary>
        /// Called when a Device is created
        /// </summary>
        public virtual void OnCreateDevice()
        {
            Create();
        }

        // 8/3/2009
        /// <summary>
        /// Sets what <see cref="ItemType"/> to use for this <see cref="Shape"/> instance; for
        /// example, 'SciFiTank1' or 'Building1' ItemType.
        /// </summary>
        /// <remarks>Base class does nothing.</remarks>
        /// <param name="itemType"><see cref="ItemType"/> to use</param>
        public virtual void SetInstancedItemTypeToUse(ItemType itemType)
        {
            return;
        }

        // 8/3/2009
        /// <summary>
        /// Sets what <see cref="ItemType"/> to use for this <see cref="Shape"/> instance; for
        /// example, 'SciFiTank1' or 'Building1' ItemType.
        /// </summary> 
        /// <remarks>This overload version is specifically used for the <see cref="ScenaryItemScene"/> type.</remarks>
        /// <param name="itemType"><see cref="ItemType"/> to use</param>
        /// <param name="itemTypeAtts">(OUT) Returns the <see cref="ScenaryItemTypeAttributes"/> populated for the given <see cref="ItemType"/></param>
        protected virtual void SetInstancedItemTypeToUse(ItemType itemType, out ScenaryItemTypeAttributes itemTypeAtts)
        {
            InstancedItemData.ItemType = itemType;

            // Retrieve Attributes from Dictionary               
            if (ScenaryItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out itemTypeAtts))
            {
                // Create Model, using the Attributes info gathered for specific ItemType Enum.
                InstancedItem.AddInstancedItem(itemType);

            }            
            
        }

        #region Dispose Methods

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="all">Is this final dispose?</param>
        private static void Dispose(bool all)
        {
           
            if (all)
            {
                Game = null;
            }

        }

        #endregion


    }
}
