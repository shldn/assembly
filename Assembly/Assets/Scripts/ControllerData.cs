using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

//--------------------------------------------------------------------------
// ControllerData
//
// Defines Data model that needs to be communicated from the Viewer to the Model/Controller Server
//
//--------------------------------------------------------------------------


[Serializable]
public class ControllerData {

    private static ControllerData inst = null;
    public static ControllerData Inst {
        get {
            if (inst == null)
                inst = new ControllerData();
            if (inst2 == null)
                inst2 = new ControllerData();
            return swap ? inst2 : inst;
        }
    }
    public static ControllerData inst2 = null;
    private static bool swap = false;

    // Swap between two buffers so one can be sending across the network while the other is gathering the next round of data.
    public void Swap() { swap = !swap; }

    public bool HasData { get { return messages.Count != 0; } }
    public List<object> Messages { get { return messages; } }

    public delegate void ControllerDataDelegate(object message);
    [NonSerialized]
    public Dictionary<Type, ControllerDataDelegate> messageHandlers = new Dictionary<Type, ControllerDataDelegate>();

    private List<object> messages = new List<object>();

    private ControllerData() {
        SetupMessageHandlers();
    }

    public void Add(object message) {
#if UNITY_EDITOR
        if(!messageHandlers.ContainsKey(message.GetType())){
            Debug.LogError("ControllerData: Unknown Message type: " + message.GetType() + " please add a handler to messageHandlers");
            return;
        }
#endif
        messages.Add(message);
    }

    public void HandleMessages(List<object> msgs) {
        foreach (object msg in msgs)
            messageHandlers[msg.GetType()](msg);
    }

    public void Clear() {
        messages.Clear();
    }

    public void SetupMessageHandlers() {
        messageHandlers.Add(typeof(AssemblyCaptured), NodeController.Inst.HandleCapturedAssembly);
        messageHandlers.Add(typeof(AssemblyReleased), NodeController.Inst.HandleReleasedAssembly);
        messageHandlers.Add(typeof(DataRequest), NodeController.Inst.HandleDataRequest);
    }
}


// Message Structure Definitions
[Serializable]
public class AssemblyCaptured
{
    public AssemblyCaptured(AssemblyViewer viewer, int playerId_) { id = viewer.Id; playerId = playerId_; }
    public AssemblyCaptured(int id_, int playerId_) { id = id_; playerId = playerId_; }
    public int id;
    public int playerId;
}

[Serializable]
public class AssemblyReleased
{
    public AssemblyReleased(string assemblyStr_, Vector3 position_, Vector3 camDir_) {
        definition = assemblyStr_;
        position = position_;
    }

    public string definition;
    private SVector3 position;
    private SVector3 camDir;
    public Vector3 Position { get { return position.Value; } }
    public Vector3 CamDir { get { return camDir.Value; } }
}

[Serializable]
public struct SVector3
{
    public static implicit operator SVector3(Vector3 v) { return new SVector3(v); }
    public SVector3(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 Value { get { return new Vector3(x, y, z); } }

    public float x;
    public float y;
    public float z;
}

public enum DataRequestType
{
    ASSEMBLY_CENTERS = 1,
    ASSEMBLY_FULL = 2,
}

[Serializable]
public class DataRequest
{
    public DataRequest(DataRequestType request_) { request = request_; }
    public DataRequestType request;
}

