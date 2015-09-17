using UnityEngine;
using System.Collections;

//--------------------------------------------------------//
// NeuroScaleDisplay
//
// Subscribes to messages received by the NeuroScaleManager
// and displays them to the screen in the GUI layer.
//
// Arthur C. Clarke Center for Human Imagination, UCSD
//--------------------------------------------------------//
public class NeuroScaleAttentionDisplay : MonoBehaviour
{

    private double lastAttention = 0;

    void Awake()
    {
        NeuroScaleManager.Inst.Messages += OnMessage;
    }

    double GetAttention(string msg)
    {
        int idx = msg.IndexOf("samples");
        int startIdx = msg.LastIndexOf('[',idx+20, 20) + 1;
        int endIdx = msg.IndexOf(']',idx);

        string attentionStr = msg.Substring(startIdx, endIdx - startIdx);
        double result = 0;
        double.TryParse(attentionStr, out result);
        return result;
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        lastAttention = GetAttention(e.msg);
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 25;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(0.25f * Screen.width, 0.25f * Screen.height, 0.5f * Screen.width, 0.5f * Screen.height), lastAttention.ToString());

        GUI.skin.label.fontSize = 15;
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(new Rect(Screen.width - 300, Screen.height - 90, 300, 30), NeuroScaleManager.Inst.Connected ? "Connected" : "Connecting...");
        GUI.TextField(new Rect(Screen.width - 300, Screen.height - 30, 300, 30), NeuroScaleManager.Inst.PublishTopic, 100);
    }

}
