using UnityEngine;
using System.Collections;


// An instanced script that runs a test on all current PhysAssemblies, then culls all but the best one.
public class ClientTest : MonoBehaviour {

	public static ClientTest Inst = null;
    protected bool unlockFrameRate = true;
	protected int runTime = 0;
    protected int testDuration = 500; // frames
    protected float nodePower = 1.0f;
    protected Assembly winner = null;

    public float NodePower { get { return nodePower; } protected set { nodePower = value; } }
    public bool UnlockFrameRate { get { return unlockFrameRate; } protected set { unlockFrameRate = value; } }
    public bool IsDone {  get{ return runTime > testDuration; } }


	protected virtual void Awake(){
		Inst = this;
	} // End of Awake().
	

	protected virtual void Update(){
		runTime ++;
	} // End of Update().

    protected virtual void OnDestroy(){
        Inst = null;
    } // End of OnDestroy().

    protected virtual void EndTest()
    {
        DestroyAllButWinner();
        CameraControl.Inst.KeepAssembliesInView();
        CameraControl.Inst.selectedAssembly = winner;
        Destroy(gameObject);
    } // End of EndTest().

    protected virtual void DestroyAllButWinner()
    {
        foreach (Assembly someAssem in Assembly.getAll)
        {
            if (winner == null)
                winner = someAssem;
            if (someAssem != winner)
                someAssem.Destroy();
            else
                AssemblyEditor.Inst.capturedAssembly = someAssem;

        }
    } // End of DestroyAllButWinner().

    protected void AssignWinnerByHighestEnergy()
    {
        float maxEnergy = -999999999f;

        foreach (Assembly someAssem in Assembly.getAll)
        {
            if (someAssem.energy > maxEnergy)
            {
                winner = someAssem;
                maxEnergy = someAssem.energy;
            }
        }
    } // End of AssignWinnerByHighestEnergy().


    void OnGUI(){
		string progressBar = "[Testing] ";
        float step = 100f / (float)(testDuration);
		for(int i = 0; i < runTime * step; i++)
			progressBar += "|";
        GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.width * 0.02f);
		GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.Label(new Rect(10f, 10f, Screen.width - 20f, Screen.height - 20f), progressBar + " " + (runTime * step).ToString("F0") + "%");
	} // End of OnGUI().

} // End of ClientTest.
