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
    public Quaternion Rotation { get { return cubeTransform.transform.rotation; } private set { cubeTransform.transform.rotation = value; } }

	private bool smoothMotion = false;
	private Vector3 positionTarget = Vector3.zero;
	private Vector3 positionVel = Vector3.zero;
    private Quaternion rotationTarget = Quaternion.identity;

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
        get { return cubeTransform != null && cubeTransform.GetComponent<Renderer>().enabled; }
        set
        {
            if (ViewerController.Hide && value)
                return;

            if(cubeTransform != null)
                cubeTransform.GetComponent<Renderer>().enabled = value;
            if (trail)
                trail.Render = value;
            if (viewCone)
                viewCone.render = value;
        }
    }

    // Constructors
    public NodeViewer(Vector3 position, NodeProperties properties, AssemblyProperties aProperties)
    {
        cubeTransform = ViewerController.Inst.NodePool.Get().transform;
        cubeTransform.position = position;
        cubeTransform.rotation = Quaternion.identity;
        cubeTransform.GetComponent<PhysNode>().nodeViewer = this;
        if(aProperties != null) {
            cubeTransform.name = "Node-" + aProperties.id;
            nodeProperties = properties;
            assemblyProperties = aProperties;
        }
        Visible = !ViewerController.Hide;
    }

    public NodeViewer(Vector3 position, AssemblyProperties aProperties, int numNeighbors, bool createTrail, SenseNodeCreationData senseData ) {
        cubeTransform = MonoBehaviour.Instantiate(ViewerController.Inst.physNodePrefab, position, Quaternion.identity) as Transform;
        cubeTransform.GetComponent<PhysNode>().nodeViewer = this;
        cubeTransform.name = "Node-" + aProperties.id;
        SetNeighborCount(numNeighbors, createTrail);
        nodeProperties.senseVector = senseData.senseVector;
        nodeProperties.fieldOfView = senseData.fov;
        assemblyProperties = aProperties;
        Visible = !ViewerController.Hide;
    }


    // Member Functions
    public void SetNeighborCount(int count, bool createTrail)
    {
        if (ViewerController.Hide || neighbors == count)
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

    public void Destroy(bool immediate = false)
    {
        if(trail) {
            trail.transform.parent = null;
            GameObject.Destroy(trail.gameObject);
		}
        CleanupNodeEffects();

        if (cubeTransform) {
            if (immediate)
                ViewerController.Inst.NodePool.Release(cubeTransform.gameObject);
            else {
                Shrink shrinker = cubeTransform.gameObject.GetComponent<Shrink>();
                if(shrinker == null)
                    shrinker = cubeTransform.gameObject.AddComponent<Shrink>();
                shrinker.enabled = true;
                shrinker.StartShrink();
                shrinker.Done += OnNodeDoneShrinking;
            }
        }
        cubeTransform = null;
    }

    public void Update()
    {
        if (ViewerController.Hide || cubeTransform == null)
            return;

        // Update mating color
        mateColorLerp = Mathf.MoveTowards(mateColorLerp, assemblyProperties.wantToMate ? 1f : 0f, Time.deltaTime);
        genderColorLerp = Mathf.MoveTowards(genderColorLerp, assemblyProperties.gender ? 1f : 0f, Time.deltaTime);

        Color genderColor = Color.Lerp(Color.magenta, Color.cyan, genderColorLerp);

        if (mateColorLerp > 0f)
            cubeTransform.GetComponent<Renderer>().material.color = Color.Lerp(nodeColor, genderColor, mateColorLerp * 0.7f);
        else
            cubeTransform.GetComponent<Renderer>().material.color = nodeColor;


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

		if(smoothMotion){
			Position = Vector3.SmoothDamp(Position, positionTarget, ref positionVel, 0.25f);
			Rotation = Quaternion.Lerp(Rotation, rotationTarget, Time.deltaTime * 2f);
			if (viewConeTrans)
				UpdateViewConeTransform();
		}

		if(trail)
			trail.fade = Mathf.Sin(Time.time);
    }

    public void UpdateTransform(Vector3 pos, Quaternion rot, bool smoothed = false) {
		if(cubeTransform.GetComponent<GrabbableObject>().IsGrabbed()) {
			if (viewConeTrans)
				UpdateViewConeTransform();
			return;
		}

		smoothMotion = smoothed;
		if(smoothed) {
			positionTarget = pos;
			rotationTarget = rot;
		} else {
			Position = pos;
			Rotation = rot;
		}

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

    void OnNodeDoneShrinking(GameObject go, Vector3 initScale) {
        Shrink shrinker = (cubeTransform != null && cubeTransform.gameObject != null) ? cubeTransform.gameObject.GetComponent<Shrink>() : null;
        if (shrinker != null)
            shrinker.Done -= OnNodeDoneShrinking;
        go.transform.localScale = initScale;
        ViewerController.Inst.NodePool.Release(go);
    }

}
