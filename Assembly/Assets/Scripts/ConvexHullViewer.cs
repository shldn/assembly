using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ConvexHullViewer : MonoBehaviour {

    public ConvexHull hull = null;
	
    void LateUpdate()
    {
        if (hull == null)
            return;
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        hull.UpdateMesh(ref mesh);
    }
}
