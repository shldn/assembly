using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public static GameManager Inst = null;

    Node selectedNode = null;

    public int numberOfFood = 10;
    public int numberOfMinAssembly = 1;
    public int numberOfMaxAssembly = 10;
    public int numberOfNodesInAssembly = 10;
    public bool addMultipleAssembly = true;
    public bool addMultipleFood = true;
    public bool refactorIfInert = false;
    public bool populationControl  = false;

    float worldSize = 100f;

    void Awake(){
        Inst = this;
    }

	void Start(){

        if (BatchModeManager.Inst.InBatchMode)
            return;

        // Generate a random assembly.
        if(addMultipleAssembly){
            Assembly.MIN_ASSEMBLY = numberOfMinAssembly;
            for(int i = numberOfMinAssembly; i > 0; --i)
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
        Assembly.MIN_ASSEMBLY = numberOfMinAssembly;
        Assembly.MAX_ASSEMBLY = numberOfMaxAssembly;
        Assembly.MAX_NODES_IN_ASSEMBLY = numberOfNodesInAssembly;
        Assembly.REFACTOR_IF_INERT = refactorIfInert;
        FoodPellet.MAX_FOOD = numberOfFood;

        //adjust food on slider
        if( FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD )
            while(FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD){
                FoodPellet newPellet = FoodPellet.AddNewFoodPellet();
                newPellet.worldPosition = MathUtilities.RandomVector3Sphere(worldSize);
            }
        else if( FoodPellet.GetAll().Count > FoodPellet.MAX_FOOD )
            for(int i = FoodPellet.GetAll().Count - 1; i >= FoodPellet.MAX_FOOD; --i)
                FoodPellet.GetAll()[i].Destroy();

        if(populationControl){
            //adjust assemblies on slider
            if( Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY )
                while(Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY){
                    SeedNewRandomAssembly();
                }
            else if( Assembly.GetAll().Count > Assembly.MAX_ASSEMBLY ){
                Assembly lowestHealthAssembly = null;
                float lowestHealth = 99999f;

                for(int i = Assembly.GetAll().Count - 1; i >= 0; --i){
                    Assembly currentAssembly = Assembly.GetAll()[i];
                    if(currentAssembly.Health < lowestHealth){
                        lowestHealthAssembly = currentAssembly;
                        lowestHealth = currentAssembly.Health;
                    }
                }
                if(lowestHealthAssembly)
                    lowestHealthAssembly.Destroy();
            }
        }


        // Update assemblies.
        for(int i = Assembly.GetAll().Count - 1; i >= 0; i--){
            if(i > (Assembly.GetAll().Count - 1))
                continue;

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
        
        // Update nodes.
		for(int i = 0; i < Node.GetAll().Count; ++i){
            Node.GetAll()[i].UpdateTransform();
            Node.GetAll()[i].UpdateColor();
        }

        // Update foodpellets.
        for(int i = 0; i < FoodPellet.GetAll().Count; ++i)
            FoodPellet.GetAll()[i].UpdateTransform();

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


    public Assembly SeedNewRandomAssembly(){
        Assembly newAssembly = Assembly.GetRandomAssembly(Assembly.MAX_NODES_IN_ASSEMBLY);
        newAssembly.WorldPosition = MathUtilities.RandomVector3Sphere(worldSize);
        return newAssembly;
    }


    public static void ClearAll(){
        for (int i = Assembly.allAssemblies.Count - 1; i >= 0; i--)
            Assembly.GetAll()[i].Destroy();
        for (int i = FoodPellet.GetAll().Count - 1; i >= 0; i--)
            FoodPellet.GetAll()[i].Destroy();
    } // End of ClearAll().


    void OnGUI(){
        // World controls
        GUI.skin.label.fontSize = 12;
        GUI.skin.toggle.fontSize = 12;
        float guiHeight = 18f;
        float guiGutter = 10f;
        Rect controlGuiRect = new Rect(25, 25, 200, guiHeight);

        GUI.Label(controlGuiRect, "Number of Food Pellets: " + numberOfFood   );
        controlGuiRect.y += guiHeight;
        numberOfFood = (int) GUI.HorizontalSlider(controlGuiRect, numberOfFood, 1.0F, 100.0F);
        controlGuiRect.y += guiHeight;

        GUI.Label(controlGuiRect, "Number of Nodes in Assembly: " + numberOfNodesInAssembly   );
        controlGuiRect.y += guiHeight;
        numberOfNodesInAssembly = (int) GUI.HorizontalSlider(controlGuiRect, numberOfNodesInAssembly, 1.0F, 100.0F);
        controlGuiRect.y += guiHeight;

        GUI.Label(controlGuiRect, "Burn Rate Multiplier: " + Assembly.burnCoefficient   );
        controlGuiRect.y += guiHeight;
        Assembly.burnCoefficient = GUI.HorizontalSlider(controlGuiRect, Assembly.burnCoefficient, 0.0F, 10.0F);
        controlGuiRect.y += guiHeight;

        refactorIfInert = GUI.Toggle(controlGuiRect, refactorIfInert, " Refactor Inert Assemblies");
        controlGuiRect.y += guiHeight;

        controlGuiRect.y += guiGutter;

        // Population control
        populationControl = GUI.Toggle(controlGuiRect, populationControl, " Population Control");
        controlGuiRect.y += guiHeight;

        GUI.enabled = populationControl;
        GUI.Label(controlGuiRect, "Min # of Assemblies: " + numberOfMinAssembly   );
        controlGuiRect.y += guiHeight;
        numberOfMinAssembly = (int) GUI.HorizontalSlider(controlGuiRect, numberOfMinAssembly, 1.0F, 100.0F);
        if(numberOfMinAssembly > numberOfMaxAssembly) //check to maintain min - max
            numberOfMaxAssembly = numberOfMinAssembly;
        controlGuiRect.y += guiHeight;

        GUI.Label(controlGuiRect, "Max # of Assemblies: " + numberOfMaxAssembly   );
        controlGuiRect.y += guiHeight;
        numberOfMaxAssembly = (int) GUI.HorizontalSlider(controlGuiRect, numberOfMaxAssembly, 1.0F, 100.0F);
        if(numberOfMinAssembly > numberOfMaxAssembly) //check to maintain min - max
            numberOfMinAssembly = numberOfMaxAssembly;
        controlGuiRect.y += guiHeight;

        controlGuiRect.y += guiGutter;
        GUI.enabled = true;





        

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