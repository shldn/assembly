using UnityEngine;
using System.Collections;

public class DestroyIfMobile : MonoBehaviour {


    void Awake(){
        if(Application.platform == RuntimePlatform.Android)
            Destroy(gameObject);
    } // End of Awake().

} // End of DestroyIfMobile.