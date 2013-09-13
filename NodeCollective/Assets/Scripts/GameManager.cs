using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	public static Node[] allNodes;
	public static Bond[] allBonds;
	
    public static Prefabs prefabs;
	float totalEnergy;
	
	public static GUISkin readoutSkin;


    void Awake()
    {
        prefabs = GetComponent<Prefabs>();
		
        allNodes = FindObjectsOfType(typeof(Node)) as Node[];
        allBonds = FindObjectsOfType(typeof(Bond)) as Bond[];
		
		// Randomly assign bonds.
		// Loop through all nodes...
		for( int i = 0; i < allNodes.Length; i++ )
		{
			Node currentNode = allNodes[i];
			
			for(int j = 0; j < allNodes.Length; j++)
			{
				Node otherNode = allNodes[j];
				if(Random.Range(0.0f, 1.0f) <= 0.01)
				{
					bool bondExists = false;
					for(int k = 0; k < allBonds.Length; k++)
					{
						Bond currentBond = allBonds[k];
						if(((currentBond.nodeA == currentNode) && (currentBond.nodeB == otherNode)) || ((currentBond.nodeA == otherNode) && (currentBond.nodeB == currentNode)))
							bondExists = true;
					}
					
					if(!bondExists)
					{
						GameObject newBondGameObject = Instantiate(prefabs.bond, Vector3.zero, Quaternion.identity) as GameObject;
						Bond newBond = newBondGameObject.GetComponent<Bond>();
						newBond.nodeA = currentNode;
						newBond.nodeB = otherNode;
					}
				}
			}
		}
		
    } // End of Awake().
	

	void Update()
	{
        allNodes = FindObjectsOfType(typeof(Node)) as Node[];
        allBonds = FindObjectsOfType(typeof(Bond)) as Bond[];
		
		if( Input.GetKey( KeyCode.F12 ))
			Application.LoadLevel( 0 );
		
		totalEnergy = 0;
		// Loop through all nodes...
		for( int i = 0; i < allNodes.Length; i++ )
		{
			Node currentNode = allNodes[i];
			totalEnergy += currentNode.calories;
			
			// Interaction with other nodes...
			for( int j = 0; j < allNodes.Length; j++ )
			{
				Node otherNode = allNodes[j];
				
				if( i != j )
				{
					Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
					float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
					
					// Repulsive force
					currentNode.rigidbody.AddForce( 1000 * ( -vectorToNode / Mathf.Pow( distToNode, 5 )));
					
				}
			}
		}
	} // End of Update().


	
	void OnGUI()
	{
		GUI.skin = readoutSkin;
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

		string readout = "";
		readout += "UCSD Experimental Game Lab 2013\n";
		readout += "total energy in sys: " + totalEnergy + "\n";
		readout += allNodes.Length + " nodes\n";
		readout += allBonds.Length + " bonds\n";
		readout += "\n";
		readout += (1.0f / Time.deltaTime).ToString("F1") + "fps\n";
		GUI.Label( new Rect( 0, 0, Screen.width, Screen.height ), readout );
		
		/*
		for( int i = 0; i < allNodes.Length; i++ )
		{
			Node currentNode = allNodes[i];
			
			// Show node information.
			Vector3 partScreenPos = Camera.main.WorldToScreenPoint( currentNode.transform.position );
			GUI.skin.label.alignment = TextAnchor.UpperLeft;
			GUI.skin.label.fontSize = 10;
			string nodeInfo = "";
			
			nodeInfo += currentNode.nodeName + "\n";
			nodeInfo += "  Sense\n";
			// Show readouts for both strong and weak forces.
			for( var j = 0; j < 2; j++ )
			{
				BindingForce currentForce;
				if( j == 0 )
				{
					currentForce = currentNode.sense.strong;
					nodeInfo += "    Strong Force\n";
				}
				else
				{
					currentForce = currentNode.sense.weak;
					nodeInfo += "    Weak Force\n";
				}
				
				nodeInfo += "      [chg] ";
				// Show charge bool as + or -
				if( currentForce.charge )
					nodeInfo += "+";
				else
					nodeInfo += "-";
				nodeInfo += "\n";
				
				nodeInfo += "      [mag] " + currentForce.magnitude + "\n";
				nodeInfo += "      [dir] " + currentForce.direction + "\n";
				nodeInfo += "      [spr] " + currentForce.spread + "\n";
				nodeInfo += "      [frq] " + currentForce.frequency + "\n";
			}
			
			nodeInfo += "  Control\n";
			nodeInfo += "  Actuate\n";
			nodeInfo += "    [sns] " + currentNode.actuate.sensitivity + "\n";
			nodeInfo += "    [atk] " + currentNode.actuate.attack + "\n";
			nodeInfo += "    [dcy] " + currentNode.actuate.decay + "\n";
			nodeInfo += "    [sus] " + currentNode.actuate.sustain + "\n";
			nodeInfo += "    [rel] " + currentNode.actuate.release + "\n";

						
			GUI.Label( new Rect( partScreenPos.x, ( Screen.height - partScreenPos.y ), 300, 300 ), nodeInfo );
		}
		*/
	} // End of OnGUI().
}
