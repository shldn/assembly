﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum CameraMode {
	STATIC, // Camera will stay in one place.
	USER_ORBIT, // Camera will orbit and zoom based on user mouse input.
	USER_FREECAM, // Camera can be moved around freely with WASDQE and mouse.
	SIMPLE_DEMO, // Camera will slowly pan around world origin while zooming in and out.
	SMART_DEMO, // Camera will move the same as SIMPLE_DEMO but also rotate to focus on interesting things in the environment.
	NEURO_SCALE // Camera control is relegated to the NeuroScaleDemo script.
} // End of CameraMode.

// Quaternion-based free camera.

public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;

    // How fast the camera orbits under its own power.
    public float autoOrbitSpeed = 1f;
    // The 'fluidity' of the camera control. Low values -> snappy movement, higher values -> more 'sweeping' movements.
    public float smoothTime = 0.5f;

    // Orbit = pan/tilt x/y of the camera around the centerPos.
    public float mouseSensitivity = 1f;
    public Quaternion targetOrbitQ;
    Quaternion orbitQ = Quaternion.identity;
    Quaternion orbitVelQ = Quaternion.identity; // for smoothdamp

    // How high or low the camera can be tilted.
    public float maxTilt = -80f;
    public float minTilt = 80f;

    // Radius = distance from the center position. "Orbit distance"
    public float maxRadius = 100f;
    public float minRadius = 1f;
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
    public Node selectedNode = null;

    public Assembly selectedAssembly = null;
	public Assembly hoveredPhysAssembly = null;

	public bool galleryCam = true;
	bool galleryCamInterrupted = false; // If a client goes into orbit mode, this ensures we return to gallery mode when that player releases orbit control.

	public Assembly assemblyOfInterest = null;
	float assemblyOfInterestStaleness = 0f;

	Vector2 targetPanTilt = new Vector2(0f, 0f);
	Vector2 panTiltVel = Vector2.zero;


	float blendToNeuroscale = 0f;
	float blendToNeuroscaleVel = 0f;
	bool clientOrbitControl = false;

	public bool lockHorizon = false;


	// centered cam lazy center
	Vector3 lazyCenter = Vector3.zero;
	Vector3 lazyCenterVel = Vector3.zero;



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
        targetOrbitQ = Quaternion.Euler(300f, (minTilt + maxTilt) * 0.5f, 0f);
        orbitQ = targetOrbitQ;

		transform.eulerAngles = new Vector3(0f, 120f, 0f);

		galleryCam = Environment.Inst && Environment.Inst.isActiveAndEnabled && !PersistentGameManager.IsClient;
	} // End of Start().
	

	void LateUpdate(){

		// Blend between normal cam and neuroscale mode.
		blendToNeuroscale = Mathf.SmoothDamp(blendToNeuroscale, (NeuroScaleDemo.Inst && NeuroScaleDemo.Inst.isActive && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0))? 1f : 0f, ref blendToNeuroscaleVel, 1f);


		if(Input.GetKeyDown(KeyCode.C) && !ConsoleScript.active)
			galleryCam = !galleryCam;

		// If a player goes into orbit mode while in gallery mode, interrupt it.
		if((CaptureNet_Manager.Inst.orbitPlayers.Count > 0) && galleryCam){
			galleryCam = false;
			galleryCamInterrupted = true;
		}

		// When no players are orbiting anymore (and we previously interrupted the gallery cam), re-instate it.
		if((CaptureNet_Manager.Inst.orbitPlayers.Count == 0) && galleryCamInterrupted){
			galleryCam = true;
			galleryCamInterrupted = false;
		}

		// Gallery cam
		if(galleryCam){
			float radiusPulseTime = 200f;
			float maxPulseRadius = 800f;

			float elevationPulseTime = 75f;
			float maxPulseElevation = 45f;
			targetRadius = (Mathf.Cos((Time.time / radiusPulseTime) * (Mathf.PI * 2f)) * 0.5f) * maxPulseRadius;
			targetOrbitQ = Quaternion.Euler(targetOrbitQ.eulerAngles.x, (Mathf.Sin((Time.time / elevationPulseTime) * (Mathf.PI * 2f)) * 0.5f) * maxPulseElevation, 0f);
		}


        // Smooth time is slowed down if cursor is locked ("cinematic mode")
        float effectiveSmoothTime = smoothTime;
        if(!PersistentGameManager.IsClient){
			if(PersistentGameManager.Inst.CursorLock && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0)){
				effectiveSmoothTime *= 5f;

				// Auto-orbit run
				targetOrbitQ *= Quaternion.AngleAxis(autoOrbitSpeed * Time.deltaTime, Vector3.up);
			}
		}

		if(AssemblyRadar.Inst && (CaptureEditorManager.capturedObj == null))
			effectiveSmoothTime *= 5f;

		if(Amalgam.allAmalgams.Count > 0)
			effectiveSmoothTime /= 5f;


        // Determine orbit target, if any.
        // Jellyfish grotto client
        if(PersistentGameManager.IsClient && JellyfishGameManager.Inst && (Jellyfish.all.Count > 0f)){
            selectedJellyfish = Jellyfish.all[0];
            center = Jellyfish.all[0].transform.position;
        }
        else if(PersistentGameManager.IsClient && NodeController.Inst && ClientTest.Inst)
            KeepAssembliesInView();
        // Grotto
        else if(selectedJellyfish)
            center = selectedJellyfish.transform.position;
        // Assembly soup
        else if(selectedNode)
            center = selectedNode.Position;
        else if(selectedAssembly)
			center = selectedAssembly.Position;
		
		// Show all assemblies constrained to the screen.
        else if(Assembly.getAll.Count > 0){
			lazyCenter = Vector3.zero;

			if(!PersistentGameManager.IsClient && !galleryCam && (!Environment.Inst || !Environment.Inst.isActiveAndEnabled) && (Assembly.getAll.Count > 0)){
				// Center on average assembly position
				for(int i = 0; i < Assembly.getAll.Count; i++)
					lazyCenter += Assembly.getAll[i].Position;
				lazyCenter /= Assembly.getAll.Count;
			}

			center = Vector3.SmoothDamp(center, lazyCenter, ref lazyCenterVel, 3f);
		}

		if(NeuroScaleDemo.Inst && NeuroScaleDemo.Inst.TargetNode)
			center = Vector3.Lerp(center, NeuroScaleDemo.Inst.TargetNode.Position, blendToNeuroscale);


		if((Assembly.getAll.Count > 0f) && Environment.Inst && Environment.Inst.isActiveAndEnabled)
			targetOrbitQ *= Quaternion.AngleAxis(1f * Time.deltaTime, Vector3.up);


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
                targetRadius -= targetRadius * (pinchDist - lastPinchDist) * 0.005f * radiusSensitivity ;

            lastPinchDist = pinchDist;
        }else{
            lastPinchDist = -1f;

            if(Input.touchCount == 0)
                pinchRelease = true;
        }

        // Mouse zoom
		if(!galleryCam)
	        targetRadius += targetRadius * -Input.GetAxis("Mouse ScrollWheel") * radiusSensitivity ;

		if(Input.GetKey(KeyCode.Comma) && !ConsoleScript.active)
	        targetRadius += targetRadius * -Time.deltaTime * radiusSensitivity ;
		if(Input.GetKey(KeyCode.Period) && !ConsoleScript.active)
	        targetRadius += targetRadius * Time.deltaTime * radiusSensitivity ;

        // Mouse/touch orbit.
        if(!galleryCam && ((PersistentGameManager.IsClient && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && (Input.touchCount < 2) && !NodeEngineering.Inst.uiLockout) || Screen.lockCursor || (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1) && pinchRelease))){
			targetOrbitQ *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * mouseSensitivity, Vector3.up);
			targetOrbitQ *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * mouseSensitivity, Vector3.right);
        }

		
            

        // Increment values, apply camera position/rotation.
        targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        radius = Mathf.SmoothDamp(radius, targetRadius, ref radiusVel, effectiveSmoothTime);

		if(lockHorizon)
			targetOrbitQ = Quaternion.Euler(Mathf.Clamp(Mathf.DeltaAngle(0f, targetOrbitQ.eulerAngles.x), -maxTilt, -minTilt), targetOrbitQ.eulerAngles.y, 0f);

		orbitQ = Quaternion.Lerp(orbitQ, targetOrbitQ, Time.deltaTime / effectiveSmoothTime);

        centerOffset = Vector3.SmoothDamp(centerOffset, Vector3.zero, ref centerOffsetVel, effectiveSmoothTime);
        transform.position = center + centerOffset + (orbitQ * (Vector3.forward * (NeuroScaleDemo.Inst? Mathf.Lerp(radius, NeuroScaleDemo.Inst.CamRadius, blendToNeuroscale) : radius)));


		
		Quaternion orbitCamRot = orbitQ * Quaternion.AngleAxis(180f, Vector3.up);


		if(!galleryCam)
	        transform.rotation = orbitCamRot;
		else{
			Quaternion targetRotation = orbitCamRot;
			targetRotation = Quaternion.Lerp(targetRotation, orbitQ, blendToNeuroscale);

			if(!assemblyOfInterest){
				assemblyOfInterestStaleness = 0f;

				// Try to find an assembly that is mating.
				foreach(Assembly someAssem in Assembly.getAll){
					float dist = Vector3.Distance(transform.position, someAssem.Position);
					if((dist < 60f) && someAssem.MatingWith){
						assemblyOfInterest = someAssem;
						break;
					}
				}

				// If no mating, look for one that is active.
				if(!assemblyOfInterest){
					foreach(Assembly someAssem in Assembly.getAll){
						float dist = Vector3.Distance(transform.position, someAssem.Position);
						if((dist < 60f) && (someAssem.energy > (someAssem.NodeDict.Values.Count * 0.5f))){
							assemblyOfInterest = someAssem;
							break;
						}
					}
				}
			}
			
			if(assemblyOfInterest){
				if(!assemblyOfInterest.MatingWith)
					assemblyOfInterestStaleness += Time.deltaTime;

				targetRotation = Quaternion.LookRotation(assemblyOfInterest.Position - transform.position, Camera.main.transform.up);
				float dist = Vector3.Distance(transform.position, assemblyOfInterest.Position);
				if((dist > 80f) || (!assemblyOfInterest.MatingWith && (assemblyOfInterestStaleness > 10f)))
					assemblyOfInterest = null;
			}

			targetPanTilt = targetRotation.eulerAngles;
			Vector3 tempEulers = transform.eulerAngles;
			float panTiltSmoothTime = 2f;
			tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetPanTilt.x, ref panTiltVel.x, panTiltSmoothTime);
			tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetPanTilt.y, ref panTiltVel.y, panTiltSmoothTime);
			Quaternion transformEulers = Quaternion.Euler(tempEulers);

			transform.rotation = Quaternion.Lerp(transformEulers, orbitCamRot, blendToNeuroscale);
		}


        // Object selection ------------------------------------------- //
        if(!NodeEngineering.Inst || !NodeEngineering.Inst.uiLockout){
            Ray selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit selectRay = new RaycastHit();

            // We have an assembly selected -- try to select a node within it.

			hoveredPhysAssembly = null;
			if(Physics.Raycast(selectionRay, out selectRay, 1000f)){
				GameObject hitObject = selectRay.transform.gameObject;
                PhysNode node = hitObject.GetComponent<PhysNode>();
                if(node != null){
                    foreach (Assembly a in Assembly.getAll){
                        if (a.Id == node.nodeViewer.AssemblyProperties.id){
                            hoveredPhysAssembly = a;
                            if (Input.GetMouseButton(0))
                                selectedAssembly = hoveredPhysAssembly;
                            break;
                        }
                    }
                }
			}
        }
		
        if(Input.GetKeyDown(KeyCode.Return) && !ConsoleScript.active){
            if(selectedNode){
                selectedNode = null;
            // Deselect all--return to main orbit.
            }else if(selectedAssembly){
                selectedAssembly = null;
                targetRadius = maxRadius;
            }
        }
	} // End of Update().

    public void KeepAssembliesInView()
    {
        List<Vector3> pts = new List<Vector3>();
        foreach (Assembly someAssembly in Assembly.getAll)
            if(!someAssembly.cull)
                pts.Add(someAssembly.Position);
        float sphereRadius = 0f;
        MathHelper.GetBoundingSphere(pts, out center, out sphereRadius);
        targetRadius = (sphereRadius + 10f) / Mathf.Tan(Camera.main.fieldOfView * 0.4f * Mathf.Deg2Rad);
    } // End of KeepAssembliesInView().


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


	void OnDrawGizmos(){

	} // End of OnDrawGizmos().

} // End of CameraOrbit.







// Euler-based horizon-locked camera (old).

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum CameraMode {
	STATIC, // Camera will stay in one place.
	USER_ORBIT, // Camera will orbit and zoom based on user mouse input.
	USER_FREECAM, // Camera can be moved around freely with WASDQE and mouse.
	SIMPLE_DEMO, // Camera will slowly pan around world origin while zooming in and out.
	SMART_DEMO, // Camera will move the same as SIMPLE_DEMO but also rotate to focus on interesting things in the environment.
	NEURO_SCALE // Camera control is relegated to the NeuroScaleDemo script.
} // End of CameraMode.


public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;

    // How fast the camera orbits under its own power.
    public float autoOrbitSpeed = 1f;
    // The 'fluidity' of the camera control. Low values -> snappy movement, higher values -> more 'sweeping' movements.
    public float smoothTime = 0.5f;

    // Orbit = pan/tilt x/y of the camera around the centerPos.
    public float mouseSensitivity = 1f;
    public Vector2 targetOrbit = Vector2.zero;
    Vector2 orbit = Vector2.zero;
    Vector2 orbitVel = Vector2.zero; // for smoothdamp

    // How high or low the camera can be tilted.
    public float minTilt = 80f;
    public float maxTilt = -80f;

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
    public Node selectedNode = null;

    public Assembly selectedAssembly = null;
	public Assembly hoveredPhysAssembly = null;

	public bool galleryCam = true;
	bool galleryCamInterrupted = false; // If a client goes into orbit mode, this ensures we return to gallery mode when that player releases orbit control.

	public Assembly assemblyOfInterest = null;
	float assemblyOfInterestStaleness = 0f;

	Vector2 targetPanTilt = new Vector2(0f, 0f);
	Vector2 panTiltVel = Vector2.zero;


	float blendToNeuroscale = 0f;
	float blendToNeuroscaleVel = 0f;
	bool clientOrbitControl = false;


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
        targetOrbit.x = 300f;
        targetOrbit.y = (minTilt + maxTilt) * 0.5f;
        orbit = targetOrbit;

		transform.eulerAngles = new Vector3(0f, 120f, 0f);

		galleryCam = Environment.Inst && Environment.Inst.isActiveAndEnabled && !PersistentGameManager.IsClient;
	} // End of Start().
	

	void LateUpdate(){

		// Blend between normal cam and neuroscale mode.
		blendToNeuroscale = Mathf.SmoothDamp(blendToNeuroscale, (NeuroScaleDemo.Inst.isActive && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0))? 1f : 0f, ref blendToNeuroscaleVel, 1f);


		if(Input.GetKeyDown(KeyCode.C) && !ConsoleScript.active)
			galleryCam = !galleryCam;

		// If a player goes into orbit mode while in gallery mode, interrupt it.
		if((CaptureNet_Manager.Inst.orbitPlayers.Count > 0) && galleryCam){
			galleryCam = false;
			galleryCamInterrupted = true;
		}

		// When no players are orbiting anymore (and we previously interrupted the gallery cam), re-instate it.
		if((CaptureNet_Manager.Inst.orbitPlayers.Count == 0) && galleryCamInterrupted){
			galleryCam = true;
			galleryCamInterrupted = false;
		}

		// Gallery cam
		if(galleryCam){
			float radiusPulseTime = 120f;
			float maxPulseRadius = 800f;

			float elevationPulseTime = 75f;
			float maxPulseElevation = 45f;
			targetRadius = (Mathf.Cos((Time.time / radiusPulseTime) * (Mathf.PI * 2f)) * 0.5f) * maxPulseRadius;
			targetOrbit.y = (Mathf.Sin((Time.time / elevationPulseTime) * (Mathf.PI * 2f)) * 0.5f) * maxPulseElevation;
		}


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
        else if (PersistentGameManager.IsClient && NodeController.Inst && ClientTest.Inst)
            KeepAssembliesInView();
        // Grotto
        else if(selectedJellyfish)
            center = selectedJellyfish.transform.position;
        // Assembly soup
        else if(selectedNode)
            center = selectedNode.Position;
        else if(selectedAssembly)
			center = selectedAssembly.Position;
		
		// Show all assemblies constrained to the screen.
        else{
			center = Vector3.zero;

			if(!galleryCam && (!Environment.Inst || !Environment.Inst.isActiveAndEnabled) && (Assembly.getAll.Count > 0)){
				// Center on average assembly position
				for(int i = 0; i < Assembly.getAll.Count; i++)
					center += Assembly.getAll[i].Position;
				center /= Assembly.getAll.Count;
			}
		}

		if(NeuroScaleDemo.Inst.TargetNode)
			center = Vector3.Lerp(center, NeuroScaleDemo.Inst.TargetNode.Position, blendToNeuroscale);


		if((Assembly.getAll.Count > 0f) && Environment.Inst && Environment.Inst.isActiveAndEnabled)
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
		if(!galleryCam)
	        targetRadius += targetRadius * -Input.GetAxis("Mouse ScrollWheel") * radiusSensitivity ;

		if(Input.GetKey(KeyCode.Comma) && !ConsoleScript.active)
	        targetRadius += targetRadius * -Time.deltaTime * radiusSensitivity ;
		if(Input.GetKey(KeyCode.Period) && !ConsoleScript.active)
	        targetRadius += targetRadius * Time.deltaTime * radiusSensitivity ;

        // Mouse/touch orbit.
        if(!galleryCam && ((PersistentGameManager.IsClient && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && (Input.touchCount < 2) && CaptureEditorManager.IsEditing && !NodeEngineering.Inst.uiLockout) || Screen.lockCursor || (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1) && pinchRelease))){
            targetOrbit.x += Input.GetAxis("Mouse X") * mouseSensitivity;
            targetOrbit.y += -Input.GetAxis("Mouse Y") * mouseSensitivity;
        }
            

        // Increment values, apply camera position/rotation.
        targetRadius = Mathf.Clamp(targetRadius, minRadius, maxRadius);
        radius = Mathf.SmoothDamp(radius, targetRadius, ref radiusVel, effectiveSmoothTime);

        targetOrbit.y = Mathf.Clamp(targetOrbit.y, minTilt, maxTilt);
        orbit.x = Mathf.SmoothDamp(orbit.x, targetOrbit.x, ref orbitVel.x, effectiveSmoothTime);
        orbit.y = Mathf.SmoothDamp(orbit.y, targetOrbit.y, ref orbitVel.y, effectiveSmoothTime);

        centerOffset = Vector3.SmoothDamp(centerOffset, Vector3.zero, ref centerOffsetVel, effectiveSmoothTime);
        Quaternion cameraRot = Quaternion.Euler(-orbit.y, orbit.x, 0f);
        transform.position = center + centerOffset + (cameraRot * (Vector3.forward * Mathf.Lerp(radius, NeuroScaleDemo.Inst.CamRadius, blendToNeuroscale)));

		Quaternion orbitCamRot = cameraRot * Quaternion.AngleAxis(180f, Vector3.up);


		if(!galleryCam)
	        transform.rotation = orbitCamRot;
		else{
			Quaternion targetRotation = orbitCamRot;
			targetRotation = Quaternion.Lerp(targetRotation, cameraRot, blendToNeuroscale);

			if(!assemblyOfInterest){
				assemblyOfInterestStaleness = 0f;

				// Try to find an assembly that is mating.
				foreach(Assembly someAssem in Assembly.getAll){
					float dist = Vector3.Distance(transform.position, someAssem.Position);
					if((dist < 60f) && someAssem.matingWith){
						assemblyOfInterest = someAssem;
						break;
					}
				}

				// If no mating, look for one that is active.
				if(!assemblyOfInterest){
					foreach(Assembly someAssem in Assembly.getAll){
						float dist = Vector3.Distance(transform.position, someAssem.Position);
						if((dist < 60f) && (someAssem.energy > (someAssem.NodeDict.Values.Count * 0.5f))){
							assemblyOfInterest = someAssem;
							break;
						}
					}
				}
			}
			
			if(assemblyOfInterest){
				if(!assemblyOfInterest.matingWith)
					assemblyOfInterestStaleness += Time.deltaTime;

				targetRotation = Quaternion.LookRotation(assemblyOfInterest.Position - transform.position);
				float dist = Vector3.Distance(transform.position, assemblyOfInterest.Position);
				if((dist > 80f) || (!assemblyOfInterest.matingWith && (assemblyOfInterestStaleness > 10f)))
					assemblyOfInterest = null;
			}

			targetPanTilt = targetRotation.eulerAngles;
			Vector3 tempEulers = transform.eulerAngles;
			float panTiltSmoothTime = 2f;
			tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetPanTilt.x, ref panTiltVel.x, panTiltSmoothTime);
			tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetPanTilt.y, ref panTiltVel.y, panTiltSmoothTime);
			Quaternion transformEulers = Quaternion.Euler(tempEulers);

			transform.rotation = Quaternion.Lerp(transformEulers, orbitCamRot, blendToNeuroscale);
		}


        // Object selection ------------------------------------------- //
        if(!NodeEngineering.Inst || !NodeEngineering.Inst.uiLockout){
            Ray selectionRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit selectRay = new RaycastHit();

            // We have an assembly selected -- try to select a node within it.

			hoveredPhysAssembly = null;
			if(Physics.Raycast(selectionRay, out selectRay, 1000f)){
				GameObject hitObject = selectRay.transform.gameObject;
				foreach(Node somePhysNode in Node.getAll){
					if(hitObject.transform == somePhysNode.cubeTransform){
						hoveredPhysAssembly = somePhysNode.PhysAssembly;
						if(Input.GetMouseButton(0))
							selectedAssembly = hoveredPhysAssembly;
						break;
					}
				}
			}
        }
		
        if(Input.GetKeyDown(KeyCode.Return) && !ConsoleScript.active){
            if(selectedNode){
                selectedNode = null;
            // Deselect all--return to main orbit.
            }else if(selectedAssembly){
                selectedAssembly = null;
                targetRadius = maxRadius;
            }
        }

	} // End of Update().

    public void KeepAssembliesInView()
    {
        List<Vector3> pts = new List<Vector3>();
        foreach (Assembly someAssembly in Assembly.getAll)
            if(!someAssembly.cull)
                pts.Add(someAssembly.Position);
        float sphereRadius = 0f;
        MathHelper.GetBoundingSphere(pts, out center, out sphereRadius);
        targetRadius = (sphereRadius + 10f) / Mathf.Tan(Camera.main.fieldOfView * 0.4f * Mathf.Deg2Rad);
    } // End of KeepAssembliesInView().


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


	void OnDrawGizmos(){

	} // End of OnDrawGizmos().

} // End of CameraOrbit.
*/