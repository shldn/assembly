using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;


    float orbitRunner = 0f;

    public float orbitDist = 50f;
    public float orbitSpeed = 1f;


    void Awake(){
        Inst = this;
    } // End of Awake().

	// Use this for initialization
	void Start(){
        RenderSettings.fog = true;
        RenderSettings.fogColor = Camera.main.backgroundColor;
	} // End of Start().
	

	// Update is called once per frame
	void Update(){
	    orbitRunner += Time.deltaTime;
        transform.position = Quaternion.AngleAxis(-orbitRunner * orbitSpeed, Vector3.up) * Quaternion.AngleAxis(-20f, Vector3.right) * Vector3.forward * orbitDist;
        transform.LookAt(Vector3.zero);
	} // End of Update().


    void OnDrawGizmos(){
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(Vector3.zero, orbitDist);
        Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
        Gizmos.DrawSphere(Vector3.zero, orbitDist);
    } // End of OnDrawGizmos().

} // End of CameraOrbit.
