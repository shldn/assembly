using UnityEngine;
using System.Collections;

public class RaidenLaser : MonoBehaviour {

	LineRenderer lineRenderer;
	float resolution = 0.1f;
	public Material material = null;
	public float width = 10f;

	public Transform startTrans;
	public Transform endTrans;

	Vector3 pos0, pos1, pos2, pos3;

	float textureRunner = 0f;
	float fadeIn = 0.01f;

	public Color color = Color.white;

	float scaleCoefficient = 1f;

	ParticleSystem pSys;
	


	void Start(){
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = material;
		scaleCoefficient = Random.Range(0.5f, 2f);

		pSys = GetComponent<ParticleSystem>();
		UnityEngine.ParticleSystem.Particle[] particles = new UnityEngine.ParticleSystem.Particle[pSys.particleCount];
		pSys.GetParticles(particles);
		for(int i = 0; i < particles.Length; i++) {
			particles[i].startColor = Color.clear;
		}
		pSys.SetParticles(particles, particles.Length);
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
		lineRenderer.material.mainTextureScale = new Vector2(distBetween * 0.01f * scaleCoefficient, 1f);
		lineRenderer.material.mainTextureOffset = new Vector2(textureRunner * 0.5f, 0f);
		lineRenderer.SetVertexCount(vertexCount);
		lineRenderer.SetWidth(width, width);

		if(startTrans)
			pos1 = startTrans.position + (startTrans.forward * distBetween * 0.5f);

		for(int i = 0; i < vertexCount; i++)
			lineRenderer.SetPosition(i, MathUtilities.CalculateBezierPoint((float)i / (float)vertexCount, pos0, pos1, pos2, pos3));

		UnityEngine.ParticleSystem.Particle[] particles = new UnityEngine.ParticleSystem.Particle[pSys.particleCount];
		pSys.GetParticles(particles);
		for(int i = 0; i < particles.Length; i++) {
			particles[i].startLifetime = distBetween * 0.001f * Mathf.Lerp(10f, 50f, (float)particles[i].randomSeed / 4294967295f);
			float lerp = particles[i].remainingLifetime / particles[i].startLifetime;
			particles[i].position = MathUtilities.CalculateBezierPoint(lerp, pos0, pos1, pos2, pos3);
			particles[i].position += particles[i].rotation3D * 4f;
			particles[i].startColor = Color.Lerp(color.SetAlpha(fadeIn), new Color(1f, 1f, 1f, fadeIn), lerp);
		}
		pSys.SetParticles(particles, particles.Length);

		if(fadeIn == 0f)
			Destroy(gameObject);

		lineRenderer.SetColors(color.SetAlpha(fadeIn), new Color(0f, 0.25f, 1f, fadeIn));
	} // End of Update().
} // End of RaidenLaser.
