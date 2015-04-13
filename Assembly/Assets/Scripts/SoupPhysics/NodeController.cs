using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NodeController : MonoBehaviour {

	public static NodeController Inst;

	public Transform physNodePrefab = null;
	public Transform physFoodPrefab = null;
	MarchingCubes myCubes;

	int worldSize = 150;
	public int WorldSize {get{return worldSize;}}

	float[][][] densityMap;

	public static float physicsStep = 0.05f;

	int foodPellets = 100;
	public static int assemStage1Size = 20;

	int minNodes = 3;
	int maxNodes = 15;

	public int worldNodeThreshold = 1000;


	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		CameraControl.Inst.maxRadius = worldSize * 3f;

		if(PersistentGameManager.IsServer){
			for(int i = 0; i < foodPellets; i++)
				new FoodPellet(Random.insideUnitSphere * worldSize);
		}
	} // End of Start().


	void Update(){

		foreach(Node someNode in Node.getAll)
			someNode.DoMath();

		foreach(Node someNode in Node.getAll)
			someNode.UpdateTransform();

		
		for(int i = 0; i < Assembly.getAll.Count; i++){
			Assembly curAssem = Assembly.getAll[i];
			if(curAssem.cull){
				Assembly.getAll.RemoveAt(i);
				i--;
			}else
				curAssem.Update();
		}

		foreach(FoodPellet someFood in FoodPellet.all)
			someFood.Update();

		// Culling
		Node[] tempHoldNodes = new Node[Node.getAll.Count];
		Node.getAll.CopyTo(tempHoldNodes);
		for(int i = 0; i < tempHoldNodes.Length; i++)
			if(tempHoldNodes[i].cull)
				Node.getAll.Remove(tempHoldNodes[i]);


		FoodPellet[] tempHoldFood = new FoodPellet[FoodPellet.all.Count];
		FoodPellet.all.CopyTo(tempHoldFood);
		for(int i = 0; i < tempHoldFood.Length; i++)
			if(tempHoldFood[i].cull)
				tempHoldFood[i].Destroy();


		// Quit on Escape
		if(Input.GetKeyUp(KeyCode.Escape))
			Application.Quit();

		// Maintain octrees
		FoodPellet.AllFoodTree.Maintain();
		Node.AllSenseNodeTree.Maintain();
		Assembly.AllAssemblyTree.Maintain();

		int cycleDir = Mathf.FloorToInt((Time.time * 0.2f) % 12);

		// Show details on selected assembly.
		Assembly selectedAssem = CameraControl.Inst.selectedPhysAssembly;
		Assembly hoveredAssem = CameraControl.Inst.hoveredPhysAssembly;
		if(selectedAssem){
			/*
			foreach(KeyValuePair<Triplet, PhysNode> kvp in selectedAssem.NodeDict){
				Triplet curPos = kvp.Key;
				PhysNode curNode = kvp.Value;
				// Render nodes
				GLDebug.DrawCube(selectedAssem.spawnPosition + HexUtilities.HexToWorld(curPos), Quaternion.identity, Vector3.one * 0.5f, kvp.Value.cubeTransform.renderer.material.color + new Color(0.1f, 0.1f, 0.1f), 0f, false);
				// Centerpoint
				//GLDebug.DrawCube(selectedAssem.WorldPosition, Quaternion.identity, Vector3.one * 0.5f, Color.white, 0f, false);
			}*/

			// Duplicate assembly using string IO methods
			if(Input.GetKey(KeyCode.D)){
				new Assembly(IOHelper.AssemblyToString(selectedAssem), null, null, false);
			}
		}/*
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

		// Keep the world populated
		if(PersistentGameManager.IsServer){
			if(Node.getAll.Count < worldNodeThreshold * 0.7f){
				Vector3 assemblySpawnPos = Random.insideUnitSphere * worldSize;

				Assembly newAssembly = new Assembly(assemblySpawnPos, Quaternion.identity);

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

				foreach(Node someNode in newAssembly.NodeDict.Values)
					someNode.ComputeEnergyNetwork();
			}
		}
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


	void OnGUI(){
		return;

		string infoString = "";
		infoString += "Nodes: " + Node.getAll.Count + "\n";
		infoString += "Assemblies: " + Assembly.getAll.Count + "\n";
		infoString += "Framerate: " + (1f / Time.deltaTime).ToString("F1") + "\n";

		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUI.skin.label.fontSize = 12;
		GUI.Label(new Rect(10f, 10f, Screen.width - 20f, Screen.height - 20f), infoString);

	} // End of OnGUI().

    void OnDestroy()
    {
        Inst = null;
        Assembly.DestroyAll();
        Node.DestroyAll();
        FoodPellet.DestroyAll();
        PersistentGameManager.CaptureObjects.Clear();
    }

} // End of PhysNodeController.
