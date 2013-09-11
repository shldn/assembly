using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public float cameraRotateSpeed = 2.0f;
	public float cameraMoveSpeed = 2.0f;
	
	Vector3 targetEulers;
	float smoothVelEulersX;
	float smoothVelEulersY;
	public float rotSmoothTime = 0.1f;
	
	public Texture2D crosshairTex;
	
	public GameObject lookedAtObject;

    bool cameraLock;
	
	void Start()
	{
		targetEulers = transform.eulerAngles;
	} // End of Start().

	void Update()
	{
        // Smoothly interpolate camera rotation.
        Vector3 tempEulers = transform.eulerAngles;
        tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetEulers.x, ref smoothVelEulersX, rotSmoothTime);
        tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetEulers.y, ref smoothVelEulersY, rotSmoothTime);
        transform.eulerAngles = tempEulers;

        if (Input.GetKeyDown(KeyCode.Escape))
            cameraLock = !cameraLock;

        if (cameraLock)
        {
            Screen.lockCursor = false;

            // If camera is locked, user can use cursor to manipulate nodes.
            if (lookedAtObject)
            {
                Node lookedAtNode = lookedAtObject.GetComponent<Node>();
                if (lookedAtNode)
                {
                    if (Input.GetMouseButton(0))
                        lookedAtNode.signal += 10.0f * Time.deltaTime;
                    else if (Input.GetMouseButton(1))
                        lookedAtNode.signal -= 10.0f * Time.deltaTime;
                }
            }
        }
        else
        {
            Screen.lockCursor = true;
            lookedAtObject = null;

            // Rotate camera via mouse movement.
            targetEulers.x -= Input.GetAxis("Mouse Y") * cameraRotateSpeed;
            targetEulers.y += Input.GetAxis("Mouse X") * cameraRotateSpeed;
            targetEulers.x = Mathf.Clamp(targetEulers.x, -90, 90);

            // Translate position with keyboard input.
            Vector3 tempPosition = transform.position;
            if (Input.GetKey(KeyCode.W))
                tempPosition += transform.forward * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                tempPosition -= transform.forward * cameraMoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
                tempPosition -= transform.right * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                tempPosition += transform.right * cameraMoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Space))
                tempPosition += transform.up * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftShift))
                tempPosition -= transform.up * cameraMoveSpeed * Time.deltaTime;
            transform.position = tempPosition;
        }
		
		// Determine what the user is looking at.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit cameraLookHit;
		if( Physics.Raycast( mouseRay, out cameraLookHit ))
			lookedAtObject = cameraLookHit.transform.gameObject;
		else
			lookedAtObject = null;

        

		
		
	} // End of Update().
}
