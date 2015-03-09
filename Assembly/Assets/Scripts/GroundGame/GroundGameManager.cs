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

        if (Input.GetKeyUp(KeyCode.C))
        {
            GameObject junk = (GameObject)GameObject.Instantiate(Input.GetKey(KeyCode.RightShift) ? Resources.Load("GroundGame/Fence") : Resources.Load("GroundGame/Palette"));
            if (GroundGameManager.Inst.LocalPlayer)
                junk.transform.position = GroundGameManager.Inst.LocalPlayer.HeadPosition + 4.0f * GroundGameManager.Inst.LocalPlayer.gameObject.transform.forward;
            Creature creature = junk.AddComponent<Creature>();
            creature.numAttachments = 5;
        }

        if (Input.GetKeyUp(KeyCode.N))
        {
            GameObject cube = GameObject.CreatePrimitive(Input.GetKey(KeyCode.RightShift) ? PrimitiveType.Sphere : PrimitiveType.Cube);
            cube.AddComponent<Rigidbody>();
            cube.AddComponent<SpringCreature>();
            cube.transform.position = new Vector3(8, 5.5F, 0);
            if (GroundGameManager.Inst.LocalPlayer)
                cube.transform.position = GroundGameManager.Inst.LocalPlayer.HeadPosition + 4.0f * GroundGameManager.Inst.LocalPlayer.gameObject.transform.forward;
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
        if( Input.GetKeyUp(KeyCode.B))
        {
            GroundGameManager.Inst.LocalPlayer.gameObject.GetComponent<AnimatorHelper>().StartAnim("Putdownball", true);
            PutDownBallCallback(GroundGameManager.Inst.LocalPlayer);
        }
        if (Input.GetKeyUp(KeyCode.M))
        {
            GroundGameManager.Inst.LocalPlayer.gameObject.GetComponent<AnimatorHelper>().StartAnim("Swing", true);
            float showTime = 4.0f;
            StartCoroutine(HideBall(GroundGameManager.Inst.LocalPlayer, 0));
            StartCoroutine(ShowBall(GroundGameManager.Inst.LocalPlayer, showTime));
        }
	}


    void PutDownBallCallback(Player p)
    {
        float distanceOffset = 0.2f;
        float timeOffset = 2.0f;
        Vector3 newPos = p.gameObject.transform.position + distanceOffset * p.gameObject.transform.forward.normalized;
        Quaternion newRot = p.gameObject.transform.rotation;
        StartCoroutine(CroquetBall.CreateDelayedImpl(newPos, newRot, timeOffset));
        StartCoroutine(HideBall(p, timeOffset));
        StartCoroutine(ShowBall(p, timeOffset + 2));
    }

    IEnumerator HideBall(Player p, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        CroquetBall ball = p.gameObject.GetComponentInChildren<CroquetBall>();
        ball.renderer.enabled = false;
    }
    IEnumerator ShowBall(Player p, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        CroquetBall ball = p.gameObject.GetComponentInChildren<CroquetBall>();
        if (ball != null)
            ball.renderer.enabled = true;
    }
}
