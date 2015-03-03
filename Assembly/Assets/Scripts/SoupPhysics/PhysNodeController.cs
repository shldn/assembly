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
		int numAssemblies = 100;
		int minNodes = 3;
		int maxNodes = 3;

		for(int i = 0; i < numAssemblies; i++){
			PhysAssembly newAssembly = new PhysAssembly(Random.insideUnitSphere * worldSize);

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

	} // End of Update().

} // End of PhysNodeController.
