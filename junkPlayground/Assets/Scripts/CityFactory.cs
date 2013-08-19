using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityFactory {

    public float cityHeight = 0;
    private static CityFactory mInstance;
    public static CityFactory Inst
    {
        get {
            if (mInstance == null)
                mInstance = new CityFactory();
            return mInstance;
        }

    }
    private List<GameObject> cities = new List<GameObject>();
    public GameObject GetCity(float x, float z)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = new Vector3(x, cityHeight, z);
        sphere.renderer.material.color = Color.red;
        cities.Add(sphere);
        return sphere;
    }

    public void DeleteAll()
    {
        for (int i = 0; i < cities.Count; ++i)
            GameObject.Destroy(cities[i]);
        cities.Clear();
    }

}
