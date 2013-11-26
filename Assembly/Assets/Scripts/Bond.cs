using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BondType {useless, signal, synapse, synapsePropogate, thrustPropogate}


public class Bond{

    public Node nodeA;
    public Node nodeB;

    public BondType bondType = BondType.useless;

    public Vector3 dirAtoB;

    public Material motionLineMaterial;

    float signalLink = 0f;

    public Transform bondBillboard;
    float bondWidth = 0.3f;


    private static List<Bond> allBonds = new List<Bond>();
    public static List<Bond> GetAll() { return allBonds; }


    public Bond(Node newNodeA, Node newNodeB){
        if (newNodeA == newNodeB) {
            Debug.LogError("Trying to bond node with itself!");
            return;
        }
        nodeA = newNodeA;
        nodeB = newNodeB;

        nodeA.bonds.Add(this);
        nodeB.bonds.Add(this);


        // If neither node are in an assembly, create a new assembly.
        if((nodeA.assembly == null) && (nodeB.assembly == null))
            new Assembly(nodeA);

        // If one of the nodes is in an assembly, add the other to it.
        if((nodeA.assembly != null) && (nodeB.assembly == null))
            nodeA.assembly.AddNode(nodeB);

        if((nodeB.assembly != null) && (nodeA.assembly == null))
            nodeB.assembly.AddNode(nodeA);

        // If both are in assemblies, merge them.
        if((nodeA.assembly != null) && (nodeB.assembly != null) && (nodeA.assembly != nodeB.assembly)){
            nodeA.assembly.Merge(nodeB.assembly);
        }


        // Randomize direction of signalLink.
        signalLink = (Random.Range(0, 2) * 2) - 1;


        // Create bond billboard.
        GameObject bondBillboardGO = new GameObject();
        bondBillboardGO.name = "BondBillboard";
        bondBillboardGO.AddComponent<MeshFilter>();
        bondBillboardGO.GetComponent<MeshFilter>().mesh = GameManager.graphics.twoPolyPlane;
        bondBillboardGO.AddComponent<MeshRenderer>();
        bondBillboard = bondBillboardGO.transform;


        allBonds.Add(this);
    } // End of Awake().


    public void Update(){

        Vector3 vectorAtoB = nodeB.transform.position - nodeA.transform.position;
        dirAtoB = vectorAtoB.normalized;
        float bondLength = vectorAtoB.magnitude;


        // The following billboards the bond graphic with the camera.
        Quaternion bondDirection = Quaternion.LookRotation(nodeB.transform.position - nodeA.transform.position);
        bondBillboard.rotation = bondDirection;
        bondBillboard.position = nodeA.transform.position + (vectorAtoB * 0.5f);
        bondBillboard.rotation *= Quaternion.AngleAxis(90, Vector3.up);

        Vector3 camRelativePos = bondBillboard.InverseTransformPoint(Camera.main.transform.position);
        float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;

        bondBillboard.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);

        Vector3 bondBillboardScale = new Vector3(bondLength * signalLink, bondWidth, 1f);
        bondBillboard.localScale = bondBillboardScale;




        // DEBUG - attraction is a constant.
        float attraction = 12.0f;

        nodeA.rigidbody.AddForce(vectorAtoB.normalized * attraction);
        nodeB.rigidbody.AddForce(-vectorAtoB.normalized * attraction);


        

		// Determine calorie transfer.
		float calorieTransferStrength = attraction * 0.03f;
		float calorieTransfer = (nodeB.calories - nodeA.calories) * calorieTransferStrength;
		nodeA.caloriesDelta += calorieTransfer;
		nodeB.caloriesDelta += -calorieTransfer;
		
		
		// Different effects based on bond type.
        if((nodeA.nodeType == NodeType.sense) && (nodeB.nodeType == NodeType.control) ||
           (nodeB.nodeType == NodeType.sense) && (nodeA.nodeType == NodeType.control)){
            // Signal Bond
            // Sense -> Control
            bondType = BondType.signal;
            bondBillboard.renderer.material = GameManager.graphics.signalBondMat;

            if(nodeA.nodeType == NodeType.sense){
                signalLink = 1;
                if(nodeA.signal > nodeB.synapse)
                    nodeB.synapse = nodeA.signal;
            }
            else{
                signalLink = -1;
                if(nodeB.signal > nodeA.synapse)
                    nodeA.synapse = nodeB.signal;
            }
        }
        else
        if((nodeA.nodeType == NodeType.control) && (nodeB.nodeType == NodeType.muscle) ||
           (nodeB.nodeType == NodeType.control) && (nodeA.nodeType == NodeType.muscle)){
            // Synapse bond
            // Control -> Muscle
            bondType = BondType.synapse;
            bondBillboard.renderer.material = GameManager.graphics.synapseBondMat;

            if(nodeA.nodeType == NodeType.control){
                signalLink = 1;
                if(nodeA.synapse > nodeB.thrust)
                    nodeB.thrust = nodeA.synapse;

                nodeB.thrust = Mathf.Clamp(nodeB.thrust, 0.1f, Mathf.Infinity);
                nodeB.rigidbody.AddForce(-vectorAtoB * 10f * nodeB.thrust);
            }
            else{
                signalLink = -1;
                if(nodeB.synapse > nodeA.thrust)
                    nodeA.thrust = nodeB.synapse;

                nodeA.thrust = Mathf.Clamp(nodeA.thrust, 0.1f, Mathf.Infinity);
                nodeA.rigidbody.AddForce(vectorAtoB * 10f * nodeA.thrust);

            }
        }
        else
        if((nodeA.nodeType == NodeType.control) && (nodeB.nodeType == NodeType.control)){
            // Synapse propogation bond
            // Control <-> Control
            bondType = BondType.synapsePropogate;
            bondBillboard.renderer.material = GameManager.graphics.synapsePropBondMat;

            if(nodeA.synapse > nodeB.synapse)
                nodeB.synapse = nodeA.synapse;
            else
                nodeA.synapse = nodeB.synapse;
        }
        else
        if((nodeA.nodeType == NodeType.muscle) && (nodeB.nodeType == NodeType.muscle)){
            // Muscle propogation bond
            // Muscle <-> Muscle
            bondType = BondType.thrustPropogate;
            bondBillboard.renderer.material = GameManager.graphics.thrustPropBondMat;

            if(nodeA.thrust > nodeB.thrust)
                nodeB.thrust = nodeA.thrust;
            else
                nodeA.thrust = nodeB.thrust;
        }
        else{
            // Not a functional bond.
            bondBillboard.renderer.material = GameManager.graphics.uselessBondMat;
            bondType = BondType.useless;
        }

	} // End of Update().


    public void Destroy() {
        nodeA.bonds.Remove(this);
        nodeB.bonds.Remove(this);
        allBonds.Remove(this);
        GameObject.Destroy(bondBillboard.gameObject);
    } // End of DestroyBond().

    public Node GetOtherNode(Node notThisNode) {
        return (nodeA == notThisNode) ? nodeB : nodeA;
    }
} // End of Bond.
