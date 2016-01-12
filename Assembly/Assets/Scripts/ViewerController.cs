﻿using UnityEngine;
using System.Collections.Generic;

public class ViewerController : MonoBehaviour {

    // Setup Singleton
    public static ViewerController Inst = null;

    // Editor Variables
    public Transform physNodePrefab = null;
    public Transform physFoodPrefab = null;

    // Bridges to Controllers
    List<MVCBridge> mvcBridges = new List<MVCBridge>();

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
            int numServers = 10;
            for(int i=0; i < numServers; ++i)
                mvcBridges.Add(new MVCBridge());
            for(int i = 0; i < mvcBridges.Count; ++i)
                mvcBridges[i].InitializeViewer("127.0.0.1", 12000 + i);
        }
    }

    void Update() {
        foreach( AssemblyViewer a in AssemblyViewer.All){
            for (int i = 0; i < a.nodes.Count; ++i) {
                a.nodes[i].Update();
            }
        }

        for (int i = 0; i < mvcBridges.Count; ++i)
            if (mvcBridges[i].viewerReadyToSend)
                mvcBridges[i].SendDataToController();

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

            // Assembly Messages
            for (int i = 0; i < data.assemblyCreations.Count; i++)
                new AssemblyViewer(amalgamId, data.assemblyCreations[i]);

            Dictionary<int, AssemblyViewer> viewers = AssemblyViewer.GetAssemblyViewers(amalgamId);
            for (int i = 0; i < data.assemblyUpdates.Count; i++) {
                AssemblyTransformUpdate update = data.assemblyUpdates[i];
                if (viewers.ContainsKey(update.id))
                    viewers[update.id].TransformUpdate(update.transforms);
            }

            for (int i = 0; i < data.assemblyPropertyUpdates.Count; ++i) {
                if (viewers.ContainsKey(data.assemblyPropertyUpdates[i].id)) {
                    AssemblyViewer av = viewers[data.assemblyPropertyUpdates[i].id];
                    av.Properties = data.assemblyPropertyUpdates[i];
                }
            }

            for (int i = 0; i < data.assemblyDeletes.Count; ++i) {
                if (viewers.ContainsKey(data.assemblyDeletes[i]))
                    viewers[data.assemblyDeletes[i]].Destroy();
            }


            // Food Messages
            for (int i = 0; i < data.foodCreations.Count; ++i)
                new FoodPelletViewer(amalgamId, data.foodCreations[i].Position, data.foodCreations[i].id);

            Dictionary<int, FoodPelletViewer> fViewers = FoodPelletViewer.GetFoodViewers(amalgamId);
            for (int i = 0; i < data.foodDeletes.Count; ++i) {
                if (fViewers.ContainsKey(data.foodDeletes[i]))
                    fViewers[data.foodDeletes[i]].Destroy();
            }

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

        if(amalgamId == -1) {
            Debug.LogError("Trying to clear amalgam: " + amalgamId);
        }
        else {
            Dictionary<int, AssemblyViewer> aViewers = AssemblyViewer.GetAssemblyViewers(amalgamId);
            foreach (KeyValuePair<int, AssemblyViewer> kvp in aViewers)
                kvp.Value.DestroyKeepInList();
            aViewers.Clear();

            Dictionary<int, FoodPelletViewer> fViewers = FoodPelletViewer.GetFoodViewers(amalgamId);
            foreach (KeyValuePair<int, FoodPelletViewer> kvp in fViewers)
                kvp.Value.Destroy(false);
            fViewers.Clear();

            MatingViewer.Inst.RemoveAmalgamMates(amalgamId);
        }
    }

    void OnDestroy() {
        for (int i = 0; i < mvcBridges.Count; ++i)
            mvcBridges[i].CloseViewerConnection();
    }

}
