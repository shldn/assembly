using UnityEngine;
using System.Collections;

public class Net_Amalgam : MonoBehaviour {

    public TextMesh nametag;

	void Update(){

        for(int i = 0; i < Net_Manager.playerList.Length; i++){
            if(Net_Manager.playerList[i].player == networkView.owner)
                nametag.text = Net_Manager.playerList[i].playerName;
        }

        if(networkView.isMine){
            transform.position = MainCameraControl.Inst.transform.position;
            transform.rotation = MainCameraControl.Inst.transform.rotation;

            Renderer[] allRenderers = transform.GetComponentsInChildren<Renderer>();
            foreach(Renderer someRenderer in allRenderers)
                someRenderer.enabled = false;
        }

	} // End of Update().
	
        // Synchronizes contantly-updating things between mechs.
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
	    Vector3 pos = Vector3.zero;
	    Quaternion rot = Quaternion.identity;

		// When sending my own data out...
		if(stream.isWriting){
			pos = transform.position;
            rot = transform.rotation;
			    
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			
            transform.position = pos;
            transform.rotation = rot;
		}
    } // End of OnSerializeNetworkView().

} // End of Net_Amalgam.
