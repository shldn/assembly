using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public enum LifeStage { connect, function, reproduce, die }

public class Assembly {

    private static List<Assembly> allAssemblies = new List<Assembly>();
    public static List<Assembly> GetAll() { return allAssemblies; }

    public string name;
    public Node[] nodes;

    public LifeStage lifeStage = LifeStage.connect;
    public float age = 0f;

    // Creates a new assembly off of a root node.
    public Assembly(Node rootNode){
        Node[] gotNodes = new Node[1] {rootNode};
        Node[] newNodes = gotNodes;

        int nodeNum = 0;

        // First pass; finds all nodes through their bonds.
        // Go until we run out of new nodes.
        while(newNodes.Length > 0){
            // Run through all of our new nodes to test.
            for(int i = 0; i < newNodes.Length; i++){
                Node currentNode = newNodes[i];
                currentNode.assemblyIndex = nodeNum;
                nodeNum++;

                // Remove this node from newNodes; we're gonna check it soon and won't need it anymore.
                Node[] subbedNewNodes = new Node[newNodes.Length - 1];
                for(int h = 0; h < subbedNewNodes.Length; h++){
                    if (h < i)
                        subbedNewNodes[h] = newNodes[h];
                    else
                        subbedNewNodes[h] = newNodes[h + 1];
                }
                newNodes = subbedNewNodes;

                // Run through each new node's bonds.
                for(int j = 0; j < currentNode.bonds.Length; j++){
                    Bond currentBond = currentNode.bonds[j];
                    // We're gonna test the node at both ends; we don't know which direction the bond goes.
                    bool gotAlreadyA = false;
                    bool gotAlreadyB = false;
                    // See if we already got the node at either end.
                    for (int k = 0; k < gotNodes.Length; k++){
                        if (currentBond.nodeA == gotNodes[k])
                            gotAlreadyA = true;
                        if (currentBond.nodeB == gotNodes[k])
                            gotAlreadyB = true;
                    }

                    // Add A if it's new.
                    if(!gotAlreadyA){
                        Node[] tempGotNodes = new Node[gotNodes.Length + 1];
                        Node[] tempNewNodes = new Node[newNodes.Length + 1];
                        for (int m = 0; m < gotNodes.Length; m++)
                            tempGotNodes[m] = gotNodes[m];
                        for (int n = 0; n < newNodes.Length; n++)
                            tempNewNodes[n] = newNodes[n];
                        tempGotNodes[tempGotNodes.Length - 1] = currentBond.nodeA;
                        tempNewNodes[tempNewNodes.Length - 1] = currentBond.nodeA;
                        gotNodes = tempGotNodes;
                        newNodes = tempNewNodes;
                    }

                    // Add B if it's new.
                    if(!gotAlreadyB){
                        Node[] tempGotNodes = new Node[gotNodes.Length + 1];
                        Node[] tempNewNodes = new Node[newNodes.Length + 1];
                        for (int r = 0; r < gotNodes.Length; r++)
                            tempGotNodes[r] = gotNodes[r];
                        for (int s = 0; s < newNodes.Length; s++)
                            tempNewNodes[s] = newNodes[s];
                        tempGotNodes[tempGotNodes.Length - 1] = currentBond.nodeB;
                        tempNewNodes[tempNewNodes.Length - 1] = currentBond.nodeB;
                        gotNodes = tempGotNodes;
                        newNodes = tempNewNodes;
                    }
                }
            }
        }

        // Assign node indices
        for(int i = 0; i < gotNodes.Length; i++)
            gotNodes[i].assemblyIndex = i;

        // Create the new assembly.
        nodes = gotNodes;
        name = GameManager.names[Random.Range(0, GameManager.names.Length)].Trim();

        // Assign assembly to all nodes.
        for(int i = 0; i < gotNodes.Length; i++)
            gotNodes[i].myAssembly = this;

        allAssemblies.Add(this);
    } // End of Assembly() constructor.


    // Constructs a new empty assembly.
    public Assembly(){
        allAssemblies.Add(this);
    } // End of Assembly() constructor.


    public void Update(){
        age += Time.deltaTime;

        // Repulsive force between nodes within assembly.
        for( int i = 0; i < nodes.Length; i++ ){
			Node currentNode = nodes[i];
			
			// Kinetic nteraction with other nodes...
			for( int j = (i + 1); j < nodes.Length; j++ ){
				Node otherNode = nodes[j];

                Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
				float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
				
				// Repulsive force
				Vector3 repulsiveForce = 1000 * ( -vectorToNode / Mathf.Pow( distToNode, 5 ));

				currentNode.rigidbody.AddForce(repulsiveForce);
				otherNode.rigidbody.AddForce(-repulsiveForce);
            }
        }
    } // End of Update().


    // Returns the average position of all nodes.
    public Vector3 GetCenter(){
        Vector3 totalPos = Vector3.zero;
        for(int i = 0; i < nodes.Length; i++) {
            totalPos += nodes[i].transform.position;
        }
        totalPos /= nodes.Length;
        return totalPos;
    } // End of GetCenter().


    // Returns the greatest distance between the center and a node.
    public float GetRadius(){
        float greatestRad = 0;
        Vector3 center = GetCenter();
        for(int i = 0; i < nodes.Length; i++){
            float radius = Vector3.Distance(center, nodes[i].transform.position);
            if(radius > greatestRad)
                greatestRad = radius;
        }
        return greatestRad;
    } // End of GetRadius().


    // Save assembly to a file. Returns the filepath.
    public string Save(){        
        DirectoryInfo dir = new DirectoryInfo("C:/Assembly/saves");
        FileInfo[] info = dir.GetFiles("*.*");
        int lastFileNum = 0;
        for (int t = 0; t < info.Length; t++){
            FileInfo currentFile = info[t];
            int currentFileNum = int.Parse(currentFile.Name.Substring(0, 3));
            if (currentFileNum >= lastFileNum)
                lastFileNum = currentFileNum;
        }
        string filename = "C:/Assembly/saves/" + (lastFileNum + 1).ToString("000") + "_" + name + ".txt";
        Debug.Log("Saving " + filename);
        System.IO.File.WriteAllText(filename, GetDNA());
        return filename;
    } // End of Save().


    private string GetDNA(){
        string newDNA = "";
        for (int i = 0; i < nodes.Length; i++){
            Node currentNode = nodes[i];
            if (currentNode.bonds.Length == 0) {
                Debug.LogError("Node in assembly has no bonds!");
                continue;
            }
            // First part of the DNA is the node's index and node characteristics.
            newDNA += i + "_" + currentNode.GetDNAInfo();

            for (int j = 0; j < currentNode.bonds.Length; j++){
                Bond currentBond = currentNode.bonds[j];
                if (currentBond.nodeA == currentNode)
                    newDNA += "-" + currentBond.nodeB.assemblyIndex;
            }

            // This node is done, indicated by ','
            newDNA += ",";
        }
        return newDNA;
    } // End of GetDNA().


    // Combines two Assemblies into one.
    public Assembly Merge(Assembly mergingAssembly){
        Assembly mergedAssembly = new Assembly(nodes[0]);

        // Combine assembly names. John + Abbey = Jobey, etc.
        mergedAssembly.name = name.Substring(0, Mathf.RoundToInt(name.Length * 0.5f)) + mergingAssembly.name.Substring(0, Mathf.RoundToInt(mergingAssembly.name.Length * 0.5f));

        allAssemblies.Remove(this);
        allAssemblies.Remove(mergingAssembly);

        if((CameraControl.selectedAssembly == this) || (CameraControl.selectedAssembly == mergingAssembly))
            CameraControl.selectedAssembly = mergedAssembly;

        mergedAssembly.age = ((this.age * this.nodes.Length) + (mergingAssembly.age * mergingAssembly.nodes.Length)) / (this.nodes.Length + mergingAssembly.nodes.Length);

        return mergedAssembly;
    } // End of Merge().


    // Splits an assembly into two smaller assemblies.
    public void Split(Bond splittingBond){
        // Remove the bond and regenerate two assemblies.
    }
    

    // Attaches a new node to this assembly.
    public void AddNode(Node newNode){
        Node[] tempNodes = new Node[nodes.Length + 1];
        for(int i = 0; i < nodes.Length; i++)
            tempNodes[i] = nodes[i];
        tempNodes[nodes.Length] = newNode;
        nodes = tempNodes;

        newNode.myAssembly = this;
    } // End of AddNode().
    

    // Delete the assembly and all of its components.
    public void Destroy(){
        for(int i = 0; i < nodes.Length; i++)
            nodes[i].Destroy();
        allAssemblies.Remove(this);
    } // End of Destroy().


    // Deconstruct all bonds within the assembly.
    public void Disband(){
        for(int i = 0; i < nodes.Length; i++){
            Node currentNode = nodes[i];
            while(currentNode.bonds.Length > 0)
                currentNode.bonds[0].Destroy();
            currentNode.myAssembly = null;
        }
        allAssemblies.Remove(this);
    } // End of Disband().
} // End of Assembly.cs.
