using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jellyfish : MonoBehaviour, CaptureObject {
    
    public static List<Jellyfish> all = new List<Jellyfish>();

    bool queuedToDestroy = false;
    public JellyFishCreator creator;


    void Awake(){
        all.Add(this);
        PersistentGameManager.CaptureObjects.Add(this);
        creator = GetComponent<JellyFishCreator>();
    } // End of Awake().


    void Update(){
        if(queuedToDestroy){
            all.Remove(this);
            Destroy(gameObject);
        }

        if(PersistentGameManager.IsClient)
            GetComponent<Rigidbody>().isKinematic = true;
    } // End of Update().


    public void Destroy(){
        queuedToDestroy = true;
    } // End of Destroy().

    void OnDestroy() {
        PersistentGameManager.CaptureObjects.Remove(this);
    }


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

    public Vector3 Position { get { return transform.position; } }

} // End of Jellyfish.
