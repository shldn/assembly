using UnityEngine;
using System.Collections;

public class JellyfishPrefabManager : MonoBehaviour {

    public static JellyfishPrefabManager Inst;

    public Transform jellyfish;
    public AudioClip pingClip;


    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
