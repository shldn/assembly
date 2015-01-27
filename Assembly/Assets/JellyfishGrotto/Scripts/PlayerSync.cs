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

    public Transform cursorObject;
    public LineRenderer cursorLine;

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Update(){
        screenPosSmoothed = Vector3.SmoothDamp(screenPosSmoothed, screenPos, ref screenPosVel, screenPosSmoothTime);

        if(cursorObject){
            cursorObject.gameObject.SetActive(!JellyfishGameManager.Inst.editing);
        }

        if(cursorObject && (JellyfishGameManager.IsClient) && !networkView.isMine)
            Destroy(cursorObject.gameObject);

        if (!JellyfishGameManager.Inst.editing && ((Network.peerType == NetworkPeerType.Server) || networkView.isMine))
        {
            if(networkView.isMine){
                if(!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0))
                    screenPos += new Vector3(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * 10f;
            }

            screenPos.x = Mathf.Clamp(screenPos.x, 0f, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0f, Screen.height);

            Ray cursorRay = Camera.main.ScreenPointToRay(new Vector3(screenPosSmoothed.x, Screen.height - screenPosSmoothed.y, 0f));
            cursorObject.position = cursorRay.origin + cursorRay.direction * 1f;


            if(networkView.isMine && Input.GetMouseButton(0) && !selecting){
                networkView.RPC("StartSelect", RPCMode.Server);
                selecting = true;
            }

            if(networkView.isMine && !Input.GetMouseButton(0) && selecting){
                networkView.RPC("StopSelect", RPCMode.Server);
                selecting = false;
            }


            // Collect points for gestural control.
            if(selecting)
                if((lastPoints.Count < 2) || (Vector3.Distance(screenPosSmoothed, lastPoints[lastPoints.Count - 1]) > 10f))
                    lastPoints.Add(screenPosSmoothed);

            // Determine circled jellies
            if(!selecting && (lastPoints.Count > 0)){
                if(Network.peerType == NetworkPeerType.Server){
                    foreach(Jellyfish someJelly in Jellyfish.all){
                        Vector3 jellyScreenPos = Camera.main.WorldToScreenPoint(someJelly.transform.position);
                        jellyScreenPos.y = Screen.height - jellyScreenPos.y;

                        float totalAngle = 0f;
                        float lastAngleToJelly = 0f;
                        for(int i = 0; i < lastPoints.Count; i++){
                            Vector2 currentVec = new Vector2(jellyScreenPos.x, jellyScreenPos.y) - lastPoints[i];
                            float angleToJelly = Mathf.Atan2(currentVec.x, currentVec.y) * Mathf.Rad2Deg;

                            if(i > 0)
                                totalAngle += Mathf.DeltaAngle(angleToJelly, lastAngleToJelly);

                            lastAngleToJelly = angleToJelly;
                        }

                        float angleForgiveness = 40f;
                        if(Mathf.Abs(totalAngle) > (360f - angleForgiveness)){
                            JellyFishCreator someJellyCreator = someJelly.GetComponent<JellyFishCreator>();
                            networkView.RPC("CaptureJelly", networkView.owner, someJellyCreator.headNum, someJellyCreator.tailNum, someJellyCreator.boballNum, someJellyCreator.wingNum);

                            Instantiate(JellyfishPrefabManager.Inst.pingBurst, someJelly.transform.position, Quaternion.identity);

                            print("Sending jelly, " + someJellyCreator.headNum + " " + someJellyCreator.tailNum + " " + someJellyCreator.boballNum + " " + someJellyCreator.wingNum);

                            someJelly.Destroy();
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


    [RPC]
    void StartSelect(){
        selecting = true;
    } // End of StartSelect().

    [RPC]
    void StopSelect(){
        selecting = false;
    } // End of StartSelect().

    [RPC] // Client receives this when it captures a jelly.
    void CaptureJelly(int head, int tail, int bobble, int wing){
        foreach(Jellyfish someJelly in Jellyfish.all)
            someJelly.Destroy();

        AudioSource.PlayClipAtPoint(JellyfishPrefabManager.Inst.pingClip, Vector3.zero);
        Transform newJellyTrans = Instantiate(JellyfishPrefabManager.Inst.jellyfish, Vector3.zero, Random.rotation) as Transform;
        JellyFishCreator newJellyCreator = newJellyTrans.GetComponent<JellyFishCreator>();
        newJellyCreator.changeHead(head);
        newJellyCreator.changeTail(tail);
        newJellyCreator.changeBoball(bobble);
        newJellyCreator.smallTail(wing);

        JellyfishGameManager.Inst.editing = true;
    } // End of StartSelect().


    void OnGUI(){
        /*
        if(!JellyfishGameManager.IsClient){
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
        Instantiate(JellyfishPrefabManager.Inst.pingBurst, transform.position, Quaternion.identity);
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
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref screenRelativePos);

            screenPos = new Vector3(screenRelativePos.x * Screen.width, screenRelativePos.y * Screen.height);
		}
    } // End of OnSerializeNetworkView().

} // End of PlayerSync.
