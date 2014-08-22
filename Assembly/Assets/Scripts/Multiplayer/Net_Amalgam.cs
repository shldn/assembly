using UnityEngine;
using System.Collections;

public class Net_Amalgam : MonoBehaviour {

	Vector3 playerPosition;


	void Start(){
	} // End of Start().
	

	void Update(){

        if(networkView.isMine){
            transform.position = MainCameraControl.Inst.transform.position;
        }
        else{
            transform.position = playerPosition;
        }

	} // End of Update().
	
        // Synchronizes contantly-updating things between mechs.
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
	    float xPos = 0;
        float yPos = 0;
        float zPos = 0;

		// When sending my own data out...
		if(stream.isWriting){
			xPos = transform.position.x;
			yPos = transform.position.y;
			zPos = transform.position.z;
			    
			stream.Serialize(ref xPos);
            stream.Serialize(ref yPos);
            stream.Serialize(ref zPos);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref xPos);
            stream.Serialize(ref yPos);
            stream.Serialize(ref zPos);
			
			if(!float.IsNaN(xPos) && !float.IsNaN(yPos) && !float.IsNaN(zPos)){
				playerPosition.x = xPos;
				playerPosition.y = yPos;
				playerPosition.z = zPos;
			}
		}
    } // End of OnSerializeNetworkView().

} // End of Net_Amalgam.
