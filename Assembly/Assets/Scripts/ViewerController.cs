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
        if (!PersistentGameManager.EmbedViewer || PersistentGameManager.ViewerOnlyApp) {

            ViewerData data = MVCBridge.GetDataFromController();
            
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
            for (int i = 0; i < data.foodCreations.Count; i++)
                new FoodPelletViewer(data.foodCreations[i].Position, data.foodCreations[i].id);

            for (int i = 0; i< data.foodDeletes.Count; ++i) {
                if (FoodPelletViewer.All.ContainsKey(data.foodDeletes[i]))
                    FoodPelletViewer.All[data.foodDeletes[i]].Destroy();
            }
        }
        ViewerData.Inst.Clear();

    }

}
