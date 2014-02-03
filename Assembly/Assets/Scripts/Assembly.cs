using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Assembly {

    public static List<Assembly> allAssemblies = new List<Assembly>();

    // temp
    public Color color = Color.white;

    public string name = "unnamed";
	public List<Node> nodes = new List<Node>();

    public Vector3 worldPosition = Vector3.zero;
    public Quaternion worldRotation = Quaternion.identity;
    public Quaternion worldRotationVel = Quaternion.identity;

    public static implicit operator bool(Assembly exists){
        return exists != null;
    }


    // Constructors
    public Assembly(){
        allAssemblies.Add(this);
    }
    public Assembly(List<Node> nodes){
        this.nodes = nodes;
        allAssemblies.Add(this);
    }


    public void AddNode(Node node){
        node.assembly = this;
        nodes.Add(node);
    } // End of AddNode().

    public void RemoveNode(Node node){
        nodes.Remove(node);
    } // End of RemoveNode().



    public void UpdateTransform(){
        worldRotation = Quaternion.RotateTowards(worldRotation, worldRotation * worldRotationVel, Time.deltaTime * 2f);
    } // End of UpdateTransform().


    // Attaches a new randomized node to a random part of the assembly.
    public void AddRandomNode(){

        // Loop through all nodes, starting with a random one.
        int nodeStartIndex = Random.Range(0, nodes.Count);
        for(int i = 0; i < nodes.Count; i++){
            Node currentNode = nodes[(nodeStartIndex + i) % nodes.Count];

            // Skip this node if it already has 3 neighbors.
            if(CountNeighbors(currentNode.localHexPosition) > 2)
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
                    List<Node> neighbors = GetNeighbors(currentPos);
                    if(neighbors.Count > 2)
                        continue;

                    bool tooManyNeighborNeighbors = false;
                    for(int k = 0; k < neighbors.Count; k++){
                        if(CountNeighbors(neighbors[k].localHexPosition) > 2){
                            tooManyNeighborNeighbors = true;
                            break;
                        }
                    }

                    // Clear spot... let's do it!
                    if(!tooManyNeighborNeighbors){
                        Node newNode = new Node(currentPos);
                        AddNode(newNode);

                        newNode.gameObject.renderer.material.color = color;
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

            List<Node> neighbors = GetNeighbors(currentNode.localHexPosition);

            // If we have neighbors, test against each one to see if the assembly would be bisected if we
            //   removed the current node.
            // We do this by just counting how many nodes the neighbor 'leads to', that is, how many
            //   it is connected to (vicariously.)
            bool fail = false;
            if(neighbors.Count > 1){
                for(int j = 0; j < neighbors.Count; j++){
                    List<Node> nodesOmitThis = new List<Node>(nodes);
                    nodesOmitThis.Remove(currentNode);

                    if(CountAllNeighborsRecursive(neighbors[j], nodesOmitThis) < (nodes.Count - 2)){
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
            Node currentNode = nodes[i];
            int neighborCount = CountNeighbors(currentNode.localHexPosition);
            switch(neighborCount){
                case 1:
                    currentNode.nodeType = NodeType.sense;
                    currentNode.gameObject.renderer.material.color = Node.senseColor;
                    break;
                case 2:
                    currentNode.nodeType = NodeType.actuate;
                    currentNode.gameObject.renderer.material.color = Node.actuatorColor;
                    break;
                case 3:
                    currentNode.nodeType = NodeType.control;
                    currentNode.gameObject.renderer.material.color = Node.controlColor;
                    break;
                default:
                    currentNode.nodeType = NodeType.none;
                    currentNode.gameObject.renderer.material.color = Node.stemColor;
                    break;
            }
        }
    } // End of UpdateNodes(). 


    int CountNeighbors(IntVector3 hexPos){
        // Count number of neighbors for the new position. If 3 or more, move on.
        int neighborCount = 0;
        for(int k = 0; k < 12; k++){
            IntVector3 currentNeighborPos = hexPos + HexUtilities.Adjacent(k);
            for(int m = 0; m < nodes.Count; m++){
                if(nodes[m].localHexPosition == currentNeighborPos){
                    neighborCount++;
                }
            }
        }
        return neighborCount;
    } // End of CountNeighbors().


    List<Node> GetNeighbors(IntVector3 hexPos){
        // Count number of neighbors for the new position. If 3 or more, move on.
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


    public int CountAllNeighborsRecursive(Node node, List<Node> nodesToTest){
        int neighborCount = 0;
        nodesToTest.Remove(node);

        List<Node> neighbors = GetNeighbors(node.localHexPosition);
        List<Node> testableNeighbors = new List<Node>();

        for(int i = 0; i < neighbors.Count; i++){
            if(nodesToTest.Contains(neighbors[i])){
                testableNeighbors.Add(neighbors[i]);
            }
        }

        neighborCount = testableNeighbors.Count;

        for(int i = 0; i < testableNeighbors.Count; i++)
            nodesToTest.Remove(testableNeighbors[i]);

        for(int i = 0; i < testableNeighbors.Count; i++){
            neighborCount += CountAllNeighborsRecursive(testableNeighbors[i], nodesToTest);
        }

        return neighborCount;
    } // End of CountAllNeighborsRecursive();



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
} // End of Assembly.
