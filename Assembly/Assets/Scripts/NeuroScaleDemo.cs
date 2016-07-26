using UnityEngine;
using System.Collections.Generic;

public class NeuroScaleDemo : MonoBehaviour {

	public static NeuroScaleDemo Inst;

	public bool isActive = true;

	// How much information is shown to the user, from 0f (very little) to 1f (everything)
	public float enviroScale = 0f;
    float enviroScaleVel = 0f;
    float fogVel = 0f;
    float timeAtZero = 0f;

	// As this increases, the number of nodes shown will increase--chosen radiating out from the origin node.
	int numNodesToShow = 1;
	int numFoodToShow = 0;
    int lastNumNodesToShow = 0;
    bool cullSelectedAssemblyNodes = false;

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

    // Genetic Engineering
    public static bool enableMutationOnFocus = true;
    public static float timeAtZeroToStartTest = 4.0f;

	bool simulateMuse = false;
	float simValue = 1f;


	void Awake(){
		Inst = this;
	} // End of Awake().


	void Update(){

		if(Input.GetKeyDown(KeyCode.E))
			simulateMuse = !simulateMuse;

		if(simulateMuse) {
			if(Input.GetKey(KeyCode.UpArrow))
				simValue += Time.deltaTime * 0.3f;
			if(Input.GetKey(KeyCode.DownArrow))
				simValue -= Time.deltaTime * 0.3f;
			simValue = Mathf.Clamp01(simValue);
		}

		if(simulateMuse || (MuseManager.Inst.TouchingForehead && !isActive)) {
			isActive = true;
		}

		if(!simulateMuse && !MuseManager.Inst.TouchingForehead && isActive) {
			isActive = false;
			targetNode = null;
			CameraControl.Inst.SetMode_GalleryAuto();
			SetAllNodeVisibility(true);
		}


		newSelectedNode = false;
		if(isActive && (!targetNode || targetNode.cull) && (Node.getAll.Count > 0) && (enviroScale < 1f) && !ClientTest.Inst){
			targetNode = Node.getAll[Random.Range(0, Node.getAll.Count)];
			CameraControl.Inst.SetMode_NeuroScaleFocus(targetNode);
			newSelectedNode = true;
		}

		if(enviroScale == 1f) {
			targetNode = null;
		}

        // Make sure target assembly is in view
        if(targetNode != null && targetNode.PhysAssembly != null)
            camRadius = Mathf.Max(camRadius, targetNode.PhysAssembly.GetBoundingSphereRadiusFromPoint(targetNode.Position) + 1f);

		lastNumNodesToShow = numNodesToShow;
		numNodesToShow = 1 + Mathf.RoundToInt(Mathf.Pow(enviroScale, 3f) * Node.getAll.Count);
		numFoodToShow = Mathf.RoundToInt(Mathf.Pow(enviroScale, 2f) * FoodPellet.all.Count);

		Cull();

        if (simulateMuse || !Debug.isDebugBuild || !Input.GetKey(KeyCode.Z)) {
			enviroScale = Mathf.SmoothDamp(enviroScale, simulateMuse? simValue : (isActive ? MuseManager.Inst.LastConcentrationMeasure : 1f), ref enviroScaleVel, MuseManager.Inst.SlowResponse? 5f : 1f);
			enviroScale = Mathf.MoveTowards(enviroScale, simulateMuse? simValue : (isActive ? MuseManager.Inst.LastConcentrationMeasure : 1f), Time.deltaTime * 0.001f);
		}
		enviroScale = Mathf.Clamp01(enviroScale);
		
        lastUseOctree = useOctree;


        // Start Asexual mutation / genetic testing if user holds enviroScale at zero for long enough.
        if (isActive && (enviroScale <= 0.01f)) {
            timeAtZero += Time.deltaTime;
            if(enableMutationOnFocus && (timeAtZeroToStartTest >= 0f) && (timeAtZero > timeAtZeroToStartTest) && (AssemblyEditor.Inst == null))
                StartTest();
        }
        else
            timeAtZero = 0f;

        // Toggle Asexual mutation at zero enviroScale value
        if (Input.GetKeyUp(KeyCode.Space))
            enableMutationOnFocus = !enableMutationOnFocus;

    } // End of Update().

    private void StartTest() {
		print("Running test!");

        AssemblyEditor testRunner = new GameObject("AssemblyEditor").AddComponent<AssemblyEditor>();
        testRunner.capturedAssembly = targetNode.PhysAssembly;
        testRunner.DoTest(AssemblyEditor.MenuType.maximumSpeed);
        testRunner.TestDone += OnTestDone;
		targetNode = null;

		CameraControl.Inst.SetMode_AssemblyHerd();
    } // End of StartTest().

    private void OnTestDone(AssemblyEditor sender) {
        if(ClientTest.Inst.Winner != null) {
            for(int i=0; i < ClientTest.Inst.Winner.Nodes.Count; ++i) {
                if (!ClientTest.Inst.Winner.Nodes[i].IsMuscle)
                    targetNode = ClientTest.Inst.Winner.Nodes[i];
            }

            // Make sure assembly doesn't die right away
            ClientTest.Inst.Winner.SetReborn();
        }
        targetNode = ClientTest.Inst.Winner != null ? ClientTest.Inst.Winner.Nodes[0] : null;
		targetNode.PhysAssembly.isTraitTest = false;
		CameraControl.Inst.SetMode_NeuroScaleFocus(targetNode);

        Destroy(sender);
        timeAtZero = 0f;

		SetAllNodeVisibility(true);
        Cull();
    } // End of OnTestDone().

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
                    if(cullSelectedAssemblyNodes)
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
            else if (enviroScale >= 0.99f || numNodesToShow >= Node.getAll.Count) {
                if (ClientTest.Inst != null)
                    return;
                SetAllNodeVisibility(true);
            }
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
                if (numFoodToShow == FoodPellet.all.Count)
                    SetAllFoodVisibility(true);
                else {
                    foodSortedSet.Clear();
                    float boundsSize = RenderSettings.fogEndDistance - RenderSettings.fogStartDistance;
                    Bounds foodBoundary = new Bounds(targetNode.Position, boundsSize * Vector3.one);
                    FoodPellet.AllFoodTree.RunActionInRange(new System.Action<FoodPellet>(CullFood), foodBoundary);
                    HandleCulledFoodVisibility();
                }
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
                FoodPellet.all[i].Visible = i < numNodesToShow;
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
    } // End of AddToSortedList().

    void SetAllNodeVisibility(bool vis)
    {
        for (int i = 0; i < Node.getAll.Count; i++) {
            if (Node.getAll[i].PhysAssembly == null || !Node.getAll[i].PhysAssembly.isTraitTest)
                Node.getAll[i].Visible = vis;
        }

    } // End of SetAllNodeVisibility().

    void HandleCulledNodeVisibility()
    {
        SetAllNodeVisibility(false);
        if (ClientTest.Inst != null)
            return;

        if (!cullSelectedAssemblyNodes)
            foreach (Node n in targetNode.PhysAssembly.Nodes)
                n.Visible = true;

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
            FoodPellet.all[i].Visible = vis;
    } // End of SetAllFoodVisibility().

    void HandleCulledFoodVisibility()
    {
        SetAllFoodVisibility(false);
        foreach (KeyValuePair<float, FoodPellet> foodPair in foodSortedSet)
            foodPair.Value.Visible = true;
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

	void OnGUI() {
		if(simulateMuse) {
			GUI.skin.label.alignment = TextAnchor.UpperCenter;
			string info = "Muse Override - " + (enviroScale * 100f).ToString("F0") + "%";
			GUI.Label(new Rect(0f, 0f, Screen.width, Screen.height), info);
		}
	} // End of OnGUI().
} // End of NeuroScaleDemo.
