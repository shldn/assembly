using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {none, sense, actuate, control}

public class Node {

    // Static ---------------------------------------------------------------------------- ||
    public static List<Node> allNodes = new List<Node>();
    public static List<Node> GetAll() { return allNodes; }

    // Variables ------------------------------------------------------------------------- ||
    public NodeProperties nodeProperties = NodeProperties.random;

    public Assembly assembly = null;

    public Vector3 worldPosition = Vector3.zero;
    public IntVector3 localHexPosition = IntVector3.zero;

    public bool doomed = false;
    public Vector3 sendOffVector = Vector3.zero;
    private float disappearTimer = Random.Range(0f, 1f);

    public Quaternion worldRotation = Quaternion.identity;
    public Quaternion localRotation = Quaternion.identity;

    public List<Node> neighbors = new List<Node>();

    public Color baseColor = PrefabManager.Inst.stemColor;
    public bool signalLock = false;
    public bool activeLogic = false;


    // Graphics -------------------------------------------------------------------------- ||
    public GameObject gameObject = null;

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
    public Node(IntVector3 hexPos, NodeProperties props){
        localHexPosition = hexPos;
        nodeProperties = props;
        Initialize(HexUtilities.HexToWorld(localHexPosition));
    }
    // Copy an old node.
    public Node(Node oldNode){
        localHexPosition = oldNode.localHexPosition;
        localRotation = oldNode.localRotation;
        nodeProperties.actuateVector = oldNode.nodeProperties.actuateVector;
        nodeProperties.fieldOfView = oldNode.nodeProperties.fieldOfView;
        nodeProperties.senseVector = oldNode.nodeProperties.senseVector;
        Initialize(HexUtilities.HexToWorld(localHexPosition));

        // Manual assembly list insertion...
        assembly = oldNode.assembly;
        assembly.nodes.Add(this);
    }


    // Set-up of basic Node stuff.
    private void Initialize(Vector3 worldPos){
        worldPosition = worldPos;
        localRotation = Random.rotation;
        gameObject = GameObject.Instantiate(PrefabManager.Inst.node, worldPosition, Quaternion.identity) as GameObject;

        allNodes.Add(this);
    } // End of Initialize().

    public void UpdateType(){
        neighbors = GetNeighbors();
        int neighborCount = (neighbors == null) ? 0 : neighbors.Count;
        switch(neighborCount){
            case 1:
                if(this.GetType() != typeof(SenseNode)){
                    SenseNode newNode = new SenseNode(this);
                    Destroy();
                }
                break;
            case 2:
                if(this.GetType() != typeof(ActuateNode)){
                    ActuateNode newNode = new ActuateNode(this);
                    Destroy();
                }
                break;
            case 3:
                if(this.GetType() != typeof(ControlNode)){
                    ControlNode newNode = new ControlNode(this);
                    Destroy();
                }
                break;
        }
    } // End of UpdateType().

    public virtual void Update(){

        gameObject.renderer.material.color = baseColor;
        if(!activeLogic)
            gameObject.renderer.material.color = Color.Lerp(baseColor, new Color(0.2f, 0.2f, 0.2f), 0.9f);

        // Destroy Node if it's dead and has reached the end of it's DisappearRate timer.
        if(doomed){
            disappearTimer -= Time.deltaTime;

            if(disappearTimer <= 0f)
                Destroy();

            worldPosition += sendOffVector * Time.deltaTime;
        } 

        if(assembly){
            worldPosition = assembly.WorldPosition + (assembly.WorldRotation * HexUtilities.HexToWorld(localHexPosition));
            worldRotation = assembly.WorldRotation * localRotation;

            assembly.currentEnergy -= GetBurnRate() * Assembly.burnCoefficient * Time.deltaTime;
        }

        // Update physical location
        gameObject.transform.position = worldPosition;
        gameObject.transform.rotation = worldRotation;
    } // End of UpdateTransform(). 


    public virtual void Destroy(){
        if(gameObject)
            GameObject.Destroy(gameObject);
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
        neighbors = GetNeighbors();

        for(int i = 0; i < neighbors.Count; i++){
            Node currentNeighbor = neighbors[i];

            // Sense transmits to control
            if(this.GetType() == typeof(SenseNode))
                if(currentNeighbor.GetType() == typeof(ControlNode))
                    logicNodes.Add(currentNeighbor);

            // Control transmits to actuate
            if(this.GetType() == typeof(ControlNode))
                if(currentNeighbor.GetType() == typeof(ActuateNode))
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
        neighbors = GetNeighbors();

        for(int i = 0; i < neighbors.Count; i++){
            Node currentNeighbor = neighbors[i];

            // Sense transmits to control
            if(this.GetType() == typeof(ActuateNode))
                if(currentNeighbor.GetType() == typeof(ControlNode))
                    logicNodes.Add(currentNeighbor);

            // Control transmits to actuate
            if(this.GetType() == typeof(ControlNode))
                if(currentNeighbor.GetType() == typeof(SenseNode))
                    logicNodes.Add(currentNeighbor);
        }

        return logicNodes;
    } // End of GetLogicConnections().


    // Neighbors ---------------------------------------------------------------------------||
    public List<Node> GetNeighbors(){
        // No assembly... no neighbors!
        if(!assembly)
            return null;

        neighbors = new List<Node>();
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

    public virtual float GetBurnRate(){return 0f;}

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
            float _fieldOfView = 90f;

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

    public override string ToString(){
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