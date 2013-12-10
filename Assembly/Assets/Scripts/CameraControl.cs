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

    public static Quaternion autoOrbit = Quaternion.identity;
	
	[HideInInspector] public static GameObject lookedAtObject;

    bool cameraLock; // Toggled with spacebar: freelook disabled.
    bool cameraFocused; // Camera is focusing on a certain element.
    Vector3 focusedPos; // Position of the focused element.

    // Currently selected element.
    public static Node selectedNode;
    public static Assembly selectedAssembly;

    

    bool spacePressed;

    Node lookedAtNode = null;
    Node mouseClickedNode = null;
    Node mouseReleasedNode = null;


    public LineRenderer dragLineRenderer = null;

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
        if (WesInput.GetKey("Camera Lock") && !spacePressed){
            spacePressed = true;
            cameraLock = !cameraLock;
        }

        // Toggle freelook/camera lock.
        if(!WesInput.GetKey("Camera Lock"))
            spacePressed = false;

        // Camera defaults to unfocused, but if an element is selected this will become true.
        cameraFocused = false;

        // Determine what the user is looking at.
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit cameraLookHit;
        lookedAtNode = null;
		if( Physics.Raycast( mouseRay, out cameraLookHit )){
			lookedAtObject = cameraLookHit.transform.gameObject;
            lookedAtNode = lookedAtObject.GetComponent<Node>();
        }
		else
			lookedAtObject = null;

        // When not in free-look mode...
        if(cameraLock){
            Screen.lockCursor = false;
            
            if (selectedAssembly != null) {
                cameraFocused = true;
                focusedPos = selectedAssembly.GetCenter();
            }

            if (selectedNode != null) {
                cameraFocused = true;
                focusedPos = selectedNode.transform.position;
            }

            // Rotate orbit via directional input.
            if (Input.GetMouseButton(1)){
                targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
                targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);
            }

            // If we are focusing on some object...
            if(cameraFocused){
                // Zoom orbit with mousewheel.
                orbitDist -= (orbitDist * 0.3f) * Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;
                orbitDist = Mathf.Clamp(orbitDist, 3f, Mathf.Infinity);

                targetPos = focusedPos - (targetRot * (Vector3.forward * orbitDist));
            }
            // If camera is locked but not focused...
            else{
                // Move in/out with mousewheel.
                targetPos += Camera.main.transform.forward * Input.GetAxis("Mouse ScrollWheel") * orbitZoomSpeed;
            }


            if(lookedAtObject){
                
                if(lookedAtNode && Input.GetMouseButtonDown(0))
                    mouseClickedNode = lookedAtNode;
                else if(lookedAtNode && Input.GetMouseButtonUp(0))
                    mouseReleasedNode = lookedAtNode;

                // 'Selecting' a single node.
                if(mouseReleasedNode && (mouseClickedNode == mouseReleasedNode)){
                    Node clickAndReleaseNode = mouseReleasedNode;
                    // Select the assembly attached to the node, if applicable...
                    if((clickAndReleaseNode.assembly != null) && (selectedAssembly != clickAndReleaseNode.assembly) && (!selectedNode || (selectedNode == clickAndReleaseNode) || (clickAndReleaseNode.assembly != selectedNode.assembly))){
                        selectedNode = null;
                        FocusOnAssembly(clickAndReleaseNode.assembly);
                    }
                    // Otherwise just select the node.
                    else{
                        selectedNode = clickAndReleaseNode;
                        selectedAssembly = null;
                        targetRot = Quaternion.LookRotation(selectedNode.transform.position - transform.position, Camera.main.transform.up);
                    }
                }
            }


            // Create a new bond.
            if(mouseClickedNode && mouseReleasedNode && !mouseClickedNode.BondedTo(mouseReleasedNode) && (mouseClickedNode != mouseReleasedNode) && (mouseClickedNode.BondCount() < 3) && (mouseReleasedNode.BondCount() < 3)){
                new Bond(mouseClickedNode, mouseReleasedNode);
                ConsoleScript.NewLine("Manually created a bond.");
            }
            // Destroy an existing bond.
            else if(mouseClickedNode && mouseReleasedNode && mouseClickedNode.BondedTo(mouseReleasedNode)){
                mouseClickedNode.GetBondTo(mouseReleasedNode).Destroy();
                ConsoleScript.NewLine("Manually removed a bond.");
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
        targetRot *= Quaternion.AngleAxis(WesInput.rotationThrottle * -cameraRotateSpeed, Vector3.forward);

        if(!cameraFocused){
            // Navigation
            // Translate position with keyboard input.
            targetPos += WesInput.forwardThrottle * transform.forward * cameraMoveSpeed * Time.deltaTime;
            targetPos += WesInput.horizontalThrottle * transform.right * cameraMoveSpeed * Time.deltaTime;
            targetPos += WesInput.verticalThrottle * transform.up * cameraMoveSpeed * Time.deltaTime;
        }


        // Auto-orbit
        targetRot = Quaternion.RotateTowards(targetRot, targetRot * autoOrbit, Time.deltaTime * 2.5f);


        if(Input.GetMouseButtonUp(0)){
            mouseClickedNode = null;
            mouseReleasedNode = null;
        }

        dragLineRenderer.enabled = false;
        if(mouseClickedNode && lookedAtNode){
            dragLineRenderer.SetPosition(0, mouseClickedNode.transform.position);
            dragLineRenderer.SetPosition(1, lookedAtNode.transform.position);

            if(mouseClickedNode.BondedTo(lookedAtNode)){
                dragLineRenderer.SetColors(Color.yellow, Color.red);
                dragLineRenderer.enabled = true;
            }
            else if((mouseClickedNode.BondCount() < 3) && (lookedAtNode.BondCount() < 3)){
                dragLineRenderer.SetColors(Color.green, Color.white);
                dragLineRenderer.enabled = true;
            }
        }
    } // End of Update(). 


    public static void OrbitOn(){
        autoOrbit = Random.rotation;
    }


    public static void OrbitOff(){
        autoOrbit = Quaternion.identity;
    }


    void OnGUI(){
        // Show camera type/controls information.
        GUI.skin = GameManager.readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        string viewInfo = "";

        // Camera settings
        if(!cameraLock)
            viewInfo += "FREE CAMERA\n(Spacebar to lock)\n\nWASD, Shift, Ctrl to move around.\nMouse rotates the camera.";
        if(cameraLock){
            if(!cameraFocused)
                viewInfo += "LOCKED CAMERA\n(Spacebar to unlock)\n\nMousewheel to move forward/back.\nRight click and drag to rotate.\nClick an entity to focus on it.";
            else{
                viewInfo += "ORBIT CAMERA\n(Spacebar to unlock)\n\nMousewheel to zoom in/out.\nRight click and drag to orbit.\n\n";
                if(selectedNode){
                    viewInfo += "Click another node to select it.\n";
                    if(selectedNode.assembly != null)
                        viewInfo += "Click the selected node to select its assembly.";
                }
                if(selectedAssembly != null)
                    viewInfo += "Click a node to select it.\n";
            }
            viewInfo += "\n\nClick and drag between nodes to create a bond.";

            // Indicate dragging node.
            if(lookedAtNode){
                Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(lookedAtNode.transform.position);
                selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
                float selectionBoxWidth = 700.0f / Vector3.Distance(Camera.main.transform.position, lookedAtNode.transform.position);
                Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
                GUI.Label(selectionBoxRect, GameManager.graphics.nodeModifyTex);
            }
        }
		
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), viewInfo);

        // Indicate selected node.
        if(selectedNode){
            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(selectedNode.transform.position);
            selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
            float selectionBoxWidth = 650.0f / Vector3.Distance(Camera.main.transform.position, selectedNode.transform.position);
            Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
            GUI.Label(selectionBoxRect, GameManager.graphics.nodeSelectTex);
        }

        // Indicate dragging node.
        if(mouseClickedNode){
            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(mouseClickedNode.transform.position);
            selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
            float selectionBoxWidth = 700.0f / Vector3.Distance(Camera.main.transform.position, mouseClickedNode.transform.position);
            Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
            GUI.Label(selectionBoxRect, GameManager.graphics.nodeModifyTex);
        }

        // Indicate dragging node.
        if(mouseReleasedNode){
            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(mouseReleasedNode.transform.position);
            selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
            float selectionBoxWidth = 700.0f / Vector3.Distance(Camera.main.transform.position, mouseReleasedNode.transform.position);
            Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
            GUI.Label(selectionBoxRect, GameManager.graphics.nodeModifyTex);
        }

        

        // Label assemblies.
        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly currentAssembly = Assembly.GetAll()[i];

            GUI.color = Color.white;
            // Highlight all nodes in selected Assembly.
            if(currentAssembly == selectedAssembly){
                for(int j = 0; j < selectedAssembly.nodes.Count; j++) {
                    Node assemblyNode = selectedAssembly.nodes[j];
                    if (Camera.main.transform.InverseTransformPoint(assemblyNode.transform.position).z > 0) {
                        Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(assemblyNode.transform.position);
                        selectedNodeScreenPos.y = Screen.height - selectedNodeScreenPos.y;
                        float selectionBoxWidth = 750.0f / Vector3.Distance(Camera.main.transform.position, assemblyNode.transform.position);
                        Rect selectionBoxRect = new Rect(selectedNodeScreenPos.x - (selectionBoxWidth * 0.5f), selectedNodeScreenPos.y - (selectionBoxWidth * 0.5f), selectionBoxWidth, selectionBoxWidth);
                        GUI.Label(selectionBoxRect, GameManager.graphics.nodeSelectTex);
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
                Rect currentAssemblyNametagRect = new Rect(currentAssemblyScreenPos.x - 2000, currentAssemblyScreenPos.y - 1000, 4000, 2000);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(currentAssemblyNametagRect, currentAssembly.name);
                GUI.skin.label.fontSize = holdFontSize;
                GUI.skin.label.fontSize = 10;
            }
        }


        // Selected entity details.
        Rect entityDetailRect = new Rect(0, 0, Screen.width - 5, Screen.height);
        GUI.skin.label.alignment = TextAnchor.UpperRight;
        GUI.color = Color.white;
        string entityInfo = "";

        if(selectedNode){
            entityInfo += selectedNode.nodeName + "\n";
            entityInfo += "\n";
            entityInfo += "Type: " + selectedNode.nodeType.ToString() + "\n";
            if(selectedNode.assembly != null){
                entityInfo += "Part of '" + selectedNode.assembly.name + ",' index " + selectedNode.assemblyIndex + "\n";
            }
            entityInfo += "\n";
            entityInfo += "Number of bonds: " + selectedNode.bonds.Count;
            entityInfo += "\n";
            entityInfo += selectedNode.calories + " calories\n";
            entityInfo += selectedNode.signal + " signal\n";
            entityInfo += selectedNode.synapse + " synapse\n";
        }
        else if(selectedAssembly != null){
            entityInfo += selectedAssembly.name + "\n";
            entityInfo += "\n";
            entityInfo += "Number of nodes: " + selectedAssembly.nodes.Count + "\n";
            entityInfo += "Age: " + Mathf.FloorToInt(selectedAssembly.age) + " sec\n";
            entityInfo += "Stage: " + selectedAssembly.lifeStage.ToString() + "\n";
            entityInfo += "\n";
            entityInfo += "Calories: " + selectedAssembly.calories.ToString("F2") + "\n";
            entityInfo += "Health: " + ((selectedAssembly.calories / selectedAssembly.nodes.Count) * 100).ToString("F0") + "%\n";
            entityInfo += "Calorie intake/burn: " + selectedAssembly.averageDeltaCalories.ToString("F2") + "/sec\n";
        }

        GUI.Label(entityDetailRect, entityInfo);

    } // End of OnGUI().


    // Focuses and adjusts the camera to focus on an assembly.
    void FocusOnAssembly(Assembly focusAssem){
        selectedAssembly = focusAssem;
        selectedNode = null;
        targetRot = Quaternion.LookRotation(focusAssem.GetCenter() - transform.position, Camera.main.transform.up);
        orbitDist = focusAssem.GetRadius() * 3;
    }
} // End of CameraControl.
