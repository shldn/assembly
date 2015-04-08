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

				if(GUILayout.Button("Vision Range", GUILayout.ExpandHeight(true))){
					// Vision range
				}
				if(GUILayout.Button("Vision Scope", GUILayout.ExpandHeight(true))){
					// Vision scope
				}

				if(GUILayout.Button("Maximum Travel", GUILayout.ExpandHeight(true))){
					for(int i = 0; i < 10; i++){
						PhysAssembly newPhysAssem = new PhysAssembly(IOHelper.AssemblyToString(capturedAssembly), false);
						newPhysAssem.Mutate(0.25f);
					}

					GameObject testObject = new GameObject("maxTravelTester", typeof(Test_MaxTravel));
					testObject.transform.position = capturedAssembly.Position;
					testRunning = true;

					capturedAssembly.Destroy();
				}

				if(GUILayout.Button("Maximum Speed", GUILayout.ExpandHeight(true))){
					for(int i = 0; i < 10; i++){
						PhysAssembly newPhysAssem = new PhysAssembly(IOHelper.AssemblyToString(capturedAssembly), false);
						newPhysAssem.Mutate(0.25f);
					}

					GameObject testObject = new GameObject("maxSpeedTester", typeof(Test_MaxSpeed));
					testObject.transform.position = capturedAssembly.Position;
					testRunning = true;

					capturedAssembly.Destroy();
				}



				if(GUILayout.Button("Rotational Speed", GUILayout.ExpandHeight(true))){
					// Sense coverage
				}
				if(GUILayout.Button("Sense Range", GUILayout.ExpandHeight(true))){
					// Sensor range
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
