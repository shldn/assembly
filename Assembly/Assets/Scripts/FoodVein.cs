using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodVein : MonoBehaviour {

    List<Transform> food = new List<Transform>();
    int length = 150;
    int numBranches = 2;
    float spacing = 2f;
    float branchAngle = 30f;

    float decayRate = 0.5f;

	void Start () {
        //BuildVein();
        BuildSphericalVein();
    }

    void BuildVein() {
        int branchesBuilt = 0;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = transform.position + i * spacing * transform.forward.normalized;
            f.forward = transform.forward;
            f.parent = transform;
            if(i > 0 && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? 1f : -1f;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, transform.up) * f.forward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = (int)(decayRate * length);
            }

            food.Add(f);
        }
    }
    void BuildSphericalVein() {
        int branchesBuilt = 0;
        float distFromCenter = transform.position.magnitude;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = lastFoodPos + spacing * lastFoodDir.normalized;
            Vector3 vToPt = f.position;
            f.position = distFromCenter * vToPt.normalized;
            f.forward = Vector3.Cross(transform.up, -f.position.normalized);
            f.parent = transform;
            if (i > 0 && (i < length - 1) && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? 1f : -1f;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, f.position.normalized) * f.forward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = (int)(decayRate * length);


                lastFoodDir = Vector3.Cross(transform.up, -f.position.normalized);
            }
            else
                lastFoodDir = f.forward;
            lastFoodPos = f.position;
            
            food.Add(f);
        }
    }


}
