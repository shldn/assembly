using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CognoAmalgam : MonoBehaviour {

	public float deformFluxRate = 0.0025f;
	MeshFilter myMeshFilter = null;
	public MeshFilter[] targetMeshFilters;

	int[] streamVerts = new int[0];
	int streamVertDensity = 30;
	LineRenderer streamLineRenderer;
	ThreeAxisCreep[] streamCreeps;

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


		/*
		int numStreamVerts = 10;
		streamVerts = new int[numStreamVerts];
		streamCreeps = new ThreeAxisCreep[numStreamVerts];
		for(int i = 0; i < streamVerts.Length; i++) {
			streamVerts[i] = Random.Range(0, myMeshFilter.mesh.vertexCount);
			streamCreeps[i] = gameObject.AddComponent<ThreeAxisCreep>();
			streamCreeps[i].relaxedness = 5f;
		}
		//*/

		//*
		streamVerts = new int[] {0, 1, 2};
		streamCreeps = new ThreeAxisCreep[streamVerts.Length];
		for(int i = 0; i < streamVerts.Length; i++) {
			streamCreeps[i] = gameObject.AddComponent<ThreeAxisCreep>();
			streamCreeps[i].relaxedness = 5f;
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
			for(int j = 0; j < HandCognoOutside.allHandMovements.Count; j++) {
				//float sqrDist = Vector3.Distance(HandCognoOutside.allHandMovements[j].hand_model.palm.position, allAVs[i].worldOriginPoint);
				//float sqrDist = Vector3.SqrMagnitude(HandCognoOutside.allHandMovements[j].hand_model.palm.position - allAVs[i].worldOriginPoint);
				//allAVs[i].deform += 1f / (sqrDist * 0.5f);
				Vector3 avFingerPos = Vector3.zero;
				for(int k = 0; k < 5; k++)
					avFingerPos += HandCognoOutside.allHandMovements[j].hand_model.fingers[k].bones[2].position;
				
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

		// Apply mesh effects.
		Mesh tempMesh = myMeshFilter.mesh;
		Vector3[] verts = tempMesh.vertices;
		Color[] colors = new Color[tempMesh.vertexCount];
		for(int i = 0; i < tempMesh.vertexCount; i++){

			// Vert offset
			Vector3 vertRotated = transform.rotation * initialVerts[i];

			float actualSkinDeform = allAVs[i].deform;

			verts[i] = initialVerts[i] * (1f + (actualSkinDeform * 3f));
			colors[i] = Color.Lerp(new Color(0f, 0f, 0.1f), Color.cyan, actualSkinDeform);
		}
		GetComponent<MeshFilter>().mesh.colors = colors;
		GetComponent<MeshFilter>().mesh.vertices = verts;

		GetComponent<MeshFilter>().mesh.RecalculateNormals();

		for(int i = 0; i < targetMeshFilters.Length; i++)
			targetMeshFilters[i].mesh = myMeshFilter.mesh;

		int numVerts = (streamVerts.Length + 1) * streamVertDensity;
		streamLineRenderer.SetVertexCount(numVerts);
		for(int i = 0; i < numVerts; i++) {
			int lastVertIdx = Mathf.FloorToInt((float)i / (float)streamVertDensity);
			int nextVertIdx = Mathf.CeilToInt((float)i / (float)streamVertDensity);

			float curLerp = Mathf.InverseLerp(lastVertIdx, nextVertIdx, (float)i / (float)streamVertDensity);

			Vector3 lastLastVert = GetStreamVertCreeped(lastVertIdx - 1);
			Vector3 lastVert = GetStreamVertCreeped(lastVertIdx);
			Vector3 nextVert = GetStreamVertCreeped(nextVertIdx);
			Vector3 nextNextVert = GetStreamVertCreeped(nextVertIdx + 1);

			Vector3 lastHandle = Vector3.Lerp(lastLastVert, lastVert, 0.5f);
			Vector3 nextHandle = Vector3.LerpUnclamped(lastVert, nextVert, 1.5f);

			streamLineRenderer.SetPosition(i, MathUtilities.CalculateBezierPoint(curLerp, lastVert, lastHandle, nextHandle, nextVert));

			//Debug.DrawRay(Vector3.Lerp(lastVert, nextVert, curLerp), Vector3.up, Color.Lerp(Color.green, Color.cyan, curLerp));

			//print("last: " + lastVertIdx + " next: " + nextVertIdx + " lerp: " + curLerp);
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

			Vector3 lastHandle = Vector3.Lerp(lastLastVert, lastVert, 0.5f);
			Vector3 nextHandle = Vector3.LerpUnclamped(lastVert, nextVert, 1.5f);

			particles[i].position = MathUtilities.CalculateBezierPoint(curLerp, lastVert, lastHandle, nextHandle, nextVert) / 80f;

			if(particles[i].lifetime < 30f)
				particles[i].startColor = Color.white.SetAlpha(Mathf.Sin(lerp * Mathf.PI));
			// ------------------------------------------------------------------------ //

			particles[i].position += particles[i].rotation3D * 0.05f;
		}
		pSys.SetParticles(particles, particles.Length);
		//*/

		streamLineRenderer.material.mainTextureScale = new Vector2(10f, 1f);
		streamLineRenderer.material.mainTextureOffset = new Vector2(Time.time * 0.5f, 0f);

	} // End of Update().


	Vector3 GetStreamVertCreeped(int i) {
		int wrappedIdx = (int)Mathf.Repeat(i, streamVerts.Length);
		return transform.rotation * Vector3.Scale((initialVerts[streamVerts[wrappedIdx]] * 1.2f) /*+ streamCreeps[wrappedIdx].creep*/, transform.localScale);
	} // End of GetStreamVertCreeped().

} // End of CognoAmalgam.
