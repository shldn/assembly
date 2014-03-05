﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Triangle
{
    public Vector3[] pts = new Vector3[3];
}

public class Tetrahedron
{
    private Vector3[] pts = new Vector3[4];
    private Plane[] planes = new Plane[4];
    private Vector3 midPt = new Vector3();

    public Vector3[] Pts { get { return pts; } }


    public Tetrahedron(Vector3[] pts_)
    {
        pts = pts_;
        midPt = 0.25f * (pts[0] + pts[1] + pts[2] + pts[3]);
        planes[0] = new Plane(pts[0], pts[1], pts[2]);
        planes[1] = new Plane(pts[0], pts[1], pts[3]);
        planes[2] = new Plane(pts[1], pts[2], pts[3]);
        planes[3] = new Plane(pts[2], pts[0], pts[3]);
    }    


    // Fill the passed in Lists with mesh info for this tetrahedron.
    public void FillMeshInfo(List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUV)
    {
        newVertices.Add(pts[0]);
        newVertices.Add(pts[1]);
        newVertices.Add(pts[2]);
        newVertices.Add(pts[3]);

        // tri 1
        newTriangles.Add(0);
        newTriangles.Add(1);
        newTriangles.Add(2);

        // tri 2
        newTriangles.Add(0);
        newTriangles.Add(1);
        newTriangles.Add(3);

        // tri 3
        newTriangles.Add(1);
        newTriangles.Add(2);
        newTriangles.Add(3);

        // tri 4
        newTriangles.Add(2);
        newTriangles.Add(0);
        newTriangles.Add(3);

        for (int i = 0; i < newVertices.Count; ++i)
            newUV.Add(new Vector2(0, 0));
    }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ConvexHull : MonoBehaviour
{
    // mesh variables
    //public Vector3[] newVertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 1) };
    //public Vector2[] newUV = new Vector2[] { new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0) };
    //public int[] newTriangles = new int[] { 0, 1, 2 };

    public List<Vector3> newVertices = new List<Vector3>();
    public List<Vector2> newUV = new List<Vector2>();
    public List<int> newTriangles = new List<int>();

    // convex hull variables
    List<Vector3> pts = new List<Vector3>();
    List<bool> used = new List<bool>();

    public void Insert(List<Vector3> newPts)
    {
        // for now re-run the algorithm on a merged point set.
        //pts.InsertRange(pts.Count, newPts);

        pts = newPts;

        ComputeHull();
    }

    void ComputeHull()
    {
        used = new List<bool>();
        for (int i = 0; i < pts.Count; ++i)
            used.Add(false);

        // create initial tetrahedron 
        Tetrahedron initTet = GetInitTetrahedron();

    }

    Tetrahedron GetInitTetrahedron()
    {
        // get the extreme points in the 3 dimensions
        Vector3[] extremePts = new Vector3[6]{  new Vector3(float.MaxValue,0,0),
                                                new Vector3(float.MinValue,0,0),
                                                new Vector3(0,float.MaxValue,0),
                                                new Vector3(0,float.MinValue,0),
                                                new Vector3(0,0,float.MaxValue),
                                                new Vector3(0,0,float.MinValue)};
        int[] extremeIdx = new int[6];

        // search for the extreme points over the whole point list.
        for (int i = 0; i < pts.Count; ++i)
        {
            if (pts[i].x < extremePts[0].x)
            {
                extremePts[0] = pts[i];
                extremeIdx[0] = i;
            }
            if (pts[i].x > extremePts[1].x)
            {
                extremePts[1] = pts[i];
                extremeIdx[1] = i;
            }
            if (pts[i].y < extremePts[2].y)
            {
                extremePts[2] = pts[i];
                extremeIdx[2] = i;
            }
            if (pts[i].y > extremePts[3].y)
            {
                extremePts[3] = pts[i];
                extremeIdx[3] = i;
            }
            if (pts[i].z < extremePts[4].z)
            {
                extremePts[4] = pts[i];
                extremeIdx[4] = i;
            }
            if (pts[i].z > extremePts[5].z)
            {
                extremePts[5] = pts[i];
                extremeIdx[5] = i;
            }
        }

        // find the most distant points
        int bestI=0, bestJ=0;
        float bestDist = 0;

        for (int i = 0; i < extremePts.Length; ++i)
        {
            for (int j = i+1; j < extremePts.Length; ++j)
            {
                float sqrDist = (extremePts[i] - extremePts[j]).sqrMagnitude;
                if( sqrDist > bestDist )
                {
                    bestDist = sqrDist;
                    bestI = extremeIdx[i];
                    bestJ = extremeIdx[j];
                }
            }
        }

        // remove indices from further consideration
        used[bestI] = true;
        used[bestJ] = true;

        // find most distant to the line between the previous iteration, to complete the base triangle
        Vector3 midPoint = 0.5f * (pts[bestI] + pts[bestJ]);
        int lastTriIdx = GetFurthestPointIdx(midPoint, extremeIdx);
        used[lastTriIdx] = true;

        // last point in tetrahedron will be the furthest in the whole list from this base triangle
        midPoint = (pts[bestI] + pts[bestJ] + pts[lastTriIdx]) / 3.0f;
        int lastTetraIdx = GetFurthestPointIdx(midPoint); 
        used[lastTetraIdx] = true;
        
        // let's see it
        Tetrahedron retTet = new Tetrahedron(new Vector3[]{pts[bestI], pts[bestJ], pts[lastTriIdx], pts[lastTetraIdx]});

        ClearMesh();
        retTet.FillMeshInfo(newVertices, newTriangles, newUV);
        return retTet;
    }

    void ClearMesh()
    {
        newVertices.Clear();
        newUV.Clear();
        newTriangles.Clear();
    }

    void LateUpdate()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();      // TODO -- inefficient
        mesh.uv = newUV.ToArray();                  // TODO -- inefficient
        mesh.triangles = newTriangles.ToArray();    // TODO -- inefficient
    }

    // fun animation
    /*
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int i = 0;
        while (i < vertices.Length)
        {
            vertices[i] += (new Vector3(i * 0.05f,0,0)) * Mathf.Sin(Time.time);
            i++;
        }
        mesh.vertices = vertices;
    }
    */


    // Helper functions

    // returns the idx in toPts of the point that is the furthest from fromPt
    int GetFurthestPointIdx(Vector3 fromPt, int[] idxSet)
    {
        int bestIdx = 0;
        float bestDist = 0;

        for (int i = 0; i < idxSet.Length; ++i)
        {
            int ptsIdx = idxSet[i];
            float sqrDist = (fromPt - pts[ptsIdx]).sqrMagnitude;
            if (!used[ptsIdx] && sqrDist > bestDist)
            {
                bestDist = sqrDist;
                bestIdx = ptsIdx;
            }
        }
        return bestIdx;
    }

    // returns the idx in the member pts List of the point that is the furthest from fromPt
    int GetFurthestPointIdx(Vector3 fromPt)
    {
        int bestIdx = 0;
        float bestDist = 0;

        for (int i = 0; i < pts.Count; ++i)
        {
            float sqrDist = (fromPt - pts[i]).sqrMagnitude;
            if (!used[i] && sqrDist > bestDist)
            {
                bestDist = sqrDist;
                bestIdx = i;
            }
        }
        return bestIdx;
    }
}
