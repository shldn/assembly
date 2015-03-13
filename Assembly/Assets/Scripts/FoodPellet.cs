using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class FoodPellet{

    public Vector3 worldPosition {
        get {
            if(gameObject)
                return gameObject.transform.position;
            return Vector3.zero;
        }
        set {
            gameObject.transform.position = value;
        }
    }

	private static List<FoodPellet> allFoodPellets = new List<FoodPellet>();
    public static List<FoodPellet> GetAll() { return allFoodPellets; }
    private static Octree<FoodPellet> allFoodTree;
    public static Octree<FoodPellet> AllFoodTree
    { 
        get {
            if( allFoodTree == null )
                allFoodTree = new Octree<FoodPellet>(new Bounds(Vector3.zero, 2.0f * GameManager.Inst.worldSize * Vector3.one), (FoodPellet x) => x.worldPosition, 15);
            return allFoodTree;
        }
        set{
            allFoodTree = value;
        }
    }
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
        allFoodTree.Insert(this);
    }

    public FoodPellet(Vector3 pos){
        
        gameObject = GameObject.Instantiate(PrefabManager.Inst.foodPellet, pos, Random.rotation) as GameObject;
        //glow = gameObject.transform.Find("glow").renderer;

        particleObject = gameObject.GetComponentInChildren<ParticleSystem>();
        //particleGlow = gameObject.transform.Find("Particle System").renderer;

        //worldPosition = pos;
        gameObject.transform.position = pos;
        worldPosition = pos;
        //currentEnergy = random.Next(0,10); //not all food are created equal
        allFoodPellets.Add(this);
        AllFoodTree.Insert(this);

    }

    //create new food node and add it to list
    public static FoodPellet AddNewFoodPellet(){
    	return new FoodPellet();
    }

    public static FoodPellet AddNewFoodPellet(Vector3 pos){
        return new FoodPellet(pos);
    }

    //create random food node
    public static void AddRandomFoodPellet(){
        Vector3 pos = Random.insideUnitSphere * GameManager.Inst.worldSize;
        new FoodPellet(pos);
    }

    public void Update(){
        particleEmitCooldown -= Time.deltaTime;

        gameObject.transform.localScale = Vector3.one * (currentEnergy / MAX_ENERGY);

        if(currentEnergy <= 0f)
            Destroy();

        worldPosition = gameObject.transform.position;

        for(int i = 0; i < GetAll().Count; i++){
            if(GetAll()[i] == this)
                continue;

            Vector3 vecToPellet = gameObject.transform.position - GetAll()[i].worldPosition;
            gameObject.rigidbody.AddForce(vecToPellet.normalized * (1000f / Mathf.Pow(vecToPellet.sqrMagnitude, 2f)));
        }

        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Vector3 vecToAssem = gameObject.transform.position - Assembly.GetAll()[i].WorldPosition;
            gameObject.rigidbody.AddForce(vecToAssem.normalized * (1000f / Mathf.Pow(vecToAssem.sqrMagnitude, 2f)));
        }
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
        transferEffects.Add(newEffect);
        newEffect.receivingNode = receivingNode;
        newEffect.sendingPellet = this;


    } // End of EnergyEffect().


    public void Destroy(){

        particleObject.enableEmission = false;
        particleObject.transform.parent = null;
        particleObject.gameObject.AddComponent("ParticleEffects");

        allFoodPellets.Remove(this);
        if (!AllFoodTree.Remove(this))
        {
            if (!AllFoodTree.Remove(this, false))
                Debug.LogError("Failed to remove Food Node: " + gameObject.transform.position.ToString());
        }
        Object.Destroy(gameObject);
    }

} // End of FoodPellet.