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
    public bool Hide { 
        get { return hide; } 
        set { 
            hide = value;
            Environment.Inst.Visible = false;
        } 
    }

    void Awake()
    {
        Inst = this;
    }

    void Start()
    {

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

#if INTEGRATED_VIEWER
#else
        for (int i = 0; i < ViewerData.Inst.assemblyCreations.Count; i++) {
            new AssemblyViewer(ViewerData.Inst.assemblyCreations[i]);
        }

        for (int i = 0; i < ViewerData.Inst.assemblyUpdates.Count; i++) {
            AssemblyTransformUpdate update = ViewerData.Inst.assemblyUpdates[i];
            if (AssemblyViewer.All.ContainsKey(update.id))
                AssemblyViewer.All[update.id].TransformUpdate(update.transforms);
        }

        for (int i = 0; i < ViewerData.Inst.assemblyDeletes.Count; ++i) {
            if (AssemblyViewer.All.ContainsKey(ViewerData.Inst.assemblyDeletes[i]))
                AssemblyViewer.All[ViewerData.Inst.assemblyDeletes[i]].Destroy();
        }

            ViewerData.Inst.Clear();
#endif
    }

}
