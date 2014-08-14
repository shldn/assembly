using UnityEngine;
using System.Collections;

public class RandomCameraTilt : MonoBehaviour {

    Quaternion initialPosition;
    float rotationOffset;
    public float maxAngle = 5f;

	// Use this for initialization
	void Start () {
	    initialPosition = transform.rotation;
        rotationOffset = Random.Range(0f, Mathf.PI * 2f);

	}
	
	// Update is called once per frame
	void Update () {
	    transform.rotation = initialPosition * Quaternion.AngleAxis(Mathf.Cos((Time.time * 0.5f) + rotationOffset) * maxAngle, Vector3.up);
	    transform.rotation *= Quaternion.AngleAxis(Mathf.Sin((Time.time * 0.5f) + rotationOffset) * maxAngle, Vector3.right);
	}
}
