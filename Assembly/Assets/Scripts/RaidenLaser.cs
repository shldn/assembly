using UnityEngine;
using System.Collections;

public class RaidenLaser : MonoBehaviour {

	LineRenderer lineRenderer;
	public int resolution = 10;
	public Material material = null;
	public float width = 1f;

	public Transform startTrans;
	public Transform endTrans;

	float textureRunner = 0f;


	void Start(){
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = material;
	} // End of Start().
	

	void Update(){
		textureRunner += Time.deltaTime;

		float distBetween = Vector3.Distance(startTrans.position, endTrans.position);
		int vertexCount = Mathf.CeilToInt(resolution * distBetween + 1);
		lineRenderer.material.mainTextureScale = new Vector2(distBetween * 0.5f, 1f);
		lineRenderer.material.mainTextureOffset = new Vector2(textureRunner, 0f);
		lineRenderer.SetVertexCount(vertexCount);
		lineRenderer.SetWidth(width, width);
		for(int i = 0; i < (vertexCount + 1); i++)
			lineRenderer.SetPosition(i, MathUtilities.CalculateBezierPoint((float)i / (float)vertexCount, startTrans.position, startTrans.position + (startTrans.forward * distBetween * 0.5f), endTrans.position - (endTrans.position - startTrans.position).normalized, endTrans.position));
	} // End of Update().
} // End of RaidenLaser.
