using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {bone, sense, control, muscle}

public class Node : MonoBehaviour {

	public string nodeName = "Node";
	public NodeType nodeType = NodeType.bone;

    public Bond[] bonds = new Bond[0];
    public int BondCount() { return bonds.Length; }

    public float calories;
	public float caloriesDelta;
	
	// Sense attributes
	public float signal;
	public float signalDecay = 1.0f;
	
	// Control attributes
	public float synapse;
	public float synapseRunner;
	float synapseFreq;
	
	// Muscle attributes
	public float muscleStrength;
	public Vector3 muscleDirection;
	
	public Color boneColor = Color.gray;
	public Color senseColor = Color.cyan;
	public Color controlColor = Color.blue;
	public Color muscleColor = Color.red;

    public Assembly myAssembly;
    public int assemblyIndex;
	
	public bool signalRelay = false;

    public Renderer billboard;
	
    Vector2 billboardTexScale = Vector2.one;

    // Prevents nodes from immediately re-bonding after being disbanded from an assembly.
    public float bondCooldown = 0;

    private static List<Node> allNodes = new List<Node>();
    public static List<Node> GetAll() { return allNodes; }
	

	void Awake(){
		nodeType = (NodeType) Random.Range(0, 4);
        calories = Random.Range(0.5f, 1.0f);
		
		// Control
		// The frequency of muscle oscillations.
		synapseFreq = Random.Range(5, 60);
		
		// Muscle
		// Randomize the actuation direction
		muscleStrength = Random.Range(1f, 5f);
		muscleDirection = Random.rotation * Vector3.forward;

        // Store billboard scale so we can flip it.
        billboardTexScale = billboard.material.GetTextureScale("_MainTex");

        allNodes.Add(this);
	} // End of Awake().

	
	void Update(){
        calories += caloriesDelta * Time.deltaTime;
		//calories = Mathf.Clamp(calories, 0.0f, 10.0f);
		caloriesDelta = 0.0f;

        // Determine node type.
        if (bonds.Length == 1)
            nodeType = NodeType.sense;
        else if (bonds.Length == 2)
            nodeType = NodeType.muscle;
        else if (bonds.Length == 3)
            nodeType = NodeType.control;
        else
            nodeType = NodeType.bone;
		
		switch(nodeType){
			case(NodeType.bone):
				renderer.material.color = boneColor;
				break;
			case(NodeType.muscle):
				renderer.material.color = muscleColor;
				break;
		}

        // Glow color
        Color glowColor = Color.clear;
        if(nodeType == NodeType.sense){
            renderer.material.color = senseColor;
            billboard.material.SetTexture("_MainTex", GameManager.graphics.senseFlare);

            glowColor = renderer.material.color;
            glowColor.a = signal;
        }
        else if(nodeType == NodeType.control){
            renderer.material.color = controlColor;
            billboard.material.SetTexture("_MainTex", GameManager.graphics.synapseFlare);

            if(synapse > 0){
                billboard.material.SetTextureScale("_MainTex", billboardTexScale);
                glowColor = renderer.material.color;
            }
            // Invert billboard color and texture scale if synapse is negative.
            else{
                billboard.material.SetTextureScale("_MainTex", billboardTexScale * -1f);
                glowColor = new Color(1f - renderer.material.color.r, 1f - renderer.material.color.g, 1f - renderer.material.color.b);
            }
            glowColor.a = Mathf.Abs(synapse);
            //glowColor.a = signal;
        }
        billboard.material.SetColor("_TintColor", glowColor);
        
		
		if(nodeType == NodeType.sense){
			renderer.material.color = Color.Lerp(renderer.material.color, Color.white * 2.0f, signal);}
		
		//signalDecay -= signal * Time.deltaTime;
		//signalDecay += 0.1f * Time.deltaTime;
		//signalDecay = Mathf.Clamp01(signalDecay);
		
		// Metabolism
        //calories -= 0.02f * Time.deltaTime;
		
		// If calories run out, node dies.
        if (calories <= 0.0f)
            Destroy();
		
        signal = 0;
		// Sense nearby food.
        if(nodeType == NodeType.sense){
		    for(int i = 0; i < FoodPellet.GetAll().Count; i++){
			    FoodPellet currentPellet = FoodPellet.GetAll()[i];
			    float distToPellet = Vector3.Distance(transform.position, currentPellet.transform.position);
			    signal += 1 / Mathf.Pow(distToPellet * 0.2f, 3);
		    }
        }
		
		// Synapse pulse
		synapseRunner += Time.deltaTime;
		synapse = Mathf.Cos(synapseRunner * synapseFreq / (2 * Mathf.PI)) * signal;
		
		if(nodeType == NodeType.sense)
			signalRelay = true;



        // TEMP
        // Un-bonded nodes are attracted to other nodes.
        if((bondCooldown <= 0f) && (bonds.Length < 3)){
            for(int i = 0; i < allNodes.Count; i++){
                Node otherNode = allNodes[i];

                if(!BondedTo(otherNode) && (otherNode != this) && (myAssembly == null || (myAssembly != otherNode.myAssembly)) && (otherNode.bonds.Length < 3)){

                    // Attractive force
		            Vector3 vectorAtoB = otherNode.transform.position - transform.position;
                    float distToNode = vectorAtoB.magnitude;
		
                    if(distToNode < 20){
                        // DEBUG - attraction is a constant.
                        float attraction = 6f / (Mathf.Pow(distToNode, 2));

                        rigidbody.AddForce(vectorAtoB.normalized * attraction);
                    }

                    // Attach if close enough.
                    if(distToNode <= 2f)
                        new Bond(this, otherNode);
                }
            }
        }

        bondCooldown -= Time.deltaTime;


        // TEMP
        // Puts a limit on node speed.
        rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 10f);
	} // End of Update().


    public string GetDNAInfo() {
        string dnaInfo = "";
        dnaInfo += nodeType.ToString()[0];
        return dnaInfo;
    }


    public void Destroy(){
        Destroy(gameObject);
        allNodes.Remove(this);
    } // End of DestroyNode().


    public bool BondedTo(Node otherNode){
        bool bondedToNode = false;
        for(int j = 0; j < bonds.Length; j++){
            Bond currentBond = bonds[j];
            if(((currentBond.nodeA == otherNode) || (currentBond.nodeB == otherNode)) && (otherNode != this))
                bondedToNode = true;
        }
        return bondedToNode;
    }

    public Bond GetBondTo(Node otherNode){
        Bond sharedBond = null;
        for(int j = 0; j < bonds.Length; j++){
            Bond currentBond = bonds[j];
            if(((currentBond.nodeA == otherNode) || (currentBond.nodeB == otherNode)) && (otherNode != this))
                sharedBond = currentBond;
        }
        return sharedBond;
    }
} // End of Node.