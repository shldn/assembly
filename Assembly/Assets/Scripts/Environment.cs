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
            float camDistToShell = Mathf.Abs(1f - Mathf.Sqrt(Mathf.Pow(Camera.main.transform.position.x / NodeController.Inst.worldSphereScale.x, 2f) + Mathf.Pow(Camera.main.transform.position.y / NodeController.Inst.worldSphereScale.y, 2f) + Mathf.Pow(Camera.main.transform.position.z / NodeController.Inst.worldSphereScale.z, 2f)));
            float fadeAmount = Mathf.Clamp01((camDistToShell - 0.2f) * 2f);
            outerShell.material.color = defaultShellColor.SetAlpha(fadeAmount);
        }
    }
}
