﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet : MonoBehaviour {

    private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }

    public float maxCalories = 10f;
    public float calories = 10f;

	void Awake(){
		transform.rotation = Random.rotation;
		
		rigidbody.AddForce(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
		rigidbody.AddTorque(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));

        allFoodPellets.Add(this);
	}

	void Update(){
        //for(int i = 0; i < Node.GetAll().Count; i++){
        //    Node currentNode = Node.GetAll()[i];
        //    currentNode.calories += Time.deltaTime * (3f / Mathf.Pow(Vector3.Distance(transform.position, currentNode.transform.position), 2));
        //}
        transform.localScale = Vector3.one * (calories / maxCalories);

        if(calories <= 0f)
            Destroy();
	}
	
    public void Destroy(){
        FoodPellet.allFoodPellets.Remove(this);
		Destroy(gameObject);
    }
}