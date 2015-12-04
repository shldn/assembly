using UnityEngine;
using System.Collections;

public class SpringForce : MonoBehaviour {

    public float strength = 10.0f;
	void FixedUpdate () {
        if (Input.GetKeyUp(KeyCode.K))
            transform.parent.GetComponent<Rigidbody>().AddForce(strength * transform.up);
	}
}
