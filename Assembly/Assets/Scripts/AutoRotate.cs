using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	public float rotateSpeed = 1f;
	
	// Update is called once per frame
	void Update () {
		transform.eulerAngles = new Vector3(0f, Time.time * (360f / rotateSpeed), 0f);
	}
}
