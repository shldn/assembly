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

}
