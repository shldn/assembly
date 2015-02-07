using UnityEngine;
using System.Collections;

public class BillboardCamera : MonoBehaviour {
	
	public bool deg90fix;
	

	void Update(){
	
		transform.rotation = Camera.main.transform.rotation;
		
	} // End of Update().
}
