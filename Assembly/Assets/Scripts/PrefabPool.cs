using UnityEngine;
using System.Collections.Generic;

public class PrefabPool {

    public List<GameObject> used = new List<GameObject>();
    public Stack<GameObject> free = new Stack<GameObject>();
    public GameObject prefab;
    public GameObject Get() {
        GameObject obj = null;
        if (free.Count > 0) {
            obj = free.Pop();
        }
        else {
            obj = MonoBehaviour.Instantiate(prefab) as GameObject;
        }
        used.Add(obj);
        obj.SetActive(true);
        return obj;
    }

    public void Release(GameObject obj) {
        obj.SetActive(false);
        used.Remove(obj);
        free.Push(obj);
    }

    public void Clear() {
        used.Clear();
        free.Clear();
    }

}
