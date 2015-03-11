using UnityEngine;
using System.Collections;

public class PhysFood : MonoBehaviour {

	Vector3 worldPosition = Vector3.zero;
	Quaternion worldRotation = Quaternion.identity;

	Transform transform = null;


	public PhysFood(Vector3 position){
		worldPosition = position;
		worldRotation = Random.rotation;
		transform = Instantiate(PhysNodeController.Inst.physFoodPrefab, worldPosition, worldRotation) as Transform;
	} // End of constructor.


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
