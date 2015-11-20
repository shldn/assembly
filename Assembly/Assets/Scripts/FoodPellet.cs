using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet {

	public static List<FoodPellet> all = new List<FoodPellet>();
	public Amalgam amalgam = null;

	private Vector3 worldPosition = Vector3.zero;
    public Vector3 WorldPosition { get { return worldPosition; } set { worldPosition = value; if (viewer != null) { viewer.Position = value; }  } }

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

	float energy = 10f;
    public float Energy { get { return energy; } set { energy = value; viewer.Scale = energy / maxEnergy; } }
	float maxEnergy = 10f;
	public bool cull = false;

    // For Olympics
    public Assembly owner = null; // to restrict energy consumption to just this entity

    FoodPelletViewer viewer = null;
    public bool Visible
    {
        get { return viewer.Visible; }
        set { viewer.Visible = value; }
    }

	public FoodPellet(Vector3 position, Assembly owner_ = null){
		worldPosition = position;
        viewer = new FoodPelletViewer(worldPosition);
        viewer.Scale = 1;

		all.Add(this);
		AllFoodTree.Insert(this);

		maxEnergy = energy;
        owner = owner_;
	} // constructor


	public void Update(){

		if(energy < 0f){
			NodeController.Inst.AdvanceWorldTick();
			cull = true;
		}

        // Destroy nodes outside of worlds
        if (!WithinBoundary(worldPosition) && Environment.Inst)
        {
            cull = true;
        }

	} // End of Update().

    public static bool WithinBoundary(Vector3 worldPosition)
    {
        return !(Mathf.Sqrt(Mathf.Pow(worldPosition.x / NodeController.Inst.worldSize.x, 2f) + Mathf.Pow(worldPosition.y / NodeController.Inst.worldSize.y, 2f) + Mathf.Pow(worldPosition.z / NodeController.Inst.worldSize.z, 2f)) > 1f);
    }


	public void Destroy(){
        all.Remove(this);
        if(!AllFoodTree.Remove(this)){
            if(!AllFoodTree.Remove(this, false))
                Debug.LogError("Failed to remove Food Node: " + worldPosition.ToString());
        }
		if(amalgam)
			amalgam.foodPellets.Remove(this);
        viewer.Destroy();
    }

    public static void DestroyAll()
    {
        try {
            foreach (FoodPellet food in all)
                food.viewer.Destroy();
        }
        catch(System.Exception e) {
            Debug.LogError("Exception destroying food node: " + e.ToString());
        }

        allFoodTree = null;
        all.Clear();
    }

} // PhysFood
