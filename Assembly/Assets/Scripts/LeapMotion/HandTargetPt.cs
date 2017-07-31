using UnityEngine;

public class HandTargetPt {

    public HandTargetPt(HandModel hand_, int finger_, int joint_, float lerpVal_) {
        hand = hand_;
        finger = finger_;
        joint = joint_;
        lerpVal = lerpVal_;
        lerpPalm = Random.Range(0f, 1f);
    }

    HandModel hand;
    int finger;
    int joint;
    float lerpVal;
    float lerpPalm;
    // Accessors
    public Vector3 Position {
        get {
            if(finger == 0 || finger == 4 || joint > 0) {
                // Return a point along the finger.
                return Vector3.Lerp(hand.fingers[finger].GetJointPosition(joint), hand.fingers[finger].GetJointPosition(joint + 1), lerpVal);
            }
            else {
                // Give a better distribution for the palm.
                int ltFinger = finger - 1;
                int rtFinger = finger + 1;
                Vector3 ltPt = Vector3.Lerp(hand.fingers[ltFinger].GetJointPosition(joint), hand.fingers[ltFinger].GetJointPosition(joint + 1), lerpVal);
                Vector3 rtPt = Vector3.Lerp(hand.fingers[rtFinger].GetJointPosition(joint), hand.fingers[rtFinger].GetJointPosition(joint + 1), lerpVal);
                return Vector3.Lerp(ltPt, rtPt, lerpPalm);
            }
        }
    }
}
