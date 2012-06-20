#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using TWEngine.SceneItems;
using TWEngine.ScreenManagerC;

namespace TWEngine.GameScreens.Generic
{
    /// <summary>
    /// A single <see cref="MenuPaneItem"/>, with multiple <see cref="MenuEntry"/> instances.
    /// </summary>
    public class MenuPaneItem : MenuScreen
    {   
       
        /// <summary>
        /// Tracks a fading selection effect on the entry.
        /// </summary>
        /// <remarks>
        /// The entries transition out of the selection effect when they are deselected.
        /// </remarks>
        float _selectionFade;


        #region Events

        /// <summary>
        /// Occurs when a new <see cref="MenuPaneItem"/> <see cref="SceneItem"/> is selected.
        /// </summary>
        public event EventHandler<EventArgs> SelectIndexChanged;


        /// <summary>
        /// Triggers the <see cref="SelectIndexChanged"/> event.
        /// </summary>
        protected internal virtual void OnSelectIndexChanged()
        {
            if (SelectIndexChanged != null)
                SelectIndexChanged(this, EventArgs.Empty);
        }


        #endregion
     
        #region Initialization

        // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
        /// <summary>
        /// Constructor fills in the <see cref="MenuPaneItem"/> contents.
        /// </summary>
        /// <param name="menuPosition">Position to place <see cref="MenuScreen"/>; if left NULL, then will be automatically centered.</param>
        /// <param name="menuSize"><see cref="MenuScreen"/> Background size.</param>
        /// <param name="menuTitle"><see cref="MenuScreen"/> Title</param>
        public MenuPaneItem(string menuTitle, Vector2? menuPosition, Vector2 menuSize)
            : base(menuTitle, menuPosition, menuSize, TemporalWars3DEngine.ContentTexturesLoc + @"\Textures\roundedMenu")
        {
           // Empty
        }


        #endregion
        
        /// <summary>
        /// Updates the <see cref="MenuPaneItem"/>, by iterating the internal collection of <see cref="MenuEntry"/> and
        /// calling the 'Update' method for each.
        /// </summary>  
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>      
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // When the menu selection changes, entries gradually fade between
            // their selected and deselected appearance, rather than instantly
            // popping to the new state.
            var fadeSpeed = (float)gameTime.ElapsedGameTime.TotalSeconds * 4;

            _selectionFade = IsSelected ? Math.Min(_selectionFade + fadeSpeed, 1) : Math.Max(_selectionFade - fadeSpeed, 0);
        }

        // 4/8/2009
        /// <summary>
        /// Draws the <see cref="MenuPaneItem"/>, by calling the base <see cref="MenuScreen"/> Draw2D method, and then
        /// checking if <see cref="MenuScreen.IsSelected"/> is true, which sets the current <see cref="MenuScreen.TitleScale"/>
        /// using the call to <see cref="MenuScreen.PulsateValue"/>.
        /// </summary> 
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>            
        public sealed override void Draw2D(GameTime gameTime)
        {
            base.Draw2D(gameTime);

            // If Selected, then have title pulsate
            if (IsSelected)
            {
                TitleScale = PulsateValue(gameTime, _selectionFade); // Updated to use inherited method.
            }

        }        

        /// <summary>
        /// Overrides the base <see cref="OnCancel"/>, but does nothing special. 
        /// </summary>
        /// <remarks>
        /// Since these are <see cref="MenuPaneItem"/>, the <see cref="OnCancel"/>
        /// should NOT be activated.  Therefore, this is just left empty, avoiding
        /// the Base.OnCancel() from being triggered.
        /// </remarks>
        protected sealed override void OnCancel()
        {
            // Since these are MenuPaneItems, the OnCancel
            // should not be activated.  Therefore, this is just
            // left empty, and the Base.OnCancel() is not called!
            return;
        }

        #region Handle Input

        // 2/20/2009
        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the <see cref="MenuPaneItem"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        public sealed override void DoHandleInput(GameTime gameTime, InputState input)
        {
            base.DoHandleInput(gameTime, input);

            // Did selectedEntry change?
            if (input.MenuUp || input.MenuDown)
            {
                // yes, so fire off event!
                OnSelectIndexChanged();                
            }            
        } 

        

        #endregion
    }
}
