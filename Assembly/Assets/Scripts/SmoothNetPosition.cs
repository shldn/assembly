using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SmoothNetPosition : MonoBehaviour {

	public static List<SmoothNetPosition> allFingertips = new List<SmoothNetPosition>();
	int id = 0;
	Vector3 targetPosition = Vector3.zero;
    bool render = true;
	public bool Render {
        get { return render; }
        set {
            if(value != render) {
                render = value;
                GetComponent<Renderer>().enabled = value;
                Transform child = transform.GetChild(0);
                if (child != null)
                    child.gameObject.SetActive(value);
            }
        }
    }

	public NetworkView netView;
	Vector3 posVel = Vector3.zero;

	void Awake() {
		netView = GetComponent<NetworkView>();

		id = allFingertips.Count;
		allFingertips.Add(this);

        Render = false;

		if(id < 10)
			transform.localScale = Vector3.one * 100f;
	} // End of Awake().


	void Update() {
		if(netView.isMine)
			targetPosition = transform.position;
		else
			transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref posVel, 0.2f);

		GetComponent<Renderer>().enabled = Render;
        Transform child = transform.GetChild(0);
        if (!Render && child != null && child != null)
            child.gameObject.SetActive(Render);

		Render = false;

	} // End of Update().


	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){

		Vector3 transferPos = targetPosition;
		bool netRender = false;

		// When sending my own data out...
		if(stream.isWriting){
			transferPos = targetPosition;
			netRender = Render;

			stream.Serialize(ref transferPos);
			stream.Serialize(ref netRender);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref transferPos);
			stream.Serialize(ref netRender);

			targetPosition = transferPos;
			Render = netRender;
		}
    } // End of OnSerializeNetworkView().

} // End of SmoothNetPosition.
