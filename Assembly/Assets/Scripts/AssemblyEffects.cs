using UnityEngine;
using System.Collections;

// This class is attached to each invidivual assembly, and handles spatial sound effects and such.
public class AssemblyEffects : MonoBehaviour {

	AudioSource audioSource = null;
	Light myLight = null;

	// Use this for initialization
	void Start () {
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = Resources.Load("AssemblyHum") as AudioClip;
		audioSource.spatialBlend = 1f;
		audioSource.loop = true;
		audioSource.minDistance = 5f;
		audioSource.pitch = Random.Range(0.5f, 1.5f);
		audioSource.Play();

		myLight = gameObject.AddComponent<Light>();
		myLight.type = LightType.Point;
		myLight.range = 20f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
