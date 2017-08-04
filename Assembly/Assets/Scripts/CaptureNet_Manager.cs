using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Networking for the Capturing of entities from Clients such as mobile devices.

public class CaptureNet_Manager : MonoBehaviour {

	public static CaptureNet_Manager Inst = null;
    public static bool HasOrbitPlayers { get { return Inst != null && Inst.orbitPlayers.Count > 0; } }

    // Multiplayer variables

    string remoteIpList = "http://132.239.235.40:5000/fbsharing/AT9dONfV"; // This file lives on khan: /Khan/Assembly/app_data/ipList.txt
    List<string> connectToIP = new List<string>();
    List<string> backupConnectToIP = new List<string>(){"127.0.0.1", "132.239.235.116", "132.239.235.115", "75.80.103.34", "67.58.54.68"};
    public int connectionPort = 25565;
    string ipAddress;
    string port;
    int maxNumberOfPlayers = 500;
    public static string playerName;
    string masterServerGameType = "ClarkeCenterAssemblySim";
    string serverName = "Assembly";
    string serverTagline = System.Environment.UserName + "\'s Computer";
	

    // admin client vars
    bool showQRCode = false;

    // server option vars
    public bool useKhanServerList = false;
    bool showNameServer = false;
    string tempServerName = "";

    public static NetworkView myNetworkView;
    public PlayerSync playerSync = null;

    
    float connectCooldown = 0f;
    int ipListConnect = 0;

	// orbit option
    public HashSet<NetworkPlayer> orbitPlayers = new HashSet<NetworkPlayer>();

    // Master server helpers
    int connectingBlock = 0;
    bool hostListReceived = false;

	AudioSource uiAudioSource;
	public AudioSource UIAudioSource { get { return uiAudioSource; } }


    void Awake(){
		Inst = this;

        if (PersistentGameManager.IsClient && Config.useMasterServer)
            InitializeClient();
        else
            InitializeServer();

        if (GetComponent<NetworkView>() == null)
        {
            NetworkView nv = gameObject.AddComponent<NetworkView>();
            nv.stateSynchronization = NetworkStateSynchronization.Off;
        }
	    myNetworkView = GetComponent<NetworkView>();
	    Network.minimumAllocatableViewIDs = 500;
        DontDestroyOnLoad(this);

    } // End of Awake().

    void InitializeServer() {
        if (PlayerPrefs.HasKey("AServerName"))
            serverTagline = PlayerPrefs.GetString("AServerName");
        else
            showNameServer = true;

        tempServerName = serverTagline;

        if(Config.useMasterServer)
            InvokeRepeating("ReregisterMasterServer", 30 * 60, 30 * 60);

		uiAudioSource = gameObject.AddComponent<AudioSource>();
		uiAudioSource.spatialBlend = 0f;

    } // End of Awake().
    void InitializeClient() {
        if (PersistentGameManager.IsClient && !useKhanServerList)
            RequestMasterServerHostList();

        if (Debug.isDebugBuild)
            connectToIP = new List<string>() { "127.0.0.1", "132.239.235.116" };
        else
            gameObject.AddComponent<DownloadHelper>().StartDownload(remoteIpList, HandleRemoteListDownloadComplete);

        if (PersistentGameManager.IsAdminClient)
            useKhanServerList = false;

    }

    void Update(){

        if (PersistentGameManager.IsServer)
            UpdateServer();
        else
            UpdateClient();

    } // End of Update().

    void UpdateClient() {
        connectCooldown -= Time.deltaTime;
        if ((Network.peerType == NetworkPeerType.Disconnected) && (PersistentGameManager.IsClient)) {
            if((connectCooldown <= 0f) && (!ClientAdminMenu.Inst.showMenu || !PersistentGameManager.IsAdminClient)) {
                if (!Config.useMasterServer && Config.fallbackServerIP != "") {
                    Network.Connect(Config.fallbackServerIP, connectionPort);
                    connectCooldown = 0.5f;
                }
                // Cycle through available IPs to connect to.
                else if (useKhanServerList && (connectToIP.Count > 0)) {
                    Network.Connect(connectToIP[ipListConnect], connectionPort);
                    ipListConnect = (ipListConnect + 1) % connectToIP.Count;
                    connectCooldown = 0.5f;
                }
            }
        }
    }

    void UpdateServer() {
        if ((Network.peerType == NetworkPeerType.Disconnected) && !showNameServer) {
            // Create the server.
            Network.InitializeServer(maxNumberOfPlayers, connectionPort, !Network.HavePublicAddress());
            MasterServer.RegisterHost(masterServerGameType, serverName, serverTagline);
        }
        // Replaced for the merge, not clear if this was needed.
        //// If player is not connected, run the ConnectWindow function.
        //if( PersistentGameManager.IsServer )
        //{
        //    if ((Network.peerType == NetworkPeerType.Disconnected) && !showNameServer)
        //    {
        //        // Create the server.
        //        Network.InitializeServer(maxNumberOfPlayers, connectionPort, !Network.HavePublicAddress());
        //        if(Config.useMasterServer)
        //            MasterServer.RegisterHost(masterServerGameType, serverName, serverTagline);
        //    }

        if (KeyInput.GetKeyUp(KeyCode.Q))
            showQRCode = !showQRCode;

        if (KeyInput.GetKeyUp(KeyCode.N))
            showNameServer = true;


        // "Single-player"
        if (playerSync == null && KeyInput.GetKeyDown(KeyCode.End)) {
            PersistentGameManager.Inst.singlePlayer = true;
            playerSync = (Instantiate(PersistentGameManager.Inst.playerSyncObj, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<PlayerSync>();
        }
        if (PersistentGameManager.Inst.singlePlayer && KeyInput.GetKeyDown(KeyCode.Home)) {
            PersistentGameManager.Inst.singlePlayer = false;
            Destroy(playerSync.cursorObject.gameObject);
            Destroy(playerSync.gameObject);
            playerSync = null;
        }


        // Clear junk blip requests
        if (blipRequests.Count > 0) {
            bool connected = false;
            do {
                for (int i = 0; i < Network.connections.Length; i++) {
                    if (Network.connections[i] == blipRequests[0].player)
                        connected = true;
                    break;
                }
                if (!connected)
                    blipRequests.RemoveAt(0);
            } while ((blipRequests.Count > 0) && (connected == false));
        }

        // Send good requests.
        if (blipRequests.Count > 0) {
            if (blipRequests[0].assembly) {
                myNetworkView.RPC("CreateBlip", blipRequests[0].player, IOHelper.AssemblyToString(blipRequests[0].assembly), blipRequests[0].assembly.Position);
                print("Sending blip!");
            }
            blipRequests.RemoveAt(0);
        }
    }

    // Once the text file with the list of ips is downloaded, add the ips to the connection list.
    void HandleRemoteListDownloadComplete(WWW downloadObj)
    {
        if (!string.IsNullOrEmpty(downloadObj.error) || string.IsNullOrEmpty(downloadObj.text))
        {
            Debug.Log("Error getting remote text file, using original list");
            connectToIP.InsertRange(0,backupConnectToIP);
            return;
        }
        string[] ips = downloadObj.text.Split(',');
        for (int i = 0; i < ips.Length; ++i)
            connectToIP.Add(ips[i].Trim());
    }

    void OnPlayerDisconnected(NetworkPlayer networkPlayer){
		orbitPlayers.Remove(networkPlayer);

	    // If the server sees a player disconnect, remove their presence across the network.
	    Network.DestroyPlayerObjects(networkPlayer);
	    Network.RemoveRPCs(networkPlayer);

    } // End of OnPlayerDisconnected(NetworkPlayer networkPlayer).

    void OnGUI(){
	    GUI.skin.label.fontStyle = FontStyle.Normal;

        if (PersistentGameManager.IsServer)
            OnServerGUI();
        else
            OnClientGUI();

    } // End of OnGUI().

    void OnClientGUI() {
        if (((ClientAdminMenu.Inst && !ClientAdminMenu.Inst.showMenu) || !PersistentGameManager.IsAdminClient) && useKhanServerList) {
            if ((PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected)) {
                GUI.skin.label.fontSize = 40;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Connecting to server...");
            }
        }
        if (PersistentGameManager.IsClient && (Network.peerType == NetworkPeerType.Disconnected) && Config.useMasterServer && !useKhanServerList && Time.frameCount > connectingBlock + 10) {
            GUI.skin = AssemblyEditor.Inst.guiSkin;
            GUI.skin.button.font = PrefabManager.Inst.assemblyFont;
            GUI.skin.label.font = PrefabManager.Inst.assemblyFont;

            HostData[] data = MasterServer.PollHostList();
            if (data.Length == 0) {
                if (hostListReceived) {
                    MasterServer.ClearHostList();
                    Invoke("RequestMasterServerHostList", 0.5f);
                    hostListReceived = false;
                }
                GUI.skin.label.fontSize = 40;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Searching for servers...");
            }
            else if (data.Length == 1) {
                // Only one server -- auto connect
                Network.Connect(data[0]);
                connectingBlock = Time.frameCount;
            }
            else {
                GUILayout.BeginArea(new Rect(10f, 10f, Screen.width, Screen.height));
                GUILayout.BeginVertical(GUILayout.MinWidth(300));
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
                GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.06f);
                GUI.color = Color.white;
                GUILayout.Label("Choose a Server:", GUILayout.Height(Mathf.Max(GUI.skin.label.fontSize + 6, Mathf.CeilToInt(Screen.height * 0.054f))));
                GUI.skin.button.fontSize = Mathf.CeilToInt(Screen.height * 0.1f);
                if (data.Length >= 4)
                    GUI.skin.button.fontSize = Mathf.CeilToInt(Screen.height * 0.35f / (float)data.Length);

                RectOffset prevPadding = GUI.skin.button.padding;
                RectOffset prevMargin = GUI.skin.button.margin;
                int padding = GUI.skin.button.fontSize / 5;
                int margin = GUI.skin.button.fontSize / 5;
                GUI.skin.button.padding = new RectOffset(padding, padding, padding, padding);
                GUI.skin.button.margin = new RectOffset(margin, margin, margin, margin);


                // Go through all the hosts in the host list
                foreach (var element in data) {
                    //var name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
                    string buttonStr = element.comment;
                    if (string.IsNullOrEmpty(buttonStr)) {
                        buttonStr = "[";
                        foreach (var host in element.ip)
                            buttonStr = buttonStr + host + ":" + element.port + " ";
                        buttonStr = buttonStr + "]";
                    }
                    if (GUILayout.Button(buttonStr)) {
                        // Connect to HostData struct, internally the correct method is used (GUID when using NAT).
                        Network.Connect(element);
                        connectingBlock = Time.frameCount;
                    }

                }
                GUILayout.EndVertical();
                GUILayout.EndArea();

                GUI.skin.button.padding = prevPadding;
                GUI.skin.button.margin = prevMargin;
            }
        }
    }

    void OnServerGUI() {
        if (Network.peerType == NetworkPeerType.Disconnected) {
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height - 10), "Initializing server...");
        }

        if (showQRCode) {
            int texSize = Screen.width / 8;
            int gutter = 20;
            GUI.DrawTexture(new Rect(Screen.width - texSize - gutter, Screen.height - texSize - gutter, texSize, texSize), PersistentGameManager.Inst.qrCodeTexture, ScaleMode.ScaleToFit);
        }

        if (showNameServer) {
            GUI.skin.label.fontSize = Mathf.Max(14, Mathf.CeilToInt(Screen.height * 0.02f));
            GUI.Label(new Rect(10, Screen.height - 50, 200, 20), "Enter Server Name:");
            GUI.SetNextControlName("NameServerTextField");
            tempServerName = GUI.TextField(new Rect(10, Screen.height - 30, 200, 20), tempServerName, 30);
            GUI.FocusControl("NameServerTextField");
            if (Event.current.type == EventType.keyUp && Event.current.keyCode == KeyCode.Return) {
                if (tempServerName != serverTagline) {
                    serverTagline = tempServerName;
                    PlayerPrefs.SetString("AServerName", serverTagline);
                    MasterServer.UnregisterHost();
                    if (Network.peerType != NetworkPeerType.Disconnected)
                        MasterServer.RegisterHost(masterServerGameType, serverName, serverTagline);
                    //else
                    // Next update will RegisterHost with MasterServer
                }
                else if (!PlayerPrefs.HasKey("AServerName"))
                    PlayerPrefs.SetString("AServerName", serverTagline);
                showNameServer = false;
            }
            KeyInput.Locked = showNameServer;
        }
    }

    void RequestMasterServerHostList()
    {
        MasterServer.RequestHostList(masterServerGameType);
    }

    [RPC] // Tell the MultiplayerScript in connected players the server info.
    void ServerInfo(string theServerName, string theServerTagline){
	    serverName = theServerName;
	    serverTagline = theServerTagline;
    } // End of ServerInfo().

	
	class AssemblyBlipRequest {
		public Assembly assembly;
		public NetworkPlayer player;

		public AssemblyBlipRequest(Assembly assembly, NetworkPlayer player){
			this.assembly = assembly;
			this.player = player;
		} // End of AssemblyBlipRequest().
	} // End of AssemblyBlipRequest.
	List<AssemblyBlipRequest> blipRequests = new List<AssemblyBlipRequest>();


    void OnConnectedToServer(){

		ClientAdminMenu.Inst.CloseAll();

        //GameObject newNetAmalgamGO = Network.Instantiate(netAmalgamPrefab, Vector3.zero, Quaternion.identity, 0) as GameObject;
        //Net_Amalgam newNetAmalgam = newNetAmalgamGO.GetComponent<Net_Amalgam>();
        //newNetAmalgam.SendAssemblies();

        playerSync = (Network.Instantiate(PersistentGameManager.Inst.playerSyncObj, Vector3.zero, Quaternion.identity, 1) as GameObject).GetComponent<PlayerSync>();
		myNetworkView.RPC("InitClient", RPCMode.Server, Network.player, PlayerSync.LassoClientDefault? 0 : 1);
		print("Sending init info...");

    } // End of OnConnectedToServer().


	[RPC] // Inform server what type of client we are. Server will send data if needed.
	void InitClient(NetworkPlayer player, int type){
		print("Client connected, type " + type);

		for(int i = 0; i < PlayerSync.all.Count; i++){
			if(PlayerSync.all[i].GetComponent<NetworkView>().owner == player)
				PlayerSync.all[i].lassoClient = (type == 0)? true : false;
		}

		if(type == 1){
			for(int i = 0; i < Assembly.getAll.Count; i++){
				if(Assembly.getAll[i].ready)
					blipRequests.Add(new AssemblyBlipRequest(Assembly.getAll[i], player));
			}
		}
	} // End of ClientType().


    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        CaptureEditorManager.ReleaseCaptured();
        if (AssemblyEditor.Inst)
            AssemblyEditor.Inst.Cleanup();
    } // End of OnDisconnectedFromServer().


    [RPC] // Receive another player's credentials; if that player is not in my list, add him.
    void Credentials(NetworkPlayer networkPlayer, string newPlayerName){	
    } // End of Credentials().


    [RPC] // Update playerList, removing the disconnected player.
    void PlayerHasLeft(NetworkPlayer networkPlayer){
    } // End of PlayerHasLeft().
    // Removes RPCs for a certain networkViewID.
    public static void RemoveBufferedRPCs(NetworkViewID viewID){
	    if(Network.peerType == NetworkPeerType.Server)
		    Network.RemoveRPCs(viewID);
	    else
		    myNetworkView.RPC("ServerRemoveBufferedRPCs", RPCMode.Server, viewID);
    } // End of RemoveBufferedRPCs().


    [RPC]
    void ServerRemoveBufferedRPCs(NetworkViewID viewID){
	    Network.RemoveRPCs(viewID);
    } // End of ServerRemoveBufferedRPCs().


    void OnApplicationQuit(){
        Network.Disconnect();
        if(PersistentGameManager.IsServer)
            MasterServer.UnregisterHost();
    } // End of OnApplicationQuit().

    void ReregisterMasterServer()
    {
        if (Config.useMasterServer){
            MasterServer.UnregisterHost();
            MasterServer.RegisterHost(masterServerGameType, serverName, serverTagline);
        }
    }

    [RPC] // Server receives this from client when they send a jellyfish back.
    void PushJelly(int head, int tail, int bobble, int wing){

        if (!JellyfishGameManager.Inst)
            return;

        float distInFrontOfCam = 10f;
        Vector3 newJellyPos = Camera.main.transform.position + (Camera.main.transform.forward * distInFrontOfCam) + (Random.insideUnitSphere * 0.6f * distInFrontOfCam);
        uiAudioSource.clip = PersistentGameManager.Inst.pushClip;
        uiAudioSource.Play();
        PlayInstantiationEffect(newJellyPos);

        Transform newJellyTrans = Instantiate(JellyfishPrefabManager.Inst.jellyfish, newJellyPos, Random.rotation) as Transform;
        JellyFishCreator newJellyCreator = newJellyTrans.GetComponent<JellyFishCreator>();
        newJellyCreator.changeHead(head);
        newJellyCreator.changeTail(tail);
        newJellyCreator.changeBoball(bobble);
        newJellyCreator.smallTail(wing);

        print("Received jelly, " + head + " " + tail + " " + bobble + " " + wing);

    } // End of PushJelly().

    [RPC] // Server receives this from client when they send an assembly back.
    void PushAssembly(string assemblyStr)
    {
		print("PushAssembly()");
        //if (!GameManager.Inst)
        //return;


        if (PersistentGameManager.IsLightServer && !PersistentGameManager.ViewerConnectsWithPhones) {
            // Ask viewer for camera info before we release
            ViewerData.Inst.messages.Add(new ViewerDataRequest(ViewerDataRequestType.CAMERA_INFO));
            NodeController.Inst.assembliesToReleaseOnCameraUpdate.Add(assemblyStr);
        }
        else
            ReleaseAssembly(assemblyStr);

    } // End of PushAssembly().

    public void ReleaseAssembly(string assemblyStr) {
        // Ensure assemblies are dropped in at a viable position, relative to the camera.
        Vector3 assemblyNewPos = Camera.main.transform.position + (Camera.main.transform.forward * 20f) + (Random.insideUnitSphere * 10f);
        uiAudioSource.clip = PersistentGameManager.Inst.pushClip;
        uiAudioSource.Play();
        PlayInstantiationEffect(assemblyNewPos);
        PersistentGameManager.Inst.EnviroImpulse(assemblyNewPos, 30f);

        if (PersistentGameManager.EmbedViewer) {
            Assembly a = new Assembly(assemblyStr, null, assemblyNewPos, false, true);

            // Assembly is "thrown" back into the environment.
            a.ThrowAwayFromCamera();
        }
        else {
            if(PersistentGameManager.ViewerConnectsWithPhones)
                ControllerData.Inst.Add(new AssemblyReleased(assemblyStr, assemblyNewPos, Camera.main.transform.forward));
            else {
                Assembly a = new Assembly(assemblyStr, null, assemblyNewPos, false, true);
                a.userReleased = true;
                a.ThrowAwayFromCamera();
                ViewerData.Inst.assemblyCreations.Add(new AssemblyCreationData(a));

                // after creation message is sent, don't consider userReleased anymore, future viewers will put labels on all of these.
                a.userReleased = false;
            }
        }
    } // End of ReleaseAssembly().

    // Client calls this to send request to server
    public void RequestNextScene()
    {
        GetComponent<NetworkView>().RPC("GoToNextScene", RPCMode.Server);
    }

    [RPC] // Server receives this request from client
    void GoToNextScene()
    {
        LevelManager.LoadNextLevel();
    }

    // Client calls this to send request to server
    public void RequestToggleQRCodeVisibility()
    {
        GetComponent<NetworkView>().RPC("ToggleQRCode", RPCMode.Server);
    }

    [RPC] // Server receives this request from client
    void ToggleQRCode()
    {
        showQRCode = !showQRCode;
    }

    void PlayInstantiationEffect(Vector3 pos)
    {
        if (!PersistentGameManager.IsLightServer) {
            AudioSource.PlayClipAtPoint(PersistentGameManager.Inst.pushClip, Vector3.zero);
            Instantiate(PersistentGameManager.Inst.pingBurstObj, pos, Quaternion.identity);
        }
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.LogError("Could not connect to Master server " + info);
        if (PersistentGameManager.IsClient)
            useKhanServerList = true;

    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
            hostListReceived = true;

    }

} // End of CaptureNet_Manager.












