using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblyRadar : MonoBehaviour {

	public static AssemblyRadar Inst;

	List<AssemblyRadarBlip> blips = new List<AssemblyRadarBlip>();
	public Transform blipNodePrefab;

	AssemblyRadarBlip selectedBlip = null;
	public Texture2D selectionRing = null;


	void Awake(){
		Inst = this;
	} // End of Awake().

	void Start(){
	} // End of Start().
	

	void Update(){
		for(int i = 0 ; i < blips.Count; i++){
			blips[i].position += Random.rotation * Vector3.forward * Time.deltaTime;
			blips[i].Update();
		}
		
	} // End of Update().


	void OnGUI(){
		for(int i = 0; i < blips.Count; i++){
			AssemblyRadarBlip curBlip = blips[i];
			Vector3 blipScreenPos = Camera.main.WorldToScreenPoint(curBlip.position);
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.fontSize = 10;
			//GUI.Label(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 500f), i.ToString());

			if(curBlip == selectedBlip){
				GUI.DrawTexture(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 5000f / Vector3.Distance(Camera.main.transform.position, curBlip.position)), selectionRing);
			}
		}

	} // End of OnGUI().


	[RPC]
	public void CreateBlip(string assemblyStr){
		Assembly newAssem = new Assembly(assemblyStr, Quaternion.identity, Vector3.zero);
		AssemblyRadarBlip newBlip = new AssemblyRadarBlip();
		blips.Add(newBlip);
		newBlip.position = Random.rotation * Vector3.forward * Random.Range(0f, 50f);

		newBlip.nodes = new BlipNode[newAssem.NodeDict.Values.Count];
		Triplet nodePos = Triplet.zero;

		int nodeNum = 0;
		foreach(KeyValuePair<Triplet, Node> kvp in newAssem.NodeDict){
			newBlip.nodes[nodeNum] = new BlipNode(Random.Range(0, 5), kvp.Key);
			nodeNum++;
		}

	} // End of CreateBlip().


} // End of AssemblyRadar.


public class AssemblyRadarBlip {

	public int assemblyID = 0;
	public Vector3 position = Vector3.zero;
	public Quaternion rotation = Quaternion.identity;
	public BlipNode[] nodes;

	public Vector3 rotationAxis;

	public AssemblyRadarBlip(){
		rotationAxis = Random.rotation * Vector3.forward;
	} // End of AssemblyRadarBlip().

	public void Update(){
		rotation *= Quaternion.AngleAxis(Time.deltaTime * 30f, rotationAxis);
		for(int i = 0; i < nodes.Length; i++){
			nodes[i].worldObject.position = position + (rotation * nodes[i].localPos.ToVector3());
		}
	} // End of Update().

} // End of AssemblyRadarBlip.


public class BlipNode {

	int nodeType = 1;
	public Triplet localPos;
	public Transform worldObject;

	public BlipNode(int nodeType, Triplet localPos){
		this.nodeType = nodeType;
		this.localPos = localPos;
		worldObject = MonoBehaviour.Instantiate(AssemblyRadar.Inst.blipNodePrefab) as Transform;

		// Set worldObject color according to type.
		Color nodeColor = PrefabManager.Inst.stemColor;
		switch(nodeType){
			case 1 :
				nodeColor = PrefabManager.Inst.senseColor;
				break;
			case 2 :
				nodeColor = PrefabManager.Inst.actuateColor;
				break;
			case 3 :
				nodeColor = PrefabManager.Inst.controlColor;
				break;
		}
		worldObject.renderer.material.color = nodeColor;

	} // End of BlipNode().

} // End of BlipNode.