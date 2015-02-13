using UnityEngine;
using System.Collections;

// Place on the object that will do the looking and specify what to look at with lookAtMe.
public class LookAtObject : MonoBehaviour {

    public GameObject lookAtMe = null;
    public bool useUpDir = false;
    public float maxAngle = 30.0f;

    // init vars
    private Quaternion initRot = Quaternion.identity;
    private Vector3 initLookAt = Vector3.up;

    // update vars
    float angle = 0.0f;
    Vector3 axis = Vector3.up;


    void Start()
    {
        initRot = transform.rotation;
        initLookAt = useUpDir ? gameObject.transform.up : gameObject.transform.forward;
    }
	
	void Update () {
        if (lookAtMe != null)
        {
            Vector3 desiredLookAtVector = lookAtMe.transform.position - gameObject.transform.position;
            Quaternion.FromToRotation(initLookAt, desiredLookAtVector).ToAngleAxis(out angle, out axis);
            gameObject.transform.rotation = Quaternion.AngleAxis(Mathf.Min(angle, maxAngle), axis) * initRot;
        }
	}
}
