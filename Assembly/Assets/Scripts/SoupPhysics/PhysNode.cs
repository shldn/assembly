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
	float wigglePhase = Random.Range(0.2f, 10f);
	float wiggleMaxAngVel = Random.Range(0f, 500f);

	float flailMaxDeflection = Random.Range(0f, 60f);
	Quaternion lastFlailOffset = Quaternion.identity; // For determining connection rotations.

	public Transform cubeTransform = null;


	void Awake(){
		all.Add(this);
	} // End of Awake().


	void Start(){
		cubeTransform.renderer.material.color = new Color(Random.RandomRange(0f, 1f), Random.RandomRange(0f, 1f), Random.RandomRange(0f, 1f), 1f);
		cubeTransform.rotation = Random.rotation;
	} // End of Start().


	void Update(){
		float wiggle = Mathf.Sin(Time.time * (2f * Mathf.PI) * (1f / wigglePhase));
		// -- Comment out to remove 'torqueing'
		transform.Rotate(rotationVector, wiggleMaxAngVel * wiggle * Time.deltaTime);


		Quaternion flailOffset = Quaternion.identity;
		// -- Comment out to remove 'flailing'
		flailOffset = Quaternion.Euler(rotationVector * wiggle * flailMaxDeflection);


		// Node tests each neighbor's target position in relation to it.
		for(int i = 0; i < neighbors.Count; i++){
			PhysNeighbor curNeighbor = neighbors[i];
			PhysNode curNeighborNode = curNeighbor.physNode;
			if((curNeighbor == null) || !curNeighborNode){
				neighbors.Remove(curNeighbor);
				continue;
			}

			Vector3 vecToNeighborTargetPos = curNeighborNode.transform.position - (transform.position + ((transform.rotation * curNeighbor.dir * flailOffset) * Vector3.forward)); 

			transform.position += vecToNeighborTargetPos * 0.5f;
			curNeighborNode.transform.position -= vecToNeighborTargetPos * 0.5f;

			transform.rotation = Quaternion.Lerp(transform.rotation, curNeighborNode.transform.rotation, 0.5f);
			curNeighborNode.transform.rotation = Quaternion.Lerp(curNeighborNode.transform.rotation, transform.rotation, 0.5f);

			Quaternion ΔFlailOffset = Quaternion.Inverse(flailOffset) * lastFlailOffset;
			lastFlailOffset = flailOffset;
			// -- Comment out to remove 'flail relative torqueing'
			transform.rotation *= Quaternion.Inverse(ΔFlailOffset);
			curNeighborNode.transform.rotation *= ΔFlailOffset;

			// -- Comment out to remove 'swimming propulsion'
			transform.position += (transform.rotation * curNeighbor.dir) * Vector3.forward * (flailMaxDeflection / wigglePhase) * Time.deltaTime * 0.2f;

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
