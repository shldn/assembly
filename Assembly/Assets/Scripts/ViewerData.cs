using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

//--------------------------------------------------------------------------
// ViewerData
//
// Defines Data model that needs to be communicated between the Model/Controller Server and the Viewer
//
//--------------------------------------------------------------------------

[Serializable]
public class PosRotPair : ISerializable{
    public PosRotPair(Vector3 pos_, Quaternion rot_) {
        pos = pos_;
        rot = rot_;
    }
    public Vector3 pos;
    public Quaternion rot;

    // custom serialization, since Vector3 and Quaternion are not marked serializable
    PosRotPair(SerializationInfo info, StreamingContext context) {
        pos = new Vector3(info.GetSingle("X"), info.GetSingle("Y"), info.GetSingle("Z"));
        rot = new Quaternion(info.GetSingle("x"), info.GetSingle("y"), info.GetSingle("z"), info.GetSingle("w"));
    }

  public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
  {
        info.AddValue("X", pos.x);
        info.AddValue("Y", pos.y);
        info.AddValue("Z", pos.z);
        info.AddValue("x", rot.x);
        info.AddValue("y", rot.y);
        info.AddValue("z", rot.z);
        info.AddValue("w", rot.w);
    }
}

[Serializable]
public struct AssemblyTransformUpdate {
    // only compile this in the model/controller server code...
    public AssemblyTransformUpdate(Assembly a) {
        id = a.Id;
        transforms = new List<PosRotPair>(a.Nodes.Count);
        foreach (Node n in a.Nodes)
            transforms.Add(new PosRotPair(n.Position, n.Rotation));

    }
    public int id;
    public List<PosRotPair> transforms;
}

[Serializable]
public struct SenseNodeCreationData{
    public SenseNodeCreationData(NodeProperties props) {
        sx = props.senseVector.x;
        sy = props.senseVector.y;
        sz = props.senseVector.z;
        sw = props.senseVector.w;
        fov = props.fieldOfView;
    }
    public SenseNodeCreationData(float sx_, float sy_, float sz_, float sw_, float fov_) {
        sx = sx_;
        sy = sy_;
        sz = sz_;
        sw = sw_;
        fov = fov_;
    }

    public static SenseNodeCreationData identity { get { return new SenseNodeCreationData(0, 0, 0, 1, 45f); } }
    public float sx;
    public float sy;
    public float sz;
    public float sw;
    public float fov; 
    public Quaternion senseVector{get{return new Quaternion(sx,sy,sz,sw);}}
}

[Serializable]
public struct AssemblyCreationData {
    public AssemblyCreationData(Assembly a) {
        id = a.Id;
        properties = a.properties;
        nodeNeighbors = new List<int>(a.Nodes.Count);
        trailIndices = new List<int>();
        senseNodeData = new Dictionary<int, SenseNodeCreationData>();
        for (int i = 0; i < a.Nodes.Count; ++i) {
            nodeNeighbors.Add(a.Nodes[i].neighbors.Count);
            if (a.Nodes[i].IsSense)
                senseNodeData.Add(i, new SenseNodeCreationData(a.Nodes[i].Properties));
            if (a.Nodes[i].IsMuscle && (!a.Nodes[i].neighbors[0].physNode.IsMuscle || !a.Nodes[i].neighbors[1].physNode.IsMuscle))
                trailIndices.Add(i);
        }
    }

    public int id;
    public AssemblyProperties properties;
    public List<int> nodeNeighbors;
    public List<int> trailIndices;
    public Dictionary<int, SenseNodeCreationData> senseNodeData; // index into nodeNeighbors to sense node data.
}

public enum ViewerMessageType
{
    ASSEMBLY_CREATE     = 1,
    ASSEMBLY_UPDATE     = 2,
    ASSEMBLY_PROPERTIES = 3,
    ASSEMBLY_DELETE     = 4,
    NODE_PROPERTIES     = 5,
    FOOD_CREATE         = 6,
    FOOD_UPDATE         = 7,
    FOOD_DELETE         = 8,
}

public class ViewerData {

    private static ViewerData inst = null;
    public static ViewerData Inst{
        get{
            if (inst == null)
                inst = new ViewerData();
            return inst;
        }
    }

    public List<AssemblyCreationData> assemblyCreations = new List<AssemblyCreationData>();
    public List<AssemblyTransformUpdate> assemblyUpdates = new List<AssemblyTransformUpdate>();
    public List<int> assemblyDeletes = new List<int>();


    public void Clear()
    {
        assemblyUpdates.Clear();
        assemblyCreations.Clear();
        assemblyDeletes.Clear();
    }
}
