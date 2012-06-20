#region File Description
//-----------------------------------------------------------------------------
// SoftBodyModule.cs
//
// Ben Scharbach - XNA Community Game Platform
// Copyright (C) Image-Nexus, LLC. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !XBOX360

using StillDesign;
using StillDesign.PhysX;
using Spacewar.Common;

#endif

namespace Spacewar.PhysX
{
    public class SoftBodyModule
    {
        ObjMesh surfaceMesh;
        public SoftBody softBody;
        public Actor collideSphere;
        Vector3 position = Vector3.Zero;

        public SoftBodyModule(Vector3 pos, string fileName, Core core)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);

            Vector3[] vertices;
            int[] tetrahedraSingles;

            CreateVerticesTetra(out vertices,
                                out tetrahedraSingles,
                                doc.SelectSingleNode("/NXUSTREAM2/NxuPhysicsCollection/NxSoftBodyMeshDesc/vertices"),
                                doc.SelectSingleNode("/NXUSTREAM2/NxuPhysicsCollection/NxSoftBodyMeshDesc/tetrahedra"));

            SoftBodyMeshDescription softBodyMeshDesc = new SoftBodyMeshDescription();
            softBodyMeshDesc.VertexCount = vertices.Length;
            softBodyMeshDesc.TetrahedraCount = tetrahedraSingles.Length / 4; // Tetrahedras come in quadruples of ints

            softBodyMeshDesc.AllocateVertices(softBodyMeshDesc.VertexCount * MeshData.SizeOfVector3, MeshData.SizeOfVector3);
            softBodyMeshDesc.AllocateTetrahedra(softBodyMeshDesc.TetrahedraCount * sizeof(int) * 4, sizeof(int) * 4);


            foreach (Vector3 vertex in vertices)
            {
                softBodyMeshDesc.VertexStream.Write(vertex);
            }

            foreach (int tet in tetrahedraSingles)
            {
                softBodyMeshDesc.TetrahedraStream.Write(tet);
            }

            MemoryWriterStream memoryWriterStream = new MemoryWriterStream();

            Cooking cooking = new Cooking();
            cooking.InitializeCooking();
            CookingResult result = cooking.CookSoftBodyMesh(softBodyMeshDesc, memoryWriterStream);
            cooking.CloseCooking();

            SoftBodyMesh softBodyMesh = core.CreateSoftBodyMesh(new MemoryReaderStream(memoryWriterStream.ToArray()));

            surfaceMesh = new ObjMesh();
            fileName = fileName.Remove(fileName.Length - 3, 3);
            fileName += "obj";
            surfaceMesh.loadFromObjFile(fileName);
            surfaceMesh.buildTetraLinks(softBodyMeshDesc.VertexStream, softBodyMeshDesc.TetrahedraStream, softBodyMeshDesc.TetrahedraCount, doc.SelectSingleNode("/NXUSTREAM2/NxuPhysicsCollection/NxSoftBodyMeshDesc/links"));

            SoftBodyDescription desc = new SoftBodyDescription();
            desc.GlobalPose = Matrix.CreateTranslation(pos.X, pos.Y, pos.Z);
            desc.SoftBodyMesh = softBodyMesh;
            desc.MeshData = new MeshData(vertices.Length, tetrahedraSingles.Length, 0, MeshData.SizeOfVector3, MeshData.SizeOfVector3, 0, sizeof(int));
            desc.Flags |= SoftBodyFlags.Visualization;
            desc.CollisionGroup = (short)(core.Scenes[0].SoftBodies.Count + 1);

            softBody = core.Scenes[0].CreateSoftBody(desc);
            collideSphere = CreateSphere(pos, 16.0f, core);
            position = pos;
        }

        private Actor CreateSphere(Vector3 position, float radius, Core core)
        {
            ActorDescription actorDesc = new ActorDescription();
            BodyDescription bodyDesc = new BodyDescription();
            bodyDesc.BodyFlags = BodyFlag.DisableGravity;
            SphereShapeDescription sphereDesc = new SphereShapeDescription();
            sphereDesc.Radius = radius;
            sphereDesc.Mass = 0.0f;
            actorDesc.Shapes.Add(sphereDesc);
            actorDesc.BodyDescription = bodyDesc;
            actorDesc.Density = 1.0f;

            actorDesc.GlobalPose = Matrix.CreateTranslation(position.X, position.Y, position.Z);
            Actor actor = core.Scenes[0].CreateActor(actorDesc);
            return actor;
        }

        public Vector3 GetPosition()
        {
            return position;
        }

        public void Setposition(Vector3 pos)
        {
            Vector3 translation = pos - position;
            Vector3[] positions = new Vector3[softBody.MeshData.NumberOfVertices];
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] += translation;
            }
            softBody.SetPositions(positions);

            collideSphere.GlobalPose = Matrix.CreateTranslation(pos);
        }

        public void GetMeshData(out VertexPositionColor[] vertex, out ArrayList triangles)
        {

            vertex = surfaceMesh.vertex;
            triangles = surfaceMesh.mTriangles;
        }

        private void ComputePosition()
        {
            position = Vector3.Zero;
            Vector3[] vertices = softBody.MeshData.PositionsStream.ToVector3Array();
            for (int i = 0; i < vertices.Length; i++)
            {
                position += vertices[i];
            }
            position /= (float)(vertices.Length);
        }

        public void Update()
        {
            ComputePosition();
            surfaceMesh.UpdateTetraLinks(softBody.MeshData);
            surfaceMesh.Update();
            ////////////////////////////////////////////////////////////////
            softBody.ExternalAcceleration += collideSphere.LinearVelocity;

            if (softBody.StretchingStiffness < 0.005f)
            {
                softBody.StretchingStiffness += 0.001f;
            }


        }

        private void CreateVerticesTetra(out Vector3[] vertices, out int[] tetrahedraSingles, XmlNode nodeVertice, XmlNode nodeTetra)
        {
            vertices = ReadVertices(nodeVertice);
            tetrahedraSingles = ReadTetrahedra(nodeTetra);
        }

        private int[] ReadTetrahedra(XmlNode node)
        {
            string innerText = node.InnerText;
            string[] tetrahedraStrings = innerText.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            List<int> tetrahedra = new List<int>();

            int tet;
            for (int x = 0; x < tetrahedraStrings.Length; x++)
            {
                if (Int32.TryParse(tetrahedraStrings[x], out tet) == false)
                    continue;

                tetrahedra.Add(tet);
            }

            return tetrahedra.ToArray();
        }

        private Vector3[] ReadVertices(XmlNode node)
        {
            string innerText = node.InnerText;

            string[] floatStrings = innerText.Split(' ');
            List<float> floats = new List<float>();

            float f;
            for (int x = 0; x < floatStrings.Length; x++)
            {
                if (Single.TryParse(floatStrings[x], out f) == false)
                    continue;

                floats.Add(f);
            }

            Vector3[] vertices = new Vector3[floats.Count / 3];

            float vx, vy, vz;
            for (int i = 0; i < floats.Count; i += 3)
            {
                vx = floats[i + 0];
                vy = floats[i + 1];
                vz = floats[i + 2];

                vertices[i / 3] = new Vector3(vx, vy, vz);
            }

            return vertices;
        }

    };
}