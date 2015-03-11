using UnityEngine;
using System.Collections;

public class BaseMapHack : MonoBehaviour {
	
	void Update () {
		Terrain myTerrain = GetComponent<Terrain>();
	    if (myTerrain.basemapDistance != 20000){
             myTerrain.basemapDistance = 20000;
         }
	} // End of Update().
} // End of BaseMapHack.
