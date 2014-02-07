using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    Node selectedNode = null;


	void Start(){

        if (BatchModeManager.Inst.InBatchMode)
            return;

        // Generate a random assembly.
        Assembly.GetRandomAssembly(10);

        // Add a food pellet a short ways away.
        new FoodPellet(new Vector3(10f, 0f, 0f));

	} // End of Start().


    void Update(){

        // Update assemblies.
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly.GetAll()[i].UpdateTransform();

            // User input -------------------------------------
            // Mutate nodes.
            if(Input.GetKeyDown(KeyCode.B)){
                for(int j = 0; j < Assembly.GetAll()[i].nodes.Count; j++){
                    Assembly.GetAll()[i].nodes[j].Mutate(0.1f);
                }
            }

            // Add a node.
            if(Input.GetKeyDown(KeyCode.N))
                Assembly.GetAll()[i].AddRandomNode();

            // Remove a node.
            if(Input.GetKeyDown(KeyCode.M))
                Assembly.GetAll()[i].RemoveRandomNode();

            // Mutate entire assembly by 1 tick.
            if(Input.GetKeyDown(KeyCode.Space))
                Assembly.GetAll()[i].Mutate(0.1f);

            // Rapidly mutate entire assembly at full speed.
            if(Input.GetKey(KeyCode.Return))
                Assembly.GetAll()[i].Mutate(0.01f);
            // ------------------------------------------------
        }

        // Update nodes.
        for(int i = 0; i < Node.GetAll().Count; i++)
            Node.GetAll()[i].UpdateTransform();




        // Save/load
        if (Input.GetKeyUp(KeyCode.P))
            IOHelper.SaveAllToFolder("./data/" + DateTime.Now.ToString("MMddyyHHmmss") + "/");

        if (Input.GetKeyUp(KeyCode.O))
            IOHelper.LoadDirectory("./data/test/");
        if (Input.GetKeyUp(KeyCode.I))
            EnvironmentManager.Save(IOHelper.GetValidFileName("./data/", "env", ".txt"));
        if (Input.GetKeyUp(KeyCode.U))
            EnvironmentManager.Load("./data/env.txt");
        if (Input.GetKeyUp(KeyCode.L))
            ClearAll();
        if (Input.GetKeyUp(KeyCode.K))
        {
            ClearAll();
            SimulationManager.Inst.Run();
        }
        if (Input.GetKeyUp(KeyCode.J))
        {
            for (int i = 0; i < Assembly.allAssemblies.Count; ++i)
            {
                Assembly.allAssemblies[i].worldPosition.x += i * 5.0f;
                for (int j = 0; j < Assembly.allAssemblies[i].nodes.Count; ++j)
                    Assembly.allAssemblies[i].nodes[j].UpdateTransform();
            }
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

    } // End of Update().


    void ClearAll(){
        for (int i = Assembly.allAssemblies.Count - 1; i >= 0; i--)
            Assembly.GetAll()[i].Destroy();
        for (int i = FoodPellet.GetAll().Count - 1; i >= 0; i--)
            FoodPellet.GetAll()[i].Destroy();
    } // End of ClearAll().


    void OnGUI(){
        if( Assembly.GetAll().Count > 0 )
            GUI.Label(new Rect(0, 0, 200, 200), Assembly.GetAll()[0].Fitness().ToString());


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
    }

}