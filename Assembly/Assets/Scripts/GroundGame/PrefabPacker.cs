using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabPacker : MonoBehaviour {

    public List<GameObject> prefabs = new List<GameObject>();
    public Mesh packShape = null;

    public bool alignWithFaces = false;
    public Vector3 scale = Vector3.one;

    int prefabIdx = 0;
    Vector3 lastScale = Vector3.one;

    int NextPrefabIdx { get { return prefabIdx++ % prefabs.Count; } }

	void Start () {
        Pack();
	}

    void Pack()
    {
        if (packShape == null || prefabs.Count == 0)
            return;

        Matrix4x4 scaleAdjust = Matrix4x4.Scale(scale);
        Vector3 posAdjust = transform.position;
        Vector3 normal = Vector3.up;
        for (int i = 0; i < packShape.triangles.Length / 3; ++i)
        {
            Vector3 pos = scaleAdjust * AttachmentHelpers.GetAttachPoint(packShape, i, out normal);
            GameObject go = GameObject.Instantiate(prefabs[NextPrefabIdx], pos + posAdjust, Quaternion.LookRotation(normal)) as GameObject;

            if( alignWithFaces )
                go.transform.Rotate(Vector3.right, 90.0f, Space.Self);

            if( go.rigidbody)
                go.rigidbody.isKinematic = true;
            go.transform.parent = this.transform;
        }
    }

    void UpdatePositions()
    {
        Matrix4x4 scaleAdjust = Matrix4x4.Scale(scale);
        Vector3 posAdjust = transform.position;
        Vector3 normal = Vector3.up;
        for (int i = 0; i < gameObject.transform.childCount; ++i)
            gameObject.transform.GetChild(i).transform.position = (Vector3)(scaleAdjust * AttachmentHelpers.GetAttachPoint(packShape, i, out normal)) + posAdjust;
    }

    void Drop()
    {
        for (int i = 0; i < gameObject.transform.childCount; ++i)
        {
            gameObject.transform.GetChild(i).rigidbody.isKinematic = false;
            gameObject.transform.GetChild(i).rigidbody.WakeUp();
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Alpha0))
            Drop();
        if (lastScale != scale)
            UpdatePositions();
        lastScale = scale;
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.white;
    //    Gizmos.DrawWireSphere(transform.position, 5);
    //}
}
