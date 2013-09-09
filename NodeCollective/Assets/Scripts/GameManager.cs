using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	
	public static Node[] allNodes;

	
	// Update is called once per frame
	void Update()
	{
		allNodes = FindObjectsOfType( typeof( Node )) as Node[];
	
		if( Input.GetKey( KeyCode.Escape ))
			Application.Quit();
		
		if( Input.GetKey( KeyCode.F12 ))
			Application.LoadLevel( 0 );
		
		// Loop through all nodes...
		for( int i = 0; i < allNodes.Length; i++ )
		{
			Node currentNode = allNodes[i];
			
			// Interaction with other nodes...
			for( int j = 0; j < allNodes.Length; j++ )
			{
				Node otherNode = allNodes[j];
				
				if( i != j )
				{
					Vector3 vectorToNode = ( otherNode.transform.position - currentNode.transform.position ).normalized;
					float distToNode = ( otherNode.transform.position - currentNode.transform.position ).magnitude;
					
					// Attractive force
					currentNode.rigidbody.AddForce( vectorToNode / distToNode );
					
					// Repulsive force
					currentNode.rigidbody.AddForce( 100 * ( -vectorToNode / Mathf.Pow( distToNode, 5 )));
					
				}
			}
		}
	}
	
	void OnGUI()
	{
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
	}
}
