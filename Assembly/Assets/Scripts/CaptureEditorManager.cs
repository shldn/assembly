using UnityEngine;
using System.Collections;

public class CaptureEditorManager {
    public enum CaptureType
    {
        NONE = 0,
        ASSEMBLY = 1,
        JELLYFISH = 2,
    }

    // Object Captured event
    public delegate void CaptureEventHandler(object sender, System.EventArgs e);
    public static event CaptureEventHandler ObjectCaptured;


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
                if (capturedObjImpl as Jellyfish){
                    captureTypeImpl = CaptureType.JELLYFISH;
                }
                else if (capturedObjImpl as Assembly){
                    captureTypeImpl = CaptureType.ASSEMBLY;
                }
                if (captureTypeImpl != CaptureType.NONE){
                    RaiseCaptureEvent();
                    CameraControl.Inst.targetRadius = 10f;
                }
                
            } 
    }
    static public bool IsEditing { get { return capturedObjImpl != null; } }

    static public void ReleaseCaptured(){
        if( capturedObj != null ){
            capturedObj.Destroy();

            CameraControl.Inst.selectedJellyfish = null;
            CameraControl.Inst.selectedPhysAssembly = null;
            CameraControl.Inst.selectedNode = null;
            CameraControl.Inst.targetRadius = 100f;

        }
        capturedObj = null;
    }

    static void RaiseCaptureEvent()
    {
        try
        {
            if (ObjectCaptured != null)
                ObjectCaptured(null, new System.EventArgs());
        }
        catch
        {
            Debug.LogError("Exception raised throwing Capture event");
        }
    }
}
