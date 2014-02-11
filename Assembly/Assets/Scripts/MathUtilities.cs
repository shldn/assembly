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
    public static Vector3 SkewVector(Vector3 vector, float deviation){
        Vector3 randomVect = Random.rotation * Vector3.forward;
        Vector3 randomPerpVect = Vector3.Cross(vector, randomVect);
        return Quaternion.LookRotation(vector) * Quaternion.AngleAxis(deviation, randomPerpVect) * Vector3.forward * vector.magnitude;
    } // End of DeviateVector().

} // End of MathUtilities.