using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assembly : CaptureObject{

	static List<Assembly> all = new List<Assembly>();
	public static List<Assembly> getAll {get{return all;}}
	public static implicit operator bool(Assembly exists){return exists != null;}

	public string name = "Some Unimportant Assembly";

	Dictionary<Triplet, Node> nodeDict = new Dictionary<Triplet, Node>();
	public Dictionary<Triplet, Node> NodeDict {get{return nodeDict;}}
	Node[] myNodesIndexed = new Node[0];

	public Vector3 spawnPosition = Vector3.zero;
	public Quaternion spawnRotation = Quaternion.identity;
	public Vector3 Position {
		get{
			Vector3 worldPos = Vector3.zero;
			foreach(Node someNode in nodeDict.Values)
				worldPos += someNode.Position;
			if(nodeDict.Keys.Count > 0f)
				worldPos /= nodeDict.Keys.Count;

			return worldPos;
		}
	}

	public float energy = 0f;
	float lastEnergy = 0f; // debug
	public bool cull = false;
	public bool needAddToList = true;

	public bool wantToMate = false;
	public Assembly matingWith = null;
	float mateAttractDist = 100f;
	float mateCompletion = 0f;

	public float distanceCovered = 0f;
	Vector3 lastPosition = Vector3.zero;

	private static Octree<Assembly> allAssemblyTree;
    public static Octree<Assembly> AllAssemblyTree{ 
        get{
            if(allAssemblyTree == null){
                allAssemblyTree = new Octree<Assembly>(new Bounds(Vector3.zero, 2.0f * NodeController.Inst.WorldSize * Vector3.one), (Assembly x) => x.Position, 5);
			}
            return allAssemblyTree;
        }
        set{
            allAssemblyTree = value;
        }
    }

    public static void DestroyAll()
    {
        all.Clear();
        allAssemblyTree = null;
    }

	public bool gender = false;


	public Assembly(Vector3 spawnPosition, Quaternion spawnRotation){
		this.spawnPosition = spawnPosition;
		this.spawnRotation = spawnRotation;
		gender = Random.Range(0f, 1f) > 0.5f;
		all.Add(this);
		AllAssemblyTree.Insert(this);
		PersistentGameManager.CaptureObjects.Add(this);
	} // End of constructor.

	// Load from string--file path, etc.
	public Assembly(string str, Quaternion? spawnRotation, Vector3? spawnPosition, bool isFilePath = false){
        List<Node> newNodes = new List<Node>();
        Vector3 worldPos = new Vector3();
        if(isFilePath)
            IOHelper.LoadAssemblyFromFile(str, ref name, ref worldPos, ref newNodes);
        else
            IOHelper.LoadAssemblyFromString(str, ref name, ref worldPos, ref newNodes);

		this.spawnPosition = spawnPosition ?? worldPos;
		this.spawnRotation = spawnRotation ?? Random.rotation;
		gender = Random.Range(0f, 1f) > 0.5f;
		all.Add(this);
		AllAssemblyTree.Insert(this);

        AddNodes(newNodes);
		foreach(Node someNode in NodeDict.Values)
			someNode.ComputeEnergyNetwork();

		PersistentGameManager.CaptureObjects.Add(this);
     } // End of constructor (from serialized).


	public void AddNodes(List<Node> nodesList){
        foreach (Node someNode in nodesList)
            AddNode(someNode.localHexPos, someNode.nodeProperties);

        // Destroy the duplicates
        for (int i = nodesList.Count - 1; i >= 0; --i)
            nodesList[i].Destroy();

            //IntegrateNode(someNode.localHexPos, someNode);
	} // End of AddNodes().


	public void AddNode(Triplet nodePos, NodeProperties? nodeProps = null){
		Node newPhysNode = new Node(this, nodePos);
		newPhysNode.nodeProperties = nodeProps ?? NodeProperties.random;
        IntegrateNode(nodePos, newPhysNode);
	} // End of AddNode().

    private void IntegrateNode(Triplet nodePos, Node physNode)
    {
        physNode.PhysAssembly = this;
        nodeDict.Add(nodePos, physNode);
        AssignNodeNeighbors(nodePos, physNode);
        energy += 1f;
    }

    private void AssignNodeNeighbors(Triplet nodePos, Node physNode)
    {
        for (int dir = 0; dir < 12; dir++)
        {
            Triplet testPos = nodePos + HexUtilities.Adjacent(dir);
            if (nodeDict.ContainsKey(testPos))
                physNode.AttachNeighbor(nodeDict[testPos]);
        }
    }


	public void RemoveNode(Node nodeToRemove){
		if(nodeDict.ContainsKey(nodeToRemove.localHexPos))
			nodeDict.Remove(nodeToRemove.localHexPos);
	} // End of AddNode().


	public void Update(){
		//MonoBehaviour.print(energy - lastEnergy);
		lastEnergy = energy;

		if(myNodesIndexed.Length != nodeDict.Values.Count){
			myNodesIndexed = new Node[nodeDict.Values.Count];
			nodeDict.Values.CopyTo(myNodesIndexed, 0);
		}

		if(PersistentGameManager.IsServer){
			if(energy < 0f)
				Destroy();
		}

		float maxEnergy = nodeDict.Values.Count * 2f;
		if(!PersistentGameManager.IsClient)
			energy = Mathf.Clamp(energy, 0f, maxEnergy);

		if((Node.getAll.Count < NodeController.Inst.worldNodeThreshold * 0.9f) && (energy > (maxEnergy * 0.9f)))
			wantToMate = true;
		else if((Node.getAll.Count > (NodeController.Inst.worldNodeThreshold * 0.8f)) || (energy < maxEnergy * 0.5f)){
			wantToMate = false;
			mateCompletion = 0f;

			if(matingWith){
				matingWith.matingWith = null;
				matingWith.wantToMate = false;
				matingWith.mateCompletion = 0f;
				matingWith.energy *= 0.5f;

				matingWith = null;
				wantToMate = false;
				mateCompletion = 0f;
				energy *= 0.5f;
			}
		}

		// We won't want to mate if our population cap has been reached.
		if(wantToMate && !matingWith){
			Bounds mateAttractBoundary = new Bounds(Position, mateAttractDist * (new Vector3(1, 1, 1)));
			allAssemblyTree.RunActionInRange(new System.Action<Assembly>(HandleFindMate), mateAttractBoundary);
		}

		if(matingWith){
			for(int i = 0; i < myNodesIndexed.Length; i++){
				Node myNode = myNodesIndexed[i];
				Node otherNode = null;
				if(matingWith.myNodesIndexed.Length > i){
					otherNode = matingWith.myNodesIndexed[i];

					GLDebug.DrawLine(myNode.Position, otherNode.Position, new Color(1f, 0f, 1f, 0.5f));
					Vector3 vectorToMate = myNode.Position - otherNode.Position;
					float distance = vectorToMate.magnitude;
			
					myNode.delayPosition += -(vectorToMate.normalized * Mathf.Clamp01(distance * 0.01f));

					if(distance < 2f)
						mateCompletion += 0.2f * NodeController.physicsStep;
				}
			}

			if(mateCompletion >= 1f){
				// Spawn a new assembly between the two.
				Assembly newAssembly = new Assembly((Position + matingWith.Position) / 2f, Random.rotation);
				int numNodes = Random.Range(myNodesIndexed.Length, matingWith.myNodesIndexed.Length);
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

				matingWith.matingWith = null;
				matingWith.wantToMate = false;
				matingWith.mateCompletion = 0f;
				matingWith.energy *= 0.5f;

				matingWith = null;
				wantToMate = false;
				mateCompletion = 0f;
				energy *= 0.5f;

				RandomMelody.Inst.PlayNote();
			}
		}

		distanceCovered += Vector3.Distance(lastPosition, Position);
		lastPosition = Position;
	} // End of Update().


	public void HandleFindMate(Assembly someAssembly){
		if((someAssembly == this) || (someAssembly.matingWith) || matingWith)
			return;

		Vector3 vectorToMate = someAssembly.Position - Position;
		float distanceToMate = vectorToMate.magnitude;

		if(distanceToMate > mateAttractDist)
			return;

		if(!someAssembly.wantToMate)
			return;

		matingWith = someAssembly;
		gender = true;

		someAssembly.matingWith = this;
		someAssembly.gender = false;
	} // End of HandleAttractMate().


	// Merges two assemblies together.
	public void AmaglamateTo(Assembly otherAssembly, Triplet offset){
		foreach(Node someNode in nodeDict.Values){
			otherAssembly.energy += 1f;
			someNode.PhysAssembly = otherAssembly;
			someNode.localHexPos += offset;
			while(someNode.PhysAssembly.nodeDict.ContainsKey(someNode.localHexPos)){
				someNode.localHexPos += HexUtilities.RandomAdjacent();
			}
			someNode.PhysAssembly.nodeDict.Add(someNode.localHexPos, someNode);

			for(int dir = 0; dir < 12; dir++){
				Triplet testPos = someNode.localHexPos + HexUtilities.Adjacent(dir);
				if(someNode.PhysAssembly.nodeDict.ContainsKey(testPos))
					someNode.AttachNeighbor(someNode.PhysAssembly.nodeDict[testPos]);
			}
		}

		nodeDict.Clear();
		Destroy();
	} // End of MergeWith().


	public void Mutate(float amount){
		foreach(Node someNode in NodeDict.Values)
			someNode.Mutate(amount);
	} // End of Mutate().


	public void Destroy(){
		foreach(KeyValuePair<Triplet, Node> somePair in nodeDict)
			somePair.Value.Destroy();

		allAssemblyTree.Remove(this);
		PersistentGameManager.CaptureObjects.Remove(this);
		cull = true;
	} // End of Destroy().


	public void Save(){
        string path = "./data/" + name + ".txt";
        Save(path);
    } // End of Save().


    public void Save(string path){
        ConsoleScript.Inst.WriteToLog("Saving " + path);
        IOHelper.SaveAssembly(path, this);
    } // End of Save().


	public string ToFileString(){
        return IOHelper.AssemblyToString(this);
    } // End of ToFileString().

} // End of PhysAssembly.