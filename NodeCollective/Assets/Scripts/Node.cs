using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
	public string nodeName = "Node";
    public int nodeKey;
    public int nodeLock;
	
	public Sense sense = new Sense();
	public Control control = new Control();
	public Actuate actuate = new Actuate();

	void Start()
	{
        int keyLockNum = 100;
        nodeKey = Random.Range(0, keyLockNum);
        nodeLock = Random.Range(0, keyLockNum);	
	}
	
	void Update()
	{
		// Update
	}
}
