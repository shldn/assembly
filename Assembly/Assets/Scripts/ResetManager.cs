using UnityEngine;

public class ResetManager : MonoBehaviour{

    void Update() {
        if(Time.timeSinceLevelLoad > Config.resetTime) {
            CameraControl.Inst.cameraTransitionSpeed = 0.1f;
            CameraControl.Inst.SetMode_AssemblyHerd(false);
            CameraControl.Inst.TransitionDone += Reset;
        }
    }

    void Reset() {
        Debug.Log("Resetting after " + Time.timeSinceLevelLoad + " secs in scene");
        CameraControl.Inst.TransitionDone -= Reset;
        TransitionManager.Inst.ChangeLevel(4);
    }
}
