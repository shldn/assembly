using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assembly : CaptureObject {

    /* all nodes in assembly --------------------------------------*/
    public static List<Assembly> allAssemblies = new List<Assembly>();
    public static List<Assembly> GetAll() { return allAssemblies; }

    public bool hasFunctioningNodes = false;
    int numFramesAlive = 0;

    /* destroying assemblies and nodes -----------------------------*/
    //stores assemblies to be deleted for the frame update
    //public static List<Assembly> destroyAssemblies = new List<Assembly>();
    //public static List<Assembly> assembliesToDestroy = new List<Assembly>();
    //private bool removedFromUpdateTransform = false;
    //public bool markedRemoved = false;

    public string name = System.DateTime.Now.ToString("MMddyyHHmmssff");
	public List<Node> nodes = new List<Node>();

    // convex hull skin variables
    public bool showMesh = true; // display convex hull mesh skin around assembly
    public bool updateMesh = false; // update the mesh with the node positions every frame

    public GameObject physicsObject = null;
    public GameObject assemblyObject = null;

    public bool imported = false;

    //asmbly control
    public static int MIN_ASSEMBLY = 1;
    public static int MAX_ASSEMBLY = 10;
    public static int MAX_NODES_IN_ASSEMBLY = 10;
    public static int MIN_NODES_IN_ASSEMBLY = 10;
    //public static bool REFACTOR_IF_INERT = false; // If an assembly is created with no logic nets, destroy it immediately.

    public Vector3 Position { get { return WorldPosition; } }
    public Vector3 WorldPosition{
        get { 
            if(physicsObject)
                return physicsObject.rigidbody.worldCenterOfMass;
            else
                return Vector3.zero;}
        set { 
            if(physicsObject)
                physicsObject.transform.position = value; }
    }

    public Quaternion WorldRotation{
        get { 
            if(physicsObject)
                return physicsObject.transform.rotation;
            else
                return Quaternion.identity;}
        set { 
            if(physicsObject)
                physicsObject.transform.rotation = value; }
    }

    public float Mass {
        get{ return nodes.Count; }
    }


    bool needRigidbodyUpdate = true;
    bool rootNodeSet = false;

    Assembly structureFriend = null;


    /* energy --------------------------------------------------- */
    public float currentEnergy = 0; //should be sum of nodes
    public float energyBurnRate = 0; //rate asm burn energy
    public bool  needBurnRateUpdate = true;
    public static float burnCoefficient = 1.0f;

    public Assembly targetMate = null;
    public Assembly gentlemanCaller = null;
    float mateCooldown = 5f;

    public float MaxEnergy { get{ return nodes.Count; }}
    public float Health { get{ return currentEnergy / MaxEnergy; }
                          set{ currentEnergy = MaxEnergy * value; }}

    public static implicit operator bool(Assembly exists){
        return exists != null;
    }

    public static Assembly GetRandomAssembly(int numNodes){
        Assembly newAssembly = new Assembly();

        newAssembly.AddNode(new Node());
        for(int j = 0; j < numNodes; j++)
            newAssembly.AddRandomNode();

        newAssembly.InitAssemblyObject();
        newAssembly.InitPhysicsObject();
        newAssembly.InitEnergyData();

        return newAssembly;
    }

    // Constructors
    public Assembly(){
        allAssemblies.Add(this);
        PersistentGameManager.CaptureObjects.Add(this);
    }
    public Assembly(List<Node> nodes){
        allAssemblies.Add(this);
        PersistentGameManager.CaptureObjects.Add(this);

        AddNodes(nodes);
        InitAssemblyObject();
        InitPhysicsObject();
        InitEnergyData();
    }


    public Assembly(string str, bool isFilePath = false){
         List<Node> newNodes = new List<Node>();
         Vector3 worldPos = new Vector3();
         if (isFilePath)
             IOHelper.LoadAssemblyFromFile(str, ref name, ref worldPos, ref newNodes);
         else
             IOHelper.LoadAssemblyFromString(str, ref name, ref worldPos, ref newNodes);

         // ordering a little tricky at the moment, multiple interdependencies
         InitPhysicsObject();
         WorldPosition = worldPos;
         AddNodes(newNodes);
         RecomputeRigidbody();
         PersistentGameManager.CaptureObjects.Add(this);
         allAssemblies.Add(this);
         InitEnergyData();
     }


    public Assembly Reproduce(){
        Assembly offspring = Duplicate();
        offspring.Mutate(0.2f);
        offspring.physicsObject.rigidbody.AddForce(Random.rotation * Vector3.forward * Random.Range(10f, 100f));
        offspring.physicsObject.rigidbody.AddTorque(Random.rotation * Vector3.forward * Random.Range(10f, 100f));
        return offspring;
    } // End of Reproduce().

    // Copy Constructor - return a copy of this assembly
    public Assembly Duplicate(){
        List<Node> newNodes = new List<Node>();
        for (int i = 0; i < nodes.Count; ++i){
            Node newNode = new Node(nodes[i]);
            newNodes.Add(newNode);
        }

        Assembly a = new Assembly(newNodes);
        return a;
    } // End of Duplicate().


    // Generates and initializes the Unity-driven physics object for the Assembly.
    public void InitPhysicsObject(){
        physicsObject = new GameObject(name);
        physicsObject.layer = LayerMask.NameToLayer("Assemblies");
        physicsObject.AddComponent<Rigidbody>();
        physicsObject.rigidbody.useGravity = false;
        physicsObject.rigidbody.angularDrag = 0.1f;
        physicsObject.rigidbody.drag = 0.3f;

    } // End of InitPhysicsObject().

    void ApplyConvexMeshToPhysicsObject()
    {
        MeshFilter meshFilter = physicsObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = physicsObject.AddComponent<MeshFilter>();
            physicsObject.AddComponent<MeshRenderer>();

            //physicsObject.renderer.material.shader = Shader.Find("Particles/Additive");
            //physicsObject.renderer.material.SetColor("_TintColor", new Color(0.05f, 0.05f, 0.07f, 1f));

            physicsObject.renderer.material = PrefabManager.Inst.assemblySkin;
            physicsObject.renderer.castShadows = false;
            physicsObject.renderer.receiveShadows = false;
        }

        // get node positions
        List<Vector3> nodePositions = new List<Vector3>(nodes.Count);

        float vertexOffset = 0.6f;
        foreach (Node n in nodes){
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(1f, 1f, 1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(1f, 1f, -1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(1f, -1f, 1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(1f, -1f, -1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(-1f, 1f, 1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(-1f, 1f, -1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(-1f, -1f, 1f) * vertexOffset)));
            nodePositions.Add(HexUtilities.HexToWorld(n.localHexPosition) + (n.worldRotation * (new Vector3(-1f, -1f, -1f) * vertexOffset)));
            
        }

        if (nodePositions.Count < 5)
            return;

        // apply the convex hull to the mesh
        Mesh mesh = meshFilter.mesh;
        ConvexHull.UpdateMeshFromPoints(nodePositions, ref mesh);
    }

    // Sets up center of mass, mass, etc. for the assembly based on current structure.
    public void RecomputeRigidbody(){
        if(!physicsObject)
            return;

        if(nodes.Count > 0){
            physicsObject.rigidbody.mass = nodes.Count * 1f;
        } else {
            MonoBehaviour.print("Zero nodes!");
            physicsObject.rigidbody.mass = 1f;
            physicsObject.rigidbody.inertiaTensor = Vector3.one;
        }
        /*
        Vector3 centerOfMass = Vector3.zero;
        for(int i = 0; i < nodes.Count; i++){
            centerOfMass += nodes[i].worldPosition;
        }
        centerOfMass /= (float)nodes.Count;
        */

        /*
        if(nodes.Count > 0){
            physicsObject.rigidbody.centerOfMass = physicsObject.transform.InverseTransformPoint(centerOfMass);
            physicsObject.rigidbody.inertiaTensor = Vector3.one * nodes.Count * 30f;
        }
        */

        if (showMesh)
            ApplyConvexMeshToPhysicsObject();

        physicsObject.AddComponent<MeshCollider>();
        physicsObject.GetComponent<MeshCollider>().convex = false;

        needRigidbodyUpdate = false;
    } // End of ComputerPhysics().


    public void InitAssemblyObject(){
        assemblyObject = MonoBehaviour.Instantiate(PrefabManager.Inst.assembly, WorldPosition, Quaternion.identity) as GameObject;
    } // End of InitAssemblyObject().


    //initialize energy for the assembly
    public void InitEnergyData(){
        currentEnergy = MaxEnergy;
    }
    //need to calibrate energy through mutation as well
    //energy decrease only when the new max is less than current
    public void CalibrateEnergy(){
        float newEnergy = MaxEnergy;
        if( newEnergy < currentEnergy)
            currentEnergy = newEnergy; 
    }

    public void Destroy(){
        for (int i = nodes.Count - 1; i >= 0; i--)
            nodes[i].Destroy();

        if(physicsObject)
            Object.Destroy(physicsObject);

        if(assemblyObject)
            GameObject.Destroy(assemblyObject);

        physicsObject = null;
        allAssemblies.Remove(this);
        PersistentGameManager.CaptureObjects.Remove(this);
    }

    public void Save(){
        string path = "./data/" + name + ".txt";
        Save(path);
    } // End of Save().


    public void Save(string path){
        ConsoleScript.Inst.WriteToLog("Saving " + path);
        IOHelper.SaveAssembly(path, this);
    } // End of Save().


    public Node AddNode(Node node){
        node.assembly = this;
        nodes.Add(node);
        //UpdateNodes();
        //needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
        return node;
    } // End of AddNode().


    public void AddNodes(List<Node> newNodes)
    {
        for (int i = 0; i < newNodes.Count; ++i)
            newNodes[i].assembly = this;
        nodes.AddRange(newNodes);
        UpdateNodes();
        needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
    } // End of AddNode().


    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
        UpdateNodes();
        needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
    } // End of RemoveNode().


    public void Update(){
        numFramesAlive++;
        UpdateNodes();


        // Dead assemblies should be destroyed with animation.
        if(currentEnergy < 0.0f){
            DestroyWithAnimation();
        }

        if(!rootNodeSet && (nodes.Count > 0)){
            nodes[Random.Range(0, nodes.Count)].rootGrowNode = true;
            rootNodeSet = true;
        }


        /*
        // Useless assemblies should be immediately deleted.
        if((numFramesAlive > 1) && REFACTOR_IF_INERT && !hasFunctioningNodes){
            Destroy();
            GameManager.Inst.SeedNewRandomAssembly();
            return;
        }
        */

        if(needRigidbodyUpdate && (numFramesAlive > 1)){
            RecomputeRigidbody();
        }


        

        if (showMesh && updateMesh)
            UpdateSkinMesh();

        if(assemblyObject && physicsObject)
            assemblyObject.transform.position = WorldPosition;

        // If assembly has 200% health, it reproduces!
        if(Health >= 2f && !PersistentGameManager.IsClient){
            Object.Instantiate(PrefabManager.Inst.reproduceBurst, WorldPosition, Quaternion.identity);
            RandomMelody.Inst.PlayNote();
            
            Assembly offspringAssem = Reproduce();
            offspringAssem.WorldPosition = WorldPosition;
            offspringAssem.WorldRotation = WorldRotation;
            offspringAssem.physicsObject.rigidbody.velocity = physicsObject.rigidbody.velocity;
            offspringAssem.physicsObject.rigidbody.angularVelocity = physicsObject.rigidbody.angularVelocity;

            Health = Mathf.Clamp(Health, 0f, 2f) * 0.5f;
        }

        /*
        if(!targetMate && !gentlemanCaller && (Random.Range(0f, 1f) <= 0.001)){
            //Find closest assembly
            float distToClosest = 9999f;
            Assembly closestAssembly = null;
            for(int i = 0; i < GetAll().Count; i++){
                if((GetAll()[i] == this) || GetAll()[i].targetMate || GetAll()[i].gentlemanCaller)
                    continue;

                float distToCurrent = Vector3.SqrMagnitude(WorldPosition - GetAll()[i].WorldPosition);
                if(distToCurrent < distToClosest){
                    distToClosest = distToCurrent;
                    closestAssembly = GetAll()[i];
                }
            }
            if(closestAssembly){
                MonoBehaviour.print("Got mate!");
                targetMate = closestAssembly;
                GameObject newMateEffectGO = MonoBehaviour.Instantiate(PrefabManager.Inst.reproducePullEffect) as GameObject;
                ReproducePullEffect newMateEffect = newMateEffectGO.GetComponent<ReproducePullEffect>();
                newMateEffect.assemblyA = this;
                newMateEffect.assemblyB = targetMate;

                targetMate.gentlemanCaller = this;
            }
        }
        */
         
        /*
        if(targetMate){
        
            if(physicsObject)
                physicsObject.rigidbody.AddForce(targetMate.WorldPosition - WorldPosition, ForceMode.Force);

            if(targetMate.physicsObject)
                targetMate.physicsObject.rigidbody.AddForce(-(targetMate.WorldPosition - WorldPosition), ForceMode.Force);

            if((Vector3.Distance(WorldPosition,targetMate.WorldPosition) <= 5f) && physicsObject && targetMate.physicsObject) {
            
                Object.Instantiate(PrefabManager.Inst.reproduceBurst, WorldPosition, Quaternion.identity);
            
                Assembly offspringAssem = Reproduce();
                offspringAssem.WorldPosition = WorldPosition;
                offspringAssem.WorldRotation = WorldRotation;
                offspringAssem.physicsObject.rigidbody.velocity = physicsObject.rigidbody.velocity;
                offspringAssem.physicsObject.rigidbody.angularVelocity = physicsObject.rigidbody.angularVelocity;
            
                targetMate.gentlemanCaller = null;
                targetMate = null;
                mateCooldown = Random.Range(10f, 20f);

                RandomMelody.Inst.PlayNote();
            }
        }
        */

        /*
        float friendDist = 50f;
        float friendMargin = 1f;
        float friendForce = 10f;
        if(structureFriend){
            Vector3 vecToFriend = structureFriend.WorldPosition - WorldPosition;
            float distToFriend = vecToFriend.magnitude;
            if(physicsObject && physicsObject.rigidbody && (distToFriend > (friendDist + friendMargin)))
                physicsObject.rigidbody.AddForce(vecToFriend.normalized * friendForce);
            
        }else{
            structureFriend = Assembly.GetAll()[Random.Range(0, Assembly.GetAll().Count)];
        }

        for(int i = 0; i < Assembly.GetAll().Count; i++){
            Assembly curAssembly = Assembly.GetAll()[i];
            if(curAssembly != this){
                Vector3 vecToOther = curAssembly.WorldPosition - WorldPosition;
                if(physicsObject && physicsObject.rigidbody && (vecToOther.magnitude < friendDist))
                    physicsObject.rigidbody.AddForce(-vecToOther.normalized * friendForce);
            }
        }
        */

    } // End of UpdateTransform().


    // Attaches a new randomized node to a random part of the assembly.
    public void AddRandomNode(){

        // Loop through all nodes, starting with a random one.
        int nodeStartIndex = Random.Range(0, nodes.Count);
        for(int i = 0; i < nodes.Count; i++){
            Node currentNode = nodes[(nodeStartIndex + i) % nodes.Count];

            // Skip this node if it already has 3 neighbors.
            if(currentNode.CountNeighbors() > 2)
                continue;

            // Loop through all directions, starting with a random one.
            int dirStart = Random.Range(0, 12);
            for(int j = 0; j < 12; j++){
                int currentDir = (dirStart + j) % 12;

                IntVector3 currentPos = currentNode.localHexPosition + HexUtilities.Adjacent(currentDir);

                // Loop through all nodes, determine if any of them occupy currentPos.
                bool occupied = false;
                for(int k = 0; k < nodes.Count; k++){
                    if(nodes[k].localHexPosition == currentPos){
                        occupied = true;
                        break;
                    }
                }

                // If the spot is unoccupied...
                if(!occupied){

                    // Too many neighbors... bail out!
                    List<Node> neighbors = GetNeighborsToPos(currentPos);
                    if(neighbors.Count > 2)
                        continue;

                    bool tooManyNeighborNeighbors = false;
                    for(int k = 0; k < neighbors.Count; k++){
                        if(neighbors[k].CountNeighbors() > 2){
                            tooManyNeighborNeighbors = true;
                            break;
                        }
                    }

                    // Clear spot... let's do it!
                    if(!tooManyNeighborNeighbors){
                        Node newNode = new Node(currentPos);
                        AddNode(newNode);
                        return;
                    }
                }
            }
        }
    } // End of AddRandomNode().


    // Removes a random single node (safely) from the assembly.
    public void RemoveRandomNode(){
        // Loop through all nodes, starting with a random one.
        int nodeStartIndex = Random.Range(0, nodes.Count);
        for(int i = 0; i < nodes.Count; i++){
            Node currentNode = nodes[(nodeStartIndex + i) % nodes.Count];

            List<Node> neighbors = currentNode.GetNeighbors();

            // If we have neighbors, test against each one to see if the assembly would be bisected if we
            //   removed the current node.
            // We do this by just counting how many nodes the neighbor 'leads to', that is, how many
            //   it is connected to (vicariously.)
            bool fail = false;
            if(neighbors.Count > 1){
                for(int j = 0; j < neighbors.Count; j++){
                    List<Node> nodesOmitThis = new List<Node>(nodes);
                    nodesOmitThis.Remove(currentNode);
                    if(Node.CountAllNeighborsRecursive(neighbors[j], nodesOmitThis) < (nodes.Count - 2)){
                        // Removing this node would bisect the assembly; we can't remove it.
                        fail = true;
                        break;
                    }
                }
                if(fail)
                    continue;
            }

            // Success--this node is safe to remove.
            RemoveNode(currentNode);
            currentNode.Destroy();
            return;
        }
    } // End of RemoveRandomNode(). 


    public void UpdateNodes(){
        for(int i = 0; i < nodes.Count; i++){
            nodes[i].UpdateType();
        }
    } // End of UpdateNodes(). 


    List<Node> GetNeighborsToPos(IntVector3 hexPos){
        List<Node> neighbors = new List<Node>();
        for(int k = 0; k < 12; k++){
            IntVector3 currentNeighborPos = hexPos + HexUtilities.Adjacent(k);
            for(int m = 0; m < nodes.Count; m++){
                if(nodes[m].localHexPosition == currentNeighborPos){
                    neighbors.Add(nodes[m]);
                }
            }
        }
        return neighbors;
    } // End of GetNeighbors().


    public void Mutate(float deviation){
        // Mutate existing nodes.
        for(int i = 0; i < nodes.Count; i++)
            nodes[i].Mutate(deviation);

        // Add/subtract entire nodes.
        int numNodesModify = 0;//= Mathf.RoundToInt(Random.Range(0f, deviation * nodes.Count));

        for(int i = 0; i < nodes.Count; i++){
            if(Random.Range(0f, 1f) <= deviation)
                numNodesModify++;
        }

        for(int i = 0; i < numNodesModify; i++){
            if(Random.Range(0f, 1f) <= 0.5)
                AddRandomNode();
            else if(nodes.Count > 1)
                RemoveRandomNode();
        }

        CalibrateEnergy();
        //change assembly burn coefficient
        //burnCoefficient = Random.Range(0.5f, 2.0f);
    } // End of Mutate().


    // Returns the assembly's sense nodes.
    public List<Node> GetSenseNodes(){
        List<Node> senseNodes = new List<Node>();
        for(int i = 0; i < nodes.Count; i++)
            if(nodes[i].GetType() == typeof(SenseNode))
                senseNodes.Add(nodes[i]);

        return senseNodes;
    } // End of GetSenseNodes().

    // Returns the assembly's actuate nodes.
    public List<Node> GetActuateNodes(){
        List<Node> actuateNodes = new List<Node>();
        for(int i = 0; i < nodes.Count; i++)
            if(nodes[i].GetType() == typeof(ActuateNode))
                actuateNodes.Add(nodes[i]);

        return actuateNodes;
    } // End of GetActuateNodes().

    // Returns the assembly's control nodes.
    public List<Node> GetControlNodes(){
        List<Node> controlNodes = new List<Node>();
        for(int i = 0; i < nodes.Count; i++)
            if(nodes[i].GetType() == typeof(ControlNode))
                controlNodes.Add(nodes[i]);

        return controlNodes;
    } // End of GetControlNodes().

    /*
    public Vector3 GetFunctionalPropulsion(){
        List<Node> senseNodes = GetSenseNodes();
        List<Node> validActuateNodes = new List<Node>();
        Vector3 propulsion = Vector3.zero;

        // Loop through all sense nodes.
        for(int i = 0; i < senseNodes.Count; i++){
            Node currentSenseNode = senseNodes[i];
            List<FoodPellet> detectedFood = new List<FoodPellet>();

            if(!currentSenseNode.DetectFood(ref detectedFood))
                // If no food pellet detected, ignore this sense node.
                continue;

            // Get the sense node's functionally connected nodes.
            List<Node> networkedNodes = senseNodes[i].GetFullLogicNet();
            // Loop through those connected nodes.
            for(int j = 0; j < networkedNodes.Count; j++){
                Node currentActuateNode = networkedNodes[j];
                // If the node is an actuator and hasn't been accounted for, stash it and get it's actuateVector.
                if((currentActuateNode.nodeType == NodeType.actuate) && !validActuateNodes.Contains(currentActuateNode)){
                    validActuateNodes.Add(currentActuateNode);

                    // For each food pellet, calculate signal to send to actuator node.
                    for(int k = 0; k < detectedFood.Count; k++){
                        FoodPellet currentFood = detectedFood[k];

                        Vector3 actuateVector = currentActuateNode.worldAcuateRot * currentSenseNode.RotToFood(currentFood) * Vector3.forward;


                        Debug.DrawRay(currentActuateNode.worldPosition, actuateVector * 3f);

                        physicsObject.rigidbody.AddForceAtPosition(actuateVector * 10f, currentActuateNode.worldPosition);

                        currentActuateNode.propelling = true;
                        needBurnRateUpdate = true;
                    }
                }
            }
            //needBurnRateUpdate = true;

        }

        return propulsion;
    } // End of GetMaximumPropulsion().
    */

    /*
    // Returns the assembly's propulsion if all of it's sense nodes fired at once.
    public void UpdateNodeValidities(){
        hasValidNodes = false;

        for(int i = 0; i < nodes.Count; i++)
            nodes[i].validLogic = false;

        List<Node> senseNodes = GetSenseNodes();
        List<Node> validActuateNodes = new List<Node>();

        // Loop through all sense nodes.
        for(int i = 0; i < senseNodes.Count; i++){

            // Get the sense node's functionally connected nodes.
            List<Node> networkedNodes = senseNodes[i].GetFullLogicNet();

            // Loop through those connected nodes.
            for(int j = 0; j < networkedNodes.Count; j++){
                Node currentNode = networkedNodes[j];

                // If the node is an actuator and hasn't been accounted for, get the 'wire' between it and the sense node.
                if((currentNode.nodeType == NodeType.actuate) && !validActuateNodes.Contains(currentNode)){
                    validActuateNodes.Add(currentNode);

                    currentNode.validLogic = true;
                    senseNodes[i].validLogic = true;
                    hasValidNodes = true;

                    List<Node> actuatorNetwork = currentNode.GetFullReverseLogicNet();

                    // Get control node wire between the two.
                    List<Node> controlNetwork = new List<Node>();
                    for(int k = 0; k < actuatorNetwork.Count; k++){
                        if((actuatorNetwork[k].nodeType == NodeType.control) && networkedNodes.Contains(actuatorNetwork[k])){
                            controlNetwork.Add(actuatorNetwork[k]);
                            actuatorNetwork[k].validLogic = true;
                        }
                    }
                }
            }
        }

        for(int i = 0; i < nodes.Count; i++)
            nodes[i].UpdateColor();
    } // End of UpdateNodeValidities().
    */
    
    // returns the fitness of this assembly in the current environment
    public float Fitness(){
        return 1;
    }

    //energy that is being used
    public void CalculateEnergyUse(){
        if(this != CameraControl.Inst.selectedAssembly)
            currentEnergy -= (energyBurnRate * Time.deltaTime * burnCoefficient * 0.1f);
    }

    //update burn rate for asmbly
    public void UpdateEnergyBurnRate(){
        float totalBurn = 0.0f;
        foreach( var node in nodes){
            totalBurn += node.GetBurnRate();
        }
        energyBurnRate = totalBurn;///nodes.Count;
    }

    public void DestroyWithAnimation(){
        for(int i = nodes.Count - 1; i >= 0; i--){
            Node node = nodes[i];

            // Special-case node removal
            nodes.RemoveAt(i);
            node.assembly = null;
            node.UpdateType();

            node.doomed = true;
            node.sendOffVector = Random.rotation * (Vector3.forward * Random.Range(1f, 10f));
        }        
        Destroy();
    }

    private void UpdateSkinMesh()
    {
        MeshFilter meshFilter = physicsObject.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            for (int i = 0; i < nodes.Count && i < meshFilter.mesh.vertices.Length; ++i)
                meshFilter.mesh.vertices[i] = HexUtilities.HexToWorld(nodes[i].localHexPosition);
        }
    } // End UpdateSkinMesh

    public string ToFileString()
    {
        return IOHelper.AssemblyToString(this);
    }

} // End of Assembly.
