using UnityEngine;
using System.Collections;

public class DiagnosticHUD : MonoBehaviour {

    // FPS Calculation
    private float fpsUpdateInterval = 0.1f;
    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval
    private float fps = 0f;

    // Memory Calculation
    private float memUpdateInterval = 1f;
    private float memTimeLeft;
    private float memUsage = 0;

    // Num GameObjects
    private float goUpdateInterval = 5f;
    private float goTimeLeft;
    private float goCount = 0;

    // Log interval
    private float logInterval = 0.5f * 60f * 60f;
    private float lastLogTime = 0f;


    void Update()
    {

        // FPS
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            fps = accum / frames;

            timeleft = fpsUpdateInterval;
            accum = 0.0F;
            frames = 0;

        }

        // Memory
        memTimeLeft -= Time.deltaTime;
        if (memTimeLeft <= 0.0)
        {
            memUsage = System.GC.GetTotalMemory(false) / (1000f * 1000f);
            memTimeLeft = memUpdateInterval;
        }

        // Game Objects
        goTimeLeft -= Time.deltaTime;
        if (goTimeLeft <= 0.0)
        {
            goCount = Resources.FindObjectsOfTypeAll(typeof(GameObject)).Length;
            goTimeLeft = goUpdateInterval;
        }

        // Logging
        if( Time.time - lastLogTime > logInterval )
        {
            Debug.Log("System Info: running time: " + Time.time + " seconds.\n" + GetDiagnosticString());
            lastLogTime = Time.time;
        }
    }

    string GetDiagnosticString()
    {
        string str = "";
        str += string.Format("Running Time:  {0:F2} hrs", (Time.time / (60f * 60f)));
        str += "\n";
        str += string.Format("FPS: {0:F1}", fps);
        str += "\n";
        str += string.Format("Mem: {0:F1} MB", memUsage);
        str += "\n";
        str += "Assemblies Allocated: " + NodeController.Inst.NumAssembliesAllocated;
        str += "\n";
		str += "Num Nodes: " + Node.getAll.Count;
        str += "\n";
        str += "Num Assemblies: " + Assembly.getAll.Count;
		str += "\n";
        str += "Num Food Pellets: " + FoodPellet.all.Count;
        str += "\n";
        str += "Game Objects: " + goCount;
        str += "\n";
        str += "Num Assembly Scores: " + NodeController.assemblyScores.Count;
        str += "\n";
        str += "Num Assembly Names: " + NodeController.Inst.assemblyNameDictionary.Count;

        if (NeuroScaleDemo.Inst != null)
        {
            str += "\n";
            str += "EnviroScale: " + (NeuroScaleDemo.Inst.enviroScale * 100f).ToString("F0") + "%";
        }

        return str;
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.015f);
        GUI.skin.label.alignment = TextAnchor.UpperRight;
        GUI.Label(new Rect((0.5f * Screen.width) - (Screen.height * 0.02f), Screen.height * 0.02f, 0.5f * Screen.width, Screen.height), GetDiagnosticString());
    }
}
