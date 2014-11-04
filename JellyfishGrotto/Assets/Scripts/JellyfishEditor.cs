﻿using UnityEngine;
using System.Collections;

public class JellyfishEditor : MonoBehaviour {

	// Use this for initialization
	void Awake() {
	    if(Application.platform != RuntimePlatform.Android)
            enabled = false;
	}
	
	// Update is called once per frame
	void OnGUI() {
        if(Jellyfish.all[0]){
	        Rect controlBarRect = new Rect(Screen.width - (Screen.height / 6f), 0f, Screen.height / 6f, Screen.height);

            GUI.skin.button.fontSize = 20;

            JellyFishCreator creator = Jellyfish.all[0].GetComponent<JellyFishCreator>();

            GUILayout.BeginArea(controlBarRect);
            if(GUILayout.Button("Head " + creator.headNum, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
                Jellyfish.all[0].NextHead();
            }
            if(GUILayout.Button("Tail " + creator.tailNum, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
                Jellyfish.all[0].NextTail();
            }
            if(GUILayout.Button("Bobbles " + creator.boballNum, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
                Jellyfish.all[0].NextBobble();
            }
            if(GUILayout.Button("Wings " + creator.wingNum, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true))){
                Jellyfish.all[0].NextWing();
            }

            GUILayout.Space(Screen.height / 6f);

            if(Network.peerType == NetworkPeerType.Disconnected){
                Jellyfish.all[0].Destroy();
                GameManager.Inst.editing = false;
            }

            if(GUILayout.Button("Done", GUILayout.ExpandHeight(true))){
                Net_Manager.myNetworkView.RPC("PushJelly", RPCMode.Server, creator.headNum, creator.tailNum, creator.boballNum, creator.wingNum);
                Instantiate(PrefabManager.Inst.pingBurst, Jellyfish.all[0].transform.position, Quaternion.identity);
                Jellyfish.all[0].Destroy();
                GameManager.Inst.editing = false;
            }
            GUILayout.EndArea();
        }
	} // End of OnGUI().

} // End of JellyfishEditor.
