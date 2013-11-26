using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {stem, sense, control, muscle}

public class Node : MonoBehaviour {

	public string nodeName = "Node";
	public NodeType nodeType = NodeType.stem;

    public List<Bond> bonds = new List<Bond>();
    public int BondCount() { return bonds.Count; }

    // Amount of 'energy' the node has.
    public float calories;
	public float caloriesDelta;
	
	// Attributes
	public float signal;
	public float synapse;
    public float thrust;

    float viewAngle = 25f;
	
	// Muscle attributes
	public float muscleStrength;
	public Vector3 muscleDirection;
	
    // Standard colors
    Color nodeColor = Color.clear;
	public Color stemColor = Color.gray;
	public Color senseColor = Color.cyan;
	public Color controlColor = Color.blue;
	public Color muscleColor = Color.red;

    public Assembly assembly;
    public int assemblyIndex;
	
    Color billboardColor = Color.clear;
    public Renderer billboard;
    Vector2 billboardTexScale = Vector2.one;

    float arcScale = 8f;
    public Transform arcBillboard;
    public Quaternion arcDirection = Random.rotation;


    // Prevents nodes from immediately re-bonding after being disbanded from an assembly.
    public float bondCooldown = 0;

    private static List<Node> allNodes = new List<Node>();

    private static List<Node> senseNodes = new List<Node>();
    private static List<Node> muscleNodes = new List<Node>();
    private static List<Node> controlNodes = new List<Node>();
    private static List<Node> stemNodes = new List<Node>();

    public static List<Node> GetAll() { return allNodes; }

    public static List<Node> GetAllSense() { return senseNodes; }
    public static List<Node> GetAllMuscle() { return muscleNodes; }
    public static List<Node> GetAllControl() { return controlNodes; }
    public static List<Node> GetAllStem() { return stemNodes; }

    public static Octree<Node> allNodeTree = new Octree<Node>(new Bounds(Vector3.zero, new Vector3(500, 500, 500)), (Node x) => x.transform.position, 20);
	

	void Awake(){
        calories = Random.Range(0.5f, 1.0f);
		
		// Muscle
		// Randomize the actuation direction
		muscleStrength = Random.Range(1f, 5f);
		muscleDirection = Random.rotation * Vector3.forward;

        // Store billboard scale so we can flip it.
        billboardTexScale = billboard.material.GetTextureScale("_MainTex");

        // Default as stem node.
        nodeColor = stemColor;
        stemNodes.Add(this);
        allNodes.Add(this);
        allNodeTree.Insert(this);
	} // End of Awake().


    // Creates the 'arc billboard' for view cones, etc.
    void MakeArcBillboard(){
        if(!arcBillboard){
            GameObject arcBillboardGO = new GameObject();
            arcBillboardGO.name = "ArcBillboard";
            arcBillboardGO.AddComponent<MeshFilter>();
            arcBillboardGO.GetComponent<MeshFilter>().mesh = GameManager.graphics.twoPolyPlane;
            arcBillboardGO.AddComponent<MeshRenderer>();
            arcBillboardGO.renderer.material = GameManager.graphics.senseArcMat;
            arcBillboard = arcBillboardGO.transform;
            arcBillboard.position = transform.position;
            arcBillboard.localScale = Vector3.one * arcScale;
        }
    }


    // Gets rid of the 'arc billboard.'
    void RemoveArcBillboard(){
        if(arcBillboard){
            Destroy(arcBillboard.gameObject);
            arcBillboard = null;
        }
    }


    // Removes the node from its type-specific array.
    void RemoveFromSpecificList(){
        switch(nodeType){
            case NodeType.sense :
                senseNodes.Remove(this);
                break;
            case NodeType.muscle :
                muscleNodes.Remove(this);
                break;
            case NodeType.control :
                controlNodes.Remove(this);
                break;
            case NodeType.stem :
                stemNodes.Remove(this);
                break;
        }
    }

	
    // Returns true if the type of the node changes.
	bool BasicUpdate(){
        bool nodeTypeChanged = false;
        billboard.material.SetColor("_TintColor", billboardColor);


        // Natural calorie burn
        if(assembly != null){
            calories = assembly.calories / assembly.nodes.Count;
            assembly.calories -= Time.deltaTime * 0.1f;
        }
        else
            calories = 1f;
        // Calories input/output
        calories += caloriesDelta * Time.deltaTime;
		caloriesDelta = 0.0f;

        // Update node type.
        if((bonds.Count == 1) && (nodeType != NodeType.sense)){
            nodeColor = senseColor;
            billboard.material.SetTexture("_MainTex", GameManager.graphics.senseFlare);

            RemoveFromSpecificList();
            nodeType = NodeType.sense;
            senseNodes.Add(this);
            nodeTypeChanged = true;
        }
        else if((bonds.Count == 2) && (nodeType != NodeType.muscle)){
			nodeColor = muscleColor;
            billboard.material.SetTexture("_MainTex", GameManager.graphics.muscleFlare);

            RemoveFromSpecificList();
            nodeType = NodeType.muscle;
            muscleNodes.Add(this);
            nodeTypeChanged = true;
        }
        else if((bonds.Count == 3) && (nodeType != NodeType.control)){
            nodeColor = controlColor;
            billboard.material.SetTexture("_MainTex", GameManager.graphics.controlFlare);

            RemoveFromSpecificList();
            nodeType = NodeType.control;
            controlNodes.Add(this);
            nodeTypeChanged = true;
        }
        else if(((bonds.Count == 0) || (bonds.Count > 3)) && (nodeType != NodeType.stem)){
			nodeColor = stemColor;

            RemoveFromSpecificList();
            nodeType = NodeType.stem;
            stemNodes.Add(this);
            nodeTypeChanged = true;
        }


        // Update arc rotation and such.
        if(arcBillboard){
            billboard.material.SetColor("_TintColor", billboardColor);

            // The following code billboards the arc with the main camera.
            arcBillboard.rotation = arcDirection;
            arcBillboard.position = transform.position + (arcDirection * (Vector3.forward * (0.5f * arcScale)));
            arcBillboard.rotation *= Quaternion.AngleAxis(90, Vector3.up);

            Vector3 camRelativePos = arcBillboard.InverseTransformPoint(Camera.main.transform.position);
            float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;

            arcBillboard.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);

            if(nodeType != NodeType.sense)
                RemoveArcBillboard();
        }


        // Show 'health' of the node based on calories.
        renderer.material.color = Color.Lerp(Color.black, nodeColor, calories);

        // TEMP
        // Puts a limit on node speed.
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 10f);

        // Washing machine effect
        rigidbody.AddForce(Vector3.Cross(transform.position, Vector3.up));
        rigidbody.AddForce(-transform.position * 0.1f);

        return nodeTypeChanged;
    } // End of BasicUpdate().


    public void SenseUpdate(){
        if(BasicUpdate())
            return;

        Vector3 viewDirection = Vector3.zero;
        if(bonds[0].nodeA == this)
            viewDirection = -bonds[0].dirAtoB;
        else
            viewDirection = bonds[0].dirAtoB;

        signal = 0f;
        if(bonds[0].bondType == BondType.signal){

            for(int i = 0; i < FoodPellet.GetAll().Count; i++){
                FoodPellet currentPellet = FoodPellet.GetAll()[i];
                assembly.calories += (3f / Vector3.Distance(transform.position, currentPellet.transform.position)) * Time.deltaTime * 50f;
                if(Vector3.Angle(viewDirection, currentPellet.transform.position - transform.position) < viewAngle)
                    signal += 3f / (Vector3.Distance(transform.position, currentPellet.transform.position) * 0.2f);
            }

            if(!arcBillboard)
                MakeArcBillboard();
        }
        else{
            if(arcBillboard)
                RemoveArcBillboard();
        }

        billboardColor = renderer.material.color;
        billboardColor.a = signal;

        // Sense node cone of vision is opposite to bond direction.
        arcDirection = Quaternion.LookRotation(viewDirection);
    }


    public void ControlUpdate(){
        if(BasicUpdate())
            return;

        billboardColor = renderer.material.color;

        if(synapse > 0){
            billboard.material.SetTextureScale("_MainTex", billboardTexScale);
        }
        // Invert billboard color and texture scale if synapse is negative.
        else{
            billboard.material.SetTextureScale("_MainTex", billboardTexScale * -1f);
        }
        billboardColor.a = synapse;
        synapse *= 0.9f;
    }


    public void MuscleUpdate(){
        if(BasicUpdate())
            return;

        billboardColor = renderer.material.color;
        billboardColor.a = thrust;
        thrust *= 0.9f;
    }


    public void StemUpdate(){
        bondCooldown -= Time.deltaTime;

        // TEMP
        if (!GameManager.inst.useOctree) {
            for (int i = 0; i < allNodes.Count; i++)
                AddForceToOtherNode(allNodes[i]);
        }
        else {
            Bounds boundary = new Bounds(this.transform.position, GameManager.inst.minDistForStemAttraction * (new Vector3(1,1,1)));
            allNodeTree.RunActionInRange(new System.Action<Node>(AddForceToOtherNode), boundary);
        }

        billboardColor = Color.clear;
        BasicUpdate();
    }


    private void AddForceToOtherNode(Node otherNode) {
        float minSqDistForAttraction = GameManager.inst.minDistForStemAttraction * GameManager.inst.minDistForStemAttraction; // 20 * 20
        if ((otherNode != this) && (assembly == null) && (otherNode.bonds.Count < 3) && (otherNode.transform.position - transform.position).sqrMagnitude < minSqDistForAttraction) {

            // Attractive force		
            // DEBUG - attraction is a constant.
            Vector3 vectorAtoB = otherNode.transform.position - transform.position;
            float sqrDistToNode = vectorAtoB.sqrMagnitude;
            float attraction = 6f / sqrDistToNode;
            rigidbody.AddForce(vectorAtoB.normalized * attraction);

            // Attach if close enough.
            if (sqrDistToNode <= 4f) {
                new Bond(this, otherNode);
            }
        }
    }



    public string GetDNAInfo() {
        string dnaInfo = "";
        dnaInfo += nodeType.ToString()[0];
        return dnaInfo;
    }


    public void Destroy(){
        RemoveFromSpecificList();
        allNodes.Remove(this);
        if (!allNodeTree.Remove(this)) {
            allNodeTree.Maintain();
            if (!allNodeTree.Remove(this))
                Debug.LogError("Failed to remove Node");
        }
        if(assembly != null)
            assembly.nodes.Remove(this);
        DestroyBonds();
        if(arcBillboard)
            GameObject.Destroy(arcBillboard.gameObject);

        Destroy(gameObject);
    } // End of DestroyNode().

    public void DestroyBonds() {
        for(int i=0; i < bonds.Count; ++i)
            bonds[i].Destroy();
    }


    public bool BondedTo(Node otherNode){
        return GetBondTo(otherNode) != null;
    }


    public Bond GetBondTo(Node otherNode){
        for(int j = 0; j < bonds.Count; j++){
            Bond currentBond = bonds[j];
            if(((currentBond.nodeA == otherNode) || (currentBond.nodeB == otherNode)) && (otherNode != this))
                return currentBond;
        }
        return null;
    }


    // This count includes the this node
    public int NumNodesAttached() {
        HashSet<Node> visited = new HashSet<Node>();
        // dfs - depth first search
        NodesAttached(visited);
        return visited.Count;
    }


    // This includes the this node
    public HashSet<Node> GetNodesAttached() {
        HashSet<Node> visited = new HashSet<Node>();
        NodesAttached(visited);
        return visited;
    }


    // This includes the this node
    private void NodesAttached(HashSet<Node> visited) {
        visited.Add(this);
        for (int i = 0; i < bonds.Count; ++i) {
            if( !visited.Contains(bonds[i].nodeA) )
                bonds[i].nodeA.NodesAttached(visited);
            if( !visited.Contains(bonds[i].nodeB) )
                bonds[i].nodeB.NodesAttached(visited);
        }
    }
} // End of Node.