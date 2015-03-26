using UnityEngine;
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

} // End of MathUtilities.