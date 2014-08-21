using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public Transform playButton;
    public Transform startServerButton;
    public Transform optionsButton;
    public Transform helpButton;
    public Transform quitButton;


	void Start(){
	
	} // End of Start().
	

	void Update(){
	
        if(Input.GetMouseButtonDown(0)){
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(mouseRay.origin, mouseRay.direction * 1000f);
            RaycastHit hitInfo = new RaycastHit();
            if(Physics.Raycast(mouseRay, out hitInfo)){
                if(hitInfo.collider.transform == playButton){
                    Application.LoadLevel("Scene0");
                    print("play");
                }
                else if(hitInfo.collider.transform == startServerButton){
                    Application.LoadLevel("Overworld");
                    print("start server");
                }
            }
        }

	} // End of Update().
} // End of MainMenu.
