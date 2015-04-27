using UnityEngine;
using System.Collections;

public class Envirotest : MonoBehaviour {

	public Transform transform;
	public int samples = 100;
	public float radius = 100f;
	public float transformScale = 10f;
	public float rotationOffset = 0f;
	public bool randomRotation = false;


	// Use this for initialization
	void Start () {

		Vector3[] points = MathUtilities.FibonacciSphere(samples);
		for(int i = 0; i < points.Length; i++){
			Transform newTrans = Instantiate(transform, points[i] * radius, Quaternion.LookRotation(points[i])) as Transform;
			newTrans.localScale *= transformScale;
			newTrans.Rotate(Vector3.forward, Random.Range(0f, 360f));
			newTrans.Rotate(Vector3.right, rotationOffset);
		}
	
	} // End of Start().
} // End of Envirotest.
