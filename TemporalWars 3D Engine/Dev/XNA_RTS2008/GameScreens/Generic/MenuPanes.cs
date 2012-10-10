#region File Description
//-----------------------------------------------------------------------------
// LobbyScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using ImageNexus.BenScharbach.TWEngine.HandleGameInput;
using ImageNexus.BenScharbach.TWEngine.ScreenManagerC;
using Microsoft.Xna.Framework;

#if XBOX360
using ImageNexus.BenScharbach.TWEngine.Common.Extensions;
#endif

namespace ImageNexus.BenScharbach.TWEngine.GameScreens.Generic
{
    // 2/20/2009
    ///<summary>
    /// The <see cref="MenuPanes"/> is simply a collection of <see cref="MenuPaneItem"/>.
    ///</summary>
    public class MenuPanes : GameScreen
    {
        private int _selectedEntry;

        #region Properties

        ///<summary>
        /// Returns a collection of <see cref="MenuPaneItem"/> contained in
        /// this <see cref="MenuPanes"/>.
        ///</summary>
        public List<MenuPaneItem> MenuPaneItems { get; private set; }

        #endregion

        /// <summary>
        /// Constructs a new <see cref="MenuPanes"/>, by creating the internal
        /// collection of <see cref="MenuPaneItem"/> and setting the values <see cref="GameScreen.TransitionOnTime"/>
        /// and <see cref="GameScreen.TransitionOffTime"/> to 0.5 seconds.
        /// </summary>
        public MenuPanes()
        {
            MenuPaneItems = new List<MenuPaneItem>();          

            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }


        /// <summary>
        /// Updates the <see cref="MenuPanes"/>, by iterating the internal collection of <see cref="MenuPaneItems"/>, and
        /// calling the Update method for each.
        /// </summary>    
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>  
        public sealed override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // 4/29/2010 - Refactored looping collection to new STATIC method.
            UpdateMenuPaneItems(gameTime, otherScreenHasFocus, coveredByOtherScreen, MenuPaneItems);
        }

        // 4/29/2010
        /// <summary>
        /// Helper method, which iterates the internal collection of <see cref="MenuPaneItems"/>, calling
        /// the Update method for each.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="otherScreenHasFocus">Another <see cref="GameScreen"/> has focus?</param>
        /// <param name="coveredByOtherScreen">Is covered by another <see cref="GameScreen"/>?</param>  
        /// <param name="menuPaneItems"><see cref="IList{t}"/> collection of <see cref="MenuPaneItems"/></param>
        private static void UpdateMenuPaneItems(GameTime gameTime, bool otherScreenHasFocus, 
                                                bool coveredByOtherScreen, IList<MenuPaneItem> menuPaneItems)
        {
            // iterate through all panes
            var count = menuPaneItems.Count; // 4/29/2010
            for (var i = 0; i < count; i++)
            {
                // 4/29/2010 - Cache
                var menuPaneItem = menuPaneItems[i];
                if (menuPaneItem == null) continue;

                menuPaneItem.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
            }
        }

        /// <summary>
        /// Responds to user input, changing the selected entry and accepting
        /// or cancelling the <see cref="MenuPanes"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        public sealed override void DoHandleInput(GameTime gameTime, InputState input)
        {
            // 4/29/2010 - Refactored code to new STATIC method.
            DoHandleInput(this, input, gameTime);
        }

        // 4/29/2010
        /// <summary>
        /// Method helper, which handles the check for the 'MenuLeft'
        /// or 'MenuRight' selections for the current <see cref="MenuPanes"/> item.
        /// </summary>
        /// <param name="menuPanes">This instance of <see cref="MenuPanes"/></param>
        /// <param name="input"><see cref="InputState"/> instance</param>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        private static void DoHandleInput(MenuPanes menuPanes, InputState input, GameTime gameTime)
        {
            // Move to the previous menu pane?
            var menuPaneItems = menuPanes.MenuPaneItems; // 4/29/2010 - Cache
            if (input.MenuLeft)
            {
                // 4/8/2009 - Set to 'unselected'
                menuPaneItems[menuPanes._selectedEntry].IsSelected = false;

                menuPanes._selectedEntry--;

                if (menuPanes._selectedEntry < 0)
                    menuPanes._selectedEntry = menuPaneItems.Count - 1;

                // 4/8/2009 - Set to 'selected'.
                menuPaneItems[menuPanes._selectedEntry].IsSelected = true;
            }

            // Move to the next menu pane?
            if (input.MenuRight)
            {
                // 4/8/2009 - Set to 'unselected'
                menuPaneItems[menuPanes._selectedEntry].IsSelected = false;

                menuPanes._selectedEntry++;

                if (menuPanes._selectedEntry >= menuPaneItems.Count)
                    menuPanes._selectedEntry = 0;

                // 4/8/2009 - Set to 'selected'.
                menuPaneItems[menuPanes._selectedEntry].IsSelected = true;
            }

            // Call HandleInput for the 'SelectedEntry' MenuPaneItem.
            menuPaneItems[menuPanes._selectedEntry].DoHandleInput(gameTime, input);
        }

        // 6/15/2009 - Find MenuPane by Title.
        ///<summary>
        /// Locates a <see cref="MenuPanes"/> by the given <paramref name="title"/>.
        ///</summary>
        ///<param name="title">Title to locate</param>
        ///<returns>Index in collection</returns>
        public int FindMenuPaneIndexByTitle(string title)
        {
            // Set name for predicate search.
            _menuEntryTitleForSearch = title;

            // search using the List 'FindIndex' method.
            return MenuPaneItems.FindIndex(FindMenuEntryByNamePredicate);
        }

        private string _menuEntryTitleForSearch;

        // 6/15/2009 - 
        /// <summary>
        /// Predicate delegate method for the FindIndex method.
        /// </summary>
        /// <param name="menuPaneItem"><see cref="MenuPaneItem"/> instance</param>
        /// <returns>true/false if found</returns>
        private bool FindMenuEntryByNamePredicate(MenuPaneItem menuPaneItem)
        {
            if (string.IsNullOrEmpty(_menuEntryTitleForSearch))
                return false;

            return menuPaneItem.MenuTitle == _menuEntryTitleForSearch;
        }

        /// <summary>
        /// Iterates the internal collection of <see cref="MenuPaneItems"/>, calling
        /// the Draw2D method for each.
        /// </summary><param name="gameTime">Instance of game time.</param>
        /// 
        public sealed override void Draw2D(GameTime gameTime)
        {
            // iterate through all panes
            var count = MenuPaneItems.Count; // 4/28/2010
            for (var i = 0; i < count; i++)
            {
                // 4/28/2010 - Cache
                var menuPaneItem = MenuPaneItems[i];
                if (menuPaneItem == null) continue;

                menuPaneItem.Draw2D(gameTime);
            }
        }

        
    }
}
