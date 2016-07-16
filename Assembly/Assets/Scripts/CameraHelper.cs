using UnityEngine;
using System.Collections;

public class CameraHelper {

    private static int cachedFrame = -1;
    private static Plane[] cachedFrustumPlanes;

    public static Plane[] GetMainCameraFrustumPlanes() {
        if (cachedFrame == Time.frameCount)
            return cachedFrustumPlanes;
        cachedFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        cachedFrame = Time.frameCount;
        return cachedFrustumPlanes;
    }
}
