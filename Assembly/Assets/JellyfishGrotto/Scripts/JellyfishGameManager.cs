using UnityEngine;
using System.Collections;

public class JellyfishGameManager : MonoBehaviour {

    public static JellyfishGameManager Inst;

    public bool editing = false;

    public Material editorSkybox;


    void Awake(){
        PersistentGameManager.Inst.Touch();
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
        if (!PersistentGameManager.IsClient){
            for(int i = 0; i < 30; i++){
                Transform newJellyTrans = Instantiate(JellyfishPrefabManager.Inst.jellyfish, MathUtilities.RandomVector3Sphere(30f), Random.rotation) as Transform;

                JellyFishCreator jellyCreator = newJellyTrans.GetComponent<JellyFishCreator>();

                jellyCreator.HeadChange();
                jellyCreator.TailChange();
                jellyCreator.SmallTaillChange();
                jellyCreator.BoballChange();

                transform.localScale *= Random.Range(0.75f, 1.5f);
            }
        }
        else{
            Camera.main.clearFlags = CameraClearFlags.Skybox;
            RenderSettings.skybox = editorSkybox;
        }
	}


    void Update(){
        LevelManager.InputHandler();
        if(Input.GetKey(KeyCode.Escape))
            Application.Quit();
    } // End of Update().

    void OnDestroy()
    {
        // Clear all static data structures
        // - If we reload this level, the objects in them will have been destroyed
        Jellyfish.all.Clear();
    }
	

} // End of GameManager.
