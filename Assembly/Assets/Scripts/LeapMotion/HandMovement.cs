using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class HandMovement : MonoBehaviour {

	public static List<HandMovement> allHandMovements = new List<HandMovement>();

	public Transform anchorPrefab = null;
	public Transform anchor = null;
	public Transform rotationAnchor = null;

	public Vector3 movementVector = Vector3.zero;
	public Quaternion rotationOffset = Quaternion.identity;
	Quaternion initialCamRotOffset = Quaternion.identity;

	public enum Gesture {
		none,
		fist,
		thumbsUp,
		okay,
		gun,
		point,
		devilHorns,
		theBird,
		teaTime,
		hangLoose,
		peace
	} // End of Gesture.
	public Gesture gesture = Gesture.none;

	// We have to hold a gesture for a moment before it applies; acts as a sort of 'debounce' or 'lowpass filter'.
	Gesture targetGesture = Gesture.none; // Gesture we currently want to achieve.
	Gesture pendingGesture = Gesture.none; // set to targetGesture constantly; if it changes, we reset the timer.
	float gestureTime = 0f;


	void Awake() {
		allHandMovements.Add(this);
	} // End of Awake().


	void LateUpdate() {
		HandModel hand_model = GetComponent<HandModel>();
		Hand leap_hand = hand_model.GetLeapHand();

		if(leap_hand == null)
			return;

		if( !leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.fist;
		else if( leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.thumbsUp;
		else if( !leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && leap_hand.Fingers[2].IsExtended && leap_hand.Fingers[3].IsExtended && leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.okay;
		else if( leap_hand.Fingers[0].IsExtended && leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.gun;
		else if( !leap_hand.Fingers[0].IsExtended && leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.point;
		else if( !leap_hand.Fingers[0].IsExtended && leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.devilHorns;
		else if( !leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.theBird;
		else if( !leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.teaTime;
		else if( leap_hand.Fingers[0].IsExtended && !leap_hand.Fingers[1].IsExtended && !leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.hangLoose;
		else if( !leap_hand.Fingers[0].IsExtended && leap_hand.Fingers[1].IsExtended && leap_hand.Fingers[2].IsExtended && !leap_hand.Fingers[3].IsExtended && !leap_hand.Fingers[4].IsExtended)
			targetGesture = Gesture.peace;
		else{
			targetGesture = Gesture.none;
			gesture = Gesture.none;
			gestureTime = 0f;
		}

		// If we're goin for a gesture consistently, we increase the time.
		if((targetGesture != Gesture.none) && (targetGesture == pendingGesture)){
			gestureTime += Time.deltaTime;

			// If we have held the gesture long enough, lock it in!
			if(gestureTime > 1f)
				gesture = targetGesture;
		}

		// If we change gestures, reset the time.
		if(targetGesture != pendingGesture)
			gestureTime = 0f;

		pendingGesture = targetGesture;




		if((gesture == Gesture.gun) && !anchor){
			anchor = Instantiate(anchorPrefab, hand_model.palm.position, Camera.main.transform.rotation) as Transform;
			rotationAnchor = new GameObject("rotationAnchor").transform;
			rotationAnchor.rotation = Camera.main.transform.rotation;
			rotationAnchor.parent = hand_model.palm;

			anchor.transform.parent = Camera.main.transform;
		}

		if(anchor){
			anchor.GetComponent<LineRenderer>().SetPosition(0, anchor.position);
			anchor.GetComponent<LineRenderer>().SetPosition(1, hand_model.palm.position);

			movementVector = hand_model.palm.position - anchor.position;
			CameraControl.Inst.transform.position += movementVector * NodeController.physicsStep;

			rotationOffset = Quaternion.Inverse(Camera.main.transform.rotation) * rotationAnchor.rotation;
			print(rotationOffset.eulerAngles);

			Camera.main.transform.parent = null;
			CameraControl.Inst.transform.parent = Camera.main.transform;
			Camera.main.transform.rotation *= Quaternion.Lerp(Quaternion.identity, rotationOffset, 0.02f);
			CameraControl.Inst.transform.parent = null;
			Camera.main.transform.parent = CameraControl.Inst.transform;
		} else {
			movementVector = Vector3.zero;
		}

		if(anchor && !(gesture == Gesture.gun)){
			Destroy(anchor.gameObject);
			Destroy(rotationAnchor.gameObject);
		}

		// Debug readout of gesture
		//print((!leap_hand.Fingers[0].IsExtended? "." : "\\") + (!leap_hand.Fingers[1].IsExtended? "n" : "|") + (!leap_hand.Fingers[2].IsExtended? "n" : "|") + (!leap_hand.Fingers[3].IsExtended? "n" : "|") + (!leap_hand.Fingers[4].IsExtended? "n" : "|") + "  " + gesture.ToString());
	} // End of Update().


	void OnDestroy(){
		if(anchor){
			Destroy(anchor.gameObject);
			Destroy(rotationAnchor.gameObject);
		}
		allHandMovements.Remove(this);
	} // End of OnDestroy().
}
 