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
    public AudioClip placePingClip;
    public Texture qrCodeTexture;


	void Awake () {
        DontDestroyOnLoad(this);
        if( !captureMgr )
            captureMgr = gameObject.AddComponent<CaptureNet_Manager>();

        // load prefabs
        if( playerSyncObj == null)
            playerSyncObj = Resources.Load("PlayerObject");
        if (pingBurstObj == null)
            pingBurstObj = Resources.Load("Ping_Effect");
        if (placePingClip == null)
            placePingClip = Resources.Load("125374__thomasevd__ping") as AudioClip;
        if (!IsClient && qrCodeTexture == null)
            qrCodeTexture = Resources.Load("Textures/Capture_QR_Code") as Texture;
        
	}
	
	void Update () {
        LevelManager.InputHandler();
	}

    // Helper function to make sure singleton instance exists and is initialized
    public void Touch() { }

}
