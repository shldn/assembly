using UnityEngine;
using System.Collections;

public class ClientAdminMenu : MonoBehaviour {

    bool showMenu = false;
    public Texture settingsIconTexture = null;
    int gutter = 10;
    int btnWidth = 50;
    int btnHeight = 50;
    bool orbitMode = false;

    void Awake()
    {
        enabled = PersistentGameManager.IsAdminClient;
    }
	
    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - btnWidth - gutter, Screen.height - btnHeight - gutter, btnWidth, btnHeight), settingsIconTexture))
            showMenu = !showMenu;

        if (showMenu)
        {
            Rect controlBarRect = new Rect(0.25f * Screen.width, gutter, 0.5f * Screen.width, Screen.height - 2 * gutter);

            GUI.skin.button.fontSize = 20;

            GUILayout.BeginArea(controlBarRect);
            if (GUILayout.Button("Next Scene", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
                PersistentGameManager.Inst.captureMgr.RequestNextScene();
                
            if (GUILayout.Button("Show Download QR Code", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
                PersistentGameManager.Inst.captureMgr.RequestToggleQRCodeVisibility();

            if (GUILayout.Button(orbitMode ? "Orbit Mode" : "Capture Mode", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
            {
                PersistentGameManager.Inst.captureMgr.playerSync.RequestToggleOrbitMode();
                orbitMode = !orbitMode;
            }
            GUILayout.EndArea();
        }
    }
}
