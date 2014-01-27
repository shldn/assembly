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

} // End of MathUtilities.
