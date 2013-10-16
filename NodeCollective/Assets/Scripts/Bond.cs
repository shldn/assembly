using UnityEngine;
using System.Collections;
using Vectrosity;

public class Bond : MonoBehaviour {

    public Node nodeA;
    public Node nodeB;
    public VectorLine muscleLine;
    public VectorLine signalLine;
    //public VectorLine calorieLine;
	public Vector3[] muscleEndPoints = new Vector3[] {Vector3.zero, Vector3.zero};
	public Vector3[] signalEndPoints = new Vector3[] {Vector3.zero, Vector3.zero};
	//public Vector3[] calorieEndPoints = new Vector3[] {Vector3.zero, Vector3.zero};
	
	public Material muscleLineMaterial;
	public Material signalLineMaterial;
	
	float signalLink;
	

	void Awake(){
		muscleLine = new VectorLine("muscleLine", muscleEndPoints, muscleLineMaterial, 3.0f);
		signalLine = new VectorLine("signalLine", signalEndPoints, signalLineMaterial, 3.0f);
		//calorieLine = new VectorLine("calorieLine", calorieEndPoints, lineMaterial, 3.0f);
		
		signalLink = Random.Range(-1.0f, 1.0f);
	} // End of Awake().
	

	void Update(){
		// Destroy the bond if either of the nodes are missing.
		if(!nodeA || !nodeB){
			VectorLine.Destroy(ref muscleLine);
			VectorLine.Destroy(ref signalLine);
			//VectorLine.Destroy(ref calorieLine);
			Destroy(gameObject);
			return;
		}
		
        // Attractive force
		Vector3 vectorAtoB = nodeB.transform.position - nodeA.transform.position;
        Vector3 dirAtoB = vectorAtoB.normalized;
        float distToNode = vectorAtoB.magnitude;
		
        float attraction = 100.0f / (0.001f + Mathf.Pow(distToNode, 2));
        //float connectionStrength = Mathf.Clamp01(attractiveVector.magnitude);
		
        nodeA.rigidbody.AddForce(dirAtoB * attraction);
        nodeB.rigidbody.AddForce(-dirAtoB * attraction);
		
		// Determine calorie transfer.
		float calorieTransferStrength = attraction * 0.03f;
		float calorieTransfer = (nodeB.calories - nodeA.calories) * calorieTransferStrength;
		nodeA.caloriesDelta += calorieTransfer;
		nodeB.caloriesDelta += -calorieTransfer;
		
		// Render muscleLine ----------------------
		float muscleEndRadius = 0.4f;
        // Line direction shows direction of calorie flow.
		if(Mathf.Abs(nodeA.calories - nodeB.calories) >= 0.1)
			muscleLine.endCap = "CalorieCaps";
		else
			muscleLine.endCap = null;
        bool lineFlip = nodeB.calories > nodeA.calories;
		muscleEndPoints[lineFlip ? 1 : 0] = nodeA.transform.position + (dirAtoB * muscleEndRadius);
		muscleEndPoints[lineFlip ? 0 : 1] = nodeB.transform.position + (-dirAtoB * muscleEndRadius);
        // Line color shows strength of connection.
		Color muscleColor = Color.Lerp(Color.gray, Color.white, Mathf.Abs(calorieTransfer) * 3.0f);
		muscleColor.a = Mathf.Clamp01(attraction * 0.1f);
        muscleLine.SetColor(muscleColor);
        // Line width based on magnitude of signal flow.
        muscleLine.SetWidths(new float[] {6.0f + (0.1f * Mathf.Clamp(attraction + Mathf.Abs(calorieTransfer * 50.0f), 0.0f, 200.0f))});
        muscleLine.Draw3D();
		
		
		
		// Render signalLine ----------------------
		if(signalLink > 0)
			nodeA.signal += nodeB.signal * 1.0f * attraction * Mathf.Abs(signalLink * nodeB.signalDecay) * Time.deltaTime;
		else
			nodeB.signal += nodeA.signal * 1.0f * attraction * Mathf.Abs(signalLink * nodeA.signalDecay) * Time.deltaTime;
		
		signalLine.endCap = "CalorieCaps";
		float signalEndRadius = 0.4f;
		float signalOffset = 0.15f;
		Vector3 signalEndOffset = Quaternion.LookRotation(dirAtoB, Camera.main.transform.forward) * -Vector3.right * signalOffset;
		if(signalLink > 0){
			signalEndPoints[0] = nodeB.transform.position + signalEndOffset + (-dirAtoB * signalEndRadius);
			signalEndPoints[1] = nodeB.transform.position + signalEndOffset + (-dirAtoB * (signalEndRadius + ((distToNode - (2.0f * signalEndRadius)) * Mathf.Abs(signalLink * nodeB.signalDecay))));
		}
		else{
			signalEndPoints[0] = nodeA.transform.position + signalEndOffset + (dirAtoB * signalEndRadius);
			signalEndPoints[1] = nodeA.transform.position + signalEndOffset + (dirAtoB * (signalEndRadius + ((distToNode - (2.0f * signalEndRadius)) * Mathf.Abs(signalLink * nodeA.signalDecay))));
		}
        // Line color shows strength of connection.
		Color signalColor = Color.cyan;
		signalColor.a = attraction * 0.1f;
        signalLine.SetColor(signalColor);
        // Line width based on magnitude of signal flow.
        signalLine.SetWidths(new float[] {4.0f});
        signalLine.Draw3D();
		/*
		// Render calorieLine ----------------------
		float calorieEndRadius = 0.7f;
		float calorieOffset = 0.15f;
		Vector3 calorieEndOffset = Quaternion.LookRotation(dirAtoB, Camera.main.transform.forward) * Vector3.right * calorieOffset;
		calorieEndPoints[0] = nodeA.transform.position + calorieEndOffset + (dirAtoB * calorieEndRadius);
		calorieEndPoints[1] = nodeB.transform.position + calorieEndOffset + (-dirAtoB * calorieEndRadius);
        // Line color shows strength of connection.
		Color calorieColor = Color.Lerp(Color.green, Color.white, Mathf.Abs(nodeA.signal - nodeB.signal));
		calorieColor.a = attraction * 0.1f;
        calorieLine.SetColor(calorieColor);
        // Line width based on magnitude of signal flow.
        calorieLine.SetWidths(new float[] {1.0f});
        calorieLine.Draw3D();
        */
	} // End of Update().
} // End of Bond.
