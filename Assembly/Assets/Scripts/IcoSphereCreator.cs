using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class IcoSphereCreator : MonoBehaviour
{
    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;
        public int id; // creation id

        public TriangleIndices(int v1, int v2, int v3, int id)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            this.id = id;
        }
    } // End of TriangleIndices.


	public static IcoSphereCreator Inst;

    private int index;
    private Dictionary<Int64, int> middlePointIndexCache;
	List<int> triangles;
	List<Vector3> vertices;

    // Helpers for finding intersecting faces
    List<TriangleIndices> baseFaces;
    Dictionary<int, List<TriangleIndices>> faceChildren = new Dictionary<int, List<TriangleIndices>>();

	void Awake()
	{
		Inst = this;
	} // End of Awake().


    // add vertex to mesh, fix position to be on unit sphere, return index
    private int AddVertex(Vector3 p)
    {
        float length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);
        vertices.Add(new Vector3(p.x/length, p.y/length, p.z/length));
        return index++;
    } // End of AddVertex().


    // return index of point in the middle of p1 and p2
    private int GetMiddlePoint(int p1, int p2)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        Int64 smallerIndex = firstIsSmaller ? p1 : p2;
        Int64 greaterIndex = firstIsSmaller ? p2 : p1;
        Int64 key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (this.middlePointIndexCache.TryGetValue(key, out ret))
            return ret;

        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3(
            (point1.x + point2.x) / 2.0f, 
            (point1.y + point2.y) / 2.0f, 
            (point1.z + point2.z) / 2.0f);

        // add vertex makes sure point is on unit sphere
        int i = AddVertex(middle); 

        // store it, return index
        this.middlePointIndexCache.Add(key, i);
        return i;
    } // End of GetMiddlePoint().


    public Mesh Create(int recursionLevel)
    {
        this.middlePointIndexCache = new Dictionary<Int64, int>();
        this.index = 0;

		triangles = new List<int>();
		vertices = new List<Vector3>();

        // create 12 vertices of a icosahedron
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        AddVertex(new Vector3(-1,  t,  0)); // point 0
        AddVertex(new Vector3( 1,  t,  0)); // point 1
        AddVertex(new Vector3(-1, -t,  0)); // point 2
        AddVertex(new Vector3( 1, -t,  0)); // point 3

        AddVertex(new Vector3( 0, -1,  t)); // point 4
        AddVertex(new Vector3( 0,  1,  t)); // point 5
        AddVertex(new Vector3( 0, -1, -t)); // point 6
        AddVertex(new Vector3( 0,  1, -t)); // point 7

        AddVertex(new Vector3( t,  0, -1)); // point 8
        AddVertex(new Vector3( t,  0,  1)); // point 9
        AddVertex(new Vector3(-t,  0, -1)); // point 10
        AddVertex(new Vector3(-t,  0,  1)); // point 11


        // create 20 triangles of the icosahedron
        baseFaces = new List<TriangleIndices>();
        int count = 0;

        // 5 faces around point 0
        baseFaces.Add(new TriangleIndices(0, 11, 5, count++)); // face 0
        baseFaces.Add(new TriangleIndices(0, 5, 1, count++)); // face 1
        baseFaces.Add(new TriangleIndices(0, 1, 7, count++)); // face 2
        baseFaces.Add(new TriangleIndices(0, 7, 10, count++)); // face 3
        baseFaces.Add(new TriangleIndices(0, 10, 11, count++)); // face 4

        // 5 adjacent faces 
        baseFaces.Add(new TriangleIndices(1, 5, 9, count++)); // face 5
        baseFaces.Add(new TriangleIndices(5, 11, 4, count++)); // face 6
        baseFaces.Add(new TriangleIndices(11, 10, 2, count++)); // face 7
        baseFaces.Add(new TriangleIndices(10, 7, 6, count++)); // face 8
        baseFaces.Add(new TriangleIndices(7, 1, 8, count++)); // face 9

        // 5 faces around point 3
        baseFaces.Add(new TriangleIndices(3, 9, 4, count++)); // face 10
        baseFaces.Add(new TriangleIndices(3, 4, 2, count++)); // face 11
        baseFaces.Add(new TriangleIndices(3, 2, 6, count++)); // face 12
        baseFaces.Add(new TriangleIndices(3, 6, 8, count++)); // face 13
        baseFaces.Add(new TriangleIndices(3, 8, 9, count++)); // face 14

        // 5 adjacent faces 
        baseFaces.Add(new TriangleIndices(4, 9, 5, count++)); // face 15
        baseFaces.Add(new TriangleIndices(2, 4, 11, count++)); // face 16
        baseFaces.Add(new TriangleIndices(6, 2, 10, count++)); // face 17
        baseFaces.Add(new TriangleIndices(8, 6, 7, count++)); // face 18
        baseFaces.Add(new TriangleIndices(9, 8, 1, count++)); // face 19


        // refine triangles
        int fIdx = 0;
        List<TriangleIndices> faces = new List<TriangleIndices>(baseFaces);
        for (int i = 0; i < recursionLevel; i++){
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (TriangleIndices tri in faces){
                // replace triangle by 4 triangles
                int a = GetMiddlePoint(tri.v1, tri.v2);
                int b = GetMiddlePoint(tri.v2, tri.v3);
                int c = GetMiddlePoint(tri.v3, tri.v1);

                faces2.Add(new TriangleIndices(tri.v1, a, c, count++));
                faces2.Add(new TriangleIndices(tri.v2, b, a, count++));
                faces2.Add(new TriangleIndices(tri.v3, c, b, count++));
                faces2.Add(new TriangleIndices(a, b, c, count++));
                faceChildren.Add(fIdx++, faces2);
            }
            faces = faces2;
        }

        // done, now add triangles to mesh
        foreach (TriangleIndices tri in faces){
            triangles.Add(tri.v1);
            triangles.Add(tri.v2);
            triangles.Add(tri.v3);
        }

		// Finally, generate our mesh.
		Mesh geometry = new Mesh();
		geometry.vertices = vertices.ToArray();
		geometry.triangles = triangles.ToArray();
		geometry.RecalculateNormals();
        return geometry;
    } // End of Create().

    // Is point inside the cone created by the 3 planes from the origin to the sides of the face triangle
    bool InsideFaceCone(TriangleIndices triIndices, Vector3 pt) {
        Plane testPlane0 = new Plane(Vector3.zero, vertices[triIndices.v1], vertices[triIndices.v2]);
        Plane testPlane1 = new Plane(Vector3.zero, vertices[triIndices.v2], vertices[triIndices.v3]);
        Plane testPlane2 = new Plane(Vector3.zero, vertices[triIndices.v3], vertices[triIndices.v1]);
        return testPlane0.GetSide(pt) && testPlane1.GetSide(pt) && testPlane2.GetSide(pt);
    }

    bool InsideFaceConeInclusive(TriangleIndices triIndices, Vector3 pt) {
        Plane testPlane0 = new Plane(Vector3.zero, vertices[triIndices.v1], vertices[triIndices.v2]);
        Plane testPlane1 = new Plane(Vector3.zero, vertices[triIndices.v2], vertices[triIndices.v3]);
        Plane testPlane2 = new Plane(Vector3.zero, vertices[triIndices.v3], vertices[triIndices.v1]);
        return testPlane0.GetDistanceToPoint(pt) >= 0f && testPlane1.GetDistanceToPoint(pt) >= 0f && testPlane2.GetDistanceToPoint(pt) >= 0f;
    }

    // Check in a recursive manner, biggest triangles first, then drill down
    // should be able to create a more direct mapping (ranges of angles define which face it belongs to)
    // Ideally the angle to the point should immediately identify the correct triangle face.
    private int GetProjectedFaceImpl(List<TriangleIndices> faces, Vector3[] verts, Vector3 pt, out bool inside) {
        for (int i = 0; i < faces.Count; ++i) {
            if (InsideFaceCone(faces[i], pt)) {

                // check if inside face
                Plane testFacePlane = new Plane(GetVert(verts,faces[i].v3), GetVert(verts,faces[i].v2), GetVert(verts,faces[i].v1));
                inside = testFacePlane.GetSide(pt);
                if (faceChildren.ContainsKey(faces[i].id))
                    return GetProjectedFaceImpl(faceChildren[faces[i].id], verts, pt, out inside);
                return i;
            }
        }

        // if we get here, the point is probably co-planar, check again inclusively
        // Code duplication - should probably refactor (could always check inclusively)
        for (int i = 0; i < faces.Count; ++i) {
            if (InsideFaceConeInclusive(faces[i], pt)) {

                // check if inside face
                Plane testFacePlane = new Plane(GetVert(verts, faces[i].v3), GetVert(verts, faces[i].v2), GetVert(verts, faces[i].v1));
                inside = testFacePlane.GetSide(pt);
                if (faceChildren.ContainsKey(faces[i].id))
                    return GetProjectedFaceImpl(faceChildren[faces[i].id], verts, pt, out inside);
                return i;
            }
        }
        inside = false;
        return -1;
    }

    public int GetProjectedFace(Vector3 pt, Vector3[] verts, out bool inside) {
        return GetProjectedFaceImpl(baseFaces, verts, pt, out inside);
    }

    public Vector3 GetVert(Vector3[] verts, int idx) {
        if (verts == null)
            return vertices[idx];
        return verts[idx];
    }

} // End of IcosphereGenerator.
