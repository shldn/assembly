using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

    public static AssemblyEditor Inst = null;

    public GuiKnob burnRateKnob = null;
    public GuiKnob densityKnob = null;
    public GuiKnob speedKnob = null;

    private Assembly capturedAssembly = null;
    public Node selectedNode = null;

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
            capturedAssembly.Update();

            // Update nodes.
            for (int i = 0; i < Node.GetAll().Count; ++i)
                Node.GetAll()[i].Update();

            // Select nodes with raycast
            if(Input.GetMouseButtonDown(0) && !NodeEngineering.Inst.uiLockout){
                Ray touchRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit touchRayHit = new RaycastHit();
                int nodesLayer = 1 << LayerMask.NameToLayer("Nodes");
                if(Physics.Raycast(touchRay, out touchRayHit, 1000f, nodesLayer)){
                    Node newSelectedNode = null;
                    for(int i = 0; i < Node.GetAll().Count; i++){
                        Node curNode = Node.GetAll()[i];
                        if(touchRayHit.transform.gameObject == curNode.gameObject){
                            newSelectedNode = curNode;
                        }
                    }
                    if(newSelectedNode == selectedNode)
                        selectedNode = null;
                    else
                        selectedNode = newSelectedNode;
                }
            }
        }
    }
    void OnGUI()
    {
        if (capturedAssembly)
        {
            burnRateKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) - 240f);
            burnRateKnob.scale = 2f;
            burnRateKnob.Draw();
            capturedAssembly.energyBurnRate = burnRateKnob.Value;

            densityKnob.pxlPos = new Vector2(Screen.width - 120f, Screen.height * 0.5f);
            densityKnob.scale = 2f;
            densityKnob.Draw();

            speedKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) + 240f);
            speedKnob.scale = 2f;
            speedKnob.Draw();


            Rect controlBarRect = new Rect(Screen.width - (Screen.height / 6f), 0f, Screen.height / 6f, Screen.height);

            GUI.skin.button.fontSize = 20;

            GUILayout.BeginArea(controlBarRect);
            if (GUILayout.Button("Done", GUILayout.Height(Screen.height / 8f)))
            {
                Assembly a = CaptureEditorManager.capturedObj as Assembly;
                Network.SetSendingEnabled(0, true);
                CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
                Network.SetSendingEnabled(0, false);
                Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
                CaptureEditorManager.ReleaseCaptured();
                capturedAssembly = null;
                selectedNode = null;
            }
            GUILayout.EndArea();
        }
    }

    public void HandleObjectCaptured(object sender, System.EventArgs args)
    {
        capturedAssembly = CaptureEditorManager.capturedObj as Assembly;
        burnRateKnob.Value = capturedAssembly.energyBurnRate;
    }
}
