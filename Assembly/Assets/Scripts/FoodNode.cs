using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodNode{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);
	private static List<FoodNode> allFoodNodes = new List<FoodNode>();
    public static List<FoodNode> GetAll() { return allFoodNodes; }

    public GameObject gameObject = null;

    public FoodNode(){
	gameObject = GameObject.Instantiate(PrefabManager.Inst.foodNode, worldPosition, Quaternion.identity) as GameObject;
    	allFoodNodes.Add(this);
    }

    //create new food node and add it to list - done through ctor
    public static void AddNewFoodNode(){
    	new FoodNode();
    	Debug.Log("New food added\n");
    }

}