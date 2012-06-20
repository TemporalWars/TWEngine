#region File Description
//-----------------------------------------------------------------------------
// bound.cs
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
using TWEngine.GameCamera;

#endif

namespace TWEngine.PhysX
{
    public class bound
    {
        public Vector3 Min;
        public Vector3 Max;
        public bound()
        {
            Min = new Vector3();
            Max = new Vector3();
        }
        public void Include(Vector3 val)
        {
            if (Min.X == 0) Min.X = val.X;
            if (Min.Y == 0) Min.Y = val.Y;
            if (Min.Z == 0) Min.Z = val.Z;
            if (Max.X == 0) Max.X = val.X;
            if (Max.Y == 0) Max.Y = val.Y;
            if (Max.Z == 0) Max.Z = val.Z;

            if (val.X < Min.X) Min.X = val.X;
            if (val.Y < Min.Y) Min.Y = val.Y;
            if (val.Z < Min.Z) Min.Z = val.Z;
            if (val.X > Max.X) Max.X = val.X;
            if (val.Y > Max.Y) Max.Y = val.Y;
            if (val.Z > Max.Z) Max.Z = val.Z;
        }

    };

    public struct ObjMeshTriangle
    {
        public void init()
        {
            vertexNr = new int[3];
            vertexNr[0] = -1; vertexNr[1] = -1; vertexNr[2] = -1;
            normalNr = new int[3];
            normalNr[0] = -1; normalNr[1] = -1; normalNr[2] = -1;
        }
        public void set(int v0, int v1, int v2, int mat)
        {
            vertexNr[0] = v0; vertexNr[1] = v1; vertexNr[2] = v2;
            normalNr[0] = v0; normalNr[1] = v1; normalNr[2] = v2;
        }
        public bool containsVertexNr(int vNr)
        {
            return vNr == vertexNr[0] || vNr == vertexNr[1] || vNr == vertexNr[2];
        }
        public int[] vertexNr;
        public int[] normalNr;
    };

    public struct ObjMeshTetraLink
    {
        public int tetraNr;
        public Vector3 barycentricCoords;
    };

    public struct TexCoord
    {
        public float u;
        public float v;
    };

    public class ObjMesh
    {
        public ArrayList mTriangles;
        public ArrayList mVertices;
        public ArrayList mDrawIndices;

        public int maxVerticesPerFace = 20;

        public ArrayList mNormals;
        public ArrayList mTetraLinks;

        public string mName;
        public bound mBounds;

        public VertexPositionColor[] vertex;

        private static void parseRef(string s, int[] nr)
        {
            int i = 0;
            int j = 0;
            int k = 0;
            for (k = 0; k < 3; k++)
                nr[k] = -1;

            int len = s.Length;
            string isa = " ";

            for (k = 0; k < 3; k++)
            {
                j = 0;
                while (i < len && s[i] != '/')
                {
                    isa += s[i];
                    i++;
                    j++;
                }
                i++;
                if (j > 0)
                {
                    nr[k] = (int)(Single.Parse(isa));
                    isa = " ";
                }
            }
        }

        public ObjMesh()
        {
            mTriangles = new ArrayList();
            mVertices = new ArrayList();
            mDrawIndices = new ArrayList();

            mNormals = new ArrayList();
            mTetraLinks = new ArrayList();
            mBounds = new bound();
            clear();
        }

        public ObjMesh(ObjMesh obj)
        {
            mTriangles = new ArrayList();
            mVertices = new ArrayList();
            mDrawIndices = new ArrayList();

            mNormals = new ArrayList();
            mTetraLinks = new ArrayList();
            mBounds = new bound();
            clear();

            for (int i = 0; i < obj.mTriangles.Count; i++)
            {
                this.mTriangles.Add(obj.mTriangles[i]);
            }
            for (int i = 0; i < obj.mVertices.Count; i++)
            {
                this.mVertices.Add(obj.mVertices[i]);
            }
            for (int i = 0; i < obj.mDrawIndices.Count; i++)
            {
                this.mDrawIndices.Add(obj.mDrawIndices[i]);
            }
            for (int i = 0; i < obj.mNormals.Count; i++)
            {
                this.mNormals.Add(obj.mNormals[i]);
            }
            for (int i = 0; i < obj.mTetraLinks.Count; i++)
            {
                this.mTetraLinks.Add(obj.mTetraLinks[i]);
            }

            this.mBounds.Max = obj.mBounds.Max;
            this.mBounds.Min = obj.mBounds.Min;
        }

        // 6/24/2009 - Predicate to remove String which are Empty.
        private static bool IsStringEmpty(string stringValue)
        {
            if (string.IsNullOrEmpty(stringValue))
                return true;
            else
                return false;
        }

        public bool loadFromObjFile(string filename)
        {
            StreamReader SR;
            SR = File.OpenText(filename);

            clear();
            List<string> subs = new List<string>(maxVerticesPerFace); // 6/24/2009 - Was an []
            List<string> tabs = new List<string>(maxVerticesPerFace); // 6/25/2009
            int i = 0;
            int j = 0;
            Vector3 v;
            ObjMeshTriangle t;

            ArrayList centermVertices = new ArrayList();
            ArrayList centermTexCoords = new ArrayList();
            ArrayList centerNormals = new ArrayList();
            t = new ObjMeshTriangle();

            int k = 0;
            while (!SR.EndOfStream)
            {
                string s = string.Empty;
                s = SR.ReadLine();
                k++;
                //Console.WriteLine(i);
                if (string.Compare(s, 0, "v ", 0, 2) == 0)
                {	// vertex
                    tabs.Clear();
                    //s = s.Replace('.', ',');
                    tabs.AddRange(s.Split(' '));

                    // 6/25/2009 - Remove all empty placeholders
                    tabs.RemoveAll(IsStringEmpty);  
                    
                    float.TryParse(tabs[1], out v.X);
                    float.TryParse(tabs[2], out v.Y);
                    float.TryParse(tabs[3], out v.Z);
                    mVertices.Add(v);
                    
                }
                else if (string.Compare(s, 0, "vn ", 0, 3) == 0)
                {	// normal
                    tabs.Clear();
                    //s = s.Replace('.', ',');
                    tabs.AddRange(s.Split(' '));

                    // 6/25/2009 - Remove all empty placeholders
                    tabs.RemoveAll(IsStringEmpty);  

                    float.TryParse(tabs[1], out v.X);
                    float.TryParse(tabs[2], out v.Y);
                    float.TryParse(tabs[3], out v.Z);
                    mNormals.Add(v);
                }
                else if (string.Compare(s, 0, "f ", 0, 2) == 0)
                {	
                    // face 
                    subs.Clear();
                    subs.AddRange(s.Split(' '));

                    // 6/24/2009 - Remove all empty placeholders
                    subs.RemoveAll(IsStringEmpty);  

                    int[] vertNr = new int[maxVerticesPerFace];
                    int[] texNr = new int[maxVerticesPerFace];
                    int[] normalNr = new int[maxVerticesPerFace];
                    int nr = subs.Count;
                    for (i = 0; i < nr - 1; i++)
                    {
                        int[] refs = new int[3];
                        parseRef(subs[i + 1], refs);
                        vertNr[i] = refs[0] - 1;
                        texNr[i] = refs[1] - 1;
                        normalNr[i] = refs[2] - 1;
                    }
                    if (nr <= 4)
                    {	// simple non-singular triangle or quad
                        if (vertNr[0] != vertNr[1] && vertNr[1] != vertNr[2] && vertNr[2] != vertNr[0])
                        {
                            t.init();
                            t.vertexNr[0] = vertNr[0];
                            t.vertexNr[1] = vertNr[1];
                            t.vertexNr[2] = vertNr[2];
                            t.normalNr[0] = normalNr[0];
                            t.normalNr[1] = normalNr[1];
                            t.normalNr[2] = normalNr[2];
                            mTriangles.Add(t);
                        }
                        if (nr == 4)
                        {	// non-singular quad -> generate a second triangle
                            if (vertNr[2] != vertNr[3] && vertNr[3] != vertNr[0] && vertNr[0] != vertNr[2])
                            {
                                t.init();
                                t.vertexNr[0] = vertNr[2];
                                t.vertexNr[1] = vertNr[3];
                                t.vertexNr[2] = vertNr[0];
                                t.normalNr[0] = normalNr[2];
                                t.normalNr[1] = normalNr[3];
                                t.normalNr[2] = normalNr[0];
                                mTriangles.Add(t);
                            }
                        }
                    }
                    else
                    {	// polygonal face
                        // compute center properties
                        Vector3 centerPos = new Vector3();
                        for (i = 0; i < nr; i++)
                        {
                            centerPos += (Vector3)(mVertices[vertNr[i]]);
                        }
                        centerPos /= (float)nr;
                        Vector3 d1 = centerPos - (Vector3)(mVertices[vertNr[0]]);
                        Vector3 d2 = centerPos - (Vector3)(mVertices[vertNr[1]]);

                        float a = (d1.Y * d2.Z) - (d1.Z * d2.Y);
                        float b = (d1.Z * d2.X) - (d1.X * d2.Z);
                        float c = (d1.X * d2.Y) - (d1.Y * d2.X);

                        Vector3 centerNormal = new Vector3(a, b, c);
                        centerNormal.Normalize();

                        // add center vertex
                        centermVertices.Add(centerPos);
                        centerNormals.Add(centerNormal);

                        // add surrounding elements
                        for (i = 0; i < nr; i++)
                        {
                            j = i + 1;
                            if (j >= nr)
                                j = 0;
                            t.init();
                            t.vertexNr[0] = mVertices.Count + centermVertices.Count - 1;
                            t.vertexNr[1] = vertNr[i];
                            t.vertexNr[2] = vertNr[j];

                            t.normalNr[0] = mNormals.Count + centerNormals.Count - 1;
                            t.normalNr[1] = normalNr[i];
                            t.normalNr[2] = normalNr[j];

                            mTriangles.Add(t);
                        }
                    }
                }
            }
            SR.Close();

            // new center mVertices are inserted here.
            // If they were inserted when generated, the vertex numbering would be corrupted
            for (i = 0; i < centermVertices.Count; i++)
                mVertices.Add(centermVertices[i]);
            for (i = 0; i < centerNormals.Count; i++)
                mNormals.Add(centerNormals[i]);

            bound B = new bound();
            B.Include(new Vector3(5, -10, 20));


            updateBounds();

            return true;
        }

        // 6/25/2009
        /// <summary>
        /// Loads the TetraLinks directly from the '.tet' file, created from
        /// the PhysXViewer.
        /// </summary>
        /// <param name="filename">The '.Tet.' file to load the tetraLinks from.</param>
        public void LoadTetraLinksFromFile(string filename)
        {
            StreamReader SR;
            SR = File.OpenText(filename);

            mTetraLinks.Clear();           
            ObjMeshTetraLink tmpLink = new ObjMeshTetraLink();


            while (!SR.EndOfStream)
            {
                string s = string.Empty;
                s = SR.ReadLine();

                if (string.Compare(s, 0, "l ", 0, 2) == 0)
                {	// links
                    string[] tab;                   
                    tab = s.Split(' ');
                    bool success;
                    success = int.TryParse(tab[1], out tmpLink.tetraNr);
                    success &= float.TryParse(tab[2], out tmpLink.barycentricCoords.X);
                    success &= float.TryParse(tab[3], out tmpLink.barycentricCoords.Y);
                    success &= float.TryParse(tab[4], out tmpLink.barycentricCoords.Z);
                    if (success) mTetraLinks.Add(tmpLink);
                }

            }
            SR.Close();
        }

        // 6/24/2009
        /// <summary>
        /// If no TetraLinks file is availble, then call this method to manually build the 
        /// 'tetraLinks'.
        /// </summary>
        /// <param name="verts"></param>
        /// <param name="ind"></param>
        /// <param name="numTets"></param>
        public void buildTetraLinks(ref SoftBodyVertexElements softBodyVertexElements, int numTets)
        {
            // Extract Tetra Data
            Vector3[] vertices = softBodyVertexElements.tetraVertices;
            int[] indices = softBodyVertexElements.tetrahedraSingles;
            // Extract SurfaceMesh Data
            mVertices.Clear();
            mVertices.AddRange(softBodyVertexElements.surfaceModelVertices);

            mTetraLinks.Clear();

            MeshHash hash = new MeshHash();

            // hash tetrahedra for faster search
            Vector3 d = mBounds.Min - mBounds.Max;
            float dista = (float)(System.Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z));
            float test = (float)(dista * 0.1f);
            hash.setGridSpacing((float)(dista * 0.1f));

            for (int i = 0; i < numTets; i++)
            {
                bound tetraBounds = new bound();

                int ix = indices[4 * i];
                Vector3 vect = vertices[ix];
                tetraBounds.Include(vertices[ix]);
                ix = indices[4 * i + 1];
                vect = vertices[ix];
                tetraBounds.Include(vertices[ix]);
                ix = indices[4 * i + 2];
                vect = vertices[ix];
                tetraBounds.Include(vertices[ix]);
                ix = indices[4 * i + 3];
                vect = vertices[ix];
                tetraBounds.Include(vertices[ix]);
                hash.add(tetraBounds, i);
            }

            for (uint i = 0; i < mVertices.Count; i++)
            {
                ObjMeshTetraLink tmpLink = new ObjMeshTetraLink();

                Vector3 triVert = (Vector3)(mVertices[(int)(i)]);
                ArrayList itemIndices = new ArrayList();
                hash.queryUnique(triVert, ref itemIndices, -1);

                float minDist = 0.0f;

                int num, isize;
                num = isize = itemIndices.Count;
                if (num == 0)
                    num = (int)(numTets);

                for (int k = 0; k < num; k++)
                {
                    int j = k;
                    if (isize > 0)
                        j = (int)(itemIndices[k]);

                    int ix = indices[4 * j];
                    Vector3 p0 = vertices[ix];
                    ix = indices[4 * j + 1];
                    Vector3 p1 = vertices[ix];
                    ix = indices[4 * j + 2];
                    Vector3 p2 = vertices[ix];
                    ix = indices[4 * j + 3];
                    Vector3 p3 = vertices[ix];

                    Vector3 b = computeBaryCoords(triVert, p0, p1, p2, p3);

                    // is the vertex inside the tetrahedron? If yes we take it
                    if (b.X >= 0.0f && b.Y >= 0.0f && b.Z >= 0.0f && (b.X + b.Y + b.Z) <= 1.0f)
                    {
                        tmpLink.barycentricCoords = b;
                        tmpLink.tetraNr = j;
                        break;
                    }

                    // otherwise, if we are not in any tetrahedron we take the closest one
                    float dist = 0.0f;
                    if (b.X + b.Y + b.Z > 1.0f)
                    {
                        dist = b.X + b.Y + b.Z - 1.0f;
                    }
                    if (b.X < 0.0f)
                    {
                        dist = (-b.X < dist) ? dist : -b.X;
                    }
                    if (b.Y < 0.0f)
                    {
                        dist = (-b.Y < dist) ? dist : -b.Y;
                    }
                    if (b.Z < 0.0f)
                    {
                        dist = (-b.Z < dist) ? dist : -b.Z;
                    }

                    if (k == 0 || dist < minDist)
                    {
                        minDist = dist;
                        tmpLink.barycentricCoords = b;
                        tmpLink.tetraNr = j;
                    }
                }
                mTetraLinks.Add(tmpLink);
            }
            vertex = new VertexPositionColor[mTriangles.Count * 3];
        }


        public bool UpdateTetraLinks(ref SoftBodyVertexElements softBodyVertexElements)
        {
            if (mTetraLinks.Count != softBodyVertexElements.surfaceModelVertices.Length)
                return false;            

            //Vector3[] vertices = tetraMeshData.PositionsStream.GetData<Vector3>();
            //int[] indices = tetraMeshData.IndicesStream.GetData<int>();

            Vector3[] vertices = softBodyVertexElements.tetraVertices;
            int[] indices = softBodyVertexElements.tetrahedraSingles;

            // 6/25/2009: Was 'mVertices', but changed to use the struct.
            //for (int i = 0; i < mVertices.Count; i++)
            for (int i = 0; i < softBodyVertexElements.surfaceModelVertices.Length; i++)
            {
                ObjMeshTetraLink temp = (ObjMeshTetraLink)(mTetraLinks[i]);

                int ix = indices[4 * temp.tetraNr];
                Vector3 p0 = vertices[ix];
                ix = indices[4 * temp.tetraNr + 1];
                Vector3 p1 = vertices[ix];
                ix = indices[4 * temp.tetraNr + 2];
                Vector3 p2 = vertices[ix];
                ix = indices[4 * temp.tetraNr + 3];
                Vector3 p3 = vertices[ix];

                Vector3 b = temp.barycentricCoords;
                softBodyVertexElements.surfaceModelVertices[i] = p0 * b.X + p1 * b.Y + p2 * b.Z + p3 * (1.0f - b.X - b.Y - b.Z);

            }

            return true;
        }

        // 6/25/2009 - Update Normals
        private static void UpdateNormals(ref SoftBodyVertexElements softBodyVertexElements)
        {
            return;
        }

        // 6/26/2009 - Update Indices
        public void UpdateIndices()
        {
            if (mVertices.Count == 0)
                return;            

            mDrawIndices.Clear();
            ObjMeshTriangle t = new ObjMeshTriangle();
            for (int i = 0; i < (int)mTriangles.Count; i++)
            {
                t = (ObjMeshTriangle)(mTriangles[i]);
                mDrawIndices.Add(t.vertexNr[0]);
                mDrawIndices.Add(t.vertexNr[1]);
                mDrawIndices.Add(t.vertexNr[2]);
            }

            /*int ind = 0;
            Color col = new Color(255, 0, 0);
            for (int x = 0; x < mTriangles.Count; x++)
            {
                vertex[x * 3 + 0].Position = (Vector3)(mVertices[(int)(mDrawIndices[ind++])]);
                vertex[x * 3 + 0].Color = col;
                vertex[x * 3 + 1].Position = (Vector3)(mVertices[(int)(mDrawIndices[ind++])]);
                vertex[x * 3 + 1].Color = col;
                vertex[x * 3 + 2].Position = (Vector3)(mVertices[(int)(mDrawIndices[ind++])]);
                vertex[x * 3 + 2].Color = col;
            }*/
        }

        public void clear()
        {
            mTriangles.Clear();
            mNormals.Clear();
            mVertices.Clear();
            mBounds = new bound();
            mName = "";
        }

        public int getNumTriangles()
        {
            return mTriangles.Count;
        }

        public ObjMeshTriangle getTriangle(int i)
        {
            return (ObjMeshTriangle)(mTriangles[i]);
        }

        public void getTriangleBounds(int i, bound bounds)
        {
            ObjMeshTriangle mt = (ObjMeshTriangle)(mTriangles[i]);
            bounds = new bound();
            Vector3 vect = (Vector3)(mVertices[mt.vertexNr[0]]);
            bounds.Include(vect);
            vect = (Vector3)(mVertices[mt.vertexNr[1]]);
            bounds.Include(vect);
            vect = (Vector3)(mVertices[mt.vertexNr[2]]);
            bounds.Include(vect);
        }

        public int getNumVertices()
        {
            return (int)mVertices.Count;
        }

        public Vector3 getVertex(int i)
        {
            return (Vector3)(mVertices[i]);
        }

        public void setDiagonal(float diagonal)
        {
            Vector3 d = mBounds.Min - mBounds.Max;
            float dista = (float)(System.Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z));
            if (dista == 0.0f)
                return;
            float s = diagonal / dista;
            for (int i = 0; i < mVertices.Count; i++)
                mVertices[i] = (Vector3)(mVertices[i]) * s;
            updateBounds();
        }

        public void setCenter(Vector3 center)
        {
            Vector3 d = center - (mBounds.Min + mBounds.Max) * 0.5f;
            for (int i = 0; i < mVertices.Count; i++)
                mVertices[i] = (Vector3)(mVertices[i]) + d;
            updateBounds();
        }

        public void getBounds(bound bounds)
        {
            bounds = mBounds;
        }

        public string getName()
        {
            return mName;
        }

        private void updateBounds()
        {
            mBounds = new bound();
            for (int i = 0; i < mVertices.Count; i++)
                mBounds.Include((Vector3)(mVertices[i]));
        }

        private static Vector3 computeBaryCoords(Vector3 vertex, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 baryCoords;

            Vector3 q = vertex - p3;
            Vector3 q0 = p0 - p3;
            Vector3 q1 = p1 - p3;
            Vector3 q2 = p2 - p3;

            Matrix m = new Matrix();
            m.M11 = q0.X;
            m.M21 = q0.Y;
            m.M31 = q0.Z;
            m.M12 = q1.X;
            m.M22 = q1.Y;
            m.M32 = q1.Z;
            m.M13 = q2.X;
            m.M23 = q2.Y;
            m.M33 = q2.Z;
            float det = computeDeterminant(m);

            m.M11 = q.X;
            m.M21 = q.Y;
            m.M31 = q.Z;
            baryCoords.X = computeDeterminant(m);

            m.M11 = q0.X;
            m.M21 = q0.Y;
            m.M31 = q0.Z;
            m.M12 = q.X;
            m.M22 = q.Y;
            m.M32 = q.Z;
            baryCoords.Y = computeDeterminant(m);

            m.M12 = q1.X;
            m.M22 = q1.Y;
            m.M32 = q1.Z;
            m.M13 = q.X;
            m.M23 = q.Y;
            m.M33 = q.Z;
            baryCoords.Z = computeDeterminant(m);

            if (det != 0.0f)
                baryCoords /= det;

            return baryCoords;
        }

        public static float computeDeterminant(Matrix mat)
        {
            return mat.M11 * mat.M22 * mat.M33 + mat.M12 * mat.M23 * mat.M31 + mat.M13 * mat.M21 * mat.M32
                 - mat.M13 * mat.M22 * mat.M31 - mat.M12 * mat.M21 * mat.M33 - mat.M11 * mat.M23 * mat.M32;
        }

    }
}
