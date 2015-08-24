using UnityEngine;
using System.Collections;

public class NeuroScaleDemo : MonoBehaviour {

	public static NeuroScaleDemo Inst;

	// How much information is shown to the user, from 0f (very little) to 1f (everything)
	float enviroScale = 0f;




	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogColor = Color.black;
	} // End of Start().
	
	void Update(){
		if(!CameraControl.Inst.selectedAssembly && (Assembly.getAll.Count > 0))
			CameraControl.Inst.selectedAssembly = Assembly.getAll[Random.Range(0, Assembly.getAll.Count)];

		RenderSettings.fogStartDistance = 10f + (1000f * enviroScale);
		RenderSettings.fogEndDistance = RenderSettings.fogStartDistance * 1.2f;

		CameraControl.Inst.targetRadius = 10f + (200f * enviroScale);
		CameraControl.Inst.radius = CameraControl.Inst.targetRadius;


		if(Input.GetKey(KeyCode.UpArrow))
			enviroScale += Time.deltaTime * 0.2f;
		if(Input.GetKey(KeyCode.DownArrow))
			enviroScale -= Time.deltaTime * 0.2f;
		enviroScale = Mathf.Clamp01(enviroScale);

		print(RenderSettings.fogStartDistance);
	} // End of Update().
}
