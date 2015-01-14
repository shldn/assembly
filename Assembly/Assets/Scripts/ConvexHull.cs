using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class ConvexHull
{
    // creates a new game object with the convex hull mesh from the input pts
    public static GameObject GetMeshFromPoints(List<Vector3> pts)
    {
        GameObject go = new GameObject("ConvexHull", typeof(MeshRenderer), typeof(MeshFilter));
        
        // Set mesh in the approx center of the mesh
        Vector3 pos = Vector3.zero;
        foreach (Vector3 p in pts)
            pos += p;
        pos /= (float)pts.Count;
        go.transform.position = pos;

        Mesh mesh = go.GetComponent<MeshFilter>().mesh;
        UpdateMeshFromPoints(pts, ref mesh);
        return go;
    }

    // pass in the points to create the convex hull from and the mesh you want to fill with the new hull mesh definition
    public static void UpdateMeshFromPoints(List<Vector3> pts, ref Mesh meshToFill)
    {
        ConvexHull hull = new ConvexHull(pts);
        hull.UpdateMesh(ref meshToFill);
    }


    // mesh variables
    public List<Vector3> newVertices = new List<Vector3>();
    public List<Vector2> newUV = new List<Vector2>();
    public List<int> newTriangles = new List<int>();
    public List<Vector3> newNormals = new List<Vector3>();

    // convex hull variables
    List<Vector3> pts = new List<Vector3>();
    List<Face> faces = new List<Face>();
    List<bool> used = new List<bool>();

    bool enforceFloatingPointCheck = true;


    public ConvexHull(List<Vector3> newPts)
    {
        Debug.LogError("Num pts: " + newPts.Count);
        string str = "";
        for (int i = 0; i < newPts.Count; ++i)
            str += i + " " + newPts[i].ToString() + "\n";
        Debug.LogError(str);
        AddPts(newPts);
        ComputeHull();
    }

    private void AddSafeEdge(Dictionary<Face, int> safeEdges, Face f, int e)
    {
        Debug.LogError("Adding safe edge: " + f.IdxStr() + " edge: " + e);
        int edgeVal = (e + 1) * (e + 1);
        if (!safeEdges.ContainsKey(f))
            safeEdges.Add(f, edgeVal);
        else
            safeEdges[f] += edgeVal;
    }
    
    // cut indices are expected to be in order as they walk along the mesh.
    public void Cut(List<int> cutVertIndices)
    {
        if (cutVertIndices.Count <= 1)
        {
            Debug.LogError("cutVertIndices is empty or 1");
            return;
        }

        string str = "Cut\n";
        for (int i = 0; i < cutVertIndices.Count; ++i)
            str += cutVertIndices[i] + " ";
        str += " available faces: ";
        for (int i = 0; i < faces.Count; ++i)
            str += faces[i].IdxStr() + "\n";
        Debug.LogError(str);

        // make sure the cut is closed
        if (cutVertIndices[0] != cutVertIndices[cutVertIndices.Count - 1])
            cutVertIndices.Add(cutVertIndices[0]);

        Dictionary<Face, int> safeEdges = new Dictionary<Face, int>();
        List<Face> cutFaces = new List<Face>();
        int startE = -1;

        // find the face that contains the first cut edge
        for (int i = 0; i < faces.Count && cutFaces.Count == 0; ++i)
        {
            startE = faces[i].idx.FindIndex(iparam => iparam == cutVertIndices[0]);
            if (startE != -1 && faces[i].idx[(startE + 1) % 3] == cutVertIndices[1])
            {
                cutFaces.Add(faces[i]);
                AddSafeEdge(safeEdges, faces[i], startE);
            }
        }

        if (cutFaces.Count == 0)
        {
            Debug.LogError("ConvexHull::Cut: first edge not found, perhaps the order is backwards");
            return;
        }

        Face face = cutFaces[0];
        int edge = startE;
        int cutIdxCount = 2; // found the first two verts in init
        for (; cutIdxCount < cutVertIndices.Count; ++cutIdxCount )
        {
            Debug.LogError("Next edge end: " + cutVertIndices[cutIdxCount]);
            GetNextBorderEdge(ref face, ref edge, fe => fe.f.GetEdgeEndVert(fe.ei) == cutVertIndices[cutIdxCount]);
            cutFaces.Add(face);
            AddSafeEdge(safeEdges, face, edge);
        }

        RemoveCutFaces(cutFaces, safeEdges);

        // update the mesh
        ClearMesh();
        FillMeshInfoWithFaces(faces, pts, newVertices, newTriangles, newUV);

    }

    void AddAllAttachedToSet(Face face, HashSet<Face> set)
    {
        for (int i = 0; i < 3; ++i)
            if (set.Add(face.adjFace[i]))
                AddAllAttachedToSet(face, set);
    }

    void RemoveCutFaces(List<Face> cutFaces, Dictionary<Face, int> safeEdges)
    {
        HashSet<Face> facesToRemove = new HashSet<Face>();
        for(int i=0; i < cutFaces.Count; ++i)
            facesToRemove.Add(cutFaces[i]);

        // remove faces attached to the cut faces
        for (int i = 0; i < cutFaces.Count; ++i)
        {
            Face cutFace = cutFaces[i];
            for (int afi = 0; afi < 3; ++afi)
            {
                int mask = (afi + 1) * (afi + 1);
                if ((safeEdges[cutFace] & mask) == 0)
                {
                    Face faceToRemove = cutFace.adjFace[afi];
                    if (facesToRemove.Add(faceToRemove))
                        AddAllAttachedToSet(faceToRemove, facesToRemove);
                }
            }
        }

        // remove all marked for removal
        foreach (Face f in facesToRemove)
        {
            Debug.LogError("Removing: " + f.IdxStr());
            faces.Remove(f);
        }
    }

    private void AddPts(List<Vector3> newPts)
    {
        // to reduce holes from floating point error, only allow positions up to a certain decimal point
        if (enforceFloatingPointCheck)
        {
            int numDecimalPlaces = 4;
            pts.Capacity = newPts.Count;
            for (int i = 0; i < newPts.Count; ++i)
                pts.Add( new Vector3((float)System.Math.Round(newPts[i].x, numDecimalPlaces),
                                     (float)System.Math.Round(newPts[i].y, numDecimalPlaces),
                                     (float)System.Math.Round(newPts[i].z, numDecimalPlaces)) );
        }
        else
            pts = newPts;
    }

    public Mesh GetMesh()
    {
        Mesh mesh = new Mesh();
        UpdateMesh(ref mesh);
        return mesh;
    }

    public void UpdateMesh(ref Mesh mesh)
    {
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.triangles = newTriangles.ToArray();
        if (newNormals.Count > 0)
            mesh.normals = newNormals.ToArray();
    }

    void ComputeHull()
    {
        if (pts.Count < 4)
        {
            Debug.LogError("Convex Hull expects at least 4 points");
            return;
        }

        used = new List<bool>();
        for (int i = 0; i < pts.Count; ++i)
            used.Add(false);

        // create initial Pyramid 
        Pyramid initTet = GetInitTetrahedron();

        AssignPtsToFaces(initTet);

        // iteration phase
        LinkedList<Face> faceStack = new LinkedList<Face>();
        initTet.PushFaces(ref faceStack);

        int count = 0;
        while (faceStack.Count > 0 && count++ < 5000)
        {
            // Pop 
            Face f = faceStack.Last.Value;
            faceStack.RemoveLast();

            if (f.visiblePts.Count == 0)
            {
                faces.Add(f);
                continue;
            }
            Vector3 furthestPt = new Vector3();
            int furthestIdx = -1;
            f.GetFurthestVisible(ref furthestPt, ref furthestIdx);
            List<FaceEdgeIdxPair> horizonEdges = GetVisibleFaces(faceStack, f, furthestPt);
            if (horizonEdges == null)
                break;
            List<Face> newFaces = Helpers.GetPyramidFaces(pts, horizonEdges, furthestIdx);

            // remove nodes marked for removal
            HashSet<Face> extraFaces = new HashSet<Face>();
            LinkedListNode<Face> it = faceStack.First;
            while (it != null)
            {
                if (it.Value.GetDistanceToPoint(furthestPt) >= 0.0f)
                {
                    LinkedListNode<Face> toRemove = it;
                    extraFaces.Add(it.Value);
                    it = it.Next;
                    faceStack.Remove(toRemove);
                }
                else
                    it = it.Next;
            }
            for (int i = faces.Count - 1; i >= 0; --i)
            {
                if (faces[i].GetDistanceToPoint(furthestPt) >= 0.0f)
                    faces.RemoveAt(i);
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
        }
        if (count >= 5000)
            Debug.LogError("hit while loop break out counter");

        // populate the vert and index arrays with the saved faces
        ClearMesh();
        FillMeshInfoWithFaces(faces, pts, newVertices, newTriangles, newUV);
    }

    private Vector3 GetCenterPt()
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 p in pts)
            center += p;
        center /= (float)pts.Count;
        return center;
    }

    private void AddNormal(Dictionary<int, List<Vector3>> vertNormals, int idx, Vector3 normal)
    {
        if (!vertNormals.ContainsKey(idx))
            vertNormals.Add(idx, new List<Vector3>());
        vertNormals[idx].Add(normal);
    }
    
    // Fill the passed in Lists with mesh info for this pyramid.
    public void FillMeshInfoWithFaces(List<Face> faces, List<Vector3> origPts, List<Vector3> newVertices, List<int> newTriangles, List<Vector2> newUV)
    {
        bool includeUvs = true;
        bool includeNormals = true;

        Dictionary<int, List<Vector3>> vertNormals = new Dictionary<int,List<Vector3>>();

        // verts
        newVertices.AddRange(origPts);

        // tris
        for(int i=0; i < faces.Count; ++i)
        {
            // Unity expects inward facing triangles... clockwise, so flip order
            newTriangles.Add(faces[i].idx[0]);
            newTriangles.Add(faces[i].idx[2]); 
            newTriangles.Add(faces[i].idx[1]);

            if (includeNormals)
            {
                Vector3 faceNormal = faces[i].normal;
                AddNormal(vertNormals, faces[i].idx[0], faceNormal);
                AddNormal(vertNormals, faces[i].idx[1], faceNormal);
                AddNormal(vertNormals, faces[i].idx[2], faceNormal);
            }
        }

        // uvs
        // more efficient if we do these calculations only for verts in the mesh
        if (includeUvs)
        {
            Vector3 center = GetCenterPt();
            float pi_recip = 1.0f / Mathf.PI;
            for (int i = 0; i < newVertices.Count; ++i)
            {
                // project uvs as if mesh was a sphere
                Vector3 v = (newVertices[i] - center).normalized;
                newUV.Add(new Vector2(0.5f + 0.5f * pi_recip * Mathf.Atan2(v.z, v.x), 0.5f - pi_recip * Mathf.Asin(v.y)));
            }
        }
        else
            for (int i = 0; i < newVertices.Count; ++i)
                newUV.Add(new Vector2(0, 0));

        // normals
        if (includeNormals)
        {
            for (int i = 0; i < newVertices.Count; ++i)
            {
                if (vertNormals.ContainsKey(i))
                {
                    Vector3 sum = Vector3.zero;
                    List<Vector3> normals = vertNormals[i];
                    for (int j = 0; j < normals.Count; ++j)
                        sum += normals[j];
                    sum /= (float)normals.Count;
                    newNormals.Add(sum);
                }
                else
                    newNormals.Add(Vector3.up);
            }
        }
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

    void PrintPoints()
    {
        string str = "";
        for (int i = 0; i < pts.Count; ++i)
            str += "(" + pts[i].x + ", " + pts[i].y + ", " + pts[i].z + ")\n";
        Debug.LogError(str);
    }

    void FindBorderEdge(ref Face face, ref int edge, Vector3 visiblePt)
    {
        int testE = (edge + 1) % 3;
        int testVIdx = face.idx[testE];
        Face testF = face.adjFace[testE];
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

            // error cases, print debugging info
            if (testF == startF)
            {
                Debug.LogError("testF == startF");
                PrintPoints();
            }
            if (edge == -1)
            {
                Debug.LogError("Bad edge, requesting edge");
                for (int i = 0; i < startF.idx.Count; ++i)
                    Debug.LogError("\t" + startF.idx[i]);
                PrintPoints();
            }

            startF = testF;
        }
    }

    void GetNextBorderEdge(ref Face startF, ref int edge, Predicate<FaceEdgeIdxPair> pred)
    {
        int testE = (edge + 1) % 3;
        if (pred(new FaceEdgeIdxPair(startF, testE)))
        {
            Debug.LogError("First Found: e: " + testE + "f: " + startF.IdxStr());
            edge = testE;
        }
        else
        {
            int testVIdx = startF.idx[testE];
            Face testF = startF.adjFace[testE];
            edge = testF.idx.FindIndex(i => i == testVIdx);
            Debug.LogError("Now looking for starts with " + testVIdx);

            // find the next edge along the walk that satisfies the predicate.
            while (testF != startF)
            {
                if (!pred(new FaceEdgeIdxPair(testF, edge)))
                {
                    testF = testF.GetAdjacentEdgeStartsWithVertIdx(testVIdx);
                    edge = testF.idx.FindIndex(i => i == testVIdx);
                }
                else
                {
                    Debug.LogError("Found e: " + edge + " f: "  + testF.IdxStr());
                    break;
                }
            }

            // error cases, print debugging info
            if (testF == startF)
            {
                Debug.LogError("testF == startF");
                PrintPoints();
            }
            if (edge == -1)
            {
                Debug.LogError("Bad edge, requesting edge");
                for (int i = 0; i < startF.idx.Count; ++i)
                    Debug.LogError("\t" + startF.idx[i]);
                PrintPoints();
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
        if (startE == -1)
        {
            Debug.LogError("Didn\'t find startEdge");
            PrintPoints();
            return null;
        }

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

            // print point positions
            PrintPoints();
            return null;
        }
        return horizonEdges;
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
        newNormals.Clear();
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
        List<Face> faces = new List<Face>();
        for (int i = 0; i < baseEdges.Count; ++i)
        {
            Face baseF = baseEdges[i].f;
            int ei = baseEdges[i].ei;
            int i1 = baseF.idx[ei];
            int i2 = baseF.idx[(ei + 1) % 3];
            Face newF = new Face(pts, i1, i2, apexIdx);

            // hook up adjacency pointers
            Face adjFace = baseF.adjFace[ei];
            newF.adjFace[0] = adjFace;

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

        for (int i = 0; i < faces.Count; ++i)
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

public class Edge
{
    public Edge(Face f, int ei, bool forward_ = true) { face = f; edgeIdx = ei; forward = forward_; }
    public Face face;
    int edgeIdx;
    bool forward;
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
    public Vector3 normal { get { return -p.normal; } }

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

    public bool ContainsVert(int vIdx)
    {
        return idx.Contains(vIdx);
    }

    public bool Equals(Face rhs)
    {
        return idx[0] == rhs.idx[0] && idx[1] == rhs.idx[1] && idx[2] == rhs.idx[2];
    }

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
            if (dist > 0 && dist > bestDist)
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
        return idx[0] + " " + idx[1] + " " + idx[2];
    }

    public int GetEdgeEndVert(int edgeIdx)
    {
        return idx[(edgeIdx + 1) % 3];
    }

    public int GetEdgeIdx(Face f)
    {
        for (int i = 0; i < adjFace.Count; ++i)
            if (adjFace[i] == f)
                return i;

        // edge not found, print some debugging info
        Debug.LogError("Edge not found");
        for (int i = 0; i < idx.Count; ++i)
            Debug.LogError("\t" + idx[i]);
        Debug.LogError("Adj: ");
        for (int i = 0; i < adjFace.Count; ++i)
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

    }

    public void AdjFaceCheck()
    {
        // adjFace sanity check
        for (int i = 0; i < faces.Count; ++i)
            faces[i].AdjFaceCheck();
    }

    public void PushFaces(ref LinkedList<Face> faceStack)
    {
        for (int i = 0; i < faces.Count; ++i)
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
