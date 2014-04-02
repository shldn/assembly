using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FoodType { distance = 1, hit = 2, passive = 4}

public enum FoodTypeSelection{  distance = 1, hit = 2, passive = 4,
    disAndHit = distance | hit, 
    passAndHit = passive | hit,
    disAndPass = distance | passive,
    all = distance | hit | passive
}

public class FoodPellet{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);

	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }
    public static int MAX_FOOD = 1;

    public static FoodTypeSelection ftFlag = 0x0;
    public static FoodTypeSelection ftPrevFlag = 0x0;
    public static bool ftDistanceEnabled = true;
    public static bool ftPassiveEnabled = false;
    public static bool ftCollisionEnabled = false;

    public static Renderer glow = null;

    public Renderer billboard = null;

    public static float MAX_ENERGY = 10.0f;
    public float currentEnergy = MAX_ENERGY;
    
    //how food can be absorb by assemblies
    public FoodType foodType = FoodType.distance;

    //random number generator
    private static System.Random random = new System.Random();

    public GameObject gameObject = null;

    public FoodPellet(){
        UpdateFoodType();

        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, worldPosition, Random.rotation) as GameObject;
        glow = gameObject.transform.Find("glow").renderer;

    	//currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
    }

    public FoodPellet(Vector3 pos){
        UpdateFoodType();
        
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Random.rotation) as GameObject;
        glow = gameObject.transform.Find("glow").renderer;

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
        if(ftFlag != ftPrevFlag)
            UpdateFoodType();
    }

    //update food type flag based on UI
    public static void UpdateEnabledFoodType(){
        if(ftDistanceEnabled)
            ftFlag |= FoodTypeSelection.distance;
        else
            ftFlag &= ~FoodTypeSelection.distance;
        
        if(ftPassiveEnabled)
            ftFlag |= FoodTypeSelection.passive;
        else
            ftFlag &= ~FoodTypeSelection.passive;
        
        if(ftCollisionEnabled)
            ftFlag |= FoodTypeSelection.hit;
        else
            ftFlag &= ~FoodTypeSelection.hit;
        //MonoBehaviour.print("The flag is " + ftFlag);
    }

    //update each node foodtype based on flag
    public void UpdateFoodType(){

        glow = gameObject.transform.Find("glow").renderer;


        if(FoodPellet.ftFlag == FoodType.hit)
            foodType = FoodType.hit;
        else if(FoodPellet.ftFlag == FoodTypeSelection.distance)
            foodType = FoodType.hit;
        else if(FoodPellet.ftFlag == FoodTypeSelection.passive)
            foodType = FoodType.passive;
        else if(FoodPellet.ftFlag == FoodTypeSelection.all){
            int selecter = random.Next(3);
            if(selecter == 0)
                foodType = FoodType.hit;
            else if( selecter == 1)
                foodType = FoodType.distance;
            else
                foodType = FoodType.passive;
        } else if( FoodPellet.ftFlag == FoodTypeSelection.passAndHit){
            int selecter = random.Next(2);
            if(selecter == 0)
                foodType = FoodType.hit;
            else if( selecter == 1)
                foodType = FoodType.passive;
        } else if( FoodPellet.ftFlag == FoodTypeSelection.disAndHit){
            int selecter = random.Next(2);
            if(selecter == 0)
                foodType = FoodType.hit;
            else if( selecter == 1)
                foodType = FoodType.distance;
        } else if( FoodPellet.ftFlag == FoodTypeSelection.disAndPass){
            int selecter = random.Next(2);
            if(selecter == 0)
                foodType = FoodType.distance;
            else if( selecter == 1)
                foodType = FoodType.passive;
        }else
            foodType = FoodType.distance; //default to distance


        switch(foodType){
            case FoodType.distance :
                glow.material.SetColor("_TintColor", Color.blue);
                break;
            case FoodType.hit:
                glow.material.SetColor("_TintColor", Color.green);
                break;
            case FoodType.passive :
                glow.material.SetColor("_TintColor", Color.green);
                break;
        }
    }

    public void Destroy(){
        allFoodPellets.Remove(this);
        Object.Destroy(gameObject);
    }

}