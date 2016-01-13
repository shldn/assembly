﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VR;


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
    public Quaternion targetOrbitQ;
	Vector2 mouseOrbitStack = Vector2.zero; // Keeps track of mouse input... allows for greater-than-180 target rotation.
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
	float mouseRadiusStack = 0f;
    float radiusVel = 0f;
    public float radiusSensitivity = 1f;

    // Camera will be zeroed on centerPos + centerOffset. CenterOffset will tend to Vector3.zero.
    [HideInInspector] public Vector3 center = Vector3.zero;
    [HideInInspector] public Vector3 centerOffset = Vector3.zero;
    Vector3 centerOffsetVel = Vector3.zero;

    float lastPinchDist = -1f;
    bool pinchRelease = true;

    // Object selections
	public CaptureObject selectedCaptureObj = null;

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

	public enum CameraType {
		NONE, // No camera control.
		GALLERY_AUTO, // Automatic cinematic camera that zooms in/out, focusing on points of interest.
		USER_ORBIT, // Fast and responsive user-controlled orbit
		USER_ORBIT_CINEMATIC, // Slow, dramatic user-controlled orbit.
		USER_FLY, // Fast and responsive user-controlled fly-around.
		ASSEMBLIES_SSBVIEW // "Super Smash Brothers" view, keeping all assemblies on the screen.
	}
	public CameraType camType = CameraType.NONE;
	// If a client goes into orbit mode, this ensures we return to gallery mode when that player releases orbit control.
	bool galleryCamInterrupted = false;

	public enum OriginType {
		WORLD, // 0, 0, 0 in world-space.
		ASSEMBLIES_CENTER,
		AMALGAMS_CENTER,
		JELLYFISH_CENTER,
		ASSEMBLIES_SSBVIEW, // "Super Smash Brothers" view, keeping all assemblies on the screen.
		SELECTED_TRANSFORM
	}
	public OriginType originType = OriginType.WORLD;


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

		if(Environment.Inst && Environment.Inst.isActiveAndEnabled && !PersistentGameManager.IsClient)
			camType = CameraType.GALLERY_AUTO;

		if(VRDevice.isPresent){
			Rigidbody camRB = Camera.main.gameObject.AddComponent<Rigidbody>();
			camRB.interpolation = RigidbodyInterpolation.Interpolate;
			camRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			camRB.useGravity = false;
			camRB.isKinematic = true;
		}
	} // End of Start().
	

	void LateUpdate(){
		// We surrender all camera control to Unity if a VR device is being used... for now.
		if(VRDevice.isPresent)
			return;

		// Blend between normal cam and neuroscale mode.
		blendToNeuroscale = Mathf.SmoothDamp(blendToNeuroscale, (NeuroScaleDemo.Inst && NeuroScaleDemo.Inst.isActive && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0))? 1f : 0f, ref blendToNeuroscaleVel, 1f);

		// Toggle gallery camera mode
		if(Input.GetKeyDown(KeyCode.C) && !ConsoleScript.active){
			if(camType == CameraType.GALLERY_AUTO)
				camType = CameraType.USER_ORBIT;
			else if(camType == CameraType.USER_ORBIT)
				camType = CameraType.GALLERY_AUTO;
		}

		// If a player goes into orbit mode while in gallery mode, interrupt it.
		if(CaptureNet_Manager.Inst != null && (CaptureNet_Manager.Inst.orbitPlayers.Count > 0) && (camType == CameraType.GALLERY_AUTO)){
			camType = CameraType.USER_ORBIT;
			galleryCamInterrupted = true;
		}

		// When no players are orbiting anymore (and we previously interrupted the gallery cam), re-instate it.
		if(CaptureNet_Manager.Inst != null && (CaptureNet_Manager.Inst.orbitPlayers.Count == 0) && galleryCamInterrupted){
			camType = CameraType.GALLERY_AUTO;
			galleryCamInterrupted = false;
		}

		// Gallery cam
		if(camType == CameraType.GALLERY_AUTO){
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

				// Auto-orbit in this 'manual cinematic mode'
				targetOrbitQ *= Quaternion.AngleAxis(autoOrbitSpeed * Time.deltaTime, Vector3.up);
			}
		}

		if(AssemblyRadar.Inst && (CaptureEditorManager.capturedObj == null))
			effectiveSmoothTime *= 5f;

		if(Amalgam.allAmalgams.Count > 0)
			effectiveSmoothTime /= 5f;

        // Determine orbit target, if any.
        // Jellyfish grotto
        if(originType == OriginType.JELLYFISH_CENTER){
            selectedCaptureObj = Jellyfish.all[0];
            center = Jellyfish.all[0].transform.position;
        }
        else if(originType == OriginType.ASSEMBLIES_SSBVIEW)
            KeepAssembliesInView();
				
        else if(originType == OriginType.ASSEMBLIES_CENTER){
			lazyCenter = Vector3.zero;

			if(Assembly.getAll.Count > 0){
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


        // General user-orbit camera controls. --------------------------------------------------------- //
		if((camType == CameraType.USER_ORBIT) || (camType == CameraType.USER_ORBIT_CINEMATIC)) {
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
			mouseRadiusStack += radius * -Input.GetAxis("Mouse ScrollWheel") * radiusSensitivity;
			radius += mouseRadiusStack;
			targetRadius += mouseRadiusStack;
			mouseRadiusStack = 0f;

			// Keyboard zoom
			if(Input.GetKey(KeyCode.Comma) && !ConsoleScript.active)
				targetRadius += targetRadius * -Time.deltaTime * radiusSensitivity ;
			if(Input.GetKey(KeyCode.Period) && !ConsoleScript.active)
				targetRadius += targetRadius * Time.deltaTime * radiusSensitivity ;

			// Mouse/touch orbit.
			if((PersistentGameManager.IsClient && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && (Input.touchCount < 2) || (Cursor.lockState == CursorLockMode.Locked) || (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1) && pinchRelease))){
				mouseOrbitStack.x += Input.GetAxis("Mouse X") * mouseSensitivity;
				mouseOrbitStack.y += Input.GetAxis("Mouse Y") * mouseSensitivity;

				targetOrbitQ *= Quaternion.AngleAxis(mouseOrbitStack.x, Vector3.up);
				targetOrbitQ *= Quaternion.AngleAxis(mouseOrbitStack.y, Vector3.right);
				orbitQ *= Quaternion.AngleAxis(mouseOrbitStack.x, Vector3.up);
				orbitQ *= Quaternion.AngleAxis(mouseOrbitStack.y, Vector3.right);

				mouseOrbitStack = Vector2.zero;
			}
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

		// Gallery cam uses the "orbit" rotation and such to move the cam, but performs its own rotation stuff in here.
		if (camType == CameraType.GALLERY_AUTO) {
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

				targetRotation = Quaternion.LookRotation(assemblyOfInterest.Position - transform.position, transform.up);
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
		// Non-gallery cam just locks the camera rotation to whatever the current orbit is.
		} else
	        transform.rotation = orbitCamRot;
		
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

} // End of CameraOrbit.