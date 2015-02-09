using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSync : MonoBehaviour {

    Vector3 screenPos = Vector3.zero;
    public Vector3 screenPosSmoothed = Vector3.zero;
    Vector3 screenPosVel = Vector3.zero;
    float screenPosSmoothTime = 0.1f;

    List<Vector2> lastPoints = new List<Vector2>();
    bool selecting = false;
    bool initialPosSent = false;

    public Transform cursorObject;
    public LineRenderer cursorLine;

    bool editing = false;

    // orbit option
    HashSet<NetworkPlayer> orbitPlayers = new HashSet<NetworkPlayer>();

    void Awake()
    {
        screenPos = new Vector3(0.5f * Screen.width, 0.5f * Screen.height, 0.0f);
        screenPosSmoothed = screenPos;
        DontDestroyOnLoad(this);
    }

    void LateUpdate(){
        screenPosSmoothed = Vector3.SmoothDamp(screenPosSmoothed, screenPos, ref screenPosVel, screenPosSmoothTime);

        if(cursorObject){
            cursorObject.gameObject.SetActive(!editing);
        }

        if(cursorObject && (PersistentGameManager.IsClient) && !networkView.isMine)
            Destroy(cursorObject.gameObject);

        if (!CaptureEditorManager.IsEditing && ((Network.peerType == NetworkPeerType.Server) || networkView.isMine))
        {
            if(networkView.isMine){
                if(!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
                    screenPos += new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * 10f;
            }

            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);

            Ray cursorRay = Camera.main.ScreenPointToRay(new Vector3(screenPosSmoothed.x, Screen.height - screenPosSmoothed.y, 0f));
            cursorObject.position = cursorRay.origin + cursorRay.direction * 1f;

            if (orbitPlayers.Contains(networkView.owner))
            {
                // handle camera orbiting for these players screenPos movements here
            }

            if(networkView.isMine && Input.GetMouseButton(0) && !selecting){
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
                if((lastPoints.Count < 2) || (Vector3.Distance(screenPosSmoothed, lastPoints[lastPoints.Count - 1]) > 10f))
                    lastPoints.Add(screenPosSmoothed);

            // Determine circled objects
            if(!selecting && (lastPoints.Count > 0)){
                if(Network.peerType == NetworkPeerType.Server){
                    foreach(CaptureObject someObj in PersistentGameManager.CaptureObjects){
                        Vector3 objScreenPos = Camera.main.WorldToScreenPoint(someObj.Position);
                        objScreenPos.y = Screen.height - objScreenPos.y;

                        float totalAngle = 0f;
                        float lastAngleToJelly = 0f;
                        for(int i = 0; i < lastPoints.Count; i++){
                            Vector2 currentVec = new Vector2(objScreenPos.x, objScreenPos.y) - lastPoints[i];
                            float angleToJelly = Mathf.Atan2(currentVec.x, currentVec.y) * Mathf.Rad2Deg;

                            if(i > 0)
                                totalAngle += Mathf.DeltaAngle(angleToJelly, lastAngleToJelly);

                            lastAngleToJelly = angleToJelly;
                        }

                        float angleForgiveness = 40f;
                        if(Mathf.Abs(totalAngle) > (360f - angleForgiveness)){
                            HandleCapturedObject(someObj);
                            break;
                        }
                    }
                }

                lastPoints.Clear();
            }

            if(lastPoints.Count > 2){
                cursorLine.SetVertexCount(lastPoints.Count + 1);
                for(int i = 0; i < lastPoints.Count; i++){
                    Ray pointRay = Camera.main.ScreenPointToRay(new Vector3(lastPoints[i].x, Screen.height - lastPoints[i].y, 0f));
                    cursorLine.SetPosition(i, pointRay.origin + pointRay.direction * 1f);
                }
                cursorLine.SetPosition(lastPoints.Count, cursorObject.position);

                cursorLine.enabled = true;
            }
            else
                cursorLine.enabled = false;
        }

    } // End of Update().

    void HandleCapturedObject(CaptureObject capturedObj)
    {
        PersistentGameManager.Inst.EnviroImpulse(capturedObj.Position, -30f);
        
        Jellyfish j = capturedObj as Jellyfish;
        if (j != null){
            networkView.RPC("CaptureJelly", networkView.owner, j.creator.headNum, j.creator.tailNum, j.creator.boballNum, j.creator.wingNum);
        }
        else{
            networkView.RPC("CaptureAssembly", networkView.owner, ((Assembly)capturedObj).ToFileString());
        }

        Instantiate(PersistentGameManager.Inst.pingBurstObj, capturedObj.Position, Quaternion.identity);
        AudioSource.PlayClipAtPoint(PersistentGameManager.Inst.captureClip, capturedObj.Position);
        editing = true;
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

    } // End of CaptureJelly().

    [RPC] // Client receives this when it captures an assembly.
    void CaptureAssembly(string assemblyStr)
    {
        float distFromCamToSpawn = 5.0f;
        AudioSource.PlayClipAtPoint(JellyfishPrefabManager.Inst.pingClip, Vector3.zero);
        Assembly a = new Assembly(assemblyStr);
        a.WorldPosition = Camera.main.transform.position + Camera.main.transform.forward * distFromCamToSpawn;
        CaptureEditorManager.capturedObj = a;
    } // End of CaptureAssembly().

    // Client calls this to send request to server
    public void RequestToggleOrbitMode()
    {
        networkView.RPC("ToggleOrbitMode", RPCMode.Server, networkView.owner);
    }

    [RPC] // Server receives this request from client
    void ToggleOrbitMode(NetworkPlayer player)
    {
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

            screenRelativePos = new Vector3(screenPos.x / Screen.width, screenPos.y / Screen.height);

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
            screenPos = new Vector3(screenRelativePos.x * Screen.width, screenRelativePos.y * Screen.height);
		}
    } // End of OnSerializeNetworkView().

    void OnDisconnectedFromServer(NetworkDisconnection info)
    {
        Destroy(gameObject);
    }

    public void ToggleOrbitPlayer(NetworkPlayer player)
    {
        if (orbitPlayers.Contains(player))
            orbitPlayers.Remove(player);
        else
            orbitPlayers.Add(player);
    }

    // Notifies server when a player sync object is spawned
    void OnNetworkInstantiate(NetworkMessageInfo info)
    {
        if (Network.isServer)
            PersistentGameManager.Inst.captureMgr.playerSync = this;
    }

} // End of PlayerSync.
