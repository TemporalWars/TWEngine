#region File Description
//-----------------------------------------------------------------------------
// PhysXEngine.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

#if !XBOX360 // 6/22/2009
using TWEngine;
using StillDesign;
using StillDesign.PhysX;

#endif


namespace TWEngine.PhysX
{ 

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class PhysXEngine : DrawableGameComponent
    {
        // PhysX Debug Visulization Basic Effect
// ReSharper disable UnaccessedField.Local
        private BasicEffect _visualizationEffect;
// ReSharper restore UnaccessedField.Local

        // PhysX Core component
        internal static Core PhysXCore;

        // PhysX Scene component
        internal static Scene PhysXScene;       


        public PhysXEngine(Game game)
            : base(game)
        { 

            // Init Core PhysX Engine 
            var coreDesc = new CoreDescription();
            var output = new UserOutput();

            PhysXCore = new Core(coreDesc, output);

            PhysXCore.SetParameter(PhysicsParameter.VisualizationScale, 2.0f);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeCollisionShapes, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeClothMesh, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeJointLocalAxes, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeJointLimits, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeFluidPosition, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeFluidEmitters, false); // Slows down rendering a bit to much
            PhysXCore.SetParameter(PhysicsParameter.VisualizeForceFields, true);
            PhysXCore.SetParameter(PhysicsParameter.VisualizeSoftBodyMesh, true);

            var sceneDesc = new SceneDescription
                                {
                                    SimulationType =
                                        PhysXCore.HardwareVersion == HardwareVersion.Athena_1_0  // 6/23/2009 - Check for Hardware support of PhsyX
                                            ? SimulationType.Hardware
                                            : SimulationType.Software,
                                    Gravity = new Vector3(0.0f, -9.81f, 0.0f),
                                    GroundPlaneEnabled = true
                                };

           


            PhysXScene = PhysXCore.CreateScene(sceneDesc);
            

            // Connect to the remote debugger if its there
            PhysXCore.Foundation.RemoteDebugger.Connect("localhost");

        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public sealed override void Initialize()
        {
            _visualizationEffect = new BasicEffect(TemporalWars3DEngine.GameInstance.GraphicsDevice, null)
            {
                VertexColorEnabled = true
            };

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public sealed override void Update(GameTime gameTime)
        {
            // Update Physics
            PhysXScene.Simulate((float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f);
            //_scene.Simulate( 1.0f / 60.0f );
            PhysXScene.FlushStream();
            PhysXScene.FetchResults(SimulationStatus.RigidBodyFinished, true);

            // Update VehicleForce, if necessary.
            //PhysXVehicle.UpdateForceForVehicle(gameTime);

            // Complete any Requests for 'SoftBody' meshes.
            PhysXSoftBody.DoRequestsForSoftBody();

            base.Update(gameTime);
        }

        #region Draw Methods

        /// <summary>
        /// Allows the game component to draw itself.
        /// </summary>
        /// <param name="gameTime"></param>
        public sealed override void Draw(GameTime gameTime)
        {         
            return;
            
/*
            GraphicsDevice.VertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionColor.VertexElements);

            _visualizationEffect.World = Matrix.Identity;
            _visualizationEffect.View = Camera.View;
            _visualizationEffect.Projection = Camera.Projection;

            DebugRenderable data = PhysXScene.GetDebugRenderable();

            _visualizationEffect.Begin();

            foreach (EffectPass pass in _visualizationEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                if (data.PointCount > 0)
                {
                    DebugPoint[] points = data.GetDebugPoints();

                    GraphicsDevice.DrawUserPrimitives<DebugPoint>(PrimitiveType.PointList, points, 0, points.Length);
                }

                if (data.LineCount > 0)
                {
                    DebugLine[] lines = data.GetDebugLines();

                    VertexPositionColor[] vertices = new VertexPositionColor[data.LineCount * 2];
                    for (int x = 0; x < data.LineCount; x++)
                    {
                        DebugLine line = lines[x];

                        vertices[x * 2 + 0] = new VertexPositionColor(line.Point0, Int32ToColor(line.Color));
                        vertices[x * 2 + 1] = new VertexPositionColor(line.Point1, Int32ToColor(line.Color));
                    }

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineList, vertices, 0, lines.Length);
                }

                if (data.TriangleCount > 0)
                {
                    DebugTriangle[] triangles = data.GetDebugTriangles();

                    VertexPositionColor[] vertices = new VertexPositionColor[data.TriangleCount * 3];
                    for (int x = 0; x < data.TriangleCount; x++)
                    {
                        DebugTriangle triangle = triangles[x];

                        vertices[x * 3 + 0] = new VertexPositionColor(triangle.Point0, Int32ToColor(triangle.Color));
                        vertices[x * 3 + 1] = new VertexPositionColor(triangle.Point1, Int32ToColor(triangle.Color));
                        vertices[x * 3 + 2] = new VertexPositionColor(triangle.Point2, Int32ToColor(triangle.Color));
                    }

                    GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, triangles.Length);
                }

                pass.End();
            }

            _visualizationEffect.End();
*/

           
        }

        public static Color Int32ToColor(int color)
        {
            var a = (byte)((color & 0xFF000000) >> 32);
            var r = (byte)((color & 0x00FF0000) >> 16);
            var g = (byte)((color & 0x0000FF00) >> 8);
            var b = (byte)((color & 0x000000FF) >> 0);

            return new Color(r, g, b, a);
        }

        public static int ColorToArgb(Color color)
        {
            var a = (int)(color.A);
            var r = (int)(color.R);
            var g = (int)(color.G);
            var b = (int)(color.B);

            return (a << 24) | (r << 16) | (g << 8) | (b << 0);
        }

        #endregion
                       

        // 6/22/2009
        internal static void GetVertexElement(VertexDeclaration vertexDeclaration, VertexElementUsage vertexElementToRetrieve, out VertexElement vertexElement)
        {
            // Set out params.
            vertexElement = new VertexElement();
            
            // Get VertexDeclaration array
            var vertexElements = vertexDeclaration.GetVertexElements();

            // Get Positions data
            for (int i = 0; i < vertexElements.Length; i++)
            {
                if (vertexElements[i].VertexElementUsage == vertexElementToRetrieve)
                {   
                    // return requested vertexElement
                    vertexElement = vertexElements[i];
                    return;
                }
            }
        }

        // 6/22/2009
        internal static bool CookMesh_Cloth(ClothMeshDescription desc, out MemoryStream stream)
        {
            // Two ways on cooking mesh: 1. Saved in memory, 2. Saved in file	
            // Cooking from memory
            stream = new MemoryStream();
            Cooking.InitializeCooking();
            bool success = Cooking.CookClothMesh(desc, stream);
            Cooking.CloseCooking();

            // Need to reset the Position of the stream to the beginning
            stream.Position = 0;

            return success;

        }

        // 6/24/2009
        internal static bool CookMesh_SoftBody(SoftBodyMeshDescription desc, out MemoryStream stream)
        {
            // Two ways on cooking mesh: 1. Saved in memory, 2. Saved in file	
            // Cooking from memory
            stream = new MemoryStream();
            Cooking.InitializeCooking();
            bool success = Cooking.CookSoftBodyMesh(desc, stream);
            Cooking.CloseCooking();

            // Need to reset the Position of the stream to the beginning
            stream.Position = 0;

            return success;
        }
      

        #region Dispose

        /// <summary>
        /// Releases the unmanaged resources used by the DrawableGameComponent and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose objects
                if (PhysXScene != null)
                {
                    PhysXScene.ShutdownWorkerThreads();                     
                    PhysXScene.Dispose();
                }

                if (_visualizationEffect != null)
                    _visualizationEffect.Dispose();

                if (PhysXCore != null)
                    PhysXCore.Dispose();

                // Null Refs
                PhysXScene = null;
                PhysXCore = null;
                _visualizationEffect = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}