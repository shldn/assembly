using UnityEngine;

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

        ViewerData.Inst.Clear();
#endif
    }

}
