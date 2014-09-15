﻿using UnityEngine;
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

    public GameObject mainTrailObject = null;
    public GameObject extendedTrailObject = null;

    public GameObject actuateVectorBillboard = null;
    float actuateVecScale = 5f;

    public ActuateNode(Node node) : base(node){
        Initialize();
    }
    public ActuateNode(Node node, Assembly assem) : base(node, assem){
        Initialize();
    }public ActuateNode(IntVector3 localHex) : base(localHex){
        Initialize();
    }


	
    public override float GetBurnRate(){return BurnRate.actuate;}

    void Initialize(){
        baseColor = PrefabManager.Inst.actuateColor;
    } // End of Initialize().

	public override void Update(){
        base.Update();

        if(!mainTrailObject){
            mainTrailObject = GameObject.Instantiate(PrefabManager.Inst.mainTrail, worldPosition, Quaternion.identity) as GameObject;
            mainTrailObject.transform.parent = gameObject.transform;
            mainTrailObject.GetComponent<TrailRenderer>().time = 4f * nodeProperties.muscleStrength;
        }

        if(!extendedTrailObject){
            extendedTrailObject = GameObject.Instantiate(PrefabManager.Inst.extendedTrail, worldPosition, Quaternion.identity) as GameObject;
            extendedTrailObject.transform.parent = gameObject.transform;
            extendedTrailObject.GetComponent<TrailRenderer>().time = 12f * nodeProperties.muscleStrength;
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

        assembly.physicsObject.rigidbody.AddForceAtPosition(((nodeProperties.actuateVector * inputQuat) * Vector3.forward) * 10f * sigStrength * nodeProperties.muscleStrength, worldPosition);
    } // End of Propel().

    public override void Destroy(){
        if(mainTrailObject){
            mainTrailObject.transform.parent = null;
            mainTrailObject.AddComponent<DestroyAfterTime>().killTimer = mainTrailObject.GetComponent<TrailRenderer>().time;;
        }
        if(extendedTrailObject){
            extendedTrailObject.transform.parent = null;
            extendedTrailObject.AddComponent<DestroyAfterTime>().killTimer = extendedTrailObject.GetComponent<TrailRenderer>().time;;
        }
        base.Destroy();
    }
} // End of ActuateNode.