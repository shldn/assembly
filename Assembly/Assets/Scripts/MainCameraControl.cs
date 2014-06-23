using UnityEngine;
using System.Collections;

public enum CamType {LOCKED, FREELOOK, ORBIT_CONTROLLED, ORBIT_DEMO}

public class MainCameraControl : MonoBehaviour {

    public static MainCameraControl Inst = null;
    Quaternion targetRot;

    Vector3 targetPos = Vector3.zero;
    Vector3 smoothVelTranslate = Vector3.zero;
    Vector3 smoothVelRotate = Vector3.zero;

    float translateSmoothTime = 0.2f;

    float cameraMoveSpeed = 30f;
    float cameraRotateSpeed = 2f;

    Node hoveredNode = null;
    public Node selectedNode = null;
    public Assembly selectedAssembly = null;

    public CamType camType = CamType.ORBIT_DEMO;
    float camOrbitDist = 100f;


    public Texture2D nodeSelectTex = null;
    public Texture2D assemblySelectTex = null;

    public DepthOfField34 depthOfField = null;

    public Quaternion randomOrbit = Quaternion.identity;
    float demoOrbitDistRunner = 0f;

    float camMaxOrbit = 500f;
    float camMinOrbit = 100f;

    void Awake(){
        depthOfField = Camera.main.GetComponent("DepthOfField34") as DepthOfField34;
    } // End of Awake().

	// Use this for initialization
	void Start(){
        Inst = this;

	    targetRot = transform.rotation;
		targetPos = transform.position;
        RenderSettings.fogColor = Camera.main.backgroundColor;
        randomOrbit = Random.rotation;
	} // End of Start().
	
	// Update is called once per frame
	void FixedUpdate(){

        float camZoomLoopTime = 60f;

        // Smoothly interpolate camera position/rotation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelTranslate, translateSmoothTime * Time.timeScale);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 10f * (GameManager.simStep / Time.timeScale));



        // Gallery-style demo-mode: camera orbits center, zooming in and out slowly.
        if(camType == CamType.ORBIT_DEMO){
            targetRot = Quaternion.RotateTowards(targetRot, targetRot * randomOrbit, 1f * (GameManager.simStep / Time.timeScale));

            camMaxOrbit += camMaxOrbit * -Input.GetAxis("Mouse ScrollWheel");
            camMinOrbit += camMinOrbit * -Input.GetAxis("Mouse ScrollWheel");

            camOrbitDist = Mathf.Lerp(camMaxOrbit, camMinOrbit, ((Mathf.Sin((demoOrbitDistRunner * Mathf.PI) / camZoomLoopTime)) * 0.5f) + 0.5f);

            demoOrbitDistRunner += GameManager.simStep;
        }


        // Orbit a selected object
        if((camType == CamType.ORBIT_CONTROLLED) || (camType == CamType.ORBIT_DEMO)){
            Vector3 orbitTarget = Vector3.zero;

            // Orbit the selected entity.
            if(selectedNode)
                orbitTarget = selectedNode.worldPosition;
            else if(selectedAssembly){
                orbitTarget = selectedAssembly.WorldPosition;
                if(Input.GetKey(KeyCode.F))
                    selectedAssembly.WorldPosition += transform.forward * 10f * GameManager.simStep;
                if(Input.GetKey(KeyCode.H))
                    selectedAssembly.currentEnergy += 5f * GameManager.simStep;
            }

            // Camera's rotation becomes the rotation of the 'boom' on which it orbits.
            targetPos = orbitTarget + (targetRot * -Vector3.forward) * camOrbitDist;

            // Orbit distance can be modified using the mousewheel.
            camOrbitDist += camOrbitDist * -Input.GetAxis("Mouse ScrollWheel");

            // Orbit can be changed by holding mouse button.
            if(Input.GetMouseButton(1) && !GameManager.Inst.pauseMenu)
                HandleMouseOrbit();

            // Camera's focal point and distance changes based on camera orbit distance.
            if(depthOfField){
                depthOfField.focalPoint = Mathf.Lerp(depthOfField.focalPoint, Vector3.Distance(Camera.main.transform.position, orbitTarget), (GameManager.simStep / Time.timeScale) * 5f);
                depthOfField.focalSize = Vector3.Distance(Camera.main.transform.position, orbitTarget) * 0.1f;
            }

            // Hitting 'space' will break out of orbit mode.
            if(Input.GetKeyDown(KeyCode.Space) && !GameManager.Inst.pauseMenu){
                selectedAssembly = null;
                selectedNode = null;
                camType = CamType.FREELOOK;
            }
        }
        else if((camType == CamType.FREELOOK) || (camType == CamType.LOCKED)){

            // Translate position with keyboard input.
            targetPos += WesInput.forwardThrottle * transform.forward * cameraMoveSpeed * (GameManager.simStep / Time.timeScale);
            targetPos += WesInput.horizontalThrottle * transform.right * cameraMoveSpeed * (GameManager.simStep / Time.timeScale);
            targetPos += WesInput.verticalThrottle * transform.up * cameraMoveSpeed * (GameManager.simStep / Time.timeScale);

            // Free-moving camera, moves with mouse and keyboard controls.
            if(camType == CamType.FREELOOK){

                // Camera rotates with mouse input.
                HandleMouseOrbit();

                // Space locks the camera in place.
                if(Input.GetKeyDown(KeyCode.Space) && !GameManager.Inst.pauseMenu)
                    camType = CamType.LOCKED;
            }
            // Locked camera does not move with mouse input.
            else if(camType == CamType.LOCKED){

                // Holding right mouse will rotate camera.
                if(Input.GetMouseButton(1) && !GameManager.Inst.pauseMenu)
                    HandleMouseOrbit();

                // Space breaks out into freelook mode.
                if(Input.GetKeyDown(KeyCode.Space) && !GameManager.Inst.pauseMenu)
                    camType = CamType.FREELOOK;
            }
        }

        // Roll camera using Q and E... generally works in every mode.
        targetRot *= Quaternion.AngleAxis(WesInput.rotationThrottle * -cameraRotateSpeed, Vector3.forward);

        // Determine if a node is being hovered over.
        hoveredNode = null;
        if(selectedAssembly){
            RaycastHit mouseRayHitNode = new RaycastHit();
            if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), out mouseRayHitNode, Mathf.Infinity, (1 << LayerMask.NameToLayer("Nodes")))){
                for(int i = 0; i < Node.GetAll().Count; i++){
                    Node currentNode = Node.GetAll()[i];
                    if((currentNode.gameObject == mouseRayHitNode.collider.gameObject) && (currentNode.assembly == selectedAssembly)){
                        hoveredNode = currentNode;
                        break;
                    }
                }
            }
        }

        Assembly hoveredAssembly = null;
        // Select an assembly
        RaycastHit mouseRayHit = new RaycastHit();
        if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), out mouseRayHit, Mathf.Infinity, (1 << LayerMask.NameToLayer("Assemblies")))){
            for(int i = 0; i < Assembly.GetAll().Count; i++){
                Assembly currentAssembly = Assembly.GetAll()[i];
                if((currentAssembly != selectedAssembly) && (currentAssembly.physicsObject == mouseRayHit.collider.gameObject)){
                    hoveredAssembly = currentAssembly;

                    if(Input.GetMouseButtonDown(0))
                        FocusOn(hoveredAssembly);

                    break;
                }
            }
        }      
        

        // When clicking on a node...
        if(Input.GetMouseButtonDown(0) && hoveredNode && !NodeEngineering.Inst.uiLockout && !GameManager.Inst.pauseMenu){
            // If nothing is selected currently, select it's assembly.
            
            // If a node is selected currently...
            if(selectedNode){
                // If you click again on the selected node, select it's assembly.
                if(hoveredNode == selectedNode){
                    FocusOn(hoveredNode.assembly);
                    selectedNode = null;
                }
                // If clicking on another node in the same assembly, select that other node.
                else
                    FocusOn(hoveredNode);
            }
            // If an assembly is selected...
            else
                FocusOn(hoveredNode);
        }

        // Return to demo mode with Enter.
        if(Input.GetKey(KeyCode.Return) && !GameManager.Inst.pauseMenu){
            randomOrbit = Random.rotation;
            selectedAssembly = null;
            selectedNode = null;
            demoOrbitDistRunner = 0f;

            camType = CamType.ORBIT_DEMO;
        }

	} // End of Update().


    void HandleMouseOrbit(){
        targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
        targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);
    } // End of HandleOrbit().


    void FocusOn(Vector3 pos){
        Vector3 vectorToPos = pos - transform.position;
        camOrbitDist = vectorToPos.magnitude;
        targetRot = Quaternion.LookRotation(vectorToPos, transform.up);

        camType = CamType.ORBIT_CONTROLLED;
    }
    void FocusOn(Node node){
        FocusOn(node.worldPosition);
        
        selectedNode = node;
        camType = CamType.ORBIT_CONTROLLED;
    }
    void FocusOn(Assembly assembly){
        if(!assembly || !assembly.physicsObject)
            return;

        selectedNode = null;
        selectedAssembly = assembly;
        FocusOn(assembly.physicsObject.transform.position);   
    }// End of FocusOn().


    void OnGUI(){

        // Assembly health/name/etc. readouts.
        if(GameManager.Inst.showAssemReadouts){
            for(int i = 0; i < Assembly.GetAll().Count; i++){
                Assembly currentAssembly = Assembly.GetAll()[i];
                Vector3 assemblyScreenPos = Camera.main.WorldToScreenPoint(Assembly.GetAll()[i].WorldPosition);

                if(assemblyScreenPos.z <= 0f)
                    continue;

                float guiSizeMult = 40f / Vector3.Distance(Camera.main.transform.position, currentAssembly.WorldPosition);

                float barWidth = 50f * guiSizeMult;
                float barHeight = 6f * guiSizeMult;
                float barSpace = 3f * guiSizeMult;

                if(barWidth < 4f)
                    continue;

                float guiStuffY = Screen.height - assemblyScreenPos.y;

                // Name
                GUI.color = Color.white;
                GUI.skin.label.fontSize = Mathf.CeilToInt(guiSizeMult * 6);
                Rect nameRect = GUIHelper.Inst.CenteredRect(assemblyScreenPos.x, Screen.height - (assemblyScreenPos.y + 100f + (guiSizeMult * 8f)), 500f, 200f);
                GUI.skin.label.fontStyle = FontStyle.Bold;
                GUI.skin.label.alignment = TextAnchor.LowerCenter;
                GUI.Label(nameRect, currentAssembly.name);

                // Health bar
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                GUIHelper.Inst.DrawCenteredRect(assemblyScreenPos.x, guiStuffY, barWidth, barHeight);
                GUI.color = new Color(1f, 1f, 1f, 1f);
                GUIHelper.Inst.DrawCenteredFillBar(assemblyScreenPos.x, guiStuffY, barWidth, barHeight, Mathf.Clamp01(currentAssembly.Health));

                // Reproduction bar
                if(currentAssembly.Health > 1f){
                    guiStuffY += barHeight + barSpace;

                    GUI.color = new Color(0f, 1f, 0f, 0.2f);
                    GUIHelper.Inst.DrawCenteredRect(assemblyScreenPos.x, guiStuffY, barWidth, barHeight * 0.5f);
                    GUI.color = new Color(0f, 1f, 0f, 1f);
                    GUIHelper.Inst.DrawCenteredFillBar(assemblyScreenPos.x, guiStuffY, barWidth, barHeight * 0.5f, Mathf.Clamp01(currentAssembly.Health - 1f));
                }
            }
        }

        // Camera controls
        GUI.color = Color.white;
        float guiHeight = 18f;
        float guiGutter = 10f;
        Rect controlsRect = new Rect(Screen.width - 225, 15f, 200, guiHeight);

        Rect centeredInfoRect = new Rect(0f, -Screen.height * 0.05f, Screen.width, Screen.height);
        GUI.skin.label.alignment = TextAnchor.LowerCenter;
        
        string cameraTypeLabel = "";
        string cameraInfo = "";
        if(camType == CamType.FREELOOK){
            cameraTypeLabel += "Free camera";
            cameraInfo += "Rotate the camera using the mouse, Q, and E.\n";
            cameraInfo += "Use W, A, S, and D to move the camera around.\n";
            cameraInfo += "Click on an assembly to select it.\n";
            cameraInfo += "Press SPACE to lock the camera in place.\n";
            cameraInfo += "\nPress ENTER to return to demonstration mode.\n";
        }
        if(camType == CamType.LOCKED){
            cameraTypeLabel += "Locked camera";
            cameraInfo += "Use W, A, S, and D to move the camera around.\n";
            cameraInfo += "Click on an assembly to select it.\n";
            cameraInfo += "Press SPACE to free the rotation of the camera.\n";
            cameraInfo += "\nPress ENTER to return to demonstration mode.\n";
        }
        if(camType == CamType.ORBIT_CONTROLLED){
            cameraTypeLabel += "Case-Orbit camera";
            cameraInfo += "Click on a node (or another assembly) to select it.\n";
            cameraInfo += "Use the MOUSEWHEEL to zoom in and out.\n";
            if(selectedAssembly){
                cameraInfo += "Use the arrow keys to rotate this assembly manually.\n";
            }
            else if(selectedNode){
                cameraInfo += "Click on the selected node to select its assembly.\n";
                cameraInfo += "Drag the vector handles to change this node's traits.\n";
            }
            cameraInfo += "Hold right-click to rotate the camera.\n";
            cameraInfo += "\nPress SPACE to disengage.\n";
            cameraInfo += "Press ENTER to return to demonstration mode.\n";
        }
        if(camType == CamType.ORBIT_DEMO){
            cameraTypeLabel += "Exhibition Camera";
            cameraInfo += "Click on an assembly to investigate it.\n";
        }

        cameraInfo += "\nPress ESC for menu.";

        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin.label.fontSize = 10;
        GUI.Label(centeredInfoRect, cameraInfo);
        centeredInfoRect.y += 15f;

        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.fontSize = 13;
        GUI.Label(centeredInfoRect, cameraTypeLabel);


        
        if(GameManager.Inst.showControls){

            

            /*
            bool camLocked = camType == CamType.LOCKED;
            if(camType != CamType.ORBIT){
                camLocked = GUI.Toggle(controlsRect, camLocked, "Camera Locked");
        
                if(camLocked)
                    camType = CamType.LOCKED;
                else if(!camLocked)
                    camType = CamType.FREELOOK;
            }
            */
        }

        if(selectedNode){
            GUI.color = Color.white;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.Label(MathUtilities.CenteredSquare(selectedNode), nodeSelectTex);
        }

        if(!NodeEngineering.Inst.uiLockout){
            if(hoveredNode){
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(MathUtilities.CenteredSquare(hoveredNode), nodeSelectTex);
            }
            /*
            if(selectedAssembly){
                // Selection ring
                GUI.color = new Color(0.5f, 1f, 0.5f, 0.2f);
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUI.Label(MathUtilities.CenteredSquare(selectedAssembly), assemblySelectTex);
            

                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                for(int i = 0; i < selectedAssembly.nodes.Count; i++){
                    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(MathUtilities.CenteredSquare(selectedAssembly.nodes[i]), nodeSelectTex);
                }
            }
            */
        }
    } // End of OnGUI().


} // End of MainCameraControl.
 