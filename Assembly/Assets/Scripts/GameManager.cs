using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour {
	
    // Up-to-date arrays of all world elements.
	public static FoodPellet[] allFoodPellets;
	
    public static Prefabs prefabs; // Contains prefab transforms.
    public static GraphicsManager graphics; // Contains graphical information.
	
	public static GUISkin readoutSkin;

    public TextAsset namesText;
    public static string[] names;

    public int numNodes = 30;
    public float nodeGenSpread = 30; // How far the nodes will be initially scattered.
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

        // Delete all elements if 'L' pressed...
        if(Input.GetKeyDown(KeyCode.L)){
            while (Node.GetAll().Count > 0)
                Node.GetAll()[Node.GetAll().Count - 1].Destroy();
            while (Bond.GetAll().Count > 0)
                Bond.GetAll()[Bond.GetAll().Count - 1].Destroy();
            while (Assembly.GetAll().Count > 0)
                Assembly.GetAll()[Assembly.GetAll().Count - 1].Destroy();
            foreach(FoodPellet aPellet in allFoodPellets)
                aPellet.Destroy();
        }

        // Update() functions in abstract classes.
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly.GetAll()[i].Update();
        }
        for(int i = 0; i < Bond.GetAll().Count; i++){
            Bond.GetAll()[i].Update();
        }



        // Add more elements if 'g' is pressed..
        if(Input.GetKeyDown(KeyCode.G)){
            Start();
        }

	} // End of Update().


    void OnGUI(){
		GUI.skin = readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        // Header information.
		string readout = "";
        readout += "'Assembly'\n";
		readout += "UCSD Arthur C. Clarke Center 2013\n";
		readout += Node.GetAll().Count + " nodes\n";
		readout += Bond.GetAll().Count+ " bonds\n";
		readout += "\n";
		readout += (1.0f / Time.deltaTime).ToString("F1") + "fps\n";
		GUI.Label( new Rect( 5, 0, Screen.width, Screen.height ), readout );
	} // End of OnGUI().


    // Create an assembly from a file.
    public static Assembly GetAssembly(string path) {
        // Create the new assembly.
        // Prepare an array to load up with nodes.
        List<Node> newNodes = new List<Node>();

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
            newNodes.Add(newNode);
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

        return (newNodes.Count == 0) ? null : new Assembly(newNodes[0]);
    } // End of GetAssembly().
} // End of GameManager.
