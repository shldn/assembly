using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttachmentHelpers{

    private static string[] actionPrefabPaths = { "Utopia/Spring", "Utopia/Propeller", "Utopia/Oarfin" };
    private static string[] sensePrefabPaths = { "Utopia/Nose", "Utopia/Ear", "Utopia/EyeLeft", "Utopia/TVAntenna" };


    // Attaches a spring to the parent game object and returns the spring
    public static GameObject AttachSpringToObject(GameObject parent, Vector3 positionOffset, Vector3 attachNormal)
    {
        return AttachObjectToObject(parent, "Utopia/Spring", positionOffset, attachNormal, true);
    }

    public static GameObject AttachObjectToObject(GameObject parent, string childPrefabPath, Vector3 positionOffset, Vector3 attachNormal, bool rotateChildObject = false)
    {
        // create child
        Object prefabObj = Resources.Load(childPrefabPath);
        GameObject child = GameObject.Instantiate(prefabObj) as GameObject;
        child.transform.position = parent.transform.position + positionOffset;
        child.transform.parent = parent.transform;
        child.transform.up = attachNormal;

        // randomly rotate direction vector, stay on proper side of the plane orthogonal to attachNormal.
        if( rotateChildObject )
        {
            float randAngle = Random.Range(UtopiaGameManager.Inst.minSpringAngleOffset, UtopiaGameManager.Inst.maxAngleOffset);
            float randAngleToRotateAxis = Random.Range(0.0f, 360.0f);
            // get random axis perpendicular to attachNormal
            Vector3 axis = Quaternion.AngleAxis(randAngleToRotateAxis, attachNormal) * GetAnyOrthogonalVector(attachNormal);

            // rotate a random angle (in range) about a random perpendicular axis -- will keep the vector in a cone maxAngleOffset from attachNormal
            child.transform.up = Quaternion.AngleAxis(randAngle, axis) * attachNormal;

        }


        // attach child to parent object
        if( child.GetComponent<Rigidbody>() )
        {
            FixedJoint joint = parent.AddComponent<FixedJoint>();
            joint.connectedBody = child.rigidbody;
        }

        return child;
    }

    public static GameObject AttachActionObjectToObject(GameObject parent, Vector3 positionOffset, Vector3 attachNormal, bool rotateChildObject = false)
    {
        int randomObjectIdx = Random.Range(0, actionPrefabPaths.Length);
        return AttachObjectToObject(parent, actionPrefabPaths[randomObjectIdx], positionOffset, attachNormal, rotateChildObject);
    }

    public static GameObject AttachSenseObjectToObject(GameObject parent, Vector3 positionOffset, Vector3 attachNormal, bool rotateChildObject = false)
    {
        int randomObjectIdx = Random.Range(0, sensePrefabPaths.Length);
        return AttachObjectToObject(parent, sensePrefabPaths[randomObjectIdx], positionOffset, attachNormal, rotateChildObject);
    }

    // Chooses a random triangle on the mesh and returns the position of the center of the triangle.
    public static Vector3 GetRandomAttachPoint(Mesh m, out Vector3 normal)
    {
        int triangleIdx = Random.Range(0, (m.triangles.Length / 3));
        return GetAttachPoint(m, triangleIdx, out normal);
    }

    public static Vector3 GetRandomAttachPoint(Mesh m, out Vector3 normal, out int triangleIdx, HashSet<int> avoidTris = null)
    {
        if (m.triangles.Length / 3 > avoidTris.Count)
        {
            int numTries = 100;
            for (int i = 0; i < numTries; ++i)
            {
                triangleIdx = Random.Range(0, (m.triangles.Length / 3));
                if (!avoidTris.Contains(triangleIdx))
                    return GetAttachPoint(m, triangleIdx, out normal);
            }
        }

        triangleIdx = -1;
        normal = Vector3.up;
        return Vector3.zero;
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

    // returns one of the infinite perpendicular vectors to the input vector.
    public static Vector3 GetAnyOrthogonalVector(Vector3 v)
    {
        if (v.x == 0.0f && v.y == 0.0f)
        {
            if (v.z == 0.0f)
            {
                Debug.LogError("Invalid input to GetAnyOrthogonalVector, must be non-zero vector");
                return v;
            }
            return Vector3.up;
        }
        return new Vector3(-v.y, v.x, 0.0f);
    }

}
