using UnityEngine;
using System.Collections;

public class BaseMapHack : MonoBehaviour {
	
	void Update () {
	    if (Terrain.activeTerrain.basemapDistance != 20000){
             Terrain.activeTerrain.basemapDistance = 20000;
         }
	} // End of Update().
} // End of BaseMapHack.
