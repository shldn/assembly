using UnityEngine;
using System.Collections;

public class TextsFaceToCam : MonoBehaviour {

	public Transform camera;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.LookAt (camera);
	}
}
