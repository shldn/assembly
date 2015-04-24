using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This Game Manager is not destroyed when levels are switched
// Use this for functionality that should be consistent across levels

public class PersistentGameManager : MonoBehaviour {

    private static PersistentGameManager mInst = null;
    public static PersistentGameManager Inst{
        get{
            if (mInst == null)
                mInst = (new GameObject("PersistentGM")).AddComponent<PersistentGameManager>();
            return mInst;
        }
    }

    public static bool IsAdminClient { get { return true; } }
    public static bool IsClient { get { return Application.loadedLevelName == "CaptureClient"; } }
    public static bool IsServer { get { return !IsClient; } }


    public static HashSet<CaptureObject> CaptureObjects = new HashSet<CaptureObject>();

    public CaptureNet_Manager captureMgr;
    public bool optimize = true;

    // Prefabs
    public UnityEngine.Object playerSyncObj;
    public UnityEngine.Object pingBurstObj;
    public AudioClip pushClip;
    public AudioClip captureClip;
    public Texture qrCodeTexture;

    // Interface
    bool cursorLock = false;
    public bool CursorLock {get{return cursorLock;}}


	void Awake () {
        DontDestroyOnLoad(this);
        if( !captureMgr )
            captureMgr = gameObject.AddComponent<CaptureNet_Manager>();

        // load prefabs
        if( playerSyncObj == null)
            playerSyncObj = Resources.Load("PlayerObject");
        if (pingBurstObj == null)
            pingBurstObj = Resources.Load("Ping_Effect");
        if (captureClip == null)
            captureClip = Resources.Load("pushClip") as AudioClip;
        if (pushClip == null)
            pushClip = Resources.Load("captureClip") as AudioClip;
        if (!IsClient && qrCodeTexture == null)
            qrCodeTexture = Resources.Load("Textures/Capture_QR_Code") as Texture;

        if (IsClient)
            cursorLock = false;
        else
			cursorLock = true;
	}


	void Update () {
        LevelManager.InputHandler();

        if (Input.GetKeyDown(KeyCode.I))
        {
            DisplayIP displayIP = gameObject.GetComponent<DisplayIP>();
            if (displayIP == null)
                displayIP = gameObject.AddComponent<DisplayIP>();
            else
                displayIP.enabled = !displayIP.enabled;
        }

        // Quit on Escape
        if (Input.GetKeyUp(KeyCode.Escape))
            Application.Quit();

        if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.C) )
        {
            GameObject go = new GameObject("CursorBotManager");
            go.AddComponent<CursorBotManager>();
        }

		if(Input.GetKeyDown(KeyCode.F1))
			cursorLock = !cursorLock;

		Screen.lockCursor = cursorLock;

		if(IsClient && (AssemblyEditor.Inst && (!ClientTest.Inst || !ClientTest.Inst.UnlockFrameRate)))
			Application.targetFrameRate = 30;
		else
			Application.targetFrameRate = 99999;

	}


    public void EnviroImpulse(Vector3 pos, float force){
        // Apply physics
        foreach(Node someNode in Node.getAll){
            Vector3 vecToAssem = pos - someNode.Position;
            if(vecToAssem.Equals(Vector3.zero))
                continue;

            someNode.delayPosition += vecToAssem.normalized * (-force / (1f + (vecToAssem.magnitude * 0.01f))) * 0.1f;
        }
    } // End of EnviroImpulse().


    // Helper function to make sure singleton instance exists and is initialized
    public void Touch() { }

}
