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
    

    public static List<CaptureObject> CaptureObjects = new List<CaptureObject>();

    public CaptureNet_Manager captureMgr;
    public bool optimize = true;

    // Prefabs
    public UnityEngine.Object playerSyncObj;
    public UnityEngine.Object pingBurstObj;
    public AudioClip pushClip;
    public AudioClip captureClip;
    public Texture qrCodeTexture;

    // Interface
    bool cursorLock = true;
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
        
	}


	void Update () {
        LevelManager.InputHandler();

		if(Time.time > 3f){
			if(Input.GetKeyDown(KeyCode.F1)){
				cursorLock = !cursorLock;
			}

			Screen.lockCursor = cursorLock;
		}
	}


    public void EnviroImpulse(Vector3 pos, float force){
        // Apply physics
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly curAssem = Assembly.GetAll()[i];
            
            Vector3 vecToAssem = pos - curAssem.WorldPosition;
            if(vecToAssem.Equals(Vector3.zero))
                continue;

            if(curAssem.physicsObject.rigidbody)
                curAssem.physicsObject.rigidbody.AddForce(vecToAssem.normalized * (-force / (1f + (vecToAssem.magnitude * 0.01f))), ForceMode.Impulse);
        }
    } // End of EnviroImpulse().


    // Helper function to make sure singleton instance exists and is initialized
    public void Touch() { }

}
