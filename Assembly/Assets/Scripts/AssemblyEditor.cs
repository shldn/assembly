using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

    public static AssemblyEditor Inst = null;

    public GuiKnob burnRateKnob = null;
    public GuiKnob densityKnob = null;
    public GuiKnob speedKnob = null;

    public PhysAssembly capturedAssembly = null;
    public PhysNode selectedNode = null;

	public bool testRunning = false;

    public bool uiLockout {get{return burnRateKnob.clicked || densityKnob.clicked || speedKnob.clicked;}}

    // Test parameters
    int numTestAssemblies = 10;
    float mutationRate = 0.25f;

    void Start()
    {
        CaptureEditorManager.ObjectCaptured += HandleObjectCaptured;
    }


    void Awake(){
        Inst = this;
    } // End of Awake().


	enum MenuType {
		main,
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
    }
    void OnGUI()
    {
        if (capturedAssembly)
        {
			float controlBarWidthRatio = 0.3f;
            Rect controlBarRect = new Rect(Screen.width * (1f - controlBarWidthRatio), 0f, Screen.width * controlBarWidthRatio, Screen.height);

            GUI.skin.button.fontSize = 20;

			if(!testRunning){
				GUILayout.BeginArea(controlBarRect);

				if(menu == MenuType.main){
					if(GUILayout.Button("Vision Range", GUILayout.ExpandHeight(true)))
						menu = MenuType.visionRange;

					if(GUILayout.Button("Vision Scope", GUILayout.ExpandHeight(true)))
						menu = MenuType.visionScope;

					if(GUILayout.Button("Maximum Travel", GUILayout.ExpandHeight(true)))
						menu = MenuType.maximumTravel;

					if(GUILayout.Button("Maximum Speed", GUILayout.ExpandHeight(true)))
						menu = MenuType.maximumSpeed;

					if (GUILayout.Button("IQ", GUILayout.ExpandHeight(true)))
						menu = MenuType.iq;

					if(GUILayout.Button("Rotation", GUILayout.ExpandHeight(true))){
						// Rotation
					}

					if(GUILayout.Button("Undulation", GUILayout.ExpandHeight(true))){
						// Undulation
					}

					GUILayout.Space(Screen.height * 0.1f);

					if (GUILayout.Button("Release", GUILayout.ExpandHeight(true)))
					{
						PhysAssembly a = CaptureEditorManager.capturedObj as PhysAssembly;
						Network.SetSendingEnabled(0, true);
						CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
						Network.SetSendingEnabled(0, false);
						Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
						Cleanup();
					}
				}else{

					//...

					GUILayout.BeginHorizontal();
					if(GUILayout.Button("Low Mutation", GUILayout.ExpandWidth(true))){
						mutationRate = 0.1f;
						DoTest();
					}
					if(GUILayout.Button("High Mutation", GUILayout.ExpandWidth(true))){
						mutationRate = 0.25f;
						DoTest();
					}
					GUILayout.EndHorizontal();
					if(GUILayout.Button("Cancel")){
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
            PhysAssembly newPhysAssem = new PhysAssembly(IOHelper.AssemblyToString(capturedAssembly), rot, false);
            newPhysAssem.Mutate(mutationRate);
        }
    }

    public void HandleObjectCaptured(object sender, System.EventArgs args)
    {
        capturedAssembly = CaptureEditorManager.capturedObj as PhysAssembly;
        if (!capturedAssembly)
            return;

        //burnRateKnob.Value = capturedAssembly.energyBurnRate;
    }

    public void Cleanup()
    {
        CaptureEditorManager.ReleaseCaptured();
        capturedAssembly = null;
        selectedNode = null;
    }
}
