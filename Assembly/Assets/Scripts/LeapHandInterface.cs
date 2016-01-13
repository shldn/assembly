using UnityEngine;
using System.Collections;

public class LeapHandInterface : MonoBehaviour {

	class Hand {
		public Vector3 thumbTip;
		public Vector3 indexTip;
		public Vector3 middleTip;
		public Vector3 rignTip;
		public Vector3 pinkyTip;
	} // End of Hand.
	Hand rightHand = new Hand();
	Hand leftHand = new Hand();
	

	void Update () {
		print(HandController.Inst.GetAllPhysicsHands().Length);

		if(HandController.Inst.GetAllPhysicsHands().Length > 0){
			FingerModel thumb = HandController.Inst.GetAllPhysicsHands()[0].fingers[0];
			rightHand.thumbTip = thumb.GetBoneCenter(3) + (thumb.GetBoneRotation(3) * (Vector3.forward * thumb.GetBoneLength(3) * transform.root.localScale.x * 0.5f));
			print(thumb.GetBoneLength(2));
			Debug.DrawRay(rightHand.thumbTip, Vector3.up);
		}
	} // End of Update().


	void OnGUI() {
		Vector3 thumbWorldPos = Camera.main.WorldToScreenPoint(rightHand.thumbTip).ScreenFixY();
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		GUI.color = Color.white;
		GUI.skin.label.fontSize = 14;
		GUI.Label(MathUtilities.CenteredSquare(thumbWorldPos.x, thumbWorldPos.y, 200f), "thumb");
	} // End of OnGUI().

} // End of LeapHandInterface.
