using UnityEngine;
using System.Collections.Generic;

// ---------------------------------------------------------------------------------- //
// MathHelper.cs
//   -Wes Hawkins
//
// Contains handy functions that aren't covered by C# or Unity's libraries.
// ---------------------------------------------------------------------------------- //

public class MathHelper {

    // Spits out a unit vector in the X/Z plane in the direction of the given y angle (in degrees.)
    public static Vector3 MoveAngleUnitVector(float yAngle)
    {
        return new Vector3(Mathf.Cos((yAngle - 90) * Mathf.Deg2Rad), 0f, Mathf.Sin(-(yAngle - 90) * Mathf.Deg2Rad));

    } // End of MoveAngleUnitVector().

    public static float UnitVectorMoveAngle(Vector3 vector)
    {
        Quaternion direction = Quaternion.LookRotation(vector);
        return direction.eulerAngles.y;
    } // End of UnitVectorMoveAngle().

    // Determines if two rectangles intersect.
    public static bool Intersect( Rect a, Rect b )
    {
        bool c1 = a.x < b.xMax;
        bool c2 = a.xMax > b.x;
        bool c3 = a.y < b.yMax;
        bool c4 = a.yMax > b.y;
        return c1 && c2 && c3 && c4;
    } // End of Intersect().

    // returns distance between the points projected on the xz plane
    public static float Distance2D(Vector3 ptA, Vector3 ptB)
    {
        float diffX = (ptA.x - ptB.x);
        float diffZ = (ptA.z - ptB.z);
        return Mathf.Sqrt(diffX * diffX + diffZ * diffZ);
    }

    // Remaps a value in a range to another range
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax){
        return toMin + (((value - fromMin) / (fromMax - fromMin)) * (toMax - toMin));
    }

    // expects a string without the leading #   example: "242424"
    public static Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = (hex.Length > 6) ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)255;
        return new Color32(r, g, b, a);
    }

    // Get Bounding sphere for set of points. -- BoundingBall
    // Using Ritter's bounding sphere - coarse result, but efficient: http://en.wikipedia.org/wiki/Bounding_sphere
    public static void GetBoundingSphere(List<Vector3> pts, out Vector3 center, out float radius)
    {
        if (pts == null || pts.Count < 1)
        {
            center = Vector3.zero;
            radius = 0.0f;
            return;
        }

        Bounds bounds = new Bounds(pts[0],Vector3.zero);
        for (int i = 0; i < pts.Count; ++i)
            bounds.Encapsulate(pts[i]);


        int xIdx = 0;
        int yIdx = GetPtWithMaxDist(pts, pts[xIdx]);
        int zIdx = GetPtWithMaxDist(pts, pts[yIdx]);
        center = bounds.center;
        radius = Mathf.Max(Mathf.Max(bounds.extents.x, bounds.extents.y), bounds.extents.z);

        // Check if points are all in sphere.
        for(int i=0; i < pts.Count; ++i)
        {
            if( !IsInsideSphere(pts[i], center, radius) )
            {
                Vector3 vToNewEdge = pts[i] - center;
                float prevRadius = radius;
                radius = 0.5f * (radius + vToNewEdge.magnitude);
                center = center + (radius - prevRadius ) * vToNewEdge.normalized;
            }
        }
    }

    public static bool IsInsideSphere(Vector3 pt, Vector3 center, float radius)
    {
        return (pt - center).sqrMagnitude <= radius * radius;
    }

    private static int GetPtWithMaxDist(List<Vector3> pts, Vector3 testPt)
    {
        int idx = 0;
        float maxSqDist = -1f;
        for (int i = 0; i < pts.Count; ++i)
        {
            float sqDist = (pts[i] - testPt).sqrMagnitude;
            if( sqDist > maxSqDist )
            {
                maxSqDist = sqDist;
                idx = i;
            }
        }
        return idx;
    }

} // End of MathHelper class.
