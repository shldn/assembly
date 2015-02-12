using UnityEngine;
using System.Collections;

public class AnimationControl : MonoBehaviour {
	Animator anim;
	public GameObject tentecal;
	public Transform target01;
	public Transform target02;
	public Transform target03;
	public Transform target04;
	public float distance01;
	public float distance02;
	public float distance03;
	public float distance04;
	// Use this for initialization
	void Start () {
		anim = tentecal.GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

		distance01 = Vector3.Distance (transform.position, target01.position);
		distance02 = Vector3.Distance (transform.position, target02.position);
		distance03 = Vector3.Distance (transform.position, target03.position);
		distance04 = Vector3.Distance (transform.position, target04.position);
		anim.SetFloat ("distance01", distance01);
		anim.SetFloat ("distance02", distance02);
		anim.SetFloat ("BlendTran", distance03);
		anim.SetFloat ("distance04", distance04);
	}
}
