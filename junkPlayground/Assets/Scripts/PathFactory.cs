using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFactory
{
    public float heightOffset = 0.1f;
    private static PathFactory mInstance;
    public static PathFactory Inst
    {
        get
        {
            if (mInstance == null)
                mInstance = new PathFactory();
            return mInstance;
        }

    }
    private List<GameObject> paths = new List<GameObject>();

    public void CreatePath(double[,] path, int citiesCount)
    {
        GameObject pathObj = new GameObject();
        LineRenderer line = pathObj.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Particles/Additive"));
        Color color = Color.cyan;
        color.a = 0.2f;
        line.SetColors(color, color);
        line.SetWidth(0.2f, 0.2f);
        line.SetVertexCount(citiesCount+1);
        for (int i = 0; i < citiesCount+1; ++i)
            line.SetPosition(i, new Vector3(TSPManager.scaleFactor * (float)path[i, 0], CityFactory.Inst.cityHeight + paths.Count * heightOffset, TSPManager.scaleFactor * (float)path[i, 1]));
        paths.Add(pathObj);
    }

    public void DeleteAll()
    {
        for (int i = 0; i < paths.Count; ++i)
            GameObject.Destroy(paths[i]);
        paths.Clear();
    }

}
