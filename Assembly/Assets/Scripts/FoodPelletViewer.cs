using System.Collections.Generic;
using UnityEngine;

public class FoodPelletViewer {

    private static Dictionary<int, Dictionary<int, FoodPelletViewer>> amalgamFood = new Dictionary<int, Dictionary<int, FoodPelletViewer>>(); // map from amalgam id -> (food id, food)

    int id = -1;
    int amalgamId = -1;
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

    public static Dictionary<int, FoodPelletViewer> GetFoodViewers(int amalgamId) {
        return amalgamFood[amalgamId];
    }


    public FoodPelletViewer(int amalgamId_, Vector3 worldPosition, int id_ = -1)
    {
        transform = MonoBehaviour.Instantiate(ViewerController.Inst.physFoodPrefab, worldPosition, Random.rotation) as Transform;
        renderers = transform.GetComponentsInChildren<Renderer>();
        Visible = !ViewerController.Hide;
        amalgamId = amalgamId_;
        id = id_;
        if (!amalgamFood.ContainsKey(amalgamId))
            amalgamFood[amalgamId] = new Dictionary<int, FoodPelletViewer>();
        amalgamFood[amalgamId].Add(id, this);
    }

    public void Destroy(bool removeFromList = true)
    {
        GameObject.Destroy(gameObject);
        if (removeFromList && amalgamId >= 0)
            amalgamFood[amalgamId].Remove(id);
    }
}
