﻿using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;


    float orbitRunner = 0f;
    float orbitRunnerTarget = 0f;
    float orbitRunnerVel = 0f;

    float orbitTilt = 20f;
    float orbitTiltTarget = 20f;
    float orbitTiltVel = 0f;

    public float orbitDist = 50f;
    public float orbitSpeed = 1f;

    float smoothTime = 0.5f;


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
	    orbitRunnerTarget += Time.deltaTime;

        transform.position = Quaternion.AngleAxis(-orbitRunner * orbitSpeed, Vector3.up) * Quaternion.AngleAxis(-orbitTilt, Vector3.right) * Vector3.forward * orbitDist;
        transform.LookAt(Vector3.zero);

        if(Network.peerType == NetworkPeerType.Server){
            float orbitSensitivity = 3f;
            orbitRunnerTarget -= Input.GetAxis("Mouse X") * orbitSensitivity;
            orbitTiltTarget -= Input.GetAxis("Mouse Y") * orbitSensitivity;

            orbitTiltTarget = Mathf.Clamp(orbitTiltTarget, -35f, 35f);

            Screen.showCursor = false;
        }

        orbitRunner = Mathf.SmoothDamp(orbitRunner, orbitRunnerTarget, ref orbitRunnerVel, smoothTime);
        orbitTilt = Mathf.SmoothDamp(orbitTilt, orbitTiltTarget, ref orbitTiltVel, smoothTime);
	} // End of Update().


    void OnDrawGizmos(){
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(Vector3.zero, orbitDist);
        Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
        Gizmos.DrawSphere(Vector3.zero, orbitDist);
    } // End of OnDrawGizmos().

} // End of CameraOrbit.
