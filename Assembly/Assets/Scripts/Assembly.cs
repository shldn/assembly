﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assembly {

    /* all nodes in assembly --------------------------------------*/
    public static List<Assembly> allAssemblies = new List<Assembly>();
    public static List<Assembly> GetAll() { return allAssemblies; }
    public bool hasValidNodes = false;

    /* destroying assemblies and nodes -----------------------------*/
    //stores assemblies to be deleted for the frame update
    //public static List<Assembly> destroyAssemblies = new List<Assembly>();
    //public static List<Assembly> assembliesToDestroy = new List<Assembly>();
    //private bool removedFromUpdateTransform = false;
    //public bool markedRemoved = false;

    public string name = System.DateTime.Now.ToString("MMddyyHHmmssff");
	public List<Node> nodes = new List<Node>();

    public GameObject physicsObject = null;

    //asmbly control
    public static int MIN_ASSEMBLY = 1;
    public static int MAX_ASSEMBLY = 10;
    public static int MAX_NODES_IN_ASSEMBLY = 10;
    public static int MIN_NODES_IN_ASSEMBLY = 10;
    public static bool REFACTOR_IF_INERT = false; // If an assembly is created with no logic nets, destroy it immediately.

    public Vector3 WorldPosition{
        get { 
            if(physicsObject)
                return physicsObject.transform.position;
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

    /* energy --------------------------------------------------- */
    public float currentEnergy = 0; //should be sum of nodes
    public float consumeRate = 10.0f; //rate asm consume food
    public float energyBurnRate = 0; //rate asm burn energy
    public bool  needBurnRateUpdate = true;
    public static float burnCoefficient = 1.0f;

    public float MaxEnergy { get{ return nodes.Count; }}
    public float Health { get{ return currentEnergy / MaxEnergy; }
                          set{ currentEnergy = MaxEnergy * value; }}

    public static implicit operator bool(Assembly exists){
        return exists != null;
    }

    public static Assembly GetRandomAssembly(int numNodes){
        Assembly newAssembly = new Assembly();

        Node seedNode = new Node();
        newAssembly.AddNode(seedNode);
        for(int j = 0; j < numNodes; j++)
            newAssembly.AddRandomNode();

        newAssembly.InitPhysicsObject();
        newAssembly.InitEnergyData();

        return newAssembly;
    }

    // Constructors
    public Assembly(){
        allAssemblies.Add(this);
    }
    public Assembly(List<Node> nodes){
        allAssemblies.Add(this);

        AddNodes(nodes);
        InitPhysicsObject();
        InitEnergyData();
    }

    
   /*public Assembly(string filePath){
        List<Node> newNodes = new List<Node>();
        Vector3 worldPos = new Vector3();
        IOHelper.LoadAssembly(filePath, ref name, ref worldPos, ref newNodes);

        // ordering a little tricky at the moment, multiple interdependencies
        InitPhysicsObject();
        WorldPosition = worldPos;
        AddNodes(newNodes);
        RecomputeRigidbody();
        allAssemblies.Add(this);
        InitEnergyData();
    }*/
    

    public Assembly Reproduce(){
        Assembly offspring = Duplicate();
        offspring.Mutate(0.2f);
        offspring.physicsObject.rigidbody.AddForce(Random.rotation * Vector3.forward * 1000f);
        offspring.physicsObject.rigidbody.AddTorque(Random.rotation * Vector3.forward * 10000f);
        return offspring;
    } // End of Reproduce().

    // Copy Constructor - return a copy of this assembly
    public Assembly Duplicate(){
        List<Node> newNodes = new List<Node>();
        for (int i = 0; i < nodes.Count; ++i){
            Node newNode = nodes[i].Duplicate();
            newNodes.Add(newNode);
        }

        Assembly a = new Assembly(newNodes);
        return a;
    } // End of Duplicate().


    // Generates and initializes the Unity-driven physics object for the Assembly.
    public void InitPhysicsObject(){
        physicsObject = new GameObject(name);
        physicsObject.AddComponent<Rigidbody>();
        physicsObject.rigidbody.useGravity = false;
        physicsObject.rigidbody.angularDrag = 0.2f;
        physicsObject.rigidbody.drag = 0.2f;
    } // End of InitPhysicsObject().

    // Sets up center of mass, mass, etc. for the assembly based on current structure.
    public void RecomputeRigidbody(){
        if(!physicsObject)
            return;

        if(nodes.Count > 0){
            physicsObject.rigidbody.inertiaTensor = Vector3.one * nodes.Count * 30f;
            physicsObject.rigidbody.mass = nodes.Count;
        } else {
            MonoBehaviour.print("Zero nodes!");
            physicsObject.rigidbody.mass = 1f;
            physicsObject.rigidbody.inertiaTensor = Vector3.one;
        }

        Vector3 centerOfMass = Vector3.zero;
        for(int i = 0; i < nodes.Count; i++){
            centerOfMass += nodes[i].worldPosition;
        }
        centerOfMass /= (float)nodes.Count;

        if(nodes.Count > 0){
            physicsObject.rigidbody.centerOfMass = physicsObject.transform.InverseTransformPoint(centerOfMass);
            physicsObject.rigidbody.inertiaTensor = Vector3.one * nodes.Count * 30f;
        }
    } // End of ComputerPhysics().


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

        physicsObject = null;
        allAssemblies.Remove(this);
    }

    /*public void Save(){
        string path = "./saves/" + name + ".txt";
        Save(path);
    } // End of Save().


    public void Save(string path){
        ConsoleScript.Inst.WriteToLog("Saving " + path);
        IOHelper.SaveAssembly(path, this);
    } // End of Save().*/


    public void AddNode(Node node){
        node.assembly = this;
        nodes.Add(node);
        UpdateNodes();
        UpdateNodeValidities();
        needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
    } // End of AddNode().


    public void AddNodes(List<Node> newNodes)
    {
        for (int i = 0; i < newNodes.Count; ++i)
            newNodes[i].assembly = this;
        nodes.AddRange(newNodes);
        UpdateNodes();
        UpdateNodeValidities();
        needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
    } // End of AddNode().


    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
        UpdateNodes();
        UpdateNodeValidities();
        needBurnRateUpdate = true;
        needRigidbodyUpdate = true;
    } // End of RemoveNode().


    public void UpdateTransform(){

        if(Input.GetKey(KeyCode.U))
            WorldPosition = Vector3.zero;


        // Dead assemblies should be destroyed with animation.
        if( currentEnergy < 0.0f){
            DestroyWithAnimation();
        }

        // Useless assemblies should be immediately deleted.
        if(REFACTOR_IF_INERT && !hasValidNodes){
            Destroy();
            GameManager.Inst.SeedNewRandomAssembly();
            return;
        }

        if(needRigidbodyUpdate){
            RecomputeRigidbody();
        }

        //Propel assembly through the world based on activated nodes.
        List<Node> allActuateNodes = GetActuateNodes();
        for(int i = 0; i < allActuateNodes.Count; i++){
            allActuateNodes[i].propelling = false;
            needBurnRateUpdate = true;
            GetFunctionalPropulsion();
        }

        //print debug
        if( needBurnRateUpdate ){
            needBurnRateUpdate = false;
            UpdateEnergyBurnRate();
        }
        //assembly consume energy
        CalculateEnergyUse();
        //ConsoleScript.Inst.WriteToLog(currentEnergy+ " remains");
            //Debug.Log(currentEnergy+ " remains");


        // If assembly has 200% health, it reproduces!
        if(Health >= 2f){
            Object.Instantiate(PrefabManager.Inst.reproduceBurst, WorldPosition, Quaternion.identity);

            Assembly offspringAssem = Reproduce();
            offspringAssem.WorldPosition = WorldPosition;
            offspringAssem.WorldRotation = WorldRotation;
            offspringAssem.physicsObject.rigidbody.velocity = physicsObject.rigidbody.velocity;
            offspringAssem.physicsObject.rigidbody.angularVelocity = physicsObject.rigidbody.angularVelocity;

            Health /= 2f;
        }

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
            nodes[i].UpdateTransform();
            nodes[i].UpdateType();
        }

        RecomputeRigidbody();
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
            if(nodes[i].nodeType == NodeType.sense)
                senseNodes.Add(nodes[i]);

        return senseNodes;
    } // End of GetSenseNodes().

    // Returns the assembly's actuate nodes.
    public List<Node> GetActuateNodes(){
        List<Node> actuateNodes = new List<Node>();
        for(int i = 0; i < nodes.Count; i++)
            if(nodes[i].nodeType == NodeType.actuate)
                actuateNodes.Add(nodes[i]);

        return actuateNodes;
    } // End of GetActuateNodes().

    // Returns the assembly's control nodes.
    public List<Node> GetControlNodes(){
        List<Node> controlNodes = new List<Node>();
        for(int i = 0; i < nodes.Count; i++)
            if(nodes[i].nodeType == NodeType.control)
                controlNodes.Add(nodes[i]);

        return controlNodes;
    } // End of GetControlNodes().


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

    // returns the fitness of this assembly in the current environment
    public float Fitness(){
        return 1;
    }

    //consume food within range
    public void Consume(FoodPellet food){
        float realConsumeRate = (consumeRate* Time.deltaTime); 
        food.currentEnergy -= realConsumeRate;
        if( food.currentEnergy < 0){
            currentEnergy += ( food.currentEnergy + realConsumeRate);
            //destroy and create
            food.Destroy();
        }else {
            currentEnergy += realConsumeRate;
        }
    }

    //energy that is being used
    public void CalculateEnergyUse(){
        currentEnergy -= (energyBurnRate * Time.deltaTime * burnCoefficient);
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
} // End of Assembly.
