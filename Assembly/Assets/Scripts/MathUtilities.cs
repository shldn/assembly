using UnityEngine;
using System.Collections;

public class MathUtilities {

	public static Vector3 RandomVector3Cube(float radius){
	    return new Vector3(Random.Range(-radius, radius),
                           Random.Range(-radius, radius),
                           Random.Range(-radius, radius));
	} // End of RandomVector3Cube().


    public static Vector3 RandomVector3Sphere(float radius){
        return Random.rotation * Vector3.forward * Random.Range(0f, radius);
	} // End of RandomVector3Cube().


    // Rotates a vector in a random direction by the deviation.
    public static Quaternion SkewRot(Quaternion rot, float deviation){
        Vector3 randomVect = Random.rotation * Vector3.forward;
        Vector3 randomPerpVect = Vector3.Cross(rot * Vector3.forward, randomVect);
        return rot * Quaternion.AngleAxis(deviation, randomPerpVect);
    } // End of DeviateVector().

} // End of MathUtilities.