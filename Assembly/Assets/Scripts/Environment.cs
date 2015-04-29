using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		transform.localScale = NodeController.Inst.worldSize;	
	}
}
