using UnityEngine;
using System.Collections.Generic;

public class ViewerController : MonoBehaviour {

    // Setup Singleton
    public static ViewerController Inst = null;

    // Editor Variables
    public Transform physNodePrefab = null;
    public Transform physFoodPrefab = null;

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
            MVCBridge.InitializeViewer();
        }
    }

    void Update() {
        foreach(KeyValuePair<int,AssemblyViewer> a in AssemblyViewer.All){
            for (int i = 0; i < a.Value.nodes.Count; ++i) {
                a.Value.nodes[i].Update();
            }
        }

        if(MVCBridge.viewerReadyToSend)
            MVCBridge.SendDataToController();
    }

    void LateUpdate()
    {
        if (!PersistentGameManager.EmbedViewer || PersistentGameManager.ViewerOnlyApp) {

            if (MVCBridge.ViewerConnectionLost)
            {
                MVCBridge.HandleViewerConnectionLost();
                return;
            }

            if (!MVCBridge.viewerDataReadyToApply)
                return;

            ViewerData data = MVCBridge.viewerData;

            try {

                // Assembly Messages
                for (int i = 0; i < data.assemblyCreations.Count; i++)
                    new AssemblyViewer(data.assemblyCreations[i]);

                for (int i = 0; i < data.assemblyUpdates.Count; i++) {
                    AssemblyTransformUpdate update = data.assemblyUpdates[i];
                    if (AssemblyViewer.All.ContainsKey(update.id))
                        AssemblyViewer.All[update.id].TransformUpdate(update.transforms);
                }

                for (int i = 0; i < data.assemblyPropertyUpdates.Count; ++i) {
                    if (AssemblyViewer.All.ContainsKey(data.assemblyPropertyUpdates[i].id)) {
                        AssemblyViewer av = AssemblyViewer.All[data.assemblyPropertyUpdates[i].id];
                        av.Properties = data.assemblyPropertyUpdates[i];
                    }
                }

                for (int i = 0; i < data.assemblyDeletes.Count; ++i) {
                    if (AssemblyViewer.All.ContainsKey(data.assemblyDeletes[i]))
                        AssemblyViewer.All[data.assemblyDeletes[i]].Destroy();
                }


                // Food Messages
                for (int i = 0; i < data.foodCreations.Count; ++i)
                    new FoodPelletViewer(data.foodCreations[i].Position, data.foodCreations[i].id);

                for (int i = 0; i < data.foodDeletes.Count; ++i) {
                    if (FoodPelletViewer.All.ContainsKey(data.foodDeletes[i]))
                        FoodPelletViewer.All[data.foodDeletes[i]].Destroy();
                }

                // Generic Messages
                for (int i = 0; i < data.messages.Count; ++i)
                    HandleGenericMessage(data.messages[i]);
            }
            catch(System.Exception e) {
                Debug.LogError("Exception in Message Application: " + e.ToString() + "\n" + e.StackTrace);
            }
            MVCBridge.viewerDataReadyToApply = false;

        }
        ViewerData.Inst.Clear();
    }

    public void HandleGenericMessage(object message) {
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
    public void Clear() {

        foreach (KeyValuePair<int, AssemblyViewer> kvp in AssemblyViewer.All)
            kvp.Value.DestroyKeepInList();
        AssemblyViewer.All.Clear();

        foreach (KeyValuePair<int, FoodPelletViewer> kvp in FoodPelletViewer.All)
            kvp.Value.Destroy(false);
        FoodPelletViewer.All.Clear();

        GameObject.Destroy(MatingViewer.Inst.gameObject);
    }

    void OnDestroy() {
        MVCBridge.CloseViewerConnection();
    }

}
