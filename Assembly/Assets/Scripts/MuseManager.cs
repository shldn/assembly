﻿using UnityEngine;
using System;
using System.Collections.Generic;

public class MuseManager : MonoBehaviour {

    public static MuseManager Inst = null;

	public Texture2D sensorDisplayOut;
	public Texture2D sensorDisplayIn;

    // Local variables
    private bool touchingForehead = false;
    private float lastConcentrationMeasure = 0f;
    private float batteryLevel = 1.0f;
    private List<int> headConnectionStatus = new List<int>() {0,0,0,0};
    private DateTime timeOfLastMessage = DateTime.Now;


    // Accessors
    public bool TouchingForehead{get{return touchingForehead;}}
    public float LastConcentrationMeasure{get{return lastConcentrationMeasure;}}
    public float BatteryPercentage { get { return batteryLevel; } }
    public float SecondsSinceLastMessage { get { return (float)(DateTime.Now - timeOfLastMessage).TotalSeconds; } }
    public List<int> HeadConnectionStatus { get { return headConnectionStatus; } }


    void Awake(){
        Inst = this;
    }

	void Start () {
		OSCHandler.Instance.Init ();
        OSCHandler.Instance.Servers["AssemblyOSC"].server.PacketReceivedEvent += Server_PacketReceivedEvent;
    }
    
    private void Server_PacketReceivedEvent(UnityOSC.OSCServer sender, UnityOSC.OSCPacket packet)
    {
        //Debug.Log("packet: " + packet.Address + " " + OSCHandler.DataToString(packet.Data));
        if (packet.Address.Contains("touching_forehead"))
            touchingForehead = ((int)packet.Data[0] != 0);
        else if (packet.Address.Contains("concentration"))
            HandleConcentrationSample((float)packet.Data[0]);
        else if(packet.Address.Contains("is_good"))
            HandleHeadConnectMessage(packet.Data);
        timeOfLastMessage = DateTime.Now;
    }

    void HandleConcentrationSample(float sample)
    {
        lastConcentrationMeasure = sample;
    }

    // These are status messages for connection to the user's head.
    // 0 = bad, 1 = good
    void HandleHeadConnectMessage(List<object> data)
    {
        for (int i = 0; i < headConnectionStatus.Count && i < data.Count; ++i)
            headConnectionStatus[i] = (int)data[i];
    }

    void HandleBatteryStatus(List<object> data)
    {
        batteryLevel = ((int)data[0]) / 100000f;
    }

	float[] sensorIndSizes = new float[4];
	float[] sensorIndSizeVels = new float[4];

	float wearingHeadsetIndication = 0f;
	float wearingHeadsetIndicationVel = 0f;

    void OnGUI()
    {
		/*
        GUI.skin.label.fontSize = 18;
        GUI.skin.label.alignment = TextAnchor.LowerCenter;
        string statusStr = SecondsSinceLastMessage > 1 ? (SecondsSinceLastMessage.ToString() + " secs since last msg") : ""; // headConnectionStatus[0] + " " + headConnectionStatus[1] + " " + headConnectionStatus[2] + " " + headConnectionStatus[3];
        GUI.Label(new Rect(0.25f * Screen.width, 0.25f * Screen.height, 0.5f * Screen.width, 0.5f * Screen.height), touchingForehead ? statusStr : (statusStr + "\nNo device detected."));
		*/

		wearingHeadsetIndication = Mathf.SmoothDamp(wearingHeadsetIndication, TouchingForehead? 1f : 0f, ref wearingHeadsetIndicationVel, 1f);

		// Sensor displays
		float sensorRingSize = 50f * wearingHeadsetIndication;
		float sensorRingSpacing = 10f;
		for(int i = 0; i < 4; i++){
			Vector2 sensorRectCenter = new Vector2(((Screen.width * 0.5f) - ((sensorRingSize * 1.5f) + (sensorRingSpacing * 1.5f))) + (i * (sensorRingSize + sensorRingSpacing)), (sensorRingSpacing + (sensorRingSize * 0.5f)));
			Rect sensorRect = MathUtilities.CenteredSquare(sensorRectCenter.x, sensorRectCenter.y, sensorRingSize);
			GUI.DrawTexture(sensorRect, sensorDisplayOut);

			Rect sensorStatusRect = MathUtilities.CenteredSquare(sensorRectCenter.x, sensorRectCenter.y, sensorRingSize * Mathf.InverseLerp(3f, 0f, sensorIndSizes[i]));
			GUI.DrawTexture(sensorStatusRect, sensorDisplayIn);

			sensorIndSizes[i] = Mathf.SmoothDamp(sensorIndSizes[i], HeadConnectionStatus[i], ref sensorIndSizeVels[i], 0.25f);
		}

		//GUI.DrawTexture(MathUtilities.CenteredSquare(Screen.width * 0.5f, Screen.height * 0.5f, 100f + (Mathf.Sqrt(NeuroScaleDemo.Inst.enviroScale) * 200f)), sensorDisplayOut);
    }
}
