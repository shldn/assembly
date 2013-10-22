using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Vectrosity;

public class Bond{

    public Node nodeA;
    public Node nodeB;
    public VectorLine muscleLine;
    public VectorLine signalLine;
    public VectorLine synapseLine;
    public VectorLine motionLine;
    public Vector3[] muscleEndPoints = new Vector3[] { Vector3.zero, Vector3.zero };
    public Vector3[] signalEndPoints = new Vector3[] { Vector3.zero, Vector3.zero };
    public Vector3[] synapseEndPoints = new Vector3[] { Vector3.zero, Vector3.zero };
    public Vector3[] motionEndPoints = new Vector3[] { Vector3.zero, Vector3.zero };


    bool calcified = false;

    // The target distance of the bond.
    float bondDist = 1f;
    // Amount of 'give' in the bond.
    float bondGive;


    public Material muscleLineMaterial;
    public Material signalLineMaterial;
    public Material synapseLineMaterial;

    public Material motionLineMaterial;

    float signalLink;

    private static List<Bond> allBonds = new List<Bond>();
    public static List<Bond> GetAll() { return allBonds; }


    public Bond(Node newNodeA, Node newNodeB){
        nodeA = newNodeA;
        nodeB = newNodeB;

        muscleLine = new VectorLine("muscleLine", muscleEndPoints, muscleLineMaterial, 3.0f);
        signalLine = new VectorLine("signalLine", signalEndPoints, signalLineMaterial, 3.0f);
        synapseLine = new VectorLine("synapseLine", synapseEndPoints, synapseLineMaterial, 3.0f);
        motionLine = new VectorLine("motionLine", motionEndPoints, motionLineMaterial, 3.0f);

        signalLink = (Random.Range(0, 2) * 2) - 1;

        allBonds.Add(this);
    } // End of Awake().


    public void Update(){

        // Attractive force
		Vector3 vectorAtoB = nodeB.transform.position - nodeA.transform.position;
        Vector3 dirAtoB = vectorAtoB.normalized;
        float distToNode = vectorAtoB.magnitude;
		
        // DEBUG - attraction is a constant.
        float attraction = 12.0f;

        nodeA.rigidbody.AddForce(vectorAtoB.normalized * attraction);
        nodeB.rigidbody.AddForce(-vectorAtoB.normalized * attraction);



		// Determine synapse transfer.
		float calorieTransferStrength = attraction * 0.03f;
		float calorieTransfer = (nodeB.calories - nodeA.calories) * calorieTransferStrength;
		nodeA.caloriesDelta += calorieTransfer;
		nodeB.caloriesDelta += -calorieTransfer;
		

		// Render muscleLine ----------------------
        // Muscle is the actual physical connection between two bonded nodes.
		float muscleEndRadius = 0.6f;
        // Line direction shows direction of calorie flow.
		if(Mathf.Abs(nodeA.calories - nodeB.calories) >= 0.1)
			muscleLine.endCap = "CalorieCaps";
		else
			muscleLine.endCap = null;
        bool lineFlip = nodeB.calories > nodeA.calories;
		muscleEndPoints[lineFlip ? 1 : 0] = nodeA.transform.position + (dirAtoB * muscleEndRadius);
		muscleEndPoints[lineFlip ? 0 : 1] = nodeB.transform.position + (-dirAtoB * muscleEndRadius);
        // Line color shows strength of connection.
		Color muscleColor = Color.Lerp(Color.gray, Color.white, Mathf.Abs(calorieTransfer) * 3.0f);
		muscleColor.a = 0.5f;
        muscleLine.SetColor(muscleColor);
        // Line width based on magnitude of calorie flow.
        muscleLine.SetWidths(new float[] {2f});
        muscleLine.Draw3D();
		
		

        // Signal
        // Ensure that if one node is a sense node and the other is not, signal travels out.
        if((nodeA.nodeType == NodeType.sense) && (nodeB.nodeType != NodeType.sense))
            signalLink = 1;
        else if((nodeB.nodeType == NodeType.sense) && (nodeA.nodeType != NodeType.sense))
            signalLink = -1;
		// If one node is a 'sense' node...
		// Render signalLine ----------------------
        // Signal is the 'information' sent from sense nodes to control nodes.
		signalLine.SetWidths(new float[] {0});
		if((nodeA.signalRelay && (signalLink == 1)) || (nodeB.signalRelay && (signalLink == -1))){

			if((signalLink < 0) && (nodeB.signal > nodeA.signal))
				nodeA.signal = nodeB.signal;
			else if((signalLink > 0) && (nodeB.signal < nodeA.signal))
				nodeB.signal = nodeA.signal;

			signalLine.endCap = "CalorieCaps";
			float signalEndRadius = 0.4f;
			float signalOffset = 0.15f;
			Vector3 signalEndOffset = Quaternion.LookRotation(dirAtoB, Camera.main.transform.forward) * -Vector3.right * signalOffset;
			
			nodeA.signalRelay = true;
			nodeB.signalRelay = true;
		
			if(signalLink < 0){
				signalEndPoints[0] = nodeB.transform.position + signalEndOffset + (-dirAtoB * signalEndRadius);
				signalEndPoints[1] = nodeB.transform.position + signalEndOffset + (-dirAtoB * (signalEndRadius + ((distToNode - (2.0f * signalEndRadius)) * Mathf.Abs(signalLink * nodeB.signalDecay))));
			}
			else{
				signalEndPoints[0] = nodeA.transform.position + signalEndOffset + (dirAtoB * signalEndRadius);
				signalEndPoints[1] = nodeA.transform.position + signalEndOffset + (dirAtoB * (signalEndRadius + ((distToNode - (2.0f * signalEndRadius)) * Mathf.Abs(signalLink * nodeA.signalDecay))));
			}
	        // Line color shows strength of connection.
			Color signalColor = nodeA.senseColor;
			signalColor.a = 0.5f;
	        signalLine.SetColor(signalColor);
	        // Line width based on magnitude of signal flow.
	        signalLine.SetWidths(new float[] {2.0f});
		}
	    signalLine.Draw3D();
		
		
		// Render synapse/MotionLine ----------------------
        // Synapse is the muscle-control signal sent from control nodes that makes them move.
		synapseLine.SetWidths(new float[] {0});
		motionLine.SetWidths(new float[] {0});
		if(((nodeA.nodeType == NodeType.control) || (nodeB.nodeType == NodeType.control)) && 
		   ((nodeA.nodeType == NodeType.muscle) || (nodeB.nodeType == NodeType.muscle))){
			
            
			Vector3 forceToAdd;
			float motionLineSize = 0.2f;
			if(nodeA.nodeType == NodeType.muscle){
				nodeA.synapse = nodeB.synapse;
				
				forceToAdd = nodeA.muscleStrength * nodeA.muscleDirection * nodeB.synapse;
				nodeA.rigidbody.AddForce(forceToAdd);
				motionEndPoints[0] = nodeA.transform.position;
				motionEndPoints[1] = nodeA.transform.position + (forceToAdd * motionLineSize);
			}
			else{
				nodeB.synapse = nodeA.synapse;
				
				forceToAdd = nodeB.muscleStrength * nodeB.muscleDirection * nodeA.synapse;
				nodeB.rigidbody.AddForce(forceToAdd);
				motionEndPoints[0] = nodeB.transform.position;
				motionEndPoints[1] = nodeB.transform.position + (forceToAdd * motionLineSize);
			}
			motionLine.SetColor(nodeA.muscleColor);
			motionLine.SetWidths(new float[] {4.0f});
			

			float synapseEndRadius = 0.4f;
			float synapseOffset = 0.15f;
			Vector3 synapseEndOffset = Quaternion.LookRotation(dirAtoB, Camera.main.transform.forward) * Vector3.right * synapseOffset;
			synapseEndPoints[0] = nodeA.transform.position + synapseEndOffset + (dirAtoB * synapseEndRadius);
			synapseEndPoints[1] = nodeB.transform.position + synapseEndOffset + (-dirAtoB * synapseEndRadius);
	        // Line color shows strength of connection.
			Color synapseColor = Color.Lerp(nodeA.muscleColor, Color.white, 0f);
	        synapseLine.SetColor(Color.Lerp(synapseColor, Color.white, Mathf.Abs(nodeA.synapse) + Mathf.Abs(nodeB.synapse)));
            synapseColor.a = 0.5f;
	        // Line width based on magnitude of signal flow.
	        synapseLine.SetWidths(new float[] {1.0f});
		}
		motionLine.Draw3D();
	    synapseLine.Draw3D();
		
		// DEBUG
        // Applies a stronger bond between nodes. If we start with them this strong, however, they
        //   blast apart due to physics imprecision.
        if (Input.GetKey(KeyCode.C)){
            calcified = true;
        }
	} // End of Update().


    public void Destroy() {
        // Clear the vectorLine objects first, else they just float around.
        VectorLine.Destroy(ref muscleLine);
        VectorLine.Destroy(ref signalLine);
        VectorLine.Destroy(ref synapseLine);
        VectorLine.Destroy(ref motionLine);
    } // End of DestroyBond().
} // End of Bond.
