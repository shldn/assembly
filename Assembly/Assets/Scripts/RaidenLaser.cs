using UnityEngine;
using System.Collections;

public class RaidenLaser : MonoBehaviour {

	LineRenderer lineRenderer;
	public float resolution = 0.5f;
	public Material material = null;
	public float width = 10f;

	public Transform startTrans;
	public Transform endTrans;

	Vector3 pos0, pos1, pos2, pos3;

	float textureRunner = 0f;
	float fadeIn = 0.01f;


	void Start(){
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = material;
	} // End of Start().
	

	void Update(){
		textureRunner += Time.deltaTime;

		fadeIn = Mathf.MoveTowards(fadeIn, (startTrans && endTrans)? 1f : 0f, NodeController.physicsStep * 0.2f);

		if(startTrans)
			pos0 = startTrans.position;

		if(endTrans) {
			pos2 = endTrans.position - (endTrans.position - startTrans.position).normalized;
			pos3 = endTrans.position;
		}

		float distBetween = Vector3.Distance(pos0, pos3);
		int vertexCount = Mathf.CeilToInt(resolution * distBetween + 1);
		lineRenderer.material.mainTextureScale = new Vector2(distBetween * 0.02f, 1f);
		lineRenderer.material.mainTextureOffset = new Vector2(textureRunner * 0.5f, 0f);
		lineRenderer.SetVertexCount(vertexCount);
		lineRenderer.SetWidth(width, width);
		lineRenderer.SetColors(new Color(1f, 1f, 1f, fadeIn), new Color(1f, 1f, 1f, fadeIn));

		if(startTrans)
			pos1 = startTrans.position + (startTrans.forward * distBetween * 0.5f);

		for(int i = 0; i < vertexCount; i++)
			lineRenderer.SetPosition(i, MathUtilities.CalculateBezierPoint((float)i / (float)vertexCount, pos0, pos1, pos2, pos3));

		if(fadeIn == 0f)
			Destroy(gameObject);
	} // End of Update().
} // End of RaidenLaser.
