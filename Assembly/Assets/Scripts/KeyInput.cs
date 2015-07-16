using UnityEngine;
using System.Collections;

// KeyInput =================================================================
//
// Allows us to lock out key press actions when a text input field has focus.
//===========================================================================
public class KeyInput {

    private static bool locked = false;
    public static bool Locked { get { return locked; } set { locked = value; } }

    public static bool GetKeyDown(KeyCode c)
    {
        return !Locked && Input.GetKeyDown(c);
    }

    public static bool GetKeyUp(KeyCode c)
    {
        return !Locked && Input.GetKeyUp(c);
    }

    public static bool GetKey(KeyCode c)
    {
        return !Locked && Input.GetKey(c);
    }
}
