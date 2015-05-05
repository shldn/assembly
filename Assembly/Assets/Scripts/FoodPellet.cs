using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet {

	public static HashSet<FoodPellet> all = new HashSet<FoodPellet>();

	private Vector3 worldPosition = Vector3.zero;
    public Vector3 WorldPosition { get { return worldPosition; } set { worldPosition = value; transform.position = value; } }
	public Quaternion worldRotation = Quaternion.identity;

	Transform transform = null;
    public GameObject gameObject { get { return (transform != null) ? transform.gameObject : null; } }

	private static Octree<FoodPellet> allFoodTree;
    public static Octree<FoodPellet> AllFoodTree{ 
        get{
            if(allFoodTree == null){
                allFoodTree = new Octree<FoodPellet>(new Bounds(Vector3.zero, 2.0f * NodeController.Inst.maxWorldSize * Vector3.one), (FoodPellet x) => x.worldPosition, 5);
			}
            return allFoodTree;
        }
        set{
            allFoodTree = value;
        }
    }

	public float energy = 10f;
	float maxEnergy = 10f;
	public bool cull = false;

    // For Olympics
    public Assembly owner = null; // to restrict energy consumption to just this entity


	public FoodPellet(Vector3 position, Assembly owner_ = null){
		worldPosition = position;
		worldRotation = Random.rotation;
		transform = MonoBehaviour.Instantiate(NodeController.Inst.physFoodPrefab, worldPosition, worldRotation) as Transform;

		all.Add(this);
		AllFoodTree.Insert(this);

		maxEnergy = energy;
        owner = owner_;
	} // constructor


	public void Update(){
		transform.localScale = Vector3.one * (energy / maxEnergy);

		if(energy < 0f){
			NodeController.Inst.AdvanceWorldTick();
			cull = true;
		}

		// Destroy nodes outside of worlds
		if(Mathf.Sqrt(Mathf.Pow(worldPosition.x / NodeController.Inst.worldSize.x, 2f) + Mathf.Pow(worldPosition.y / NodeController.Inst.worldSize.y, 2f) + Mathf.Pow(worldPosition.z / NodeController.Inst.worldSize.z, 2f)) > 1f){
			cull = true;
		}
	} // End of Update().


	public void Destroy(){
        all.Remove(this);
        if(!AllFoodTree.Remove(this)){
            if(!AllFoodTree.Remove(this, false))
                Debug.LogError("Failed to remove Food Node: " + worldPosition.ToString());
        }
        Object.Destroy(transform.gameObject);
    }

    public static void DestroyAll()
    {
        try
        {
            foreach (FoodPellet food in all)
                if( food.transform != null )
                    Object.Destroy(food.transform.gameObject);
        }
        catch(System.Exception e)
        {
            Debug.LogError("Exception destroying food node: " + e.ToString());
        }

        allFoodTree = null;
        all.Clear();
    }

} // PhysFood
