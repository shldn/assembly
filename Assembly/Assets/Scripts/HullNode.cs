using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Used to test the ConvexHull algorithm
// When one of these nodes moves, the hull is recalculated.
public class HullNode : MonoBehaviour {

    static public List<HullNode> allNodes = new List<HullNode>();
    public ConvexHullViewer hull = null;
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
                hull.hull = new ConvexHull(nodePos);
            }
        }
	}

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
        // don't create a new hull
        lastPos = pos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }

    public static void CreateRandom(float bound = 10)
    {
        GameObject go = new GameObject("HullNode");
        go.transform.position = bound * Random.insideUnitSphere;
        HullNode hn = go.AddComponent<HullNode>();
        hn.hull = (ConvexHullViewer)UnityEngine.Object.FindObjectOfType(typeof(ConvexHullViewer));
    }

    public static void Create(Vector3 pos)
    {
        GameObject go = new GameObject("HullNode");
        HullNode hn = go.AddComponent<HullNode>();
        go.transform.position = pos;
        hn.lastPos = pos;
        hn.hull = (ConvexHullViewer)UnityEngine.Object.FindObjectOfType(typeof(ConvexHullViewer));
    }

    public static void CreateRandomSet(int number)
    {
        for (int i = 0; i < number; ++i)
            HullNode.CreateRandom(20);
        // force re-compute
        HullNode.allNodes[0].transform.position = HullNode.allNodes[0].transform.position + 0.01f * Vector3.up;
    }

    public static void PrintHullNodePositions()
    {
        string str = "";
        for (int i = 0; i < allNodes.Count; ++i)
            str += allNodes[i].transform.position.ToString() + "\n";
        Debug.LogError(str);
    }

    public static void CreateFromPtString()
    {
        string ptStr = @"(4.7, 17.3, 3.6)
(-12.6, -8.1, 1.2)
(16.7, -5.8, -7.6)
(11.9, -14.0, 4.8)
(-8.0, -1.7, 3.4)";

        string[] ptStrs = ptStr.Split('\n');

        ConvexHullViewer hullViewer = (ConvexHullViewer)UnityEngine.Object.FindObjectOfType(typeof(ConvexHullViewer));

        List<Vector3> nodePos = new List<Vector3>();
        for (int i = 0; i < ptStrs.Length; ++i)
        {
            Vector3 pos = IOHelper.Vector3FromString(ptStrs[i]);
            if (HullNode.allNodes.Count > i)
                HullNode.allNodes[i].SetPosition(pos);
            else
                HullNode.Create(pos);
            nodePos.Add(pos);
        }
        hullViewer.hull = new ConvexHull(nodePos);
    }
}
