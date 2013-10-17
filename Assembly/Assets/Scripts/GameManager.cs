using UnityEngine;
using System.Collections;
using System.IO;

// erik was here
// Wes was here
// Wes was here AGAIN
public class GameManager : MonoBehaviour {
	
    // Up-to-date arrays of all world elements.
	public static Node[] allNodes;
	public static Bond[] allBonds = new Bond[0];
	public static FoodPellet[] allFoodPellets;
    public static Assembly[] allAssemblies = new Assembly[0];
	
    public static Prefabs prefabs; // Contains prefab transforms.
    public static GraphicsManager graphics; // Contains graphical information.

	float totalEnergy; // Total energy present in the world (realistically, shouldn't change.)
	
	public static GUISkin readoutSkin;

    public TextAsset namesText;
    public static string[] names;

    public int numNodes = 30;
    public float nodeGenSpread = 30; // How far the nodes will be initially scattered.
	public float bondRatio = 0.02f; // The chance that a bond will form between any two nodes.
    public int numFoodPellets = 5;

    public int numFrames = 0;


    void Start(){
        prefabs = GetComponent<Prefabs>();
        graphics = GetComponent<GraphicsManager>();

        // Create seeding nodes.
        float halfNodeGenSpread = nodeGenSpread * 0.5f;
        for(int i = 0; i < numNodes; i++){
            Vector3 randomPos = new Vector3(Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread));
            Instantiate(prefabs.node, randomPos, Random.rotation);
        }

        // Create food pellets.
        float foodPelletGenSpread = nodeGenSpread;
        for (int i = 0; i < numFoodPellets; i++) {
            Vector3 randomPos = new Vector3(Random.Range(-foodPelletGenSpread, foodPelletGenSpread), Random.Range(-foodPelletGenSpread, foodPelletGenSpread), Random.Range(-foodPelletGenSpread, foodPelletGenSpread));
            Instantiate(prefabs.foodPellet, randomPos, Random.rotation);
        }
		
        allNodes = FindObjectsOfType(typeof(Node)) as Node[];

		// Randomly assign bonds.
		// Loop through all nodes...
		for(int i = 0; i < allNodes.Length; i++){
			Node currentNode = allNodes[i];
			// Check against all other nodes.
			for(int j = 0; j < allNodes.Length; j++){
				Node otherNode = allNodes[j];
                // Random chance of bond being created.
				if(Random.Range(0.0f, 1.0f) <= bondRatio){
                    // Make sure the bond doesn't already exist.
					bool bondExists = false;
					for(int k = 0; k < allBonds.Length; k++){
						Bond currentBond = allBonds[k];
						if(((currentBond.nodeA == currentNode) && (currentBond.nodeB == otherNode)) || ((currentBond.nodeA == otherNode) && (currentBond.nodeB == currentNode)))
							bondExists = true;
					}

                    int bondLimit = 3;
                    if (!bondExists && (i != j) && (currentNode.bonds.Length < bondLimit) && (otherNode.bonds.Length < bondLimit)) {
						GameObject newBondGameObject = Instantiate(prefabs.bond, Vector3.zero, Quaternion.identity) as GameObject;
						Bond newBond = newBondGameObject.GetComponent<Bond>();
						newBond.nodeA = currentNode;
						newBond.nodeB = otherNode;

                        // Update current node's bonds:
                        Bond[] curNewBonds = new Bond[currentNode.bonds.Length + 1];
                        for(int l = 0; l < currentNode.bonds.Length; l++){
                            curNewBonds[l] = currentNode.bonds[l];
                        }
                        curNewBonds[currentNode.bonds.Length] = newBond;
                        currentNode.bonds = curNewBonds;

                        // Update other node's bonds:
                        Bond[] othNewBonds = new Bond[otherNode.bonds.Length + 1];
                        for (int m = 0; m < otherNode.bonds.Length; m++){
                            othNewBonds[m] = otherNode.bonds[m];
                        }
                        othNewBonds[otherNode.bonds.Length] = newBond;
                        otherNode.bonds = othNewBonds;
					}
				}
			}
		}
		
        // Initialize 'names' array for random Assembly names to be pulled from.
        names = namesText.text.Split('\n');

        // Create Assembly directory.
        if (!System.IO.Directory.Exists("C:/Assembly")) {
            System.IO.Directory.CreateDirectory("C:/Assembly");
        }
        // Create saved assemblies directory.
        if (!System.IO.Directory.Exists("C:/Assembly/saves")) {
            System.IO.Directory.CreateDirectory("C:/Assembly/saves");
        }
        // Create README file.
        System.IO.File.WriteAllText("C:/Assembly/README.txt", "This is an automatically-generated directory for Assembly.\r\nUCSD Arthur C. Clarke Center 2013"); 
    } // End of Awake().
	

	void Update(){
        numFrames++;

        // Update game element arrays.
        allNodes = FindObjectsOfType(typeof(Node)) as Node[];
        allBonds = FindObjectsOfType(typeof(Bond)) as Bond[];
        allFoodPellets = FindObjectsOfType(typeof(FoodPellet)) as FoodPellet[];

        // Delete all elements if 'L' pressed...
        if(Input.GetKeyDown(KeyCode.L)){
            foreach(Node aNode in allNodes)
                aNode.Destroy();
            foreach(Bond aBond in allBonds)
                aBond.Destroy();
            foreach(FoodPellet aPellet in allFoodPellets)
                aPellet.Destroy();
        }

        // Add more elements if 'g' is pressed..
        if(Input.GetKeyDown(KeyCode.G)){
            Start();
        }

		
		totalEnergy = 0;
		// Loop through all nodes...
        // Node loop 0.
		for( int i = 0; i < allNodes.Length; i++ ){
			Node currentNode = allNodes[i];
			totalEnergy += currentNode.calories;
			
			// Kinetic nteraction with other nodes...
			for( int j = 0; j < allNodes.Length; j++ ){
				Node otherNode = allNodes[j];
				
				if( i != j ){
					Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
					float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
					
					// Repulsive force
					currentNode.rigidbody.AddForce( 1000 * ( -vectorToNode / Mathf.Pow( distToNode, 5 )));
				}
			}
		}
		 
		// Node loop 1.
		//for( int i = 0; i < allNodes.Length; i++ ){
		//	Node currentNode = allNodes[i];
		//}


        if(Input.GetKeyDown(KeyCode.P))
            GetAssembly("C:/Assembly/saves/123_Faviola.txt");
	} // End of Update().


    void OnGUI(){
		GUI.skin = readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        // Header information.
		string readout = "";
        readout += "'Assembly'\n";
		readout += "UCSD Arthur C. Clarke Center 2013\n";
		readout += "total energy in sys: " + totalEnergy + "\n";
		readout += allNodes.Length + " nodes\n";
		readout += allBonds.Length + " bonds\n";
		readout += "\n";
		readout += (1.0f / Time.deltaTime).ToString("F1") + "fps\n";
		GUI.Label( new Rect( 5, 0, Screen.width, Screen.height ), readout );
	} // End of OnGUI().


    // Creates a new assembly off of a root node and saves it to a file.
    public static Assembly GetAssembly(Node rootNode){
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

        // Second pass; write DNA, recording bond connections and information.
        string newDNA = "";
        for(int i = 0; i < gotNodes.Length; i++) {
            Node currentNode = gotNodes[i];
            currentNode.assemblyIndex = i;
            // First part of the DNA is the node's index and node characteristics.
            newDNA += i + "_" + currentNode.GetDNAInfo();

            for (int j = 0; j < currentNode.bonds.Length; j++) {
                Bond currentBond = currentNode.bonds[j];
                if (currentBond.nodeA == currentNode)
                    newDNA += "-" + currentBond.nodeB.assemblyIndex;
            }

            // This node is done, indicated by ','
            newDNA += ",";
        }

        // Create the new assembly.
        Assembly newAssembly = new Assembly();
        newAssembly.nodes = gotNodes;
        newAssembly.callsign = GameManager.names[Random.Range(0, GameManager.names.Length)].Trim();

        // Assign assembly to all nodes.
        for(int i = 0; i < gotNodes.Length; i++)
            gotNodes[i].myAssembly = newAssembly;

        // Save new assembly to a file.
        DirectoryInfo dir = new DirectoryInfo("C:/Assembly/saves");
        FileInfo[] info = dir.GetFiles("*.*");
        int lastFileNum = 0;
        for (int t = 0; t < info.Length; t++) {
            FileInfo currentFile = info[t];
            int currentFileNum = int.Parse(currentFile.Name.Substring(0,3));
            if(currentFileNum >= lastFileNum)
                lastFileNum = currentFileNum;
        }
        System.IO.File.WriteAllText("C:/Assembly/saves/" + (lastFileNum + 1).ToString("000") + "_" + newAssembly.callsign + ".txt", newDNA);
        
        return newAssembly;
    } // End of GetAssembly().
	

    // Create an assembly from a file.
    public static Assembly GetAssembly(string path) {
        // Create the new assembly.
        Assembly newAssembly = new Assembly();
        // Prepare an array to load up with nodes.
        Node[] allNodes = new Node[0];

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
            string currentNode = rawNodes[i];

            // Create the node in the game environment (at a random position).
            float halfNodeGenSpread = 10;
            Vector3 randomPos = new Vector3(Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread), Random.Range(-halfNodeGenSpread, halfNodeGenSpread));
            GameObject newNodeTrans = Instantiate(GameManager.prefabs.node, randomPos, Quaternion.identity) as GameObject;
            Node newNode = newNodeTrans.GetComponent<Node>();
            newNode.myAssembly = newAssembly;

            // Find the point at which the node's index stops being defined.
            int idIndex = currentNode.IndexOf("_");

            // Get the index of the node (the integer number before the index-stop character).
            newNode.assemblyIndex = int.Parse(currentNode.Substring(0, idIndex));

            // Add the new node to the allNodes array.
            Node[] tempAllNodes = new Node[allNodes.Length + 1];
            for(int j = 0; j < allNodes.Length; j++){
                tempAllNodes[j] = allNodes[j];
            }
            tempAllNodes[tempAllNodes.Length - 1] = newNode;
            allNodes = tempAllNodes;
        }

        // Second pass: Loop back through all nodes to assign bonds
        for(int i = 0; i < rawNodes.Length; i++) {
            string currentNode = rawNodes[i];
            Node currentRealNode = allNodes[i];

            // Find the point at which the node's index stops being defined.
            int idIndex = currentNode.IndexOf("_");

            // Get bonds for all nodes that have not been tested..
            if (currentNode.Length > idIndex + 2){
                // Parse the bond information for the node.
                string[] rawBondNum = currentNode.Substring(idIndex + 3).Split('-');
                for (int k = 0; k < rawBondNum.Length; k++) {
                    // Create the new bond.
                    GameObject newBondTrans = Instantiate(GameManager.prefabs.bond, Vector3.zero, Quaternion.identity) as GameObject;
                    Bond newBond = newBondTrans.GetComponent<Bond>();

                    // Find the other node we are going to bond to, based on the given index.
                    Node currentBondNode = allNodes[int.Parse(rawBondNum[k])];

                    // Assign both nodes to the new bond.
                    newBond.nodeA = currentRealNode;
                    newBond.nodeB = currentBondNode;

                    // Inform each node about the new bond.
                    Bond[] curRealNodeTempBonds = new Bond[currentRealNode.bonds.Length + 1];
                    for (int j = 0; j < currentRealNode.bonds.Length; j++)
                        curRealNodeTempBonds[j] = currentRealNode.bonds[j];
                    curRealNodeTempBonds[curRealNodeTempBonds.Length - 1] = newBond;
                    currentRealNode.bonds = curRealNodeTempBonds;

                    Bond[] curBondNodeTempBonds = new Bond[currentBondNode.bonds.Length + 1];
                    for (int j = 0; j < currentBondNode.bonds.Length; j++)
                        curBondNodeTempBonds[j] = currentBondNode.bonds[j];
                    curBondNodeTempBonds[curBondNodeTempBonds.Length - 1] = newBond;
                    currentBondNode.bonds = curBondNodeTempBonds;
                }
            }
        }

        return newAssembly;
    } // End of GetAssembly().
} // End of GameManager.
