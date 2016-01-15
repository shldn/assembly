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

    List<MatingPair> mates = new List<MatingPair>();
    Dictionary<int, MatingPair> idToPairMap = new Dictionary<int, MatingPair>(); // amalgam id --> mate id to pair map
	
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

    public void AddMates(AssemblyViewer assembly1, AssemblyViewer assembly2) {
        int id1 = assembly1.Id;
        int id2 = assembly2.Id;
        MatingPair mp;
        if (idToPairMap.TryGetValue(id1, out mp)) {
            // bail out if this already added.
            if (mp.a1.Id == id2 || mp.a2.Id == id2)
                return;
        }

        mp = new MatingPair(assembly1, assembly2);
        mates.Add(mp);
        idToPairMap.Add(id1, mp);
        idToPairMap.Add(id2, mp);
    }

    public void RemoveMates(int mateId) {
        MatingPair mp;
        if (idToPairMap.TryGetValue(mateId, out mp)) {
            mates.Remove(mp);
            idToPairMap.Remove(mp.a1.Id);
            idToPairMap.Remove(mp.a2.Id);
        }
    }
}
