using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {none, sense, actuate, control}

public class Node {

    // Static ---------------------------------------------------------------------------- ||
    public static List<Node> allNodes = new List<Node>();
    public static List<Node> GetAll() { return allNodes; }

    // Variables ------------------------------------------------------------------------- ||
    public NodeType nodeType = NodeType.none;
    public NodeProperties nodeProperties = NodeProperties.random;

    public Assembly assembly = null;

    public Vector3 worldPosition = Vector3.zero;
    public IntVector3 localHexPosition = IntVector3.zero;

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

    // Calculated properties ------------------------------------------------------------- ||

    public Vector3 worldSenseVector {
        get{
            if(assembly)
                return assembly.worldRotation * nodeProperties.senseVector;
            else
                return nodeProperties.senseVector;
        }
    }

    public Vector3 worldAcuateVector {
        get{
            if(assembly)
                return assembly.worldRotation * nodeProperties.actuateVector;
            else
                return nodeProperties.actuateVector;
        }
    }

    public static implicit operator bool(Node exists){
        return exists != null;
    }


    // ------------------------------------------------------------------------------------ ||

    // Constructors
	public Node(){
        Initialize(Vector3.zero);
    }
	public Node(IntVector3 hexPos){
        localHexPosition = hexPos;
        Initialize(HexUtilities.HexToWorld(localHexPosition));
    }
    public Node(Vector3 worldPos){
        Initialize(worldPos);
    }

    // Copy Constructor - Make new node with current node position and orientation
    public Node Duplicate() {
        return new Node(localHexPosition);
    }

    // Set-up of basic Node stuff.
    private void Initialize(Vector3 worldPos){
        worldPosition = worldPos;

        // Initialize graphic
        gameObject = GameObject.Instantiate(PrefabManager.Inst.node, worldPosition, Quaternion.identity) as GameObject;

        allNodes.Add(this);
    } // End of Initialize().


    public void UpdateType(){
        int neighborCount = GetNeighbors().Count;
            switch(neighborCount){
                case 1:
                    nodeType = NodeType.sense;
                    gameObject.renderer.material.color = Node.senseColor;
                    break;
                case 2:
                    nodeType = NodeType.actuate;
                    gameObject.renderer.material.color = Node.actuatorColor;
                    break;
                case 3:
                    nodeType = NodeType.control;
                    gameObject.renderer.material.color = Node.controlColor;
                    break;
                default:
                    nodeType = NodeType.none;
                    gameObject.renderer.material.color = Node.stemColor;
                    break;
            }
    } // End of UpdateType().


    public void UpdateTransform(){
        if(assembly){
            worldPosition = assembly.worldPosition + (assembly.worldRotation * HexUtilities.HexToWorld(localHexPosition));

            // Update physical location
            gameObject.transform.position = worldPosition;
        }

        // Update arc rotation and such. 
        if(senseFieldBillboard){
            senseFieldBillboard.transform.position = worldPosition + (worldSenseVector * arcScale);
            senseFieldBillboard.transform.localScale = Vector3.one * arcScale;


            Color tempColor = senseColor;

            //calling detect food on sense node
            for(int j = 0; j < FoodPellet.GetAll().Count; ++j){
                bool detected = this.DetectFood(FoodPellet.GetAll()[j] );
                if( detected ){
                    tempColor = Color.cyan;
                    break;
                }
            }
                               
            senseFieldBillboard.renderer.material.SetColor("_TintColor", tempColor);

            // The following code billboards the arc with the main camera.
            senseFieldBillboard.transform.rotation = Quaternion.LookRotation(worldSenseVector);
            senseFieldBillboard.transform.position = worldPosition + (senseFieldBillboard.transform.rotation * (Vector3.forward * (0.5f * arcScale)));
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
        nodeProperties.senseVector = Quaternion.AngleAxis(Random.Range(0f, deviation) * 180f, Random.rotation * Vector3.forward) * nodeProperties.senseVector;
        nodeProperties.actuateVector = Quaternion.AngleAxis(Random.Range(0f, deviation) * 180f, Random.rotation * Vector3.forward) * nodeProperties.actuateVector;
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


    // Does this sense node detect a food node?
    public bool DetectFood(FoodPellet food){
        if(this.nodeType != NodeType.sense)
            return false;;
        Vector3 foodDir = food.worldPosition - this.worldPosition;
        float angle = Vector3.Angle(worldSenseVector, foodDir);
        if(angle < nodeProperties.fieldOfView)
            return true;
        return false;
    }


    // 'General' detect food... returns true if node detects any food pellet.
    public bool DetectFood(){
        for(int i = 0; i < FoodPellet.GetAll().Count; i++)
            if(DetectFood(FoodPellet.GetAll()[i]))
                return true;

        return false;
    }


    public int CountNeighbors(){
        // Count number of neighbors for the new position. If 3 or more, move on.
        int neighborCount = 0;
        for(int k = 0; k < 12; k++){
            IntVector3 currentNeighborPos = localHexPosition + HexUtilities.Adjacent(k);
            for(int m = 0; m < assembly.nodes.Count; m++){
                if(assembly.nodes[m].localHexPosition == currentNeighborPos){
                    neighborCount++;
                }
            }
        }
        return neighborCount;
    } // End of CountNeighbors().


    public int CountAllNeighborsRecursive(){
        List<Node> nodesOmitThis = new List<Node>(assembly.nodes);
        nodesOmitThis.Remove(this);
        return CountAllNeighborsRecursive(this, nodesOmitThis);
    } // End of CountAllNeighborsRecursive();


    public static int CountAllNeighborsRecursive(Node node, List<Node> nodesToTest){
        int neighborCount = 0;
        nodesToTest.Remove(node);

        List<Node> neighbors = node.GetNeighbors();
        List<Node> testableNeighbors = new List<Node>();

        for(int i = 0; i < neighbors.Count; i++){
            if(nodesToTest.Contains(neighbors[i])){
                testableNeighbors.Add(neighbors[i]);
            }
        }

        neighborCount = testableNeighbors.Count;

        for(int i = 0; i < testableNeighbors.Count; i++)
            nodesToTest.Remove(testableNeighbors[i]);

        for(int i = 0; i < testableNeighbors.Count; i++){
            neighborCount += CountAllNeighborsRecursive(testableNeighbors[i], nodesToTest);
        }

        return neighborCount;
    } // End of CountAllNeighborsRecursive();


    // The string representation of this class for file saving (could use ToString, but want to be explicit)
    public string ToFileString(int format)
    {
        return  localHexPosition.x + "," +
                localHexPosition.y + "," +
                localHexPosition.z;

        /* old - removed orientation
        return  localHexPosition.x + "," +
                localHexPosition.y + "," +
                localHexPosition.z + "," +
                orientation.x + "," +
                orientation.y + "," +
                orientation.z + "," +
                orientation.w;
        */
    }

    public static Node FromString(string str, int format=1)
    {
        string[] tok = str.Split(',');
        IntVector3 pos = new IntVector3(int.Parse(tok[0]), int.Parse(tok[1]), int.Parse(tok[2]));
        return new Node(pos);

        /* old - removed orientation
        string[] tok = str.Split(',');
        IntVector3 pos = new IntVector3(int.Parse(tok[0]), int.Parse(tok[1]), int.Parse(tok[2]));
        Quaternion rot = new Quaternion(float.Parse(tok[3]), float.Parse(tok[4]), float.Parse(tok[5]), float.Parse(tok[6]));
        return new Node(pos, rot);
        */
    }

} // End of Node.


public struct NodeProperties {

    // Sense
    public Vector3 senseVector;
    public float fieldOfView;

    // Actuate
    public Vector3 actuateVector;


    // A fully randomly-seeded NodeProperties.
    public static NodeProperties random{
        get{
            // Sense
            Vector3 _senseVector = Random.rotation * Vector3.forward;
            float _fieldOfView = 45f;

            // Actuate
            Vector3 _actuateVector = Random.rotation * Vector3.forward;

            return new NodeProperties(Random.rotation * Vector3.forward, 45f, Random.rotation * Vector3.forward);
        }
    } // End of NodeProperties.random.

    // Constructor
    public NodeProperties(Vector3 _senseVector, float _fieldOfView, Vector3 _actuateVector){
        senseVector = _senseVector;
        fieldOfView = _fieldOfView;
        actuateVector = _actuateVector;
    } // End of NodeProperties constructor.

} // End of NodeProperties.
