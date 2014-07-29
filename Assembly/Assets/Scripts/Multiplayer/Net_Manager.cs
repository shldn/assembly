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
    string connectToIP = "132.239.235.116";
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

    // Main window variables
    Rect connectionWindowRect;
    int connectionWindowWidth = 400;
    int connectionWindowHeight = 280;
    int buttonHeight =  60;
    int leftIndent;
    int topIndent;

    // Server shutdown window
    Rect serverDisWindowRect;
    int serverDisWindowWidth = 200;
    int serverDisWindowHeight = 150;
    int serverDisWindowLeftIndent = 10;
    int serverDisWindowTopIndent = 10;

    // Client disconnect window
    Rect clientDisWindowRect;
    int clientDisWindowWidth = 300;
    int clientDisWindowHeight = 150;
    bool showClientDisWindow;

    float previousUpdateTime;
    float updateFrequency = 5;

    public static NetworkView myNetworkView;
    public NetworkPlayer myOwner;


    void Awake(){
	    myNetworkView = networkView;
	    Network.minimumAllocatableViewIDs = 500;
    } // End of Awake().


    void Start(){
	    // Load details from registry.
	    playerName = PlayerPrefs.GetString("playerName");
	    serverName = PlayerPrefs.GetString("serverName");
	    serverTagline = PlayerPrefs.GetString("serverTagline");
	
	    // If nothin in the registry, give a default name.
	    if(serverName == "")
		    serverName = "Assembly Server";
	
	    if(serverTagline == "")
		    serverTagline = "UCSD Gamelab - indurain";
	
	    if(playerName == "")
		    playerName = "Player";
    } // End of Start().


    void Update(){
	    showClientDisWindow = Network.peerType == NetworkPeerType.Disconnected;

    } // End of Update().


    void ConnectWindow(int windowID){
	    // Leave a gap from the header.
	    GUILayout.Space(15);
	
	    // Give option to create or join a server.
	    if(!iWantToHost && !iWantToConnect){
		    if(GUILayout.Button("Host a Server", GUILayout.Height(buttonHeight)))
			    iWantToHost = true;

		    if(GUILayout.Button("Connect to Server", GUILayout.Height(buttonHeight)))
			    iWantToConnect = true;
		
		    GUILayout.Space(40);
		
		    // If this is a standalone, include a 'quit' button.
		    if(!Application.isWebPlayer && !Application.isEditor){
			    if(GUILayout.Button("Quit", GUILayout.Height(buttonHeight)))
				    Application.Quit();
		    }		
	    }
	
	    if(iWantToHost){
		    GUILayout.Label("Server name:");
		    serverName = GUILayout.TextField(serverName);
		
		    GUILayout.Label("Tagline:");
		    serverTagline = GUILayout.TextField(serverTagline);
		
		    GUILayout.Label("Player name:");
		    playerName = GUILayout.TextField(playerName);
		
		    GUILayout.Label("Port:");
		    // Display default port as String, but read as int. lol.
		    connectionPort = int.Parse(GUILayout.TextField(connectionPort.ToString()));
		
		    GUILayout.Space(10);
		    if(GUILayout.Button("Host", GUILayout.Height(45))){
			    if(playerName == "")
				    playerName = "Player";
			
			    if(playerName != ""){
				    // Connect to the server.
				    Network.Connect(connectToIP, connectionPort);
				    PlayerPrefs.SetString("playerName", playerName);
			    }
			
			    // Create the server.
			    Network.InitializeServer(maxNumberOfPlayers, connectionPort, useNAT);
			
			    // Save server name/tagline using PlayerPrefs (saves to the registry in Windows.)
			    PlayerPrefs.SetString("serverName", serverName);
			    PlayerPrefs.SetString("serverTagline", serverTagline);
			
			    iWantToConnect = false;
		    }
		    if(GUILayout.Button("Back", GUILayout.Height(30)))
			    iWantToHost = false;
	    }
	
	    if(iWantToConnect){
		    GUILayout.Label("Player name:");
		    playerName = GUILayout.TextField(playerName);
		
		    GUILayout.Label("Server IP:");
		    connectToIP = GUILayout.TextField(connectToIP);
		
		    GUILayout.Label("Port:");
		    connectionPort = int.Parse(GUILayout.TextField(connectionPort.ToString()));
		    GUILayout.Space(10);
		
		    if(GUILayout.Button("Connect", GUILayout.Height(45))){
			    if(playerName == "")
				    playerName = "Player";
			
			    if(playerName != ""){
				    // Connect to the server.
				    Network.Connect(connectToIP, connectionPort);
				    PlayerPrefs.SetString("playerName", playerName);
			    }
		    }
		
		    if(GUILayout.Button("Back", GUILayout.Height(30)))
			    iWantToConnect = false;
	    }
    } // End of ConnectWindow().


    void ServerDisconnectWindow(int windowID){
	    GUILayout.Label(serverName);
	    GUILayout.Label(serverTagline);
	    GUILayout.Space(10);
	    GUILayout.Label(Network.player.ipAddress);
	    GUILayout.Label("Number of players: " + (Network.connections.Length + 1));
	    // If at least one person playing, show average ping.
	    if(Network.connections.Length > 0)
		    GUILayout.Label("Average ping: " + Network.GetAveragePing(Network.connections[0]));

	    GUILayout.Space(5);
	
	    if(GUILayout.Button("End Server", GUILayout.Height(25)))
		    Network.Disconnect();
    } // End of ServerDisconnectWindow().


    void ClientDisconnectWindow(int windowID)
    {
	    GUILayout.Label(serverTagline);
	    GUILayout.Space(10);
	    GUILayout.Label("Number of players: " + (Network.connections.Length + 1));
	    GUILayout.Label("Ping: " + Network.GetAveragePing(Network.connections[0]));
	    GUILayout.Space(5);
	
	    if(GUILayout.Button("Disconnect", GUILayout.Height(25)))
		    Network.Disconnect();
	
	    GUILayout.Space(5);
	
	    // This allows a player using a webplayer in fullscreen to return to the game.
	    // (Pressing ESC in fullscreen just exits fullscreen.)
	    if(GUILayout.Button("Return to Game", GUILayout.Height(25))){
		    showClientDisWindow = false;
	    }
    }// End of ClientDisconnectWindow().


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
			    ConsoleScript.Inst.WriteToLog("![" + playerList[i].playerName + "] has disconnected.");
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
	
	    // If player is not connected, run the ConnectWindow function.
	    if(Network.peerType == NetworkPeerType.Disconnected){
		    // Place the connect window in the middle of the visible screen.
		
		    leftIndent = (Screen.width / 2) - (connectionWindowWidth / 2);
		    topIndent = (Screen.height / 2) - (connectionWindowHeight / 2);
		
		    connectionWindowRect = new Rect(leftIndent, topIndent, connectionWindowWidth, connectionWindowHeight);
		    connectionWindowRect = GUILayout.Window(0, connectionWindowRect, ConnectWindow, titleMessage);
	    }
	
	    // Show server info/disconnect if game is running as a server.
	    if(Network.peerType == NetworkPeerType.Server){
		    serverDisWindowRect = new Rect(Screen.width - (serverDisWindowLeftIndent + serverDisWindowWidth), serverDisWindowTopIndent, serverDisWindowWidth, serverDisWindowHeight);
		    serverDisWindowRect = GUILayout.Window(1, serverDisWindowRect, ServerDisconnectWindow, "Hosting Server");
	    }
	
	    // If client is connected to server, show server info window.
	    if(showClientDisWindow && (Network.peerType == NetworkPeerType.Client)){
		    clientDisWindowRect = new Rect((Screen.width / 2) - (clientDisWindowWidth / 2), (Screen.height / 2) - (clientDisWindowHeight / 2), clientDisWindowWidth, clientDisWindowHeight);
		    clientDisWindowRect = GUILayout.Window(1, clientDisWindowRect, ClientDisconnectWindow, "Connected to Server: " + serverName);
	    }
    } // End of OnGUI().


    [RPC] // Tell the MultiplayerScript in connected players the server info.
    void ServerInfo(string theServerName, string theServerTagline){
	    serverName = theServerName;
	    serverTagline = theServerTagline;
    } // End of ServerInfo().


    [RPC]
    void EnvironmentUpdate(float newTimeOfDay, int newDayLength){

    } // End of EnvironmentUpdate().


    // When my server starts, set me up as player 1.
    void OnServerInitialized(){
	    // Set me up as the first player.
	    ANetworkPlayer[] playerList = new ANetworkPlayer[1];
	    playerList[0] = new ANetworkPlayer();
	
	    playerList[0].player = networkView.owner;
	    playerList[0].playerName = playerName;
	
	    ConsoleScript.Inst.WriteToLog("!Server initialized as [" + playerName + "].");
    } // End of OnServerInitialized().


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
	
	    networkView.RPC("Credentials", RPCMode.Others, Network.player, playerName);
	    ConsoleScript.Inst.GlobalWriteToLog("![" + playerName + "] has connected.", RPCMode.Others);
        ConsoleScript.Inst.WriteToLog("Connected to server as [" + playerName + "].");
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
    void ServerRemoveBufferedRPCs(NetworkViewID viewID)
    {
	    Network.RemoveRPCs(viewID);
    } // End of ServerRemoveBufferedRPCs().
}












