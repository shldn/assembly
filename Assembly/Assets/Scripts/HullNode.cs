using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Used to test the ConvexHull algorithm
// When one of these nodes moves, the hull is recalculated.
public class HullNode : MonoBehaviour {

    static public List<HullNode> allNodes = new List<HullNode>();
    public ConvexHull hull = null;
    Vector3 lastPos = new Vector3();
    public Color color = new Color(1, 0, 0, 0.5F);

    void Awake()
    {
        allNodes.Add(this);
        if( allNodes.Count > 1 )
            lastPos = transform.position;
    }

	void Update () {

        if (lastPos != transform.position)
        {
            lastPos = transform.position;
            if( hull != null )
            {
                List<Vector3> nodePos = new List<Vector3>();
                for (int i = 0; i < allNodes.Count; ++i)
                    nodePos.Add(allNodes[i].transform.position);
                hull.Insert(nodePos);
            }
        }
	}

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    public static void CreateRandom(float bound = 10)
    {
        GameObject go = new GameObject("HullNode");
        go.AddComponent<HullNode>();
        go.transform.position = bound * Random.insideUnitSphere;
    }

    public static void PrintHullNodePositions()
    {
        string str = "";
        for (int i = 0; i < allNodes.Count; ++i)
            str += allNodes[i].transform.position.ToString() + "\n";
        Debug.LogError(str);
    }
}
