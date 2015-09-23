using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum VertexType {
	normal,
	receptor,
	distributor
} // End of VertexType.


public class Amalgam : MonoBehaviour
{
	public static List<Amalgam> allAmalgams = new List<Amalgam>();

	public List<Assembly> assemblies = new List<Assembly>();
	public List<FoodPellet> foodPellets = new List<FoodPellet>();
	int targetNumAssems = 20;
	int targetNumFood = 60;

	public float radius = 50f;
	Vector3[] initialVerts;

	ActiveVertex[] activeVertices;

	int[][] vertexGraph; // List of vertices, followed by list of neighbors.
	int[] shortestPath;
	MeshFilter meshFilter;

	Color color = Color.white;


	class ActiveVertex
	{
		public VertexType type = VertexType.normal;
		public Assembly linkedAssembly = null;

		public int index = 0;
		public ActiveVertex[] neighbors;
		public Amalgam amalgam;
		public Vector3 originPoint = Vector3.zero; // The initial point on the mesh.
		public Vector3 meshPoint{get{
			return amalgam.GetComponent<MeshFilter>().mesh.vertices[index];
		}}
		public Vector3 worldPoint{get{
			return amalgam.transform.position + (amalgam.transform.rotation * (Vector3.Scale(amalgam.GetComponent<MeshFilter>().mesh.vertices[index], amalgam.transform.localScale)));
		}}
		public Vector3 worldOriginPoint{get{
			return amalgam.transform.position + (amalgam.transform.rotation * (Vector3.Scale(originPoint, amalgam.transform.localScale)));
		}}

		public bool[] openGates = new bool[0];

		public float energy = 0f;
		float[] flux = new float[0];


		public void Update(){ 

			energy = Mathf.MoveTowards(energy, 0f, NodeController.physicsStep * 0.4f);
			energy = Mathf.Clamp01(energy);

			flux = new float[neighbors.Length];
			for(int i = 0; i < neighbors.Length; i++){
				if(!openGates[i])
					continue;

				ActiveVertex curNeighbor = neighbors[i];
				flux[i] = (curNeighbor.energy - energy);
			}

		} // End of Update().

		public void RevengeOfUpdate(){
			for(int i = 0; i < neighbors.Length; i++){
				ActiveVertex curNeighbor = neighbors[i];
				energy += flux[i] * NodeController.physicsStep;
				curNeighbor.energy -= flux[i] * NodeController.physicsStep;
			}
		} // End of RevengeOfUpdate().

	} // End of ActiveVertex.


	void Awake(){
		allAmalgams.Add(this);
		meshFilter = GetComponent<MeshFilter>();
	} // End of Awake().


	void Start()
	{
		Mesh newIcoSphere = IcoSphereCreator.Inst.Create(3);
		GetComponent<MeshFilter>().mesh = newIcoSphere;
		initialVerts = newIcoSphere.vertices;

		activeVertices = new ActiveVertex[newIcoSphere.vertexCount]; // Set up active vertex list
		for(int i = 0; i < newIcoSphere.vertexCount; i++){
			activeVertices[i] = new ActiveVertex();
			activeVertices[i].amalgam = this;
			activeVertices[i].index = i;
			activeVertices[i].originPoint = newIcoSphere.vertices[i];
		}

		vertexGraph = new int[activeVertices.Length][]; // Set up vertex graph
		for(int i = 0; i < newIcoSphere.vertexCount; i++){
			int[] neighborVerts = FindNeighborVertices(i, newIcoSphere);
			activeVertices[i].neighbors = new ActiveVertex[neighborVerts.Length];

			// Fill vertex graph
			vertexGraph[i] = new int[neighborVerts.Length];
			for(int j = 0; j < neighborVerts.Length; j++)
				vertexGraph[i][j] = neighborVerts[j];
			
			// Randomize gates
			activeVertices[i].openGates = new bool[activeVertices[i].neighbors.Length];
			for(int j = 0; j < neighborVerts.Length; j++){
				activeVertices[i].neighbors[j] = activeVertices[neighborVerts[j]];
				//activeVertices[i].openGates[j] = Random.Range(0f, 1f) < 0.2f;
				activeVertices[i].openGates[j] = true;

				// Randomize vertex types
				if(Random.Range(0f, 1f) < 0.01f)
					activeVertices[i].type = VertexType.receptor;
				else if(Random.Range(0f, 1f) < 0.01f)
					activeVertices[i].type = VertexType.distributor;

			}
		}

		color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		renderer.material.SetColor("_RimColor", color);

	} // End of Start().


	// Update is called once per frame
	void Update()
	{
		// Find closest vert to mouse.
		float maxDist = 9999f;
		float distFromCam = 9999f;
		ActiveVertex curVert = null;
		for(int i = 0; i < activeVertices.Length; i++){
			Vector3 screenPos = Camera.main.WorldToScreenPoint(activeVertices[i].worldOriginPoint);
			float dist = Vector2.Distance(new Vector2(Input.mousePosition.x, Input.mousePosition.y), new Vector2(screenPos.x, screenPos.y));
			if((dist < maxDist) && (screenPos.z < (distFromCam + 2f))){
				maxDist = dist;
				distFromCam = screenPos.z;
				curVert = activeVertices[i];
			}
		}

		Vector3 positionChange = Vector3.zero;
		// Update active vertices
		for(int i = 0; i < activeVertices.Length; i++){
			activeVertices[i].Update();

			if(Input.GetKey(KeyCode.K) && (activeVertices[i].type == VertexType.receptor))
				activeVertices[i].linkedAssembly = assemblies[Random.Range(0, assemblies.Count)];

			if(activeVertices[i].linkedAssembly){
				float proximity = Mathf.Clamp01(1f / (1f + (Mathf.Pow(Vector3.SqrMagnitude(activeVertices[i].worldOriginPoint - activeVertices[i].linkedAssembly.Position) * 0.002f, 2f))));
				activeVertices[i].energy += proximity * NodeController.physicsStep * 10f;
				for(int j = 0; j < activeVertices[i].neighbors.Length; j++){
					activeVertices[i].neighbors[j].energy += proximity * NodeController.physicsStep * 8f;

					/*
					for(int k = 0; k < activeVertices[i].neighbors[j].neighbors.Length; k++){
						if(activeVertices[i].neighbors[j].neighbors[k] != activeVertices[i])
							activeVertices[i].neighbors[j].neighbors[k].energy += proximity * NodeController.physicsStep * 5f;
					}
					*/
				}

				// Line assembly --> origin
				GLDebug.DrawLine(activeVertices[i].worldOriginPoint, activeVertices[i].linkedAssembly.Position, new Color(0f, 1f, 1f, proximity * 10f));
				// Box at origin
				GLDebug.DrawCube(activeVertices[i].worldOriginPoint, Quaternion.LookRotation(meshFilter.mesh.normals[i]), Vector3.one * 2f, color.SetAlpha(proximity * 10f));
				// Line origin --> current
				GLDebug.DrawLine(activeVertices[i].worldOriginPoint, activeVertices[i].worldPoint, color.SetAlpha(proximity * 10f));
				// Current point box
				GLDebug.DrawCube(activeVertices[i].worldPoint, Quaternion.LookRotation(meshFilter.mesh.normals[i]), Vector3.one * 4f, color.SetAlpha(proximity * 10f));
			}

			// Propel amalgam
			positionChange += meshFilter.mesh.normals[i] * activeVertices[i].energy * 0.1f * NodeController.physicsStep;
			
			// Draw energy lines between neighbors.
			for(int j = 0; j < activeVertices[i].neighbors.Length; j++){
				if(activeVertices[i].openGates[j] && (activeVertices[i].energy > 0.01f)){
					//GLDebug.DrawLine(activeVertices[i].worldPoint, activeVertices[i].neighbors[j].worldPoint, new Color(0f, 0f, 1f, activeVertices[i].energy));
				}
			}
		}

		/*
		transform.position += positionChange;
		for(int i = 0; i < assemblies.Count; i++)
			foreach(KeyValuePair<Triplet, Node> someNodeKVP in assemblies[i].NodeDict)
				someNodeKVP.Value.delayPosition += positionChange;
		for(int i = 0; i < foodPellets.Count; i++)
			foodPellets[i].WorldPosition += positionChange;
		*/

		for(int i = 0; i < activeVertices.Length; i++)
			activeVertices[i].RevengeOfUpdate();

		// Keep assemblies up!
		if((assemblies.Count < targetNumAssems) || Input.GetKeyDown(KeyCode.N)){
			Assembly newAssem = Assembly.RandomAssembly(transform.position + (Random.insideUnitSphere * radius), Quaternion.identity, Random.Range(4, 10));
			assemblies.Add(newAssem);
			newAssem.amalgam = this;
		}

		// Keep food up!
		if(foodPellets.Count < targetNumFood){
			FoodPellet newFood = new FoodPellet(transform.position + (Random.insideUnitSphere * radius));
			foodPellets.Add(newFood);
			newFood.amalgam = this;
		}

		Mesh tempMesh = GetComponent<MeshFilter>().mesh;
		Vector3[] verts = tempMesh.vertices;
		Color[] colors = new Color[tempMesh.vertexCount];
		for(int i = 0; i < tempMesh.vertexCount; i++){
			verts[i] = initialVerts[i] + (tempMesh.normals[i] * activeVertices[i].energy * 1f);
			if(activeVertices[i].type == VertexType.receptor)
				colors[i] = Color.cyan;
			else
				colors[i] = Color.Lerp(new Color(0f, 0f, 0.1f), Color.cyan, activeVertices[i].energy);
		}
		GetComponent<MeshFilter>().mesh.colors = colors;
		GetComponent<MeshFilter>().mesh.vertices = verts;


		// Shortest path test
		/*
		shortestPath = ShortestVertexPath(curVert.index, 0);
		for(int i = 0; i < shortestPath.Length - 1; i++){
			GLDebug.DrawLine(activeVertices[shortestPath[i]].worldPoint, activeVertices[shortestPath[i + 1]].worldPoint, new Color(0f, 1f, 1f));
		}
		*/

	} // End of Update().
	

	int[] FindNeighborVertices(int vertex, Mesh mesh)
	{
		int[] triangles = mesh.triangles;
		HashSet<int> neighbors = new HashSet<int>();
		for(int i = 0; i < triangles.Length / 3; i++){
			int[] thisTriangle = new int[]{triangles[(i * 3)], triangles[(i * 3) + 1], triangles[(i * 3) + 2]};
			for(int j = 0; j < 3; j++){
				if(thisTriangle[j] == vertex){
					for(int k = 0; k < 3; k++){
						if(!neighbors.Contains(thisTriangle[k]))
							neighbors.Add(thisTriangle[k]);
					}
					break;
				}
			}
		}
		neighbors.Remove(vertex);
		int[] neighborVerts = new int[neighbors.Count];
		neighbors.CopyTo(neighborVerts);
		return neighborVerts;
	} // End of FindNeighborVertices().


	// Uses A* to find the shortest path between two vertices.
	int[] ShortestVertexPath(int initialVert, int targetVert){
		List<int> shortestPath = new List<int>();
		shortestPath.Add(initialVert);
		int totalChecks = 0;

		// Find the neighbor closest to target vert.
		int currentVert = initialVert;
		while((currentVert != targetVert) && (totalChecks < 100)){
			float shortestDistToTarget = 9999f;
			int bestNeighbor = 0;
			for(int i = 0; i < activeVertices[currentVert].neighbors.Length; i++){
				float distToTarget = Vector3.SqrMagnitude(activeVertices[currentVert].neighbors[i].originPoint - activeVertices[targetVert].originPoint);
				if(distToTarget < shortestDistToTarget){
					shortestDistToTarget = distToTarget;
					bestNeighbor = i;
				}
			}
			currentVert = activeVertices[currentVert].neighbors[bestNeighbor].index;
			shortestPath.Add(currentVert);
			totalChecks++;
		}

		return shortestPath.ToArray();
	} // End of ShortestVertexPath().

	
	void OnGUI(){
		/*
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		for(int i = 0; i < activeVertices.Length; i++){
			ActiveVertex curVert = activeVertices[i];

			if(curVert.energy > 0.01f){
				Vector3 screenPos = Camera.main.WorldToScreenPoint(curVert.worldPoint);
				GUI.skin.label.alignment = TextAnchor.MiddleCenter;
				GUI.skin.label.fontSize = Mathf.CeilToInt((1f / screenPos.z) * 1000f);
				GUI.Label(MathUtilities.CenteredSquare(screenPos.x, screenPos.y, 100f), (curVert.energy * 100f).ToString("F0"));
			}
		}
		*/
	} // End of OnGUI().

} // End of Amalgam.
