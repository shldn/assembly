using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assembly {

    public static List<Assembly> allAssemblies = new List<Assembly>();
    public static List<Assembly> GetAll() { return allAssemblies; }

    public string name = "unnamed";
	public List<Node> nodes = new List<Node>();

    public Vector3 worldPosition = Vector3.zero;
    public Quaternion worldRotation = Quaternion.identity;
    public Quaternion worldRotationVel = Quaternion.identity;

    public static implicit operator bool(Assembly exists){
        return exists != null;
    }

    public static Assembly GetRandomAssembly(int numNodes){
        Assembly newAssembly = new Assembly();

        Node seedNode = new Node();
        newAssembly.AddNode(seedNode);
        for(int j = 0; j < numNodes; j++)
            newAssembly.AddRandomNode();
        return newAssembly;
    }

    // Constructors
    public Assembly(){
        allAssemblies.Add(this);
    }
    public Assembly(List<Node> nodes){
        AddNodes(nodes);
        allAssemblies.Add(this);
    }
    public Assembly(string filePath){
        List<Node> newNodes = new List<Node>();
        IOHelper.LoadAssembly(filePath, ref name, ref worldPosition, ref newNodes);
        AddNodes(newNodes);
        allAssemblies.Add(this);
    }

    // Copy Constructor - return a copy of this assembly
    public Assembly Duplicate(){
        List<Node> newNodes = new List<Node>();
        for (int i = 0; i < nodes.Count; ++i)
            newNodes.Add(nodes[i].Duplicate());

        Assembly a = new Assembly(newNodes);
        a.worldPosition = worldPosition;
        a.worldRotation = worldRotation;
        a.worldRotationVel = worldRotationVel;
        return a;
    } // End of Duplicate().


    public void Destroy(){
        for (int i = nodes.Count-1; i >= 0; --i)
            nodes[i].Destroy();
        allAssemblies.Remove(this);
    }

    public void Save(){
        string path = "./saves/" + name + ".txt";
        Save(path);
    } // End of Save().


    public void Save(string path){
        Debug.Log("Saving " + path);
        IOHelper.SaveAssembly(path, this);
    } // End of Save().


    public void AddNode(Node node){
        node.assembly = this;
        nodes.Add(node);
    } // End of AddNode().


    public void AddNodes(List<Node> newNodes)
    {
        for (int i = 0; i < newNodes.Count; ++i)
            newNodes[i].assembly = this;
        nodes.AddRange(newNodes);
        UpdateNodes();
    } // End of AddNode().


    public void RemoveNode(Node node)
    {
        nodes.Remove(node);
    } // End of RemoveNode().


    public void UpdateTransform(){
        worldRotation = Quaternion.RotateTowards(worldRotation, worldRotation * worldRotationVel, Time.deltaTime * 2f);

        //Propel assembly through the world based on activated nodes.
        List<Node> allActuateNodes = GetActuateNodes();
        for(int i = 0; i < allActuateNodes.Count; i++){
            if(!allActuateNodes[i].jetEngine)
                continue;

            ParticleEmitter emitter = allActuateNodes[i].jetEngine.GetComponentInChildren<ParticleEmitter>();
            if(!emitter)
                continue;

            emitter.emit = false;
        }

        worldPosition += GetFunctionalPropulsion() * Time.deltaTime;

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
                        UpdateNodes();
                        return;
                    }
                }
            }
        }
    } // End of Mutate().


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
            currentNode.Destroy();
            UpdateNodes();
            return;
        }
    } // End of RemoveRandomNode(). 


    public void UpdateNodes(){
        for(int i = 0; i < nodes.Count; i++){
            nodes[i].UpdateTransform();
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


    // Returns the assembly's propulsion if all of it's sense nodes fired at once.
    public Vector3 GetFunctionalPropulsion(){
        List<Node> senseNodes = GetSenseNodes();
        List<Node> validActuateNodes = new List<Node>();
        Vector3 propulsion = Vector3.zero;

        // Loop through all sense nodes.
        for(int i = 0; i < senseNodes.Count; i++){
            if(!senseNodes[i].DetectFood())
                continue;

            // Get the sense node's functionally connected nodes.
            List<Node> networkedNodes = senseNodes[i].GetFullLogicNet();
            // Loop through those connected nodes.
            for(int j = 0; j < networkedNodes.Count; j++){
                Node currentNode = networkedNodes[j];
                // If the node is an actuator and hasn't been accounted for, stash it and get it's actuateVector.
                if((currentNode.nodeType == NodeType.actuate) && !validActuateNodes.Contains(currentNode)){
                    validActuateNodes.Add(currentNode);
                    propulsion += currentNode.worldAcuateVector;
                }
            }
        }

        for(int i = 0; i < validActuateNodes.Count; i++){
            if(!validActuateNodes[i].jetEngine)
                continue;

            ParticleEmitter emitter = validActuateNodes[i].jetEngine.GetComponentInChildren<ParticleEmitter>();
            if(!emitter)
                continue;

            emitter.emit = true;
        }

        return propulsion;
    } // End of GetMaximumPropulsion().


    // Returns the assembly's propulsion if all of it's sense nodes fired at once.
    public Vector3 GetMaximumPropulsion(){
        List<Node> senseNodes = GetSenseNodes();
        List<Node> validActuateNodes = new List<Node>();
        Vector3 propulsion = Vector3.zero;

        // Loop through all sense nodes.
        for(int i = 0; i < senseNodes.Count; i++){
            // Get the sense node's functionally connected nodes.
            List<Node> networkedNodes = senseNodes[i].GetFullLogicNet();
            // Loop through those connected nodes.
            for(int j = 0; j < networkedNodes.Count; j++){
                Node currentNode = networkedNodes[j];
                // If the node is an actuator and hasn't been accounted for, stash it and get it's actuateVector.
                if((currentNode.nodeType == NodeType.actuate) && !validActuateNodes.Contains(currentNode)){
                    validActuateNodes.Add(currentNode);
                    propulsion += currentNode.worldAcuateVector;
                }
            }
        }
        
        for(int i = 0; i < validActuateNodes.Count; i++){
            if(!validActuateNodes[i].jetEngine)
                continue;

            ParticleEmitter emitter = validActuateNodes[i].jetEngine.GetComponentInChildren<ParticleEmitter>();
            if(!emitter)
                continue;

            emitter.emit = true;
        }

        return propulsion;
    } // End of GetMaximumPropulsion().


    // returns the fitness of this assembly in the current environment
    public float Fitness()
    {
        Vector3 functionalPropulsion = GetFunctionalPropulsion();

        if(functionalPropulsion.Equals(Vector3.zero))
            return(180);

        return Vector3.Angle(FoodPellet.GetAll()[0].worldPosition - worldPosition, GetFunctionalPropulsion());
    }

} // End of Assembly.
