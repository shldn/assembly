using UnityEngine;
using System.Collections;

public class DestroyIfServer : MonoBehaviour {


    void Awake(){
        if (!JellyfishGameManager.IsClient)
            Destroy(gameObject);
    } // End of Awake().

} // End of DestroyIfMobile.