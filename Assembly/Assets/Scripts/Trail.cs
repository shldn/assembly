using UnityEngine;
using System.Collections.Generic;

public class Trail {

    float minSqDistBtnPts = 0.1f * 0.1f;
    List<Vector3> pts = new List<Vector3>();
    public List<Vector3> Points{ get{ return pts;}}

    public Trail(float minDist)
    {
        minSqDistBtnPts = minDist * minDist;
    }

    public Trail(Vector3 pt, float minDist = 0.1f)
    {
        pts.Add(pt);
        minSqDistBtnPts = minDist * minDist;
    }

    public void Add(Vector3 pt)
    {
        if( pts.Count == 0 )
        {
            pts.Add(pt);
            return;
        }

        Vector3 lastPt = pts[pts.Count - 1];
        if ((lastPt - pt).sqrMagnitude >= minSqDistBtnPts)
            pts.Add(pt);
    }

    // Angles cannot be negative, so undulations will not cancel themselves out
    public float GetAbsRotation()
    {
        if (pts.Count <= 2)
            return 0f;

        float angle = 0f;
        for (int i = 2; i < pts.Count; ++i)
            angle += Vector3.Angle(pts[i - 1] - pts[i - 2], pts[i] - pts[i - 1]);

        return angle;
    }

    public float GetRotation()
    {
        if (pts.Count <= 2)
            return 0f;

        Vector3 initialUpDir = Vector3.Cross(pts[1] - pts[0], pts[2] - pts[1]);

        float angle = 0f;
        for(int i=2; i < pts.Count; ++i)
        {
            Vector3 v1 = pts[i - 1] - pts[i - 2];
            Vector3 v2 = pts[i] - pts[i - 1];
            Vector3 upDir = Vector3.Cross(v1, v2);
            float sign = Vector3.Angle(upDir, initialUpDir) > 90.0f ? -1f : 1f;
            angle += sign * Vector3.Angle(v1, v2);
        }

        return angle;
    }
}
