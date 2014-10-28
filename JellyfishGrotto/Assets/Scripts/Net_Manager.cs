using UnityEngine;
using System.Collections;

// Multiplayer script. This is attached to the MultiplayerManager and is the
// foundation for the Multiplayer system.

// Accessed by the CursorControl script.

public class Net_Manager : MonoBehaviour {

    // Multiplayer variables
    public static ANetworkPlayer[] playerList = new ANetworkPlayer[0];

    static int nextViewID;

    string titleMessage = "Assembly --- http://imagination.ucsd.edu/assembly";
    string[] connectToIP = {"132.239.235.116", "132.239.235.115"};
    int connectionPort = 25565;
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

    public static NetworkView myNetworkView;
    public NetworkPlayer myOwner;

    
    float connectCooldown = 0f;
    int ipListConnect = 0;


    void Awake(){
	    myNetworkView = networkView;
	    Network.minimumAllocatableViewIDs = 500;
    } // End of Awake().


    void Update(){
	    connectCooldown -= Time.deltaTime;
        // Cycle through available IPs to connect to.
        if((Application.platform == RuntimePlatform.Android) && (Network.peerType == NetworkPeerType.Disconnected) && (connectCooldown <= 0f)){
			Network.Connect(connectToIP[ipListConnect], connectionPort);
            ipListConnect++;
            ipListConnect = Mathf.FloorToInt(Mathf.Repeat(ipListConnect, connectToIP.Length));

            connectCooldown = 3f;
        }

        // If player is not connected, run the ConnectWindow function.
	    if((Application.platform != RuntimePlatform.Android) && (Network.peerType == NetworkPeerType.Disconnected) && Input.GetKeyDown(KeyCode.Space)){
            // Create the server.
			Network.InitializeServer(maxNumberOfPlayers, connectionPort, useNAT);
	    }
    } // End of Update().


    void OnDisconnectedFromServer(){
	    // If connection is lost, restart the level.
	    Application.LoadLevel(Application.loadedLevel);
    } // End of OnDisconnectedFromServer().


    void OnPlayerDisconnected(NetworkPlayer networkPlayer){
	    // If the server sees a player disconnect, remove their presence across the network.
	    Network.DestroyPlayerObjects(networkPlayer);
	    networkView.RPC("PlayerHasLeft", RPCMode.Others, networkPlayer);
	    Network.RemoveRPCs(networkPlayer);
	
	    // Remove player from player list.
	    ANetworkPlayer[] newPlayerList = new ANetworkPlayer[playerList.Length - 1];
	    for(int i = 0; i < playerList.Length; i++){
		    if(playerList[i].player == networkPlayer){
			    //ConsoleScript.Inst.WriteToLog("![" + playerList[i].playerName + "] has disconnected.");
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
    } // End of function OnPlayerDisconnected(NetworkPlayer networkPlayer).


    void OnGUI(){
	    GUI.skin.label.fontStyle = FontStyle.Normal;
	 
        if((Application.platform == RuntimePlatform.Android) && (Network.peerType == NetworkPeerType.Disconnected)){
            GUI.skin.label.fontSize = 40;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Connecting to server...");
        }

        if((Application.platform != RuntimePlatform.Android) && (Network.peerType == NetworkPeerType.Disconnected)){
            GUI.skin.label.fontSize = 20;
            GUI.skin.label.alignment = TextAnchor.LowerCenter;
            GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), "Press SPACE to initialize server.");
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
	
	    networkView.RPC("Credentials", RPCMode.Others, Network.player, playerName);
	    //ConsoleScript.Inst.GlobalWriteToLog("![" + playerName + "] has connected.", RPCMode.Others);
        //ConsoleScript.Inst.WriteToLog("Connected to server as [" + playerName + "].");

        //GameObject newNetAmalgamGO = Network.Instantiate(netAmalgamPrefab, Vector3.zero, Quaternion.identity, 0) as GameObject;
        //Net_Amalgam newNetAmalgam = newNetAmalgamGO.GetComponent<Net_Amalgam>();
        //newNetAmalgam.SendAssemblies();

        Network.Instantiate(PrefabManager.Inst.playerSyncObject, Vector3.zero, Quaternion.identity, 1);

    } // End of OnConnectedToServer().


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
		    Net_Manager.myNetworkView.RPC("ServerRemoveBufferedRPCs", RPCMode.Server, viewID);
    } // End of RemoveBufferedRPCs().


    [RPC]
    void ServerRemoveBufferedRPCs(NetworkViewID viewID){
	    Network.RemoveRPCs(viewID);
    } // End of ServerRemoveBufferedRPCs().


    void OnApplicationQuit(){
        Network.Disconnect();
    } // End of OnApplicationQuit().

} // End of Net_Manager.












