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
    public float Energy { get { return energy; } set { energy = value; } }
	float maxEnergy = 10f;
	public bool cull = false;


    // Cognogenesis
    bool cognoQuickKill = true;
    bool activated = false;
    bool captured = false;
    public bool Activated { get { return activated; } set { activated = value; if (activated) { captured = false; } } }
    public bool Captured { get { return captured; } set { captured = value; } }

    // For Olympics
    public Assembly owner = null; // to restrict energy consumption to just this entity

    public FoodPelletViewer viewer = null;
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

        if (Cognogenesis_Networking.Inst) {

            // Enforce lifetime
            if (!activated && MuseManager.Inst.TouchingForehead) {
                if(!captured)
                    Energy -= (1f - MuseManager.Inst.LastConcentrationMeasure) * 0.25f;
            }
            else if (cognoQuickKill && !captured)
                Energy -= NodeController.physicsStep * 0.25f;
        }

		if (energy < 0f){
			WorldSizeController.Inst.AdvanceWorldTick();
			cull = true;
		}

        if (CognoAmalgam.Inst != null && Random.Range(0f, 1f) >= 0.6f && !CognoAmalgam.Inst.IsInside(WorldPosition))
            velocity += -WorldPosition.normalized * NodeController.physicsStep * 8;


        // Destroy nodes outside of worlds
		/*
        if (!WorldSizeController.Inst.WithinBoundary(worldPosition) && Environment.Inst)
        {
            cull = true;
        }
		*/
        GrabbableObject grabbable = (viewer != null) ? viewer.gameObject.GetComponent<GrabbableObject>() : null;
        if(viewer != null)
            viewer.gameObject.GetComponent<Rigidbody>().isKinematic = grabbable ? !grabbable.IsGrabbed() : true;
		if(grabbable && grabbable.IsGrabbed()) {
			WorldPosition = viewer.gameObject.transform.position;
			velocity = viewer.gameObject.GetComponent<Rigidbody>().velocity;
		}
		else {
			worldPosition += velocity * NodeController.physicsStep;
			velocity = Vector3.MoveTowards(velocity, Vector3.zero, NodeController.physicsStep * 4f);
		}

        if (viewer != null) {
            viewer.Position = worldPosition;
            viewer.Scale = (energy / maxEnergy) * 0.5f/* * (activated? 1f : Random.Range(0.3f, 0.5f))*/;
        }
    } // End of Update().

    public static bool WithinBoundary(Vector3 worldPosition)
    {
        return WorldSizeController.Inst.WithinBoundary(worldPosition);
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
