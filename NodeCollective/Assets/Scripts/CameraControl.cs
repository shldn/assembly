using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {
	
	public float cameraRotateSpeed = 2.0f;
	public float cameraMoveSpeed = 2.0f;

    public float orbitRotateSpeed = 2.0f;
    public float orbitZoomSpeed = 6.0f;

	Quaternion targetRot = Quaternion.identity;

    Vector3 targetPos; // Target position of camera translation.
    Vector3 smoothVelTranslate;
    public float translateSmoothTime = 0.1f;
    float orbitDist = 10.0f; // Distance the camera will orbit from focusedPos.

    Quaternion autoOrbit = Quaternion.identity;
	
	[HideInInspector] public static GameObject lookedAtObject;

    bool cameraLock; // Toggled with spacebar: freelook disabled.
    bool cameraFocused; // Camera is focusing on a certain element.
    Vector3 focusedPos; // Position of the focused element.

    // Currently selected element.
    Node selectedNode;
    public Assembly selectedAssembly;

    // Texture (crosshair) shown around selected ndoes.
    public Texture2D nodeSelectTex;

    bool spacePressed;


	void Start(){
		targetRot = transform.rotation;
		targetPos = transform.position;
	} // End of Start().


	void Update(){
        // Smoothly interpolate camera position/rotation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelTranslate, translateSmoothTime);
        Quaternion tempRot = transform.rotation;
        tempRot = Quaternion.Slerp(tempRot, targetRot, 5 * Time.deltaTime);
        transform.rotation = tempRot;

        // Toggle camera lock/free movement with Spacebar.
        if (Input.GetKey(KeyCode.Space) && !spacePressed){
            spacePressed = true;
            cameraLock = !cameraLock;
        }

        // Toggle freelook/camera lock.
        if(!Input.GetKey(KeyCode.Space))
            spacePressed = false;

        // Camera defaults to unfocused, but if an element is selected this will become true.
        cameraFocused = false;

        // Determine what the user is looking at.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit cameraLookHit;
		if( Physics.Raycast( mouseRay, out cameraLookHit ))
			lookedAtObject = cameraLookHit.transform.gameObject;
		else
			lookedAtObject = null;

        // When not in free-look mode...
        if(cameraLock){
            Screen.lockCursor = false;
            
            if(selectedNode){
                cameraFocused = true;
                focusedPos = selectedNode.transform.position;

                // Pump calories in/out of selected node with up/down arrow keys.
                if(Input.GetKey(KeyCode.UpArrow))
                    selectedNode.calories += 10.0f * Time.deltaTime;
                if(Input.GetKey(KeyCode.DownArrow))
                    selectedNode.calories -= 10.0f * Time.deltaTime;
            }

            if (selectedAssembly != null) {
                cameraFocused = true;
                focusedPos = selectedAssembly.GetCenter();
            }

            // If we are focusing on some object...
            if(cameraFocused){
                // Zoom orbit with mousewheel.
                orbitDist -= (orbitDist * 0.3f) * Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;
                orbitDist = Mathf.Clamp(orbitDist, 3f, Mathf.Infinity);

                // Rotate orbit via directional input.
                if (Input.GetMouseButton(1)){
                    targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
                    targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);
                }
                targetPos = focusedPos - (targetRot * (Vector3.forward * orbitDist));
            }
            // If camera is locked but not focused...
            else{
                // Move in/out with mousewheel.
                targetPos += Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;
            }

            // If camera is locked, user can use cursor to manipulate assemblies/nodes.
            // If the cursor is over some object...
            if(lookedAtObject){
                // If we click on a node...
                Node lookedAtNode = lookedAtObject.GetComponent<Node>();
                if(lookedAtNode){
                    if (Input.GetMouseButtonDown(0)){
                        // Select the assembly attached to the node, if applicable...
                        if((lookedAtNode.myAssembly != null) && (selectedAssembly != lookedAtNode.myAssembly) && (!selectedNode || (selectedNode == lookedAtNode) || (lookedAtNode.myAssembly != selectedNode.myAssembly))){
                            selectedNode = null;
                            FocusOnAssembly(lookedAtNode.myAssembly);
                        }
                        // Otherwise just select the node.
                        else{
                            selectedNode = lookedAtNode;
                            selectedAssembly = null;
                            targetRot = Quaternion.LookRotation(selectedNode.transform.position - transform.position, Camera.main.transform.up);
                        }
                    }
                    // DEBUG - Right click will pump in signal.
                    else if (Input.GetMouseButton(1))
                        lookedAtNode.signal = 1.0f;
                }
				
				FoodPellet lookedAtPellet = lookedAtObject.GetComponent<FoodPellet>();
            }
        }
        // If we're in free-look mode...
        else{
            Screen.lockCursor = true;
            lookedAtObject = null;
            selectedNode = null;
            selectedAssembly = null;

            // Pitch/yaw camera via mouse movement.
            targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
            targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);
        }

        // Roll camera using Q and E
        if (Input.GetKey(KeyCode.Q))
            targetRot *= Quaternion.AngleAxis(1f * cameraRotateSpeed, Vector3.forward);
        if (Input.GetKey(KeyCode.E))
            targetRot *= Quaternion.AngleAxis(1f * cameraRotateSpeed, -Vector3.forward);

        if(!cameraFocused){
            // Navigation
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


        // Make new assembly based on the selected node when Return is pressed.
        if (selectedNode) {
            if(selectedNode && (selectedNode.myAssembly == null) && Input.GetKeyDown(KeyCode.Return)){
                // Gonna plop the new assembly into the GameManager.allAssemblies list.
                Assembly[] newAssemblies = new Assembly[GameManager.allAssemblies.Length + 1];
                for (int i = 0; i < GameManager.allAssemblies.Length; i++)
                    newAssemblies[i] = GameManager.allAssemblies[i];

                // Make our new assembly.
                Assembly newAssembly = GameManager.GetAssembly(selectedNode);

                // Update GameManager assembly list and select the new assembly.
                newAssemblies[newAssemblies.Length - 1] = newAssembly;
                GameManager.allAssemblies = newAssemblies;
                FocusOnAssembly(newAssembly);
            }
        }


        // Auto-random-orbit toggled by 'O' key.
        if(Input.GetKeyDown(KeyCode.O)){
            if(autoOrbit == Quaternion.identity)
                autoOrbit = Random.rotation;
            else
                autoOrbit = Quaternion.identity;
        }
        targetRot = Quaternion.RotateTowards(targetRot, targetRot * autoOrbit, Time.deltaTime * 2.5f);
    } // End of Update(). 


    void OnGUI(){
        // Show camera type/controls.
        GUI.skin = GameManager.readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        string viewInfo = "";

        // Camera settings
        if(!cameraLock)
            viewInfo += "FREE CAMERA\n(Spacebar to lock)\n\nWASD, Shift, Ctrl to navigate.\nMouse to look around.";
        if(cameraLock && !cameraFocused)
            viewInfo += "LOCKED CAMERA\n(Spacebar to unlock)\n\nMousewheel to move in/out.\nClick node to focus.";
        if (cameraLock && selectedNode)
            viewInfo += "ORBIT CAMERA\n(Spacebar to unlock)\n\nMousewheel to zoom in/out.\nRight click and drag mouse to rotate.\n\nRight click to generate signal.\nUp/Down arrows to input/siphon calories.\nClick another node to focus.";
		
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), viewInfo);

        // Indicate selected node.
        if(selectedNode){
            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(selectedNode.transform.position);
            selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
            float selectionBoxWidth = 650.0f / Vector3.Distance(Camera.main.transform.position, selectedNode.transform.position);
            Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
            GUI.Label(selectionBoxRect, nodeSelectTex);

            // Show node details.
            Rect nodeInfoRect = new Rect(0, 0, Screen.width - 5, Screen.height);
            GUI.skin.label.alignment = TextAnchor.UpperRight;
            string nodeInfo = "";
            nodeInfo += selectedNode.nodeName + " [" + selectedNode.nodeType.ToString() + "]\n";
            if(selectedNode.myAssembly != null){
                nodeInfo += "Part of '" + selectedNode.myAssembly.callsign + "'\n";
                nodeInfo += "Index " + selectedNode.assemblyIndex + "\n";
            }
            nodeInfo += "\n";
            nodeInfo += selectedNode.signal + "[signal]\n";
            nodeInfo += selectedNode.synapse + "[synapse]\n";
            GUI.Label(nodeInfoRect, nodeInfo);
        }

        // Label assemblies.
        for(int i = 0; i < GameManager.allAssemblies.Length; i++){
            Assembly currentAssembly = GameManager.allAssemblies[i];

            GUI.color = Color.white;
            // Highlight all nodes in selected Assembly.
            if(currentAssembly == selectedAssembly){
                for(int j = 0; j < selectedAssembly.nodes.Length; j++) {
                    Node assemblyNode = selectedAssembly.nodes[j];
                    if (Camera.main.transform.InverseTransformPoint(assemblyNode.transform.position).z > 0) {
                        Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(assemblyNode.transform.position);
                        selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
                        float selectionBoxWidth = 750.0f / Vector3.Distance(Camera.main.transform.position, assemblyNode.transform.position);
                        Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
                        GUI.Label(selectionBoxRect, nodeSelectTex);
                    }
                }
            }
            else
                GUI.color = new Color(1f, 1f, 1f, 0.25f);

            // Show selected assembly's name.
            if(Camera.main.transform.InverseTransformPoint(currentAssembly.GetCenter()).z > 0){
                Vector3 currentAssemblyScreenPos = Camera.main.WorldToScreenPoint(currentAssembly.GetCenter());
                currentAssemblyScreenPos.y = Screen.height - currentAssemblyScreenPos.y;
                int holdFontSize = GUI.skin.label.fontSize;
                GUI.skin.label.fontSize = (int) Mathf.Clamp((550.0f / Vector3.Distance(Camera.main.transform.position, currentAssembly.GetCenter())), 10, Mathf.Infinity);
                Rect currentAssemblyNametagRect = new Rect(currentAssemblyScreenPos.x - 2000, currentAssemblyScreenPos.y, 4000, 2000);
                GUI.Label(currentAssemblyNametagRect, currentAssembly.callsign);
                GUI.skin.label.fontSize = holdFontSize;
                GUI.skin.label.fontSize = 10;
            }
        }

    } // End of OnGUI().


    // Focuses and adjusts the camera to focus on an assembly.
    void FocusOnAssembly(Assembly focusAssem){
        selectedAssembly = focusAssem;
        selectedNode = null;
        targetRot = Quaternion.LookRotation(focusAssem.GetCenter() - transform.position, Camera.main.transform.up);
        orbitDist = focusAssem.GetRadius() * 3;
    }
} // End of CameraControl.
