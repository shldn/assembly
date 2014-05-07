using UnityEngine;

class ParticleEffects : MonoBehaviour{
	private ParticleSystem particleObject;

	void Start(){
		particleObject = GetComponent<ParticleSystem>();
	}
	
	void LateUpdate (){
		if (!particleObject.IsAlive())
			Object.Destroy (this.gameObject);	
	}
	
	/*
	void Update(){
		if (!particleObject.IsAlive())
			Object.Destroy (this.gameObject);	
	}*/

}