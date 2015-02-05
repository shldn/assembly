using UnityEngine;
using System.Collections;

public class SpringCreature : MonoBehaviour {

    public int numSprings = 5;
    public int repeatDelayMin = 1;
    public int repeatDelayMax = 6;
	
	void Start () {
        Vector3 normal = Vector3.up;
        Matrix4x4 scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        for (int i = 0; i < numSprings; ++i)
        {
            GameObject spring = AttachmentHelpers.AttachSpringToObject(gameObject, (scaleAdjust * AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal)), normal);

            // set repeat delay
            BoxColliderStagedScaler scaler = spring.GetComponent<BoxColliderStagedScaler>();
            scaler.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);
        }
	}
	

}
