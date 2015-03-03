using UnityEngine;
using System.Collections;

public class DisplayIP : MonoBehaviour {
    bool display = false;

	void OnGUI () {
        GUI.skin.label.fontSize = 30;
        GUI.Label(new Rect(0,0,Screen.width, Screen.height), Network.player.ipAddress);
	}
}
