using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlNode : Node {

    public ControlNode(Node node) : base(node){
        Initialize();
    }
    public ControlNode(Node node, Assembly assem) : base(node, assem){
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
        if(signalLock)
            return;

        signalLock = true;
        for(int i = 0; i < neighbors.Count; i++){
            if(neighbors[i].GetType() == typeof(ActuateNode)){
                ((ActuateNode)neighbors[i]).Propel(inputQuat, sigStrength);
            }
            else if(neighbors[i].GetType() == typeof(ControlNode)){
                ((ControlNode)neighbors[i]).Process(inputQuat, sigStrength);
            }
        }
        signalLock = false;
    } // End of Process().

    // Returns true is this node could possibly send data to a muscle node.
    /*
    public bool LogicCheck(){

        // Do whatever

        signalLock = true;
        for(int i = 0; i < neighbors.Count; i++){
            if((neighbors[i].GetType() == typeof(ActuateNode)) || (neighbors[i].GetType() == typeof(ControlNode))){
                activeLogic = true;
                neighbors[i].activeLogic = true;
            }
        }
        signalLock = false;
        return activeLogic;
    } // End of logicCheck().
    */
} // End of ControlNode.