﻿using UnityEngine;
using System.Collections;

public class PhysNodeController : MonoBehaviour {

	public Transform physNodePrefab = null;


	void Start(){ 
		// Create some random nodes adjacent to each other.
		IntVector3 spawnHexPos = IntVector3.zero;
		for(int i = 0; i < 150; i++){
			// Make sure no phys node is here currently.
			bool spaceOccupied = false;
			for(int j = 0; j < PhysNode.all.Count; j++){
				if(PhysNode.all[j].hexPos == spawnHexPos){
					spaceOccupied = true;
					break;
				}
			}
			if(!spaceOccupied){
				Transform newPhysNodeTrans = Instantiate(physNodePrefab, Vector3.zero, Quaternion.identity) as Transform;
				PhysNode newPhysNode = newPhysNodeTrans.GetComponent<PhysNode>();
				newPhysNode.hexPos = spawnHexPos;
				newPhysNode.transform.position = HexUtilities.HexToWorld(spawnHexPos);
			}
			spawnHexPos += HexUtilities.RandomAdjacent();
		}

		// Assign neighbors.
		for(int i = 0; i < PhysNode.all.Count; i++){
			PhysNode thisNode = PhysNode.all[i];
			for(int j = 0; j < PhysNode.all.Count; j++){
				PhysNode testNode = PhysNode.all[j];
				for(int dir = 0; dir < 12; dir++){
					if(testNode.hexPos == (thisNode.hexPos + HexUtilities.Adjacent(dir))){
						thisNode.AttachNeighbor(testNode, Quaternion.LookRotation(testNode.transform.position - thisNode.transform.position));
						break;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
