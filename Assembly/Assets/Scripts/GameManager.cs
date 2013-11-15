using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour {
	
    // Up-to-date arrays of all world elements.
	public static FoodPellet[] allFoodPellets = new FoodPellet[0];
	
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

        // Create default saved assemblies directory.
        IOHelper.CreateDefaultDirectory();

        if (!System.IO.Directory.Exists("C:/Assembly"))
            System.IO.Directory.CreateDirectory("C:/Assembly");

        // Create README file.
        System.IO.File.WriteAllText("C:/Assembly/README.txt", "This is an automatically-generated directory for Assembly.\r\nUCSD Arthur C. Clarke Center 2013");

        if (BatchModeManager.Inst.InBatchMode) {
            Debug.Log("Batch Mode!");
        }
    } // End of Awake().

	void Update(){
        numFrames++;


        // Update() functions in abstract classes.
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly.GetAll()[i].Update();
        }
        for(int i = 0; i < Bond.GetAll().Count; i++){
            Bond.GetAll()[i].Update();
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


    public static void MutateAll() {
        for (int i = 0; i < Assembly.GetAll().Count; ++i)
            Assembly.GetAll()[i].Mutate();
    } // End of MutateAll


    public static void ClearAll(){
        while(Assembly.GetAll().Count > 0)
            Assembly.GetAll()[Assembly.GetAll().Count - 1].Destroy();
        while(Node.GetAll().Count > 0)
                Node.GetAll()[Node.GetAll().Count - 1].Destroy();
        while(Bond.GetAll().Count > 0)
            Bond.GetAll()[Bond.GetAll().Count - 1].Destroy();
        foreach(FoodPellet aPellet in allFoodPellets)
            aPellet.Destroy();
    }
} // End of GameManager.
