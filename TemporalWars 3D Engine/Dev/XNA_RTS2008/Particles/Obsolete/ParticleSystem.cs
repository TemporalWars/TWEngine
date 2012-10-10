#region File Description
//-----------------------------------------------------------------------------
// ParticleSystem.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using TWEngine.GameCamera;
using TWEngine.Particles.Structs;
using TWEngine.Utilities;

namespace TWEngine.Particles
{
    /// <summary>
    /// The <see cref="ParticleSystem"/> abstract class, provides the base functionality for
    /// updating and drawing all particles for a given <see cref="ParticleSystem"/>.
    /// </summary>
    public abstract class ParticleSystem : IDisposable
    {
        #region Fields       

        // Settings class controls the appearance and animation of this particle system.
        internal ParticleSettings Settings = new ParticleSettings();        


        // For loading the _effect and particle _texture.
        static ContentManager _contentManager;

        readonly GraphicsDevice _device;


        private readonly Effect _originalEffect; 
        private Texture2D _texture;

        // Custom _effect for drawing point sprite _particles. This computes the particle
        // animation entirely in the vertex shader: no per-particle CPU work required!
        internal Effect ParticleEffect; // 1/16/2010 - Made Internal, for ParticlesSettings.


        // Shortcuts for accessing frequently changed _effect parameters.
        static EffectParameter _effectViewParameter;
        static EffectParameter _effectProjectionParameter;
        static EffectParameter _effectViewportScaleParameter;
        static EffectParameter _effectTimeParameter;


        // An array of _particles, treated as a circular queue.
        ParticleVertex[] _particles;


        // A vertex buffer holding our _particles. This contains the same data as
        // the _particles array, but copied across to where the GPU can access it.
        DynamicVertexBuffer _vertexBuffer;

        // XNA 4.0 Updates - VertexDeclaration removed.
        // Vertex declaration describes the format of our ParticleVertex structure.
        //VertexDeclaration _vertexDeclaration;

        // XNA 4.0 Updates - New IndexBuffer
        // Index buffer turns sets of four vertices into particle quads (pairs of triangles).
        IndexBuffer _indexBuffer;


        // The _particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no _particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // _particles last for the same amount of Time, old _particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, _particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed _particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several _particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new _particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old _particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste Time waiting around every Time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in Total, our queue contains four different regions:
        //
        // Vertices between _firstActiveParticle and _firstNewParticle are actively
        // being drawn, and exist in both the managed _particles array and the GPU
        // vertex buffer.
        //
        // Vertices between _firstNewParticle and _firstFreeParticle are newly created,
        // and exist only in the managed _particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between _firstFreeParticle and _firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between _firstRetiredParticle and _firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        int _firstActiveParticle;
        int _firstNewParticle;
        int _firstFreeParticle;
        int _firstRetiredParticle;


        // Store the current Time, in seconds.
        float _currentTime;


        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old _particles back into the free list.
        int _drawCounter;


        // Shared Random number generator.
        static readonly Random Random = new Random();


        #endregion

        #region Initialization

        /// <summary>
        /// Constructor, which creates the required <see cref="ParticleVertex"/> collection, and
        /// initializes the required <see cref="DynamicVertexBuffer"/>, used to draw the particles.
        /// </summary>
        protected ParticleSystem()            
        {
            // 1/16/2010
            var game = TemporalWars3DEngine.GameInstance;

            // 1/16/2010 - Init 'ParticleSettings' instance.
            //Settings = new ParticleSettings(this);     
            Settings.Parent = this;
             
            // 8/27/2008 - Moved from Draw
            _device = game.GraphicsDevice;

            // 8/18/2008
            if (_contentManager == null)                
                _contentManager = game.Content;


            // XNA 4.0 Updates; now call Initialize().
            //InitializeSettings(Settings);
            //_particles = new ParticleVertex[Settings.MaxParticles];
            //_vertexDeclaration = new VertexDeclaration(game.GraphicsDevice, ParticleVertex.VertexElements);
            Initialize();

            // 4/6/2010: Updated to use 'ContentMiscLoc' global var.
            _originalEffect = _contentManager.Load<Effect>(TemporalWars3DEngine.ContentMiscLoc + @"\Shaders\ParticleEffect");

           

            // XNA 4.0 Updates; now call LoadContent()
            // Create a dynamic vertex buffer.
            //var size = ParticleVertex.SizeInBytes * _particles.Length;
            //_vertexBuffer = new DynamicVertexBuffer(game.GraphicsDevice, size, BufferUsage.WriteOnly | BufferUsage.Points);
            //LoadParticleEffect();
            LoadContent();
           

        }

        // XNA 4.0 Updates - New method.
        /// <summary>
        /// Initializes the component.
        /// </summary>
        public void Initialize()
        {
            InitializeSettings(Settings);

            // Allocate the particle array, and fill in the corner fields (which never change).
            _particles = new ParticleVertex[Settings.MaxParticles * 4];

            for (var i = 0; i < Settings.MaxParticles; i++)
            {
                _particles[i * 4 + 0].Corner = new Short2(-1, -1);
                _particles[i * 4 + 1].Corner = new Short2(1, -1);
                _particles[i * 4 + 2].Corner = new Short2(1, 1);
                _particles[i * 4 + 3].Corner = new Short2(-1, 1);
            }

        }

        /// <summary>
        /// Derived particle system classes should override this method
        /// and use it to initalize their tweakable Settings.
        /// </summary>
        /// <param name="settings">Instance of particle settings.</param>
        protected abstract void InitializeSettings(ParticleSettings settings);

        // XNA 4.0 Updates - New method.
        /// <summary>
        /// Loads graphics for the particle system.
        /// </summary>
        private void LoadContent()
        {
            LoadParticleEffect();

            // Create a dynamic vertex buffer.
            _vertexBuffer = new DynamicVertexBuffer(_device, ParticleVertex.VertexDeclaration,
                                                   Settings.MaxParticles * 4, BufferUsage.WriteOnly);

            // Create and populate the index buffer.
            var indices = new ushort[Settings.MaxParticles * 6];

            for (var i = 0; i < Settings.MaxParticles; i++)
            {
                indices[i * 6 + 0] = (ushort)(i * 4 + 0);
                indices[i * 6 + 1] = (ushort)(i * 4 + 1);
                indices[i * 6 + 2] = (ushort)(i * 4 + 2);

                indices[i * 6 + 3] = (ushort)(i * 4 + 0);
                indices[i * 6 + 4] = (ushort)(i * 4 + 2);
                indices[i * 6 + 5] = (ushort)(i * 4 + 3);
            }

            _indexBuffer = new IndexBuffer(_device, typeof(ushort), indices.Length, BufferUsage.WriteOnly);

            _indexBuffer.SetData(indices);
        }
       
        /// <summary>
        /// Helper for loading and initializing the particle <see cref="Effect"/>.
        /// </summary>
        private void LoadParticleEffect()
        {            

            // If we have several particle systems, the content manager will return
            // a single shared _effect instance to them all. But we want to preconfigure
            // the _effect with parameters that are specific to this particular
            // particle system. By cloning the _effect, we prevent one particle system
            // from stomping over the parameter Settings of another.

            // XNA 4.0 Updates; - 'Device' param removed.
            //ParticleEffect = _originalEffect.Clone(_device);
            ParticleEffect = _originalEffect.Clone();

            var parameters = ParticleEffect.Parameters;

            // Look up shortcuts for parameters that change every frame.
            _effectViewParameter = parameters["View"];
            _effectProjectionParameter = parameters["Projection"];
            _effectViewportScaleParameter = parameters["ViewportScale"];
            _effectTimeParameter = parameters["CurrentTime"];

            // Set the values of parameters that do not change.
            parameters["Duration"].SetValue((float)Settings.Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(Settings.DurationRandomness);
            parameters["Gravity"].SetValue(Settings.Gravity);
            parameters["EndVelocity"].SetValue(Settings.EndVelocity);
            parameters["MinColor"].SetValue(Settings.MinColor.ToVector4());
            parameters["MaxColor"].SetValue(Settings.MaxColor.ToVector4());

            parameters["RotateSpeed"].SetValue(
                new Vector2(Settings.MinRotateSpeed, Settings.MaxRotateSpeed));
            
            parameters["StartSize"].SetValue(
                new Vector2(Settings.MinStartSize, Settings.MaxStartSize));
            
            parameters["EndSize"].SetValue(
                new Vector2(Settings.MinEndSize, Settings.MaxEndSize));

            // Load the particle _texture, and set it onto the _effect.
            //Texture2D _texture = _contentManager.Load<Texture2D>(@"Textures\Particles\" + Settings.TextureName);

            // 4/6/2010: Updated to use 'ContentTexturesLoc' global var.
            // Load the particle _texture, and set it onto the _effect.
            _texture = _contentManager.Load<Texture2D>(String.Format(@"{0}\Textures\Particles\{1}", TemporalWars3DEngine.ContentTexturesLoc, Settings.TextureName));

            //Storage.SaveTexture(_texture, ImageFileFormat.Png, @"C:\Downloads\" + Settings.TextureName);

            parameters["Texture"].SetValue(_texture);
            
            // XNA 4.0 Updates - Technique setting removed.
            #region OLDCode
            // Choose the appropriate _effect technique. If these _particles will never
            // rotate, we can use a simpler pixel shader that requires less GPU power.
            /*string techniqueName;

            if ((Settings.MinRotateSpeed == 0) && (Settings.MaxRotateSpeed == 0))
                techniqueName = "NonRotatingParticles";
            else
                techniqueName = "RotatingParticles";

            ParticleEffect.CurrentTechnique = ParticleEffect.Techniques[techniqueName];*/
            #endregion
        }


        #endregion        

        #region Update and Draw      

        /// <summary>
        /// Updates the current <see cref="ParticleSystem"/> particles.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameTime"/> instance is null.</exception>
        public void Update(GameTime gameTime)
        {
            // 8/15/2008 - Skip updating if not Visible
            //if (!Visible)
            //   return;
            

            if (gameTime == null)
                throw new ArgumentNullException("gameTime");
            

            _currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the _particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the Time value doesn't matter when no _particles are being drawn,
            // so we can reset it back to zero any Time the active queue is empty.

            if (_firstActiveParticle == _firstFreeParticle)
                _currentTime = 0;

            if (_firstRetiredParticle == _firstActiveParticle)
                _drawCounter = 0;           
            
        }


        /// <summary>
        /// Helper for checking when active _particles have reached the end of
        /// their life. It moves old _particles from the active area of the queue
        /// to the retired section.
        /// </summary>       
        void RetireActiveParticles()
        {           
            // 5/12/2009
            try
            {
                var particleDuration = (float)Settings.Duration.TotalSeconds;

                while (_firstActiveParticle != _firstNewParticle)
                {
                    // Is this particle old enough to retire?
                    var particleAge = _currentTime - _particles[_firstActiveParticle * 4].Time;

                    if (particleAge < particleDuration)
                        break;

                    // Remember the Time at which we retired this particle.
                    _particles[_firstActiveParticle * 4].Time = _drawCounter;

                    // Move the particle from the active to the retired queue.
                    _firstActiveParticle++;

                    if (_firstActiveParticle >= Settings.MaxParticles) // _particles.Length
                        _firstActiveParticle = 0;
                }
            }
            catch (NullReferenceException)
            {
                // Return if Null, since this should only occur when Dispose was called.
                return;
            }
        }


        /// <summary>
        /// Helper for checking when retired _particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old _particles from the retired area of the queue to the free section.
        /// </summary>       
        void FreeRetiredParticles()
        {
            while (_firstRetiredParticle != _firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                var age = _drawCounter - (int)_particles[_firstRetiredParticle * 4].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                _firstRetiredParticle++;

                if (_firstRetiredParticle >= Settings.MaxParticles) // _particles.Length
                    _firstRetiredParticle = 0;
            }
        }

        
        /// <summary>
        /// Draws the current <see cref="ParticleSystem"/>.
        /// </summary>
        /// <param name="gameTime"><see cref="GameTime"/> instance</param>
        public void Draw(GameTime gameTime)
        {
            // 4/25/2010 - Cache
            if (_vertexBuffer == null) return;

            // 4/25/2010 - Cache
            var graphicsDevice = _device;

            // Restore the vertex buffer contents if the graphics _device was lost.
            if (_vertexBuffer.IsContentLost)
            {
                _vertexBuffer.SetData(_particles);
            }

            // If there are any _particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (_firstNewParticle != _firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active _particles, draw them now!
            if (_firstActiveParticle != _firstFreeParticle)
            {
                // XNA 4.0 updates - No 'RenderState'.
                //SetParticleRenderStates(graphicsDevice.RenderState);
                _device.BlendState = Settings.BlendState;
                _device.DepthStencilState = DepthStencilState.DepthRead;

                // XNA 4.0 Updates
                // Set an _effect parameter describing the viewport size. This is needed
                // to convert particle sizes into screen space point sizes.
                //_effectViewportHeightParameter.SetValue(graphicsDevice.Viewport.Height);
                _effectViewportScaleParameter.SetValue(new Vector2(0.5f / _device.Viewport.AspectRatio, -0.5f));

                // Set an _effect parameter describing the current Time. All the vertex
                // shader particle animation is keyed off this value.
                _effectTimeParameter.SetValue(_currentTime);

                // XNA 4.0 Updates - Set the particle vertex and index buffer.
                // Set the particle vertex buffer and vertex declaration.
                /*graphicsDevice.Vertices[0].SetSource(dynamicVertexBuffer, 0,
                                              ParticleVertex.SizeInBytes);
                graphicsDevice.VertexDeclaration = _vertexDeclaration;*/
                graphicsDevice.SetVertexBuffer(_vertexBuffer);
                graphicsDevice.Indices = _indexBuffer;

                // XNA 4.0 Updates - Begin() removed.
                // Activate the particle _effect.
                //ParticleEffect.Begin();

                // 8/27/2008: Updated to For-Loop, rather than ForEach.
                var passCollection = ParticleEffect.CurrentTechnique.Passes; // 4/25/2010
                var count = passCollection.Count; // 4/25/2010
                for (var i = 0; i < count; i++)
                {
                    // XNA 4.0 Updates - Apply() replaces Begin().
                    passCollection[i].Apply();

                    if (_firstActiveParticle < _firstFreeParticle)
                    {
                        // XNA 4.0 Updates
                        // If the active _particles are all in one consecutive range,
                        // we can draw them all in a single call.
                        //graphicsDevice.DrawPrimitives(PrimitiveType.PointList,_firstActiveParticle,_firstFreeParticle - _firstActiveParticle);
                        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                    _firstActiveParticle * 4, (_firstFreeParticle - _firstActiveParticle) * 4,
                                                    _firstActiveParticle * 6, (_firstFreeParticle - _firstActiveParticle) * 2);
                    }
                    else
                    {
                        // XNA 4.0 Updates
                        // If the active particle range wraps past the end of the queue
                        // back to the start, we must split them over two draw calls.
                        //graphicsDevice.DrawPrimitives(PrimitiveType.PointList,_firstActiveParticle,_particles.Length - _firstActiveParticle);
                        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                    _firstActiveParticle * 4, (Settings.MaxParticles - _firstActiveParticle) * 4,
                                                    _firstActiveParticle * 6, (Settings.MaxParticles - _firstActiveParticle) * 2);

                        if (_firstFreeParticle > 0)
                        {
                            // XNA 4.0 Updates
                            //graphicsDevice.DrawPrimitives(PrimitiveType.PointList,0,_firstFreeParticle);
                            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0,
                                                                 0, _firstFreeParticle * 4,
                                                                 0, _firstFreeParticle * 2);
                        }
                    }

                    // XNA 4.0 Updates - END() obsolete.
                    //passCollection[i].End();
                   
                }

                // XNA 4.0 Updates - END() obsolete.
                //ParticleEffect.End();     

                // XNA 4.0 Updates - New setting.
                // Reset some of the renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                graphicsDevice.DepthStencilState = DepthStencilState.Default;

                // XNA 4.0 Updates - RenderState setting obsolete.
                // Reset a couple of the more unusual renderstates that we changed,
                // so as not to mess up any other subsequent drawing.
                //graphicsDevice.RenderState.PointSpriteEnable = false;
                //graphicsDevice.RenderState.DepthBufferWriteEnable = true;

                // 9/16/2008 - Release VB to Avoid the XBOX crash when calling the VB.SetData call, in the
                //             'AddNewParticlesToVertexBuffer' method
#if XBOX360
                // XNA 4.0 Updates
                //_device.Vertices[0].SetSource(null, 0, 0);
                graphicsDevice.SetVertexBuffer(null);
#endif
            }

            _drawCounter++;
            
        }       

        /// <summary>
        /// Helper for uploading new _particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>       
        void AddNewParticlesToVertexBuffer()
        {
            try // 1/1/2010
            {
                const int stride = ParticleVertex.SizeInBytes;

                // 4/25/2010 - Cache
                if (_firstNewParticle < _firstFreeParticle)
                {
                    // If the new _particles are all in one consecutive range,
                    // we can upload them all in a single call.                
                    _vertexBuffer.SetData(_firstNewParticle * stride * 4, _particles,
                                          _firstNewParticle * 4,
                                          (_firstFreeParticle - _firstNewParticle) * 4,
                                          stride, SetDataOptions.NoOverwrite);
                }
                else
                {
                    // If the new particle range wraps past the end of the queue
                    // back to the start, we must split them over two upload calls.
                    _vertexBuffer.SetData(_firstNewParticle * stride * 4, _particles,
                                          _firstNewParticle * 4,
                                          (Settings.MaxParticles - _firstNewParticle) * 4,
                                          stride, SetDataOptions.NoOverwrite);

                    if (_firstFreeParticle > 0)
                    {
                        _vertexBuffer.SetData(0, _particles,
                                              0, _firstFreeParticle * 4,
                                              stride, SetDataOptions.NoOverwrite);
                    }
                }

                // Move the _particles we just uploaded from the new to the active queue.
                _firstNewParticle = _firstFreeParticle;
            }
#pragma warning disable 168
            catch (Exception err)
#pragma warning restore 168
            {
                Debug.WriteLine(string.Format("(AddNewParticlesToVertexBuffer) threw an exception {0}.",err.Message));
               
            }
        }

        // 9/20/2010 - XNA 4.0 Updates - Obsolete method for setting 'RenderSTate'.
        #region OLDcode
        // 8/13/2009: Optimized, by moving all STATIC settings to the 'ParticleEffect.fx' file
        //            , set in the 'Techinique' itself, while the dynamic settings are left in
        //            the method below.
        /*/// <summary>
        /// Helper for setting the renderstates used to draw _particles.
        /// </summary>
        void SetParticleRenderStates(RenderState renderState)
        {
            // Enable point sprites.
            //RenderState.PointSpriteEnable = true;
            //RenderState.PointSizeMax = 256;

            // Set the alpha blend mode.
            //RenderState.AlphaBlendEnable = true;
            renderState.AlphaBlendOperation = BlendFunction.Add;
            renderState.SourceBlend = Settings.SourceBlend;
            renderState.DestinationBlend = Settings.DestinationBlend;

            // Set the alpha test mode.
            //RenderState.AlphaTestEnable = true;
            //RenderState.AlphaFunction = CompareFunction.Greater;
            //RenderState.ReferenceAlpha = 0;

            // 4/10/2009: Updated to set the DepthBufferEnable to True/False, depending on RenderType.
            // Enable the depth buffer (so _particles will not be visible through
            // solid objects like the ground plane), but disable depth writes
            // (so _particles will not obscure other _particles).
            //RenderState.DepthBufferEnable = true; // was True (3/20/2009)
            //RenderState.DepthBufferWriteEnable = false;
        }*/
        #endregion

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the camera view and projection matrices
        /// that will be used to draw this <see cref="ParticleSystem"/>.
        /// </summary>
        public static void SetCamera()
        {
            // 12/8/2008: Add Null check to capture error cause due to timing when
            //            a particle system is called while it is not available 
            //            anymore.
            if (_effectViewParameter != null)
                _effectViewParameter.SetValue(Camera.View);

            if (_effectProjectionParameter != null)
                _effectProjectionParameter.SetValue(Camera.Projection);
        }

        // NOTE: Was ref 2nd param.
        // 9/16/2008: Tried adding Reference for the parameters, however, this causes the _particles
        //            to fly all over the screen!
        // 1/29/2009: Updated the 2nd parameter to Ref, and this seems to work.    
        /// <summary>
        /// Adds a new particle to the <see cref="ParticleSystem"/>.
        /// </summary>
        /// <param name="position"><see cref="Vector3"/> position for particle</param>
        /// <param name="velocity"><see cref="Vector3"/> as velocity of particle</param>
        public void AddParticle(Vector3 position, Vector3 velocity)
        {
            // 12/14/2008 - Make sure '_particles' array is not null; otherwise, crash can occur!
            if (_particles == null) return;
                //_particles = new ParticleVertex[Settings.MaxParticles * 4];

            // 3/24/2011 - Check to make sure ParticleVertex is correct size
            var maxSize = Settings.MaxParticles * 4;
            if (_particles.Length < maxSize) Array.Resize(ref _particles, maxSize);

            // 4/25/2010 - Refactored out the rest of the code, to be in a static method.
            AddParticleHelper(ref velocity, ref position, ref _particles, _firstRetiredParticle, ref _firstFreeParticle, Settings, _currentTime);
        }

        // 4/25/2010 - 
        /// <summary>
        /// Method helper, which adds a new particle to the <see cref="ParticleSystem"/>.
        /// </summary>
        /// <param name="velocity">Velocity for particle</param>
        /// <param name="position">Position of particle</param>
        /// <param name="particleVertices"><see cref="ParticleVertex"/> collection</param>
        /// <param name="firstRetiredParticle">Pass in <see cref="_firstRetiredParticle"/></param>
        /// <param name="firstFreeParticle">Pass in <see cref="_firstFreeParticle"/>; will be modified</param>
        /// <param name="particleSettings"><see cref="ParticleSettings"/> instance</param>
        /// <param name="currentTime">Pass in <see cref="_currentTime"/></param>
        private static void AddParticleHelper(ref Vector3 velocity, ref Vector3 position, ref ParticleVertex[] particleVertices, 
            int firstRetiredParticle, ref int firstFreeParticle, ParticleSettings particleSettings, float currentTime)
        {
            // Figure out where in the circular queue to allocate the new particle.
            var nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= particleSettings.MaxParticles)
                nextFreeParticle = 0;

            // If there are no free _particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;               

            // Add in some Random amount of horizontal velocity.
            var horizontalVelocity = MathHelper.Lerp(particleSettings.MinHorizontalVelocity,
                                                     particleSettings.MaxHorizontalVelocity,
                                                     (float)Random.NextDouble());

            var horizontalAngle = Random.NextDouble() * MathHelper.TwoPi;

            // 5/12/2009 - Do Calcs seperately to improve CPI in Vtune.
            var horizontalAngleCos = (float)Math.Cos(horizontalAngle);
            var horizontalAngleSin = (float)Math.Sin(horizontalAngle);
            var horizontalAngleWithCos = horizontalVelocity * horizontalAngleCos;
            var horizontalAngleWithSin = horizontalVelocity * horizontalAngleSin;

            // Adjust the input velocity based on how much
            // this particle system wants to be affected by it.     
            Vector3 newVelocity;
            Vector3.Multiply(ref velocity, particleSettings.EmitterVelocitySensitivity, out newVelocity);

            newVelocity.X += horizontalAngleWithCos;
            newVelocity.Z += horizontalAngleWithSin;

            // Add in some Random amount of vertical velocity.
            newVelocity.Y += MathHelper.Lerp(particleSettings.MinVerticalVelocity,
                                             particleSettings.MaxVerticalVelocity,
                                             (float)Random.NextDouble());

            // 1/26/2009 - Updated to set the color channels directly, rather than creating new Color every Time!
            // Choose four Random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            //randomValues = new Color((byte)Random.Next(255), (byte)Random.Next(255), (byte)Random.Next(255), (byte)Random.Next(255));
            var randomValues = Color.White;
            randomValues.R = (byte)Random.Next(255); randomValues.G = (byte)Random.Next(255);
            randomValues.B = (byte)Random.Next(255); randomValues.A = (byte)Random.Next(255);

            // Fill in the particle vertex structure.
            for (int i = 0; i < 4; i++)
            {
                // Fill in the particle vertex structure.
                particleVertices[firstFreeParticle * 4 + i].Position = position;
                particleVertices[firstFreeParticle * 4 + i].Velocity = newVelocity;
                particleVertices[firstFreeParticle * 4 + i].Random = randomValues;
                particleVertices[firstFreeParticle * 4 + i].Time = currentTime;
            }

            firstFreeParticle = nextFreeParticle;
            
        }

        #endregion
              

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            // Dispose
            if (ParticleEffect != null)
                ParticleEffect.Dispose();
            if (_vertexBuffer != null)
                _vertexBuffer.Dispose();
            // XNA 4.0 Updates
            if (_indexBuffer != null)
                _indexBuffer.Dispose();

            // Null refs
            _particles = null;
            ParticleEffect = null;
            _effectViewParameter = null;
            _effectProjectionParameter = null;
            _effectViewportScaleParameter = null;
            _effectTimeParameter = null;
            _vertexBuffer = null;
            _indexBuffer = null; // XNA 4.0 Updates.
            Settings = null;

            if (_contentManager != null)
            {
                _contentManager.Unload();
                _contentManager = null;
            }
        }

        #endregion

        
    }
}


