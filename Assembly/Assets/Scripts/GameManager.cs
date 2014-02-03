using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    Node selectedNode = null;


	void Start(){
        // Generate random assemblies
        int numAssemblies = 1;
        for(int i = 0; i < numAssemblies; i++){
            Assembly newAssembly = new Assembly();
            newAssembly.worldPosition = MathUtilities.RandomVector3Sphere(20f);

            Node seedNode = new Node();
            newAssembly.AddNode(seedNode);
            for(int j = 0; j < 10; j++)
                newAssembly.AddRandomNode();
        }

	} // End of Start().

    void Update(){

        for(int i = 0; i < Assembly.allAssemblies.Count; i++){
            // Only mutate nodes.
            if(Input.GetKeyDown(KeyCode.B)){
                for(int j = 0; j < Assembly.allAssemblies[i].nodes.Count; j++){
                    Assembly.allAssemblies[i].nodes[j].Mutate(0.1f);
                }
            }

            // Add a node.
            if(Input.GetKeyDown(KeyCode.N))
                Assembly.allAssemblies[i].AddRandomNode();

            // Remove a node.
            if(Input.GetKeyDown(KeyCode.M))
                Assembly.allAssemblies[i].RemoveRandomNode();

            if(Input.GetKeyDown(KeyCode.Space))
                Assembly.allAssemblies[i].Mutate(0.1f);

            if(Input.GetKey(KeyCode.Return))
                Assembly.allAssemblies[i].Mutate(0.01f);
            
            if(Input.GetKeyDown(KeyCode.F))
                FoodNode.AddNewFoodNode();

            Assembly.allAssemblies[i].UpdateTransform();
        }

        for(int i = 0; i < Node.allNodes.Count; i++)
            Node.allNodes[i].UpdateTransform();

        float closestDistance = 9999f;
        for(int i = 0; i < Node.allNodes.Count; i++){
            Node currentNode = Node.allNodes[i];
            float distToNode = Vector3.Distance(Camera.main.transform.position, currentNode.worldPosition);
            if(distToNode < closestDistance){
                closestDistance = distToNode;
                selectedNode = currentNode;
            }
        }

    } // End of Update().


    void OnGUI(){
        if(selectedNode){
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            Vector2 nodeScreenPos = Camera.main.WorldToScreenPoint(selectedNode.worldPosition);
            nodeScreenPos.y = Screen.height - nodeScreenPos.y;
            Rect labelRect = new Rect(nodeScreenPos.x - 150, nodeScreenPos.y - 150, 300, 300);
            GUI.skin.label.fontSize = 25;
            GUI.skin.label.fontStyle = FontStyle.Bold;
            GUI.Label(labelRect, "Root");


            // Show neighbors
            List<Node> neighbors = selectedNode.GetNeighbors();
            for(int i = 0; i < neighbors.Count; i++){
                Node currentNode = neighbors[i];
                Vector2 logNodeScreenPos = Camera.main.WorldToScreenPoint(currentNode.worldPosition);
                logNodeScreenPos.y = Screen.height - logNodeScreenPos.y;
                Rect logLabelRect = new Rect(logNodeScreenPos.x - 150, logNodeScreenPos.y - 150, 300, 300);
                GUI.skin.label.fontSize = 15;
                GUI.skin.label.fontStyle = FontStyle.Normal;
                GUI.Label(logLabelRect, "neighbor");
            }

            // Show logic connections
            List<Node> logicNodes = selectedNode.GetLogicConnections();
            for(int i = 0; i < logicNodes.Count; i++){
                Node currentNode = logicNodes[i];
                Vector2 logNodeScreenPos = Camera.main.WorldToScreenPoint(currentNode.worldPosition);
                logNodeScreenPos.y = Screen.height - logNodeScreenPos.y;
                Rect logLabelRect = new Rect(logNodeScreenPos.x - 150, logNodeScreenPos.y - 135, 300, 300);
                GUI.skin.label.fontSize = 15;
                GUI.skin.label.fontStyle = FontStyle.Bold;
                GUI.Label(logLabelRect, "logic");
            }

            
            // Show full logic net
            List<Node> logicNetNodes = selectedNode.GetFullLogicNet();
            for(int i = 0; i < logicNetNodes.Count; i++){
                Node currentNode = logicNetNodes[i];
                if(logicNodes.Contains(currentNode))
                    continue;

                Vector2 logNodeScreenPos = Camera.main.WorldToScreenPoint(currentNode.worldPosition);
                logNodeScreenPos.y = Screen.height - logNodeScreenPos.y;
                Rect logLabelRect = new Rect(logNodeScreenPos.x - 150, logNodeScreenPos.y - 135, 300, 300);
                GUI.skin.label.fontSize = 15;
                GUI.skin.label.fontStyle = FontStyle.Italic;
                GUI.Label(logLabelRect, "net");
            }
            
        }
    }

}