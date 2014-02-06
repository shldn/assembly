using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);
	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }

    public GameObject gameObject = null;

    public FoodPellet(){
	gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, worldPosition, Quaternion.identity) as GameObject;
    	allFoodPellets.Add(this);
    }

    public FoodPellet(Vector3 pos)
    {
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Quaternion.identity) as GameObject;
        worldPosition = pos;
        allFoodPellets.Add(this);
    }

    //create new food node and add it to list - done through ctor
    public static void AddNewFoodPellet(){
    	new FoodPellet();
    	Debug.Log("New food added\n");
    }

    public void Destroy()
    {
        Object.Destroy(gameObject);
    }

}