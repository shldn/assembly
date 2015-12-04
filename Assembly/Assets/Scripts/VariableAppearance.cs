using UnityEngine;
using System.Collections;

public class VariableAppearance : MonoBehaviour 
{
	public Transform targetPos;
	public Material blue;
	public Material cyan;
	public Material red;
	float origDistance;
	Vector3 origScale;
	Vector3 maxScale;

	// Use this for initialization
	void Start () {
		origDistance = Vector3.Distance (gameObject.transform.position, targetPos.position);
		origScale = gameObject.transform.localScale;
		float scaleX = origScale.x * 3;
		float scaleY = origScale.y * 3;
		float scaleZ = origScale.z * 3;
		maxScale = new Vector3 (scaleX, scaleY, scaleZ);
	}
	
	// Update is called once per frame
	void Update () {
		Debug.DrawLine (gameObject.transform.position, targetPos.position, Color.red);

		float distance = Vector3.Distance (gameObject.transform.position, targetPos.position);

		float myLerp = distance / (origDistance * 2);
		GetComponent<Renderer>().material.Lerp(cyan, blue, myLerp);
		transform.localScale = Vector3.Lerp (maxScale, origScale, myLerp);
	}
}
