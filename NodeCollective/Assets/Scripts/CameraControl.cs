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
	
	void Start()
	{
		//Screen.lockCursor = true;
		
		targetEulers = transform.eulerAngles;
	}

	void Update()
	{
		
		// Smoothly interpolate camera rotation via mouse movement.
		Vector3 tempEulers = transform.eulerAngles;
		targetEulers.x -= Input.GetAxis ( "Mouse Y" ) * cameraRotateSpeed;
		targetEulers.y += Input.GetAxis ( "Mouse X" ) * cameraRotateSpeed;
		tempEulers.x = Mathf.SmoothDampAngle( tempEulers.x, targetEulers.x, ref smoothVelEulersX, rotSmoothTime );
		tempEulers.y = Mathf.SmoothDampAngle( tempEulers.y, targetEulers.y, ref smoothVelEulersY, rotSmoothTime );
		transform.eulerAngles = tempEulers;
		
		
		// Determine what the user is looking at.
		Ray cameraLookRay = Camera.main.ViewportPointToRay( new Vector3( 0.5f, 0.5f, 0f ));
		RaycastHit cameraLookHit;
		if( Physics.Raycast( cameraLookRay, out cameraLookHit ))
			lookedAtObject = cameraLookHit.transform.gameObject;
		else
			lookedAtObject = null;
		
		
		
		
		
		// Translate position with keyboard input.
		Vector3 tempPosition = transform.position;
		if( Input.GetKey( KeyCode.W ))
			tempPosition += transform.forward * cameraMoveSpeed * Time.deltaTime;
		if( Input.GetKey( KeyCode.S ))
			tempPosition -= transform.forward * cameraMoveSpeed * Time.deltaTime;
		
		if( Input.GetKey( KeyCode.A ))
			tempPosition -= transform.right * cameraMoveSpeed * Time.deltaTime;
		if( Input.GetKey( KeyCode.D ))
			tempPosition += transform.right * cameraMoveSpeed * Time.deltaTime;
		
		if( Input.GetKey( KeyCode.Space ))
			tempPosition += transform.up * cameraMoveSpeed * Time.deltaTime;
		if( Input.GetKey( KeyCode.LeftShift ))
			tempPosition -= transform.up * cameraMoveSpeed * Time.deltaTime;
		transform.position = tempPosition;
		
	}
	
	
	void OnGUI()
	{
		/*
		GUI.DrawTexture( new Rect(( Screen.width * 0.5f ) - 5f, ( Screen.height * 0.5f ) - 5f, 10f, 10f ), crosshairTex );
		
		
		if( lookedAtObject )
		{
			Part lookedAtPart = lookedAtObject.GetComponent< Part >();
			if( lookedAtPart )
			{
				Vector3 partScreenPos = Camera.main.WorldToScreenPoint( lookedAtPart.transform.position );
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.Label( new Rect( partScreenPos.x - 100, ( Screen.height - partScreenPos.y ) - 100, 200, 200 ), lookedAtPart.dna );
			}
		}
		*/
	}
}
