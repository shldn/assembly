using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Networking for the Capturing of entities from Clients such as mobile devices.

public class Cognogenesis_Networking : MonoBehaviour {

	public static Cognogenesis_Networking Inst = null;

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
    float connectCooldown = 0f;

	public GameObject serverStuff;
	public GameObject clientStuff;

	public float externalEnviroScale = 0f;

    // Accessors
    public bool IsCognoServer { get { return serverStuff != null && serverStuff.activeInHierarchy; } }
    public bool IsCognoClient { get { return clientStuff != null && clientStuff.activeInHierarchy; } }


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

        if(IsCognoClient && IsCognoServer) {
            if (Config.cognoView.StartsWith("in"))
                serverStuff.SetActive(false);
            else
                clientStuff.SetActive(false);
        }

    } // End of Awake().


    void Update(){

        // Create server or Connect to server.
        if (Network.peerType == NetworkPeerType.Disconnected) {
            if (IsCognoServer) {
                Network.InitializeServer(maxNumberOfPlayers, connectionPort, !Network.HavePublicAddress());
            }
            else {
                connectCooldown -= Time.deltaTime;
                if (connectCooldown < 0f) {
                    // try to connect to the server
                    Network.Connect(Config.cognoIP, connectionPort);
                    connectCooldown = 0.5f;
                }
            }
        }

		externalEnviroScale = (Mathf.Sin(Time.time * 0.2f) * 0.5f) + 0.5f;
		if(Network.peerType == NetworkPeerType.Server) {
			if(MuseManager.Inst.TouchingForehead)
				externalEnviroScale = NeuroScaleDemo.Inst.enviroScale;
				

			// Bullshittery
			for(int i = 0; i < FoodPellet.all.Count; i++) {
				for(int j = 22; j < 24; j++) {
					Vector3 vectorToPellet = FoodPellet.all[i].WorldPosition - SmoothNetPosition.allFingertips[j].transform.position;
					FoodPellet.all[i].velocity += (-vectorToPellet / Mathf.Clamp(vectorToPellet.sqrMagnitude * 0.01f, 0.1f, Mathf.Infinity)) * 0.1f;
				}
			}

		}



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
