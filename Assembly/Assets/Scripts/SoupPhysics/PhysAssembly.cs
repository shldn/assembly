using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysAssembly {

	static HashSet<PhysAssembly> all = new HashSet<PhysAssembly>();
	public static HashSet<PhysAssembly> getAll {get{return all;}}
	public static implicit operator bool(PhysAssembly exists){return exists != null;}

	Dictionary<Triplet, PhysNode> nodeDict = new Dictionary<Triplet, PhysNode>();
	public Dictionary<Triplet, PhysNode> NodeDict {get{return nodeDict;}}

	public Vector3 spawnPosition = Vector3.zero;
	public Quaternion spawnRotation = Quaternion.identity;
	public Vector3 WorldPosition {
		get{
			Vector3 worldPos = Vector3.zero;
			foreach(PhysNode someNode in nodeDict.Values)
				worldPos += someNode.Position;
			worldPos /= nodeDict.Keys.Count;
			return worldPos;
		}
	}
	public Quaternion worldRotation = Random.rotation;

	public float energy = 0f;
	public bool cull = false;

	public bool wantToMate = false;
	float mateAttractDist = 100f;

	private static Octree<PhysAssembly> allAssemblyTree;
    public static Octree<PhysAssembly> AllAssemblyTree{ 
        get{
            if(allAssemblyTree == null){
                allAssemblyTree = new Octree<PhysAssembly>(new Bounds(Vector3.zero, 2.0f * PhysNodeController.Inst.WorldSize * Vector3.one), (PhysAssembly x) => x.WorldPosition, 5);
			}
            return allAssemblyTree;
        }
        set{
            allAssemblyTree = value;
        }
    }


	public PhysAssembly(Vector3 spawnPosition, Quaternion spawnRotation){
		this.spawnPosition = spawnPosition;
		this.spawnRotation = spawnRotation;
		all.Add(this);
		AllAssemblyTree.Insert(this);
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

		energy += 1f;
	} // End of AddNode().


	public void RemoveNode(PhysNode nodeToRemove){
		if(nodeDict.ContainsKey(nodeToRemove.localHexPos))
			nodeDict.Remove(nodeToRemove.localHexPos);
	} // End of AddNode().


	public void Update(){
		if(energy < 0f)
			Destroy();

		float maxEnergy = nodeDict.Values.Count * 2f;
		energy = Mathf.Clamp(energy, 0f, maxEnergy);

		if(energy > (maxEnergy * 0.9f))
			wantToMate = true;
		else if(energy < maxEnergy * 0.5f)
			wantToMate = false;

		//calling detect food on sense node, determines power of node
		if(wantToMate){
			Bounds mateAttractBoundary = new Bounds(WorldPosition, mateAttractDist * (new Vector3(1, 1, 1)));
			allAssemblyTree.RunActionInRange(new System.Action<PhysAssembly>(HandleAttractMate), mateAttractBoundary);
		}
	} // End of Update().


	public void HandleAttractMate(PhysAssembly someAssembly){
		Vector3 vectorToMate = someAssembly.WorldPosition - WorldPosition;
		float distanceToMate = vectorToMate.magnitude;

		if(distanceToMate > mateAttractDist)
			return;

		if(!someAssembly.wantToMate){
			GLDebug.DrawLine(WorldPosition, someAssembly.WorldPosition, Color.blue);
			return;
		}

		foreach(PhysNode someNode in nodeDict.Values)
			someNode.delayPosition += vectorToMate.normalized;

		GLDebug.DrawLine(WorldPosition, someAssembly.WorldPosition, Color.magenta);

	} // End of HandleAttractMate().


	// Merges two assemblies together.
	public void AmaglamateTo(PhysAssembly otherAssembly, Triplet offset){
		foreach(PhysNode someNode in nodeDict.Values){
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


	public void Destroy(){
		foreach(KeyValuePair<Triplet, PhysNode> somePair in nodeDict)
			somePair.Value.Destroy();

		allAssemblyTree.Remove(this);
		cull = true;
	} // End of Destroy().

} // End of PhysAssembly.