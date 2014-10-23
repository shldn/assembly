using UnityEngine;
using System.Collections;

public class JellyfishController : MonoBehaviour {


    Vector3 torqueVector = Vector3.zero;
    float torqueChangeCooldown = 0f;


	void Awake(){
        transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
	} // End of Awake().
	

	// Update is called once per frame
	void Update(){
	    if(torqueChangeCooldown <= 0f){
            torqueVector = Random.rotation * Vector3.forward;
            torqueChangeCooldown = Random.Range(0f, 10f);
        }
        torqueChangeCooldown -= Time.deltaTime;

        if(Vector3.Distance(Vector3.zero, transform.position) >= 30f){
            torqueVector = Vector3.Cross(transform.up, -transform.position).normalized;
        }
	} // End of Update().


    void FixedUpdate(){
        rigidbody.AddForce(transform.up * 0.1f, ForceMode.Force);
        rigidbody.AddTorque(torqueVector * 0.1f, ForceMode.Force);
    } // End of OnLateUpdate().

} // End of JellyfishController.
