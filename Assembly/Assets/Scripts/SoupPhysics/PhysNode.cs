using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PhysNode : MonoBehaviour {

	public static List<PhysNode> all = new List<PhysNode>();

	public IntVector3 hexPos = IntVector3.zero;
	List<PhysNeighbor> neighbors = new List<PhysNeighbor>();

	[System.Serializable]
	class PhysNeighbor {
		public PhysNode physNode = null;
		public Quaternion dir = Quaternion.identity;
	} // End of PhysNeighbor.

	Vector3 rotationVector = Random.onUnitSphere;
	float wigglePhase = Random.Range(0.1f, 20f);
	float wiggleMaxAngVel = Random.Range(0f, 300f);

	public Transform cubeTransform = null;


	void Awake(){
		all.Add(this);
	} // End of Awake().


	void Start(){
		cubeTransform.renderer.material.color = new Color(Random.RandomRange(0f, 1f), Random.RandomRange(0f, 1f), Random.RandomRange(0f, 1f), 1f);
		cubeTransform.rotation = Random.rotation;
	} // End of Start().


	void Update(){
		float wiggleVel = Mathf.Sin(Time.time * (2f * Mathf.PI) * (1f / wigglePhase));
		transform.Rotate(rotationVector, wiggleMaxAngVel * wiggleVel * Time.deltaTime);

		// Node tests each neighbor's target position in relation to it.
		for(int i = 0; i < neighbors.Count; i++){
			PhysNeighbor curNeighbor = neighbors[i];
			PhysNode curNeighborNode = curNeighbor.physNode;
			Vector3 vecToNeighborTargetPos = curNeighborNode.transform.position - (transform.position + ((transform.rotation * curNeighbor.dir) * Vector3.forward)); 

			transform.position += vecToNeighborTargetPos * 0.5f;
			curNeighborNode.transform.position -= vecToNeighborTargetPos * 0.5f;

			transform.rotation = Quaternion.Lerp(transform.rotation, curNeighborNode.transform.rotation, 0.5f);
			curNeighborNode.transform.rotation = Quaternion.Lerp(curNeighborNode.transform.rotation, transform.rotation, 0.5f);

			Debug.DrawLine(curNeighborNode.transform.position, transform.position + ((transform.rotation)* curNeighbor.dir) * Vector3.forward);

		}

		/*
		for(int i = 0; i < all.Count; i++){
			PhysNode someNode = all[i];
			Vector3 vecToNode = someNode.transform.position - transform.position;
			if(vecToNode.sqrMagnitude < 1f){
				transform.position += vecToNode.normalized * (vecToNode.magnitude - 1f) * 0.5f;
				someNode.transform.position -= vecToNode.normalized * (vecToNode.magnitude - 1f) * 0.5f;
			}
		}
		*/

		if(neighbors.Count == 0)
			Destroy(gameObject);
	} // End of Update().


	public void AttachNeighbor(PhysNode _newNeighbor, Quaternion _dir){
		// Early out if we already have this neighbor.
		for(int i = 0; i < neighbors.Count; i++)
			if(neighbors[i].physNode == _newNeighbor)
				return;

		PhysNeighbor newNeighbor = new PhysNeighbor();
		newNeighbor.physNode = _newNeighbor;
		newNeighbor.dir = _dir;
		neighbors.Add(newNeighbor);
		newNeighbor.physNode.AttachNeighbor(this, _dir * Quaternion.Euler(0f, 180f, 0f));

	} // End of AttachNeighbor().


	void OnDestroy(){
		all.Remove(this);
	} // End of OnDestroy().


} // End of PhysNode.
