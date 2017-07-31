using System.Collections.Generic;
using UnityEngine;

public class FoodPelletViewer {

    int id = -1;
    Transform transform = null;
    private bool visible = true;
    Renderer[] renderers;
    public GameObject gameObject { get { return (transform != null) ? transform.gameObject : null; } }
    public bool Visible
    {
        get { return visible; }
        set
        {
            if (ViewerController.Hide && value)
                return;
            visible = value;
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].enabled = visible;
        }
    }
    public Vector3 Position {
        set {
            transform.position = value;
        }
    }
    public float Scale
    {
        set {
            transform.localScale = Vector3.one * value;
        }
    }

    public FoodPelletViewer(Vector3 worldPosition, int id_ = -1)
    {
        transform = ViewerController.Inst.FoodPool.Get().transform;
        transform.position = worldPosition;
        Scale = 1;
        renderers = transform.GetComponentsInChildren<Renderer>();
        Visible = !ViewerController.Hide;
        id = id_;
    }

    public void Destroy()
    {
        ViewerController.Inst.FoodPool.Release(gameObject);
    }
}
