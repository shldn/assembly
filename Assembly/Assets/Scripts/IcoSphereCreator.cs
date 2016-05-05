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

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    } // End of TriangleIndices.


	public static IcoSphereCreator Inst;

    private int index;
    private Dictionary<Int64, int> middlePointIndexCache;
	List<int> triangles;
	List<Vector3> vertices;


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

        AddVertex(new Vector3(-1,  t,  0));
        AddVertex(new Vector3( 1,  t,  0));
        AddVertex(new Vector3(-1, -t,  0));
        AddVertex(new Vector3( 1, -t,  0));

        AddVertex(new Vector3( 0, -1,  t));
        AddVertex(new Vector3( 0,  1,  t));
        AddVertex(new Vector3( 0, -1, -t));
        AddVertex(new Vector3( 0,  1, -t));

        AddVertex(new Vector3( t,  0, -1));
        AddVertex(new Vector3( t,  0,  1));
        AddVertex(new Vector3(-t,  0, -1));
        AddVertex(new Vector3(-t,  0,  1));


        // create 20 triangles of the icosahedron
        List<TriangleIndices> faces = new List<TriangleIndices>();

        // 5 faces around point 0
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));


        // refine triangles
        for (int i = 0; i < recursionLevel; i++){
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach (TriangleIndices tri in faces){
                // replace triangle by 4 triangles
                int a = GetMiddlePoint(tri.v1, tri.v2);
                int b = GetMiddlePoint(tri.v2, tri.v3);
                int c = GetMiddlePoint(tri.v3, tri.v1);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
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

    // Test the 15 faces, then recurse to find the correct face
    // should be able to create a more direct mapping (ranges of angles define which face it belongs to)
    // If a point is inside a triangular cone to the face, then it maps to that face.
    // Check in a recursive manner, biggest triangles first, then drill down
    // Ideally the distance from the triangle cone edges should immediately identify the correct triangle face.
    public int GetProjectedFace(Vector3 pt, out bool inside) {

        // Narrow down which of the original 20 faces the ray intersects.

        // Is in first set of five faces around point 0
        Vector3 section1normal = vertices[0].normalized;
        Debug.LogError("Normal: " + section1normal);
        Plane section1TestPlane = new Plane(section1normal, vertices[1]);
        inside = section1TestPlane.GetSide(pt);

        return -1;
    }

} // End of IcosphereGenerator.
