using UnityEngine;
using System.Collections;

public class Environment : MonoBehaviour {

	public static Environment Inst;
	public Renderer outerShell;
	Color defaultShellColor;

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

        // Fade outer shell if camera gets close.
        if (Camera.main) {
            float camDistToShell = WorldSizeController.Inst.DistToBoundary(Camera.main.transform.position);
            float fadeAmount = Mathf.Clamp01((camDistToShell - 0.2f) * 2f);
            //outerShell.material.color = defaultShellColor.SetAlpha(fadeAmount);
        }
    }
}
