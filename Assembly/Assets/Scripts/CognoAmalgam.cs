using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CognoAmalgam : MonoBehaviour {

    public static CognoAmalgam Inst = null;

	public List<Assembly> assemblies = new List<Assembly>();
	public int targetNumAssems = 50;
	
	public float deformFluxRate = 0.0025f;
	MeshFilter myMeshFilter = null;
	public MeshFilter[] targetMeshFilters;

	List<int> streamVerts = new List<int>();
	int streamVertDensity = 30;
	LineRenderer streamLineRenderer;
	List<ThreeAxisCreep> streamCreeps = new List<ThreeAxisCreep>();

    // Optimizations
    float minDeformRadius = 0f;

	public class ActiveVertex
	{
		public int index = 0;
		public ActiveVertex[] neighbors;
		public CognoAmalgam cognoAmalgam;
		public Vector3 originPoint = Vector3.zero; // The initial point on the mesh.
		public Vector3 meshPoint{get{
			return cognoAmalgam.GetComponent<MeshFilter>().mesh.vertices[index];
		}}
		public Vector3 worldPoint{get{
			return cognoAmalgam.transform.position + (cognoAmalgam.transform.rotation * (Vector3.Scale(cognoAmalgam.GetComponent<MeshFilter>().mesh.vertices[index], cognoAmalgam.transform.localScale)));
		}}
		public Vector3 worldOriginPoint{get{
			return cognoAmalgam.transform.position + (cognoAmalgam.transform.rotation * (Vector3.Scale(originPoint, cognoAmalgam.transform.localScale)));
		}}

		public float deform = 0f;
		public float power = 0f;
		float[] deformFlux = new float[0];

		public void Update(){ 

			deform = Mathf.Lerp(deform, 0f, Time.deltaTime * 0.02f);
			//energy = Mathf.Clamp01(energy);

			deformFlux = new float[neighbors.Length];
			for(int i = 0; i < neighbors.Length; i++){
				ActiveVertex curNeighbor = neighbors[i];
				deformFlux[i] = (curNeighbor.deform - deform);
			}

		} // End of Update().

		public void RevengeOfUpdate(){
			for(int i = 0; i < neighbors.Length; i++){
				ActiveVertex curNeighbor = neighbors[i];
				float deformFluxTransfer = deformFlux[i] * Time.deltaTime * cognoAmalgam.deformFluxRate * (1f / neighbors.Length);
				deform += deformFluxTransfer;
				curNeighbor.deform -= deformFluxTransfer;
			}
		} // End of RevengeOfUpdate().

	} // End of ActiveVertex.
	ActiveVertex[] allAVs;
	int[][] vertexGraph;
	public Vector3[] initialVerts;

	ParticleSystem pSys;

    void Awake() {
        Inst = this;
    }

    void OnDestroy() {
        Inst = null;
    }

	void Start () {
		myMeshFilter = GetComponent<MeshFilter>();
		pSys = GetComponent<ParticleSystem>();

		Mesh newIcoSphere = IcoSphereCreator.Inst.Create(3);
		myMeshFilter.mesh = newIcoSphere;

		initialVerts = myMeshFilter.mesh.vertices;

		// Create all AVs.
		allAVs = new ActiveVertex[myMeshFilter.mesh.vertexCount];
		for(int i = 0; i < myMeshFilter.mesh.vertexCount; i++) {
			allAVs[i] = new ActiveVertex();
			allAVs[i].index = i;
			allAVs[i].originPoint = myMeshFilter.mesh.vertices[i];
			allAVs[i].cognoAmalgam = this;
		}

		vertexGraph = new int[allAVs.Length][]; // Set up vertex graph
		for(int i = 0; i < myMeshFilter.mesh.vertexCount; i++){
			int[] neighborVerts = FindNeighborVertices(i, myMeshFilter.mesh);
			allAVs[i].neighbors = new ActiveVertex[neighborVerts.Length];

			// Fill vertex graph
			vertexGraph[i] = new int[neighborVerts.Length];
			for(int j = 0; j < neighborVerts.Length; j++)
				vertexGraph[i][j] = neighborVerts[j];

			for(int j = 0; j < neighborVerts.Length; j++){
				allAVs[i].neighbors[j] = allAVs[neighborVerts[j]];
			}
		}


		//*
		streamVerts = new List<int>(new int[]{413, 238, 231, 212, 309, 9, 450, 440, 339, 84, 49, 12, 347, 363, 466, 487, 588, 390, 621});
		for(int i = 0; i < streamVerts.Count; i++) {
			streamCreeps.Add(gameObject.AddComponent<ThreeAxisCreep>());
			streamCreeps[streamCreeps.Count - 1].relaxedness = 5f;
			streamCreeps[streamCreeps.Count - 1].maxCreep = 0.2f;
		}
		//*/

		streamLineRenderer = GetComponent<LineRenderer>();

	} // End of Start().
	

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


	void Update () {
		// Update ActiveVertices.
		for(int i = 0; i < allAVs.Length; i++)
			allAVs[i].Update();
		for(int i = 0; i < allAVs.Length; i++)
			allAVs[i].RevengeOfUpdate();

		for(int i = 0; i < allAVs.Length; i++) {
			for(int j = 0; j < 2; j++) {
				Vector3 avFingerPos = Vector3.zero;
				// Get networked fingertip positions
				for(int k = 0; k < 5; k++)
					avFingerPos += SmoothNetPosition.allFingertips[(j * 5) + k].transform.position;
				
				avFingerPos /= 5f;

				float sqrDist = Vector3.SqrMagnitude(avFingerPos - allAVs[i].worldPoint);
				float deformAmount = (1f / (sqrDist * 0.05f)) * 0.5f;
				allAVs[i].deform += deformAmount;

				float distributedDeform = deformAmount / allAVs.Length;
				for(int l = 0; l < allAVs.Length; l++)
					allAVs[l].deform -= distributedDeform;
            }
			//allAVs[i].deform = 1f;

			//Debug.DrawRay(allAVs[i].worldOriginPoint, Vector3.up);
        }

        // Reset min deform radius, will get reset as we loop through all of the verts.
        minDeformRadius = 0f;

        // Apply mesh effects.
        Mesh tempMesh = myMeshFilter.mesh;
		Vector3[] verts = tempMesh.vertices;
		Color[] colors = new Color[tempMesh.vertexCount];
		for(int i = 0; i < tempMesh.vertexCount; i++){

			// Vert offset
			Vector3 vertRotated = transform.rotation * initialVerts[i];

			float actualSkinDeform = allAVs[i].deform;
            float skinDeformAmount = 3f * actualSkinDeform;
            if (minDeformRadius > skinDeformAmount)
                minDeformRadius = skinDeformAmount;
            verts[i] = initialVerts[i] * (1f + skinDeformAmount);
			colors[i] = Color.Lerp(new Color(0f, 0f, 0.1f), Color.cyan, actualSkinDeform);
		}
		GetComponent<MeshFilter>().mesh.colors = colors;
		GetComponent<MeshFilter>().mesh.vertices = verts;

		GetComponent<MeshFilter>().mesh.RecalculateNormals();

		for(int i = 0; i < targetMeshFilters.Length; i++)
			targetMeshFilters[i].mesh = myMeshFilter.mesh;

		int numVerts = (streamVerts.Count + 1) * streamVertDensity;
		streamLineRenderer.SetVertexCount(numVerts);
		for(int i = 0; i < numVerts; i++) {
			int lastVertIdx = Mathf.FloorToInt((float)i / (float)streamVertDensity);
			int nextVertIdx = Mathf.CeilToInt((float)i / (float)streamVertDensity);

			float curLerp = Mathf.InverseLerp(lastVertIdx, nextVertIdx, (float)i / (float)streamVertDensity);

			Vector3 lastLastVert = GetStreamVertCreeped(lastVertIdx - 1);
			Vector3 lastVert = GetStreamVertCreeped(lastVertIdx);
			Vector3 nextVert = GetStreamVertCreeped(nextVertIdx);
			Vector3 nextNextVert = GetStreamVertCreeped(nextVertIdx + 1);

			Vector3 lastHandleA = Vector3.LerpUnclamped(lastVert, nextVert, 0.5f);
			Vector3 lastHandleB = Vector3.LerpUnclamped(lastVert, lastLastVert, -0.5f);

			Vector3 nextHandleA = Vector3.LerpUnclamped(nextVert, lastVert, 0.5f);
			Vector3 nextHandleB = Vector3.LerpUnclamped(nextVert, nextNextVert, -0.5f);

			Debug.DrawLine(lastVert, lastHandleA, Color.green);
			Debug.DrawLine(nextVert, nextHandleA, Color.cyan);
			Debug.DrawLine(lastVert, nextVert, Color.white);

			Vector3 linePoint = MathUtilities.CalculateBezierPoint(curLerp, lastVert, Vector3.Lerp(lastHandleA, lastHandleB, 0.5f), Vector3.Lerp(nextHandleA, nextHandleB, 0.5f), nextVert);
			streamLineRenderer.SetPosition(i, MathUtilities.CalculateBezierPoint(curLerp, lastVert, Vector3.Lerp(lastHandleA, lastHandleB, 0.5f), Vector3.Lerp(nextHandleA, nextHandleB, 0.5f), nextVert));

			if((Random.Range(0f, 1f) < (0.005f * (1f - Cognogenesis_Networking.Inst.externalEnviroScale))) && IsInside(linePoint)) {
				FoodPellet newFood = new FoodPellet(linePoint);
				newFood.velocity = -newFood.WorldPosition * Random.Range(0.1f, 0.35f);
			}
        }

		//*
		UnityEngine.ParticleSystem.Particle[] particles = new UnityEngine.ParticleSystem.Particle[pSys.particleCount];
		pSys.GetParticles(particles);
		for(int i = 0; i < particles.Length; i++) {
			//particles[i].startLifetime = 10f * Mathf.Lerp(10f, 50f, (float)particles[i].randomSeed / 4294967295f);
			particles[i].startLifetime = 30f;
			float lerp = Mathf.Repeat((particles[i].lifetime / particles[i].startLifetime) + ((float)particles[i].randomSeed / 4294967295f), 1f);

			// ------------------------------------------------------------------------ //
			int lastVertIdx = Mathf.FloorToInt(((float)lerp * (float)numVerts) / (float)streamVertDensity);
			int nextVertIdx = Mathf.CeilToInt(((float)lerp * (float)numVerts) / (float)streamVertDensity);

			float curLerp = Mathf.InverseLerp((float)lastVertIdx, (float)nextVertIdx, ((float)lerp * (float)numVerts) / (float)streamVertDensity);

			Vector3 lastLastVert = GetStreamVertCreeped(lastVertIdx - 1);
			Vector3 lastVert = GetStreamVertCreeped(lastVertIdx);
			Vector3 nextVert = GetStreamVertCreeped(nextVertIdx);
			Vector3 nextNextVert = GetStreamVertCreeped(nextVertIdx + 1);

			Vector3 lastHandleA = Vector3.LerpUnclamped(lastVert, nextVert, 0.5f);
			Vector3 lastHandleB = Vector3.LerpUnclamped(lastVert, lastLastVert, -0.5f);

			Vector3 nextHandleA = Vector3.LerpUnclamped(nextVert, lastVert, 0.5f);
			Vector3 nextHandleB = Vector3.LerpUnclamped(nextVert, nextNextVert, -0.5f);

			Debug.DrawLine(lastVert, lastHandleA, Color.green);
			Debug.DrawLine(nextVert, nextHandleA, Color.cyan);
			Debug.DrawLine(lastVert, nextVert, Color.white);

			Vector3 linePoint = MathUtilities.CalculateBezierPoint(curLerp, lastVert, Vector3.Lerp(lastHandleA, lastHandleB, 0.5f), Vector3.Lerp(nextHandleA, nextHandleB, 0.5f), nextVert);
			particles[i].position = linePoint / 80f;

			particles[i].startColor = Color.white.SetAlpha(Mathf.Sin(lerp * Mathf.PI) * (1f - Cognogenesis_Networking.Inst.externalEnviroScale) * Random.Range(0, 1f));
			// ------------------------------------------------------------------------ //

			particles[i].position += particles[i].rotation3D * 0.05f * (0.2f + ((1f - Cognogenesis_Networking.Inst.externalEnviroScale) * 0.8f));
		}
		pSys.SetParticles(particles, particles.Length);
		//*/


		streamLineRenderer.material.mainTextureScale = new Vector2(10f, 1f);
		streamLineRenderer.material.mainTextureOffset = new Vector2(Time.time * 0.5f, 0f);

		float power = (1f - Cognogenesis_Networking.Inst.externalEnviroScale);

		float width = 15f * (0.2f + Random.Range(power * 0.3f, power * 0.8f));
		streamLineRenderer.SetWidth(width, width);
		Color color = Color.Lerp(Color.white.SetAlpha(0f), Color.white, power);
		streamLineRenderer.SetColors(color, color);



		// Assemblies
		// Keep assemblies up!
		if(assemblies.Count < targetNumAssems){
			Assembly newAssem = Assembly.RandomAssembly(transform.position + (Random.insideUnitSphere * 80f), Quaternion.identity, Random.Range(4, 10));
			assemblies.Add(newAssem);
            newAssem.cognoAmalgam = this;
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

	} // End of Update().


	bool vertSelected = false;
	void OnGUI() {
		return;

		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		float minDist = Vector3.Distance(Camera.main.transform.position, transform.position) - 50f;

		float minDistToCursor = 9999f;
		int closestVert = -1;

		for(int i = 0; i < allAVs.Length; i++) {
			Vector3 vertexScreenPos = Camera.main.WorldToScreenPoint(allAVs[i].worldPoint);
			if(vertexScreenPos.z < minDist) {
				Rect labelRect = MathUtilities.CenteredSquare(vertexScreenPos.x, vertexScreenPos.y, 200f);
				GUI.color = streamVerts.Contains(i)? Color.red : Color.white;
				GUI.Label(labelRect, i.ToString());

				float distToCursor = Vector2.Distance(new Vector2(vertexScreenPos.x, vertexScreenPos.y), Input.mousePosition);
				if(distToCursor < minDistToCursor) {
					minDistToCursor = distToCursor;
					closestVert = i;
				}
			}
		}

		Vector3 bestVertexScreenPos = Camera.main.WorldToScreenPoint(allAVs[closestVert].worldPoint);
		Rect bestLabelRect = MathUtilities.CenteredSquare(bestVertexScreenPos.x, bestVertexScreenPos.y, 200f);
		GUI.color = Color.green;
		GUI.Label(bestLabelRect, closestVert.ToString());

		if(!vertSelected && Input.GetMouseButton(0) && (closestVert > -1)) {
			vertSelected = true;
			streamVerts.Add(closestVert);
			streamCreeps.Add(gameObject.AddComponent<ThreeAxisCreep>());
			streamCreeps[streamCreeps.Count - 1].relaxedness = 5f;
			streamCreeps[streamCreeps.Count - 1].maxCreep = 0.2f;
		}

		if(!Input.GetMouseButton(0))
			vertSelected = false;


		Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
		GUI.skin.label.alignment = TextAnchor.UpperLeft;
		string vertList = "";
		for(int i = 0; i < streamVerts.Count; i++)
			vertList += streamVerts[i] + "\n";
		GUI.Label(screenRect, vertList);
	} // End of OnGUI().


	Vector3 GetStreamVertCreeped(int i) {
		int wrappedIdx = (int)Mathf.Repeat(i, streamVerts.Count);
		return transform.rotation * Vector3.Scale((initialVerts[streamVerts[wrappedIdx]] * 1.35f) + streamCreeps[wrappedIdx].creep, transform.localScale);
	} // End of GetStreamVertCreeped().

    public bool IsInside(Vector3 pt) {

        //check inner sphere first
        if (pt.sqrMagnitude < transform.lossyScale.x * (1 + minDeformRadius) * transform.lossyScale.x * (1 + minDeformRadius))
            return true;

        // now check outer geometry
        bool isInside = false;
        Vector3 transformedPt = new Vector3(pt.x/transform.lossyScale.x, pt.y/transform.lossyScale.y, pt.z/transform.lossyScale.z);
        IcoSphereCreator.Inst.GetProjectedFace(transformedPt, GetComponent<MeshFilter>().mesh.vertices, out isInside);
        return isInside;
    }
} // End of CognoAmalgam.
