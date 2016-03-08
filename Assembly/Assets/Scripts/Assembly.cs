using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VR;

public class Assembly : CaptureObject{

	static List<Assembly> all = new List<Assembly>();
	public static List<Assembly> getAll {get{return all;}}
    public static HashSet<int> captured = new HashSet<int>(); // contains assembly ids of the assemblies that have been captured by users.
    public static Dictionary<int, HashSet<int>> cachedFamilyTrees = new Dictionary<int, HashSet<int>>();
    public static Dictionary<int, Dictionary<int, int>> cachedFamilyGenerationsRemoved = new Dictionary<int, Dictionary<int, int>>();
	public static implicit operator bool(Assembly exists){return exists != null;}

	public HashSet<int> familyTree = new HashSet<int>();
    public Dictionary<int, int> familyGenerationRemoved = new Dictionary<int, int>(); // how many generations removed is an entity in familyTree (maps assemblyID to generationCount) - parents would have value of 1
    public AssemblyProperties properties = new AssemblyProperties();
    bool propertiesDirty = false;
    public bool userReleased = false; // Did a user just drop this assembly into the world?
    public bool isOffspring = false;
    private int id = -1;
    public int Id { 
        get { return id; } 
        set { 
            id = value;
            properties.id = id;
            if( id != -1 )
            {
                familyTree.Add(value);
                familyGenerationRemoved.Add(value, 0);
                NodeController.Inst.assemblyNameDictionary.Add(value, Name);
                NodeController.assemblyScores.Add(value, 0);
            }
        } 
    }
    public string Name { get { return properties.name; } set { properties.name = value; } }
    

	Dictionary<Triplet, Node> nodeDict = new Dictionary<Triplet, Node>();
	public Dictionary<Triplet, Node> NodeDict {get{return nodeDict;}}
    List<Node> nodes = new List<Node>();
    public List<Node> Nodes { get { return nodes; } }


	public Vector3 spawnPosition = Vector3.zero;
	public Quaternion spawnRotation = Quaternion.identity;
    private Vector3 cachedPosition = Vector3.zero;
    private int cachedFrame = -1;
	public Vector3 Position {
		get{
            if (cachedFrame == Time.frameCount)
                return cachedPosition;
			Vector3 worldPos = Vector3.zero;
			foreach(Node someNode in nodeDict.Values)
				worldPos += someNode.Position;
			if(nodeDict.Keys.Count > 0f)
				worldPos /= nodeDict.Keys.Count;

            cachedFrame = Time.frameCount;
            cachedPosition = worldPos;
			return worldPos;
		}
	}
    public Vector3 Velocity {
        get { return velocity; }
    }

	public float energy = 0f;
	float lastEnergy = 0f; // debug
	public bool cull = false;
	public bool needAddToList = true;
    public bool hasBeenCaptured = false;
	public float Health {get{return energy / nodeDict.Count;}}

	private Assembly matingWith = null;
    public Assembly MatingWith {
        get { return matingWith; }
        set {
            int prevId = properties.matingWith;
            properties.matingWith = (value != null) ? value.Id : -1;
            if (matingWith != value) {
                propertiesDirty = true;
            }
            matingWith = value;
        }
    }
    public bool WantToMate {
        get { return properties.wantToMate; }
        set {
            if (value != properties.wantToMate)
                propertiesDirty = true;
            properties.wantToMate = value;
        }
    }
	float mateAttractDist = 100f;
	float mateCompletion = 0f;

	public float distanceCovered = 0f;
	Vector3 lastPosition = Vector3.zero;
    Vector3 velocity = Vector3.zero;

	public Amalgam amalgam = null;

	bool pushedToClients = false;

	public bool ready = false; // Turns true after having survived an Update() step.

	private static Octree<Assembly> allAssemblyTree;
    public static Octree<Assembly> AllAssemblyTree{ 
        get{
            if(allAssemblyTree == null){
                allAssemblyTree = new Octree<Assembly>(new Bounds(Vector3.zero, 2.0f * NodeController.Inst.maxWorldSize * Vector3.one), (Assembly x) => x.Position, 5);
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
        captured.Clear();
        allAssemblyTree = null;
    }

    public static Assembly Get(int id) {
        for (int i = 0; i < getAll.Count; ++i)
            if (getAll[i].id == id)
                return getAll[i];
        return null;
    }


	private Assembly(Vector3 spawnPosition, Quaternion spawnRotation, int numNodes, string name = "", bool isOffspring_ = false){
		this.spawnPosition = spawnPosition;
		this.spawnRotation = spawnRotation;
		properties.gender = Random.Range(0f, 1f) > 0.5f;
		all.Add(this);
		AllAssemblyTree.Insert(this);
		Name = (name == "" ? NodeController.Inst.GetRandomName() : name);
        Id = NodeController.Inst.GetNewAssemblyID();
        isOffspring = isOffspring_;
        AddRandomNodes(numNodes);

        foreach (Node someNode in NodeDict.Values)
            someNode.ComputeEnergyNetwork();


        if (!PersistentGameManager.EmbedViewer) {
            ViewerData.Inst.assemblyCreations.Add(new AssemblyCreationData(this));
            ViewerData.Inst.assemblyUpdates.Add(new AssemblyTransformUpdate(this));
        }
        else {
            PersistentGameManager.CaptureObjects.Add(this);
            if (isOffspring && RandomMelody.Inst)
                RandomMelody.Inst.PlayNote();
        }
    } // End of constructor.


    // Load from string--file path, etc.
    public Assembly(string str, Quaternion? spawnRotation, Vector3? spawnPosition, bool isFilePath = false, bool userReleased_ = false){
        List<Node> newNodes = new List<Node>();
        Vector3 worldPos = new Vector3();
        if(isFilePath)
            IOHelper.LoadAssemblyFromFile(str, ref properties.name, ref id, ref worldPos, ref newNodes);
        else
            IOHelper.LoadAssemblyFromString(str, ref properties.name, ref id, ref worldPos, ref newNodes);
        properties.id = id;

        hasBeenCaptured = !isFilePath && id != -1;
        if( hasBeenCaptured )
            captured.Add(Id);
        if( PersistentGameManager.IsServer )
        {
            RebuildFamilyTree();
            if( hasBeenCaptured )
                foreach (int someInt in familyTree)
                    NodeController.UpdateBirthCount(someInt, familyGenerationRemoved[someInt]);
        }

		this.spawnPosition = spawnPosition ?? worldPos;
		this.spawnRotation = spawnRotation ?? Random.rotation;
		properties.gender = Random.Range(0f, 1f) > 0.5f;
		all.Add(this);
		AllAssemblyTree.Insert(this);

        AddNodes(newNodes);
		foreach(Node someNode in NodeDict.Values)
			someNode.ComputeEnergyNetwork();

		PersistentGameManager.CaptureObjects.Add(this);
        userReleased = userReleased_;
        if (!PersistentGameManager.EmbedViewer) {
            ViewerData.Inst.assemblyCreations.Add(new AssemblyCreationData(this));
            ViewerData.Inst.assemblyUpdates.Add(new AssemblyTransformUpdate(this));
        }
    } // End of constructor (from serialized).


	public static Assembly RandomAssembly(Vector3 spawnPos, Quaternion rotation, int numNodes){
		Assembly newAssembly = new Assembly(spawnPos, rotation, numNodes);
        NodeController.UpdateBirthCount(newAssembly.Id, 0);
		return newAssembly;
	} // End of RandomAssembly().

    void AddRandomNodes(int numNodes, bool forceSenseNode = true) {
        // Try a node structure... if there are no sense nodes, re-roll.
        bool containsSenseNode = false;
        do {
            RemoveAllNodes();
            Triplet spawnHexPos = Triplet.zero;
            int nodesToMake = numNodes;
            while (nodesToMake > 0) {
                // Make sure no phys node is here currently.
                if (!NodeDict.ContainsKey(spawnHexPos)) {
                    AddNode(spawnHexPos);
                    nodesToMake--;
                }
                spawnHexPos += HexUtilities.RandomAdjacent();
            }

            foreach (Node someNode in NodeDict.Values) {
                if (someNode.neighbors.Count == 1) {
                    containsSenseNode = true;
                    break;
                }
            }
        } while (forceSenseNode && !containsSenseNode);
    }

	public void AddNodes(List<Node> nodesList){
        foreach (Node someNode in nodesList)
            AddNode(someNode.localHexPos, someNode.Properties);

        // Destroy the duplicates
        for (int i = nodesList.Count - 1; i >= 0; --i)
            nodesList[i].Destroy();

            //IntegrateNode(someNode.localHexPos, someNode);
	} // End of AddNodes().


	public Node AddNode(Triplet nodePos, NodeProperties? nodeProps = null){
		Node newPhysNode = new Node(this, nodePos);
		newPhysNode.Properties = nodeProps ?? NodeProperties.random;
        IntegrateNode(nodePos, newPhysNode);
		return newPhysNode;
	} // End of AddNode().


    private void IntegrateNode(Triplet nodePos, Node physNode)
    {
        physNode.PhysAssembly = this;
        nodeDict.Add(nodePos, physNode);
        nodes.Add(physNode);
        AssignNodeNeighbors(nodePos, physNode);
        energy += 1f;
    }


    private void AssignNodeNeighbors(Triplet nodePos, Node physNode)
    {
        for (int dir = 0; dir < 12; dir++)
        {
            Triplet testPos = nodePos + HexUtilities.Adjacent(dir);
            if (nodeDict.ContainsKey(testPos)){
                physNode.AttachNeighbor(nodeDict[testPos]);
                nodeDict[testPos].AttachNeighbor(physNode);
			}
        }
    }


	public void RemoveNode(Node nodeToRemove){
		if(nodeDict.ContainsKey(nodeToRemove.localHexPos))
        {
            nodeDict.Remove(nodeToRemove.localHexPos);
            nodes.Remove(nodeToRemove);
        }
	} // End of AddNode().


	public Assembly Duplicate(){
		return new Assembly(ToString(), spawnRotation, Position);
	} // End of Duplicate().


	public void Update(){
		lastEnergy = energy;

		if(PersistentGameManager.IsServer){
			if(energy < 0f)
				Destroy();
		}

		float maxEnergy = nodeDict.Values.Count * 2f;
		if(!PersistentGameManager.IsClient)
			energy = Mathf.Clamp(energy, 0f, maxEnergy);

        if (!PersistentGameManager.IsClient && (Node.getAll.Count < NodeController.Inst.worldNodeThreshold * 0.9f) && (energy > (maxEnergy * 0.9f)))
            WantToMate = true;
        else if ((Node.getAll.Count > (NodeController.Inst.worldNodeThreshold * 0.8f)) || (energy < maxEnergy * 0.5f) && (Random.Range(0f, 1f) < 0.01f)){
            WantToMate = false;
			mateCompletion = 0f;

			if(MatingWith){

                MatingWith.WantToMate = false;
				MatingWith.mateCompletion = 0f;
				MatingWith.energy *= 0.5f;
                MatingWith.MatingWith = null;

                WantToMate = false;
				mateCompletion = 0f;
				energy *= 0.5f;
                MatingWith = null;
            }
        }

		// We won't want to mate if our population cap has been reached.
        if (properties.wantToMate && !MatingWith){
			Bounds mateAttractBoundary = new Bounds(Position, mateAttractDist * (new Vector3(1, 1, 1)));
			allAssemblyTree.RunActionInRange(new System.Action<Assembly>(HandleFindMate), mateAttractBoundary);
		}

		if(MatingWith){
			for(int i = 0; i < nodes.Count; i++){
				Node myNode = nodes[i];
				Node otherNode = null;
                if (MatingWith.nodes.Count > i) {
					otherNode = MatingWith.nodes[i];

					//if((!VRDevice.isPresent) && myNode.Visible && otherNode.Visible)
					//	GLDebug.DrawLine(myNode.Position, otherNode.Position, new Color(1f, 0f, 1f, 0.5f));
					Vector3 vectorToMate = myNode.Position - otherNode.Position;
					float distance = vectorToMate.magnitude;
			
					myNode.delayPosition += -(vectorToMate.normalized * Mathf.Clamp01(distance * 0.01f));

					if(distance < 2f)
						mateCompletion += 0.2f * NodeController.physicsStep;
				}
			}

			// Create offspring!
			if(mateCompletion >= 1f){
				// Spawn a new assembly between the two.
				int numNodes = Random.Range(nodes.Count, MatingWith.nodes.Count + 1);
                string name = Name.Substring(0, Mathf.RoundToInt(Name.Length * 0.5f)) + MatingWith.Name.Substring(MatingWith.Name.Length - Mathf.RoundToInt(MatingWith.Name.Length * 0.5f), Mathf.RoundToInt(MatingWith.Name.Length * 0.5f));
                Assembly newAssembly = new Assembly((Position + MatingWith.Position) / 2f, Random.rotation, numNodes, name, true);

                // Update family trees
                newAssembly.UpdateFamilyTreeFromParent(this);
                newAssembly.UpdateFamilyTreeFromParent(MatingWith);
				newAssembly.amalgam = amalgam;
				if(newAssembly.amalgam)
					newAssembly.amalgam.assemblies.Add(newAssembly);

                //Debug.LogError("Baby assembly born: family size: " + familyTree.Count);

                // Mating is over :/
                MatingWith.WantToMate = false;
				MatingWith.mateCompletion = 0f;
				MatingWith.energy *= 0.5f;
                MatingWith.MatingWith = null;

                WantToMate = false;
				mateCompletion = 0f;
				energy *= 0.5f;
                MatingWith = null;
			}
		}

        Vector3 newPosition = Position;
        distanceCovered += Vector3.Distance(lastPosition, newPosition);
        velocity = (newPosition - lastPosition) / Time.deltaTime;
        lastPosition = newPosition;

        // Keeps assemblies in Capsule
        if (Environment.Inst && !PersistentGameManager.IsClient && !WorldSizeController.Inst.WithinBoundary(Position)) {
            foreach (Node someNode in nodeDict.Values) {
                Vector3 dir = (WorldSizeController.Inst.WorldOrigin - Position).normalized;
                someNode.velocity += dir * 2f * NodeController.physicsStep;
            }
        }

		// Send new assembly to clients.
		if((Network.peerType == NetworkPeerType.Server) && !pushedToClients){
			pushedToClients = true;
			if(Id == -1){
				int newAssemID = NodeController.Inst.GetNewAssemblyID();
				Id = newAssemID;
			}
			AssemblyRadar.Inst.GetComponent<NetworkView>().RPC("CreateBlip", RPCMode.Others, IOHelper.AssemblyToString(this), Position);
		}

        if (propertiesDirty) {
            ViewerData.Inst.assemblyPropertyUpdates.Add(new AssemblyProperties(properties));
            propertiesDirty = false;
        }


        ready = true;

	} // End of Update().


    public void ThrowAwayFromCamera() {
        foreach (Node someNode in NodeDict.Values)
            someNode.velocity = (Camera.main.transform.forward * 3f) + Random.insideUnitSphere * 1.5f;
    }

    private void UpdateFamilyTreeFromParent(Assembly parent)
    {
        foreach (int someInt in parent.familyTree)
        {
            familyTree.Add(someInt);
            if (familyGenerationRemoved.ContainsKey(someInt))
                familyGenerationRemoved[someInt] = Mathf.Min(familyGenerationRemoved[someInt], parent.familyGenerationRemoved[someInt] + 1);
            else
                familyGenerationRemoved.Add(someInt, parent.familyGenerationRemoved[someInt] + 1);
            NodeController.UpdateBirthCount(someInt, familyGenerationRemoved[someInt]);
        }
    }

    // Captured assembly is released back into the world, retrieve its cached family tree
    private void RebuildFamilyTree()
    {
        if( id == -1 && Debug.isDebugBuild )
        {
            Debug.LogError("Attempting to rebuild family tree for an invalid index");
            return;
        }

		if( cachedFamilyTrees.ContainsKey(id) )
		{
	        familyTree = cachedFamilyTrees[id];
		    familyGenerationRemoved = cachedFamilyGenerationsRemoved[id];
			
			cachedFamilyTrees.Remove(id);
			cachedFamilyGenerationsRemoved.Remove(id);
		}
    }

    // When an assembly is captured, server saves its family tree until its return
    public void SaveFamilyTree()
    {
        if( id == -1 )
            Id = NodeController.Inst.GetNewAssemblyID();
        cachedFamilyTrees.Add(id, familyTree);
        cachedFamilyGenerationsRemoved.Add(id, familyGenerationRemoved);
    }


	public void HandleFindMate(Assembly someAssembly){
		if((someAssembly == this) || (someAssembly.MatingWith) || MatingWith)
			return;

		Vector3 vectorToMate = someAssembly.Position - Position;
		float distanceToMate = vectorToMate.magnitude;

		if(distanceToMate > mateAttractDist)
			return;

        if (!someAssembly.properties.wantToMate)
			return;

        properties.gender = true;
        MatingWith = someAssembly;

        someAssembly.properties.gender = false;
        someAssembly.MatingWith = this;
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
            someNode.PhysAssembly.nodes.Add(someNode);

			for(int dir = 0; dir < 12; dir++){
				Triplet testPos = someNode.localHexPos + HexUtilities.Adjacent(dir);
				if(someNode.PhysAssembly.nodeDict.ContainsKey(testPos))
					someNode.AttachNeighbor(someNode.PhysAssembly.nodeDict[testPos]);
			}
		}

        RemoveAllNodes(false);
		Destroy();
	} // End of MergeWith().


	public void Mutate(float amount){
		foreach(Node someNode in NodeDict.Values)
			someNode.Mutate(amount);
	} // End of Mutate().


	public void AddRandomNode(){
		int randomStart = Random.Range(0, NodeDict.Count);
		for(int index = 0; index < NodeDict.Count; index++) {
			KeyValuePair<Triplet, Node> item = NodeDict.ElementAt((index + randomStart) % NodeDict.Count);
			Triplet structurePos = item.Key;

			int randomDir = Random.Range(0, 12);
			for(int i = 0; i < 12; i++){
				Triplet testPos = structurePos + HexUtilities.Adjacent((randomDir + i) % 12);
				// If this position is not filled, we have our position.
				if(!NodeDict.Keys.Contains(testPos)){
					Node newNode = AddNode(testPos);
					MonoBehaviour.print(newNode.localHexPos);
					return;
				}
			}
		}
	} // End of AddRandomNode().

    public void RemoveAllNodes(bool destroy = true) {
        if(destroy)
            foreach (Node someNode in NodeDict.Values)
                someNode.Destroy();

        nodeDict.Clear();
        nodes.Clear();
    }

	public void RemoveRandomNode(){

	} // End of RemoveRandomNode().

    public void SetVisibility(bool vis)
    {
        foreach (KeyValuePair<Triplet, Node> node in NodeDict)
            node.Value.Visible = vis;
    } // End of SetVisibility().


	public void Destroy(){
		foreach(KeyValuePair<Triplet, Node> somePair in nodeDict)
			somePair.Value.Destroy();

		allAssemblyTree.Remove(this);
        foreach (int someInt in familyTree)
            NodeController.UpdateDeathCount(someInt, familyGenerationRemoved[someInt]);
		PersistentGameManager.CaptureObjects.Remove(this);

		if(amalgam)
			amalgam.assemblies.Remove(this);
		cull = true;
        ViewerData.Inst.assemblyDeletes.Add(id);
	} // End of Destroy().


	public void Save(){
        string path = "./data/" + Name + ".txt";
        Save(path);
    } // End of Save().


    public void Save(string path){
        //ConsoleScript.Inst.WriteToLog("Saving " + path);
        IOHelper.SaveAssembly(path, this);
    } // End of Save().


	public string ToFileString(){
        return IOHelper.AssemblyToString(this);
    } // End of ToFileString().

} // End of PhysAssembly.