using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    Node selectedNode = null;

    public int numberOfFood = 10;
    public bool addMultipleFood = true;

	void Start(){

        if (BatchModeManager.Inst.InBatchMode)
            return;

        // Generate a random assembly.
        Assembly.GetRandomAssembly(10);

        //Generate random food pellet
        if(addMultipleFood)
            for(int i = 0; i< numberOfFood; ++i)
                FoodPellet.AddRandomFoodPellet();
        else 
            FoodPellet.AddNewFoodPellet();
        // Add a food pellet a short ways away.
        //new FoodPellet(new Vector3(10f, 0f, 0f));

	} // End of Start().


    void LateUpdate(){

        // Update assemblies.
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly.GetAll()[i].UpdateTransform();

            // User input -------------------------------------
            // Mutate nodes.
            if(Input.GetKeyDown(KeyCode.B)){
                for(int j = 0; j < Assembly.GetAll()[i].nodes.Count; j++){
                    Assembly.GetAll()[i].nodes[j].Mutate(0.1f);
                }
                ConsoleScript.Inst.WriteToLog("Mutated all assembly genes.");
            }

            // Add a node.
            if(Input.GetKeyDown(KeyCode.N)){
                Assembly.GetAll()[i].AddRandomNode();
                ConsoleScript.Inst.WriteToLog("Added a random node to all assemblies.");
            }

            // Remove a node.
            if(Input.GetKeyDown(KeyCode.M)){
                Assembly.GetAll()[i].RemoveRandomNode();
                ConsoleScript.Inst.WriteToLog("Removed a random node from all assemblies.");
            }

            // Mutate entire assembly by 1 tick.
            //if(Input.GetKeyDown(KeyCode.Space))
            //    Assembly.GetAll()[i].Mutate(0.1f);

            // Rapidly mutate entire assembly at full speed.
            if(Input.GetKey(KeyCode.Return))
                Assembly.GetAll()[i].Mutate(0.01f);
            // ------------------------------------------------
        }

        //destroy assemblies out side of update
        for(int i = 0; i < Assembly.GetToDestroy().Count; ++i)
            Assembly.GetToDestroy()[i].SplitOff();
        
        // Update nodes.
		for(int i = 0; i < Node.GetAll().Count; ++i){
            Node.GetAll()[i].UpdateTransform();
            Node.GetAll()[i].UpdateColor();
        }

        // Find closest node for rendering HUD information.
        float closestDistance = 9999f;
        for(int i = 0; i < Node.GetAll().Count; i++){
            Node currentNode = Node.GetAll()[i];
            float distToNode = Vector3.Distance(Camera.main.transform.position, currentNode.worldPosition);
            if(distToNode < closestDistance){
                closestDistance = distToNode;
                selectedNode = currentNode;
            }
        }


        if(Input.GetKeyDown(KeyCode.P)){
            for(int i = 0; i < 10; i++){
                Assembly newAssembly = Assembly.GetRandomAssembly(UnityEngine.Random.Range(5, 30));
                newAssembly.physicsObject.transform.position = MathUtilities.RandomVector3Sphere(30f);
            }
            ConsoleScript.Inst.WriteToLog("Created random assemblies.");
        }


    } // End of Update().


    public static void ClearAll(){
        for (int i = Assembly.allAssemblies.Count - 1; i >= 0; i--)
            Assembly.GetAll()[i].Destroy();
        for (int i = FoodPellet.GetAll().Count - 1; i >= 0; i--)
            FoodPellet.GetAll()[i].Destroy();
    } // End of ClearAll().


    void OnGUI(){
        
        /*
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

                GUI.Label(new Rect(10, 10, 100, 100), neighbors.Count + " neighbors");
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
        */
        
    } // End of OnGUI().

}