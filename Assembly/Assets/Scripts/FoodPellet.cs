using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);
	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }

    public static float MAX_ENERGY = 10.0f;
    public float currentEnergy = MAX_ENERGY;
    
    //random number generator
    private static System.Random random = new System.Random();

    public GameObject gameObject = null;

    public FoodPellet(){
	gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, worldPosition, Quaternion.identity) as GameObject;
    	currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
    }

    public FoodPellet(Vector3 pos){
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Quaternion.identity) as GameObject;
        worldPosition = pos;
        currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
    }

    //create new food node and add it to list
    public static void AddNewFoodPellet(){
    	new FoodPellet();
    	Debug.Log("New food added\n");
    }

    //create random food node
    public static void AddRandomFoodPellet(){
        int min = -25, max = 25; //range, can be chnaged later
        Vector3 pos = new Vector3(random.Next(min, max), random.Next(min, max) ,random.Next(min, max) );
        new FoodPellet(pos);
        Debug.Log("New food added\n");
    }

    public void Destroy()
    {
        allFoodPellets.Remove(this);
        Object.Destroy(gameObject);
    }

}