﻿using UnityEngine;
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
    }

    void Update() {
        foreach(KeyValuePair<int,AssemblyViewer> a in AssemblyViewer.All){
            for (int i = 0; i < a.Value.nodes.Count; ++i) {
                a.Value.nodes[i].Update();
            }
        }
    }

    void LateUpdate()
    {
        if (!PersistentGameManager.EmbedViewer) {

            // Assembly Messages
            for (int i = 0; i < ViewerData.Inst.assemblyCreations.Count; i++)
                new AssemblyViewer(ViewerData.Inst.assemblyCreations[i]);

            for (int i = 0; i < ViewerData.Inst.assemblyUpdates.Count; i++) {
                AssemblyTransformUpdate update = ViewerData.Inst.assemblyUpdates[i];
                if (AssemblyViewer.All.ContainsKey(update.id))
                    AssemblyViewer.All[update.id].TransformUpdate(update.transforms);
            }

            for (int i = 0; i < ViewerData.Inst.assemblyPropertyUpdates.Count; ++i) {
                if (AssemblyViewer.All.ContainsKey(ViewerData.Inst.assemblyPropertyUpdates[i].id)) {
                    AssemblyViewer av = AssemblyViewer.All[ViewerData.Inst.assemblyPropertyUpdates[i].id];
                    av.Properties = ViewerData.Inst.assemblyPropertyUpdates[i];
                }
            }

            for (int i = 0; i < ViewerData.Inst.assemblyDeletes.Count; ++i) {
                if (AssemblyViewer.All.ContainsKey(ViewerData.Inst.assemblyDeletes[i]))
                    AssemblyViewer.All[ViewerData.Inst.assemblyDeletes[i]].Destroy();
            }


            // Food Messages
            for (int i = 0; i < ViewerData.Inst.foodCreations.Count; i++)
                new FoodPelletViewer(ViewerData.Inst.foodCreations[i].Position, ViewerData.Inst.foodCreations[i].id);

            for (int i = 0; i< ViewerData.Inst.foodDeletes.Count; ++i) {
                if (FoodPelletViewer.All.ContainsKey(ViewerData.Inst.foodDeletes[i]))
                    FoodPelletViewer.All[ViewerData.Inst.foodDeletes[i]].Destroy();
            }
        }
        ViewerData.Inst.Clear();

    }

}
