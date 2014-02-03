using UnityEngine;
using System.Collections;

public class PrefabManager : MonoBehaviour {

    public static PrefabManager Inst = null;

    public GameObject node = null;
	public GameObject bond = null;
	public GameObject billboard = null;
	public GameObject foodNode = null;

    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
