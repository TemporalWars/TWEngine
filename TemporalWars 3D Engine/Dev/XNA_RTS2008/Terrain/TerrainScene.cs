#region File Description
//-----------------------------------------------------------------------------
// TerrainScene.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;
using TWEngine.Terrain.Enums;

namespace TWEngine.Terrain
{
    ///<summary>
    /// The <see cref="TerrainScene"/> class.
    ///</summary>
    public class TerrainScene : SceneItem
    {
        /// <summary>
        /// This constructor creates an SceneItemOwner with a particular shape at a particular point
        /// </summary>
        /// <param name="game"></param>
        /// <param name="initialPosition">The World space Position</param>
        /// <param name="terrainIsIn"></param>
        public TerrainScene(Game game, ref Vector3 initialPosition,  TerrainIsIn terrainIsIn)
            : base(game, new TerrainShape(game,  terrainIsIn), initialPosition)
        {
            //IsTerrain = true;
        }
        
    }
}