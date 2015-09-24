using UnityEngine;
using System.Collections;

public class Attention {

    private double lastAttention = 0;
    public double Value { get { return lastAttention; } }

    public Attention() {
        NeuroScaleManager.Inst.Messages += OnMessage;
    }

    double GetAttention(string msg)
    {
        int idx = msg.IndexOf("samples");
        int startIdx = msg.LastIndexOf('[', idx + 20, 20) + 1;
        int endIdx = msg.IndexOf(']', idx);

        string attentionStr = msg.Substring(startIdx, endIdx - startIdx);
        double result = 0;
        double.TryParse(attentionStr, out result);
        return result;
    }

    void OnMessage(object sender, MessageEventArgs e)
    {
        lastAttention = GetAttention(e.msg);
    }
}
