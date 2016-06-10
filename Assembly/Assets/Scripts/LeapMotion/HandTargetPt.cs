using UnityEngine;

public class HandTargetPt {

    public HandTargetPt(HandModel hand_, int finger_, int joint_, float lerpVal_) {
        hand = hand_;
        finger = finger_;
        joint = joint_;
        lerpVal = lerpVal_;
    }

    HandModel hand;
    int finger;
    int joint;
    float lerpVal;

    // Accessors
    public Vector3 Position { get { return Vector3.Lerp(hand.fingers[finger].GetJointPosition(joint), hand.fingers[finger].GetJointPosition(joint+1), lerpVal); } }
}
