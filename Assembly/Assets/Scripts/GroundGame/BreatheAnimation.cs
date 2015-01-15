using UnityEngine;
using System.Collections;

public class BreatheAnimation : MonoBehaviour {

    public float scale = 0.01f;
    public float timeScale = 1.0f;
    public Vector3 angle = Vector3.up;
    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        int i = 0;
        while (i < vertices.Length)
        {
            Vector3 normal = ( normals.Length <= i ) ? (vertices[i] - gameObject.transform.position).normalized : normals[i];
            if (Vector3.Angle(normal, angle) <= 90f)
                vertices[i] += scale * normal * Mathf.Sin(timeScale * Time.time);        
            i++;
        }
        mesh.vertices = vertices;
    }
}
