using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FoodPellet{
	
	public Vector3 worldPosition = new Vector3( 0, 9 , 0);

	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }
    public static int MAX_FOOD = 1;

    float particleEmitCooldown = 0f;

    List<EnergyTransferEffect> transferEffects = new List<EnergyTransferEffect>();


    public static float MAX_ENERGY = 50.0f;
    public float currentEnergy = MAX_ENERGY;

    //random number generator
    private static System.Random random = new System.Random();

    public GameObject gameObject = null;
    private ParticleSystem particleObject;
    private Renderer particleGlow = null;

    public FoodPellet(){


        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, worldPosition, Random.rotation) as GameObject;

        particleObject = gameObject.GetComponentInChildren<ParticleSystem>();

        allFoodPellets.Add(this);
    }

    public FoodPellet(Vector3 pos){
        
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Random.rotation) as GameObject;
        //glow = gameObject.transform.Find("glow").renderer;

        particleObject = gameObject.GetComponentInChildren<ParticleSystem>();
        //particleGlow = gameObject.transform.Find("Particle System").renderer;

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
        Vector3 pos = MathUtilities.RandomVector3Sphere(80f);
        new FoodPellet(pos);
    }

    public void Update(){
        particleEmitCooldown -= Time.deltaTime;

        gameObject.transform.position = worldPosition;
        gameObject.transform.localScale = Vector3.one * (currentEnergy / MAX_ENERGY);

        if(currentEnergy <= 0f)
            Destroy();

    }


    public void EnergyEffect(SenseNode receivingNode){

        // Check to see if transfer effect already exists.
        for (int i = 0; i < transferEffects.Count; i++){   
            if(transferEffects[i].receivingNode == receivingNode){
                transferEffects[i].disableCheck = false;
                return;
            }
        }

        // If not, generate a transfer effect.
        GameObject newEffectTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.energyTransferEffect) as GameObject;
        EnergyTransferEffect newEffect = newEffectTrans.GetComponent<EnergyTransferEffect>();
        newEffect.receivingNode = receivingNode;
        newEffect.sendingPellet = this;

    } // End of EnergyEffect().


    public void Destroy(){

        particleObject.enableEmission = false;
        particleObject.transform.parent = null;
        particleObject.gameObject.AddComponent("ParticleEffects");

        allFoodPellets.Remove(this);
        Object.Destroy(gameObject);
    }

} // End of FoodPellet.