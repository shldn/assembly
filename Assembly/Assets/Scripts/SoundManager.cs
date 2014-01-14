using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

	public static SoundManager inst = null;

    public AudioClip replicate;

    void Awake(){
        inst = this;
    } // End of Awake().
} // End of SoundManager class.
