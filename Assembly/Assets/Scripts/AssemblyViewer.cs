using UnityEngine;
using System.Collections.Generic;

public class AssemblyViewer : CaptureObject{

    private AmalgamViewer amalgam = null;
    private AssemblyProperties properties;
    private int id = -1;
    private Vector3 position = Vector3.zero;
    public List<NodeViewer> nodes = new List<NodeViewer>();
    public TimedLabel label = null;
	public AssemblyEffects effects = null;

    public int Id { get { return properties.id; } }
    public Vector3 Position { get { return position; } private set { position = value; } }

    // internal helpers
    int creationFrame = -1;

    public AssemblyProperties Properties {
        get {
            return properties;
        }
        set {
            for (int i = 0; i < nodes.Count; ++i)
                nodes[i].AssemblyProperties = value;
            if(amalgam != null) {
                if (value.matingWith != -1 && properties.matingWith != -1 && value.matingWith != properties.matingWith) {
                    amalgam.RemoveMates(Id);
                    amalgam.AddMates(Id, value.matingWith);
                }
                if (value.matingWith != -1 && properties.matingWith == -1)
                    amalgam.AddMates(Id, value.matingWith);
                if (value.matingWith == -1 && properties.matingWith != -1)
                    amalgam.RemoveMates(Id);
            }
            properties = value;
        }
    }

    public AssemblyViewer(AmalgamViewer amalgam_, AssemblyCreationData config) {
        amalgam = amalgam_;
        properties = new AssemblyProperties(config.properties); // make sure it is a fresh copy, not sharing with Model/Controller side.
        if (properties.matingWith != -1)
            Debug.LogError("Assembly created while mating, are we handling this -- will have to apply after all viewers are created? " + id + " " + properties.matingWith);
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

        PersistentGameManager.CaptureObjects.Add(this);

        creationFrame = Time.frameCount;
    }

    public void TransformUpdate(List<PosRotPair> updates){
        if(updates.Count != nodes.Count){
            Debug.LogError("AssemblyViewer: Num updates != Num nodes");
        }
        Vector3 posSum = Vector3.zero;
        bool smoothTransform = creationFrame != Time.frameCount;
        for (int i = 0; i < nodes.Count; ++i){
            nodes[i].UpdateTransform(updates[i].pos, updates[i].rot, smoothTransform);
            posSum += updates[i].pos;
        }
        Position = posSum / nodes.Count;
        if (label)
            label.gameObject.transform.position = Position;
		if (effects)
			effects.gameObject.transform.position = Position;
    }

    public void CenterUpdate(Vector3 center) {
        Vector3 offset = center - Position;
        bool smoothTransform = false;
        for (int i = 0; i < nodes.Count; ++i)
            nodes[i].UpdateTransform(nodes[i].Position + offset, nodes[i].Rotation, smoothTransform);
        Position = center;
    }

    public void Destroy() {
        for (int i = 0; i < nodes.Count; ++i)
            nodes[i].Destroy();

        if(amalgam != null)
            amalgam.RemoveMates(Id);
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
