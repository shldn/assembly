using UnityEngine;
using System.Collections.Generic;


public class Node {

    static List<Node> all = new List<Node>();
    public static List<Node> getAll { get { return all; } }
    public static implicit operator bool(Node exists) { return exists != null; }

    public Triplet localHexPos = Triplet.zero;
    public List<PhysNeighbor> neighbors = new List<PhysNeighbor>();
    int lastNeighborCount = 0;
    private System.Action<FoodPellet> handleFoodDelegate;

    Assembly physAssembly = null;
    public Assembly PhysAssembly { get { return physAssembly; } set { physAssembly = value; } }
    public bool IsSense { get { return neighbors.Count == 1; } }
    public bool IsMuscle { get { return neighbors.Count == 2; } }

    public static readonly float[] Default_Node_Properties = {90.0f, 35.0f,1.0f, 5f, 100f, 45f};
    public const int Num_Node_Properties = 6;

    private NodeProperties nodeProperties = NodeProperties.random;
    public NodeProperties Properties { 
        get { return nodeProperties; }
        set
        {
            nodeProperties = value;
            if (viewer != null)
                viewer.Properties = value;
        }
    }
    public bool Visible
    {
        get {
            return (viewer != null) ? viewer.Visible : false;
        }
        set {
            if(viewer != null)
                viewer.Visible = value;
        }
    }

	// The offset from actionRotation determined by incoming signal.
	// For sense nodes, this is the rotation from the sense node to the most powerful food node source.
	// For actuators, this rotation modifies the muscle output.
	public Quaternion signalRotation = Quaternion.identity;

    // Viewer
    public NodeViewer viewer = null;

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

	float velocityCoefficient = 0.1f; // How much of motion is converted to velocity.

	public Vector3 velocity = Vector3.zero;

	Vector3 position  = Vector3.zero;
	public Vector3 Position {
		get{
			return position;
		}set{
			// Neutralize velocity.
			//Vector3 flux = position - value;
			//velocity += flux * velocityCoefficient;

			position = value;
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

	// This movement will be applied after velocity calculations.
	public Vector3 staticMove = Vector3.zero;

	// If true, node will be destroyed.
	public bool cull = false;

	List<SenseActuateLink> senseActuateLinks = new List<SenseActuateLink>();

	public float senseAttractRange = 10f;
    public Vector3 SenseForward { get { return Rotation * (nodeProperties.senseVector * Vector3.forward); } }
    

	private static Octree<Node> allSenseNodeTree;
    public static Octree<Node> AllSenseNodeTree{ 
        get{
            if(allSenseNodeTree == null){
                allSenseNodeTree = new Octree<Node>(new Bounds(Vector3.zero, 2.0f * NodeController.Inst.maxWorldSize * Vector3.one), (Node x) => x.position, 5);
			}
            return allSenseNodeTree;
        }
        set{
            allSenseNodeTree = value;
        }
    }

    Color nodeColor = Color.white;


	public Node(Assembly physAssembly, Triplet localHexPos){
		all.Add(this);
		this.physAssembly = physAssembly;
		this.localHexPos = localHexPos;
		Position = physAssembly.spawnPosition + (physAssembly.spawnRotation * HexUtilities.HexToWorld(localHexPos));
        Rotation = physAssembly.spawnRotation;
		delayPosition = Position;
        if (PersistentGameManager.EmbedViewer)
            viewer = new NodeViewer(Position, nodeProperties, physAssembly.properties);

        handleFoodDelegate = new System.Action<FoodPellet>(HandleDetectedFood);
} // End of Awake().


    public Node(Triplet localHexPos, NodeProperties props){
		all.Add(this);
		this.localHexPos = localHexPos;
		this.nodeProperties = props;

        // This constructor appears to create a temporary node, so likely doesn't need a viewer.
        // Will keep in case usage changes, could change code path to pass in Assembly pointer.
        if (PersistentGameManager.EmbedViewer)
            viewer = new NodeViewer(Position, props, null);
	} // End of Awake().

    public void UpdateSenseVector(Quaternion newSenseVector)
    {
        nodeProperties.senseVector = newSenseVector;
        if (viewer != null)
            viewer.Properties = nodeProperties;
    }

    public void UpdateActuateVector(Quaternion newActuateVector)
    {
        nodeProperties.actuateVector = newActuateVector;
        if (viewer != null)
            viewer.Properties = nodeProperties;
    }

	public void DoMath(){
		if(cull || !physAssembly)
			return;
		
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

			//GLDebug.DrawLine(position, curNeighborNode.position, Color.Lerp(cubeTransform.renderer.material.color, curNeighborNode.cubeTransform.renderer.material.color, 0.5f), 0, false);
		}
		
		// Update node type?
		if(neighbors.Count != lastNeighborCount){

            if (lastNeighborCount == 1)
				AllSenseNodeTree.Remove(this);
            lastNeighborCount = neighbors.Count;

            bool createTrail = neighbors.Count == 2 && ((neighbors[0].physNode.neighbors.Count != 2) || (neighbors[1].physNode.neighbors.Count != 2));

			// Don't need trails on amalgam level... the amalgams are heavy enough to render already.
			if(Application.loadedLevelName == "Soup_Amalgams")
				createTrail = false;

			//createTrail = false;
            if (viewer != null)
                viewer.SetNeighborCount(neighbors.Count, createTrail);
            if (neighbors.Count == 1)
                AllSenseNodeTree.Insert(this);
		}


		// Metabolism --------------------------------- //
		if(((CameraControl.Inst == null) || (PhysAssembly != CameraControl.Inst.selectedCaptureObj)) && neighbors.Count < 4)
			physAssembly.energy -= NodeController.physicsStep * 0.01f;


		// Reel in to amalgam
		Vector3 worldSize = Vector3.one * 100f;
		if(Application.loadedLevelName.Equals("Soup_Assemblies") && (Mathf.Sqrt(Mathf.Pow(Position.x / worldSize.x, 2f) + Mathf.Pow(Position.y / worldSize.y, 2f) + Mathf.Pow(Position.z / worldSize.z, 2f)) > 1f)){
			delayPosition = Vector3.MoveTowards(delayPosition, Vector3.zero, NodeController.physicsStep * 1f);
		}
		

		// Reset power
		smoothedPower = Mathf.MoveTowards(smoothedPower, power, NodeController.physicsStep * 3f);

        // Sense nodes at full power if we're in the client, otherwise residual.
        if (neighbors.Count == 1) {
            power = ClientTest.Inst ? ClientTest.Inst.NodePower : (PersistentGameManager.IsClient ? 1f : 0.2f);
        }
        // "Residual" power
        else if (neighbors.Count == 2)
            power = 0f;
		
	} // End of DoMath().


	public void UpdateTransform(){
		if(cull || !physAssembly)
			return;

		if(viewer != null)
			viewer.cubeTransform.GetComponent<Rigidbody>().isKinematic = !viewer.cubeTransform.GetComponent<GrabbableObject>().IsGrabbed();

		if(viewer.cubeTransform.GetComponent<GrabbableObject>().IsGrabbed()) {
			delayPosition = viewer.cubeTransform.position;
			delayRotation = viewer.cubeTransform.rotation;
			position = delayPosition;
			rotation = delayRotation;
			velocity = viewer.cubeTransform.GetComponent<Rigidbody>().velocity * 0.25f;
			return;
		}

		Vector3 thisFrameVelocity = delayPosition - Position;
		velocity += thisFrameVelocity * velocityCoefficient;
		velocity *= 0.98f;

		delayPosition += velocity;
		delayPosition += staticMove;
		staticMove = Vector3.zero;
		Position = delayPosition;
		Rotation = delayRotation;

        // Moved this to the Assembly class.
		//if(Environment.Inst && Environment.Inst.isActiveAndEnabled){
		//	if((Mathf.Abs(delayPosition.x) > NodeController.Inst.worldSphereScale.x) || (Mathf.Abs(delayPosition.y) > NodeController.Inst.worldSphereScale.y) || (Mathf.Abs(delayPosition.z) > NodeController.Inst.worldSphereScale.z))
		//		velocity += -delayPosition.normalized * NodeController.physicsStep;
		//}

        if (viewer != null)
            viewer.UpdateTransform(Position,Rotation);

		foreach(PhysNeighbor someNeighbor in neighbors)
			if(Random.Range(0f, 1f) < 0.2f)
				someNeighbor.arrowDist = Random.Range(0.25f, 0.4f);


		// Type-specific behaviours
		switch(neighbors.Count){
			case 1 : 
				//calling detect food on sense node, determines power of node
                Bounds foodDetectBoundary = new Bounds(position, nodeProperties.senseRange * (Vector3.one));
				FoodPellet.AllFoodTree.RunActionInRange(handleFoodDelegate, foodDetectBoundary);

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

        if (viewer != null)
            viewer.Update();

		//GLDebug.DrawCube(Position, rotation, Vector3.one * velocity.magnitude * 10f);
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

    public void Destroy(bool immediate = false){
        if (viewer != null) {
			viewer.Destroy(immediate);
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

		Vector3 vectorToFood = food.WorldPosition - position;
		float distanceToFood = vectorToFood.magnitude;
		if(distanceToFood > nodeProperties.senseRange || (food.owner != null && food.owner != physAssembly ) || (CognoAmalgam.Inst != null && !food.Activated))
			return;

		float angleToFood = Vector3.Angle(rotation * nodeProperties.senseVector * Vector3.forward, vectorToFood);
        float strength = 1f - (distanceToFood / nodeProperties.senseRange);

		if(angleToFood < 0.5f * nodeProperties.fieldOfView){
			power = 1f;

			signalRotation = Quaternion.Inverse(rotation) * Quaternion.LookRotation(vectorToFood, rotation * Vector3.up);
			//GLDebug.DrawLine(position, food.worldPosition, new Color(0.4f, 1f, 0.4f, Mathf.Pow(1f - (distanceToFood / senseDetectRange), 2f)));

			float foodToPull = NodeController.physicsStep * 0.3f;

            food.Energy -= foodToPull;
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
    public float[] getNodeProperties()
    {
        float[] props = new float[] {0f,0f,0f,0f,0f,0f};
        switch (neighbors.Count)
        {
            case 1:
                props[0] = nodeProperties.fieldOfView;
                props[1] = nodeProperties.senseRange;
                break;
            case 2:
                props[2] = nodeProperties.muscleStrength;
                props[3] = nodeProperties.oscillateFrequency;
                props[4] = nodeProperties.torqueStrength;
                props[5] = nodeProperties.flailMaxAngle;
                break;
            case 3:
                break;
            default:
                break;
        }
        
        return props;
    }// End of GetNodeProperties

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
    public Quaternion senseVector;  // viewer needs
    public float fieldOfView;       // viewer needs
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
            return new NodeProperties(Random.rotation, 90f, Random.Range(50.0f, 80.0f), Random.rotation, Random.Range(0.1f, 1f), Random.onUnitSphere, Random.Range(1f, 10f), Random.Range(10f, 200f), Random.Range(10f, 80f));
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
        fieldOfView = Node.Default_Node_Properties[0];
        senseRange = Node.Default_Node_Properties[1];
        muscleStrength = Node.Default_Node_Properties[2];
        actuateVector = Quaternion.identity;

		torqueAxis = Vector3.forward;
		oscillateFrequency = Node.Default_Node_Properties[3];
		torqueStrength = Node.Default_Node_Properties[4];
		flailMaxAngle = Node.Default_Node_Properties[5];

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