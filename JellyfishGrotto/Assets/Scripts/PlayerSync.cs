using UnityEngine;
using System.Collections;

public class PlayerSync : MonoBehaviour {

    Vector3 inputPos = Vector3.zero;
    public Vector3 inputPosSmoothed = Vector3.zero;
    Vector3 inputPosVel = Vector3.zero;
    float inputSmoothTime = 0.1f;

    void Update(){
        inputPosSmoothed = Vector3.SmoothDamp(inputPosSmoothed, inputPos, ref inputPosVel, inputSmoothTime);

        if(networkView.isMine)
            inputPos = new Vector3(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);

        rigidbody.MovePosition(GameManager.Inst.ScreenToPlayArea(inputPosSmoothed));


        if(networkView.isMine && Input.GetMouseButtonDown(0))
            networkView.RPC("Ping", RPCMode.All);
        
    } // End of Update().


    void FixedUpdate(){
        // Jellyfish affect
        foreach(Jellyfish someJelly in Jellyfish.all){
            Vector3 vecToJelly = someJelly.transform.position - transform.position;
            someJelly.rigidbody.AddForce(-vecToJelly.normalized * (5f / (vecToJelly.magnitude * 10f)), ForceMode.Force);
        }
    } // End of FixedUpdate().



    [RPC]
    void Ping(){
        Instantiate(PrefabManager.Inst.pingBurst, transform.position, Quaternion.identity);
        foreach(Jellyfish someJelly in Jellyfish.all){
            Vector3 vecToJelly = someJelly.transform.position - transform.position;
            someJelly.rigidbody.AddForce(vecToJelly.normalized * (10f / (vecToJelly.magnitude * 10f)), ForceMode.Impulse);
        }
    } // End of Ping().


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){

		// When sending my own data out...
		if(stream.isWriting){
			stream.Serialize(ref inputPos);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref inputPos);
		}
    } // End of OnSerializeNetworkView().

} // End of PlayerSync.
