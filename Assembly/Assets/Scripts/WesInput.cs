using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Useful functions Wes uses more often than not.
public class WesInput : MonoBehaviour{

    public static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    public static float forwardThrottle;
    public static float horizontalThrottle;
    public static float verticalThrottle;
    public static float rotationThrottle;

    public static float editHorizontalThrottle;
    public static float editVerticalThrottle;

    public static bool active = true;

    void Awake(){

	    keys[ "Forward" ] = KeyCode.W;
	    keys[ "Backward" ] = KeyCode.S;
        keys[ "Up" ] = KeyCode.LeftShift;
        keys[ "Down" ] = KeyCode.LeftControl;
	    keys[ "Left" ] = KeyCode.A;
	    keys[ "Right" ] = KeyCode.D;
	    keys[ "Roll Left" ] = KeyCode.Q;
	    keys[ "Roll Right" ] = KeyCode.E;

        keys[ "Edit Up" ] = KeyCode.UpArrow;
        keys[ "Edit Down" ] = KeyCode.DownArrow;
	    keys[ "Edit Left" ] = KeyCode.LeftArrow;
	    keys[ "Edit Right" ] = KeyCode.RightArrow;
	
	    keys[ "Camera Lock" ] = KeyCode.Space;
	    keys[ "Auto Orbit" ] = KeyCode.O;

	    keys[ "Add Node" ] = KeyCode.Alpha1;
	    keys[ "Remove Node" ] = KeyCode.Alpha2;
        keys[ "Replicate Assembly" ] = KeyCode.R;

        keys[ "Disband Assembly" ] = KeyCode.Delete;

	    keys[ "Open Console" ] = KeyCode.Return;
	    keys[ "Quit" ] = KeyCode.Escape;
	    keys[ "Reload Application" ] = KeyCode.F12;

    } // End of Awake().


    void Update(){

        // Camera throttle
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

        verticalThrottle = 0;
	    if(WesInput.GetKey("Up"))
		    verticalThrottle += 1;
	    if(WesInput.GetKey("Down"))
		    verticalThrottle -= 1;
		
	    rotationThrottle = 0;
	    if(WesInput.GetKey("Roll Right"))
		    rotationThrottle += 1;
	    if(WesInput.GetKey("Roll Left"))
		    rotationThrottle -= 1;

        // Editor throttle
        editHorizontalThrottle = 0;
	    if(WesInput.GetKey("Edit Right"))
		    editHorizontalThrottle  += 1;
	    if(WesInput.GetKey("Edit Left"))
		    editHorizontalThrottle  -= 1;

        editVerticalThrottle = 0;
	    if(WesInput.GetKey("Edit Up"))
		    editVerticalThrottle += 1;
	    if(WesInput.GetKey("Edit Down"))
		    editVerticalThrottle -= 1;

    } // End of Update().


    public static bool GetKeyDown(string bindingName){
	    return active && Input.GetKeyDown(WesInput.keys[ bindingName ]);
    } // End of GetKeyDown().


    public static bool GetKey(string bindingName){
	    return active && Input.GetKey(WesInput.keys[ bindingName ]);
    } // End of GetKey().
} // End of WesInput().