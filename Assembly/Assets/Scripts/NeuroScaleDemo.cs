using UnityEngine;
using System.Collections.Generic;

public class NeuroScaleDemo : MonoBehaviour {

	public static NeuroScaleDemo Inst;

	// How much information is shown to the user, from 0f (very little) to 1f (everything)
	public float enviroScale = 0f;

	// As this increases, the number of nodes shown will increase--chosen radiating out from the origin node.
	int numNodesToShow = 1;
	int numFoodToShow = 0;
    int lastNumNodesToShow = 1;

	//ThreeAxisCreep simulator;

    bool newSelectedNode = false;

    // Octree optimization -- Perhaps use a sortedList for the first 50 nodes, then just let the octree cull whole assemblies    
    Bounds nodeCullBoundary = new Bounds();
    SortedList<float, Node> nodeSortedSet = new SortedList<float, Node>();


    // Testing
    public bool useOctree = true;
    bool lastUseOctree = false;

	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogColor = Color.black;

		// simulator = gameObject.AddComponent<ThreeAxisCreep>();
		// simulator.maxCreep = 1f;
		// simulator.relaxedness = 10f;
	} // End of Start().
	
	void Update(){

        newSelectedNode = false;
		if((!CameraControl.Inst.selectedNode || CameraControl.Inst.selectedNode.cull) && (Node.getAll.Count > 0)){
			CameraControl.Inst.selectedNode = Node.getAll[Random.Range(0, Node.getAll.Count)];
            newSelectedNode = true;
			print("Getting new node to focus.");
		}

		
		CameraControl.Inst.targetRadius = 10f + (200f * Mathf.Pow(enviroScale, 1f));
		CameraControl.Inst.radius = CameraControl.Inst.targetRadius;

		RenderSettings.fogStartDistance = CameraControl.Inst.targetRadius + (1000f * Mathf.Pow(enviroScale, 2f));
		RenderSettings.fogEndDistance = RenderSettings.fogStartDistance * 2f;

        lastNumNodesToShow = numNodesToShow;
		numNodesToShow = 1 + Mathf.RoundToInt(Mathf.Pow(enviroScale, 3f) * Node.getAll.Count);
		numFoodToShow = Mathf.RoundToInt(Mathf.Pow(enviroScale, 2f) * FoodPellet.all.Count);

        Cull();

		if(Input.GetKey(KeyCode.UpArrow))
			enviroScale += Time.deltaTime * 0.2f;
		if(Input.GetKey(KeyCode.DownArrow))
			enviroScale -= Time.deltaTime * 0.2f;


		//enviroScale = Mathf.Pow(0.5f + simulator.creep.x, 2f);

		CameraControl.Inst.targetOrbit.x -= Time.deltaTime * 5f;

		enviroScale = Mathf.Clamp01(enviroScale);

        lastUseOctree = useOctree;

	} // End of Update().


    void Cull()
    {
        if (useOctree)
        {
            nodeSortedSet.Clear();

            // Only worry about the first assembly if the node count is low enough.
            if (CameraControl.Inst.selectedNode.PhysAssembly.NodeDict.Count >= numNodesToShow)
            {
                // Only cull when the enviroScale amount changes to minimize the popping in and out of nodes in the same assembly as one gets further from the selectedNode
                if (newSelectedNode || lastNumNodesToShow != numNodesToShow || !lastUseOctree)
                {
                    CullNodes(CameraControl.Inst.selectedNode.PhysAssembly);
                    HandleCulledVisibility();
                }
            }
            else if (enviroScale >= 0.99f || numNodesToShow >= Node.getAll.Count)
                SetAllNodeVisibility(true);
            else
            {
                float boundsSize = RenderSettings.fogEndDistance - RenderSettings.fogStartDistance;
                nodeCullBoundary = new Bounds(CameraControl.Inst.selectedNode.Position, boundsSize * Vector3.one);
                Assembly.AllAssemblyTree.RunActionInRange(new System.Action<Assembly>(CullNodes), nodeCullBoundary);
                HandleCulledVisibility();
            }

        }
        else
        {
            // Previous Approach
            SortNodes();
            SortFood();
            for (int i = 0; i < Node.getAll.Count; i++)
            {
                if (Node.getAll[i].cubeTransform)
                    Node.getAll[i].cubeTransform.renderer.enabled = i < numNodesToShow;
            }
            for (int i = 0; i < FoodPellet.all.Count; i++)
            {
                FoodPellet.all[i].render = i < numNodesToShow;
            }
        }
    } // End of Cull().

    void CullNodes(Assembly someAssembly)
    {
        foreach (KeyValuePair<Triplet, Node> node in someAssembly.NodeDict)
        {
            if (nodeSortedSet.Count < numNodesToShow)
            {
                float dist = (node.Value.Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
                nodeSortedSet.Add(dist, node.Value);
            }
            else
            {
                float dist = (node.Value.Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
                if (nodeSortedSet.Keys[nodeSortedSet.Keys.Count - 1] > dist)
                {
                    nodeSortedSet.RemoveAt(nodeSortedSet.Keys.Count - 1);
                    nodeSortedSet.Add(dist, node.Value);
                }
            }
        }
    } // End of CullNodes().

    void SetAllNodeVisibility(bool vis)
    {
        for (int i = 0; i < Node.getAll.Count; i++)
        {
            if (Node.getAll[i].cubeTransform)
                Node.getAll[i].cubeTransform.renderer.enabled = vis;
        }
    } // End of SetAllNodeVisibility().

    void HandleCulledVisibility()
    {
        SetAllNodeVisibility(false);

        // turn on selected nodes
        foreach (KeyValuePair<float, Node> nodePair in nodeSortedSet)
        {
            if (nodePair.Value.cubeTransform)
                nodePair.Value.cubeTransform.renderer.enabled = true;
        }
    } // End of HandleCulledVisibility().
    

	// bubble-sort nodes
	void SortNodes(){
		for (int i = 0; i < Node.getAll.Count - 1; i ++ ){
			float sqrMag1 = (Node.getAll[i + 0].Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
			float sqrMag2 = (Node.getAll[i + 1].Position - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
         
			if(sqrMag2 < sqrMag1){
				Node tempStore = Node.getAll[i];
				Node.getAll[i] = Node.getAll[i + 1];
				Node.getAll[i + 1] = tempStore;
				i = 0;
			}
		}
	} // End of SortNodes().

	// bubble-sort nodes
	void SortFood(){
		for (int i = 0; i < FoodPellet.all.Count - 1; i ++ ){
			float sqrMag1 = (FoodPellet.all[i + 0].WorldPosition - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
			float sqrMag2 = (FoodPellet.all[i + 1].WorldPosition - CameraControl.Inst.selectedNode.Position).sqrMagnitude;
         
			if(sqrMag2 < sqrMag1){
				FoodPellet tempStore = FoodPellet.all[i];
				FoodPellet.all[i] = FoodPellet.all[i + 1];
				FoodPellet.all[i + 1] = tempStore;
				i = 0;
			}
		}
	} // End of SortNodes().
} // End of NeuroScaleDemo.
