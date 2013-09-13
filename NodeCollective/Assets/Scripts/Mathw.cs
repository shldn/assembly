using UnityEngine;
using System.Collections;

public class Mathw
{
    public static Vector2 XYToPos(Vector3 start, Vector3 end)
    {
        Vector2 returnAngles;
        returnAngles.y = Mathf.Atan2(end.x - start.x, end.z - start.z) * Mathf.Rad2Deg;
        returnAngles.x = Mathf.Atan2(end.y - start.y, Vector2.Distance(new Vector2(start.x, start.z), new Vector2(end.x, end.z))) * -Mathf.Rad2Deg;
        return returnAngles;
    }
}
