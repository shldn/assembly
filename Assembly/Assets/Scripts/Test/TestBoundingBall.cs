using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestBoundingBall {

    static public void RunTests() {

        if (!TestOnePoints())
            Debug.LogError("TestOnePoints Failed"); 
        if (!TestTwoPoints())
            Debug.LogError("TestTwoPoints Failed");
        if (!TestThreePoints())
            Debug.LogError("TestThreePoints Failed");
        if (!TestFourPoints())
            Debug.LogError("TestFourPoints Failed");
        if (!TestFivePoints())
            Debug.LogError("TestFivePoints Failed");

#if UNITY_EDITOR
        // Stop editor
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    static private bool TestOnePoints()
    {
        Vector3 center;
        float radius = 0;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(new Vector3(1, -1, 1));

        MathHelper.GetBoundingSphere(pts, out center, out radius);
        for (int i = 0; i < pts.Count; ++i)
            if (!MathHelper.IsInsideSphere(pts[i], center, radius))
                return false;
        return true;
    }

    static private bool TestTwoPoints()
    {
        Vector3 center;
        float radius = 0;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(new Vector3(0, 0, 0));
        pts.Add(new Vector3(0, 1, 0));

        MathHelper.GetBoundingSphere(pts, out center, out radius);
        for (int i = 0; i < pts.Count; ++i)
            if (!MathHelper.IsInsideSphere(pts[i], center, radius))
                return false;
        return true;
    }

    static private bool TestThreePoints()
    {
        Vector3 center;
        float radius = 0;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(new Vector3(0,0,0));
        pts.Add(new Vector3(0,1,0));
        pts.Add(new Vector3(0,2,0));

        MathHelper.GetBoundingSphere(pts, out center, out radius);
        for (int i = 0; i < pts.Count; ++i)
            if (!MathHelper.IsInsideSphere(pts[i], center, radius))
                return false;
        return true;
    }

    static private bool TestFourPoints()
    {
        Vector3 center;
        float radius = 0;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(new Vector3(0, 0, 0));
        pts.Add(new Vector3(0, 1, 0));
        pts.Add(new Vector3(0, 2, 0));
        pts.Add(new Vector3(0, 4, 0));

        MathHelper.GetBoundingSphere(pts, out center, out radius);
        for (int i = 0; i < pts.Count; ++i)
            if (!MathHelper.IsInsideSphere(pts[i], center, radius))
                return false;
        return true;

    }

    static private bool TestFivePoints()
    {
        Vector3 center;
        float radius = 0;

        List<Vector3> pts = new List<Vector3>();
        pts.Add(new Vector3(0, 0, 0));
        pts.Add(new Vector3(0, 1, 0));
        pts.Add(new Vector3(0, 2, 0));
        pts.Add(new Vector3(4, 2, 0));
        pts.Add(new Vector3(0, 0, -5));

        MathHelper.GetBoundingSphere(pts, out center, out radius);
        for (int i = 0; i < pts.Count; ++i)
            if (!MathHelper.IsInsideSphere(pts[i], center, radius + 0.00001f))
                return false;
        return true;

    }
}
