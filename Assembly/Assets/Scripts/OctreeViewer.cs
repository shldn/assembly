using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OctreeViewer : MonoBehaviour{

    private static OctreeViewer mInst = null;
    public static OctreeViewer Inst
    {
        get
        {
            if (mInst == null)
                mInst = (new GameObject("OctViewer")).AddComponent<OctreeViewer>();
            return mInst;
        }
    }

    private List<Bounds> bounds = new List<Bounds>();

    public void AddBounds(Bounds b)
    {
        bounds.Add(b);
    }

    void OnDrawGizmos()
    {
        for(int i=0; i < bounds.Count; ++i)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(bounds[i].center, bounds[i].size);
        }
    }

    void OnDestroy()
    {
        mInst = null;
    }
}
