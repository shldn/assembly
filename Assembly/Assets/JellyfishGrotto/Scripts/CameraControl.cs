using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

    public static CameraControl Inst;


    float orbitRunner = 0f;
    float orbitRunnerTarget = 0f;
    float orbitRunnerVel = 0f;

    float orbitTilt = 20f;
    float orbitTiltTarget = 20f;
    float orbitTiltVel = 0f;

    float orbitDist = 6f;
    float targetOrbitDist = 6f;
    public float orbitSpeed = 1f;
    float orbitDistVel = 0f;

    float lastPinchDist = -1f;

    float smoothTime = 0.5f;

    Vector3 orbitTarget = Vector3.zero;

    Quaternion rotEditor = Quaternion.identity;
    Quaternion targetRotEditor = Quaternion.identity;

    Vector3 positionVel = Vector3.zero;

    bool pinchRelease = true;

    public Transform target = null;


    void Awake(){
        Inst = this;
    } // End of Awake().

	// Use this for initialization
	void Start(){
        RenderSettings.fog = true;
        RenderSettings.fogColor = Camera.main.backgroundColor;

        targetRotEditor = Random.rotation;
        rotEditor = targetRotEditor;
        transform.rotation = rotEditor;
	} // End of Start().
	

	// Update is called once per frame
	void Update(){
	    orbitRunnerTarget += Time.deltaTime;

        if(Network.peerType == NetworkPeerType.Server){
            orbitDist = 40f;
            transform.position = orbitTarget + Quaternion.AngleAxis(-orbitRunner * orbitSpeed, Vector3.up) * Quaternion.AngleAxis(-orbitTilt, Vector3.right) * Vector3.forward * orbitDist;
            transform.LookAt(orbitTarget);

            if((Network.peerType == NetworkPeerType.Server)){
                float orbitSensitivity = 3f;
                orbitRunnerTarget -= Input.GetAxis("Mouse X") * orbitSensitivity;
                orbitTiltTarget -= Input.GetAxis("Mouse Y") * orbitSensitivity;

                orbitTiltTarget = Mathf.Clamp(orbitTiltTarget, -35f, 35f);

                Screen.showCursor = false;
                Screen.lockCursor = true;
            }

            orbitRunner = Mathf.SmoothDamp(orbitRunner, orbitRunnerTarget, ref orbitRunnerVel, smoothTime);
            orbitTilt = Mathf.SmoothDamp(orbitTilt, orbitTiltTarget, ref orbitTiltVel, smoothTime);
        }
        else if (PersistentGameManager.IsClient){

            if(Jellyfish.all.Count > 0 && Jellyfish.all[0])
                orbitTarget = Jellyfish.all[0].transform.position;
            if((Assembly.GetAll().Count > 0) && Assembly.GetAll()[0]){
                Assembly editingAssembly = Assembly.GetAll()[0];
                if(AssemblyEditor.Inst.selectedNode)
                    orbitTarget = AssemblyEditor.Inst.selectedNode.gameObject.transform.position;
                else
                    orbitTarget = editingAssembly.physicsObject.rigidbody.worldCenterOfMass;
            }

            if (Input.touchCount >= 2){
                pinchRelease = false;

                Vector2 touch0, touch1;
                float pinchDist;
                touch0 = Input.GetTouch(0).position;
                touch1 = Input.GetTouch(1).position;
 
                pinchDist = Vector2.Distance(touch0, touch1);

                if(lastPinchDist != -1){
                    targetOrbitDist -= (pinchDist - lastPinchDist) * 0.05f;
                }
                lastPinchDist = pinchDist;
            }
            else{
                lastPinchDist = -1f;

                if(Input.touchCount == 0)
                    pinchRelease = true;
            }

            targetOrbitDist += targetOrbitDist * -Input.GetAxis("Mouse ScrollWheel");

            if(!Input.GetMouseButtonDown(0) && Input.GetMouseButton(0) && pinchRelease && !NodeEngineering.Inst.uiLockout && ((Jellyfish.all.Count > 0) || (Assembly.GetAll().Count > 0))){
                targetRotEditor *= Quaternion.AngleAxis(Input.GetAxis("Mouse X") * 3f, Vector3.up);
                targetRotEditor *= Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * 3f, -Vector3.right);
            }
            
            
            targetOrbitDist = Mathf.Clamp(targetOrbitDist, 3f, 40f);


            orbitDist = Mathf.SmoothDamp(orbitDist, targetOrbitDist, ref orbitDistVel, 0.1f);
            rotEditor = Quaternion.Lerp(rotEditor, targetRotEditor, Time.deltaTime);

            transform.position = Vector3.SmoothDamp(transform.position, orbitTarget + (targetRotEditor * (-Vector3.forward * orbitDist)), ref positionVel, 0.2f);
            transform.rotation = targetRotEditor;
            
            
        }
	} // End of Update().


    void OnDestroy()
    {
        Screen.showCursor = true;
        Screen.lockCursor = false;
    }


    void OnDrawGizmos(){
        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(Vector3.zero, orbitDist);
        Gizmos.color = new Color(0f, 1f, 1f, 0.05f);
        Gizmos.DrawSphere(Vector3.zero, orbitDist);
    } // End of OnDrawGizmos().

} // End of CameraOrbit.
