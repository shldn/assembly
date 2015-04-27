﻿using UnityEngine;
using System.Collections;

public class MathUtilities {

	public static Vector3 RandomVector3Cube(float radius){
	    return new Vector3(Random.Range(-radius, radius),
                           Random.Range(-radius, radius),
                           Random.Range(-radius, radius));
	} // End of RandomVector3Cube().


    // Rotates a vector in a random direction by the deviation.
    public static Quaternion SkewRot(Quaternion rot, float deviation){
        Vector3 randomVect = Random.rotation * Vector3.forward;
        Vector3 randomPerpVect = Vector3.Cross(rot * Vector3.forward, randomVect);
        return rot * Quaternion.AngleAxis(deviation, randomPerpVect);
    } // End of DeviateVector().


    public static Rect CenteredSquare(float x, float y, float size){
        return new Rect(x - (size * 0.5f), Screen.height - (y + (size * 0.5f)), size, size);
    }

	/*
    public static Rect CenteredSquare(Node node){
        Vector3 nodeScreenPos = Camera.main.WorldToScreenPoint(node.worldPosition);
        return CenteredSquare(nodeScreenPos.x, nodeScreenPos.y, 2000f / Vector3.Distance(Camera.main.transform.position, node.worldPosition));
    }
    public static Rect CenteredSquare(Assembly assembly){
        Vector3 assemblyScreenPos = Camera.main.WorldToScreenPoint(assembly.physicsObject.transform.position);
        return CenteredSquare(assemblyScreenPos.x, assemblyScreenPos.y, 12000f / Vector3.Distance(Camera.main.transform.position, assembly.physicsObject.transform.position));
    }// End of CenteredSquare().
	*/


	public static Vector3[] FibonacciSphere(int samples, bool randomize = true){
		Vector3[] points = new Vector3[samples];

		float rnd = 1f;
		if(randomize)
			rnd = Random.Range(0f, samples);

		float offset = 2f / samples;
		float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

		for(int i = 0; i < samples; i++){
			float y = ((i * offset) - 1f) + (offset / 2f);
			float r = Mathf.Sqrt(1f - Mathf.Pow(y, 2f));
			float phi = ((i + rnd) % samples) * increment;
			float x = Mathf.Cos(phi) * r;
			float z = Mathf.Sin(phi) * r;
			points[i] = new Vector3(x, y, z);
		}

		return points;
	} // End of FibonacciSphere().


	/*
	def fibonacci_sphere(samples=1,randomize=True):
    rnd = 1.
    if randomize:
        rnd = random.random() * samples

    points = []
    offset = 2./samples
    increment = math.pi * (3. - math.sqrt(5.));

    for i in range(samples):
        y = ((i * offset) - 1) + (offset / 2);
        r = math.sqrt(1 - pow(y,2))

        phi = ((i + rnd) % samples) * increment

        x = math.cos(phi) * r
        z = math.sin(phi) * r

        points.append([x,y,z])

    return points
	*/

} // End of MathUtilities.