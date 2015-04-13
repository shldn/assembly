using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

    public static AssemblyEditor Inst = null;

    public GuiKnob burnRateKnob = null;
    public GuiKnob densityKnob = null;
    public GuiKnob speedKnob = null;

    public Assembly capturedAssembly = null;
    public Node selectedNode = null;

	public bool testRunning = false;

    public bool uiLockout {get{return burnRateKnob.clicked || densityKnob.clicked || speedKnob.clicked;}}

    // Test parameters
    int numTestAssemblies = 10;
    float mutationRate = 0.25f;

	public Texture2D visionRangeIcon;
	public Texture2D visionFOVIcon;
	public Texture2D travelIcon;
	public Texture2D speedIcon;
	public Texture2D rotationIcon;
	public Texture2D undulationIcon;
	public Texture2D iqIcon;


    void Start(){
        CaptureEditorManager.ObjectCaptured += HandleObjectCaptured;
    } // End of Start().


    void Awake(){
        Inst = this;
    } // End of Awake().


	enum MenuType {
		main,
		test,
		visionRange,
		visionScope,
		maximumTravel,
		maximumSpeed,
		iq,
		rotation,
		undulation
	}
	MenuType menu = MenuType.main;


    void Update()
    {
        if (capturedAssembly)
        {
            // Update assembly.
            //capturedAssembly.Update();

            // Update nodes.
            //for (int i = 0; i < Node.GetAll().Count; ++i)
                //Node.GetAll()[i].Update();

        }
    } // End of Update().


    void OnGUI(){
		GUI.skin.label.font = PrefabManager.Inst.assemblyFont;
		GUI.skin.button.font = PrefabManager.Inst.assemblyFont;
        if (capturedAssembly){
			float controlBarWidthRatio = 0.3f;
			float gutter = Screen.height * 0.01f;
			float defaultButtonSize = Screen.height * 0.1f;
            Rect controlBarRect = new Rect(Screen.width * (1f - controlBarWidthRatio), gutter, (Screen.width * controlBarWidthRatio) - gutter, Screen.height - (gutter * 2));

            GUI.skin.button.fontSize = Mathf.CeilToInt(Screen.width * 0.03f);

			if(!testRunning){
				GUILayout.BeginArea(controlBarRect);

				if(menu == MenuType.main){

					int numOptions = 7;
					float buttonSize = (controlBarRect.height / (numOptions + 2f)) - gutter;
					Rect buttonRect = new Rect(controlBarRect.x, controlBarRect.y, controlBarRect.width, buttonSize);
					buttonRect = new Rect(controlBarRect.width * 0.5f, 0f, controlBarRect.width * 0.5f, buttonSize);

					if(GUI.Button(buttonRect, visionRangeIcon))
						menu = MenuType.visionRange;
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, visionFOVIcon))
						menu = MenuType.visionScope;
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, travelIcon))
						menu = MenuType.maximumTravel;
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, speedIcon))
						menu = MenuType.maximumSpeed;
					GUI.enabled = false;
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, rotationIcon))
						menu = MenuType.rotation;
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, undulationIcon))
						menu = MenuType.undulation;
					buttonRect.y += buttonRect.height + gutter;

					GUI.enabled = true;
					if(GUI.Button(buttonRect, iqIcon))
						menu = MenuType.iq;
					buttonRect.y += buttonRect.height + gutter;

					buttonRect.y += buttonRect.height + gutter;
					if(GUI.Button(buttonRect, "Release"))
					{
						Assembly a = CaptureEditorManager.capturedObj as Assembly;
						Network.SetSendingEnabled(0, true);
						CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
						Network.SetSendingEnabled(0, false);
						Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
						Cleanup();
					}
				}else{
					// Test details
					string title = "";
					string info = "";
					switch(menu){
						case(MenuType.visionRange):
							title = "Vision Range";
							info = " Sense nodes detect food and transmit signal based on what they detect.\n\n";
							info += " This test will attempt to improve the range of sense nodes.";
							break;
						case(MenuType.visionScope):
							title = "Vision Scope";
							info = " Sense nodes detect food and transmit signal based on what they detect.\n\n";
							info += " This test will attempt to improve the total scope of sense nodes.";
							break;
						case(MenuType.maximumTravel):
							title = "Travel";
							info = " Assembly motion is determined by the signals passed from sense nodes to muscle nodes.\n\n";
							info += " This test will attempt to improve the net distance-travel of this assembly.";
							break;
						case(MenuType.maximumSpeed):
							title = "Speed";
							info = " Assembly motion is determined by the signals passed from sense nodes to muscle nodes.\n\n";
							info += " This test will attempt to improve the overall speed of the assembly.";
							break;
						case(MenuType.iq):
							title = "Intelligence";
							info = " The behaviour of assemblies is determined by the signal pipeline of the control nodes.\n\n";
							info += " This test will attempt to improve the food-approaching behaviour of the assembly.";
							break;
						case(MenuType.rotation):
							title = "Rotation";
							info = " Assembly motion is determined by the signals passed from sense nodes to muscle nodes.\n\n";
							info += " This test will attempt to induce greater rotation in the trajectory of the assembly.";
							break;
						case(MenuType.undulation):
							title = "Undulation";
							info = " Assembly motion is determined by the signals passed from sense nodes to muscle nodes.\n\n";
							info += " This test will attempt to induce greater rotational speed overall for the assembly.";
							break;
					}

					GUI.skin.label.alignment = TextAnchor.MiddleLeft;
					GUI.skin.label.clipping = TextClipping.Overflow;
					GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.02f);
					GUILayout.Label("IMPROVE");
					GUILayout.Space(Screen.height * 0.005f);
					GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.06f);
					GUI.skin.label.fontStyle = FontStyle.Italic;
					GUILayout.Label(title);
					GUILayout.Space(Screen.height * 0.03f);
					GUI.skin.label.fontStyle = FontStyle.Normal;
					GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.03f);
					GUILayout.Label(info);

					GUILayout.FlexibleSpace();

					// Controls
					GUILayout.Label("Run Mutation:");
					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Low", GUILayout.Height(defaultButtonSize))){
						mutationRate = 0.1f;
						DoTest();
					}
					if(GUILayout.Button("High", GUILayout.Height(defaultButtonSize))){
						mutationRate = 0.25f;
						DoTest();
					}
					GUILayout.EndHorizontal();
					if(GUILayout.Button("Cancel", GUILayout.Height(defaultButtonSize))){
						menu = MenuType.main;
					}
				}

				GUILayout.EndArea();
			}
        }
    }


	void DoTest(){
		GameObject testObject;
		switch(menu){
			case(MenuType.maximumTravel):
				SpawnTestAssemblies(numTestAssemblies, mutationRate, null);
				testObject = new GameObject("maxTravelTester", typeof(Test_MaxTravel));
				testObject.transform.position = capturedAssembly.Position;
				testRunning = true;
				capturedAssembly.Destroy();
				break;
			case(MenuType.maximumSpeed):
				SpawnTestAssemblies(numTestAssemblies, mutationRate, null);
				testObject = new GameObject("maxSpeedTester", typeof(Test_MaxSpeed));
				testObject.transform.position = capturedAssembly.Position;
				testRunning = true;
				capturedAssembly.Destroy();
				break;
            case (MenuType.visionRange):
                SpawnTestAssemblies(numTestAssemblies, mutationRate, null);
                testObject = new GameObject("maxVisionRange", typeof(Test_SenseRange));
                testObject.transform.position = capturedAssembly.Position;
                testRunning = true;
                capturedAssembly.Destroy();
                break;
            case (MenuType.visionScope):
                SpawnTestAssemblies(numTestAssemblies, mutationRate, null);
                testObject = new GameObject("maxVisionScope", typeof(Test_SenseFov));
                testObject.transform.position = capturedAssembly.Position;
                testRunning = true;
                capturedAssembly.Destroy();
                break;
			case(MenuType.iq):
				SpawnTestAssemblies(numTestAssemblies, mutationRate, capturedAssembly.spawnRotation);
				testObject = new GameObject("maxIQTester", typeof(Test_IQ));
				testObject.transform.position = capturedAssembly.Position;
				testRunning = true;
				capturedAssembly.Destroy();
				break;
		}
		menu = MenuType.main;
	} // End of DoTest().


    void SpawnTestAssemblies(int num, float mutationRate, Quaternion? rot)
    {
        for (int i = 0; i < num; i++)
        {
            Assembly newPhysAssem = new Assembly(IOHelper.AssemblyToString(capturedAssembly), rot, null, false);
            newPhysAssem.Mutate(mutationRate);
        }
    }

    public void HandleObjectCaptured(object sender, System.EventArgs args)
    {
        capturedAssembly = CaptureEditorManager.capturedObj as Assembly;
        if (!capturedAssembly)
            return;

        //burnRateKnob.Value = capturedAssembly.energyBurnRate;
    }

    public void Cleanup()
    {
        CaptureEditorManager.ReleaseCaptured();
        if( capturedAssembly )
    		capturedAssembly.Destroy();
        capturedAssembly = null;
        selectedNode = null;
    }
}
