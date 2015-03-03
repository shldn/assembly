using UnityEngine;
using System.Collections;

/*
Derived from Lengyel, Eric. “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon Software 3D Graphics Library, 2001.
[url]http://www.terathon.com/code/tangent.html[/url]
*/
 
public class TangentSolver
{
    public static void Solve(Mesh theMesh, int calcsPerFrame = 1024)
    {
        int vertexCount = theMesh.vertexCount;
        Vector3[] vertices = theMesh.vertices;
        Vector3[] normals = theMesh.normals;
        Vector2[] texcoords = theMesh.uv;
        int[] triangles = theMesh.triangles;
        int triangleCount = triangles.Length/3;
        Vector4[] tangents = new Vector4[vertexCount];
        Vector3[] tan1 = new Vector3[vertexCount];
        Vector3[] tan2 = new Vector3[vertexCount];
        int i1, i2, i3, tri = 0;
		Vector3 v1, v2, v3, sdir, tdir, n, t;
		Vector2 w1, w2, w3;
		float x1, x2, y1, y2, z1, z2, s1, s2, t1, t2, r;
		
        for ( int i = 0; i < (triangleCount); i++)
        {
            i1 = triangles[tri];
            i2 = triangles[tri+1];
            i3 = triangles[tri+2];
           
            v1 = vertices[i1];
            v2 = vertices[i2];
            v3 = vertices[i3];
           
            w1 = texcoords[i1];
            w2 = texcoords[i2];
            w3 = texcoords[i3];
           
            x1 = v2.x - v1.x;
            x2 = v3.x - v1.x;
            y1 = v2.y - v1.y;
            y2 = v3.y - v1.y;
            z1 = v2.z - v1.z;
            z2 = v3.z - v1.z;
           
            s1 = w2.x - w1.x;
            s2 = w3.x - w1.x;
            t1 = w2.y - w1.y;
            t2 = w3.y - w1.y;

			r = (s1 * t2 - s2 * t1);
			if (r != 0)		// if UV coords are in error (too many zeros in s & t) then we'll get a 0 here.
	            r = 1f / (s1 * t2 - s2 * t1);
			else
				r = 0.01f;
			
            sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
            tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
           
            tan1[i1] += sdir;
            tan1[i2] += sdir;
            tan1[i3] += sdir;
           
            tan2[i1] += tdir;
            tan2[i2] += tdir;
            tan2[i3] += tdir;
           
            tri += 3;
//			if (i % calcsPerFrame == 0)
//				yield return new WaitForEndOfFrame();
        }
       
        for (int i = 0; i < (vertexCount); i++)
        {
            n = normals[i];
            t = tan1[i];
           
            // Gram-Schmidt orthogonalize
            Vector3.OrthoNormalize( ref n, ref t );
           
            tangents[i].x  = t.x;
            tangents[i].y  = t.y;
            tangents[i].z  = t.z;
       
            // Calculate handedness
            tangents[i].w = ( Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f ) ? -1.0f : 1.0f;
        }      
        theMesh.tangents = tangents;
    }
}


//public class TangentSolver
//{
//    public static IEnumerator Solve(Mesh mesh, int calcsPerFrame = 64)
//    {
//		int i1, i2, i3;
//		float x1, x2, y1, y2, z1, z2, s1, s2, t1, t2, r;
//		Vector3 v1, v2, v3, sdir, tdir, n, t, tmp;
//		Vector2 w1, w2, w3;
//        int triangleCount = mesh.triangles.Length; // / 3;	// why only 1/3?
//        int vertexCount = mesh.vertices.Length;
//
//        Vector3[] tan1 = new Vector3[vertexCount];
//        Vector3[] tan2 = new Vector3[vertexCount];
//
//        Vector4[] tangents = new Vector4[vertexCount];
//
//        for(int a = 0; a < triangleCount; a+=3)
//        {
//            i1 = mesh.triangles[a+0];
//            i2 = mesh.triangles[a+1];
//            i3 = mesh.triangles[a+2];
//
//            v1 = mesh.vertices[i1];
//            v2 = mesh.vertices[i2];
//            v3 = mesh.vertices[i3];
//
//            w1 = mesh.uv[i1];
//            w2 = mesh.uv[i2];
//            w3 = mesh.uv[i3];
//
//            x1 = v2.x - v1.x;
//            x2 = v3.x - v1.x;
//            y1 = v2.y - v1.y;
//            y2 = v3.y - v1.y;
//            z1 = v2.z - v1.z;
//            z2 = v3.z - v1.z;
//
//            s1 = w2.x - w1.x;
//            s2 = w3.x - w1.x;
//            t1 = w2.y - w1.y;
//            t2 = w3.y - w1.y;
//
//            r = 1.0f / (s1 * t2 - s2 * t1);
//
//            sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
//            tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);
//
//            tan1[i1] += sdir;
//            tan1[i2] += sdir;
//            tan1[i3] += sdir;
//
//            tan2[i1] += tdir;
//            tan2[i2] += tdir;
//            tan2[i3] += tdir;
//			if (a % calcsPerFrame == 0)
//				yield return new WaitForEndOfFrame();
//        }
//
//
//        for (int a = 0; a < vertexCount; ++a)
//        {
//            n = mesh.normals[a];
//            t = tan1[a];
//
//            tmp = (t - n * Vector3.Dot(n, t)).normalized;
//            tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
//
//            tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
////			if (a % calcsPerFrame == 0)
////				yield return new WaitForEndOfFrame();
//        }
//
//        mesh.tangents = tangents;
//    }
//}
