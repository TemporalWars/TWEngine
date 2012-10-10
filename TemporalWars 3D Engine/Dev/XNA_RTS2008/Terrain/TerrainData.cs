#region File Description
//-----------------------------------------------------------------------------
// TerrainData.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.GameScreens.Generic;
using ImageNexus.BenScharbach.TWEngine.Terrain.Enums;
using ImageNexus.BenScharbach.TWEngine.Terrain.Structs;
using ImageNexus.BenScharbach.TWEngine.Utilities;
using ImageNexus.BenScharbach.TWEngine.Utilities.Structs;
using ImageNexus.BenScharbach.TWLate.AStarInterfaces.AStarAlgorithm;
using ImageNexus.BenScharbach.TWLate.RTS_FogOfWarInterfaces.FOW;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

#if !XBOX360
using System.Drawing;
using System.Windows.Forms;
using Color = System.Drawing.Color;
#endif

namespace ImageNexus.BenScharbach.TWEngine.Terrain
{
    ///<summary>
    /// The <see cref="TerrainData"/> class is used to hold the important meta-data for the <see cref="TWEngine.Terrain"/> class; for example,
    /// the HeightData collection, <see cref="VertexMultitextured_Stream1"/> collection, and the terrain Normals collection
    /// to name a few.
    ///</summary>
    public class TerrainData : IFOWTerrainData, IMinimapTerrainData
    {
        // Const
        private const float DefTerrainElevStr = 6.0f;

        ///<summary>
        /// Quad-Tree width constant.
        ///</summary>
        public const int QuadTreeWidth = 32;

        ///<summary>
        /// Terrain scale constant.
        ///</summary>
// ReSharper disable InconsistentNaming
        public const int cScale = 10; // 5/14/2009 - Set as const; 12/31/2009: Renamed to 'cScale'.
// ReSharper restore InconsistentNaming

        private static float _scaleOverOne = 1.0f/cScale; // 4/13/2009

        // 4/9/2008: Ben - Add 
        /// <summary>
        /// Array to hold the Quad's Instance numbers relative to other Quads.
        /// This now gives the ability to get another adjacent Quad by simply using it's
        /// own OffsetX/Y value to find it location into the collection, and then move in the
        /// direction of the adjacent Quad and lookup it's Quad Instance Key#.
        /// </summary>
        internal static int[,] QuadLocationArray;

        // 7/8/2009: Add VertexTerrain to hold VertexBuffer Raw Data
        internal static VertexMultitextured_Stream1[] VertexBufferDataStream1;
        // 7/8/2009 - Add 2nd VertexBuffer stream for Texture data.
        //internal static VertexMultitextured_Stream2[] VertexBufferDataStream2;
        // 7/8/2009 - Add 3rd VertexBuffer stream for Tangent data.
        //internal static VertexMultitextured_Stream3[] VertexBufferDataStream3;

#if !XBOX360
        // Add Dictionary Array for use of finding the VertexData Position quickly!
        // 8/26/2008: Updated Key from String to Int.

        ///<summary>
        /// Used for finding the VertexData position quickly.
        ///</summary>
        public static Dictionary<int, int> VertexDataLookup { get; private set; }

#endif


        #region Properties

        // 12/31/2009 - Non-Static Property for interface ref
        /// <summary>
        /// The spacing between the individual triangles when creating the <see cref="TWEngine.Terrain"/> mesh.
        /// </summary>
        int IFOWTerrainData.Scale
        {
            get
            {
                return cScale;
            }
        }

        // 12/31/2009 - Non-Static Property for interface ref
        /// <summary>
        /// Width of heightmap multiplied by scale value.
        /// </summary>
        int IFOWTerrainData.MapWidthToScale
        {
            get { return MapWidthToScale; }
        }

        // 12/31/2009 - Non-Static Property for interface ref
        /// <summary>
        ///  Height of heightmap multiplied by scale value.
        /// </summary>
        int IFOWTerrainData.MapHeightToScale
        {
            get { return MapHeightToScale; }
        }

        // 1/2/2010
        /// <summary>
        /// Width of heightmap, multiplied by scale value.
        /// </summary>
        int IMinimapTerrainData.MapHeightToScale
        {
            get { return MapHeightToScale; }
        }

        // 1/2/2010
        /// <summary>
        ///  Height of heightmap, multiplied by scale value.
        /// </summary>
        int IMinimapTerrainData.MapWidthToScale
        {
            get { return MapWidthToScale; }
        }
      
        // 8/12/2009 - Convert to a Collection, per FxCop.
        private static readonly Vector3 Vector3Zero = Vector3.Zero;

        /// <summary>
        /// Holds the height (Y coordinate) for each [x, z] coordinate
        /// </summary>
        public static float[] HeightData { set; get; }

        // 4/25/2008: 
        /// <summary>
        /// Stores reference to all Quad's in dictionay for direct access, eliminating the
        /// recursive searchs through the root <see cref="TerrainQuadTree"/>.
        /// </summary>
        public static Dictionary<int, TerrainQuadTree> QuadList { get; private set; }

        // 4/25/2008:
        /// <summary>
        /// Quad dictionary used to hold child Quad keys and return the parent key.
        /// </summary>
        public static Dictionary<int, int> QuadChildToParent { get; private set; }

        // 4/14/2008: 
        // 8/26/2008: Updated Key from String to float; Key format is "342.1", where the decimal
        //            number can be 1-4; 1 = TL, 2 = TR, 3 = BL, & 4 = BR.
        /// <summary>
        /// Quad dictionary used to hold the level-2 smaller Quad's. 
        /// </summary>
        /// <remarks>
        /// The Key format is "Quad#342-TL", where 342 is the parent Key and
        /// TL is Top-Left Section, in this example.
        /// </remarks>
        public static Dictionary<float, int> QuadLocationArrayL2 { get; private set; }


        /// <summary>
        /// Holds the normal vectors for each vertex in the <see cref="TWEngine.Terrain"/>.
        /// The normals for lighting are later stored in each vertex, but
        /// we want to store these values permanent for proper physics
        /// collisions with the ground.
        /// </summary>
        public static List<Vector3> TerrainNormals { get; private set; }


        /// <summary>
        /// <see cref="VertexBuffer"/> geometry data for vertex stream-1.
        /// </summary>
        public static VertexBuffer TerrainVertexBufferStream1 { get; private set; }


        // 7/1/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
        /// <summary>
        /// <see cref="VertexBuffer"/> geometry data for vertex stream-1. (2nd copy)
        /// </summary>
        public static VertexBuffer TerrainVertexBufferStream1A { get; private set; }
      

        // XNA 4.0 Updates - For custom VertexDeclarations, the VertexDeclaration is now created there using the 'IVertexType' interface.
        //                               In this case, it would be the 'VertexMultitextured_Steam1...3'.
        /*/// <summary>
        /// Set or get the <see cref="VertexDeclaration"/>.
        /// </summary>
        public static VertexDeclaration TerrainVertDeclaration { get; set; }*/

        /// <summary>
        /// Default terrain <see cref="LOD"/> (level-of-detail) for level-1.
        /// </summary>
        public static LOD DetailDefaultLevel1 { get; set; }

        /// <summary>
        /// Default terrain <see cref="LOD"/> (level-of-detail) for level-2.
        /// </summary>
        public static LOD DetailDefaultLevel2 { get; set; }

        /// <summary>
        /// Terrain <see cref="LOD"/> (level-of-detail) setting.
        /// </summary>
        public static LOD Detail { get; set; }

        /// <summary>
        /// Width of the <see cref="TWEngine.Terrain"/>
        /// </summary>
        public static int MapWidth { get; set; }

        /// <summary>
        /// Width of <see cref="TWEngine.Terrain"/>, multiplied by scale value.
        /// </summary>
        public static int MapWidthToScale { get; private set; }

        /// <summary>
        /// Height of <see cref="TWEngine.Terrain"/>
        /// </summary>
        public static int MapHeight { get; set; }

        /// <summary>
        /// Height of <see cref="TWEngine.Terrain"/>, multiplied by scale value.
        /// </summary>
        public static int MapHeightToScale { get; private set; }

        /// <summary>
        /// Must be power of two values. ALL Quads in the tree will be set to this size.
        /// </summary>
        public static int MinimumLeafSize { get; set; }

        // 4/28/2008 - 
        /// <summary>
        /// Stores middle of <see cref="TWEngine.Terrain"/> Width(X) value.
        /// </summary>
        public static int MiddleMapX { get; private set; }

        /// <summary>
        /// Stores middle of <see cref="TWEngine.Terrain"/> Height(Z) value.
        /// </summary>
        public static int MiddleMapY { get; private set; }

        /// <summary>
        /// Maximum height the <see cref="TWEngine.Terrain"/> can be set to.
        /// </summary>
        public static float? HeightMapMaxHeight { get; private set; }

        /// <summary>
        /// Elevation strength used when creating hills.
        /// </summary>
        public static float ElevationStrength { private get; set; }

        #endregion

        /// <summary>
        /// Sets up the internal collections, like <see cref="QuadList"/>, <see cref="QuadChildToParent"/>, and other misc settings.  MUST be called
        /// after the <see cref="TerrainData"/> 'HeightData' is loaded or created first.
        /// </summary>
        /// <param name="mapWidth">Map width</param>
        /// <param name="mapHeight">Map height</param>
        /// <exception cref="InvalidOperationException">Thrown when either <paramref name="mapWidth"/> or <paramref name="mapHeight"/> is zero.</exception>
        private static void CommonInitilization(int mapWidth, int mapHeight)
        {
            // 6/30/2009 - Make sure mapWidth/mapHeight are set first.
            if (mapWidth == 0 || mapHeight == 0)
                throw new InvalidOperationException(
                    "TerrainData information MUST be set first, before calling this method!");


            MapWidth = mapWidth;
            MapHeight = mapHeight;
            MapWidthToScale = mapWidth*cScale;
            MapHeightToScale = mapHeight*cScale;

            MinimumLeafSize = QuadTreeWidth*QuadTreeWidth;

            QuadList = new Dictionary<int, TerrainQuadTree>();
            QuadChildToParent = new Dictionary<int, int>();
            QuadLocationArrayL2 = new Dictionary<float, int>();

            // Calculate Middle of Map X/Y            
            MiddleMapX = (mapWidth/2)*cScale;
            MiddleMapY = (mapHeight/2)*cScale;

            //terrainNormals = new Vector3[mapWidth * mapHeight];

            QuadLocationArray = new int[(mapWidth/QuadTreeWidth),(mapWidth/QuadTreeWidth)];
            
            // 6/30/2009
            InitAStarPathNodeSize();
        }

        #region oldCode

        // 5/14/2009: Update to be 'Private', and removed the creation of the tmpTerrainVertices, since
        //            this data is already available from the calling method 'RebuildNormals'.
        // 1/20/2009 - Updated to remove the Ops Vector3 overloads, which are slow on XBOX!
        /*/// <summary>
        /// This sets up the vertices for all of the triangles.
        /// </summary>        
        private static void SetupTerrainNormals()
        {
            Vector3 mPosition = Vector3.Zero;
            Vector3 firstvec, secondvec, tmpNormal1, tmpNormal2, tmpPos1, tmpPos2;

            // 5/14/2009: Tested using the 'vertexBufferData', but then Position updating screws things up!
            //            Therefore, this tmpTerrainVertices is required in order to calculate the Normals correctly!!
            VertexMultitextured_Stream1[] tmpTerrainVertices = new VertexMultitextured_Stream1[mapWidth * mapHeight];

            
            { // Section 1
                // Determine vertex positions so we can figure out normals in section below.
                int arrayLocation;
                for (int loopX = 0; loopX < mapWidth; loopX++)
                    for (int loopY = 0; loopY < mapHeight; loopY++)
                    {
                        mPosition.X = loopX * Scale;
                        mPosition.Y = HeightData[loopX + loopY * mapWidth];
                        mPosition.Z = loopY * Scale;

                        // 4/9/2009 - Doing calc 1st, actually reduces CPI in VTUNE!?
                        arrayLocation = loopX + loopY * mapWidth;
                        tmpTerrainVertices[arrayLocation].Position = mPosition;
                    }
            } // End Section 1

            { // Section 2

                // Setup normals for lighting and physics (Credit: Riemer's method)
                int arrayLocation, arrayLocation2;
                for (int loopX = 1; loopX < mapWidth - 1; loopX++)
                    for (int loopY = 1; loopY < mapHeight - 1; loopY++)
                    {
                        arrayLocation = loopX + loopY * mapWidth;

                        // 1/20/2009 - Updated to remove the Ops Vector3 overloads, which are slow on XBOX!
                        arrayLocation2 = (loopX + 1) + loopY * mapWidth;
                        tmpPos1 = tmpTerrainVertices[arrayLocation2].Position;
                        tmpPos2 = tmpTerrainVertices[arrayLocation].Position;
                        //firstvec = tmpTerrainVertices[loopX + 1 + loopY * mapWidth].Position - tmpTerrainVertices[loopX + loopY * mapWidth].Position;
                        Vector3.Subtract(ref tmpPos1, ref tmpPos2, out firstvec);

                        tmpPos1 = tmpTerrainVertices[arrayLocation].Position;
                        arrayLocation2 = loopX + ((loopY + 1) * mapWidth);
                        tmpPos2 = tmpTerrainVertices[arrayLocation2].Position;
                        //secondvec = tmpTerrainVertices[loopX + loopY * mapWidth].Position - tmpTerrainVertices[loopX + ((loopY + 1) * mapWidth)].Position;
                        Vector3.Subtract(ref tmpPos1, ref tmpPos2, out secondvec);

                        //normal = Vector3.Cross(firstvec, secondvec);
                        Vector3.Cross(ref firstvec, ref secondvec, out tmpNormal1);
                        tmpNormal1.Normalize();

                        tmpNormal2 = tmpTerrainVertices[arrayLocation].Normal;
                        //tmpTerrainVertices[loopX + loopY * mapWidth].Normal += tmpNormal1;
                        Vector3.Add(ref tmpNormal2, ref tmpNormal1, out tmpNormal2);
                        tmpTerrainVertices[arrayLocation].Normal = tmpNormal2;

                        arrayLocation2 = (loopX + 1) + loopY * mapWidth;
                        tmpNormal2 = tmpTerrainVertices[arrayLocation2].Normal;
                        //tmpTerrainVertices[loopX + 1 + loopY * mapWidth].Normal += tmpNormal1;
                        Vector3.Add(ref tmpNormal2, ref tmpNormal1, out tmpNormal2);                       
                        tmpTerrainVertices[arrayLocation2].Normal = tmpNormal2;

                        arrayLocation2 = loopX + ((loopY + 1) * mapWidth);
                        tmpNormal2 = tmpTerrainVertices[arrayLocation2].Normal;
                        //tmpTerrainVertices[loopX + ((loopY + 1) * mapWidth)].Normal += tmpNormal1;
                        Vector3.Add(ref tmpNormal2, ref tmpNormal1, out tmpNormal2);                       
                        tmpTerrainVertices[arrayLocation2].Normal = tmpNormal2;

                    }
            } // End Section 2

            // A 2nd pass of Normalize required.
            for (int loop1 = 0; loop1 < tmpTerrainVertices.Length; loop1++)
                tmpTerrainVertices[loop1].Normal.Normalize();

            
            { // Section 3
                // Store into TerrainNormals Array
                int arrayLocation;
                for (int loopX = 1; loopX < mapWidth - 1; loopX++)
                    for (int loopY = 1; loopY < mapHeight - 1; loopY++)
                    {
                        arrayLocation = loopX + loopY * mapHeight;
                        terrainNormals[arrayLocation] = tmpTerrainVertices[arrayLocation].Normal;    // Stored for use in physics and for the
                        // quad-tree component to reference.
                    }
            } // End Section 3

            
        }*/

        #endregion

        // 8/22/2008 - Updated for performance

        ///<summary>
        /// Returns the <see cref="TWEngine.Terrain"/> height at the given coordinates.
        ///</summary>
        ///<param name="xPos">X value</param>
        ///<param name="yPos">Y value</param>
        ///<returns>Height value</returns>
        public static float GetTerrainHeight(ref int xPos, ref int yPos)
        {
            // 12/12/2009 - Check if MapWidth/MapHeight are 0.
            if (MapHeight == 0 || MapWidth == 0)
                return 0;

            try
            {
                // we first get the height of 4 points of the quad underneath the point
                // Check to make sure this point is not off the map at all
                var xDivS = (int)MathHelper.Clamp(xPos * _scaleOverOne, 0, MapWidth - 2); // 11/5/2009 - Clamp given values to map range!
                // was (Xpos / scale); changed to 1/scale, which is faster Float op!
                var yDivS = (int)MathHelper.Clamp(yPos * _scaleOverOne, 0, MapHeight - 2); // 11/5/2009 - Clamp given values to map range!
                // was (Ypos / scale); changed to 1/scale, which is faster Float op!

                if (xDivS < 0 || xDivS > MapWidth - 2)
                {
                    Debug.WriteLine("(GetTerrainHeight) had X value out of range of map 0-512");
                    return 0;
                    //throw new ArgumentOutOfRangeException("xPos", "X value out of range of map 0-512");
                }

                if (yDivS < 0 || yDivS > MapHeight - 2)
                {
                    Debug.WriteLine("(GetTerrainHeight) had Y value out of range of map 0-512");
                    return 0;
                    //throw new ArgumentOutOfRangeException("yPos", "Y value out of range of map 0-512");

                }

                // 4/22/2009 - Cache Index values to improve CPI in Vtune!
                var tlIndex = xDivS + yDivS*MapHeight;
                var trIndex = (xDivS + 1) + yDivS*MapHeight;
                var blIndex = xDivS + (yDivS + 1)*MapHeight;
                var brIndex = (xDivS + 1) + (yDivS + 1)*MapHeight;

                // 6/27/2012 - Check if HeightData is Null.
                if (HeightData == null)
                {
                    return 0;
                }

                var triY0 = (HeightData[tlIndex]); // TL
                var triY1 = (HeightData[trIndex]); // TR
                var triY2 = (HeightData[blIndex]); // BL
                var triY3 = (HeightData[brIndex]); // BR

                float sqX = (xPos/cScale) - xDivS;
                float sqY = (yPos/cScale) - yDivS;
                float height;
                if ((sqX + sqY) < 1)
                {
                    height = triY0;
                    height += (triY1 - triY0)*sqX;
                    height += (triY2 - triY0)*sqY;
                }
                else
                {
                    height = triY3;
                    height += (triY1 - triY3)*(1.0f - sqY);
                    height += (triY2 - triY3)*(1.0f - sqX);
                }

                return height;
            }
            catch
            {
                Debug.WriteLine("Method Error: GetTerrainHeight.");
                return 0;
            }
        }

        ///<summary>
        /// Returns the <see cref="TWEngine.Terrain"/> height at the given coordinates.
        ///</summary>
        ///<param name="xPos">X value</param>
        ///<param name="yPos">Y value</param>
        ///<returns>Height value</returns>
        public static float GetTerrainHeight(float xPos, float yPos)
        {
            // 12/12/2009 - Check if MapWidth/MapHeight are 0.
            if (MapHeight == 0 || MapWidth == 0)
                return 0;
          
            try
            {
                // we first get the height of 4 points of the quad underneath the point
                // Check to make sure this point is not off the map at all
                var xDivS = (int)MathHelper.Clamp(xPos * _scaleOverOne, 0, MapWidth - 2); // 11/5/2009 - Clamp given values to map range!
                    // was (Xpos / scale); changed to 1/scale, which is faster Float op!
                var yDivS = (int)MathHelper.Clamp(yPos * _scaleOverOne, 0, MapHeight - 2); // 11/5/2009 - Clamp given values to map range!
                    // was (Ypos / scale); changed to 1/scale, which is faster Float op!

                // 8/7/2009 - Cache to local var.
                var mapHeight = MapHeight;

                if (xDivS < 0.0f || xDivS > MapWidth - 2)
                {
                    Debug.WriteLine("(GetTerrainHeight) had X value out of range of map 0-512");
                    return 0;
                    //throw new ArgumentOutOfRangeException("xPos", "X value out of range of map 0-512");
                }

                if (yDivS < 0.0f || yDivS > mapHeight - 2)
                {
                    Debug.WriteLine("(GetTerrainHeight) had Y value out of range of map 0-512");
                    return 0;
                    //throw new ArgumentOutOfRangeException("yPos", "Y value out of range of map 0-512");
                }

                // 4/22/2009 - Cache Index values to improve CPI in Vtune!
                var tlIndex = xDivS + yDivS*mapHeight;
                var trIndex = (xDivS + 1) + yDivS*mapHeight;
                var blIndex = xDivS + (yDivS + 1)*mapHeight;
                var brIndex = (xDivS + 1) + (yDivS + 1)*mapHeight;

                // 6/27/2012 - Check if HeightData is Null.
                if (HeightData == null)
                {
                    return 0;
                }

                var triY0 = (HeightData[tlIndex]); // TL
                var triY1 = (HeightData[trIndex]); // TR
                var triY2 = (HeightData[blIndex]); // BL
                var triY3 = (HeightData[brIndex]); // BR

                var sqX = (xPos*_scaleOverOne) - xDivS;
                // was (Xpos / scale); changed to 1/scale, which is faster Float op!
                var sqY = (yPos*_scaleOverOne) - yDivS;
                // was (Ypos / scale); changed to 1/scale, which is faster Float op!
                float height;
                if ((sqX + sqY) < 1)
                {
                    height = triY0;
                    height += (triY1 - triY0)*sqX;
                    height += (triY2 - triY0)*sqY;
                }
                else
                {
                    height = triY3;
                    height += (triY1 - triY3)*(1.0f - sqY);
                    height += (triY2 - triY3)*(1.0f - sqX);
                }
                return height;
            }
            catch
            {
                Debug.WriteLine("Method Error: GetTerrainHeight.");
                return 0;
            }
        }

        /// <summary>
        /// Checks if given X/Y cordinates are on the heightmap.
        /// </summary>
        /// <param name="xPos">X value</param>
        /// <param name="yPos">Y value</param>
        /// <returns>True/False as result</returns>
        public static bool IsOnHeightmap(float xPos, float yPos)
        {
            // Keep object from going off the edge of the map
            var xPosScaled = xPos*_scaleOverOne; // 5/12/2009
            if (xPosScaled > MapWidth)
                return false;

            if (xPosScaled < 0)
                return false;

            // Keep object from going off the edge of the map
            var yPosScaled = yPos*_scaleOverOne; // 5/12/2009
            if (yPosScaled > MapHeight)
                return false;

            return yPosScaled >= 0;
        }

        // 12/31/2009 - Non-Static version for Interface reference
        /// <summary>
        /// Checks if given X/Y cordinates are on the heightmap.
        /// </summary>
        /// <param name="xPos">X value</param>
        /// <param name="yPos">Y value</param>
        /// <returns>True/False as result</returns>
        bool IFOWTerrainData.IsOnHeightmap(float xPos, float yPos)
        {
            return IsOnHeightmap(xPos, yPos);
        }

        // 2/12/2009 - Overload 1
        /// <summary>
        /// Checks if given X/Y cordinates are on the heightmap.
        /// </summary>
        /// <param name="inPosition">(ref) Vector3</param>
        /// <returns>True/False of result</returns>
        public static bool IsOnHeightmap(ref Vector3 inPosition)
        {
            // Keep object from going off the edge of the map
            var inPosXScaled = inPosition.X*_scaleOverOne; // 5/12/2009
            if (inPosXScaled > MapWidth)
                return false;

            if (inPosXScaled < 0)
                return false;

            // Keep object from going off the edge of the map
            var inPosYScaled = inPosition.Z*_scaleOverOne; // 5/12/2009
            if (inPosYScaled > MapHeight)
                return false;

            return inPosYScaled >= 0;
        }

        /// <summary>
        /// Gets the normal of a Position on the heightmap.
        /// </summary>
        /// <param name="xPos">X Position on the map</param>
        /// <param name="yPos">Y Position on the map</param>
        /// <param name="avgNormal"></param>
        /// <returns>Normal vector of this spot on the terrain</returns>        
        public static void GetNormal(float xPos, float yPos, out Vector3 avgNormal)
        {
            // 5/18/2010 - Cache values
            var xValue = xPos;
            var yValue = yPos;

            // 5/18/2010 - Refactored out core code for reusability
            GetAvgNormal(xValue, yValue, out avgNormal);
        }

        // 5/18/2010: New overload#2 version
        /// <summary>
        /// Gets the normal of a Position on the heightmap.
        /// </summary>
        /// <param name="position"><see cref="Vector3"/> position</param>
        /// <param name="avgNormal"></param>
        /// <returns>Normal vector of this spot on the terrain</returns>        
        public static void GetNormal(ref Vector3 position, out Vector3 avgNormal)
        {
            // 5/18/2010 - Cache values
            var xValue = position.X;
            var yValue = position.Z;

            // 5/18/2010 - Refactored out core code for reusability
            GetAvgNormal(xValue, yValue, out avgNormal);
        }

        // 5/18/2010
        /// <summary>
        /// Method helper, which gets the average normal for the given coordinates.
        /// </summary>
        /// <param name="xValue">X value</param>
        /// <param name="yValue">Y value</param>
        /// <param name="avgNormal">(OUT) average normal vector</param>
        private static void GetAvgNormal(float xValue, float yValue, out Vector3 avgNormal)
        {
            // 8/12/2009 - Cache array
            var terrainNormals = TerrainNormals;
            
            var xDivS = (int) (xValue*_scaleOverOne);
            if (xDivS > MapWidth - 2)
                xDivS = MapWidth - 2;
                // if it is outside the heightmap.
            else if (xDivS < 0)
                xDivS = 0;
            
            var yDivS = (int) (yValue*_scaleOverOne);
            if (yDivS > MapHeight - 2)
                yDivS = MapHeight - 2;
            else if (yDivS < 0)
                yDivS = 0;
            
            var triY0Vec = (terrainNormals[xDivS + yDivS*MapHeight]);
            var triY1Vec = (terrainNormals[(xDivS + 1) + yDivS*MapHeight]);
            var triY2Vec = (terrainNormals[xDivS + ((yDivS + 1)*MapHeight)]);
            var triY3Vec = (terrainNormals[(xDivS + 1) + ((yDivS + 1)*MapHeight)]);

            var sqX = (xValue*_scaleOverOne) - xDivS;
            var sqY = (yValue*_scaleOverOne) - yDivS;
            Vector3 tmpOp1;
            Vector3 tmpOp2;
            if ((sqX + sqY) < 1)
            {
                avgNormal = triY0Vec;

                // 4/10/2009 - Updated to use Vector Ref Overloads, to Optimize on XBOX!
                //avgNormal += (TriY1Vec - TriY0Vec) * SqX;
                Vector3.Subtract(ref triY1Vec, ref triY0Vec, out tmpOp1);
                Vector3.Multiply(ref tmpOp1, sqX, out tmpOp2);
                Vector3.Add(ref avgNormal, ref tmpOp2, out avgNormal);

                // 4/10/2009 - Updated to use Vector Ref Overloads, to Optimize on XBOX!
                //avgNormal += (TriY2Vec - TriY0Vec) * SqY;
                Vector3.Subtract(ref triY2Vec, ref triY0Vec, out tmpOp1);
                Vector3.Multiply(ref tmpOp1, sqY, out tmpOp2);
                Vector3.Add(ref avgNormal, ref tmpOp2, out avgNormal);
            }
            else
            {
                avgNormal = triY3Vec;

                // 4/10/2009 - Updated to use Vector Ref Overloads, to Optimize on XBOX!
                //avgNormal += (TriY1Vec - TriY3Vec) * (1.0f - SqY);
                Vector3.Subtract(ref triY1Vec, ref triY3Vec, out tmpOp1);
                Vector3.Multiply(ref tmpOp1, (1.0f - sqY), out tmpOp2);
                Vector3.Add(ref avgNormal, ref tmpOp2, out avgNormal);

                // 4/10/2009 - Updated to use Vector Ref Overloads, to Optimize on XBOX!
                //avgNormal += (TriY2Vec - TriY3Vec) * (1.0f - SqX);
                Vector3.Subtract(ref triY2Vec, ref triY3Vec, out tmpOp1);
                Vector3.Multiply(ref tmpOp1, (1.0f - sqX), out tmpOp2);
                Vector3.Add(ref avgNormal, ref tmpOp2, out avgNormal);
            }

            // 7/10/2009 - Make sure avgNormal is not an 'NaN' or Zero Vector; otherwise
            //             ground items, like the 'Tanks' will Vanish!  This is because the
            //             Zero vector causes the orientation matrix to scale to zero!
            {
                if (Single.IsNaN(avgNormal.X))
                    avgNormal.X = 0;

                if (Single.IsNaN(avgNormal.Y))
                    avgNormal.Y = 1;

                if (Single.IsNaN(avgNormal.Z))
                    avgNormal.Z = 0;

                // Check for Vector Zero.
                if (avgNormal == Vector3Zero)
                    avgNormal = Vector3.Up;
            }
           
        }

        /// <summary>
        /// Loads in the <see cref="HeightData"/> using a height map image.
        /// </summary>
        /// <param name="contentManager"><see cref="ContentManager"/> instance</param>
        /// <param name="mapName">Map name</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public static void LoadHeightData(ContentManager contentManager, string mapName, string mapType)
        {
            LoadingScreen.LoadingMessage = "Loading Height Data";

            // 10/30/2008 - Load HeightData from new .xnb file; this load method now only took 4 seconds, compared to 15 before on XBOX!           
            var heightData = contentManager.Load<TerrainHeightData>(String.Format(@"{0}\{1}\tdHeightData", mapType, mapName));
            HeightData = heightData.HeightData.ToArray();
            TerrainNormals = new List<Vector3>(heightData.NormalData.Length); // 7/30/2009
            TerrainNormals.AddRange(heightData.NormalData);
                
            // 1/8/2010 - Call Dispose on the temp heightData var
            heightData.Dispose();

            // Set Scale over 1           
            _scaleOverOne = 1.0f/cScale; // 4/13/2009                    

            // If elevation strength was never initialized, use 6 by default.
            if (ElevationStrength < 0.0f)
                ElevationStrength = DefTerrainElevStr;

            // Store Map's MaxHeight value for use in the AlphaMap Creation
            // It turns out since we always divide the height by its internal Max value,
            // then only the ElevationStrength actual determines the height!
            HeightMapMaxHeight = ElevationStrength*cScale;

            // Init Common arrays
            CommonInitilization(heightData.MapWidth, heightData.MapHeight);

            // 6/23/2009 - Set 'PhysX' HeightField; ONLY for PC.
            /*#if !XBOX360
                        PhysX.PhysXHeightField.CreateHeightField(HeightData.HeightData, mapWidth, mapHeight, Scale, LOD.Low);
            #endif*/
        }

        // 6/30/2009
        private static void InitAStarPathNodeSize()
        {
            // 5/20/2008 - Update the PathFinding Size using the larger of MapWidth or MapHeight Squared.
            // 1/14/2009: Updated the Calc below to now Round the Result up!  For example, if map is 512x512, with 90 Pathfinding stride,
            //            then (512 * 10) / 90 = 56.89; the result should then be rounded up to 57, otherwise, if 56, we loose a row!
            float calcSize = (Math.Max(MapWidth, MapHeight)*cScale);
            calcSize /= TemporalWars3DEngine._pathNodeStride;
            TemporalWars3DEngine.SPathNodeSize = (int) Math.Round(calcSize);

            // 6/16/2009 - Init AStarManager 'Neighbors' lists; done here, since PathNodeSize needs to be set.
            var aStarManager = (IAStarManager)TemporalWars3DEngine.GameInstance.Services.GetService(typeof(IAStarManager)); // 1/13/2010
            if (aStarManager != null) aStarManager.InitAStarEngines(TemporalWars3DEngine.SPathNodeSize);
        }

        #region Random Height Map Methods

        // 8/7/2008
        // Ben: This algorithm is taken directly from the book "Programming an RTS Game with Direct3D",
        //      Chapter 4.
        /// <summary>
        /// Creates a Perlin Noise map using the Perlin Noise algorithm.  The values returned can by used for
        /// HeightMaps, or Texture Splatting for example.
        /// </summary>
        /// <param name="seed">Random Seed Value</param>
        /// <param name="noiseSize">The Perlin Noise Size</param>
        /// <param name="persistence">Amplitude control of Perlin Noise; values less than 1.0f produce spiky curves.</param>
        /// <param name="octaves">Higher Octave values produce more gradient Perlin Noise Maps</param>
        public static List<float> CreatePerlinNoiseMap(int seed, float noiseSize, float persistence, int octaves)
        {
            // Holds Random HeightData
            var tmpHeightData = new float[MapWidth * MapHeight];

            // For each map node
            for (var y = 0; y < MapHeight; y++)
                for (var x = 0; x < MapWidth; x++)
                {
                    // Scale x & y to the range of [0.0, noiseSize]
                    var xf = (x/(float) MapWidth)*noiseSize;
                    var yf = (y/(float) MapHeight)*noiseSize;
                    var total = 0.0f;

                    // For each octave
                    for (var i = 0; i < octaves; i++)
                    {
                        // Calculate frequency and amplitude
                        //(different for each octave)
                        var freq = (float) Math.Pow(2, i);
                        var amp = (float) Math.Pow(persistence, i);

                        // Calculate the x,y noise coordinates
                        var tx = xf*freq;
                        var ty = yf*freq;
                        var txInt = (int) tx;
                        var tyInt = (int) ty;

                        // Calculate the fractions of x & y
                        var fracX = tx - txInt;
                        var fracy = ty - tyInt;

                        // Get noise per octave for these 4 points
                        var v1 = Noise(txInt + tyInt*57 + seed);
                        var v2 = Noise(txInt + 1 + tyInt*57 + seed);
                        var v3 = Noise(txInt + (tyInt + 1)*57 + seed);
                        var v4 = Noise(txInt + 1 + (tyInt + 1)*57 + seed);

                        // Smooth noise in the X-axis
                        var i1 = CosInterpolate(v1, v2, fracX);
                        var i2 = CosInterpolate(v3, v4, fracX);

                        // Smooth in the Y-axis
                        total += CosInterpolate(i1, i2, fracy)*amp;
                    }

                    // Calculate Height Final Value
                    var heightValue = total*255;
                    
                    // Save to HeightData Array
                    tmpHeightData[x + y*MapHeight] = heightValue;
                }


            // Iterate through HeightData Array and Clamp between 0 and 255.
            for (var x = 0; x < MapWidth; x++)
                for (var y = 0; y < MapHeight; y++)
                {
                    tmpHeightData[x + y*MapHeight] = MathHelper.Clamp(tmpHeightData[x + y*MapHeight], 0, 255);
                }

            // Copy Values into Color Array to finally save into Texture2D Format.
            var heightDataList = new List<float>();
            heightDataList.AddRange(tmpHeightData);

            return heightDataList;
        }

#if !XBOX360
        // 11/20/2009
        /// <summary>
        /// Given some Perlin Noise data, this method will return 
        /// a <see cref="Bitmap"/> of the perlin noise.
        /// </summary>
        /// <param name="noiseData">Collection of perlin noise data</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="noiseData"/> is null.</exception>
        /// <returns><see cref="Bitmap"/> of perlin noise</returns>
        public static Bitmap CreateBitmapFromPerlinNoise(List<float> noiseData)
        {
            if (noiseData == null)
                throw new ArgumentNullException("noiseData", @"NoiseData array given can not be NULL!");

            const int textureSize = 256;
            var bitmap = new Bitmap(textureSize, textureSize);

            for (float x = 0; x < textureSize; x++)
                for (float y = 0; y < textureSize; y++)
                {
                    // Get relative value from noise array
                    var relX = (x / textureSize) * MapWidth;
                    var relY = (y / textureSize) * MapHeight;
                    var index = (int)(relX + relY * MapHeight);

                    MathHelper.Clamp(noiseData[index], 0, 255);

                    var bitmapColor = Color.FromArgb(255, (int)noiseData[index], 0, 0);
                    bitmap.SetPixel((int)x, (int)y, bitmapColor);
                }

            return bitmap;
        }
#endif

        // 8/7/2008
        /// <summary>
        /// Combines two HeightMaps into one.
        /// </summary>
        /// <param name="hData1">HeightMap 1</param>
        /// <param name="hData2">HeightMap 2</param>
        /// <returns>Combined HeightMaps</returns>
        public static float[] CombineHeightMaps(float[] hData1, float[] hData2)
        {
            var tmpHeightData = new float[MapWidth*MapHeight];
            const float maxHeight = 255;

            for (var x = 0; x < MapWidth; x++)
                for (var y = 0; y < MapHeight; y++)
                {
                    // Scale heightmaps to the range of [0.0, 1.0]
                    var a = hData1[x + y*MapHeight]/maxHeight;
                    var b = hData2[x + y*MapHeight]/maxHeight;

                    // Multiply heightmaps and scale to [0.0, maxHeight]
                    tmpHeightData[x + y*MapHeight] = a*b*maxHeight;
                }

            return tmpHeightData;
        }

        // 8/10/2009
        /// <summary>
        /// Combines two HeightMaps into one.
        /// </summary>
        /// <param name="hData1">HeightMap 1</param>
        /// <param name="hData2">HeightMap 2</param>
        /// <returns>Combined HeightMaps</returns>
        public static List<float> CombineHeightMaps(List<float> hData1, List<float> hData2)
        {
            var tmpHeightData = new float[MapWidth * MapHeight];
            const float maxHeight = 255;

            for (var x = 0; x < MapWidth; x++)
                for (var y = 0; y < MapHeight; y++)
                {
                    // Scale heightmaps to the range of [0.0, 1.0]
                    var a = hData1[x + y * MapHeight] / maxHeight;
                    var b = hData2[x + y * MapHeight] / maxHeight;

                    // Multiply heightmaps and scale to [0.0, maxHeight]
                    tmpHeightData[x + y * MapHeight] = a * b * maxHeight;
                }

            // 8/12/2009 - Return a List
            var heightDataList = new List<float>();
            heightDataList.AddRange(tmpHeightData);

            return heightDataList;
        }

        // 8/7/2008
        // Ben: This algorithm is taken directly from the book "Programming an RTS Game with Direct3D",
        //      Chapter 4.
        /// <summary>
        /// Interpolates between two random values; Helper Function for CreateRandomHeightMap Method.
        /// </summary>
        /// <param name="v1">Random Value 1</param>
        /// <param name="v2">Random Value 2</param>
        /// <param name="fracX">Cosine Angle</param>
        /// <returns>Interpolated Float Value</returns>
        private static float CosInterpolate(float v1, float v2, float fracX)
        {
            var angle = fracX*MathHelper.Pi;
            var prc = (float) ((1.0f - Math.Cos(angle))*0.5f);
            return v1*(1.0f - prc) + v2*prc;
        }

        // 8/7/2008
        // Ben: This algorithm is taken directly from the book "Programming an RTS Game with Direct3D",
        //      Chapter 4.
        /// <summary>
        /// Noise Random Generator Helper Function for CreateRandomHeightMap Method.
        /// </summary>
        /// <param name="x">Seed value</param>
        /// <returns>Same random number for a specific seed as Float</returns>
        private static float Noise(int x)
        {
            x = (x << 13) ^ x;
            return (1.0f - ((x*(x*x*15731 + 789221) + 1376312589) & 0x7fffffff)/1073741824.0f);
        }

        #endregion

        // 6/30/2009
        ///<summary>
        /// Creates and populates the <see cref="HeightData"/> collection.
        ///</summary>
        ///<param name="mapWidth">Map width</param>
        ///<param name="mapHeight">Map height</param>
        public static void CreateNewHeightData(int mapWidth, int mapHeight)
        {
            // 1st - Populate HeightData table
            HeightData = new float[mapWidth*mapHeight];

            // 2nd - Init Common arrays
            CommonInitilization(mapWidth, mapHeight);
        }

        // 4/24/2008 - 
        ///<summary>
        /// Return the Given Quad's <see cref="TessellateLevel"/>.
        ///</summary>
        ///<param name="currentQuadKey">Quad key</param>
        ///<returns><see cref="TessellateLevel"/> Enum or null</returns>
        public static TessellateLevel? GetQuadLOD(int currentQuadKey)
        {
            TessellateLevel? quadLOD;
            TerrainQuadTree quad;
            if (QuadList.TryGetValue(currentQuadKey, out quad))
                quadLOD = quad.LODLevel;
            else
                quadLOD = null;

            return quadLOD;
        }

        // 4/9/2008        
        /// <summary>
        /// Return the Adjacent Quad using the given <paramref name="currentQuadKey"/>.
        /// </summary>
        /// <param name="currentQuadKey">Quad key lookup value</param>
        /// <param name="quadAdjacent"><see cref="QuadAdjacent"/> Enum to return</param>
        /// <param name="quadSection"><see cref="QuadSection"/> Enum to return</param>
        /// <returns>(int?) The adjacent Quad's instance key</returns>
        public static int? GetAdjacentQuadInstanceKey(int currentQuadKey, QuadAdjacent quadAdjacent,
                                                      QuadSection quadSection)
        {
            int foundX;
            int foundY;
            var foundIt = GetQuadKeyArrayPositionValues(currentQuadKey, out foundX, out foundY);

            // Return the Adjacent Quad Key
            int? quadKey = null;
            if (foundIt)
            {
                switch (quadAdjacent)
                {
                    case QuadAdjacent.Top:
                        if (foundY - 1 > 0)
                            quadKey = QuadLocationArray[foundX, foundY - 1];
                        break;
                    case QuadAdjacent.Bottom:
                        if (foundY + 1 < (MapWidth/QuadTreeWidth))
                            quadKey = QuadLocationArray[foundX, foundY + 1];
                        break;
                    case QuadAdjacent.Left:
                        if (foundX - 1 > 0)
                            quadKey = QuadLocationArray[foundX - 1, foundY];
                        break;
                    case QuadAdjacent.Right:
                        if (foundX + 1 < (MapWidth/QuadTreeWidth))
                            quadKey = QuadLocationArray[foundX + 1, foundY];
                        break;
                    default:
                        break;
                } // End Switch

                // Only continue if QuadKey was found.
                if (quadKey != null)
                {
                    // Check if Result is Leaf; if not, then we know this has 
                    // been tessellated into 4 smaller Quads.
                    TerrainQuadTree quad;
                    if (QuadList.TryGetValue((int) quadKey, out quad))
                        if (!quad.Leaf)
                        {
                            // Since not a leaf, lookup in quadLocationArrayL2
                            // This is a Child Quad
                            quadKey = GetChildQuadKey(quadAdjacent, quadSection, (int) quadKey);
                        } // End If Leaf               
                } // End If QuadKey == null  
            } // End FoundIt
                // This is a Child Quad
            else
            {
                quadKey = GetChildQuadKey(quadAdjacent, quadSection, currentQuadKey);
            }

            return quadKey;
        }

        // 4/25/2008 - Get the Child Quad Key using the QuadLocationArrayL2.
        private static int? GetChildQuadKey(QuadAdjacent quadAdjacent, QuadSection quadSection, int currentQuadKey)
        {
            // Let's make sure we are working with a ParentQuadKey!
            int parentKey;
            int? childQuadKey;
            if (QuadChildToParent.TryGetValue(currentQuadKey, out parentKey))
                currentQuadKey = parentKey;


            //string key = String.Empty;
            float key = -1;
            switch (quadAdjacent)
            {
                case QuadAdjacent.Top:
                    switch (quadSection)
                    {
                        case QuadSection.TopLeft:
                            // Then Return BottomLeft
                            //key = "Quad#" + currentQuadKey.ToString() + "-BL";
                            key = currentQuadKey + 0.3f;
                            break;
                        case QuadSection.TopRight:
                            // Then Return BottomRight
                            //key = "Quad#" + currentQuadKey.ToString() + "-BR";
                            key = currentQuadKey + 0.4f;
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Bottom:
                    switch (quadSection)
                    {
                        case QuadSection.BottomLeft:
                            // Then Return TopLeft
                            //key = "Quad#" + currentQuadKey.ToString() + "-TL";
                            key = currentQuadKey + 0.1f;
                            break;
                        case QuadSection.BottomRight:
                            // Then Return BottomRight
                            //key = "Quad#" + currentQuadKey.ToString() + "-TR";
                            key = currentQuadKey + 0.2f;
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Left:
                    switch (quadSection)
                    {
                        case QuadSection.TopLeft:
                            // Then Return TopRight
                            //key = "Quad#" + currentQuadKey.ToString() + "-TR";
                            key = currentQuadKey + 0.2f;
                            break;
                        case QuadSection.BottomLeft:
                            // Then Return BottomRight
                            //key = "Quad#" + currentQuadKey.ToString() + "-BR";
                            key = currentQuadKey + 0.4f;
                            break;
                        default:
                            break;
                    }
                    break;
                case QuadAdjacent.Right:
                    switch (quadSection)
                    {
                        case QuadSection.TopRight:
                            // Then Return TopLeft
                            //key = "Quad#" + currentQuadKey.ToString() + "-TL";
                            key = currentQuadKey + 0.1f;
                            break;
                        case QuadSection.BottomRight:
                            // Then Return BottomLeft
                            //key = "Quad#" + currentQuadKey.ToString() + "-BL";
                            key = currentQuadKey + 0.3f;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            int quadKey;
            if (QuadLocationArrayL2.TryGetValue(key, out quadKey))
                childQuadKey = quadKey;
            else
                childQuadKey = null;


            return childQuadKey;
        }

        // Locates a Given Quad Key in the QuadLocationArray and returns the Array (x,y) Position values to caller.        
        private static bool GetQuadKeyArrayPositionValues(int currentQuadKey, out int foundX, out int foundY)
        {
            // Let's make sure we are working with a ParentQuadKey!
            int parentKey;
            if (QuadChildToParent.TryGetValue(currentQuadKey, out parentKey))
                currentQuadKey = parentKey;

            // Located given key in array
            foundX = 0;
            foundY = 0;
            var foundIt = false;
            // Note: Note sure if error, but both loops use the same 'MapWidth'?
            var i = (MapWidth/QuadTreeWidth); // 5/18/2010
            for (var loop1 = 0; loop1 < i; loop1++)
            {
                for (var loop2 = 0; loop2 < i; loop2++)
                {
                    if (QuadLocationArray[loop1, loop2] != currentQuadKey) continue;

                    foundX = loop1;
                    foundY = loop2;
                    foundIt = true;
                    break;
                } // End For Inner Loop

                if (foundIt)
                    break;
            } // End For Outer Loop
            return foundIt;
        }

        /*private static int _setupCounter;
        private static VertexMultitextured_Stream1[] vb1Check;
        private static VertexMultitextured_Stream2[] vb2Check;
        private static VertexMultitextured_Stream3[] vb3Check;*/

        // 8/11/2008: Updated the Y-Stride to by 1+ the MapHeight; this is to correct the error of the last Quad's
        //            to the far right and bottom were incorrectly creating triangles which span accross the terrain!  
        //            This was due to the IndexBuffer formula, which multiplies each value by the 'Detail' value.  
        //
        //            For example, if the 'Detail' Value is 16, then when the first Quad is made on the far Left, the
        //            result of (x+y*mapHeight)*Detail for (1+0*512)*16 = 16, when it should have been 15, because 0-based!!!
        //            This makes the first Quad 17 accross, while all others are 16, and thereby, pushes the last triangle 
        //            connection back to the next row, accross the Terrain!
        //
        //            Furthermore, if you try adjust the formula in the IndexBuffer to be (Detail-1), even though it does
        //            correct the spacing problem, a new problem arise; none of the BoundingBoxes match the Quads anymore.
        //
        //            Therefore, the best solution it to simply ADD +1 to the Y-Stride during the creation of the Verticies. - Ben
        /// <summary>
        /// Creates the Verticies for the <see cref="VertexBuffer"/> for the entire <see cref="TWEngine.Terrain"/>.
        /// </summary>               
        public static void SetupTerrainVertexBuffer()
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
            try
            {
                var mPosition = Vector3Zero;
                const int texCellSpacing = 32; // Used LOD 8 * LOD 4 = 32.

                // 8/7/2009
                const int scale = cScale;
                var mapHeight = MapHeight;
                var mapWidthPlus1 = (MapWidth + 1);
                var mapHeightPlus1 = (mapHeight + 1);
                var mapWidthNHeight = (mapWidthPlus1 * mapHeightPlus1);

                VertexBufferDataStream1 = new VertexMultitextured_Stream1[mapWidthNHeight];

                // XNA 4.0 Updates - Streams Obsolete.
                //VertexBufferDataStream2 = new VertexMultitextured_Stream2[mapWidthNHeight];
                //VertexBufferDataStream3 = new VertexMultitextured_Stream3[mapWidthNHeight];

                // Set Position for all Verticies
                for (var loopX = 0; loopX < mapWidthPlus1; loopX++)
                    for (var loopY = 0; loopY < mapHeightPlus1; loopY++)
                    {
                        var vertexBufferIndex = loopX + loopY * mapHeightPlus1;

                        // 8/11/2008
                        if (loopX >= MapWidth || loopY >= mapHeight)
                            mPosition.Y = 0;
                        else
                            mPosition.Y = HeightData[loopX + loopY * mapHeight];


                        mPosition.X = loopX * scale;
                        mPosition.Z = loopY * scale;

                        VertexBufferDataStream1[vertexBufferIndex].Position = mPosition;
                        // 8/11/2008: Added +1 to the Y-Stride.

                        // 4/21/2008
                        //mTexCords.X = loopX; mTexCords.Y = loopY; // 1/20/2009 - Test dividing by width/height
                        var tmpTexCords = new Vector2(loopX, loopY);
                        Vector2 tmpTexCords2;
                        Vector2.Divide(ref tmpTexCords, texCellSpacing, out tmpTexCords2);
                        var mTexCords = new HalfVector2(tmpTexCords);
                        var mTexCords2 = new HalfVector2(tmpTexCords2);
                        
                        VertexBufferDataStream1[vertexBufferIndex].TextureCoordinate1 = mTexCords;
                        // 8/11/2008: Added +1 to the Y-Stride.
                        VertexBufferDataStream1[vertexBufferIndex].TextureCoordinate2 = mTexCords2;
                        // 8/11/2008: Added +1 to the Y-Stride.

                        // 7/8/2009 -(This will be properly calculated in the 'RebuildNormals' method.
                        VertexBufferDataStream1[vertexBufferIndex].Tangent = new HalfVector4(); // 1/29/2010 - Update to Packed format
                        VertexBufferDataStream1[vertexBufferIndex].BiNormal = new HalfVector4(); // 1/29/2010 - Update to Packed format

                        // 8/11/2008
                        if (loopX >= MapWidth || loopY >= mapHeight)
                            VertexBufferDataStream1[vertexBufferIndex].Normal = Vector3.Up;
                        // 8/11/2008: Added +1 to the Y-Stride.
                        else
                        {
                            var terrainNormal = TerrainNormals[loopX + loopY * mapHeight];
                            VertexBufferDataStream1[vertexBufferIndex].Normal = terrainNormal;
                        }
                        // 8/11/2008: Added +1 to the Y-Stride.   
                    }

                // 11/18/09 - DEBUG CHECK
                /*if (_setupCounter == 0)
                {
                    vb1Check = new VertexMultitextured_Stream1[mapWidthNHeight];
                    vb2Check = new VertexMultitextured_Stream2[mapWidthNHeight];
                    vb3Check = new VertexMultitextured_Stream3[mapWidthNHeight];

                    // copy data
                    VertexBufferDataStream1.CopyTo(vb1Check, 0);
                    VertexBufferDataStream2.CopyTo(vb2Check, 0);
                    VertexBufferDataStream3.CopyTo(vb3Check, 0);
                }
                else if (_setupCounter == 1)
                {
                    // verify same as before, otherwise throw error.
                    for (int i = 0; i < mapWidthNHeight; i++)
                    {
                        // check VB1
                        if (vb1Check[i].Position != VertexBufferDataStream1[i].Position)
                            throw new InvalidOperationException("Out of Sync!");
                        if (vb1Check[i].Normal != VertexBufferDataStream1[i].Normal)
                            throw new InvalidOperationException("Out of Sync!");

                        // check VB2
                        if (vb2Check[i].TextureCoordinate1 != VertexBufferDataStream2[i].TextureCoordinate1)
                            throw new InvalidOperationException("Out of Sync!");
                        if (vb2Check[i].TextureCoordinate2 != VertexBufferDataStream2[i].TextureCoordinate2)
                            throw new InvalidOperationException("Out of Sync!");

                        // check VB3
                        if (vb3Check[i].BiNormal != VertexBufferDataStream3[i].BiNormal)
                            throw new InvalidOperationException("Out of Sync!");
                        if (vb3Check[i].Tangent != VertexBufferDataStream3[i].Tangent)
                            throw new InvalidOperationException("Out of Sync!");
                    }
                }
                _setupCounter++;*/



#if EditMode // 8/14/2009 - Set 'BufferUsage', depending on if in 'EditMode'.
                {
                    // XNA 4.0 Updates
                    #region OLDCode
                    /*TerrainVertexBufferStream1 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice,
                                                                  VertexMultitextured_Stream1.SizeInBytes * mapWidthNHeight,
                                                                  BufferUsage.None);

                    // 7/1/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
                    TerrainVertexBufferStream1A = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice,
                                                                  VertexMultitextured_Stream1.SizeInBytes * mapWidthNHeight,
                                                                  BufferUsage.None);

                    // 7/8/2009 - Stream 2
                    TerrainVertexBufferStream2 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice,
                                                                  VertexMultitextured_Stream2.SizeInBytes * mapWidthNHeight,
                                                                  BufferUsage.None);
                    // 7/8/2009 - Stream 3
                    TerrainVertexBufferStream3 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice,
                                                                  VertexMultitextured_Stream3.SizeInBytes * mapWidthNHeight,
                                                                  BufferUsage.None);*/
                    #endregion

                    //TerrainVertexBufferStream1 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                      //                                            mapWidthNHeight, BufferUsage.None);

                    // 7/1/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
                    //TerrainVertexBufferStream1A = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                      //                                            mapWidthNHeight, BufferUsage.None);

                    TerrainVertexBufferStream1 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                                                                  mapWidthNHeight, BufferUsage.None);

                    // 7/1/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
                    TerrainVertexBufferStream1A = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                                                                  mapWidthNHeight, BufferUsage.None);

                }
#else
            {

#region OldCode
                TerrainVertexBufferStream1 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                                                                  mapWidthNHeight, BufferUsage.None);
                
              
#endregion

                TerrainVertexBufferStream1 = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                                                                  mapWidthNHeight, BufferUsage.WriteOnly);

#if EditMode
                // 10/3/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
                TerrainVertexBufferStream1A = new VertexBuffer(TemporalWars3DEngine.GameInstance.GraphicsDevice, typeof(VertexMultitextured_Stream1),
                                                                mapWidthNHeight, BufferUsage.WriteOnly);
#endif


            }
#endif
                // Set VB for Stream1
                TerrainVertexBufferStream1.SetData(VertexBufferDataStream1);
                // 7/1/2010 - During EditMode, double-buffer the VertexBuffer for stream#1 to avoid InvalidOpExp errors.
#if EditMode
                TerrainVertexBufferStream1A.SetData(VertexBufferDataStream1); // Set copy of stream#1.
#endif



            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(SetupTerrainVertexBuffer) threw the InvalidOpExp error.");
            }

           
        }

#if !XBOX360

        /// <summary>
        /// Populates the <see cref="VertexDataLookup"/> collection with <see cref="VertexBuffer"/> data.
        /// </summary>     
        public static void SetupVertexDataAndVertexLookup()
        {
            // 5/14/2009 - Moved here to init, so Cap can be set now.
            var vertexBufferDataStream1 = VertexBufferDataStream1; // /5/18/2010 - Cache
            var length = vertexBufferDataStream1.Length; // 5/18/2010
            VertexDataLookup = new Dictionary<int, int>(length);

            // 4/2/2008 - Add Vertex Data into Dictionary for quick lookup of Position
            //            The idea is to use the Vertex Position's X & Z values as a key
            //            into the Dictionary, and then store the index this was at!
            for (var loop1 = 0; loop1 < length; loop1++)
            {
                var vertexDataKey = (int) vertexBufferDataStream1[loop1].Position.X +
                                    ((int)vertexBufferDataStream1[loop1].Position.Z * MapWidth);

                if (!VertexDataLookup.ContainsKey(vertexDataKey))
                    VertexDataLookup.Add(vertexDataKey, loop1);
            }
        }

#endif

        // 4/4/2008 - Ben - Create function to retrieve any component from my VertexBuffer.
        /// <summary>
        /// This will retrieve any component within the <see cref="VertexBuffer"/> using the given <see cref="PickTriangles"/>
        /// location from the <see cref="TWEngine.Terrain"/>; for example, the Normals or Position components.
        /// </summary>
        /// <param name="pickedTriangle">The current picked location on the <see cref="TWEngine.Terrain"/></param>
        /// <param name="vbComponent"><see cref="VertexBuffer"/> component you want return</param>
        /// <param name="vbData">(OUT) Collection of 3 Objects containing the data for the specified component</param>
        public static void GetVertexBufferComponent(ref PickTriangles pickedTriangle, VertexBufferComponent vbComponent,
                                                    out object[] vbData) 
        {
            var vbComponentData = new object[3];

            switch (vbComponent)
            {
                case VertexBufferComponent.Position:
                    vbComponentData[0] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].Position;
                    vbComponentData[1] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].Position;
                    vbComponentData[2] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].Position;
                    break;
                case VertexBufferComponent.TexCoordinate1:
                    vbComponentData[0] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].TextureCoordinate1;
                    vbComponentData[1] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].TextureCoordinate1;
                    vbComponentData[2] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].TextureCoordinate1;
                    break;
                case VertexBufferComponent.TexCoordinate2:
                    vbComponentData[0] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].TextureCoordinate2;
                    vbComponentData[1] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].TextureCoordinate2;
                    vbComponentData[2] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].TextureCoordinate2;
                    break;
                case VertexBufferComponent.Normal:
                    vbComponentData[0] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].Normal;
                    vbComponentData[1] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].Normal;
                    vbComponentData[2] = VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].Normal;
                    break;
                default:
                    break;
            }


            vbData = vbComponentData;
        }

        // 4/4/2008 - Ben - Create function to set any component from my VertexBuffer.
        /// <summary>
        /// This will set any component within the <see cref="VertexBuffer"/>  using the given <see cref="PickTriangles"/>
        /// location from the <see cref="TWEngine.Terrain"/>; for example, the Normals or Position components.
        /// </summary>
        /// <param name="pickedTriangle">The current picked Location on the <see cref="TWEngine.Terrain"/></param>
        /// <param name="vbComponent"><see cref="VertexBuffer"/> component you want to set</param>
        /// <param name="vbData">(OUT) Collection of 3 Objects containing the data for the specified component</param>
        public static void SetVertexBufferComponent(ref PickTriangles pickedTriangle, VertexBufferComponent vbComponent,
                                                    object[] vbData)
        {
            var newPosition = new Vector3[3];
            var newTexCord1 = new HalfVector2[3];
            var newTexCord2 = new HalfVector2[3];
            var newNormal = new Vector3[3];

            switch (vbComponent)
            {
                case VertexBufferComponent.Position:
                    vbData.CopyTo(newPosition, 0);
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].Position = newPosition[0];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].Position = newPosition[1];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].Position = newPosition[2];
                    break;
                case VertexBufferComponent.TexCoordinate1:
                    vbData.CopyTo(newTexCord1, 0);
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].TextureCoordinate1 = newTexCord1[0];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].TextureCoordinate1 = newTexCord1[1];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].TextureCoordinate1 = newTexCord1[2];
                    break;
                case VertexBufferComponent.TexCoordinate2:
                    vbData.CopyTo(newTexCord2, 0);
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].TextureCoordinate2 = newTexCord2[0];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].TextureCoordinate2 = newTexCord2[1];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].TextureCoordinate2 = newTexCord2[2];
                    break;
                case VertexBufferComponent.Normal:
                    vbData.CopyTo(newNormal, 0);
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[0]].Normal = newNormal[0];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[1]].Normal = newNormal[1];
                    VertexBufferDataStream1[pickedTriangle.VertexArrayValue[2]].Normal = newNormal[2];
                    break;
                default:
                    break;
            }

            TerrainVertexBufferStream1.SetData(VertexBufferDataStream1);
        }

        // 4/3/2008; 7/8/2009: Updated to calculate the Tangent/Binormal values in Stream-3.
        /// <summary>
        /// This rebuilds all normals in the vertexbuffer.
        /// </summary>
        public static void RebuildNormals(ref TerrainQuadTree rootQuadTree)
        {
            // 11/20/2009 - Capture 'TileBracket' error, which is thrown sometime when calling 'SetData'.
            try
            {
                // Recursively Call the Quads to Rebuild Normals using each Quad's IndexBuffer.
                var vertexBufferDataStream1 = VertexBufferDataStream1; // 5/18/2010 - Cache
                TerrainQuadTree.RebuildNormalsUsingQuadIb(ref rootQuadTree, ref vertexBufferDataStream1);

                // A 2nd pass of Normalize required.
                var length = vertexBufferDataStream1.Length; // 5/18/2010 - Cache
                for (var loop1 = 0; loop1 < length; loop1++)
                {
                    // 5/18/2010 - Cache
                    var normal = vertexBufferDataStream1[loop1].Normal; 

                    // 3/23/2010 - To avoid NaN, check if zero before attempting 'Normalize' call.
                    if (!normal.Equals(Vector3Zero)) normal.Normalize();
                }

                // 8/12/2009
                /*var vertexBufferDataStream3 = VertexBufferDataStream3; // /5/18/2010 - Cache
                if (vertexBufferDataStream3 == null)
                {
                    TemporalWars3DEngine.GameInstance.Window.Title =
                        "Must be in EditMode to apply Normal Lighting calculations!";
                    return;
                }*/

                
                // Check for any 'NaN' in the Normals; 'NaN' = Not A Number.
                var tangentVector = Vector3.Right;
                for (var loop1 = 0; loop1 < length; loop1++)
                {
                    // 5/19/2009 - Get values at same Time and cache, to improve CPI in VTUNE.
                    var normal = vertexBufferDataStream1[loop1].Normal; // 8/7/2009; 
                    if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z))
                        normal = Vector3.Up;

                    // 7/8/2009 - Build Tangent data for BumpMapping.
                    Vector3 tangent; //cross(input.Normal, (0,0,1));
                    Vector3 binormal; //cross(Tangent, input.Normal);
                    Vector3.Cross(ref tangentVector, ref normal, out tangent);
                    Vector3.Cross(ref tangent, ref normal, out binormal);
                    if (!binormal.Equals(Vector3Zero)) binormal.Normalize();

                    vertexBufferDataStream1[loop1].Tangent = new HalfVector4(tangent.X, tangent.Y, tangent.Z, 0);  // 1/29/2010 - Update to Packed format
                    vertexBufferDataStream1[loop1].BiNormal = new HalfVector4(binormal.X, binormal.Y, binormal.Z, 0); // 1/29/2010 - Update to Packed format
                }

                // TODO: Why dosen't this work?
                // 1/22/2010 - Rebuild Tangent/BiNormal data
                //TerrainQuadTree.RebuildTangentDataUsingQuadIB(ref rootQuadTree, ref VertexBufferDataStream1, ref VertexBufferDataStream2, ref VertexBufferDataStream3);

#if DEBUG
                // 3/23/2010 - Check if any NaN gots through!?
                for (var loop1 = 0; loop1 < length; loop1++)
                {
                    var normal = vertexBufferDataStream1[loop1].Normal; 
                    if (float.IsNaN(normal.X) || float.IsNaN(normal.Y) || float.IsNaN(normal.Z))
                        throw new InvalidOperationException("NaN detected!  Corrupt data.");
                }

#endif

                // Update Terrain VertexBuffer with Rebuilt Normals.
                TerrainVertexBufferStream1.SetData(vertexBufferDataStream1);

                // 7/8/2009 - Update Terrain Stream-3 VertexBuffer with Tangent data.
                //TerrainVertexBufferStream3.SetData(vertexBufferDataStream3);


#if !EditMode // 8/14/2009 - DO NOT dispose when in EditMode.

                // 8/7/2009 - Dispose of the Stream#3 array.
                vertexBufferDataStream1 = null;
#endif

#if XBOX
                // Always dispose on XBOX, since it never in EditMode.
                //vertexBufferDataStream1 = null;
#endif
            }
            catch (InvalidOperationException)
            {
                Debug.WriteLine("(RebuildNormals) threw the InvalidOpExp error.");
               
            }
            
        }

#if !XBOX360
        // 10/7/2009
        /// <summary>
        /// Saves the internal <see cref="HeightData"/>, as a collection; this should be called
        /// from the 'TerrainStorageRoutine' class.
        /// </summary>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name to save to</param>
        /// <param name="mapType">Map type; either SP or MP.</param>
        public static void SaveHeightMapData(Storage storageTool, string mapName, string mapType)
        {
            var tmpHeightData = new float[MapWidth * MapHeight];
            // 1st - store 'HeightData' into 1st half of array.
            for (var loopX = 0; loopX < MapWidth; loopX++)
                for (var loopY = 0; loopY < MapHeight; loopY++)
                {
                    // 5/18/2010 - Cache calc
                    var index0 = loopX + loopY * MapHeight;
                    tmpHeightData[index0] = HeightData[index0];
                }

            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            // 1/8/2009: Updated to save to the 'ContentMaps' folder.
            int errorCode;
            if (storageTool.StartBitsSaveOperation(tmpHeightData, "tdHeightData.thd",
                                                   String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc,
                                                                 mapType, mapName), out errorCode)) return;
            // 4/9/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'HeightData' (tdHeightData.thd) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'HeightData' (tdHeightData.thd) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            throw new InvalidOperationException("The Save HeightData Operation Failed.");
        }

        // 10/7/2009
        /// <summary>
        /// Save the QuadMetaData, using the given <paramref name="data"/> struct.
        /// </summary>
        /// <param name="data"><see cref="SaveTerrainData"/> struct to save</param>
        /// <param name="storageTool"><see cref="Storage"/> instance</param>
        /// <param name="mapName">Map name to save to</param>
        /// <param name="mapType">Map type is either SP or MP</param>
        public static void SaveQuadMetaData(ref SaveTerrainData data, Storage storageTool, string mapName, string mapType)
        {
            // 4/7/2010: Updated to use 'ContentMapsLoc' global var.
            int errorCode;
            if (storageTool.StartSaveOperation(data, "tdQuadMetaData.sav",
                                               String.Format(@"{0}\{1}\{2}\", TemporalWars3DEngine.ContentMapsLoc,
                                                             mapType, mapName), out errorCode)) return;
            // 4/7/2010 - Error occured, so check which one.
            if (errorCode == 1)
            {
                MessageBox.Show(@"Locked files detected for 'QuadMetaData' (tdQuadMetaData.sav) save.  Unlock files, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (errorCode == 2)
            {
                MessageBox.Show(@"Directory location for 'QuadMetaData' (tdQuadMetaData.sav) save, not found.  Verify directory exist, and try again.",
                                @"Save Operation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            throw new InvalidOperationException("The Save Struct data Operation Failed.");
        }
#endif

        // 10/7/2009
        /// <summary>
        /// Iterate the internal QuadList dictionary, returning
        /// all QuadKeys, which are at <see cref="TessellateLevel.Level3"/>, in a List(int).
        /// </summary>
        /// <returns>Collection of keys</returns>
        public static List<int> GetQuadKeysOfLOD3()
        {
            var tmpQuadLOD3 = new List<int>();
            foreach (var kvp in QuadList)
            {
                var quad = kvp.Value;

                if (quad.LODLevel == TessellateLevel.Level3)
                    tmpQuadLOD3.Add(quad.QuadKeyInstance);
            }
            return tmpQuadLOD3;
        }

       

        // 4/5/2009 - Dispose of resources
        ///<summary>
        /// Disposes of unmanaged resources.
        ///</summary>
        ///<param name="disposing">Is this final dispose?</param>
        public static void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Dispose of Resources
            if (TerrainVertexBufferStream1 != null)
                TerrainVertexBufferStream1.Dispose();
#if EditMode
            if (TerrainVertexBufferStream1A != null)
                TerrainVertexBufferStream1A.Dispose(); // 11/17/09
#endif
            

            // Arrays            
            if (QuadList != null)
                QuadList.Clear();
            if (QuadChildToParent != null)
                QuadChildToParent.Clear();
            if (QuadLocationArrayL2 != null)
                QuadLocationArrayL2.Clear();
            if (TerrainNormals != null)
                TerrainNormals.Clear(); // 11/17/09
            if (QuadLocationArray != null) Array.Clear(QuadLocationArray, 0, QuadLocationArray.Length); // 1/8/2010
            if (VertexBufferDataStream1 != null) Array.Clear(VertexBufferDataStream1, 0, VertexBufferDataStream1.Length); // 1/8/2010
           
            if (HeightData != null) Array.Clear(HeightData, 0, HeightData.Length); // 1/8/2010

#if !XBOX360
                if (VertexDataLookup != null)
                    VertexDataLookup.Clear();
#endif

            // Null Refs
            TerrainVertexBufferStream1 = null;
#if EditMode
            TerrainVertexBufferStream1A = null; // 11/17/09
#endif

            QuadList = null;
            QuadChildToParent = null;
            QuadLocationArrayL2 = null;
#if !XBOX360
                VertexDataLookup = null;
#endif
          

            VertexBufferDataStream1 = null;
           
            HeightData = null;
            TerrainNormals = null;
        }

      
    }
}