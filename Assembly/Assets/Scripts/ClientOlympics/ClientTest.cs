﻿using UnityEngine;
using System.Collections;


// An instanced script that runs a test on all current PhysAssemblies, then culls all but the best one.
public class ClientTest : MonoBehaviour {

	public static ClientTest Inst = null;
	protected int runTime = 0;


	protected virtual void Awake(){
		Inst = this;
	} // End of Awake().
	

	// Update is called once per frame
	protected virtual void Update(){
		runTime ++;
	} // End of Update().


	void OnGUI(){
		string progressBar = "[Testing] ";
		for(int i = 0; i < runTime * 0.2f; i++)
			progressBar += "|";
		GUI.skin.label.alignment = TextAnchor.LowerLeft;
		GUI.Label(new Rect(10f, 10f, Screen.width - 20f, Screen.height - 20f), progressBar + " " + (runTime / 5f).ToString("F0") + "%");
	} // End of OnGUI().

} // End of ClientTest.