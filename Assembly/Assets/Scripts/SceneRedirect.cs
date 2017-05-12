using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRedirect : MonoBehaviour {

    [Tooltip("Must match the index in the build settings")]
    public int destinationScene = -1;

    void Start() {
        LevelManager.LoadLevel(destinationScene);
    }
}
