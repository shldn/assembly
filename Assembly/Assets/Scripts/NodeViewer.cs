using UnityEngine;
using System.Collections;

public class NodeViewer {

    // Type-specific elements, effects
    public Transform cubeTransform = null;
    Color nodeColor = Color.white;
    TimedTrailRenderer trail = null;
    Transform viewConeTrans = null;
    ViewCone viewCone = null;
    public float viewConeSize = 3.5f;
    public Transform ViewConeLeft { get { return viewConeTrans; } }

    // mating colors
    float mateColorLerp = 0f;
    float genderColorLerp = 0f;

    // internal vars
    NodeProperties nodeProperties = NodeProperties.random;
    int neighbors = -1;
    AssemblyProperties assemblyProperties = null;


    // Accessors
    public Vector3 Position { get { return cubeTransform.transform.position; } private set { cubeTransform.transform.position = value; } }
    private Quaternion Rotation { get { return cubeTransform.transform.rotation; } set { cubeTransform.transform.rotation = value; } }
    public int Neighbors { get { return neighbors; } }
    public NodeProperties Properties
    {
        private get { return nodeProperties; }
        set { nodeProperties = value; }
    }
    public AssemblyProperties AssemblyProperties
    {
        get { return assemblyProperties; }
        set { assemblyProperties = value; }
    }
    public bool Visible {
        get { return cubeTransform.renderer.enabled; }
        set
        {
            if (ViewerController.Inst.Hide && value)
                return;

            if(cubeTransform != null)
                cubeTransform.renderer.enabled = value;
            if (trail)
                trail.render = value;
            if (viewCone)
                viewCone.render = value;
        }
    }

    // Constructors
    public NodeViewer(Vector3 position, NodeProperties properties, AssemblyProperties aProperties)
    {
        cubeTransform = MonoBehaviour.Instantiate(ViewerController.Inst.physNodePrefab, position, Quaternion.identity) as Transform;
        cubeTransform.GetComponent<PhysNode>().nodeViewer = this;
        if(aProperties != null) {
            cubeTransform.name = "Node-" + aProperties.id;
            nodeProperties = properties;
            assemblyProperties = aProperties;
        }
        Visible = !ViewerController.Inst.Hide;
    }

    public NodeViewer(Vector3 position, AssemblyProperties aProperties, int numNeighbors, bool createTrail, SenseNodeCreationData senseData ) {
        cubeTransform = MonoBehaviour.Instantiate(ViewerController.Inst.physNodePrefab, position, Quaternion.identity) as Transform;
        cubeTransform.GetComponent<PhysNode>().nodeViewer = this;
        cubeTransform.name = "Node-" + aProperties.id;
        SetNeighborCount(numNeighbors, createTrail);
        nodeProperties.senseVector = senseData.senseVector;
        nodeProperties.fieldOfView = senseData.fov;
        assemblyProperties = aProperties;
        Visible = !ViewerController.Inst.Hide;
    }


    // Member Functions
    public void SetNeighborCount(int count, bool createTrail)
    {
        if (ViewerController.Inst.Hide || neighbors == count)
            return;

        CleanupNodeEffects();
        neighbors = count;
        switch (neighbors)
        {
            // Sense node.
            case 1:
                nodeColor = PrefabManager.Inst.senseColor;
                CreateViewCone();
                break;
            // Muscle node.
            case 2:
                nodeColor = PrefabManager.Inst.actuateColor;
                if (createTrail)
                    CreateTrail();
                break;
            // Control node.
            case 3:
                nodeColor = PrefabManager.Inst.controlColor;
                break;
            default:
                nodeColor = PrefabManager.Inst.stemColor;
                break;
        }
    }

    void CleanupNodeEffects()
    {
        if (viewConeTrans)
        {
            GameObject.Destroy(viewConeTrans.gameObject);
            viewConeTrans = null;
        }

        if (trail)
        {
            GameObject.Destroy(trail.gameObject);
            GameObject.Destroy(trail);
            trail = null;
        }
    }

    void CreateViewCone()
    {
        Transform newViewConeTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.viewCone, Position, Rotation) as Transform;
        viewConeTrans = newViewConeTrans;
        viewCone = viewConeTrans.GetComponent<ViewCone>();
    }

    void CreateTrail()
    {
        Transform newTrailTrans = MonoBehaviour.Instantiate(PrefabManager.Inst.motorNodeTrail, Position, Rotation) as Transform;
        newTrailTrans.parent = cubeTransform;
        trail = newTrailTrans.GetComponent<TimedTrailRenderer>();

        if (PersistentGameManager.IsClient)
            trail.lifeTime *= 0.3f;
    }

    public void Destroy()
    {
        if(trail)
            trail.transform.parent = null;
        CleanupNodeEffects();

        if (cubeTransform)
            MonoBehaviour.Destroy(cubeTransform.gameObject);
    }

    public void Update()
    {
        if (ViewerController.Inst.Hide)
            return;
        // Update mating color
        mateColorLerp = Mathf.MoveTowards(mateColorLerp, assemblyProperties.wantToMate ? 1f : 0f, Time.deltaTime);
        genderColorLerp = Mathf.MoveTowards(genderColorLerp, assemblyProperties.gender ? 1f : 0f, Time.deltaTime);

        Color genderColor = Color.Lerp(Color.magenta, Color.cyan, genderColorLerp);

        if (mateColorLerp > 0f)
            cubeTransform.renderer.material.color = Color.Lerp(nodeColor, genderColor, mateColorLerp * 0.7f);
        else
            cubeTransform.renderer.material.color = nodeColor;


        // Type-specific behaviours
        switch (neighbors)
        {
            case 1:
                Debug.DrawRay(Position, (Rotation * nodeProperties.senseVector * Vector3.forward) * 2f, Color.green);
                UpdateViewConeTransform();
                break;
            case 2:
                break;
            case 3:
                break;
        }
    }

    public void UpdateTransform(Vector3 pos, Quaternion rot) {
        Position = pos;
        Rotation = rot;
        if (viewConeTrans)
            UpdateViewConeTransform();
    }

    void UpdateViewConeTransform() {
        //viewConeTrans.position = Position + (nodeProperties.senseVector * (Rotation * Vector3.forward)) * viewConeSize;
        viewConeTrans.position = Position;
        viewConeTrans.localScale = Vector3.one * viewConeSize;

        // Billboard the arc with the main camera.
        viewConeTrans.rotation = Rotation * nodeProperties.senseVector;
        viewConeTrans.position = Position;
        viewConeTrans.rotation *= Quaternion.AngleAxis(-90, Vector3.up);

        Vector3 camRelativePos = viewConeTrans.InverseTransformPoint(Camera.main.transform.position);
        float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;
        viewConeTrans.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);
        viewCone.fovAngle = nodeProperties.fieldOfView;
    }



}
