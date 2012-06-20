#region File Description
//-----------------------------------------------------------------------------
// InstancedModel.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#if !XBOX360
using System.Diagnostics;
//using TWEngine.PhysX;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpeedCollectionComponent;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.ForceBehaviors;
using TWEngine.GameCamera;
using TWEngine.SceneItems;
using TWEngine.Shadows;
using TWEngine.Shapes;
using TWEngine.ForceBehaviors.SteeringBehaviors;
using TWEngine.Terrain;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;
using TWEngine.Terrain.Enums;
using InstancedModelAttsData = TWEngine.InstancedModels.Structs.InstancedModelAttsData;
using ShaderToUseEnum = TWEngine.InstancedModels.Enums.ShaderToUseEnum;

namespace TWEngine.InstancedModels
{
    /// <summary>
    /// The <see cref="InstancedModel"/> class can efficiently draw many copies of itself,
    /// using various different GPU instancing techniques.  
    /// This class is the parent for the <see cref="InstancedModelPart"/>, which represents each
    /// drawable mesh, for a given <see cref="InstancedModel"/> item.
    /// </summary>
    /// <remarks>This class has been modified to also allow instancing of items with moving bone parts.</remarks>
    public sealed class InstancedModel : IDisposable
    {
        #region Fields     

        const int SizeOfVector4 = sizeof(float) * 4;

        // 3/13/2011 - XNA 4.0 Updates - new Xna Model (ModelContent)
        internal Model XnaModel;
   
        // 8/12/2009 - Used in the 'SetAdjustingBoneTransform' method, to skip applying rotation which have not change
        //             from prior value.
        private float _oldRotationValue;
        private static readonly Matrix MatrixIdentity = Matrix.Identity;
   
        // 8/28/2009 - InstancedModel ChangeRequest Manager instance.
        internal readonly InstancedModelChangeRequests InstancedModelChangeRequestManager = new InstancedModelChangeRequests();

        // 1/17/2011 - Save ref to current explosion boneName.
        private string _currentExplosionBoneName;

        // 3/24/2011 - XNA 4.0 Updates
        private readonly InstancedModelExtra _instancedModelContent;

        ///<summary>
        /// Returns a reference to the <see cref="InstancedModelChangeRequests"/> manager.
        ///</summary>
        public InstancedModelChangeRequests ChangeRequestManager
        {
            get { return InstancedModelChangeRequestManager; }
        }

        // 3/28/2009 - StopWatchTimer for performance debugging        
        private static bool _timersInit;
        private static readonly object ThreadLock = new object();

        // Internally our custom _model is made up from a list of _model parts.
        // Most of the interesting code lives in the InstancedModelPart class.
// ReSharper disable InconsistentNaming
        /// <summary>
        /// An array of <see cref="InstancedModelPart"/>, which makes up this <see cref="InstancedModel"/> item.
        /// </summary>
        internal readonly List<InstancedModelPart> _modelParts = new List<InstancedModelPart>();
// ReSharper restore InconsistentNaming

        // 3/28/2009 - List of INT Keys, which are the index values into the 'ModelParts' List Array above.  One
        //             list holds the _keys to the normal bones to draw during the life of the unit, while the other
        //             holds the bones for the explosion pieces; 'Piece1', 'Piece2', 'Piece3', & 'Piece4'.
        /// <summary>
        /// An array of <see cref="int"/> values, which represent the index values into the <see cref="_modelParts"/> list array.
        /// </summary>
        internal readonly List<int> ModelPartsKeys; // 
        private readonly List<int> _explosionPartsKeys;
        private int _explosionPartsCount; // 7/9/2009
        private int _modelPartsCount; // 8/21/2009

        // 2/15/2010 - When some model or models are exploding, then BOTH batches need to 
        //             be drawn in the same draw call.
        private bool _drawExplosionPiecesToo;

        // 1/27/2010 - InstancedModel Bones Collection
// ReSharper disable InconsistentNaming
        /// <summary>
        /// Reference to the <see cref="InstancedModelBoneCollection"/>.
        /// </summary>
        internal InstancedModelBoneCollection _bonesCollection;
        private readonly bool _bonesLoaded;

        /// <summary>
        /// Stores the <see cref="InstancedModelPart"/> Adjusting Bone transforms, which is used in the 
        /// <see cref="CopyAbsoluteBoneTranformsTo"/> method.
        /// </summary>
        /// <remarks>This collection uses the custom type <see cref="SpeedCollectionComponent.SpeedCollection{TValue}"/>.</remarks>
        internal readonly SpeedCollection<Matrix[]> _adjustingBoneTransforms = new SpeedCollection<Matrix[]>(55);

        /// <summary>
        /// Stores if an Ajdusting Bone transform entry was just made.  This is to eliminate the constant
        /// updating of the <see cref="CopyAbsoluteBoneTranformsTo"/> method; internally now, the method checks if 
        /// this Collection has an entry which is 'TRUE'; only then is the calculations made.  Otherwise,
        /// they are skipped for current call.
        /// </summary>
        /// <remarks>This collection uses the custom type <see cref="SpeedCollectionComponent.SpeedCollection{TValue}"/>.</remarks>
        internal readonly Dictionary<int, bool> _adjustingBoneTransformsEntryMade = new Dictionary<int, bool>(55);

        /// <summary>
        /// Updated to store the <see cref="InstancedItemTransform"/> structure, instead of the <see cref="Matrix"/>; this will eliminate
        /// the problem of having to constanly access the <see cref="Matrix"/> values, to always get the newest copy; furthermore,
        /// with references to the class, the changes are immediate!
        /// </summary>
        /// <remarks>This collection uses the custom type <see cref="SpeedCollectionComponent.SpeedCollection{TValue}"/>.</remarks>
        internal readonly SpeedCollection<InstancedItemTransform[]> _absoluteBoneTransforms = new SpeedCollection<InstancedItemTransform[]>(55);
        // ReSharper restore InconsistentNaming

        // 1/17/2011 - Stores the Explosion Velocity for given 'BoneName'.
        private Dictionary<int, Dictionary<string, Vector3>> _explosionVelocities =
            new Dictionary<int, Dictionary<string, Vector3>>(55);


        #region Obsolete Animation settings.
        // 1/9/2009 - Animation Settings;
        //            1st line is to know if ItemType has animation.
        //            2nd line is used to store the global animation Settings for the specific ItemType.
        //            Dictionary is used to store the individual 'Instance' animation Settings for a specific ItemType.

        /*private readonly bool _instancedModelAnimates;

        private readonly InstancedModelAnimation _instancedModelAnimation = new InstancedModelAnimation();

        private readonly Dictionary<int, InstancedModelAnimation> _instancedModelAnimations =
            new Dictionary<int, InstancedModelAnimation>();*/
        #endregion

        internal Matrix AdjustCollisionRadiusMatrix;
        // 5/7/2009 - Used to rescale the BoundingSphere in the InstancedItem picking.  

        internal bool AdjustCollisionRadiusSet;
        // 5/7/2009 - USed to tell Picking routine to skip Transform if not set.                  

        // 11/15/2008 - Use Baked Transforms?
        /// <summary>
        /// If current model used 'Baked' transforms.
        /// </summary>
        internal readonly bool UseBakedTransforms;

        // 12/1/2008 - AlwaysDrawShadow? - 
        /// <summary>
        /// Set for moveable items, like tanks, which always need to be drawn every cycle.
        /// </summary>
        internal bool AlwaysDrawShadow;

        // 12/9/2008 - Was already applied?
        private bool _appliedRotationValues;

        // 8/19/2009 - Updatd to use the new 'SpeedCollection', rather than Dictionary! 
        // An Array of 'InstanceData' Structs, used to store the location of _model, if
        // _model is currently Picked, and if it is in the camera's view.
        // 12/18/2008 - Change to Dictionary for fast retrieval! Key = ItemInstanceKey
        //internal Dictionary<int, InstanceData> InstanceWorldTransforms = new Dictionary<int, InstanceData>(55);
        /// <summary>
        /// An array of <see cref="InstancedDataCommunication"/> structures, used to carry transform changes between
        /// the original <see cref="SceneItem"/>, and the <see cref="InstancedModelPart"/> items.
        /// </summary>
        /// <remarks>This collection uses the custom type <see cref="SpeedCollectionComponent.SpeedCollection{TValue}"/>.</remarks>
        internal Dictionary<int, InstancedDataCommunication> InstanceWorldTransforms = new Dictionary<int, InstancedDataCommunication>(55);
        
        
        // 5/11/2009 - 
        /// <summary>
        /// Keeps the set of Keys for Dictionary; reduces garbage by having each set kept in class instance.
        /// </summary>
        internal int[] InstanceWorldTransformKeys = new int[1];

        // 5/26/2009: Updated to be STATIC!
        // Keep track of what graphics Device we are using.
        private static GraphicsDevice _graphicsDevice;

        // vertex data stream used for HardwareInstancing.
        internal DynamicVertexBuffer _instanceVertexBuffer;
        // XNA 4.0 - Now stores Stream#2 vertexDeclaration.
        internal VertexDeclaration InstanceVertexDeclaration;

        // 8/26/2009
        private readonly InstancedModelAttsData _attsData;

        #endregion

        #region Properties

        // 6/18/2010
        /// <summary>
        /// The <see cref="ItemType"/> in use for this <see cref="InstancedModel"/>.
        /// </summary>
        /// <remarks>This is set from the <see cref="InstancedItemLoader.PreLoadInstanceItem"/> method.</remarks>
        public ItemType ItemTypeInUse { get; private set; }

        // 3/25/2011
        /// <summary>
        /// Is this instance a scenary item?
        /// </summary>
        public bool IsScenaryItem { get; private set; }

        // 6/6/2010
        ///<summary>
        /// Stores the accumlative elapsed game time.
        ///</summary>
        /// <remarks>Purpose of use is for the explosion animations on the shader.</remarks>
        public static float AccumElapsedTime { get; set; }

        // 5/6/2009
        /// <summary>
        /// Stores this <see cref="InstancedModel"/> <see cref="BoundingSphere"/>, which is used in determining if within cameraView
        /// <see cref="Camera"/> View frustum.
        /// </summary>
        public BoundingSphere CollisionRadius { get; internal set; }

        // 10/17/2008 - 
        /// <summary>
        /// Rotation offset for X
        /// </summary>
        public float RotX { get; private set; }

        /// <summary>
        ///  Rotation offset for Y
        /// </summary>
        public float RotY { get; private set; }

        /// <summary>
        ///  Rotation offset for Z
        /// </summary>
        public float RotZ { get; private set; }

        /// <summary>
        /// Scale or size of the item in the game world.
        /// </summary>
        public float Scale { get; internal set; }

        // 5/22/2009-
        /// <summary>
        /// Was <see cref="InstancedModel"/> imported using the 'FBXImporter'.
        /// </summary>
        public bool IsFBXImported { get; private set; }

        /// <summary>
        /// Array of <see cref="InstancedModelPart"/>.
        /// </summary>
        internal List<InstancedModelPart> ModelParts
        {
            get { return _modelParts; }
        }

        /// <summary>
        /// Gets the current <see cref="InstancingTechnique"/> Enum technique.
        /// </summary>
        public InstancingTechnique InstancingTechnique { get; private set; }

        // 2/3/2010
        /// <summary>
        /// Gets the count for the 'Normal' type model parts; as opposed to
        /// the 'Explosion' type model parts.
        /// </summary>
        public int ModelPartsCount
        {
            get { return _modelPartsCount; }
        }

        #endregion

        #region Initialization

        // 3/16/2011 - 
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="xnaModel">Instance of <see cref="Model"/></param>
        /// <param name="itemType"></param>
        /// <param name="isScenaryItem"></param>
        internal InstancedModel(Model xnaModel, ItemType itemType, bool isScenaryItem)
        {
            // Look up our graphics Device.
            if (_graphicsDevice == null)
                _graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // 3/25/2011 - Save ItemType
            ItemTypeInUse = itemType;

            // 3/25/2011 - Save IsScenaryITem
            IsScenaryItem = isScenaryItem;

            // 3/18/2011 - Retrieve custom tag class
            _instancedModelContent = (InstancedModelExtra)xnaModel.Tag;
           
            IsFBXImported = _instancedModelContent.IsFbxFormat;
            UseBakedTransforms = _instancedModelContent.UseBakeTransforms;
            Scale = _instancedModelContent.Scale;
            RotX = _instancedModelContent.RotX;
            RotY = _instancedModelContent.RotY;
            RotZ = _instancedModelContent.RotZ;

             // 3/13/2011 - XNA 4.0 - Read new Xna Model (ModelContent)
            XnaModel = xnaModel;
           
            // Connect Bone Collection
            ReadBones(xnaModel.Bones);

            ModelPartsKeys = new List<int>();
            _explosionPartsKeys = new List<int>(); 

            // Create InstancedModelParts
            CreateInstancedModelPartsFromXnaModel(xnaModel);

            // Re-Order modelParts collection to put Explosion pieces last.
            ReOrderExplosionPieces();

            // Seperate the Explosion Pieces from the normal pieces.   
            CreateNormalAndExplosionKeyCollections();

            // Let's now save the boneOffset Index value into the proper ModelPart
            UpdateInstancedModelPartsBoneIndexes();

        }

       

        #region OldConstructor

        /*/// <summary>
        /// Constructor reads instanced <see cref="InstancedModel"/> data from the custom XNB format.
        /// </summary>  
        /// <param name="input">The <see cref="ContentReader"/> used to load the XNB format files.</param>      
        internal InstancedModel(ContentReader input)
        {
            // 3/28/2009
            if (!_timersInit)
            {
                lock (ThreadLock)
                {
                    StopWatchTimers.CreateStopWatchInstance(StopWatchName.IMDraw, false);//"IM-Draw"
                    StopWatchTimers.CreateStopWatchInstance(StopWatchName.UpdateTransforms, false);//"UpdateTransforms"
                    _timersInit = true;
                }
            }

            // 5/26/2009: Updated to set only once, since STATIC!
            // Look up our graphics Device.
            if (_graphicsDevice == null)
                _graphicsDevice = GetGraphicsDevice(input);

            // Load the _model data.
            var partCount = input.ReadInt32();

            // 7/22/209 - Read 'IsStaticItem' flag.
            var isStaticItem = input.ReadBoolean();

            // 3/14/2011 - XNA 4.0
            var hasSpawnBulletMarkers = input.ReadBoolean();

            // 3/14/2011 -  XNA 4.0 - Save boolean 'IsFBX' format.
            IsFBXImported = input.ReadBoolean();

            // 3/13/2011 - XNA 4.0 - Read new Xna Model (ModelContent)
            XnaModel = input.ReadExternalReference<Model>();

            // 4/22/2009
            _modelParts.Capacity = partCount;
            ModelPartsKeys = new List<int>();
            _explosionPartsKeys = new List<int>(); 
            // Create Model Parts
            for (var i = 0; i < partCount; i++)
            {
                // Create InstancedModelPart
                _modelParts.Add(new InstancedModelPart(input, _graphicsDevice, this, isStaticItem, i));

                // Seperate the Explosion Pieces from the normal pieces.     
                if (_modelParts[i].ModelPartName != null)
                    if (_modelParts[i].ModelPartName.ToString().StartsWith("Piece"))
                    {
                        _explosionPartsKeys.Add(i);
                    }
                    else
                        ModelPartsKeys.Add(i);
            }

            // 5/27/2009 - Read Index Keys for Explosion/Normal parts
            _explosionPartsCount = input.ReadInt32();
            _explosionPartsKeys = (_explosionPartsCount > 0) ? new List<int>(_explosionPartsCount) : null; // 7/9/2009
            for (var i = 0; i < _explosionPartsCount; i++)
            {
                if (_explosionPartsKeys == null) continue; // TODO: ??

                var indexKey = input.ReadInt32(); // 6/4/2010
                _explosionPartsKeys.Add(indexKey);
              
            }

            _modelPartsCount = input.ReadInt32();
            ModelPartsKeys = new List<int>(ModelPartsCount); // 7/9/2009
            for (var i = 0; i < ModelPartsCount; i++)
            {
                ModelPartsKeys.Add(input.ReadInt32());
            }

            // 6/12/2009 - Set AttsData into each ModelPart.        
            _attsData = input.ReadObject<InstancedModelAttsData>();
            for (var i = 0; i < partCount; i++)
            {
                _modelParts[i].AttsData = _attsData;
            }


            // 11/15/2008 - Load UseBakedTransforms flag, to determine if applying the AbsoluteBoneTransform calc is necessary.
            UseBakedTransforms = input.ReadBoolean();

            // 10/2/2008 - Load BonesCollection
            // 11/19/2008 - Only Load if 'useBakeTransforms' is False.
            // 1/20/2009 - Set to only load once per ItemType.            
            if (!UseBakedTransforms && !_bonesLoaded)
            {
                // 1/27/2010 - Read 'BonesCollection'.
                ReadBones(input);
            }

            // 1/28/2010 - Updated to read PackedVector3.
            // 10/17/2008 - Load rotation offsets; 
            {
                // RotX = input.ReadSingle(); RotY = input.ReadSingle(); RotZ = input.ReadSingle();
                var packedVector = input.ReadObject<PackedVector3>();
                Vector3 vector3;
                packedVector.UnPackVector3(out vector3);
                RotX = vector3.X;
                RotY = vector3.Y;
                RotZ = vector3.Z;
            }

            // 1/1/2009 - Load scale value
            Scale = input.ReadSingle();

            

            #region Obsolete Animation settings
            // 1/9/2009 - Load Animation Atts for Bone-1 and Bone-2.
            // Bone-1
            /*_instancedModelAnimates = input.ReadBoolean();
            // 4/22/2009 - Only load when True.
            if (_instancedModelAnimates)
            {
                _instancedModelAnimation.bone1_AnimationType = input.ReadInt32();
                _instancedModelAnimation.bone1_Name = input.ReadString();
                _instancedModelAnimation.bone1_RotateOnAxis = input.ReadInt32();
                _instancedModelAnimation.bone1_RotationSpeed = input.ReadSingle();
            }
            // Bone-2
            _instancedModelAnimation.bone2_Animates = input.ReadBoolean();
            // 4/22/2009 - Only load when True.
            if (_instancedModelAnimation.bone2_Animates)
            {
                _instancedModelAnimation.bone2_AnimationType = input.ReadInt32();
                _instancedModelAnimation.bone2_Name = input.ReadString();
                _instancedModelAnimation.bone2_RotateOnAxis = input.ReadInt32();
                _instancedModelAnimation.bone2_RotationSpeed = input.ReadSingle();
            }
            #endregion

            // 10/17/2008 - Get Root bone, apply the rotation offsets
            // 11/19/2008 - Only Update if 'useBakeTransforms' is False.
            // 1/20/2009 - Set to only load once per ItemType.
            if (!UseBakedTransforms && !_bonesLoaded)
            {
                // Let's now save the boneOffset Index value into the proper ModelPart
                for (var i = 0; i < _modelParts.Count; i++)
                {
                    // Locate bone name, to get index of array
                    var modelBonesCount = _bonesCollection.Count; // 8/12/2009; 1/28/2010 - was '_model'.
                    for (var j = 0; j < modelBonesCount; j++)
                    {
                        // Found bone name.
                        var modelBone = _bonesCollection[j]; // 8/12/2009; 1/28/2010 - was '_model'.
                        if (modelBone.Name != _modelParts[i].ModelPartName.ToString()) continue;

                        // Copy Absolute Bone Offset Index, and break out of search loop                         
                        _modelParts[i].BoneOffsetIndex = modelBone.Index;

                        break;
                    } // End Loop Model.Bones
                } // End Loop PartCount
            } // End If Not 'UseBakedTransforms'.

            _bonesLoaded = true;
        }*/

        #endregion

        /// <summary>
        /// Will iterate the current parts list, and move all Explosion 'Pieces'
        /// to the end of the list.  This will guarantee that the first index position
        /// will always be a normal part piece of the model!
        /// </summary>
        private void ReOrderExplosionPieces()
        {
            // iterate list, to seperate explosion pieces into its own queue.
            var normalParts = new Queue<InstancedModelPart>();
            var explosionParts = new Queue<InstancedModelPart>();
            foreach (var modelPart in _modelParts)
            {
                var modelPartName = modelPart.ModelPartName;

                if (modelPartName != null)
                    if (modelPartName.ToString().StartsWith("Piece"))
                    {
                        explosionParts.Enqueue(modelPart);
                    }
                    else
                        normalParts.Enqueue(modelPart);
            }

            // clear part list.
            _modelParts.Clear();

            // Rebuild list, putting Explosion pieces last!
            {
                // Iterate Normal pieces first.
                while (normalParts.Count > 0)
                {
                    var modelPart = normalParts.Dequeue();
                    _modelParts.Add(modelPart);
                }

                // Finally, iterate Explosion pieces.
                while (explosionParts.Count > 0)
                {
                    var modelPart = explosionParts.Dequeue();
                    _modelParts.Add(modelPart);
                }
            } // End Rebuild List
        }

        // 3/23/2011
        /// <summary>
        /// Seperates the explosion and normal <see cref="InstancedModelPart"/> pieces, by
        /// saving the index values from the original <see cref="_modelParts"/> collection.
        /// </summary>
        private void CreateNormalAndExplosionKeyCollections()
        {
            for (var i = 0; i < _modelParts.Count; i++)
            {
                // cache
                var instancedModelPart = _modelParts[i];
                var modelPartName = instancedModelPart.ModelPartName;

                if (modelPartName != null)
                    if (modelPartName.ToString().StartsWith("Piece"))
                    {
                        _explosionPartsKeys.Add(i);

                        // 3/23/2011 - Set as Explosion Piece
                        instancedModelPart.IsExplosionPiece = true;
                    }
                    else
                        ModelPartsKeys.Add(i);
            }

            // 3/18/2011 - Save Key counts
            _modelPartsCount = ModelPartsKeys.Count;
            _explosionPartsCount = _explosionPartsKeys.Count;
        }

        // 3/23/2011
        /// <summary>
        /// Iterates the Xna <see cref="Model"/> to create the <see cref="InstancedModelPart"/> collection.
        /// </summary>
        /// <param name="xnaModel">Instance of <see cref="Model"/></param>
        private void CreateInstancedModelPartsFromXnaModel(Model xnaModel)
        {
            var partIndex = 0;
            foreach (var mesh in xnaModel.Meshes)
            {
                // Retrieve BoundingSphere
                var boundingSphere = mesh.BoundingSphere;

                // 3/25/2011 - Skip any SpawnBullet meshParts
                if (mesh.Name.StartsWith("SpawnBullet")) continue;

                foreach (var meshPart in mesh.MeshParts)
                {
                    var instancedModelPart = new InstancedModelPart(_graphicsDevice, this, false, partIndex);

                    // Retrieve InstancedModelPartExtra instance
                    instancedModelPart.InstancedModelPartExtra = (InstancedModelPartExtra)meshPart.Tag;

                    // Attach to MeshPart
                    instancedModelPart.XnaModelMeshPart = meshPart;

                    // Save BoundingSphere
                    instancedModelPart.BoundingSphere = boundingSphere;

                    // Save Mesh Name
                    instancedModelPart.ModelPartName.Append(mesh.Name);

                    // Create InstancedModelPart
                    _modelParts.Add(instancedModelPart);

                    partIndex++;
                }
            }
        }

        // 3/23/2011
        /// <summary>
        /// Updates the <see cref="InstancedModelPart.BoneOffsetIndex"/> value for all
        /// <see cref="InstancedModelPart"/> instances.
        /// </summary>
        private void UpdateInstancedModelPartsBoneIndexes()
        {
            for (var i = 0; i < _modelParts.Count; i++)
            {
                // Locate bone name, to get index of array
                var modelBonesCount = _bonesCollection.Count; // 8/12/2009; 1/28/2010 - was '_model'.
                for (var j = 0; j < modelBonesCount; j++)
                {
                    // Found bone name.
                    var modelBone = _bonesCollection[j]; // 8/12/2009; 1/28/2010 - was '_model'.
                    if (modelBone.Name != _modelParts[i].ModelPartName.ToString()) continue;

                    // Copy Absolute Bone Offset Index, and break out of search loop                         
                    _modelParts[i].BoneOffsetIndex = modelBone.Index;

                    break;
                } // End Loop Model.Bones
            }
        }

        /// <summary>
        /// Method helper, which reads in the <see cref="InstancedModelBone"/> attributes, including
        /// name, transform, parent indexes, and root bone node.
        /// </summary>
        /// <param name="xnaBones">Instance of <see cref="ModelBoneCollection"/></param>
        /// <summary>
        /// Method helper, which reads in the <see cref="InstancedModelBone"/> attributes, including
        /// name, transform, parent indexes, and root bone node.
        /// </summary>
        private void ReadBones(ModelBoneCollection xnaBones)
        {
            var bones = new InstancedModelBone[xnaBones.Count];
            for (var index = 0; index < xnaBones.Count; index++)
            {
                var bone = xnaBones[index];
    
                if (bone == null) continue;
                
                // create new instancedModelBone, and add to colletion.
                bones[index] = new InstancedModelBone(bone.Name, bone.Transform, bone.Index, (bone.Parent == null) ? -1 : bone.Parent.Index);
               
            }

            _bonesCollection = new InstancedModelBoneCollection(bones);

            // Now set the Parent bone refs.
            foreach (var instancedModelBone in bones)
            {
                if (instancedModelBone.ParentIndex == -1) continue;

                instancedModelBone.Parent = bones[instancedModelBone.ParentIndex];
            }
            // Store Root Bone index
            _bonesCollection.Root = (XnaModel.Root == null) ? null : bones[XnaModel.Root.Index];

         
        }
        

        // 12/9/2008
        /// <summary>
        /// Call this method if you want to affect the rotation of the <see cref="InstancedModel"/> 
        /// on the Display only!  In other words, this will not be used in the <see cref="Shape.WorldP"/> transform within the <see cref="SceneItem"/>.  
        /// This is useful if you need to rotate the models front to correct a rotation error, but don't want this
        /// rotation value to be included in any Orientation <see cref="AbstractBehavior"/>.
        /// </summary>
        public void ApplyRotationValuesToRootTranform()
        {
            // Make sure we only apply it once; othewise, all other instances of same
            // _model type will keep rotating around additional rotation values.
            if (_appliedRotationValues) return;

            _appliedRotationValues = true;

            // 3/2/2009 - check if _bonesCollection null                
            if (_bonesCollection == null)
                return;

            var sceneTrans = _bonesCollection.Root.Transform;
            var quaternion = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(RotY),
                                                               MathHelper.ToRadians(RotX),
                                                               MathHelper.ToRadians(RotZ));
            Matrix.Transform(ref sceneTrans, ref quaternion, out sceneTrans);
            _bonesCollection.Root.Transform = sceneTrans;
        }
       

        #endregion

        #region Technique Selection
               
      
        /// <summary>
        /// Sets this <see cref="InstancedModel"/> to use the given
        /// <see cref="InstancingTechnique"/> Enum.
        /// </summary>
        /// <param name="technique"><see cref="InstancingTechnique"/> to use.</param>
        public void SetInstancingTechnique(InstancingTechnique technique)
        {
            InstancingTechnique = technique;

            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            if (instancedModelParts == null) return;

            switch (technique)
            {
                case InstancingTechnique.HardwareInstancing:
                case InstancingTechnique.HardwareInstancingAlphaDraw:
                    InitializeHardwareInstancing();
                    break;
                default:
                    break;
            }

            //_techniqueChanged = true;
           
        }

        /// <summary>
        /// Initializes geometry to use the HardwareInstancing technique.
        /// </summary>        
        private void InitializeHardwareInstancing()
        {
            // When using hardware instancing, the instance Transform matrix is
            // specified using a second vertex stream that provides 4x4 matrices
            // in texture coordinate channels 1 to 4. We must modify our vertex
            // declaration to include these channels.
            var extraElements = new VertexElement[6]; // 8/28/2009 - Extend for additional data.

            short offset = 0;
            byte usageIndex = 0; // XNA 4.0 - Updated from 1 to 0.
            //const short stream = 2; // XNA 4.0 - Now set stream using the SetVertexBuffer call on graphicsDevice.

            // 9/23/2010 - XNA 4.0 Updates
            // Set the Matrix 1st, which is only through channel 4.
            for (var i = 0; i < 4; i++)
            {
                extraElements[i] = new VertexElement(offset,
                                                VertexElementFormat.Vector4,
                                                VertexElementUsage.BlendWeight, // XNA 4.0 - Update to 'BlendWeight' from 'TextureCoordinate'.
                                                usageIndex);

                offset += SizeOfVector4;
                usageIndex++;
            }

            // XNA 4.0 Updates - Add 1 to usage to sync with shader TextureCoordinate.
            usageIndex++;

            // 6/6/2010 - Updated to Vector2, so the 'AccumTime' value can be stored.
            // 8/28/2009 - Extend VertexElement array, with new 'PlayerNumber' value, in channel 5.
            extraElements[4] = new VertexElement(offset,
                                                VertexElementFormat.Vector2,
                                                VertexElementUsage.TextureCoordinate,
                                                usageIndex);

            // 6/6/2010 - Extend VertexElement array, with new 'ProjectileVelocity' value, in channel 7;
            //            Channel 6 is already used by stream#1 for Velocity.
            usageIndex += 2;
            offset += sizeof(float) * 2;
            extraElements[5] = new VertexElement(offset,
                                                VertexElementFormat.Vector3,
                                                VertexElementUsage.TextureCoordinate,
                                                usageIndex);


            //ExtendVertexDeclaration(ref extraElements);

            // XNA 4.0 Updates
            // Create a new vertex declaration.
            InstanceVertexDeclaration = new VertexDeclaration(extraElements);

        }


        // XNA 4.0 Updates - GraphicsDeviceCapabilities obsolete!
        // Note: Just removed, since not used in engine at all.
        /*/// <summary>
        /// Checks whether the specified <see cref="InstancingTechnique"/> technique
        /// is supported by the current graphics Device.
        /// </summary>
        public static bool IsTechniqueSupported(InstancingTechnique technique)
        {
#if !XBOX360
            // Hardware instancing is only supported on pixel shader 3.0 devices.
            if (technique == InstancingTechnique.HardwareInstancing)
            {
                return _graphicsDevice.GraphicsDeviceCapabilities
                           .PixelShaderVersion.Major >= 3;
            }
#endif

            // Otherwise, everything is good.
            return true;
        }*/

        #endregion

        // 6/13/2010; 6/14/2010
        ///<summary>
        /// Processes all changes requests within the ChangeBuffers for given
        /// <see cref="InstancedModel"/>.
        ///</summary>
        ///<param name="instancedModel"><see cref="InstancedModel"/> instance</param>
        public static void ProcessDoubleBuffers(InstancedModel instancedModel)
        {
#if DEBUG
            // 4/21/2010 - Debug Purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IMPProcBuffs);
#endif
            //if (instancedModel.ItemTypeInUse == ItemType.treePalmNew002c)
              //  Debugger.Break();

            try
            {
                var instancedModelParts = instancedModel._modelParts;
                var modelPartsKeys = instancedModel.ModelPartsKeys;

                // 6/16/2010 - Check if null
                if (modelPartsKeys == null) return;

                // Iterate ModelParts
                var modelPartsKeysCount = modelPartsKeys.Count; // 8/12/2009
                for (var i = 0; i < modelPartsKeysCount; i++)
                {
                    // Cache data 
                    var modelPartsIndex = modelPartsKeys[i];

                    // Cache modelPart 
                    var modelPart = instancedModelParts[modelPartsIndex];
                    if (modelPart == null) continue;

                    // Process DoubleBuffer at ModePart level.
                    InstancedModelPart.ProcessDoubleBuffers(modelPart);

                } // End For ModelParts Loop

                // 6/14/2010 - Check for Explosion processing
                if (!instancedModel._drawExplosionPiecesToo) return;
                // Iterate Explosion ModelParts
                modelPartsKeys = instancedModel._explosionPartsKeys;

                // 6/16/2010 - Check if null
                if (modelPartsKeys == null) return;

                modelPartsKeysCount = modelPartsKeys.Count;
                for (var i = 0; i < modelPartsKeysCount; i++)
                {
                    // Cache data 
                    var modelPartsIndex = modelPartsKeys[i];

                    // Cache modelPart 
                    var modelPart = instancedModelParts[modelPartsIndex];
                    if (modelPart == null) continue;

                    // Process DoubleBuffer at ModePart level.
                    InstancedModelPart.ProcessDoubleBuffers(modelPart);

                } // End For ModelParts Loop
            }
            finally
            {
#if DEBUG
                // 4/21/2010 - Debug Purposes
                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMPProcBuffs);
#endif 
            }

        }

        // 5/24/2010: Updated method to be STATIC.
        // 5/19/2009: Removed the params 'View', 'Projection', & 'LightPos' since these are avaible as STATIC variables!
        /// <summary>
        /// Draws a batch of culled <see cref="InstancedModel"/> items, using proper <see cref="Effect"/> type.
        /// </summary>
        /// <param name="instancedModel">this instance of <see cref="InstancedModel"/></param>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param>
        public static void DrawCulledInstances(InstancedModel instancedModel, GameTime gameTime)
        {
            // 4/20/2010 - Cache
            var instanceWorldTransforms = instancedModel.InstanceWorldTransforms; 
            if (instanceWorldTransforms.Count == 0) return;

#if DEBUG
            // 3/28/2009 - TODO: Debug Purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IMDraw);//"IM-Draw"
#endif

            // 4/20/2010 - Cache values
            var instancedModelParts = instancedModel._modelParts; 
            var modelPartsKeys = instancedModel.ModelPartsKeys;
            var drawExplosionPiecesToo = instancedModel._drawExplosionPiecesToo;
           
            
            // Draw Parts
            DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.NormalTransforms_Culled,
                           instancedModelParts, modelPartsKeys, gameTime);
#if DEBUG
            // 3/28/2009 - TODO: Debug Purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMDraw);//"IM-Draw"
#endif

            // 3/26/2011 - Note: Need to fix explosion code.
            return;

            // 2/15/2010 - Explosion Pieces are drawn in same draw call, when true!
            if (drawExplosionPiecesToo)
            {
                // 4/20/2010 - Cache
                var explosionPartsKeys = instancedModel._explosionPartsKeys;
                if (explosionPartsKeys == null) return;

                DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.ExplosionTransforms_Culled,
                               instancedModelParts, explosionPartsKeys, gameTime);
            }

           
        }

        // 5/24/2010: Updated method to be STATIC.
        /// <summary>
        /// Draws a batch of culled <see cref="InstancedModel"/> items, using shadow <see cref="Effect"/> type.
        /// </summary>
        /// <param name="instancedModel">this instance of <see cref="InstancedModel"/></param>
        /// <param name="lightView"><see cref="ShadowMap"/> LightView matrix</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> LightProjection matrix</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void DrawCulledInstances(InstancedModel instancedModel, ref Matrix lightView, ref Matrix lightProj, GameTime gameTime)
        {
            // 6/10/2009
            if (instancedModel.InstanceWorldTransforms.Count == 0)
                return;

#if DEBUG
            // 3/28/2009 - Debug Purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IMDraw);//"IM-Draw"
#endif

            // 4/20/2010 - Cache values
            var instancedModelParts = instancedModel._modelParts;
            var modelPartsKeys = instancedModel.ModelPartsKeys;
            var drawExplosionPiecesToo = instancedModel._drawExplosionPiecesToo;
            
            // Draw Parts
            DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.NormalTransforms_Culled, instancedModelParts, modelPartsKeys,
                           ref lightView, ref lightProj, gameTime, false);

#if DEBUG
            // 3/28/2009 - Debug Purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMDraw);//"IM-Draw"
#endif

            // 3/26/2011 - Note: Need to fix explosion code.
            return;

            // 2/15/2010 - Explosion Pieces are drawn in same draw call, when true!
            if (drawExplosionPiecesToo)
            {
                // 4/20/2010 - Cache
                var explosionPartsKeys = instancedModel._explosionPartsKeys;
                if (explosionPartsKeys == null) return;

                DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.ExplosionTransforms_Culled, instancedModelParts,
                               explosionPartsKeys, ref lightView, ref lightProj, gameTime, false);
            }

        }



        // 3/7/2009; // 5/24/2010: Updated method to be STATIC.
        /// <summary>
        /// Draw a batch of <see cref="InstancedModel"/> items, for all instances!  (No culling is performed.)
        /// </summary>
        /// <param name="instancedModel">this instance of <see cref="InstancedModel"/></param>
        /// <param name="lightView"><see cref="ShadowMap"/> LightView matrix</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> LightProjection matrix</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="isStaticShadows">Is Draw call for Static shadow maps?</param>
        public static void DrawAllInstances(InstancedModel instancedModel, ref Matrix lightView, ref Matrix lightProj, GameTime gameTime, bool isStaticShadows)
        {
            // 6/10/2009
            if (instancedModel.InstanceWorldTransforms.Count == 0) return;
           

            // 4/20/2010 - Cache values
            var instancedModelParts = instancedModel._modelParts;
            var modelPartsKeys = instancedModel.ModelPartsKeys;
            var drawExplosionPiecesToo = instancedModel._drawExplosionPiecesToo;
           
            // Draw Parts
            DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.NormalTransforms_All, instancedModelParts, modelPartsKeys,
                           ref lightView, ref lightProj, gameTime, isStaticShadows);


            // 3/26/2011 - Note: Need to fix explosion code.
            return;

            // 2/15/2010 - Explosion Pieces are drawn in same draw call, when true!
            if (!drawExplosionPiecesToo) return;

            // 4/20/2010 - Cache
            var explosionPartsKeys = instancedModel._explosionPartsKeys;
            if (explosionPartsKeys == null) return;

            DrawModelParts(instancedModel.InstancingTechnique, DrawTransformsType.ExplosionTransforms_Culled, instancedModelParts,
                           explosionPartsKeys, ref lightView, ref lightProj, gameTime, isStaticShadows);
        }

        // 6/10/2009
        /// <summary>
        /// Calls the <see cref="InstancedModelPart"/> DrawShadow method, passing in the <see cref="DrawTransformsType"/>, 
        /// which specifies to draw either culled or all transforms.
        /// </summary>
        /// <param name="technique"><see cref="InstancingTechnique"/> used for this model</param>
        /// <param name="drawTransformsType"><see cref="DrawTransformsType"/> to use</param>
        /// <param name="modelParts">Array of <see cref="InstancedModelPart"/> to draw</param>
        /// <param name="modelPartKeys">Array of Keys which index the <see cref="InstancedModelPart"/> array</param>
        /// <param name="lightView"><see cref="ShadowMap"/> LightView matrix</param>
        /// <param name="lightProj"><see cref="ShadowMap"/> LightProjection matrix</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="isStaticShadows">Is Draw call for Static shadow maps?</param>
        private static void DrawModelParts(InstancingTechnique technique, DrawTransformsType drawTransformsType, 
                                            IList<InstancedModelPart> modelParts, IList<int> modelPartKeys, 
                                            ref Matrix lightView, ref Matrix lightProj, GameTime gameTime, bool isStaticShadows)
        {
            // 2/15/2010 - Check for null
            if (modelPartKeys == null || modelParts == null)
                return;

            // 3/28/2009: Updated to Loop using the Keys array.             
            var modelPartsKeysCount = modelPartKeys.Count; // 8/12/2009
            for (var loop1 = 0; loop1 < modelPartsKeysCount; loop1++)
            {
                // 4/13/2009 - Cache data to improve CPI in VTUNE!
                var modelPartsIndex = modelPartKeys[loop1];

                // 8/6/2009 - Cache modelPart to improve perm.
                var modelPart = modelParts[modelPartsIndex];
                if (modelPart == null) continue; // 4/20/2010

                // 7/14/2009 - Updated to set on ALL _modelParts, otherwise, Settings don't propagate correctly for the PC, like they
                //            do on the XBOX!
                InstancedModelPart.SetStaticRenderStates(modelPart, ref lightView, ref lightProj);

                // 2/11/2010 - Updated to call 'DrawShadows'.
                // Draw Batch of ModelParts  
                InstancedModelPart.DrawShadows(modelPart, technique, drawTransformsType, gameTime, isStaticShadows);
            } // End For ModelParts Loop 
        }


        /// <summary>
        /// Calls the <see cref="InstancedModelPart"/> 'Draw' method, passing in the <see cref="DrawTransformsType"/>, 
        /// which specifies to draw culled or all transforms.
        /// </summary>
        /// <param name="technique"><see cref="InstancingTechnique"/>  used for this model</param>
        /// <param name="drawTransformsType"><see cref="DrawTransformsType"/> to use</param>
        /// <param name="modelParts">Array of <see cref="InstancedModelPart"/> to draw</param>
        /// <param name="modelPartKeys">Array of Keys which index the <see cref="InstancedModelPart"/> array</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DrawModelParts(InstancingTechnique technique, DrawTransformsType drawTransformsType, IList<InstancedModelPart> modelParts, 
                                        IList<int> modelPartKeys, GameTime gameTime)
        {
            // 2/15/2010 - Check for null
            if (modelPartKeys == null || modelParts == null)
                return;

            #region Xna Draw Testing Code
            // TEST
            /*foreach (ModelMesh mesh in instancedModel.XnaModel.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Retrieve InstancedModelPart
                    var instancedModelPart = (InstancedModelPart)meshPart.Tag;

                    int instanceCount = instancedModelPart.TransformsToDrawAllList.Count;
                    if (instanceCount == 0) continue;

                    if (instancedModel._instanceTransforms.Length < instanceCount)
                        Array.Resize(ref instancedModel._instanceTransforms, instanceCount);

                    int index = 0;
                    foreach (var instancedDataForDraw in instancedModelPart.TransformsToDrawAllList)
                    {
                        instancedModel._instanceTransforms[index] = instancedDataForDraw.Value.Transform;
                        index++;
                    }

                    var instanceVertexBuffer = instancedModelPart._instanceVertexBuffer;

                    // If we have more instances than room in our vertex buffer, grow it to the neccessary size.
                    if ((instanceVertexBuffer == null) ||
                        (instanceCount > instanceVertexBuffer.VertexCount))
                    {
                        if (instanceVertexBuffer != null)
                            instanceVertexBuffer.Dispose();

                        instancedModelPart._instanceVertexBuffer = new DynamicVertexBuffer(graphicsDevice, instancedModelPart.InstanceVertexDeclaration,
                                                                       instanceCount, BufferUsage.WriteOnly);

                        instanceVertexBuffer = instancedModelPart._instanceVertexBuffer; 
                    }

                    // Transfer the latest instance transform matrices into the instanceVertexBuffer.
                    instanceVertexBuffer.SetData(instancedModel._instanceTransforms, 0, instanceCount, SetDataOptions.Discard);

                     // Tell the GPU to read from both the model vertex buffer plus our instanceVertexBuffer.
                    graphicsDevice.SetVertexBuffers(
                        new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0),
                        new VertexBufferBinding(instancedModelPart._instanceVertexBuffer, 0, 1)
                    );

                    graphicsDevice.Indices = meshPart.IndexBuffer;

                    // Set up the instance rendering effect.
                    var effect = meshPart.Effect;

                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];

                    //effect.Parameters["World"].SetValue(instancedModel._instancedModelBones[mesh.ParentBone.Index]); // _bonesCollection
                    effect.Parameters["World"].SetValue(instancedModel._bonesCollection[instancedModelPart.BoneOffsetIndex].Transform);
                    effect.Parameters["View"].SetValue(Camera.View);
                    effect.Parameters["Projection"].SetValue(Camera.Projection);

                    // Draw all the instance copies in a single call.
                    foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();

                        graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                               meshPart.NumVertices, meshPart.StartIndex,
                                                               meshPart.PrimitiveCount, 
                                                               instanceCount);
                    }
                    
                }
            }*/
            #endregion

            // 3/28/2009: Updated to Loop using the Keys array.             
            var modelPartsKeysCount = modelPartKeys.Count; // 8/12/2009
            for (var loop1 = 0; loop1 < modelPartsKeysCount; loop1++)
            {
                // 4/13/2009 - Cache data to improve CPI in VTUNE!
                var modelPartsIndex = modelPartKeys[loop1];

                // 8/6/2009 - Cache modelPart to improve perm.
                //var modelPart = modelParts[modelPartsIndex];
                var modelPart = modelParts[modelPartsIndex];
                if (modelPart == null) continue; // 4/20/2010

                // 7/14/2009 - Updated to set on ALL _modelParts, otherwise, Settings don't propagate correctly for the PC, like they
                //            do on the XBOX!
                InstancedModelPart.SetStaticRenderStates(modelPart);
                
                // Draw Batch of ModelParts  
                InstancedModelPart.Draw(modelPart, technique, drawTransformsType, gameTime);

            } // End For ModelParts Loop 

        }

        // 6/18/2010
        /// <summary>
        /// Updates a specific effect parameter with given <see cref="Vector3"/> value, for
        /// all <see cref="InstancedModelPart"/> in 'Normal' set; not 'Explosion' set.
        /// </summary>
        /// <param name="effectParamName">EffectParam string name</param>
        /// <param name="valueToSet"><see cref="Vector3"/> value to set</param>
        public void SetSpecificEffectParam(string effectParamName, Vector3 valueToSet)
        {
            // TODO: restore once original Effect in place.
            //return;

            // itereate the Normal modelParts to retrieve Effect.
            foreach (var normalModelPart in IterateNormalModelParts())
            {
                var effectParameter = normalModelPart.Effect.Parameters[effectParamName];
                if (effectParameter == null)
                    throw new ArgumentException("EffectParam name given does not exist.");

                effectParameter.SetValue(valueToSet);
            }
        }

        /// <summary>
        /// Updates a specific effect parameter with given <see cref="float"/> value, for
        /// all <see cref="InstancedModelPart"/> in 'Normal' set; not 'Explosion' set.
        /// </summary>
        /// <param name="effectParamName">EffectParam string name</param>
        /// <param name="valueToSet"><see cref="float"/> value to set</param>
        public void SetSpecificEffectParam(string effectParamName, float valueToSet)
        {
            // TODO: restore once original Effect in place.
            //return;

            // itereate the Normal modelParts to retrieve Effect.
            foreach (var normalModelPart in IterateNormalModelParts())
            {
                var effectParameter = normalModelPart.Effect.Parameters[effectParamName];
                if (effectParameter == null)
                    throw new ArgumentException("EffectParam name given does not exist.");

                effectParameter.SetValue(valueToSet);
            }
        }

        // 2/3/2010
        /// <summary>
        /// Used to set the <see cref="InstancedModelPart.ProceduralMaterialId"/> for the given <see cref="InstancedModel"/> type,
        /// for a given <see cref="InstancedModelPart"/>; set <see cref="InstancedModelPart"/> index to null, for all <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="materialId"/> set, will apply to ALL instances of the given 
        /// <see cref="InstancedModel"/> type.
        /// </remarks>
        /// <param name="materialId"><see cref="InstancedModelPart.ProceduralMaterialId"/> to apply to model; lookup ID using the 'LightingShaders.HLSL' materials file.</param>
        /// <param name="modelPartIndexKey">ModelPartIndex key to apply Id to; set to null to apply to all.</param>
        public void AssignProceduralMaterialId(int materialId, int? modelPartIndexKey)
        {
            IList<int> modelPartsKeys = ModelPartsKeys;
            var modelPartsKeysCount = ModelPartsCount;

            for (var i = 0; i < modelPartsKeysCount; i++)
            {
                var modelPartIndex = modelPartsKeys[i];
                var modelPart = _modelParts[modelPartIndex];
                if (modelPart == null) continue;

                // check if for specific model part.
                if (modelPartIndexKey != null && modelPartIndexKey.Value != modelPartIndex) continue;

                // Assign new MaterialId
                modelPart.ProceduralMaterialId = materialId;
            }
        }

        // 6/18/2010 - Overload version.
        /// <summary>
        /// Used to set the <see cref="InstancedModelPart.ProceduralMaterialId"/> for the given <see cref="InstancedModel"/> type,
        /// for a given <see cref="InstancedModelPart"/>; set <see cref="InstancedModelPart"/> index to null, for all <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="materialId"/> set, will apply to ALL instances of the given 
        /// <see cref="InstancedModel"/> type.
        /// </remarks>
        /// <param name="materialId"><see cref="Enums.ShaderToUseEnum"/> to apply to model.</param>
        /// <param name="modelPartIndexKey">ModelPartIndex key to apply Id to; set to null to apply to all.</param>
        public void AssignProceduralMaterialId(ShaderToUseEnum materialId, int? modelPartIndexKey)
        {
            AssignProceduralMaterialId((int) materialId, modelPartIndexKey);
        }

#if !XBOX360
        // 2/4/2010
        /// <summary>
        /// Sets the given <see cref="ProceduralMaterialParameters"/> parameter, to the given new value,
        /// for a given <see cref="InstancedModelPart"/>; set <see cref="InstancedModelPart"/> index to null, for all <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <remarks>
        /// The <paramref name="newValue"/> set, will apply to ALL instances of the given 
        /// <see cref="InstancedModel"/> type.
        /// </remarks>
        /// <param name="parameterToUpdate">Choose <see cref="ProceduralMaterialParameters"/> parameter to update</param>
        /// <param name="newValue">New value to set</param>
        /// <param name="modelPartIndexKey">ModelPartIndex key to apply Id to; set to null to apply to all.</param>
        public void SetProceduralMaterialParameter(ProceduralMaterialParameters parameterToUpdate, object newValue, int? modelPartIndexKey)
        {
            // itereate the Normal modelParts to retrieve part names.
            foreach (var normalModelPart in IterateNormalModelParts())
            {
                // check if for specific model part.
                if (modelPartIndexKey != null && modelPartIndexKey.Value != normalModelPart.ModelPartIndexKey) continue;

                // Assign new Material parameter value.
                InstancedModelPartExtra.InstancedModelCustomMaterials.SetProceduralMaterialParameter(parameterToUpdate, normalModelPart.Effect, newValue);
                if (modelPartIndexKey != null) break;
            }

        }
        

        // 2/10/2010
        /// <summary>
        /// Gets the given <see cref="ProceduralMaterialParameters"/> parameters value, for a given <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <param name="parameterToRetrieve">Choose <see cref="ProceduralMaterialParameters"/> parameter to retrieve</param>
        /// <param name="modelPartIndexKey">ModelPartIndex key to retrieve value for</param>
        /// <param name="newValue">(OUT) value</param>
        /// <param name="type">(OUT) value Type</param>
        public void GetProceduralMaterialParameter(ProceduralMaterialParameters parameterToRetrieve, int modelPartIndexKey, 
                                                    out object newValue, out Type type)
        {
            newValue = null;
            type = default(Type);

            // itereate the Normal modelParts to retrieve part names.
            foreach (var normalModelPart in IterateNormalModelParts())
            {
                // check if for specific model part.
                if (modelPartIndexKey != normalModelPart.ModelPartIndexKey) continue;

                // Retrieve Material parameter value.
                InstancedModelPartExtra.InstancedModelCustomMaterials.GetProceduralMaterialParameter(parameterToRetrieve, normalModelPart.Effect, out newValue, out type);
                break;
            }
        }

        // 2/10/2010
        /// <summary>
        /// Gets the given <see cref="InstancedModelPart.ProceduralMaterialId"/>, for a given <see cref="InstancedModelPart"/>.  
        /// If <see cref="InstancedModelPart"/> not found, then will return the ID from the 1st <see cref="InstancedModelPart"/>
        /// in array; if no <see cref="InstancedModelPart"/> at all, just returns zero.
        /// </summary>
        /// <param name="modelPartIndexKey">ModelPartIndex key to retrieve Material ID</param>
        /// <param name="materialId">(OUT) Material ID</param>
        /// <returns>True/False of result</returns>
        public bool GetProceduralMaterialId(int modelPartIndexKey, out int materialId)
        {
            // itereate the Normal modelParts to retrieve part names.
            foreach (var normalModelPart in IterateNormalModelParts())
            {
                // check if for specific model part.
                if (modelPartIndexKey != normalModelPart.ModelPartIndexKey) continue;

                materialId = normalModelPart.ProceduralMaterialId;
                return true;
            }

            // Key not found, so just return from first modelPart
            if (_modelParts[0] != null)
            {
                materialId = _modelParts[0].ProceduralMaterialId;
                return true;
            }

            materialId = 0;
            return false;
        }
#endif

        // 7/9/2009
        /// <summary>
        /// This method is called indirectly from the <see cref="TerrainQuadTree"/> via the <see cref="InstancedItem"/> class, 
        /// when drawing a specific quad which is in <see cref="Camera"/> view.  Then, the internal list of instanceItemKeys
        /// are passed to this method, which then populates the <see cref="InstancedModelPart.TransformsToDrawList"/> for the
        /// next draw call.
        /// </summary>
        /// <param name="itemsInView">List of instanceItemKeys within <see cref="Camera"/> view.</param>
        public void CreateScenaryInstancesCulledList(List<int> itemsInView)
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            if (instancedModelParts == null) return;

            // iterate through itemsInView list, searching the InstanceWorldTransforms dictionary, for the given key.
            var itemsInViewCount = itemsInView.Count; // 8/12/2009
           
            for (var i = 0; i < itemsInViewCount; i++)
            {
                // search 'InstanceWorldTransforms' dictionary for given key.
                InstancedDataCommunication instanceWorldData;
                if (!InstanceWorldTransforms.TryGetValue(itemsInView[i], out instanceWorldData)) continue;

                // Iterate each ModelPart to update.
                var modelPartsCount = instancedModelParts.Count; // 8/12/2009
                for (var modelPartIndex = 0; modelPartIndex < modelPartsCount; modelPartIndex++)
                {
                    // 3/25/2011 - Scenary items with bones, like trees, need to be calculated for Absolute transforms.
                    var transformResult = instanceWorldData.Transform;
                    if (!UseBakedTransforms)
                    {
                        CopyAbsoluteBoneTranformsTo(this, instanceWorldData.ItemInstanceKey);

                        // Optimize by removing Matrix Overload operations, which are slow on XBOX!                        
                        var tmpTransform = instanceWorldData.Transform;
                        // 6/17/2010 - was cast to (IDictionary<int, InstancedItemTransform[]>)
                        Matrix.Multiply(ref _absoluteBoneTransforms[instanceWorldData.ItemInstanceKey][((IList<InstancedModelPart>)_modelParts)[modelPartIndex].BoneOffsetIndex].AbsoluteTransform,
                            ref tmpTransform, out transformResult); // was 'out tmpTransformsToDraw[i]
                    }

                    // 7/21/2009 - Create ChangeRequestItem
                    var changeRequestItem = new ChangeRequestItem
                                                {
                                                    ChangeRequest = ChangeRequest.AddUpdateSceneryPart_InstanceItem,
                                                    Transform = transformResult
                                                };

                    InstancedModelChangeRequests.EnterChangeRequestItemToCurrentChangeBuffer(modelPartIndex, instanceWorldData.ItemInstanceKey,
                                                                ref changeRequestItem, instancedModelParts);
                } // End Loop ModelParts                    
            } // End Loop itemsInView array.
        }

        // 7/10/2009
        /// <summary>
        /// This method is called directly from the <see cref="InstancedItem"/> classes 'Draw' method, which clears
        /// out this <see cref="InstancedModel"/> 'Culled' list.
        /// </summary>
        public void ClearScenaryInstancesCulledList()
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            if (instancedModelParts == null) return;

            var modelPartsCount = instancedModelParts.Count; // 8/12/2009
            for (var modelPartIndex = 0; modelPartIndex < modelPartsCount; modelPartIndex++)
            {
                // 7/21/2009 - Create ChangeRequestItem
                var changeRequestItem = new ChangeRequestItem
                                            {
                                                ChangeRequest = ChangeRequest.DeleteAllCulledParts_AllItems,
                                                Transform = MatrixIdentity
                                            };

                // ItemIndexKey is set to Zero, which always makes as first request.
                InstancedModelChangeRequests.EnterChangeRequestItemToCurrentChangeBuffer(modelPartIndex, 0, ref changeRequestItem, instancedModelParts);
               
            }
        }

        // 6/17/2010 - Used to store a local copy when iterating the collection in the 'SetDrawExplosionPiecesFlag' method.
        private InstancedDataCommunication[] _localCopyInstanceWorldTransforms = new InstancedDataCommunication[1];

        // 7/24/2009; // 6/17/2010: Updated removing Lambda expression, and instead use simply array.
        /// <summary>
        /// Check if the dictionary <see cref="InstanceWorldTransforms"/> has
        /// some instance with the <see cref="InstancedDataCommunication.DrawWithExplodePieces"/> set to TRUE.  
        /// The <see cref="_drawExplosionPiecesToo"/> flag is updated with the given result.   
        /// </summary>
        /// <remarks>
        /// Ultimately, this is used in the draw methods to eliminate
        /// calls to drawing <see cref="SceneItem"/> 'ExplosionPieces', until a <see cref="SceneItem"/> is actually exploding.
        /// </remarks>
        public void SetDrawExplosionPiecesFlag()
        {
            //_drawExplosionPiecesToo = InstanceWorldTransforms.Count(x => x.Value.DrawWithExplodePieces) > 0;

            // Copy dictionary values into local copy, from thread copy.
            var indexCount = InstanceWorldTransforms.Values.Count;
            var copyLength = _localCopyInstanceWorldTransforms.Length;
            if (copyLength < indexCount)
                Array.Resize(ref _localCopyInstanceWorldTransforms, indexCount);

            InstanceWorldTransforms.Values.CopyTo(_localCopyInstanceWorldTransforms, 0);

            // iterate collection, counting number of items with 'DrawWithExplodePieces' TRUE.
            _drawExplosionPiecesToo = false;
            for (var i = 0; i < copyLength; i++)
            {
                if (!_localCopyInstanceWorldTransforms[i].DrawWithExplodePieces) continue;
                // Found at least one explosion piece, so set TRUE and exit loop.
                _drawExplosionPiecesToo = true;
                break;
            }

        }

        #region Obsolete Animation Methods

        // 1/9/2009
        /*/// <summary>
        /// Updates the Animations for any Bones within the <see cref="ItemType"/> for 
        /// all ItemInstances of the <see cref="ItemType"/>.
        /// </summary> 
        /// <param name="itemType"><see cref="ItemType"/> to use</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance.</param> 
        public void UpdateAnimations(ItemType itemType, GameTime gameTime)
        {
            // Get Keys to Dictionary
            if (_keys.Length != InstanceWorldTransforms.Keys.Count)
                Array.Resize(ref _keys, InstanceWorldTransforms.Keys.Count);
            InstanceWorldTransforms.Keys.CopyTo(_keys, 0);

            // Iterate through Dictionary Keys
            for (var i = 0; i < _keys.Length; i++)
            {
                // Skip any items without animations.
                if (!_instancedModelAnimates)
                    continue;

                // 1/14/2009 - Skip any items not in Camera view
                if (!InstanceWorldTransforms[_keys[i]].InCameraView)
                    continue;

                // Check if Dictionary has animation Settings yet
                if (_instancedModelAnimations.ContainsKey(InstanceWorldTransforms[_keys[i]].ItemInstanceKey))
                {
                    // Update Animation for the given ItemInstance.
                    _instancedModelAnimations[InstanceWorldTransforms[_keys[i]].ItemInstanceKey].UpdateAnimation(gameTime);
                }
                else // Add entry into Dictionary
                {
                    // Create new Instance of Animation class
                    var itemInstanceAnimation = new InstancedModelAnimation
                                                    {
                                                        InstancedItemData =
                                                            {
                                                                ItemType = itemType,
                                                                ItemInstanceKey =
                                                                    InstanceWorldTransforms[_keys[i]].ItemInstanceKey
                                                            },
                                                        Bone1AnimationType =
                                                            _instancedModelAnimation.Bone1AnimationType,
                                                        Bone1Name = _instancedModelAnimation.Bone1Name,
                                                        Bone1RotateOnAxis = _instancedModelAnimation.Bone1RotateOnAxis,
                                                        Bone1RotationSpeed =
                                                            _instancedModelAnimation.Bone1RotationSpeed,
                                                        Bone2Animates = _instancedModelAnimation.Bone2Animates,
                                                        Bone2AnimationType =
                                                            _instancedModelAnimation.Bone2AnimationType,
                                                        Bone2Name = _instancedModelAnimation.Bone2Name,
                                                        Bone2RotateOnAxis = _instancedModelAnimation.Bone2RotateOnAxis,
                                                        Bone2RotationSpeed =
                                                            _instancedModelAnimation.Bone2RotationSpeed
                                                    };

                    // Set ItemType / ItemInstanceKey

                    //
                    // Set animation atts using the global Settings for this ItemType.
                    //
                    // Bone-1
                    // Bone-2

                    // Add to Dictionary
                    _instancedModelAnimations.Add(InstanceWorldTransforms[_keys[i]].ItemInstanceKey,
                                                  itemInstanceAnimation);
                }
            } // End For InstanceWorldTransforms 
        }

        // 2/6/2009
        /// <summary>
        /// Removes an <see cref="ItemType"/> instance animation from the animation dictionary.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        public void RemoveInstanceModelAnimation(int itemInstanceKey)
        {
            if (_instancedModelAnimations.ContainsKey(itemInstanceKey))
            {
                _instancedModelAnimations[itemInstanceKey].IsDead = true;
            }
        }*/

        #endregion

        private int[] _keys = new int[1];

        // 2/16/2009
        /// <summary>
        /// Iterates through this <see cref="InstancedModelPart"/> collection, while calling
        /// the <see cref="UpdateInstanceTransforms"/> method, which updates all bone transforms 
        /// and stores the <see cref="Matrix"/> transforms of only the culled items.
        /// </summary>
        public void UpdateInstanceTransforms()
        {
            // 4/20/2010 - Cache
            var instancedDataCommunications = InstanceWorldTransforms;
            if (instancedDataCommunications == null) return;

            // 11/7/2009 - Thread Lock op; avoids the 'Count' changing before attempting the CopyTo.
            //lock (instancedDataCommunications.ThreadLock)
            {
                StopWatchTimers.StartStopWatchInstance(StopWatchName.UpdateTransforms);//"UpdateTransforms"

                // 4/13/2009 - Cache data to improve CPI in VTUNE!
                var keys = instancedDataCommunications.Keys; // 4/20/2010
                var instanceWorldCount = keys.Count;

                // 8/20/2009 - skip any processing, if empty.
                if (instanceWorldCount <= 0) return;

                // Get Keys to Dictionary
                if (_keys.Length < instanceWorldCount)
                    Array.Resize(ref _keys, instanceWorldCount);
                keys.CopyTo(_keys, 0);

                // Iterate through Dictionary Keys, which represents each instance of
                // this modelType.
                for (var i = 0; i < instanceWorldCount; i++)
                {
                    var instanceWorldData = instancedDataCommunications[_keys[i]];

                    // 7/25/2009
                    UpdateInstanceTransforms(ref instanceWorldData);
                }

                StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.UpdateTransforms);//"UpdateTransforms"

            } // End ThreadLock
        }

        // 7/21/2009 - 
        /// <summary>
        /// Update transforms for a specific instance, using <paramref name="itemInstanceKey"/> to retrieve.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        private void UpdateInstanceTransforms(int itemInstanceKey)
        {
            // retrieve the 'InstanceWorldData' node, using the given key
            InstancedDataCommunication instanceWorldData;
            if (InstanceWorldTransforms.TryGetValue(itemInstanceKey, out instanceWorldData))
            {
                UpdateInstanceTransforms(ref instanceWorldData);
            }
        }

        // 7/21/2009 - 
        /// <summary>
        /// Update transforms for a specific instance, using <see cref="InstancedDataCommunication"/> node to retrieve.
        /// </summary>
        /// <param name="instanceDataCommunication"><see cref="InstancedDataCommunication"/> structure</param>
        internal void UpdateInstanceTransforms(ref InstancedDataCommunication instanceDataCommunication)
        {
            // Draw with Exploding Parts?
            UpdateInstanceTransforms(this,
                                     instanceDataCommunication.DrawWithExplodePieces ? PartType.ExplosionPart : PartType.NormalPart,
                                     ref instanceDataCommunication);
            
        }

        /// <summary>
        /// Iterates through the current <see cref="InstancedModelPart"/> collection, updating all parts
        /// which are within the <see cref="Camera"/> view, using the data from the <see cref="InstancedDataCommunication"/>
        /// structure.  For each <see cref="InstancedModelPart"/> update required, a new <see cref="ChangeRequestItem"/> structure
        /// is created and added to the <see cref="InstancedModelChangeRequests"/> for processing.
        /// </summary>    
        /// <param name="instancedModel"><see cref="InstancedModel"/> instance to process</param>
        /// <param name="partType"><see cref="PartType"/> to process for</param>
        /// <param name="instanceDataCommunication"><see cref="InstancedDataCommunication"/> to retrieve data from</param>    
        private static void UpdateInstanceTransforms(InstancedModel instancedModel, PartType partType, ref InstancedDataCommunication instanceDataCommunication)
        {
            // 3/25/2011 - Skip any Scenary items, since update done in CreateScenaryInstancesCulledList().
            //if (instanceDataCommunication.IsSceneryItem) return;
            
            //if (instancedModel.ItemTypeInUse == ItemType.treePalmNew002c)
            //    Debugger.Break();

            // 8/27/2009 - PartType setting, to process either Explosion/Normal set
            IList<int> modelPartsKeys = null;
            var modelPartsKeysCount = 0;
            switch (partType)
            {
                case PartType.NormalPart:
                    modelPartsKeys = instancedModel.ModelPartsKeys;
                    modelPartsKeysCount = instancedModel.ModelPartsCount;
                    break;
                case PartType.ExplosionPart:
                    modelPartsKeys = instancedModel._explosionPartsKeys;
                    modelPartsKeysCount = instancedModel._explosionPartsCount;
                    break;
            }

            if (modelPartsKeys == null)
                return;
            
            // 2/16/2010 - TODO: Testing ParallelFor
            //_transformParallelFor.ParallelFor(instancedModel, partType, ref instanceData, modelPartsKeys, 0, modelPartsKeysCount);

            var bonesCollection = instancedModel._bonesCollection; // 1/28/2010; was '_model'.
            var useBakedTransforms = instancedModel.UseBakedTransforms;

            // 2/3/2010: Updated to now check if the 'InstanceData' struct is restricted to one model-part.
            // Iterate through the _modelParts, creating ChangeRequestItem's for each ModelPart!
            for (var i = 0; i < modelPartsKeysCount; i++)
            {
                var modelPartIndex = modelPartsKeys[i];

                // 2/3/2010 - Check if restricted to one modelpart.
                // Note: Its important to note, the check MUST be done using the internal '_modelPartIndexKey', and NOT the Property!
                var restrictToModelPart = (instanceDataCommunication._modelPartIndexKey > 0 && instanceDataCommunication.ModelPartIndexKey != modelPartIndex);

                // Create new ChangeRequestItem
                var changeRequestItem = new ChangeRequestItem
                                            {
                                                ChangeRequest = ChangeRequest.AddUpdatePart_InstanceItem,
                                                PlayerNumber = instanceDataCommunication.PlayerNumber, // 8/28/2009
                                                ProceduralMaterialId = (restrictToModelPart) ? 0 : instanceDataCommunication.ProceduralMaterialId, // 2/3/2010
                                                ProjectileVelocity = instancedModel.RetrieveBoneExplosionVelocity(instancedModel._currentExplosionBoneName, instanceDataCommunication.ItemInstanceKey), // 6/6/2010; 1/17/2010
                                                ShowFlashWhite = instanceDataCommunication.ShowFlashWhite, // 10/12/2009 (Scripting Purposes)
                                                PartType = partType // 7/24/2009
                                            };

                // Only add if in camera view.
                if (instanceDataCommunication.InCameraView)
                {
                    // Only Calc if NOT using BakedTransforms
                    var transformResult = instanceDataCommunication.Transform;
                    if (!useBakedTransforms)
                    {
                        CopyAbsoluteBoneTranformsTo(instancedModel, instanceDataCommunication.ItemInstanceKey);

                        // Optimize by removing Matrix Overload operations, which are slow on XBOX!                        
                        var tmpTransform = instanceDataCommunication.Transform;
                        // 6/17/2010 - was cast to (IDictionary<int, InstancedItemTransform[]>)
                        Matrix.Multiply(ref instancedModel._absoluteBoneTransforms[instanceDataCommunication.ItemInstanceKey][((IList<InstancedModelPart>) instancedModel._modelParts)[modelPartIndex].BoneOffsetIndex].AbsoluteTransform,
                            ref tmpTransform, out transformResult); // was 'out tmpTransformsToDraw[i]
                    }
                   
                    // Update ChangeRequestItem                    
                    changeRequestItem.Transform = transformResult;
                }
                else // Update ChangeRequestItem                                
                    changeRequestItem.Transform = instanceDataCommunication.Transform;

                // 1/16/2011 - Updated to check if 'TerrainIsIn' playableMode before hiding units for FOW.
                // 4/21/2010 - Updated to check if 'ShapeItem' is null.
                // 1/30/2010 - Set Transform to zero, if FOW=false; 6/13/2010 - Not for Scenary items.
                if (TerrainShape.TerrainIsIn == TerrainIsIn.PlayableMode && !instanceDataCommunication.ShapeItem.IsFOWVisible) 
                    changeRequestItem.Transform = new Matrix();

                // NOTE: DEBUG
                if (instancedModel.ItemTypeInUse == ItemType.treePalmNew002c)
                    System.Console.WriteLine(string.Format("The UpdateTransforms = {0}.", changeRequestItem.Transform));

                InstancedModelChangeRequests.EnterChangeRequestItemToCurrentChangeBuffer(modelPartIndex, instanceDataCommunication.ItemInstanceKey,
                                                            ref changeRequestItem, instancedModel._modelParts);

                                        
            } // End Loop ModelPartKeys  
            
       
        }

        // 1/17/2011
        /// <summary>
        /// Add or updates the given <paramref name="velocity"/> for the given <paramref name="boneName"/>.
        /// </summary>
        /// <param name="boneName">Bone name to update.</param>
        ///<param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="velocity">New explosion velocity value as <see cref="Vector3"/>.</param>
        public void AddBoneExplosionVelocity(string boneName, int itemInstanceKey, ref Vector3 velocity)
        {
            if (String.IsNullOrEmpty(boneName))
                throw new ArgumentNullException("boneName", @"Bone name given CANNOT be null.");

            // Retrieve Explosion Dictionary for given instance Key
            Dictionary<string, Vector3> explosionDictionary;
            if (_explosionVelocities.TryGetValue(itemInstanceKey, out explosionDictionary))
            {
                // Add or update the new explosion velocity
                if (explosionDictionary.ContainsKey(boneName))
                {
                    // Update to new value
                    explosionDictionary[boneName] = velocity;
                }
                else
                {
                    // Add to new value to dictionary
                    explosionDictionary.Add(boneName, velocity);
                }
            }
            else
            {
                // Create new dictionary instance and add boneName entry.
                explosionDictionary = new Dictionary<string, Vector3> {{boneName, velocity}};

                // Add to main dictionary
                _explosionVelocities.Add(itemInstanceKey, explosionDictionary);
            }

        }

        // 1/17/2011
        /// <summary>
        /// Retrieves the explosion velocity for the given <paramref name="boneName"/>.
        /// </summary>
        /// <param name="boneName">>Bone name to retrieve velocity.</param>
        ///<param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <returns>Explosion velocity as <see cref="Vector3"/>.</returns>
        public Vector3 RetrieveBoneExplosionVelocity(string boneName, int itemInstanceKey)
        {
            if (String.IsNullOrEmpty(boneName))
                return Vector3.Zero;

            // Retrieve explosion dictionary
            Dictionary<string, Vector3> explosionDictionary;
            if (_explosionVelocities.TryGetValue(itemInstanceKey, out explosionDictionary))
            {
                //Debugger.Break();

                // Retrieve given boneName velocity
                Vector3 explosionVelocity;
                if (explosionDictionary.TryGetValue(boneName, out explosionVelocity))
                {
                    return explosionVelocity;
                }

                //throw new InvalidOperationException("Bone name given does not exist.");
#if DEBUG && !XBOX360
                Debug.WriteLine("RetrieveBoneExplosionVelocity method - Bone name given does not exist.", "Warning");
#endif
                return Vector3.One;

            }

            return Vector3.Zero;
        }

        // 8/18/2009
        ///<summary>
        /// Applies a 'Rotation' bone adjustment Transform to a specific <see cref="InstancedModel"/> bone.
        ///</summary>
        ///<param name="boneName">Bone name to affect</param>
        ///<param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        ///<param name="rotationAxis"><see cref="RotationAxis"/> Enum to use</param>
        ///<param name="rotationValue">Rotation value of some value 0 or greater.</param>
        public void SetAdjustingBoneTransform(string boneName, int itemInstanceKey, RotationAxis rotationAxis, float rotationValue)
        {
            // 8/23/2009: Updated to Return, if no update required.
            // Call Initial FBX 'SetAdjustingBoneTransform' method
            Matrix rotationTransform;
            var isFBXImported = IsFBXImported; // 4/20/2010
            if (!SetAdjustingBoneTransform(rotationAxis, rotationValue, ref _oldRotationValue, isFBXImported, out rotationTransform))
                return;

            if (isFBXImported)
            {
                // Now call 'SetAdjustingBoneTransform' with proper rotation Transform.
                SetAdjustingBoneTransform(this, boneName, itemInstanceKey, ref rotationTransform);
            }
            else
            {
                // Now call 'SetAdjustingBoneTransform' with proper rotation Transform.
                SetAdjustingBoneTransform(boneName, itemInstanceKey, ref rotationTransform);
            }
        }

        // 5/22/2009 - Applies a 'Rotation' bone adjustment Transform to a specific bone.
        /// <summary>
        /// Applies a 'Rotation' bone adjustment Transform to a specific <see cref="InstancedModel"/> bone.
        /// </summary>
        /// <param name="rotationAxis"><see cref="RotationAxis"/> Enum to use</param>
        /// <param name="rotationValue">Rotation value of some value 0 or greater.</param>
        /// <param name="oldRotationValue">Prior rotation value</param>
        /// <param name="isFBXImported">This <see cref="InstancedModel"/> loaded as FBX format?</param>
        /// <param name="rotationTransform">(OUT) New rotation transform</param>
        /// <remarks>If <paramref name="isFBXImported"/> is TRUE, then Y-Z channels are flipped.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="RotationAxis"/> given is invalid.</exception>
        /// <returns>True/False of result</returns>
        private static bool SetAdjustingBoneTransform(RotationAxis rotationAxis, float rotationValue, ref float oldRotationValue, 
                                                      bool isFBXImported, out Matrix rotationTransform)
        {
            rotationTransform = MatrixIdentity;

            // 8/12/2009 - Check if new value differs from prior; if not, return False.
            if (rotationValue.Equals(oldRotationValue))
                return false;

            oldRotationValue = rotationValue;

            // 1st check _model's import type from content pipeline.
            // If FBX format, then the rotation requests for Y/Z axises are still in 3DS Max format, which is Z is up!            
            var fbxImported = isFBXImported; // 4/20/2010
            if (fbxImported)
            {
                // create proper rotation Transform
                switch (rotationAxis)
                {
                    case RotationAxis.X:
                        Matrix.CreateRotationX(rotationValue, out rotationTransform);
                        break;
                    case RotationAxis.Y:
                        Matrix.CreateRotationZ(rotationValue, out rotationTransform); // Z is up
                        break;
                    case RotationAxis.Z:
                        Matrix.CreateRotationY(rotationValue, out rotationTransform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rotationAxis");
                }
            }
            else // This was imported with 'DirectX' Importer, and Y is Up.
            {
                // create proper rotation Transform
                switch (rotationAxis)
                {
                    case RotationAxis.X:
                        Matrix.CreateRotationX(rotationValue, out rotationTransform);
                        break;
                    case RotationAxis.Y:
                        Matrix.CreateRotationY(rotationValue, out rotationTransform); // Y is up
                        break;
                    case RotationAxis.Z:
                        Matrix.CreateRotationZ(rotationValue, out rotationTransform);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("rotationAxis");
                }
            }

            return true; // 8/23/2009
        }

       


        // 8/13/2009 - 
        ///<summary>
        /// Applies a bone adjustment transform to a specific bone.
        ///</summary>
        ///<param name="boneName">Bone name to affect</param>
        ///<param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        ///<param name="adjustingTransform"><see cref="Matrix"/> transform adjustment to apply</param>
        public void SetAdjustingBoneTransform(string boneName, int itemInstanceKey, ref Matrix adjustingTransform)
        {
            // 8/13/2009 - Call Static version
            SetAdjustingBoneTransform(this, boneName, itemInstanceKey, ref adjustingTransform);

            // 1/17/2011 - Save ref to current explosion boneName.
            _currentExplosionBoneName = boneName;

            // 7/24/2009 - Update the InstanceTransform for this instance.
            UpdateInstanceTransforms(itemInstanceKey);
        }

        // 8/13/2009: Optimized to be STATIC method.
        // 10/24/2008 - Applies a bone adjustment Transform to a specific bone.
        /// <summary>
        /// Applies a bone adjustment transform to a specific bone.
        /// </summary>
        /// <param name="instancedModel"><see cref="InstancedModel"/> instance to affect</param>
        /// <param name="boneName">Bone name to affect</param>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="adjustingTransform"><see cref="Matrix"/> transform adjustment to apply</param>
        private static void SetAdjustingBoneTransform(InstancedModel instancedModel, string boneName, int itemInstanceKey, ref Matrix adjustingTransform)
        {
            var bonesCollection = instancedModel._bonesCollection; // 1/28/2010; was '_model'.
            if (bonesCollection == null) return;

            // 11/5/2009 - Store param value into local copy, so changes won't affect original copy!
            var adjustingTransformCopy = adjustingTransform;

            // 6/2/2009
            // If FBX format, then the rotation requests for Y/Z axises are still in 3DS Max format, which is Z is up!
            if (instancedModel.IsFBXImported)
            {
                // Get translation vector3
                var tmpTranslation = adjustingTransformCopy.Translation;

                // store y value of translation
                var tmpY = tmpTranslation.Y;

                // flip z/y values in translation vector3, and * z by -1.
                tmpTranslation.Y = tmpTranslation.Z*-1;
                tmpTranslation.Z = tmpY;

                // Store Updated translation vector back into adjustingTransform.
                adjustingTransformCopy.Translation = tmpTranslation;
            }

            // 1st - Get BoneName Index
            InstancedModelBone modelBone;
            if (!bonesCollection.TryGetValue(boneName, out modelBone))
                return;

            // 8/13/2009 - Cache
            var bone = bonesCollection[boneName];
            var boneIndex = bone.Index;

            // Does Key exist?
            var adjustingBoneTransforms = ((IDictionary<int, Matrix[]>) instancedModel._adjustingBoneTransforms); // 6/2/2010
            if (adjustingBoneTransforms.ContainsKey(itemInstanceKey))
            {
                // 11/19/2008 - Optimize by removing Matrix Overload operations, which are slow on XBOX!                
                var tmpTransform = bone.Transform;
                Matrix.Multiply(ref adjustingTransformCopy, ref tmpTransform,
                                out adjustingBoneTransforms[itemInstanceKey][boneIndex]);
            }
            else
            {
                // Else, add new entry                
                Matrix[] bones;
                // Can't use Array.Resize here; each instance needs a new Instance array!
                CopyBoneTransformsTo(bonesCollection, out bones); // 1/28/2010; was '_model' / was 'CopyBoneTransformsTo'.

                // 11/19/2008 - Optimize by removing Matrix Overload operations, which are slow on XBOX!        
                //bones[boneIndex] = adjustingTransform * _model.Bones[boneName].Transform;
                var tmpTransform = bone.Transform;
                Matrix.Multiply(ref adjustingTransformCopy, ref tmpTransform, out bones[boneIndex]);

                adjustingBoneTransforms.Add(itemInstanceKey, bones);
            }

            // 2/11/2009 - Update Dictionary which tracks if adjusting entry was made.
            var adjustingBoneTransformsEntryMade = ((IDictionary<int, bool>) instancedModel._adjustingBoneTransformsEntryMade); // 6/2/2010
            if (adjustingBoneTransformsEntryMade.ContainsKey(itemInstanceKey))
            {
                adjustingBoneTransformsEntryMade[itemInstanceKey] = true;
            }
            else
            {
                adjustingBoneTransformsEntryMade.Add(itemInstanceKey, true);
            }
        }

        // 6/8/2009
        /// <summary>
        /// Resets the given adjusting <paramref name="boneName"/> to the original 
        /// tranform contain in the <see cref="InstancedModel"/> instance.
        /// </summary>
        /// <param name="boneName">Bone name to affect</param>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        public void ResetAdjustingBoneTransform(string boneName, int itemInstanceKey)
        {
            // 1/28/2010; was '_model'.
            var bonesCollection = _bonesCollection;
            if (bonesCollection == null) return;

            // 1st - Get BoneName Index
            int boneIndex;
            InstancedModelBone modelBone;
            if (bonesCollection.TryGetValue(boneName, out modelBone))
            {
                boneIndex = bonesCollection[boneName].Index;
            }
            else
                return;

            // 4/20/2010 - Cache
            var adjustingBoneTransforms = _adjustingBoneTransforms; 
            if (adjustingBoneTransforms == null) return;

            // Does Key exist?
            if (adjustingBoneTransforms.ContainsKey(itemInstanceKey))
            {
                // Apply original bone Transform.            
                adjustingBoneTransforms[itemInstanceKey][boneIndex] = bonesCollection[boneName].Transform;
            }
        }

        // 2/23/2009
        /// <summary>
        /// Removes an adjusting bone transform.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        public void RemoveAdjustingBoneTransform(int itemInstanceKey)
        {
            // 4/20/2010 - Cache
            var adjustingBoneTransforms = _adjustingBoneTransforms;
            if (adjustingBoneTransforms == null) return;

            // 4/20/2010 - Cache
            var adjustingBoneTransformsEntryMade = _adjustingBoneTransformsEntryMade;
            if (adjustingBoneTransformsEntryMade == null) return;

            // Does Key exist?
            if (adjustingBoneTransforms.ContainsKey(itemInstanceKey))
            {
                // remove entry from adjusting dictionary
                adjustingBoneTransforms.Remove(itemInstanceKey);

                // remove entry from _adjustingBoneTransformsEntryMade dictionary
                adjustingBoneTransformsEntryMade.Remove(itemInstanceKey);
            }

            // Recalc all transforms.
            UpdateInstanceTransforms(itemInstanceKey); // 7/21/2009: Updated to use 'ItemKey' version.          
        }

        // 10/24/2008; 2/17/2009: Updated to pass back 'InstancedItemTransform' class instance, rather than Matrix.
        /// <summary>
        /// Retrieves the absolute bone transform, via the <see cref="InstancedItemTransform"/> structure, which is combine 
        /// with the adjusting transform, of a given <see cref="InstancedModel"/> bone.
        /// </summary>
        /// <param name="boneName">Bone name to affect</param>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="instancedItemTransform">(OUT) <see cref="InstancedItemTransform"/> structure</param>
        public void GetModelPartCombineAbsoluteTransform(string boneName, int itemInstanceKey,
                                                         out InstancedItemTransform instancedItemTransform)
        {
            instancedItemTransform = new InstancedItemTransform();

            // 3/11/2009 - Make sure '_model' is not null.
            var bonesCollection = _bonesCollection; // 1/28/2010; was '_model'.
            if (bonesCollection == null) return;


            // 2/11/2009 - Updated to check if BoneName exist.
            int boneIndex;
            InstancedModelBone modelBone;
            if (bonesCollection.TryGetValue(boneName, out modelBone))
            {
                // 1st - Get BoneName Index
                boneIndex = bonesCollection[boneName].Index;
            }
            else
            {
                // Bone name does not exist, so just return.  
                return;
            }

            // 4/20/2010 - Cache
            var instancedItemTransformses = _absoluteBoneTransforms;
            if (instancedItemTransformses == null) return;

            // 2nd - Get Transform from Dictionary
            if (instancedItemTransformses.ContainsKey(itemInstanceKey))
            {
                instancedItemTransform = instancedItemTransformses[itemInstanceKey][boneIndex];
            }
        }

        // 12/8/2008; 12/19/2008 - Updated to return True/False for success.
        // 1/15/2009 - Make sure boneName exist; otherwise crash!
        /// <summary>
        /// Retrieves the original absolute <see cref="InstancedModelBone"/> transform of a given <see cref="InstancedModel"/>.
        /// </summary>
        /// <param name="boneName"><see cref="InstancedModelBone"/> name</param>
        /// <param name="transform">(OUT) <see cref="InstancedModelBone"/> transform</param>
        /// <returns>True/False of result</returns>
        public bool GetModelPartAbsoluteTransform(string boneName, out Matrix transform)
        {
            transform = MatrixIdentity;

            // Get Absolute Bone Transform
            var bonesCollection = _bonesCollection; // 1/28/2010; was '_model'.
            if (bonesCollection != null)
            {
                // 1/15/2009 - Make sure boneName exist; otherwise crash!
                InstancedModelBone outModelBone;
                if (bonesCollection.TryGetValue(boneName, out outModelBone))
                {
                    transform = outModelBone.Transform;
                    return true;
                }
                return false;
            }
            transform = MatrixIdentity;
            return false;
        }

        // 10/24/2008
        /// <summary>
        /// Copys the absolute <see cref="InstancedModelBone"/> transforms, and stores into
        /// <see cref="_absoluteBoneTransforms"/> array.
        /// </summary>
        /// <param name="instancedModel">this instance of <see cref="InstancedModel"/></param>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        internal static void CopyAbsoluteBoneTranformsTo(InstancedModel instancedModel, int itemInstanceKey)
        {
            // 1st - Make sure there is an entry in the Dictionary for the given Instance
            var modelBoneCollection = instancedModel._bonesCollection; // 8/13/2009
            if (!instancedModel._absoluteBoneTransforms.ContainsKey(itemInstanceKey))
            {
                // Get AbsoluteBone Transforms from original 'Model'.
                InstancedItemTransform[] itemTransforms;
                // Can't use Array.Resize here; each instance needs a new Instance array!
                CopyAbsoluteBoneTransformsTo(instancedModel._bonesCollection, out itemTransforms);

                // Store into Dictionary for use.
                instancedModel._absoluteBoneTransforms.Add(itemInstanceKey, itemTransforms);
            }

            // Is there an ajusting bone Transform to use?
            if (!instancedModel._adjustingBoneTransforms.ContainsKey(itemInstanceKey)) return;

            // 2/11/2009 - Check if the entry was just made; otherwise, we can just skip this step if not.
            if (instancedModel._adjustingBoneTransformsEntryMade.ContainsKey(itemInstanceKey))
            {
                if (instancedModel._adjustingBoneTransformsEntryMade[itemInstanceKey] == false)
                    return;
            }

            // 8/13/2009 - Cache
            var adjustingBoneTransform1 = instancedModel._adjustingBoneTransforms[itemInstanceKey];
            var instancedItemTransforms = instancedModel._absoluteBoneTransforms[itemInstanceKey]; // 6/2/2010 - Cache
            var length = adjustingBoneTransform1.Length;

            // Iterate through collection of bones
            for (var i = 0; i < length; i++)
            {
                // For Given, get Parent index
                var parent = modelBoneCollection[i].Parent; // 8/13/2009
                if (parent != null)
                {
                    var parentIndex = parent.Index;

                    // 6/8/2009
                    var adjustingBoneTransform = adjustingBoneTransform1[i];
                    var absoluteBoneTransform = instancedItemTransforms[parentIndex].AbsoluteTransform;

                    // 11/19/2008 - Optimize by removing Matrix Overload operations, which are slow on XBOX!
                    // Apply Absolute Transform and store                        
                    Matrix.Multiply(ref adjustingBoneTransform, ref absoluteBoneTransform,
                                    out instancedItemTransforms[i].AbsoluteTransform);
                }
                else
                    instancedItemTransforms[i].AbsoluteTransform = adjustingBoneTransform1[i];
            } // End Loop bones

            // 2/11/2009
            if (instancedModel._adjustingBoneTransformsEntryMade.ContainsKey(itemInstanceKey))
            {
                // Set to False.
                instancedModel._adjustingBoneTransformsEntryMade[itemInstanceKey] = false;
            }
        }

        // 6/1/2010: Optimized, by returning the collection via OUT, rather than giving a pre-made collection just to return it anyways!
        // 1/28/2010; 2/11/2010: NOTE: IMPORTANT!! Do NOT change the 'Matrix[]' param below, to be IList<Matrix>; otherwise XBOX crash will occur!!!
        /// <summary>
        /// Copies a transform of each bone in a model relative to all parent bones of the bone into a given array.
        /// </summary>
        /// <param name="instancedModelBoneCollection">Read only collection of <see cref="InstancedModelBone"/></param>
        /// <param name="destinationBoneTransforms">(OUT) The collection to receive bone transforms</param>
        private static void CopyAbsoluteBoneTransformsTo(ReadOnlyCollection<InstancedModelBone> instancedModelBoneCollection, out InstancedItemTransform[] destinationBoneTransforms)
        {
            // 6/1/2010 - Create destination collection
            destinationBoneTransforms = new InstancedItemTransform[instancedModelBoneCollection.Count];
            
            var count = instancedModelBoneCollection.Count;
            for (var i = 0; i < count; i++)
            {
                var bone = instancedModelBoneCollection[i];
                if (bone == null) continue; // 4/20/2010

                var boneTransform = bone.Transform; // 6/1/2010
                if (bone.Parent == null)
                {
                    destinationBoneTransforms[i].AbsoluteTransform = boneTransform;
                }
                else
                {
                    // 6/1/2010: Optimized by using the Matrix.Mult overload, which is faster on the XBOX!
                    var index = bone.Parent.Index;
                    //destinationBoneTransforms[i].AbsoluteTransform = bone.Transform * destinationBoneTransforms[index].AbsoluteTransform;
                    Matrix.Multiply(ref boneTransform, ref destinationBoneTransforms[index].AbsoluteTransform,
                                    out destinationBoneTransforms[i].AbsoluteTransform);
                }
            }
        }

        // 6/1/2010: Optimized, by returning the collection via OUT, rather than giving a pre-made collection just to return it anyways!
        // 1/28/2010; 2/11/2010: NOTE: IMPORTANT!! Do NOT change the 'Matrix[]' param below, to be IList<Matrix>; otherwise XBOX crash will occur!!!
        /// <summary>
        /// Copies each bone transform relative only to the parent bone of the model to a given array.
        /// </summary>
        /// <param name="instancedModelBoneCollection">Read only collection of <see cref="InstancedModelBone"/></param>
        /// <param name="destinationBoneTransforms">(OUT) The collection to receive bone transforms.</param>
        private static void CopyBoneTransformsTo(ReadOnlyCollection<InstancedModelBone> instancedModelBoneCollection, out Matrix[] destinationBoneTransforms)
        {
            // 6/1/2010 - Create destination collection
            var count = instancedModelBoneCollection.Count;
            destinationBoneTransforms = new Matrix[count];
            
            for (var i = 0; i < count; i++)
            {
                destinationBoneTransforms[i] = instancedModelBoneCollection[i].Transform;
            }
        }

        // 2/8/2010 - Note: Struct MUST have Properties, in order for the Binding to work on the combo boxes!
        /// <summary>
        /// Used with the <see cref="GetModelPartNamesWithIndexes"/> method, to hold a <see cref="InstancedModelPart"/>'s 
        /// name and indexKey.
        /// </summary>
        public struct ModelPartNames
        {
            ///<summary>
            /// Name of given <see cref="InstancedModelPart"/>.
            ///</summary>
            public string Name { get; set; }
            ///<summary>
            /// Index key for given <see cref="InstancedModelPart"/>.
            ///</summary>
            public int IndexKey { get; set; }
        }

        // 2/8/2010
        /// <summary>
        /// Iterates the 'Normal' <see cref="InstancedModelPart"/>, returning a list of <see cref="ModelPartNames"/> nodes,
        /// to be used in the 'PropertiesTools' Form; specifically the Materials tab.
        /// </summary>
        /// <param name="modelPartNames">(OUT) List array of <see cref="ModelPartNames"/> nodes</param>
        public void GetModelPartNamesWithIndexes(out List<ModelPartNames> modelPartNames)
        {
            // create new list
            modelPartNames = new List<ModelPartNames>(ModelPartsCount);

            // itereate the Normal modelParts to retrieve part names.
// ReSharper disable LoopCanBeConvertedToQuery
            foreach (var normalModelPart in IterateNormalModelParts())
// ReSharper restore LoopCanBeConvertedToQuery
            {
                // create new ModelPartsNames node
                var modelPartName = new ModelPartNames
                                        {
                                            Name = normalModelPart.ModelPartName.ToString(),
                                            IndexKey = normalModelPart.ModelPartIndexKey
                                        };

                // add to array
                modelPartNames.Add(modelPartName);
            }
        }

        // 2/8/2010
        /// <summary>
        /// Helper method, used to only iterate the 'Normal' <see cref="InstancedModelPart"/>,
        /// specifically to be used in a ForEach construct; for example, 
        /// 'ForEach(var part in IterateNormalModelParts()'.
        /// </summary>
        /// <remarks>
        /// ForEach construct creates garbage on the Heap; therefore, avoid use
        /// in critical game code!
        /// </remarks>
        /// <returns>An <see cref="IEnumerable{TValue}"/> of type <see cref="InstancedModelPart"/>.</returns>
        private IEnumerable<InstancedModelPart> IterateNormalModelParts()
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            
            IList<int> modelPartsKeys = ModelPartsKeys;
            var modelPartsKeysCount = ModelPartsCount;
            
            for (var i = 0; i < modelPartsKeysCount; i++)
            {
                var modelPartIndex = modelPartsKeys[i];
                var modelPart = instancedModelParts[modelPartIndex];
                if (modelPart == null) continue;

                yield return modelPart;
            }
        }

        // 2/3/2010
        /// <summary>
        /// Retrieves an <see cref="InstancedDataCommunication"/> node, using the given <paramref name="itemInstanceKey"/>, from
        /// internal <see cref="InstanceWorldTransforms"/> dictionary.  If one does not exist, an empty
        /// <see cref="InstancedDataCommunication"/> node will be created and added to the dictionary.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="instancedDataCommunication">(OUT) <see cref="instancedDataCommunication"/> structure</param>
        /// <returns>True/False of result</returns>
        public bool GetInstanceDataNode(int itemInstanceKey, out InstancedDataCommunication instancedDataCommunication)
        {
            // 4/20/2010 - Cache and check if null.
            var instanceWorldTransforms = InstanceWorldTransforms;
            if (instanceWorldTransforms == null)
            {
                instancedDataCommunication = default(InstancedDataCommunication);
                return false;
            }

            // Check Dictionary if already exist?
            if (instanceWorldTransforms.TryGetValue(itemInstanceKey, out instancedDataCommunication))
            {
                return true;
            }

            // no, so add new InstanceData node
            var node = new InstancedDataCommunication
                           {
                               InCameraView = true,
                               ItemInstanceKey = itemInstanceKey,
                               PlayerNumber = 0,
                               DrawWithExplodePieces = false,
                               ShowFlashWhite = false, // 10/12/2009
                           };

            instanceWorldTransforms.Add(itemInstanceKey, node);

            return false;
        }

        // 2/3/2010
        /// <summary>
        /// Updates an <see cref="InstancedDataCommunication"/> node, using the given <paramref name="itemInstanceKey"/>, to the
        /// internal <see cref="InstanceWorldTransforms"/> dictionary.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="instancedDataCommunication">Updated <see cref="instancedDataCommunication"/> node</param>
        /// <returns>True/False of result</returns>
        public bool UpdateInstanceDataNode(int itemInstanceKey, ref InstancedDataCommunication instancedDataCommunication)
        {
            // 4/20/2010 - Cache and check if null.
            var instanceWorldTransforms = InstanceWorldTransforms;
            if (instanceWorldTransforms == null)
            {
                instancedDataCommunication = default(InstancedDataCommunication);
                return false;
            }

            // Check Dictionary if already exist?
            if (instanceWorldTransforms.ContainsKey(itemInstanceKey))
            {
                // yes, so save new updated node.
                instanceWorldTransforms[itemInstanceKey] = instancedDataCommunication;
                return true;
            }

            return false;
        }

        // 2/3/2010
        /// <summary>
        /// Retrieves an <see cref="InstancedDataCommunication"/> node, using the given <paramref name="itemInstanceKey"/>, from
        /// internal <see cref="InstanceWorldTransforms"/> dictionary; then updates the node's choosen
        /// parameter, and saves the result back to the <see cref="InstanceWorldTransforms"/> dictionary.
        /// </summary>
        /// <remarks>
        /// If one does not exist, an empty <see cref="InstancedDataCommunication"/> node will be created and added to the dictionary.
        /// </remarks>
        /// <typeparam name="T">Some Valuetype to update; like bool or int.</typeparam>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="instanceDataParam"><see cref="InstanceDataParam"/> Enum to update</param>
        /// <param name="newValue">New value to enter</param>
        public void UpdateInstanceDataNodeSpecificParameter<T>(int itemInstanceKey, InstanceDataParam instanceDataParam, T newValue) where T : struct
        {
            // 1st - Get Node out of dictionary.
            InstancedDataCommunication instanceData;
            GetInstanceDataNode(itemInstanceKey, out instanceData);

            // 2nd - Update the requested internal var.
            switch (instanceDataParam)
            {
                case InstanceDataParam.IsPicked:
                    instanceData.IsPicked = Convert.ToBoolean(newValue);
                    break;
                case InstanceDataParam.InCameraView:
                    instanceData.InCameraView = Convert.ToBoolean(newValue);
                    break;
                case InstanceDataParam.ProceduralMaterialId:
                    instanceData.ProceduralMaterialId = Convert.ToInt32(newValue);
                    break;
                case InstanceDataParam.DrawWithExplodePieces:
                    instanceData.DrawWithExplodePieces = Convert.ToBoolean(newValue);
                    break;
                case InstanceDataParam.ModelPartIndexKey:
                    instanceData.ModelPartIndexKey = Convert.ToInt32(newValue);
                    break;
                case InstanceDataParam.ItemInstanceKey:
                    instanceData.ItemInstanceKey = Convert.ToInt32(newValue);
                    break;
                case InstanceDataParam.PlayerNumber:
                    instanceData.PlayerNumber = Convert.ToInt32(newValue);
                    break;
                case InstanceDataParam.IsSceneryItem:
                    instanceData.IsSceneryItem = Convert.ToBoolean(newValue);
                    break;
                case InstanceDataParam.ShowFlashWhite:
                    instanceData.ShowFlashWhite = Convert.ToBoolean(newValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("instanceDataParam");
            }

            // 3rd - Save the Node back into dictionary.
            UpdateInstanceDataNode(itemInstanceKey, ref instanceData);
            
        }

        // 7/21/2009
        /// <summary>
        /// Updates the <see cref="InstancedModelPart"/> static effect parameters <see cref="InstancedModelPart.ViewParam"/> 
        /// and InstancedModelPart.ProjectionParam with the current <see cref="Camera"/>'s 
        /// <see cref="Camera.View"/> and <see cref="Camera.Projection"/> matricies.
        /// </summary>
        internal static void SetCameraEffectParams()
        {
            // 4/20/2010 - Cache
            var cameraView = Camera.View;
            var projection = Camera.Projection;
            var viewParam = InstancedModelPart.ViewParam;
            var viewInverseParam = InstancedModelPart.ViewInverseParam;
            var projectionParam = InstancedModelPart.ProjectionParam;
            
            if (viewParam != null) 
                viewParam.SetValue(cameraView);

            // 1/21/2010
            if (viewInverseParam != null)
            {
                Matrix viewInv;
                var view = cameraView;
                Matrix.Invert(ref view, out viewInv);
                viewInverseParam.SetValue(viewInv);
            }
            
            if (projectionParam != null)
                projectionParam.SetValue(projection);
            
        }

        // 2/11/2009
        /// <summary>
        /// Returns a reference to the internal list for the SpawnBullet position matrix transforms.
        /// </summary>
        /// <param name="itemInstanceKey"><see cref="SceneItem"/> owner's InstanceKey</param>
        /// <param name="spawnBulletTransforms">(Out) Collection of <see cref="InstancedItemTransform"/> bullet references</param>
        internal void GetInstancedModelSpawnBulletTransforms(int itemInstanceKey,
                                                             List<InstancedItemTransform> spawnBulletTransforms)
        {
            if (spawnBulletTransforms == null)
                return;

            spawnBulletTransforms.Clear();

            {
                // Populate List with InstancedItemTransform instances
                InstancedItemTransform transform;

                // Get Combine-Absolute Transform for SpawnBullet-1
                GetModelPartCombineAbsoluteTransform("SpawnBullet", itemInstanceKey, out transform);
                spawnBulletTransforms.Add(transform);

                // Get Combine-Absolute Transform for SpawnBullet-2
                GetModelPartCombineAbsoluteTransform("SpawnBullet2", itemInstanceKey, out transform);
                spawnBulletTransforms.Add(transform);

                // Get Combine-Absolute Transform for SpawnBullet-3
                GetModelPartCombineAbsoluteTransform("SpawnBullet3", itemInstanceKey, out transform);
                spawnBulletTransforms.Add(transform);

                // Get Combine-Absolute Transform for SpawnBullet-2
                GetModelPartCombineAbsoluteTransform("SpawnBullet4", itemInstanceKey, out transform);
                spawnBulletTransforms.Add(transform);
            }
        }

        // 2/19/2009
        /// <summary>
        /// Returns if the current <see cref="InstancedModel"/> has Illuminations mapping.
        /// </summary>
        /// <remarks>This is achieved by ONLY checking the first <see cref="InstancedModelPart"/> position
        /// in the <see cref="_modelParts"/> collection.</remarks>
        /// <returns>True/False of result</returns>
        internal bool HasIlluminationsMapping()
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            // let's just return the Status from the 1st part, since it will be same for all!
            return instancedModelParts != null && instancedModelParts[0].UseIllumMap;
        }

        // 6/1/2010
        ///<summary>
        /// Before a new Render cycle, this MUST be called to force a switch on 
        /// the 'CurrentDoubleBuffer' to use.
        ///</summary>
        public static void AlternateDoubleBuffer()
        {
            // 6/1/2010 - Save current to prior.
            var currentUpdateBuffer = InstancedModelChangeRequests.CurrentUpdateBuffer;
            InstancedModelPart.PriorUpdateBuffer = currentUpdateBuffer;

            // 7/20/2009 - Update the CurrentDoubleBuffer #.
            InstancedModelChangeRequests.CurrentUpdateBuffer = (currentUpdateBuffer == 0) ? 1 : 0;
        }

#if !XBOX360

        // 6/22/2009
        /// <summary>
        /// Connects the PhysX 'CLOTH' to the given bone transform.
        /// </summary>
        /// <param name="boneName">Bone name to affect</param>
        internal void SetPhysXClothForBoneTransform(string boneName)
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            if (instancedModelParts == null) return;

            // Locate given boneName, and connect the PhysX 'CLOTH' to this bone Transform.
            var count = instancedModelParts.Count; // 8/20/2009
            for (var i = 0; i < count; i++)
            {
                // 4/20/2010 - Cache
                var instancedModelPart = instancedModelParts[i];
                if (instancedModelPart == null) continue;

                // 8/20/2009 - Cache
                var modelPart = instancedModelPart;

                // Found Bone, so connect 'CLOTH'.
                if (modelPart.ModelPartName.ToString() != boneName) continue;

                // Only Create ONCE!
                if (modelPart.UsePhsyXCloth)
                    continue;
                
                // 8/20/2009 - Updated to only set 'UsePhysXCloth', if successful creation.
                //modelPart.PhysXCloth = new PhysXCloth();
                //modelPart.UsePhsyXCloth = modelPart.PhysXCloth.CreateCloth(modelPart);

            } // End Loop
        }

        // 6/24/2009
        /// <summary>
        /// This method sets the PhysX component on the given model.
        /// </summary>
        /// <param name="boneName">Bone name to affect</param>
        /// <param name="modelPathName">Directory path name</param>
        internal void SetPhysXSoftBodyForBoneTransform(string boneName, string modelPathName)
        {
            // 4/20/2010 - Cache
            var instancedModelParts = _modelParts;
            if (instancedModelParts == null) return;

            // Iterate through all ModelParts and apply 'SoftBody'
            var count = instancedModelParts.Count; // 8/20/2009
            for (var i = 0; i < count; i++)
            {
                // 4/20/2010
                var instancedModelPart = instancedModelParts[i];
                if (instancedModelPart == null) continue;

                // 8/20/2009 - Cache
                var modelPart = instancedModelPart;

                // Found Bone, so connect 'CLOTH'.
                if (modelPart.ModelPartName.ToString() != boneName) continue;

                // Only Create ONCE!
                if (modelPart.UsePhysXSoftBody)
                    continue;
               
                //modelPart.PhysXSoftBody = new PhysXSoftBody();
                //modelPart.PhysXSoftBody.CreateRequestForSoftBody(modelPart, modelPathName);
                //modelPart.UsePhysXSoftBody = true;

            } // End Loop
        }

#endif

        #region Dispose

        // 1/8/2010
        /// <summary>
        /// Clears all internal arrays for level reloads.  Should only be called for
        /// items which the graphic content remains in memory; otherwise, Dispose should
        /// be called.
        /// </summary>
        public void ClearForLevelReloads()
        {
            // Iterate ModelParts, to clear internal arrays.
            var instancedModelParts = _modelParts; // 4/20/2010
            if (instancedModelParts != null)
            {
                var count = instancedModelParts.Count;
                for (var i = 0; i < count; i++)
                {
                    var instancedModelPart = instancedModelParts[i];
                    if (instancedModelPart == null) continue;

                    instancedModelPart.ClearInternalArrays();
                }
            }

            // Clear All Worlds Trans for this model
            if (InstanceWorldTransforms != null)
                InstanceWorldTransforms.Clear();
        }
        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            // dispose managed resources
            // Dispose of all ModelParts
            var instancedModelParts = _modelParts; // 4/20/2010
            if (instancedModelParts != null)
            {
                var count = instancedModelParts.Count;
                for (var i = 0; i < count; i++)
                {
                    // 4/20/2010
                    var instancedModelPart = instancedModelParts[i];
                    if (instancedModelPart == null) continue;

                    instancedModelPart.Dispose();
                    instancedModelParts[i] = null;
                }
                instancedModelParts.Clear();
            }

            // 1/6/2010 - Clear out ChangeRequests by calling Dispose.
            InstancedModelChangeRequestManager.Dispose();

            // 9/12/2008
            if (InstanceWorldTransforms != null)
                InstanceWorldTransforms.Clear();

            // 3/18/2011
            if (_instanceVertexBuffer != null)
                _instanceVertexBuffer.Dispose();

            _graphicsDevice = null;

        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

       
    }

// End InstanceModel class


}