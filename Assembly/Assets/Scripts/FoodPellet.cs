using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FoodPellet {

	public static List<FoodPellet> all = new List<FoodPellet>();
	public Amalgam amalgam = null;

	private Vector3 worldPosition = Vector3.zero;
    public Vector3 WorldPosition { get { return worldPosition; } set { worldPosition = value; if (viewer != null) { viewer.Position = value; }  } }

	public Vector3 velocity = Vector3.zero;

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

    int id = -1;
    public int Id { get { return id; } }
	float energy = 10f;
    public float Energy { get { return energy; } set { energy = value; if (viewer != null) { viewer.Scale = energy / maxEnergy; } } }
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

		all.Add(this);
		AllFoodTree.Insert(this);

		maxEnergy = energy;
        owner = owner_;

        if (PersistentGameManager.EmbedViewer) {
            viewer = new FoodPelletViewer(worldPosition);
            viewer.Scale = 1;
        }
        else {
            id = NextFoodID();
            ViewerData.Inst.foodCreations.Add(new FoodCreationData(id, worldPosition));
        }

    } // constructor

    static int foodCount = 0;
    static int NextFoodID() {
        return foodCount++;
    }

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

		viewer.gameObject.GetComponent<Rigidbody>().isKinematic = !viewer.gameObject.GetComponent<GrabbableObject>().IsGrabbed();
		if(viewer.gameObject.GetComponent<GrabbableObject>().IsGrabbed()) {
			WorldPosition = viewer.gameObject.transform.position;
			velocity = viewer.gameObject.GetComponent<Rigidbody>().velocity;
		}
		else {
			worldPosition += velocity * NodeController.physicsStep;
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, NodeController.physicsStep * 4f);
		}

		if(viewer != null)
			viewer.Position = worldPosition;

	} // End of Update().

    public static bool WithinBoundary(Vector3 worldPosition)
    {
        return !(Mathf.Sqrt(Mathf.Pow(worldPosition.x / NodeController.Inst.worldSphereScale.x, 2f) + Mathf.Pow(worldPosition.y / NodeController.Inst.worldSphereScale.y, 2f) + Mathf.Pow(worldPosition.z / NodeController.Inst.worldSphereScale.z, 2f)) > 1f);
    }


	public void Destroy(){
        all.Remove(this);
        if(!AllFoodTree.Remove(this)){
            if(!AllFoodTree.Remove(this, false))
                Debug.LogError("Failed to remove Food Node: " + worldPosition.ToString());
        }
		if(amalgam)
			amalgam.foodPellets.Remove(this);
        if (viewer != null)
            viewer.Destroy();
        else
            ViewerData.Inst.foodDeletes.Add(id);
    }

    public static void DestroyAll()
    {
        try {
            foreach (FoodPellet food in all) {
                if (food.viewer != null)
                    food.viewer.Destroy();
                else
                    ViewerData.Inst.foodDeletes.Add(food.id);
            }
        }
        catch(System.Exception e) {
            Debug.LogError("Exception destroying food node: " + e.ToString());
        }

        allFoodTree = null;
        all.Clear();
    }

} // PhysFood
