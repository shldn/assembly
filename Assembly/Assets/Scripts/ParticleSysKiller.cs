using UnityEngine;
using System.Collections;

public class ParticleSysKiller : MonoBehaviour {

	private ParticleSystem ps;
      
	void Start(){
		ps = gameObject.GetComponent<ParticleSystem>();
	} // End of Start().
    
    void Update(){
		if(ps)
			if(!ps.IsAlive())
				Destroy(gameObject);
    } // End of Update().

} // End of ParticleSysKiller.
