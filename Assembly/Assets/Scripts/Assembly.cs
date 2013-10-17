using UnityEngine;
using System.Collections;

public class Assembly {

    public string callsign;
    public Node[] nodes;

    public Vector3 GetCenter(){
        Vector3 totalPos = Vector3.zero;
        for(int i = 0; i < nodes.Length; i++) {
            totalPos += nodes[i].transform.position;
        }
        totalPos /= nodes.Length;
        return totalPos;
    }

    public float GetRadius(){
        float greatestRad = 0;
        Vector3 center = GetCenter();
        for(int i = 0; i < nodes.Length; i++){
            float radius = Vector3.Distance(center, nodes[i].transform.position);
            if(radius > greatestRad)
                greatestRad = radius;
        }
        return greatestRad;
    }
}
