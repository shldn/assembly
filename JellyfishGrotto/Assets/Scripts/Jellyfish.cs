using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jellyfish : MonoBehaviour {
    
    public static List<Jellyfish> all = new List<Jellyfish>();


    void Awake(){
        all.Add(this);
    } // End of Awake().


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
