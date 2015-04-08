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
            Rect controlBarRect = new Rect(Screen.width - (Screen.height / 6f), 0f, Screen.height / 6f, Screen.height);

            GUI.skin.button.fontSize = 20;

			if(!testRunning){
				GUILayout.BeginArea(controlBarRect);
				if(GUILayout.Button("Maximum Travel", GUILayout.ExpandHeight(true))){
                    SpawnTestAssemblies(numTestAssemblies, mutationRate, null);

					GameObject testObject = new GameObject("maxTravelTester", typeof(Test_MaxTravel));
					testObject.transform.position = capturedAssembly.Position;
					testRunning = true;

					capturedAssembly.Destroy();
				}

				if(GUILayout.Button("Maximum Speed", GUILayout.ExpandHeight(true))){
                    SpawnTestAssemblies(numTestAssemblies, mutationRate, null);

					GameObject testObject = new GameObject("maxSpeedTester", typeof(Test_MaxSpeed));
					testObject.transform.position = capturedAssembly.Position;
					testRunning = true;

					capturedAssembly.Destroy();
				}



				/*
				if(GUILayout.Button("Rotational Speed", GUILayout.ExpandHeight(true))){
					// Sense coverage
				}
				if(GUILayout.Button("Sense Range", GUILayout.ExpandHeight(true))){
					// Sensor range
				}
				if(GUILayout.Button("Sense Coverage", GUILayout.ExpandHeight(true))){
					// Sense coverage
				}
				*/
                if (GUILayout.Button("IQ", GUILayout.ExpandHeight(true)))
                {
                    // Brain Training
                    SpawnTestAssemblies(numTestAssemblies, mutationRate, capturedAssembly.spawnRotation);

                    GameObject testObject = new GameObject("maxIQTester", typeof(Test_IQ));
                    testObject.transform.position = capturedAssembly.Position;
                    testRunning = true;

                    capturedAssembly.Destroy();
                }

				GUILayout.Space(20f);

				if (GUILayout.Button("Done", GUILayout.ExpandHeight(true)))
				{
					PhysAssembly a = CaptureEditorManager.capturedObj as PhysAssembly;
					Network.SetSendingEnabled(0, true);
					CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
					Network.SetSendingEnabled(0, false);
					Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
					Cleanup();
				}
				GUILayout.EndArea();
			}
        }
    }

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
