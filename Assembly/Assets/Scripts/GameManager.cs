using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public static GameManager Inst = null;

    Node selectedNode = null;

    public bool showControls = false;

    public int numFoodPellets = 25;
    public int minNumAssemblies = 60;
    public int maxNumAssemblies = 100;
    public int minNumNodes = 5;
    public int maxNumNodes = 15;
    public bool refactorIfInert = false;
    public bool populationControl  = false;
    public bool showAssemReadouts = true;

    public float targetTimeScale = 1f;

    public float fade = 1f;
    public float initialFadeIn = 1f;

    float worldSize = 100f;

    public bool pauseMenu = false;

    public float deltaRealTime = 0f;


    public GameObject pauseMenuObject = null;
    public OrbitMenuItem closeMenuOption = null;
    public OrbitMenuItem exitOption = null;
    public OrbitMenuItem optionOption = null;
    public OrbitMenuItem siteOption = null;

    void Awake(){
        Inst = this;
    }

	void Start(){

        //if (BatchModeManager.Inst.InBatchMode)
        //    return;

        if(Application.isWebPlayer)
            exitOption.gameObject.SetActive(false);

	} // End of Start().


    void LateUpdate(){
        deltaRealTime = Time.deltaTime / Time.timeScale;

        if(Input.GetKeyDown(KeyCode.Escape))
            pauseMenu = !pauseMenu;

        pauseMenuObject.SetActive(pauseMenu);

        if(closeMenuOption.down)
            pauseMenu = false;

        if(exitOption.held)
            Application.Quit();

        if(optionOption.down)
            showControls = !showControls;

        if(siteOption.held)
            Application.OpenURL("http://imagination.ucsd.edu/assembly/index.html");

        initialFadeIn = Mathf.MoveTowards(initialFadeIn, 0f, (Time.deltaTime / Time.timeScale) * 0.3f);
        
        fade = Mathf.Lerp(fade, pauseMenu? 0.8f : 0f, Time.deltaTime * 10f);

        if(initialFadeIn > fade)
            fade = initialFadeIn;


        Time.timeScale = Mathf.MoveTowards(Time.timeScale, targetTimeScale, (deltaRealTime));
        Time.fixedDeltaTime = 0.05f * Time.timeScale;

        //updating values from the gui
        Assembly.MIN_ASSEMBLY = minNumAssemblies;
        Assembly.MAX_ASSEMBLY = maxNumAssemblies;
        Assembly.MIN_NODES_IN_ASSEMBLY = minNumNodes;
        Assembly.MAX_NODES_IN_ASSEMBLY = maxNumNodes;
        Assembly.REFACTOR_IF_INERT = refactorIfInert;
        FoodPellet.MAX_FOOD = numFoodPellets;

        //adjust food on slider
        if( FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD )
            while(FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD){
                FoodPellet newPellet = FoodPellet.AddNewFoodPellet();
                newPellet.worldPosition = MathUtilities.RandomVector3Sphere(worldSize);
                UnityEngine.Object lightEffect = Instantiate(PrefabManager.Inst.reproduceBurst, newPellet.worldPosition, Quaternion.identity);

                //destroy effect after 1.5 sec
                UnityEngine.Object.Destroy(lightEffect, 1.5F);
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
                float lowestHealth = Mathf.Infinity;

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
            /*
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
            */
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

        // Game fade
        GUI.color = new Color(0f, 0f, 0f, fade);
        GUI.DrawTexture(new Rect(-10, -10, Screen.width + 10, Screen.height + 10), GUIHelper.Inst.white);
        GUI.color = Color.white;

        // World controls
        GUI.skin.label.fontSize = 12;
        GUI.skin.toggle.fontSize = 12;
        float guiHeight = 18f;
        float guiGutter = 10f;
        Rect controlGuiRect = new Rect(15, 15, 200, guiHeight);

        if(showControls){
            GUI.Label(controlGuiRect, "Number of Food Pellets: " + numFoodPellets   );
            controlGuiRect.y += guiHeight;
            numFoodPellets = (int) GUI.HorizontalSlider(controlGuiRect, numFoodPellets, 1.0F, 100.0F);
            controlGuiRect.y += guiHeight;

            GUI.Label(controlGuiRect, "Min Nodes/Assembly: " + minNumNodes   );
            controlGuiRect.y += guiHeight;
            minNumNodes = (int) GUI.HorizontalSlider(controlGuiRect, minNumNodes, 1.0F, 100.0F);
            controlGuiRect.y += guiHeight;
            if(minNumNodes > maxNumNodes) //check to maintain min - max
                maxNumNodes = minNumNodes;

            GUI.Label(controlGuiRect, "Max Nodes/Assembly: " + maxNumNodes   );
            controlGuiRect.y += guiHeight;
            maxNumNodes = (int) GUI.HorizontalSlider(controlGuiRect, maxNumNodes, 1.0F, 100.0F);
            controlGuiRect.y += guiHeight;
            if(minNumNodes > maxNumNodes) //check to maintain min - max
                minNumNodes = maxNumNodes;

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
            GUI.Label(controlGuiRect, "Min # of Assemblies: " + minNumAssemblies   );
            controlGuiRect.y += guiHeight;
            minNumAssemblies = (int) GUI.HorizontalSlider(controlGuiRect, minNumAssemblies, 1.0F, 100.0F);
            if(minNumAssemblies > maxNumAssemblies) //check to maintain min - max
                maxNumAssemblies = minNumAssemblies;
            controlGuiRect.y += guiHeight;

            GUI.Label(controlGuiRect, "Max # of Assemblies: " + maxNumAssemblies   );
            controlGuiRect.y += guiHeight;
            maxNumAssemblies = (int) GUI.HorizontalSlider(controlGuiRect, maxNumAssemblies, 1.0F, 100.0F);
            if(minNumAssemblies > maxNumAssemblies) //check to maintain min - max
                minNumAssemblies = maxNumAssemblies;
            controlGuiRect.y += guiHeight;

            controlGuiRect.y += guiGutter;
            GUI.enabled = true;

            GUI.Label(controlGuiRect, "Time scale: " + targetTimeScale.ToString("F2")   );
            controlGuiRect.y += guiHeight;
            targetTimeScale = GUI.HorizontalSlider(controlGuiRect, targetTimeScale, 0.05F, 1F);
            controlGuiRect.y += guiHeight;

            showAssemReadouts = GUI.Toggle(controlGuiRect, showAssemReadouts, "Show Assembly Info");
            controlGuiRect.y += guiHeight;

/*  changes the different types of food. taken out
            GUI.Label(controlGuiRect, "Food Property:");
            controlGuiRect.y += guiHeight;
            FoodPellet.ftDistanceEnabled = GUI.Toggle(controlGuiRect, FoodPellet.ftDistanceEnabled, " Distance");
            controlGuiRect.y += guiHeight;
            FoodPellet.ftCollisionEnabled = GUI.Toggle(controlGuiRect, FoodPellet.ftCollisionEnabled, " Collision");
            controlGuiRect.y += guiHeight;
            FoodPellet.ftPassiveEnabled = GUI.Toggle(controlGuiRect, FoodPellet.ftPassiveEnabled, " Passive");
            controlGuiRect.y += guiHeight;

            GUI.enabled = FoodPellet.ftDistanceEnabled;
            GUI.Label(controlGuiRect, "Food Consumeable Range: " + Node.consumeRange);
            controlGuiRect.y += guiHeight;
            Node.consumeRange = GUI.HorizontalSlider(controlGuiRect, Node.consumeRange, 10F, 50F);
            controlGuiRect.y += guiHeight;
            GUI.enabled = true;

            GUI.enabled = FoodPellet.ftPassiveEnabled;
            GUI.Label(controlGuiRect, "Food Passive Range: " + FoodPellet.passiveRange);
            controlGuiRect.y += guiHeight;
            FoodPellet.passiveRange = GUI.HorizontalSlider(controlGuiRect, FoodPellet.passiveRange, 10F, 50F);
            controlGuiRect.y += guiHeight;
            GUI.enabled = true;
            */

            /*GUI to control food detection and distance*/
            GUI.Label(controlGuiRect, "Adjust Detection Range: " + Node.detectRange);
            controlGuiRect.y += guiHeight;
            Node.detectRange = GUI.HorizontalSlider(controlGuiRect, Node.detectRange, 10F, 100F);
            controlGuiRect.y += guiHeight;
            if(Node.detectRange < Node.consumeRange)
                Node.consumeRange = Node.detectRange;
            GUI.Label(controlGuiRect, "Adjust Consume Range: " + Node.consumeRange);
            controlGuiRect.y += guiHeight;
            Node.consumeRange = GUI.HorizontalSlider(controlGuiRect, Node.consumeRange, 10F, 100F);
            if( Node.consumeRange > Node.detectRange)
                Node.detectRange = Node.consumeRange;
            controlGuiRect.y += guiHeight;
            GUI.Label(controlGuiRect, "Adjust Consume Rate: " + Node.consumeRate);
            controlGuiRect.y += guiHeight;
            Node.consumeRate = GUI.HorizontalSlider(controlGuiRect, Node.consumeRate, 5F, 20F);
            controlGuiRect.y += guiHeight;

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


        // Information readout
        GUI.skin.label.fontSize = 10;

        string bottomRightInfo = "\'Assembly\'" + "\n";
        bottomRightInfo += (1.0f / (Time.deltaTime / Time.timeScale)).ToString("F1") + " frames per second" + "\n";
        bottomRightInfo += "Arthur C. Clarke Center for Human Imagination" + "\n\n";
        bottomRightInfo += System.DateTime.Now + "\n";

        TimeSpan timeSpan = TimeSpan.FromSeconds(Time.time);
        string timeText = string.Format("{0:D1} hour(s), {1:D1} minute(s), and {2:D1} second(s).", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        bottomRightInfo += "Sim has been running for " + timeText;

        Rect infoReadoutRect = new Rect(-8f, -5f, Screen.width, Screen.height);
        GUI.skin.label.alignment = TextAnchor.LowerRight;
        GUI.Label(infoReadoutRect, bottomRightInfo);
        

        

    } // End of OnGUI().

}