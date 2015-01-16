using UnityEngine;
using System.Collections;

public class GroundGameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.N))
        {
            GameObject cube = GameObject.CreatePrimitive(Input.GetKey(KeyCode.RightShift) ? PrimitiveType.Sphere : PrimitiveType.Cube);
            cube.AddComponent<Rigidbody>();
            cube.AddComponent<SpringCreature>();
            cube.transform.position = new Vector3(0, 5.5F, 0);
        }
	}
}
