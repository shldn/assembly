using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    Node selectedNode = null;

    public int numberOfFood = 10;
    public int numberOfAssembly = 1;
    public int numberOfNodesInAssembly = 10;
    public bool addMultipleAssembly = true;
    public bool addMultipleFood = true;

	void Start(){

        if (BatchModeManager.Inst.InBatchMode)
            return;

        // Generate a random assembly.
        if(addMultipleAssembly){
            Assembly.MAX_ASSEMBLY = numberOfAssembly;
            for(int i = numberOfAssembly; i > 0; --i)
                Assembly.GetRandomAssembly(numberOfNodesInAssembly);
        } else
            Assembly.GetRandomAssembly(numberOfNodesInAssembly);

        //Generate random food pellet
        if(addMultipleFood){
            FoodPellet.MAX_FOOD = numberOfFood;
            for(int i = numberOfFood; i > 0; --i)
                FoodPellet.AddRandomFoodPellet();
        } else 
            FoodPellet.AddNewFoodPellet();
        // Add a food pellet a short ways away.
        //new FoodPellet(new Vector3(10f, 0f, 0f));

	} // End of Start().


    void LateUpdate(){
        //updating values from the gui
        Assembly.MAX_ASSEMBLY = numberOfAssembly;
        Assembly.MAX_NODES_IN_ASSEMBLY = numberOfNodesInAssembly;
        FoodPellet.MAX_FOOD = numberOfFood;

        //adjust food on slider
        if( FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD )
            while(FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD){
                FoodPellet.AddNewFoodPellet();
            }
        else if( FoodPellet.GetAll().Count > FoodPellet.MAX_FOOD )
            for(int i = FoodPellet.GetAll().Count - 1; i >= FoodPellet.MAX_FOOD; --i)
                FoodPellet.GetAll()[i].Destroy();

        //adjust assemblies on slider
        if( Assembly.GetAll().Count < Assembly.MAX_ASSEMBLY )
            while(Assembly.GetAll().Count < Assembly.MAX_ASSEMBLY){
                Assembly.GetRandomAssembly(Assembly.MAX_NODES_IN_ASSEMBLY);
            }
        else if( Assembly.GetAll().Count > Assembly.MAX_ASSEMBLY )
            for(int i = Assembly.GetAll().Count - 1; i >= Assembly.MAX_NODES_IN_ASSEMBLY; --i)
                Assembly.GetAll()[i].Destroy();


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
        //sliders controlling assembly
        GUI.Label(new Rect(25, 10, 200, 30), "Number of Food: " + numberOfFood   );
        numberOfFood = (int) GUI.HorizontalSlider(new Rect(25, 40, 100, 30), numberOfFood, 1.0F, 100.0F);
        GUI.Label(new Rect(25, 70, 250, 30), "Number of Assembly: " + numberOfAssembly   );
        numberOfAssembly = (int) GUI.HorizontalSlider(new Rect(25, 100, 100, 30), numberOfAssembly, 1.0F, 100.0F);
        GUI.Label(new Rect(25, 130, 250, 30), "Number of Nodes in Assembly: " + numberOfNodesInAssembly   );
        numberOfNodesInAssembly = (int) GUI.HorizontalSlider(new Rect(25, 160, 100, 30), numberOfNodesInAssembly, 1.0F, 100.0F);
        GUI.Label(new Rect(25, 190, 250, 30), "Current Burn Rate multiplyer: " + Assembly.burnCoefficient   );
        Assembly.burnCoefficient = GUI.HorizontalSlider(new Rect(25, 220, 100, 30), Assembly.burnCoefficient, 0.0F, 10.0F);
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