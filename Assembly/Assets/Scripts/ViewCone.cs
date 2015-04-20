using UnityEngine;
using System.Collections;

public class ViewCone : MonoBehaviour {

	public Transform leftArc;
	public Transform rightArc;
	public float fovAngle = 45f;


	void Update(){
		leftArc.localEulerAngles = new Vector3(0f, 0f, fovAngle * 0.5f);
		rightArc.localEulerAngles = new Vector3(0f, 0f, -fovAngle * 0.5f);
	} // End of Update().

} // End of ViewCone.
