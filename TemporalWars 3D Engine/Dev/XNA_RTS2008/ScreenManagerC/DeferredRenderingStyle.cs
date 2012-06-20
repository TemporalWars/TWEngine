#region File Description
//-----------------------------------------------------------------------------
// DeferredRenderingStyle.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ScreenTextDisplayer.ScreenText;
using SimpleQuadDrawer;
using TWEngine.GameCamera;
using TWEngine.InstancedModels;
using TWEngine.PostProcessEffects.BloomEffect;
using TWEngine.PostProcessEffects.GlowEffect;
using TWEngine.ScreenManagerC.Enums;
using TWEngine.Shadows;
using TWEngine.Shadows.Enums;
using TWEngine.SkyDomes;
using TWEngine.Utilities;

namespace TWEngine.ScreenManagerC
{
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.ScreenManagerC"/> namespace contains the classes
    /// which make up the entire <see cref="ScreenManagerC"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    // 4/28/2010 - Created from methods in ScreenManager.
    /// <summary>
    /// The <see cref="DeferredRenderingStyle"/> class is used to draw the game, using
    /// the 'DeferredRendering' method, which draws content onto 3 render targets first, then
    /// applies the postprocess effects, finally drawing the final image to the backbuffer.
    /// </summary>
    class DeferredRenderingStyle
    {
        private static Game _gameInstance;
        private static IList<GameScreen> _screens;

        //Effect effect1Scene;
        private static Effect _effect2Lights;
        private static Effect _effect3Final;

        private const int NumberOfLights = 6;
        private static SpotLight[] _spotLights;

        private ScreenTextItem _screenText1;
        private ScreenTextItem _screenText2;
        private ScreenTextItem _screenText3;
        private ScreenTextItem _screenText4;
        private Vector2 _halfPixel; // 3/19/2009

        private Vector4 _offsetSmTltr; // 3/19/2009 (Top-Left & Top-Right Quads)
        private Vector4 _offsetSmByTltr; // 3/19/2009
        private Vector4 _offsetSmBlbr; // 3/19/2009 (Bottom-Left & Bottom-Right Quads)
        private Vector4 _offsetSmByBlbr; // 3/19/2009
       
        private readonly Vector2[] _pcfSamples = new Vector2[9]; // PCF Array Samples (For Deferred PCF Sampling method only!)  

        private static VertexPositionTexture[] _fsVertices;
        private static VertexDeclaration _fsVertexDeclaration;    

        private static EffectParameter _shadingMapEp;
        private static EffectParameter _normalMapEp;
        private static EffectParameter _depthMapEp;
        private static EffectParameter _shadowMapEp;
        private static EffectParameter _lightPosEp;
        private static EffectParameter _lightStrengthEp;
        private static EffectParameter _coneDirectionEp;
        private static EffectParameter _coneAngleEp;
        private static EffectParameter _coneDecayEp;

        private static RenderTarget2D _colorTarget;
        private static RenderTarget2D _normalTarget;
        private static RenderTarget2D _depthTarget;
        private static RenderTarget2D _shadingTarget;
        private static RenderTarget2D _shadowTarget; // sm
        private static Texture2D _colorMap;
        private static Texture2D _normalMap;
        private static Texture2D _depthMap;
        private static Texture2D _shadingMap;
        private static Texture2D _shadowMap; // sm
        private static Texture2D _blackImage; // sm

        struct SpotLight
        {
            public Vector3 Position;
            public Vector3 Target;
            public float Strength;
            public Vector3 Direction;
            public float ConeAngle;
            public float ConeDecay;
            public Matrix ViewMatrix;
            public Matrix ProjectionMatrix;
            public Matrix ViewProjMatrix;
        }

        public DeferredRenderingStyle(Game game, IList<GameScreen> screens, ContentManager content, int width, int height)
        {
            _gameInstance = game;
            _screens = screens; // Save reference to ScreenManager's screen collection.

            // 4/28/2010 - Initialize DR.
            InitalizeDeferredRendering(content, width, height);
        }

        // 7/17/2009
        /// <summary>
        /// Initializes Deferred Rendering mode, by creating the required render targets and effect params.
        /// </summary>
        /// <param name="content"><see cref="ContentManager"/> instance</param>
        /// <param name="width">Width value</param>
        /// <param name="height">Height value</param>
        private void InitalizeDeferredRendering(ContentManager content, int width, int height)
        {
            // 4/28/2010 - Cache
            var graphicsDevice = _gameInstance.GraphicsDevice;
            if (graphicsDevice == null) return;

            // XNA 4.0 Updates - Signature update for RenderTarget2D
            #region OLDcode
            /*_colorTarget = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);
            _normalTarget = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);
            _depthTarget = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Single,
                graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);
            _shadingTarget = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Color,
                graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);
            _shadowTarget = new RenderTarget2D(graphicsDevice, width, height, 1, SurfaceFormat.Vector2,
                graphicsDevice.PresentationParameters.MultiSampleType, graphicsDevice.PresentationParameters.MultiSampleQuality);*/
            #endregion
            _colorTarget = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _normalTarget = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _depthTarget = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            _shadingTarget = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            _shadowTarget = new RenderTarget2D(graphicsDevice, width, height, true, SurfaceFormat.Vector2, DepthFormat.Depth24Stencil8);

            // 9/20/2010 - XNA 4.0 Updates
            _blackImage = new Texture2D(graphicsDevice, width, height, true, SurfaceFormat.Color);
            
            //effect1Scene = content.Load<Effect>("content/Shaders/Deferred1Scene");
            _effect2Lights = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Deferred2Lights");
            _effect3Final = content.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\Deferred3Final");

            // EffectParameters
            _shadingMapEp = _effect2Lights.Parameters["xPreviousShadingContents"];
            _normalMapEp = _effect2Lights.Parameters["xNormalMap"];
            _depthMapEp = _effect2Lights.Parameters["xDepthMap"];
            _shadowMapEp = _effect2Lights.Parameters["xShadowMap"];
            _lightPosEp = _effect2Lights.Parameters["xLightPosition"];
            _lightStrengthEp = _effect2Lights.Parameters["xLightStrength"];
            _coneDirectionEp = _effect2Lights.Parameters["xConeDirection"];
            _coneAngleEp = _effect2Lights.Parameters["xConeAngle"];
            _coneDecayEp = _effect2Lights.Parameters["xConeDecay"];

            // 3/19/2009 - Setup PCF Sampling Array
            {
                var texelSizeW = 1.5f / width;
                var texelSizeH = 1.5f / height;

                _pcfSamples[0] = new Vector2(0.0f, 0.0f);
                _pcfSamples[1] = new Vector2(-texelSizeW, 0.0f);
                _pcfSamples[2] = new Vector2(texelSizeW, 0.0f);
                _pcfSamples[3] = new Vector2(0.0f, -texelSizeH);
                _pcfSamples[4] = new Vector2(-texelSizeW, -texelSizeH);
                _pcfSamples[5] = new Vector2(texelSizeW, -texelSizeH);
                _pcfSamples[6] = new Vector2(0.0f, texelSizeH);
                _pcfSamples[7] = new Vector2(-texelSizeW, texelSizeH);
                _pcfSamples[8] = new Vector2(texelSizeW, texelSizeH);

                _effect2Lights.Parameters["PCFSamples"].SetValue(_pcfSamples);
            }

            // 3/19/2009 - Set HalfPixel Alignment
            var presentationParameters = graphicsDevice.PresentationParameters; // 4/28/2010 - Cache
            _halfPixel.X = 0.5f / presentationParameters.BackBufferWidth;
            _halfPixel.Y = 0.5f / presentationParameters.BackBufferHeight;
            _effect2Lights.Parameters["xHalfPixel"].SetValue(_halfPixel);
            _effect3Final.Parameters["xHalfPixel"].SetValue(_halfPixel);

            // 3/19/2009 - Set OffsetPixels for the Shadow Mapping
            _offsetSmByTltr = Vector4.Zero;
            _offsetSmByTltr.X = 0.5f; _offsetSmByTltr.Y = 0.5f; _offsetSmByTltr.Z = 0.5f; _offsetSmByTltr.W = 0.5f;
            _offsetSmTltr = Vector4.Zero;
            _offsetSmTltr.X = _offsetSmByTltr.X / presentationParameters.BackBufferWidth;
            _offsetSmTltr.Y = _offsetSmByTltr.Y / presentationParameters.BackBufferHeight;
            _offsetSmTltr.Z = _offsetSmByTltr.W / presentationParameters.BackBufferWidth;
            _offsetSmTltr.W = _offsetSmByTltr.Z / presentationParameters.BackBufferHeight;
            _effect2Lights.Parameters["xOffsetSM_TLTR"].SetValue(_offsetSmTltr);
            _offsetSmByBlbr = Vector4.Zero;
            _offsetSmByBlbr.X = 0.5f; _offsetSmByBlbr.Y = 0.5f; _offsetSmByBlbr.Z = 0.5f; _offsetSmByBlbr.W = 0.5f;
            _offsetSmBlbr = Vector4.Zero;
            _offsetSmBlbr.X = _offsetSmByBlbr.X / presentationParameters.BackBufferWidth;
            _offsetSmBlbr.Y = _offsetSmByBlbr.Y / presentationParameters.BackBufferHeight;
            _offsetSmBlbr.Z = _offsetSmByBlbr.W / presentationParameters.BackBufferWidth;
            _offsetSmBlbr.W = _offsetSmByBlbr.Z / presentationParameters.BackBufferHeight;
            _effect2Lights.Parameters["xOffsetSM_BLBR"].SetValue(_offsetSmBlbr);

            _spotLights = new SpotLight[NumberOfLights];

            const float coneAngle = MathHelper.PiOver4;
            var lightPosition = new Vector3(0, 2000, 0);
            var lightTarget = new Vector3(2500, 10, 2500);
            var lightDirection = lightTarget - lightPosition;
            lightDirection.Normalize();
            _spotLights[0].Position = lightPosition; // LightPosition
            _spotLights[0].Target = lightTarget; // LightTarget
            _spotLights[0].Strength = 1.5f;
            _spotLights[0].Direction = lightDirection; // LightDirection
            _spotLights[0].ConeAngle = (float)Math.Cos(coneAngle);
            _spotLights[0].ConeDecay = 1.5f;
            _spotLights[0].ViewMatrix = Matrix.CreateLookAt(lightPosition, lightPosition + lightDirection, Vector3.Up);
            //_spotLights[0].ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(coneAngle * 2.0f, 1.0f, 0.5f, 10000.0f);
            _spotLights[0].ProjectionMatrix = Matrix.CreateOrthographic(8000, 5000, 0, 10000);
            _spotLights[0].ViewProjMatrix = _spotLights[0].ViewMatrix * _spotLights[0].ProjectionMatrix;

            lightPosition = new Vector3(5000, 2000, 5000);
            lightTarget = new Vector3(2500, 0, 2500);
            lightDirection = lightTarget - lightPosition;
            lightDirection.Normalize();
            _spotLights[1].Position = lightPosition; // LightPosition
            _spotLights[1].Target = lightTarget; // LightTarget
            _spotLights[1].Strength = 1.1f;
            _spotLights[1].Direction = lightDirection; // LightDirection
            _spotLights[1].ConeAngle = (float)Math.Cos(coneAngle);
            _spotLights[1].ConeDecay = 1.5f;
            _spotLights[1].ViewMatrix = Matrix.CreateLookAt(lightPosition, lightPosition + lightDirection, Vector3.Up);
            //_spotLights[1].ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(coneAngle * 2.0f, 1.0f, 0.5f, 10000.0f);
            _spotLights[0].ProjectionMatrix = Matrix.CreateOrthographic(8000, 5000, 0, 10000);
            _spotLights[1].ViewProjMatrix = _spotLights[1].ViewMatrix * _spotLights[1].ProjectionMatrix;

            // Init ScreenText Class
            ScreenTextManager.AddNewScreenTextItem(String.Empty, new Vector2(550, 530), Color.Black, out _screenText1);
            ScreenTextManager.AddNewScreenTextItem(String.Empty, new Vector2(550, 550), Color.Black, out _screenText2);
            ScreenTextManager.AddNewScreenTextItem(String.Empty, new Vector2(550, 570), Color.Black, out _screenText3);
            ScreenTextManager.AddNewScreenTextItem(String.Empty, new Vector2(550, 590), Color.Red, out _screenText4);

            InitFullscreenVertices();
        }

        // 3/13/2009 - For Deferred Rendering
        private static void InitFullscreenVertices()
        {
            _fsVertices = new VertexPositionTexture[4];
            var i = 0;
            _fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, 1, 0f), new Vector2(0, 0));
            _fsVertices[i++] = new VertexPositionTexture(new Vector3(1, 1, 0f), new Vector2(1, 0));
            _fsVertices[i++] = new VertexPositionTexture(new Vector3(-1, -1, 0f), new Vector2(0, 1));
            _fsVertices[i] = new VertexPositionTexture(new Vector3(1, -1, 0f), new Vector2(1, 1));

            // XNA 4.0 Updates - VertexDeclaration Only set at creation time.
            //_fsVertexDeclaration = new VertexDeclaration(_gameInstance.GraphicsDevice, VertexPositionTexture.VertexElements);
        }    


        // 3/13/2009 - DeferredRendering Method.
        /// <summary>
        /// Draw method using the deferred rendering style.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <param name="useBloom">Use <see cref="Bloom"/> PostProcess effect?</param>
        /// <param name="useGlow">Use <see cref="Glow"/> PostProcess effect?</param>
        /// <param name="useSkyBox">Use <see cref="SkyDome"/>?</param>
        public static void DrawWithDeferredRendering(GameTime gameTime,  bool useBloom, bool useGlow, bool useSkyBox)
        {
            //render color, normal and depth into 3 render targets
            RenderSceneTo3RenderTargets(gameTime);

            //Add lighting contribution of each light onto _shadingMap
            GenerateShadingMap(gameTime, out _shadingMap);

            // 3/17/2009 - Use BlurGlow Effect?            
            // if (useBlurGlowEffect)
            //DrawWithBlurGlowRender_Deferred(gameTime);

            //Combine base color map and shading map
            CombineColorAndShading();

            /*if (DebugValues)
            {
                // DEBUG: Write data to screen
                {
                    // lightPosition Attributes      
                    Vector3 lightPos = _spotLights[0].Position; Vector3 lightTar = _spotLights[0].Target;
                    _screenText1.DrawText = String.Format("Light Pos: {0},{1},{2}", lightPos.X.ToString(CultureInfo.CurrentCulture), lightPos.Y.ToString(CultureInfo.CurrentCulture), lightPos.Z.ToString(CultureInfo.CurrentCulture));
                    // lightTarget Attributes                   
                    _screenText2.DrawText = String.Format("Light Tar: {0},{1},{2}", lightTar.X.ToString(CultureInfo.CurrentCulture), lightTar.Y.ToString(CultureInfo.CurrentCulture), lightTar.Z.ToString(CultureInfo.CurrentCulture));
                    // ShadowOffset Attributes    
                    switch (_debugIsFor)
                    {                        
                        case DebugIsFor.shadowTexelOffset_TopLeft:
                            _screenText3.DrawText = String.Format("Shadow TopLeft Offset: {0},{1}", _offsetSmByTltr.X.ToString(CultureInfo.CurrentCulture), _offsetSmByTltr.Y.ToString(CultureInfo.CurrentCulture)); 
                            break;
                        case DebugIsFor.shadowTexelOffset_TopRight:
                            _screenText3.DrawText = String.Format("Shadow TopRight Offset: {0},{1}", _offsetSmByTltr.Z.ToString(CultureInfo.CurrentCulture), _offsetSmByTltr.W.ToString(CultureInfo.CurrentCulture)); 
                            break;
                        case DebugIsFor.shadowTexelOffset_BottomLeft:
                            _screenText3.DrawText = String.Format("Shadow BottomLeft Offset: {0},{1}", _offsetSmByBlbr.X.ToString(CultureInfo.CurrentCulture), _offsetSmByBlbr.Y.ToString(CultureInfo.CurrentCulture)); 
                            break;
                        case DebugIsFor.shadowTexelOffset_BottomRight:
                            _screenText3.DrawText = String.Format("Shadow BottomRight Offset: {0},{1}", _offsetSmByBlbr.Z.ToString(CultureInfo.CurrentCulture), _offsetSmByBlbr.W.ToString(CultureInfo.CurrentCulture)); 
                            break;
                        default:
                            break;
                    }                    
                    // DEBUG: Show Value of DebugIsFor ENUM                    
                    _screenText4.DrawText = _debugIsFor.ToString(); // causes boxing.
                }
            }*/


        }

        private static void RenderSceneTo3RenderTargets(GameTime gameTime)
        {

            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //bind render targets to outputs of pixel shaders
            /*graphicsDevice.SetRenderTarget(0, _colorTarget);
            graphicsDevice.SetRenderTarget(1, _normalTarget);
            graphicsDevice.SetRenderTarget(2, _depthTarget);*/
            graphicsDevice.SetRenderTargets(_colorTarget, _normalTarget, _depthTarget);

            // if (useBlurGlowEffect)
            //GraphicsDevice.SetRenderTarget(3, _colorRtIllumination);  // Glow Effect

            //clear all render targets
            graphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

            //RenderScene(effect1Scene);
            var gameScreens = _screens; // 5/23/2010 - Cache
            var count = gameScreens.Count; // 4/28/2010
            for (var i = 0; i < count; i++)
            {
                // 5/23/2010 - Cache
                var gameScreen = gameScreens[i];
                if (gameScreen == null) continue;

                if (gameScreen.ScreenState == ScreenState.Hidden) continue;

                gameScreen.Draw3D(gameTime);
                gameScreen.Draw3DSceneryItems(gameTime); // 8/1/2009
                gameScreen.Draw3DSelectables(gameTime);
                // 3/19/2009 - Draw AlphaItems Now, like Trees for example.
                //gameScreen.Draw3DAlpha(gameTime);

                // 3/19/2009 - RenderState Resets required, since AlphaDraw screws up the Settings!
                //             Without these Settings, the screen could blank out during draw sessions.
                /*graphicsDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
                graphicsDevice.RenderState.AlphaBlendEnable = false;
                graphicsDevice.RenderState.AlphaTestEnable = false;
                graphicsDevice.RenderState.DepthBufferEnable = true;
                graphicsDevice.RenderState.DepthBufferWriteEnable = true;*/

            }

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //de-activate render targets to resolve them
            /*graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(1, null);
            graphicsDevice.SetRenderTarget(2, null);*/
            graphicsDevice.SetRenderTarget(null);

            //if (useBlurGlowEffect)
            //GraphicsDevice.SetRenderTarget(3, null); // Glow Effect 

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //copy contents of render targets into texture
            /*_colorMap = _colorTarget.GetTexture();
            _normalMap = _normalTarget.GetTexture();
            _depthMap = _depthTarget.GetTexture();*/
            _colorMap = _colorTarget;
            _normalMap = _normalTarget;
            _depthMap = _depthTarget;

        }


        private static void GenerateShadingMap(GameTime gameTime, out Texture2D shadingTexture)
        {

            _shadingMap = _blackImage;

            //for (int i = 0; i < NumberOfLights; i++)
            for (var i = 0; i < 1; i++)
            {
                RenderShadowMap(gameTime, _spotLights[i]);
                AddLight(_spotLights[i]);
            }

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //shadingTexture = _shadingTarget.GetTexture();
            shadingTexture = _shadingTarget;
        }

        private static void RenderShadowMap(GameTime gameTime, SpotLight spotLight)
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, _shadowTarget);
            graphicsDevice.SetRenderTarget(_shadowTarget);

            InstancedItem.DrawForShadowMap_AllItems(gameTime, ref spotLight.ViewMatrix, ref spotLight.ProjectionMatrix, true);

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //_shadowMap = _shadowTarget.GetTexture()
            _shadowMap = _shadowTarget;

        }

        // 4/28/2010
        public static void SetShadowMapSettings(bool isVisible, ref Matrix lightView, ref Matrix lightProj,
                                         ref Matrix lightViewStatic, ref Matrix lightProjStatic, ref Vector3 lightPosition)
        {
            // 4/2/2009 - Calc the Light/View projection
            Matrix lightViewProj;
            Matrix.Multiply(ref lightView, ref lightProj, out lightViewProj);

            // 7/10/2009 - Calc the Light/View projection for STATIC
            Matrix lightViewProjStatic;
            Matrix.Multiply(ref lightViewStatic, ref lightProjStatic, out lightViewProjStatic);

            _lightPosEp.SetValue(lightPosition);
            _effect2Lights.Parameters["xLightViewProjection"].SetValue(lightViewProj);
        }

        private static void AddLight(SpotLight spotLight)
        {
            var graphicsDevice = TemporalWars3DEngine.GameInstance.GraphicsDevice;

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, _shadingTarget);
            graphicsDevice.SetRenderTarget(_shadingTarget);

            graphicsDevice.Clear(ClearOptions.Target, Color.Black, 1, 0);

            _effect2Lights.CurrentTechnique = _effect2Lights.Techniques["DeferredSpotLight"];

            _shadingMapEp.SetValue(_shadingMap);
            _normalMapEp.SetValue(_normalMap);
            _depthMapEp.SetValue(_depthMap);
            _shadowMapEp.SetValue(_shadowMap);
            
            _lightStrengthEp.SetValue(spotLight.Strength);
            _coneDirectionEp.SetValue(spotLight.Direction);
            _coneAngleEp.SetValue(spotLight.ConeAngle);
            _coneDecayEp.SetValue(spotLight.ConeDecay);

            //Matrix viewProjInv = Matrix.Invert(camera.View * camera.Projection);   
            Matrix tmpViewProj; Matrix viewProjInv;
            var tmpView = Camera.View;
            var tmpProj = Camera.Projection;
            Matrix.Multiply(ref tmpView, ref tmpProj, out tmpViewProj);
            Matrix.Invert(ref tmpViewProj, out viewProjInv);

            _effect2Lights.Parameters["xView"].SetValue(Camera.View); // 3/15/2009
            _effect2Lights.Parameters["xWorld"].SetValue(Matrix.Identity); // 3/16/2009
            _effect2Lights.Parameters["xViewProjectionInv"].SetValue(viewProjInv);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_effect2Lights.Begin();
            _effect2Lights.CurrentTechnique.Passes[0].Apply();

            SimpleQuadDraw.DrawSimpleQuad(_gameInstance.GraphicsDevice);

            /*graphicsDevice.VertexDeclaration = _fsVertexDeclaration;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _fsVertices, 0, 2);*/

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_effect2Lights.CurrentTechnique.Passes[0].End();
            //_effect2Lights.End();

            // XNA 4.0 Updates - Index no longer required on SetRenderTarget.
            //graphicsDevice.SetRenderTarget(0, null);
            graphicsDevice.SetRenderTarget(null);

            // XNA 4.0 Updates - GetTexture obsolete; RenderTarget inherit from Texture.
            //_shadingMap = _shadingTarget.GetTexture();
            _shadingMap = _shadingTarget;
        }


        private static void CombineColorAndShading()
        {
            _effect3Final.CurrentTechnique = _effect3Final.Techniques["CombineColorAndShading"];
            _effect3Final.Parameters["xColorMap"].SetValue(_colorMap);
            _effect3Final.Parameters["xShadingMap"].SetValue(_shadingMap);
            _effect3Final.Parameters["xUseGlow"].SetValue(false);
            //if (useBlurGlowEffect)
            //_effect3Final.Parameters["xGlowMap"].SetValue(glowRT2.GetTexture()); // 3/17/2009
            //_effect3Final.Parameters["xAmbient"].SetValue(0.4f);

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_effect3Final.Begin();
            _effect3Final.CurrentTechnique.Passes[0].Apply();

            SimpleQuadDraw.DrawSimpleQuad(_gameInstance.GraphicsDevice);

            /*var graphicsDevice = _gameInstance.GraphicsDevice;
            graphicsDevice.VertexDeclaration = _fsVertexDeclaration;
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _fsVertices, 0, 2);*/

            // XNA 4.0 updates - Begin() and End() obsolete.
            //_effect3Final.CurrentTechnique.Passes[0].End();
            //_effect3Final.End();
        }


        // 3/17/2009
        /// <summary>
        /// Renders the scene using the BlurGlow PostProcesing Effect for
        /// Deferred Rendering method.
        /// </summary>
        // ReSharper disable UnusedMember.Local
        private static void DrawWithBlurGlowRender_Deferred()
        // ReSharper restore UnusedMember.Local
        {

            //_glowEffect.PostProcess(_colorRtEffects.GetTexture(), _glowRt);
        }

        // 3/13/2009 -
        private static KeyboardState _keyState;
        private static KeyboardState _oldKeyState;
        private DebugIsFor _debugIsFor = DebugIsFor.LightPosition; // DEBUG which Attribute - Used to change an attribute using number keypad.

        // 3/13/2009
        /// <summary>
        /// Handles Input for Debugging.
        /// </summary>
        public void HandleInputForDebugging(InputState inputState)
        {
            // Get Current Keystate
            _keyState = Keyboard.GetState();

            // DEBUG: Test moving various attributes related to the _spotLights
            {
                // DEBUG: Changes to which attribute we want to affect
                if (_keyState.IsKeyDown(Keys.F12))
                {
                    if (!_oldKeyState.IsKeyDown(Keys.F12))
                    {
                        _debugIsFor++;

                        if (_debugIsFor > DebugIsFor.ShadowTexelOffsetBottomRight)
                            _debugIsFor = DebugIsFor.LightPosition;
                    }
                }

                // DEBUG: Test moving LightPos Height for shadows
                var lightPos = _spotLights[0].Position;
                var lightTar = _spotLights[0].Target;
                if (_keyState.IsKeyDown(Keys.A) && !(_keyState.IsKeyDown(Keys.LeftAlt)) && _debugIsFor == DebugIsFor.LightPosition)
                    lightPos.Y += 10;

                if (_keyState.IsKeyDown(Keys.Z) && !(_keyState.IsKeyDown(Keys.LeftAlt)) && _debugIsFor == DebugIsFor.LightPosition)
                    lightPos.Y -= 10;

                // DEBUG: Test moving LightTarget Height for shadows
                if (_keyState.IsKeyDown(Keys.A) && !(_keyState.IsKeyDown(Keys.LeftAlt)) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.Y += 10;

                if (_keyState.IsKeyDown(Keys.Z) && !(_keyState.IsKeyDown(Keys.LeftAlt)) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.Y -= 10;


                // DEBUG: Test moving LightPos's Position for shadows
                if (_keyState.IsKeyDown(Keys.NumPad8) && _debugIsFor == DebugIsFor.LightPosition)
                {
                    lightPos.Z += 10;
                    //LightPositionUpdated = true;
                }

                if (_keyState.IsKeyDown(Keys.NumPad2) && _debugIsFor == DebugIsFor.LightPosition)
                {
                    lightPos.Z -= 10;
                    //LightPositionUpdated = true;
                }

                if (_keyState.IsKeyDown(Keys.NumPad4) && _debugIsFor == DebugIsFor.LightPosition)
                {
                    lightPos.X -= 10;
                    //LightPositionUpdated = true;
                }

                if (_keyState.IsKeyDown(Keys.NumPad6) && _debugIsFor == DebugIsFor.LightPosition)
                {
                    lightPos.X += 10;
                    //LightPositionUpdated = true;
                }

                // DEBUG: Test moving LightTarget's Position for shadows
                if (_keyState.IsKeyDown(Keys.NumPad8) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.Z += 10;

                if (_keyState.IsKeyDown(Keys.NumPad2) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.Z -= 10;

                if (_keyState.IsKeyDown(Keys.NumPad4) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.X -= 10;

                if (_keyState.IsKeyDown(Keys.NumPad6) && _debugIsFor == DebugIsFor.LightTarget)
                    lightTar.X += 10;

                // DEBUG: Test adjusting ShadowMap Offset Texels
                var tmpOffsetSm = Vector2.Zero; var tmpOffsetSmBy = Vector2.Zero;
                switch (_debugIsFor)
                {

                    case DebugIsFor.ShadowTexelOffsetTopLeft:
                        tmpOffsetSm.X = _offsetSmTltr.X; tmpOffsetSm.Y = _offsetSmTltr.Y;
                        tmpOffsetSmBy.X = _offsetSmByTltr.X; tmpOffsetSmBy.Y = _offsetSmByTltr.Y;
                        ShadowMap.UpdateShadowTexelOffsets(inputState, ref tmpOffsetSm, ref tmpOffsetSmBy);
                        _offsetSmTltr.X = tmpOffsetSm.X; _offsetSmTltr.Y = tmpOffsetSm.Y;
                        _offsetSmByTltr.X = tmpOffsetSmBy.X; _offsetSmByTltr.Y = tmpOffsetSmBy.Y;
                        _effect2Lights.Parameters["xOffsetSM_TLTR"].SetValue(_offsetSmTltr);
                        break;
                    case DebugIsFor.ShadowTexelOffsetTopRight:
                        tmpOffsetSm.X = _offsetSmTltr.Z; tmpOffsetSm.Y = _offsetSmTltr.W;
                        tmpOffsetSmBy.X = _offsetSmByTltr.Z; tmpOffsetSmBy.Y = _offsetSmByTltr.W;
                        ShadowMap.UpdateShadowTexelOffsets(inputState, ref tmpOffsetSm, ref tmpOffsetSmBy);
                        _offsetSmTltr.Z = tmpOffsetSm.X; _offsetSmTltr.W = tmpOffsetSm.Y;
                        _offsetSmByTltr.Z = tmpOffsetSmBy.X; _offsetSmByTltr.W = tmpOffsetSmBy.Y;
                        _effect2Lights.Parameters["xOffsetSM_TLTR"].SetValue(_offsetSmTltr);
                        break;
                    case DebugIsFor.ShadowTexelOffsetBottomLeft:
                        tmpOffsetSm.X = _offsetSmBlbr.X; tmpOffsetSm.Y = _offsetSmBlbr.Y;
                        tmpOffsetSmBy.X = _offsetSmByBlbr.X; tmpOffsetSmBy.Y = _offsetSmByBlbr.Y;
                        ShadowMap.UpdateShadowTexelOffsets(inputState, ref tmpOffsetSm, ref tmpOffsetSmBy);
                        _offsetSmBlbr.X = tmpOffsetSm.X; _offsetSmBlbr.Y = tmpOffsetSm.Y;
                        _offsetSmByBlbr.X = tmpOffsetSmBy.X; _offsetSmByBlbr.Y = tmpOffsetSmBy.Y;
                        _effect2Lights.Parameters["xOffsetSM_BLBR"].SetValue(_offsetSmBlbr);
                        break;
                    case DebugIsFor.ShadowTexelOffsetBottomRight:
                        tmpOffsetSm.X = _offsetSmBlbr.Z; tmpOffsetSm.Y = _offsetSmBlbr.W;
                        tmpOffsetSmBy.X = _offsetSmByBlbr.Z; tmpOffsetSmBy.Y = _offsetSmByBlbr.W;
                        ShadowMap.UpdateShadowTexelOffsets(inputState, ref tmpOffsetSm, ref tmpOffsetSmBy);
                        _offsetSmBlbr.Z = tmpOffsetSm.X; _offsetSmBlbr.W = tmpOffsetSm.Y;
                        _offsetSmByBlbr.Z = tmpOffsetSmBy.X; _offsetSmByBlbr.W = tmpOffsetSmBy.Y;
                        _effect2Lights.Parameters["xOffsetSM_BLBR"].SetValue(_offsetSmBlbr);
                        break;
                    default:
                        break;
                }

                _oldKeyState = _keyState;

                // Set back into Spotllight
                _spotLights[0].Position = lightPos;
                _spotLights[0].Target = lightTar;
                _spotLights[0].Direction = lightTar - lightPos;
                _spotLights[0].Direction.Normalize();
                _spotLights[0].ViewMatrix = Matrix.CreateLookAt(lightPos, lightPos + _spotLights[0].Direction, Vector3.Up);
                _spotLights[0].ViewProjMatrix = _spotLights[0].ViewMatrix * _spotLights[0].ProjectionMatrix;

            }

        }
    }
}
