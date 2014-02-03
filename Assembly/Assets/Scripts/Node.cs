using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NodeType {none, sense, actuate, control}

public class Node {

    public static List<Node> allNodes = new List<Node>();

    public Vector3 worldPosition = Vector3.zero;
    public IntVector3 localHexPosition = IntVector3.zero;

    public Quaternion orientation = Quaternion.identity;
    public float fieldOfView = 45f;

    public Assembly assembly = null;
    public List<Node> bondedNodes = new List<Node>();

    public GameObject gameObject = null;
    public GameObject senseFieldBillboard = null;
    float arcScale = 5f;

    public static Color stemColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public static Color senseColor = new Color(0.64f, 0.8f, 0.44f, 1f);
    public static Color actuatorColor = new Color(0.62f, 0.18f, 0.18f, 1f);
    public static Color controlColor = new Color(0.35f, 0.59f, 0.84f, 1f);

    public NodeType nodeType = NodeType.none;


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


            Color tempColor = senseColor;

            //calling detect food on sense node
            for(int j = 0; j < FoodNode.GetAll().Count; ++j){
                bool detected = this.DetectFood(FoodNode.GetAll()[j] );
                if( detected ){
                    tempColor = Color.cyan;
                    break;
                }
            }
                               
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
    

    //take sense node to detect foodnode
    public bool DetectFood( FoodNode food){
        if(this.nodeType != NodeType.sense)
            return false;
        Vector3 viewDir = this.orientation * Vector3.forward;
        Vector3 foodDir = food.worldPosition - this.worldPosition;
        float angle = Vector3.Angle( viewDir, foodDir);
        if(angle < fieldOfView)
            return true;
        return false;
    }

} // End of Node.