using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	public static Environment Inst;
	public Renderer outerShell;
	Color defaultShellColor;

	public bool overrideScale = false;

    public bool Visible{
        set{
            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
                r.enabled = value;
        }
    }

    public Vector3 WorldSize { set { transform.localScale = value; } }

	void Awake(){
		Inst = this;
		defaultShellColor = outerShell.material.color;
	} // End of Awake().


	// Update is called once per frame
	void Update () {
		if(!overrideScale)
			transform.localScale = NodeController.Inst.worldSphereScale;

        // Fade outer shell if camera gets close.
        if (Camera.main) {
            //float camDistToShell = Mathf.Abs(1f - Mathf.Sqrt(Mathf.Pow(Camera.main.transform.position.x / NodeController.Inst.worldSphereScale.x, 2f) + Mathf.Pow(Camera.main.transform.position.y / NodeController.Inst.worldSphereScale.y, 2f) + Mathf.Pow(Camera.main.transform.position.z / NodeController.Inst.worldSphereScale.z, 2f))); // Merge -- replaced with line below
            float camDistToShell = WorldSizeController.Inst.DistToBoundary(Camera.main.transform.position);
            float fadeAmount = Mathf.Clamp01((camDistToShell - 0.2f) * 2f);
            outerShell.material.color = defaultShellColor.SetAlpha(fadeAmount);
        }
    }

    public bool IsInside(Vector3 pos) {
        return (Mathf.Sqrt(Mathf.Pow(pos.x / NodeController.Inst.worldSphereScale.x, 2f) + Mathf.Pow(pos.y / NodeController.Inst.worldSphereScale.y, 2f) + Mathf.Pow(pos.z / NodeController.Inst.worldSphereScale.z, 2f)) <= 1f);
    }
}
