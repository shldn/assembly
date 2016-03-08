﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

//--------------------------------------------------------------------------
// ViewerData
//
// Defines Data model that needs to be communicated from the Model/Controller Server to the Viewer
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
public struct AssemblyCenterUpdate
{
    public AssemblyCenterUpdate(int id_, Vector3 c) {
        id = id_;
        x = c.x;
        y = c.y;
        z = c.z;
    }

    public int id;
    private float x;
    private float y;
    private float z;
    public Vector3 Center { get { return new Vector3(x, y, z); } }
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
        userReleased = a.userReleased;
        offspring = a.isOffspring;
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
    public bool userReleased;
    public bool offspring;
    public AssemblyProperties properties;
    public List<int> nodeNeighbors;
    public List<int> trailIndices;
    public Dictionary<int, SenseNodeCreationData> senseNodeData; // index into nodeNeighbors to sense node data.
}

[Serializable]
public struct FoodCreationData
{
    public FoodCreationData(int id_, Vector3 pos) {
        id = id_;
        x = pos.x;
        y = pos.y;
        z = pos.z;
    }

    public Vector3 Position { get { return new Vector3(x, y, z); } }
    public int id;
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class ClientCursor
{
    public ClientCursor(int id_, float x_, float y_, float zoom_ = 0) {
        id = id_;
        x = x_;
        y = y_;
        zoom = zoom_;
    }

    public int id;
    public float x;
    public float y;
    public float zoom;
    public Vector3 Pos { get { return new Vector3(x, y, zoom); } }

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

public enum ViewerDataRequestType
{
    CAMERA_INFO = 1,
}

[Serializable]
public class CaptureData
{
    public CaptureData(int id_, string def_) { id = id_;  definition = def_; }
    public int id;
    public string definition;
}

[Serializable]
public class WorldSizeData
{
    public WorldSizeData(Vector3 size_) { size = size_; }
    public SVector3 size;
    public Vector3 Size { get { return size.Value;} }
}

[Serializable]
public class TargetWorldSizeData
{
    public TargetWorldSizeData(float size_) { size = size_; }
    public float size;
}

[Serializable]
public class LassoEvent
{
    public LassoEvent(int id_, bool start_) { id = id_;  start = start_; }
    public int id;
    public bool start;
}

[Serializable]
public class ViewerDataRequest
{
    public ViewerDataRequest(ViewerDataRequestType type_) { type = type_; }
    public ViewerDataRequestType type;
}

[Serializable]
public class ViewerData {

    private static ViewerData inst = null;
    public static ViewerData Inst{
        get{
            if (inst == null)
                inst = new ViewerData();
            if (inst2 == null)
                inst2 = new ViewerData();
            return swap ? inst2 : inst;
        }
    }

    public static ViewerData inst2 = null;
    private static bool swap = false;

    // Swap between two buffers so one can be sending across the network while the other is gathering the next round of data.
    public void Swap() { swap = !swap; }

    // Assemblies
    public List<AssemblyCreationData> assemblyCreations = new List<AssemblyCreationData>();
    public List<AssemblyTransformUpdate> assemblyUpdates = new List<AssemblyTransformUpdate>();
    public List<AssemblyCenterUpdate> assemblyCenters = new List<AssemblyCenterUpdate>();
    public List<AssemblyProperties> assemblyPropertyUpdates = new List<AssemblyProperties>();
    public List<int> assemblyDeletes = new List<int>();

    // Food
    public List<FoodCreationData> foodCreations = new List<FoodCreationData>();
    public List<int> foodDeletes = new List<int>();

    // Capture Client Messages
    public List<ClientCursor> cursorData = new List<ClientCursor>();

    // Generic Messages
    public List<object> messages = new List<object>();

    public void Clear()
    {
        assemblyUpdates.Clear();
        assemblyCenters.Clear();
        assemblyCreations.Clear();
        assemblyDeletes.Clear();
        assemblyPropertyUpdates.Clear();

        foodCreations.Clear();
        foodDeletes.Clear();

        cursorData.Clear();

        messages.Clear();
    }
}
