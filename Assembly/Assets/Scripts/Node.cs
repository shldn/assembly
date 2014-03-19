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

    public bool doomed = false;
    public Vector3 sendOffVector = Vector3.zero;
    private float disappearTimer = Random.Range(0f, 1f);

    public Quaternion worldRotation = Quaternion.identity;
    public Quaternion localRotation = Quaternion.identity;

    // Metabolism ------------------------------------------------------------------------ ||
    public static float MAX_ENERGY = 10.0f; //not used?
    public float energy = MAX_ENERGY; //not used?
    public float consumeRange = 30; //how far away can it consume?

    // Graphics -------------------------------------------------------------------------- ||
    public GameObject gameObject = null;

    public GameObject senseFieldBillboard = null;
    float arcScale = 5f;

    public GameObject actuateVectorBillboard = null;
    float actuateVecScale = 5f;

    public bool validLogic = false; //sense - control 
    public bool propelling = false; //red node moving

    // debug
    public GameObject propulsionEffect = null;

    public static Color stemColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public static Color senseColor = new Color(0.64f, 0.8f, 0.44f, 1f);
    public static Color actuatorColor = new Color(0.67f, 0.22f, 0.22f, 1f);
    public static Color controlColor = new Color(0.35f, 0.59f, 0.84f, 1f);

    // Calculated properties ------------------------------------------------------------- ||

    public Quaternion worldSenseRot {
        get{
            if(assembly)
                return assembly.physicsObject.transform.rotation * nodeProperties.senseVector;
            else
                return nodeProperties.senseVector;
        }
    }

    public Quaternion worldAcuateRot {
        get{
            if(assembly)
                return assembly.physicsObject.transform.rotation * nodeProperties.actuateVector;
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
    public Node(IntVector3 hexPos, NodeProperties props)
    {
        localHexPosition = hexPos;
        nodeProperties = props;
        Initialize(HexUtilities.HexToWorld(localHexPosition));
    }

    // Copy Constructor - Make new node with current node position and orientation
    public Node Duplicate() {
        Node newNode = new Node(localHexPosition);
        newNode.nodeProperties.actuateVector = nodeProperties.actuateVector;
        newNode.nodeProperties.fieldOfView = nodeProperties.fieldOfView;
        newNode.nodeProperties.senseVector = nodeProperties.senseVector;
        return newNode;
    }

    // Set-up of basic Node stuff.
    private void Initialize(Vector3 worldPos){
        worldPosition = worldPos;
        localRotation = Random.rotation;

        allNodes.Add(this);

    } // End of Initialize().


    public void UpdateType(){
        List<Node> neighbors = GetNeighbors();
        int neighborCount = (neighbors == null) ? 0 : neighbors.Count;

            switch(neighborCount){
                case 1:
                    nodeType = NodeType.sense;
                    break;
                case 2:
                    nodeType = NodeType.actuate;
                    break;
                case 3:
                    nodeType = NodeType.control;
                    break;
                default:
                    nodeType = NodeType.none;
                    break;
            }
    } // End of UpdateType().


    public void UpdateColor(){
        switch(nodeType){
            case NodeType.sense:
                gameObject.renderer.material.color = Node.senseColor;
                break;
            case NodeType.actuate:
                gameObject.renderer.material.color = Node.actuatorColor;
                break;
            case NodeType.control:
                gameObject.renderer.material.color = Node.controlColor;
                break;
            default:
                gameObject.renderer.material.color = Node.stemColor;
                break;
        }
        if(!validLogic)
            gameObject.renderer.material.color = Color.Lerp(gameObject.renderer.material.color, new Color(0.2f, 0.2f, 0.2f), 0.8f);
    } // End of UpdateColor().


    public void UpdateTransform(){

        // Destroy Node if it's dead and has reached the end of it's DissapearRate timer.
        if(doomed){
            disappearTimer -= Time.deltaTime;

            if(disappearTimer <= 0f)
                Destroy();

            worldPosition += sendOffVector * Time.deltaTime;
        }
            

        // Initialize graphic
        if( !gameObject ){
            gameObject = GameObject.Instantiate(PrefabManager.Inst.node, worldPosition, Quaternion.identity) as GameObject;
        }

        if(assembly){
            worldPosition = assembly.WorldPosition + (assembly.WorldRotation * HexUtilities.HexToWorld(localHexPosition));
            worldRotation = assembly.WorldRotation * localRotation;
        }

        // Update physical location
        gameObject.transform.position = worldPosition;
        gameObject.transform.rotation = worldRotation;

        // Sense node view arc ----------------------------------------------------------
        // Dynamically update existence of senseFieldBillboard.
        if(((nodeType != NodeType.sense) || !validLogic) && senseFieldBillboard)
            GameObject.Destroy(senseFieldBillboard);

        if((nodeType == NodeType.sense) && validLogic && !senseFieldBillboard)
            senseFieldBillboard = GameObject.Instantiate(PrefabManager.Inst.billboard, worldPosition, Quaternion.identity) as GameObject;

        // Update arc rotation and such. 
        if(senseFieldBillboard){
            senseFieldBillboard.transform.position = worldPosition + (worldSenseRot * Vector3.forward * arcScale);
            senseFieldBillboard.transform.localScale = Vector3.one * arcScale;

            Debug.DrawRay(worldPosition, worldSenseRot * Vector3.forward * 3f, Color.green);

            Color tempColor = senseColor;

            //calling detect food on sense node
            for(int j = 0; j < FoodPellet.GetAll().Count; ++j){
                bool detected = this.DetectFood(FoodPellet.GetAll()[j] );
                if( detected ){
                    //change detection color
                    tempColor = Color.cyan;

                    if(SenseDetectFoodRange(FoodPellet.GetAll()[j]) ){
                        //sense node consume food source
                        assembly.Consume( FoodPellet.GetAll()[j] );
                    }
                    break;
                }
            }
                               
            senseFieldBillboard.renderer.material.SetColor("_TintColor", tempColor);

            // The following code billboards the arc with the main camera.
            senseFieldBillboard.transform.rotation = worldSenseRot;
            senseFieldBillboard.transform.position = worldPosition + (senseFieldBillboard.transform.rotation * (Vector3.forward * (0.5f * arcScale)));
            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);

            Vector3 camRelativePos = senseFieldBillboard.transform.InverseTransformPoint(Camera.main.transform.position);
            float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;

            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);
        }


        // debug
        // Actuate node jet engine prop ------------------------------------------------- ||
        // Dynamically update existence of jetEngine.
        if(((nodeType != NodeType.actuate) || !validLogic) && propulsionEffect)
            GameObject.Destroy(propulsionEffect);

        if((nodeType == NodeType.actuate) && validLogic && !propulsionEffect)
            propulsionEffect = GameObject.Instantiate(PrefabManager.Inst.jetEngine, worldPosition, Quaternion.identity) as GameObject;

        if(propulsionEffect){
            Debug.DrawRay(worldPosition, worldAcuateRot * Vector3.forward * 3f, Color.red);

            propulsionEffect.transform.position = worldPosition + (worldAcuateRot * Vector3.forward) * -0.5f;
            propulsionEffect.transform.rotation = worldAcuateRot;
        }
    } // End of UpdateTransform(). 


    public void Destroy(){
        if(gameObject)
            GameObject.Destroy(gameObject);
        if(senseFieldBillboard)
            GameObject.Destroy(senseFieldBillboard);
        if(propulsionEffect)
            GameObject.Destroy(propulsionEffect);
        if(assembly)
            assembly.RemoveNode(this);

        allNodes.Remove(this);
    } // End of Destroy().


    // Randomly 'mutates' the node's values. A deviation of 1 will completely randomize the node.
    public void Mutate(float deviation){
        nodeProperties.senseVector = MathUtilities.SkewRot(nodeProperties.senseVector, Random.Range(0f, deviation * 180f));
        nodeProperties.actuateVector = MathUtilities.SkewRot(nodeProperties.actuateVector, Random.Range(0f, deviation * 180f));
    } // End of Mutate().


    // Logic ---------------------------------------------------------------------------------||

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


    // Returns neighbors that this node can reseive a signal from.
    public List<Node> GetReverseLogicConnections(){
        // No assembly... no neighbors... no logic!
        if(!assembly)
            return null;

        List<Node> logicNodes = new List<Node>();
        List<Node> neighbors = GetNeighbors();

        for(int i = 0; i < neighbors.Count; i++){
            Node currentNeighbor = neighbors[i];

            // Control transmits to actuate
            if(nodeType == NodeType.actuate)
                if(currentNeighbor.nodeType == NodeType.control)
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


    // Returns all nodes 'down the line' that this node would propogate a signal to.
    public List<Node> GetFullReverseLogicNet(){
        // No assembly... no neighbors... no logic!
        if(!assembly)
            return null;

        List<Node> logicNodes = new List<Node>();

        // Churn through (logical) nodes to test for new connections...
        List<Node> nodesToTest = GetReverseLogicConnections();

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


    // Food Pellets ---------------------------------------------------------------------------||

    // Does this sense node detect a certain food node?
    public bool DetectFood(FoodPellet food){
        if(this.nodeType != NodeType.sense)
            return false;
        Vector3 foodDir = food.worldPosition - this.worldPosition;
        float angle = Vector3.Angle(worldSenseRot * Vector3.forward, foodDir);

        if(angle < nodeProperties.fieldOfView)
            return true;
        // Return false if no food pellets found.
        return false;
    }

    // 'General' detect food... returns true if node detects any food pellet.
    public bool DetectFood(){
        for(int i = 0; i < FoodPellet.GetAll().Count; i++)
            if(DetectFood(FoodPellet.GetAll()[i]))
                return true;
        // Return false if no food pellets found.
        return false;
    }

    // Same as general DetectFood() but references a list of all detected food pellets.
    public bool DetectFood(ref List<FoodPellet> allFood){
        bool sensedFood = false;
        for(int i = 0; i < FoodPellet.GetAll().Count; i++)
            if(DetectFood(FoodPellet.GetAll()[i])){
                allFood.Add(FoodPellet.GetAll()[i]);
                sensedFood = true;
            }
        // Return false if no food pellets found.
        return sensedFood;
    }


    // Gets the rotation from the node to a certain foodPellet.
    public Quaternion RotToFood(FoodPellet food){
        // Get rotation to food
        Quaternion quatToFood = Quaternion.LookRotation(food.worldPosition - worldPosition, worldSenseRot * Vector3.up);

        Quaternion relativeQuatToFood = Quaternion.Inverse(worldSenseRot) * quatToFood;

        Debug.DrawRay(worldPosition, (worldSenseRot * relativeQuatToFood) * Vector3.forward * 3f);


        return relativeQuatToFood;
    }

    public float FoodSignalStrength(FoodPellet food){
        return 1f / Vector3.Distance(worldPosition, food.worldPosition);
    }

    
    // Neighbors ---------------------------------------------------------------------------||

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


    // Save/load -------------------------------------------------------------------------||

    // The string representation of this class for file saving (could use ToString, but want to be explicit)
    /*public string ToFileString(int format)
    {
        return localHexPosition.ToString() + nodeProperties.ToString();
    }

    public static Node FromString(string str, int format=1)
    {
        int splitIdx = str.IndexOf(')');
        IntVector3 pos = IOHelper.IntVector3FromString(str.Substring(0,splitIdx+1));
        NodeProperties props = new NodeProperties(str.Substring(splitIdx + 1));
        return new Node(pos, props);
    }*/

    public float GetBurnRate(){
        switch(nodeType){
            case NodeType.none:
                return BurnRate.none;
            case NodeType.sense:
                if(validLogic) //if green light
                    return BurnRate.senseValid;
                else //no green light
                    return BurnRate.sense;
            case NodeType.actuate:
                if(propelling) //if red moving
                    return BurnRate.actuateValid;
                else //if red not moving
                    return BurnRate.actuate;
            case NodeType.control:
                if(validLogic) //if control logic set
                    return BurnRate.controlValid;
                else // no connection
                    return BurnRate.control;
        }
        return BurnRate.none;
    }

    //check if food is within consumption range
    public bool SenseDetectFoodRange(FoodPellet food){
        Vector3 foodDist = food.worldPosition - this.worldPosition;
        //if mag^2 < consume^2
        if(foodDist.sqrMagnitude < (consumeRange * consumeRange)){
            return true;
        }
        return false;
    }

} // End of Node.


public struct NodeProperties {

    // Sense
    public Quaternion senseVector;
    public float fieldOfView;

    // Actuate
    public Quaternion actuateVector;


    // A fully randomly-seeded NodeProperties.
    public static NodeProperties random{
        get{
            // Sense
            Quaternion _senseVector = Random.rotation;
            float _fieldOfView = 45f;

            // Actuate
            Quaternion _actuateVector = Random.rotation;

            return new NodeProperties(Random.rotation, 45f, Random.rotation);
        }
    } // End of NodeProperties.random.

    // Constructor
    public NodeProperties(Quaternion _senseVector, float _fieldOfView, Quaternion _actuateVector){
        senseVector = _senseVector;
        fieldOfView = _fieldOfView;
        actuateVector = _actuateVector;
    } // End of NodeProperties constructor.

    /*public NodeProperties(string str){

        senseVector = Quaternion.identity;
        fieldOfView = 45.0f;
        actuateVector = Quaternion.identity;

        string[] tok = str.Split(';');
        for(int i=0; i < tok.Length; ++i)
        {
            string[] pair = tok[i].Split(':');
            switch(pair[0]){
                case "sv":
                    senseVector = IOHelper.QuaternionFromString(pair[1]);
                    break;
                case "av":
                    actuateVector = IOHelper.QuaternionFromString(pair[1]);
                    break;
                case "fov":
                    if(!float.TryParse(pair[1], out fieldOfView))
                        Debug.LogError("fov failed to parse");
                    break;
                default:
                    Debug.LogError("Unknown property: " + pair[0]);
                    break;
            }
        }
    } // End of NodeProperties constructor.*/

    public override string ToString()
    {
        return  "sv" + ":" + senseVector.ToString() + ";" +
                "av" + ":" + actuateVector.ToString() + ";" +
                "fov" + ":" + fieldOfView.ToString();
    }

} // End of NodeProperties.

//burn rate for different types: none, sense, actuate- static, actuate- woring, control
public static class BurnRate{
    public static float none = 0.0f;
    public static float sense = 0.01f;
    public static float actuate = 0.02f;
    public static float control = 0.03f;
    public static float actuateValid = 0.04f;
    public static float senseValid = 0.015f;
    public static float controlValid = 0.05f;
} // End of BurnRate
