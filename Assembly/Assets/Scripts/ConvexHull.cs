using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Helpers
{
    // basePts are assumed to be counter-clockwise so they point toward apex
    // this does not return the base face, only those connected to the apex
    public static List<Face> GetPyramidFaces(List<Vector3> pts, List<int> baseIdxs, int apexIdx)
    {
        List<Face> faces = new List<Face>();
        for (int i = 0; i < baseIdxs.Count - 1; ++i)
            faces.Add(new Face(pts, baseIdxs[i], baseIdxs[i + 1], apexIdx));
        faces.Add(new Face(pts, baseIdxs[baseIdxs.Count - 1], baseIdxs[0], apexIdx));

        return faces;
    }
}

public class Face
{

    public List<Vector3> pts = new List<Vector3>(3); 
    public List<Vector3> visiblePts = new List<Vector3>();
    public List<int> idx = new List<int>(3); 
    Plane p;

    public Face(List<Vector3> pts_, int idx1, int idx2, int idx3)
    {
        pts.Add(pts_[idx1]);
        pts.Add(pts_[idx2]);
        pts.Add(pts_[idx3]);
        idx.Add(idx1);
        idx.Add(idx2);
        idx.Add(idx3);
        p = new Plane(pts[0], pts[1], pts[2]);
    }

    /*
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
     */

    public float GetDistanceToPoint(Vector3 pt)
    {
        return p.GetDistanceToPoint(pt);
    }

    public bool IsPtVisible(Vector3 pt)
    {
        return p.GetDistanceToPoint(pt) >= 0;
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
    public List<Face> faces = new List<Face>(4);
    private List<int> idx = new List<int>(); // temp for mesh debugging

    // refactor to use Faces
    public Pyramid(List<Vector3> pts, List<int> idxs)
    {
        //pts = pts_;
        idx = idxs;
        Vector3 midPt = 0.25f * (pts[idxs[0]] + pts[idxs[1]] + pts[idxs[2]] + pts[idxs[3]]);


        // determine correct orientation of 0,1,2
        // want to face away from the midpoint
         Plane p = new Plane(pts[idxs[0]], pts[idxs[1]], pts[idxs[2]]);
         if (p.GetSide(midPt))
         {
             int temp = idxs[2];
             idxs[2] = idxs[1];
             idxs[1] = temp;
         }
         faces.Add(new Face(pts, idxs[0], idxs[1], idxs[2]));
         faces.Add(new Face(pts, idxs[3], idxs[1], idxs[0]));
         faces.Add(new Face(pts, idxs[3], idxs[2], idxs[1]));
         faces.Add(new Face(pts, idxs[3], idxs[0], idxs[2]));

    }

    public void PushFaces(ref LinkedList<Face> faceStack)
    {
        for(int i=0; i < faces.Count; ++i)
            faceStack.AddLast(faces[i]);
    }

    public bool AddToPtSet(Vector3 pt)
    {
        int bestIdx = -1;
        float bestDist = float.MaxValue;
        for (int i = 0; i < faces.Count; ++i)
        {
            float dist = faces[i].GetDistanceToPoint(pt);
            if (dist > 0 && dist < bestDist)
            {
                bestIdx = i;
                bestDist = dist;
            }
        }
        bool found = bestIdx > -1;
        if (found)
            faces[bestIdx].visiblePts.Add(pt);
        return found;
    }


    // Fill the passed in Lists with mesh info for this pyramid.
    public void FillMeshInfo(List<Vector3> origPts, List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUV)
    {
        newVertices.Add(origPts[idx[0]]);
        newVertices.Add(origPts[idx[1]]);
        newVertices.Add(origPts[idx[2]]);
        newVertices.Add(origPts[idx[3]]);

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
        List<Face> savedFaces = new List<Face>();
        LinkedList<Face> faceStack = new LinkedList<Face>();
        initTet.PushFaces(ref faceStack);
        
        while (faceStack.Count > 0)
        {
            // Pop 
            Face f = faceStack.Last.Value;
            faceStack.RemoveLast();

            if (f.visiblePts.Count == 0)
            {
                savedFaces.Add(f);
                continue;
            }
            Vector3 furthestPt = f.GetFurthestVisible();
            List<Face> facesSeenFromPt = GetVisibleFaces(faceStack, f, furthestPt);
        }

    }

    List<Face> GetVisibleFaces(LinkedList<Face> faceStack, Face origFace, Vector3 furthestPt)
    {
        List<Face> visFaces = new List<Face>();
        visFaces.Add(origFace);

        // can be optimized, must be adjacent to origFace, so should just check these, could have hash table from point to faces
        foreach( Face f in faceStack)
        {
            if( f != origFace && f.IsPtVisible(furthestPt) )
                visFaces.Add(f);
        }
        return visFaces;
    }

    void ShowPtSets(Pyramid tet)
    {
        for (int i = 0; i < HullNode.allNodes.Count; ++i)
        {
            if (tet.faces[0].visiblePts.Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(0, 1, 0, 0.5F);
            else if (tet.faces[1].visiblePts.Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(0, 0, 1, 0.5F);
            else if (tet.faces[2].visiblePts.Contains(HullNode.allNodes[i].transform.position))
                HullNode.allNodes[i].color = new Color(1, 0, 1, 0.5F);
            else if (tet.faces[3].visiblePts.Contains(HullNode.allNodes[i].transform.position))
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
        Pyramid retTet = new Pyramid(pts, new List<int>(4) { bestI, bestJ, lastTriIdx, lastTetraIdx });

        ClearMesh();
        retTet.FillMeshInfo(pts, newVertices, newTriangles, newUV);
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
