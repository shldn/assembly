using UnityEngine;
using System.Collections;
using LSL;


public class MuseLSLManager : MonoBehaviour {

    private static MuseLSLManager inst = null;
    public static MuseLSLManager Inst{
        get{
            if(inst == null)
                inst = (new GameObject("MuseLSLManager")).AddComponent<MuseLSLManager>();
            return inst;
        }
    }

    private string controlStream = "ConcentrationStream";
    private liblsl.StreamInlet inlet;
    private liblsl.ContinuousResolver resolver;
    private float lastConcentrationMetric = 0f;

    // Accessors
    public bool IsConnected { get { return inlet != null; } }
    public float LastConcentrationMetric { get { return lastConcentrationMetric; } }


    void Start() {
        Debug.Log("Creating LSL resolver for stream " + controlStream);
        resolver = new liblsl.ContinuousResolver("name", controlStream);
    }

    void Update() {
        // if not yet connected, search for a stream...
        if (inlet == null && resolver != null) {
            // Looking for the stream named controlStream.ToString()
            liblsl.StreamInfo[] results = resolver.results();
            if (results.Length > 0) {
                inlet = new liblsl.StreamInlet(results[0]);
                Debug.Log("connected.");
            }
        }

        // read all new values since last query
        if (inlet != null) {
            float[] sample = new float[1];
            if (inlet.pull_sample(sample, 0.0f) != 0) {
                while (inlet.pull_sample(sample, 0.0f) != 0) ; // make sure we consume all samples so we don't start to lag
                lastConcentrationMetric = sample[0];
            }
        }
    }

    void OnDestroy() {
        inst = null;
    }


    //void OnGUI() {

    //    // Sensor displays
    //    float sensorRingSize = 50f;
    //    float sensorRingSpacing = 10f;
    //    GUI.skin.label.fontSize = 14;
    //    GUI.skin.label.alignment = TextAnchor.LowerCenter;

    //    string statusStr = "";
    //    // Attention metric
    //    if (IsConnected) {
    //        statusStr += (lastConcentrationMetric * 100f).ToString("F0") + "%";
    //        GUI.Label(MathUtilities.CenteredSquare(Screen.width * 0.5f, 505f, 1000f), statusStr);
    //    }
    //}
}
