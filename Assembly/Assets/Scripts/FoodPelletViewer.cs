using System.Collections.Generic;
using UnityEngine;

public class FoodPelletViewer {

    private static Dictionary<int, FoodPelletViewer> all = new Dictionary<int, FoodPelletViewer>();
    public static Dictionary<int, FoodPelletViewer> All { get { return all; } }

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
        transform = MonoBehaviour.Instantiate(ViewerController.Inst.physFoodPrefab, worldPosition, Random.rotation) as Transform;
        renderers = transform.GetComponentsInChildren<Renderer>();
        Visible = !ViewerController.Hide;
        id = id_;
        if(id != -1)
            all.Add(id, this);
    }

    public void Destroy(bool removeFromList = true)
    {
        GameObject.Destroy(gameObject);
        if (removeFromList && all.ContainsKey(id))
            all.Remove(id);
    }
}
