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
        SphericalCoordinates currentSpherePos = new SphericalCoordinates(transform,1,200, 0, 2*Mathf.PI, 0, 2*Mathf.PI);
        for (int i = 0; i < length; ++i) {
            Transform f = ViewerController.Inst.FoodPool.Get().transform;
            f.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            f.position = transform.position + i * spacing * transform.forward.normalized;
            SphericalCoordinates newSpherePos = new SphericalCoordinates(f.transform, 1, 200, 0, 2 * Mathf.PI, 0, 2 * Mathf.PI);
            newSpherePos.SetRadius(currentSpherePos.radius);
            f.position = newSpherePos.toCartesian;
            f.forward = Vector3.Cross(transform.up, -f.position.normalized);
            f.parent = transform;
            if (i > 0 && i % (length / (numBranches + 1)) == 0 && length > 1) {
                float sign = ++branchesBuilt % 2 == 1 ? 1f : -1f;
                f.forward = Quaternion.AngleAxis(sign * branchAngle, f.position.normalized) * f.forward;
                FoodVein vein = f.gameObject.AddComponent<FoodVein>();
                vein.length = (int)(decayRate * length);
            }

            food.Add(f);
        }
    }


}
