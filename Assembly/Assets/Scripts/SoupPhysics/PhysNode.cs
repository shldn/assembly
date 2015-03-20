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
	Quaternion actionRotation = Random.rotation; // The genetic 'zero rotation' inherent to the node.
	// The offset from actionRotation determined by incoming signal.
	// For sense nodes, this is the rotation from the sense node to the most powerful food node source.
	// For actuators, this rotation modifies the muscle output.
	public Quaternion signalRotation = Quaternion.identity;

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
	float smoothedPower = 0f;
	float waveformRunner = 0f;

	Vector3 velocity = Vector3.zero;

	Vector3 position  = Vector3.zero;
	public Vector3 Position {
		get{
			return position;
		}set{
			position = value;
			delayPosition = value;
		}
	}
	Quaternion rotation = Quaternion.identity;
	public Quaternion Rotation {
		get{
			return rotation;
		}set{
			rotation = value;
			delayRotation = value;
		}
	}

	// If true, node will be destroyed.
	public bool cull = false;

	public float senseDetectRange = 120f;
	List<SenseActuateLink> senseActuateLinks = new List<SenseActuateLink>();

	Quaternion transformLastRot = Quaternion.identity; // For in-editor rotation.
	Vector3 transformLastPos = Vector3.zero; // For in-editor rotation.


	public PhysNode(PhysAssembly physAssembly, Triplet localHexPos){
		all.Add(this);
		this.physAssembly = physAssembly ;
		this.localHexPos = localHexPos;
		Position = physAssembly.spawnPosition + (physAssembly.spawnRotation * HexUtilities.HexToWorld(localHexPos));
		Rotation = physAssembly.worldRotation;
		delayPosition = Position;

		cubeTransform = MonoBehaviour.Instantiate(PhysNodeController.Inst.physNodePrefab, Position, Quaternion.identity) as Transform;

		transformLastRot = cubeTransform.rotation;
		transformLastPos = cubeTransform.position;
	} // End of Awake().


	public void DoMath(){
		if(cull)
			return;

		//power = 1f;

		float wiggle = Mathf.Sin(waveformRunner * (2f * Mathf.PI) * (1f / wigglePhase)) * smoothedPower;
		waveformRunner += PhysNodeController.physicsStep * power;
		bool functioningMuscle = (neighbors.Count == 2) && ((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2));

		// Torque
		if(functioningMuscle){
			delayRotation *= Quaternion.AngleAxis(wiggleMaxAngVel * wiggle * PhysNodeController.physicsStep * power, rotationVector);
			delayRotation = Quaternion.RotateTowards(delayRotation, delayRotation * signalRotation, wiggleMaxAngVel * PhysNodeController.physicsStep * power);
		}

		Quaternion flailOffset = Quaternion.identity;
		// -- Comment out to remove 'flailing'
		if(functioningMuscle)
			flailOffset = Quaternion.Euler(rotationVector * wiggle * flailMaxDeflection);

		//Quaternion ΔFlailOffset = Quaternion.Inverse(flailOffset) * lastFlailOffset;
		//lastFlailOffset = flailOffset;

		// Node tests each neighbor's target position in relation to it.
		for(int i = 0; i < neighbors.Count; i++){
			PhysNeighbor curNeighbor = neighbors[i];
			PhysNode curNeighborNode = curNeighbor.physNode;
			if((curNeighbor == null) || !curNeighborNode){
				neighbors.Remove(curNeighbor);
				continue;
			}
			//Vector3 vecToNeighborTargetPos = curNeighborNode.Position - (Position + ((Rotation * curNeighbor.dir * flailOffset * signalRotation) * Vector3.forward * sizeMult)); 
			Vector3 vecToNeighborTargetPos = curNeighborNode.Position - (Position + ((Rotation * curNeighbor.dir * flailOffset) * Vector3.forward)); 

			float lerpStep = 0.48f;

			// All nodes try to align to their 'resting position' with their neighbors.
			curNeighborNode.delayPosition -= vecToNeighborTargetPos * lerpStep / neighbors.Count;
			curNeighborNode.delayRotation = Quaternion.Lerp(delayRotation, curNeighborNode.rotation, lerpStep);
			
			// Muscle propulsion
			if(functioningMuscle){
				Vector3 propulsion = (Rotation * curNeighbor.dir) * -Vector3.forward * (flailMaxDeflection / (1f + Mathf.Pow(wigglePhase, 2f))) * PhysNodeController.physicsStep * (1f - Mathf.Abs(wiggle)) * power;
				delayPosition += propulsion;
			}

			/*
			float updateLerpBias = 2f;
			if((neighbors.Count == 2) && (curNeighborNode.neighbors.Count != 2)){
				// Trace motor nodes' neighbors and relative rotations.
				Debug.DrawLine(Position, Position + (Rotation * curNeighbor.dir * Vector3.forward), Color.cyan);
				Debug.DrawLine(Position, Position + (Rotation * curNeighbor.dir * flailOffset * Vector3.forward), Color.blue);

				if(i == 0){
					delayRotation *= Quaternion.Inverse(Rotation) * Quaternion.Lerp(Rotation, curNeighborNode.Rotation * Quaternion.Inverse(flailOffset), updateLerpBias * power);
					curNeighborNode.delayRotation *= Quaternion.Inverse(curNeighborNode.Rotation) * Quaternion.Lerp(curNeighborNode.Rotation, Rotation * flailOffset, updateLerpBias * power);
				}
				if(i == 1){
					delayRotation *= Quaternion.Inverse(Rotation) * Quaternion.Lerp(Rotation, curNeighborNode.Rotation * flailOffset, updateLerpBias * power);
					curNeighborNode.delayRotation *= Quaternion.Inverse(curNeighborNode.Rotation) * Quaternion.Lerp(curNeighborNode.Rotation, Rotation * Quaternion.Inverse(flailOffset), updateLerpBias * power);
				}

				// Muscle propulsion
				Vector3 propulsion = (Rotation * curNeighbor.dir) * -Vector3.forward * (flailMaxDeflection / (1f + Mathf.Pow(wigglePhase, 2f))) * PhysNodeController.physicsStep * (1f - Mathf.Abs(wiggle)) * power;
				
				delayPosition += propulsion;
				Debug.DrawRay(Position, propulsion * 6f, Color.red);
			}
			// Inert nodes simply try to torque to match their neighbors.
			else if((curNeighborNode.neighbors.Count != 2) || ((neighbors.Count == 2) && (curNeighborNode.neighbors.Count == 2))){
				delayRotation = Quaternion.Lerp(delayRotation, curNeighborNode.Rotation, updateLerpBias);
				curNeighborNode.delayRotation = Quaternion.Lerp(curNeighborNode.Rotation, delayRotation, updateLerpBias);
			}

			GLDebug.DrawLine(position, curNeighborNode.position, new Color(1f, 1f, 1f, 0.1f), 0, false);
			//GLDebug.DrawLine(position, curNeighborNode.position, cubeTransform.renderer.material.color, 0, false);
			*/
		}
		
		// Update node type?
		if(neighbors.Count != lastNeighborCount){
			lastNeighborCount = neighbors.Count;

			switch(neighbors.Count){
			case 1 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.senseColor;
				Transform newViewConeTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.senseNodeBillboard, Position, Rotation) as Transform;
				viewCone = newViewConeTrans;
				break;
			case 2 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.actuateColor;
				if((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2)){
					Transform newTrailTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.motorNodeTrail, Position, Rotation) as Transform;
					newTrailTrans.parent = cubeTransform;
					trail = newTrailTrans.GetComponent<TimedTrailRenderer>();
				}
				break;
			case 3 : 
				cubeTransform.renderer.material.color = PrefabManager.Inst.controlColor;
				break;
			}
		}


		// Metabolism --------------------------------- //
		physAssembly.energy -= PhysNodeController.physicsStep * 0.05f;


		// Reset power
		smoothedPower = Mathf.MoveTowards(smoothedPower, power, PhysNodeController.physicsStep);
		power = 0.02f;
	} // End of DoMath().


	public void UpdateTransform(){
		if(cull)
			return;

		Vector3 thisFrameVelocity = delayPosition - Position;
		velocity += thisFrameVelocity * 0.1f;
		velocity *= 0.98f;

		// In-editor control
		rotation *= Quaternion.Inverse(Rotation) * cubeTransform.rotation;
		position += cubeTransform.position - Position;

		Position = delayPosition + velocity;
		Rotation = delayRotation;

		cubeTransform.position = Position;
		cubeTransform.rotation = Rotation;

		foreach(PhysNeighbor someNeighbor in neighbors)
			if(Random.Range(0f, 1f) < 0.2f)
				someNeighbor.arrowDist = Random.Range(0.25f, 0.4f);


		// Position error bandaid...
		if(Position.x > 1000f || Position.y > 1000f || Position.z > 1000f)
			physAssembly.Destroy();

		// Type-specific behaviours
		switch(neighbors.Count){
			case 1 : 
				float viewConeSize = 2.5f;
				Debug.DrawRay(Position, (Rotation * actionRotation * Vector3.forward) * 2f, Color.green);

				viewCone.position = Position + (actionRotation * (Rotation * Vector3.forward)) * viewConeSize;
				viewCone.localScale = Vector3.one * viewConeSize;

				// Billboard the arc with the main camera.
				viewCone.rotation = Rotation * actionRotation;
				viewCone.position = Position + (viewCone.rotation * (Vector3.forward * viewConeSize * 0.5f));
				viewCone.rotation *= Quaternion.AngleAxis(-90, Vector3.up);

				Vector3 camRelativePos = viewCone.InverseTransformPoint(Camera.main.transform.position);
				float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;
				viewCone.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);

				//calling detect food on sense node, determines power of node
				Bounds boundary = new Bounds(position, senseDetectRange * (new Vector3(1, 1, 1)));
				PhysFood.AllFoodTree.RunActionInRange(new System.Action<PhysFood>(HandleDetectedFood), boundary);

				// Transmit signal!
				foreach(SenseActuateLink someLink in senseActuateLinks){
					someLink.targetActuator.power += power * someLink.signalStrength;
					someLink.targetActuator.signalRotation = signalRotation;
				}

				break;
			case 2 : 
				break;
			case 3 : 
				break;
		}
	} // End of UpdateTransform().


	public void AttachNeighbor(PhysNode _newNeighbor){
		// Early out if we already have this neighbor.
		for(int i = 0; i < neighbors.Count; i++)
			if(neighbors[i].physNode == _newNeighbor)
				return;

		PhysNeighbor newNeighbor = new PhysNeighbor();
		newNeighbor.physNode = _newNeighbor;
		newNeighbor.dir = Quaternion.LookRotation(_newNeighbor.Position - Position, Random.onUnitSphere);
		neighbors.Add(newNeighbor);
		newNeighbor.physNode.AttachNeighbor(this);
	} // End of AttachNeighbor().


	public void Destroy(){
		if(cubeTransform)
			MonoBehaviour.Destroy(cubeTransform.gameObject);

		if(viewCone)
			MonoBehaviour.Destroy(viewCone.gameObject);

		if(trail){
			trail.transform.parent = null;
		}

		cull = true;
	} // End of OnDestroy().




	// When a sense node calls this function, it will rebuild its energy transfer network.
	public void ComputeEnergyNetwork(){
		senseActuateLinks = ComputeCircuitry(new HashSet<PhysNode>(new PhysNode[]{this}), 1f);
	} // End of ComputeEnergyNetwork().


	// Returns a collection of actuator nodes linked to this sense node.
	public List<SenseActuateLink> ComputeCircuitry(HashSet<PhysNode> checkedNodes, float signalStrength){
		List<SenseActuateLink> linksToReturn = new List<SenseActuateLink>();

		checkedNodes.Add(this);
		if(signalStrength < 0.02f)
			return linksToReturn;

		if(neighbors.Count == 2)
			linksToReturn.Add(new SenseActuateLink(this, signalStrength));

		for(int i = 0; i < neighbors.Count; i++){
			PhysNode curNeighbor = neighbors[i].physNode;
			if(!checkedNodes.Contains(curNeighbor) && (curNeighbor.neighbors.Count > 1)){
				float sig = signalStrength / Mathf.Max(1f, (neighbors.Count - 1));

				linksToReturn.AddRange(curNeighbor.ComputeCircuitry(new HashSet<PhysNode>(checkedNodes), sig * 0.95f));

				// Trace effect
				//if(Input.GetKey(KeyCode.T))
					//GLDebug.DrawLineArrow(Position, Vector3.Lerp(Position, curNeighbor.Position, neighbors[i].arrowDist), 0.1f, 20f, new Color(1f, 1f, 0f, signalStrength), 0f, false);
			}
		}
		return linksToReturn;

	} // End of Transmit().


    private void HandleDetectedFood(PhysFood food){
		Vector3 vectorToFood = food.worldPosition - position;
		float distanceToFood = vectorToFood.magnitude;

		float angleToFood = Vector3.Angle(rotation * actionRotation * Vector3.forward, vectorToFood);
		float strength = 1f - (distanceToFood / senseDetectRange);

		if(angleToFood < 45f){
			power = 1f;
			signalRotation = Quaternion.Inverse(rotation) * Quaternion.LookRotation(vectorToFood, rotation * Vector3.up);
			GLDebug.DrawLine(position, food.worldPosition, new Color(0.4f, 1f, 0.4f, Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));

			float foodToPull = PhysNodeController.physicsStep * 1f;

			food.energy -= foodToPull;
			physAssembly.energy += foodToPull;
		}
		//else
			//GLDebug.DrawLine(position, food.worldPosition, new Color(1f, 1f, 1f, 0.25f * Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));

	} // End of HandleDetectedFood().


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


// Each sense node keeps track of where to send its signal based on these.
public class SenseActuateLink {

	public PhysNode targetActuator = null;
	public float signalStrength = 0f;

	public SenseActuateLink(PhysNode linkedNode, float signalStrength){
		targetActuator = linkedNode;
		this.signalStrength = signalStrength;
	} // End of constructor.

} // End of SenseActuateLink.
