using UnityEngine;
using System.Collections;

public class CaptureEditorManager {
    public enum CaptureType
    {
        NONE = 0,
        ASSEMBLY = 1,
        JELLYFISH = 2,
    }

    static private CaptureType captureTypeImpl = CaptureType.NONE;
    static public CaptureObject capturedObjImpl = null;

    static public CaptureType captureType { get { return captureTypeImpl; } }
    static public CaptureObject capturedObj
    { 
        get { return capturedObjImpl; } 
        set { 
                capturedObjImpl = value;

                // set type
                captureTypeImpl = CaptureType.NONE;
                if (capturedObjImpl as Jellyfish)
                    captureTypeImpl = CaptureType.JELLYFISH;
                else if (capturedObjImpl as Assembly)
                    captureTypeImpl = CaptureType.ASSEMBLY;
            } 
    }
    static public bool IsEditing { get { return capturedObjImpl != null; } }

    static public void ReleaseCaptured(){
        if( capturedObj != null )
            capturedObj.Destroy();
        capturedObj = null;
    }
}
