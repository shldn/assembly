using UnityEngine;
using System.Collections;

public class PrefabManager : MonoBehaviour {

    public static PrefabManager Inst;

    public Transform playerSyncObject;
    public Transform pingBurst;



    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
