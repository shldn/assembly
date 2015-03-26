/*
using UnityEngine;
using System.Collections;

public class ReproducePullEffect : MonoBehaviour {

	public Assembly assemblyA = null;
    public Assembly assemblyB = null;
    
    public LineRenderer absorbLineRenderer = null;

    void Update(){

        if(!assemblyA || !assemblyA.targetMate || !assemblyA.physicsObject || !assemblyB || !assemblyB.gentlemanCaller || !assemblyB.physicsObject) {
            Destroy(gameObject);
        }

        float pointResolution = 0.5f;
        int numPoints = Mathf.CeilToInt(Vector3.Distance(assemblyA.WorldPosition, assemblyB.WorldPosition) * pointResolution);
        Vector3 vectorToNode = assemblyB.WorldPosition - assemblyA.WorldPosition;

        absorbLineRenderer.SetVertexCount(numPoints);
        for(int i = 0; i < numPoints;i++) {
            
            Vector3 truePoint = assemblyA.WorldPosition + (vectorToNode * ((float)i / numPoints));

            Quaternion spiralQuat = Quaternion.LookRotation(vectorToNode);
            spiralQuat *= Quaternion.AngleAxis(90, Vector3.up);

            float spiralRadius = 1.5f;
            float spiralStrength = 30f;
            float spiralSpeed = 200f;

            //spiralRadius *= 1f - ((float)i / (float)numPoints);
            spiralQuat *= Quaternion.AngleAxis((i * spiralStrength) - (Time.time * spiralSpeed), Vector3.right);
            absorbLineRenderer.SetPosition(i, truePoint + (spiralQuat * Vector3.forward * spiralRadius));
        }

    } // End of Update().

} // End of EnergyTransferEffect.
*/