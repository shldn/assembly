using UnityEngine;
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
    public float LastConcentrationMeasure{get{return !invertConcentration? lastConcentrationMeasure : 1f - lastConcentrationMeasure;}}
    public float SecondsSinceLastMessage { get { return (float)(DateTime.Now - timeOfLastMessage).TotalSeconds; } }
    // float (0-1)
    public float BatteryPercentage { get { return batteryLevel; } }
    // 4 ints for the 4 sensors -- 1 = good, 2 = ok, >=3 bad
    public List<int> HeadConnectionStatus { get { return headConnectionStatus; } }


	bool invertConcentration = false;
	bool slowResponse = false;
	public bool SlowResponse {get{return slowResponse;}}


    void Awake(){
        Inst = this;
    }

	void Start () {
		OSCHandler.Instance.Init ();
        OSCHandler.Instance.Servers["AssemblyOSC"].server.PacketReceivedEvent += Server_PacketReceivedEvent;
    }

	void Update () {
		if(Input.GetKeyDown(KeyCode.RightAlt))
			invertConcentration = !invertConcentration;
		if(Input.GetKeyDown(KeyCode.S))
			slowResponse = !slowResponse;
	}
    
    private void Server_PacketReceivedEvent(UnityOSC.OSCServer sender, UnityOSC.OSCPacket packet)
    {
        //Debug.Log("packet: " + packet.Address + " " + OSCHandler.DataToString(packet.Data));
        if (packet.Address.Contains("touching_forehead"))
            touchingForehead = ((int)packet.Data[0] != 0);
        else if (packet.Address.Contains("concentration"))
            HandleConcentrationSample((float)packet.Data[0]);
        else if (packet.Address.Contains("horseshoe"))
            HandleHeadConnectMessage(packet.Data);
        else if (packet.Address.Contains("batt"))
            HandleBatteryStatus(packet.Data);
        timeOfLastMessage = DateTime.Now;
    }

    void HandleConcentrationSample(float sample)
    {
        lastConcentrationMeasure = sample;
    }

    // These are status messages for connection to the user's head.
    // 1 = good, 2 = ok, >=3 bad
    void HandleHeadConnectMessage(List<object> data)
    {
        for (int i = 0; i < headConnectionStatus.Count && i < data.Count; ++i)
            headConnectionStatus[i] = (int)((float)data[i]);
    }

    void HandleBatteryStatus(List<object> data)
    {
        batteryLevel = ((int)data[0]) / 10000f;
    }

	float[] sensorIndSizes = new float[4];
	float[] sensorIndSizeVels = new float[4];

	float wearingHeadsetIndication = 0f;
	float wearingHeadsetIndicationVel = 0f;

    void OnGUI()
    {
		wearingHeadsetIndication = Mathf.SmoothDamp(wearingHeadsetIndication, TouchingForehead? 1f : 0f, ref wearingHeadsetIndicationVel, 1f);

		// Sensor displays
		float sensorRingSize = 50f * wearingHeadsetIndication;
		float sensorRingSpacing = 10f;
		for(int i = 0; i < 4; i++){
			Vector2 sensorRectCenter = new Vector2(((Screen.width * 0.5f) - ((sensorRingSize * 1.5f) + (sensorRingSpacing * 1.5f))) + (i * (sensorRingSize + sensorRingSpacing)), (sensorRingSpacing + (sensorRingSize * 0.5f)));
			Rect sensorRect = MathUtilities.CenteredSquare(sensorRectCenter.x, sensorRectCenter.y, sensorRingSize);
			GUI.DrawTexture(sensorRect, sensorDisplayOut);

			Rect sensorStatusRect = MathUtilities.CenteredSquare(sensorRectCenter.x, sensorRectCenter.y, sensorRingSize * Mathf.InverseLerp(3f, 1f, sensorIndSizes[i]));
			GUI.DrawTexture(sensorStatusRect, sensorDisplayIn);

			sensorIndSizes[i] = Mathf.SmoothDamp(sensorIndSizes[i], HeadConnectionStatus[i], ref sensorIndSizeVels[i], 0.25f);
		}

        GUI.skin.label.fontSize = 14;
        GUI.skin.label.alignment = TextAnchor.LowerCenter;

		string statusStr = "";
		// Attention metric
		if(SecondsSinceLastMessage < 1f){
			if(touchingForehead)
				statusStr += (NeuroScaleDemo.Inst.enviroScale * 100f).ToString("F0") + "%";
			else
				statusStr += "EEG device ready.";
		}

		if(invertConcentration)
			statusStr += " ~";

		if(slowResponse)
			statusStr += " s";

		if(BatteryPercentage < 0.2f)
			statusStr += "\n" + (BatteryPercentage * 100f).ToString("F0") + "% battery remaining.";


        GUI.Label(MathUtilities.CenteredSquare(Screen.width * 0.5f, ((sensorRingSize + (sensorRingSpacing)) * Mathf.Sqrt(wearingHeadsetIndication)) + 505f, 1000f), statusStr);
    }
}
