using UnityEngine;
using System.Collections;

public class NeuroScaleDemo : MonoBehaviour {

	public static NeuroScaleDemo Inst;

	// How much information is shown to the user, from 0f (very little) to 1f (everything)
	float enviroScale = 0f;

	// As this increases, the number of nodes shown will increase--chosen radiating out from the origin node.
	int numNodesToShow = 1;
	int numFoodToShow = 0;





	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogColor = Color.black;
	} // End of Start().
	
	void Update(){
		
		if((!CameraControl.Inst.selectedNode || CameraControl.Inst.selectedNode.cull) && (Node.getAll.Count > 0)){
			CameraControl.Inst.selectedNode = Node.getAll[Random.Range(0, Node.getAll.Count)];
			print("Getting new node to focus.");
		}

		
		CameraControl.Inst.targetRadius = 10f + (200f * Mathf.Pow(enviroScale, 1f));
		CameraControl.Inst.radius = CameraControl.Inst.targetRadius;

		RenderSettings.fogStartDistance = CameraControl.Inst.targetRadius + (1000f * Mathf.Pow(enviroScale, 2f));
		RenderSettings.fogEndDistance = RenderSettings.fogStartDistance * 2f;

		numNodesToShow = 1 + Mathf.RoundToInt(Mathf.Pow(enviroScale, 3f) * Node.getAll.Count);
		numFoodToShow = Mathf.RoundToInt(Mathf.Pow(enviroScale, 2f) * FoodPellet.all.Count);


		SortNodes();
		SortFood();
		for(int i = 0; i < Node.getAll.Count; i++){
			if(Node.getAll[i].cubeTransform)
				Node.getAll[i].cubeTransform.renderer.enabled = i < numNodesToShow;
		}
		for(int i = 0; i < FoodPellet.all.Count; i++){
			FoodPellet.all[i].render = i < numNodesToShow;
		}

		if(Input.GetKey(KeyCode.UpArrow))
			enviroScale += Time.deltaTime * 0.2f;
		if(Input.GetKey(KeyCode.DownArrow))
			enviroScale -= Time.deltaTime * 0.2f;
		enviroScale = Mathf.Clamp01(enviroScale);


		print(RenderSettings.fogStartDistance);
	} // End of Update().


	// bubble-sort nodes
	void SortNodes(){
		for (int i = 0; i < Node.getAll.Count - 1; i ++ ){
			float sqrMag1 = (Node.getAll[i + 0].Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
			float sqrMag2 = (Node.getAll[i + 1].Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
         
			if(sqrMag2 < sqrMag1){
				Node tempStore = Node.getAll[i];
				Node.getAll[i] = Node.getAll[i + 1];
				Node.getAll[i + 1] = tempStore;
				i = 0;
			}
		}
	} // End of SortNodes().

	// bubble-sort nodes
	void SortFood(){
		for (int i = 0; i < FoodPellet.all.Count - 1; i ++ ){
			float sqrMag1 = (FoodPellet.all[i + 0].WorldPosition - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
			float sqrMag2 = (FoodPellet.all[i + 1].WorldPosition - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
         
			if(sqrMag2 < sqrMag1){
				FoodPellet tempStore = FoodPellet.all[i];
				FoodPellet.all[i] = FoodPellet.all[i + 1];
				FoodPellet.all[i + 1] = tempStore;
				i = 0;
			}
		}
	} // End of SortNodes().
} // End of NeuroScaleDemo.
