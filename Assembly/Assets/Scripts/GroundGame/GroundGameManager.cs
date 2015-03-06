using UnityEngine;
using System.Collections;

public class GroundGameManager : MonoBehaviour {

    public float minSpringAngleOffset = 5.0f;
    public float maxAngleOffset = 60.0f;

    public Bounds randomHullBounds = new Bounds(new Vector3(0, 5, 0), new Vector3(3, 1, 2));
    public float randomHullSpringPercent = 0.1f;


    // Player variables
    private Player localPlayer = null;
    public Player LocalPlayer { get { return localPlayer; } set { localPlayer = value; } }

    public static GroundGameManager Inst = null;

    void Awake()
    {
        Inst = this;
        SpawnLocalPlayer(0, new Vector3(-228.1f, -11.38f, -334.8f), Quaternion.EulerAngles(0.0f, 90.0f, 0.0f));
    }

    void SpawnLocalPlayer(int id, Vector3 pos, Quaternion rot)
    {
        localPlayer = new Player(id, pos, rot);
    }

	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.N))
        {
            GameObject cube = GameObject.CreatePrimitive(Input.GetKey(KeyCode.RightShift) ? PrimitiveType.Sphere : PrimitiveType.Cube);
            cube.AddComponent<Rigidbody>();
            cube.AddComponent<SpringCreature>();
            cube.transform.position = new Vector3(8, 5.5F, 0);
            if (GroundGameManager.Inst.LocalPlayer)
                cube.transform.position = GroundGameManager.Inst.LocalPlayer.HeadPosition + 4.0f * GroundGameManager.Inst.LocalPlayer.gameObject.transform.forward;
        }
 
        if (Input.GetKeyUp(KeyCode.M))
        {
            GameObject hull = ConvexHull.GetRandomHullMesh(randomHullBounds);
            hull.AddComponent<MeshCollider>();
            hull.AddComponent<Rigidbody>();
            hull.renderer.material = new Material(Shader.Find("Diffuse"));
            hull.transform.position = new Vector3(8, 5.5F, 0);
            SpringCreature creature = hull.AddComponent<SpringCreature>();
            creature.numSprings = (int)(randomHullSpringPercent * (float)(hull.GetComponent<MeshFilter>().mesh.triangles.Length / 3));
        }

        if (Input.GetKeyUp(KeyCode.J))
        {
            GameObject junk = (GameObject)GameObject.Instantiate(Input.GetKey(KeyCode.RightShift) ? Resources.Load("GroundGame/Block") : Resources.Load("GroundGame/Palette"));
            if (GroundGameManager.Inst.LocalPlayer)
                junk.transform.position = GroundGameManager.Inst.LocalPlayer.HeadPosition + 4.0f * GroundGameManager.Inst.LocalPlayer.gameObject.transform.forward;
            SpringCreature creature = junk.AddComponent<SpringCreature>();
            creature.numSprings = (int)(randomHullSpringPercent * (float)(junk.GetComponent<MeshFilter>().mesh.triangles.Length / 3));
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            GameObject junk = (GameObject)GameObject.Instantiate(Resources.Load("GroundGame/Palette"));
            if (GroundGameManager.Inst.LocalPlayer)
                junk.transform.position = GroundGameManager.Inst.LocalPlayer.HeadPosition + 4.0f * GroundGameManager.Inst.LocalPlayer.gameObject.transform.forward;
            SpringCreature creature = junk.AddComponent<SpringCreature>();
            creature.numSprings = (int)(randomHullSpringPercent * (float)(junk.GetComponent<MeshFilter>().mesh.triangles.Length / 3));
        }
        if( Input.GetKeyUp(KeyCode.P))
        {
            localPlayer.SwitchModel(localPlayer.Model + 1);
        }
	}
}
