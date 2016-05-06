using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Networking for the Capturing of entities from Clients such as mobile devices.

public class Cognogenesis_Networking : MonoBehaviour {

	public static Cognogenesis_Networking Inst = null;

	string connectToIP = "132.239.235.116";
    public int connectionPort = 25565;
    string ipAddress;
    string port;
    int maxNumberOfPlayers = 500;
    public static string playerName;
    string masterServerGameType = "ClarkeCenterAssemblySim";
    string serverName = "Assembly";
    string serverTagline = System.Environment.UserName + "\'s Computer";

    public static NetworkView myNetworkView;

    int ipListConnect = 0;

	public GameObject serverStuff;
	public GameObject clientStuff;

	public float externalEnviroScale = 0f;


    void Awake(){
		Inst = this;
		
        if (GetComponent<NetworkView>() == null)
        {
            NetworkView nv = gameObject.AddComponent<NetworkView>();
            nv.stateSynchronization = NetworkStateSynchronization.Off;
        }
	    myNetworkView = GetComponent<NetworkView>();
	    Network.minimumAllocatableViewIDs = 500;
        DontDestroyOnLoad(this);

    } // End of Awake().


    void Update(){

		if(Input.GetKeyDown(KeyCode.S))
	        Network.InitializeServer(maxNumberOfPlayers, connectionPort, !Network.HavePublicAddress());

		if(Input.GetKeyDown(KeyCode.C))
			Network.Connect(connectToIP, connectionPort);
		
		if(Network.peerType == NetworkPeerType.Server)
			externalEnviroScale = NeuroScaleDemo.Inst.enviroScale;

    } // End of Update().
	

    void OnGUI(){
	    GUI.skin.label.fontStyle = FontStyle.Normal;

		string connectionString = "";
		if(Network.peerType == NetworkPeerType.Server)
			connectionString = "Server running.";
		else if(Network.peerType == NetworkPeerType.Client)
			connectionString = "Connected as client.";

		GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), connectionString);

    } // End of OnGUI().


	void OnServerInitialized() {
		serverStuff.SetActive(true);
	} // End of OnServerInitialized().


	void OnPlayerConnected(NetworkPlayer player) {
	} // End of OnPlayerConnected().

	
	void OnPlayerDisconnected(NetworkPlayer networkPlayer){
    } // End of OnPlayerDisconnected(NetworkPlayer networkPlayer).


	void OnConnectedToServer() {
		clientStuff.SetActive(true);
		for(int i = 10; i < 20; i++) {
			NetworkViewID newID = Network.AllocateViewID();
			SmoothNetPosition.allFingertips[i].netView.viewID = newID;
			myNetworkView.RPC("AssignFingerViewID", RPCMode.Server, i, newID);
		}

		for(int i = 22; i < 24; i++) {
			NetworkViewID newID = Network.AllocateViewID();
			SmoothNetPosition.allFingertips[i].netView.viewID = newID;
			myNetworkView.RPC("AssignFingerViewID", RPCMode.Server, i, newID);
		}

	} // End of OnConnectedToServer().


	[RPC]
	void AssignFingerViewID(int index, NetworkViewID newID) {
		SmoothNetPosition.allFingertips[index].netView.viewID = newID;
	} // End of AssignFingerViewID().


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){

		float serverEnviroScale = 0f;

		// When sending my own data out...
		if(stream.isWriting){
			serverEnviroScale = externalEnviroScale;

			stream.Serialize(ref serverEnviroScale);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref serverEnviroScale);

			externalEnviroScale = serverEnviroScale;
		}
    } // End of OnSerializeNetworkView().

} // End of CaptureNet_Manager.












