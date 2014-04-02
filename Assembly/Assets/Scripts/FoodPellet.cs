using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FoodType { distance, hit, mix, passive}

public class FoodPellet{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);
	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }
    public static int MAX_FOOD = 1;
    public static int userFoodProp = 0;// 0, 1, 2 FoodType

    public static float MAX_ENERGY = 10.0f;
    public float currentEnergy = MAX_ENERGY;
    
    //how food can be absorb by assemblies
    public FoodType foodType = FoodType.distance;

    //random number generator
    private static System.Random random = new System.Random();
    //not needed for now

    public GameObject gameObject = null;

    public FoodPellet(){
        if(FoodPellet.userFoodProp == 1){
            foodType = FoodType.hit;
        } else if(FoodPellet.userFoodProp == 2){
            if(random.Next(2)  == 0)
                foodType = FoodType.hit;
            else
                foodType = FoodType.distance;
        }

        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, worldPosition, Random.rotation) as GameObject;
    	//currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
    }

    public FoodPellet(Vector3 pos){
        if(FoodPellet.userFoodProp == 1){
            foodType = FoodType.hit;
        } else if(FoodPellet.userFoodProp == 2){
            if(random.Next(2)  == 0)
                foodType = FoodType.hit;
            else
                foodType = FoodType.distance;
        }
        
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Random.rotation) as GameObject;
        worldPosition = pos;
        //currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
    }

    //create new food node and add it to list
    public static FoodPellet AddNewFoodPellet(){
    	return new FoodPellet();
    }

    //create random food node
    public static void AddRandomFoodPellet(){
        int min = -50, max = 50; //range, can be chnaged later
        Vector3 pos = new Vector3(random.Next(min, max), random.Next(min, max) ,random.Next(min, max) );
        new FoodPellet(pos);
    }

    public void UpdateTransform(){
        gameObject.transform.position = worldPosition;
        gameObject.transform.localScale = Vector3.one * (currentEnergy / MAX_ENERGY);
        
        //updateFoodType
        UpdateFoodType();
    }

    public void UpdateFoodType(){
        if(FoodPellet.userFoodProp == 1){
            foodType = FoodType.hit;
        } else if(FoodPellet.userFoodProp == 2){
            if(random.Next(2)  == 0)
                foodType = FoodType.hit;
            else
                foodType = FoodType.distance;
        } else{
            foodType = FoodType.distance; //default to distance
        }
    }
    public void Destroy(){
        allFoodPellets.Remove(this);
        Object.Destroy(gameObject);
    }

}