using UnityEngine;
using System.Collections;
using UnityEngine.VR;


public class SetActiveIfVR : MonoBehaviour {

	void Start(){
		if(!VRDevice.isPresent)
			gameObject.SetActive(false);
	} // End of Start().

} // End of SetActiveIfVR.
