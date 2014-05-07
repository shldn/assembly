using UnityEngine;
using System.Collections;

public class PrefabManager : MonoBehaviour {

    public static PrefabManager Inst = null;

    public GameObject node = null;
	public GameObject bond = null;
	public GameObject billboard = null;
	public GameObject foodPellet = null;
	public GameObject reproduceBurst = null;
    public GameObject propulsionEffect = null;

    public Material assemblySkin = null;

    public Color stemColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color senseColor = new Color(0.64f, 0.8f, 0.44f, 1f);
    public Color actuateColor = new Color(0.67f, 0.22f, 0.22f, 1f);
    public Color controlColor = new Color(0.35f, 0.59f, 0.84f, 1f);

    void Awake(){
        Inst = this;
    } // End of Awake().

} // End of PrefabManager.
