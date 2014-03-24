using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct FaceEdgeIdxPair
{
    public FaceEdgeIdxPair(Face f_, int ei_) { f = f_; ei = ei_; }
    public Face f;
    public int ei;
}

public class Helpers
{
    // basePts are assumed to be counter-clockwise so they point toward apex
    // this does not return the base face, only those connected to the apex
    public static List<Face> GetPyramidFaces(List<Vector3> pts, List<FaceEdgeIdxPair> baseEdges, int apexIdx)
    {
        //Debug.LogError("horizon: ");
        //for (int i = 0; i < baseEdges.Count; ++i)
        //    Debug.LogError("\t" + i + " f: " + baseEdges[i].f.IdxStr() + " edge: " + baseEdges[i].ei);
        //for (int i = 0; i < baseEdges.Count; ++i)
        //    baseEdges[i].f.AdjFaceCheck();
        //Debug.LogError("Done precheck");

        //Debug.LogError("P faces: apex: " + apexIdx);
        List<Face> faces = new List<Face>();
        for (int i = 0; i < baseEdges.Count; ++i)
        {
            //Debug.LogError("\t" + i + " f: " + baseEdges[i].f.IdxStr() + " edge: " + baseEdges[i].ei);
            Face baseF = baseEdges[i].f;
            int ei = baseEdges[i].ei;
            //if (ei < 0 || ei >= baseF.idx.Count)
            //    Debug.LogError("Bad ei: " + ei + " base.idx.Count: " + baseF.idx.Count);
            int i1 = baseF.idx[ei];
            int i2 = baseF.idx[(ei + 1) % 3];
            //Debug.LogError(i1 + " --> " + i2 + " adj: " + baseF.idx[0] + " " + baseF.idx[1] + " " + baseF.idx[2]);
            Face newF = new Face(pts, i1, i2, apexIdx);

            // hook up adjacency pointers
            //if (!baseF.AdjFaceCheck())
            //    Debug.LogError("baseF failed");
            Face adjFace = baseF.adjFace[ei];
            //if( !adjFace.AdjFaceCheck() )
            //    Debug.LogError("adjFace failed");
            newF.adjFace[0] = adjFace;
            //Debug.LogError("Old: Set " + adjFace.IdxStr() + " idx: " + adjFace.GetEdgeIdx(baseF) + " to newF " + newF.IdxStr() + " was: " + adjFace.adjFace[adjFace.GetEdgeIdx(baseF)].IdxStr());
            //Debug.LogError("Set " + adjFace.IdxStr() + " idx: " + adjFace.GetAdjacentIdxEdgeStartsWithVertIdx(i2) + " to newF " + newF.IdxStr() + " was: " + adjFace.adjFace[adjFace.GetAdjacentIdxEdgeStartsWithVertIdx(i2)].IdxStr());
            // adj face will have indices in the opposite direction
            adjFace.adjFace[adjFace.GetAdjacentIdxEdgeStartsWithVertIdx(i2)] = newF;
            baseF.adjFace[ei] = newF; // for sanity checks

            
            // adjFace[1] is the next iteration, adjFace[2] is the previous
            if (faces.Count > 0)
            {
                newF.adjFace[2] = faces[faces.Count - 1];
                faces[faces.Count - 1].adjFace[1] = newF;
            }
            faces.Add(newF);
        }

        // hook up the first and last adjacency
        faces[0].adjFace[2] = faces[faces.Count - 1];
        faces[faces.Count - 1].adjFace[1] = faces[0];

        for(int i=0; i < faces.Count; ++i)
            faces[i].AdjFaceCheck();
        return faces;
    }

    public static bool AddToPtSet(List<Face> faces, Vector3 pt, int idx)
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
        {
            faces[bestIdx].visiblePts.Add(pt);
            faces[bestIdx].visibleIdxs.Add(idx);
        }
        return found;
    }
}

public class Face
{

    public List<Vector3> pts = new List<Vector3>(3);
    public List<Vector3> visiblePts = new List<Vector3>();
    public List<int> visibleIdxs = new List<int>();
    public List<int> idx = new List<int>(3); // vert idx in the original pt list
    public List<Face> adjFace = new List<Face>(3);
    Plane p;
    public bool removeMe = false;

    public Face(List<Vector3> pts_, int idx1, int idx2, int idx3)
    {
        pts.Add(pts_[idx1]);
        pts.Add(pts_[idx2]);
        pts.Add(pts_[idx3]);
        idx.Add(idx1);
        idx.Add(idx2);
        idx.Add(idx3);
        adjFace.Add(null);
        adjFace.Add(null);
        adjFace.Add(null);
        p = new Plane(pts[0], pts[1], pts[2]);
    }

    public bool Equals(Face rhs)
    {
        return idx[0] == rhs.idx[0] && idx[1] == rhs.idx[1] && idx[2] == rhs.idx[2];
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
        return -p.GetDistanceToPoint(pt);
    }

    public bool IsPtVisible(Vector3 pt)
    {
        return GetDistanceToPoint(pt) >= 0;
    }

    public void GetFurthestVisible(ref Vector3 pos, ref int idx)
    {
        int bestIdx = 0;
        float bestDist = -1.0f;
        for (int i = 0; i < visiblePts.Count; ++i)
        {
            float dist = GetDistanceToPoint(visiblePts[i]);
            if (dist > 0 && dist < bestDist)
            {
                bestIdx = i;
                bestDist = dist;
            }
        }
        pos = visiblePts[bestIdx];
        idx = visibleIdxs[bestIdx];
    }

    public string IdxStr()
    {
        return idx[0] + " " + idx[1] + " "  + idx[2];
    }

    public int GetEdgeIdx(Face f)
    {
        for (int i = 0; i < adjFace.Count; ++i)
            if (adjFace[i] == f)
                return i;
        Debug.LogError("Edge not found");
        for(int i=0; i < idx.Count; ++i)
            Debug.LogError("\t" + idx[i]);
        Debug.LogError("Adj: ");
        for(int i=0; i < adjFace.Count; ++i)
            Debug.LogError(i + " adj: " + adjFace[i].idx[0] + " " + adjFace[i].idx[1] + " " + adjFace[i].idx[2]);
        return -1;
    }

    public int GetEdgeIdxCount(Face f)
    {
        int count = 0;
        for (int i = 0; i < adjFace.Count; ++i)
            if (adjFace[i] == f)
                count++;
        return count;
    }

    // returns the next face that shares the first edge vert with this Face, but not an edge, (adjacent to the adjacent)
    public Face GetNextAdjacent(int edgeIdx)
    {
        int nextEdgeIdx = GetNextAdjacentEdgeIdx(edgeIdx);
        return adjFace[edgeIdx].adjFace[nextEdgeIdx];
    }

    public int GetNextAdjacentEdgeIdx(int edgeIdx)
    {
        return (adjFace[edgeIdx].GetEdgeIdx(this) + 1) % 3;
    }

    // Gets the adjacent face that has an edge that starts with the supplied vertIdx
    public Face GetAdjacentEdgeStartsWithVertIdx(int vertIdx)
    {
        int idx = GetAdjacentIdxEdgeStartsWithVertIdx(vertIdx);
        return (idx >= 0) ? adjFace[idx] : null;
    }

    public int GetAdjacentIdxEdgeStartsWithVertIdx(int vertIdx)
    {
        for (int i = 0; i < idx.Count; ++i)
            if (vertIdx == idx[i])
                return i;
        return -1;
    }

    public bool AdjFaceCheck()
    {
        for (int i = 0; i < adjFace.Count; ++i)
            if (adjFace[i].GetEdgeIdxCount(this) != GetEdgeIdxCount(adjFace[i]))
            {
                Debug.LogError("bad adjFace assignment: " + i + " " + adjFace[i].IdxStr() + " doesn\'t know about " + this.IdxStr() + " count: " + adjFace[i].GetEdgeIdxCount(this) + " != " + GetEdgeIdxCount(adjFace[i]));
                return false;
            }
        return true;
    }
}

public class Pyramid
{
    public List<Face> faces = new List<Face>(4);
    private List<int> idx = new List<int>(); // temp for mesh debugging

    // refactor to use Faces
    public Pyramid(List<Vector3> pts, List<int> idxs)
    {
        //Debug.LogError("init pyramid");
        //for (int i = 0; i < idxs.Count; ++i)
        //    Debug.LogError("\t" + idxs[i]);
        //pts = pts_;
        idx = idxs;
        Vector3 midPt = 0.25f * (pts[idxs[0]] + pts[idxs[1]] + pts[idxs[2]] + pts[idxs[3]]);


        // determine correct orientation of 0,1,2
        // want to face away from the midpoint
         Plane p = new Plane(pts[idxs[0]], pts[idxs[1]], pts[idxs[2]]);
         if (!p.GetSide(midPt))
         {
             int temp = idxs[2];
             idxs[2] = idxs[1];
             idxs[1] = temp;
         }
         faces.Add(new Face(pts, idxs[0], idxs[1], idxs[2]));
         faces.Add(new Face(pts, idxs[3], idxs[1], idxs[0]));
         faces.Add(new Face(pts, idxs[3], idxs[2], idxs[1]));
         faces.Add(new Face(pts, idxs[3], idxs[0], idxs[2]));

        // setup adjacency pointers
        faces[0].adjFace[0] = faces[1]; // 0-1
        faces[0].adjFace[1] = faces[2]; // 1-2
        faces[0].adjFace[2] = faces[3]; // 2-0

        faces[1].adjFace[0] = faces[2]; // 3-1
        faces[1].adjFace[1] = faces[0]; // 1-0
        faces[1].adjFace[2] = faces[3]; // 0-3

        faces[2].adjFace[0] = faces[3]; // 3-2
        faces[2].adjFace[1] = faces[0]; // 2-1
        faces[2].adjFace[2] = faces[1]; // 1-3

        faces[3].adjFace[0] = faces[1]; // 3-0
        faces[3].adjFace[1] = faces[0]; // 0-2
        faces[3].adjFace[2] = faces[2]; // 2-3

        AdjFaceCheck();

    }

    public void AdjFaceCheck()
    {
        // adjFace sanity check
        for (int i = 0; i < faces.Count; ++i)
            faces[i].AdjFaceCheck();
    }

    public void PushFaces(ref LinkedList<Face> faceStack)
    {
        for(int i=0; i < faces.Count; ++i)
            faceStack.AddLast(faces[i]);
    }

    public bool AddToPtSet(Vector3 pt, int idx)
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
        {
            faces[bestIdx].visiblePts.Add(pt);
            faces[bestIdx].visibleIdxs.Add(idx);
        }
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

        int count = 0;
        while (faceStack.Count > 0 && count++ < 1000)
        {
            // Pop 
            Face f = faceStack.Last.Value;
            faceStack.RemoveLast();

            if (f.visiblePts.Count == 0)
            {
                savedFaces.Add(f);
                //Debug.LogError(count + " Saving face: " + savedFaces.Count + " - " + f.IdxStr());
                continue;
            }
            Vector3 furthestPt = new Vector3();
            int furthestIdx = -1;
            f.GetFurthestVisible(ref furthestPt, ref furthestIdx);
            List<FaceEdgeIdxPair> horizonEdges = GetVisibleFaces(faceStack, f, furthestPt);
            if (horizonEdges == null)
                break;
            List<Face> newFaces = Helpers.GetPyramidFaces(pts, horizonEdges, furthestIdx);

            HashSet<Face> extraFaces = new HashSet<Face>();
            // remove nodes marked for removal
            LinkedListNode<Face> it = faceStack.First;
            while (it != null)
            {
                if (it.Value.GetDistanceToPoint(furthestPt) > 0.0f)
                {
                    LinkedListNode<Face> toRemove = it;
                    extraFaces.Add(it.Value);
                    it = it.Next;
                    faceStack.Remove(toRemove);
                }
                else
                    it = it.Next;
            }
            for (int i = savedFaces.Count - 1; i >= 0; --i)
            {
                if (savedFaces[i].GetDistanceToPoint(furthestPt) > 0.0f)
                    savedFaces.RemoveAt(i);
            }

            // Add visible points to new faces.
            HashSet<Face> horizonFaces = new HashSet<Face>();
            foreach (Face ef in extraFaces)
            {
                horizonFaces.Add(ef);
                for (int j = 0; j < ef.visibleIdxs.Count; ++j)
                {
                    int visIdx = ef.visibleIdxs[j];
                    if (visIdx != furthestIdx)
                        Helpers.AddToPtSet(newFaces, pts[visIdx], visIdx);
                }
            }
            for (int i = 0; i < horizonEdges.Count; ++i)
            {
                if( horizonFaces.Contains(horizonEdges[i].f ) )
                    continue;
                horizonFaces.Add(horizonEdges[i].f);
                for (int j = 0; j < horizonEdges[i].f.visibleIdxs.Count; ++j)
                {
                    int visIdx = horizonEdges[i].f.visibleIdxs[j];
                    if( visIdx != furthestIdx)
                        Helpers.AddToPtSet(newFaces, pts[visIdx], visIdx);
                }
            }
            
            // add new faces to stack
            for (int i = 0; i < newFaces.Count; ++i)
                faceStack.AddLast(newFaces[i]);

            //testing
            //Debug.LogError("test break");
            //savedFaces.AddRange(faceStack);
            //break;
        }
        if (count >= 1000)
            Debug.LogError("hit while loop break out counter");

        // visualize the saved faces
        ClearMesh();
        FillMeshInfoWithFaces(savedFaces, pts, newVertices, newTriangles, newUV);
    }

    
    // Fill the passed in Lists with mesh info for this pyramid.
    public void FillMeshInfoWithFaces(List<Face> faces, List<Vector3> origPts, List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUV)
    {
        newVertices.AddRange(origPts);

        for(int i=0; i < faces.Count; ++i)
        {
            // Unity expects inward facing triangles... clockwise, so flip order
            newTriangles.Add(faces[i].idx[0]);
            newTriangles.Add(faces[i].idx[2]); 
            newTriangles.Add(faces[i].idx[1]);
        }

        for (int i = 0; i < newVertices.Count; ++i)
            newUV.Add(new Vector2(0, 0));
    }


    void GetFaceWithoutAdjacentVisible(ref Face startF, ref int edge, Vector3 visiblePt)
    {
        Stack<Face> faces = new Stack<Face>();
        HashSet<Face> visited = new HashSet<Face>();
        faces.Push(startF);
        visited.Add(startF);
        startF.removeMe = true;
        while (faces.Count > 0)
        {
            Face f = faces.Pop();
            for (int i = 0; i < f.adjFace.Count; ++i)
            {
                if (!f.adjFace[i].IsPtVisible(visiblePt))
                {
                    startF = f;
                    edge = i;
                    return;
                }
                else
                {
                    if (!visited.Contains(f.adjFace[i]))
                    {
                        faces.Push(f.adjFace[i]);
                        visited.Add(f);
                        f.removeMe = true;
                    }
                }
            }
        }
        Debug.LogError("Ran out of faces start: " + startF.IdxStr());
    }

    void FindBorderEdge(ref Face startF, ref int edge, Vector3 visiblePt)
    {
        int testE = (edge + 1) % 3;
        int testVIdx = startF.idx[testE];
        Face testF = startF.adjFace[testE];
        if (!testF.IsPtVisible(visiblePt))
        {
            edge = testE;
        }
    }

    void GetNextBorderEdge(ref Face startF, ref int edge, Vector3 visiblePt)
    {
        int testE = (edge + 1) % 3;
        int testVIdx = startF.idx[testE];
        Face testF = startF.adjFace[testE];
        if (!testF.IsPtVisible(visiblePt))
        {
            edge = testE;
        }
        else
        {
            // is visible, remove from face list
            testF.removeMe = true;


            // find the next edge along the walk that has a visible face on one side and not visible on the other.
            Face nextF = null;
            while (testF != startF)
            {
                nextF = testF.GetAdjacentEdgeStartsWithVertIdx(testVIdx);
                if (nextF.IsPtVisible(visiblePt))
                {
                    testF = nextF;
                    testF.removeMe = true;
                }
                else
                {
                    edge = testF.GetAdjacentIdxEdgeStartsWithVertIdx(testVIdx);
                    break;
                }
            }

            if (testF == startF)
            {
                Debug.LogError("testF == startF");
            }
            if (edge == -1)
            {
                Debug.LogError("Bad edge, requesting edge");
                for (int i = 0; i < startF.idx.Count; ++i)
                    Debug.LogError("\t" + startF.idx[i]);

                Debug.LogError("pts: ");
                for (int i = 0; i < pts.Count; ++i)
                    Debug.LogError("\t" + i + " " + pts[i].ToString());
            }

            startF = testF;
        }
    }

    void GetNextFaceWithoutAdjacentVisible(ref Face startF, ref int edge, Vector3 visiblePt)
    {
        int testE = (edge + 1) % 3;
        Face testF = startF.adjFace[testE];
        if (!testF.IsPtVisible(visiblePt))
        {
            edge = testE;
        }
        else
        {
            // is visible, test the next adjacent
            edge = testF.GetEdgeIdx(startF);
            if (edge == -1)
            {
                Debug.LogError("Bad edge, requesting edge");
                for (int i = 0; i < startF.idx.Count; ++i)
                    Debug.LogError("\t" + startF.idx[i]);

                Debug.LogError("pts: ");
                for (int i = 0; i < pts.Count; ++i)
                    Debug.LogError("\t" + i + " " + pts[i].ToString());
                return;
            }
            startF = testF;
            testF.removeMe = true;
            GetNextFaceWithoutAdjacentVisible(ref startF, ref edge, visiblePt);
        }
    }

    List<FaceEdgeIdxPair> GetVisibleFaces(LinkedList<Face> faceStack, Face origFace, Vector3 furthestPt)
    {

        List<FaceEdgeIdxPair> horizonEdges = new List<FaceEdgeIdxPair>();

        // find start edge
        Face startF = origFace;
        int startE = -1;
        GetFaceWithoutAdjacentVisible(ref startF, ref startE, furthestPt);
        horizonEdges.Add(new FaceEdgeIdxPair(startF, startE));
        origFace = startF;
        if (!origFace.IsPtVisible(furthestPt))
            Debug.LogError("Original face cannot see pt");
        if (origFace.adjFace[startE].IsPtVisible(furthestPt))
            Debug.LogError("original edge borders visible face");


        int edge = startE;
        int count = 0;
        while (count++ < 200)
        {
            GetNextBorderEdge(ref startF, ref edge, furthestPt);
            // check if we're back at our starting point.
            if ((startF == origFace && edge == startE) || edge == -1)
                break;

            horizonEdges.Add(new FaceEdgeIdxPair(startF, edge));
        }
        if (count >= 200)
        {
            Debug.LogError("Hit while loop failsafe, startF: " + origFace.IdxStr() + " vis pt: " + furthestPt.ToString());
            if (!origFace.IsPtVisible(furthestPt))
                Debug.LogError("Original face cannot see pt");
            if( origFace.adjFace[startE].IsPtVisible(furthestPt))
                Debug.LogError("original edge borders visible face");
                
            for (int i = 0; i < horizonEdges.Count; ++i)
                Debug.LogError("\t" + i + " f: " + horizonEdges[i].f.IdxStr() + " edge: " + horizonEdges[i].ei);

            HullNode.PrintHullNodePositions();
            return null;
        }
        return horizonEdges;
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
            used[i] = !tet.AddToPtSet(pts[i], i);
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
