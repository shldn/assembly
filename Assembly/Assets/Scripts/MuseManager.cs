using UnityEngine;
using System;
using System.Collections.Generic;

public class MuseManager : MonoBehaviour {

    public static MuseManager Inst = null;

    // Local variables
    private bool touchingForehead = false;
    private float lastConcentrationMeasure = 0f;
    private float batteryLevel = 1.0f;
    private List<int> headConnectionStatus = new List<int>() {0,0,0,0};
    private DateTime timeOfLastMessage = DateTime.Now;


    // Accessors
    public bool TouchingForehead{get{return touchingForehead;}}
    public float LastConcentrationMeasure{get{return lastConcentrationMeasure;}}
    public float SecondsSinceLastMessage { get { return (float)(DateTime.Now - timeOfLastMessage).TotalSeconds; } }
    // float (0-1)
    public float BatteryPercentage { get { return batteryLevel; } }
    // 4 ints for the 4 sensors --  0 = bad, 1 = good
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
        else if (packet.Address.Contains("is_good"))
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
    // 0 = bad, 1 = good
    void HandleHeadConnectMessage(List<object> data)
    {
        for (int i = 0; i < headConnectionStatus.Count && i < data.Count; ++i)
            headConnectionStatus[i] = (int)data[i];
    }

    void HandleBatteryStatus(List<object> data)
    {
        batteryLevel = ((int)data[0]) / 10000f;
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.skin.label.alignment = TextAnchor.LowerCenter;
        string statusStr = SecondsSinceLastMessage > 1 ? (SecondsSinceLastMessage.ToString() + " secs since last msg") : ""; // headConnectionStatus[0] + " " + headConnectionStatus[1] + " " + headConnectionStatus[2] + " " + headConnectionStatus[3];
        GUI.Label(new Rect(0.25f * Screen.width, 0.25f * Screen.height, 0.5f * Screen.width, 0.5f * Screen.height), touchingForehead ? statusStr : (statusStr + "\nNo device detected."));
    }
}
