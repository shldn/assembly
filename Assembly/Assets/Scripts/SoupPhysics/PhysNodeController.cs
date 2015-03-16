using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PhysNodeController : MonoBehaviour {

	public static PhysNodeController Inst;

	public Transform physNodePrefab = null;
	public Transform physFoodPrefab = null;
	MarchingCubes myCubes;

	int worldSize = 100;
	public int WorldSize {get{return worldSize;}}

	float[][][] densityMap;

	public static float physicsStep = 0.05f;

	int foodPellets = 200;


	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		CameraControl.Inst.maxRadius = worldSize * 3f;

		// Create random assemblies.
		int numAssemblies = 500;
		int minNodes = 3;
		int maxNodes = 15;

		for(int i = 0; i < numAssemblies; i++){
			Vector3 assemblySpawnPos = Vector3.zero;
			if(numAssemblies > 1)
				assemblySpawnPos = Random.insideUnitSphere * worldSize;

			PhysAssembly newAssembly = new PhysAssembly(assemblySpawnPos, Random.rotation);

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

			/*
			newAssembly.AddNode(new Triplet(0, 0, 0));
			newAssembly.AddNode(new Triplet(1, 0, 0));
			newAssembly.AddNode(new Triplet(2, 0, 0));
			newAssembly.AddNode(new Triplet(3, 0, 0));

			newAssembly.AddNode(new Triplet(0, 1, 0));
			newAssembly.AddNode(new Triplet(0, 2, 0));
			newAssembly.AddNode(new Triplet(0, 3, 0));

			newAssembly.AddNode(new Triplet(1, 1, 0));
			newAssembly.AddNode(new Triplet(2, 1, 0));
			newAssembly.AddNode(new Triplet(1, 2, 0));

			newAssembly.AddNode(new Triplet(3, 1, 0));
			newAssembly.AddNode(new Triplet(3, 2, 0));
			newAssembly.AddNode(new Triplet(3, 3, 0));
			newAssembly.AddNode(new Triplet(2, 3, 0));
			newAssembly.AddNode(new Triplet(1, 3, 0));

			newAssembly.AddNode(new Triplet(2, 2, 0));
			*/
		}


		for(int i = 0; i < foodPellets; i++){
			PhysFood newFood = new PhysFood(Random.insideUnitSphere * worldSize);
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

		// Quit on Escape
		if(Input.GetKeyUp(KeyCode.Escape))
			Application.Quit();

		PhysFood.AllFoodTree.Maintain();

		/*
		int cycleDir = Mathf.FloorToInt((Time.time * 0.2f) % 12);

		// Show details on selected assembly.
		PhysAssembly selectedAssem = CameraControl.Inst.selectedPhyAssembly;
		PhysAssembly hoveredAssem = CameraControl.Inst.hoveredPhysAssembly;
		if(selectedAssem){
			foreach(KeyValuePair<Triplet, PhysNode> kvp in selectedAssem.NodeDict){
				Triplet curPos = kvp.Key;
				PhysNode curNode = kvp.Value;
				// Render nodes
				GLDebug.DrawCube(selectedAssem.WorldPosition + HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(curPos, Mathf.FloorToInt(cycleDir))), HexUtilities.HexDirToRot(Mathf.FloorToInt(cycleDir)), Vector3.one, hoveredAssem ? Color.magenta : (Color.white), 0f, false);
				// Centerpoint
				GLDebug.DrawCube(selectedAssem.WorldPosition, Quaternion.identity, Vector3.one * 0.5f, Color.white, 0f, false);
			}

			// Determine closest fit with hovered assembly.
			if(hoveredAssem){
				Triplet[] testThisBuiltin = new Triplet[hoveredAssem.NodeDict.Keys.Count];
				Triplet[] againstThisBuiltin = new Triplet[selectedAssem.NodeDict.Keys.Count];
				hoveredAssem.NodeDict.Keys.CopyTo(testThisBuiltin, 0);
				selectedAssem.NodeDict.Keys.CopyTo(againstThisBuiltin, 0);

				int bestRotation;
				Triplet bestTranslation;
				SnugFit(testThisBuiltin, againstThisBuiltin, out bestRotation, out bestTranslation);
				foreach(KeyValuePair<Triplet, PhysNode> kvp in hoveredAssem.NodeDict){
					Triplet curPos = kvp.Key;
					PhysNode curNode = kvp.Value;
					// Render nodes
					GLDebug.DrawCube(selectedAssem.WorldPosition + HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(curPos, bestRotation) + bestTranslation), Quaternion.identity, Vector3.one, Color.cyan, 0f, false);
					// Center point
					GLDebug.DrawCube(selectedAssem.WorldPosition + HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(Triplet.zero, bestRotation) + bestTranslation), Quaternion.identity, Vector3.one * 0.5f, Color.white, 0f, false);
					GLDebug.DrawLine(selectedAssem.WorldPosition, selectedAssem.WorldPosition + HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(Triplet.zero, bestRotation) + bestTranslation), Color.white, 0f, false);
				}
			}
		}


		GLDebug.DrawLine(Vector3.zero, Vector3.forward, Color.blue);
		GLDebug.DrawLine(Vector3.zero, Vector3.right, Color.red);
		GLDebug.DrawLine(Vector3.zero, Vector3.up, Color.green);
		GLDebug.DrawCube(HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(new Triplet(2, 0, 0), Mathf.FloorToInt(Time.time % 12))), HexUtilities.HexDirToRot(Mathf.FloorToInt(Time.time % 12)));
		for(int i = 0; i < 12; i++){
			GLDebug.DrawCube(HexUtilities.HexToWorld(HexUtilities.HexRotateAxis(new Triplet(2, 0, 0), i)), HexUtilities.HexDirToRot(i), Vector3.one * 0.5f, Color.green);
		}
		print(Mathf.FloorToInt(Time.time % 12));
		*/
	} // End of Update().


	// Fits two closest-packed triplet structures together in all possible ways, and returns the fit with the highgest number of adjacencies.
	void SnugFit(Triplet[] testThis, Triplet[] againstThis, out int bestRotation, out Triplet bestTranslation){
		// Test from every direction.
		int maxTestDistance = 20;
		int bestNumAdjacencies = 0;

		// These will store our best distance and direction.
		bestRotation = 0;
		int bestDirection = 0;
		int bestDistance = 0;

		// Test for adjacencies by testing each 'throw' in this order:
		//   1. offset the test structure from its origin by some triplet.
		//   2. rotate the test structure around its axis by some direction.
		//   3. choose a direction from which to throw the test structure at the against structure.
		//   4. 'throw' the test structure at the 'against structure' by testing for overlap from decreasing distances.
		//        - If this succeeds at colliding at any point, we test adjacencies, then move on to the next direction
		//          from which to throw (3).
		for(int testRotation = 0; testRotation < 12; testRotation++){
			for(int testDirection = 0; testDirection < 12; testDirection++){
				bool collision = false;
				// Start far away, move closer until we collide.
				for(int testDistance = maxTestDistance; testDistance > -testDistance; testDistance--){
					// Current offset is based on distance, direction, and translation.
					Triplet curOffset = testDistance * HexUtilities.Adjacent(testDirection);
					// Test if any points overlap.
					foreach(Triplet someTestPoint in testThis){
						// Apply current offset to testPoint.
						Triplet curTestPoint = HexUtilities.HexRotateAxis(someTestPoint, testRotation) + curOffset;
						foreach(Triplet someAgainstPoint in againstThis){
							// If we have an overlap...
							if(curTestPoint.Equals(someAgainstPoint)){
								// We step back one unit of distance and test our adjacencies.
								// Rebuild our test array with the current offset.
								Triplet[] transformedTest = new Triplet[testThis.Length];
								for(int i = 0; i < transformedTest.Length; i++)
									transformedTest[i] = testThis[i] + ((testDistance + 1) * HexUtilities.Adjacent(testDirection));

								// Test the adjacencies, and if it's more than we've seen yet, this is our 'snuggest fist'.
								int numAdjacencies = GetNumAdjacencies(transformedTest, againstThis);
								if(numAdjacencies > bestNumAdjacencies){
									bestNumAdjacencies = numAdjacencies;

									bestRotation = testRotation;
									bestDirection = testDirection;
									bestDistance = testDistance + 1;
								}
								// We're done with this direction and can move on.
								collision = true;
								break; 
							}
						}
						if(collision){ // Done with direction...
							break;
						}
					}
					if(collision){ // Done with direction...
						break;
					}
				}
			}
		}

		bestTranslation = bestDistance * HexUtilities.Adjacent(bestDirection);
		print(bestNumAdjacencies + "  at direction " + bestDirection + " with rotation " + bestRotation);
	} // End of SnugFit().


	int GetNumAdjacencies(Triplet[] testThis, Triplet[] againstThis){
		int numAdjacencies = 0;
		// For every point A...
		foreach(Triplet someTestPoint in testThis){
			// ...take every point B...
			foreach(Triplet someAgainstPoint in againstThis){
				// ...and see if point A is neighboring point B.
				for(int dir = 0; dir < 12; dir++){
					if((someTestPoint + HexUtilities.Adjacent(dir)).Equals(someAgainstPoint)){
						numAdjacencies++;
						break; // Two points can't neighbor in more than one direction, so we can break.
					}
				}
			}
		}
		return numAdjacencies;
	} // End of GetNumAdjacencies().


} // End of PhysNodeController.
