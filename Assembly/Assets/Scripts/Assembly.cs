using UnityEngine;
using System.Collections;
using System.IO;

public class Assembly {

    public string callsign;
    public Node[] nodes;

    public void Update(){
        // Repulsive force between nodes within assembly.
        for( int i = 0; i < nodes.Length; i++ ){
			Node currentNode = nodes[i];
			
			// Kinetic nteraction with other nodes...
			for( int j = (i + 1); j < nodes.Length; j++ ){
				Node otherNode = nodes[j];

                Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
				float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
				
				// Repulsive force
				Vector3 repulsiveForce = 1000 * ( -vectorToNode / Mathf.Pow( distToNode, 5 ));

				currentNode.rigidbody.AddForce(repulsiveForce);
				otherNode.rigidbody.AddForce(-repulsiveForce);
            }
        }
    }

    public Vector3 GetCenter(){
        Vector3 totalPos = Vector3.zero;
        for(int i = 0; i < nodes.Length; i++) {
            totalPos += nodes[i].transform.position;
        }
        totalPos /= nodes.Length;
        return totalPos;
    }

    public float GetRadius(){
        float greatestRad = 0;
        Vector3 center = GetCenter();
        for(int i = 0; i < nodes.Length; i++){
            float radius = Vector3.Distance(center, nodes[i].transform.position);
            if(radius > greatestRad)
                greatestRad = radius;
        }
        return greatestRad;
    }

    // Save assembly to a file.
    public void Save()
    {        
        DirectoryInfo dir = new DirectoryInfo("C:/Assembly/saves");
        FileInfo[] info = dir.GetFiles("*.*");
        int lastFileNum = 0;
        for (int t = 0; t < info.Length; t++)
        {
            FileInfo currentFile = info[t];
            int currentFileNum = int.Parse(currentFile.Name.Substring(0, 3));
            if (currentFileNum >= lastFileNum)
                lastFileNum = currentFileNum;
        }
        System.IO.File.WriteAllText("C:/Assembly/saves/" + (lastFileNum + 1).ToString("000") + "_" + callsign + ".txt", GetDNA());
        
    }

    private string GetDNA()
    {
        string newDNA = "";
        for (int i = 0; i < nodes.Length; i++)
        {
            Node currentNode = nodes[i];
            // First part of the DNA is the node's index and node characteristics.
            newDNA += i + "_" + currentNode.GetDNAInfo();

            for (int j = 0; j < currentNode.bonds.Length; j++)
            {
                Bond currentBond = currentNode.bonds[j];
                if (currentBond.nodeA == currentNode)
                    newDNA += "-" + currentBond.nodeB.assemblyIndex;
            }

            // This node is done, indicated by ','
            newDNA += ",";
        }
        return newDNA;
    }
}
