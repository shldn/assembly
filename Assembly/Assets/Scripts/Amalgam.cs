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
        // mesh from points with boundary info
        List<int> boundaryAssemblies = new List<int>();
        List<float> boundaryDistances = new List<float>();
        ConvexHull.UpdateMeshFromPointsWithInfo(amalgamPoints, ref mesh, ref boundaryAssemblies, ref boundaryDistances);

        //// example usage of boundary info
        //// set all assemblies white
        //for (int i = 0; i < Assembly.GetAll().Count; ++i) 
        //    Assembly.GetAll()[i].physicsObject.renderer.material.SetColor("_TintColor", new Color(1.0f, 1.0f, 1.0f, 1f));

        //// set all boundary assemblies red
        //for (int i = 0; i < boundaryAssemblies.Count; ++i)
        //    Assembly.GetAll()[boundaryAssemblies[i]].physicsObject.renderer.material.SetColor("_TintColor", new Color(1.0f, 0.05f, 0.07f, 1f));

	} // End of Start().
} // End of Amalgam.