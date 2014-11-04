using UnityEngine;
using System.Collections;

public class PrefabManager : MonoBehaviour {

    public static PrefabManager Inst;

    public Transform playerSyncObject;
    public Transform pingBurst;
    public Transform jellyfish;

    public AudioClip pingClip;
    public AudioClip placePingClip;



    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
