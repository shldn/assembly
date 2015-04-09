using UnityEngine;

// ---------------------------------------------------------------------------------- //
// PlayerController.cs
//
// Injects user input to the attached 'PlayerController' script for driving around
//   the local player.
//
// Not sure what the 'keycodemap' nonsense is...
// ---------------------------------------------------------------------------------- //

public class PlayerInputManager : MonoBehaviour {


    PlayerController playerController;
    int consecutiveInputCount = 0;
    public bool disableKeyPressMovement = false;
    //KeyCodeMap keyMapState;

    CustomKey upKey = new CustomKey(new KeyCode[]{KeyCode.UpArrow, KeyCode.W});
    CustomKey downKey = new CustomKey(new KeyCode[]{KeyCode.DownArrow, KeyCode.S});
    CustomKey leftKey = new CustomKey(new KeyCode[]{KeyCode.LeftArrow, KeyCode.A});
    CustomKey rightKey = new CustomKey(new KeyCode[]{KeyCode.RightArrow, KeyCode.D});
    CustomKey jumpKey = new CustomKey(new KeyCode[]{KeyCode.Space});

    float v = 0f;
    float h = 0f;
    bool jump = false;

    // Let other systems simulate key presses
    public float addV = 0f;
    public float addH = 0f;


    // Use this for initialization
	void Start(){
        playerController = gameObject.GetComponent<PlayerController>();
        //keyMapState = KeyCodeMap.none;
	} // End of Start().

    void OnGUI(){
        upKey.Check();
        downKey.Check();
        leftKey.Check();
        rightKey.Check();
        jumpKey.Check();

        v = 0f;
        h = 0f;

        if(upKey.down)
            v += 1f;
        if(downKey.down)
            v -= 1f;

        if(leftKey.down)
            h -= 1f;
        if(rightKey.down)
            h += 1f;

        if(jumpKey.down)
            jump = true;
    }


	void Update(){

        bool run = Input.GetKey (KeyCode.LeftShift) | Input.GetKey (KeyCode.RightShift);

        if(jump){
            playerController.Jump();
            jump = false;
        }

        // Overhaul
        if (MainCameraController.Inst.cameraType != CameraType.FIRSTPERSON){
            playerController.forwardThrottle = v + Mathf.Clamp(addV,-1f, 1f);
            playerController.turnThrottle = h + Mathf.Clamp(addH, -1f, 1f);
            playerController.speed = run ? PlayerController.MovementSpeed.run : PlayerController.MovementSpeed.walk;
        }

        if ((v != 0f) || (h != 0f) || (addV != 0f) || (addH != 0f)){
            playerController.pathfindingActive = false;
            playerController.StopFollowingPlayer();
        }

        addV = Mathf.MoveTowards(Mathf.Clamp(addV, -2f,2f), 0.0f, 2f * Time.deltaTime);
        addH = Mathf.MoveTowards(Mathf.Clamp(addH, -2f, 2f), 0.0f, 2f * Time.deltaTime);

        // bug fix for players stuck rotating
        upKey.UpdateCheck();
        downKey.UpdateCheck();
        leftKey.UpdateCheck();
        rightKey.UpdateCheck();
        jumpKey.UpdateCheck();

    } // End of Update().

}


public class CustomKey {

    KeyCode[] keyCodes;
    public bool down = false;
    int upCount = 0;

    public CustomKey(KeyCode _keyCode){
        keyCodes = new KeyCode[]{_keyCode};
    }

    public CustomKey(KeyCode[] _keyCodes){
        keyCodes = _keyCodes;
    }

    public void Check(){
        Event e = Event.current;
        for(int i = 0; i < keyCodes.Length; i++){
            KeyCode keyCode = keyCodes[i];
            if((e.type == EventType.keyDown) && (Event.current.keyCode == keyCode))
            {
                down = true;
                upCount = 0;
            }
            else if((e.type == EventType.keyUp) && (Event.current.keyCode == keyCode))
                down = false;
        }
    } // End of Check().

    // some up events weren't getting called in OnGUI, leaving keys in the down state
    public void UpdateCheck()
    {
        if( down )
        {
            for(int i = 0; i < keyCodes.Length; i++){
                if (Input.GetKey(keyCodes[i]))
                    return;
            }

            // none of the keys are still down - give a 2 frame buffer, then consider the key up
            if (++upCount > 1)
                down = false;
        }
    } // End of UpdateCheck().

} // End of CustomKey.