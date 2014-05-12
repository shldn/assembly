using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Amalgam : MonoBehaviour {


	void Update(){
	
        if(Assembly.GetAll().Count > 4)
            UpdatePoints();
	} // End of Update().

    void UpdatePoints(){

        // get node positions
        List<Vector3> amalgamPoints = new List<Vector3>();

        foreach (Assembly assem in Assembly.GetAll()){
            amalgamPoints.Add(assem.WorldPosition);
        }

        // apply the convex hull to the mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        ConvexHull.UpdateMeshFromPoints(amalgamPoints, ref mesh);

	} // End of Start().
} // End of Amalgam.