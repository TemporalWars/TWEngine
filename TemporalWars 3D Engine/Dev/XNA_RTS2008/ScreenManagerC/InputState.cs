#region File Description
//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using TWEngine.Shadows;
using System;


namespace TWEngine.ScreenManagerC
{
    /// <summary>
    /// The <see cref="InputState"/> class, is a helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        ///<summary>
        /// Maximum number of game input controllers allowed.
        ///</summary>
        public const int MaxInputs = 4;

        private readonly KeyboardState[] _currentKeyboardStates;       
        private readonly GamePadState[] _currentGamepadStates;
        private readonly KeyboardState[] _lastKeyboardStates;
        private readonly GamePadState[] _lastGamepadStates;

        private MouseState _currentMouseState;       
        private MouseState _lastMouseState;

        private int _currentMouseWheelValue;       
        private int _lastMouseWheelValue;       

        // 11/6/2009 - Gametime
        private GameTime _gameTime;

        // 11/6/2009 - DoubleClick atts.
        private static bool _previousClicked;
        private static double _previousGameTime;
        private const double DoubleClickMinValue = 200; 
        private const double DoubleClickMaxValue = 700; 
       

        // 5/2/2009 - AreaSelect TimeSpan - Used to start AreaSelect after 0.10f seconds.
        private TimeSpan _areaSelectElapsedTime = TimeSpan.Zero;

        #endregion       

        #region Initialization

        /// <summary>
        /// Constructs a new <see cref="InputState"/>.
        /// </summary>
        public InputState()
        {
            _currentKeyboardStates = new KeyboardState[MaxInputs];
            _currentGamepadStates = new GamePadState[MaxInputs];
            _currentMouseState = new MouseState(); // 4/28/2009

            _lastKeyboardStates = new KeyboardState[MaxInputs];
            _lastGamepadStates = new GamePadState[MaxInputs];
            _lastMouseState = new MouseState(); // 4/28/2009
            
        }


        #endregion

        #region Properties

        ///<summary>
        /// Returns the current <see cref="KeyboardState"/> collection.
        ///</summary>
        public KeyboardState[] CurrentKeyboardStates
        {
            get { return _currentKeyboardStates; }
        }

        ///<summary>
        /// Returns the current <see cref="GamePadState"/> collection.
        ///</summary>
        public GamePadState[] CurrentGamepadStates
        {
            get { return _currentGamepadStates; }
        }

        ///<summary>
        /// Returns the prior <see cref="KeyboardState"/> collection.
        ///</summary>
        public KeyboardState[] LastKeyboardStates
        {
            get { return _lastKeyboardStates; }
        }

        ///<summary>
        /// Returns the prior <see cref="GamePadState"/> collection.
        ///</summary>
        public GamePadState[] LastGamepadStates
        {
            get { return _lastGamepadStates; }
        }

        ///<summary>
        /// Returns the current <see cref="MouseState"/>
        ///</summary>
        public MouseState CurrentMouseState
        {
            get { return _currentMouseState; }
        }


        ///<summary>
        /// Returns the prior <see cref="MouseState"/>
        ///</summary>
        public MouseState LastMouseState
        {
            get { return _lastMouseState; }
        }

        ///<summary>
        /// Returns the current mouse wheel value
        ///</summary>
        public int CurrentMouseWheelValue
        {
            get { return _currentMouseWheelValue; }
        }

        ///<summary>
        /// Returns the prior mouse wheel value
        ///</summary>
        public int LastMouseWheelValue
        {
            get { return _lastMouseWheelValue; }
        }

        // 4/3/2011
        public bool MouseScrollRight
        {
            get
            {
                return (_currentMouseState.X > _lastMouseState.X ? true : false);
            }
        }

        // 4/3/2011
        public bool MouseScrollLeft
        {
            get
            {
                return (_currentMouseState.X < _lastMouseState.X ? true : false);
            }
        }

        /// <summary>
        /// Checks for a "menu up" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuUp
        {
            get
            {
                return IsNewKeyPress(Keys.Up) ||
                       IsNewButtonPress(Buttons.DPadUp) ||
                       IsNewButtonPress(Buttons.LeftThumbstickUp);
            }
        }


        /// <summary>
        /// Checks for a "menu down" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuDown
        {
            get
            {
                return IsNewKeyPress(Keys.Down) ||
                       IsNewButtonPress(Buttons.DPadDown) ||
                       IsNewButtonPress(Buttons.LeftThumbstickDown);
            }
        }

        // 2/20/2009
        /// <summary>
        /// Checks for a "menu left" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuLeft
        {
            get
            {
                return IsNewKeyPress(Keys.Left) ||
                       IsNewButtonPress(Buttons.DPadLeft) ||
                       IsNewButtonPress(Buttons.LeftThumbstickLeft);
            }
        }

        // 2/20/2009
        /// <summary>
        /// Checks for a "menu right" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuRight
        {
            get
            {
                return IsNewKeyPress(Keys.Right) ||
                       IsNewButtonPress(Buttons.DPadRight) ||
                       IsNewButtonPress(Buttons.LeftThumbstickRight);
            }
        }


        /// <summary>
        /// Checks for a "menu select" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuSelect
        {
            get
            {
                return IsNewKeyPress(Keys.Space) ||
                       IsNewKeyPress(Keys.Enter) ||
                       IsNewButtonPress(Buttons.A) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }


        /// <summary>
        /// Checks for a "menu cancel" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool MenuCancel
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.B) ||
                       IsNewButtonPress(Buttons.Back);
            }
        }


        /// <summary>
        /// Checks for a "pause the game" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool PauseGame
        {
            get
            {
                return IsNewKeyPress(Keys.Escape) ||
                       IsNewButtonPress(Buttons.Back) ||
                       IsNewButtonPress(Buttons.Start);
            }
        }

        // 5/29/2012
        /// <summary>
        /// Checks for a "pause the game" input action and pauses without pause screen, 
        /// which is useful for developer pics.
        /// </summary>
        public bool PauseGameForPic
        {
            get { return IsNewKeyPress(Keys.P); }
        }

        // 1/19/2011
        /// <summary>
        /// Shows the <see cref="Guide.ShowMarketplace"/> menu, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool ShowMarketplace
        {
            get
            {
                return GamepadRightTriggerReleased ||
                       AltM;
            }
        }

        // 1/19/2011
        /// <summary>
        /// Checks if the XBox Right-Trigger is released, from any player.
        /// </summary>
        public bool GamepadRightTriggerReleased
        {
            get { return IsNewButtonReleased(Buttons.RightTrigger); }
        }

        // 4/23/2011
        /// <summary>
        /// Checks if the XBox Left-Trigger is released, from any player.
        /// </summary>
        public bool GamepadLeftTriggerReleased
        {
            get { return IsNewButtonReleased(Buttons.LeftTrigger); }
        }

        // 1/19/2011
        /// <summary>
        /// Checks for the keys 'LeftAlt' and 'M'.
        /// </summary>
        public bool AltM
        {
            get
            {
                return IsKeyPress(Keys.LeftAlt) && IsKeyPress(Keys.M);
            }
        }

        // 4/28/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'S'.
        /// </summary>
        public bool LeftCtlS
        {
            get
            {
                return IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.S);

            }

        }

        // 8/13/2009
        /// <summary>
        /// Checks for the keys 'LeftShift'.
        /// </summary>
        public bool LeftShift
        {
            get
            {
                return IsKeyPress(Keys.LeftShift);
            }
        }

        // 8/13/2009
        /// <summary>
        /// Checks for the keys 'RightShift'.
        /// </summary>
        public bool RightShift
        {
            get
            {
                return IsKeyPress(Keys.RightShift);
            }
        }

        // 6/16/2009
        ///<summary>
        /// Checks for keys 'LeftShift' or 'RightShift'.
        ///</summary>
        public bool ShiftSelected
        {
            get
            {
                return LeftShift || RightShift;
            }
        }        

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-1'.
        /// </summary>
        public bool AssignGroup1
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad1)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D1));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-2'.
        /// </summary>
        public bool AssignGroup2
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad2)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D2));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-3'.
        /// </summary>
        public bool AssignGroup3
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad3)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D3));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-4'.
        /// </summary>
        public bool AssignGroup4
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad4)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D4));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-5'.
        /// </summary>
        public bool AssignGroup5
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad5)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D5));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-6'.
        /// </summary>
        public bool AssignGroup6
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad6)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D6));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-7'.
        /// </summary>
        public bool AssignGroup7
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad7)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D7));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-8'.
        /// </summary>
        public bool AssignGroup8
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad8)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D8));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-9'.
        /// </summary>
        public bool AssignGroup9
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad9)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D9));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-0'.
        /// </summary>
        public bool AssignGroup10
        {
            get
            {
                return (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.NumPad0)) ||
                    (IsKeyPress(Keys.LeftControl) && IsKeyPress(Keys.D0));
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-1'.
        /// </summary>
        public bool SelectGroup1
        {
            get
            {
                return IsKeyPress(Keys.NumPad1) || IsKeyPress(Keys.D1);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-2'.
        /// </summary>
        public bool SelectGroup2
        {
            get
            {
                return IsKeyPress(Keys.NumPad2) || IsKeyPress(Keys.D2);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-3'.
        /// </summary>
        public bool SelectGroup3
        {
            get
            {
                return IsKeyPress(Keys.NumPad3) || IsKeyPress(Keys.D3);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-4'.
        /// </summary>
        public bool SelectGroup4
        {
            get
            {
                return IsKeyPress(Keys.NumPad4) || IsKeyPress(Keys.D4);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-5'.
        /// </summary>
        public bool SelectGroup5
        {
            get
            {
                return IsKeyPress(Keys.NumPad5) || IsKeyPress(Keys.D5);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-6'.
        /// </summary>
        public bool SelectGroup6
        {
            get
            {
                return IsKeyPress(Keys.NumPad6) || IsKeyPress(Keys.D6);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-7'.
        /// </summary>
        public bool SelectGroup7
        {
            get
            {
                return IsKeyPress(Keys.NumPad7) || IsKeyPress(Keys.D7);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-8'.
        /// </summary>
        public bool SelectGroup8
        {
            get
            {
                return IsKeyPress(Keys.NumPad8) || IsKeyPress(Keys.D8);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-9'.
        /// </summary>
        public bool SelectGroup9
        {
            get
            {
                return IsKeyPress(Keys.NumPad9) || IsKeyPress(Keys.D9);
            }
        }

        // 5/13/2009
        /// <summary>
        /// Checks for the keys 'LeftControl' and 'Number-0'.
        /// </summary>
        public bool SelectGroup10
        {
            get
            {
                return IsKeyPress(Keys.NumPad0) || IsKeyPress(Keys.D0);
            }
        }

        // 4/28/2009
        /// <summary>
        /// Checks for a "Delete" input action, from any player,
        /// on either keyboard or gamepad.
        /// </summary>
        public bool Delete
        {
            get
            {
                return IsNewKeyPress(Keys.Delete) ||
                    IsNewButtonPress(Buttons.X); 
            }
        }

        // 4/28/2009
        /// <summary>
        /// Checks for a 'IsPicked' input action, from any player, 
        /// on either mouse or gamepad.
        /// </summary>
        public bool IsPicked
        {
            get
            {
                return (_currentMouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released) || IsNewButtonPress(Buttons.A);

            }

        }

        // 4/28/2009
        ///<summary>
        /// Checks if the middle mouse button was pressed.
        ///</summary>
        public bool MiddleMouseButton
        {
            get
            {
                return _currentMouseState.MiddleButton == ButtonState.Pressed &&
                    _lastMouseState.MiddleButton == ButtonState.Released;
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks if the left mouse button was pressed.
        ///</summary>
        public bool LeftMouseButton
        {
            get
            {
                return _currentMouseState.LeftButton == ButtonState.Pressed &&
                    _lastMouseState.LeftButton == ButtonState.Released;
            }
        }

        // 11/6/2009
        ///<summary>
        /// Checks if the left mouse button was just released.
        ///</summary>
        public bool LeftMouseButtonReleased
        {
            get
            {
                return _currentMouseState.LeftButton == ButtonState.Released &&
                    _lastMouseState.LeftButton == ButtonState.Pressed;
            }
        }

        // 11/21/2009
        ///<summary>
        /// Checks if the left mouse button is being held down continously.
        ///</summary>
        public bool LeftMouseButtonHeldDown
        {
            get
            {
                return _currentMouseState.LeftButton == ButtonState.Pressed;// && _lastMouseState.LeftButton == ButtonState.Pressed; 
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks if the right mouse button is pressed.
        ///</summary>
        public bool RightMouseButton
        {
            get
            {
                return _currentMouseState.RightButton == ButtonState.Pressed &&
                    _lastMouseState.RightButton == ButtonState.Released;
            }
        }

        // 1/11/2010
        /// <summary>
        /// Checks if the right mouse button was just released.
        /// </summary>
        public bool RightMouseButtonReleased
        {
            get
            {
                return _currentMouseState.RightButton == ButtonState.Released &&
                       _lastMouseState.RightButton == ButtonState.Pressed;
            }
        }

        #region DoubleClick Properties

       
        // 11/6/2009 - Updated.
        ///<summary>
        /// Checks for a double left-click mouse event.
        ///</summary>
        public bool DoubleClick
        {
            get
            {
                //bool isDoubleClick = (LeftMouseButton && DoubleClickWithinMinMaxElapsedTime && DoubleClickSameItem) ||
                    //(IsNewButtonPress(Buttons.A) && DoubleClickWithinMinMaxElapsedTime && DoubleClickSameItem);

                var isDoubleClick = (_previousClicked && LeftMouseButtonReleased && DoubleClickWithinMinMaxElapsedTime) ||
                    (_previousClicked && IsNewButtonReleased(Buttons.A) && DoubleClickWithinMinMaxElapsedTime);

                if (!isDoubleClick)
                {
                    if (LeftMouseButtonReleased || IsNewButtonReleased(Buttons.A))
                    {
                        _previousGameTime = _gameTime.TotalGameTime.TotalMilliseconds;
                        _previousClicked = true;
                    }
                    return false;
                }
                _previousClicked = false;
                
                return true;
            }
        }

        // 6/16/2009
        private bool DoubleClickWithinMinMaxElapsedTime
        {
            get
            {
                var doubleClickElapsedTime = _gameTime.TotalGameTime.TotalMilliseconds - _previousGameTime;

                return doubleClickElapsedTime > DoubleClickMinValue && doubleClickElapsedTime < DoubleClickMaxValue;
            }
        }
       

        #endregion

        // 4/28/2009
        ///<summary>
        /// Checks for 'Up' or 'LeftThumbstickUp' being pressed.
        ///</summary>
        public bool MoveCameraForward
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.Up) || (IsButtonPress(Buttons.LeftThumbstickUp)); //  && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'Right' or 'LeftThumbstickRight' being pressed.
        ///</summary>
        public bool MoveCameraRight
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.Right) || (IsButtonPress(Buttons.LeftThumbstickRight)); // && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'Down' or 'LeftThumbstickDown' being pressed.
        ///</summary>
        public bool MoveCameraBackward
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.Down) || (IsButtonPress(Buttons.LeftThumbstickDown)); // && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'Left' or 'LeftThumbstickLeft' being pressed.
        ///</summary>
        public bool MoveCameraLeft
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.Left) || (IsButtonPress(Buttons.LeftThumbstickLeft)); // && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'PageUp' or 'RightThumbstickDown' being pressed.
        ///</summary>
        public bool MoveCameraHigher
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.PageUp) ||
                    (IsButtonPress(Buttons.RightThumbstickDown)) || // && !IFDTileManager.IFDTileSetIsDisplaying
                    _currentMouseWheelValue > _lastMouseWheelValue;
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'PageDown' or 'RightThumbstickUp' being pressed.
        ///</summary>
        public bool MoveCameraLower
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return IsKeyPress(Keys.PageDown) ||
                    (IsButtonPress(Buttons.RightThumbstickUp)) || // && !IFDTileManager.IFDTileSetIsDisplaying
                    _currentMouseWheelValue < _lastMouseWheelValue;
            }
        }

        // 4/28/2009; 6/5/2009: Updated to not rotate when 'DebugValues' is True for ShadowMap class.
        ///<summary>
        /// Checks for 'NumPad4' or 'RightThumbstickLeft' being pressed.
        ///</summary>
        public bool RotateCameraLeft
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return (IsKeyPress(Keys.NumPad4) && !ShadowMap._DebugValues) || (IsButtonPress(Buttons.RightThumbstickLeft)); // && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009; 6/5/2009: Updated to not rotate when 'DebugValues' is True for ShadowMap class.
        ///<summary>
        /// Checks for 'NumPad6' or 'RightThumbstickRight' being pressed.
        ///</summary>
        public bool RotateCameraRight
        {
            get
            {
                // 11/9/2009: Removed the check on the 'IFDTileSetIsDisplaying', since now the Camera's LockAll is used in its place!
                return (IsKeyPress(Keys.NumPad6) && !ShadowMap._DebugValues) || (IsButtonPress(Buttons.RightThumbstickRight)); // && !IFDTileManager.IFDTileSetIsDisplaying
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="MiddleMouseButton"/> or 'RightStick' being pressed.
        ///</summary>
        public bool ResetCameraHeight
        {
            get
            {
                return MiddleMouseButton || IsNewButtonPress(Buttons.RightStick);
            }

        }

        // 4/28/2009; 5/2/2009: Updated to also check the '_areaSelectElapsedTime' value.
        ///<summary>
        /// Checks for left mouse button pressed.
        ///</summary>
        public bool StartAreaSelect
        {
            get
            {
                return _currentMouseState.LeftButton == ButtonState.Pressed && _areaSelectElapsedTime > TimeSpan.FromSeconds(0.08f);
            }

        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'L' or 'RightShoulder' being pressed.
        ///</summary>
        public bool SelectLocalUnits
        {
            get
            {
                return IsNewKeyPress(Keys.L) || IsNewButtonPress(Buttons.RightShoulder);
            }

        }      

        // 4/28/2009
        ///<summary>
        /// Checks for 'A' or 'LeftShoulder' being pressed.
        ///</summary>
        public bool SelectAllUnits
        {
            get
            {
                return (IsNewKeyPress(Keys.A) && !(IsKeyPress(Keys.LeftAlt) || IsKeyPress(Keys.RightAlt))) || IsNewButtonPress(Buttons.LeftShoulder);
            }

        }

        // 4/28/2009
        ///<summary>
        /// Checks for 'X' or 'Gamepad.B' being pressed.
        ///</summary>
        public bool DeselectAllUnits
        {
            get
            {
                return IsNewKeyPress(Keys.X) || IsNewButtonPress(Buttons.B);
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> or 'Gamepad.Y' being pressed.
        ///</summary>
        public bool AttackOrderGiven
        {
            get
            {
                return RightMouseButton || IsNewButtonPress(Buttons.Y);
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'LeftControl'.
        ///</summary>
        public bool AttackGroundOrderGiven
        {
            get
            {
                return (RightMouseButton && IsNewKeyPress(Keys.LeftControl));
            }
        }


        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'gamepad.X'.
        ///</summary>
        public bool MoveOrderGiven
        {
            get
            {
                return (RightMouseButton && !(IsNewKeyPress(Keys.LeftControl) || IsKeyPress(Keys.LeftShift)) ||
                     IsNewButtonPress(Buttons.X));
            }
        }

        // 10/19/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> with 'LeftShift' and 'LeftControl' on PC, or 'gamepad.Y'.
        ///</summary>
        public bool AttackMoveOrderGiven
        {
            get
            {
                return ((RightMouseButton && IsKeyPress(Keys.LeftShift) && IsKeyPress(Keys.LeftControl)) || IsNewButtonPress(Buttons.Y));
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="LeftMouseButton"/> and 'gamepad.A'.
        ///</summary>
        public bool SelectAction
        {
            get
            {
                return LeftMouseButton  || IsButtonPress(Buttons.A);
            }
        }

        // 7/12/2009
        ///<summary>
        /// Checks for left mouse button released, with prior state pressed.
        ///</summary>
        public bool SelectActionFinshed
        {
            get
            {
                return (_currentMouseState.LeftButton == ButtonState.Released &&
                    _lastMouseState.LeftButton == ButtonState.Pressed);
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="LeftMouseButton"/> with 'LeftControl'.
        ///</summary>
        public bool SameItemsPick
        {
            get
            {
                return (LeftMouseButton && IsKeyPress(Keys.LeftControl));
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'gamepad.y'.
        ///</summary>
        public bool ItemAttack
        {
            get
            {
                return RightMouseButton ||
                    IsNewButtonPress(Buttons.Y);
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> with 'LeftControl'.
        ///</summary>
        public bool ForceItemAttack
        {
            get
            {
                return (RightMouseButton && IsKeyPress(Keys.LeftControl));
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="LeftMouseButton"/> and 'gamepad.A'.
        ///</summary>
        public bool MinimapMoveCamera
        {
            get
            {
                return (LeftMouseButton || IsNewButtonPress(Buttons.A));
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'gamepad.X'.
        ///</summary>
        public bool MinimapMoveUnits
        {
            get
            {
                return (RightMouseButton || IsNewButtonPress(Buttons.X));
            }
        }

        // 4/28/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'gamepad.X'.
        ///</summary>
        public bool PlaceBuildingMarker
        {
            get
            {
                return (RightMouseButton || IsNewButtonPress(Buttons.X));
            }
        }

        // 4/29/2009; 11/9/2009: Updated to 'WhenReleased'.
        ///<summary>
        /// Checks for <see cref="LeftMouseButtonReleased"/> and 'gamepad.A'.
        ///</summary>
        public bool IFDTileSelectedWhenReleased
        {
            get
            {
                // 11/9/2009 - Updated the XBOX to be ButtonReleased, rather than ButtonPressed.
                return LeftMouseButtonReleased || IsNewButtonReleased(Buttons.A);
            }
        }

        // 11/9/2009
        ///<summary>
        /// Checks for <see cref="LeftMouseButton"/> and 'gamepad.A'.
        ///</summary>
        public bool IFDTileSelectedWhenPressed
        {
            get
            {
                // 11/9/2009 - Updated the XBOX to be ButtonReleased, rather than ButtonPressed.
                return LeftMouseButton || IsNewButtonPress(Buttons.A);
            }
        }

        // 4/29/2009
        ///<summary>
        /// Checks for <see cref="RightMouseButton"/> and 'gamepad.X'.
        ///</summary>
        public bool IFDTileCanceled
        {
            get
            {
                // 11/9/2009: Updated the XBOX to use the 'X' for cancel, rather than B.
                return RightMouseButton || IsNewButtonPress(Buttons.X);
            }
        }

        // 4/29/2009
        ///<summary>
        /// Checks for <see cref="LeftMouseButton"/> and 'gamepad.A'.
        ///</summary>
        public bool IFDPlaceItem
        {
            get
            {
                return LeftMouseButton || IsNewButtonPress(Buttons.A);
            }
        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickUp' with 'LeftThumbstickUp'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos1
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickUp) && IsButtonPress(Buttons.LeftThumbstickLeft);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickDown' with 'LeftThumbstickLeft'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos2
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickDown) && IsButtonPress(Buttons.LeftThumbstickLeft);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickUp' with 'LeftThumbstickRight'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos3
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickUp) && IsButtonPress(Buttons.LeftThumbstickRight);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickDown' with 'LeftThumbstickRight'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos4
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickDown) && IsButtonPress(Buttons.LeftThumbstickRight);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickLeft'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos5
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickLeft) && !IsButtonPress(Buttons.LeftThumbstickUp) && !IsButtonPress(Buttons.LeftThumbstickDown);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickRight'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos6
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickRight) && !IsButtonPress(Buttons.LeftThumbstickUp) && !IsButtonPress(Buttons.LeftThumbstickDown);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickUp'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos7
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickUp) && !IsButtonPress(Buttons.LeftThumbstickLeft) && !IsButtonPress(Buttons.LeftThumbstickRight);
            }

        }

        // 4/29/2009 - For XBOX ONLY
        ///<summary>
        /// Checks for 'LeftThumbstickDown'.
        ///</summary>
        /// <remarks>Applies to XBOX only</remarks>
        public bool IFDTileSelectedPos8
        {
            get
            {
                return IsButtonPress(Buttons.LeftThumbstickDown) && !IsButtonPress(Buttons.LeftThumbstickLeft) && !IsButtonPress(Buttons.LeftThumbstickRight);
            }

        }

        // 6/1/2009 - DefenseAI Stance 'Aggressive'.
        ///<summary>
        /// Checks for 'LeftAlt' or 'RightAlt' with 'A'.
        ///</summary>
        public bool SelectAggressiveStance
        {
            get
            {
                return (IsKeyPress(Keys.LeftAlt) || IsKeyPress(Keys.RightAlt)) && IsKeyPress(Keys.A);
            }
        }

        // 6/1/2009 - DefenseAI Stance 'Guard'.
        ///<summary>
        /// Checks for 'LeftAlt' or 'RightAlt' with 'S'.
        ///</summary>
        public bool SelectGuardStance
        {
            get
            {
                return (IsKeyPress(Keys.LeftAlt) || IsKeyPress(Keys.RightAlt)) && IsKeyPress(Keys.S);
            }
        }

        // 6/1/2009 - DefenseAI Stance 'Hold Ground'.
        ///<summary>
        /// Checks for 'LeftAlt' or 'RightAlt' with 'D'.
        ///</summary>
        public bool SelectHoldGroundStance
        {
            get
            {
                return (IsKeyPress(Keys.LeftAlt) || IsKeyPress(Keys.RightAlt)) && IsKeyPress(Keys.D);
            }
        }

        // 6/1/2009 - DefenseAI Stance 'Hold Fire'.
        ///<summary>
        /// Checks for 'LeftAlt' or 'RightAlt' with 'G'.
        ///</summary>
        public bool SelectHoldFireStance
        {
            get
            {
                return (IsKeyPress(Keys.LeftAlt) || IsKeyPress(Keys.RightAlt)) && IsKeyPress(Keys.G);
            }
        }

        // 9/1/2009 - Is Gamer Ready to start game.
        ///<summary>
        /// Checks for 'X' or 'Enter' and 'gamepad.Start'
        ///</summary>
        public bool IsReady
        {
            get
            {
                return (IsNewKeyPress(Keys.X) || IsNewKeyPress(Keys.Enter) || IsNewButtonPress(Buttons.X) || IsNewButtonPress(Buttons.Start));
            }
        }

        // 2/8/2010
        ///<summary>
        /// Checks for 'LeftAlt'.
        ///</summary>
        public bool LeftAlt
        {
            get {  return IsKeyPress(Keys.LeftAlt); }
            
        }

        #endregion

        #region Methods


        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Update(GameTime gameTime)
        {
            // 11/6/2009 - Store Gametime
            _gameTime = gameTime;

            _lastMouseState = _currentMouseState; // 4/28/2009
            _currentMouseState = Mouse.GetState(); // 4/28/2009

            _lastMouseWheelValue = _currentMouseWheelValue; // 4/28/2009
            _currentMouseWheelValue = _currentMouseState.ScrollWheelValue; // 4/28/2009

            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                _lastKeyboardStates[i] = _currentKeyboardStates[i];
                _lastGamepadStates[i] = _currentGamepadStates[i];                

                _currentKeyboardStates[i] = Keyboard.GetState((PlayerIndex)i);
                _currentGamepadStates[i] = GamePad.GetState((PlayerIndex)i);                
            }

            // 5/2/2009 - Select Button held down for some ElapsedTime; will be used
            //            in the AreaSelect, which will only be triggered after 0.08f.
            if (_currentMouseState.LeftButton == ButtonState.Pressed
                    && _lastMouseState.LeftButton == ButtonState.Pressed)
            {
                // Increase Time
                _areaSelectElapsedTime += gameTime.ElapsedGameTime;

            }
            else if (_currentMouseState.LeftButton == ButtonState.Released
                    && _lastMouseState.LeftButton == ButtonState.Pressed)
            {
                // Reset Time
                _areaSelectElapsedTime = TimeSpan.Zero;
            }

        }

        /// <summary>
        /// Helper for checking if a key is pressed during this update,
        /// by any player.
        /// </summary>
        /// <param name="key">Instance of <see cref="Keys"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsKeyPress(Keys key)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Helper for checking if a key is pressed during this update,
        /// by the specified player.
        /// </summary>
        /// <param name="key">Instance of <see cref="Keys"/>.</param>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return _currentKeyboardStates[(int)playerIndex].IsKeyDown(key);
                    
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by any player.
        /// </summary>
        /// <param name="key">Instance of <see cref="Keys"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsNewKeyPress(Keys key)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsNewKeyPress(key, (PlayerIndex)i))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Helper for checking if a key was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        /// <param name="key">Instance of <see cref="Keys"/>.</param>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsNewKeyPress(Keys key, PlayerIndex playerIndex)
        {
            return (_currentKeyboardStates[(int)playerIndex].IsKeyDown(key) &&
                    _lastKeyboardStates[(int)playerIndex].IsKeyUp(key));
        }


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by any player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsNewButtonPress(Buttons button)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsNewButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        // 11/6/2009
        /// <summary>
        /// Helper for checking if a button was newly released during this update,
        /// by any player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsNewButtonReleased(Buttons button)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsNewButtonReleased(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Helper for checking if a button was pressed during last update,
        /// by any player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsOldButtonPress(Buttons button)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsOldButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Helper for checking if a button is pressed during this update,
        /// by any player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsButtonPress(Buttons button)
        {
            const int maxInputs = MaxInputs; // 4/27/2010
            for (var i = 0; i < maxInputs; i++)
            {
                if (IsButtonPress(button, (PlayerIndex)i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Helper for checking if a button is pressed during this update,
        /// by the specified player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return _currentGamepadStates[(int)playerIndex].IsButtonDown(button);
                    
        }        


        /// <summary>
        /// Helper for checking if a button was newly pressed during this update,
        /// by the specified player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <returns>True/False of result.</returns>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        public bool IsNewButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return (_currentGamepadStates[(int)playerIndex].IsButtonDown(button) &&
                    _lastGamepadStates[(int)playerIndex].IsButtonUp(button));
        }

        /// <summary>
        /// Helper for checking if a button was pressed during last update,
        /// by the specified player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsOldButtonPress(Buttons button, PlayerIndex playerIndex)
        {
            return _lastGamepadStates[(int)playerIndex].IsButtonDown(button);
        }

        // 11/6/2009
        /// <summary>
        /// Helper for checking if a button was newly released during this update,
        /// by the specified player.
        /// </summary>
        /// <param name="button">Instance of <see cref="Buttons"/>.</param>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsNewButtonReleased(Buttons button, PlayerIndex playerIndex)
        {
            return (_currentGamepadStates[(int)playerIndex].IsButtonUp(button) &&
                   _lastGamepadStates[(int)playerIndex].IsButtonDown(button));
        }

        /// <summary>
        /// Checks for a "menu select" input action from the specified player.
        /// </summary>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsMenuSelect(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Space, playerIndex) ||
                   IsNewKeyPress(Keys.Enter, playerIndex) ||
                   IsNewButtonPress(Buttons.A, playerIndex) ||
                   IsNewButtonPress(Buttons.Start, playerIndex);
        }


        /// <summary>
        /// Checks for a "menu cancel" input action from the specified player.
        /// </summary>
        /// <param name="playerIndex">Instance of <see cref="PlayerIndex"/>.</param>
        /// <returns>True/False of result.</returns>
        public bool IsMenuCancel(PlayerIndex playerIndex)
        {
            return IsNewKeyPress(Keys.Escape, playerIndex) ||
                   IsNewButtonPress(Buttons.B, playerIndex) ||
                   IsNewButtonPress(Buttons.Back, playerIndex);
        }


        #endregion
    }
}
