#region File Description
//-----------------------------------------------------------------------------
// IFDTileTextureLoader.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ParallelTasksComponent.LocklessQueue;
using TWEngine.InstancedModels.Enums;

namespace TWEngine.IFDTiles
{
    // 2/17/2010
    /// <summary>
    /// The <see cref="IFDTileTextureLoader"/> class loads <see cref="Texture2D"/> into memory via the <see cref="ThreadPool"/>, using
    /// the proper <see cref="ResourceContentManager"/>, depending of Pc or Xbox platform.  The <see cref="Texture2D"/> instances
    /// loaded into memory are used as the background for given <see cref="IFDTile"/> instances.
    /// </summary>
    public class IFDTileTextureLoader
    {
        // 3/26/2011 - XNA 4.0 Updates - Updated to the LocklessQueue.
        private static volatile LocklessQueue<string> _preLoadTileTexturesQueue = new LocklessQueue<string>();

        // 2/17/2010 - Create WaitCallBack delegate.
        private static WaitCallback _waitCallBack1;

        /// <summary>
        /// To save time during game play, some <see cref="IFDTile"/> <see cref="Texture2D"/> are preloaded during game bootup by
        /// calling this method.  This will in turn, call the internal <see cref="ThreadPool"/> to start the process.
        /// </summary>
        public static void PreLoadIFDTileTextures()
        {
            #region OldCode

            // Pre-Load SciFi-Building Tiles
            // Set-1
            /*IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld02_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld03_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld04_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld05_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld06_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld07_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld08_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set1\SciFi_Bld09_Pic");

            // Set-2
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB02_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB03_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB04_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB05_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB06_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB07_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB08_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB09_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB10_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB11_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB12_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB13_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB14_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Buildings\Set2\SciFi_BldB15_Pic");


            // Pre-Load SciFi-Tank Tiles
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank02_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank03_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank04_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank05_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank06_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank07_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank08_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank09_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank10_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Tank11_Pic");


            // Pre-Load Jeep Tiles
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Jeep01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Vehicles\SciFi_Jeep03_Pic");

            // Pre-Load SciFi-Defense Tiles
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Defenses\SciFi_AAGun01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Defenses\SciFi_AAGun02_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Defenses\SciFi_AAGun04_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Defenses\SciFi_AAGun05_Pic");


            // Pre-Load SciFi-Aircraft Tiles
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Heli01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Heli02_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Bomber01_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Bomber06_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Bomber07_Pic");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\Aircrafts\SciFi_Gunship02_Pic");


            // Pre-Load GroupControl Tiles
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\IFDTileGC_Buildings");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\IFDTileGC_People");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\IFDTileGC_Vehicles");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\IFDTileGC_Shields");
            IFDTile.ContentManager.Load<Texture2D>(@"InterfaceTiles\IFDTileGC_Airplanes");*/

            #endregion

            // Pre-Load GroupControl Tiles
            PreLoadIFDTileTexture("IFDTileGC_Buildings");
            PreLoadIFDTileTexture("IFDTileGC_People");
            PreLoadIFDTileTexture("IFDTileGC_Vehicles");
            PreLoadIFDTileTexture("IFDTileGC_Shields");
            PreLoadIFDTileTexture("IFDTileGC_Airplanes");

            // Pre-Load SciFi-Defense Tiles
            PreLoadIFDTileTexture("SciFi_AAGun01_Pic");
            PreLoadIFDTileTexture("SciFi_AAGun02_Pic");
            PreLoadIFDTileTexture("SciFi_AAGun04_Pic");
            PreLoadIFDTileTexture("SciFi_AAGun05_Pic");

            // Load IFDtiles
            LoadIFDTileTexturesInQueue();
            
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> production set, like the 'WarFactory-Tank' set, into memory.
        /// </summary>
        /// <param name="productionType">The <see cref="ItemGroupType"/> Enum production set to load</param>
        public static void PreLoadIFDTileSet(ItemGroupType? productionType)
        {
            if (productionType == null)
                return;

            switch (productionType.Value)
            {
                case ItemGroupType.Buildings:

                    break;
                case ItemGroupType.Shields:
                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:

                    // Flag Marker
                    PreLoadIFDTileTexture("FlagMarker");

                    switch (TemporalWars3DEngine.SThisPlayer)
                    {
                        case 0:
                            PreLoadIFDTileTexture("SciFi_Tank01_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank04_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank06_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank09_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank10_Pic");
                            PreLoadIFDTileTexture("SciFi_Jeep01_Pic");
                            break;
                        case 1:
                            PreLoadIFDTileTexture("SciFi_Tank02_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank03_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank07_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank08_Pic");
                            PreLoadIFDTileTexture("SciFi_Artilery01_Pic");
                            PreLoadIFDTileTexture("SciFi_Jeep03_Pic");
                            break;
                    }

                    break;
                case ItemGroupType.Airplanes:

                    // Flag Marker
                    PreLoadIFDTileTexture("FlagMarker");

                    switch (TemporalWars3DEngine.SThisPlayer)
                    {
                        case 0:
                            PreLoadIFDTileTexture("SciFi_Heli01_Pic");
                            PreLoadIFDTileTexture("SciFi_Bomber06_Pic");
                            PreLoadIFDTileTexture("SciFi_Gunship02_Pic");
                            break;
                        case 1:
                            PreLoadIFDTileTexture(@"SciFi_Heli02_Pic");
                            PreLoadIFDTileTexture(@"SciFi_Bomber01_Pic");
                            PreLoadIFDTileTexture(@"SciFi_Bomber07_Pic");
                            break;
                    }

                    break;
                default:
                    break;
            }

            if (_waitCallBack1 == null)
                _waitCallBack1 = WaitCallBack1;

            // 2/17/2010 - Start in ThreadPool
            ThreadPool.QueueUserWorkItem(_waitCallBack1);
        }

        // 2/6/2011
        /// <summary>
        /// Loads a <see cref="Texture2D"/> production set, like the 'WarFactory-Tank' set, into memory.
        /// </summary>
        /// <param name="productionType">The <see cref="ItemGroupType"/> Enum production set to load</param>
        /// <param name="assetSide">The player's asset side to load; either side 1 or 2.</param>
        public static void PreLoadIFDTileSet(ItemGroupType? productionType, int assetSide)
        {
            if (productionType == null)
                return;

            if (assetSide < 1 || assetSide > 2)
                throw new ArgumentOutOfRangeException("assetSide", @"Value MUST be either 1 or 2."); 

            switch (productionType.Value)
            {
                case ItemGroupType.Buildings:

                    break;
                case ItemGroupType.Shields:
                    break;
                case ItemGroupType.People:
                    break;
                case ItemGroupType.Vehicles:

                    // Flag Marker
                    PreLoadIFDTileTexture("FlagMarker");

                    switch (assetSide)
                    {
                        case 1:
                            PreLoadIFDTileTexture("SciFi_Tank01_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank04_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank06_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank09_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank10_Pic");
                            PreLoadIFDTileTexture("SciFi_Jeep01_Pic");
                            break;
                        case 2:
                            PreLoadIFDTileTexture("SciFi_Tank02_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank03_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank07_Pic");
                            PreLoadIFDTileTexture("SciFi_Tank08_Pic");
                            PreLoadIFDTileTexture("SciFi_Artilery01_Pic");
                            PreLoadIFDTileTexture("SciFi_Jeep03_Pic");
                            break;
                    }

                    break;
                case ItemGroupType.Airplanes:

                    // Flag Marker
                    PreLoadIFDTileTexture("FlagMarker");

                    switch (assetSide)
                    {
                        case 1:
                            PreLoadIFDTileTexture("SciFi_Heli01_Pic");
                            PreLoadIFDTileTexture("SciFi_Bomber06_Pic");
                            PreLoadIFDTileTexture("SciFi_Gunship02_Pic");
                            break;
                        case 2:
                            PreLoadIFDTileTexture(@"SciFi_Heli02_Pic");
                            PreLoadIFDTileTexture(@"SciFi_Bomber01_Pic");
                            PreLoadIFDTileTexture(@"SciFi_Bomber07_Pic");
                            break;
                    }

                    break;
                default:
                    break;
            }

            LoadIFDTileTexturesInQueue();
        }

        /// <summary>
        /// Enqueues <see cref="IFDTile"/> <see cref="Texture2D"/> requests into the internal queue, which is processed
        /// by the <see cref="ThreadPool"/>.
        /// </summary>
        /// <param name="ifdTileToLoad">Name of <see cref="Texture2D"/> to load</param>
        private static void PreLoadIFDTileTexture(string ifdTileToLoad)
        {

#if XBOX360

            if (IFDTileManager.ContentResourceManager == null)
                IFDTileManager.ContentResourceManager = new ResourceContentManager(TemporalWars3DEngine.GameInstance.Services, Resource360.ResourceManager);
#else
            if (IFDTileManager.ContentResourceManager == null)
                IFDTileManager.ContentResourceManager = new ResourceContentManager(TemporalWars3DEngine.GameInstance.Services, Resources.ResourceManager);
#endif

            // Enqueue new request
            _preLoadTileTexturesQueue.Enqueue(ifdTileToLoad);
        }

        // 2/17/2010
        /// <summary>
        /// <see cref="WaitCallback"/> delegate method, used for <see cref="ThreadPool"/>, which in turn calls
        /// the <see cref="LoadIFDTileTexturesInQueue"/> method.
        /// </summary>
        private static void WaitCallBack1(object state)
        {
            // process
            LoadIFDTileTexturesInQueue();
        }

        // 2/17/2010
        /// <summary>
        /// Loads the <see cref="Texture2D"/> names queued up, using the proper
        /// <see cref="ResourceContentManager"/>, depending if Pc or Xbox platforms.
        /// </summary>
        static void LoadIFDTileTexturesInQueue()
        {
            try
            {
                // 3/23/2010 - Add Lock on Queue
                lock (_preLoadTileTexturesQueue)
                {
                    // 5/30/2009 - Check if Queue has an entry
                    var tileName = new StringBuilder(50);
                    while (_preLoadTileTexturesQueue.Count > 0)
                    {
                        tileName.Remove(0, tileName.Length);

                        string ifdTileToLoad;
                        if (_preLoadTileTexturesQueue.TryDequeue(out ifdTileToLoad))
                        {
                            tileName.Append(ifdTileToLoad); // .Dequeue()

                            IFDTileManager.ContentResourceManager.Load<Texture2D>(tileName.ToString()); //8/30/2009
                            Thread.Sleep(10);
#if DEBUG
                            Debug.WriteLine(String.Format("Preload IFDTile Texture Loaded: {0}", tileName));
                            
#endif
                        }

                       

                    } // End While
                } // End Thread Lock
            }
            catch (ContentLoadException)
            {
#if DEBUG
                // Empty
                Debug.WriteLine("Unable to Load Content in PreLoad IFD Tiles Thread");
#endif
            }
            catch (InvalidOperationException)
            {
#if DEBUG
                // Empty
                Debug.WriteLine("Unable to Load Content in PreLoad IFD Tiles Thread");
#endif
            }
            catch(Exception ex) // 3/26/2011
            {
#if DEBUG
                // Empty
                Debug.WriteLine(string.Format("Unable to Load Content in PreLoad IFD Tiles Thread with exception {0}", ex.Message));
#endif
            }
        }

       
    }
}
