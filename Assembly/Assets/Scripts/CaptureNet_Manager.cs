using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Networking for the Capturing of entities from Clients such as mobile devices.

public class CaptureNet_Manager : MonoBehaviour {

	public static CaptureNet_Manager Inst = null;

    // Multiplayer variables

    string remoteIpList = "http://132.239.235.40:5000/fbsharing/AT9dONfV"; // This file lives on khan: /Khan/Assembly/app_data/ipList.txt
    List<string> connectToIP = new List<string>();
    List<string> backupConnectToIP = new List<string>(){"127.0.0.1", "132.239.235.116", "132.239.235.115", "75.80.103.34", "67.58.54.68"};
    public int connectionPort = 25565;
    bool useNAT = false; // Not sure what NAT is... do some research.
    string ipAddress;
    string port;
    int maxNumberOfPlayers = 500;
    public static string playerName;
    string masterServerGameType = "ClarkeCenterAssemblySim";
    string serverName = "Assembly";
    string serverTagline = System.Environment.UserName + "\'s Computer";
	public bool useKhanServerList = false;

    // admin client vars
    bool showQRCode = false;

    public static NetworkView myNetworkView;
    public PlayerSync playerSync = null;

    
    float connectCooldown = 0f;
    int ipListConnect = 0;

	// orbit option
    public HashSet<NetworkPlayer> orbitPlayers = new HashSet<NetworkPlayer>();

    // Master server helpers
    int connectingBlock = 0;
    bool hostListReceived = false;


    void Awake(){
		Inst = this;

        if( PersistentGameManager.IsClient && !useKhanServerList)
            RequestMasterServerHostList();

        if (Debug.isDebugBuild)
            connectToIP = new List<string>() { "127.0.0.1", "132.239.235.116" };
        else
            gameObject.AddComponent<DownloadHelper>().StartDownload(remoteIpList, HandleRemoteListDownloadComplete);

        if (GetComponent<NetworkView>() == null)
        {
            NetworkView nv = gameObject.AddComponent<NetworkView>();
            nv.stateSynchronization = NetworkStateSynchronization.Off;
        }
	    myNetworkView = networkView;
	    Network.minimumAllocatableViewIDs = 500;
        DontDestroyOnLoad(this);

		if(PersistentGameManager.IsAdminClient)
			useKhanServerList = false;
    } // End of Awake().


    void Update(){
	    connectCooldown -= Time.deltaTime;
        // Cycle through available IPs to connect to.
        if (useKhanServerList && (connectToIP.Count > 0) && (PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected) && (connectCooldown <= 0f) && (!ClientAdminMenu.Inst.showMenu || !PersistentGameManager.IsAdminClient)){
			Network.Connect(connectToIP[ipListConnect], connectionPort);
            ipListConnect = (ipListConnect + 1) % connectToIP.Count;

            connectCooldown = 0.5f;
        }

        // If player is not connected, run the ConnectWindow function.
        if ((!PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected)){
            // Create the server.
			Network.InitializeServer(maxNumberOfPlayers, connectionPort, useNAT);
            MasterServer.RegisterHost(masterServerGameType, serverName, serverTagline);
	    }

        if (Input.GetKeyUp(KeyCode.Q))
            showQRCode = !showQRCode;

    } // End of Update().

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
	    // If the server sees a player disconnect, remove their presence across the network.
	    Network.DestroyPlayerObjects(networkPlayer);
	    Network.RemoveRPCs(networkPlayer);

    } // End of OnPlayerDisconnected(NetworkPlayer networkPlayer).

    void OnGUI(){
	    GUI.skin.label.fontStyle = FontStyle.Normal;

        // Client GUI
		if (((ClientAdminMenu.Inst && !ClientAdminMenu.Inst.showMenu) || !PersistentGameManager.IsAdminClient)  && useKhanServerList){
			if ((PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected)){
				GUI.skin.label.fontSize = 40;
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Connecting to server...");
			}
        }
        if (PersistentGameManager.IsClient && (Network.peerType == NetworkPeerType.Disconnected) && !useKhanServerList && Time.frameCount > connectingBlock + 10)
        {
            GUI.skin = AssemblyEditor.Inst.guiSkin;
            GUI.skin.button.font = PrefabManager.Inst.assemblyFont;
            GUI.skin.label.font = PrefabManager.Inst.assemblyFont;

            HostData[] data = MasterServer.PollHostList();
            if( data.Length == 0 )
            {
                if(hostListReceived)
                {
                    MasterServer.ClearHostList();
                    Invoke("RequestMasterServerHostList", 0.5f);
                    hostListReceived = false;
                }
                GUI.skin.label.fontSize = 40;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Searching for servers...");
            }
            else if( data.Length == 1 )
            {
                // Only one server -- auto connect
                Network.Connect(data[0]);
                connectingBlock = Time.frameCount;
            }
            else
            {
                GUILayout.BeginArea(new Rect(10f, 10f, Screen.width, Screen.height));
                GUILayout.BeginVertical(GUILayout.MinWidth(300));
                GUI.skin.label.alignment = TextAnchor.UpperLeft;
                GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.04f);
                GUI.color = Color.white;
                GUILayout.Label("Choose a Server:", GUILayout.Height(Mathf.Max(GUI.skin.label.fontSize + 6, Mathf.CeilToInt(Screen.height * 0.054f))));
                GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.02f);

                // Go through all the hosts in the host list
                foreach (var element in data)
                {
                    //var name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
                    string buttonStr = element.comment;
                    if( string.IsNullOrEmpty(buttonStr) )
                    {
                        buttonStr = "[";
                        foreach (var host in element.ip)
                            buttonStr = buttonStr + host + ":" + element.port + " ";
                        buttonStr = buttonStr + "]";
                    }
                    if (GUILayout.Button(buttonStr))
                    {
                        // Connect to HostData struct, internally the correct method is used (GUID when using NAT).
                        Network.Connect(element);
                        connectingBlock = Time.frameCount;
                    }

                }
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
        }

        // Server GUI
        if( PersistentGameManager.IsServer ) {

			if (Network.peerType == NetworkPeerType.Disconnected){
				GUI.skin.label.fontSize = 20;
				GUI.skin.label.alignment = TextAnchor.LowerCenter;
				GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Initializing server...");
			}

			if (showQRCode)
			{
				int texSize = Screen.width / 8;
				int gutter = 20;
				GUI.DrawTexture(new Rect(Screen.width - texSize - gutter, Screen.height - texSize - gutter, texSize, texSize), PersistentGameManager.Inst.qrCodeTexture, ScaleMode.ScaleToFit);            
			}
		}
    } // End of OnGUI().

    void RequestMasterServerHostList()
    {
        MasterServer.RequestHostList(masterServerGameType);
    }

    [RPC] // Tell the MultiplayerScript in connected players the server info.
    void ServerInfo(string theServerName, string theServerTagline){
	    serverName = theServerName;
	    serverTagline = theServerTagline;
    } // End of ServerInfo().


    void OnPlayerConnected(NetworkPlayer networkPlayer){
    } // End of OnPlayerConnected().


    void OnConnectedToServer(){

		ClientAdminMenu.Inst.CloseAll();

        //GameObject newNetAmalgamGO = Network.Instantiate(netAmalgamPrefab, Vector3.zero, Quaternion.identity, 0) as GameObject;
        //Net_Amalgam newNetAmalgam = newNetAmalgamGO.GetComponent<Net_Amalgam>();
        //newNetAmalgam.SendAssemblies();

        playerSync = (Network.Instantiate(PersistentGameManager.Inst.playerSyncObj, Vector3.zero, Quaternion.identity, 1) as GameObject).GetComponent<PlayerSync>();

    } // End of OnConnectedToServer().

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



    [RPC] // Server receives this from client when they send a jellyfish back.
    void PushJelly(int head, int tail, int bobble, int wing){

        if (!JellyfishGameManager.Inst)
            return;

        Vector3 newJellyPos = Camera.main.transform.position + (Camera.main.transform.forward * 10f);
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
        //if (!GameManager.Inst)
            //return;

		// Ensure assemblies are dropped in at a viable position, relative to the camera.
        Vector3 assemblyNewPos = Camera.main.transform.position.normalized * Mathf.Min(NodeController.Inst.worldSize.x, NodeController.Inst.worldSize.y, NodeController.Inst.worldSize.z, Camera.main.transform.position.magnitude - 25f);
		assemblyNewPos += Random.insideUnitSphere * 10f;
        PlayInstantiationEffect(assemblyNewPos);
        PersistentGameManager.Inst.EnviroImpulse(assemblyNewPos, 30f);

        Assembly a = new Assembly(assemblyStr, null, assemblyNewPos);
		a.nametagFade = 30f;

		// Assembly is "thrown" back into the environment.
		foreach(Node someNode in a.NodeDict.Values)
			someNode.velocity = (Camera.main.transform.forward * 3f) + Random.insideUnitSphere * 1.5f;

    } // End of PushAssembly().

    // Client calls this to send request to server
    public void RequestNextScene()
    {
        networkView.RPC("GoToNextScene", RPCMode.Server);
    }

    [RPC] // Server receives this request from client
    void GoToNextScene()
    {
        LevelManager.LoadNextLevel();
    }

    // Client calls this to send request to server
    public void RequestToggleQRCodeVisibility()
    {
        networkView.RPC("ToggleQRCode", RPCMode.Server);
    }

    [RPC] // Server receives this request from client
    void ToggleQRCode()
    {
        showQRCode = !showQRCode;
    }

    void PlayInstantiationEffect(Vector3 pos)
    {
        AudioSource.PlayClipAtPoint(PersistentGameManager.Inst.pushClip, Vector3.zero);
        Instantiate(PersistentGameManager.Inst.pingBurstObj, pos, Quaternion.identity);
    }

    void OnFailedToConnectToMasterServer(NetworkConnectionError info)
    {
        Debug.LogError("Could not connect to Master server " + info);

    }

    void OnMasterServerEvent(MasterServerEvent msEvent)
    {
        if (msEvent == MasterServerEvent.HostListReceived)
            hostListReceived = true;

    }

} // End of CaptureNet_Manager.












