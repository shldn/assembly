using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class AssemblyEditor : MonoBehaviour {

    public static AssemblyEditor Inst = null;

    public Assembly capturedAssembly = null;
    public Node selectedNode = null;

    // Test data structures
    public List<Assembly> testAssemblies = new List<Assembly>();

    // Test parameters
    public int numTestAssemblies = 10;
    public float mutationRate = 0.25f;

	public Texture2D visionRangeIcon;
	public Texture2D visionFOVIcon;
	public Texture2D travelIcon;
	public Texture2D speedIcon;
	public Texture2D rotationIcon;
	public Texture2D undulationIcon;
	public Texture2D iqIcon;

	public Texture2D helpIcon;
	public Texture2D vignette;

	public GUISkin guiSkin;

	public AudioClip buttonForwardClip;
	public AudioClip buttonBackwardClip;

    // Events
    public delegate void TestDoneHandler(AssemblyEditor sender);
    public TestDoneHandler TestDone;

    void Start(){
        CaptureEditorManager.ObjectCaptured += HandleObjectCaptured;
    } // End of Start().


    void Awake(){
        Inst = this;
    } // End of Awake().

    void OnDestroy() {
        Inst = null;
    } // End of OnDestroy()


	public enum MenuType {
		main,
		help,
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

        if (!PersistentGameManager.IsClient)
            return;

		// Vignette
		GUI.depth = 999;
		GUI.DrawTexture(new Rect(-10f, -10f, Screen.width + 20f, Screen.height + 20f), vignette);



		GUI.skin = guiSkin;

		GUI.skin.label.font = PrefabManager.Inst.assemblyFont;
		GUI.skin.button.font = PrefabManager.Inst.assemblyFont;

        GUI.skin.button.fontSize = Mathf.CeilToInt(Screen.width * 0.03f);
        GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.width * 0.03f);


		// Help menu button
        if (!ClientTest.Inst)
        {
            float helpButtonSize = Screen.height * 0.08f;
            if (GUI.Button(new Rect(10f, Screen.height - (helpButtonSize + 10f), helpButtonSize, helpButtonSize), helpIcon))
            {
                if (menu == MenuType.help)
                    menu = MenuType.main;
                else
                    menu = MenuType.help;
            }
        }

		if(menu == MenuType.help){
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUILayout.BeginArea(new Rect(0.1f * Screen.width, 0.1f * Screen.height, 0.8f * Screen.width, Screen.height));
			    GUILayout.Label("Doodle on your device while looking for your cursor on the main screen.");
                GUILayout.Label("");
			    GUILayout.Label("Draw a circle around an Assembly you wish to capture.");
                GUILayout.Label("");
			    GUILayout.Label("Once captured, use the side menu to genetically engineer your Assembly for desired traits.");
                GUILayout.Label("");
			    GUILayout.Label("Use the 'Release' button to drop it back into the environment.");
           GUILayout.EndArea();
		}
		else if(capturedAssembly){
            float controlBarWidthRatio = 0.34f;
			float gutter = Screen.height * 0.01f;
			float defaultButtonSize = Screen.height * 0.1f;
            Rect controlBarRect = new Rect(Screen.width * (1f - controlBarWidthRatio), gutter, (Screen.width * controlBarWidthRatio) - gutter, Screen.height - (gutter * 2));

			if(!ClientTest.Inst){

				GUI.skin.label.alignment = TextAnchor.UpperLeft;
				GUI.Label(new Rect(10f, 10f, Screen.width, Screen.height), CultureInfo.CurrentCulture.TextInfo.ToTitleCase(capturedAssembly.Name));

				GUILayout.BeginArea(controlBarRect);

				if(menu == MenuType.main){

					int numOptions = 6;
					float buttonSize = (controlBarRect.height / (numOptions + 2f)) - gutter;
					Rect buttonRect = new Rect(controlBarRect.x, controlBarRect.y, controlBarRect.width, buttonSize);
					buttonRect = new Rect(controlBarRect.width * 0.5f, 0f, controlBarRect.width * 0.5f, buttonSize);

					if(GUI.Button(buttonRect, visionRangeIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.visionRange;
					}
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, visionFOVIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.visionScope;
					}
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, travelIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.maximumTravel;
					}
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, speedIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.maximumSpeed;
					}
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, rotationIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.rotation;
					}
					buttonRect.y += buttonRect.height + gutter;

					if(GUI.Button(buttonRect, iqIcon)){
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						menu = MenuType.iq;
					}
					buttonRect.y += buttonRect.height + gutter;

					buttonRect.y += buttonRect.height + gutter;
					if(GUI.Button(buttonRect, "Release"))
					{
						if(PersistentGameManager.Inst.singlePlayer)
							Application.LoadLevel("SoupPhysics");

						Assembly a = CaptureEditorManager.capturedObj as Assembly;
						Network.SetSendingEnabled(0, true);
						CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
						Network.SetSendingEnabled(0, false);
						Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
						Cleanup();
					}
				}else if(menu == MenuType.help){

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
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						DoTest(menu);
					}
					if(GUILayout.Button("High", GUILayout.Height(defaultButtonSize))){
						mutationRate = 0.25f;
						AudioSource.PlayClipAtPoint(buttonForwardClip, Vector3.zero);
						DoTest(menu);
					}
					GUILayout.EndHorizontal();
					if(GUILayout.Button("Cancel", GUILayout.Height(defaultButtonSize))){
						AudioSource.PlayClipAtPoint(buttonBackwardClip, Vector3.zero);
						menu = MenuType.main;
					}
				}

				GUILayout.EndArea();
			}
        }

    } // End of OnGUI().


	public void DoTest(MenuType traitTestType){
		GameObject testObject = null;
		switch(traitTestType) {
			case(MenuType.maximumTravel):
				SpawnTestAssemblies(numTestAssemblies, mutationRate, Random.rotation);
				testObject = new GameObject("maxTravelTester", typeof(Test_MaxTravel));
				testObject.transform.position = capturedAssembly.Position;
				capturedAssembly.DestroyImmediate();
				break;
			case(MenuType.maximumSpeed):
				SpawnTestAssemblies(numTestAssemblies, mutationRate, Random.rotation);
				testObject = new GameObject("maxSpeedTester", typeof(Test_MaxSpeed));
				testObject.transform.position = capturedAssembly.Position;
				capturedAssembly.DestroyImmediate();
				break;
            case (MenuType.visionRange):
                SpawnTestAssemblies(numTestAssemblies, mutationRate, Random.rotation);
                testObject = new GameObject("maxVisionRange", typeof(Test_SenseRange));
                testObject.transform.position = capturedAssembly.Position;
                capturedAssembly.DestroyImmediate();
                break;
            case (MenuType.visionScope):
                SpawnTestAssemblies(numTestAssemblies, mutationRate, Random.rotation);
                testObject = new GameObject("maxVisionScope", typeof(Test_SenseFov));
                testObject.transform.position = capturedAssembly.Position;
                capturedAssembly.DestroyImmediate();
                break;
			case(MenuType.iq):
				//SpawnTestAssemblies(numTestAssemblies, mutationRate, capturedAssembly.spawnRotation);
				testObject = new GameObject("maxIQTester", typeof(Test_IQ));
				testObject.transform.position = capturedAssembly.Position;
				capturedAssembly.DestroyImmediate();
				break;
            case(MenuType.rotation):
                SpawnTestAssemblies(10, mutationRate, Random.rotation, 2.0f);
				testObject = new GameObject("rotationTester", typeof(Test_MaxRotation));
				testObject.transform.position = capturedAssembly.Position;
				capturedAssembly.DestroyImmediate();
                break;
		}
        if (testObject != null)
            testObject.GetComponent<ClientTest>().TestDone += OnTestDone;
        menu = MenuType.main;
	} // End of DoTest().

    void OnTestDone() {
        if (TestDone != null)
            TestDone(this);
    }

    void SpawnTestAssemblies(int num, float mutationRate, Quaternion? rot, float posOffset = 0.0f)
    {
        Vector3 initPos = capturedAssembly.Position;
        Vector3 offset = posOffset * Camera.main.transform.right;
        for (int i = 0; i < num; i++)
        {
            Assembly newPhysAssem = new Assembly(IOHelper.AssemblyToString(capturedAssembly), rot, initPos + i * offset, false);
            newPhysAssem.isTraitTest = true;
            newPhysAssem.Mutate(mutationRate);
            testAssemblies.Add(newPhysAssem);
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

} // End of AssemblyEditor.
