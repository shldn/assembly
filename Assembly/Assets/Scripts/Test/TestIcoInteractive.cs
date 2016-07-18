using UnityEngine;
using System.Collections;

public class TestIcoInteractive : MonoBehaviour {

    IcoSphereCreator skinMeshCreator = null;
    void Start() {
        skinMeshCreator = gameObject.AddComponent<IcoSphereCreator>();
    }
	void Update () {
        bool inside = false;
        skinMeshCreator.GetProjectedFace(gameObject.transform.position / 80f, null, out inside);
        //inside = CognoAmalgam.Inst.IsInside(gameObject.transform.position);
        GetComponent<Renderer>().material.color = inside ? Color.green : Color.red;
	}
}
