using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jellyfish : MonoBehaviour {
    
    public static List<Jellyfish> all = new List<Jellyfish>();

    bool queuedToDestroy = false;
    JellyFishCreator creator;


    void Awake(){
        all.Add(this);
        creator = GetComponent<JellyFishCreator>();
    } // End of Awake().


    void Update(){
        if(queuedToDestroy){
            all.Remove(this);
            Destroy(gameObject);
        }
    } // End of Update().


    public void Destroy(){
        queuedToDestroy = true;
    } // End of Destroy().


    public void NextHead(){
        creator.NextHead();
    }
    public void NextBobble(){
        creator.NextBobble();
    }
    public void NextTail(){
        creator.NextTail();
    }
    public void NextWing(){
        creator.NextWing();
    }

} // End of Jellyfish.
