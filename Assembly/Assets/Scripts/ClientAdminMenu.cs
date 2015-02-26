using UnityEngine;
using System.Collections;

public class ClientAdminMenu : MonoBehaviour {

	public static ClientAdminMenu Inst = null;

    public bool showMenu = false;
    public Texture settingsIconTexture = null;
	public GUIStyle clientGUISkin = null;
    int gutter = 10;
    bool orbitMode = false;
	bool showIPNumpad = false;
	string ipString = "";

	public bool isOpen = false;
	float isOpenCooldown = 0f;

    void Awake(){
		Inst = this;
        enabled = PersistentGameManager.IsAdminClient;

		if(PlayerPrefs.HasKey("manualIP"))
			ipString = PlayerPrefs.GetString("manualIP");
    } // End of Awake().


	void Update(){
		if(showMenu || showIPNumpad){
			isOpen = true;
			isOpenCooldown = 0.2f;
		}
		else{
			isOpenCooldown -= Time.deltaTime;
			if(isOpenCooldown < 0f)
				isOpen = false;
		}
	} // End of Update().
	

    void OnGUI()
    {
        if( CaptureEditorManager.capturedObj != null )
            return;

		int cogSize = Mathf.CeilToInt(Screen.height * 0.1f);
        if (GUI.Button(new Rect(Screen.width - cogSize - gutter, Screen.height - cogSize - gutter, cogSize, cogSize), settingsIconTexture))
            showMenu = !showMenu;

		Rect controlBarRect = new Rect(0.25f * Screen.width, gutter, 0.5f * Screen.width, Screen.height - 2 * gutter);
		GUI.skin.button.fontSize = Mathf.CeilToInt(Mathf.Min(Screen.width, Screen.height) * 0.06f);

        if (showMenu)
        {
			GUILayout.BeginArea(controlBarRect);
			if(showIPNumpad){
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUILayout.Label(ipString, GUILayout.Height(Screen.height * 0.1f), GUILayout.ExpandWidth(true));

				int hitButton = -1;
				string[] buttonList = {"1", "2", "3", "4", "5", "6", "7", "8", "9", ".", "0", "\u2190"};
				hitButton = GUILayout.SelectionGrid(hitButton, buttonList, 3, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
				if(hitButton == 9)
					ipString += ".";
				else if(hitButton == 10)
					ipString += "0";
				else if(hitButton == 11)
					ipString = ipString.Substring(0, ipString.Length - 1);
				else if(hitButton >= 0)
					ipString += (hitButton + 1).ToString();

				GUILayout.Space(Screen.height * 0.05f);
				if(GUILayout.Button("<b>\u221A Connect to IP</b>", GUILayout.Height(Screen.height * 0.1f), GUILayout.ExpandWidth(true))){
					PlayerPrefs.SetString("manualIP", ipString);
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

				if(!CaptureNet_Manager.Inst.autoIPConnect){
					if (GUILayout.Button("Auto-Connect (IP list)", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
						CaptureNet_Manager.Inst.autoIPConnect = true;
					}
				}else{
					if (GUILayout.Button("Auto-connecting...", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
						CaptureNet_Manager.Inst.autoIPConnect = false;
					}
				}

				if(Input.GetKeyDown(KeyCode.Escape))
					showMenu = false;
			}
			GUILayout.EndArea();
        }
    } // End of OnGUI().


	public void CloseAll(){
		showMenu = false;
		showIPNumpad = false;
	} // End of CloseAll().

} // End of ClientAdminMenu.
