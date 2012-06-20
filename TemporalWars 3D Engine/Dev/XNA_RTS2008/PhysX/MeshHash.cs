#region File Description
//-----------------------------------------------------------------------------
// MeshHash.cs
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
    // 3/10/2010: NOTE: In order to give the namespace the XML doc, must do it this way;
    /// <summary>
    /// The <see cref="TWEngine.PhysX"/> namespace contains the classes
    /// which make up the entire <see cref="PhysX"/> component.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    public struct MeshHashRoot
    {
        public int first;
        public int timeStamp;
    };

    public struct MeshHashEntry
    {
        public int itemIndex;
        public int next;
    };

    public class MeshHash
    {
        private int mHashIndexSize = 17011;
        private float mSpacing;
        private float mInvSpacing;
        private int mTime;
        private MeshHashRoot[] mHashIndex;
        private ArrayList mEntries;

        public MeshHash()
        {
            mHashIndex = new MeshHashRoot[mHashIndexSize];

            mTime = 1;
            for (int i = 0; i < mHashIndexSize; i++)
            {
                mHashIndex[i].first = -1;
                mHashIndex[i].timeStamp = 0;
            }
            mSpacing = 0.25f;
            mInvSpacing = 1.0f / mSpacing;
            mEntries = new ArrayList();
        }

        public void setGridSpacing(float spacing)
        {
            mSpacing = spacing;
            mInvSpacing = 1.0f / spacing;
            reset();
        }

        public float getGridSpacing()
        {
            return 1.0f / mInvSpacing;
        }

        public void reset()
        {
            mTime++;
            if (mEntries.Count > 0)
                mEntries.Clear();
        }

        public void add(bound bounds, int itemIndex)
        {
            int x = 0; int y = 0; int z = 0;
            int x1 = 0; int y1 = 0; int z1 = 0;
            int x2 = 0; int y2 = 0; int z2 = 0;

            cellCoordOf(bounds.Min, ref x1, ref y1, ref z1);
            cellCoordOf(bounds.Max, ref x2, ref y2, ref z2);

            MeshHashEntry entry;
            entry.itemIndex = itemIndex;

            for (x = x1; x <= x2; x++)
            {
                for (y = y1; y <= y2; y++)
                {
                    for (z = z1; z <= z2; z++)
                    {
                        int h = hashFunction(x, y, z);
                        int n = mEntries.Count;
                        if (mHashIndex[h].timeStamp != mTime || mHashIndex[h].first < 0)
                            entry.next = -1;
                        else
                            entry.next = mHashIndex[h].first;
                        mHashIndex[h].first = n;
                        mHashIndex[h].timeStamp = mTime;
                        mEntries.Add(entry);
                    }
                }
            }
        }

        public void add(Vector3 pos, int itemIndex)
        {
            int x = 0; int y = 0; int z = 0;
            cellCoordOf(pos, ref x, ref y, ref z);
            MeshHashEntry entry;
            entry.itemIndex = itemIndex;

            int h = hashFunction(x, y, z);
            int n = mEntries.Count;
            if (mHashIndex[h].timeStamp != mTime || mHashIndex[h].first < 0)
                entry.next = -1;
            else
                entry.next = mHashIndex[h].first;
            mHashIndex[h].first = n;
            mHashIndex[h].timeStamp = mTime;
            mEntries.Add(entry);
        }

        public void query(Bounds3 bounds, ref ArrayList itemIndices, int maxIndices)
        {
            int x = 0; int y = 0; int z = 0;
            int x1 = 0; int y1 = 0; int z1 = 0;
            int x2 = 0; int y2 = 0; int z2 = 0;

            cellCoordOf(bounds.Min, ref x1, ref y1, ref z1);
            cellCoordOf(bounds.Max, ref x2, ref y2, ref z2);
            itemIndices.Clear();

            for (x = x1; x <= x2; x++)
            {
                for (y = y1; y <= y2; y++)
                {
                    for (z = z1; z <= z2; z++)
                    {
                        int h = hashFunction(x, y, z);
                        if (mHashIndex[h].timeStamp != mTime)
                            continue;
                        int i = mHashIndex[h].first;
                        while (i >= 0)
                        {
                            MeshHashEntry entry = (MeshHashEntry)(mEntries[i]);
                            itemIndices.Add(entry.itemIndex);
                            if (maxIndices >= 0 && (int)itemIndices.Count >= maxIndices)
                                return;
                            i = entry.next;
                        }
                    }
                }
            }
        }

        public void queryUnique(Bounds3 bounds, ref ArrayList itemIndices, int maxIndices)
        {
            query(bounds, ref itemIndices, maxIndices);
        }

        void query(Vector3 pos, ref ArrayList itemIndices, int maxIndices)
        {
            int x = 0; int y = 0; int z = 0;
            cellCoordOf(pos, ref x, ref y, ref z);
            itemIndices.Clear();

            int h = hashFunction(x, y, z);

            if (mHashIndex[h].timeStamp != mTime)
                return;
            int i = mHashIndex[h].first;
            while (i >= 0)
            {
                MeshHashEntry entry = (MeshHashEntry)(mEntries[i]);
                itemIndices.Add(entry.itemIndex);
                if (maxIndices >= 0 && (int)itemIndices.Count >= maxIndices)
                    return;
                i = entry.next;
            }
        }

        public void queryUnique(Vector3 pos, ref ArrayList itemIndices, int maxIndices)
        {
            query(pos, ref itemIndices, maxIndices);
        }

        private void cellCoordOf(Vector3 v, ref int xi, ref int yi, ref int zi)
        {
            xi = (int)(v.X * mInvSpacing); if (v.X < 0.0f) xi--;
            yi = (int)(v.Y * mInvSpacing); if (v.Y < 0.0f) yi--;
            zi = (int)(v.Z * mInvSpacing); if (v.Z < 0.0f) zi--;
        }

        private int hashFunction(int xi, int yi, int zi)
        {
            uint h = (uint)((xi * 92837111) ^ (yi * 689287499) ^ (zi * 283923481));
            return (int)(h % mHashIndexSize);
        }

    }
}