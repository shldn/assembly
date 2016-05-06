using UnityEngine;
using System.Collections;

public class TestIcoInteractive : MonoBehaviour {

	void Update () {
        bool inside = false;
        IcoSphereCreator.Inst.GetProjectedFace(gameObject.transform.position / 80f, null, out inside);
        //inside = CognoAmalgam.Inst.IsInside(gameObject.transform.position);
        GetComponent<Renderer>().material.color = inside ? Color.green : Color.red;
	}
}
