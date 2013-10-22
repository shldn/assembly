using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet : MonoBehaviour {

	void Start(){
		transform.rotation = Random.rotation;
		
		rigidbody.AddForce(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
		rigidbody.AddTorque(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
	}
	
	void Update(){
		for(int i = 0; i < Node.GetAll().Count; i++){
            Node currentNode = Node.GetAll()[i];
			currentNode.calories += Time.deltaTime * (3f / Mathf.Pow(Vector3.Distance(transform.position, currentNode.transform.position), 2));
		}
	}
	
	public void Burst(){
		for(int i = 0; i < Node.GetAll().Count; i++){
            Node currentNode = Node.GetAll()[i];
			currentNode.calories += 30f / Mathf.Pow(Vector3.Distance(transform.position, currentNode.transform.position), 2);
		}
		Destroy(gameObject);
	}

    public void Destroy(){
		Destroy(gameObject);
    }
}
