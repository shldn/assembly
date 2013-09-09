using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
	public string nodeName = "Node";
	
	public Sense sense = new Sense();
	public Control control = new Control();
	public Actuate actuate = new Actuate();

	void Start()
	{
		// Start		
	}
	
	void Update()
	{
		// Update
	}
}
