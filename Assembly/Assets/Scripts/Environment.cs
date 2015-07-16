using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	public static Environment Inst;


	void Awake(){
		Inst = this;
	} // End of Awake().


	// Update is called once per frame
	void Update () {
		transform.localScale = NodeController.Inst.worldSize;	
	}
}
