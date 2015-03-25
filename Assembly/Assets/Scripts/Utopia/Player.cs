using UnityEngine;
using System.Collections.Generic;

public class Player {

    private static string[] playerModelNames = { "Avatars/box_man", "Avatars/ribbon_man", "Avatars/chain_man" };

    // Boolean 'exists' comparison; replaces "!= null".
    public static implicit operator bool(Player exists){
        return exists != null;
    } // End of boolean operator.

    public Player(int _modelIdx, Vector3 pos, Quaternion rot)
    {
        SpawnPlayer(_modelIdx, pos, rot);
    }

    public void SwitchModel(int newModel)
    {
        if( newModel != modelIdx )
        {
            Vector3 pos = gameObject.transform.position;
            Quaternion rot = gameObject.transform.rotation;
            GameObject.Destroy(gameObject);
            SpawnPlayer(newModel, pos, rot);
        }
    }

    private void SpawnPlayer(int newModel, Vector3 pos, Quaternion rot)
    {
        modelIdx = newModel % playerModelNames.Length;
        gameObject = GameObject.Instantiate(Resources.Load(playerModelNames[modelIdx])) as GameObject;
        gameObject.AddComponent<PlayerInputManager>();
        gameObject.transform.position = pos;
        gameObject.transform.rotation = rot;
        gameObject.GetComponent<PlayerController>().forwardAngle = rot.eulerAngles.y;   
    }

    private GameObject go;
    private int modelIdx;
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
    public int Model { get { return modelIdx; } }
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
