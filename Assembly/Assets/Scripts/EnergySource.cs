using UnityEngine;
using System.Collections;

public class EnergySource : MonoBehaviour {

	public float energy = 100f;


	// Use this for initialization
	void Start () {
		transform.rotation = Random.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * 50f, transform.up);
		transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * 30f, transform.right);
		transform.rotation *= Quaternion.AngleAxis(Time.deltaTime * 10f, transform.forward);

		//if(energy <= 0f)
			//Destroy(gameObject);
	}
}
