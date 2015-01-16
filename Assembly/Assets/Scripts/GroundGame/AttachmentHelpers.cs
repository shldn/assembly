using UnityEngine;
using System.Collections;

public class AttachmentHelpers{

    // Attaches a spring to the parent game object and returns the spring
    public static GameObject AttachSpringToObject(GameObject parent, Vector3 positionOffset, Vector3 attachNormal)
    {
        // create spring
        Object prefabObj = Resources.Load("GroundGame/Spring");
        GameObject spring = GameObject.Instantiate(prefabObj) as GameObject;
        spring.transform.position = parent.transform.position + positionOffset;

        // randomly rotate spring direction vector, stay on proper side of the plane orthogonal to attachNormal.
        // Random vector on unit hemisphere about y-up vector
        Vector3 randV = Random.onUnitSphere;
        randV.y = Mathf.Abs(randV.y);

        Quaternion adjustment = Quaternion.FromToRotation(Vector3.up, attachNormal);
        spring.transform.up = adjustment * randV;

        // attach spring to parent object
        FixedJoint joint = parent.AddComponent<FixedJoint>();
        joint.connectedBody = spring.rigidbody;

        return spring;
    }

    // Chooses a random triangle on the mesh and returns the position of the center of the triangle.
    public static Vector3 GetRandomAttachPoint(Mesh m, out Vector3 normal)
    {
        int triangleIdx = Random.Range(0, (m.triangles.Length / 3));
        return GetAttachPoint(m, triangleIdx, out normal);
    }

    // returns the position of the center of the indicated triangle on mesh m.
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
