using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysFood {

	public static HashSet<PhysFood> allPhysFood = new HashSet<PhysFood>();

	public Vector3 worldPosition = Vector3.zero;
	public Quaternion worldRotation = Quaternion.identity;

	Transform transform = null;

	private static Octree<PhysFood> allFoodTree;
    public static Octree<PhysFood> AllFoodTree{ 
        get{
            if(allFoodTree == null){
                allFoodTree = new Octree<PhysFood>(new Bounds(Vector3.zero, 2.0f * PhysNodeController.Inst.worldSize * Vector3.one), (PhysFood x) => x.worldPosition, 5);
			}
            return allFoodTree;
        }
        set{
            allFoodTree = value;
        }
    }


	public PhysFood(Vector3 position){
		worldPosition = position;
		worldRotation = Random.rotation;
		transform = MonoBehaviour.Instantiate(PhysNodeController.Inst.physFoodPrefab, worldPosition, worldRotation) as Transform;

		allPhysFood.Add(this);
		AllFoodTree.Insert(this);
	} // constructor


	public void Destroy(){
        allPhysFood.Remove(this);
        if(!AllFoodTree.Remove(this)){
            if(!AllFoodTree.Remove(this, false))
                Debug.LogError("Failed to remove Food Node: " + worldPosition.ToString());
        }
        Object.Destroy(transform.gameObject);
    }

} // PhysFood
