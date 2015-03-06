using UnityEngine;
using System.Collections.Generic;

public class Player {

    // Boolean 'exists' comparison; replaces "!= null".
    public static implicit operator bool(Player exists){
        return exists != null;
    } // End of boolean operator.

    public Player(GameObject go_)
    {
        gameObject = go_;
    }

    private GameObject go;
    private bool visible = true;
    private bool visibilityHasBeenSet = false;
    public PlayerController playerController = null;
    private Stack<GameObject> disabledGOs = new Stack<GameObject>();

 	
    // Accessors
    public GameObject gameObject { 
        get { return go; } 
        set { 
            go = value;
            playerController = go.GetComponent<PlayerController>();
            playerController.playerScript = this;
        } 
    }
    public Vector3 HeadPosition { get { return gameObject.transform.position + (Vector3.up * 3f); } }
    public bool IsLocal { get { return true; } } // is this the player I'm controlling
    public bool IsRemote { get { return !IsLocal; } }
    public bool Visible
    {
        get { return visible; }

        set
        {
            if (gameObject == null || (visible == value && visibilityHasBeenSet))
                return;
            visible = value;
            visibilityHasBeenSet = true;

            if (IsRemote)
                gameObject.SetActive(visible);
            if (IsLocal)
            {
                // Turning off children still allows input to effect player
                if (!visible)
                {
                    Transform[] ts = gameObject.GetComponentsInChildren<Transform>(); // can't use this approach when they are disabled, GetComponents function will return nothing.
                    for (int i = 0; i < ts.Length; ++i)
                        if (ts[i].gameObject != gameObject)
                        {
                            ts[i].gameObject.SetActive(visible);
                            disabledGOs.Push(ts[i].gameObject);
                        }
                }
                else
                {
                    while (disabledGOs.Count > 0)
                        disabledGOs.Pop().SetActive(visible);
                }

            }
            gameObject.layer = LayerMask.NameToLayer(visible ? "Default" : "Ignore Raycast");
        }
    }

}
