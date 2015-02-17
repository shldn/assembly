using UnityEngine;
using System.Collections;

public class ClientAdminMenu : MonoBehaviour {

	public static ClientAdminMenu Inst = null;

    public bool showMenu = false;
    public Texture settingsIconTexture = null;
    int gutter = 10;
    int btnWidth = 50;
    int btnHeight = 50;
    bool orbitMode = false;
	bool showIPNumpad = false;
	string ipString = "";

    void Awake()
    {
		Inst = this;
        enabled = PersistentGameManager.IsAdminClient;
    }
	
    void OnGUI()
    {
        if( CaptureEditorManager.capturedObj != null )
            return;

        if (GUI.Button(new Rect(Screen.width - btnWidth - gutter, Screen.height - btnHeight - gutter, btnWidth, btnHeight), settingsIconTexture))
            showMenu = !showMenu;

		Rect controlBarRect = new Rect(0.25f * Screen.width, gutter, 0.5f * Screen.width, Screen.height - 2 * gutter);
		GUI.skin.button.fontSize = 20;

        if (showMenu)
        {
			GUILayout.BeginArea(controlBarRect);
			if(showIPNumpad){
				GUILayout.Label(ipString, GUILayout.Height(Screen.height * 0.1f), GUILayout.ExpandWidth(true));

				int hitButton = -1;
				string[] buttonList = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "del"};
				hitButton = GUILayout.SelectionGrid(hitButton, buttonList, 3, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				if(hitButton == 10)
					ipString += ".";
				else if(hitButton == 11)
					ipString = ipString.Substring(0, ipString.Length - 1);
				else if(hitButton >= 0)
					ipString += hitButton.ToString();

				GUILayout.Space(Screen.height * 0.05f);
				if(GUILayout.Button("<b>\u221A Connect to IP</b>", GUILayout.Height(Screen.height * 0.1f), GUILayout.ExpandWidth(true))){
					Network.Disconnect();
					Network.Connect(ipString, CaptureNet_Manager.Inst.connectionPort);
				}

				if(Input.GetKeyDown(KeyCode.Escape))
					showIPNumpad = false;
			}else{
				if (GUILayout.Button("Next Scene", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
					PersistentGameManager.Inst.captureMgr.RequestNextScene();
                
				if (GUILayout.Button("Show Download QR Code", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
					PersistentGameManager.Inst.captureMgr.RequestToggleQRCodeVisibility();

				if (GUILayout.Button(orbitMode ? "Orbit Mode" : "Capture Mode", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					PersistentGameManager.Inst.captureMgr.playerSync.RequestToggleOrbitMode();
					orbitMode = !orbitMode;
				}

				if (GUILayout.Button("Manual IP Connect", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
					showIPNumpad = true;
				}
			}
			GUILayout.EndArea();
        }
    }
}
