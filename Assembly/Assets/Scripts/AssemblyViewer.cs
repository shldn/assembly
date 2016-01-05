using UnityEngine;
using System.Collections.Generic;

public class AssemblyViewer : CaptureObject{

    private static Dictionary<int, AssemblyViewer> all = new Dictionary<int, AssemblyViewer>();
    public static Dictionary<int, AssemblyViewer> All { get { return all; } }

    private AssemblyProperties properties;
    private int id = -1;
    private Vector3 position = Vector3.zero;
    public List<NodeViewer> nodes = new List<NodeViewer>();
    public TimedLabel label = null;
	public AssemblyEffects effects = null;

    public int Id { get { return properties.id; } }
    public Vector3 Position { get { return position; } private set { position = value; } }

    public AssemblyProperties Properties {
        get {
            return properties;
        }
        set {
            for (int i = 0; i < nodes.Count; ++i)
                nodes[i].AssemblyProperties = value;
            if(value.matingWith != -1 && properties.matingWith != -1 && value.matingWith != properties.matingWith) {
                MatingViewer.Inst.RemoveMates(Id);
                MatingViewer.Inst.AddMates(Id, value.matingWith);
            }
            if (value.matingWith != -1 && properties.matingWith == -1)
                MatingViewer.Inst.AddMates(Id, value.matingWith);
            if (value.matingWith == -1 && properties.matingWith != -1)
                MatingViewer.Inst.RemoveMates(Id);
            properties = value;
        }
    }

    public AssemblyViewer(AssemblyCreationData config) {
        properties = new AssemblyProperties(config.properties); // make sure it is a fresh copy, not sharing with Model/Controller side.
        for (int i = 0; i < config.nodeNeighbors.Count; ++i) {
            SenseNodeCreationData senseData = (config.senseNodeData.ContainsKey(i)) ? config.senseNodeData[i] : SenseNodeCreationData.identity;
            NodeViewer nv = new NodeViewer(Vector3.zero, config.properties, config.nodeNeighbors[i], config.trailIndices.Contains(i), senseData);
            nodes.Add(nv);
        }
        if (config.userReleased)
            CreateLabel(config.properties.name);
        if (config.offspring && RandomMelody.Inst)
            RandomMelody.Inst.PlayNote();
		CreateEffects();
        all.Add(Id, this);
        PersistentGameManager.CaptureObjects.Add(this);
    }

    public void TransformUpdate(List<PosRotPair> updates){
        if(updates.Count != nodes.Count){
            Debug.LogError("AssemblyViewer: Num updates != Num nodes");
        }
        Vector3 posSum = Vector3.zero;
        for (int i = 0; i < nodes.Count; ++i){
            nodes[i].UpdateTransform(updates[i].pos, updates[i].rot, true);
            posSum += updates[i].pos;
        }
        Position = posSum / nodes.Count;
        if (label)
            label.gameObject.transform.position = Position;
		if (effects)
			effects.gameObject.transform.position = Position;
    }

    public void DestroyKeepInList() {
        DestroyImpl(false);
    }

    public void Destroy() {
        DestroyImpl(true);
    }
    private void DestroyImpl(bool removeFromList) {
        for (int i = 0; i < nodes.Count; ++i)
            nodes[i].Destroy();

        MatingViewer.Inst.RemoveMates(id);
        PersistentGameManager.CaptureObjects.Remove(this);

		/*
		if(label) {
			Destroy(label.gameObject);
			label = null;
		}
		*/

		if(effects) {
			GameObject.Destroy(effects.gameObject);
			effects = null;
		}

        if(removeFromList && all.ContainsKey(Id))
            all.Remove(Id);
    }

    private void CreateLabel(string text) {
        label = new GameObject("AssemblyLabel").AddComponent<TimedLabel>();
        label.label = text;
        label.fadeTime = 30f;
        label.FadeComplete += LabelDone;
    }

	private void CreateEffects() {
        effects = new GameObject("AssemblyEffects").AddComponent<AssemblyEffects>();
	}

    private void LabelDone(object sender) {
        GameObject.Destroy(label.gameObject);
        label = null;
    }

}
