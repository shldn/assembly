using UnityEngine;
using System.Collections.Generic;

public class AmalgamViewer {

    private Dictionary<int, AssemblyViewer> assemblies = new Dictionary<int, AssemblyViewer>(); // map from assembly id to assembly
    private Dictionary<int, FoodPelletViewer> food = new Dictionary<int, FoodPelletViewer>(); // map from food id to food
    public MatingViewer matingViewer = null;

    public AmalgamViewer() {
        matingViewer = (new GameObject("MatingViewer")).AddComponent<MatingViewer>();
	}
	
	public void HandleMessages (ViewerData data) {

        // Assembly Messages
        for (int i = 0; i < data.assemblyCreations.Count; i++) {
            AssemblyViewer v = new AssemblyViewer(this, data.assemblyCreations[i]);
            assemblies.Add(v.Id, v);
        }

        for (int i = 0; i < data.assemblyUpdates.Count; i++) {
            AssemblyTransformUpdate update = data.assemblyUpdates[i];
            if (assemblies.ContainsKey(update.id))
                assemblies[update.id].TransformUpdate(update.transforms);
        }

        for (int i = 0; i < data.assemblyPropertyUpdates.Count; ++i) {
            if (assemblies.ContainsKey(data.assemblyPropertyUpdates[i].id)) {
                AssemblyViewer av = assemblies[data.assemblyPropertyUpdates[i].id];
                av.Properties = data.assemblyPropertyUpdates[i];
            }
        }

        for (int i = 0; i < data.assemblyDeletes.Count; ++i) {
            if (assemblies.ContainsKey(data.assemblyDeletes[i]))
                assemblies[data.assemblyDeletes[i]].Destroy();
        }


        // Food Messages
        for (int i = 0; i < data.foodCreations.Count; ++i) {
            FoodPelletViewer v = new FoodPelletViewer(data.foodCreations[i].Position, data.foodCreations[i].id);
            food.Add(data.foodCreations[i].id, v);
        }

        for (int i = 0; i < data.foodDeletes.Count; ++i) {
            if (food.ContainsKey(data.foodDeletes[i]))
                food[data.foodDeletes[i]].Destroy();
        }

    }

    public void Update() {
        foreach(KeyValuePair<int,AssemblyViewer> av in assemblies) {
            for (int i = 0; i < av.Value.nodes.Count; ++i) {
                av.Value.nodes[i].Update();
            }
        }
    }

    public void AddMates(int id1, int id2) {
        if(assemblies.ContainsKey(id1) && assemblies.ContainsKey(id2))
            matingViewer.AddMates(assemblies[id1], assemblies[id2]);
    }

    public void RemoveMates(int id) {
        matingViewer.RemoveMates(id);
    }

    public void Destroy() {
        foreach (KeyValuePair<int, AssemblyViewer> kvp in assemblies)
            kvp.Value.Destroy();
        assemblies.Clear();

        foreach (KeyValuePair<int, FoodPelletViewer> kvp in food)
            kvp.Value.Destroy();
        food.Clear();

        GameObject.Destroy(matingViewer);
        matingViewer = null;
    }
}
