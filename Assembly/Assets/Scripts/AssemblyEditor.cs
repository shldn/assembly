using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

    public GuiKnob burnRateKnob = null;
    public GuiKnob densityKnob = null;
    public GuiKnob speedKnob = null;


    void Update()
    {
        if (CaptureEditorManager.captureType == CaptureEditorManager.CaptureType.ASSEMBLY)
        {
            // Update assembly.
            for (int i = Assembly.GetAll().Count - 1; i >= 0; i--)
                Assembly.GetAll()[i].Update();

            // Update nodes.
            for (int i = 0; i < Node.GetAll().Count; ++i)
                Node.GetAll()[i].Update();

        }
    }
    void OnGUI()
    {

        burnRateKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) - 240f);
        burnRateKnob.scale = 2f;
        burnRateKnob.Draw();

        densityKnob.pxlPos = new Vector2(Screen.width - 120f, Screen.height * 0.5f);
        densityKnob.scale = 2f;
        densityKnob.Draw();

        speedKnob.pxlPos = new Vector2(Screen.width - 120f, (Screen.height * 0.5f) + 240f);
        speedKnob.scale = 2f;
        speedKnob.Draw();


        if( CaptureEditorManager.captureType == CaptureEditorManager.CaptureType.ASSEMBLY )
        {
            Rect controlBarRect = new Rect(Screen.width - (Screen.height / 6f), 0f, Screen.height / 6f, Screen.height);

            GUI.skin.button.fontSize = 20;

            GUILayout.BeginArea(controlBarRect);
            if (GUILayout.Button("Done", GUILayout.ExpandHeight(true)))
            {
                Assembly a = CaptureEditorManager.capturedObj as Assembly;
                CaptureNet_Manager.myNetworkView.RPC("PushAssembly", RPCMode.Server, a.ToFileString());
                Instantiate(PersistentGameManager.Inst.pingBurstObj, CaptureEditorManager.capturedObj.Position, Quaternion.identity);
                CaptureEditorManager.capturedObj.Destroy();
                CaptureEditorManager.capturedObj = null;
            }
            GUILayout.EndArea();
        }
    }
}
