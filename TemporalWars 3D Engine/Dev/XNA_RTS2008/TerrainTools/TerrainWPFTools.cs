#region File Description
//-----------------------------------------------------------------------------
// TerrainWPFTools.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Xna.Framework;
using TWEngine.Terrain.Enums;

namespace TWEngine.TerrainTools
{
    /// <summary>
    /// Simple delegate used for Dispatcher communication.
    /// </summary>
    public delegate void SimpleDelegate();

    // 7/1/2010
    /// <summary>
    /// The <see cref="TerrainWPFTools"/> class holds all instances of the WPF Forms routine classes.
    /// </summary>
    public class TerrainWPFTools : IDisposable
    {
        // 8/17/2010
        private static MainMenuToolRoutines _mainMenuToolRoutines;

        // 7/1/2010
        private static TerrainHeightToolRoutines _terrainHeightToolRoutines;
        // 7/4/2010
        private static TerrainItemToolRoutines _terrainItemToolRoutines;
        // 7/7/2010
        private static TerrainPaintToolRoutines _terrainPaintToolRoutines;

        // 7/2/2010
        private static Thread _threadInStdMode;
        private static volatile GameTime _gameTime;
        private static volatile bool _stopThread;
        public static Dispatcher CurrentDispatcher;

        #region Properties

        // 1/10/2011
        internal static bool ActivateHeightTool { get; set; }
        // 1/10/2011
        internal static bool ActivatePaintTool { get; set; }
        // 1/10/2011
        internal static bool ActivateItemTool { get; set; }
        // 1/10/2011
        internal static bool ActivatePropertiesTool { get; set; }

        // 3/30/2011
        /// <summary>
        /// Get or set the <see cref="ToolType"/> to start close cycle.
        /// </summary>
        public static ToolType ToolTypeToClose { get; set; }

        // 4/3/2011
        /// <summary>
        /// Gets the instance of <see cref="TerrainHeightToolRoutines"/>.
        /// </summary>
        public static TerrainHeightToolRoutines TerrainHeightToolRoutines
        {
            get { return _terrainHeightToolRoutines; }
        }

        // 4/3/2011
        /// <summary>
        /// Gets the instance of <see cref="TerrainItemToolRoutines"/>.
        /// </summary>
        public static TerrainItemToolRoutines TerrainItemToolRoutines
        {
            get { return _terrainItemToolRoutines; }
        }

        // 4/3/2011
        /// <summary>
        /// Gets the instance of <see cref="TerrainPaintToolRoutines"/>.
        /// </summary>
        public static TerrainPaintToolRoutines TerrainPaintToolRoutines
        {
            get { return _terrainPaintToolRoutines; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public TerrainWPFTools()
        {
            // 1/21/2011 - Check if Thread exist.
            if (_threadInStdMode != null) return;

            // create new thread
            _threadInStdMode = new Thread(ThreadMethod);
            // set 'STA' mode, required for WPF Forms!
            _threadInStdMode.SetApartmentState(ApartmentState.STA);
            _threadInStdMode.IsBackground = true;
            _threadInStdMode.Start();
        }

        // 7/2/2010
        private static void ThreadMethod()
        {
            // 7/2/2010 - Create Dispatcher and start for inter-Thread communication.
            CurrentDispatcher = Dispatcher.CurrentDispatcher;
            Dispatcher.Run();

            try
            {
                while (!_stopThread)
                {
                    //empty
                    Debugger.Break();
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            
        }

        // 7/8/2010
        /// <summary>
        /// Shows the WPF HeightTool window.
        /// </summary>
        public static void HeightToolShow()
        {
            // Show WPF tool
            // 7/2/2010 - Use Dispatcher to communicate call to STA thread.
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, new SimpleDelegate(HeightToolShowDelegate));
        }

        // 7/8/2010
        /// <summary>
        /// Delegate method for dispatcher call.
        /// </summary>
        private static void HeightToolShowDelegate()
        {
            try
            {
                // Instantiate WPF tool
                //if (_terrainHeightToolRoutines == null)
                _terrainHeightToolRoutines = new TerrainHeightToolRoutines();
                
                TerrainHeightToolRoutines.HeightToolWindowI.Show();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(string.Format("HeightToolShowDelegate threw the exception {0}", ex.Message));
            }
            
        }

        // 7/8/2010
        /// <summary>
        /// Shows the WPF ItemTool window.
        /// </summary>
        public static void ItemToolShow()
        {
            // Show WPF tool
            // 7/4/2010 - Use Dispatcher to communicate call to STA thread.
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, new SimpleDelegate(ItemToolShowDelegate));
        }

        // 7/8/2010
        /// <summary>
        /// Delegate method for dispatcher call.
        /// </summary>
        private static void ItemToolShowDelegate()
        {
            try
            {
                // 1/11/2011 - Dispose of old windows instance, and create new.
                // Instantiate WPF tool
                //if (_terrainItemToolRoutines == null)
                _terrainItemToolRoutines = new TerrainItemToolRoutines();

                TerrainItemToolRoutines.ItemToolWindowI.Show();
            }
            catch (Exception ex)
            {
               System.Console.WriteLine(string.Format("ItemToolShowDelegate threw the exception {0}", ex.Message));
            }
           
        }

        // 7/8/2010
        /// <summary>
        /// Shows the WPF PaintTool window.
        /// </summary>
        public static void PaintToolShow()
        {
            try
            {
                // Show WPF tool
                // 7/7/2010 - Use Dispatcher to communicate call to STA thread.
                CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, new SimpleDelegate(PaintToolShowDelegate));
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(string.Format("HeightToolShowDelegate threw the exception {0}", ex.Message));
            }
           

        }

        // 7/8/2010
        /// <summary>
        /// Delegate method for dispatcher call.
        /// </summary>
        private static void PaintToolShowDelegate()
        {
            try
            {
                // Instantiate WPF tool
                //if (_terrainPaintToolRoutines == null)
                _terrainPaintToolRoutines = new TerrainPaintToolRoutines();

                TerrainPaintToolRoutines.PaintToolWindowI.Show();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(string.Format("PaintToolShowDelegate threw the exception {0}", ex.Message));
            }
            
        }

        // 8/17/2010
        /// <summary>
        /// Shows the WPF MainMenu window.
        /// </summary>
        public static void MainMenuToolShow()
        {
            // Show WPF tool
            // 8/17/2010 - Use Dispatcher to communicate call to STA thread.
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, new SimpleDelegate(MainMenuToolShowDelegate));

        }

        // 8/17/2010
        /// <summary>
        /// Delegate method for dispatcher call.
        /// </summary>
        private static void MainMenuToolShowDelegate()
        {
            try
            {
                // Instantiate WPF tool
                //if (_mainMenuToolRoutines == null)
                _mainMenuToolRoutines = new MainMenuToolRoutines();

                MainMenuToolRoutines.MainMenuWindowI.Show();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(string.Format("MainMenuToolShowDelegate threw the exception {0}", ex.Message));
            }
           
        }

        /// <summary>
        /// Updates all internal WPF tool routines.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {
            // tell thread to do updates.
            _gameTime = gameTime;

            // 1/10/2011 - Check for Tool activation, set by MainMenuToolRoutines
            if (ActivateHeightTool)
            {
                ActivateHeightTool = false;
                TerrainEditRoutines.ActivateTool(ToolType.HeightTool);
            }
            if (ActivateItemTool)
            {
                ActivateItemTool = false;
                TerrainEditRoutines.ActivateTool(ToolType.ItemTool);
            }
            if (ActivatePaintTool)
            {
                ActivatePaintTool = false;
                TerrainEditRoutines.ActivateTool(ToolType.PaintTool);
            }
            if (ActivatePropertiesTool)
            {
                ActivatePropertiesTool = false;
                TerrainEditRoutines.ActivateTool(ToolType.PropertiesTool);
            }

            // 7/4/2010 - Update tool in use.
            switch (TerrainEditRoutines.ToolInUse)
            {
                case ToolType.None:
                    break;
                case ToolType.HeightTool:
                    // 7/2/2010
                    if (TerrainHeightToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainHeightToolRoutines.Update(_gameTime);
                        CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);
                    }
                    break;
                case ToolType.PaintTool:
                    // 7/7/2010
                    if (TerrainPaintToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainPaintToolRoutines.Update(_gameTime);
                        CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);
                    }
                    break;
                case ToolType.ItemTool:
                    // 7/4/2010
                    if (TerrainItemToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainItemToolRoutines.Update(_gameTime);
                        CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);
                    }
                    break;
                case ToolType.PropertiesTool:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 3/30/2011
            DoCloseWpfWindowCheck();
        }

        /// <summary>
        /// Helper method which closes a <see cref="ToolType"/> Wpf window.
        /// </summary>
        private static void DoCloseWpfWindowCheck()
        {

            switch (ToolTypeToClose)
            {
                case ToolType.None:
                    return;
                case ToolType.HeightTool:
                    // 7/2/2010
                    if (TerrainHeightToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainHeightToolRoutines.CloseForm();
                        var op = CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);

                        // 3/30/2011 - Wait for operation to complete
                        op.Wait();

                        ToolTypeToClose = ToolType.None;
                       
                    }
                    break;
                case ToolType.MainMenuTool:
                    // 7/2/2010
                    if (_mainMenuToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => _mainMenuToolRoutines.CloseForm();
                        var op = CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);

                        // 3/30/2011 - Wait for operation to complete
                        op.Wait();

                        ToolTypeToClose = ToolType.None;
                        
                    }
                    break;
                case ToolType.PaintTool:
                    // 7/7/2010
                    if (TerrainPaintToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainPaintToolRoutines.CloseForm();
                        var op = CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);

                        // 3/30/2011 - Wait for operation to complete
                        op.Wait();

                        ToolTypeToClose = ToolType.None;
                       
                    }
                    break;
                case ToolType.ItemTool:
                    // 7/4/2010
                    if (TerrainItemToolRoutines != null)
                    {
                        // Use Dispatcher to communicate call to STA thread.
                        SimpleDelegate del1 = () => TerrainItemToolRoutines.CloseForm();
                        var op = CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, del1);

                        // 3/30/2011 - Wait for operation to complete
                        op.Wait();

                        ToolTypeToClose = ToolType.None;
                       
                    }
                    break;
                case ToolType.PropertiesTool:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // 1/16/2011
            TerrainEditRoutines.TerrainTools_FormClosed(null, null);
        }

        // 7/2/2010
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // stop thread
            // Use Dispatcher to communicate call to STA thread.
            CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, new SimpleDelegate(DisposeDelegate));
            CurrentDispatcher.InvokeShutdown(); // 1/21/2011

            // wait a few ms, to see if works; otherwise force shutdown.
            _threadInStdMode.Join(5000);
            
            _threadInStdMode = null;
        }

        // 7/9/2010
        public void DisposeDelegate()
        {
            // dispose all WPF forms.
            if (TerrainHeightToolRoutines != null)
            {
                TerrainHeightToolRoutines.Dispose();
                _terrainHeightToolRoutines = null;
            }

            if (TerrainItemToolRoutines != null)
            {
                TerrainItemToolRoutines.Dispose();
                _terrainItemToolRoutines = null;
            }

            if (TerrainPaintToolRoutines != null)
            {
                TerrainPaintToolRoutines.Dispose();
                _terrainPaintToolRoutines = null;
            }

            // stop thread
            _stopThread = true;
        }
    }
}
