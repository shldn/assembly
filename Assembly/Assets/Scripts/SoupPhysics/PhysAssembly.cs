﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysAssembly {

	static HashSet<PhysAssembly> all = new HashSet<PhysAssembly>();
	public static HashSet<PhysAssembly> getAll {get{return all;}}
	public static implicit operator bool(PhysAssembly exists){return exists != null;}

	Dictionary<Triplet, PhysNode> nodeDict = new Dictionary<Triplet, PhysNode>();
	public Dictionary<Triplet, PhysNode> NodeDict {get{return nodeDict;}}

	public Vector3 worldPosition = Vector3.zero;


	public PhysAssembly(Vector3 worldPosition){
		this.worldPosition = worldPosition;
		all.Add(this);
	} // End of PhysAssembly().


	public void AddNode(Triplet nodePos){
		PhysNode newPhysNode = new PhysNode(this, nodePos);
		nodeDict.Add(nodePos, newPhysNode);

		// Assign neighbors.
		for(int dir = 0; dir < 12; dir++){
			Triplet testPos = nodePos + HexUtilities.Adjacent(dir);
			if(nodeDict.ContainsKey(testPos))
				newPhysNode.AttachNeighbor(nodeDict[testPos]);
		}
	} // End of AddNode().


	public void RemoveNode(PhysNode nodeToRemove){
		if(nodeDict.ContainsKey(nodeToRemove.localHexPos))
			nodeDict.Remove(nodeToRemove.localHexPos);
	} // End of AddNode().


	public void Destroy(){
		foreach(KeyValuePair<Triplet, PhysNode> somePair in nodeDict)
			somePair.Value.Destroy();

		all.Remove(this);
	} // End of Destroy().

} // End of PhysAssembly.