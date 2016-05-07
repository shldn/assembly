using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;

public class HandCognoInside : MonoBehaviour {

	public static List<HandCognoInside> allHandMovements = new List<HandCognoInside>();

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

	List<FoodPellet> capturedPellets = new List<FoodPellet>();


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

		Vector3 palmPos = hand_model.palm.position + (-hand_model.palm.up * 2f);
		Debug.DrawRay(palmPos, Vector3.up, Color.cyan);
		Debug.DrawRay(palmPos, -Vector3.up, Color.cyan);
		Debug.DrawRay(palmPos, Vector3.right, Color.cyan);
		Debug.DrawRay(palmPos, -Vector3.right, Color.cyan);
		Debug.DrawRay(palmPos, Vector3.forward, Color.cyan);
		Debug.DrawRay(palmPos, -Vector3.forward, Color.cyan);


		// Find fingertip orientations
		Vector3 averageFingerDirection = Vector3.zero;
		for(int i = 0; i < 5; i++) {
			//Debug.DrawRay(hand_model.fingers[i].GetBoneCenter(3), hand_model.fingers[i].GetBoneDirection(3) * 10f, Color.cyan);
			averageFingerDirection += hand_model.fingers[i].GetBoneDirection(3);
		}
		averageFingerDirection /= 5f;
		Debug.DrawRay(palmPos, averageFingerDirection * 50f, Color.white);


		// If we're goin for a gesture consistently, we increase the time.
		if((targetGesture != Gesture.none) && (targetGesture == pendingGesture) && (gesture != targetGesture)){
			gestureTime += Time.deltaTime;

			// If we have held the gesture long enough, lock it in!
			if(gestureTime > 0.1f) {

                // On-gesture-change stuff here --------------------------------------------------------- //

                /*
				// Crush assembly
				if((gesture == Gesture.none) && (targetGesture == Gesture.fist)) {
					for(int i = 0; i < Assembly.getAll.Count; i++) {
						float sqrDistanceToPalm = (Assembly.getAll[i].Position - palmPos).sqrMagnitude;
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
					Assembly.RandomAssembly(palmPos, Random.rotation, 5);
					Instantiate(PersistentGameManager.Inst.pingBurstObj, palmPos, Quaternion.identity);
					AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("createAssembly"), hand_model.palm.position);
				}
                */

                gesture = targetGesture;
			}
		}

		// If we change gestures, reset the time.
		if(targetGesture != pendingGesture)
			gestureTime = 0f;

		pendingGesture = targetGesture;

        /*
		foodCooldown -= Time.deltaTime;
		if((gesture == Gesture.hangLoose) && (foodCooldown < 0f)) {
			FoodPellet newFood = new FoodPellet(palmPos);
			newFood.velocity = Random.insideUnitSphere * 10f;
			foodCooldown = Random.Range(0.1f, 0.5f);
			AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("createFood"), hand_model.palm.position);
		}
        */


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
		}


		// Energy collection/dispersion
		float distFromCam = Vector3.Distance(Camera.main.transform.position, leap_hand.PalmPosition.ToUnity());
		float attractDistribute = Mathf.InverseLerp(200f, 300f, distFromCam);

		if(attractDistribute < 0.8f) {
			for(int i = 0; i < FoodPellet.all.Count; i++) {
				Vector3 vectorToPellet = FoodPellet.all[i].WorldPosition - hand_model.GetPalmPosition();
                if (capturedPellets.Contains(FoodPellet.all[i])) {
                    FoodPellet.all[i].velocity *= 0.8f;
                    FoodPellet.all[i].velocity += -vectorToPellet * 2f;
                    FoodPellet.all[i].velocity += Random.insideUnitSphere * 20f;
                } else {
                    if (vectorToPellet.sqrMagnitude < 15f) {
                        capturedPellets.Add(FoodPellet.all[i]);
                    }
					else {
						FoodPellet.all[i].velocity += (-vectorToPellet / Mathf.Clamp(vectorToPellet.sqrMagnitude * 0.01f, 0.1f, Mathf.Infinity)) * 0.1f;
					}
				}
			}
		} else {
            for(int i = 0; i < FoodPellet.all.Count; i++)
                FoodPellet.all[i].activated = true;
            capturedPellets.Clear();
		}

		// Sync networked fingertip
		if(leap_hand.IsLeft) {
			for(int i = 10; i < 15; i++) {
				SmoothNetPosition.allFingertips[i].transform.position = hand_model.fingers[i - 10].GetTipPosition();
				SmoothNetPosition.allFingertips[i].render = true;
				SmoothNetPosition.allFingertips[i].GetComponent<Renderer>().material.SetColor("_TintColor", Color.green);
			}
			SmoothNetPosition.allFingertips[22].render = true;
			SmoothNetPosition.allFingertips[22].transform.position = hand_model.GetPalmPosition();
			SmoothNetPosition.allFingertips[22].transform.localScale = Vector3.one * (30f + (attractDistribute * 40f));
			SmoothNetPosition.allFingertips[22].GetComponent<Renderer>().material.SetColor("_TintColor", Color.Lerp(Color.green, Color.white, attractDistribute));
		} else {
			for(int i = 15; i < 20; i++) {
				SmoothNetPosition.allFingertips[i].transform.position = hand_model.fingers[i - 15].GetTipPosition();
				SmoothNetPosition.allFingertips[i].render = true;
				SmoothNetPosition.allFingertips[i].GetComponent<Renderer>().material.SetColor("_TintColor", Color.green);
			}
			SmoothNetPosition.allFingertips[23].render = true;
			SmoothNetPosition.allFingertips[23].transform.position = hand_model.GetPalmPosition();
			SmoothNetPosition.allFingertips[23].transform.localScale = Vector3.one * (30f + (attractDistribute * 40f));
			SmoothNetPosition.allFingertips[23].GetComponent<Renderer>().material.SetColor("_TintColor", Color.Lerp(Color.green, Color.white, attractDistribute));
		}

	} // End Update().

} // End HandCognoInside.cs.
 