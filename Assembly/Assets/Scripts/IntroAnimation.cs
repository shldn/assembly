using UnityEngine;
using System.Collections;

public class IntroAnimation : MonoBehaviour {

	public Transform startPos;
	public Transform hangPos;
	public Transform endPos;

	int step = 0;
	float lerp = 0f;

	float elapsedTime = 0f;

	public Texture2D white;


	// Use this for initialization
	void Start(){
	
	}
	
	// Update is called once per frame
	void Update(){
		elapsedTime += Time.deltaTime;

		if(step == 0){
			transform.position = Vector3.Lerp(startPos.position, hangPos.position, 1f - Mathf.Pow(1f - lerp, 2f));
			lerp += Time.deltaTime * 0.5f;
			if(lerp > 1f){
				lerp = 0f;
				step = 1;
			}
		}

		if(step == 1){
			transform.position = hangPos.position;
			if(Input.anyKeyDown || (elapsedTime > 20f)){
				step = 2;
				lerp = 0f;
			}
		}

		if(step == 2){
			transform.position = Vector3.Lerp(hangPos.position, endPos.position, Mathf.Pow(lerp, 2f));
			lerp += Time.deltaTime * 0.75f;
			if(lerp > 1f){
				step = 3;
				Application.LoadLevel(Application.loadedLevel + 1);
			}
		}
	
	}

	void OnGUI(){

		if(step == 0){
			GUI.color = new Color(0f, 0f, 0f, 1f - lerp);
			GUI.DrawTexture(new Rect(-10f, -10f, Screen.width + 20f, Screen.height + 20f), white);
		}
		if(step >= 2){
			GUI.color = new Color(0f, 0f, 0f, -19f + (lerp * 20f));
			GUI.DrawTexture(new Rect(-10f, -10f, Screen.width + 20f, Screen.height + 20f), white);
		}

	} // End of OnGUI().
}
