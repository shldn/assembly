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

    // Events
    public delegate void TestDoneHandler();
    public TestDoneHandler TestDone;

    // Accessors
    public Assembly Winner { get { return winner; } }
    public float NodePower { get { return nodePower; } protected set { nodePower = value; } }
    public bool UnlockFrameRate { get { return unlockFrameRate; } protected set { unlockFrameRate = value; } }
    public bool IsDone {  get{ return runTime > testDuration; } }


	protected virtual void Awake(){
		Inst = this;
        CameraControl.Inst.SetMode_AssemblyHerd();
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
        //CameraControl.Inst.KeepAssembliesInView();
        if (TestDone != null)
            TestDone();
        Destroy(gameObject);
    } // End of EndTest().

    protected virtual void DestroyAllButWinner()
    {
        foreach (Assembly someAssem in AssemblyEditor.Inst.testAssemblies)
        {
            if (winner == null)
                winner = someAssem;
            if (someAssem != winner)
                someAssem.Destroy();
            else
                AssemblyEditor.Inst.capturedAssembly = someAssem;

        }
        if (winner != null)
            winner.isTraitTest = false;
        AssemblyEditor.Inst.testAssemblies.Clear();
    } // End of DestroyAllButWinner().

    protected void AssignWinnerByHighestEnergy()
    {
        float maxEnergy = -999999999f;

        foreach (Assembly someAssem in AssemblyEditor.Inst.testAssemblies)
        {
            if (someAssem.energy > maxEnergy)
            {
                winner = someAssem;
                maxEnergy = someAssem.energy;
            }
        }
    } // End of AssignWinnerByHighestEnergy().


    void OnGUI(){
        float step = 100f / (float)(testDuration);
		string progressBar = (runTime * step).ToString("F0") + "%" + "\n" + "Test in progress...";
		GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.width * 0.02f);
		GUI.skin.label.alignment = TextAnchor.LowerCenter;
        GUI.Label(new Rect(10f, 10f, Screen.width - 20f, Screen.height - 20f), progressBar);
	} // End of OnGUI().

} // End of ClientTest.
