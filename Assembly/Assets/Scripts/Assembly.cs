using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public enum LifeStage { connect, function, reproduce, die }

public class Assembly {

    private static List<Assembly> allAssemblies = new List<Assembly>();
    public static List<Assembly> GetAll() { return allAssemblies; }

    public string name;
    public List<Node> nodes;

    public LifeStage lifeStage = LifeStage.connect;
    public float age = 0f;

    public float calories = 2f;

    // Creates a new assembly off of a root node.
    public Assembly(Node rootNode){
        List<Node> gotNodes = new List<Node>();
        List<Node> newNodes = new List<Node>() {rootNode};

        int nodeNum = 0;

        // First pass; finds all nodes through their bonds.
        // Go until we run out of new nodes.
        while(newNodes.Count > 0){
            // Run through all of our new nodes to test.
            for(int i = 0; i < newNodes.Count; i++){
                Node currentNode = newNodes[i];
                currentNode.assemblyIndex = nodeNum;
                nodeNum++;

                // Remove this node from newNodes; we're gonna check it soon and won't need it anymore.
                newNodes.RemoveAt(i);

                // Run through each new node's bonds.
                for(int j = 0; j < currentNode.bonds.Count; j++){
                    Bond currentBond = currentNode.bonds[j];
                    // We're gonna test the node at both ends; we don't know which direction the bond goes.
                    bool gotAlreadyA = false;
                    bool gotAlreadyB = false;
                    // See if we already got the node at either end.
                    for (int k = 0; k < gotNodes.Count; k++){
                        Node checkingNode = gotNodes[k];
                        if (currentBond.nodeA == checkingNode)
                            gotAlreadyA = true;
                        if (currentBond.nodeB == checkingNode)
                            gotAlreadyB = true;
                    }

                    // Add A if it's new.
                    if(!gotAlreadyA){
                        gotNodes.Add(currentBond.nodeA);
                        newNodes.Add(currentBond.nodeA);
                    }

                    // Add B if it's new.
                    if(!gotAlreadyB){
                        gotNodes.Add(currentBond.nodeB);
                        newNodes.Add(currentBond.nodeB);
                    }
                }
            }
        }

        // Assign node indices
        for(int i = 0; i < gotNodes.Count; i++)
            gotNodes[i].assemblyIndex = i;

        // Create the new assembly.
        nodes = gotNodes;
        name = GameManager.names[Random.Range(0, GameManager.names.Length)].Trim();

        // Assign assembly to all nodes.
        for(int i = 0; i < gotNodes.Count; i++)
            gotNodes[i].assembly = this;

        allAssemblies.Add(this);
    } // End of Assembly constructor.


    // Create an assembly from a file.
    public Assembly(string path) {
        // Create the new assembly.
        // Prepare an array to load up with nodes.
        List<Node> newNodes = new List<Node>();


        int nameSeparator = path.IndexOf("_");
        string assemblyName = path.Substring(nameSeparator + 1, (path.Length - 4) - (nameSeparator + 1));
        name = assemblyName;

        // Load individual node data, separated by ','.
        string newAssemDna = System.IO.File.ReadAllText(path);
        string[] rawNodes = newAssemDna.Split(',');

        // DNA looks like this:
        //    <index>_<type>,<index>_<type>-<bond 1>-<bond 2>, etc.

        // Last node is a 'junk' node picked up by the trailing ',' so we just delete it.
        string[] tempRawNodes = new string[rawNodes.Length - 1];
        for (int j = 0; j < (rawNodes.Length - 1); j++)
            tempRawNodes[j] = rawNodes[j];
        rawNodes = tempRawNodes;

        // First pass: Loop through all loaded nodes.
        for(int i = 0; i < rawNodes.Length; i++){

            // Create the node in the game environment (at a random position).
            float halfNodeGenSpread = 10;
            Vector3 randomPos = new Vector3(Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread));
            GameObject newNodeTrans = Object.Instantiate(GameManager.prefabs.node, randomPos, Quaternion.identity) as GameObject;
            Node newNode = newNodeTrans.GetComponent<Node>();
            newNodes.Add(newNode);
            newNode.assembly = this;
            newNode.assemblyIndex = i;
        }

        // Second pass: Loop back through all nodes to assign bonds
        for(int i = 0; i < rawNodes.Length; i++) {
            string currentNode = rawNodes[i];
            Node currentRealNode = newNodes[i];

            // Find the point at which the node's index stops being defined.
            int idIndex = currentNode.IndexOf("_");

            // Get bonds for all nodes that have not been tested..
            if (currentNode.Length > idIndex + 2){
                // Parse the bond information for the node.
                string[] rawBondNum = currentNode.Substring(idIndex + 3).Split('-');
                for (int k = 0; k < rawBondNum.Length; k++) {
                    // Create the new bond.
                    // Find the other node we are going to bond to, based on the given index.
                    Node currentBondNode = newNodes[int.Parse(rawBondNum[k])];

                    // new Bond adds itself to the Node bond lists
                    Bond newBond = new Bond(currentRealNode, currentBondNode);
                }
            }
        }

        nodes = newNodes;
        allAssemblies.Add(this);
    } // End of Assembly constructor.


    // Constructs a new empty assembly.
    public Assembly(){
        allAssemblies.Add(this);
    } // End of Assembly() constructor.


    public void Update(){
        age += Time.deltaTime;
        
        // Calorie limit is twice the number of nodes. When this point is reached, the assembly will multiply.
        calories = Mathf.Clamp(calories, 0f, nodes.Count * 2f);

        // Repulsive force between nodes within assembly.
        for( int i = 0; i < nodes.Count; i++ ){
			Node currentNode = nodes[i];
			
			// Kinetic nteraction with other nodes...
			for( int j = (i + 1); j < nodes.Count; j++ ){
				Node otherNode = nodes[j];

                Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
				float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
				
				// Repulsive force
				Vector3 repulsiveForce = 1000 * ( -vectorToNode / Mathf.Pow( distToNode, 5 ));

				currentNode.rigidbody.AddForce(repulsiveForce);
				otherNode.rigidbody.AddForce(-repulsiveForce);
            }
        }

        if(calories <= 0){
            Vector3 centerPoint = GetCenter();
            for(int i = 0; i < nodes.Count; i++){
                Vector3 vecToCenter = nodes[i].transform.position - centerPoint;
                // Bust-apart force is limited.
                nodes[i].rigidbody.AddForce((vecToCenter.normalized * (Mathf.Clamp(5f / vecToCenter.magnitude, 0f, 5f))) * 100f);

                nodes[i].bondCooldown = Random.Range(3f, 5f);
            }
            Disband();
        }
    } // End of Update().


    // Returns the average position of all nodes.
    public Vector3 GetCenter(){
        Vector3 totalPos = Vector3.zero;
        for(int i = 0; i < nodes.Count; i++) {
            totalPos += nodes[i].transform.position;
        }
        totalPos /= nodes.Count;
        return totalPos;
    } // End of GetCenter().


    // Returns the greatest distance between the center and a node.
    public float GetRadius(){
        float greatestRad = 0;
        Vector3 center = GetCenter();
        for(int i = 0; i < nodes.Count; i++){
            float radius = Vector3.Distance(center, nodes[i].transform.position);
            if(radius > greatestRad)
                greatestRad = radius;
        }
        return greatestRad;
    } // End of GetRadius().


    public void Mutate() {
        if (nodes.Count <= 2)
            return;

        // Choose some random bonds and change their destination node.
        // in this first pass it is possible for a bond to change the secondary node and then change back in another iteration.
        int attemptCount = 0; // fail safe from an infinite loop
        int numBondsToChange = Random.Range(1, 5);
        while (numBondsToChange > 0 && nodes.Count > 1 && attemptCount < (numBondsToChange + 100)) {
            ++attemptCount;
            Node node = nodes[Random.Range(0, nodes.Count)];
            int newBondBuddyIdx = (node.assemblyIndex + Random.Range(1, nodes.Count)) % nodes.Count; // make sure it doesn't bond to itself
            if (node.BondedTo(nodes[newBondBuddyIdx]) || node.bonds.Count == 0)
                continue;

            // Destroy a Random bond and create a new one in its place.
            Bond bond = node.bonds[Random.Range(0, node.bonds.Count)];

            // Make sure we don't cut a node off of the assembly by breaking it's only bond.
            Node otherNode = bond.GetOtherNode(node);
            new Bond(node, nodes[newBondBuddyIdx]);
            bond.Destroy();

            // Check if the assembly was severed, if so delete the severed nodes.
            HashSet<Node> otherAttachedNodes = otherNode.GetNodesAttached();
            if (otherAttachedNodes.Count != nodes.Count) {
                Debug.LogError("Detached from assembly: " + otherAttachedNodes.Count + " != " + (nodes.Count));
                foreach (Node n in otherAttachedNodes) {
                    n.DestroyBonds();
                    nodes.Remove(n);
                    n.Destroy();
                }
            }
            --numBondsToChange;
        }
    }

    public static void SaveAll() {
        foreach(Assembly a in GetAll())
            a.Save();
    }


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
        // Loop through all nodes in assembly.
        for (int i = 0; i < nodes.Count; i++){
            Node currentNode = nodes[i];
            if (currentNode.bonds.Count == 0) {
                Debug.LogError("Node in assembly has no bonds!");
                continue;
            }
            // First part of the DNA is the node's index and node characteristics.
            newDNA += i + "_" + currentNode.GetDNAInfo();

            for (int j = 0; j < currentNode.bonds.Count; j++){
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

        mergedAssembly.age = ((this.age * this.nodes.Count) + (mergingAssembly.age * mergingAssembly.nodes.Count)) / (this.nodes.Count + mergingAssembly.nodes.Count);

        return mergedAssembly;
    } // End of Merge().
    

    // Attaches a new node to this assembly.
    public void AddNode(Node newNode){
        nodes.Add(newNode);
        newNode.assemblyIndex = nodes.Count;
        calories += newNode.calories;

        newNode.assembly = this;
    } // End of AddNode().
    

    // Delete the assembly and all of its components.
    public void Destroy(){
        for(int i = 0; i < nodes.Count; i++)
            nodes[i].Destroy();
        allAssemblies.Remove(this);
    } // End of Destroy().


    // Deconstruct all bonds within the assembly.
    public void Disband(){
        for(int i = 0; i < nodes.Count; i++){
            Node currentNode = nodes[i];
            while(currentNode.bonds.Count > 0)
                currentNode.bonds[0].Destroy();
            currentNode.assembly = null;
        }
        allAssemblies.Remove(this);
    } // End of Disband().
} // End of Assembly.cs.
