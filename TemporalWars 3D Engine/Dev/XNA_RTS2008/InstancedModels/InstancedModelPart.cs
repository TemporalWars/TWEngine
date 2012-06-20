#region File Description
//-----------------------------------------------------------------------------
// InstancedModelPart.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PerfTimersComponent.Timers;
using PerfTimersComponent.Timers.Enums;
using TWEngine.GameCamera;
using TWEngine.Players;
using TWEngine.Shadows;
using TWEngine.Terrain;
using TWEngine.InstancedModels.Enums;
using TWEngine.InstancedModels.Structs;

namespace TWEngine.InstancedModels
{
    

    /// <summary>
    /// The <see cref="InstancedModelPart"/> class is just a collection of
    /// model parts, each one of which can have a different material. These model
    /// parts are responsible for all the heavy lifting of drawing themselves
    /// using the various different instancing techniques.
    /// </summary>
    internal sealed class InstancedModelPart : IDisposable
    {
        #region Constants
       
        // This must match the constant at the top of InstancedModel.fx!
        const int MaxShaderMatrices = 100;

        #endregion

        #region Fields

        // XNA 4.0 - Add parent ref.
        internal InstancedModel Parent;

        // XNA 4.0 Updates - Xna ModelMeshPart
        internal ModelMeshPart XnaModelMeshPart;

        // 3/19/2011 - Stores Misc Part data, like EffectParams, EffectTech, etc.
        internal InstancedModelPartExtra InstancedModelPartExtra;

        // 3/25/2011 - XNA 4.0 Updates
        private static BlendState _blendState = new BlendState()
                                                    {
                                                        AlphaBlendFunction = BlendFunction.Max,
                                                        ColorSourceBlend = Blend.SourceAlpha,
                                                        ColorDestinationBlend = Blend.InverseSourceAlpha,

                                                    };

        // 6/22/2009 - ClothMesh / SoftBodyMesh
        internal bool UsePhsyXCloth;
        internal bool UsePhysXSoftBody;

        // 8/14/2009 - CameraUpdated 
        bool _cameraUpdated;
       
#if !XBOX
        // 6/22/2009 - ClothMesh / SoftBodyMesh        
        //internal PhysX.PhysXCloth PhysXCloth; 
        //internal PhysX.PhysXSoftBody PhysXSoftBody;       
#endif

        // 8/28/2009 - Updated the Value type, in the Dictionaries below, to be the 'InstancedDataStruct', rather than Matrix.
        // 8/19/2009 - Updated to use the new 'SpeedCollection', rather than Dictionary!    
        // 4/20/2009 - Matrix items to draw; 7/21/2009 - Updated to Dictionary, where Key = InstancedItemKey.
        internal readonly Dictionary<int, InstancedDataForDraw> TransformsToDrawList = new Dictionary<int, InstancedDataForDraw>(MaxShaderMatrices); // Culled List
        internal readonly Dictionary<int, InstancedDataForDraw> TransformsToDrawExpList = new Dictionary<int, InstancedDataForDraw>(MaxShaderMatrices); // Culled Explosions List
        internal readonly Dictionary<int, InstancedDataForDraw> TransformsToDrawAllList = new Dictionary<int, InstancedDataForDraw>(MaxShaderMatrices);  // All List
       
        
        // 8/25/2009 - Updated to be an Array of Dictionaries, rather than a Dictionary with Dictionary.
        // 7/21/2009 - Change Buffers - Outer Dictionary will hold the reference to two buffers; 0 & 1.  While the inner
        //             Dictionary, will hold each change request, where the Key (Int) represents the index of the change
        //             into the List<> DrawLists.
        /// <summary>
        /// The <see cref="ChangeBuffers"/> collection is used for the double-buffering techinique.  
        /// Each buffer contains a <see cref="ChangeRequestItem"/> structure.
        /// </summary>
        public readonly Dictionary<int, ChangeRequestItem>[] ChangeBuffers = new Dictionary<int, ChangeRequestItem>[2];
        //public readonly Dictionary<int, Dictionary<int, ChangeRequestItem>> ChangeBuffers = new Dictionary<int, Dictionary<int, ChangeRequestItem>>(2);

        // 6/1/2010 - DoubleBuffer
        /// <summary>
        /// The prior Buffer value, for double buffering.
        /// </summary>
        internal static int PriorUpdateBuffer;

        // 6/4/2010 - Tracks when changes are made to any of the instances, per draw cycle.
        private bool _isDirty;
        // 6/14/2010 - Track when StaticShadow map is being drawn, and force update of current 'Transforms' to GPU.
        private bool _isStaticShadows;

        // 6/22/2009 - Change to DynamicVertexBuffer; for updating of PhysX cloth data.
        internal DynamicVertexBuffer DynamicVertexBuffer;
        internal DynamicIndexBuffer DynamicIndexBuffer;

        // ReSharper disable UnaccessedField.Local
        private bool _isStaticItem; // 7/22/2009
// ReSharper restore UnaccessedField.Local

        // 7/21/2008 - Ben: BoundingSphere
        public BoundingSphere BoundingSphere;

        // 1/20/2010 - 
        /// <summary>
        /// Procedural Material ID, used to know which material lighting type to use.
        /// </summary>
        private int _proceduralMaterialId = 2; // default to 'Blinn'.
        
        // 5/27/2009 
        /// <summary>
        /// Reference to the <see cref="InstancedModelAttsData"/> structure.
        /// </summary>
        internal InstancedModelAttsData AttsData;

        // 9/30/2008 - Ben: name
        internal StringBuilder ModelPartName = new StringBuilder(25);   
     
        // 10/3/2008 - Ben: Absolute BoneOffset Index into array.
        internal int BoneOffsetIndex;    

        // Track which graphics Device we are using.
        private static GraphicsDevice _graphicsDevice;

        #endregion

        // 2/2/2010
        #region Properties

        // 6/4/2010
        /// <summary>
        /// Sets if this <see cref="InstancedModelPart"/> is an explosion piece.
        /// </summary>
        public bool IsExplosionPiece { get; set; }

        /// <summary>
        /// The default <see cref="InstancedModelPart"/> <see cref="Effect"/> shader.
        /// </summary>
        internal Effect Effect
        {
            get
            {
                return (InstancedModelPartExtra != null) ? InstancedModelPartExtra.MpEffect : null;
            }
        }

        // 7/1/2009 - 
        /// <summary>
        /// Reference to shader's <see cref="EffectParameter"/> 'ViewParam'.
        /// </summary>
        public static EffectParameter ViewParam { get; internal set; }

        // 1/21/2010
        /// <summary>
        /// Reference to shader's <see cref="EffectParameter"/> 'ViewInverseParam'.
        /// </summary>
// ReSharper disable UnusedAutoPropertyAccessor.Local
        public static EffectParameter ViewInverseParam { get; private set; }
// ReSharper restore UnusedAutoPropertyAccessor.Local

        // 7/21/2009
        /// <summary>
        /// Reference to shader's <see cref="EffectParameter"/> 'ProjectionParam'.
        /// </summary>
        public static EffectParameter ProjectionParam { get; internal set; }

        /// <summary>
        /// Reference to this <see cref="InstancedModelPart"/> use of Illumination map.
        /// </summary>
        public bool UseIllumMap
        {
            get { return (InstancedModelPartExtra != null) ? InstancedModelPartExtra.UseIllumMap : false; }
           
        }

        /// <summary>
        /// <see cref="InstancedItem"/> classes 'RayIntersectsModel' method will check if the given
        /// <see cref="InstancedModelPart"/> is picked by user's mouse, and set result into this property.
        /// </summary>
        public bool IsMeshPicked { get; set; }

        // 2/3/2010
        /// <summary>
        /// Used to apply a specific Procedural MaterialId to the given model part.
        /// Reference the 'LightingShader.HLSL' file for specific material Ids.
        /// </summary>
        public int ProceduralMaterialId
        {
            get { return (InstancedModelPartExtra != null) ? InstancedModelPartExtra.ProceduralMaterialId : 2; }

            set 
            {
                if (InstancedModelPartExtra != null)
                    InstancedModelPartExtra.ProceduralMaterialId = value;
            }
        }

        // 2/8/2010
        /// <summary>
        /// This <see cref="InstancedModelPart"/>'s Array index location within the parent's <see cref="InstancedModelPart"/> collection.
        /// </summary>
        public int ModelPartIndexKey { get; set; }

        #endregion


        #region Initialization

        // 3/16/2011
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="graphicsDevice">Instance of <see cref="GraphicsDevice"/></param>
        /// <param name="parent">Parent instance of <see cref="InstancedModel"/></param>
        /// <param name="isStaticItem">Is this <see cref="InstancedModel"/> a static item.</param>
        /// <param name="indexKey">Index key reference into parent collection.</param>
        internal InstancedModelPart(GraphicsDevice graphicsDevice, InstancedModel parent, bool isStaticItem, int indexKey)
        {
            // XNA 4.0 - Store parent
            Parent = parent;

            _graphicsDevice = graphicsDevice;
            _isStaticItem = isStaticItem; // 7/22/2009
            ModelPartIndexKey = indexKey; // 2/8/2010

            // 8/13/2009 - Capture camera movement, to force an update for Shadow EffectParams.
            Camera.CameraUpdated += CameraUpdated;

            // 7/21/2009 - Init the ChangeBuffer dictionaries
            ChangeBuffers[0] = new Dictionary<int, ChangeRequestItem>(55);
            ChangeBuffers[1] = new Dictionary<int, ChangeRequestItem>(55);
        }

        #region oldConstructor

        /*// 2/8/2010: Updated to add new param 'indexKey', which is simply the index into the parent's ModelParts List for this modelpart.
        /// <summary>
        /// Constructor reads <see cref="InstancedModel"/> data from the custom XNB format.
        /// </summary>
        /// <param name="input"><see cref="ContentReader"/> instance used to load data</param>
        /// <param name="graphicsDevice"><see cref="GraphicsDevice"/> instance</param>
        /// <param name="parent">The instance parent of <see cref="InstancedModel"/>.</param>
        /// <param name="isStaticItem">static item?</param>
        /// <param name="indexKey">This <see cref="InstancedModelPart"/> index position in the collection kept by the parent <see cref="InstancedModel"/>.</param>
        internal InstancedModelPart(ContentReader input, GraphicsDevice graphicsDevice, InstancedModel parent, bool isStaticItem, int indexKey)
        {
            // XNA 4.0 - Store parent
            _parent = parent;

            // XNA 4.0 - Store Xna ModelMeshPart indexes
            var meshIndex = input.ReadInt32();
            var meshPartIndex = input.ReadInt32();
            XnaModelMeshPart = parent.XnaModel.Meshes[meshIndex].MeshParts[meshPartIndex];
            

            _graphicsDevice = graphicsDevice;           
            _isStaticItem = isStaticItem; // 7/22/2009
            ModelPartIndexKey = indexKey; // 2/8/2010

            // 8/13/2009 - Capture camera movement, to force an update for Shadow EffectParams.
            Camera.CameraUpdated += CameraUpdated;
        
            // 7/21/2009 - Init the ChangeBuffer dictionaries
            ChangeBuffers[0] = new Dictionary<int, ChangeRequestItem>(55);
            ChangeBuffers[1] = new Dictionary<int, ChangeRequestItem>(55);

            // Load the model data.
            //IndexCount = input.ReadInt32();
            //VertexCount = input.ReadInt32();

            // 3/14/2011 - XNA 4.0 Updates -
            //_vertexOffset = input.ReadInt32();
            //_startIndex = input.ReadInt32();
            //_primitiveCount = input.ReadInt32();
            
            // 7/21/2008: Ben: Add BoundingSphere
            BoundingSphere = input.ReadObject<BoundingSphere>();

            // 1/20/2010 - Read in the "ProceduralMaterial' ID, used for material lighting type.
            _proceduralMaterialId = input.ReadInt32();

            // 8/1/2009
            _useDynamicBuffers = input.ReadBoolean(); 

            // 9/30/2008 - Ben: Add name
            ModelPartName.Append(input.ReadString());

            // TODO: Debug test
            //if (ModelPartName.ToString() != parent.XnaModel.Meshes[meshIndex].Name)
              //  Debugger.Break();


            // 5/27/2009 - Read the BoneAnimationAtts
            {
                var boneRotates1 = input.ReadBoolean();
                if (boneRotates1) // BoneAtts-1
                {
                    var packedVector = input.ReadObject<PackedVector3>();
                    packedVector.UnPackVector3(out _boneRotationData);
                }

                var boneRotates2 = input.ReadBoolean();
                if (boneRotates2) // BoneAtts-2
                {
                    var packedVector = input.ReadObject<PackedVector3>();
                    packedVector.UnPackVector3(out _boneRotationData);
                }

                if (boneRotates1 || boneRotates2)
                    _boneRotates = true;
            }        

            // XNA 4.0 Updated - obsolete.
            //VertexDeclaration = input.ReadObject<VertexDeclaration>();           

            // XNA 4.0 - Read in VertexBuffer / IndexBuffer
            //_vertexBuffer = input.ReadObject<VertexBuffer>();
            //_indexBuffer = input.ReadObject<IndexBuffer>();

            // XNA 4.0 - Not required - Can not retrieve directly from VertexDeclaration
            //VertexStride = _vertexBuffer.VertexDeclaration.VertexStride; 

            // XNA 4.0 Updates - Set if ExplosionPiece
            _isExplosionPiece = input.ReadBoolean();

            // XNA 4.0 - Not required.
            /*_vertexData = new byte[input.ReadInt32()];
            var vertexDataLength = _vertexData.Length; // 4/20/2010
            for (var i = 0; i < vertexDataLength; i++)
            {
                _vertexData[i] = input.ReadByte(); 
            }*/

            // 7/18/2009 - ONLY set 'DynamicVB' for PhysX items!
            /*if (_useDynamicBuffers)
            {
                // XNA 4.0 Updates
                //DynamicVertexBuffer = new DynamicVertexBuffer(graphicsDevice, vertexDataLength, BufferUsage.None);
                //DynamicVertexBuffer = new DynamicVertexBuffer(graphicsDevice, VertexDeclaration, vertexDataLength, BufferUsage.None);
                
                //DynamicVertexBuffer.SetData(_vertexData);
            }
            else
            {
                // XNA 4.0 Updates
                //_vertexBuffer = new VertexBuffer(graphicsDevice, vertexDataLength, BufferUsage.None);
                _vertexBuffer = new VertexBuffer(graphicsDevice, VertexDeclaration, vertexDataLength, BufferUsage.None);
                
                _vertexBuffer.SetData(_vertexData);
            }  */          
            
            // IndexData
            /*IndexData = new ushort[input.ReadInt32()];
            var indexDataLength = IndexData.Length; // 4/20/2010
            for (var i = 0; i < indexDataLength; i++)
            {
                IndexData[i] = input.ReadUInt16();
            }*/

            // IF XBOX, then skip this section, since the XBOX call the ReplicateIndexBuffer method below.


            // 7/18/2009 - ONLY set 'DynamicIB' for PhysX items!
            /*if (_useDynamicBuffers)
            {
                // XNA 4.0 Updates
                // Create a new index buffer, and set the replicated data into it.
                //DynamicIndexBuffer = new DynamicIndexBuffer(graphicsDevice, sizeof(ushort) * indexDataLength,BufferUsage.None, IndexElementSize.SixteenBits);
                DynamicIndexBuffer = new DynamicIndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, sizeof(ushort) * indexDataLength, BufferUsage.None);

                DynamicIndexBuffer.SetData(IndexData);
            }
            else
            {
                // XNA 4.0 Updates
                //_indexBuffer = new IndexBuffer(graphicsDevice, sizeof(ushort) * indexDataLength,BufferUsage.None, IndexElementSize.SixteenBits);
                _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, sizeof(ushort) * indexDataLength, BufferUsage.None);

                _indexBuffer.SetData(IndexData);
            }

            // 6/14/2010 - Updated from Anoymous delegate to new Named delegate.
            input.ReadSharedResource(delegate(Effect value){}); 

            input.ReadSharedResource<Effect>(InitializeEffectAndEffectParams);
            
            // Work out how many shader instances we can fit into a single batch.
            //var indexOverflowLimit = ushort.MaxValue / VertexCount;

            //_maxInstances = Math.Min(indexOverflowLimit, MaxShaderMatrices);
            _maxInstances = 1000;

#if XBOX360
            // On Xbox, we must replicate several copies of our index buffer data for
            // the VFetch instancing technique. We could alternatively precompute this
            // in the content processor, but that would bloat the size of the XNB file.
            // It is more efficient to generate the repeated values at load Time.
            //
            // We also require replicated index data for the Windows ShaderInstancing
            // technique, but this is computed lazily on Windows, so as to avoid
            // bloating the index buffer if it turns out that we only ever use the
            // HardwareInstancingTechnique (which does not require any repeated data).

            
            ReplicateIndexData(IndexData); 
#else
            // XNA 4.0 Updates
            // On Windows, store a copy of the original vertex declaration.
            //_originalVertexDeclaration = VertexDeclaration.GetVertexElements();
            //_originalVertexDeclaration = _vertexBuffer.VertexDeclaration.GetVertexElements();
#endif
        }*/

        #endregion
        
        // 8/13/2009
        /// <summary>
        /// Captures the CameraUpdated event, and set the internal flag to <see cref="_cameraUpdated"/>.  Will
        /// be used to force the <see cref="ShadowMap"/> to update.
        /// </summary>
        private void CameraUpdated(object sender, EventArgs e)
        {
            _cameraUpdated = true;
        }
        // 5/14/2009
        ushort[] _oldIndices = new ushort[1];
        ushort[] _newIndices = new ushort[1]; // was uShort

        #endregion
        

       

        // 5/24/2010: Updated to be STATIC method.
        // 5/19/2009: Removed the params 'View', 'Projection', & 'LightPos' since these are avaible as STATIC variables!
        // 1/28/2009: Updated to include the 'drawAll' parameter.
        // 8/4/2008 - Ben: Added the LightPosition parameter, in order to calculate proper
        //                 direction of diffuse lighting.
        /// <summary>
        /// Helper function sets up the <see cref="GraphicsDevice"/> and
        /// effect ready for drawing instanced geometry.
        /// </summary>
        /// <param name="instancedModelPart">this instance of <see cref="InstancedModelPart"/></param>
        /// <param name="shadows">Drawing shadows for <see cref="ShadowMap"/></param>
        static void SetRenderStates(InstancedModelPart instancedModelPart, bool shadows)
        {
            try
            {
                
#if !XBOX
                // 6/22/2009 - PhysX Cloth - Update Buffers with PhysX Receive data
                /*if (instancedModelPart.UsePhsyXCloth)
                {
                    // 9/22/2010 - XNA 4.0 Updates - Removed use of all Physx Classes.
                    //if (!shadows) instancedModelPart.PhysXCloth.UpdateClothBuffers(instancedModelPart);

                    graphicsDevice.RenderState.CullMode = CullMode.None;
                }
                else*/

                // 9/23/2010 - XNA 4.0 Updates
                //graphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
               

                // Set renderstates for drawing 3D models.
                instancedModelPart.Effect.Parameters["xLightPos"].SetValue(TerrainShape.LightPosition); 

                //if (instancedModelPart.UsePhysXSoftBody)
                {
                    // 9/22/2010 - XNA 4.0 Updates - Removed use of all Physx Classes.
                    //if (!shadows) instancedModelPart.PhysXSoftBody.UpdateSoftBodyBuffers(instancedModelPart);
                }
#endif

                // XNA 4.0 updates
                // Set the graphics Device to use our vertex data.
                //graphicsDevice.VertexDeclaration = instancedModelPart.VertexDeclaration;

                // 7/18/2009
                //var vertexStreamCollection = graphicsDevice.Vertices; // 4/20/2010

                /*if (instancedModelPart.UsePhsyXCloth || instancedModelPart.UsePhysXSoftBody || instancedModelPart._useDynamicBuffers)
                {
                    // XNA 4.0 Updates
                    //vertexStreamCollection[0].SetSource(instancedModelPart.DynamicVertexBuffer, 0, instancedModelPart.VertexStride);
                    //graphicsDevice.SetVertexBuffer(instancedModelPart.DynamicVertexBuffer);

                    // 7/9/2009 - Set Stream#2 data for 'Tangent'/'Binormal' data.
                    //if (!shadows) vertexStreamCollection[1].SetSource(instancedModelPart.DynamicVertexBuffer, 0, instancedModelPart.VertexStride);

                    //graphicsDevice.Indices = instancedModelPart.DynamicIndexBuffer;
                }
                else
                {
                    // XNA 4.0 Updates
                    //vertexStreamCollection[0].SetSource(vertexBuffer, 0, instancedModelPart.VertexStride);

                    // 7/9/2009 - Set Stream#2 data for 'Tangent'/'Binormal' data.
                    //if (!shadows) vertexStreamCollection[1].SetSource(vertexBuffer, 0, instancedModelPart.VertexStride);

                    //graphicsDevice.Indices = instancedModelPart._indexBuffer;
                }*/
                

                // Make sure our effect is set to use the right technique.
                if (instancedModelPart.InstancedModelPartExtra.TechniqueChanged)
                {
                    // 2/11/2010: Since Technique doesn't change too often, going back to string names.
                    // 8/25/2009: Updated to use the Enum #, rather than string to eliminate the Boxing.
                    //instancedModelPart.Effect.CurrentTechnique = instancedModelPart.Effect.Techniques[instancingTechnique.ToString()]; //"HardwareInstancing"
                    instancedModelPart.Effect.CurrentTechnique = instancedModelPart.Effect.Techniques["HardwareInstancing"];
                    instancedModelPart.InstancedModelPartExtra.TechniqueChanged = false;
                }

              
                // 11/17/2008 - Set EffectParams only once.
                // 1/15/2009 - Updated to use VFetchShadowMapRender for XBOX.
                if (instancedModelPart.Effect != null && !instancedModelPart.InstancedModelPartExtra.EffectParamsSet)
                {
                    // 11/19/2008 - Set Team Colors
                    for (var i = 0; i < TemporalWars3DEngine._maxAllowablePlayers; i++)
                    {
                        Player player;
                        TemporalWars3DEngine.GetPlayer(i, out player);

                        if (player == null) continue;

                        var teamColor = player.PlayerColor.ToVector4();
                        instancedModelPart.Effect.Parameters[String.Format("xTeamColor{0}", i + 1)].SetValue(teamColor); // boxing.
                    }

                    instancedModelPart.InstancedModelPartExtra.EffectParamsSet = true;
                }

                // 8/6/2009 - If ShadowMap draw, then skip setting the rest of the items, since ONLY applies
                //            to textured items.
                if (shadows) return;

                // 8/14/2009 - Set ShadowMaps LightViewProjection Matrix
                if (instancedModelPart._cameraUpdated && instancedModelPart.Effect != null)
                {
                    instancedModelPart._cameraUpdated = false;

                    // 4/20/2010 - Cache values
                    var shadowMapLightView = ShadowMap.LightView;
                    var shadowMapLightProj = ShadowMap.LightProj;
                    var shadowMapLightViewStatic = ShadowMap.LightViewStatic;
                    var shadowMapLightProjStatic = ShadowMap.LightProjStatic;

                    var lightView = shadowMapLightView;
                    var lightProj = shadowMapLightProj;

                    Matrix lightViewProj;
                    Matrix.Multiply(ref lightView, ref lightProj, out lightViewProj);

                    // 3/24/2010
                    if (instancedModelPart.InstancedModelPartExtra.LightViewProjEp != null)
                        instancedModelPart.InstancedModelPartExtra.LightViewProjEp.SetValue(lightViewProj);

                    // 8/14/2009 - Set Static ShadowMap
                    {
                        lightView = shadowMapLightViewStatic;
                        lightProj = shadowMapLightProjStatic;

                        Matrix.Multiply(ref lightView, ref lightProj, out lightViewProj);

                        // 3/24/2010
                        if (instancedModelPart.InstancedModelPartExtra.LightViewProjStaticEp != null)
                            instancedModelPart.InstancedModelPartExtra.LightViewProjStaticEp.SetValue(lightViewProj);
                    }
                }
            } // 1/6/2010: Captures the ObjectDisposed Exception, which occurs peridically during level reloads.
            catch (ObjectDisposedException)
            {
                Debug.WriteLine("(SetRenderStates) threw the 'ObjectDisposedException' error.");
                //throw;
            }

        }


        // 6/9/2009; // 5/24/2010: Updated method to be STATIC.
        /// <summary>
        /// Helper method to update all static <see cref="EffectParameter"/>, which only needs to be 
        /// done once for the entire model draw call, and not for each <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <param name="instancedModelPart">this instance of <see cref="InstancedModelPart"/></param>
        /// <param name="lightView"><see cref="Matrix"/> light view</param>
        /// <param name="lightProj"><see cref="Matrix"/> light projection</param>
        internal static void SetStaticRenderStates(InstancedModelPart instancedModelPart, ref Matrix lightView, ref Matrix lightProj)
        {
            // Calc the Light/View projection
            Matrix lightViewProj;
            Matrix.Multiply(ref lightView, ref lightProj, out lightViewProj);

            // Set LightViewProj
            if (instancedModelPart.InstancedModelPartExtra.LightViewProj22Ep != null)
                instancedModelPart.InstancedModelPartExtra.LightViewProj22Ep.SetValue(lightViewProj);

            // 3/22/2011 - XNA 4.0 Updates - Not required during shadow call.
            // 3/23/2010
            //SetStaticRenderStates(instancedModelPart);
           
        }

        // 5/24/2010: Updated method to be STATIC.
        // 2/23/2010: Updated to check the 'EffectStaticParamsSet' boolean.
        /// <summary>
        /// Helper method to update all static <see cref="EffectParameter"/>, which only 
        /// needs to be done once for the entire model draw call, and  not for each <see cref="InstancedModelPart"/>.
        /// </summary>
        /// <param name="instancedModelPart">this instance of <see cref="InstancedModelPart"/></param>
        internal static void SetStaticRenderStates(InstancedModelPart instancedModelPart)
        {
            // 4/20/2010 - Cache
            var shadowMapTexture = ShadowMap.ShadowMapTexture;
            var shadowMapTerrainTexture = ShadowMap.ShadowMapTerrainTexture;

            // 6/13/2010 - Updated to use EffectParam
            if (shadowMapTexture != null && instancedModelPart.InstancedModelPartExtra.ShadowMapTextureEp != null)
                instancedModelPart.InstancedModelPartExtra.ShadowMapTextureEp.SetValue(shadowMapTexture); // 6/4/2009 - ShadowMap Dynamic  
            // 6/13/2010 - Updated to use EffectParam
            if (shadowMapTerrainTexture != null && instancedModelPart.InstancedModelPartExtra.TerrainShadowMapEp != null)
                instancedModelPart.InstancedModelPartExtra.TerrainShadowMapEp.SetValue(shadowMapTerrainTexture); // 7/14/2009 - ShadowMap STATIC
        }


        #region Draw      
 

        // 8/17/2009
        /// <summary>
        /// Processes the double buffers, by iterating through the 'Current' double buffer, and processing
        /// all 'ChangeRequests' given.
        /// </summary>
        /// <param name="instancedModelPart"><see cref="instancedModelPart"/> to process</param>
        internal static void ProcessDoubleBuffers(InstancedModelPart instancedModelPart)
        {
            var changeBuffers = instancedModelPart.ChangeBuffers;

            // 6/1/2010 - Updated to use the 'PriorUpdateBuffer', since old way was causing high CPI of 17.0 in V-Tune!
            //var currentUpdateBuffer = InstancedModel.CurrentUpdateBuffer; // 4/20/2010 - Cache
            //var useDoubleBuffer = (currentUpdateBuffer == 0) ? 1 : 0;
            var useDoubleBuffer = PriorUpdateBuffer;

            // Get Keys to Dictionary
            var changeRequestItems = changeBuffers[useDoubleBuffer]; // 6/1/2010 - Cache
            var changeBuffersCount = changeRequestItems.Keys.Count;

            // 6/4/2010 - Track 'Dirty' flag - to know when changes take place per draw cycle.
            instancedModelPart._isDirty = false; // reset

            // don't waste time processing anything if empty changeBuffer!
            if (changeBuffersCount <= 0) return;

            // 6/4/2010 - There are changes, so set to 'Dirty'.
            instancedModelPart._isDirty = true;

            if (_keys.Length < changeBuffersCount)
                Array.Resize(ref _keys, changeBuffersCount);
            changeRequestItems.Keys.CopyTo(_keys, 0);

            // 6/1/2010 - Cache
            var keys = _keys;

            // Iterate through changeBuffer & update drawList
            for (var i = 0; i < changeBuffersCount; i++)
            {
                // Cache key 
                var indexKey = keys[i];

                // 2/24/2011
                try
                {
                    // Retrieve ChangeRequestItem from internal Queue
                    var changeRequestItem = changeRequestItems[indexKey];

                    // Update TransformsLists (Culled/All).
                    InstancedModelChangeRequests.UpdateTransformsBasedOnChangeRequestItem(instancedModelPart, indexKey, ref changeRequestItem);

                }
                catch (KeyNotFoundException)
                {
                    // Skip
                }

            } // End For Loop

            // 7/21/2009 - Clear Current Buffers
            changeRequestItems.Clear();


        }

        // 7/10/2009; // 5/24/2010: Updated to be a STATIC method.
        /// <summary>
        /// Draws a batch of <see cref="InstancedModelPart"/> geometry, using the default <see cref="Effect"/> shader.
        /// </summary>
        /// <param name="instancedModelPart">this instance of <see cref="InstancedModelPart"/></param>
        /// <param name="instancingTechnique"><see cref="InstancingTechnique"/> to use</param>
        /// <param name="drawTransformsType"><see cref="DrawTransformsType"/> to draw</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public static void Draw(InstancedModelPart instancedModelPart, InstancingTechnique instancingTechnique, DrawTransformsType drawTransformsType, GameTime gameTime)
        {
#if DEBUG
            // 4/21/2010 - Debug Purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IMPSetRt);
#endif
            SetRenderStates(instancedModelPart, false);
            var effect = instancedModelPart.Effect; // 4/20/2010
#if DEBUG
            // 4/21/2010 - Debug Purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMDraw);
#endif


          


#if DEBUG
            // 4/21/2010 - Debug Purposes           
            StopWatchTimers.StartStopWatchInstance(StopWatchName.IMPDraw);
#endif

            // 6/11/2010 - Set Regular technique.
            switch (instancingTechnique)
            {
                case InstancingTechnique.HardwareInstancing:
                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    break;
                case InstancingTechnique.HardwareInstancingAlphaDraw:
                    effect.CurrentTechnique = effect.Techniques["HardwareInstancingAlphaDraw"];
                    //_graphicsDevice.BlendState = BlendState.AlphaBlend;
                    break;
                default:
                    effect.CurrentTechnique = effect.Techniques["HardwareInstancing"];
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    break;

            }

            // 2/15/2010 - Fixed: Brought back the check of the TransformsType.
            IDictionary<int, InstancedDataForDraw> toDrawList;
            switch (drawTransformsType)
            {
                case DrawTransformsType.NormalTransforms_All:
                    var transformsToDrawAllList = instancedModelPart.TransformsToDrawAllList; // 4/20/2010
                    toDrawList = transformsToDrawAllList;
                    break;
                case DrawTransformsType.NormalTransforms_Culled:
                    var transformsToDrawList = instancedModelPart.TransformsToDrawList; // 4/20/2010
                    toDrawList = transformsToDrawList;

                    break;
                case DrawTransformsType.ExplosionTransforms_Culled:
                    var transformsToDrawExpList = instancedModelPart.TransformsToDrawExpList; // 4/20/2010
                    toDrawList = transformsToDrawExpList;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("drawTransformsType");
            }

            // Now Draw, passing in regular 'Effect'.
            Draw(instancedModelPart,
                instancingTechnique,
                gameTime, effect, toDrawList);
#if DEBUG
            // 4/21/2010 - Debug Purposes
            StopWatchTimers.StopAndUpdateAverageMaxTimes(StopWatchName.IMPDraw);
#endif

        }

        // 2/11/2010; // 5/24/2010: Updated to be a STATIC method.
        /// <summary>
        ///  Draws a batch of <see cref="InstancedModelPart"/> geometry, using the shadowing 'Effect'.
        /// </summary>
        /// <param name="instancedModelPart"></param>
        /// <param name="instancingTechnique"><see cref="InstancingTechnique"/> to use</param>
        /// <param name="drawTransformsType"><see cref="DrawTransformsType"/> to draw</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="isStaticShadows">Is Draw call for Static shadow maps?</param>
        public static void DrawShadows(InstancedModelPart instancedModelPart, InstancingTechnique instancingTechnique, DrawTransformsType drawTransformsType, GameTime gameTime, bool isStaticShadows)
        {
            SetRenderStates(instancedModelPart, true);

            // 6/11/2010 - Set Shadow technique.
            switch (instancingTechnique)
            {
                case InstancingTechnique.HardwareInstancing:
                    instancedModelPart.Effect.CurrentTechnique = instancedModelPart.InstancedModelPartExtra.ShadowMapHwTechnique;
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    break;
                case InstancingTechnique.HardwareInstancingAlphaDraw:
                    instancedModelPart.Effect.CurrentTechnique = instancedModelPart.InstancedModelPartExtra.ShadowMapHwAlphaTechnique;
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    break;
                default:
                    instancedModelPart.Effect.CurrentTechnique = instancedModelPart.InstancedModelPartExtra.ShadowMapTechnique;
                    _graphicsDevice.BlendState = BlendState.Opaque;
                    break;

            }

            var shadowEffect = instancedModelPart.Effect; // 4/20/2010

            // 2/15/2010 - Fixed: Brought back the check of the TransformsType.
            IDictionary<int, InstancedDataForDraw> toDrawList;
            switch (drawTransformsType)
            {
                case DrawTransformsType.NormalTransforms_All:
                    var transformsToDrawAllList = instancedModelPart.TransformsToDrawAllList; // 4/20/2010
                    toDrawList = transformsToDrawAllList;
                    break;
                case DrawTransformsType.NormalTransforms_Culled:
                    var transformsToDrawList = instancedModelPart.TransformsToDrawList; // 4/20/2010
                    toDrawList = transformsToDrawList;
                    break;
                case DrawTransformsType.ExplosionTransforms_Culled:
                    var transformsToDrawExpList = instancedModelPart.TransformsToDrawExpList; // 4/20/2010
                    toDrawList = transformsToDrawExpList;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("drawTransformsType");
            }

            // 6/14/2010 - To avoid Shadow anomalies, which occur if current 'Transforms' are not updated to GPU, the
            //             'isStaticShadows' flag is set.
            instancedModelPart._isStaticShadows = isStaticShadows;


            // Now Draw, passing in shadow 'Effect'.
            Draw(instancedModelPart, instancingTechnique, gameTime, shadowEffect, toDrawList);
        }

        // 7/21/2009;  Updated to 50 intial array, to avoid calling Array.Resize as much!
        private static int[] _keys = new int[50];

        // 3/27/2011 - XNA 4.0 Updates
        private static VertexBufferBinding _vertexBufferBindingA;
        private static VertexBufferBinding _vertexBufferBindingB;

        // 5/19/2009: Removed the params 'View', 'Projection', & 'LightPos' since these are available as STATIC variables!       
        /// <summary>
        /// Draws a batch of <see cref="InstancedModelPart"/> geometry, using the collection of <see cref="InstancedDataForDraw"/>.
        /// </summary>
        /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance to draw batch for</param>
        /// <param name="instancingTechnique"><see cref="InstancingTechnique"/> to use</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance for animations</param>
        /// <param name="effect"><see cref="Effect"/> shader file to use</param>
        /// <param name="transformsToDrawList">Collection of <see cref="InstancedDataForDraw"/> to draw</param>
        private static void Draw(InstancedModelPart instancedModelPart, InstancingTechnique instancingTechnique, GameTime gameTime, 
                                Effect effect, IDictionary<int, InstancedDataForDraw> transformsToDrawList)
        {

            try // 7/6/2010
            {
                // 2/11/2010 - Skip if Null
                if (transformsToDrawList == null)
                    return;
                // 2/11/2010 - Skip if Count is zero.
                if (transformsToDrawList.Count == 0)
                    return;

                // cache
                var instancedModelPartExtra = instancedModelPart.InstancedModelPartExtra;

                // 4/20/2010 - Cache
                if (instancedModelPartExtra.TimeEp != null)
                    instancedModelPartExtra.TimeEp.SetValue((gameTime == null) ? 0 : (float)gameTime.TotalGameTime.TotalSeconds);

                // 6/5/2010 - Update accumulative ElapsedTime.
                if (instancedModelPartExtra.AccumElapsedTimeEp != null)
                    instancedModelPartExtra.AccumElapsedTimeEp.SetValue(InstancedModel.AccumElapsedTime);

                // 6/4/2010 - Update ExplosionPiece param.
                if (instancedModelPartExtra.IsExplosionPieceEp != null)
                    instancedModelPartExtra.IsExplosionPieceEp.SetValue(instancedModelPart.IsExplosionPiece); //

                // Note: Debug
                effect.Parameters["oUseWind"].SetValue(instancedModelPartExtra.UseWind);
                

                // XNA 4.0 Updates - Obsolete CommiteChanges()
                // 6/14/2010 - Commit changes.
                //effect.CommitChanges();

                // XNA 4.0 updates - Begin() and End() obsolete.
                // Begin the effect, then loop over all the effect passes.
                //effect.Begin();

                // XNA 4.0 Updates
                UpdateTransformsStream(instancedModelPart, instancedModelPart.TransformsToDrawList);

                // 3/27/2011 - Allocate VertexBufferBinding to reduce GC
                _vertexBufferBindingA = new VertexBufferBinding(instancedModelPart.XnaModelMeshPart.VertexBuffer,
                                                                instancedModelPart.XnaModelMeshPart.VertexOffset, 0);
                _vertexBufferBindingB = new VertexBufferBinding(instancedModelPart.Parent._instanceVertexBuffer, 0, 1);

                // XNA 4.0 Updates
                _graphicsDevice.SetVertexBuffers(_vertexBufferBindingA, _vertexBufferBindingB);
                _graphicsDevice.Indices = instancedModelPart.XnaModelMeshPart.IndexBuffer;

                // TODO: TEST
                //effect.Parameters["World"].SetValue(instancedModelPart._parent._bonesCollection[instancedModelPart.BoneOffsetIndex].Transform);
                effect.Parameters["View"].SetValue(Camera.View);
                effect.Parameters["Projection"].SetValue(Camera.Projection);

                var effectPassCollection = effect.CurrentTechnique.Passes; // 8/13/2009
                var count = effectPassCollection.Count; // 8/13/2009

                for (var i = 0; i < count; i++)
                {
                    // 4/20/2010 - Cache
                    var effectPass = effectPassCollection[i];
                    if (effectPass == null) continue;

                    // XNA 4.0 updates - Begin() and End() obsolete.
                    effectPass.Apply();

                    // Draw instanced geometry using the specified technique.
                    switch (instancingTechnique)
                    {
                        case InstancingTechnique.HardwareInstancing:
                        case InstancingTechnique.HardwareInstancingAlphaDraw:
                            DrawHardwareInstancing(instancedModelPart, transformsToDrawList.Count);
                            break;
                        default:
                            break;

                    }

                    // XNA 4.0 updates - Begin() and End() obsolete.
                    //effectPass.End();

                } // End ForLoop - Passes

                // XNA 4.0 updates - Begin() and End() obsolete.
                //effect.End();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Draw method in 'InstancedModelPart' threw the exception; " +ex.Message ?? "No Message");
            }

        }

        /// <summary>
        /// Draws geometry using the HardwareInstancing technique.
        /// </summary>
        /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance to draw batch for</param>
        /// <param name="instanceCount"></param>
        private static void DrawHardwareInstancing(InstancedModelPart instancedModelPart, int instanceCount)
        {
            try
            {
                //var vertexCount = instancedModelPart.VertexCount;
                //var indexCount = instancedModelPart.IndexCount;
                //var vertexOffset = instancedModelPart.VertexCount / instancedModelPart.VertexStride; // XNA 4.0
                //var startIndex = instancedModelPart._startIndex; // XNA 4.0
                //var primitiveCount = instancedModelPart._primitiveCount; // XNA 4.0

                // Set up two vertex streams for instanced rendering.
                // The first stream provides the actual vertex data, while
                // the second provides per-instance Transform matrices.
                //var vertices = graphicsDevice.Vertices;

                // XNA 4.0 Updates
                //vertices[0].SetFrequencyOfIndexData(transformsCount);
                //vertices[1].SetFrequencyOfIndexData(transformsCount); // 7/9/2009 - Stream#2

                // 7/9/2009 - Updated the InstancedChannel to be stream#3, since stream#2 is now used for Tangent data.
                //var instanceDataStream = instancedModelPart._instanceVertexBuffer; // 4/20/2010


                // XMA 4.0 Updates - Set 'InstanceVertexBuffer' to 2nd stream.
                //vertices[2].SetSource(instanceDataStream, 0, InstancedDataForDraw.SizeInBytes); // 8/28/2009 - was 2nd param = SizeOfMatrix
                //vertices[2].SetFrequencyOfInstanceData(1);

                // XNA 4.0 Update - Replaced with new 'DrawInstancedPrimitives' method call.
                // Draw all the instances in a single batch.
                //graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, indexCount / 3);
                _graphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                        instancedModelPart.XnaModelMeshPart.NumVertices,
                                                        instancedModelPart.XnaModelMeshPart.StartIndex,
                                                        instancedModelPart.XnaModelMeshPart.PrimitiveCount,
                                                       instanceCount);
                
                // XNA 4.0 Update - 
                // NOTE: XBOX MUST have the VB cleared each call; otherwise, SetData exception thrown.
                // Reset the instancing streams.
                /*vertices[0].SetSource(null, 0, 0);
                vertices[1].SetSource(null, 0, 0);
                vertices[2].SetSource(null, 0, 0); // 7/9/2009*/
                _graphicsDevice.SetVertexBuffer(null);

            }
            catch (Exception ex)
            {
                //Debugger.Break();
               
            }
            
            
        }

        // XNA 4.0 Updates.
        InstancedDataForDraw[] _instanceTransformsForDraw = new InstancedDataForDraw[1];

        // 3/18/2011
        /// <summary>
        ///  Updates the vertices stream with the new 'Transforms' to draw at.
        /// </summary>
        /// <param name="instancedModelPart"><see cref="InstancedModelPart"/> instance to draw batch for</param> 
        /// <param name="transformsToDrawList">Collection of <see cref="InstancedDataForDraw"/> to draw</param>  
        internal static void UpdateTransformsStream(InstancedModelPart instancedModelPart, IDictionary<int, InstancedDataForDraw> transformsToDrawList)
        {
            // cache.
            var transformsCount = transformsToDrawList.Count;

            // 7/20/2009 - Saftey Check; if Count zero, then return.
            if (transformsCount == 0) return;

            // 3/25/2011 - XNA 4.0 Updates - Removed check of 'IsDirty'; otherwise trees do not resolve the absolute transforms correctly.
            // 6/14/2010 - Updates also if this is a 'StaticShadow' draw call.
            // 6/4/2010 - Update ONLY if changes made since last draw cycle!
            // 7/22/2009 - Updates the 'Transforms' data.
            //if (!instancedModelPart._isDirty && !instancedModelPart._isStaticShadows) return;

            // NOTE: Do NOT try to cache the 'instancedModelPart._instanceTransforms', because will actually slow down the Pc, since causes HEAP garbage!
            // 4/21/2009; 5/14/2009: Updated to only Grow array, and not shrink.
            // Resize array, if necessary
            if (instancedModelPart._instanceTransformsForDraw.Length < transformsCount)
                Array.Resize(ref instancedModelPart._instanceTransformsForDraw, transformsCount);

            //inTransformsToDrawList.CopyTo(_instanceTransforms);
            transformsToDrawList.Values.CopyTo(instancedModelPart._instanceTransformsForDraw, 0);

            // XNA 4.0 Updates - Not required anymore.
            // Make sure our instance data vertex buffer is big enough.                
            //var instanceDataSize = InstancedDataForDraw.SizeInBytes * transformsCount; // SizeOfMatrix * transformsCount

            // 4/20/2010 - Cache
            var instancedModel = instancedModelPart.Parent;

            var instanceVertexBuffer = instancedModel._instanceVertexBuffer;
            if ((instanceVertexBuffer == null) ||
                (transformsCount > instanceVertexBuffer.VertexCount)) // transformsCount
            {
                if (instanceVertexBuffer != null)
                    instanceVertexBuffer.Dispose();

                // XNA 4.0 Updates
                //instancedModelPart._instanceDataStream = new DynamicVertexBuffer(graphicsDevice, instanceDataSize, BufferUsage.WriteOnly);
                instancedModel._instanceVertexBuffer = new DynamicVertexBuffer(_graphicsDevice, instancedModel.InstanceVertexDeclaration,
                                                                               transformsCount, BufferUsage.WriteOnly); // transformsCount

                instanceVertexBuffer = instancedModel._instanceVertexBuffer; // 4/20/2010
            }

            // Upload Transform matrices to the instance data vertex buffer.
            instanceVertexBuffer.SetData(instancedModelPart._instanceTransformsForDraw, 0, transformsCount, SetDataOptions.Discard);

            instancedModelPart._isStaticShadows = false;
        }


        #endregion

        // 1/8/2010
        /// <summary>
        /// Clear all internal arrays; called from Dispose and Level reloads.
        /// </summary>
        internal void ClearInternalArrays()
        {
            if (TransformsToDrawList != null)
                TransformsToDrawList.Clear();
            if (TransformsToDrawAllList != null)
                TransformsToDrawAllList.Clear();
            if (TransformsToDrawExpList != null)
                TransformsToDrawExpList.Clear();
           
            if (_keys != null) Array.Clear(_keys, 0, _keys.Length);
            //if (_instanceTransforms != null) Array.Clear(_instanceTransforms, 0, _instanceTransforms.Length);
            // 1/8/2010 - Clear ChangeBuffers
            foreach (var dictionary in ChangeBuffers)
            {
                // Clear each Dictionary
                if (dictionary != null)
                    dictionary.Clear();
            }

        }

        #region Dispose

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            
            // dispose managed resources
            // Dispose of Resources
            if (DynamicVertexBuffer != null)
                DynamicVertexBuffer.Dispose();
            if (DynamicIndexBuffer != null)
                DynamicIndexBuffer.Dispose();
            if (Effect != null)
                Effect.Dispose();
            
#if !XBOX360
           

            // PhysX - Dispose of instances   
            /*if (PhysXCloth != null)
            {
                PhysXCloth.Dispose(true);
                PhysXCloth = null;
            }
                
            if (PhysXSoftBody != null)
            {
                PhysXSoftBody.Dispose(true);
                PhysXSoftBody = null;
            }*/
                
#endif
            // 1/8/2010 - Clear Arrays
            ClearInternalArrays();


            // Null Refs
            DynamicVertexBuffer = null;
            DynamicIndexBuffer = null;
            _graphicsDevice = null;
            ViewParam = null; // 1/6/2010
            ProjectionParam = null; // 1/6/2010
           
            
#if !XBOX360

#endif
            // free native resources
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
}
