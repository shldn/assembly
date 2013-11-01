using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Useful functions Wes uses more often than not.
public class WesInput : MonoBehaviour{

    public static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    public static float forwardThrottle;
    public static float horizontalThrottle;
    public static float rotationThrottle;

    public static bool active = true;

    void Awake(){
	    keys[ "Forward" ] = KeyCode.W;
	    keys[ "Backward" ] = KeyCode.S;
	    keys[ "Left" ] = KeyCode.A;
	    keys[ "Right" ] = KeyCode.D;
	    keys[ "Roll Left" ] = KeyCode.Q;
	    keys[ "Roll Right" ] = KeyCode.E;
	
	    keys[ "Camera Lock" ] = KeyCode.Space;

        keys[ "Disband Assembly" ] = KeyCode.Delete;

	    keys[ "Open Console" ] = KeyCode.C;
    } // End of Awake().


    void Update(){
	    forwardThrottle = 0;
	    if(WesInput.GetKey("Forward"))
		    forwardThrottle += 1;
	    if(WesInput.GetKey("Backward"))
		    forwardThrottle -= 1;

        horizontalThrottle = 0;
	    if(WesInput.GetKey("Right"))
		    horizontalThrottle += 1;
	    if(WesInput.GetKey("Left"))
		    horizontalThrottle -= 1;
		
	    rotationThrottle = 0;
	    if(WesInput.GetKey("Turn Right"))
		    rotationThrottle += 1;
	    if(WesInput.GetKey("Turn Left"))
		    rotationThrottle -= 1;
    } // End of Update().


    public static bool GetKeyDown(string bindingName){
	    return active && Input.GetKeyDown(WesInput.keys[ bindingName ]);
    } // End of GetKeyDown().


    public static bool GetKey(string bindingName){
	    return active && Input.GetKey(WesInput.keys[ bindingName ]);
    } // End of GetKey().
} // End of WesInput().