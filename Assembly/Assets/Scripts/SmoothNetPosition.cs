using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmoothNetPosition : MonoBehaviour {

	public static List<SmoothNetPosition> allFingertips = new List<SmoothNetPosition>();
	int id = 0;
	Vector3 targetPosition = Vector3.zero;
	public bool render = false;

	public NetworkView netView;
	Vector3 posVel = Vector3.zero;

	void Awake() {
		netView = GetComponent<NetworkView>();

		id = allFingertips.Count;
		allFingertips.Add(this);

		if(id < 10)
			transform.localScale = Vector3.one * 100f;
	} // End of Awake().


	void Update() {
		if(netView.isMine)
			targetPosition = transform.position;
		else
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref posVel, 0.2f);

		GetComponent<Renderer>().enabled = render;
		render = false;

	} // End of Update().


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){

		Vector3 transferPos = Vector3.zero;
		bool netRender = false;

		// When sending my own data out...
		if(stream.isWriting){
			transferPos = targetPosition;
			netRender = render;

			stream.Serialize(ref transferPos);
			stream.Serialize(ref netRender);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref transferPos);
			stream.Serialize(ref netRender);

			targetPosition = transferPos;
			render = netRender;
		}
    } // End of OnSerializeNetworkView().

} // End of SmoothNetPosition.
