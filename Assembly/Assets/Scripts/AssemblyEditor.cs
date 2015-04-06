using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

    public static AssemblyEditor Inst = null;

    public GuiKnob burnRateKnob = null;
    public GuiKnob densityKnob = null;
    public GuiKnob speedKnob = null;

    private PhysAssembly capturedAssembly = null;
    public PhysNode selectedNode = null;

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
            //capturedAssembly.WorldPosition = Vector3.zero;

			/*
            burnRateKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) - 240f);
            burnRateKnob.scale = 2f;
            burnRateKnob.Draw();
            //capturedAssembly.energyBurnRate = burnRateKnob.Value;

            densityKnob.pxlPos = new Vector2(Screen.width - 120f, Screen.height * 0.5f);
            densityKnob.scale = 2f;
            densityKnob.Draw();

            speedKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) + 240f);
            speedKnob.scale = 2f;
            speedKnob.Draw();
			*/

            Rect controlBarRect = new Rect(Screen.width - (Screen.height / 6f), 0f, Screen.height / 6f, Screen.height);

            GUI.skin.button.fontSize = 20;

            GUILayout.BeginArea(controlBarRect);
			if(GUILayout.Button("Maximum Speed", GUILayout.ExpandHeight(true))){
				// Max speed
				//for(int i = 0; i < 10; i++){
//					new PhysAssembly(capturedAssembly.ToString());
				//}
			}
			if(GUILayout.Button("Rotational Speed", GUILayout.ExpandHeight(true))){
				// Sense coverage
			}
			if(GUILayout.Button("Sense Range", GUILayout.ExpandHeight(true))){
				// Sensor range
			}
			if(GUILayout.Button("Sense Coverage", GUILayout.ExpandHeight(true))){
				// Sense coverage
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
