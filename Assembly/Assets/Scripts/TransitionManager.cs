using UnityEngine;
using System.Collections;

public class TransitionManager : MonoBehaviour {

    public static TransitionManager Inst = null;
    public Texture2D white = null;

    public enum TransitionType {
        initialize,
        fadeIn,
        normal,
        fadeOut,
        waitFornext
    } // End of TransitionType.
    TransitionType transitionType = TransitionType.initialize;


    float fadeAlpha = 1f;
    float fadeVel = 0f;

    public float fadeInTime = 3f;
    public float fadeOutTime = 3f;

    int targetLevel = 0;

    Jellyfish zoomJelly = null;


    void Awake(){
        Inst = this;
        AudioListener.volume = 1f - fadeAlpha;
        targetLevel = Application.loadedLevel;
    } // End of Awake().


    public void PreviousLevel(){
        ChangeLevel(Mathf.FloorToInt(Mathf.Repeat((Application.loadedLevel - 1), Application.levelCount)));
    } // End of NextLevel().

    public void NextLevel(){
        ChangeLevel(Mathf.FloorToInt(Mathf.Repeat((Application.loadedLevel + 1), Application.levelCount)));
    } // End of NextLevel().

    public void ChangeLevel(int nextLevel){
        // If we are already pending a level change, a second attempt happens instantly.
        targetLevel = nextLevel;
    } // End of ChangeLevel.

	
	// Update is called once per frame
	void Update(){

        // Scene setup. All dark, all quiet--wait until scene is ready to go.
        if(transitionType == TransitionType.initialize){
            fadeAlpha = 1f;
            
            if(JellyfishGameManager.Inst){
                zoomJelly = Jellyfish.all[Random.Range(0, Jellyfish.all.Count)];
                CameraControl.Inst.selectedJellyfish = zoomJelly;

                CameraControl.Inst.radius = 0f;
                CameraControl.Inst.targetRadius = CameraControl.Inst.maxRadius;
            }
            if(GameManager.Inst){
                CameraControl.Inst.radius = 1000f;
            }


            transitionType = TransitionType.fadeIn;
        }

        // Scene setup. All dark, all quiet--wait until scene is ready to go.
        if(transitionType == TransitionType.fadeIn){
            fadeAlpha = Mathf.SmoothDamp(fadeAlpha, 0f, ref fadeVel, fadeInTime);
            fadeAlpha = Mathf.MoveTowards(fadeAlpha, 0f, 0.1f * Time.deltaTime);
            if(fadeAlpha == 0f)
                transitionType = TransitionType.normal;

            CameraControl.Inst.smoothTime = Mathf.Lerp(0.5f, 5f, fadeAlpha);

            // If we wanna change levels, we jump to fadeOut.
            if(targetLevel != Application.loadedLevel){
                transitionType = TransitionType.fadeOut;
            }
        }

        // Scene setup. All dark, all quiet--wait until scene is ready to go.
        if(transitionType == TransitionType.normal){

            if(CameraControl.Inst.selectedJellyfish){
                CameraControl.Inst.centerOffset = CameraControl.Inst.center - Vector3.zero;
                CameraControl.Inst.selectedJellyfish = null;
                zoomJelly = null;
            }

            fadeAlpha = 0f;
            // If we wanna change levels, we jump to fadeOut.
            if(targetLevel != Application.loadedLevel){
                transitionType = TransitionType.fadeOut;
            }
        }

        // Scene setup. All dark, all quiet--wait until scene is ready to go.
        if(transitionType == TransitionType.fadeOut){

            if(JellyfishGameManager.Inst){
                if(!zoomJelly){
                    zoomJelly = Jellyfish.all[Random.Range(0, Jellyfish.all.Count)];
                    CameraControl.Inst.selectedJellyfish = zoomJelly;
                    CameraControl.Inst.centerOffset = CameraControl.Inst.center - zoomJelly.transform.position;

                }
                CameraControl.Inst.minRadius = 0f;
                CameraControl.Inst.targetRadius = 0f;
                CameraControl.Inst.smoothTime = 0.3f;
            }
            else if(GameManager.Inst){
                CameraControl.Inst.targetRadius = CameraControl.Inst.radius + 500f;
                CameraControl.Inst.maxRadius = CameraControl.Inst.targetRadius;
            }

            fadeAlpha = Mathf.SmoothDamp(fadeAlpha, 1f, ref fadeVel, fadeOutTime);
            fadeAlpha = Mathf.MoveTowards(fadeAlpha, 1f, 0.1f * Time.deltaTime);
            if(fadeAlpha == 1f)
                transitionType = TransitionType.waitFornext;
        }

        if(transitionType == TransitionType.waitFornext){
            Application.LoadLevel(targetLevel);
        }

        AudioListener.volume = 1f - fadeAlpha;
        
	} // End of Update().


    void OnGUI(){
        if(fadeAlpha > 0f){
            GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
            GUI.DrawTexture(new Rect(-5f, -5f, Screen.width + 10f, Screen.height + 10f), white);
        }

    } // End of OnGUI().


    

} // End of Transitionmanager.
