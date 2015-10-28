using UnityEngine;
using System.Collections.Generic;

public class NeuroScaleDemo : MonoBehaviour {

	public static NeuroScaleDemo Inst;


	public bool isActive = true;


	// How much information is shown to the user, from 0f (very little) to 1f (everything)
	public float enviroScale = 0f;
    float enviroScaleVel = 0f;

	// As this increases, the number of nodes shown will increase--chosen radiating out from the origin node.
	int numNodesToShow = 1;
	int numFoodToShow = 0;
    int lastNumNodesToShow = 0;

	//ThreeAxisCreep simulator;

    bool newSelectedNode = false;

    // Octree optimization -- Perhaps use a sortedList for the first 50 nodes, then just let the octree cull whole assemblies    
    Bounds nodeCullBoundary = new Bounds();
    SortedList<float, Node> nodeSortedSet = new SortedList<float, Node>();
    SortedList<float, FoodPellet> foodSortedSet = new SortedList<float, FoodPellet>();

    // Testing
    public bool useOctree = true;
    bool lastUseOctree = false;


	float camRadius = 0f;
	public float CamRadius {get{return camRadius;}}
	Node targetNode = null;
	public Node TargetNode {get{return targetNode;}}



	void Awake(){
		Inst = this;
	} // End of Awake().


	void Start(){
		RenderSettings.fog = true;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogColor = Color.black;
	} // End of Start().
	
	void Update(){

		isActive = MuseManager.Inst.TouchingForehead;

        newSelectedNode = false;
		if((!targetNode || targetNode.cull) && (Node.getAll.Count > 0)){
			targetNode = Node.getAll[Random.Range(0, Node.getAll.Count)];
            newSelectedNode = true;
		}
		
		camRadius = 10f + (200f * Mathf.Pow(enviroScale, 1f));

		RenderSettings.fogStartDistance = camRadius + (1000f * Mathf.Pow(enviroScale, 2f));
		RenderSettings.fogEndDistance = RenderSettings.fogStartDistance * 2f;

        lastNumNodesToShow = numNodesToShow;
		numNodesToShow = 1 + Mathf.RoundToInt(Mathf.Pow(enviroScale, 3f) * Node.getAll.Count);
		numFoodToShow = Mathf.RoundToInt(Mathf.Pow(enviroScale, 2f) * FoodPellet.all.Count);

        Cull();

        enviroScale = Mathf.SmoothDamp(enviroScale, isActive? MuseManager.Inst.LastConcentrationMeasure : 1f, ref enviroScaleVel, MuseManager.Inst.SlowResponse? 5f : 1f);
		enviroScale = Mathf.Clamp01(enviroScale);

        lastUseOctree = useOctree;


		// Keep targetted assembly from getting too far from the origin.
		if(targetNode && !PersistentGameManager.IsClient){
			foreach(KeyValuePair<Triplet, Node> kvp in targetNode.PhysAssembly.NodeDict){
				kvp.Value.Position += targetNode.PhysAssembly.Position * NodeController.physicsStep * 0.1f;
			}
		}

	} // End of Update().


    void Cull()
    {
        if (useOctree)
        {

            // Nodes

            if (!targetNode)
                return;

            nodeSortedSet.Clear();
            Assembly selectedAssembly = targetNode.PhysAssembly;

            // Only worry about the first assembly if the node count is low enough.
            if (selectedAssembly.NodeDict.Count >= numNodesToShow)
            {
                // Only cull when the enviroScale amount changes to minimize the popping in and out of nodes in the same assembly as one gets further from the selectedNode
                if (newSelectedNode || lastNumNodesToShow != numNodesToShow || !lastUseOctree)
                {
                    CullNodes(selectedAssembly);
                    HandleCulledNodeVisibility();
                }
                else
                {
                    // make sure if a new assembly is spawned, it gets hidden.
                    foreach (Assembly a in Assembly.getAll)
                        if (a != selectedAssembly)
                            a.SetVisibility(false);
                }
            }
            else if (enviroScale >= 0.99f || numNodesToShow >= Node.getAll.Count)
                SetAllNodeVisibility(true);
            else
            {
                float boundsSize = RenderSettings.fogEndDistance - RenderSettings.fogStartDistance;
                nodeCullBoundary = new Bounds(targetNode.Position, boundsSize * Vector3.one);
                Assembly.AllAssemblyTree.RunActionInRange(new System.Action<Assembly>(CullNodes), nodeCullBoundary);
                HandleCulledNodeVisibility();
            }

            // Food
            if (numFoodToShow > 0)
            {
                foodSortedSet.Clear();
                float boundsSize = RenderSettings.fogEndDistance - RenderSettings.fogStartDistance;
                Bounds foodBoundary = new Bounds(targetNode.Position, boundsSize * Vector3.one);
                FoodPellet.AllFoodTree.RunActionInRange(new System.Action<FoodPellet>(CullFood), foodBoundary);
                HandleCulledFoodVisibility();
            }
            else
                SetAllFoodVisibility(false);

        }
        else
        {
            // Previous Approach
            SortNodes();
            SortFood();
            for (int i = 0; i < Node.getAll.Count; i++)
                Node.getAll[i].Visible = i < numNodesToShow;
            for (int i = 0; i < FoodPellet.all.Count; i++)
                FoodPellet.all[i].render = i < numNodesToShow;
        }
    } // End of Cull().

    void CullNodes(Assembly someAssembly)
    {
        foreach (KeyValuePair<Triplet, Node> node in someAssembly.NodeDict)
        {
            float dist = (node.Value.Position - targetNode.Position).sqrMagnitude;
            if (nodeSortedSet.Count < numNodesToShow)
                AddToSortedList(nodeSortedSet, dist, node.Value);
            else
            {
                if (nodeSortedSet.Keys[nodeSortedSet.Keys.Count - 1] > dist)
                {
                    nodeSortedSet.RemoveAt(nodeSortedSet.Keys.Count - 1);
                    AddToSortedList(nodeSortedSet, dist, node.Value);
                }
            }
        }
    } // End of CullNodes().

    void AddToSortedList<T>(SortedList<float, T> sortedList, float dist, T obj)
    {
        try {
            sortedList.Add(dist, obj);
        }
        catch (System.ArgumentException e) {
            // a node with the same distance away already exists, try again...
            try {
                sortedList.Add(dist - Random.Range(0.001f, 0.004f), obj);
            }
            catch (System.ArgumentException) { }
        }
    }

    void SetAllNodeVisibility(bool vis)
    {
        for (int i = 0; i < Node.getAll.Count; i++)
            Node.getAll[i].Visible = vis;
    } // End of SetAllNodeVisibility().

    void HandleCulledNodeVisibility()
    {
        SetAllNodeVisibility(false);

        // turn on selected nodes
        foreach (KeyValuePair<float, Node> nodePair in nodeSortedSet)
            nodePair.Value.Visible = true;
    } // End of HandleCulledNodeVisibility().

    void CullFood(FoodPellet someFood)
    {
        float dist = (someFood.WorldPosition - targetNode.Position).sqrMagnitude;
        if (foodSortedSet.Count < numFoodToShow)
            AddToSortedList(foodSortedSet, dist, someFood);
        else
        {
            if (foodSortedSet.Keys[foodSortedSet.Keys.Count - 1] > dist)
            {
                foodSortedSet.RemoveAt(foodSortedSet.Keys.Count - 1);
                AddToSortedList(foodSortedSet, dist, someFood);
            }
        }
    } // End of CullFood().

    void SetAllFoodVisibility(bool vis)
    {
        for (int i = 0; i < FoodPellet.all.Count; i++)
            FoodPellet.all[i].render = vis;
    } // End of SetAllFoodVisibility().

    void HandleCulledFoodVisibility()
    {
        SetAllFoodVisibility(false);
        foreach (KeyValuePair<float, FoodPellet> foodPair in foodSortedSet)
            foodPair.Value.render = true;
    } // End of HandleCulledFoodVisibility().

	// bubble-sort nodes
	void SortNodes(){
		for (int i = 0; i < Node.getAll.Count - 1; i ++ ){
			float sqrMag1 = (Node.getAll[i + 0].Position - targetNode.Position).sqrMagnitude;
			float sqrMag2 = (Node.getAll[i + 1].Position - targetNode.Position).sqrMagnitude;
         
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
			float sqrMag1 = (FoodPellet.all[i + 0].WorldPosition - targetNode.Position).sqrMagnitude;
			float sqrMag2 = (FoodPellet.all[i + 1].WorldPosition - targetNode.Position).sqrMagnitude;
         
			if(sqrMag2 < sqrMag1){
				FoodPellet tempStore = FoodPellet.all[i];
				FoodPellet.all[i] = FoodPellet.all[i + 1];
				FoodPellet.all[i + 1] = tempStore;
				i = 0;
			}
		}
	} // End of SortNodes().
} // End of NeuroScaleDemo.
