using UnityEngine;
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

    // Orbit = pan/tilt x/y of the camera around the centerPos.
    public static float mouseSensitivity = 3f;

    // Object selections
	public CaptureObject selectedCaptureObj = null;

	public float targetRadius = 0f;

	float fogAmount = 0f;
	float fogVel = 0f;

	// Camera Engines --------------------------------------------------------------------------------------------------------- //
	// A CameraEngine is a self-contained scheme for driving the camera's position and rotation. Whenever the camera behaviour is
	//    changed, a new CameraEngine scheme is created, and it is blended to from the previous CameraEngine. This allows for
	//    solidly-locked tracking and precision behaviours that can be nonetheless smoothly transitioned.
	public class CameraEngine {
		protected Vector3 position = Vector3.zero;
		public Vector3 Position { get { return position; } }
		protected Quaternion rotation = Quaternion.identity;
		public Quaternion Rotation { get { return rotation; } }

		public static implicit operator bool (CameraEngine exists) { return exists != null; }

		public virtual void Init() { }
		public virtual void Update() { }
	} // End of CameraEngine.
	
	CameraEngine currentCamEngine = new StaticCamera(); // Which cam engine we are currently running, or transitioning from.
	CameraEngine nextCamEngine = null; // Cam engine we are currently transitioning to.
	CameraEngine storedCamEngine = null; // When we try to transition while already transitioning, we save it here to get to once the transition is done.
	float camEngineLerp = 0f;

	// Does nothing.
	public class StaticCamera : CameraEngine {
	} // End of StaticCamera.

	public class UserOrbitCamera : CameraEngine {
		float radius = 100f;
		float radiusSensitivity = 1f;

		float lastPinchDist = -1f;
		bool pinchRelease = true;

		public override void Update() {
			// Touch-screen pinch-zoom
			if(Input.touchCount >= 2){
				pinchRelease = false;

				Vector2 touch0, touch1;
				float pinchDist;
				touch0 = Input.GetTouch(0).position;
				touch1 = Input.GetTouch(1).position;
 
				pinchDist = Vector2.Distance(touch0, touch1);

				if(lastPinchDist != -1)
					radius -= radius * (pinchDist - lastPinchDist) * 0.005f * radiusSensitivity ;

				lastPinchDist = pinchDist;
			}else{
				lastPinchDist = -1f;

				if(Input.touchCount == 0)
					pinchRelease = true;
			}

			// Keyboard zoom
			if(Input.GetKey(KeyCode.Comma) && !ConsoleScript.active)
				radius += radius * -Time.deltaTime * radiusSensitivity ;
			if(Input.GetKey(KeyCode.Period) && !ConsoleScript.active)
				radius += radius * Time.deltaTime * radiusSensitivity;

			radius += radius * -Input.GetAxis("Mouse ScrollWheel");

			// Mouse/touch orbit.
			if((PersistentGameManager.IsClient && Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0) && (Input.touchCount < 2) || (Cursor.lockState == CursorLockMode.Locked) || (!Input.GetMouseButtonDown(1) && Input.GetMouseButton(1) && pinchRelease))){
				rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * mouseSensitivity, Vector3.up);
				rotation *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * mouseSensitivity, -Vector3.right);
			}

			position = rotation * -Vector3.forward * radius;
		} // End of Update().
	} // End of UserOrbitCamera.

	// Automatically orbits in and out of the environment, eyeballing assemblies and other interesting things.
	public class GalleryAutoCamera : CameraEngine {
		Assembly assemblyOfInterest = null;
		float assemblyOfInterestStaleness = 0f;

		// Offset from center point
		float orbitRadius = 0f;
		float minRadius = 1f;
		float maxRadius = 250f;
		float galleryCamZoomRate = 60f;

		// Camera offset rotation from center point (NOT camera rotation!)
		Quaternion orbitRot = Quaternion.identity;

		// How high or low the camera can be tilted.
		float maxTilt = -80f;
		float minTilt = 80f;

		Vector2 targetPanTilt = new Vector2(0f, 0f);
		Vector2 panTiltVel = Vector2.zero;

		public override void Update() {
			float elevationPulseTime = 75f;
			float radiusLerp = Mathf.Pow((0.5f + (Mathf.Cos((Time.timeSinceLevelLoad / galleryCamZoomRate) * (Mathf.PI * 2f)) * 0.5f)), 5f);
			orbitRadius = Mathf.Lerp(minRadius, maxRadius, radiusLerp);
			orbitRot = Quaternion.Euler(Mathf.Lerp(minTilt, maxTilt, 0.5f + ((Mathf.Sin((Time.timeSinceLevelLoad / elevationPulseTime) * (Mathf.PI * 2f)) * 0.5f))), orbitRot.eulerAngles.y, 0f);

			Quaternion targetRotation = orbitRot;

			if(!assemblyOfInterest){
				assemblyOfInterestStaleness = 0f;

				// Try to find an assembly that is mating.
				foreach(Assembly someAssem in Assembly.getAll){
					float dist = Vector3.Distance(Camera.main.transform.position, someAssem.Position);
					if((dist < 60f) && someAssem.MatingWith){
						assemblyOfInterest = someAssem;
						break;
					}
				}

				// If no mating, look for one that is active.
				if(!assemblyOfInterest){
					foreach(Assembly someAssem in Assembly.getAll){
						float dist = Vector3.Distance(Camera.main.transform.position, someAssem.Position);
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

				targetRotation = Quaternion.LookRotation(assemblyOfInterest.Position - Camera.main.transform.position, Camera.main.transform.up);
				float dist = Vector3.Distance(Camera.main.transform.position, assemblyOfInterest.Position);
				if((dist > 80f) || (!assemblyOfInterest.MatingWith && (assemblyOfInterestStaleness > 10f)))
					assemblyOfInterest = null;
			}

			targetPanTilt = targetRotation.eulerAngles;
			Vector3 tempEulers = Camera.main.transform.eulerAngles;
			float panTiltSmoothTime = 2f;
			tempEulers.x = Mathf.SmoothDampAngle(tempEulers.x, targetPanTilt.x, ref panTiltVel.x, panTiltSmoothTime);
			tempEulers.y = Mathf.SmoothDampAngle(tempEulers.y, targetPanTilt.y, ref panTiltVel.y, panTiltSmoothTime);
			rotation = Quaternion.Euler(tempEulers);

			position = orbitRot * -Vector3.forward * orbitRadius;
		} // End of Update().
	} // End of GalleryAutoCamera.

	public class NeuroFocusCamera : CameraEngine {
		float orbitRadius = 0f;
		float minRadius = 1f;
		float maxRadius = 250f;
		float galleryCamZoomRate = 120f;

		float maxTilt = -80f;
		float minTilt = 80f;
		Quaternion orbitRot = Quaternion.identity;

		Node focusNode = null;

		public NeuroFocusCamera(Node focusNode) {
			this.focusNode = focusNode;
		}

		public override void Update() {
			float elevationPulseTime = 75f;
			float radiusLerp = Mathf.Pow((0.5f + (Mathf.Cos((Time.timeSinceLevelLoad / galleryCamZoomRate) * (Mathf.PI * 2f)) * 0.5f)), 5f);
			rotation = Quaternion.Euler(Mathf.Lerp(minTilt, maxTilt, 0.5f + ((Mathf.Sin((Time.timeSinceLevelLoad / elevationPulseTime) * (Mathf.PI * 2f)) * 0.5f))), Time.timeSinceLevelLoad, 0f);

			float orbitRadius = 10f + (240f * Mathf.Pow(NeuroScaleDemo.Inst.enviroScale, 1f));

			Vector3 focusPoint = Vector3.zero;
			if(focusNode && (NeuroScaleDemo.Inst.enviroScale < 1f))
				focusPoint = Vector3.Lerp(focusNode.Position, Vector3.zero, NeuroScaleDemo.Inst.enviroScale);
			position = focusPoint + rotation * -Vector3.forward * orbitRadius;

			// Clear focus node if we pull all the way back.
			if(NeuroScaleDemo.Inst.enviroScale == 1f)
				focusNode = null;
		} // End of Update().
	} // End of NeuroFocusCamera.

	public class AssemblyHerdCamera : CameraEngine {
		Vector3 center = Vector3.zero;
		float radius = 0f;

		public override void Update() {
			List<Vector3> pts = new List<Vector3>();
			if(AssemblyEditor.Inst && (AssemblyEditor.Inst.testAssemblies != null) && (AssemblyEditor.Inst.testAssemblies.Count > 0)) {
				foreach (Assembly someAssembly in AssemblyEditor.Inst.testAssemblies)
					if(!someAssembly.cull)
						pts.Add(someAssembly.Position);
				float sphereRadius = 0f;
				MathHelper.GetBoundingSphere(pts, out center, out sphereRadius);
				radius = (sphereRadius + 10f) / Mathf.Tan(Camera.main.fieldOfView * 0.4f * Mathf.Deg2Rad);
			}

			position = center - Camera.main.transform.forward * radius;
		} // End of Update().
	} // End of AssemblyHerdCamera.

	// ----------------------------------------------------------------------------------------------------------------------- //



    void Awake(){
        Inst = this;
    } // End of Awake().


	void Start(){
		if(VRDevice.isPresent){
			Rigidbody camRB = Camera.main.gameObject.AddComponent<Rigidbody>();
			camRB.interpolation = RigidbodyInterpolation.Interpolate;
			camRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			camRB.useGravity = false;
			camRB.isKinematic = true;
		}

		currentCamEngine = new UserOrbitCamera();
	} // End of Start().
	

	void LateUpdate(){
		// Surrender all camera control to Unity if a VR device is being used... for now.
		if(VRDevice.isPresent)
			return;

		// Update cam engines
		if(currentCamEngine)
			currentCamEngine.Update();
		if(nextCamEngine)
			nextCamEngine.Update();
		if(storedCamEngine)
			storedCamEngine.Update();

		if(Input.GetKeyDown(KeyCode.Alpha1))
			SetMode_Static();
		if(Input.GetKeyDown(KeyCode.Alpha2))
			SetMode_UserOrbit();
		if(Input.GetKeyDown(KeyCode.Alpha3))
			SetMode_GalleryAuto();
		if(Input.GetKeyDown(KeyCode.Alpha4))
			SetMode_AssemblyHerd();


		if(storedCamEngine && !nextCamEngine) {
			nextCamEngine = storedCamEngine;
			storedCamEngine = null;
		}

		if(!nextCamEngine) {
			transform.position = currentCamEngine.Position;
			transform.rotation = currentCamEngine.Rotation;
		} else {
			transform.position = Vector3.Lerp(currentCamEngine.Position, nextCamEngine.Position, MathUtilities.LinToSmoothLerp(MathUtilities.LinToSmoothLerp(camEngineLerp)));
			transform.rotation = Quaternion.Lerp(currentCamEngine.Rotation, nextCamEngine.Rotation, MathUtilities.LinToSmoothLerp(MathUtilities.LinToSmoothLerp(camEngineLerp)));

			camEngineLerp += Time.deltaTime * 0.3f;
			if(camEngineLerp >= 1f) {
				camEngineLerp = 0f;

				currentCamEngine = nextCamEngine;
				nextCamEngine = null;
			}
		}

		// Manual control of fog distance
		float targetFogLerp = 0f;
		if(NeuroScaleDemo.Inst && NeuroScaleDemo.Inst.isActive && !ClientTest.Inst)
			targetFogLerp = 1f - NeuroScaleDemo.Inst.enviroScale;
		fogAmount = Mathf.SmoothDamp(fogAmount, targetFogLerp, ref fogVel, 1f);
		RenderSettings.fogStartDistance = Mathf.Lerp(300f, 10f, fogAmount);
		RenderSettings.fogEndDistance = Mathf.Lerp(1000f, 20f, fogAmount);

	} // End of Update().

	// Setting camera mode
	public void SetMode_Static() { storedCamEngine = new StaticCamera(); storedCamEngine.Init(); }
	public void SetMode_NeuroScaleFocus(Node focusNode) {
		storedCamEngine = new NeuroFocusCamera(focusNode);
		storedCamEngine.Init();
		print("Setting camera mode to NeuroScaleFocus.");
	}
	public void SetMode_UserOrbit() { storedCamEngine = new UserOrbitCamera(); storedCamEngine.Init(); }
	public void SetMode_GalleryAuto() { storedCamEngine = new GalleryAutoCamera(); storedCamEngine.Init(); }
	public void SetMode_AssemblyHerd() { storedCamEngine = new AssemblyHerdCamera(); storedCamEngine.Init(); }


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

} // End of CameraOrbit.