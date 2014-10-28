using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jellyfish : MonoBehaviour {
    
    public static List<Jellyfish> all = new List<Jellyfish>();

    bool queuedToDestroy = false;


    void Awake(){
        all.Add(this);
    } // End of Awake().


    void Start(){
        JellyFishCreator myJellyCreator = GetComponent<JellyFishCreator>();

        myJellyCreator.HeadChange();
        myJellyCreator.TailChange();
        myJellyCreator.SmallTaillChange();
        myJellyCreator.BoballChange();

        transform.localScale *= Random.Range(0.75f, 1.5f);
    } // End of Start().


    void Update(){
        if(queuedToDestroy){
            all.Remove(this);
            Destroy(gameObject);
        }
    } // End of Update().


    public void Destroy(){
        queuedToDestroy = true;
    } // End of Destroy().

} // End of Jellyfish.
