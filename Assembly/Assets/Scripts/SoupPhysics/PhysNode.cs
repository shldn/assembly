using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PhysNode : MonoBehaviour {

	public static List<PhysNode> all = new List<PhysNode>();

	public IntVector3 hexPos = IntVector3.zero;
	List<PhysNeighbor> neighbors = new List<PhysNeighbor>();
	int lastNeighborCount = 0;

	// Type-specific elements
	TimedTrailRenderer trail = null;
	Transform viewCone = null;

	[System.Serializable]
	class PhysNeighbor {
		public PhysNode physNode = null;
		public Quaternion dir = Quaternion.identity;
	} // End of PhysNeighbor.

	Vector3 rotationVector = Random.onUnitSphere;
	float wigglePhase = Random.Range(0.5f, 10f);
	float wiggleMaxAngVel = Random.Range(10f, 300f);

	float flailMaxDeflection = Random.Range(20f, 80f);
	Quaternion lastFlailOffset = Quaternion.identity; // For determining connection rotations.

	public Transform cubeTransform = null;


	void Awake(){
		all.Add(this);
	} // End of Awake().


	void Start(){
		//cubeTransform.rotation = Random.rotation;
	} // End of Start().


	void Update(){
		float wiggle = Mathf.Sin(Time.time * (2f * Mathf.PI) * (1f / wigglePhase));
		// -- Comment out to remove 'torqueing'
		if(neighbors.Count == 2)
			transform.Rotate(rotationVector, wiggleMaxAngVel * wiggle * Time.deltaTime);

		Quaternion flailOffset = Quaternion.identity;
		// -- Comment out to remove 'flailing'
		if(neighbors.Count == 2)
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

			transform.position += vecToNeighborTargetPos * 0.45f;
			curNeighborNode.transform.position -= vecToNeighborTargetPos * 0.45f;

			transform.rotation = Quaternion.Lerp(transform.rotation, curNeighborNode.transform.rotation, 0.5f);
			curNeighborNode.transform.rotation = Quaternion.Lerp(curNeighborNode.transform.rotation, transform.rotation, 0.5f);

			Quaternion ΔFlailOffset = Quaternion.Inverse(flailOffset) * lastFlailOffset;
			lastFlailOffset = flailOffset;
			// -- Comment out to remove 'flail relative torqueing'
			transform.rotation *= Quaternion.Inverse(ΔFlailOffset);
			curNeighborNode.transform.rotation *= ΔFlailOffset;

			// -- Comment out to remove 'swimming propulsion'
			transform.position += (transform.rotation * curNeighbor.dir) * Vector3.forward * (flailMaxDeflection / Mathf.Pow(wigglePhase, 2f)) * Time.deltaTime * 0.2f;

			Debug.DrawLine(curNeighborNode.transform.position, transform.position + ((transform.rotation)* curNeighbor.dir) * Vector3.forward);
		}
		
		// Update node type?
		if(neighbors.Count != lastNeighborCount){
			lastNeighborCount = neighbors.Count;

			switch(neighbors.Count){
			case 1 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.senseColor;
				Transform newViewConeTrans = Instantiate(PrefabManager.Inst.senseNodeBillboard, transform.position, transform.rotation) as Transform;
				viewCone = newViewConeTrans;
				break;
			case 2 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.actuateColor;
				Transform newTrailTrans = Instantiate(PrefabManager.Inst.motorNodeTrail, transform.position, transform.rotation) as Transform;
				newTrailTrans.parent = transform;
				trail = newTrailTrans.GetComponent<TimedTrailRenderer>();
				break;
			case 3 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.controlColor;
				break;
			}
		}

		// Type-specific behaviours
		switch(neighbors.Count){
			case 1 : 
				float viewConeSize = 2.5f;
				Debug.DrawRay(transform.position, transform.forward * 3f, Color.green);

				viewCone.position = transform.position + transform.forward * viewConeSize;
				viewCone.localScale = Vector3.one * viewConeSize;

				// Billboard the arc with the main camera.
				viewCone.rotation = transform.rotation;
				viewCone.position = transform.position + (viewCone.rotation * (Vector3.forward * viewConeSize * 0.5f));
				viewCone.rotation *= Quaternion.AngleAxis(90, Vector3.up);

				Vector3 camRelativePos = viewCone.InverseTransformPoint(Camera.main.transform.position);
				float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;
				viewCone.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);
				break;
			case 2 : 
				break;
			case 3 : 
				break;
			}
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
