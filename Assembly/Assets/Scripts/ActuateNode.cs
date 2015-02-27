using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActuateNode : Node {

    float totalSigStrength = 0f;
    float smoothedTailSize = 0f;
    float tailSizeVel = 0f;
    //TrailRenderer mainTrail = null;
    //TrailRenderer extendedTrail = null;

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

        localRotation = nodeProperties.actuateVector;

        if(!mainTrailObject){
            mainTrailObject = GameObject.Instantiate(PrefabManager.Inst.mainTrail, worldPosition, Quaternion.identity) as GameObject;
            mainTrailObject.transform.parent = gameObject.transform;
            //mainTrail = mainTrailObject.GetComponent<TrailRenderer>();
            //mainTrail.time = 4f * nodeProperties.muscleStrength;
        }

        if(!extendedTrailObject){
            extendedTrailObject = GameObject.Instantiate(PrefabManager.Inst.extendedTrail, worldPosition, Quaternion.identity) as GameObject;
            extendedTrailObject.transform.parent = gameObject.transform;
            //extendedTrail = extendedTrailObject.GetComponent<TrailRenderer>();
            //extendedTrail.time = 12f * nodeProperties.muscleStrength;
        }

        //mainTrailObject.transform.position = gameObject.transform.position;
        //extendedTrailObject.transform.position = gameObject.transform.position;
        
        smoothedTailSize = Mathf.SmoothDamp(smoothedTailSize, totalSigStrength * 0.3f, ref tailSizeVel, 0.1f);
        //mainTrail.startWidth = smoothedTailSize;

        mainTrailObject.transform.position = worldPosition;
        extendedTrailObject.transform.position = worldPosition;

        totalSigStrength = 0f;

	    Debug.DrawRay(worldPosition, worldAcuateRot * Vector3.forward * 3f, Color.red);

	} // End of Update().
    
    public void Propel(Quaternion inputQuat, float sigStrength){
        totalSigStrength += sigStrength;
        assembly.physicsObject.rigidbody.AddForceAtPosition(((assembly.WorldRotation * nodeProperties.actuateVector * inputQuat) * Vector3.forward) * 10f * sigStrength * nodeProperties.muscleStrength, worldPosition);
    } // End of Propel().

    public override void Destroy(){
        if(mainTrailObject){
            mainTrailObject.transform.parent = null;
            mainTrailObject.gameObject.AddComponent<DestroyAfterTime>().killTimer = mainTrailObject.GetComponent<TimedTrailRenderer>().lifeTime;
			extendedTrailObject.gameObject.name = "Doomed mainTrail";

        }
        if(extendedTrailObject){
            extendedTrailObject.transform.parent = null;
            extendedTrailObject.gameObject.AddComponent<DestroyAfterTime>().killTimer = extendedTrailObject.GetComponent<TimedTrailRenderer>().lifeTime;
			extendedTrailObject.gameObject.name = "Doomed extTrail";
        }
        base.Destroy();
    }
} // End of ActuateNode.