using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpringCreature : MonoBehaviour, CaptureObject {

    public int numSprings = 5;
    public int repeatDelayMin = 1;
    public int repeatDelayMax = 6;
    Matrix4x4 scaleAdjust;
    HashSet<int> attachedTriangles = new HashSet<int>();

    public Vector3 Position { get { return transform.position; } }

    void Awake()
    {
        PersistentGameManager.CaptureObjects.Add(this);
    }

	void Start () {
        if (numSprings < 0)
            AddSpringsToAllFaces();
        for (int i = 0; i < numSprings; ++i)
            AddSpringImpl(false);
	}

    public bool AddSpring()
    {
        return AddSpringImpl(true);
    }

    private bool AddSpringImpl(bool incrementNumSprings = true)
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int triangleIdx = -1;
        Vector3 attachPoint = transform.rotation * (scaleAdjust * AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal, out triangleIdx, attachedTriangles));
        if (triangleIdx == -1)
            return false;
        attachedTriangles.Add(triangleIdx);
        GameObject spring = AttachmentHelpers.AttachSpringToObject(gameObject, attachPoint, transform.rotation * normal);

        // set repeat delay
        BoxColliderStagedScaler scaler = spring.GetComponent<BoxColliderStagedScaler>();
        scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);

        if (incrementNumSprings)
            ++numSprings;
        return true;
    }

    public void AddSpringsToAllFaces()
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int numTris = gameObject.GetComponent<MeshFilter>().mesh.triangles.Length / 3;
        for (numSprings = 0; numSprings < numTris; ++numSprings)
        {
            GameObject spring = AttachmentHelpers.AttachSpringToObject(gameObject, transform.rotation * (scaleAdjust * AttachmentHelpers.GetAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, numSprings, out normal)), transform.rotation * normal);
            
            // set repeat delay
            BoxColliderStagedScaler scaler = spring.GetComponent<BoxColliderStagedScaler>();
            if( scaler != null )
                scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);
        }

    }

    public void Destroy(){
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        PersistentGameManager.CaptureObjects.Remove(this);
    }
	

}
