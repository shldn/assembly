using UnityEngine;

public class ViewerManager {

    // Setup Singleton
    private static ViewerManager inst = null;
    public static ViewerManager Inst{
        get{
            if (inst == null)
                inst = new ViewerManager();
            return inst;
        }
    }

    // Variables
    bool hide = false;

    // Accessors
    public bool Hide { 
        get { return hide; } 
        set { 
            hide = value;
            Environment.Inst.Visible = false;
        } 
    }

}
