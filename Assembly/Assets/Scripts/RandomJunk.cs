using UnityEngine;
using System.Collections;

public class RandomJunk : MonoBehaviour {

    public static RandomJunk Inst = null;

    public GameObject[] junkObjects;

	// Use this for initialization
	void Start () {
	    Inst = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
