using UnityEngine;
using System.Collections;

public class ViewCone : MonoBehaviour {

	public Transform leftArc;
	public Transform rightArc;
	public float fovAngle = 45f;
	public bool render = true;
	Renderer[] myRenderers;


	void Start(){
		myRenderers = GetComponentsInChildren<Renderer>();
	} // End of Star().


	void Update(){
		leftArc.localEulerAngles = new Vector3(0f, 0f, fovAngle * 0.5f);
		rightArc.localEulerAngles = new Vector3(0f, 0f, -fovAngle * 0.5f);

		for(int i = 0; i < myRenderers.Length; i++)
			myRenderers[i].enabled = render;
	} // End of Update().

} // End of ViewCone.
