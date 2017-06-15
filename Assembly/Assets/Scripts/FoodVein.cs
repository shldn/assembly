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
    int length = 150;
    int numBranches = 3;
    float spacing = 2f;
    float branchAngleMin = 15f;
    float branchAngleMax = 55f;
    float foodScale = 3f;

    float minFoodScale = 0.05f;
    float foodSizeDecayRate = 0.98f;
    float branchLengthDecayRate = 0.5f; // works well for noisy veins (not as good for trees)

    NoiseTest.OpenSimplexNoise noise3d;

    void Start () {
        noise3d = new NoiseTest.OpenSimplexNoise();

        bool makeNoisy = true;
        //BuildVein(makeNoisy);
        BuildSphericalVein(makeNoisy);
    }

    void BuildVein(bool noisy = false) {
        if (foodScale < minFoodScale)
            return;
        int branchesBuilt = 0;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        float startBranchSign = Random.value > 0.5f ? 1f : -1f;
        for (int i = 0; i < length; ++i) {
            Transform f = GetNewFood();
            f.position = noisy ? GetNextBest2DNoisePos(lastFoodPos, lastFoodDir.normalized) : (lastFoodPos + spacing * lastFoodDir.normalized);
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
                if(noisy)
                    vein.length = (int)(branchLengthDecayRate * length);
                else
                    vein.length = length - i; //better for more tree like behavior

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

    Transform GetNewFood() {
        Transform f = ViewerController.Inst.FoodPool.Get().transform;
        f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        f.gameObject.GetComponent<MeshRenderer>().enabled = false;
        return f;
    }

    // This is 2D noise and assumes y is up, need alternate implementation for 3D noise.
    Vector3 GetNextBest2DNoisePos(Vector3 lastPos, Vector3 dir) {
        Vector3 up = transform.up;
        // test possible points around the lastPos for the best choice according to a noise function
        List<Vector3> testPts = new List<Vector3>() { lastPos + spacing * dir,
                                                      lastPos + spacing * (Quaternion.AngleAxis(60f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(45f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(30f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(15f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-15f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-30f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-45f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-60f, up) * dir)
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

    Vector3 GetNextBest3DNoisePos(Vector3 lastPos, Vector3 dir, Vector3 up) {
        // test possible points around the lastPos for the best choice according to a noise function
        List<Vector3> testPts = new List<Vector3>() { lastPos + spacing * dir,
                                                      lastPos + spacing * (Quaternion.AngleAxis(60f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(45f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(30f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(15f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-15f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-30f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-45f, up) * dir),
                                                      lastPos + spacing * (Quaternion.AngleAxis(-60f, up) * dir)
        };
        double bestNoiseVal = -1f;
        Vector3 bestPos = testPts[0];
        for (int i = 0; i < testPts.Count; ++i) {
            double noise = noise3d.Evaluate(testPts[i].x, testPts[i].y, testPts[i].z);
            if (noise > bestNoiseVal) {
                bestNoiseVal = noise;
                bestPos = testPts[i];
            }
        }
        return bestPos;
    }

    void BuildSphericalVein(bool noisy) {
        if (foodScale < minFoodScale)
            return;
        int branchesBuilt = 0;
        float distFromCenter = transform.position.magnitude;
        Vector3 lastFoodPos = transform.position;
        Vector3 lastFoodDir = transform.forward;
        Vector3 lastFoodUp = transform.up;
        float dirSign = Vector3.Angle(transform.position.normalized, transform.right) > 90f ? 1f : -1f;
        float startBranchSign = Random.value > 0.5f ? 1f : -1f;
        for (int i = 0; i < length; ++i) {
            Transform f = GetNewFood();
            if(noisy)
                f.position = GetNextBest3DNoisePos(lastFoodPos, lastFoodDir.normalized, lastFoodPos.normalized);
            else
                f.position = lastFoodPos + spacing * lastFoodDir.normalized;
            Vector3 vToPt = f.position;
            f.position = distFromCenter * vToPt.normalized;
            Vector3 newForward = Vector3.Cross(lastFoodUp, dirSign * f.position.normalized);
            f.forward = newForward;
            f.localScale = foodScale * Vector3.one;
            f.parent = transform;
            if (i > 0 && (i < length - 2) && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? startBranchSign : -startBranchSign;
                float branchAngle = Random.Range(branchAngleMin, branchAngleMax);
                // up relative to the sphere
                Vector3 fUp = f.position.normalized;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, fUp) * newForward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = length - i; //(int)(branchLengthDecayRate * length);

                float branchPercent = 0.9f;
                vein.foodScale = branchPercent * foodScale;

                // adjust for the branch, by offsetting the parent branch angle a bit
                float parentAngle = 0.2f * branchAngle;
                lastFoodDir = Quaternion.AngleAxis(-sign * parentAngle, fUp) * newForward;
                lastFoodUp = Quaternion.AngleAxis(-sign * parentAngle, fUp) * lastFoodUp;
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
