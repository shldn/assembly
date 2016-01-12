using UnityEngine;
using System.Collections.Generic;

public struct MatingPair
{
    public MatingPair(AssemblyViewer mate1, AssemblyViewer mate2) {
        a1 = mate1;
        a2 = mate2;
    }
    public AssemblyViewer a1;
    public AssemblyViewer a2;
}

public class MatingViewer : MonoBehaviour {

    private static MatingViewer inst = null;
    public static MatingViewer Inst {
        get {
            if (inst == null)
                inst = (new GameObject("MatingViewer")).AddComponent<MatingViewer>();
            return inst;
        }
    }


    List<MatingPair> mates = new List<MatingPair>();
    Dictionary<int,Dictionary<int, MatingPair>> amalgamPairs = new Dictionary<int, Dictionary<int, MatingPair>>(); // amalgam id --> mate id to pair map
	
	void Update () {
        for(int m = 0; m < mates.Count; ++m) {
            MatingPair mp = mates[m];
            for (int i = 0; i < mp.a1.nodes.Count; i++) {
                NodeViewer myNode = mp.a1.nodes[i];
                if (mp.a2.nodes.Count > i) {
                    NodeViewer otherNode = mp.a2.nodes[i];

                    if (myNode.Visible && otherNode.Visible)
                        GLDebug.DrawLine(myNode.Position, otherNode.Position, new Color(1f, 0f, 1f, 0.5f));
                }
            }
        }
    }

    void OnDestroy() {
        inst = null;
    }

    public void AddMates(int amalgamId, int id1, int id2) {

        if (!amalgamPairs.ContainsKey(amalgamId))
            amalgamPairs.Add(amalgamId, new Dictionary<int, MatingPair>());

        MatingPair mp;
        Dictionary<int, MatingPair> idToPairMap = amalgamPairs[amalgamId];
        if (idToPairMap.TryGetValue(id1, out mp)) {
            // bail out if this already added.
            if (mp.a1.Id == id2 || mp.a2.Id == id2)
                return;
        }
        if(AssemblyViewer.GetAssemblyViewers(amalgamId).ContainsKey(id1) && AssemblyViewer.GetAssemblyViewers(amalgamId).ContainsKey(id2)) {
            mp = new MatingPair(AssemblyViewer.GetAssemblyViewers(amalgamId)[id1], AssemblyViewer.GetAssemblyViewers(amalgamId)[id2]);
            mates.Add(mp);
            idToPairMap.Add(id1, mp);
            idToPairMap.Add(id2, mp);
        }
        else {
            Debug.LogError(id1 + " or " + id2 + " Does not exist");
        }
    }

    public void RemoveMates(int amalgamId, int mateId) {
        if (!amalgamPairs.ContainsKey(amalgamId))
            return;
        Dictionary<int, MatingPair> idToPairMap = amalgamPairs[amalgamId];
        MatingPair mp;
        if (idToPairMap.TryGetValue(mateId, out mp)) {
            mates.Remove(mp);
            idToPairMap.Remove(mp.a1.Id);
            idToPairMap.Remove(mp.a2.Id);
        }
    }

    public void RemoveAmalgamMates(int amalgamId) {
        Dictionary<int, MatingPair> idToPairMap = amalgamPairs[amalgamId];
        foreach (KeyValuePair<int, MatingPair> kv in idToPairMap)
            mates.Remove(kv.Value);
        idToPairMap.Clear();
    }
}
