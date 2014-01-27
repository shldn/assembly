using UnityEngine;
using System.Collections;

public class MainCameraControl : MonoBehaviour {

    Quaternion targetRot;

    Vector3 targetPos = Vector3.zero;
    Vector3 smoothVelTranslate = Vector3.zero;
    float translateSmoothTime = 0.2f;

    float cameraMoveSpeed = 4f;
    float cameraRotateSpeed = 2f;


	// Use this for initialization
	void Start(){
	    targetRot = transform.rotation;
		targetPos = transform.position;
	} // End of Start().
	
	// Update is called once per frame
	void Update(){

        // Smoothly interpolate camera position/rotation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelTranslate, translateSmoothTime);
        Quaternion tempRot = transform.rotation;
        tempRot = Quaternion.Slerp(tempRot, targetRot, 5 * Time.deltaTime);
        transform.rotation = tempRot;

        // Pitch/yaw camera via mouse movement.
        targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
        targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);

        // Translate position with keyboard input.
        targetPos += WesInput.forwardThrottle * transform.forward * cameraMoveSpeed * Time.deltaTime;
        targetPos += WesInput.horizontalThrottle * transform.right * cameraMoveSpeed * Time.deltaTime;
        targetPos += WesInput.verticalThrottle * transform.up * cameraMoveSpeed * Time.deltaTime;

        // Roll camera using Q and E
        targetRot *= Quaternion.AngleAxis(WesInput.rotationThrottle * -cameraRotateSpeed, Vector3.forward);

	} // End of Update().
} // End of MainCameraControl.
 