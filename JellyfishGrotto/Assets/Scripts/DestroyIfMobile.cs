using UnityEngine;
using System.Collections;

public class DestroyIfMobile : MonoBehaviour {


    void Awake(){
        if (GameManager.IsClient)
            Destroy(gameObject);
    } // End of Awake().

} // End of DestroyIfMobile.