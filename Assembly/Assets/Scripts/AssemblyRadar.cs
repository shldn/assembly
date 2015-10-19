using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssemblyRadar : MonoBehaviour {

	public static AssemblyRadar Inst;

	List<AssemblyRadarBlip> blips = new List<AssemblyRadarBlip>();

	AssemblyRadarBlip selectedBlip = null;
	Texture2D selectionRing = null;

	int assemToBroadcast = 0;
	bool buttonHovered = true;

	float tapCheck = 0f;

	[HideInInspector] public Texture2D senseTexture, controlTexture, muscleTexture, boneTexture;


	class StoredAssem {
		public int id = -1;
		public string name = "unnamed";

		public StoredAssem(int id, string name){
			this.id = id;
			this.name = name;
		} // End of StoredAssem().
	} // End of StoredAssem.
	List<StoredAssem> storedAssems = new List<StoredAssem>();


	void Awake(){
		Inst = this;
	} // End of Awake().

	void Start(){
		selectionRing = Resources.Load<Texture2D>("Textures/selection_circle");

		senseTexture = Resources.Load<Texture2D>("Textures/sense_pixel");
		controlTexture = Resources.Load<Texture2D>("Textures/control_pixel");
		muscleTexture = Resources.Load<Texture2D>("Textures/muscle_pixel");
		boneTexture = Resources.Load<Texture2D>("Textures/bone_pixel");
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
		if(Input.GetMouseButton(0))
			tapCheck += Time.deltaTime;
		else
			tapCheck = 0f;
		if(Input.GetMouseButtonUp(0) && (tapCheck < 0.25f) && !buttonHovered && (CaptureEditorManager.capturedObj == null)){

			bool blipFound = false;

			// Sort assemblies by distance from camera.
			List<AssemblyRadarBlip> sortedBlips = blips;
			for (int i = 0; i < blips.Count - 1; i ++ ){
				float sqrMag1 = (blips[i + 0].position - Camera.main.transform.position).sqrMagnitude;
				float sqrMag2 = (blips[i + 1].position - Camera.main.transform.position).sqrMagnitude;
				if(sqrMag2 < sqrMag1){
					AssemblyRadarBlip tempStore = blips[i];
					blips[i] = blips[i + 1];
					blips[i + 1] = tempStore;
					i = 0;
				}
			}

			// Go through and find the first blip that the cursor is close enough to.
			for(int i = 0; i < sortedBlips.Count; i++){
				Vector3 screenPos = Camera.main.WorldToScreenPoint(sortedBlips[i].position);
				if((screenPos.z > 0f) && Vector2.SqrMagnitude(screenPos - Input.mousePosition) < (50000f / screenPos.z)){
					selectedBlip = sortedBlips[i];
					blipFound = true;
					break;
				}
			}

			if((selectedBlip != null) && !blipFound){
				selectedBlip = null;
				CameraControl.Inst.targetRadius += 20f;
			}

		}
		
		buttonHovered = false;
	} // End of Update().


	void LateUpdate(){
		if((CaptureEditorManager.capturedObj == null) && selectedBlip != null){
			CameraControl.Inst.targetOrbitQ = Quaternion.LookRotation(selectedBlip.position, Camera.main.transform.up);
			CameraControl.Inst.targetRadius = selectedBlip.position.magnitude + 30f;

			//GLDebug.DrawLine(selectedBlip.position, Camera.main.ScreenToWorldPoint(new Vector3(Screen.width * 0.55f, Screen.height * 0.5f, 100f)));
		}

		// Clear dead captured assems
		for(int i = 0; i < storedAssems.Count; i++){
			bool found = false;
			// Find blip with this id.
			for(int j = 0; j < blips.Count; j++){
				if(blips[j].assemblyID == storedAssems[i].id){
					found = true;
					break;
				}
			}

			if(!found){
				storedAssems.RemoveAt(i);
				i--;
			}
		}
	} // End of LateUpdate().


	void OnGUI(){
		// Escape if we have a captured object.
		if(CaptureEditorManager.capturedObj != null)
			return;

		for(int i = 0; i < blips.Count; i++){
			AssemblyRadarBlip curBlip = blips[i];
			Vector3 blipScreenPos = Camera.main.WorldToScreenPoint(curBlip.position);

			/*
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.fontSize = 10;
			GUI.Label(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 500f), curBlip.assemblyID.ToString());
			*/

			if(curBlip == selectedBlip){
				GUI.DrawTexture(MathUtilities.CenteredSquare(blipScreenPos.x, blipScreenPos.y, 5000f / Vector3.Distance(Camera.main.transform.position, curBlip.position)), selectionRing);
			}
		}

		if(selectedBlip != null){
			GUI.skin.label.fontSize = Mathf.CeilToInt(Screen.height * 0.06f);
			GUI.skin.label.alignment = TextAnchor.MiddleLeft;

			Vector3 labelWorldPos = Camera.main.WorldToScreenPoint(selectedBlip.position + (Camera.main.transform.right * 8f));
			GUI.Label(MathUtilities.CenteredSquare(labelWorldPos.x + 500f, labelWorldPos.y, 1000f), selectedBlip.name);
		}

		if(AssemblyEditor.Inst){
			GUI.skin = AssemblyEditor.Inst.guiSkin;
			Rect captureButtonRect = MathUtilities.CenteredRect(Screen.width * 0.5f, Screen.height * 0.1f, Screen.width * 0.2f, Screen.height * 0.09f);
			if(captureButtonRect.Contains(Input.mousePosition.ScreenFixY()))
				buttonHovered = true;
			if(selectedBlip != null){
				if(GUI.Button(captureButtonRect, "Capture")){
					networkView.RPC("CaptureRequest", RPCMode.Server, Network.player, selectedBlip.assemblyID);
					print("Requesting assembly capture...");
					storedAssems.Add(new StoredAssem(selectedBlip.assemblyID, selectedBlip.name));
				}
			}
		}

		// Show captured assemblies
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width * 0.3f, Screen.height));
			GUILayout.BeginVertical();
				for(int i = 0; i < storedAssems.Count; i++){
					if(GUILayout.Button(storedAssems[i].name)){
						// Find blip with this id.
						for(int j = 0; j < blips.Count; j++){
							if(blips[j].assemblyID == storedAssems[i].id){
								selectedBlip = blips[j];
								buttonHovered = true;
								break;
							}
						}
					}
				}
			GUILayout.EndVertical();
		GUILayout.EndArea();
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
		print("Assembly capture request received!");
		// Find the requesting player's PlayerSync.
		for(int i = 0; i < PlayerSync.all.Count; i++){
			if(PlayerSync.all[i].networkView.owner == requestingPlayer){
				print("Requesting playerSync found!");
				// Find the target assembly.
				for(int j = 0; j < Assembly.getAll.Count; j++){
					if(Assembly.getAll[j].Id == assemID){
						print("Targetted assembly found! Handling capture...");
						PlayerSync.all[i].HandleCapturedObject(Assembly.getAll[j]);
						break;
					}
				}
				break;
			}
		}
	} // End of CaptureRequest().


	public void ClearSelectedBlip(){
		print("clear selected blip");
		selectedBlip = null;
	} // End of ClearSelectedBlip().

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
			nodes[i].worldObject.renderer.enabled = CaptureEditorManager.capturedObj == null;
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
		Texture2D tex = null;
		switch(nodeType){
			case 1 :
				tex = AssemblyRadar.Inst.senseTexture;
				break;
			case 2 :
				tex = AssemblyRadar.Inst.controlTexture;
				break;
			case 3 :
				tex = AssemblyRadar.Inst.muscleTexture;
				break;
			default:
				tex = AssemblyRadar.Inst.boneTexture;
				break;
		}
		worldObject.renderer.material.mainTexture = tex;

	} // End of BlipNode().

} // End of BlipNode.