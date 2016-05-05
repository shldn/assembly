using UnityEngine;
using System.Collections;

public class TestIcoInteractive : MonoBehaviour {

	void Update () {
        bool inside = false;
        IcoSphereCreator.Inst.GetProjectedFace(gameObject.transform.position, out inside);
        GetComponent<Renderer>().material.color = inside ? Color.green : Color.red;
	}
}
