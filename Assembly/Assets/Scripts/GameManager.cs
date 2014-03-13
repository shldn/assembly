using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    Node selectedNode = null;

    public int numberOfFood = 10;
    public int numberOfMinAssembly = 1;
    public int numberOfMaxAssembly = 10;
    public int numberOfNodesInAssembly = 10;
    public bool addMultipleAssembly = true;
    public bool addMultipleFood = true;
    public bool refactorIfInert = false;

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

        float worldSize = 100f;

        //adjust food on slider
        if( FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD )
            while(FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD){
                FoodPellet newPellet = FoodPellet.AddNewFoodPellet();
                newPellet.worldPosition = MathUtilities.RandomVector3Sphere(worldSize);
            }
        else if( FoodPellet.GetAll().Count > FoodPellet.MAX_FOOD )
            for(int i = FoodPellet.GetAll().Count - 1; i >= FoodPellet.MAX_FOOD; --i)
                FoodPellet.GetAll()[i].Destroy();

        //adjust assemblies on slider
        if( Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY )
            while(Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY){
                Assembly newAssembly = Assembly.GetRandomAssembly(Assembly.MAX_NODES_IN_ASSEMBLY);
                newAssembly.WorldPosition = MathUtilities.RandomVector3Sphere(worldSize);
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


    public static void ClearAll(){
        for (int i = Assembly.allAssemblies.Count - 1; i >= 0; i--)
            Assembly.GetAll()[i].Destroy();
        for (int i = FoodPellet.GetAll().Count - 1; i >= 0; i--)
            FoodPellet.GetAll()[i].Destroy();
    } // End of ClearAll().


    void OnGUI(){
        // World controls
        float sliderVertSpacing = 15f;
        float sliderLabelSpacing = 30f;
        Rect sliderRect = new Rect(25, 10, 250, 30);

        GUI.Label(sliderRect, "Number of Food: " + numberOfFood   );
        sliderRect.y += sliderLabelSpacing;
        numberOfFood = (int) GUI.HorizontalSlider(sliderRect, numberOfFood, 1.0F, 100.0F);
        sliderRect.y += sliderVertSpacing;
        GUI.Label(sliderRect, "Number of Min Assemblies: " + numberOfMinAssembly   );
        sliderRect.y += sliderLabelSpacing;
        numberOfMinAssembly = (int) GUI.HorizontalSlider(sliderRect, numberOfMinAssembly, 1.0F, 100.0F);
        if(numberOfMinAssembly > numberOfMaxAssembly) //check to maintain min - max
            numberOfMaxAssembly = numberOfMinAssembly;
        sliderRect.y += sliderVertSpacing;
        GUI.Label(sliderRect, "Number of Max Assemblies: " + numberOfMaxAssembly   );
        sliderRect.y += sliderLabelSpacing;
        numberOfMaxAssembly = (int) GUI.HorizontalSlider(sliderRect, numberOfMaxAssembly, 1.0F, 100.0F);
        sliderRect.y += sliderVertSpacing;
        GUI.Label(sliderRect, "Number of Nodes in Assembly: " + numberOfNodesInAssembly   );
        sliderRect.y += sliderLabelSpacing;
        numberOfNodesInAssembly = (int) GUI.HorizontalSlider(sliderRect, numberOfNodesInAssembly, 1.0F, 100.0F);
        sliderRect.y += sliderVertSpacing;
        GUI.Label(sliderRect, "Current Burn Rate multiplyer: " + Assembly.burnCoefficient   );
        sliderRect.y += sliderLabelSpacing;
        Assembly.burnCoefficient = GUI.HorizontalSlider(sliderRect, Assembly.burnCoefficient, 0.0F, 10.0F);

        sliderRect.y += sliderLabelSpacing;
        refactorIfInert = GUI.Toggle(sliderRect, refactorIfInert, " Refactor Inert Assemblies");


        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly currentAssembly = Assembly.GetAll()[i];
            Vector3 assemblyScreenPos = Camera.main.WorldToScreenPoint(Assembly.GetAll()[i].WorldPosition);

            float barWidth = 50f;
            float barHeight = 6f;
            float barSpace = 3f;

            float guiStuffY = Screen.height - assemblyScreenPos.y;

            GUI.color = new Color(1f, 1f, 1f, 0.2f);
            GUIHelper.Inst.DrawCenteredRect(assemblyScreenPos.x, guiStuffY, barWidth, barHeight);
            GUI.color = new Color(1f, 1f, 1f, 1f);
            GUIHelper.Inst.DrawCenteredFillBar(assemblyScreenPos.x, guiStuffY, barWidth, barHeight, Mathf.Clamp01(currentAssembly.Health));

            // Reproduction bar
            guiStuffY += barHeight + barSpace;

            GUI.color = new Color(0f, 1f, 0f, 0.2f);
            GUIHelper.Inst.DrawCenteredRect(assemblyScreenPos.x, guiStuffY, barWidth, barHeight);
            GUI.color = new Color(0f, 1f, 0f, 1f);
            GUIHelper.Inst.DrawCenteredFillBar(assemblyScreenPos.x, guiStuffY, barWidth, barHeight, Mathf.Clamp01(currentAssembly.Health - 1f));
        }

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