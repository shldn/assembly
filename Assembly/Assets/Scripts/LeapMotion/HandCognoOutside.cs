using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class HandCognoOutside : MonoBehaviour {

	public static List<HandCognoOutside> allHandMovements = new List<HandCognoOutside>();

	[HideInInspector] public Transform anchorPrefab = null;
	[HideInInspector] public Transform anchor = null;
	[HideInInspector] public Transform rotationAnchor = null;

	[HideInInspector] public Vector3 movementVector = Vector3.zero;
	[HideInInspector] public Quaternion rotationOffset = Quaternion.identity;
	Quaternion initialCamRotOffset = Quaternion.identity;

	float foodCooldown = 0f;

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
	[HideInInspector] public Gesture gesture = Gesture.none;

	// We have to hold a gesture for a moment before it applies; acts as a sort of 'debounce' or 'lowpass filter'.
	Gesture targetGesture = Gesture.none; // Gesture we currently want to achieve.
	Gesture pendingGesture = Gesture.none; // set to targetGesture constantly; if it changes, we reset the timer.
	float gestureTime = 0f;

	bool crushDist = false;

	[HideInInspector] public HandModel hand_model;
	[HideInInspector] public Hand leap_hand;


	void Awake() {
		allHandMovements.Add(this);
	} // End of Awake().

	void Start() {
	} // End of Start().


	void LateUpdate() {
		hand_model = GetComponent<HandModel>();
		leap_hand = hand_model.GetLeapHand();

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

		Vector3 crushPos = hand_model.palm.position + (-hand_model.palm.up * 2f);
		Debug.DrawRay(crushPos, Vector3.up, Color.red);
		Debug.DrawRay(crushPos, -Vector3.up, Color.red);
		Debug.DrawRay(crushPos, Vector3.right, Color.red);
		Debug.DrawRay(crushPos, -Vector3.right, Color.red);
		Debug.DrawRay(crushPos, Vector3.forward, Color.red);
		Debug.DrawRay(crushPos, -Vector3.forward, Color.red);

		// If we're goin for a gesture consistently, we increase the time.
		if((targetGesture != Gesture.none) && (targetGesture == pendingGesture) && (gesture != targetGesture)){
			gestureTime += Time.deltaTime;

			// If we have held the gesture long enough, lock it in!
			if(gestureTime > 0.1f) {

				// On-gesture-change stuff here --------------------------------------------------------- //

				// Crush assembly
				if((gesture == Gesture.none) && (targetGesture == Gesture.fist)) {
					for(int i = 0; i < Assembly.getAll.Count; i++) {
						float sqrDistanceToPalm = (Assembly.getAll[i].Position - crushPos).sqrMagnitude;
						if(sqrDistanceToPalm < 50f) {
							Instantiate(PersistentGameManager.Inst.pingBurstObj, Assembly.getAll[i].Position, Quaternion.identity);
							AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("crushAssembly"), hand_model.palm.position);
							Assembly.getAll[i].Destroy();
						}
					}
				}

				// Create assembly
				if((gesture == Gesture.none) && (targetGesture == Gesture.peace)) {
					print("New assembly!");
					Assembly.RandomAssembly(crushPos, Random.rotation, 5);
					Instantiate(PersistentGameManager.Inst.pingBurstObj, crushPos, Quaternion.identity);
					AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("createAssembly"), hand_model.palm.position);
				}

				gesture = targetGesture;
			}
		}

		// If we change gestures, reset the time.
		if(targetGesture != pendingGesture)
			gestureTime = 0f;

		pendingGesture = targetGesture;


		foodCooldown -= Time.deltaTime;
		if((gesture == Gesture.hangLoose) && (foodCooldown < 0f)) {
			FoodPellet newFood = new FoodPellet(crushPos);
			newFood.velocity = Random.insideUnitSphere * 10f;
			foodCooldown = Random.Range(0.1f, 0.5f);
			AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("createFood"), hand_model.palm.position);
		}

		if((gesture == Gesture.gun) && !anchor){
			anchor = Instantiate(anchorPrefab, hand_model.palm.position, Camera.main.transform.rotation) as Transform;
			rotationAnchor = new GameObject("rotationAnchor").transform;
			rotationAnchor.rotation = Camera.main.transform.rotation;
			rotationAnchor.parent = hand_model.palm;

			anchor.transform.parent = Camera.main.transform;

			AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("moveEngaged"), hand_model.palm.position);
		}

		if(anchor){
			anchor.GetComponent<LineRenderer>().SetPosition(0, anchor.position);
			anchor.GetComponent<LineRenderer>().SetPosition(1, hand_model.palm.position);

			movementVector = hand_model.palm.position - anchor.position;
			CameraControl.Inst.transform.position += movementVector * NodeController.physicsStep;

			rotationOffset = Quaternion.Inverse(Camera.main.transform.rotation) * rotationAnchor.rotation;

			Camera.main.transform.parent = null;
			CameraControl.Inst.transform.parent = Camera.main.transform;
			Camera.main.transform.rotation *= Quaternion.Lerp(Quaternion.identity, rotationOffset, 0.02f);
			CameraControl.Inst.transform.parent = null;
			Camera.main.transform.parent = CameraControl.Inst.transform;
		} else {
			movementVector = Vector3.zero;
		}

		if(anchor && (gesture != Gesture.gun)){
			Destroy(anchor.gameObject);
			Destroy(rotationAnchor.gameObject);

			AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("moveRelease"), hand_model.palm.position);
		}

		// Debug readout of gesture
		//print((!leap_hand.Fingers[0].IsExtended? "." : "\\") + (!leap_hand.Fingers[1].IsExtended? "n" : "|") + (!leap_hand.Fingers[2].IsExtended? "n" : "|") + (!leap_hand.Fingers[3].IsExtended? "n" : "|") + (!leap_hand.Fingers[4].IsExtended? "n" : "|") + "  " + gesture.ToString());


		// Two-hand gestures
		if((allHandMovements.Count == 2) && allHandMovements[0].hand_model && allHandMovements[1].hand_model) {
			float distBetweenPalms = Vector3.Distance(allHandMovements[0].hand_model.palm.position, allHandMovements[1].hand_model.palm.position);

			// Assembly crush
			if(distBetweenPalms > 8f)
				crushDist = false;
			else if(!crushDist) {
				crushDist = true;

				Vector3 palmAveragePos = Vector3.Lerp(allHandMovements[0].hand_model.palm.position, allHandMovements[1].hand_model.palm.position, 0.5f);

				for(int i = 0; i < Assembly.getAll.Count; i++) {
					float sqrDistanceToPalm = (Assembly.getAll[i].Position - palmAveragePos).sqrMagnitude;
					if(sqrDistanceToPalm < 50f) {
						Instantiate(PersistentGameManager.Inst.pingBurstObj, Assembly.getAll[i].Position, Quaternion.identity);
						AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("crushAssembly"), hand_model.palm.position);
						Assembly.getAll[i].Destroy();
					}
				}
			}

			// Two-handed steering
			if(allHandMovements[0].anchor && allHandMovements[1].anchor) {

				HandCognoOutside leftHand = null;
				HandCognoOutside rightHand = null;
				for(int i = 0; i < allHandMovements.Count; i++) {
					if(allHandMovements[i].leap_hand.IsLeft)
						leftHand = allHandMovements[i];
					else
						rightHand = allHandMovements[i];
				}

				if(leftHand && rightHand) {
					Vector3 forwardVector = rightHand.movementVector + leftHand.movementVector;
					Vector3 differenceVector = rightHand.movementVector - leftHand.movementVector;

					Camera.main.transform.parent = null;
					CameraControl.Inst.transform.parent = Camera.main.transform;
					Camera.main.transform.rotation *= Quaternion.AngleAxis(differenceVector.magnitude * 0.05f, Quaternion.Inverse(Camera.main.transform.rotation * Quaternion.Euler(-90f, 0f, 0f)) * differenceVector);
					CameraControl.Inst.transform.parent = null;
					Camera.main.transform.parent = CameraControl.Inst.transform;
                }
			}
		}


		// Sync networked fingertip
		if(leap_hand.IsLeft) {
			for(int i = 0; i < 5; i++) {
				SmoothNetPosition.allFingertips[i].transform.position = hand_model.fingers[i].GetTipPosition();
				SmoothNetPosition.allFingertips[i].render = true;
			}
			SmoothNetPosition.allFingertips[20].transform.position = hand_model.GetPalmPosition();
			SmoothNetPosition.allFingertips[20].render = true;
			SmoothNetPosition.allFingertips[21].transform.localScale = Vector3.one * 200f;
		} else {
			for(int i = 5; i < 10; i++) {
				SmoothNetPosition.allFingertips[i].transform.position = hand_model.fingers[i - 5].GetTipPosition();
				SmoothNetPosition.allFingertips[i].render = true;
			}
			SmoothNetPosition.allFingertips[21].transform.position = hand_model.GetPalmPosition();
			SmoothNetPosition.allFingertips[21].render = true;
			SmoothNetPosition.allFingertips[21].transform.localScale = Vector3.one * 200f;
		}


	} // End of Update().


	void OnDestroy(){
		if(anchor){
			Destroy(anchor.gameObject);
			Destroy(rotationAnchor.gameObject);
		}
		allHandMovements.Remove(this);
	} // End of OnDestroy().
}
 