using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelGen : MonoBehaviour {

	int voxWorldSize = 100;
	float[][][] densityMap;
	MarchingCubes mCubes;
	float scaleRatio = 1f;

	Vector3 originOffset = Vector3.zero;
	MeshFilter meshFilter;
	Vector3[] initialVerts;
	Vector3[] currentVerts;

	public Transform[] extOrganTransforms;
	List<ExternalOrgan> externalOrgans = new List<ExternalOrgan>();

	Vector3 randomRotVec = Vector3.forward;
	Quaternion motionVector = Quaternion.identity;

	List<int>[] neighborBook;


	void Start(){
	
		//transform.position = voxWorldSize * Vector3.one * -0.5f;
		mCubes = MarchingCubes.Singleton;

		// Initialize density map
		densityMap = new float[voxWorldSize][][];
		for(int i = 0; i < voxWorldSize; i++){
			densityMap[i] = new float[voxWorldSize][];
			for(int j = 0; j < voxWorldSize; j++){
				densityMap[i][j] = new float[voxWorldSize];
			}
		}

		scaleRatio = 1f / transform.localScale.x;
		originOffset = Vector3.one * voxWorldSize * 0.5f / scaleRatio;
		transform.position = -originOffset;

		meshFilter = GetComponent<MeshFilter>();

	} // End of Start().
	

	void Update(){
		if(KeyInput.GetKeyUp(KeyCode.M) && !ConsoleScript.active)
			UpdateMesh();

		Mesh mesh = meshFilter.sharedMesh;
		Vector3[] verts = mesh.vertices;
		Vector3[] normals = mesh.normals;

		Plane motionPlane = new Plane(motionVector * Vector3.forward, transform.position + (Vector3.one * 0.5f * voxWorldSize));
		motionVector *= Quaternion.AngleAxis(Time.deltaTime * 10f, randomRotVec);

		if(initialVerts != null){
			Vector3[] vertsRef = mesh.vertices;

			for(int i = 0; i < verts.Length; i++){
				//verts[i] = initialVerts[i] + (normals[i] * Mathf.Sin(Time.time + (motionPlane.GetDistanceToPoint(initialVerts[i]) * 0.2f)) * 3.5f);

				if(neighborBook[i] != null){
					Vector3 averageNeighborPos = Vector3.zero;
					for(int j = 0; j < neighborBook[i].Count; j++)
						averageNeighborPos += vertsRef[neighborBook[i][j]];
					averageNeighborPos /= neighborBook[i].Count;

					verts[i] = averageNeighborPos;
				}

				//verts[i] += normals[i] * Mathf.Sin(Time.time + (initialVerts[i].x * 1f)) * 0.15f;

				// Loyalty to initial shape.
				verts[i] = Vector3.Lerp(verts[i], initialVerts[i], 0.05f);

			}
		}

		if(Input.GetKeyDown(KeyCode.B)){
			int randomIdx = Random.Range(0, verts.Length);
			verts[randomIdx] += normals[randomIdx] * 10000f;
		}

		if(Input.GetKeyDown(KeyCode.V) && !ConsoleScript.active){
			verts = initialVerts;
		}

		mesh.vertices = verts;
		mesh.RecalculateNormals();

		for(int i = 0; i < verts.Length; i++)
			verts[i] += normals[i] * 0.05f;

		mesh.vertices = verts;


		foreach(ExternalOrgan someOrgan in externalOrgans){
			someOrgan.transform.position = transform.position + (Vector3.Scale(verts[someOrgan.attachmentVertIdx], transform.localScale));
			someOrgan.transform.rotation = Quaternion.LookRotation(normals[someOrgan.attachmentVertIdx]);
		}

	} // End of Update().


	int[] FindNeighborVertices(int vertex, Mesh mesh){

		int[] triangles = mesh.triangles;
		HashSet<int> neighbors = new HashSet<int>();
		for(int i = 0; i < triangles.Length / 3; i++){
			int[] thisTriangle = new int[]{triangles[(i * 3)], triangles[(i * 3) + 1], triangles[(i * 3) + 2]};
			for(int j = 0; j < 3; j++){
				if(thisTriangle[j] == vertex){
					for(int k = 0; k < 3; k++)
						neighbors.Add(thisTriangle[k]);
					break;
				}
			}
		}
		neighbors.Remove(vertex);
		int[] neighborVerts = new int[neighbors.Count];
		neighbors.CopyTo(neighborVerts);
		return neighborVerts;
	} // End of FindNeighborVertices().


	void UpdateMesh(){

		// Initialize density map
		for(int x = 0; x < voxWorldSize; x++){
			for(int y = 0; y < voxWorldSize; y++){
				for(int z = 0; z < voxWorldSize; z++){
					densityMap[x][y][z] = -1f;
				}
			}
		}

		foreach(Assembly someAssembly in Assembly.getAll){
			//Triplet nearestTrip = new Triplet(someAssembly.Position);
			Triplet nearestTrip = new Triplet(Mathf.RoundToInt((someAssembly.Position.x + originOffset.x) * scaleRatio), Mathf.RoundToInt((someAssembly.Position.y + originOffset.y) * scaleRatio), Mathf.RoundToInt((someAssembly.Position.z + originOffset.z) * scaleRatio));

			Triplet currentTrip;
			float radius = 15f;
			for(float x = nearestTrip.x - radius; x < nearestTrip.x + radius; x++){
				for(float y = nearestTrip.y - radius; y < nearestTrip.y + radius; y++){
					for(float z = nearestTrip.z - radius; z < nearestTrip.z + radius; z++){
						currentTrip = new Triplet(Mathf.RoundToInt(x), Mathf.RoundToInt(y), Mathf.RoundToInt(z));
						if(currentTrip.x < 0 || currentTrip.y < 0 || currentTrip.z < 0 || currentTrip.x >= voxWorldSize || currentTrip.y >= voxWorldSize || currentTrip.z >= voxWorldSize)
							continue;

						densityMap[currentTrip.x][currentTrip.y][currentTrip.z] += Mathf.Clamp01(((radius * 0.5f) - Vector3.Distance(new Vector3(x, y, z), nearestTrip.ToVector3())) * 0.5f) * 2f;
					}
				}
			}
		}

		/*
		for(int x = 0; x < voxWorldSize; x++)
			for(int y = 0; y < voxWorldSize; y++)
				for(int z = 0; z < voxWorldSize; z++){
					float distFromCenter = Vector3.Distance(Vector3.one * 0.5f * voxWorldSize, new Vector3(x, y, z));
					densityMap[x][y][z] += Mathf.Clamp01(((voxWorldSize * 0.25f) - (distFromCenter * 2f)) * 0.2f) * 2f;
				}
		*/

		mCubes.SetDensityMap(densityMap);
		mCubes.GutterSize = 2;
		mCubes.March();
		Mesh mesh = meshFilter.mesh;
 		mCubes.UpdateMesh(mesh);
		initialVerts = mesh.vertices;

		foreach(Transform someTrans in extOrganTransforms){
			externalOrgans.Add(new ExternalOrgan(someTrans, Random.Range(0, initialVerts.Length)));
		}

		neighborBook = new List<int>[mesh.vertexCount];
		for(int i = 0; i < mesh.vertexCount; i++){
			neighborBook[i] = new List<int>();
			neighborBook[i].AddRange(FindNeighborVertices(i, mesh));
		}

	} // End of UpdateMesh().




	public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float v321 = p3.x * p2.y * p1.z;
		float v231 = p2.x * p3.y * p1.z;
		float v312 = p3.x * p1.y * p2.z;
		float v132 = p1.x * p3.y * p2.z;
		float v213 = p2.x * p1.y * p3.z;
		float v123 = p1.x * p2.y * p3.z;
 
		return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
	}
 
	public float VolumeOfMesh(Mesh mesh)
	{
		float volume = 0;
 
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;
 
		for (int i = 0; i < mesh.triangles.Length; i += 3)
		{
			Vector3 p1 = vertices[triangles[i + 0]];
			Vector3 p2 = vertices[triangles[i + 1]];
			Vector3 p3 = vertices[triangles[i + 2]];
			volume += SignedVolumeOfTriangle(p1, p2, p3);
		}
 
		return Mathf.Abs(volume);
	}


	
} // End of VoxelGen.


class ExternalOrgan{

	public Transform transform = null;
	public int attachmentVertIdx = 0;

	public ExternalOrgan(Transform transform, int attachmentVertIdx){
		this.transform = transform;
		this.attachmentVertIdx = attachmentVertIdx;
	} // End of constructor.

} // End of ExternalOrgan().

