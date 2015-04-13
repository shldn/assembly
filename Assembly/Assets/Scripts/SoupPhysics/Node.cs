﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Node {

	static HashSet<Node> all = new HashSet<Node>();
	public static HashSet<Node> getAll {get{return all;}}
	public static implicit operator bool(Node exists){return exists != null;}

	public Triplet localHexPos = Triplet.zero;
	public List<PhysNeighbor> neighbors = new List<PhysNeighbor>();
	int lastNeighborCount = 0;

	Assembly physAssembly = null;
	public Assembly PhysAssembly {get{return physAssembly;} set{physAssembly = value;}}
    public bool IsSense{ get{ return neighbors.Count == 1; } }

    public NodeProperties nodeProperties = NodeProperties.random;

	// The offset from actionRotation determined by incoming signal.
	// For sense nodes, this is the rotation from the sense node to the most powerful food node source.
	// For actuators, this rotation modifies the muscle output.
	public Quaternion signalRotation = Quaternion.identity;

	// Type-specific elements, effects
	public Transform cubeTransform = null;
	TimedTrailRenderer trail = null;
	Transform viewCone = null;
    public float viewConeSize = 2.5f;
    public Transform ViewCone { get { return viewCone;  } }

	[System.Serializable]
	public class PhysNeighbor {
		public Node physNode = null;
		public Quaternion dir = Quaternion.identity;
		public float arrowDist = 1f;
	} // End of PhysNeighbor.

	Quaternion lastFlailOffset = Quaternion.identity; // For determining connection rotations.

	// These store position/rotation to be updated after neighbor math is done.
	public Vector3 delayPosition = Vector3.zero;
	public Quaternion delayRotation = Quaternion.identity;

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

	List<SenseActuateLink> senseActuateLinks = new List<SenseActuateLink>();

	public float senseAttractRange = 30f;

	Quaternion transformLastRot = Quaternion.identity; // For in-editor rotation.
	Vector3 transformLastPos = Vector3.zero; // For in-editor rotation.

	private static Octree<Node> allSenseNodeTree;
    public static Octree<Node> AllSenseNodeTree{ 
        get{
            if(allSenseNodeTree == null){
                allSenseNodeTree = new Octree<Node>(new Bounds(Vector3.zero, 2.0f * NodeController.Inst.WorldSize * Vector3.one), (Node x) => x.position, 5);
			}
            return allSenseNodeTree;
        }
        set{
            allSenseNodeTree = value;
        }
    }

	Color nodeColor = Color.white;
	float mateColorLerp = 0f;
	float genderColorLerp = 0f;


	public Node(Assembly physAssembly, Triplet localHexPos){
		all.Add(this);
		this.physAssembly = physAssembly ;
		this.localHexPos = localHexPos;
		Position = physAssembly.spawnPosition + (physAssembly.spawnRotation * HexUtilities.HexToWorld(localHexPos));
        Rotation = physAssembly.spawnRotation;
		delayPosition = Position;

		cubeTransform = MonoBehaviour.Instantiate(NodeController.Inst.physNodePrefab, Position, Quaternion.identity) as Transform;

		transformLastRot = cubeTransform.rotation;
		transformLastPos = cubeTransform.position;
	} // End of Awake().


	public Node(Triplet localHexPos, NodeProperties props){
		all.Add(this);
		this.localHexPos = localHexPos;
		this.nodeProperties = props;

		cubeTransform = MonoBehaviour.Instantiate(NodeController.Inst.physNodePrefab, Position, Quaternion.identity) as Transform;

		transformLastRot = cubeTransform.rotation;
		transformLastPos = cubeTransform.position;
	} // End of Awake().


	public void DoMath(){
		if(cull || !physAssembly)
			return;

		//power = 1f;

		float wiggle = Mathf.Sin(waveformRunner * (2f * Mathf.PI) * (1f / nodeProperties.oscillateFrequency)) * smoothedPower;
		waveformRunner += NodeController.physicsStep * power;
		bool functioningMuscle = (neighbors.Count == 2) && ((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2));

		// Torque
		if(functioningMuscle){
			delayRotation *= Quaternion.AngleAxis(nodeProperties.torqueStrength * wiggle * NodeController.physicsStep * power, nodeProperties.torqueAxis);
			delayRotation = Quaternion.RotateTowards(delayRotation, delayRotation * signalRotation, nodeProperties.torqueStrength * NodeController.physicsStep * power);
		}

		Quaternion flailOffset = Quaternion.identity;
		// -- Comment out to remove 'flailing'
		if(functioningMuscle)
			flailOffset = Quaternion.Euler(nodeProperties.torqueAxis * wiggle * nodeProperties.flailMaxAngle);

		//Quaternion ΔFlailOffset = Quaternion.Inverse(flailOffset) * lastFlailOffset;
		//lastFlailOffset = flailOffset;

		// Node tests each neighbor's target position in relation to it.
		for(int i = 0; i < neighbors.Count; i++){
			PhysNeighbor curNeighbor = neighbors[i];
			Node curNeighborNode = curNeighbor.physNode;
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
				Vector3 propulsion = (Rotation * curNeighbor.dir) * -Vector3.forward * (nodeProperties.flailMaxAngle / (1f + Mathf.Pow(nodeProperties.oscillateFrequency, 2f))) * NodeController.physicsStep * (1f - Mathf.Abs(wiggle)) * power;
				delayPosition += propulsion;
			}

			GLDebug.DrawLine(position, curNeighborNode.position, Color.Lerp(cubeTransform.renderer.material.color, curNeighborNode.cubeTransform.renderer.material.color, 0.5f), 0, false);
		}
		
		// Update node type?
		if(neighbors.Count != lastNeighborCount){
			lastNeighborCount = neighbors.Count;

			if(lastNeighborCount == 2)
				AllSenseNodeTree.Remove(this);

			if(viewCone)
				GameObject.Destroy(viewCone.gameObject);

			if(trail)
            {
                GameObject.Destroy(trail.gameObject);
                GameObject.Destroy(trail);
            }


			switch(neighbors.Count){
			// Sense node.
			case 1 : 
				nodeColor = PrefabManager.Inst.senseColor;
				Transform newViewConeTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.senseNodeBillboard, Position, Rotation) as Transform;
				viewCone = newViewConeTrans;
                AllSenseNodeTree.Insert(this);
				break;
			// Muscle node.
			case 2 : 
				nodeColor = PrefabManager.Inst.actuateColor;
				if((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2)){
					Transform newTrailTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.motorNodeTrail, Position, Rotation) as Transform;
					newTrailTrans.parent = cubeTransform;
					trail = newTrailTrans.GetComponent<TimedTrailRenderer>();

					if(PersistentGameManager.IsClient)
						trail.lifeTime *= 0.3f;
				}
				break;
			// Control node.
			case 3 : 
				nodeColor = PrefabManager.Inst.controlColor;
				break;
			default :
				nodeColor = PrefabManager.Inst.stemColor;
				break;
			}
		}


		// Metabolism --------------------------------- //
		if(PhysAssembly != CameraControl.Inst.selectedPhysAssembly)
			physAssembly.energy -= NodeController.physicsStep * 0.05f;


		mateColorLerp = Mathf.MoveTowards(mateColorLerp, physAssembly.wantToMate? 1f : 0f, Time.deltaTime);
		genderColorLerp = Mathf.MoveTowards(genderColorLerp, physAssembly.gender? 1f : 0f, Time.deltaTime);

		Color genderColor = Color.Lerp(new Color(1f, 0f, 1f), new Color(1f, 0.5f, 0f), genderColorLerp);

		if(mateColorLerp > 0f)
			cubeTransform.renderer.material.color = Color.Lerp(nodeColor, genderColor, mateColorLerp * 0.7f);
		else
			cubeTransform.renderer.material.color = nodeColor;



		// Reset power
		smoothedPower = Mathf.MoveTowards(smoothedPower, power, NodeController.physicsStep);
		power = PersistentGameManager.IsClient? (ClientTest.Inst ? ClientTest.Inst.NodePower : 1f) : 0.2f;
	} // End of DoMath().


	public void UpdateTransform(){
		if(cull || !physAssembly)
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

		// Type-specific behaviours
		switch(neighbors.Count){
			case 1 : 
				Debug.DrawRay(Position, (Rotation * nodeProperties.senseVector * Vector3.forward) * 2f, Color.green);

				viewCone.position = Position + (nodeProperties.senseVector * (Rotation * Vector3.forward)) * viewConeSize;
				viewCone.localScale = Vector3.one * viewConeSize;

				// Billboard the arc with the main camera.
				viewCone.rotation = Rotation * nodeProperties.senseVector;
				viewCone.position = Position + (viewCone.rotation * (Vector3.forward * viewConeSize * 0.5f));
				viewCone.rotation *= Quaternion.AngleAxis(-90, Vector3.up);

				Vector3 camRelativePos = viewCone.InverseTransformPoint(Camera.main.transform.position);
				float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;
				viewCone.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);

				//calling detect food on sense node, determines power of node
                Bounds foodDetectBoundary = new Bounds(position, nodeProperties.senseRange * (new Vector3(1, 1, 1)));
				FoodPellet.AllFoodTree.RunActionInRange(new System.Action<FoodPellet>(HandleDetectedFood), foodDetectBoundary);

				// Amalgamation attraction
				//Bounds attractBoundary = new Bounds(position, senseAttractRange * (new Vector3(1, 1, 1)));
				//PhysNode.AllSenseNodeTree.RunActionInRange(new System.Action<PhysNode>(HandleDetectedSenseNode), attractBoundary);

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


	public void AttachNeighbor(Node _newNeighbor){
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

		if(trail)
        {
            trail.transform.parent = null;
            MonoBehaviour.Destroy(trail.gameObject);
            MonoBehaviour.Destroy(trail);
            trail = null;
        }

		cull = true;
	} // End of OnDestroy().


	// When a sense node calls this function, it will rebuild its energy transfer network.
	public void ComputeEnergyNetwork(){
		senseActuateLinks = ComputeCircuitry(new HashSet<Node>(new Node[]{this}), 1f);
	} // End of ComputeEnergyNetwork().


	// Returns a collection of actuator nodes linked to this sense node.
	public List<SenseActuateLink> ComputeCircuitry(HashSet<Node> checkedNodes, float signalStrength){
		List<SenseActuateLink> linksToReturn = new List<SenseActuateLink>();

		checkedNodes.Add(this);
		if(signalStrength < 0.02f)
			return linksToReturn;

		if(neighbors.Count == 2)
			linksToReturn.Add(new SenseActuateLink(this, signalStrength));

		for(int i = 0; i < neighbors.Count; i++){
			Node curNeighbor = neighbors[i].physNode;
			if(!checkedNodes.Contains(curNeighbor) && (curNeighbor.neighbors.Count > 1)){
				float sig = signalStrength / Mathf.Max(1f, (neighbors.Count - 1));

				linksToReturn.AddRange(curNeighbor.ComputeCircuitry(new HashSet<Node>(checkedNodes), sig * 0.95f));

				// Trace effect
				//if(Input.GetKey(KeyCode.T))
					//GLDebug.DrawLineArrow(Position, Vector3.Lerp(Position, curNeighbor.Position, neighbors[i].arrowDist), 0.1f, 20f, new Color(1f, 1f, 0f, signalStrength), 0f, false);
			}
		}
		return linksToReturn;

	} // End of Transmit().


	// Zeroes out all velocities.
	public void ZeroVelocity(){
		velocity = Vector3.zero;
	} // End of ZeroMovement().


    private void HandleDetectedFood(FoodPellet food){
		Vector3 vectorToFood = food.worldPosition - position;
		float distanceToFood = vectorToFood.magnitude;
		if(distanceToFood > nodeProperties.senseRange)
			return;

		float angleToFood = Vector3.Angle(rotation * nodeProperties.senseVector * Vector3.forward, vectorToFood);
        float strength = 1f - (distanceToFood / nodeProperties.senseRange);

		if(angleToFood < 45f){
			power = 1f;
			signalRotation = Quaternion.Inverse(rotation) * Quaternion.LookRotation(vectorToFood, rotation * Vector3.up);
			//GLDebug.DrawLine(position, food.worldPosition, new Color(0.4f, 1f, 0.4f, Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));

			float foodToPull = NodeController.physicsStep * 0.1f;

			food.energy -= foodToPull;
			physAssembly.energy += foodToPull;
		}
		//else
			//GLDebug.DrawLine(position, food.worldPosition, new Color(1f, 1f, 1f, 0.25f * Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));
	} // End of HandleDetectedFood().

	private void HandleDetectedSenseNode(Node otherSenseNode){
		// We don't care about nodes under our own assembly (this includes us!)
		if(otherSenseNode.physAssembly == physAssembly || ((physAssembly.NodeDict.Values.Count + otherSenseNode.physAssembly.NodeDict.Values.Count) > NodeController.assemStage1Size))
			return;

		Vector3 vectorToNode = otherSenseNode.position - position;
		float distanceToNode = vectorToNode.magnitude;
		if(distanceToNode > senseAttractRange)
			return;

		// Merge these assemblies if they are close enough.
		if(distanceToNode < 1f){
			physAssembly.AmaglamateTo(otherSenseNode.PhysAssembly, otherSenseNode.localHexPos);
			return;
		}

		float angleToNode = Vector3.Angle(rotation * nodeProperties.senseVector * Vector3.forward, vectorToNode);
		float strength = 1f - (distanceToNode / senseAttractRange);

		if(true){
			delayPosition += vectorToNode * strength * 0.01f;
		}
		//else
			//GLDebug.DrawLine(position, food.worldPosition, new Color(1f, 1f, 1f, 0.25f * Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));
	} // End of HandleDetectedFood().


	// Save/load -------------------------------------------------------------------------||
    // The string representation of this class for file saving (could use ToString, but want to be explicit)
    public string ToFileString(int format){
        return localHexPos.ToString() + nodeProperties.ToString();
    } // End of ToFileString().

    public static Node FromString(string str, int format=1){
        int splitIdx = str.IndexOf(')');
        Triplet pos = IOHelper.TripletFromString(str.Substring(0,splitIdx+1));
        NodeProperties props = new NodeProperties(str.Substring(splitIdx + 1));
        return new Node(pos, props);
    } // End of FromString().

	public void Mutate(float amount){
		nodeProperties.Mutate(amount);
	} // End of Mutate().


    public static void DestroyAll()
    {
        all.Clear();
        allSenseNodeTree = null;
    }

} // End of PhysNode.


// Each sense node keeps track of where to send its signal based on these.
public class SenseActuateLink {

	public Node targetActuator = null;
	public float signalStrength = 0f;

	public SenseActuateLink(Node linkedNode, float signalStrength){
		targetActuator = linkedNode;
		this.signalStrength = signalStrength;
	} // End of constructor.

} // End of SenseActuateLink.


public struct NodeProperties {

    // Sense
    public Quaternion senseVector;
    public float fieldOfView;
    public float senseRange;
    public float muscleStrength;

	public Vector3 torqueAxis;
	public float oscillateFrequency;
	public float torqueStrength;
	public float flailMaxAngle;

    // Actuate
    public Quaternion actuateVector;

    // A fully randomly-seeded NodeProperties.
    public static NodeProperties random{
        get{
            return new NodeProperties(Random.rotation, 45f, Random.Range(60.0f, 180.0f), Random.rotation, Random.Range(0.1f, 1f), Random.onUnitSphere, Random.Range(1f, 10f), Random.Range(10f, 200f), Random.Range(10f, 80f));
        }
    } // End of NodeProperties.random.


    // Constructor
    public NodeProperties(Quaternion senseVector, float fieldOfView, float senseRange, Quaternion actuateVector, float muscleStrength, Vector3 torqueAxis, float wigglePhase, float torqueStrength, float flailMaxAngle){
        this.senseVector = senseVector;
        this.senseRange = senseRange;
        this.fieldOfView = fieldOfView;
        this.actuateVector = actuateVector;
        this.muscleStrength = muscleStrength;

		this.torqueAxis = torqueAxis;
		this.oscillateFrequency= wigglePhase;
		this.torqueStrength = torqueStrength;
		this.flailMaxAngle = flailMaxAngle;
    } // End of NodeProperties constructor.


    public NodeProperties(string str){

        senseVector = Quaternion.identity;
        fieldOfView = 45.0f;
        senseRange = 120.0f;
        muscleStrength = 1.0f;
        actuateVector = Quaternion.identity;

		torqueAxis = Vector3.forward;
		oscillateFrequency = 5f;
		torqueStrength = 100f;
		flailMaxAngle = 45f;

        string[] tok = str.Split(';');
        for(int i=0; i < tok.Length; ++i){
            string[] pair = tok[i].Split(':');
            switch(pair[0]){
                case "sv":
                    senseVector = IOHelper.QuaternionFromString(pair[1]);
                    break;
                case "sr":
                    if (!float.TryParse(pair[1], out senseRange))
                        Debug.LogError("sr failed to parse");
                    break;
                case "av":
                    actuateVector = IOHelper.QuaternionFromString(pair[1]);
                    break;
                case "fov":
                    if(!float.TryParse(pair[1], out fieldOfView))
                        Debug.LogError("fov failed to parse");
                    break;
                case "m":
                    if (!float.TryParse(pair[1], out muscleStrength))
                        Debug.LogError("muscleStrength failed to parse");
                    break;

				case "ta":
                    torqueAxis = IOHelper.Vector3FromString(pair[1]);
                    break;
				case "of":
                    if (!float.TryParse(pair[1], out oscillateFrequency))
                        Debug.LogError("oscillateFrequency failed to parse");
                    break;
				case "ts":
                    if (!float.TryParse(pair[1], out torqueStrength))
                        Debug.LogError("torqueStrength failed to parse");
                    break;
				case "fma":
                    if (!float.TryParse(pair[1], out flailMaxAngle))
                        Debug.LogError("flailMaxAngle failed to parse");
                    break;
                default:
                    Debug.LogError("Unknown property: " + pair[0]);
                    break;
            }
        }
    } // End of NodeProperties constructor.


	public void Mutate(float amount){
		senseVector *= Quaternion.AngleAxis(Random.Range(0f, 180f * amount), Random.onUnitSphere);

        senseRange = (1.0f + Random.Range(-amount, amount)) * senseRange;
        senseRange = Mathf.Clamp(senseRange, 1f, 1000f);

		fieldOfView += Random.Range(-180f, 180f) * amount;
		fieldOfView = Mathf.Clamp(fieldOfView, 1f, 180f);

		muscleStrength += Random.Range(-1f, 1f) * amount;
		muscleStrength = Mathf.Clamp01(muscleStrength);

		actuateVector *= Quaternion.AngleAxis(Random.Range(0f, 180f * amount), Random.onUnitSphere);

		torqueAxis = Quaternion.AngleAxis(Random.Range(0f, 180f * amount), Random.onUnitSphere) * torqueAxis;

		oscillateFrequency += Random.Range(-5f, 5f) * amount;
		oscillateFrequency = Mathf.Clamp(oscillateFrequency, 1f, 10f);

		torqueStrength += Random.Range(-100f, 100f) * amount;
		torqueStrength = Mathf.Clamp(torqueStrength, 1f, 200f);

		flailMaxAngle += Random.Range(-45f, 45f) * amount;
		flailMaxAngle = Mathf.Clamp(flailMaxAngle, 0f, 90f);

	} // End of Mutate().


    public override string ToString(){
        return  "sv" + ":" + senseVector.ToString() + ";" +
                "av" + ":" + actuateVector.ToString() + ";" +
                "sr" + ":" + senseRange.ToString() + ";" +
                "fov" + ":" + fieldOfView.ToString() + ";" +
                "m" + ":" + muscleStrength.ToString() + ";" +

				"ta" + ":" + torqueAxis.ToString() + ";" +
				"of" + ":" + oscillateFrequency.ToString() + ";" +
				"ts" + ":" + torqueStrength.ToString() + ";" +
				"fma" + ":" + flailMaxAngle.ToString();
    } // End of ToString().

} // End of NodeProperties.