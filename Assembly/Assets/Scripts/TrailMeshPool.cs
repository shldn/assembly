using UnityEngine;
using System.Collections.Generic;

public class TrailMeshPool {

    public static List<GameObject> used = new List<GameObject>();
    public static Stack<GameObject> free = new Stack<GameObject>();

    public static GameObject Get() {
        GameObject trail = null;
        if (free.Count > 0) {
            trail = free.Pop();
        }
        else {
            trail = new GameObject("Trail");
            trail.AddComponent<TrailMesh>();
        }
        used.Add(trail);
        trail.SetActive(true);
        return trail;
    }

    public static void Release(GameObject m) {
        m.SetActive(false);
        used.Remove(m);
        free.Push(m);
    }

    public static void Clear() {
        used.Clear();
        free.Clear();
    }
}
