#region File Description
//-----------------------------------------------------------------------------
// SkyDome.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using ImageNexus.BenScharbach.TWEngine.BeginGame;
using ImageNexus.BenScharbach.TWEngine.BeginGame.Enums;
using ImageNexus.BenScharbach.TWEngine.GameCamera;
using ImageNexus.BenScharbach.TWEngine.SkyDomes.Enums;
using ImageNexus.BenScharbach.TWEngine.SkyDomes.Structs;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ImageNexus.BenScharbach.TWEngine.SkyDomes
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.SkyDomes"/> namespace contains the classes
    /// which make up the entire <see cref="TWEngine.SkyDomes"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    
    
    /// <summary>
    /// The <see cref="SkyDome"/> class is used to create the illusion of the sky surrounding the game map.  
    /// </summary>
    public class SkyDome : DrawableGameComponent
    {
        // 6/1/2012
        private static SkyDomeTextureEnum _skyboxTextureToUse = SkyDomeTextureEnum.DarkMountainsSkyBox;

        private readonly GraphicsDevice _device;
        private VertexBuffer _skyboxVertexBuffer;
        private Effect _skyboxEffect;

        // XNA 4.0 Updates - New DepthStencilState
        private readonly DepthStencilState _depthStencilStateOn;
        private readonly DepthStencilState _depthStencilStateOff;

        private ContentManager _contentManager;

        private bool _contentLoaded;

        #region Properties
        
        // 6/1/2012
        /// <summary>
        /// Gets or sets the <see cref="SkyDomeTextureEnum"/> background texture.
        /// </summary>
        public SkyDomeTextureEnum SkyboxTextureToUse
        {
            get { return _skyboxTextureToUse; }
            set
            {
                _skyboxTextureToUse = value;

                // load new texture
                TextureCube textureCube;
                LoadTextureCube(value, out textureCube);
                SkyboxTextureCube = textureCube;

                // set texture into effect.
                if (_skyboxEffect != null)
                    _skyboxEffect.Parameters["xCubeTexture"].SetValue(SkyboxTextureCube);
            }
        }

        // 12/14/2009
        /// <summary>
        /// Returns a reference to <see cref="TextureCube"/> instance.
        /// </summary>
        public static TextureCube SkyboxTextureCube { get; private set; }

        #endregion

        /// <summary>
        /// The <see cref="SkyDome"/> constructor, which initializes the <see cref="VertexDeclaration"/> and loads
        /// the proper terrain textures, depending on the <see cref="TerrainTextures"/> Enum.
        /// </summary>
        /// <param name="game"></param>
        public SkyDome(Game game)
            : base(game)
        {
            _device = Game.GraphicsDevice;

            // XNA 4.0 Updates - Custom VertexDeclaration are now done within the Structure; VertexPosition in this case.
            //_vertexDeclaration = new VertexDeclaration(_device, VertexPosition.VertexElements); // 10/27/2009

            // XNA 4.0 Updates - Replaced RenderState with 4 new states.
            _depthStencilStateOn = new DepthStencilState { DepthBufferEnable = true, DepthBufferWriteEnable = false };
            _depthStencilStateOff = new DepthStencilState { DepthBufferEnable = false, DepthBufferWriteEnable = true };
           
            DrawOrder = 99;
        }

        // 11/2/2009
        /// <summary>
        /// Creates the <see cref="SkyDome"/> <see cref="VertexBuffer"/> collection.
        /// </summary>
        public override void Initialize()
        {
            // 1/6/2010: Note: Added checks below, since this Init method is called twice at the beg of the game!
            //                 Once by the XNa framework, and again manually in the TerrainScreen class.  2nd time
            //                 is needed for level reloads, since the Init is not called again.

            // Create the SkyBox VB
            if (_skyboxVertexBuffer == null) // 1/6/2010
                CreateSkyboxVertexBuffer();

            // 11/17/2009
            if (!_contentLoaded) // 1/6/2010
                LoadContent();
            
        }

        // 11/17/2009
        /// <summary>
        /// Loads the <see cref="SkyDome"/> textures and effect.
        /// </summary>
        protected override void LoadContent()
        {
            // 6/1/2012 - Loads the skydome based on the enum setting.
            var texturePath = GetSkydomeTexturePath(SkyboxTextureToUse);

            // Create ContentManager
            _contentManager = new ContentManager(Game.Services);
            SkyboxTextureCube = _contentManager.Load<TextureCube>(texturePath); // 10/27/2009
            _skyboxEffect = _contentManager.Load<Effect>(TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\skyboxEffect");

            // set texture into effect.
            if (_skyboxEffect != null) 
                _skyboxEffect.Parameters["xCubeTexture"].SetValue(SkyboxTextureCube);

            // 1/5/2010 - Set Visible true.
            Visible = true;

            // 1/6/2010
            _contentLoaded = true;

            base.LoadContent();
        }

        // 2/9/2010
        /// <summary>
        /// Used to load a <see cref="TextureCube"/>, using
        /// the internal ZipContent provider.
        /// </summary>
        /// <param name="texturePathName">Content location to load <see cref="TextureCube"/></param>
        /// <param name="textureCube">(OUT) Instance of <see cref="TextureCube"/></param>
        public void LoadTextureCube(string texturePathName, out TextureCube textureCube)
        {
            if (string.IsNullOrEmpty(texturePathName))
                texturePathName = TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\DarkMountainsSkyBox";

            textureCube = _contentManager.Load<TextureCube>(texturePathName);

        }

        // 6/1/2012
        /// <summary>
        /// Used to load a <see cref="TextureCube"/>, using
        /// the internal ZipContent provider.
        /// </summary>
        /// <param name="texturePathName"><see cref="SkyDomeTextureEnum"/> type to load.</param>
        /// <param name="textureCube">(OUT) Instance of <see cref="TextureCube"/></param>
        public void LoadTextureCube(SkyDomeTextureEnum texturePathName, out TextureCube textureCube)
        {
            // 6/1/2012
            var texturePath = GetSkydomeTexturePath(texturePathName);
            textureCube = _contentManager.Load<TextureCube>(texturePath);
        }

        // 1/6/2010
        /// <summary>
        /// Unloads the <see cref="SkyDome"/> texture content.
        /// </summary>
        protected override void UnloadContent()
        {
            _contentManager.Unload();
            _contentLoaded = false;

            base.UnloadContent();
        }
        

        /// <summary>
        /// Creates the <see cref="SkyDome"/>  <see cref="VertexBuffer"/> collection.
        /// </summary>
        private void CreateSkyboxVertexBuffer()
        {
            var forwardBottomLeft = new Vector3(-1, -1, -1);
            var forwardBottomRight = new Vector3(1, -1, -1);
            var forwardUpperLeft = new Vector3(-1, 1, -1);
            var forwardUpperRight = new Vector3(1, 1, -1);

            var backBottomLeft = new Vector3(-1, -1, 1);
            var backBottomRight = new Vector3(1, -1, 1);
            var backUpperLeft = new Vector3(-1, 1, 1);
            var backUpperRight = new Vector3(1, 1, 1);

            var vertices = new VertexPosition[36];
            int i = 0;

            //face in front of the camera
            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);

            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperRight);
            vertices[i++] = new VertexPosition(forwardBottomRight);

            //face to the right of the camera
            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i++] = new VertexPosition(forwardUpperRight);
            vertices[i++] = new VertexPosition(backUpperRight);

            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(backBottomRight);

            //face behind camera
            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(backUpperLeft);

            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backBottomRight);
            vertices[i++] = new VertexPosition(backUpperRight);

            //face to the left of the camera
            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(backUpperLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);

            vertices[i++] = new VertexPosition(backBottomLeft);
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(forwardBottomLeft);

            //face above the camera
            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(backUpperLeft);
            vertices[i++] = new VertexPosition(backUpperRight);

            vertices[i++] = new VertexPosition(forwardUpperLeft);
            vertices[i++] = new VertexPosition(backUpperRight);
            vertices[i++] = new VertexPosition(forwardUpperRight);

            //face under the camera
            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(backBottomRight);
            vertices[i++] = new VertexPosition(backBottomLeft);

            vertices[i++] = new VertexPosition(forwardBottomLeft);
            vertices[i++] = new VertexPosition(forwardBottomRight);
            vertices[i] = new VertexPosition(backBottomRight);

            // 1/6/2010 - Make sure any prior VB is disposed of.
            if (_skyboxVertexBuffer != null)
                _skyboxVertexBuffer.Dispose();

            // XNA 4.0 Updates - Include VertexDeclaration into signature now.
            //_skyboxVertexBuffer = new VertexBuffer(_device, vertices.Length * VertexPosition.SizeInBytes, BufferUsage.WriteOnly);
            _skyboxVertexBuffer = new VertexBuffer(_device, typeof(VertexPosition), vertices.Length, BufferUsage.WriteOnly);
            _skyboxVertexBuffer.SetData(vertices);
        }


        /// <summary>
        /// Called when the DrawableGameComponent needs to be drawn. Override this method with component-specific drawing code. Reference page contains links to related conceptual articles.
        /// </summary>
        /// <param name="gameTime">Time passed since the last call to Draw.</param>
        public sealed override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        // 12/14/2009
        /// <summary>
        /// Used to draw the internal <see cref="SkyDome"/>, with 
        /// the given <paramref name="viewMatrix"/> and <paramref name="cameraPosition"/>.
        /// </summary>
        /// <param name="viewMatrix">camera's view <see cref="Matrix"/></param>
        /// <param name="cameraPosition">camera's <see cref="Vector3"/> position</param>
        internal void DrawSkyDome(ref Matrix viewMatrix, ref Vector3 cameraPosition)
        {
            // 1/5/2010 - check visible setting.
            if (!Visible) return;

            // 11/17/09 - Capture the Disposed exception.
            try
            {
                // 11/17/2009
                if (_skyboxEffect == null) return;

                _device.BlendState = BlendState.Opaque;
                _device.DepthStencilState = DepthStencilState.Default;
                _device.RasterizerState = RasterizerState.CullCounterClockwise;

                // Create Skydome translation matrix.
                var position = cameraPosition; // was Camera.CameraPosition
                //position.Y -= 0.2f;
                Matrix skyDomeTranslation;
                Matrix.CreateTranslation(ref position, out skyDomeTranslation);

                // XNA 4.0 Updates - RenderState obsolete; now set with precreated instances.
                //_device.RenderState.DepthBufferWriteEnable = false;
                //_device.RenderState.DepthBufferEnable = true;
                //_device.DepthStencilState = _depthStencilStateOn;

                _skyboxEffect.CurrentTechnique = _skyboxEffect.Techniques["SkyBox"];
                _skyboxEffect.Parameters["xWorld"].SetValue(skyDomeTranslation); // 
                _skyboxEffect.Parameters["xView"].SetValue(viewMatrix);
                _skyboxEffect.Parameters["xProjection"].SetValue(Camera.Projection);

                // XNA 4.0 updates - Begin() and End() obsolete.
                //_skyboxEffect.Begin();

                var count = _skyboxEffect.CurrentTechnique.Passes.Count;
                for (var i = 0; i < count; i++)
                {
                    var pass = _skyboxEffect.CurrentTechnique.Passes[i];

                    // XNA 4.0 updates - Begin() and End() obsolete.
                    pass.Apply();

                    // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
                    //_device.VertexDeclaration = _vertexDeclaration;
                    //_device.Vertices[0].SetSource(_skyboxVertexBuffer, 0, VertexPosition.SizeInBytes);
                    _device.SetVertexBuffer(_skyboxVertexBuffer);

                    _device.DrawPrimitives(PrimitiveType.TriangleList, 0, 12);

                    // XNA 4.0 updates - Begin() and End() obsolete.
                    //pass.End();

                }

                // XNA 4.0 updates - Begin() and End() obsolete.
                //_skyboxEffect.End();

                // XNA 4.0 Updates - RenderState obsolete; now set with precreated instances.
                //_device.RenderState.DepthBufferWriteEnable = true;
                //_device.DepthStencilState = _depthStencilStateOff;

            }
            catch (ObjectDisposedException)
            {
                // then recreate the LoadContent
                LoadContent();
            }
        }

        /// <summary>
        /// Gets the skydome texture string, based on the given <paramref name="skyboxTextureToUse"/>.
        /// </summary>
        /// <param name="skyboxTextureToUse"><see cref="SkyDomeTextureEnum"/> texture to use.</param>
        /// <returns>Skydome texture string.</returns>
        private static string GetSkydomeTexturePath(SkyDomeTextureEnum skyboxTextureToUse)
        {
            // 6/1/2012 - Load the Skydome texture
            string texturePath;
            switch (skyboxTextureToUse)
            {
                case SkyDomeTextureEnum.GreenWhispSkyBox:
                    texturePath = TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\GreenWhispSkyBox";
                    break;
                case SkyDomeTextureEnum.DustDesertSkyBox:
                    texturePath = TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\DustDesertSkyBox";
                    break;
                case SkyDomeTextureEnum.AmberSeriesSkyBox:
                    texturePath = TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\AmberSeriesSkyBox";
                    break;
                case SkyDomeTextureEnum.DarkMountainsSkyBox:
                    texturePath = TemporalWars3DEngine.ContentTexturesLoc + @"\SkyDome\DarkMountainsSkyBox";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return texturePath;
        }
        
    }
}
