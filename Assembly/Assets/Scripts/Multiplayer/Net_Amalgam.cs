using UnityEngine;
using System.Collections;

public class Net_Amalgam : MonoBehaviour {

    public TextMesh nametag;


	void Update(){

        for(int i = 0; i < Net_Manager.playerList.Length; i++){
            if(Net_Manager.playerList[i].player == networkView.owner)
                nametag.text = Net_Manager.playerList[i].playerName;
        }

        if(networkView.isMine){
            transform.position = MainCameraControl.Inst.transform.position;
            transform.rotation = MainCameraControl.Inst.transform.rotation;

            Renderer[] allRenderers = transform.GetComponentsInChildren<Renderer>();
            foreach(Renderer someRenderer in allRenderers)
                someRenderer.enabled = false;
        }

        Rect screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
        if(screenRect.Contains(Input.mousePosition) && Input.GetKeyDown(KeyCode.Insert))
            SendAssemblies();

	} // End of Update().
	
    // Synchronizes contantly-updating things between amalgams.
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info){
	    Vector3 pos = Vector3.zero;
	    Quaternion rot = Quaternion.identity;

		// When sending my own data out...
		if(stream.isWriting){
			pos = transform.position;
            rot = transform.rotation;
			    
			stream.Serialize(ref pos);
			stream.Serialize(ref rot);
		}
		// When receiving data from someone else...
		else{
            stream.Serialize(ref pos);
			stream.Serialize(ref rot);
			
            transform.position = pos;
            transform.rotation = rot;
		}
    } // End of OnSerializeNetworkView().

    public void SendAssemblies(){
        // Create a new array out of the array list because every time we make a new assembly it is added to
        //   the existing allAssemblies array.
        Assembly[] initialAssemblies = Assembly.allAssemblies.ToArray();
        for(int i = 0; i < initialAssemblies.Length; i++){
            Assembly currentAssembly = initialAssemblies[i];

            string assemblyData = "";

            // Values are sent as "<value><character indicator>", so "positions.x = 3.534" is sent as "3.534x".

            // Store world position
            assemblyData += currentAssembly.WorldPosition.x + "x";
            assemblyData += currentAssembly.WorldPosition.y + "y";
            assemblyData += currentAssembly.WorldPosition.z + "z";

            assemblyData += currentAssembly.WorldRotation.x + "x";
            assemblyData += currentAssembly.WorldRotation.y + "y";
            assemblyData += currentAssembly.WorldRotation.z + "z";
            assemblyData += currentAssembly.WorldRotation.z + "w";
            
            // Store node data.
            for(int j = 0; j < currentAssembly.nodes.Count; j++){
                Node currentNode = currentAssembly.nodes[j];

                // Node position.
                assemblyData += currentNode.localHexPosition.x + "x";
                assemblyData += currentNode.localHexPosition.y + "y";
                assemblyData += currentNode.localHexPosition.z + "z";
                assemblyData += "n";
            }

            networkView.RPC("ReceiveAssembly", RPCMode.All, assemblyData);
        }
    } // End of SendAssemblies().

    [RPC]
    void ReceiveAssembly(string assemblyData){
        int cursor = 0;

        // Find world position.
        Vector3 newPosition = Vector3.zero;

        newPosition.x = ExtractFloat(ref assemblyData, 'x');
        newPosition.y = ExtractFloat(ref assemblyData, 'y');
        newPosition.z = ExtractFloat(ref assemblyData, 'z');

        // World rotation
        Quaternion newRotation = Quaternion.identity;

        newRotation.x = ExtractFloat(ref assemblyData, 'x');
        newRotation.y = ExtractFloat(ref assemblyData, 'y');
        newRotation.z = ExtractFloat(ref assemblyData, 'z');
        newRotation.z = ExtractFloat(ref assemblyData, 'w');

        // Create assembly
        Assembly newAssembly = Assembly.GetRandomAssembly(1);
        newAssembly.WorldPosition = newPosition;
        newAssembly.WorldRotation = newRotation;
        newAssembly.networkEffect = true;

        // Generate nodes
        string[]nodeData = assemblyData.Split('n');
        for(int j = 0; j < nodeData.Length - 1; j++){
            string currentNodeData = nodeData[j];
            IntVector3 newNodeHexPos = IntVector3.zero;

            newNodeHexPos.x = ExtractInt(ref currentNodeData, 'x');
            newNodeHexPos.y = ExtractInt(ref currentNodeData, 'y');
            newNodeHexPos.z = ExtractInt(ref currentNodeData, 'z');

            // Make this more specific...
            newAssembly.AddNode(new Node(newNodeHexPos));
        }
    } // End of ReceiveAssemblies().

    float ExtractFloat(ref string text, char delimiter){
        int cursor = text.IndexOf(delimiter);
        float data = float.Parse(text.Substring(0, cursor));
        text = text.Remove(0, cursor + 1);
        return data;
    } // End of ExtractFloat().

    int ExtractInt(ref string text, char delimiter){
        int cursor = text.IndexOf(delimiter);
        int data = int.Parse(text.Substring(0, cursor));
        text = text.Remove(0, cursor + 1);
        return data;
    } // End of ExtractFloat().

} // End of Net_Amalgam.
