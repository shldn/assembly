using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    IntVector3 crawlerPos = IntVector3.zero;

    Node lastNode = null;

	void Start(){
        // Generate random assemblies
        int numAssemblies = 1;
        for(int i = 0; i < numAssemblies; i++){
            Assembly newAssembly = new Assembly();
            newAssembly.worldPosition = MathUtilities.RandomVector3Sphere(20f);

            //newAssembly.worldRotationVel = Random.rotation;

            // Generate a random color for the assembly.
            float colorMin = 0.3f;
            float colorMax = 0.6f;
            Color assemblyColor = new Color(Random.Range(colorMin, colorMax), Random.Range(colorMin, colorMax), Random.Range(colorMin, colorMax));
            newAssembly.color = assemblyColor;

            // Generate a random structure of nodes for the Assembly.
            crawlerPos = IntVector3.zero;
            int numNodes = 1;
            for(int j = 0; j < numNodes; j++){
                Node newNode = new Node();

                newNode.localHexPosition = crawlerPos;
                newAssembly.AddNode(newNode);

                newNode.gameObject.renderer.material.color = assemblyColor;

                crawlerPos += HexUtilities.RandomAdjacent();
            }
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

    } // End of Update().
}