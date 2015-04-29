using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;


public class NodeController : MonoBehaviour {

	public static NodeController Inst;

	public Transform physNodePrefab = null;
	public Transform physFoodPrefab = null;
	MarchingCubes myCubes;

	int worldSize = 150;
	public int WorldSize {get{return worldSize;}}

	float[][][] densityMap;

	public static float physicsStep = 0.05f;

	int foodPellets = 150;
	public static int assemStage1Size = 20;

	int minNodes = 3;
	int maxNodes = 15;

	public int worldNodeThreshold = 1000;
    public bool showLeaderboard = false;

	int nextAssemblyID = 0;
	Dictionary<int, string> assemblyDictionary = new Dictionary<int, string>();
	Dictionary<int, string> assemblyNameDictionary = new Dictionary<int, string>();

	string[] nameList;

	List<Assembly> relativesToHighlight = new List<Assembly>();
	int currentLeaderIndex = 0;
	int lastLeaderIndex = 0;

	// Highest-'ranked' assemblies, based on the number of times the index shows up in hierarchies.
    static List<int> leaderboard = new List<int>(); // list of assembly ids -- will keep sorted by assemblyScores
    static int leaderboardMaxDisplaySize = 10;
    static int leaderboardMaxSize = leaderboardMaxDisplaySize + 5;  // The number of entries that will be stored in memory, store more to avoid searching if/when some fall off the leaderboard.
	static Dictionary<int, float> assemblyScores = new Dictionary<int, float>();  // maps assembly id to score

	void Awake(){
		Inst = this;
		bool isWindows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
        string lineEnding = isWindows ? "\r\n" : "\n";
        TextAsset maleNamesText = Resources.Load("Text/randomwords.txt") as TextAsset;
        nameList = maleNamesText.text.Split(new string[] { lineEnding }, System.StringSplitOptions.RemoveEmptyEntries);
	} // End of Awake().


	void Start(){
	} // End of Start().


	public string GetRandomName(){
		return nameList[Random.Range(0, nameList.Length)];
	} // End of GetRandomName().


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

		// Maintain octrees
		FoodPellet.AllFoodTree.Maintain();
		Node.AllSenseNodeTree.Maintain();
		Assembly.AllAssemblyTree.Maintain();

		int cycleDir = Mathf.FloorToInt((Time.time * 0.2f) % 12);

		// Show details on selected assembly.
		Assembly selectedAssem = CameraControl.Inst.selectedAssembly;
		Assembly hoveredAssem = CameraControl.Inst.hoveredPhysAssembly;
		if(selectedAssem){

			// debug
			if(Input.GetKeyDown(KeyCode.N))
				CameraControl.Inst.selectedAssembly.AddRandomNode();

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
				int newAssemID = GetNewAssemblyID();
				newAssembly.familyTree.Add(newAssemID);
                newAssembly.familyGenerationRemoved.Add(newAssemID, 0);
				assemblyDictionary.Add(newAssemID, newAssembly.ToString());
				assemblyNameDictionary.Add(newAssemID, newAssembly.name);
				assemblyScores.Add(newAssemID, 0);
                UpdateBirthCount(newAssemID,0);

				// Try a node structure... if there are no sense nodes, re-roll.
				bool containsSenseNode = false;
				do{
					foreach(Node someNode in newAssembly.NodeDict.Values)
						someNode.Destroy();
					newAssembly.NodeDict.Clear();

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
						if(someNode.neighbors.Count == 1){
							containsSenseNode = true;
							break;
						}
				}while(!containsSenseNode);

				foreach(Node someNode in newAssembly.NodeDict.Values)
					someNode.ComputeEnergyNetwork();
			}

			if(FoodPellet.all.Count < foodPellets){
				//Vector3 foodPosition = Random.insideUnitSphere * worldSize;

				float randomSeed = Random.Range(-100f, 100f);
				float radius = worldSize * 0.5f;
				float spiralIntensity = 0.1f;
				Vector3 foodPosition = new Vector3(Mathf.Sin(randomSeed * spiralIntensity) * radius, Mathf.Cos(randomSeed * spiralIntensity) * radius, randomSeed * 3f);

				new FoodPellet(foodPosition);
			}
		}


		// Leaderboard
		float timePerIndex = 3f; // How long each leaderboard entry is highlighted.
		currentLeaderIndex = Mathf.FloorToInt((Time.time / timePerIndex) % 10f);
		float fadeInLerp = (Time.time / timePerIndex) % 1f;
		float fadeAmount = 1f - (0.5f + (Mathf.Cos(fadeInLerp * (Mathf.PI * 2f)) * 0.5f));
		fadeAmount = Mathf.Pow(fadeAmount, 1f); // Make fade stronger.

		//print(lastLeaderIndex + "   " + currentLeaderIndex);
        if(lastLeaderIndex != currentLeaderIndex){
            lastLeaderIndex = currentLeaderIndex;
			relativesToHighlight = FindRelatives(leaderboard[currentLeaderIndex]);
        }


		for(int i = 0; i < relativesToHighlight.Count - 1; i++){
			GLDebug.DrawLine(relativesToHighlight[i].Position, relativesToHighlight[i + 1].Position, new Color(0f, 1f, 1f, fadeAmount));
		}

		/*
		foreach(Assembly someAssembly in relativesToHighlight){
			GLDebug.DrawLine(someAssembly.Position, Vector3.zero, new Color(0f, 1f, 1f, fadeAmount));
		}
		*/

        if( Input.GetKeyUp(KeyCode.L))
            showLeaderboard = !showLeaderboard;

	} // End of Update().


	List<Assembly> FindRelatives(Assembly assembly){
		List<Assembly> relatives = new List<Assembly>();
		foreach(int someInt in assembly.familyTree){
			foreach(Assembly someAssembly in Assembly.getAll){
				if(someAssembly.familyTree.Contains(someInt)){
					relatives.Add(someAssembly);
					continue;
				}
			}
		}

		return relatives;
	} // End of FindRelatives().


	List<Assembly> FindRelatives(int id){
		List<Assembly> relatives = new List<Assembly>();
		foreach(Assembly someAssembly in Assembly.getAll){
			if(someAssembly.familyTree.Contains(id)){
				relatives.Add(someAssembly);
				continue;
			}
		}

		return relatives;
	} // End of FindRelatives().


	public int GetNewAssemblyID(){
		return nextAssemblyID++;
	} // End of GetNewAssemblyID().


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

		/*
		string infoString = "";
		infoString += "Nodes: " + Node.getAll.Count + "\n";
		infoString += "Assemblies: " + Assembly.getAll.Count + "\n";
		infoString += "Framerate: " + (1f / Time.deltaTime).ToString("F1") + "\n";

		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		GUI.skin.label.fontSize = 12;
		GUI.Label(new Rect(10f, 10f, Screen.width - 20f, Screen.height - 20f), infoString);
		*/

		/*
		if(!PersistentGameManager.IsClient){
			foreach(Assembly someAssem in Assembly.getAll){

				Vector3 screenPos = Camera.main.WorldToScreenPoint(someAssem.Position);
				//screenPos.y = Screen.height - screenPos.y;
				if(screenPos.z < 0f)
					continue;

				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.skin.label.fontSize = Mathf.Clamp(Mathf.CeilToInt(20f / (screenPos.z * 0.01f)), 0, 50);
				

				if(relativesToHighlight.Contains(someAssem))
					GUI.color = Color.cyan;
				else
					GUI.color = Color.white;

				string familyString = "";
				foreach(int someInt in someAssem.familyTree)
					familyString += someInt + " ";
				GUI.Label(MathUtilities.CenteredSquare(screenPos.x, screenPos.y, 1000f), familyString);
			}
		}
		*/

        // Leaderboard
        if (PersistentGameManager.IsServer && showLeaderboard)
        {
            GUILayout.BeginArea(new Rect(10f, 10f, Screen.width, Screen.height));
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.04f);
            GUILayout.Label("Leaderboard", GUILayout.Height(Mathf.Max(GUI.skin.label.fontSize + 6, Mathf.CeilToInt(Screen.height * 0.05f))));
            GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.02f);
            int leaderCount = 0;
            foreach (int leaderEntry in leaderboard)
            {
                if (currentLeaderIndex == leaderCount)
                    GUI.color = Color.cyan;
                else
                    GUI.color = Color.white;

                GUILayout.Label("  " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(assemblyNameDictionary[leaderEntry]) + " - " + assemblyScores[leaderEntry].ToString("0.0"));

                ++leaderCount;
                if (leaderCount >= leaderboardMaxDisplaySize)
                    break;

            }
            GUILayout.EndArea();
        }

	} // End of OnGUI().


    void OnDestroy()
    {
        Inst = null;
        ClearLeaderboard();
        Assembly.DestroyAll();
        Node.DestroyAll();
        FoodPellet.DestroyAll();
        PersistentGameManager.CaptureObjects.Clear();
    }



    // Leader Board

    static int LeaderboardIndex(int assemblyID)
    {
        int idx = -1;
        for (int i = 0; i < leaderboard.Count && idx == -1; ++i)
            if (leaderboard[i] == assemblyID)
                idx = i;
        return idx;
    }

    // Clean up memory used for the leaderboard
    public static void ClearLeaderboard()
    {
        assemblyScores.Clear();
        leaderboard.Clear();
    }

    public static void UpdateBirthCount(int assemblyID, int generationRemoved)
    {
        float posGen = (generationRemoved < 0) ? 0f : (float)generationRemoved;
        assemblyScores[assemblyID] += 1f / (posGen + 1f);

        int idx = LeaderboardIndex(assemblyID);
        if (idx != -1)
        {
            // find new insert point
            int newIdx = -1;
            for (int i = 1; i <= idx && assemblyScores[leaderboard[idx - i]] < assemblyScores[assemblyID]; ++i)
                newIdx = idx - i;

            if (newIdx != -1)
            {
                leaderboard.RemoveAt(idx);
                leaderboard.Insert(newIdx, assemblyID);
            }
        }
        else
        {
            if (leaderboard.Count < leaderboardMaxSize || assemblyScores[leaderboard.Last<int>()] < assemblyScores[assemblyID])
            {
                // find insert point
                int insertIdx = -1;
                for (int i = 1; i <= leaderboard.Count && assemblyScores[leaderboard[leaderboard.Count - i]] < assemblyScores[assemblyID]; ++i)
                    insertIdx = leaderboard.Count - i;
                if (insertIdx != -1)
                    leaderboard.Insert(insertIdx, assemblyID);
                else
                    leaderboard.Add(assemblyID);
            }
            if (leaderboard.Count > leaderboardMaxSize)
                leaderboard.RemoveAt(leaderboard.Count - 1);
        }
    }

    public static void UpdateDeathCount(int assemblyID, int generationRemoved)
    {
        float posGen = (generationRemoved < 0) ? 0f : (float)generationRemoved;
        assemblyScores[assemblyID] -= 1f / (posGen + 1f);
        int idx = LeaderboardIndex(assemblyID);
        if (idx != -1)
        {
            if (assemblyScores[assemblyID] == 0)
                leaderboard.RemoveAt(idx);
            else
            {

                // find new insert point
                int newIdx = -1;
                for (int i = idx + 1; i < leaderboard.Count && assemblyScores[leaderboard[i]] > assemblyScores[assemblyID]; ++i)
                    newIdx = i;

                if (newIdx != -1)
                {
                    leaderboard.RemoveAt(idx);
                    leaderboard.Insert(newIdx, assemblyID);
                }
            }
        }

        if (assemblyScores[assemblyID] == 0)
            assemblyScores.Remove(assemblyID);

    }

} // End of PhysNodeController.
