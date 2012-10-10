#region File Description
//-----------------------------------------------------------------------------
// IFDTileMessage.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.IFDTiles.Enums;
using ImageNexus.BenScharbach.TWEngine.InstancedModels.Enums;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes;
using ImageNexus.BenScharbach.TWEngine.ItemTypeAttributes.Structs;
using ImageNexus.BenScharbach.TWEngine.SceneItems;
using ImageNexus.BenScharbach.TWLate.RTS_MinimapInterfaces.Minimap;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ImageNexus.BenScharbach.TWEngine.IFDTiles
{
    // 9/25/2008: Created
    /// <summary>
    /// The <see cref="IFDTileMessage"/> tile, inherited from the base <see cref="IFDTile"/>,
    /// is used to display message(s) to the end-user.
    /// </summary>
    public sealed class IFDTileMessage : IFDTile
    {
        // 1/15/2010 - 
        /// <summary>
        /// Event to notify message displayed in Multi-Displayed setting.
        /// </summary>
        public event EventHandler MessageDisplayed;

// ReSharper disable UnaccessedField.Local
        /// <summary>
        /// for "itemTag" type, who it belongs to.
        /// </summary>
        private IFDTile _parentTile; // 
// ReSharper restore UnaccessedField.Local
 
        // 5/14/2009 - Updated to use StringBuilder, which avoids Garbage on HEAP.
        //private string messageToDisplay = string.Empty;
        public StringBuilder SbMessageToDisplay;
        
        private Vector2 _messageOrigin = Vector2.Zero;   // 2/22/2009   
        private static IMinimap _miniMap;
        
        private readonly IFDMessageType _messageType;
        private MessageTagDescription _messageTag;
        private SpriteFont _titleFont;
        private SpriteFont _descFont;
        private SpriteFont _typeFont;
        private Color _fontColor = Color.White; // 1/15/2010

        private const int MessageTagWidth = 200;

        // 1/15/2010 - Struct for MultiTimedMessages.
        private struct MultiTimedMessages
        {
            public StringBuilder SbMessageToDisplay;
            public float DisplayTime;
            public bool TriggerEvent;
        }
        // 1/15/2010 - Queue of MultiTimedMessages
        private readonly Queue<MultiTimedMessages> _timedMessages;
        private MultiTimedMessages? _currentTimedMessage;
       

        #region Properties

        ///<summary>
        /// Retrieves the message to display, from the internal
        /// <see cref="StringBuilder"/> instance.
        ///</summary>
        public string MessageToDisplay
        {
            get { return SbMessageToDisplay.ToString(); }
            set 
            {
                // 5/14/2009
                SbMessageToDisplay.Remove(0, SbMessageToDisplay.Length); // remove all previous chars.
                SbMessageToDisplay.Insert(0, value);            
            }
        }

        /// <summary>
        /// The <see cref="SpriteFont"/> to use for display.
        /// </summary>
        public SpriteFont MessageFont { get; set; }

        // 2/22/2009
        ///<summary>
        /// A <see cref="Vector2"/> location, used for the <see cref="SpriteBatch"/>
        /// draw origin parameter.
        ///</summary>
        public Vector2 MessageOrigin
        {
            get { return _messageOrigin; }
            set { _messageOrigin = value; }
        }

        // 1/15/2010
        /// <summary>
        /// Message font <see cref="Color"/> to use.
        /// </summary>
        public Color FontColor
        {
            get { return _fontColor; }
            set { _fontColor = value; }
        }

        #endregion

        // 1/15/2010
        /// <summary>
        /// Constructor for creating a MultiMessage timed <see cref="IFDTileMessage"/>, which is used
        /// to show several messages, seperated by some amount of time, located on the screen.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> location to display messages</param>
        /// <param name="drawTile">Draw message tile immediately?</param>
        public IFDTileMessage(Game game, Rectangle tileLocation, bool drawTile)
            : base(game)
        {
            // Set Message Type
            _messageType = IFDMessageType.MultiTimedMessages;

            // 1/15/2010 - Init Queue for Timed messages.
            _timedMessages = new Queue<MultiTimedMessages>();

            // 9/22/2010 - XNA 4.0 Updates
            // Setup Message Background Texture
            //BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);

            // ReSharper disable RedundantExplicitArraySize
            BackgroundTexture.SetData(new Color[1] { new Color(0, 0, 125, 125) });
            // ReSharper restore RedundantExplicitArraySize

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // Setup Default Font to use
            MessageFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc +  @"\Fonts\ConsoleFont");

            // Set Drawtile
            DrawTile = drawTile;

            // Set Message to Display
            SbMessageToDisplay = new StringBuilder(string.Empty);

            IFDTileLocation.X = tileLocation.X; IFDTileLocation.Y = tileLocation.Y;
            TextureRectangleSize = tileLocation;          
        }
        
        /// <summary>
        /// Constructor for creating a Message <see cref="IFDTileMessage"/>, which is used
        /// to show some message located on the screen.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="messageToDisplay">Message to display</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> location to display message</param>
        /// <param name="drawTile">Draw message tile immediately?</param>
        public IFDTileMessage(Game game, string messageToDisplay,
                                Rectangle tileLocation, bool drawTile)
            : base(game)
        {   
            // Set Message Type
            _messageType = IFDMessageType.SimpleOneLine;

            // 9/22/2010 - XNA 4.0 Updates
            // Setup Message Background Texture
            //BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);


// ReSharper disable RedundantExplicitArraySize
            BackgroundTexture.SetData(new Color[1] { new Color(0, 0, 125, 125) });
// ReSharper restore RedundantExplicitArraySize

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // Setup Default Font to use
            MessageFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\ConsoleFont");           

            // Set Drawtile
            DrawTile = drawTile;

            // Set Message to Display
            SbMessageToDisplay = new StringBuilder(messageToDisplay);

            IFDTileLocation.X = tileLocation.X; IFDTileLocation.Y = tileLocation.Y;
            TextureRectangleSize = tileLocation;          

           
        }

        
        /// <summary>
        /// Constructor for creating a Message <see cref="IFDTileMessage"/>, which displays 
        /// <see cref="SceneItem"/> owner information on screen; for example, Title, Description, Cost.
        /// </summary>
        /// <param name="game"><see cref="Game"/> Instance</param>
        /// <param name="parentTile">Parent <see cref="IFDTile"/> it belongs to</param>
        /// <param name="itemType"><see cref="SceneItem"/> owner type to get message attributes for</param>
        /// <param name="tileLocation"><see cref="Rectangle"/> location to display message</param>
        /// <param name="drawTile">Draw message tile immediately?</param>
        public IFDTileMessage(Game game, IFDTile parentTile, ItemType itemType,
                               Rectangle tileLocation, bool drawTile)
            : base(game)
        {
            // Set Message Type
            _messageType = IFDMessageType.ItemTagDescription;

            // Save Ref to Parent Tile
            _parentTile = parentTile;

            // 9/26/2008 - Get MessageTagDescription Data for ItemType given
            MessageItemTypeAtts.ItemTypeAtts.TryGetValue(itemType, out _messageTag);

            // 9/28/2008 - Set the Round Meter Time for this SceneItemOwner
            var ifdTilePlacement = (parentTile as IFDTilePlacement); // 8/12/2009
            if (ifdTilePlacement != null)
            {
                ifdTilePlacement.RoundMeter.FullMeterValue =
                   (float)Convert.ToDouble(_messageTag.TimeToBuild);
#if DEBUG
                if (TemporalWars3DEngine.FastBuildTimes)
                    ifdTilePlacement.RoundMeter.FullMeterValue = 1;
#endif
               
            }

            // XNA 4.0 Updates
            // Setup Message Background Texture
            //BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, 1, TextureUsage.None, SurfaceFormat.Color);
            BackgroundTexture = new Texture2D(game.GraphicsDevice, 1, 1, true, SurfaceFormat.Color);


// ReSharper disable RedundantExplicitArraySize
            BackgroundTexture.SetData(new Color[1] { new Color(0, 0, 125, 125) });
// ReSharper restore RedundantExplicitArraySize

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            // Setup Default Fonts to use
            MessageFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\ConsoleFont");
            _titleFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\CourierNew"); // size=14
            _descFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\Arial10");
            _typeFont = ContentManager.Load<SpriteFont>(TemporalWars3DEngine.ContentMiscLoc + @"\Fonts\Arial12");

            // Set Drawtile
            DrawTile = drawTile;            

            // Location to draw the message box.
            if (_miniMap == null)
                _miniMap = (IMinimap)game.Services.GetService(typeof(IMinimap));

            if (_miniMap != null) IFDTileLocation.X = _miniMap.MiniMapDestination.Left - MessageTagWidth;
            IFDTileLocation.Y = parentTile.IFDTileLocationP.Y;

            // Background size; height doesn't matter, since it grows automatically in draw method.
            BackgroundTextureRectSize = new Rectangle(tileLocation.X, tileLocation.Y, MessageTagWidth, 0);

        }
        /// <summary>
        /// Constructor for creating the generic <see cref="IFDTileMessage"/>.
        /// </summary>
        /// <param name="game"><see cref="Game"/> instance</param>
        public IFDTileMessage(Game game)
            : base(game)
        {
            return;
        }
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public IFDTileMessage() : this(TemporalWars3DEngine.GameInstance)
        {
            return;
        }

        /// <summary>
        /// Renders this <see cref="IFDTileMessage"/>, checking the <see cref="IFDMessageType"/> Enum,
        /// which determines draw type.
        /// </summary>   
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>    
        public override void RenderInterFaceTile(GameTime gameTime)
        {
            // Draw Tile?
            if (!DrawTile)
                return;

            // Draw Message
            switch (_messageType)
            {
                case IFDMessageType.SimpleOneLine:
                    DrawSimpleMessageType(SbMessageToDisplay);
                    break;
                case IFDMessageType.MultiTimedMessages: // 1/15/2010
                    DrawMultiTimedMessagesType(gameTime);
                    break;
                case IFDMessageType.ItemTagDescription:
                    DrawTagItemMessageType(ref _messageTag, _titleFont, _descFont, _typeFont, BackgroundTexture, 
                                            ref BackgroundTextureRectSize, ref IFDTileLocation);
                    break;
                default:
                    break;
            }
            

            base.RenderInterFaceTile(gameTime);
        }

        // 1/15/2010
        /// <summary>
        /// Draws the Multi-Message timed type to screen.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>    
        private void DrawMultiTimedMessagesType(GameTime gameTime)
        {
            // check if Queue is empty.
            if (_timedMessages.Count <= 0 && _currentTimedMessage == null)
                return;

            // check if need to Dequeue new TimedMessage
            if (_currentTimedMessage == null)
            {
                _currentTimedMessage = _timedMessages.Dequeue();
            }

            // reduce timer by gametime.
            var multiTimedMessages = _currentTimedMessage.Value;
            multiTimedMessages.DisplayTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            // Draw current Message.
            DrawSimpleMessageType(multiTimedMessages.SbMessageToDisplay);
            _currentTimedMessage = multiTimedMessages;

            // check if timer is up.
            if (multiTimedMessages.DisplayTime <= 0)
            {
                // check if event needs to be fired off?
                if (multiTimedMessages.TriggerEvent && MessageDisplayed != null)
                    MessageDisplayed(this, EventArgs.Empty);

                _currentTimedMessage = null;
            }
        }

        /// <summary>
        /// Draws the simple 1 line message type to screen
        /// </summary>
        /// <param name="sbMessageToDisplay">Message to display</param>        
        private void DrawSimpleMessageType(StringBuilder sbMessageToDisplay)
        {
            // Draw Message Background   
            // 3rd param was = TextureRectSize
            /*SpriteBatch.Draw(BackgroundTexture, IFDTileLocationP, null, Color.White
                   , 0, Vector2.Zero, 1, SpriteEffects.None, 1); // Last Parameter, 1 = back.*/

            // 2/22/2009: Updated to use Origin property.
            // Draw Message
            var tmpRectSize = Vector2.Zero;
            tmpRectSize.X = IFDTileLocation.X + 5; tmpRectSize.Y = IFDTileLocation.Y + 2;
            SpriteBatch.DrawString(MessageFont, sbMessageToDisplay, tmpRectSize, _fontColor,
                0, _messageOrigin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front.      

        }

        // 9/25/2008
        /// <summary>
        /// Draws the <see cref="SceneItem"/> owner description tag message type to screen
        /// </summary>        
        private static void DrawTagItemMessageType(ref MessageTagDescription messageTag, SpriteFont titleFont, SpriteFont descFont,
                                                    SpriteFont typeFont, Texture2D messageBackground, ref Rectangle backgroundTextureRectSize
                                                    , ref Vector2 ifdTileLocation)
        {
            // Line Position / Spacing
            var tmpRectSize = Vector2.Zero;
            var origin = Vector2.Zero;
            var linePosition = (int)ifdTileLocation.Y;
            var indent = (int)ifdTileLocation.X + 15;
            const int lineSpace = 20;
            const int lineIndent = 2;
            var totalHeight = 0;

            // Draw Title
            tmpRectSize.X = indent; tmpRectSize.Y = linePosition;
            SpriteBatch.DrawString(titleFont, messageTag.SbTitle, tmpRectSize, Color.White,
                0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front. 

            // Draw Cost
            linePosition += lineSpace + lineIndent; totalHeight += lineSpace + lineIndent;
            tmpRectSize.X = indent; tmpRectSize.Y = linePosition;
            SpriteBatch.DrawString(titleFont, messageTag.SbCost, tmpRectSize, Color.OrangeRed,
                0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front. 

            // Draw Requirements, if any.
            if (messageTag.Reqs != null)
            {
                linePosition += lineSpace + lineIndent; totalHeight += lineSpace + lineIndent;
                tmpRectSize.X = indent; tmpRectSize.Y = linePosition;
                SpriteBatch.DrawString(typeFont, "Requires:", tmpRectSize, Color.Yellow,
                    0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front.  

                for (var i = 0; i < messageTag.Reqs.Count; i++)
                {
                    linePosition += lineSpace; totalHeight += lineSpace;
                    tmpRectSize.X = indent + 5; tmpRectSize.Y = linePosition;
                    SpriteBatch.DrawString(typeFont, messageTag.Reqs[i], tmpRectSize, Color.Yellow,
                        0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front.     
                }

            }

            // Draw Type
            linePosition += lineSpace + lineIndent; totalHeight += lineSpace + lineIndent;
            tmpRectSize.X = indent; tmpRectSize.Y = linePosition;
            SpriteBatch.DrawString(typeFont, messageTag.SbType, tmpRectSize, Color.Orange,
                0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front.   

            // Draw Description
            linePosition += lineIndent;
            for (var i = 0; i < messageTag.Description.Count; i++)
            {
                linePosition += lineSpace; totalHeight += lineSpace;
                tmpRectSize.X = indent; tmpRectSize.Y = linePosition;
                SpriteBatch.DrawString(descFont, messageTag.Description[i], tmpRectSize, Color.Silver,
                    0, origin, 1, SpriteEffects.None, 0);  // Last Parameter, 0 = front.   
            }

            // Draw Message Background Last, so we can adjust the height.
            backgroundTextureRectSize.Height = 40 + totalHeight; // 40 = 20 for top and 20 for bottom.
            SpriteBatch.Draw(messageBackground, ifdTileLocation, backgroundTextureRectSize, Color.White
                   , 0, origin, 1, SpriteEffects.None, 1); // Last Parameter, 1 = back.

            // Update height for RenderBackgroundTexture draw.
            backgroundTextureRectSize.Height = 40 + totalHeight; // 40 = 20 for top and 20 for bottom.
            // Draw Message Background Last, so we can adjust the height.
            SpriteBatch.Draw(messageBackground, ifdTileLocation, backgroundTextureRectSize, Color.White
                   , 0, origin, 1, SpriteEffects.None, 1); // Last Parameter, 1 = back.

        } 

        // 3/5/2011 - Override Background Texture
        ///<summary>
        /// Draws the IFDTile background, if any.
        ///</summary>
        public override void RenderBackgroundTexture()
        {
            base.RenderBackgroundTexture();
            
        }

        // 1/15/2010
        /// <summary>
        /// Adds new MultiTimed messages to be displayed into the internal queue.
        /// </summary>
        /// <param name="messageToDisplay">Message to display</param>
        /// <param name="messageTimer">Amount of time to display the message</param>
        /// <param name="triggerEvent">Should trigger event, to signal message displayed?</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="messageToDisplay"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="messageTimer"/> is less than zero.</exception>
        public void AddMultiTimedMessageToQueue(string messageToDisplay, float messageTimer, bool triggerEvent)
        {
            // Make sure Message not empty.
            if (string.IsNullOrEmpty(messageToDisplay))
                throw new ArgumentNullException("messageToDisplay", @"Message to display CAN NOT be NULL!");

            // Make sure timer value is greater than 0
            if (Math.Abs(messageTimer - 0) < float.Epsilon)
                throw new ArgumentOutOfRangeException("messageTimer", @"Timer value MUST be greater than zero!");

            // create new Struct and add to Queue.
            var newMultiTimedMessage = new MultiTimedMessages
                                           {
                                               DisplayTime = messageTimer,
                                               SbMessageToDisplay = new StringBuilder(messageToDisplay),
                                               TriggerEvent = triggerEvent
                                           };
            // Add to internal Queue
            _timedMessages.Enqueue(newMultiTimedMessage);

        }

        // 6/14/2012
        /// <summary>
        /// Clears out all messages from the Multi-Timed message queue.
        /// </summary>
        public void ClearMultiTimedMessagesInQueue()
        {
            if (_timedMessages == null)
                return;
            _timedMessages.Clear();
        }

        /// <summary>
        ///  Disposes of unmanaged resources.
        /// </summary>
        /// <param name="finalDispose">Is this final dispose?</param>
        public override void Dispose(bool finalDispose)
        {
            if (finalDispose)
            {
                if (BackgroundTexture != null)
                    BackgroundTexture.Dispose();

                // 1/15/2010
                if (_timedMessages != null)
                    _timedMessages.Clear();

                _parentTile = null;
                MessageFont = null;
                BackgroundTexture = null;
                _titleFont = null;
                _descFont = null;
                _typeFont = null;
            }

            base.Dispose(finalDispose);

        }
       
    }
}
