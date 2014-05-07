using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlNode : Node {

    public ControlNode(Node node) : base(node){
        Initialize();
    }
    public ControlNode(IntVector3 localHex) : base(localHex){
        Initialize();
    }


    public override float GetBurnRate(){return BurnRate.control;}

    void Initialize(){
        baseColor = PrefabManager.Inst.controlColor;
    } // End of Initialize().

	// Update is called once per frame
	public override void Update(){
        base.Update();
	
	} // End of Update().

    public void Process(Quaternion inputQuat, float sigStrength){
        signalLock = true;
        for(int i = 0; i < neighbors.Count; i++){
            if(neighbors[i].GetType() == typeof(ActuateNode)){
                ((ActuateNode)neighbors[i]).Propel(inputQuat, sigStrength);
            }
        }
        signalLock = false;
    } // End of Process().

    // Returns true is this node could possibly send data to a muscle node.
    public bool LogicCheck(){
        for(int i = 0; i < neighbors.Count; i++){
            if(neighbors[i].GetType() == typeof(ActuateNode)){
                activeLogic = true;
                neighbors[i].activeLogic = true;
            }
        }
        return activeLogic;
    } // End of logicCheck().
} // End of ControlNode.