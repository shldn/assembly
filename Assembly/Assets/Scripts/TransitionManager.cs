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

		float targetFade = 0f;
		float fadeTime = 0f;

        // Scene setup. All dark, all quiet--wait until scene is ready to go.
        if(transitionType == TransitionType.initialize){
            fadeAlpha = 1f;
            transitionType = TransitionType.fadeIn;
        }

        // Fade in to/reveal scene.
        if(transitionType == TransitionType.fadeIn){
			targetFade = 0f;
			fadeTime = fadeInTime;

			// Switch to normal mode once faded completely in.
            if(fadeAlpha == 0f)
                transitionType = TransitionType.normal;

            // If we wanna change levels, we jump to fadeOut.
            if(targetLevel != Application.loadedLevel)
                transitionType = TransitionType.fadeOut;
        }

        // Scene running as normal.
        if(transitionType == TransitionType.normal){
            fadeAlpha = 0f;

            // If we wanna change levels, we jump to fadeOut.
            if(targetLevel != Application.loadedLevel)
                transitionType = TransitionType.fadeOut;
        }

        // Scene exit--fade out.
        if(transitionType == TransitionType.fadeOut){
			targetFade = 1f;
			fadeTime = fadeOutTime;

			// Load next level once we've fadedo out completely.
            if(fadeAlpha == 1f)
                transitionType = TransitionType.waitFornext;
        }

        if(transitionType == TransitionType.waitFornext)
            Application.LoadLevel(targetLevel);

		fadeAlpha = Mathf.SmoothDamp(fadeAlpha, targetFade, ref fadeVel, fadeTime);
        fadeAlpha = Mathf.MoveTowards(fadeAlpha, targetFade, 0.01f * Time.deltaTime); // Make sure we actually get there...
        AudioListener.volume = 1f - fadeAlpha;
        
	} // End of Update().


    void OnGUI(){
        if(fadeAlpha > 0f){
            GUI.color = new Color(0f, 0f, 0f, fadeAlpha);
            GUI.DrawTexture(new Rect(-5f, -5f, Screen.width + 10f, Screen.height + 10f), white);
        }

    } // End of OnGUI().

	
} // End of Transitionmanager.
