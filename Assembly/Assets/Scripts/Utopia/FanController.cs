using UnityEngine;
using System.Collections;

public class FanController : MonoBehaviour {

    public float repeatDelay = 1.0f;
    public float durationPercent = 0.5f; // once fired, it will persist for this percentage of the delay time

	void Start () {
        InvokeRepeating("FanOn", 0.0f, repeatDelay);
	}
	
	void FanOn() {
        GetComponent<ConstantForce>().enabled = true;
        Animation fanAnim = gameObject.GetComponentInChildren<Animation>();
        if (fanAnim)
            fanAnim.Play();
        Invoke("FanOff", durationPercent * repeatDelay);
	}

    void FanOff()
    {
        GetComponent<ConstantForce>().enabled = false;
        Animation fanAnim = gameObject.GetComponentInChildren<Animation>();
        if( fanAnim )
            fanAnim.Stop();
    }
}
