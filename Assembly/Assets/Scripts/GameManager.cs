using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public static float simStep = 0.05f;


    

    public static GameManager Inst = null;

    Node selectedNode = null;

    public bool showControls = false;

    public int numFoodPellets = 25;
    public int minNumAssemblies = 60;
    public int maxNumAssemblies = 100;
    public int minNumNodes = 5;
    public int maxNumNodes = 15;
    //public bool refactorIfInert = false;
    public bool populationControl  = false;
    public bool showAssemReadouts = true;

    public float targetTimeScale = 1f;

    public float fade = 1f;
    public float initialFadeIn = 1f;

    public float worldSize = 100f;

    public bool pauseMenu = false;

    public float deltaRealTime = 0f;

    public GuiKnob timeScaleKnob = null;
    public GuiKnob minNumAssemKnob = null;
    public GuiKnob maxNumAssemKnob = null;
    public GuiKnob numFoodPelletsKnob = null;
    public GuiKnob minNodesKnob = null;
    public GuiKnob maxNodesKnob = null;
    public GuiKnob consumeRateKnob = null;
    public GuiKnob burnRateKnob = null;

    float controlRingAngleMod = 0f;
    float controlRingAngleModVel = 0f;
    float controlRingFade = 0f;


    void Awake(){
        Inst = this;
        PersistentGameManager.Inst.Touch();

        if(Application.platform == RuntimePlatform.Android){
            maxNumAssemKnob.initialValue = 20;
            minNumAssemKnob.initialValue = 15;
        }
    }

	void Start(){

        //if (BatchModeManager.Inst.InBatchMode)
        //    return;

	} // End of Start().


    void LateUpdate(){
        deltaRealTime = Time.deltaTime / Time.timeScale;
        
        if(Input.GetKeyDown(KeyCode.Escape))
            pauseMenu = !pauseMenu;

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
        //Assembly.REFACTOR_IF_INERT = refactorIfInert;
        FoodPellet.MAX_FOOD = numFoodPellets;

        //adjust food on slider
        if( FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD )
            while(FoodPellet.GetAll().Count < FoodPellet.MAX_FOOD){
                FoodPellet newPellet = FoodPellet.AddNewFoodPellet();

                /*
                // Cool spiral
                float spiralDensity = 0.05f;
                float foodSpread = 300f;
                float xPos = UnityEngine.Random.Range(0f, UnityEngine.Random.Range(-foodSpread, foodSpread));
                float spiralSize = 50f * (1f + (Mathf.Abs(xPos) * 0.01f));
                newPellet.worldPosition = new Vector3(Mathf.Cos(xPos * spiralDensity) * spiralSize, xPos, Mathf.Sin(xPos * spiralDensity) * spiralSize);
                */
                newPellet.worldPosition = MathUtilities.RandomVector3Cube(worldSize);
                UnityEngine.Object lightEffect = Instantiate(PrefabManager.Inst.newPelletBurst, newPellet.worldPosition, Quaternion.identity);

                //destroy effect after 1.5 sec
                UnityEngine.Object.Destroy(lightEffect, 1.5F);
            }
        else if( FoodPellet.GetAll().Count > FoodPellet.MAX_FOOD )
            for(int i = FoodPellet.GetAll().Count - 1; i >= FoodPellet.MAX_FOOD; --i)
                FoodPellet.GetAll()[i].Destroy();

        if(populationControl){
            //adjust assemblies on slider
            if( Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY ){
                while(Assembly.GetAll().Count < Assembly.MIN_ASSEMBLY){
                    SeedNewRandomAssembly();
                }
            }
            else if( Assembly.GetAll().Count > Assembly.MAX_ASSEMBLY ){
                for(int i = 0; i < (Assembly.GetAll().Count - Assembly.MAX_ASSEMBLY); i++){
                    Assembly lowestHealthAssembly = null;
                    float lowestHealth = Mathf.Infinity;

                    for(int j = Assembly.GetAll().Count - 1; j >= 0; --j){
                        Assembly currentAssembly = Assembly.GetAll()[j];
                        if(currentAssembly.Health < lowestHealth){
                            lowestHealthAssembly = currentAssembly;
                            lowestHealth = currentAssembly.Health;
                        }
                    }
                    if(lowestHealthAssembly)
                        lowestHealthAssembly.Destroy();
                }
            }
        }


        // Update assemblies.
        for(int i = Assembly.GetAll().Count - 1; i >= 0; i--){
            if(i > (Assembly.GetAll().Count - 1))
                continue;

            Assembly.GetAll()[i].Update();

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
            Node.GetAll()[i].Update();
        }

        // Update foodpellets.
        for(int i = 0; i < FoodPellet.GetAll().Count; ++i)
            FoodPellet.GetAll()[i].Update();
        
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


        LevelManager.InputHandler();

        if(Application.platform == RuntimePlatform.Android){
            // Quit on back button.
            if(Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }
    } // End of Update().


    public Assembly SeedNewRandomAssembly(){
        Assembly newAssembly = Assembly.GetRandomAssembly(UnityEngine.Random.Range(Assembly.MIN_NODES_IN_ASSEMBLY, Assembly.MAX_NODES_IN_ASSEMBLY));
        newAssembly.WorldPosition = MathUtilities.RandomVector3Cube(worldSize);
        //Instantiate(PrefabManager.Inst.newPelletBurst, newAssembly.WorldPosition, Quaternion.identity);
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


        targetTimeScale = timeScaleKnob.Value;
        minNumAssemblies = (int)minNumAssemKnob.Value;
        maxNumAssemblies = (int)maxNumAssemKnob.Value;
        numFoodPellets = (int)numFoodPelletsKnob.Value;
        minNumNodes = (int)minNodesKnob.Value;
        maxNumNodes = (int)maxNodesKnob.Value;
        Assembly.burnCoefficient = burnRateKnob.Value;
        SenseNode.consumeRate = consumeRateKnob.Value;

        if((Application.platform == RuntimePlatform.Android) || (Application.platform == RuntimePlatform.IPhonePlayer)){
            minNumAssemKnob.maxValue = 50f;
            maxNumAssemKnob.maxValue = 50f;
            minNodesKnob.maxValue = 25f;
            maxNodesKnob.maxValue = 25f;
            numFoodPelletsKnob.maxValue = 30f;
        }
        

        
        float ringRadius = 250f;
        float ringAngleRatio = 0.25f;

        controlRingAngleMod = Mathf.SmoothDamp(controlRingAngleMod, (Mathf.PI * 2f) - (((Screen.width * 0.5f) - Input.mousePosition.x) * 0.0045f), ref controlRingAngleModVel, 0.2f);
        float controlRingAngle = 0f + controlRingAngleMod;

        Vector2 circleCenter = new Vector2(Screen.width * 0.5f, Screen.height - (Screen.height * 0.225f));
        bool controlBeingUsed = false;
        for(int i = 0; i < 10; i++){
            GuiKnob currentKnob = null;

            switch(i){
                case 0 :
                    currentKnob = timeScaleKnob;
                    break;
                case 1 : 
                    currentKnob = minNumAssemKnob;
                    break;
                case 2 : 
                    currentKnob = maxNumAssemKnob;
                    break;
                case 3 : 
                    currentKnob = numFoodPelletsKnob;
                    break;
                case 4 : 
                    currentKnob = minNodesKnob;
                    break;
                case 5 : 
                    currentKnob = maxNodesKnob;
                    break;
                case 6 : 
                    currentKnob = consumeRateKnob;
                    break;
                case 7 : 
                    currentKnob = burnRateKnob;
                    break;
            };


            if(currentKnob != null){
                // Auto-rotate ring on desktop systems
                if(Application.platform != RuntimePlatform.Android){
                    currentKnob.pxlPos = circleCenter + new Vector2(Mathf.Cos(controlRingAngle) * ringRadius, Mathf.Sin(controlRingAngle) * ringRadius * ringAngleRatio);
                    float closeness = 0.5f + (Mathf.Cos(controlRingAngle - (Mathf.PI * 0.5f)) * 0.5f);
                    currentKnob.scale = 0.25f + (closeness * 0.7f);
                    currentKnob.alpha = closeness * controlRingFade;
                }
                // Static controls on handheld systems
                else{
                    currentKnob.pxlPos = new Vector2(150f + (i * 220f), Screen.height - 150f);
                    currentKnob.scale = 2f;
                }


                if(currentKnob.clicked)
                    controlBeingUsed = true;
                
                currentKnob.Draw();

            }
            controlRingAngle += Mathf.PI * 0.17f;

        }

        Rect controlFadeInRect = new Rect(circleCenter.x - (ringRadius * 1.5f), circleCenter.y - (ringRadius * 1.5f * ringAngleRatio), ringRadius * 3f, ringRadius * 3f * ringAngleRatio);
        float ringFadeSpeed = 2f;

        // Menu always visible if on handheld
        if(Application.platform == RuntimePlatform.Android)
            controlRingFade = 1f;
        // Otherwise only fades in if mouse is nearby.
        else if(!Input.GetMouseButton(1) && (controlBeingUsed || controlFadeInRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)))){
            controlRingFade = Mathf.MoveTowards(controlRingFade, 1f, Time.deltaTime * ringFadeSpeed);
        }
        else
            controlRingFade = Mathf.MoveTowards(controlRingFade, 0f, Time.deltaTime * ringFadeSpeed);



        if(showControls){
            /*
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

            GUI.Label(controlGuiRect, "Burn Rate Multiplier: " + Assembly.burnCoefficient.ToString("F2")  );
            controlGuiRect.y += guiHeight;
            Assembly.burnCoefficient = GUI.HorizontalSlider(controlGuiRect, Assembly.burnCoefficient, 0.0F, 10.0F);
            controlGuiRect.y += guiHeight;





            

            GUI.Label(controlGuiRect, "Time scale: " + targetTimeScale.ToString("F2")   );
            controlGuiRect.y += guiHeight;
            targetTimeScale = GUI.HorizontalSlider(controlGuiRect, targetTimeScale, 0.05F, 1F);
            controlGuiRect.y += guiHeight;

            showAssemReadouts = GUI.Toggle(controlGuiRect, showAssemReadouts, "Show Assembly Info");
            controlGuiRect.y += guiHeight;

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

            GUI.Label(controlGuiRect, "Adjust Detection Range: " + SenseNode.detectRange);
            controlGuiRect.y += guiHeight;
            SenseNode.detectRange = GUI.HorizontalSlider(controlGuiRect, SenseNode.detectRange, 10F, 100F);
            controlGuiRect.y += guiHeight;
            if(SenseNode.detectRange < SenseNode.consumeRange)
                SenseNode.consumeRange = SenseNode.detectRange;
            GUI.Label(controlGuiRect, "Adjust Consume Range: " + SenseNode.consumeRange);
            controlGuiRect.y += guiHeight;
            SenseNode.consumeRange = GUI.HorizontalSlider(controlGuiRect, SenseNode.consumeRange, 10F, 100F);
            if( SenseNode.consumeRange > SenseNode.detectRange)
                SenseNode.detectRange = SenseNode.consumeRange;
            controlGuiRect.y += guiHeight;
            GUI.Label(controlGuiRect, "Adjust Consume Rate: " + SenseNode.consumeRate);
            controlGuiRect.y += guiHeight;
            SenseNode.consumeRate = GUI.HorizontalSlider(controlGuiRect, SenseNode.consumeRate, 5F, 20F);
            controlGuiRect.y += guiHeight;
            */
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


    void OnDestroy()
    {
        // Destroy all assemblies
        for (int i = Assembly.GetAll().Count - 1; i >= 0; --i)
            Assembly.GetAll()[i].Destroy();

        // Clear all static data structures
        Node.GetAll().Clear();
        Assembly.GetAll().Clear();
        FoodPellet.GetAll().Clear();
        PersistentGameManager.CaptureObjects.Clear();
        Inst = null;
    }

}