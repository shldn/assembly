using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Creature : MonoBehaviour
{

    public int numAttachments = 5;
    public int repeatDelayMin = 1;
    public int repeatDelayMax = 6;
    Matrix4x4 scaleAdjust;
    HashSet<int> attachedTriangles = new HashSet<int>();

    void Start()
    {
        if (numAttachments < 0)
            AddObjectsToAllFaces();
        for (int i = 0; i < numAttachments; ++i)
            AddAttachmentImpl(false);
    }

    public bool AddAttachment()
    {
        return AddAttachmentImpl(true);
    }

    private bool AddAttachmentImpl(bool incrementAttachCount = true)
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int triangleIdx = -1;
        Vector3 attachPoint = transform.rotation * (scaleAdjust * AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal, out triangleIdx, attachedTriangles));
        if (triangleIdx == -1)
            return false;
        attachedTriangles.Add(triangleIdx);
        GameObject child = (Random.Range(0.0f,1.0f) >= 0.5f) 
            ? AttachmentHelpers.AttachActionObjectToObject(gameObject, attachPoint, transform.rotation * normal)
            : AttachmentHelpers.AttachSenseObjectToObject(gameObject, attachPoint, transform.rotation * normal);

        // spring specific settings - set repeat delay
        BoxColliderStagedScaler scaler = child.GetComponent<BoxColliderStagedScaler>();
        if( scaler != null )
            scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);

        if (incrementAttachCount)
            ++numAttachments;
        return true;
    }

    public void AddObjectsToAllFaces()
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int numTris = gameObject.GetComponent<MeshFilter>().mesh.triangles.Length / 3;
        for (numAttachments = 0; numAttachments < numTris; ++numAttachments)
        {
            GameObject spring = AttachmentHelpers.AttachSpringToObject(gameObject, transform.rotation * (scaleAdjust * AttachmentHelpers.GetAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, numAttachments, out normal)), transform.rotation * normal);

            // spring specific settings - set repeat delay
            BoxColliderStagedScaler scaler = spring.GetComponent<BoxColliderStagedScaler>();
            if (scaler != null)
                scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);
        }

    }


}
