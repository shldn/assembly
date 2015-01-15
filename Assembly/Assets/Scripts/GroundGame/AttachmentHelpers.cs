using UnityEngine;
using System.Collections;

public class AttachmentHelpers{

    public static void AttachSpringToObject(GameObject parent, Vector3 positionOffset, Vector3 attachNormal)
    {
        // create spring
        Object prefabObj = Resources.Load("GroundGame/Spring");
        GameObject spring = GameObject.Instantiate(prefabObj) as GameObject;
        spring.transform.position = parent.transform.position + positionOffset;
        spring.transform.up = attachNormal;

        // attach spring to parent object
        FixedJoint joint = parent.AddComponent<FixedJoint>();
        joint.connectedBody = spring.rigidbody;
    }

    public static Vector3 GetRandomAttachPoint(Mesh m, out Vector3 normal)
    {
        int triangleIdx = Random.Range(0, (m.triangles.Length / 3));
        return GetAttachPoint(m, triangleIdx, out normal);
    }

    public static Vector3 GetAttachPoint(Mesh m, int triangleIdx, out Vector3 normal)
    {
        triangleIdx *= 3;
        int[] vertIdx = { m.triangles[triangleIdx + 0], m.triangles[triangleIdx + 1], m.triangles[triangleIdx + 2] };
        normal = Vector3.up;
        if (m.normals.Length > vertIdx[0])
            normal = (m.normals[vertIdx[0]] + m.normals[vertIdx[1]] + m.normals[vertIdx[2]]) / 3.0f;
        return (m.vertices[vertIdx[0]] + m.vertices[vertIdx[1]] + m.vertices[vertIdx[2]]) / 3.0f;
    }

}
