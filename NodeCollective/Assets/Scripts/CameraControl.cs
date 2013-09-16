using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public float cameraRotateSpeed = 2.0f;
	public float cameraMoveSpeed = 2.0f;

    public float orbitRotateSpeed = 2.0f;
    public float orbitZoomSpeed = 6.0f;


	Vector3 targetEulers;
	float smoothVelEulersX;
	float smoothVelEulersY;
	public float rotSmoothTime = 0.1f;

    Vector3 targetPos;
    Vector3 smoothVelTranslate;
    public float translateSmoothTime = 0.1f;
	
	[HideInInspector] public GameObject lookedAtObject;

    bool cameraLock;

    bool cameraFocused;
    Vector3 focusedPos;
    Node focusedNode;
    public Texture2D nodeSelectTex;

    Vector2 orbitEulers;
    float orbitDist = 10.0f;

    bool spacePressed;
	
	void Start()
	{
		targetEulers = transform.eulerAngles;
		targetPos = transform.position;
	} // End of Start().

	void FixedUpdate()
	{
        // Smoothly interpolate camera position/rotation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelTranslate, translateSmoothTime);

        Vector3 tempEulers = transform.eulerAngles;
        tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetEulers.x, ref smoothVelEulersX, rotSmoothTime);
        tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetEulers.y, ref smoothVelEulersY, rotSmoothTime);
        transform.eulerAngles = tempEulers;

        // Toggle camera lock/free movement with Spacebar.
        if (Input.GetKey(KeyCode.Space) && !spacePressed)
        {
            spacePressed = true;
            cameraLock = !cameraLock;
        }
        if (!Input.GetKey(KeyCode.Space))
            spacePressed = false;


        cameraFocused = false;
        if (cameraLock)
        {
            Screen.lockCursor = false;

            // Move in/out with mousewheel.
            targetPos += Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;


            if(focusedNode)
            {
                cameraFocused = true;
                focusedPos = focusedNode.transform.position;

                // Pump calories in/out with up/down arrow keys.
                if(Input.GetKey(KeyCode.UpArrow))
                    focusedNode.calories += 10.0f * Time.deltaTime;
                if(Input.GetKey(KeyCode.DownArrow))
                    focusedNode.calories -= 10.0f * Time.deltaTime;
            }

            if(cameraFocused)
            {
                Vector2 xyToFocusedPos = Mathw.XYToPos(Camera.main.transform.position, focusedPos);
                targetEulers.x = xyToFocusedPos.x;
                targetEulers.y = xyToFocusedPos.y;

                // Zoom orbit with mousewheel.
                orbitDist -= Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;
                orbitDist = Mathf.Clamp(orbitDist, 2.5f, Mathf.Infinity);

                // Rotate orbit via directional input.
                if (Input.GetMouseButton(1))
                {
                    orbitEulers.x -= Input.GetAxis("Mouse Y") * orbitRotateSpeed;
                    orbitEulers.y += Input.GetAxis("Mouse X") * orbitRotateSpeed;
                }
                orbitEulers.x = Mathf.Clamp(orbitEulers.x, -90, 90);
                targetPos = focusedPos - (Quaternion.Euler(orbitEulers.x, orbitEulers.y, 0) * (Vector3.forward * orbitDist));
            }

            // If camera is locked, user can use cursor to manipulate nodes.
            if(lookedAtObject)
            {
                Node lookedAtNode = lookedAtObject.GetComponent<Node>();
                if (lookedAtNode)
                {
                    if (Input.GetMouseButton(0))
                    {
                        focusedNode = lookedAtNode;
                        Vector2 xyToNode = Mathw.XYToPos(Camera.main.transform.position, focusedNode.transform.position);
                        orbitEulers.x = xyToNode.x;
                        orbitEulers.y = xyToNode.y;
                    }
                    else if (Input.GetMouseButton(1))
                        lookedAtNode.signal = 1.0f;
                }
            }
        }
        else
        {
            Screen.lockCursor = true;
            lookedAtObject = null;
            focusedNode = null;

            // Rotate camera via mouse movement.
            targetEulers.x -= Input.GetAxis("Mouse Y") * cameraRotateSpeed;
            targetEulers.y += Input.GetAxis("Mouse X") * cameraRotateSpeed;
            targetEulers.x = Mathf.Clamp(targetEulers.x, -90, 90);

            // Translate position with keyboard input.
            if (Input.GetKey(KeyCode.W))
                targetPos += transform.forward * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                targetPos -= transform.forward * cameraMoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A))
                targetPos -= transform.right * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                targetPos += transform.right * cameraMoveSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
                targetPos += transform.up * cameraMoveSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.LeftControl))
                targetPos -= transform.up * cameraMoveSpeed * Time.deltaTime;
        }
		
		// Determine what the user is looking at.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit cameraLookHit;
		if( Physics.Raycast( mouseRay, out cameraLookHit ))
			lookedAtObject = cameraLookHit.transform.gameObject;
		else
			lookedAtObject = null;
	} // End of Update().


    void OnGUI()
    {
        // Show camera type/controls.
        GUI.skin = GameManager.readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        string viewInfo = "";

        // Camera settings
        if(!cameraLock)
            viewInfo += "FREE CAMERA\n(Spacebar to lock)\n\nWASD, Shift, Ctrl to navigate.\nMouse to look around.";
        if(cameraLock && !cameraFocused)
            viewInfo += "LOCKED CAMERA\n(Spacebar to unlock)\n\nMousewheel to move in/out.\nClick node to focus.";
        if (cameraLock && focusedNode)
            viewInfo += "ORBIT CAMERA\n(Spacebar to unlock)\n\nMousewheel to zoom in/out.\nRight click and drag mouse to rotate.\n\nRight click to generate signal.\nUp/Down arrows to input/siphon calories.\nClick another node to focus.";

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), viewInfo);


        // Indicate selected node.
        if(focusedNode)
        {
            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(focusedNode.transform.position);
            selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
            float selectionBoxWidth = 650.0f / Vector3.Distance(Camera.main.transform.position, focusedNode.transform.position);
            Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
            GUI.Label(selectionBoxRect, nodeSelectTex);
        }
    } // End of OnGUI().
}
