using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActuateNode : Node {

    public Quaternion worldAcuateRot {
        get{
            if(assembly && assembly.physicsObject)
                return assembly.physicsObject.transform.rotation * nodeProperties.actuateVector;
            else
                return nodeProperties.actuateVector;
        }
    }

    public GameObject propulsionEffect = null;
    public TrailRenderer trail = null;

    public GameObject actuateVectorBillboard = null;
    float actuateVecScale = 5f;

    public ActuateNode(Node node) : base(node){
        Initialize();
    }
    public ActuateNode(IntVector3 localHex) : base(localHex){
        Initialize();
    }


	
    public override float GetBurnRate(){return BurnRate.actuate;}

    void Initialize(){
        baseColor = PrefabManager.Inst.actuateColor;
    } // End of Initialize().

	public override void Update(){
        base.Update();

        if(activeLogic && !propulsionEffect){
            propulsionEffect = GameObject.Instantiate(PrefabManager.Inst.propulsionEffect, worldPosition, Quaternion.identity) as GameObject;
            propulsionEffect.transform.parent = gameObject.transform;
            trail = propulsionEffect.GetComponent<TrailRenderer>();
        }

	    Debug.DrawRay(worldPosition, worldAcuateRot * Vector3.forward * 3f, Color.red);

	} // End of Update().
    
    public void Propel(Quaternion inputQuat, float sigStrength){
        /*
        if(trail){
            trail.startWidth = sigStrength;
            trail.endWidth = sigStrength;
        }
        */

        assembly.physicsObject.rigidbody.AddForceAtPosition(((nodeProperties.actuateVector * inputQuat) * Vector3.forward) * 100f * sigStrength, worldPosition);
    } // End of Propel().
} // End of ActuateNode.