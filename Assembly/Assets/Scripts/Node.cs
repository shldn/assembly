using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {none, sense, actuate, control}

public class Node {

    // Static ---------------------------------------------------------------------------- ||
    public static List<Node> allNodes = new List<Node>();

    // Variables ------------------------------------------------------------------------- ||
    public NodeType nodeType = NodeType.none;

    public Assembly assembly = null;

    public Vector3 worldPosition = Vector3.zero;
    public IntVector3 localHexPosition = IntVector3.zero;

    public Quaternion orientation = Quaternion.identity;
    public float fieldOfView = 45f;

    // Graphics -------------------------------------------------------------------------- ||
    public GameObject gameObject = null;

    public GameObject senseFieldBillboard = null;
    float arcScale = 5f;

    public GameObject actuateVectorBillboard = null;
    float actuateVecScale = 5f;

    public static Color stemColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public static Color senseColor = new Color(0.64f, 0.8f, 0.44f, 1f);
    public static Color actuatorColor = new Color(0.62f, 0.18f, 0.18f, 1f);
    public static Color controlColor = new Color(0.35f, 0.59f, 0.84f, 1f);



    public static implicit operator bool(Node exists){
        return exists != null;
    }


    // Constructors
	public Node(){
        Initialize();
    }
	public Node(IntVector3 hexPos){
        localHexPosition = hexPos;
        worldPosition = HexUtilities.HexToWorld(localHexPosition);
        Initialize();
    }
    public Node(Vector3 worldPos){
        worldPosition = worldPos;
        Initialize();
    }

    // Set-up of basic Node stuff.
    private void Initialize(){
        // Initialize graphic
        gameObject = GameObject.Instantiate(PrefabManager.Inst.node, worldPosition, Quaternion.identity) as GameObject;
        // Randomize attributes
        orientation = Random.rotation;

        allNodes.Add(this);
    } // End of Initialize().


    public void UpdateTransform(){
        if(assembly){
            worldPosition = assembly.worldPosition + (assembly.worldRotation * HexUtilities.HexToWorld(localHexPosition));

            // Update physical location
            gameObject.transform.position = worldPosition;
        }

        // Update arc rotation and such. 
        if(senseFieldBillboard){
            senseFieldBillboard.transform.position = worldPosition + ((orientation * assembly.worldRotation) * (Vector3.forward * arcScale));
            senseFieldBillboard.transform.localScale = Vector3.one * arcScale;


            Color tempColor = senseFieldBillboard.renderer.material.GetColor("_TintColor");
            senseFieldBillboard.renderer.material.SetColor("_TintColor", tempColor);


            // The following code billboards the arc with the main camera.
            senseFieldBillboard.transform.rotation = orientation;
            senseFieldBillboard.transform.position = worldPosition + (orientation * (Vector3.forward * (0.5f * arcScale)));
            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);

            Vector3 camRelativePos = senseFieldBillboard.transform.InverseTransformPoint(Camera.main.transform.position);
            float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;

            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);
        }


        // Dynamically update existence of senseFieldBillboard.
        if((nodeType != NodeType.sense) && senseFieldBillboard)
            GameObject.Destroy(senseFieldBillboard);

        if((nodeType == NodeType.sense) && !senseFieldBillboard)
            senseFieldBillboard = GameObject.Instantiate(PrefabManager.Inst.billboard, worldPosition, Quaternion.identity) as GameObject;
    } // End of UpdateTransform(). 


    public void Destroy(){
        if(gameObject)
            GameObject.Destroy(gameObject);
        if(senseFieldBillboard)
            GameObject.Destroy(senseFieldBillboard);
        if(assembly)
            assembly.RemoveNode(this);

        allNodes.Remove(this);
    } // End of Destroy().


    // Randomly 'mutates' the node's values. A deviation of 1 will completely randomize the node.
    public void Mutate(float deviation){
        orientation *= Quaternion.AngleAxis(Random.Range(0f, deviation) * 180f, Random.rotation * Vector3.forward);
    } // End of Mutate().


    public List<Node> GetNeighbors(){
        // No assembly... no neighbors!
        if(!assembly)
            return null;

        List<Node> neighbors = new List<Node>();
        // Loop through all adjacent positions and see if they are occupied.
        for(int i = 0; i < 12; i++){
            IntVector3 currentNeighborPos = localHexPosition + HexUtilities.Adjacent(i);
            for(int j = 0; j < assembly.nodes.Count; j++){
                if(assembly.nodes[j].localHexPosition == currentNeighborPos){
                    neighbors.Add(assembly.nodes[j]);
                }
            }
        }
        return neighbors;
    } // End of GetNeighbors().


    // Returns neighbors that this node can send a signal to.
    public List<Node> GetLogicConnections(){
        // No assembly... no neighbors... no logic!
        if(!assembly)
            return null;

        List<Node> logicNodes = new List<Node>();
        List<Node> neighbors = GetNeighbors();

        for(int i = 0; i < neighbors.Count; i++){
            Node currentNeighbor = neighbors[i];

            // Sense transmits to control
            if(nodeType == NodeType.sense)
                if(currentNeighbor.nodeType == NodeType.control)
                    logicNodes.Add(currentNeighbor);

            // Control transmits to actuate
            if(nodeType == NodeType.control)
                if(currentNeighbor.nodeType == NodeType.actuate)
                    logicNodes.Add(currentNeighbor);

            // Actuate transmits to other actuate
            if(nodeType == NodeType.actuate)
                if(currentNeighbor.nodeType == NodeType.actuate)
                    logicNodes.Add(currentNeighbor);
        }

        return logicNodes;
    } // End of GetLogicConnections().


    // Returns all nodes 'down the line' that this node would propogate a signal to.
    public List<Node> GetFullLogicNet(){
        // No assembly... no neighbors... no logic!
        if(!assembly)
            return null;

        List<Node> logicNodes = new List<Node>();

        // Churn through (logical) nodes to test for new connections...
        List<Node> nodesToTest = GetLogicConnections();

        int logicDumpCatch = 0;

        while(nodesToTest.Count > 0){
            Node currentNode = nodesToTest[0];
            logicNodes.Add(currentNode);
            nodesToTest.Remove(currentNode);

            // Test the node for logic neighbors.
            List<Node> newNeighbors = currentNode.GetLogicConnections();
            for(int i = 0; i < newNeighbors.Count; i++){
                Node curNewNeighbor = newNeighbors[i];
                // If a logic neighbor hasn't been captured, add it to logicNodes and the nodesToTest pile.
                if((curNewNeighbor != this) && !logicNodes.Contains(curNewNeighbor)){
                    nodesToTest.Add(curNewNeighbor);
                }
            }

            // debug
            logicDumpCatch++;
            if(logicDumpCatch > 999){
                MonoBehaviour.print("LogicNet while() loop is stuck!");
                break;
            }
        }

        return logicNodes;
    } // End of GetFullLogicNet().
} // End of Node.