using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysFood {

	public static HashSet<PhysFood> all = new HashSet<PhysFood>();

	public Vector3 worldPosition = Vector3.zero;
	public Quaternion worldRotation = Quaternion.identity;

	Transform transform = null;

	private static Octree<PhysFood> allFoodTree;
    public static Octree<PhysFood> AllFoodTree{ 
        get{
            if(allFoodTree == null){
                allFoodTree = new Octree<PhysFood>(new Bounds(Vector3.zero, 2.0f * PhysNodeController.Inst.WorldSize * Vector3.one), (PhysFood x) => x.worldPosition, 5);
			}
            return allFoodTree;
        }
        set{
            allFoodTree = value;
        }
    }

	public float energy = 100f;
	float maxEnergy = 100f;
	public bool cull = false;


	public PhysFood(Vector3 position){
		worldPosition = position;
		worldRotation = Random.rotation;
		transform = MonoBehaviour.Instantiate(PhysNodeController.Inst.physFoodPrefab, worldPosition, worldRotation) as Transform;

		all.Add(this);
		AllFoodTree.Insert(this);

		maxEnergy = energy;
	} // constructor


	public void Update(){
		transform.localScale = Vector3.one * (energy / maxEnergy);

		if(energy < 0f)
			cull = true;
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
        foreach(PhysFood food in all)
        {
            Object.Destroy(food.transform.gameObject);

            // Need a Clear function for Octree.
            if(!AllFoodTree.Remove(food)){
                if(!AllFoodTree.Remove(food, false))
                    Debug.LogError("Failed to remove Food Node: " + food.worldPosition.ToString());
            }
        }

        all.Clear();
    }

} // PhysFood
