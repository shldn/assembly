using UnityEngine;
using System.Collections;

public class CroquetBall : MonoBehaviour {

    public static void Create(Vector3 pos, Quaternion rot)
    {
        GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("GroundGame/CroquetBall"));
        go.transform.position = pos;
        go.transform.rotation = rot;
    }

    public static IEnumerator CreateDelayedImpl(Vector3 pos, Quaternion rot, float waitSeconds)
    {
        yield return new WaitForSeconds(waitSeconds);
        Create(pos, rot);
    }

    void OnMouseUpAsButton()
    {
        Transform playerTransform = GroundGameManager.Inst.LocalPlayer.gameObject.transform;

        // put player in position
        float distanceFromBall = 1.0f;
        //GameManager.Inst.playerManager.SetLocalPlayerTransform(gameObject.transform.position + (distanceFromBall * -playerTransform.forward.normalized), playerTransform.rotation);
        //GameManager.Inst.LocalPlayer.gameObject.GetComponent<AnimatorHelper>().StartAnim("Swing", true);
    }
}
