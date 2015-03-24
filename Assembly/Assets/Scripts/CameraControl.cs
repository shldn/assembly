using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;

    // How fast the camera orbits under its own power.
    public float autoOrbitSpeed = 1f;
    // The 'fluidity' of the camera control. Low values -> snappy movement, higher values -> more 'sweeping' movements.
    public float smoothTime = 0.5f;

    // Orbit = pan/tilt x/y of the camera around the centerPos.
    public float orbitSensitivity = 1f;
    Vector2 orbit = Vector2.zero;
    public Vector2 targetOrbit = Vector2.zero;
    Vector2 orbitVel = Vector2.zero;
    // How high or low the camera can be tilted.
    public float minTilt = 0f;
    public float maxTilt = 80f;

    // Radius = distance from the center position. "Orbit distance"
    public float minRadius = 1f;
    public float maxRadius = 100f;
    [HideInInspector] public float radius = 6f;
    [HideInInspector] public float targetRadius = 6f;
    float radiusVel = 0f;
    public float radiusSensitivity = 1f;

    // Camera will be zeroed on centerPos + centerOffset. CenterOffset will tend to Vector3.zero.
    [HideInInspector] public Vector3 center = Vector3.zero;
    [HideInInspector] public Vector3 centerOffset = Vector3.zero;
    Vector3 centerOffsetVel = Vector3.zero;

    float lastPinchDist = -1f;
    bool pinchRelease = true;


    // Object selections
    public Jellyfish selectedJellyfish = null;
    public Assembly selectedAssembly = null;
    public Node selectedNode = null;

    public PhysAssembly selectedPhyAssembly = null;
	public PhysAssembly hoveredPhysAssembly = null;


    void Awake(){
        Inst = this;
    } // End of Awake().


	void Start(){
        // Set up camera rendering effects
		if(Application.loadedLevelName != "SoupPhysics"){
			RenderSettings.fog = true;
			RenderSettings.fogColor = Camera.main.backgroundColor;
		}

        // Camera initial values
        targetRadius = maxRadius;
        radius = targetRadius;
        targetOrbit.x = Random.Range(0f, 360f);
        targetOrbit.y = (minTilt + maxTilt) * 0.5f;
        orbit = targetOrbit;
	} // End of Start().
	

	void FixedUpdate(){
        // Smooth time is slowed down if cursor is locked ("cinematic mode")
        float effectiveSmoothTime = smoothTime;
        if(!PersistentGameManager.IsClient){
			if(PersistentGameManager.Inst.CursorLock && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0)){
				effectiveSmoothTime *= 5f;

				// Auto-orbit run
				targetOrbit.x -= autoOrbitSpeed * Time.deltaTime;
			}
		}

        // Determine orbit target, if any.
        // Jellyfish grotto client
        if(PersistentGameManager.IsClient && JellyfishGameManager.Inst && (Jellyfish.all.Count > 0f)){
            selectedJellyfish = Jellyfish.all[0];
            center = Jellyfish.all[0].transform.position;
        }
        // Assembly client
        else if(PersistentGameManager.IsClient && (Assembly.GetAll().Count > 0) && Assembly.GetAll()[0]){
            selectedAssembly = Assembly.GetAll()[0];
            if(selectedNode)
                center = selectedNode.gameObject.transform.position;
            else
                center = selectedAssembly.WorldPosition;
        }
        // Grotto
        else if(selectedJellyfish)
            center = selectedJellyfish.transform.position;
        // Assembly soup
        else if(selectedNode)
            center = selectedNode.gameObject.transform.position;
        else if(selectedAssembly)
            center = selectedAssembly.WorldPosition;
		else if(selectedPhyAssembly)
			center = selectedPhyAssembly.WorldPosition;
		
        else
            center = Vector3.zero;

		if(PhysAssembly.getAll.Count > 0f)
			targetOrbit.x -= 1f * Time.deltaTime;


        // General camera controls. ----------------------------------------------- //
        // Touch-screen pinch-zoom
        if(Input.touchCount >= 2){
            pinchRelease = false;

            Vector2 touch0, touch1;
            float pinchDist;
            touch0 = Input.GetTouch(0).position;
            touch1 = Input.GetTouch(1).position;
 
            pinchDist = Vector2.Distance(touch0, touch1);

            if(lastPinchDist != -1)
                targetRadius -= (pinchDist - lastPinchDist) * 0.05f * radiusSensitivity ;

            lastPinchDist = pinchDist;
        }else{
            lastPinchDist = -1f;

            if(Input.touchCount == 0)
                pinchRelease = true;
        }

        // Mouse zoom
        targetRadius += targetRadius * -Input.GetAxis("Mouse ScrollWheel") * radiusSensitivity ;

		if(Input.GetKey(KeyCode.Comma))
	        targetRadius += targetRadius * -Time.deltaTime * radiusSensitivity ;
		if(Input.GetKey(KeyCode.Period))
	        targetRadius += targetRadius * Time.deltaTime * radiusSensitivity ;

        // Mouse/touch orbit.
        if((PersistentGameManager.IsClient && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && (Input.touchCount < 2) && CaptureEditorManager.IsEditing && !NodeEngineering.Inst.uiLockout && !AssemblyEditor.Inst.uiLockout) || Screen.lockCursor || (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1) && pinchRelease)){
            targetOrbit.x += Input.GetAxis("Mouse X") * orbitSensitivity;
            targetOrbit.y += -Input.GetAxis("Mouse Y") * orbitSensitivity;
        }
            

        // Increment values, apply camera position/rotation.
        targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        radius = Mathf.SmoothDamp(radius, targetRadius, ref radiusVel, effectiveSmoothTime);

        targetOrbit.y = Mathf.Clamp(targetOrbit.y, minTilt, maxTilt);
        orbit.x = Mathf.SmoothDamp(orbit.x, targetOrbit.x, ref orbitVel.x, effectiveSmoothTime);
        orbit.y = Mathf.SmoothDamp(orbit.y, targetOrbit.y, ref orbitVel.y, effectiveSmoothTime);

        centerOffset = Vector3.SmoothDamp(centerOffset, Vector3.zero, ref centerOffsetVel, effectiveSmoothTime);
        Quaternion cameraRot = Quaternion.Euler(-orbit.y, orbit.x, 0f);
        transform.position = center + centerOffset + (cameraRot * (Vector3.forward * radius));
        transform.rotation = cameraRot * Quaternion.AngleAxis(180f, Vector3.up);


        // Object selection ------------------------------------------- //
        if(!NodeEngineering.Inst || !NodeEngineering.Inst.uiLockout){
            Ray selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit selectRay = new RaycastHit();

            // We have an assembly selected -- try to select a node within it.
			bool selectedSomething = false;
			if(Input.GetMouseButtonDown(0)){
				if(selectedAssembly){
					int objectsMask = 1 << LayerMask.NameToLayer("Nodes");
					if(Physics.Raycast(selectionRay, out selectRay, 1000f, objectsMask)){
						Node rayHitNode = null;
						for(int i = 0; i < Node.GetAll().Count; i++){
							Node curNode = Node.GetAll()[i];
							if((selectRay.transform.gameObject == curNode.gameObject) && (curNode.assembly == selectedAssembly)){
								rayHitNode = curNode;
								break;
							}
						}
						if(rayHitNode){
							print("Hit node!");
							if(rayHitNode.assembly == selectedAssembly){
								selectedNode = rayHitNode;
								centerOffset = center - selectedNode.worldPosition;
								selectedSomething = true;
							}
						}
					}
				}
            
				// No selected assembly, so we're looking for those.
				if(!selectedSomething){
					int objectsMask = 1 << LayerMask.NameToLayer("Assemblies");
					if(Physics.Raycast(selectionRay, out selectRay, 1000f, objectsMask)){
						Assembly rayHitAssembly = null;
						for(int i = 0; i < Assembly.GetAll().Count; i++){
							Assembly curAssem = Assembly.GetAll()[i];
							if((selectRay.transform.gameObject == curAssem.physicsObject) && (curAssem != selectedAssembly)){
								rayHitAssembly = curAssem;
							}
						}
						if(rayHitAssembly && (rayHitAssembly != selectedAssembly)){
							// Select new assembly.
							selectedAssembly = rayHitAssembly;
							centerOffset = center - selectedAssembly.WorldPosition;
							targetRadius = 20f;
							selectedSomething = true;
						}
					}
				}
			}

			hoveredPhysAssembly = null;
			if(!selectedSomething){
				if(Physics.Raycast(selectionRay, out selectRay, 1000f)){
					GameObject hitObject = selectRay.transform.gameObject;
					foreach(PhysNode somePhysNode in PhysNode.getAll){
						if(hitObject.transform == somePhysNode.cubeTransform){
							hoveredPhysAssembly = somePhysNode.PhysAssembly;
							if(Input.GetMouseButton(0))
								selectedPhyAssembly = hoveredPhysAssembly;
							break;
						}
					}
				}
			}
        }

        if(Input.GetKeyDown(KeyCode.Return)){
            if(selectedNode){
                selectedNode = null;
                centerOffset = center - selectedAssembly.WorldPosition;
            // Deselect all--return to main orbit.
            }else if(selectedAssembly){
                selectedAssembly = null;
                centerOffset = center - Vector3.zero;
                targetRadius = maxRadius;
            }
        }

	} // End of Update().


    void OnGUI(){
        if(Input.GetKey(KeyCode.F2)){
            GUILayout.BeginVertical(GUILayout.Width(Screen.width * 0.2f));

            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.label.fontSize = 14;
            GUILayout.Label((1f / Time.smoothDeltaTime).ToString("F1"));

            foreach(Camera c in Camera.allCameras){
                List<MonoBehaviour> components = new List<MonoBehaviour>(c.gameObject.GetComponents<MonoBehaviour>());
                for(int i = 0; i < components.Count; i++){
                    if(components[i] == this)
                        continue;

                    components[i].enabled = GUILayout.Toggle(components[i].enabled, components[i].ToString());
                }
            }

            GUILayout.EndVertical();
        }
    } // End of OnGUI().


    void OnDrawGizmosSelected(){
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(Vector3.zero, minRadius);
        Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
        Gizmos.DrawSphere(Vector3.zero, maxRadius);
    } // End of OnDrawGizmos().

} // End of CameraOrbit.
