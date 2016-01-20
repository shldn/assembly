using UnityEngine;
using System.Collections.Generic;

public class ViewerController : MonoBehaviour {

    // Setup Singleton
    public static ViewerController Inst = null;

    // Editor Variables
    public Transform physNodePrefab = null;
    public Transform physFoodPrefab = null;

    // Bridges to Controllers
    List<MVCBridge> mvcBridges = new List<MVCBridge>();
    List<AmalgamViewer> amalgams = new List<AmalgamViewer>();

    // Internal Variables
    bool hide = false;

    // Accessors
    public static bool Hide { 
        get { return Inst != null && Inst.hide; } 
        set {
            if (!Inst)
                return;
            Inst.hide = value;
            Environment.Inst.Visible = false;
#if UNITY_EDITOR
            QualitySettings.vSyncCount = value ? 0 : 1;
#endif
        } 
    }

    void Awake()
    {
        Inst = this;
        if (WorldSizeController.Inst == null)
            gameObject.AddComponent<WorldSizeController>();

    }

    void Start() {
        if(PersistentGameManager.ViewerOnlyApp) {
            Debug.LogError("Initializing Viewer Networking");
            
            for (int i = 0; i < Config.controllerAddress.Count; ++i) {
                mvcBridges.Add(new MVCBridge());
                mvcBridges[i].InitializeViewer(Config.controllerAddress[i].ip, Config.controllerAddress[i].port);
                amalgams.Add(null);
            }
        }
    }

    void Update() {
        foreach(AmalgamViewer av in amalgams) {
            if (av != null)
                av.Update();
        }

        for (int i = 0; i < mvcBridges.Count; ++i)
            if (mvcBridges[i].viewerReadyToSend)
                mvcBridges[i].SendDataToController();


        // User input
        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyUp(KeyCode.L))
            ControllerData.Inst.Messages.Add(new DataRequest(DataRequestType.ASSEMBLY_FULL));
        else if (Input.GetKeyUp(KeyCode.L))
            ControllerData.Inst.Messages.Add(new DataRequest(DataRequestType.ASSEMBLY_CENTERS));

    }

    void LateUpdate()
    {
        if (!PersistentGameManager.EmbedViewer || PersistentGameManager.ViewerOnlyApp) {

            for (int i = 0; i < mvcBridges.Count; ++i) {
                if (mvcBridges[i].ViewerConnectionLost) {
                    mvcBridges[i].HandleViewerConnectionLost();
                    continue;
                }

                if (!mvcBridges[i].viewerDataReadyToApply)
                    continue;

                HandleViewerMessages(i, mvcBridges[i].viewerData);
                mvcBridges[i].viewerDataReadyToApply = false;
            }
        }
        ViewerData.Inst.Clear();
    }

    private void HandleViewerMessages(int amalgamId, ViewerData data) {

        try {
            if (amalgams[amalgamId] == null)
                amalgams[amalgamId] = new AmalgamViewer();

            // Amalgam Messages
            amalgams[amalgamId].HandleMessages(data);


            // Generic Messages
            for (int i = 0; i < data.messages.Count; ++i)
                HandleGenericMessage(amalgamId, data.messages[i]);
        }
        catch (System.Exception e) {
            Debug.LogError("Exception in Message Application: " + e.ToString() + "\n" + e.StackTrace);
        }
    }

    public void HandleGenericMessage(int amalgamId, object message) {
        System.Type type = message.GetType();
        if(type.Equals(typeof(CaptureData))) {
            CaptureData capturedata = message as CaptureData;
            if (PlayerSync.capturedToPlayerSync.ContainsKey(capturedata.id)) {
                PlayerSync.capturedToPlayerSync[capturedata.id].SendCaptureAssemblyRPC(capturedata.definition);
                PlayerSync.capturedToPlayerSync.Remove(capturedata.id);
            }            
        }
        else if (type.Equals(typeof(TargetWorldSizeData))) {
            TargetWorldSizeData data = message as TargetWorldSizeData;
            WorldSizeController.Inst.TargetWorldSize = data.size;
        }
        else if (type.Equals(typeof(WorldSizeData))) {
            WorldSizeData data = message as WorldSizeData;
            WorldSizeController.Inst.WorldSize = data.Size;
        }
        else
            Debug.LogError("HandleGenericMessage: unknown message type: " + type);
    }

    // Clear all Viewer elements
    public void Clear(MVCBridge caller = null) {

        int amalgamId = -1;
        if (caller != null)
            for (int i = 0; i < mvcBridges.Count; ++i)
                if (caller == mvcBridges[i])
                    amalgamId = i;

        if(amalgamId >= 0 && amalgams[amalgamId] != null) {
            amalgams[amalgamId].Destroy();
            amalgams[amalgamId] = null;
        }
    }

    void OnDestroy() {
        for (int i = 0; i < mvcBridges.Count; ++i)
            mvcBridges[i].CloseViewerConnection();
    }

}
