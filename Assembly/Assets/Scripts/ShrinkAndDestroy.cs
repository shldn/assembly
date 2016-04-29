using UnityEngine;
using System.Collections;

public class ShrinkAndDestroy : MonoBehaviour {

	float curScale = 1f;
	float shrinkRate = 1f;
	Vector3 initScale = Vector3.one;


	void Start () {
		initScale = transform.localScale;
		shrinkRate = Random.Range(0.25f, 2f);
	} // End of Start().
	

	void Update () {
		curScale = Mathf.Lerp(curScale, 0f, NodeController.physicsStep * shrinkRate);
		transform.localScale = initScale * curScale;

		if(curScale < 0.05f)
			Destroy(gameObject);
	} // End of Update().

} // End of ShrinkAndDestroy.
