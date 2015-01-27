using UnityEngine;
using System.Collections;

public class JellyfishPrefabManager : MonoBehaviour {

    public static JellyfishPrefabManager Inst;

    public Transform playerSyncObject;
    public Transform pingBurst;
    public Transform jellyfish;

    public AudioClip pingClip;
    public AudioClip placePingClip;



    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
