using UnityEngine;
using System.Collections;

public interface CaptureObject {

    Vector3 Position{ get; }
    void Destroy();
}
