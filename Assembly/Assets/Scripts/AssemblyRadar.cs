using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblyRadar : MonoBehaviour {

	public static AssemblyRadar Inst;

	List<AssemblyRadarBlip> blips = new List<AssemblyRadarBlip>();

	AssemblyRadarBlip selectedBlip = null;
	Texture2D selectionRing = null;

	int assemToBroadcast = 0;


	void Awake(){
		Inst = this;
	} // End of Awake().

	void Start(){
		selectionRing = Resources.Load<Texture2D>("Textures/selection_circle");
	} // End of Start().
	

	void Update(){
		// Broadcast assembly position updates across network.
		if(Network.peerType == NetworkPeerType.Server){
			Assembly broadcastAssem = Assembly.getAll[assemToBroadcast];
			if(broadcastAssem)
				networkView.RPC("UpdatePos", RPCMode.Others, broadcastAssem.id, broadcastAssem.Position);

			assemToBroadcast++;
			if(assemToBroadcast >= Assembly.getAll.Count)
				assemToBroadcast = 0;
		}

		int blipsNum = blips.Count;
		for(int i = 0 ; i < blipsNum; i++){
			if(blips[i].cull){
				blips.RemoveAt(i);
				i--;
				blipsNum--;
			}else
				blips[i].Update();
		}

		// Blip selection
		if(Input.GetMouseButtonDown(0)){
			print("Searching...");

			bool blipFound = false;

			// Sort assemblies by distance from camera.
			List<AssemblyRadarBlip> sortedBlips = blips;
			for (int i = 0; i < Node.getAll.Count - 1; i ++ ){
				float sqrMag1 = (blips[i + 0].position - Camera.main.transform.position).sqrMagnitude;
				float sqrMag2 = (blips[i + 1].position - Camera.main.transform.position).sqrMagnitude;
				if(sqrMag2 < sqrMag1){
					Node tempStore = Node.getAll[i];
					Node.getAll[i] = Node.getAll[i + 1];
					Node.getAll[i + 1] = tempStore;
					i = 0;
				}
			}

			// Go through and find the first blip that the cursor is close enough to.
			for(int i = 0; i < sortedBlips.Count; i++){
				Vector3 screenPos = Camera.main.WorldToScreenPoint(sortedBlips[i].position);
				if(Vector2.SqrMagnitude(screenPos - Input.mousePosition) < (30000f / screenPos.z)){
					selectedBlip = sortedBlips[i];
					blipFound = true;
					print("Found!");
					break;
				}
			}

			if((selectedBlip != null) && !blipFound){
				print("No selection found.");
				selectedBlip = null;
				CameraControl.Inst.targetRadius += 20f;
			}

		}
		
	} // End of Update().


	void LateUpdate(){
		if(selectedBlip != null){
			CameraControl.Inst.targetOrbitQ = Quaternion.LookRotation(selectedBlip.position, Camera.main.transform.up);
			CameraControl.Inst.targetRadius = selectedBlip.position.magnitude + 30f;

			//GLDebug.DrawLine(selectedBlip.position, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.55f, Screen.height * 0.5f, 100f)));
		}
	} // End of LateUpdate().


	void OnGUI(){
		for(int i = 0; i < blips.Count; i++){
			AssemblyRadarBlip curBlip = blips[i];
			Vector3 blipScreenPos = Camera.main.WorldToScreenPoint(curBlip.position);
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.fontSize = 10;

//			GUI.Label(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 500f), curBlip.assemblyID.ToString());

			if(curBlip == selectedBlip){
				GUI.DrawTexture(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 5000f / Vector3.Distance(Camera.main.transform.position, curBlip.position)), selectionRing);
			}
		}

		if(selectedBlip != null){
			GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.04f);
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;
			GUI.Label(MathUtilities.CenteredSquare((Screen.width * 0.58f) + 500f, Screen.height * 0.5f, 1000f), selectedBlip.name);
		}

		if(AssemblyEditor.Inst){
			GUI.skin = AssemblyEditor.Inst.guiSkin;
			if(selectedBlip != null){
				if(GUI.Button(MathUtilities.CenteredRect(Screen.width * 0.5f, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.09f), "Capture")){
					networkView.RPC("CaptureRequest", RPCMode.Server, Network.player, selectedBlip.assemblyID);
				}
			}
		}
	} // End of OnGUI().


	[RPC]
	public void CreateBlip(string assemblyStr, Vector3 pos){
		Assembly newAssem = new Assembly(assemblyStr, Quaternion.identity, Vector3.zero);
		AssemblyRadarBlip newBlip = new AssemblyRadarBlip();
		blips.Add(newBlip);
		newBlip.position = pos;
		newBlip.targetPosition = pos;
		newBlip.assemblyID = newAssem.id;
		newBlip.name = newAssem.name;

		newBlip.nodes = new BlipNode[newAssem.NodeDict.Values.Count];
		Triplet nodePos = Triplet.zero;

		int nodeNum = 0;
		foreach(KeyValuePair<Triplet, Node> kvp in newAssem.NodeDict){
			newBlip.nodes[nodeNum] = new BlipNode(kvp.Value.neighbors.Count, kvp.Key);
			nodeNum++;
		}
		newBlip.ComputerCOM();
		newAssem.Destroy();
	} // End of CreateBlip().


	[RPC]
	public void RemoveBlip(int id){
		for(int i = 0; i < blips.Count; i++){
			if(blips[i].assemblyID == id){
				blips[i].Destroy();
				break;
			}
		}
	} // End of CreateBlip().

	
	[RPC]
	public void UpdatePos(int id, Vector3 pos){
		for(int i = 0; i < blips.Count; i++){
			if(blips[i].assemblyID == id){
				blips[i].targetPosition = pos;
				break;
			}
		}
	} // End of UpdatePos().


	[RPC]
	void CaptureRequest(NetworkPlayer requestingPlayer, int assemID){
		// Find the requesting player's PlayerSync.
		for(int i = 0; i < PlayerSync.all.Count; i++){
			if(PlayerSync.all[i].networkView.owner == requestingPlayer){
				// Find the target assembly.
				for(int j = 0; j < Assembly.getAll.Count; j++){
					if(Assembly.getAll[i].Id == assemID){
						PlayerSync.all[i].HandleCapturedObject(Assembly.getAll[i]);
						break;
					}
				}
				break;
			}
		}
	} // End of CaptureRequest().


} // End of AssemblyRadar.


public class AssemblyRadarBlip {

	public int assemblyID = 0;
	public string name = "";
	public Vector3 position = Vector3.zero;

	public Vector3 targetPosition = Vector3.zero;
	Vector3 positionVel = Vector3.zero;

	public Quaternion rotation = Quaternion.identity;
	public BlipNode[] nodes;

	Vector3 centerOfMass = Vector3.zero;

	public Vector3 rotationAxis;
	float rotSpeed = 0f;
	public bool cull = false;

	public AssemblyRadarBlip(){
		rotationAxis = Random.rotation * Vector3.forward;
		rotSpeed = Random.Range(5f, 30f);
	} // End of AssemblyRadarBlip().

	public void Update(){
		position = Vector3.SmoothDamp(position, targetPosition, ref positionVel, 4f);

		rotation *= Quaternion.AngleAxis(Time.deltaTime * rotSpeed, rotationAxis);
		for(int i = 0; i < nodes.Length; i++){
			nodes[i].worldObject.position = position + (rotation * nodes[i].localPos.ToVector3() - (rotation * centerOfMass));
		}
	} // End of Update().

	// Compute center of mass based on nodes.
	public void ComputerCOM(){
		centerOfMass = Vector3.zero;
		for(int i = 0; i < nodes.Length; i++)
			centerOfMass += nodes[i].localPos.ToVector3();
		centerOfMass /= nodes.Length;
	} // End of ComputerCOM().

	public void Destroy(){
		cull = true;
		for(int i = 0; i < nodes.Length; i++)
			MonoBehaviour.Destroy(nodes[i].worldObject.gameObject);
	} // End of Destroy().

} // End of AssemblyRadarBlip.


public class BlipNode {

	int nodeType = 1;
	public Triplet localPos;
	public Transform worldObject;

	public BlipNode(int nodeType, Triplet localPos){
		this.nodeType = nodeType;
		this.localPos = localPos;
		worldObject = MonoBehaviour.Instantiate(PrefabManager.Inst.nodeBlipPrefab) as Transform;

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