using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpringCreature : MonoBehaviour {

    public int numInitSprings = 5;
    public int repeatDelayMin = 1;
    public int repeatDelayMax = 6;
    Matrix4x4 scaleAdjust;
    HashSet<int> attachedTriangles = new HashSet<int>();

    private int springCount = 0;
    public int SpringCount { get { return springCount; } set { springCount = value; } }
	
	void Start () {
        if (numInitSprings < 0)
            AddSpringsToAllFaces();
        for (int i = 0; i < numInitSprings; ++i)
            AddSpring();
	}

    public bool AddSpring()
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

        ++springCount;
        return true;
    }

    public void AddSpringsToAllFaces()
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int numTris = gameObject.GetComponent<MeshFilter>().mesh.triangles.Length / 3;
        for (springCount = 0; springCount < numTris; ++springCount)
        {
            GameObject spring = AttachmentHelpers.AttachSpringToObject(gameObject, transform.rotation * (scaleAdjust * AttachmentHelpers.GetAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, springCount, out normal)), transform.rotation * normal);
            
            // set repeat delay
            BoxColliderStagedScaler scaler = spring.GetComponent<BoxColliderStagedScaler>();
            if( scaler != null )
                scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);
        }

    }
	

}
