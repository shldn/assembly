using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

    public float killTimer = 10f;


	// Update is called once per frame
	void Update(){
	    killTimer -= Time.deltaTime;

        if(killTimer <= 0f)
            Destroy(gameObject);

	} // End of Update().
} // End of DestroyAfterTime.
