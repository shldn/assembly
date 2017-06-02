using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodVein : MonoBehaviour {

    List<Transform> food = new List<Transform>();
    int length = 50;
    int numBranches = 2;
    float spacing = 2f;
    float branchAngle = 45f;
    float foodScale = 2f;

    float branchLengthDecayRate = 0.5f;
    float foodSizeDecayRate = 0.98f;

	void Start () {
        BuildVein();
        //BuildSphericalVein();
    }

    void BuildVein() {
        int branchesBuilt = 0;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = lastFoodPos + spacing * lastFoodDir.normalized;
            f.forward = lastFoodDir;
            f.localScale = foodScale * Vector3.one;
            f.parent = transform;
            // numBranches + 1 and ignore the end caps
            if(i > 0 && (i < length - 2) && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? 1f : -1f;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, transform.up) * lastFoodDir;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = (int)(branchLengthDecayRate * length);

                float branchPercent = 0.9f;
                vein.foodScale = branchPercent * foodScale;

                // size of branch should be a proportion of the parent branch and the parent should lose size proportionally
                //float branchPercent = 0.25f;
                //vein.foodScale = branchPercent * foodScale;
                //// main branch reduces its size be branch size
                //foodScale = (1f - branchPercent) * foodScale;

                // adjust for the branch, by offsetting the parent branch angle a bit
                float parentAngle = 0.2f * branchAngle;
                lastFoodDir = Quaternion.AngleAxis(-sign * parentAngle, transform.up) * lastFoodDir;
            }
            else
                lastFoodDir = f.forward;


            lastFoodPos = f.position;
            food.Add(f);
            foodScale = foodSizeDecayRate * foodScale;
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
            if (i > 0 && (i < length - 2) && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? 1f : -1f;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, f.position.normalized) * f.forward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = (int)(branchLengthDecayRate * length);


                lastFoodDir = Vector3.Cross(transform.up, -f.position.normalized);
            }
            else
                lastFoodDir = f.forward;
            lastFoodPos = f.position;
            
            food.Add(f);
        }
    }


}
