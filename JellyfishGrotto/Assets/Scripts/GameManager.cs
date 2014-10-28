using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public static GameManager Inst;

    public Transform jellyfishPrefab;


    void Awake(){
        Inst = this;
    } // End of Awake().


    // Put in (0f-1f, 0f-1f, 0f), receive area on play screen.
    public Vector3 ScreenToPlayArea(Vector3 screenPosNormal){
        Vector3 screenPosAdjusted = screenPosNormal;
        screenPosAdjusted -= new Vector3(0.5f, 0.5f, 0f);
        screenPosAdjusted.x *= ((float)Screen.width / (float)Screen.height);
        return Vector3.zero;
    } // End of ScreenToPlayArea().


	// Use this for initialization
	void Start(){
        if(Application.platform != RuntimePlatform.Android){
            for(int i = 0; i < 30; i++){
                Instantiate(jellyfishPrefab, MathUtilities.RandomVector3Sphere(30f), Random.rotation);
            }
        }
	}


    void Update(){

        if(Input.GetKey(KeyCode.Escape))
            Application.Quit();
    } // End of Update().
	

} // End of GameManager.
