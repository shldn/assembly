using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

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
                CaptureEditorManager.ReleaseCaptured();
            }
            GUILayout.EndArea();
        }
    }
}
