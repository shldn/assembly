using UnityEngine;
using System.Collections;

public class AmalgamArtery : MonoBehaviour {

	TubeRenderer tubeRenderer;
	public float resolution = 0.5f;
	float width = 1f;
	public Amalgam amalgam;

	float textureRunner = 0f;

	public int[] vertices;


	void Start(){
		tubeRenderer = gameObject.GetComponent<TubeRenderer>();
	} // End of Start().
	

	void Update(){
		textureRunner += Time.deltaTime;

		tubeRenderer.material.mainTextureScale = new Vector2(10f, 1f);
		tubeRenderer.material.mainTextureOffset = new Vector2(textureRunner * 0.5f, 0f);
		//tubeRenderer.SetVertexCount(vertices.Length);
		//tubeRenderer.SetWidth(width, width);

		int resolution = 10;
		Vector3[] points = new Vector3[vertices.Length * resolution];
		for(int i = 0; i < points.Length; i++){
			int lastPoint = Mathf.Clamp(Mathf.FloorToInt((float)i / (float)resolution), 0, vertices.Length - 1);
			int nextPoint = Mathf.Clamp(Mathf.CeilToInt((float)i / (float)resolution), 0, vertices.Length - 1);
			float lerpRaw = (float)(i % resolution) / (float)resolution;
			float lerp = 0.5f - (Mathf.Cos(lerpRaw * 2 * Mathf.PI) * 0.5f);
			points[i] = Vector3.Lerp(amalgam.GetVertexWorldPoint(vertices[lastPoint]), amalgam.GetVertexWorldPoint(vertices[nextPoint]), lerp);
		}

		tubeRenderer.crossSegments = 6;
		tubeRenderer.SetPoints(points, width, Color.white);
	} // End of Update().
} // End of RaidenLaser.
