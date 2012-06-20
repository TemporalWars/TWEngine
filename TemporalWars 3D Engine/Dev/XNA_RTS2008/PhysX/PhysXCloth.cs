#region File Description
//-----------------------------------------------------------------------------
// PhysXCloth.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using TWEngine.InstancedModels;

#if !XBOX360 // 6/22/2009
using StillDesign.PhysX;
#endif

namespace TWEngine.PhysX
{
    // 6/23/2009
    class PhysXCloth : System.IDisposable
    {
        private Cloth _cloth;
        private Actor _flagPoleActor;

        // 6/22/2009 - Cloth Struct Elements
        struct ClothVertexElements
        {
            public ClothVertexElements(int vertexCount)
            {
                Positions = new Vector3[vertexCount];
                Normals = new Vector3[vertexCount];
            }

            public readonly Vector3[] Positions;
            public readonly Vector3[] Normals;
        }
        ClothVertexElements _clothVertexElements;
        VertexElement _vertexElementPosition;
        VertexElement _vertexElementNormal;

        // 6/21/2009 - Create Cloth
        internal bool CreateCloth(InstancedModelPart modelPart)
        {
            try // 8/20/2009
            {
                // Get VertexDeclaration information, needed for the Receive Buffers.
                PhysXEngine.GetVertexElement(modelPart.VertexDeclaration, VertexElementUsage.Position,
                                             out _vertexElementPosition);
                PhysXEngine.GetVertexElement(modelPart.VertexDeclaration, VertexElementUsage.Normal,
                                             out _vertexElementNormal);

                // Populate Positions array
                _clothVertexElements = new ClothVertexElements(modelPart.VertexCount);
                modelPart.DynamicVertexBuffer.GetData(_vertexElementPosition.Offset, _clothVertexElements.Positions, 0,
                                                      modelPart.VertexCount, modelPart.VertexStride);

                // Populate Normals array
                modelPart.DynamicVertexBuffer.GetData(_vertexElementNormal.Offset, _clothVertexElements.Normals, 0,
                                                      modelPart.VertexCount, modelPart.VertexStride);


                // Cloth Mesc Desc
                var clothMeshDesc = new ClothMeshDescription();

                clothMeshDesc.AllocateVertices<Vector3>(_clothVertexElements.Positions.Length);
                clothMeshDesc.AllocateTriangles<ushort>(modelPart.IndexData.Length/3);

                clothMeshDesc.VertexCount = _clothVertexElements.Positions.Length;
                clothMeshDesc.TriangleCount = modelPart.IndexData.Length/3;

                clothMeshDesc.VerticesStream.SetData(_clothVertexElements.Positions);
                clothMeshDesc.TriangleStream.SetData(modelPart.IndexData);

                // Set to use 16-bit indices
                clothMeshDesc.Flags = MeshFlag.Indices16Bit;

                // Cook Mesh
                MemoryStream stream;
                if (!PhysXEngine.CookMesh_Cloth(clothMeshDesc, out stream))
                    //throw new InvalidOperationException("Unable to 'Cook' the clothMesh data.");
                    return false;

                // Create ClothMesh
                ClothMesh clothMesh = PhysXEngine.PhysXCore.CreateClothMesh(stream);

                var clothDesc = new ClothDescription
                                    {
                                        ClothMesh = clothMesh,
                                        Flags =
                                            ClothFlag.Gravity | ClothFlag.Bending | ClothFlag.CollisionTwoway |
                                            ClothFlag.Visualization,
                                        GlobalPose = Matrix.CreateTranslation(0, 0, 0),
                                        SolverIterations = 2
                                    };

                clothDesc.MeshData.AllocatePositions<Vector3>(_clothVertexElements.Positions.Length);
                clothDesc.MeshData.AllocateIndices<ushort>(modelPart.IndexData.Length);
                clothDesc.MeshData.AllocateNormals<Vector3>(_clothVertexElements.Normals.Length);

                clothDesc.MeshData.MaximumVertices = _clothVertexElements.Positions.Length;
                clothDesc.MeshData.MaximumIndices = modelPart.IndexData.Length;

                clothDesc.MeshData.NumberOfVertices = _clothVertexElements.Positions.Length;
                clothDesc.MeshData.NumberOfIndices = modelPart.IndexData.Length;

                // Create Cloth in Scene instance
                _cloth = PhysXEngine.PhysXScene.CreateCloth(clothDesc);

                // Attach Flag Pole
                var flagPoleActorDesc = new ActorDescription
                                            {
                                                GlobalPose = Matrix.CreateTranslation(-45, 0, 0),
                                                Shapes = {new BoxShapeDescription(1.0f, 100.0f, 1.0f)}
                                            };

                _flagPoleActor = PhysXEngine.PhysXScene.CreateActor(flagPoleActorDesc);

                //_cloth.AttachToShape(_flagPoleActor.Shapes.Single(), 0);
                _cloth.AttachVertexToShape(0, _flagPoleActor.Shapes.Single(), Vector3.Zero, 0);
                _cloth.AttachVertexToShape(98, _flagPoleActor.Shapes.Single(), new Vector3(0, 80, 0), 0);
                _cloth.WindAcceleration = new Vector3(450, 0, 300);
                _cloth.BendingStiffness = 0.1f;

                return true;
            }
            catch
            {
                Debug.WriteLine("Method Error: (CreatCloth) Creation of PhyX Cloth failed!");
                return false;
            }
        }

        // 6/22/2009
        internal void UpdateClothBuffers(InstancedModelPart modelPart)
        {
            // 8/1/2009 - Make sure _cloth is created.
            if (_cloth == null)
                CreateCloth(modelPart);
            
            // Retrieve updated data from PhysX Streams
            if (_cloth != null)
            {
                _cloth.MeshData.PositionsStream.GetData(_clothVertexElements.Positions);
                _cloth.MeshData.NormalsStream.GetData(_clothVertexElements.Normals);
                _cloth.MeshData.IndicesStream.GetData(modelPart.IndexData);
            }

            // Update Positions
            modelPart.DynamicVertexBuffer.SetData(_vertexElementPosition.Offset, _clothVertexElements.Positions, 0, modelPart.VertexCount, modelPart.VertexStride, SetDataOptions.Discard);
            // Update Normals
            modelPart.DynamicVertexBuffer.SetData(_vertexElementNormal.Offset, _clothVertexElements.Normals, 0, modelPart.VertexCount, modelPart.VertexStride, SetDataOptions.Discard);
            // Update Indices
            modelPart.DynamicIndexBuffer.SetData(0, modelPart.IndexData, 0, modelPart.IndexCount, SetDataOptions.Discard);
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
                if (_cloth != null)
                    _cloth.Dispose();

                if (_flagPoleActor != null)
                    _flagPoleActor.Dispose();

                // Null Refs
                _cloth = null;
                _flagPoleActor = null;

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
