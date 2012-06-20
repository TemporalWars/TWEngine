#region File Description
//-----------------------------------------------------------------------------
// PhysXSoftBody.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using TWEngine.InstancedModels;
using TWEngine.Utilities;

#if !XBOX360 
using StillDesign.PhysX;
using System.Xml;
#endif

namespace TWEngine.PhysX
{
    // SofyBody Struct Elements
    public struct SoftBodyVertexElements
    {
        public SoftBodyVertexElements(int surfaceModelVerticesCount, int tetraVerticesCount, int tetrahedraSinglesCount)
        {
            surfaceModelVertices = new Vector3[surfaceModelVerticesCount];
            surfaceModelNormals = new Vector3[surfaceModelVerticesCount];

            tetraVertices = new Vector3[tetraVerticesCount];
            tetrahedraSingles = new int[tetrahedraSinglesCount];
        }

        public Vector3[] surfaceModelVertices;
        public Vector3[] surfaceModelNormals;

        public Vector3[] tetraVertices;
        public int[] tetrahedraSingles;
    }

    // 6/24/2009
    class PhysXSoftBody : System.IDisposable
    {
        Actor poleActor; // Used to attach leaves to pole, as a trunk bases.
        SoftBody softBody;
        ObjMesh surfaceMesh;
        string modelPathName;
        
        SoftBodyVertexElements softBodyVertexElements;
        VertexElement vertexElementPosition;
        VertexElement vertexElementNormal;

        static Stack<InstancedModelPart> requests;

        // constructor
        public PhysXSoftBody()
        {
            if (requests == null)
                requests = new Stack<InstancedModelPart>(10);
        }

        internal void CreateRequestForSoftBody(InstancedModelPart modelPart, string modelPathName)
        {
            // Add Request to stack.
            requests.Push(modelPart);
            // Save load pathName
            this.modelPathName = modelPathName;
        }


        internal static void DoRequestsForSoftBody()
        {
            if (requests == null)
                return;

            // Pop a request off the stack
            if (requests.Count > 0)
            {
                InstancedModelPart modelPart = requests.Pop();

                // Create Soft Body
                modelPart.PhysXSoftBody.CreateSoftBody(modelPart);
            }
        }       

        // Create SoftBody
        internal void CreateSoftBody(InstancedModelPart modelPart)
        {                   

            XmlDocument doc = new XmlDocument();
            //doc.Load(@"ContentInstancedModels\Models\Trees\STPack\PalmSet\treePalmNew002c_Leafs.xml");
            doc.Load(@"ContentInstancedModels\" + modelPathName + "_Leafs.xml");

            // NOTE: The 'NUSteam2' path is the hierachy in the XML file, to get to the given 'Vertices' and 'tetrahedra' data!            
            Vector3[] tetraVertices = ReadVertices(doc.SelectSingleNode("/NXUSTREAM2/NxuPhysicsCollection/NxSoftBodyMeshDesc/vertices"));
            int[] tetrahedraSingles = ReadTetrahedra(doc.SelectSingleNode("/NXUSTREAM2/NxuPhysicsCollection/NxSoftBodyMeshDesc/tetrahedra"));

            // Get VertexDeclaration information, needed for the Receive Buffers. 
            PhysXEngine.GetVertexElement(modelPart.VertexDeclaration, VertexElementUsage.Position, out vertexElementPosition);
            PhysXEngine.GetVertexElement(modelPart.VertexDeclaration, VertexElementUsage.Normal, out vertexElementNormal);
            
            softBodyVertexElements = new SoftBodyVertexElements(modelPart.VertexCount, tetraVertices.Length, tetrahedraSingles.Length);
            softBodyVertexElements.tetraVertices = tetraVertices;
            softBodyVertexElements.tetrahedraSingles = tetrahedraSingles;           
            

            // SoftBody Mesc Desc
            SoftBodyMeshDescription softBodyMeshDesc = new SoftBodyMeshDescription()
            {
                VertexCount = tetraVertices.Length,
                TetrahedraCount = tetrahedraSingles.Length / 4 // Tetrahedras come in quadruples of ints
            };

            softBodyMeshDesc.AllocateVertices<Vector3>( softBodyMeshDesc.VertexCount );
            softBodyMeshDesc.AllocateTetrahedra<int>( softBodyMeshDesc.TetrahedraCount );

            softBodyMeshDesc.VertexStream.SetData(softBodyVertexElements.tetraVertices);
            softBodyMeshDesc.TetrahedraStream.SetData(softBodyVertexElements.tetrahedraSingles);
            

            // Cook Mesh
            MemoryStream stream;
            if (PhysXEngine.CookMesh_SoftBody(softBodyMeshDesc, out stream))
            {
                // Create SoftBodyMesh
                SoftBodyMesh softBodyMesh = PhysXEngine.PhysXCore.CreateSoftBodyMesh(stream);
              
                // Create SufaceMesh, used to Translate the 'TetraLinks' to 'Vertices'.
                surfaceMesh = new ObjMesh();
                //surfaceMesh.loadFromObjFile(@"ContentInstancedModels\" + modelPathName + "_Leafs_SB.obj");                
                surfaceMesh.LoadTetraLinksFromFile(@"ContentInstancedModels\" + modelPathName + "_Leafs_SB.tet");
                //surfaceMesh.buildTetraLinks(ref softBodyVertexElements, softBodyMeshDesc.TetrahedraCount);

                // Populate SurfaceVertices array  
                //surfaceMesh.mVertices.CopyTo(softBodyVertexElements.surfaceModelVertices);
                modelPart.DynamicVertexBuffer.GetData<Vector3>(vertexElementPosition.Offset, softBodyVertexElements.surfaceModelVertices, 0, modelPart.VertexCount, modelPart.VertexStride);
                // Populate Normals array
                modelPart.DynamicVertexBuffer.GetData<Vector3>(vertexElementNormal.Offset, softBodyVertexElements.surfaceModelNormals, 0, modelPart.VertexCount, modelPart.VertexStride);
                
                SoftBodyDescription softBodyDesc = new SoftBodyDescription()
                {
                    SoftBodyMesh = softBodyMesh,               
                    Flags = SoftBodyFlag.CollisionTwoway | SoftBodyFlag.Gravity,
                    GlobalPose = Matrix.CreateTranslation(0, 0, 0),
                    ParticleRadius = 0.2f,
                    VolumeStiffness = 0.5f,
                    StretchingStiffness = 1.0f,
                    Friction = 1.0f,
                    CollisionResponseCoefficient = 0.9f,
                    SolverIterations = 5                    
                                        
                };
               
                softBodyDesc.MeshData.AllocatePositions<Vector3>( tetraVertices.Length );
                softBodyDesc.MeshData.AllocateIndices<int>( tetrahedraSingles.Length );                


                // Create SoftBody in Scene instance
                softBody = PhysXEngine.PhysXScene.CreateSoftBody(softBodyDesc);

                // Attach Pole as Trunk
                ActorDescription poleActorDesc = new ActorDescription()
                {
                    GlobalPose = Matrix.CreateTranslation(0, 0, 0),
                    Shapes = { new BoxShapeDescription(20.0f, 500.0f, 20.0f) }                    

                };

                Actor poleActor = PhysXEngine.PhysXScene.CreateActor(poleActorDesc);                

                //softBody.AttachToShape(poleActor.Shapes.Single(), 0);
                softBody.AttachToShape(poleActor.Shapes.Single(), 0);
                softBody.ExternalAcceleration = new Vector3(250, 10, 100);
                
            }
            else
                throw new InvalidOperationException("Unable to 'Cook' the softBodyMesh data.");


        }
          
        // 6/26/2009 - Did Reset on IndexBuffer?
        bool resetIndexBuffer;
        int[] tmpDrawIndices = new int[1];

        // 6/24/2009
        internal void UpdateSoftBodyBuffers(InstancedModelPart modelPart)
        {
            if (softBody == null)
                return;

            /// 7/3/2009
            // Retrieve updated data from PhysX Streams
            softBody.MeshData.PositionsStream.GetData<Vector3>(softBodyVertexElements.tetraVertices);
            //cloth.MeshData.NormalsStream.GetData<Vector3>(clothVertexElements.normals);
            //cloth.MeshData.IndicesStream.GetData<ushort>(modelPart.IndexData);

           
            // Retrieve updated data from PhysX Streams           
            for (int i = 0; i < softBody.NumberOfParticles; i++)
            {
                softBodyVertexElements.tetraVertices[i] = softBody.GetPosition(i);
            }    

            // Update TetraLinks
            surfaceMesh.UpdateTetraLinks(ref softBodyVertexElements);
            // Update Indices
            //surfaceMesh.UpdateIndices();

            // Update Positions
            modelPart.DynamicVertexBuffer.SetData<Vector3>(vertexElementPosition.Offset, softBodyVertexElements.surfaceModelVertices, 0, modelPart.VertexCount, modelPart.VertexStride, SetDataOptions.None);

            /*if (!resetIndexBuffer)
            {
                if (modelPart.IndexCount != surfaceMesh.mDrawIndices.Count)
                {
                    modelPart.IndexBuffer.Dispose();
                    modelPart.IndexBuffer = new DynamicIndexBuffer(ImageNexusRTSGameEngine.GameInstance.GraphicsDevice, sizeof(int) * surfaceMesh.mDrawIndices.Count,
                                                                    BufferUsage.WriteOnly, IndexElementSize.ThirtyTwoBits);
                    resetIndexBuffer = true;

                    if (tmpDrawIndices.Length != surfaceMesh.mDrawIndices.Count)
                        Array.Resize<int>(ref tmpDrawIndices, surfaceMesh.mDrawIndices.Count);
                }
            }

            // Update Indices    
            surfaceMesh.mDrawIndices.CopyTo(tmpDrawIndices);
            modelPart.IndexBuffer.SetData<int>(tmpDrawIndices);*/

            // Update Wind ExternalForce
            const float range = 350.0f;
            softBody.ExternalAcceleration = new Vector3(MathUtils.RandomBetween(-range, range) * 1.5f, MathUtils.RandomBetween(-range, range), MathUtils.RandomBetween(-range, range));
        }

        // 6/24/2009
        private static Vector3[] ReadVertices(XmlNode node)
        {
            // Updated to use the following loop, rather than the LINQ statement,
            // since crashes.
            List<float> tmpFloats = new List<float>();
            char[] testChar = new char[1] { ' ' };
            foreach (var c in node.InnerText.Split(testChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Skip 'r/n/' entries
                if (c == "\r\n")
                    continue;

                tmpFloats.Add(float.Parse(c));
            }

            /*var floats = from c in node.InnerText.Split(' ')
                         select Single.Parse(c);*/

            Vector3[] vertices = new Vector3[tmpFloats.Count() / 3];
            for (int i = 0; i < tmpFloats.Count(); i += 3)
            {
                float x = tmpFloats.ElementAt(i + 0);
                float y = tmpFloats.ElementAt(i + 1);
                float z = tmpFloats.ElementAt(i + 2);

                vertices[i / 3] = new Vector3(x, y, z);
            }

            // clear List array
            tmpFloats.Clear();

            return vertices;
        }

        private static int[] ReadTetrahedra(XmlNode node)
        {
            // Updated to use the following loop, rather than the LINQ statement,
            // since crashes.
            List<int> tmpTets = new List<int>();
            char[] testChar = new char[1] { ' ' };
            foreach (var c in node.InnerText.Split(testChar, StringSplitOptions.RemoveEmptyEntries))
            {
                // Skip 'r/n/' entries
                if (c == "\r\n")
                    continue;

                tmpTets.Add(int.Parse(c));
            }

            /*var tet = from c in node.InnerText.Split(' ')
                      select UInt16.Parse(c);*/

            return tmpTets.ToArray();
        }

        // 6/25/2009 - Dispose
        #region IDisposable Members

        /// <summary>
        /// Disposes of unmanaged resources.
        /// </summary>
        /// <param name="disposing">Is this final dispose?</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of objects
                if (poleActor != null)
                    poleActor.Dispose();

                if (softBody != null)
                    softBody.Dispose();

                if (requests != null)
                {
                    requests.Clear();
                    requests = null;
                }

                // Null Refs
                poleActor = null;
                softBody = null;
                surfaceMesh = null;
            }

        }        

        void System.IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
