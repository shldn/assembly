using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Amalgam : MonoBehaviour
{
	public static List<Amalgam> allAmalgams = new List<Amalgam>();

	public List<Assembly> assemblies = new List<Assembly>();
	public List<FoodPellet> foodPellets = new List<FoodPellet>();
	int targetNumAssems = 20;
	int targetNumFood = 60;

	public float radius = 50f;
	public Vector3[] initialVerts;
	public Vector3[] initialNorms;

	ActiveVertex[] activeVertices;
	public ActiveVertex[] ActiveVertices { get { return activeVertices; } }

	int[][] vertexGraph; // List of vertices, followed by list of neighbors.
	int[] shortestPath;
	public MeshFilter meshFilter;

	public Color color = Color.white;

	public List<AmalgamHandle> handles = new List<AmalgamHandle>();
	public UnityEngine.Object handlePrefabInt = null;
	public UnityEngine.Object handlePrefabExt = null;

	List<RaidenLaser>absorbEffects = new List<RaidenLaser>();

	Vector3 randomRotVector = Vector3.zero;

	//Vector3 motionVector = Vector3.zero;
	//Vector3 motionVectorVel = Vector3.zero;
	//Vector3 currentVelocity = Vector3.zero;

	public float deformFluxRate = 2f;
	public float wiggleFluxRate = 5f;
	public static int sphereResolution = 2;

	public class ActiveVertex
	{
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

		public float deform = 0f;
		float[] deformFlux = new float[0];

		public Vector3 wiggle = Vector3.zero;
		Vector3[] wiggleFlux = new Vector3[0];


		public void Update(){ 

			deform = Mathf.MoveTowards(deform, 0f, NodeController.physicsStep * 0.2f);
			//energy = Mathf.Clamp01(energy);

			deformFlux = new float[neighbors.Length];
			wiggleFlux = new Vector3[neighbors.Length];
			for(int i = 0; i < neighbors.Length; i++){
				if(!openGates[i])
					continue;

				ActiveVertex curNeighbor = neighbors[i];
				deformFlux[i] = (curNeighbor.deform - deform);
				wiggleFlux[i] = (curNeighbor.wiggle - wiggle);
			}

		} // End of Update().

		public void RevengeOfUpdate(){
			for(int i = 0; i < neighbors.Length; i++){
				ActiveVertex curNeighbor = neighbors[i];
				float deformFluxTransfer = deformFlux[i] * NodeController.physicsStep * amalgam.deformFluxRate * (1f / neighbors.Length);
				deform += deformFluxTransfer;
				curNeighbor.deform -= deformFluxTransfer;

				Vector3 wiggleFluxTransfer = wiggleFlux[i] * (NodeController.physicsStep * amalgam.wiggleFluxRate * (1f / neighbors.Length));
				wiggle += wiggleFluxTransfer;
				curNeighbor.wiggle -= wiggleFluxTransfer;
			}
		} // End of RevengeOfUpdate().

	} // End of ActiveVertex.


	void Awake(){
		allAmalgams.Add(this);
		meshFilter = GetComponent<MeshFilter>();
		//motionVector = Random.rotation * Vector3.forward;
	} // End of Awake().


	void Start()
	{
		Mesh newIcoSphere = IcoSphereCreator.Inst.Create(sphereResolution);
		GetComponent<MeshFilter>().mesh = newIcoSphere;
		initialVerts = newIcoSphere.vertices;
		initialNorms = newIcoSphere.normals;

		deformFluxRate = Mathf.Pow(Random.Range(1f, 2f), 3f);
		wiggleFluxRate = Mathf.Pow(Random.Range(1f, 3f), 2f);



		// UV mapping
        float pi_recip = 1.0f / Mathf.PI;
		Vector3 center = Vector3.zero;
		List<Vector2> newUV = new List<Vector2>();
        for (int i = 0; i < meshFilter.mesh.vertices.Length; ++i)
        {
            // project uvs as if mesh was a sphere
            Vector3 v = (meshFilter.mesh.vertices[i] - center).normalized;
            newUV.Add(new Vector2(0.5f + 0.5f * pi_recip * Mathf.Atan2(v.z, v.x), 0.5f - pi_recip * Mathf.Asin(v.y)));
        }
		meshFilter.mesh.uv = newUV.ToArray();

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
			}
		}

		color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
		//color = new Color(11f / 255f, 62f / 255f, 110f / 255f);
		GetComponent<Renderer>().materials[1].SetColor("_RimColor", color);


		handlePrefabInt = Resources.Load("AmalgamHandleInt");
		handlePrefabExt = Resources.Load("AmalgamHandleExt");
		int numHandles = Random.Range(3, 12);
		for(int i = 0; i < numHandles; i++){
			handles.Add(new AmalgamHandle(this, Random.Range(0, newIcoSphere.vertexCount)));
			
			// Initialize arteries
			GameObject newArteryGO = Instantiate(Resources.Load("AmalgamArtery")) as GameObject;
			AmalgamArtery newArtery = newArteryGO.GetComponent<AmalgamArtery>();
			newArtery.amalgam = this;
			newArtery.vertices = ShortestVertexPath(handles[i].attachedVert, handles[Random.Range(0, handles.Count)].attachedVert);
        }

		transform.rotation = Random.rotationUniform;
		randomRotVector = Random.rotationUniform * Vector3.forward;

		transform.localScale *= Random.Range(0.5f, 2f);

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
		
		// This tracks if the amalgam should move around.
		Vector3 positionChange = Vector3.zero;
		for(int i = 0; i < handles.Count; i++){
			positionChange += 0.1f * handles[i].energyBuildup * (transform.rotation * initialNorms[handles[i].attachedVert]) * NodeController.physicsStep;

			//Debug.DrawRay(transform.position, (transform.rotation * initialNorms[handles[i].attachedVert]) * 100f);
		}

		//positionChange += motionVector * NodeController.physicsStep * 2f;
		
		//motionVector = Vector3.SmoothDamp(motionVector, currentVelocity, ref motionVectorVel, 5f);


		// Update active vertices
		for(int i = 0; i < activeVertices.Length; i++){
			activeVertices[i].Update();
			
			// Draw energy lines between neighbors.
			//for(int j = 0; j < activeVertices[i].neighbors.Length; j++){
				//if(activeVertices[i].openGates[j] && (activeVertices[i].skinDeform > 0.01f)){
					//GLDebug.DrawLine(activeVertices[i].worldPoint, activeVertices[i].neighbors[j].worldPoint, new Color(0f, 0f, 1f, activeVertices[i].energy));
				//}
			//}
		}


		// Propel amalgam
		//transform.position += positionChange;

		/*
		for(int i = 0; i < assemblies.Count; i++)
			foreach(KeyValuePair<Triplet, Node> someNodeKVP in assemblies[i].NodeDict)
				someNodeKVP.Value.staticMove += positionChange;
		for(int i = 0; i < foodPellets.Count; i++)
			foodPellets[i].WorldPosition += positionChange;
		*/


		//currentVelocity = positionChange / NodeController.physicsStep;


		for(int i = 0; i < activeVertices.Length; i++)
			activeVertices[i].RevengeOfUpdate();

		// Keep assemblies up!
		if(assemblies.Count < targetNumAssems){
			Assembly newAssem = Assembly.RandomAssembly(transform.position + (Random.insideUnitSphere * radius), Quaternion.identity, Random.Range(4, 10));
			assemblies.Add(newAssem);
			newAssem.amalgam = this;
		}

		// ...but not TOO many! Cull the herd if there are too many.
		if(assemblies.Count > targetNumAssems * 1.2f){
			float highestHealth = 9999f;
			Assembly worstAssembly = null;
			for(int i = 0; i < assemblies.Count; i++){
				if(assemblies[i].Health < highestHealth){
					highestHealth = assemblies[i].Health;
					worstAssembly = assemblies[i];
				}
			}
			if(worstAssembly)
				worstAssembly.Destroy();
		} 


		// Keep food up!
		/*
		if(foodPellets.Count < targetNumFood){
			FoodPellet newFood = new FoodPellet(transform.position + (Random.insideUnitSphere * radius));
			foodPellets.Add(newFood);
			newFood.amalgam = this;
		}
		*/

		Mesh tempMesh = GetComponent<MeshFilter>().mesh;
		Vector3[] verts = tempMesh.vertices;
		Color[] colors = new Color[tempMesh.vertexCount];
		//float motionVectorMagnitude = Mathf.Clamp01(motionVector.magnitude);
		for(int i = 0; i < tempMesh.vertexCount; i++){

			// Vert offset
			Vector3 vertRotated = transform.rotation * initialVerts[i];
			//float motionPosition = Vector3.Dot(vertRotated, motionVector);
			//float vertOffset = 1.15f + ((Mathf.Cos(Time.time + (motionPosition * 5f)) * 0.15f) * motionVectorMagnitude);

			float actualSkinDeform = activeVertices[i].deform;
			if(actualSkinDeform < 0f)
				actualSkinDeform = -Mathf.Sqrt(-actualSkinDeform);
			else
				actualSkinDeform = Mathf.Sqrt(actualSkinDeform);

			verts[i] = (Quaternion.Euler(activeVertices[i].wiggle) * initialVerts[i]) * actualSkinDeform;
			colors[i] = Color.Lerp(new Color(0f, 0f, 0.1f), Color.cyan, actualSkinDeform);
		}
		GetComponent<MeshFilter>().mesh.colors = colors;
		GetComponent<MeshFilter>().mesh.vertices = verts;

		GetComponent<MeshFilter>().mesh.RecalculateNormals();

		/*
		// Shortest path test
		shortestPath = ShortestVertexPath(curVert.index, 0);
		for(int i = 0; i < shortestPath.Length - 1; i++){
			GLDebug.DrawLine(activeVertices[shortestPath[i]].worldPoint, activeVertices[shortestPath[i + 1]].worldPoint, new Color(0f, 1f, 1f));
		}
		*/

		// Rotation
		transform.rotation *= Quaternion.AngleAxis(NodeController.physicsStep * 1f, randomRotVector);

		for(int i = 0; i < handles.Count; i++)
			handles[i].Update();


		//renderer.materials[0].mainTextureOffset = new Vector2(Time.time * 0.01f, Time.time * 0.01f);

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


	public Vector3 GetVertexWorldPoint(int index){
		return transform.position + (transform.rotation * Vector3.Scale(meshFilter.mesh.vertices[index], transform.localScale));
	} // End of GetVertexWorldPoint().

} // End of Amalgam.


public class AmalgamHandle {

	Amalgam amalgam = null;
	public int attachedVert = 0;

	float extEffect = 1f;
	float intEffect = 1f;

	public Transform gameObjectInt = null;
	public Transform gameObjectExt = null;

	//public float frequency = 0f;

	public float energyBuildup = 0f; // When this is high enough, it will emit a food pellet.

	public RaidenLaser raidenLaser;

	float energySearchCooldown = 0f;
	public EnergySource energySource = null;


	public float testBubble = 0f;
	public Vector3 testWiggle = Vector3.zero;
	public float testWiggleFreq = 0f;


	public AmalgamHandle (Amalgam amalgam, int attachedVert){
		this.attachedVert = attachedVert;
		this.amalgam = amalgam;
		GameObject newGOInt = MonoBehaviour.Instantiate(amalgam.handlePrefabInt) as GameObject;
		GameObject newGOExt = MonoBehaviour.Instantiate(amalgam.handlePrefabExt) as GameObject;
		gameObjectInt = newGOInt.transform;
		gameObjectExt = newGOExt.transform;

		extEffect = Random.Range(0.2f, 1f);
		intEffect = Random.Range(0.2f, 1f);

		energySearchCooldown = Random.Range(0f, 10f);
		//frequency = Random.Range(2f, 30f);

		RandomizeHandleBehaviours();
	} // End of AmalgamHandle().

	public void Update(){
		gameObjectInt.position = amalgam.transform.position + amalgam.transform.rotation * Vector3.Scale(amalgam.transform.localScale, amalgam.GetComponent<MeshFilter>().mesh.vertices[attachedVert]);
		gameObjectInt.rotation = Quaternion.LookRotation(amalgam.transform.rotation * -amalgam.initialNorms[attachedVert], gameObjectInt.transform.up);
		gameObjectExt.position = amalgam.transform.position + amalgam.transform.rotation * Vector3.Scale(amalgam.transform.localScale, amalgam.GetComponent<MeshFilter>().mesh.vertices[attachedVert]);
		gameObjectExt.rotation = Quaternion.LookRotation(amalgam.transform.rotation * amalgam.initialNorms[attachedVert], gameObjectExt.transform.up);

		gameObjectInt.localScale = new Vector3(1f, 1f, intEffect);
		gameObjectExt.localScale = new Vector3(1f, 1f, extEffect);
		
		// Try to find an energy source if we don't have one.
		energySearchCooldown -= NodeController.physicsStep;
		if(energySearchCooldown < 0f){
			energySearchCooldown = Random.Range(0f, 10f);

			if(energySource == null){
				EnergySource[] allSources = Component.FindObjectsOfType<EnergySource>();
				if(allSources.Length > 0f){
					// Look for a suitable energy source.
					for(int i = 0; i < allSources.Length; i++){
						if(CheckEnergySource(allSources[i])) {
							energySource = allSources[i];

							GameObject newAbsorbGO = MonoBehaviour.Instantiate(Resources.Load("EnergyTransferEffect")) as GameObject;
							raidenLaser = newAbsorbGO.GetComponent<RaidenLaser>();
							raidenLaser.startTrans = gameObjectExt;
							raidenLaser.endTrans = energySource.transform;
							raidenLaser.color = amalgam.color;

							break;
						}
					}
				}
			}else{
				if(!CheckEnergySource(energySource)){
					energySource = null;
					if(raidenLaser)
						raidenLaser.endTrans = null;
				}
			}
		}


		if(energySource != null){
			float energyToAbsorb = 1f * NodeController.physicsStep /* * (0.5f + (0.5f * Mathf.Cos((2f * Mathf.PI * Time.time) / curHandle.frequency)))*/;
			energySource.energy -= energyToAbsorb;
			energyBuildup += energyToAbsorb;

			Vector3 vecToSource = energySource.transform.position - gameObjectExt.position;
			//amalgam.transform.rotation = Quaternion.RotateTowards(amalgam.transform.rotation, Quaternion.LookRotation(vecToSource), NodeController.physicsStep * 2f);
		} else {
			energyBuildup -= 0.5f * NodeController.physicsStep;
		}

		energyBuildup = Mathf.MoveTowards(energyBuildup, 0f, NodeController.physicsStep * 0.02f);

		if((energyBuildup >= 1f) && (Random.Range(0f, 1f) < 0.03f)) {
			FoodPellet newFood = new FoodPellet(gameObjectInt.position);
			newFood.velocity = (gameObjectInt.forward * Random.Range(16f, 24f)) + (Random.rotation * Vector3.forward);
			amalgam.foodPellets.Add(newFood);
			newFood.amalgam = amalgam;
			energyBuildup -= 1f;
		}

		amalgam.ActiveVertices[attachedVert].deform += NodeController.physicsStep + (0.02f * energyBuildup);

		amalgam.ActiveVertices[attachedVert].deform = testBubble;
		amalgam.ActiveVertices[attachedVert].wiggle = testWiggle * Mathf.Sin(Time.time * Mathf.PI * 2f * testWiggleFreq);

		if(Input.GetKeyDown(KeyCode.B))
			RandomizeHandleBehaviours();
		
	} // End of Update().

	void RandomizeHandleBehaviours() {
		testWiggle = Random.rotation * Vector3.forward * Random.Range(0f, 90f);
		testWiggleFreq = Random.Range(0.02f, 0.1f);
		testBubble = Random.Range(0f, 3f);
	} // End of RandomizeHandleBehaviours().

	bool CheckEnergySource(EnergySource source){
		if(Vector3.Distance(gameObjectExt.position, source.transform.position) < 500f){
			Vector3 vecToSource = source.transform.position - gameObjectExt.position;
			if(Vector3.Angle(gameObjectExt.forward, vecToSource) < 45f)
				return true;
		}

		return false;
	} // End of CheckEnergySource().
	
} // End of AmalgamHandle.
