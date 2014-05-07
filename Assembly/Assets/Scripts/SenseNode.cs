﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SenseNode : Node {

    public Quaternion worldSenseRot {
        get{
            if(assembly && assembly.physicsObject)
                return assembly.physicsObject.transform.rotation * nodeProperties.senseVector;
            else
                return nodeProperties.senseVector;
        }
    }

    bool neighborsChecked = false;
    List<Node> logicNeighbors = new List<Node>();

    // Metabolism ------------------------------------------------------------------------ ||
    public static float consumeRange = 30.0f; //how far away can it consume?
    public static float detectRange  = 50.0f; //how far can it detect food
    public static float consumeRate = 10.0f; //rate asm consume food

    public GameObject senseFieldBillboard = null;
    float arcScale = 5f;

    public SenseNode(Node node) : base(node){
        Initialize();
    }
    public SenseNode(IntVector3 localHex) : base(localHex){
        Initialize();
    }

    

    void Initialize(){
        baseColor = PrefabManager.Inst.senseColor;
    } // End of Initialize().

    public override float GetBurnRate(){return BurnRate.sense;}
	
	// Update is called once per frame
	public override void Update(){
        base.Update();

        if(!neighborsChecked){
            neighbors = GetNeighbors();
            LogicCheck();
            neighborsChecked = true;
        }

        if(activeLogic && !senseFieldBillboard)
            senseFieldBillboard = GameObject.Instantiate(PrefabManager.Inst.billboard, worldPosition, Quaternion.identity) as GameObject;

        if(activeLogic){
            Debug.DrawRay(worldPosition, worldSenseRot * Vector3.forward * 3f, Color.green);

            senseFieldBillboard.transform.position = worldPosition + (worldSenseRot * Vector3.forward * arcScale);
            senseFieldBillboard.transform.localScale = Vector3.one * arcScale;

            // Billboard the arc with the main camera.
            senseFieldBillboard.transform.rotation = worldSenseRot;
            senseFieldBillboard.transform.position = worldPosition + (senseFieldBillboard.transform.rotation * (Vector3.forward * (0.5f * arcScale)));
            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);

            Vector3 camRelativePos = senseFieldBillboard.transform.InverseTransformPoint(Camera.main.transform.position);
            float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;
            senseFieldBillboard.transform.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);
        }
        
        Color tempColor = Color.red;
        //calling detect food on sense node
        for(int j = 0; j < FoodPellet.GetAll().Count; ++j){
            if(SenseDetectFoodRange(FoodPellet.GetAll()[j]) && this.DetectFood(FoodPellet.GetAll()[j])){

                //sense node consume food source
                //Consume( FoodPellet.GetAll()[j] );

                // Get vector to food:
                Quaternion quatToFood = RotToFood(FoodPellet.GetAll()[j]);
                float sigStrength = FoodSignalStrength(FoodPellet.GetAll()[j]);

                tempColor = Color.Lerp(tempColor, Color.cyan, sigStrength);

                signalLock = true;
                if(neighbors != null)
                    for(int i = 0; i < neighbors.Count; i++){
                        if(neighbors[i].GetType() == typeof(ControlNode)){

                            ((ControlNode)neighbors[i]).Process(quatToFood, sigStrength);

                            FoodPellet.GetAll()[j].SendParticleTo(FoodPellet.GetAll()[j].gameObject.transform.position - gameObject.transform.position);
                        }
                    }
                signalLock = false;
            }
        }

        if(senseFieldBillboard)
            senseFieldBillboard.renderer.material.SetColor("_TintColor", tempColor);
        
	} // End of Update().


    // Returns true is this sense node could possibly send data to a muscle node, and also informs those nodes
    //   that they are involved in a functioning logic net.
    public void LogicCheck(){
        for(int i = 0; i < neighbors.Count; i++){
            if(neighbors[i].GetType() == typeof(ControlNode)){
                if(((ControlNode)neighbors[i]).LogicCheck()){
                    activeLogic = true;
                    assembly.hasFunctioningNodes = true;
                }
            }
        }
    } // End of logicCheck().


    public override void Destroy(){
        if(senseFieldBillboard)
            GameObject.Destroy(senseFieldBillboard);
        base.Destroy();
    }

    // Food Pellets ---------------------------------------------------------------------------||
    // Does this sense node detect a certain food node?
    public bool DetectFood(FoodPellet food){

        Vector3 foodDir = food.worldPosition - this.worldPosition;
        float angle = Vector3.Angle(worldSenseRot * Vector3.forward, foodDir);

        if(angle < nodeProperties.fieldOfView) //detect through view angle
            if( foodDir.sqrMagnitude < (detectRange * detectRange) ) //detect within range
                return true;
        // Return false if no food pellets found.
        return false;
    } // End of DetectFood().

    // 'General' detect food... returns true if node detects any food pellet.
    public bool DetectFood(){
        for(int i = 0; i < FoodPellet.GetAll().Count; i++)
            if(DetectFood(FoodPellet.GetAll()[i]))
                return true;
        // Return false if no food pellets found.
        return false;
    } // End of DetectFood().

    // Same as general DetectFood() but references a list of all detected food pellets.
    public bool DetectFood(ref List<FoodPellet> allFood){
        bool sensedFood = false;
        for(int i = 0; i < FoodPellet.GetAll().Count; i++)
            if(DetectFood(FoodPellet.GetAll()[i])){
                allFood.Add(FoodPellet.GetAll()[i]);
                sensedFood = true;
            }
        // Return false if no food pellets found.
        return sensedFood;
    } // End of DetectFood().


    // Gets the rotation from the node to a certain foodPellet.
    public Quaternion RotToFood(FoodPellet food){
        
        // Get rotation to food
        Quaternion quatToFood = Quaternion.LookRotation(food.worldPosition - worldPosition, worldSenseRot * Vector3.up);
        Quaternion relativeQuatToFood = Quaternion.Inverse(worldSenseRot) * quatToFood;
        Debug.DrawRay(worldPosition, (worldSenseRot * relativeQuatToFood) * Vector3.forward * 3f);

        return relativeQuatToFood;
    } // End of RotToFood().

    public float FoodSignalStrength(FoodPellet food){
        return Mathf.Clamp01(10f / Vector3.Distance(worldPosition, food.worldPosition));
    } // End of FoodSignalStrength().

    
    //check if food is within consumption range
    public bool SenseDetectFoodRange(FoodPellet food){
        Vector3 foodDist = food.worldPosition - this.worldPosition;
        
        //if mag^2 < consume^2
        if(foodDist.sqrMagnitude < (Mathf.Pow(consumeRange, 2f))){
            return true;
        }
        return false;
    } // End of SenseDetectFoodRange().


    //consume food within range
    public void Consume(FoodPellet food){
        float realConsumeRate = (consumeRate* Time.deltaTime); 
        Vector3 foodDist = food.worldPosition - this.worldPosition;
        //consume rate square drop off
        realConsumeRate *= (1 - foodDist.sqrMagnitude / (consumeRange * consumeRange) );
        
        food.currentEnergy -= realConsumeRate;
        if( food.currentEnergy < 0){
            assembly.currentEnergy += ( food.currentEnergy + realConsumeRate);
            //destroy and create
            food.Destroy();
        }else {
            assembly.currentEnergy += realConsumeRate;
        }
    } // End of Consume().
} // End of SenseNode.