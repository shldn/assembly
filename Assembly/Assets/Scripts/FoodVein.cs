using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Branch length based on amount of trunk length left
// Branch thickness and continuing trunk thickness related
// Seems to be a bug in Spherical version with a branch direction down a couple iterations.
// Randomize branch angles
// Randomize branching positions along the trunk
public class FoodVein : MonoBehaviour {

    List<Transform> food = new List<Transform>();
    int length = 50;
    int numBranches = 3;
    float spacing = 2f;
    float branchAngleMin = 15f;
    float branchAngleMax = 55f;
    float foodScale = 2f;

    float foodSizeDecayRate = 0.98f;
    float branchLengthDecayRate = 0.5f; // works well for noisy veins (not as good for trees)

    void Start () {
        //BuildVein();
        BuildNoiseVein();
        //BuildSphericalVein();
    }

    void BuildVein() {
        int branchesBuilt = 0;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        float startBranchSign = Random.value > 0.5f ? 1f : -1f;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = lastFoodPos + spacing * lastFoodDir.normalized;
            f.forward = lastFoodDir;
            f.localScale = foodScale * Vector3.one;
            f.parent = transform;
            // numBranches + 1 and ignore the end caps
            // allow about 2 - 4 branches
            if(i > 0 && (i < length - 2) && length > 1 && i % (length / (numBranches + 1)) == 0) {  // && Random.Range(0, length) < 3) {
                float sign = ++branchesBuilt % 2 == 1 ? startBranchSign : -startBranchSign;
                float branchAngle = Random.Range(branchAngleMin, branchAngleMax);
                f.forward = Quaternion.AngleAxis(sign * branchAngle, transform.up) * lastFoodDir;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = length - i; //(int)(branchLengthDecayRate * length);

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

    Vector3 GetNextBestNoisePos(Vector3 lastPos, Vector3 dir) {
        // test possible points around the lastPos for the best choice according to a noise function
        List<Vector3> testPts = new List<Vector3>() { lastPos + spacing * dir,
                                                      lastPos + spacing * (Quaternion.AngleAxis(60f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(45f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(30f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(15f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-15f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-30f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-45f, transform.up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-60f, transform.up) * dir)
        };
        float bestNoiseVal = -1f;
        Vector3 bestPos = testPts[0];
        for(int i=0; i < testPts.Count; ++i) {
            float noise = Mathf.PerlinNoise(testPts[i].x, testPts[i].z);
            if(noise > bestNoiseVal) {
                bestNoiseVal = noise;
                bestPos = testPts[i];
            }
        }
        return bestPos;
    }

    void BuildNoiseVein() {
        int branchesBuilt = 0;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        float startBranchSign = Random.value > 0.5f ? 1f : -1f;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            
            f.position = GetNextBestNoisePos(lastFoodPos, lastFoodDir.normalized);
            f.forward = lastFoodDir;
            f.localScale = foodScale * Vector3.one;
            f.parent = transform;
            // numBranches + 1 and ignore the end caps
            // allow about 2 - 4 branches
            if (i > 0 && (i < length - 2) && length > 1 && i % (length / (numBranches + 1)) == 0) {  // && Random.Range(0, length) < 3) {
                float sign = ++branchesBuilt % 2 == 1 ? startBranchSign : -startBranchSign;
                float branchAngle = Random.Range(branchAngleMin, branchAngleMax);
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
        Vector3 lastFoodUp = transform.up;
        float startBranchSign = Random.value > 0.5f ? 1f : -1f;
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = lastFoodPos + spacing * lastFoodDir.normalized;
            Vector3 vToPt = f.position;
            f.position = distFromCenter * vToPt.normalized;
            Vector3 newForward = Vector3.Cross(lastFoodUp, -f.position.normalized);
            f.forward = newForward;
            f.localScale = foodScale * Vector3.one;
            f.parent = transform;
            if (i > 0 && (i < length - 2) && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? startBranchSign : -startBranchSign;
                float branchAngle = Random.Range(branchAngleMin, branchAngleMax);
                f.forward = Quaternion.AngleAxis(sign * branchAngle, f.position.normalized) * newForward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = length - i; //(int)(branchLengthDecayRate * length);

                float branchPercent = 0.9f;
                vein.foodScale = branchPercent * foodScale;

                // adjust for the branch, by offsetting the parent branch angle a bit
                float parentAngle = 0.2f * branchAngle;
                lastFoodDir = Quaternion.AngleAxis(-sign * parentAngle, f.position.normalized) * newForward;
                lastFoodUp = Quaternion.AngleAxis(-sign * parentAngle, f.position.normalized) * lastFoodUp;
            }
            else {
                lastFoodDir = f.forward;
                lastFoodUp = f.up;
            }
            lastFoodPos = f.position;
            
            food.Add(f);
            foodScale = foodSizeDecayRate * foodScale;
        }
    }
}
