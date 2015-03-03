using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PhysNode {

	static HashSet<PhysNode> all = new HashSet<PhysNode>();
	public static HashSet<PhysNode> getAll {get{return all;}}
	public static implicit operator bool(PhysNode exists){return exists != null;}

	public Triplet localHexPos = Triplet.zero;
	public List<PhysNeighbor> neighbors = new List<PhysNeighbor>();
	int lastNeighborCount = 0;

	PhysAssembly physAssembly = null;
	public PhysAssembly PhysAssembly {get{return physAssembly;}}

	// Node attributes
	Quaternion actionRotation = Random.rotation;

	// Type-specific elements, effects
	public Transform cubeTransform = null;
	TimedTrailRenderer trail = null;
	Transform viewCone = null;

	[System.Serializable]
	public class PhysNeighbor {
		public PhysNode physNode = null;
		public Quaternion dir = Quaternion.identity;
		public float arrowDist = 1f;
	} // End of PhysNeighbor.

	Vector3 rotationVector = Random.onUnitSphere;
	float wigglePhase = Random.Range(1f, 10f);
	float wiggleMaxAngVel = Random.Range(10f, 200f);

	float flailMaxDeflection = Random.Range(10f, 80f);
	Quaternion lastFlailOffset = Quaternion.identity; // For determining connection rotations.

	float pushMult = Random.Range(0.5f, 2f);


	// These store position/rotation to be updated after neighbor math is done.
	Vector3 delayPosition = Vector3.zero;
	Quaternion delayRotation = Quaternion.identity;

	float power = 0f;
	float waveformRunner = 0f;

	public Vector3 position = Vector3.zero;
	public Quaternion rotation = Quaternion.identity;


	public PhysNode(PhysAssembly physAssembly, Triplet localHexPos){
		all.Add(this);
		this.physAssembly = physAssembly ;
		this.localHexPos = localHexPos;
		position = physAssembly.worldPosition + HexUtilities.HexToWorld(localHexPos);
		delayPosition = position;

		cubeTransform = MonoBehaviour.Instantiate(PhysNodeController.Inst.physNodePrefab, position, Quaternion.identity) as Transform;
	} // End of Awake().


	public void DoMath(){
		float wiggle = Mathf.Sin(waveformRunner * (2f * Mathf.PI) * (1f / wigglePhase));

		if(Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.Y))
			waveformRunner += PhysNodeController.physicsStep * power;
		else
			waveformRunner += PhysNodeController.physicsStep;

		bool functioningMuscle = (neighbors.Count == 2) && ((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2));

		// -- Comment out to remove 'torqueing'
		if(functioningMuscle)
			delayRotation *= Quaternion.AngleAxis(wiggleMaxAngVel * wiggle * PhysNodeController.physicsStep, rotationVector);

		Quaternion flailOffset = Quaternion.identity;
		// -- Comment out to remove 'flailing'
		if(functioningMuscle)
			flailOffset = Quaternion.Euler(rotationVector * wiggle * flailMaxDeflection * 0.00001f);

		Quaternion ΔFlailOffset = Quaternion.Inverse(flailOffset) * lastFlailOffset;
		lastFlailOffset = flailOffset;


		float sizeMult = 1f;
		//sizeMult = 1f + (/*(0.5f + (Mathf.Sin(Time.time * 2f) * 0.5f)) **/ ((neighbors.Count - 1f) * 0.1f)) * (Mathf.Lerp(1f, pushMult, 0.5f + (wiggle * 0.5f)));
		//cubeTransform.localScale = Vector3.one * sizeMult;


		// Node tests each neighbor's target position in relation to it.
		for(int i = 0; i < neighbors.Count; i++){
			PhysNeighbor curNeighbor = neighbors[i];
			PhysNode curNeighborNode = curNeighbor.physNode;
			if((curNeighbor == null) || !curNeighborNode){
				neighbors.Remove(curNeighbor);
				continue;
			}
			Debug.DrawLine(position, curNeighborNode.position, new Color(1f, 1f, 1f, 0.15f));

			Vector3 vecToNeighborTargetPos = curNeighborNode.position - (position + ((rotation * curNeighbor.dir * flailOffset) * Vector3.forward * sizeMult)); 


			// All nodes try to align to their 'resting position' with their neighbors.
			delayPosition += vecToNeighborTargetPos * 0.4f / neighbors.Count;
			curNeighborNode.delayPosition -= vecToNeighborTargetPos * 0.4f / neighbors.Count;
			
			float updateLerpBias = 0.25f;
			if((neighbors.Count == 2) && (curNeighborNode.neighbors.Count != 2)){
				// Trace motor nodes' neighbors and relative rotations.
				//Debug.DrawLine(position, position + (rotation * curNeighbor.dir * Vector3.forward), Color.cyan);
				//Debug.DrawLine(position, position + (rotation * curNeighbor.dir * flailOffset * Vector3.forward), Color.blue);

				if(i == 0){
					delayRotation *= Quaternion.Inverse(rotation) * Quaternion.Lerp(rotation, curNeighborNode.rotation * Quaternion.Inverse(flailOffset), updateLerpBias);
					//curNeighborNode.delayRotation *= Quaternion.Inverse(curNeighborNode.rotation) * Quaternion.Lerp(curNeighborNode.rotation, rotation * flailOffset, updateLerpBias);
				}
				if(i == 1){
					delayRotation *= Quaternion.Inverse(rotation) * Quaternion.Lerp(rotation, curNeighborNode.rotation * flailOffset, updateLerpBias);
					//curNeighborNode.delayRotation *= Quaternion.Inverse(curNeighborNode.rotation) * Quaternion.Lerp(curNeighborNode.rotation, rotation * Quaternion.Inverse(flailOffset), updateLerpBias);
				}

				// Muscle propulsion
				Vector3 propulsion = (rotation * curNeighbor.dir) * -Vector3.forward * (flailMaxDeflection / (1f + Mathf.Pow(wigglePhase, 2f))) * PhysNodeController.physicsStep * (1f - Mathf.Abs(wiggle)) * ((Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.Y))? power : 1f);
				
				delayPosition += propulsion;
				Debug.DrawRay(position, propulsion * 6f, Color.red);
			}
			// Inert nodes simply try to torque to match their neighbors.
			//else if((curNeighborNode.neighbors.Count != 2) || ((neighbors.Count == 2) && (curNeighborNode.neighbors.Count == 2))){
				delayRotation = Quaternion.Lerp(delayRotation, curNeighborNode.rotation, updateLerpBias);
				//delayRotation *= Quaternion.Inverse(transform.rotation) * Quaternion.Lerp(transform.rotation, curNeighborNode.transform.rotation, 0.5f);

				//curNeighborNode.delayRotation *= Quaternion.Inverse(curNeighborNode.transform.rotation) * Quaternion.Lerp(curNeighborNode.transform.rotation, transform.rotation, 0.5f);
			//}

			//GLDebug.DrawLine(transform.position, curNeighborNode.transform.position, new Color(1f, 1f, 1f, 0.1f), 0, false);
			//GLDebug.DrawLine(transform.position, curNeighborNode.transform.position, cubeTransform.renderer.material.color, 0, false);
		}
		
		// Update node type?
		if(neighbors.Count != lastNeighborCount){
			lastNeighborCount = neighbors.Count;

			switch(neighbors.Count){
			case 1 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.senseColor;
				Transform newViewConeTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.senseNodeBillboard, position, rotation) as Transform;
				viewCone = newViewConeTrans;
				break;
			case 2 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.actuateColor;
				break;
				if((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2)){
					Transform newTrailTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.motorNodeTrail, position, rotation) as Transform;
					newTrailTrans.parent = cubeTransform;
					trail = newTrailTrans.GetComponent<TimedTrailRenderer>();
				}
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
				Debug.DrawRay(position, (rotation * actionRotation * Vector3.forward) * 2f, Color.green);

				viewCone.position = position + (actionRotation * (rotation * Vector3.forward)) * viewConeSize;
				viewCone.localScale = Vector3.one * viewConeSize;

				// Billboard the arc with the main camera.
				viewCone.rotation = rotation * actionRotation;
				viewCone.position = position + (viewCone.rotation * (Vector3.forward * viewConeSize * 0.5f));
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

		// Reset power
		power = 0f;
	} // End of DoMath().


	public void UpdateTransform(){
		position = delayPosition;
		rotation = delayRotation;

		cubeTransform.position = position;
		cubeTransform.rotation = rotation;

		foreach(PhysNeighbor someNeighbor in neighbors)
			if(Random.Range(0f, 1f) < 0.2f)
				someNeighbor.arrowDist = Random.Range(0.25f, 0.4f);

		if((Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.Y)) && (neighbors.Count == 1))
			Transmit(new HashSet<PhysNode>(new PhysNode[]{this}), 1f);


		// Position error bandaid...
		if(position.x > 1000f || position.y > 1000f || position.z > 1000f)
			physAssembly.Destroy();
	} // End of UpdateTransform().


	public void AttachNeighbor(PhysNode _newNeighbor){
		// Early out if we already have this neighbor.
		for(int i = 0; i < neighbors.Count; i++)
			if(neighbors[i].physNode == _newNeighbor)
				return;

		PhysNeighbor newNeighbor = new PhysNeighbor();
		newNeighbor.physNode = _newNeighbor;
		newNeighbor.dir = Quaternion.LookRotation(_newNeighbor.position - position, Random.onUnitSphere);
		neighbors.Add(newNeighbor);
		newNeighbor.physNode.AttachNeighbor(this);
	} // End of AttachNeighbor().


	public void Destroy(){
		if(cubeTransform)
			MonoBehaviour.Destroy(cubeTransform.gameObject);

		all.Remove(this);
	} // End of OnDestroy().



	public void Transmit(HashSet<PhysNode> checkedNodes, float signalStrength){
		checkedNodes.Add(this);
		if(signalStrength < 0.02f)
			return;

		for(int i = 0; i < neighbors.Count; i++){
			PhysNode curNeighbor = neighbors[i].physNode;
			if(!checkedNodes.Contains(curNeighbor) && (curNeighbor.neighbors.Count > 1)){
				float sig = signalStrength / Mathf.Max(1f, (neighbors.Count - 1));
				curNeighbor.Transmit(new HashSet<PhysNode>(checkedNodes), sig * 0.95f);
				//curNeighbor.Transmit(checkedNodes);

				// Trace effect
				if(Input.GetKey(KeyCode.T))
					GLDebug.DrawLineArrow(position, Vector3.Lerp(position, curNeighbor.position, neighbors[i].arrowDist), 0.1f, 20f, new Color(1f, 1f, 0f, signalStrength), 0f, false);
			}
		}
		power += signalStrength;

	} // End of Transmit().


	/*
	void OnGUI(){
		if(Input.GetKey(KeyCode.T)){
			Vector3 nodeScreenPos = Camera.main.WorldToScreenPoint(transform.position);
			if((nodeScreenPos.z < 0f) || power.Equals(0f))
				return;

			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			//GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.width * 0.009f);
			GUI.skin.label.fontSize = 10;
			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUI.color = Color.black;
			GUI.Label(MathUtilities.CenteredSquare(nodeScreenPos.x, nodeScreenPos.y, 200f), power.ToString("F2"));
			GUI.color = Color.yellow;
			GUI.Label(MathUtilities.CenteredSquare(nodeScreenPos.x - 1f, nodeScreenPos.y + 1f, 200f), power.ToString("F2"));
		}
	} // End of OnGUI().
	*/

} // End of PhysNode.
