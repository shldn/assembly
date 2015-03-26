/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bond {

	public Node nodeA;
    public Node nodeB;

    public GameObject gameObject = null;

    public static List<Bond> allBonds = new List<Bond>();


    // Constructors
    public Bond(Node a, Node b){
        nodeA = a;
        nodeB = b;
        InitializeGraphic();
        allBonds.Add(this);
    }


    void InitializeGraphic(){
        gameObject = GameObject.Instantiate(PrefabManager.Inst.bond) as GameObject;
    } // End of InitializeGraphic().

    public void UpdateGraphic(){
        Vector3 vectorAtoB = nodeB.worldPosition - nodeA.worldPosition;
        float bondLength = vectorAtoB.magnitude;

        // The following billboards the bond graphic with the camera.
        Quaternion bondDirection = Quaternion.LookRotation(nodeB.worldPosition - nodeA.worldPosition);
        gameObject.transform.rotation = bondDirection;
        gameObject.transform.position = nodeA.worldPosition + (vectorAtoB * 0.5f);
        gameObject.transform.rotation *= Quaternion.AngleAxis(90, Vector3.up);

        Vector3 camRelativePos = gameObject.transform.InverseTransformPoint(Camera.main.transform.position);
        float arcBillboardAngle = Mathf.Atan2(camRelativePos.z, camRelativePos.y) * Mathf.Rad2Deg;

        gameObject.transform.rotation *= Quaternion.AngleAxis(arcBillboardAngle + 90, Vector3.right);

        float bondWidth = 0.2f;
        Vector3 bondBillboardScale = new Vector3(bondLength, bondWidth, 1f);
        gameObject.transform.localScale = bondBillboardScale;

    } // End of UpdateGraphic().
} // End of Bond.
*/