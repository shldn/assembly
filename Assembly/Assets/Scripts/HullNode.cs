using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Used to test the ConvexHull algorithm
// When one of these nodes moves, the hull is recalculated.
public class HullNode : MonoBehaviour {

    static List<HullNode> allNodes = new List<HullNode>();
    public ConvexHull hull = null;
    Vector3 lastPos = new Vector3();

    void Awake()
    {
        allNodes.Add(this);
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
        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawCube(transform.position, new Vector3(1, 1, 1));
    }
}
