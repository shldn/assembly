using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSync : MonoBehaviour {

	public static List<PlayerSync> all = new List<PlayerSync>();

    Vector3 screenPos = Vector3.zero;
    public Vector3 screenPosSmoothed = Vector3.zero;
    Vector3 screenPosVel = Vector3.zero;
    float screenPosSmoothTime = 0.1f;

    LinkedList<Vector2> lastPoints = new LinkedList<Vector2>();
    LinkedList<float> lastPointDists = new LinkedList<float>();
    bool selecting = false;
    bool initialPosSent = false;

    public Transform cursorObject;
    public LineRenderer cursorLine;
    float cursorLineDist = 0.0f;
    float cursorLineMaxDist = 40000f;

    bool editing = false;

	bool pinchRelease = true;
	float lastPinchDist = -1f;

	bool orbitModeInit = false;
    int levelWasLoadedFrame = 0;

	bool lassoSelectEnabled = false;


    void Awake()
    {
        screenPos = new Vector3(0.5f * Screen.width, 0.5f * Screen.height, 0.0f);
        screenPosSmoothed = screenPos;
        cursorLineMaxDist = Screen.width + Screen.height;
        DontDestroyOnLoad(this);
		all.Add(this);
    }

	void Start(){
	} // End of Start().

    void LateUpdate(){

		if(Application.loadedLevelName.Equals("CaptureClient") && PersistentGameManager.Inst.serverCapturedAssem != ""){
			print("Whoop!");
			CaptureAssembly(PersistentGameManager.Inst.serverCapturedAssem);
			PersistentGameManager.Inst.serverCapturedAssem = "";
		}

        screenPosSmoothed = Vector3.SmoothDamp(screenPosSmoothed, screenPos, ref screenPosVel, screenPosSmoothTime);

        if(cursorObject){
            cursorObject.gameObject.SetActive((!editing || PersistentGameManager.Inst.singlePlayer) && lassoSelectEnabled);
        }

        if(cursorObject && (PersistentGameManager.IsClient) && !networkView.isMine)
            Destroy(cursorObject.gameObject);

		if(CaptureNet_Manager.Inst.orbitPlayers.Contains(networkView.owner))
        {
            // handle camera orbiting for these players screenPos movements here
			if(orbitModeInit){
                Vector2 centerOffset = (new Vector2(screenPos.x, screenPos.y) - new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
				CameraControl.Inst.targetOrbitQ *= Quaternion.AngleAxis(centerOffset.x * 1.5f, Vector3.up);
				CameraControl.Inst.targetOrbitQ *= Quaternion.AngleAxis(centerOffset.y * 1.5f, -Vector3.right);
				CameraControl.Inst.targetRadius += screenPos.z;

                if( UtopiaGameManager.Inst )
                {
                    // Control avatar
                    PlayerInputManager inputManager = UtopiaGameManager.Inst.LocalPlayer.gameObject.GetComponent<PlayerInputManager>();
                    inputManager.addH += centerOffset.x * 0.4f;
                    inputManager.addV += -centerOffset.y * 0.4f;
                }
			}

			screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
			orbitModeInit = true;
        }
        else if (!CaptureEditorManager.IsEditing && ((Network.peerType == NetworkPeerType.Server) || networkView.isMine))
        {
			orbitModeInit = false;

			// Remote orbit control.
			if(ClientAdminMenu.Inst && ClientAdminMenu.Inst.orbitMode){
				screenPos = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

				// Touch-screen pinch-zoom
				if(Input.touchCount >= 2){
					pinchRelease = false;

					Vector2 touch0, touch1;
					float pinchDist;
					touch0 = Input.GetTouch(0).position;
					touch1 = Input.GetTouch(1).position;
 
					pinchDist = Vector2.Distance(touch0, touch1);

					if(lastPinchDist != -1)
						screenPos.z -= (pinchDist - lastPinchDist);

					lastPinchDist = pinchDist;
				}else{
					lastPinchDist = -1f;

					if(Input.touchCount == 0)
						pinchRelease = true;
				}
			}

            if(networkView.isMine){
#if UNITY_ANDROID || UNITY_IOS
                if(!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) && ((Input.touchCount == 1) || (Application.platform == RuntimePlatform.WindowsEditor)))
#else
                if (!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
#endif
                    screenPos += new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * 10f;
            }

            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);

            Ray cursorRay = Camera.main.ScreenPointToRay(new Vector3(screenPosSmoothed.x, Screen.height - screenPosSmoothed.y, 0f));
            cursorObject.position = cursorRay.origin + cursorRay.direction * 1f;

			if(!lassoSelectEnabled)
                Network.SetSendingEnabled(0, true);


            if(lassoSelectEnabled && networkView.isMine && Input.GetMouseButton(0) && !selecting){
                selecting = true;
                Network.SetSendingEnabled(0, selecting);
                networkView.RPC("StartSelect", RPCMode.Server);
            }

            if(networkView.isMine && !Input.GetMouseButton(0) && selecting){
                selecting = false;
                networkView.RPC("StopSelect", RPCMode.Server);

                // don't send packets while the client is not selecting anything
                Network.SetSendingEnabled(0, selecting);
            }



            // Collect points for gestural control.
            if(selecting)
            {
                float distToCurrent = (lastPoints.Count > 0) ? Vector3.Distance(screenPosSmoothed, lastPoints.Last.Value) : 0f;
                if ((lastPoints.Count < 2) || distToCurrent > 10f)
                {
                    lastPoints.AddLast(screenPosSmoothed);
                    lastPointDists.AddLast(distToCurrent);
                    cursorLineDist += distToCurrent;
                    while (cursorLineDist > cursorLineMaxDist && lastPoints.Count > 0)
                    {
                        cursorLineDist -= lastPointDists.First.Value;
                        lastPoints.RemoveFirst();
                        lastPointDists.RemoveFirst();
                    }
                }
            }


            // Determine circled objects
            if(!selecting && (lastPoints.Count > 0)){
                if(Network.peerType == NetworkPeerType.Server){
                    CaptureObject captureObj = null;
                    float distToCam = 9999999f;
                    foreach(CaptureObject someObj in PersistentGameManager.CaptureObjects){
                        Vector3 objScreenPos = Camera.main.WorldToScreenPoint(someObj.Position);
                        objScreenPos.y = Screen.height - objScreenPos.y;

                        float totalAngle = 0f;
                        float lastAngleToJelly = 0f;
                        bool firstElem = true;
                        foreach(Vector2 pt in lastPoints){
                            Vector2 currentVec = new Vector2(objScreenPos.x, objScreenPos.y) - pt;
                            float angleToJelly = Mathf.Atan2(currentVec.x, currentVec.y) * Mathf.Rad2Deg;

                            if(!firstElem)
                                totalAngle += Mathf.DeltaAngle(angleToJelly, lastAngleToJelly);

                            lastAngleToJelly = angleToJelly;
                            firstElem = false;
                        }

                        float angleForgiveness = 40f;
                        float currDistToCam = (someObj.Position - Camera.main.transform.position).sqrMagnitude;
                        if (Mathf.Abs(totalAngle) > (360f - angleForgiveness) && (captureObj == null || currDistToCam < distToCam))
                        {
                            captureObj = someObj;
                            distToCam = currDistToCam;
                        }
                    }
                    if(captureObj != null)
                        HandleCapturedObject(captureObj);

                }

                lastPoints.Clear();
                lastPointDists.Clear();
                cursorLineDist = 0f;
            }

            if(lastPoints.Count > 2){
                cursorLine.SetVertexCount(lastPoints.Count + 1);
                int count = 0;
                foreach (Vector2 pt in lastPoints){
                    Ray pointRay = Camera.main.ScreenPointToRay(new Vector3(pt.x, Screen.height - pt.y, 0f));
                    cursorLine.SetPosition(count, pointRay.origin + pointRay.direction * 1f);
                    ++count;
                }
                cursorLine.SetPosition(lastPoints.Count, cursorObject.position);

                cursorLine.enabled = true;
            }
            else
                cursorLine.enabled = false;
        }

    } // End of Update().

    void OnLevelWasLoaded(int level)
    {
        if(PersistentGameManager.IsServer)
        {
            // for some reason flares go crazy on level switching, disabling and re-enabling fixes, not sure why the problem occurs.
            enabled = false;
            levelWasLoadedFrame = Time.frameCount;
            Invoke("ReEnable", 0.1f);
        }

    } // End of OnLevelWasLoaded().

    void ReEnable()
    {
        if (Time.frameCount > levelWasLoadedFrame)
            enabled = true;
        else
            Invoke("ReEnable", 0.1f);
    } // End of ReEnable().

    public void HandleCapturedObject(CaptureObject capturedObj)
    {
        PersistentGameManager.Inst.EnviroImpulse(capturedObj.Position, -30f);
        
        Jellyfish j = capturedObj as Jellyfish;
        if (j != null){
            networkView.RPC("CaptureJelly", networkView.owner, j.creator.headNum, j.creator.tailNum, j.creator.boballNum, j.creator.wingNum);
        }
        else{
			Assembly a = capturedObj as Assembly;

            if( a != null )
            {
                a.SaveFamilyTree();
                networkView.RPC("CaptureAssembly", networkView.owner, (a).ToFileString());

				// Single-player
				if(PersistentGameManager.Inst.singlePlayer){
					PersistentGameManager.Inst.serverCapturedAssem = (a).ToFileString();
					string validFilename = IOHelper.GetValidFileName("./data/", "env", ".txt");
					EnvironmentManager.Save(validFilename);
					PersistentGameManager.Inst.capturedWorldFilename = validFilename.Substring(7);
					Application.LoadLevel("CaptureClient");
				}

				// Clear mating data
				if(a.matingWith){
					a.matingWith.matingWith = null;
					a.matingWith = null;
				}
            }
            else
                networkView.RPC("CaptureUCreature", networkView.owner);
        }

        Instantiate(PersistentGameManager.Inst.pingBurstObj, capturedObj.Position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(PersistentGameManager.Inst.captureClip, capturedObj.Position);
        editing = true;
        //capturedObj.Destroy();
        capturedObj.Destroy();
    }

    [RPC] // Server receives this message
    void StartSelect(){
        selecting = true;
    } // End of StartSelect().

    [RPC] // Server receives this message
    void StopSelect(){
        selecting = false;
    } // End of StopSelect().

    [RPC] // Client receives this when it captures a jelly.
    void CaptureJelly(int head, int tail, int bobble, int wing){
        foreach(Jellyfish someJelly in Jellyfish.all)
            someJelly.Destroy();

        AudioSource.PlayClipAtPoint(PersistentGameManager.Inst.captureClip, Vector3.zero);
        Transform newJellyTrans = Instantiate(JellyfishPrefabManager.Inst.jellyfish, Vector3.zero, Random.rotation) as Transform;
        JellyFishCreator newJellyCreator = newJellyTrans.GetComponent<JellyFishCreator>();
        newJellyCreator.changeHead(head);
        newJellyCreator.changeTail(tail);
        newJellyCreator.changeBoball(bobble);
        newJellyCreator.smallTail(wing);
        CaptureEditorManager.capturedObj = newJellyTrans.GetComponent<Jellyfish>();

		AssemblyRadar.Inst.ClearSelectedBlip();


    } // End of CaptureJelly().

    [RPC] // Client receives this when it captures an assembly.
    void CaptureAssembly(string assemblyStr)
    {
        float distFromCamToSpawn = 5.0f;
        AudioSource.PlayClipAtPoint(JellyfishPrefabManager.Inst.pingClip, Vector3.zero);
        Assembly a = new Assembly(assemblyStr, null, null);
		CameraControl.Inst.selectedAssembly = a;
        a.spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * distFromCamToSpawn;
        CaptureEditorManager.capturedObj = a;

		AssemblyRadar.Inst.ClearSelectedBlip();

    } // End of CaptureAssembly().

    [RPC] // Client receives this when it captures a Utopia Creature
    void CaptureUCreature()
    {
        float distFromCamToSpawn = 5.0f;
        AudioSource.PlayClipAtPoint(JellyfishPrefabManager.Inst.pingClip, Vector3.zero);
        SpringCreature c = new SpringCreature();
        c.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distFromCamToSpawn;
        CaptureEditorManager.capturedObj = c;

		AssemblyRadar.Inst.ClearSelectedBlip();

    } // End of CaptureAssembly().

    // Client calls this to send request to server
    public void RequestToggleOrbitMode()
    {
		print("RequestToggleOrbitMode()");
        networkView.RPC("ToggleOrbitMode", RPCMode.Server, networkView.owner);
    }

    [RPC] // Server receives this request from client
    void ToggleOrbitMode(NetworkPlayer player)
    {
		print("ToggleOrbitMode()");
        ToggleOrbitPlayer(player);
    }

    void OnGUI(){
        /*
        if(!PersistentGameManager.IsClient){
            Rect playerInfoRect = new Rect(screenPosSmoothed.x, screenPosSmoothed.y, 500f, 500f);
            GUI.Label(playerInfoRect, " Player");
        }
        */
    } // End of OnGUI().


    void FixedUpdate(){
        // Jellyfish affect
        /*
        foreach(Jellyfish someJelly in Jellyfish.all){
            Vector3 vecToJelly = someJelly.transform.position - transform.position;
            someJelly.rigidbody.AddForce(-vecToJelly.normalized * (5f / (vecToJelly.magnitude * 10f)), ForceMode.Force);
        }
        */
    } // End of FixedUpdate().



    [RPC]
    void Ping(){
        /*
        Instantiate(PersistentGameManager.Inst.pingBurstObj, transform.position, Quaternion.identity);
        foreach(Jellyfish someJelly in Jellyfish.all){
            Vector3 vecToJelly = someJelly.transform.position - transform.position;
            someJelly.rigidbody.AddForce(vecToJelly.normalized * (10f / (vecToJelly.magnitude * 10f)), ForceMode.Impulse);
        }
        */
    } // End of Ping().


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){

        Vector3 screenRelativePos = Vector3.zero;

		// When sending my own data out...
		if(stream.isWriting){

            screenRelativePos = new Vector3(screenPos.x / Screen.width, screenPos.y / Screen.height, screenPos.z);

			stream.Serialize(ref screenRelativePos);

            // after initializing the position, don't send updates until selecting.
            if (!initialPosSent)
            {
                initialPosSent = true;
                if( PersistentGameManager.IsClient )
                    Network.SetSendingEnabled(0, selecting);
            }
		}
		// When receiving data from someone else...
		else{
            editing = false;

            stream.Serialize(ref screenRelativePos);
            screenPos = new Vector3(screenRelativePos.x * Screen.width, screenRelativePos.y * Screen.height, screenRelativePos.z);

            if (PersistentGameManager.IsServer && NodeController.Inst)
                NodeController.Inst.lastPlayerActivityTime = System.DateTime.Now;
		}
    } // End of OnSerializeNetworkView().

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Destroy(gameObject);
    }

    public void ToggleOrbitPlayer(NetworkPlayer player)
    {
        if (CaptureNet_Manager.Inst.orbitPlayers.Contains(player))
            CaptureNet_Manager.Inst.orbitPlayers.Remove(player);
        else
            CaptureNet_Manager.Inst.orbitPlayers.Add(player);
    }

    // Notifies server when a player sync object is spawned
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            PersistentGameManager.Inst.captureMgr.playerSync = this;
    }

	void OnDestroy(){
		all.Remove(this);
	} // End of OnDestroy().

} // End of PlayerSync.
