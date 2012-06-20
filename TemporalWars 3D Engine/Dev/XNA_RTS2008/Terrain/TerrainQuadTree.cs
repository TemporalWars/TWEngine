#region File Description
//-----------------------------------------------------------------------------
// TerrainQuadTree.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TWEngine.GameCamera;
using TWEngine.InstancedModels;
using TWEngine.InstancedModels.Structs;
using TWEngine.Terrain.Enums;
using TWEngine.Terrain.Structs;
using TWEngine.TerrainTools;

namespace TWEngine.Terrain
{
    /// <summary>
    /// The <see cref="TerrainQuadTree"/> class makes up the entire structure of the <see cref="Terrain"/>, where each
    /// instance of the <see cref="TerrainQuadTree"/> class contains a collection of four additional <see cref="TerrainQuadTree"/> instances.
    /// At the base of the tree, which are the leafs, are references to a <see cref="TerrainQuadPatch"/>.
    /// Due to the structure of the <see cref="TerrainQuadTree"/>, only <see cref="TerrainQuadPatch"/> which are in the camera view or frustum, will
    /// be drawn.  Furthermore, the layout of the tree structure allows for use of recursion, which is used heavily for the draw and locate methods.
    /// </summary>
    public class TerrainQuadTree
    {
        // 3/28/2008: Ben - When each Quad is created, the QuadKey is increased by 1.
        //            Serves as the Key for the TerrainIndexBuffer Dictionary, which is used
        //            to determine the Quads in the Camera Frustum for picking.
        private static BasicEffect _lineEffect;
        private static VertexDeclaration _lineVertexDeclaration;

        internal static ITerrainShape TerrainShapeInterface;
        internal static int TreeLeafCount;
        internal static List<TerrainQuadTree> TreeLeafList;
        internal readonly int Height;
        internal readonly int Width;
        internal readonly int OffsetX;
        internal readonly int OffsetY;
        internal readonly int? ParentQuadKeyInstance; // 4/14/2008 - Store Parent QuadKey from LOD Level 1.
        internal readonly int QuadKey;
        private static int _quadKeyCounter;

        internal readonly QuadSection QuadSection; // 4/14/2008 - Store QuadSection        
        internal readonly int RootWidth;
        private readonly Dictionary<int, Dictionary<int, int>> _scenaryItemTypes;  // 6/7/2012 - Was = Dictionary<int, List<int>>.
        private readonly int _vertexBufferOffset;
        

        // This holds the references to the 4 child nodes.
        // These remain null if this quadtree is a _leaf node.
        internal int BottomLeftIndex;
        internal int BottomRightIndex;

#if DEBUG && !XBOX360
        private VertexPositionColor[] _boundingBoxMesh;
#endif

        private BoundingBox _treeBoundingBox; // Holds bounding box used for culling

        internal LOD Detail; // 4/8/2008 - Should Track LOD at Quad Level, since each Quad can be different!

        private Vector2 _firstCorner;
        private Vector2 _lastCorner;

        internal TerrainQuadPatch LeafPatch;

        // 4/3/2008
        private float _maxHeight;
        private float _minHeight;
        internal int[] TmpIndexBufferData;
        internal int TopLeftIndex;
        internal int TopRightIndex;

        internal List<TerrainQuadTree> TreeList;
        internal int WidthXHeight; // was named quadSize.     

        #region Properties

        ///<summary>
        /// Returns a collection of <see cref="int"/>, which represents the <see cref="IndexBuffer"/> data.
        ///</summary>
        public List<int> IndexBufferData { get; private set; }

        /// <summary>
        /// Track Depth of Tessellation.
        /// </summary>
        public TessellateLevel LODLevel { get; internal set; }

        ///<summary>
        /// Checks if this instance of the <see cref="TerrainQuadTree"/> is at the 'Leaf' level of the tree structure.
        ///</summary>
        public bool Leaf { get; internal set; }

        ///<summary>
        /// Unique key given to each <see cref="TerrainQuadTree"/> instance.
        ///</summary>
        public int QuadKeyInstance
        {
            get { return QuadKey; }
        }

        ///<summary>
        /// Get or set if there is a bounding box hit.
        ///</summary>
        public static bool BoundingBoxHit { get; set; }

        #endregion

        #region Initialization

        // 7/10/2009
        private static readonly List<Vector3> BoxList = new List<Vector3>(36);
        private static readonly Vector3[] ThisVectors = new Vector3[8];
        public static bool UpdateSceneryCulledList;
        private static GraphicsDevice _graphicsDevice; // 4/21/2010

        /// <summary>
        /// Use this constructor only when creating the root node for the entire <see cref="TerrainQuadTree"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        /// <param name="verticesLength">Full list of vertices for the <see cref="Terrain"/></param>
        public TerrainQuadTree(Game game, int verticesLength)
        {
            // Set Default Values for Struct
            {
                LeafPatch = new TerrainQuadPatch();
                Leaf = false;
                IndexBufferData = new List<int>();
                WidthXHeight = 0;
                _vertexBufferOffset = 0;
                BoundingBoxHit = false;
                _minHeight = 1000000;
                _maxHeight = 0;
                OffsetX = 0;
                OffsetY = 0;
                TmpIndexBufferData = new int[1];
#if DEBUG && !XBOX360
                _boundingBoxMesh = new VertexPositionColor[1];
#endif
                _lastCorner = Vector2.Zero;
                _firstCorner = Vector2.Zero;
                Detail = TerrainData.DetailDefaultLevel1;
                LODLevel = TessellateLevel.Level1;
                TreeList = null;
                TopLeftIndex = 0;
                TopRightIndex = 0;
                BottomLeftIndex = 0;
                BottomRightIndex = 0;
                QuadSection = QuadSection.TopLeft;
                ParentQuadKeyInstance = null;
                _treeBoundingBox = new BoundingBox();
                _scenaryItemTypes = new Dictionary<int, Dictionary<int, int>>(); // 7/9/2009; 6/7/2012
            }

            // 4/21/2010 - Save GraphicsDevice instance.
            _graphicsDevice = game.GraphicsDevice;
           
            // This truncation requires all heightmap images to be
            // a power of two in _height and _width
            Width = (int) Math.Sqrt(verticesLength);
            Height = Width;
            RootWidth = Width;

            WidthXHeight = Width*Height;

            // 8/25/2008 - Instantiate the Static Quad Tree Leaf List
            TreeLeafCount = 0;

            // 6/30/2009
            switch (Width)
            {
                case 512:
                    TreeLeafList = new List<TerrainQuadTree>(256);
                    break;
                case 1024:
                    TreeLeafList = new List<TerrainQuadTree>(1024);
                    break;
            }

            // 7/31/2008 - Get ITerrainShape Interface from Services
            TerrainShapeInterface = (ITerrainShape) game.Services.GetService(typeof (ITerrainShape));

            // Ben - Add 1 to QuadKey & Store into QuadKeyInstance
            _quadKeyCounter = 0; // 11/17/09 - Start at zero.
            _quadKeyCounter++;
            QuadKey = _quadKeyCounter;

            // XNA 4.0 Updates - Remove 2nd param.
            // Line effect is used for rendering debug bounding boxes
            //_lineEffect = new BasicEffect(TerrainShapeInterface.Device, null) {VertexColorEnabled = true};
            _lineEffect = new BasicEffect(TerrainShapeInterface.Device) { VertexColorEnabled = true };


            // XNA 4.0 Updates - VertexDeclaration created with VertexBuffers now.
            //_lineVertexDeclaration = new VertexDeclaration(TerrainShapeInterface.Device, VertexPositionColor.VertexElements);

            // Vertices are only used for setting up the dimensions of
            // the bounding box. The vertices used in rendering are
            // located in the terrain class.
            SetUpBoundingBoxes(this);

            // If this tree is the smallest allowable size, set it as a _leaf
            // so that it will not continue branching smaller.
            if (verticesLength <= TerrainData.MinimumLeafSize)
            {
                Leaf = true;

                // 4/8/2008 - Set Tessellation Level to 1
                LODLevel = TessellateLevel.Level1;
                Detail = TerrainData.DetailDefaultLevel1;

#if DEBUG && !XBOX360
                CreateBoundingBoxMesh(ref _treeBoundingBox, out _boundingBoxMesh);
#else
                CreateBoundingBoxMesh(ref _treeBoundingBox);
#endif

                // 3/3/2009: Updated to get the indices array via the TerrainQuadPatch 'Out' parameter.
                LeafPatch = new TerrainQuadPatch(game.GraphicsDevice, Width,
                                                  TerrainData.DetailDefaultLevel1, OffsetX, OffsetY,
                                                  out TmpIndexBufferData);
                // 4/21/2009
                IndexBufferData.Clear();
                IndexBufferData.InsertRange(0, TmpIndexBufferData);

                // 8/25/2008 - Add Leaf to Static List
                TreeLeafList.Add(this);
                //TreeLeafList[TreeLeafCount] = this;
                TreeLeafCount++;
            }
            else
                BranchOffRoot();

            // 10/31/2008 - Attach EventHandler to TerrainShape
            TerrainShape.TerrainShapeCreated += TerrainShapeTerrainShapeCreated;

            // 7/10/2009
            Camera.CameraUpdated += CameraUpdated;
        }

        /// <summary>
        /// Internal constructor, used to create additional <see cref="TerrainQuadTree"/> instances.
        /// </summary>
        /// <param name="verticesLength">Full list of vertices for the <see cref="Terrain"/></param>
        /// <param name="offsetX">offsetX value</param>
        /// <param name="offsetY">offsetY value</param>
        /// <param name="section"><see cref="Enums.QuadSection"/> Enum</param>
        /// <param name="rootWidth">root width</param>
        private TerrainQuadTree(ref int verticesLength, int offsetX, int offsetY, QuadSection section, int rootWidth)
        {
            // Set Default Values for Struct
            {
                LeafPatch = new TerrainQuadPatch();
                Leaf = false;
                IndexBufferData = new List<int>();
                WidthXHeight = 0;
                _vertexBufferOffset = 0;
                BoundingBoxHit = false;
                _minHeight = 1000000;
                _maxHeight = 0;
                OffsetX = 0;
                OffsetY = 0;
                TmpIndexBufferData = new int[1];
#if DEBUG && !XBOX360
                _boundingBoxMesh = new VertexPositionColor[1];
#endif
                _lastCorner = Vector2.Zero;
                _firstCorner = Vector2.Zero;
                Detail = TerrainData.DetailDefaultLevel1;
                LODLevel = TessellateLevel.Level1;
                TreeList = null;
                TopLeftIndex = 0;
                TopRightIndex = 0;
                BottomLeftIndex = 0;
                BottomRightIndex = 0;
                QuadSection = section; // 11/17/09
                ParentQuadKeyInstance = null;
                _treeBoundingBox = new BoundingBox();
                _scenaryItemTypes = new Dictionary<int, Dictionary<int, int>>(); // 7/9/2009; 6/7/2012
            }

            IndexBufferData = new List<int>();

            // Ben - Add 1 to QuadKey & Store into QuadKeyInstance
            _quadKeyCounter++;
            QuadKey = _quadKeyCounter;

            OffsetX = offsetX;
            OffsetY = offsetY;

            // This truncation requires all heightmap images to be
            // a power of two in _height and _width
            Width = ((int) Math.Sqrt(verticesLength)/2) + 1;
            Height = Width;
            RootWidth = rootWidth;

            WidthXHeight = Width*Height;

            SetUpBoundingBoxes(this);

            // If this tree is the smallest allowable size, set it as a _leaf
            // so that it will not continue branching smaller.
            if ((Width - 1)*(Height - 1) <= TerrainData.MinimumLeafSize)
            {
                Leaf = true;

                // 4/8/2008 - Set Tessellation Level to 1
                LODLevel = TessellateLevel.Level1;
                Detail = TerrainData.DetailDefaultLevel1;

#if DEBUG && !XBOX360
                CreateBoundingBoxMesh(ref _treeBoundingBox, out _boundingBoxMesh);
#else
                CreateBoundingBoxMesh(ref _treeBoundingBox);
#endif

                // 3/3/2009 - Updated to get the Indices Array, via the 'Out' parameter.
                LeafPatch = new TerrainQuadPatch(TerrainShapeInterface.Device, Width,
                                                  TerrainData.DetailDefaultLevel1, offsetX, offsetY,
                                                  out TmpIndexBufferData);
                // 4/21/2009
                IndexBufferData.Clear();
                IndexBufferData.InsertRange(0, TmpIndexBufferData);

                // 8/25/2008 - Add Leaf to Static List
                TreeLeafList.Add(this);
                //TreeLeafList[TreeLeafCount] = this;
                TreeLeafCount++;

                // 4/9/2008
                // Store the relative Quad Position into QuadPositionArray.              
                StoreQuadLocationIntoArray(offsetX, offsetY, QuadKey);

                // 4/25/2008
                // Store a Ref of current Quad into Terrain Dictionary
                TerrainData.QuadList.Add(QuadKey, this);

                // 5/6/2008 - Test having textures per Quad
                /*TerrainTextures = new Texture2D[8];
                TerrainTextures[0] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround01");
                TerrainTextures[1] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround02");
                TerrainTextures[2] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround03");
                TerrainTextures[3] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround04");

                TerrainTextures[4] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround05");
                TerrainTextures[5] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround06");
                TerrainTextures[6] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround07");
                TerrainTextures[7] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampRock01");*/
            }
            else
                BranchOff();
        }

        /// <summary>
        /// Private construtor, used to create additional <see cref="TerrainQuadTree"/> instances.
        /// </summary>
        /// <param name="verticesLength">Full list of vertices for the <see cref="Terrain"/></param>
        /// <param name="offsetX">offsetX value</param>
        /// <param name="offsetY">offsetY value</param>
        /// <param name="section"><see cref="Enums.QuadSection"/> Enum</param>
        /// <param name="rootWidth">root width</param>
        /// <param name="parentQuadKeyInstance">parent's quad key</param>
        public TerrainQuadTree(int verticesLength, int offsetX, int offsetY, int rootWidth, QuadSection section,
                               int parentQuadKeyInstance)
        {
            // Set Default Values for Struct
            {
                LeafPatch = new TerrainQuadPatch();
                Leaf = false;
                IndexBufferData = new List<int>();
                WidthXHeight = 0;
                _vertexBufferOffset = 0;
                BoundingBoxHit = false;
                _minHeight = 1000000;
                _maxHeight = 0;
                OffsetX = 0;
                OffsetY = 0;
                TmpIndexBufferData = new int[1];
#if DEBUG && !XBOX360
                _boundingBoxMesh = new VertexPositionColor[1];
#endif
                _lastCorner = new Vector2();
                _firstCorner = new Vector2();
                Detail = TerrainData.DetailDefaultLevel2;
                LODLevel = TessellateLevel.Level2;
                TreeList = null;
                TopLeftIndex = 0;
                TopRightIndex = 0;
                BottomLeftIndex = 0;
                BottomRightIndex = 0;
                QuadSection = section; // 11/17/09 - Should be set to the 'Section' param, and not alway 'TopLeft'.
                ParentQuadKeyInstance = null;
                _treeBoundingBox = new BoundingBox();
                _scenaryItemTypes = new Dictionary<int, Dictionary<int, int>>(); // 7/9/2009; 6/7/2012
            }


            IndexBufferData = new List<int>();

            // Ben - Add 1 to QuadKey & Store into QuadKeyInstance
            _quadKeyCounter++;
            QuadKey = _quadKeyCounter;

            OffsetX = offsetX;
            OffsetY = offsetY;

            // This truncation requires all heightmap images to be
            // a power of two in _height and _width
            Width = ((int) Math.Sqrt(verticesLength)/2) + 1;
            Height = Width;
            RootWidth = rootWidth;

            //string QKey = "Quad#" + _quadKeyInstance.ToString();

            LODLevel = TessellateLevel.Level2;
            Detail = TerrainData.DetailDefaultLevel2;
            Leaf = true;

            // 3/3/2009 - Updated to get the Indices Array, via the 'Out' parameter.
            // Create LeafPatch
            LeafPatch = new TerrainQuadPatch(TerrainShapeInterface.Device, Width, Detail, offsetX, offsetY,
                                              out TmpIndexBufferData);
            // 4/21/2009
            IndexBufferData.Clear();
            IndexBufferData.InsertRange(0, TmpIndexBufferData);

            // 8/25/2008 - Add Leaf to Static List
            TreeLeafList.Add(this);
            //TreeLeafList[TreeLeafCount] = this;
            TreeLeafCount++;

            // Store QuadSection
            QuadSection = section;
            // Store ParentQuadKey Instance
            ParentQuadKeyInstance = parentQuadKeyInstance;
            // Store this Child's Quad section into quadLocationArrayL2
            StoreChildQuadIntoLocationArray2(section, parentQuadKeyInstance, QuadKey);
            // Store ChildToParent Key RelationShip
            TerrainData.QuadChildToParent.Add(QuadKey, parentQuadKeyInstance);
            // 4/25/2008
            // Store a Ref of current Quad into Terrain Dictionary
            TerrainData.QuadList.Add(QuadKey, this);
            // 4/25/2008
            // Store the Parent Quad #
            if (!TerrainShapeInterface.QuadParentsTessellated.Contains(parentQuadKeyInstance))
                TerrainShapeInterface.QuadParentsTessellated.Add(parentQuadKeyInstance);

            // Eliminate Cracks for Quad - Bottom
            var quadAdjacentBottomKey = TerrainData.GetAdjacentQuadInstanceKey(parentQuadKeyInstance,
                                                                                QuadAdjacent.Bottom, section);
            if (quadAdjacentBottomKey != null)
                TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentBottomKey, QuadAdjacent.Bottom, section,
                                                        Detail);

            // Eliminate Cracks for Quad - Top
            var quadAdjacentTopKey = TerrainData.GetAdjacentQuadInstanceKey(parentQuadKeyInstance, QuadAdjacent.Top,
                                                                             section);
            if (quadAdjacentTopKey != null)
                TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentTopKey, QuadAdjacent.Top, section, Detail);

            // Eliminate Cracks for Quad - Left
            var quadAdjacentLeftKey = TerrainData.GetAdjacentQuadInstanceKey(parentQuadKeyInstance, QuadAdjacent.Left,
                                                                              section);
            if (quadAdjacentLeftKey != null)
                TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentLeftKey, QuadAdjacent.Left, section, Detail);

            // Eliminate Cracks for Quad - Right
            var quadAdjacentRightKey = TerrainData.GetAdjacentQuadInstanceKey(parentQuadKeyInstance, QuadAdjacent.Right,
                                                                               section);
            if (quadAdjacentRightKey != null)
                TerrainEditRoutines.CrackFixCurrentQuad((int) quadAdjacentRightKey, QuadAdjacent.Right, section, Detail);

            // 5/6/2008 -  Test having textures per Quad
            /*TerrainTextures = new Texture2D[8];
            TerrainTextures[0] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround01");
            TerrainTextures[1] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround02");
            TerrainTextures[2] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround03");
            TerrainTextures[3] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround04");

            TerrainTextures[4] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround05");
            TerrainTextures[5] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround06");
            TerrainTextures[6] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampGround07");
            TerrainTextures[7] = TerrainShape.Content.Load<Texture2D>(SpacewarGame.Settings.MediaPath + @"Terrain\texturesSwampSet\SwampRock01");*/

            SetUpBoundingBoxes(this);

#if DEBUG && !XBOX360
            CreateBoundingBoxMesh(ref _treeBoundingBox, out _boundingBoxMesh);
#else
            CreateBoundingBoxMesh(ref _treeBoundingBox);
#endif
        }

        // 7/10/2009
        private static void CameraUpdated(object sender, EventArgs e)
        {
            UpdateSceneryCulledList = true;
        }

        // 10/31/2008
        /// <summary>
        /// <see cref="EventHandler"/> for the 'TerrainShapeCreated' event, which will set the internal reference.
        /// </summary>
        private static void TerrainShapeTerrainShapeCreated(object sender, EventArgs e)
        {
            TerrainShapeInterface = (ITerrainShape) sender;
        }


        // Use this when creating child trees/branches

        // 4/9/2008 -
        /// <summary>
        ///  Helper Function: Store Quad Location into QuadLocationArray  
        /// </summary>
        /// <param name="offsetX">offsetX value</param>
        /// <param name="offsetY">offsetY value</param>
        /// <param name="quadKeyInstance"></param>
        private static void StoreQuadLocationIntoArray(float offsetX, float offsetY, int quadKeyInstance)
        {
            // 8/26/2009 - Cache
            var mapWidth = TerrainData.MapWidth;
            const int quadTreeWidth = TerrainData.QuadTreeWidth;

            var normalizeX = ((offsetX + 1)/mapWidth);
            var normalizeY = ((offsetY + 1)/mapWidth);

            var i = (mapWidth/quadTreeWidth); // 11/17/09
            var quadPosX = (int) (normalizeX*i);
            var quadPosY = (int) (normalizeY*i);

            TerrainData.QuadLocationArray[quadPosX, quadPosY] = quadKeyInstance;
        }

        // 4/8/2008 - 
        // Use this when Tessellating the current Quad from LOD Level-1 to LOD Level-2       


        // 4/24/2008 - Helper Fn: Stores the current Child Quads keyInstance into the
        //                        Terrain's quadLocationArrayL2, so it can be searched
        //                        upon quickly when needed.        
        private static void StoreChildQuadIntoLocationArray2(QuadSection section, int parentQuadKeyInstance, int quadKeyInstance)
        {
            //key = string.Empty;
            float key = -1;
            switch (section)
            {
                case QuadSection.TopLeft:
                    //key = "Quad#" + parentQuadKeyInstance.ToString() + "-TL";
                    key = parentQuadKeyInstance + 0.1f;
                    break;
                case QuadSection.TopRight:
                    //key = "Quad#" + parentQuadKeyInstance.ToString() + "-TR";
                    key = parentQuadKeyInstance + 0.2f;
                    break;
                case QuadSection.BottomLeft:
                    //key = "Quad#" + parentQuadKeyInstance.ToString() + "-BL";
                    key = parentQuadKeyInstance + 0.3f;
                    break;
                case QuadSection.BottomRight:
                    //key = "Quad#" + parentQuadKeyInstance.ToString() + "-BR";
                    key = parentQuadKeyInstance + 0.4f;
                    break;
                default:
                    break;
            }

            if (!TerrainData.QuadLocationArrayL2.ContainsKey(key))
                TerrainData.QuadLocationArrayL2.Add(key, quadKeyInstance);
        }


        // 4/3/2008
        /// <summary>
        ///  Rebuild Normals for VertexData by recursively going down the _treeList
        ///  and rebuilding each Quad Section using the given Quad IndexBuffer for accuracy.
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> instance</param>
        /// <param name="vertexData"><see cref="VertexMultitextured_Stream1"/> collection</param>
        public static void RebuildNormalsUsingQuadIb(ref TerrainQuadTree terrainQuadTree, ref VertexMultitextured_Stream1[] vertexData)
        {
            if (terrainQuadTree.Leaf)
            {
                // 4/4/2008 - Create mNormal on Heap once here, rather than in loop below!
                var mNormal = Vector3.Zero;

                // 1st - Clear out old Normals
                var indexBufferLengthBy3 = terrainQuadTree.TmpIndexBufferData.Length/3;
                for (var loop1 = 0; loop1 < indexBufferLengthBy3; loop1++)
                {
                    // First clear our old Normals
                    var index0 = loop1*3; // 8/26/2009
                    vertexData[terrainQuadTree.TmpIndexBufferData[index0]].Normal = mNormal;  
                    vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 1)]].Normal = mNormal;  
                    vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 2)]].Normal = mNormal; 
                }

                // 2nd - Update Normals in VertexData Array
                Vector3 normal;
                for (var loop1 = 0; loop1 < indexBufferLengthBy3; loop1++)
                {
                    // Normals
                    var index0 = loop1*3; // 8/26/2009
                    var firstvec = vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 1)]].Position -
                                       vertexData[terrainQuadTree.TmpIndexBufferData[index0]].Position;
                    var secondvec = vertexData[terrainQuadTree.TmpIndexBufferData[index0]].Position -
                                        vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 2)]].Position;
                    //Vector3 normal = Vector3.Cross(firstvec, secondvec);
                    Vector3.Cross(ref firstvec, ref secondvec, out normal);
                    normal.Normalize();

                    vertexData[terrainQuadTree.TmpIndexBufferData[index0]].Normal += normal; 
                    vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 1)]].Normal += normal; 
                    vertexData[terrainQuadTree.TmpIndexBufferData[(index0 + 2)]].Normal += normal; 
                }
            }
                // If there are branches on this node, move down through them recursively
            else if (terrainQuadTree.TreeList != null)
            {
                // 10/2/2008: Updated to use ForLoop, rather than ForEach.
                var count = terrainQuadTree.TreeList.Count; // 8/26/2009
                for (var i = 0; i < count; i++)
                {
                    var quadTree = terrainQuadTree.TreeList[i];
                    RebuildNormalsUsingQuadIb(ref quadTree, ref vertexData);
                }
            }
        }

        // XNA 4.0 updates - removed 2/3 vertex streams from param. 
        ///<summary>
        /// Refences article at 'http://www.terathon.com/code/tangent.html'; 
        ///</summary>
        ///<param name="terrainQuadTree"><see cref="TerrainQuadTree"/> instance</param>
        ///<param name="vertexData"><see cref="VertexMultitextured_Stream1"/> collection</param>
        public static void RebuildTangentDataUsingQuadIb(ref TerrainQuadTree terrainQuadTree, ref VertexMultitextured_Stream1[] vertexData)
        {
            if (terrainQuadTree.Leaf)
            {
                var vertexCount = terrainQuadTree.TmpIndexBufferData.Length;
                var tan1 = new Vector3[vertexCount*2];
                var tan2 = new Vector3[vertexCount*3];

                // Update Tangents/BiNormals in VertexData Array
                var indexBufferLengthBy3 = vertexCount / 3;
                for (var loop1 = 0; loop1 < indexBufferLengthBy3; loop1++)
                {
                    var index0 = loop1 * 3;

                    // 1st - Retrieve verticies of triangle
                    var v1 = vertexData[index0].Position;
                    var v2 = vertexData[index0 + 1].Position;
                    var v3 = vertexData[index0 + 2].Position;

                    // 2nd - Retrieve texture coordinates of this triangle.
                    var w1 = vertexData[index0].TextureCoordinate1.ToVector2();
                    var w2 = vertexData[index0 + 1].TextureCoordinate1.ToVector2();
                    var w3 = vertexData[index0 + 2].TextureCoordinate1.ToVector2();

                    // 3rd - Calculate Diff equations
                    // Vert;
                    var x1 = v2.X - v1.X;
                    var x2 = v3.X - v1.X;
                    var y1 = v2.Y - v1.Y;
                    var y2 = v3.Y - v1.Y;
                    var z1 = v2.Z - v1.Z;
                    var z2 = v3.Z - v1.Z;
                    // Texs;
                    var s1 = w2.X - w1.X;
                    var s2 = w3.X - w1.X;
                    var t1 = w2.Y - w1.Y;
                    var t2 = w3.Y - w1.Y;

                    // 4th - Calculate directions
                    var r = 1.0f / (s1 * t2 - s2 * t1);
                    var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                    var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                    // check for NaNs in sDir.
                    if (float.IsNaN(sdir.X) || float.IsNaN(sdir.Y) || float.IsNaN(sdir.Z))
                    {
                        // then calc the tangent/binormals manually
                        var tangentVector = Vector3.Right;
                        var normal = vertexData[index0].Normal;
                       
                        Vector3.Cross(ref tangentVector, ref normal, out sdir);
                        Vector3.Cross(ref sdir, ref normal, out tdir);
                    }
                    // check for NaNs in tDir
                    if (float.IsNaN(tdir.X) || float.IsNaN(tdir.Y) || float.IsNaN(tdir.Z))
                    {
                        // then calc the binormal manually
                        var normal = vertexData[index0].Normal;
                       
                        Vector3.Cross(ref sdir, ref normal, out tdir);
                    }

                    tan1[index0] += sdir;
                    tan1[index0 + 1] += sdir;
                    tan1[index0 + 2] += sdir;

                    tan2[index0] += tdir;
                    tan2[index0 + 1] += tdir;
                    tan2[index0 + 2] += tdir;
                    
                }

                // Update VB with new Tangent/BiNormals
                for (var i = 0; i < vertexCount; i++)
                {
                    var normal = vertexData[i].Normal;
                    var t = tan1[i];

                    // Gram-Schmidt orthogonalize  
                    var tangent = t - normal * Vector3.Dot(normal, t);
                    tangent.Normalize();

                    // Calculate handedness (here maybe you need to switch >= with <= depend on the geometry winding order)  
                    var tangentdir = (Vector3.Dot(Vector3.Cross(normal, t), tan2[i]) >= 0.0f) ? 1.0f : -1.0f;
                    var binormal = Vector3.Cross(normal, tangent) * tangentdir;  

                    // Directly update with new values
                    vertexData[i].Tangent = new HalfVector4(tangent.X, tangent.Y, tangent.Z, 0); // 1/29/2010 - Update to Packed format
                    vertexData[i].BiNormal = new HalfVector4(binormal.X, binormal.Y, binormal.Z, 0); // 1/29/2010 - Update to Packed format
                }
            }
            // If there are branches on this node, move down through them recursively
            else if (terrainQuadTree.TreeList != null)
            {
                var count = terrainQuadTree.TreeList.Count; 
                for (var i = 0; i < count; i++)
                {
                    var quadTree = terrainQuadTree.TreeList[i];
                    RebuildTangentDataUsingQuadIb(ref quadTree, ref vertexData);
                }
            }
        }

        /// <summary>
        /// Creates the invisible bounding boxes, used to detected cursor ray collisions during pick routines.
        /// </summary>
        /// <param name="terrainQuadTree">this instance of <see cref="TerrainQuadTree"/></param>
        private static void SetUpBoundingBoxes(TerrainQuadTree terrainQuadTree)
        {
            // 8/26/2009 - Cache
            const int scale = TerrainData.cScale;
            var mapHeight = TerrainData.MapHeight;
            var heightData = TerrainData.HeightData;
            var widthMinus1 = terrainQuadTree.Width - 1; // 5/19/2010
            var heightMinus1 = terrainQuadTree.Height - 1; // 5/19/2010

            terrainQuadTree._firstCorner.X = terrainQuadTree.OffsetX * scale;
            terrainQuadTree._firstCorner.Y = terrainQuadTree.OffsetY * scale;

            terrainQuadTree._lastCorner.X = (widthMinus1 + terrainQuadTree.OffsetX) * scale;
            terrainQuadTree._lastCorner.Y = (heightMinus1 + terrainQuadTree.OffsetY) * scale;

            // Determine heights for use with the bounding box
            for (var loopX = 0; loopX < widthMinus1; loopX++)
                for (var loopY = 0; loopY < heightMinus1; loopY++)
                {
                    // 5/19/2010 - Cache calculation
                    var index0 = (loopX + terrainQuadTree.OffsetX) + (loopY + terrainQuadTree.OffsetY) * mapHeight;

                    if (heightData[index0] <
                        terrainQuadTree._minHeight)
                        terrainQuadTree._minHeight =
                            heightData[index0] - 0.1f;
                    else if (heightData[index0] >
                             terrainQuadTree._maxHeight)
                        terrainQuadTree._maxHeight =
                            heightData[index0];
                }

            terrainQuadTree._treeBoundingBox = new BoundingBox(new Vector3(terrainQuadTree._firstCorner.X, terrainQuadTree._minHeight, terrainQuadTree._firstCorner.Y),
                                               new Vector3(terrainQuadTree._lastCorner.X, terrainQuadTree._maxHeight, terrainQuadTree._lastCorner.Y));

            // 11/18/2009 - Checks if already exist.
            // 3/31/2008: Ben: Add Quad's BoundingBox to Dictionary for use in Picking routines. 
            BoundingBox boundingBox;
            if (TerrainShapeInterface.TerrainBoundingBoxes.TryGetValue(terrainQuadTree.QuadKey, out boundingBox))
            {
                // update
                boundingBox = terrainQuadTree._treeBoundingBox;
                TerrainShapeInterface.TerrainBoundingBoxes[terrainQuadTree.QuadKey] = boundingBox;
            }
            else
                TerrainShapeInterface.TerrainBoundingBoxes.Add(terrainQuadTree.QuadKey, terrainQuadTree._treeBoundingBox);
        }

#if DEBUG && !XBOX360
        // 9/12/2008 - Optimize for memory.
        /// <summary>
        /// Method helper, which creates the actual bounding box mesh used to draw the boxes during debug mode.
        /// </summary>
        /// <param name="treeBoundingBox"><see cref="BoundingBox"/> structure</param>
        /// <param name="boundingBoxMesh">(OUT) collection of <see cref="VertexPositionColor"/></param>
        private static void CreateBoundingBoxMesh(ref BoundingBox treeBoundingBox,
                                                  out VertexPositionColor[] boundingBoxMesh)
        {
            // 8/21/2008 - Clear
            BoxList.Clear();

            boundingBoxMesh = new VertexPositionColor[36];
            for (var loop1 = 0; loop1 < 36; loop1++)
                boundingBoxMesh[loop1].Color = Color.Magenta;

            // 1/28/2009: Updated to use the Overload-2 version, which takes a current Vector[] array, to optimize memory.
            treeBoundingBox.GetCorners(ThisVectors);
            

            var length = treeBoundingBox.GetCorners().Length; // 8/24/2009
            for (var loop1 = 0; loop1 < length; loop1++)
            {
                BoxList.Add(ThisVectors[loop1]);
            }
            

            // Front
            boundingBoxMesh[0].Position = BoxList[0];
            boundingBoxMesh[1].Position = BoxList[1];
            boundingBoxMesh[2].Position = BoxList[2];

            boundingBoxMesh[3].Position = BoxList[2];
            boundingBoxMesh[4].Position = BoxList[3];
            boundingBoxMesh[5].Position = BoxList[0];

            // Top
            boundingBoxMesh[6].Position = BoxList[0];
            boundingBoxMesh[7].Position = BoxList[5];
            boundingBoxMesh[8].Position = BoxList[1];

            boundingBoxMesh[9].Position = BoxList[0];
            boundingBoxMesh[10].Position = BoxList[4];
            boundingBoxMesh[11].Position = BoxList[5];

            // Left
            boundingBoxMesh[12].Position = BoxList[0];
            boundingBoxMesh[13].Position = BoxList[3];
            boundingBoxMesh[14].Position = BoxList[7];

            boundingBoxMesh[15].Position = BoxList[7];
            boundingBoxMesh[16].Position = BoxList[4];
            boundingBoxMesh[17].Position = BoxList[0];

            // Right
            boundingBoxMesh[18].Position = BoxList[1];
            boundingBoxMesh[19].Position = BoxList[5];
            boundingBoxMesh[20].Position = BoxList[6];

            boundingBoxMesh[21].Position = BoxList[6];
            boundingBoxMesh[22].Position = BoxList[3];
            boundingBoxMesh[23].Position = BoxList[1];

            // Bottom
            boundingBoxMesh[24].Position = BoxList[3];
            boundingBoxMesh[25].Position = BoxList[7];
            boundingBoxMesh[26].Position = BoxList[6];

            boundingBoxMesh[27].Position = BoxList[6];
            boundingBoxMesh[28].Position = BoxList[2];
            boundingBoxMesh[29].Position = BoxList[3];

            // Back
            boundingBoxMesh[30].Position = BoxList[7];
            boundingBoxMesh[31].Position = BoxList[4];
            boundingBoxMesh[32].Position = BoxList[6];

            boundingBoxMesh[33].Position = BoxList[6];
            boundingBoxMesh[34].Position = BoxList[4];
            boundingBoxMesh[35].Position = BoxList[5];
        }
#else
         // 8/24/2009 - Overload method used just for release versions.
        private static void CreateBoundingBoxMesh(ref BoundingBox treeBoundingBox)
        {
            // 1/28/2009: Updated to use the Overload-2 version, which takes a current Vector[] array, to optimize memory.
            treeBoundingBox.GetCorners(ThisVectors);
        }
#endif

        #endregion

        // 8/12/2008
        /// <summary>
        /// Clears out all LeafPatch References and QuadTree List.
        /// Primarily called from TerrainStorageRoutine when loading
        /// a new map.
        /// </summary>
        public void ClearQuadTree()
        {
            // Clear Static Arrays
            if (TreeLeafList != null)
                TreeLeafList.Clear();
            TreeLeafList = null;

            // Iterate Recursively through QuadTree List and delete
            if (Leaf)
            {
                // Dispose
                if (_lineEffect != null)
                    _lineEffect.Dispose();
                if (_lineVertexDeclaration != null)
                    _lineVertexDeclaration.Dispose();
                if (IndexBufferData != null)
                    IndexBufferData.Clear();
                LeafPatch.Dispose(); // 1/8/2010

                // 1/8/2010 - Clear Arrays
                if (_scenaryItemTypes != null)
                    _scenaryItemTypes.Clear();
#if DEBUG && !XBOX360
                if (_boundingBoxMesh != null)
                    Array.Clear(_boundingBoxMesh, 0, _boundingBoxMesh.Length);
#endif
                if (TmpIndexBufferData != null) 
                    Array.Clear(TmpIndexBufferData, 0, TmpIndexBufferData.Length);
                
                if (BoxList != null)
                    BoxList.Clear();
                if (ThisVectors != null)
                    Array.Clear(ThisVectors, 0, ThisVectors.Length);

                // Null Refs
                _lineEffect = null;
                _lineVertexDeclaration = null;
                TerrainShapeInterface = null;
                IndexBufferData = null;
                TmpIndexBufferData = null;
                LeafPatch.IndexBuffers = null;
                //_leafPatch.Parent = null;
                //_leafPatch = null;
#if DEBUG && !XBOX360
                _boundingBoxMesh = null;
#endif
            }
                // If there are branches on this node, move down through them recursively
            else if (TreeList != null)
            {
                var count = TreeList.Count;
                for (var i = 0; i < count; i++)
                {
                    TreeList[i].ClearQuadTree();

                    // Clear Quads
                    //topLeft = null;
                    //topRight = null;
                    //bottomLeft = null;
                    //bottomRight = null;  
                }

                // Now Delete the entire List Array for this Instance
                TreeList.Clear();
                TreeList = null;
            }
        }
       
        /// <summary>
        /// Only called from the main root node, to create 4 additional <see cref="TerrainQuadTree"/> branch child instances.
        /// </summary>
        private void BranchOffRoot()
        {
            TreeList = new List<TerrainQuadTree> {new TerrainQuadTree(ref WidthXHeight, 0, 0, QuadSection.TopLeft, RootWidth)};
            TopLeftIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, Width / 2, 0, QuadSection.TopRight, RootWidth));
            TopRightIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, 0, Height / 2, QuadSection.BottomLeft, RootWidth));
            BottomLeftIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, Width / 2, Height / 2, QuadSection.BottomRight, RootWidth));
            BottomRightIndex = TreeList.Count - 1;
        }
       
        /// <summary>
        /// This is called to branch off of child nodes, creating 4 additional <see cref="TerrainQuadTree"/> branch child instances.
        /// </summary>
        private void BranchOff()
        {
            TreeList = new List<TerrainQuadTree> { new TerrainQuadTree(ref WidthXHeight, OffsetX, OffsetY, QuadSection.TopLeft, RootWidth) };
            TopLeftIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, (Width - 1) / 2 + OffsetX, OffsetY, QuadSection.TopRight, RootWidth));
            TopRightIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, OffsetX, (Height - 1) / 2 + OffsetY, QuadSection.BottomLeft, RootWidth));
            BottomLeftIndex = TreeList.Count - 1;
            TreeList.Add(new TerrainQuadTree(ref WidthXHeight, (Width - 1)/2 + OffsetX, (Height - 1) / 2 + OffsetY, QuadSection.BottomRight, RootWidth));
            BottomRightIndex = TreeList.Count - 1;
        }

        // 11/18/2009 - TEST
        /*public static void Draw(BoundingFrustum bFrustum, Effect terrainEffect)
        {
            // cache GraphicsDevice to improve CPI in Vtune.
            GraphicsDevice graphicsDevice = ImageNexusRTSGameEngine.GameInstance.GraphicsDevice;
            // cache Technique                    
            EffectTechnique technique = terrainEffect.CurrentTechnique;

            // iterate the Terrain Leaf List
            for (int i = 0; i < TreeLeafList.Count; i++)
            {
                var leaf = TreeLeafList[i];

                // Check only the Quads within the Camera view.
                ContainmentType cameraNodeContainment;
                bFrustum.Contains(ref leaf._treeBoundingBox, out cameraNodeContainment);

                if (cameraNodeContainment == ContainmentType.Disjoint) continue;

                graphicsDevice.Indices = leaf._leafPatch.IndexBuffers;

                terrainEffect.Begin();
                var count = technique.Passes.Count; // 8/13/2009
                for (int loop2 = 0; loop2 < count; loop2++)
                {
                    technique.Passes[loop2].Begin();

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, leaf._vertexBufferOffset, 0, leaf._widthXHeight, 0, leaf._leafPatch.NumTris);

                    technique.Passes[loop2].End();
                }

                terrainEffect.End();


                // 7/9/2009 - Now create scenery items 'Culled' list connected to this Quad!
                if (leaf._scenaryItemTypes.Count > 0 && UpdateSceneryCulledList)
                    InstancedItem.CreateSceneryInstancesCulledList(leaf._scenaryItemTypes);
            }
        }*/

        // 7/7/2009
        /// <summary>
        /// Draws the <see cref="TerrainQuadTree"/> using a recursive search, directly working
        /// towards the section of the <see cref="TerrainQuadTree"/> structure which is in the <see cref="Camera"/> frustrum.
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start recurive search</param>
        /// <param name="bFrustum"><see cref="BoundingFrustum"/> instance</param>
        /// <param name="terrainEffect"><see cref="Effect"/> used to draw terrain.</param>
        public static void Draw(ref TerrainQuadTree terrainQuadTree, BoundingFrustum bFrustum, Effect terrainEffect)
        {
            // Check only the Quads within the Camera view.
            ContainmentType cameraNodeContainment;
            bFrustum.Contains(ref terrainQuadTree._treeBoundingBox, out cameraNodeContainment);

            if (cameraNodeContainment == ContainmentType.Disjoint) return;

            if (terrainQuadTree.Leaf)
            {
                // cache GraphicsDevice to improve CPI in Vtune.
                var graphicsDevice = _graphicsDevice; // 4/21/2010 - Updated to use new static field '_graphicsDevice'
                // cache Technique                    
                var technique = terrainEffect.CurrentTechnique;
                // 4/21/2010 - Cache passes
                var passCollection = technique.Passes;

                graphicsDevice.Indices = terrainQuadTree.LeafPatch.IndexBuffers;

                // XNA 4.0 updates - Begin() and End() obsolete.
                //terrainEffect.Begin();

                var count = passCollection.Count; // 8/13/2009
                var vertexBufferOffset = terrainQuadTree._vertexBufferOffset; // 5/19/2010 - Cache
                var widthXHeight = terrainQuadTree.WidthXHeight; // 5/19/2010 - Cache
                var primitiveCount = terrainQuadTree.LeafPatch.NumTris; // 5/19/2010 - Cache
                for (var i = 0; i < count; i++)
                {
                    // 5/19/2010 - Cache
                    var effectPass = passCollection[i];
                    if (effectPass == null) continue;

                    // XNA 4.0 - Begin() and End() obsolete; Apply() replaces.
                    effectPass.Apply();

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexBufferOffset, 0, widthXHeight,
                                                         0, primitiveCount);
                    //effectPass.End();
                }

                //terrainEffect.End();


                // 7/9/2009 - Now create scenery items 'Culled' list connected to this Quad!
                if (terrainQuadTree._scenaryItemTypes.Count > 0 && UpdateSceneryCulledList) 
                {
                    InstancedItem.CreateSceneryInstancesCulledList(terrainQuadTree._scenaryItemTypes);
                }
            }
            else
            {
                // Search each child-quad.
                var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010
                if (terrainQuadTrees != null)
                {
                    var terrainQuadTree1 = terrainQuadTrees[terrainQuadTree.TopLeftIndex];
                    var terrainQuadTree2 = terrainQuadTrees[terrainQuadTree.TopRightIndex];
                    var terrainQuadTree3 = terrainQuadTrees[terrainQuadTree.BottomLeftIndex];
                    var terrainQuadTree4 = terrainQuadTrees[terrainQuadTree.BottomRightIndex];

                    Draw(ref terrainQuadTree1, bFrustum, terrainEffect);
                    Draw(ref terrainQuadTree2, bFrustum, terrainEffect);
                    Draw(ref terrainQuadTree3, bFrustum, terrainEffect);
                    Draw(ref terrainQuadTree4, bFrustum, terrainEffect);
                }
            }
        }

        // 7/8/2009
        /// <summary>
        /// Draws the entire <see cref="TerrainQuadTree"/> using a recursive search.
        /// </summary>    
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start recurive search</param>
        /// <param name="terrainEffect"><see cref="Effect"/> used to draw terrain.</param>   
        public static void Draw(ref TerrainQuadTree terrainQuadTree, Effect terrainEffect)
        {
            if (terrainQuadTree.Leaf)
            {
                // cache GraphicsDevice to improve CPI in Vtune.
                var graphicsDevice = _graphicsDevice; // 4/21/2010 - Updated to use new static field '_graphicsDevice'
                // cache Technique                
                var technique = terrainEffect.CurrentTechnique;
                // 4/21/2010 - Cache passes
                var passCollection = technique.Passes;

                graphicsDevice.Indices = terrainQuadTree.LeafPatch.IndexBuffers;

                // XNA 4.0 updates - Begin() and End() obsolete.
                //terrainEffect.Begin();
                
                var count = passCollection.Count;
                var vertexBufferOffset = terrainQuadTree._vertexBufferOffset; // 5/19/2010 - Cache
                var widthXHeight = terrainQuadTree.WidthXHeight; // 5/19/2010 - Cache
                var primitiveCount = terrainQuadTree.LeafPatch.NumTris; // 5/19/2010 - Cache
                for (var i = 0; i < count; i++)
                {
                    // 5/19/2010 - Cache
                    var effectPass = passCollection[i];
                    if (effectPass == null) continue;

                    // XNA 4.0 - Begin() and End() obsolete; Apply() replaces.
                    effectPass.Apply();

                    // 3/9/2011
                    graphicsDevice.BlendState = BlendState.Opaque;

                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, vertexBufferOffset, 0, widthXHeight, 0, primitiveCount);
                    
                    //effectPass.End();
                }

                //terrainEffect.End();
            }
            else
            {
                // Search each child-quad.
                var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
                if (terrainQuadTrees != null)
                {
                    var terrainQuadTree1 = terrainQuadTrees[terrainQuadTree.TopLeftIndex];
                    var terrainQuadTree2 = terrainQuadTrees[terrainQuadTree.TopRightIndex];
                    var terrainQuadTree3 = terrainQuadTrees[terrainQuadTree.BottomLeftIndex];
                    var terrainQuadTree4 = terrainQuadTrees[terrainQuadTree.BottomRightIndex];

                    Draw(ref terrainQuadTree1, terrainEffect);
                    Draw(ref terrainQuadTree2, terrainEffect);
                    Draw(ref terrainQuadTree3, terrainEffect);
                    Draw(ref terrainQuadTree4, terrainEffect);
                }
            }
        }

#if DEBUG && !XBOX360

        /// <summary>
        /// Draws the <see cref="TerrainQuadTree"/> BoundingBox for debug purposes.
        /// </summary>
        public static void DrawBoundingBox(TerrainQuadTree terrainQuadTree, BoundingFrustum bFrustum)
        {
            // Check only the Quads within the Camera view.
            ContainmentType cameraNodeContainment;
            bFrustum.Contains(ref terrainQuadTree._treeBoundingBox, out cameraNodeContainment);
            if (cameraNodeContainment == ContainmentType.Disjoint) return;

            if (terrainQuadTree.Leaf)
            {
                // cache GraphicsDevice to improve CPI in Vtune.
                var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

                _lineEffect.View = Camera.View;
                _lineEffect.Projection = Camera.Projection;

                // XNA 4.0 updates - Begin() and End() obsolete.
                //_lineEffect.Begin();
                _lineEffect.CurrentTechnique.Passes[0].Apply();

                // XNA 4.0 Updates - VertexDeclaration obsolete.
                // Draw the triangle.
                //graphicsDevice.VertexDeclaration = _lineVertexDeclaration;
                graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, terrainQuadTree._boundingBoxMesh, 0, 12);

                // XNA 4.0 updates - Begin() and End() obsolete.
                //_lineEffect.CurrentTechnique.Passes[0].End();
                //_lineEffect.End();
            }
            else
            {
                // Search each child-quad.
                var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
                if (terrainQuadTrees != null)
                {
                    DrawBoundingBox(terrainQuadTrees[terrainQuadTree.TopLeftIndex], bFrustum);
                    DrawBoundingBox(terrainQuadTrees[terrainQuadTree.TopRightIndex], bFrustum);
                    DrawBoundingBox(terrainQuadTrees[terrainQuadTree.BottomLeftIndex], bFrustum);
                    DrawBoundingBox(terrainQuadTrees[terrainQuadTree.BottomRightIndex], bFrustum);
                }
            }
        }

#endif
       
        // 5/18/2010: Updated to return the closest intersection quads; can be more than one in some cases!
        // 7/8/2009
        /// <summary>
        /// Searchs the <see cref="TerrainQuadTree"/> using a recursive search, directly working
        /// towards the section of the <see cref="TerrainQuadTree"/> which is in the <see cref="Camera"/> frustrum, 
        /// while checking if the given <see cref="Ray"/> intersects the <see cref="TerrainQuadPatch"/>.  
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start search from</param>
        /// <param name="rayToCheck"><see cref="Ray"/> to check for intersection</param>
        /// <param name="quads">Collection of two <see cref="QuadToRayIntersection"/></param>
        /// <returns>True/False of results</returns>
        public static void GetQuadForGivenRayIntersectionInCameraFrustum(TerrainQuadTree terrainQuadTree, ref Ray rayToCheck, ref QuadToRayIntersection[] quads)
        {
            var cameraFrustum = Camera.CameraFrustum;

            // Check only the Quads within the Camera view.
            ContainmentType cameraNodeContainment;
            cameraFrustum.Contains(ref terrainQuadTree._treeBoundingBox, out cameraNodeContainment);
            if (cameraNodeContainment == ContainmentType.Disjoint) return;

            if (terrainQuadTree.Leaf)
            {
                // Check if given ray intersects this quad?
                float? isIntersect;
                rayToCheck.Intersects(ref terrainQuadTree._treeBoundingBox, out isIntersect);

                // was there an intersection to some quad?
                if (isIntersect != null)
                {
                    // 5/18/2010 - Store intersection quad, where index 0 is always the closest.
                    if (quads[0].Intersection == null)
                    {
                        quads[0].Intersection = isIntersect;
                        quads[0].Quad = terrainQuadTree;
                    }
                    else
                    {
                        // is new quad intersection the closest?
                        if (isIntersect < quads[0].Intersection)
                        {
                            // Yes, then copy index 0 to 1, and store new quad at 0.
                            quads[1].Intersection = quads[0].Intersection;
                            quads[1].Quad = quads[0].Quad;

                            // New closest Quad
                            quads[0].Intersection = isIntersect;
                            quads[0].Quad = terrainQuadTree;
                        } 
                        // else, is this the new 2nd closest?
                        else if (quads[1].Intersection == null)
                        {
                            // then store 2nd closest quad.
                            quads[1].Intersection = isIntersect;
                            quads[1].Quad = terrainQuadTree;
                        }
                        // else, is this closer than current 2nd cloest?
                        else if (isIntersect < quads[1].Intersection)
                        {
                            // yes, so this is new 2nd closest quad.
                            quads[1].Intersection = isIntersect;
                            quads[1].Quad = terrainQuadTree;
                        }
                    }
                   
                } // Was intersection

                return;
            } // Is leaf.

            // Search each child-quad.
            var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
            if (terrainQuadTrees == null) return;

            GetQuadForGivenRayIntersectionInCameraFrustum(terrainQuadTrees[terrainQuadTree.TopLeftIndex], ref rayToCheck,
                                                                                   ref quads);

            GetQuadForGivenRayIntersectionInCameraFrustum(terrainQuadTrees[terrainQuadTree.TopRightIndex], ref rayToCheck,
                                                                                    ref quads);

            GetQuadForGivenRayIntersectionInCameraFrustum(terrainQuadTrees[terrainQuadTree.BottomLeftIndex], ref rayToCheck,
                                                                                      ref quads);

            GetQuadForGivenRayIntersectionInCameraFrustum(terrainQuadTrees[terrainQuadTree.BottomRightIndex], ref rayToCheck,
                                                                                       ref quads);
        }

        // 7/8/2009
        /// <summary>
        /// Searchs the Terrain, using a recursive search down the QuadTree, checking
        /// if the given 'Ray' intersects the Quadtree patch.  
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start search from</param>
        /// <param name="rayToCheck">Ray to check for intersection</param>
        /// <param name="quad">(OUT) Quad of intersection</param>
        /// <returns>True/False of results</returns>
        public static bool GetQuadForGivenRayIntersection(TerrainQuadTree terrainQuadTree, ref Ray rayToCheck, out TerrainQuadTree quad)
        {
            quad = null;

            if (terrainQuadTree.Leaf)
            {
                // Check if given ray intersects this quad?
                float? isIntersect;
                rayToCheck.Intersects(ref terrainQuadTree._treeBoundingBox, out isIntersect);

                if (isIntersect != null)
                {
                    // yes, so return quad
                    quad = terrainQuadTree;
                    return true;
                }
            }
            else
            {
                // Search each child-quad.
                var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
                if (terrainQuadTrees != null)
                {
                    if (GetQuadForGivenRayIntersection(terrainQuadTrees[terrainQuadTree.TopLeftIndex], ref rayToCheck, out quad))
                        return true;
                    if (GetQuadForGivenRayIntersection(terrainQuadTrees[terrainQuadTree.TopRightIndex], ref rayToCheck, out quad))
                        return true;
                    if (GetQuadForGivenRayIntersection(terrainQuadTrees[terrainQuadTree.BottomLeftIndex], ref rayToCheck, out quad))
                        return true;
                    if (GetQuadForGivenRayIntersection(terrainQuadTrees[terrainQuadTree.BottomRightIndex], ref rayToCheck, out quad))
                        return true;
                }
            } // End if Leaf

            return false;
        }

        // 7/9/2009
        /// <summary>
        /// Will recursively search the <see cref="TerrainQuadTree"/> for the given <paramref name="quadKey"/>, and when found
        /// will store the given scenary <see cref="InstancedItemData"/> in the internal dictionary; this in
        /// turn allows culling of the given items.
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start search from</param>
        /// <param name="quadKey">Quad key for search</param>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <returns>True/False of success</returns>
        public static bool ConnectScenaryItemToGivenQuad(TerrainQuadTree terrainQuadTree, int quadKey, ref InstancedItemData instancedItemData)
        {
            // Recursively search for the correct quad.
            if (terrainQuadTree.QuadKeyInstance == quadKey)
            {
                // Make sure it is only added once!
                if (!terrainQuadTree._scenaryItemTypes.ContainsKey((int)instancedItemData.ItemType))
                {
                    // Create Dictionary<int,int> for itemInstanceKeys.
                    var itemKeys = new Dictionary<int, int> {{instancedItemData.ItemInstanceKey, instancedItemData.ItemInstanceKey}};

                    // Add 'ItemType' with List to dictionary
                    terrainQuadTree._scenaryItemTypes.Add((int)instancedItemData.ItemType, itemKeys);
                }
                else // ItemType already in Dictionary, so update internal List<>.
                {
                    // Retrieve internal Dictionary<int, int>.
                    Dictionary<int, int> itemKeys;
                    if (terrainQuadTree._scenaryItemTypes.TryGetValue((int)instancedItemData.ItemType, out itemKeys))
                    {
                        // Add new key to list
                        if (!itemKeys.ContainsKey(instancedItemData.ItemInstanceKey))
                            itemKeys.Add(instancedItemData.ItemInstanceKey, instancedItemData.ItemInstanceKey);

                        // update list back to dictionary.
                        terrainQuadTree._scenaryItemTypes[(int)instancedItemData.ItemType] = itemKeys;
                    }
                }

                return true;
            }

            // Search each child-quad.
            var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
            if (terrainQuadTrees != null)
            {
                if (ConnectScenaryItemToGivenQuad(terrainQuadTrees[terrainQuadTree.TopLeftIndex], quadKey, ref instancedItemData))
                    return true;
                if (ConnectScenaryItemToGivenQuad(terrainQuadTrees[terrainQuadTree.TopRightIndex], quadKey, ref instancedItemData))
                    return true;
                if (ConnectScenaryItemToGivenQuad(terrainQuadTrees[terrainQuadTree.BottomLeftIndex], quadKey, ref instancedItemData))
                    return true;
                if (ConnectScenaryItemToGivenQuad(terrainQuadTrees[terrainQuadTree.BottomRightIndex], quadKey, ref instancedItemData))
                    return true;
            }

            return false;
        }

        // 6/7/2012
        /// <summary>
        /// Will recursively search the <see cref="TerrainQuadTree"/> for the given <paramref name="instancedItemData"/>, and when found
        /// will disconnect the given scenary <see cref="InstancedItemData"/> in the internal dictionary.
        /// </summary>
        /// <param name="terrainQuadTree"><see cref="TerrainQuadTree"/> root to start search from</param>
        /// <param name="instancedItemData"><see cref="InstancedItemData"/> structure</param>
        /// <returns>True/False of success</returns>
        public static bool DisconnectScenaryItemFromGivenQuad(TerrainQuadTree terrainQuadTree, ref InstancedItemData instancedItemData)
        {
            // Recursively search for the correct quad.
            if (terrainQuadTree.QuadKeyInstance == instancedItemData.QuadKey)
            {
                // Make sure it is only added once!
                if (terrainQuadTree._scenaryItemTypes.ContainsKey((int)instancedItemData.ItemType))
                {
                    // Retrieve internal Dictionary<int, int>.
                    Dictionary<int, int> itemKeys;
                    if (terrainQuadTree._scenaryItemTypes.TryGetValue((int)instancedItemData.ItemType, out itemKeys))
                    {
                        // remove this ItemInstanceKEy from this quad.
                        itemKeys.Remove(instancedItemData.ItemInstanceKey);

                        // update list back to dictionary.
                        terrainQuadTree._scenaryItemTypes[(int)instancedItemData.ItemType] = itemKeys;
                    }
                }

                return true;
            }

            // Search each child-quad.
            var terrainQuadTrees = terrainQuadTree.TreeList; // 5/19/2010 - Cache
            if (terrainQuadTrees != null)
            {
                if (DisconnectScenaryItemFromGivenQuad(terrainQuadTrees[terrainQuadTree.TopLeftIndex], ref instancedItemData))
                    return true;
                if (DisconnectScenaryItemFromGivenQuad(terrainQuadTrees[terrainQuadTree.TopRightIndex], ref instancedItemData))
                    return true;
                if (DisconnectScenaryItemFromGivenQuad(terrainQuadTrees[terrainQuadTree.BottomLeftIndex], ref instancedItemData))
                    return true;
                if (DisconnectScenaryItemFromGivenQuad(terrainQuadTrees[terrainQuadTree.BottomRightIndex], ref instancedItemData))
                    return true;
            }

            return false;
        }

        // 11/18/2009 - Verification test
        /*private static List<TerrainQuadTree> _treeLeafListCopy;
        public static short _verificationCounter;

        // 11/18/2009 - TEST
        public static void CreateVerificationCopy()
        {
            // Create Copy
            _treeLeafListCopy = new List<TerrainQuadTree>(TreeLeafList.Count);

            // Copy original to copy
            _treeLeafListCopy.AddRange(TreeLeafList);
        }

        // 11/18/2009 - Test if new data matches last copy
        public static void DoVerificationTest()
        {
            // iterate new list, and compare to last copy; if different, throw error.
            int count = _treeLeafListCopy.Count;
            for (int i = 0; i < count; i++)
            {
                // get copy node
                var copyNode = _treeLeafListCopy[i];

                // get new node
                var newNode = TreeLeafList[i];

                // check if any differecnes
                for (int j = 0; j < copyNode.IndexBufferData.Count; j++)
                {
                    var indexCopyData = copyNode.IndexBufferData[j];
                    var indexNewData = newNode.IndexBufferData[j];

                    if (indexCopyData != indexNewData)
                        System.Diagnostics.Debugger.Break();
                }
                  
                
            }
        }*/

      
    }
}