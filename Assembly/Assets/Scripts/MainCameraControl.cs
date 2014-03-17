using UnityEngine;
using System.Collections;

public enum CamType {LOCKED, FREELOOK, ORBIT}

public class MainCameraControl : MonoBehaviour {

    public static MainCameraControl Inst = null;
    Quaternion targetRot;

    Vector3 targetPos = Vector3.zero;
    Vector3 smoothVelTranslate = Vector3.zero;
    Vector3 smoothVelRotate = Vector3.zero;

    float translateSmoothTime = 0.2f;

    float cameraMoveSpeed = 10f;
    float cameraRotateSpeed = 2f;

    Node hoveredNode = null;
    public Node selectedNode = null;
    Assembly selectedAssembly = null;

    CamType camType = CamType.FREELOOK;
    float camOrbitDist = 100f;

    public bool showAssemReadouts = true;

    public Texture2D nodeSelectTex = null;
    public Texture2D assemblySelectTex = null;

    public DepthOfField34 depthOfField = null;

    public bool autoOrbit = true;
    public Quaternion randomOrbit = Quaternion.identity;


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

        if(autoOrbit){
            targetRot = Quaternion.RotateTowards(targetRot, targetRot * randomOrbit, 1f * Time.deltaTime);
            camType = CamType.ORBIT;
        }
        


        // Smoothly interpolate camera position/rotation.
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref smoothVelTranslate, translateSmoothTime);

        /*
        // This code produces gimbal lock...
        Vector3 tempEulers = transform.rotation.eulerAngles;
        tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetRot.eulerAngles.x, ref smoothVelRotate.x, translateSmoothTime);
        tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetRot.eulerAngles.y, ref smoothVelRotate.y, translateSmoothTime);
        tempEulers.z = Mathf.SmoothDampAngle(tempEulers.z, targetRot.eulerAngles.z, ref smoothVelRotate.z, translateSmoothTime);
        print(tempEulers);
        transform.eulerAngles = tempEulers;
        */

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 10f * Time.deltaTime);


        // check if selectedAssembly has been destroyed
        if (selectedAssembly && selectedAssembly.physicsObject == null)
            selectedAssembly = null;

        if(selectedNode || selectedAssembly)
            camType = CamType.ORBIT;

        if((camType == CamType.ORBIT) && (!selectedNode && !selectedAssembly) && !autoOrbit)
            camType = CamType.FREELOOK;

        // Orbit a selected object
        if(camType == CamType.ORBIT){
            Vector3 orbitTarget = Vector3.zero;
            if(selectedAssembly)
                orbitTarget = selectedAssembly.WorldPosition;
            else if(selectedNode)
                orbitTarget = selectedNode.worldPosition;

            targetPos = orbitTarget + (targetRot * Vector3.forward) * -camOrbitDist;

            camOrbitDist += camOrbitDist * -Input.GetAxis("Mouse ScrollWheel");

            if(Input.GetKeyDown(KeyCode.Space)){
                selectedAssembly = null;
                selectedNode = null;
                camType = CamType.FREELOOK;
            }

            depthOfField.focalPoint = Mathf.Lerp(depthOfField.focalPoint, Vector3.Distance(Camera.main.transform.position, orbitTarget), Time.deltaTime * 5f);
            depthOfField.focalSize = Vector3.Distance(Camera.main.transform.position, orbitTarget) * 0.1f;
        }
        // Toggle camera lock.
        else if(Input.GetKeyDown(KeyCode.Space)){
            if(camType == CamType.FREELOOK)
                camType = CamType.LOCKED;
            else if(camType == CamType.LOCKED)
                camType = CamType.FREELOOK;
        }

        // Rotate selected assembly
        if(selectedAssembly){
            selectedAssembly.physicsObject.transform.rotation *= Quaternion.Inverse(Quaternion.AngleAxis(WesInput.editHorizontalThrottle * 90f * Time.deltaTime, Quaternion.Inverse(selectedAssembly.physicsObject.transform.rotation) * Camera.main.transform.up));
            selectedAssembly.physicsObject.transform.rotation *= Quaternion.Inverse(Quaternion.AngleAxis(WesInput.editVerticalThrottle * 90f * Time.deltaTime, Quaternion.Inverse(selectedAssembly.physicsObject.transform.rotation) * -Camera.main.transform.right));
        }



        // Pitch/yaw camera via mouse movement.
        if((camType == CamType.FREELOOK) || (Input.GetMouseButton(1) && ((camType == CamType.ORBIT) || (camType == CamType.LOCKED)))){
            targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * cameraRotateSpeed, -Vector3.right);
            targetRot *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * cameraRotateSpeed, Vector3.up);
        }

        // Translate camera with arrow keys/wasd.
        if(camType == CamType.FREELOOK){
            // Translate position with keyboard input.
            targetPos += WesInput.forwardThrottle * transform.forward * cameraMoveSpeed * Time.deltaTime;
            targetPos += WesInput.horizontalThrottle * transform.right * cameraMoveSpeed * Time.deltaTime;
            targetPos += WesInput.verticalThrottle * transform.up * cameraMoveSpeed * Time.deltaTime;
        }

        // Roll camera using Q and E
        targetRot *= Quaternion.AngleAxis(WesInput.rotationThrottle * -cameraRotateSpeed, Vector3.forward);

        hoveredNode = null;
        RaycastHit mouseRayHit = new RaycastHit();
        if(Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), out mouseRayHit)){
            for(int i = 0; i < Node.GetAll().Count; i++){
                Node currentNode = Node.GetAll()[i];
                if(currentNode.gameObject == mouseRayHit.collider.gameObject){
                    hoveredNode = currentNode;
                    hoveredNode.assembly.Health += 0.5f * Time.deltaTime;
                    break;
                }
            }
        }
            // If clicking on 'nothing', deselect all.
        else if(Input.GetMouseButtonDown(0) && !NodeEngineering.Inst.uiLockout){
            selectedNode = null;
            selectedAssembly = null;
        }

        // When clicking on a node...
        if(Input.GetMouseButtonDown(0) && hoveredNode && !NodeEngineering.Inst.uiLockout){
            // If nothing is selected currently...
            if(!selectedNode && !selectedAssembly){
                selectedAssembly = hoveredNode.assembly;
                Refocus(selectedAssembly);
            }
            // If a node is selected currently...
            else if(selectedNode){
                // If you click again on the selected node, select it's assembly.
                if(hoveredNode == selectedNode){
                    selectedAssembly = hoveredNode.assembly;
                    Refocus(selectedAssembly);
                    selectedNode = null;
                }
                // If clicking on a node in the same assembly, select that node.
                else if(hoveredNode.assembly == selectedNode.assembly){
                    selectedNode = hoveredNode;
                    Refocus(selectedNode);
                }
                // Otherwise it's a node in a different assembly... so select that one.
                else{
                    selectedAssembly = hoveredNode.assembly;
                    Refocus(selectedAssembly);
                    selectedNode = null;
                }
            }
            // If an assembly is selected...
            else{
                // If clicking on a node in the selected assembly, select that.
                if(hoveredNode.assembly == selectedAssembly){
                    selectedNode = hoveredNode;
                    Refocus(selectedNode);
                    selectedAssembly = null;
                }
                // Otherwise, select the hovered node's assembly.
                else
                    selectedAssembly = hoveredNode.assembly;
                    Refocus(selectedAssembly);
            }
        }

	} // End of Update().


    void Refocus(Vector3 pos){
        Vector3 vectorToPos = pos - transform.position;
        camOrbitDist = vectorToPos.magnitude;
        targetRot = Quaternion.LookRotation(vectorToPos, transform.up);
    }
    void Refocus(Node node){
        Refocus(node.worldPosition);   
    }
    void Refocus(Assembly assembly){
        if(!assembly || !assembly.physicsObject)
            return;

        Refocus(assembly.physicsObject.transform.position);   
    }// End of Refocus().


    void OnGUI(){

        // Assembly health/name/etc. readouts.
        if(showAssemReadouts){
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
        Rect controlsRect = new Rect(25, 300f, 200, guiHeight);

        if(GameManager.Inst.showControls){

            showAssemReadouts = GUI.Toggle(controlsRect, showAssemReadouts, "Show Assembly Info");
            controlsRect.y += guiHeight;

            bool camLocked = camType == CamType.LOCKED;
            if(camType != CamType.ORBIT){
                camLocked = GUI.Toggle(controlsRect, camLocked, "Camera Locked");
        
                if(camLocked)
                    camType = CamType.LOCKED;
                else if(!camLocked)
                    camType = CamType.FREELOOK;
            }
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
 