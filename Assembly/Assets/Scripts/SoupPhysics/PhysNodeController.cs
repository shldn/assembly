using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysNodeController : MonoBehaviour {

	public static PhysNodeController Inst;

	public Transform physNodePrefab = null;
	MarchingCubes myCubes;

	int worldSize = 50;
	float[][][] densityMap;

	public static float physicsStep = 0.05f;


	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		CameraControl.Inst.maxRadius = worldSize * 3f;

		// Create random assemblies.
		int numAssemblies = 300;
		int minNodes = 3;
		int maxNodes = 15;

		for(int i = 0; i < numAssemblies; i++){
			Vector3 assemblySpawnPos = Vector3.zero;
			if(numAssemblies > 1)
				assemblySpawnPos = Random.insideUnitSphere * worldSize;

			PhysAssembly newAssembly = new PhysAssembly(assemblySpawnPos);

			int numNodes = Random.Range(minNodes, maxNodes);
			Triplet spawnHexPos = Triplet.zero;
			while(numNodes > 0){
				// Make sure no phys node is here currently.
				if(!newAssembly.NodeDict.ContainsKey(spawnHexPos)){
					newAssembly.AddNode(spawnHexPos);
					numNodes--;
				}
				spawnHexPos += HexUtilities.RandomAdjacent();
			}
		}


		// Marching cubes ------------------------------------------------- //
		// Initialize density map
		int densityMapSize = worldSize * 2;
		densityMap = new float[densityMapSize][][];
		for(int i = 0; i < densityMapSize; i++){
			densityMap[i] = new float[densityMapSize][];
			for(int j = 0; j < densityMapSize; j++){
				densityMap[i][j] = new float[densityMapSize];
			}
		}

	} // End of Start().

	void Update(){

		foreach(PhysNode someNode in PhysNode.getAll)
			someNode.DoMath();

		foreach(PhysNode someNode in PhysNode.getAll)
			someNode.UpdateTransform();

		PhysNode[] tempHoldNodes = new PhysNode[PhysNode.getAll.Count];
		PhysNode.getAll.CopyTo(tempHoldNodes);
		for(int i = 0; i < tempHoldNodes.Length; i++)
			if(tempHoldNodes[i].cull)
				PhysNode.getAll.Remove(tempHoldNodes[i]);
	} // End of Update().

} // End of PhysNodeController.
