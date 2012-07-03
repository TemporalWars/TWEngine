using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PerfTimersComponent.Timers.Enums;
using ScreenTextDisplayer.ScreenText;
using SpeedCollectionComponent;


namespace PerfTimersComponent.Timers
{

    /// <summary>
    /// The <see cref="StopWatchTimers"/> class is used to measure elapsed <see cref="GameTime"/> within the game engine for debug purposes.  Internally, the
    /// component uses the system's <see cref="Stopwatch"/> class, but then averages the times into an array and draws to screen for user convenience.  This is especially 
    /// important for the XBox-360, since few (if any) commerical products exist to measure performance on this system.
    /// </summary>
    public sealed class StopWatchTimers : GameComponent
    {
        // 11/07/2008 - Dictionary of StopWatch Timers
        private static SpeedCollection<Stopwatch> _stopWatches; 

        // 8/20/2009 - Constant number of enum names.
        private const int StopWatchNamesCount = 63;

        // 4/21/2010 - Used in UpdateStopWatchAverages method.
        private static int[] _keys = new int[1];

        // Starting position of first line 
        private static int _textItemYLoc = 115; // 2/16/2010: Was 75
        // Spacing between each line.
        private const int TextItemYSpacing = 12;
        
        // 4/21/2009
        private static bool _visible;

        // 6/3/2010: Updated to be required class, rather than struct, to be used in SpeedCollection;
        //           this is due to the change of using InterLocked calls within SpeedCollection.
        private class Averages
        {
            public int Count;
            public float Total;
            public float Average;
        }

        // 8/24/2009 - 
        /// <summary>
        /// <see cref="Stopwatch"/> names as strings
        /// </summary>
        private static SpeedCollection<string> _stringNames;

        // 
        /// <summary>
        /// Collection of Averages, used to keep the average times of each <see cref="Stopwatch"/> instance.
        /// </summary>
        private static SpeedCollection<Averages> _averageTimes;
        // 11/17/2008 - 
        /// <summary>
        /// Collection of Max <see cref="Stopwatch"/> times.
        /// </summary>
        private static Dictionary<int, float> _maxTimes; // 6/3/2010: Changed to Dictionary, since SpeedCollection doesn't accept floats anymore.
      
        /// <summary>
        /// Collection of <see cref="ScreenTextItem"/>, used to display time data from each <see cref="Stopwatch"/> instance.
        /// </summary>
        private static List<ScreenTextItem> _screenTextItems;

        // 1/21/2011 - Draw color for stop watch text.
        private static readonly Color StopWatchDrawColor = Color.WhiteSmoke;

        #region Properties

        ///<summary>
        /// Display the <see cref="Stopwatch"/> timers on screen?
        ///</summary>
        public bool IsVisible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;

                // 4/21/2010 - cache
                var screenTextItems = _screenTextItems; 
                if (screenTextItems == null) return;
                
                // Loop StopWatches and Turn off Drawing
                var count = screenTextItems.Count;
                for (var i = 0; i < count; i++)
                {
                    var textItem = screenTextItems[i];
                    textItem.Visible = value;
                    screenTextItems[i] = textItem;
                }
                
            }
        }

        #endregion

        ///<summary>
        /// The constructor for <see cref="StopWatchTimers"/>, which initializes
        /// the required internal collections, and populates the 'Names' collection
        /// with all <see cref="StopWatchName"/> Enums, as string names.
        ///</summary>
        ///<param name="game"><see cref="Game"/> instance</param>
        public StopWatchTimers(Game game)
            : base(game)
        {
            _stringNames = new SpeedCollection<string>(StopWatchNamesCount); // 8/24/2009
            _stopWatches = new SpeedCollection<Stopwatch>(StopWatchNamesCount);
            _averageTimes = new SpeedCollection<Averages>(StopWatchNamesCount);
            _maxTimes = new Dictionary<int, float>(StopWatchNamesCount);
            _screenTextItems = new List<ScreenTextItem>();
           
            // 8/24/2009
            // Get String names for all Enums
            for (var i = 0; i < StopWatchNamesCount; i++)
            {
                _stringNames.Add(i, ((StopWatchName) i).ToString()); // boxing with ToString().
            }

        }

        /// <summary>
        /// Called each game cycle automatically, which in turn calls the internal
        /// method <see cref="UpdateStopWatchAverages"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public override void Update(GameTime gameTime)
        {
            if (!_visible)
                return;

            UpdateStopWatchAverages(gameTime);
        
            base.Update(gameTime);
        }

        // 4/21/2010
        /// <summary>
        /// Iterates the <see cref="_stopWatches"/> collection, updating the average times.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void UpdateStopWatchAverages(GameTime gameTime)
        {
            try // 8/6/2009
            {
                // Draw the StopWatch Times to screen for Debug Purposes
                // Note: Since an entry was made into both the Dictionary & List Array, there should
                //       be a 1-1 ratio.
                var count = _stopWatches.Keys.Count;
                var keysLength = _keys.Length;
                if (keysLength != count)
                {
                    Array.Resize(ref _keys, count);
                    // copy _keys into array
                    _stopWatches.Keys.CopyTo(_keys, 0);
                }

                // 4/21/2010 - Cache collection
                var averageTimes = _averageTimes;
                var screenTextItems = _screenTextItems;
                var milliseconds = gameTime.ElapsedGameTime.Milliseconds;

                // Loop StopWatches and Store Average Time into screenTextItem.
                for (var i = 0; i < keysLength; i++)
                {
                    // 12/18/2008 - Calc % of Frame ElapsedTime.
                    var frameElapsedPortion = 0;
                    var key = _keys[i]; // 8/13/2009

                    if (averageTimes.ContainsKey(key)) // 3/28/2009: Updated to avoid trying to use a key which does not exist!
                    {
                        frameElapsedPortion = (int)((averageTimes[key].Average / milliseconds) * 100);
                    }

                    // 4/21/2009 - Updated to use the string.concat.                
                    var textItem = screenTextItems[i];
                    if (!textItem.Visible) continue;

                    // Populate StringBuilder
                    textItem.SbDrawText.Length = 0;
                    //textItem.SbDrawText.Insert(0, ((StopWatchName)key).ToString());
                    try
                    {
                        // 8/24/2009
                        textItem.SbDrawText.Append(_stringNames[key]);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // missing, so add Enum to string names array
                        _stringNames.Add(key, ((StopWatchName)key).ToString()); // boxing with ToString().
                    }
                    textItem.SbDrawText.Append(": Avg= ");
                    textItem.SbDrawText.Append(averageTimes[key].Average);
                    textItem.SbDrawText.Append(", Max= ");
                    textItem.SbDrawText.Append(_maxTimes[key]);
                    textItem.SbDrawText.Append(", Frame%= ");
                    textItem.SbDrawText.Append(frameElapsedPortion);
                    
                    screenTextItems[i] = textItem;
                }
                
            }
            catch (ArgumentOutOfRangeException)
            {
                Debug.WriteLine("Method Error: Update of StopWatchTimers threw 'ArgOutOfRangeExp'.");
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Method Error: Update of StopWatchTimers threw 'NullRefExp'.");
            }
        }


        // 11/7/2008
        /// <summary>
        /// Creates a new <see cref="Stopwatch"/> timer with given <see cref="StopWatchName"/> Enum as name.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        /// <param name="startTimerNow">Should timer be started immediately upon creation?</param>
        public static void CreateStopWatchInstance(StopWatchName name, bool startTimerNow)
        {
            try
            {
                var stopWatch = new Stopwatch();

                // 4/11/2009
                if (_stopWatches == null)
                    _stopWatches = new SpeedCollection<Stopwatch>(StopWatchNamesCount);

                // 4/11/2009
                if (_screenTextItems == null)
                    _screenTextItems = new List<ScreenTextItem>();

                // 4/11/2009
                if (_averageTimes == null)
                    _averageTimes = new SpeedCollection<Averages>(StopWatchNamesCount);

                // 4/11/2009
                if (_maxTimes == null)
                    _maxTimes = new Dictionary<int, float>(StopWatchNamesCount);

                // 4/21/2010 - Cache collection
                var stopWatches = _stopWatches;
                var screenTextItems = _screenTextItems;
                var averageTimes = _averageTimes;
                var maxTimes = _maxTimes;

                // Check if in dictionary for Stopwatches
                var stopWatchName = (int)name;
                if (stopWatches.ContainsKey(stopWatchName))
                {
                    stopWatches[stopWatchName] = stopWatch;
                }
                else
                {
                    // Add new Instance to Dictionary
                    stopWatches.Add(stopWatchName, stopWatch);

                    // Add new ScreenTextItem to List Array
                    ScreenTextItem textItem;
                    ScreenTextManager.AddNewScreenTextItem(string.Empty, new Vector2(50, _textItemYLoc), StopWatchDrawColor, out textItem);
                    _textItemYLoc += TextItemYSpacing; // Set for next line SceneItemOwner.
                   
                    screenTextItems.Add(textItem);

                    // Add new Averages struct to Dictionary
                    var average = new Averages { Count = 0, Total = 0, Average = 0 };
                   
                    if (!averageTimes.ContainsKey(stopWatchName)) // 8/10/2009
                        averageTimes.Add(stopWatchName, average);
                    
                    // 11/17/2008
                    // Add new Max-Time zero entry to Dictionary
                    if (!maxTimes.ContainsKey(stopWatchName)) // 8/10/2009
                        maxTimes.Add(stopWatchName, 0);
                    
                }

                // Start Stopwatch?
                if (startTimerNow)
                    stopWatch.Start();
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("Method Error in CreateStopWatchInstance; Null Ref exception.");
            }

        }

        // 11/14/2008
        /// <summary>
        /// Deletes a <see cref="Stopwatch"/> instance.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        public static void DeleteStopWatchInstance(StopWatchName name)
        {
            // 4/21/2010 - Cache
            var stopWatches = _stopWatches;
            if (stopWatches == null) return;

            // 4/21/2010 - Cache
            var screenTextItems = _screenTextItems;
            var averageTimes = _averageTimes;
            var maxTimes = _maxTimes;
           
            // Delete entry from Dictionary
            var stopWatchName = (int)name;
            if (stopWatches.Remove(stopWatchName))
            {
                // Remove one entry from the _screenTextItems List
                screenTextItems[screenTextItems.Count - 1].Dispose();
                screenTextItems.RemoveAt(screenTextItems.Count - 1);
            }

            // Delete entry from Averages Dictionary
            averageTimes.Remove(stopWatchName);

            // Delete entry from Max-Times Dictionary
            maxTimes.Remove(stopWatchName);
            
        }

        // 4/6/2009
        /// <summary>
        /// Starts a <see cref="Stopwatch"/> using given <see cref="StopWatchName"/> Enum.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        public static void StartStopWatchInstance(StopWatchName name)
        {
            // 4/21/2010 - Cache
            var stopWatches = _stopWatches;
            if (stopWatches == null) return;
            
            // start stopwatch
            if (stopWatches.ContainsKey((int)name))
                stopWatches[(int)name].Start();
            else
                CreateStopWatchInstance(name, true); // 7/20/2009
            
        }

        /// <summary>
        /// Stops current <see cref="Stopwatch"/> instance, and then updates the 
        /// average.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        public static void StopAndUpdateAverageMaxTimes(StopWatchName name)
        {
            try
            {
                // 4/21/2010 - Cache
                var stopWatches = _stopWatches;
                if (stopWatches == null) return;

                // 4/21/2010 - Cache
                var maxTimes = _maxTimes;
                var averageTimes = _averageTimes;

                // Check if in dictionary
                var stopWatchName = (int)name;
                if (stopWatches.ContainsKey(stopWatchName))
                {
                    // 8/13/2009 - Cache
                    var stopwatch = stopWatches[stopWatchName];

                    // Stop 
                    stopwatch.Stop();
                    
                    // 11/17/2008
                    // Update Max-Time Value
                    float maxTime;
                    if (maxTimes.TryGetValue(stopWatchName, out maxTime))
                    {
                        // Check if new value the Max.
                        if (stopwatch.Elapsed.Milliseconds > maxTime)
                        {
                            maxTime = stopwatch.Elapsed.Milliseconds; // Was TotalSeconds
                            maxTimes[stopWatchName] = maxTime;
                        }
                    }

                    // Update Average Time for current StopWatch instance
                    Averages average;
                    if (averageTimes.TryGetValue(stopWatchName, out average))
                    {
                        // Update values
                        average.Count += 1;
                        average.Total += stopwatch.Elapsed.Milliseconds;
                        average.Average = average.Total / average.Count;

                        // Store back into Dictionary
                        averageTimes[stopWatchName] = average;
                    }

                    // Reset StopWatch
                    stopwatch.Reset();

                } // End If in Dictionary
               
            }
            catch
            {
                Debug.WriteLine("Method Error: StopWatchTimers classes StopAndUpdateAverageMaxTimes.");
            }
        }

        /// <summary>
        /// Stops current <see cref="Stopwatch"/> instance, and then updates the 
        /// average.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        /// <param name="lastStopWatchTime">(OUT) <see cref="TimeSpan"/> structure</param>
        public static void StopAndUpdateAverageMaxTimes(StopWatchName name, out TimeSpan lastStopWatchTime)
        {
            try
            {
                // 5/28/2009
                lastStopWatchTime = TimeSpan.Zero;

                // 4/21/2010 - Cache
                var stopWatches = _stopWatches;
                if (stopWatches == null) return;

                // 4/21/2010 - Cache
                var maxTimes = _maxTimes;
                var averageTimes = _averageTimes;
            
                // Check if in dictionary
                var stopWatchName = (int)name;
                
                if (stopWatches.ContainsKey(stopWatchName))
                {
                    // 8/13/2009 - Cache
                    var stopwatch = stopWatches[stopWatchName]; 

                    // Stop 
                    stopwatch.Stop();

                    // 5/28/2009 - Store Time Elapsed
                    lastStopWatchTime = stopwatch.Elapsed;

                    // 11/17/2008
                    // Update Max-Time Value
                    float maxTime;
                    if (maxTimes.TryGetValue(stopWatchName, out maxTime))
                    {
                        // Check if new value the Max.
                        if (stopwatch.Elapsed.Milliseconds > maxTime)
                        {
                            maxTime = stopwatch.Elapsed.Milliseconds; // Was TotalSeconds
                            maxTimes[stopWatchName] = maxTime;
                        }
                    }

                    // Update Average Time for current StopWatch instance
                    Averages average;
                    if (averageTimes.TryGetValue(stopWatchName, out average))
                    {
                        // Update values
                        average.Count += 1;
                        average.Total += stopwatch.Elapsed.Milliseconds;
                        average.Average = average.Total / average.Count;

                        // Store back into Dictionary
                        averageTimes[stopWatchName] = average;
                    }

                    // Reset StopWatch
                    stopwatch.Reset();

                } // End If in Dictionary
                
            }
            catch
            {
                // 5/28/2009
                lastStopWatchTime = TimeSpan.Zero;

                Debug.WriteLine("Method Error: StopWatchTimers classes StopAndUpdateAverageMaxTimes.");
            }
        }


        // 5/29/2009
        /// <summary>
        /// Gets the average <see cref="GameTime"/> for the given <see cref="StopWatchName"/> Enum.
        /// </summary>
        /// <param name="name"><see cref="StopWatchName"/> Enum</param>
        /// <returns><see cref="Stopwatch"/> average</returns>
        public static float GetStopWatchAverage(StopWatchName name)
        {
            // 4/21/2010 - Cache
            var stopWatches = _stopWatches;
            if (stopWatches == null) return 0;
            
             Averages average;
            var averageTimes = _averageTimes; // 4/21/2010 - Cache
            return averageTimes.TryGetValue((int)name, out average) ? average.Average : 0;
        }

        // 11/7/2008
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 8/24/2009 - Clear Names Dictionary
                if (_stringNames != null)
                    _stringNames.Clear();
                // clear StopWatches Dictionary
                if (_stopWatches != null)
                    _stopWatches.Clear();
                // clear Averages Dictionary
                if (_averageTimes != null)
                    _averageTimes.Clear();
                // dispose of ScreenTextItems
                if (_screenTextItems != null)
                {
                    var count = _screenTextItems.Count;
                    for (var i = 0; i < count; i++)
                    {
                        _screenTextItems[i].Dispose();
                    }
                    _screenTextItems.Clear();
                }

                // Null Regs
                _stringNames = null; // 8/24/2009
                _stopWatches = null;
                _averageTimes = null;
                _screenTextItems = null;

            }

            base.Dispose(disposing);
        }
    }
}