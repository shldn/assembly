using UnityEngine;
using System.Collections;

public class MovementRange : MonoBehaviour {


	float maxValue;
	float minValue;
	float moveX;
	float moveZ;
	float moveY;
	// Use this for initialization
	void Start () {
		maxValue = 5.0f;
		minValue = -5.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		moveX = gameObject.transform.position.x;
		moveY = gameObject.transform.position.y;
		moveZ = gameObject.transform.position.z;

		if (gameObject.transform.position.x > maxValue) 
		{
			moveX = 5.0f;
		}
		if (gameObject.transform.position.x < minValue) 
		{
			moveX = -5.0f;
		}
		if (gameObject.transform.position.y > maxValue) 
		{
			moveY = 5.0f;
		}
		if (gameObject.transform.position.y < minValue) 
		{
			moveY = -5.0f;
		}
		if (gameObject.transform.position.z > maxValue) 
		{
			moveZ = 5.0f;
		}
		if (gameObject.transform.position.z < minValue) 
		{
			moveZ = -5.0f;
		}
			
	}
}
