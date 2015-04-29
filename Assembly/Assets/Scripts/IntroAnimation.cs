using UnityEngine;
using System.Collections;

public class IntroAnimation : MonoBehaviour {

	public Transform startPos;
	public Transform hangPosStart;
	public Transform hangPosEnd;
	public Transform endPos;

	int step = 0;
	float lerp = 0f;

	public Texture2D white;


	// Use this for initialization
	void Start(){
	
	}
	
	// Update is called once per frame
	void Update(){

		if(step == 0){
			transform.position = Vector3.Lerp(startPos.position, hangPosStart.position, 1f - Mathf.Pow(1f - lerp, 2f));
			lerp += Time.deltaTime * 0.5f;
			if(lerp > 1f){
				lerp = 0f;
				step = 1;
			}
		}

		if(step == 1){
			transform.position = Vector3.Lerp(hangPosStart.position, hangPosEnd.position, lerp);
			lerp += Time.deltaTime * 0.1f;
			if(lerp > 1f){
				lerp = 0f;
				step = 2;
			}
		}

		if(step == 2){
			transform.position = Vector3.Lerp(hangPosEnd.position, endPos.position, Mathf.Pow(lerp, 2f));
			lerp += Time.deltaTime * 0.75f;
			if(lerp > 1f)
				step = 3;
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
