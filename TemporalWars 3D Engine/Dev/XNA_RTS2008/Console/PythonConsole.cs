#region File Description
//-----------------------------------------------------------------------------
// PythonConsoleComponent.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.IO;
using System.Text;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.Console.Enums;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ImageNexus.BenScharbach.TWEngine.Console
{
    ///<summary>
    /// Pyhton game scripting console.
    ///</summary>
    public sealed class PythonConsoleComponent : DrawableGameComponent
    {
        private const double AnimationTime = 0.2;
        private const double CursorBlinkTime = 0.3;
        private const int LinesDisplayed = 15;
        private const int MaxHistorySize = 1024;
        private const string Prompt = ">>> ";
        private KeyboardState _actualKeyState;
        private ASCIIEncoding _asciiEncoder;

        // Render Attributes
        private Texture2D _background;

        // 8/14/2008 - Add ContentManager Instance
        private ContentManager _contentManager;

        // Python Execution Attributes

        // Console Text Attributes
        private string _currentLine;
        private GraphicsDevice _device;
        private SpriteFont _font;
        private string[] _history;
        private int _historyHead, _historySize, _historySizeMaxValue;

        // _state and timing attributes
        private KeyboardState _lastKeyState;
        private ScriptEngine _pythonEngine; // 9/20/2010
        private MemoryStream _pythonOutput;
        private SpriteBatch _spriteBatch;
        private double _stateStartTime;

        #region Properties

        ///<summary>
        /// Enumeration of the state of the Python Console.
        ///</summary>
        public ConsoleState ConsoleState { get; private set; }

        #endregion

        ///<summary>
        /// Constructor for Python console, which sets up the scripting console 
        /// integrated into the TWEngine.
        ///</summary>
        ///<param name="game">Game instance</param>
        ///<param name="fontName">Font name to load and use for writing in the console</param>
        public PythonConsoleComponent(Game game, string fontName)
            :
                base(game)
        {
            // 8/14/2008; 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            if (_contentManager == null)
                _contentManager = new ContentManager(game.Services, TemporalWars3DEngine.ContentMiscLoc); // was "Content"

            _device = game.GraphicsDevice;
            // 9/11/2008 - Get Global SpriteBatch form Game.Services
            //this.SpriteBatch = (SpriteBatch)game.Services.GetService(typeof(SpriteBatch));
            _spriteBatch = new SpriteBatch(game.GraphicsDevice);

            _font = _contentManager.Load<SpriteFont>(fontName);

            // XNA 4.0 Updates
            //_background = new Texture2D(_device, 1, 1, 1, TextureUsage.None,SurfaceFormat.Color);
            _background = new Texture2D(_device, 1, 1, true, SurfaceFormat.Color);

            _background.SetData(new [] {new Color(0, 0, 0, 125)});

            // 9/20/2010 - IronPYthon 2.6 updates
            //_pythonEngine = new PythonEngine();
            _pythonEngine = Python.CreateEngine();
            _pythonOutput = new MemoryStream();

            // TODO: How set this is IronPython 2.6
            //_pythonEngine.SetStandardOutput(_pythonOutput);
            _asciiEncoder = new ASCIIEncoding();

            // 9/20/2010 - IronPython 2.6 updates
            //var clr = _pythonEngine.Runtime.ImportModule("Clr") as ClrModule;
            //var clr = _pythonEngine.ImportModule("clr") as ClrModule;
            /*if (clr != null)
            {
                clr.AddReference("Microsoft.Xna.Framework");
                clr.AddReference("Microsoft.Xna.Framework.Game");
                
            }*/

            // 10/1/2010 - Note: In order to add references, just need to reference one item within the assembly wanted.
            _pythonEngine.Runtime.LoadAssembly(typeof(Game).Assembly); // References 'Microsoft.XNA.Framework'.
            _pythonEngine.Runtime.LoadAssembly(typeof(AlphaTestEffect).Assembly);  // References 'Microsoft.Xna.Framework.Graphics'.
            _pythonEngine.Runtime.LoadAssembly(typeof(ContentLoadException).Assembly); // Referenes 'Microsoft.Xna.Framework.Content'

            // 9/20/2010 - Python 2.6 test for above code
            _pythonEngine.Execute("import clr");
            _pythonEngine.Execute("from Microsoft.Xna.Framework import *");
            _pythonEngine.Execute("from Microsoft.Xna.Framework.Graphics import *");
            _pythonEngine.Execute("from Microsoft.Xna.Framework.Content import *");

            _currentLine = "";
            _history = new string[MaxHistorySize];
            _history[0] = "###";
            _history[1] = "### Python Game Console Component";
            _history[2] = "###";
            _history[3] = "### Integrated By Ben Scharbach - (2008)";
            _history[4] = "###";
            _historyHead = _historySize = _historySizeMaxValue = 5;

            ConsoleState = ConsoleState.Closed;
            _lastKeyState = _actualKeyState = Keyboard.GetState();
        }

        

        ///<summary>
        /// Set global aliases to code instances.
        ///</summary>
        ///<param name="name">Set alias name.</param>
        ///<param name="value">Reference to code instances.</param>
        public void AddGlobal(string name, object value)
        {
            // 9/20/2010 - IronPython 2.6 updates
            //_pythonEngine.Globals.Add(name, value);
            _pythonEngine.Runtime.Globals.SetVariable(name, value);
        }

        /// <summary>
        /// Updates the console internal values, to track the user's
        /// typing of commands, which are then drawn to screen via the
        /// draw method.
        /// </summary>
        /// <param name="gameTime">GameTime instance</param>
        public override void Update(GameTime gameTime)
        {
            // 6/6/2009 - Add Try/Catch
            try
            {
                // XNA 4.0 Updates
                //var now = gameTime.TotalRealTime.TotalSeconds;
                var now = gameTime.TotalGameTime.TotalSeconds;

                _lastKeyState = _actualKeyState;
                _actualKeyState = Keyboard.GetState();

                #region Closing & Opening states management

                if (ConsoleState == ConsoleState.Closing)
                {
                    if (now - _stateStartTime > AnimationTime)
                    {
                        ConsoleState = ConsoleState.Closed;
                        _stateStartTime = now;
                    }

                    return;
                }

                if (ConsoleState == ConsoleState.Opening)
                {
                    if (now - _stateStartTime > AnimationTime)
                    {
                        ConsoleState = ConsoleState.Open;
                        _stateStartTime = now;
                    }

                    return;
                }

                #endregion

                #region Closed state management

                if (ConsoleState == ConsoleState.Closed)
                {
                    if (_actualKeyState.IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.P))
                    {
                        ConsoleState = ConsoleState.Opening;
                        _stateStartTime = now;
                        Visible = true;
                    }
                    else
                    {
                        return;
                    }
                }

                #endregion

                if (ConsoleState == ConsoleState.Open)
                {
                    #region initialize closing animation if user presses ctrl+p

                    if (_actualKeyState.IsKeyDown(Keys.LeftControl) && IsKeyPressed(Keys.P))
                    {
                        ConsoleState = ConsoleState.Closing;
                        _stateStartTime = now;
                        return;
                    }

                    #endregion

                    if (IsKeyPressed(Keys.Enter))
                    {
                        #region execute command

                        AddToHistory(Prompt + _currentLine);

                        try
                        {
                            _pythonEngine.Execute(_currentLine);

                            var statementOutput = new byte[_pythonOutput.Length];
                            _pythonOutput.Position = 0;
                            _pythonOutput.Read(statementOutput, 0, (int) _pythonOutput.Length);
                            _pythonOutput.Position = 0;
                            _pythonOutput.SetLength(0);

                            // 8/20/2008 - Ben: XBOX360 does not have the GetString in Compact Framework!
#if !XBOX360
                            var statementOutputString = _asciiEncoder.GetString(statementOutput);
#else
                            var statementOutputString = statementOutput.ToString();

#endif


                            var historyLines = statementOutputString.
                                Split(new [] {'\n'});

                            for (var i = 0; i < historyLines.Length; i++)
                                if ((i != historyLines.Length - 1) ||
                                    (historyLines[i].Length != 0))
                                    AddToHistory(historyLines[i]);
                        }
                        catch (Exception ex)
                        {
                            AddToHistory("ERROR: " + ex.Message);
                        }

                        _currentLine = "";

                        #endregion
                    }
                    else if (IsKeyPressed(Keys.Back))
                    {
                        if (_currentLine.Length != 0)
                            _currentLine = _currentLine.Substring(0, _currentLine.Length - 1);
                    }
                    else if (IsKeyPressed(Keys.Up))
                    {
                        _currentLine = RewindHistory();
                        _currentLine = _currentLine.Substring(4);
                    }
                    else if (IsKeyPressed(Keys.Down))
                    {
                        _currentLine = ForwardHistory();
                        _currentLine = _currentLine.Substring(4);
                    }
                    else
                    {
                        #region add next character to current command

                        var nextChar = GetCharFromKeyState();

                        if (nextChar != (char) 0)
                            _currentLine += nextChar.ToString();

                        #endregion
                    }
                }
            }
            catch (Exception)
            {
                // Capture any exceptions thrown.
                return;
            }
        }

        /// <summary>
        /// Draws users scripting commands to screen, with
        /// prior lines drawn as history.
        /// </summary>
        /// <param name="gameTime">GameTime instance</param>
        public override void Draw(GameTime gameTime)
        {
            if (ConsoleState == ConsoleState.Closed)
                Visible = false;

            // XNA 4.0 Updates
            //var now = gameTime.TotalRealTime.TotalSeconds;
            var now = gameTime.TotalGameTime.TotalSeconds;

            #region Console size & dimension management

            var consoleXSize = Game.Window.ClientBounds.Right - Game.Window.ClientBounds.Left - 20;
            var consoleYSize = _font.LineSpacing*LinesDisplayed + 20;

            const int consoleXOffset = 10;
            var consoleYOffset = 10;
            switch (ConsoleState)
            {
                case ConsoleState.Opening:
                    {
                        var startPosition = 0 - consoleYOffset - consoleYSize;
                        var endPosition = consoleYOffset;
                        consoleYOffset =
                            (int)
                            MathHelper.Lerp(startPosition, endPosition, (float) (now - _stateStartTime)/(float) AnimationTime);
                    }
                    break;
                case ConsoleState.Closing:
                    {
                        var startPosition = consoleYOffset;
                        var endPosition = 0 - consoleYOffset - consoleYSize;
                        consoleYOffset =
                            (int)
                            MathHelper.Lerp(startPosition, endPosition, (float) (now - _stateStartTime)/(float) AnimationTime);
                    }
                    break;
            }

            #endregion

            // XNA 4.0 Updates
            //_spriteBatch.Begin(SpriteBlendMode.AlphaBlend);
            _spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend);

            #region Background Drawing

            _spriteBatch.Draw(_background, new Rectangle(consoleXOffset, consoleYOffset, consoleXSize, consoleYSize),
                              Color.White);

            #endregion

            #region Current line Drawing

            var cursorVisible = (int) (now/CursorBlinkTime)%2 == 0;
            var displayedLine = Prompt + _currentLine + (cursorVisible ? "_" : "");

            _spriteBatch.DrawString(_font, displayedLine,
                                    new Vector2(consoleXOffset + 10,
                                                consoleYOffset + consoleYSize - 10 - _font.LineSpacing), Color.White);

            #endregion

            #region History Drawing

            for (var i = 1; i <= LinesDisplayed - 1; i++)
            {
                if (i > _historySize)
                    break;

                var line = _historyHead - i;
                if (line < 0)
                    line += MaxHistorySize;

                _spriteBatch.DrawString(_font, _history[line],
                                        new Vector2(consoleXOffset + 10,
                                                    consoleYOffset + consoleYSize - 10 - _font.LineSpacing*(i + 1)),
                                        Color.White);
            }

            #endregion

            _spriteBatch.End();
        }

        #region push/pop commands to _history

        private void AddToHistory(string line)
        {
            _history[_historyHead] = line;
            _historyHead = (_historyHead + 1)%MaxHistorySize;
            _historySize = _historySize + (_historySize == MaxHistorySize ? 0 : 1);
            _historySizeMaxValue = Math.Max(_historySizeMaxValue, _historySize);
        }

        private string ForwardHistory()
        {
            if (_historyHead >= _historySizeMaxValue)
                return _history[_historyHead - 1];
            _historyHead++;
            _historySize++;
            return _history[_historyHead - 1];
        }

        private string RewindHistory()
        {
            if (_historyHead <= 0)
                return "";
            _historyHead--;
            _historySize--;
            return _history[_historyHead];
        }

        #endregion

        #region keyboard Status management

        private bool IsKeyPressed(Keys key)
        {
            return _actualKeyState.IsKeyDown(key) && !_lastKeyState.IsKeyDown(key);
        }

        private char GetCharFromKeyState()
        {
            var shiftPressed = _actualKeyState.IsKeyDown(Keys.LeftShift) || _actualKeyState.IsKeyDown(Keys.RightShift);
            var altPressed = _actualKeyState.IsKeyDown(Keys.LeftAlt) || _actualKeyState.IsKeyDown(Keys.RightAlt);

            for (var index = 0; index < KeyboardHelper.AmericanBindings.Length; index++)
            {
                var binding = KeyboardHelper.AmericanBindings[index];
                if (!IsKeyPressed(binding.Key)) continue;

                if (!shiftPressed && !altPressed)
                    return binding.UnmodifiedChar;

                if (shiftPressed && !altPressed)
                    return binding.ShiftChar;

                return !shiftPressed ? binding.AltChar : binding.ShiftAltChar;
            }

            return (char) 0;
        }

        #endregion

        // 8/27/2008 - Dispose of resources
        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            // Dispose of Resources
            if (_spriteBatch != null)
                _spriteBatch.Dispose();
            if (_background != null)
                _background.Dispose();
            if (_pythonOutput != null)
                _pythonOutput.Dispose();


            // Nulls Refs
            _device = null;
            _spriteBatch = null;
            _font = null;
            _background = null;
            _pythonEngine = null;
            _pythonOutput = null;
            _asciiEncoder = null;
            _history = null;

            if (_contentManager != null)
            {
                _contentManager.Unload();
                _contentManager.Dispose();
                _contentManager = null;
            }

            base.Dispose(disposing);
        }
    }
}