using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Helpers
{
    // basePts are assumed to be counter-clockwise so they point toward apex
    public static List<Face> GetPyramidFaces(List<Vector3> basePts, Vector3 apexPt)
    {
        List<Face> faces = new List<Face>();
        for(int i=0; i < basePts.Count-1; ++i)
            faces.Add(new Face(basePts[i], basePts[i+1], apexPt));
        faces.Add(new Face(basePts[basePts.Count - 1], basePts[0], apexPt));

        return faces;
    }
}

public class Triangle
{
    public Vector3[] pts = new Vector3[3];
}

public class Face
{

    public List<Vector3> pts = new List<Vector3>(3); 
    public List<Vector3> visiblePts = new List<Vector3>();
    Plane p;

    public Face(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        pts.Add(p1);
        pts.Add(p2);
        pts.Add(p3);
        p = new Plane(p1, p2, p3);
    }
    public Face(List<Vector3> pts_)
    {
        pts = pts_;
        p = new Plane(pts_[0], pts_[1], pts_[2]);
    }

    public Vector3 GetFurthestVisible()
    {
        int bestIdx = 0;
        float bestDist = -1.0f;
        for (int i = 0; i < visiblePts.Count; ++i)
        {
            float dist = p.GetDistanceToPoint(visiblePts[i]);
            if (dist > 0 && dist < bestDist)
            {
                bestIdx = i;
                bestDist = dist;
            }
        }
        return visiblePts[bestIdx];
    }
}

public class Pyramid
{
    private Vector3[] pts = new Vector3[4];
    private Plane[] planes = new Plane[4];
    private Vector3 midPt = new Vector3();
    public List<List<Vector3>> ptSets = new List<List<Vector3>>(4);

    public Vector3[] Pts { get { return pts; } }

    private Plane GetPlane(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Plane p = new Plane(p1, p2, p3);

        // error checking, shouldn't be needed.
        if (p.GetSide(midPt))
        {
            Debug.LogError("Swapping");
            p.Set3Points(p1, p3, p2);
        }
        if (p.GetSide(midPt))
            Debug.LogError("Still not correct direction?");
        return p;
    }

    // refactor to use Faces
    public Pyramid(Vector3[] pts_)
    {
        pts = pts_;
        midPt = 0.25f * (pts[0] + pts[1] + pts[2] + pts[3]);

        // determine correct orientation of 0,1,2
        // want to face away from the midpoint
         Plane p = new Plane(pts[0], pts[1], pts[2]);
         if (p.GetSide(midPt))
         {
             Vector3 temp = pts[2];
             pts[2] = pts[1];
             pts[1] = temp;
         }
        planes[0] = GetPlane(pts[0], pts[1], pts[2]);
        planes[1] = GetPlane(pts[3], pts[1], pts[0]);
        planes[2] = GetPlane(pts[3], pts[2], pts[1]);
        planes[3] = GetPlane(pts[3], pts[0], pts[2]);

        ptSets.Add(new List<Vector3>());
        ptSets.Add(new List<Vector3>());
        ptSets.Add(new List<Vector3>());
        ptSets.Add(new List<Vector3>());
    }

    public void PushFaces(ref Stack<Face> faceStack)
    {
        // base
        Face baseFace = new Face(pts[0], pts[1], pts[2]);
        baseFace.visiblePts = ptSets[0];
        if( baseFace.visiblePts.Count > 0)
            faceStack.Push(baseFace);
        
        // non-base Pyramid faces
        List<Face> faces = Helpers.GetPyramidFaces(new List<Vector3>(){pts[0], pts[1], pts[2]}, pts[3]);
        for (int i = 0; i < faces.Count; ++i)
        {
            faces[i].visiblePts = ptSets[i + 1];
            if (faces[i].visiblePts.Count > 0)
                faceStack.Push(faces[i]);
        }
    }

    public bool AddToPtSet(Vector3 pt)
    {
        int bestIdx = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < planes.Length; ++i)
        {
            float dist = planes[i].GetDistanceToPoint(pt);
            if (dist > 0 && dist < bestDist)
            {
                bestIdx = i;
                bestDist = dist;
            }
        }
        bool added = bestIdx > -1;
        if (added)
            ptSets[bestIdx].Add(pt);
        return added;
    }


    // Fill the passed in Lists with mesh info for this pyramid.
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

        // create initial Pyramid 
        Pyramid initTet = GetInitTetrahedron();

        AssignPtsToFaces(initTet);

        // testing -- visual
        ShowPtSets(initTet);


        // iteration phase
        Stack<Face> faceStack = new Stack<Face>();
        initTet.PushFaces(ref faceStack);

        while (faceStack.Count > 0)
        {
            Face f = faceStack.Pop();
            Vector3 furthestPt = f.GetFurthestVisible();
        }

    }

    void ShowPtSets(Pyramid tet)
    {
        for (int i = 0; i < HullNode.allNodes.Count; ++i)
        {
            if (tet.ptSets[0].Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(0, 1, 0, 0.5F);
            else if (tet.ptSets[1].Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(0, 0, 1, 0.5F);
            else if (tet.ptSets[2].Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(1, 0, 1, 0.5F);
            else if (tet.ptSets[3].Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(1, 1, 0, 0.5F);
            else
                HullNode.allNodes[i].color = new Color(1, 0, 0, 0.5F);
        }
    }

    void AssignPtsToFaces(Pyramid tet)
    {
        for (int i = 0; i < pts.Count; ++i)
        {
            if (used[i])
                continue;
            used[i] = !tet.AddToPtSet(pts[i]);
        }
    }

    Pyramid GetInitTetrahedron()
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
        Pyramid retTet = new Pyramid(new Vector3[] { pts[bestI], pts[bestJ], pts[lastTriIdx], pts[lastTetraIdx] });

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
