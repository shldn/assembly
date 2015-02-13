using UnityEngine;
using System.Collections;

public class LookAtObject : MonoBehaviour {

    public GameObject lookAtMe = null;
    public bool useUpDir = false;
	
	void Update () {
        if (lookAtMe != null)
        {
            gameObject.transform.LookAt(lookAtMe.transform.position);
            if( useUpDir )
                gameObject.transform.Rotate(Vector3.right, 90.0f);
        }
	}
}
