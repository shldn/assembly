using UnityEngine;
using System.Collections;

public class MuseManager : MonoBehaviour {

    public static MuseManager Inst = null;

    // Local variables
    private bool touchingForehead = false;
    private float lastConcentrationMeasure = 0f;


    // Accessors
    public bool TouchingForehead{get{return touchingForehead;}}
    public float LastConcentrationMeasure{get{return lastConcentrationMeasure;}}


    void Awake(){
        Inst = this;
    }

	void Start () {
		OSCHandler.Instance.Init ();
        OSCHandler.Instance.Servers["AssemblyOSC"].server.PacketReceivedEvent += Server_PacketReceivedEvent;
    }
    
    private void Server_PacketReceivedEvent(UnityOSC.OSCServer sender, UnityOSC.OSCPacket packet)
    {
        if (packet.Address.Contains("touching_forehead"))
            touchingForehead = ((int)packet.Data[0] != 0);
        else if (packet.Address.Contains("concentration"))
            HandleConcentrationSample((float)packet.Data[0]);

    }

    void HandleConcentrationSample(float sample)
    {
        lastConcentrationMeasure = sample;
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 18;
        GUI.skin.label.alignment = TextAnchor.LowerCenter;
        GUI.Label(new Rect(0.25f * Screen.width, 0.25f * Screen.height, 0.5f * Screen.width, 0.5f * Screen.height), touchingForehead ? "" : "No device detected.");
    }
}
