using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Networking for the Capturing of entities from Clients such as mobile devices.

public class CaptureNet_Manager : MonoBehaviour {

	public static CaptureNet_Manager Inst = null;

    // Multiplayer variables
    public static ANetworkPlayer[] playerList = new ANetworkPlayer[0];

    static int nextViewID;

    string titleMessage = "Assembly --- http://imagination.ucsd.edu/assembly";
    string remoteIpList = "http://132.239.235.40:5000/fbsharing/AT9dONfV"; // This file lives on khan: /Khan/Assembly/app_data/ipList.txt
    List<string> connectToIP = new List<string>();
    List<string> backupConnectToIP = new List<string>(){"127.0.0.1", "132.239.235.116", "132.239.235.115", "75.80.103.34", "67.58.54.68"};
    public int connectionPort = 25565;
    bool useNAT = false; // Not sure what NAT is... do some research.
    string ipAddress;
    string port;
    int maxNumberOfPlayers = 16;
    public static string playerName;
    string serverName;
    string serverTagline;
    string serverNameForClient;
    bool iWantToHost = false;
    bool iWantToConnect = false;
	public bool autoIPConnect = true;

    // admin client vars
    bool showQRCode = false;

    public static NetworkView myNetworkView;
    public NetworkPlayer myOwner;
    public PlayerSync playerSync = null;

    
    float connectCooldown = 0f;
    int ipListConnect = 0;

	// orbit option
    public HashSet<NetworkPlayer> orbitPlayers = new HashSet<NetworkPlayer>();


    void Awake(){
		Inst = this;

        if (Debug.isDebugBuild)
            connectToIP = new List<string>() { "132.239.235.116" };
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
			autoIPConnect = false;
    } // End of Awake().


    void Update(){
	    connectCooldown -= Time.deltaTime;
        // Cycle through available IPs to connect to.
        if (autoIPConnect && (connectToIP.Count > 0) && (PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected) && (connectCooldown <= 0f) && (!ClientAdminMenu.Inst.showMenu || !PersistentGameManager.IsAdminClient)){
			Network.Connect(connectToIP[ipListConnect], connectionPort);
            ipListConnect = (ipListConnect + 1) % connectToIP.Count;

            connectCooldown = 0.5f;
        }

        // If player is not connected, run the ConnectWindow function.
        if ((!PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected)){
            // Create the server.
			Network.InitializeServer(maxNumberOfPlayers, connectionPort, useNAT);
	    }
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
	    networkView.RPC("PlayerHasLeft", RPCMode.Others, networkPlayer);
	    Network.RemoveRPCs(networkPlayer);

	    // Remove player from player list.
        if (playerList.Length > 0){
            ANetworkPlayer[] newPlayerList = new ANetworkPlayer[playerList.Length - 1];
            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i].player == networkPlayer)
                {
                    //ConsoleScript.Inst.WriteToLog("![" + playerList[i].playerName + "] has disconnected.");
                    // Update playerList, subtracting the disconnected player.
                    for (int j = 0; j < playerList.Length; j++)
                    {
                        if (j < i)
                            newPlayerList[j] = playerList[j];
                        if (j > i)
                            newPlayerList[j - 1] = playerList[j];
                    }
                    // Skip the rest.
                    i = playerList.Length;
                }
            }
            playerList = newPlayerList;
        }
    } // End of function OnPlayerDisconnected(NetworkPlayer networkPlayer).


    void OnGUI(){
	    GUI.skin.label.fontStyle = FontStyle.Normal;

        // Client GUI
		if (((ClientAdminMenu.Inst && !ClientAdminMenu.Inst.showMenu) || !PersistentGameManager.IsAdminClient)  && autoIPConnect){
			if ((PersistentGameManager.IsClient) && (Network.peerType == NetworkPeerType.Disconnected)){
				GUI.skin.label.fontSize = 40;
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Connecting to server...");
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


    [RPC] // Tell the MultiplayerScript in connected players the server info.
    void ServerInfo(string theServerName, string theServerTagline){
	    serverName = theServerName;
	    serverTagline = theServerTagline;
    } // End of ServerInfo().


    void OnPlayerConnected(NetworkPlayer networkPlayer){
	    networkView.RPC("ServerInfo", networkPlayer, serverName, serverTagline);
	
	    // Send the new player information for existing players.
	    for(int i = 0; i < playerList.Length; i++)
		    networkView.RPC("Credentials", networkPlayer, playerList[i].player, playerList[i].playerName);
    } // End of OnPlayerConnected().


    // When I connect to a server, send my credentials to everybody.
    // Show a message when a player has connected.
    void OnConnectedToServer(){
	    // Set me up as the first player.
	    playerList = new ANetworkPlayer[1];
	    playerList[0] = new ANetworkPlayer();
	
	    playerList[0].player = Network.player;
	    playerList[0].playerName = playerName;
        ANetworkPlayer.me = playerList[0];

		ClientAdminMenu.Inst.CloseAll();
	
	    //networkView.RPC("Credentials", RPCMode.Others, Network.player, playerName);
	    //ConsoleScript.Inst.GlobalWriteToLog("![" + playerName + "] has connected.", RPCMode.Others);
        //ConsoleScript.Inst.WriteToLog("Connected to server as [" + playerName + "].");

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
	    bool playerAlreadyInList = false;
	    for(int i = 0; i < playerList.Length; i++)
		    if(playerList[i].player == networkPlayer){
			    playerAlreadyInList = true;
			    playerList[i].playerName = newPlayerName;
		    }
	
	    if(!playerAlreadyInList){	
		    ANetworkPlayer[] newPlayerList = new ANetworkPlayer[ playerList.Length + 1 ];
		    for(int j = 0; j < playerList.Length; j++)
			    newPlayerList[j] = playerList[j];
			
		    newPlayerList[ newPlayerList.Length - 1 ] = new ANetworkPlayer();
		
		    newPlayerList[ newPlayerList.Length - 1 ].player = networkPlayer;
		    newPlayerList[ newPlayerList.Length - 1 ].playerName = newPlayerName;
		
		    playerList = newPlayerList;
	    }
    } // End of Credentials().


    [RPC] // Update playerList, removing the disconnected player.
    void PlayerHasLeft(NetworkPlayer networkPlayer){
	    // Remove player from player list.
	    ANetworkPlayer[] newPlayerList = new ANetworkPlayer[ playerList.Length - 1 ];
	    for(int i = 0; i < playerList.Length; i++){
		    if(playerList[i].player == networkPlayer){
			    // Update playerList, subtracting the disconnected player.
			    for(int j = 0; j < playerList.Length; j++){
				    if(j < i)
					    newPlayerList[j] = playerList[j];
				    if(j > i)
					    newPlayerList[j - 1] = playerList[j];
			    }
			    // Skip the rest.
			    i = playerList.Length;
		    }
	    }
	    playerList = newPlayerList;
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

} // End of CaptureNet_Manager.












