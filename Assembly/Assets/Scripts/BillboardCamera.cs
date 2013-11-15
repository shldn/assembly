using UnityEngine;
using System.Collections;

public class BillboardCamera : MonoBehaviour {
	
	public bool deg90fix;
	

	void Update(){
	
		transform.LookAt(Camera.main.transform, Camera.main.transform.up);
		
		if(deg90fix)
			transform.rotation *= Quaternion.AngleAxis (90, Vector3.right);
		
	} // End of Update().
}
