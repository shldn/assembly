using UnityEngine;
using System.Collections;

public class AssemblyEditor : MonoBehaviour {

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
                CaptureEditorManager.capturedObj.Destroy();
                CaptureEditorManager.capturedObj = null;
            }
            GUILayout.EndArea();
        }
    }
}
