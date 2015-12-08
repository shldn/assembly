using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FanCreature : MonoBehaviour, CaptureObject {

    public int numFans = 5;
    public int repeatDelayMin = 1;
    public int repeatDelayMax = 6;

    public float forceStrengthMin = 1;
    public float forceStrengthMax = 100;
    public bool allowIntersectingFans = false;

    Matrix4x4 scaleAdjust;
    HashSet<int> attachedTriangles = new HashSet<int>();

    public Vector3 Position { get { return transform.position; } }

    void Awake()
    {
        PersistentGameManager.CaptureObjects.Add(this);
    }

    void Start()
    {
        for (int i = 0; i < numFans; ++i)
        {
            // one extra try if the add fails
            if (!AddFanImpl(false))
                AddFanImpl(false);
        }
    }

    public bool AddFan()
    {
        return AddFanImpl(true);
    }

    private bool AddFanImpl(bool incrementNumFans = true)
    {
        Vector3 normal = Vector3.up;
        scaleAdjust = Matrix4x4.Scale(gameObject.transform.localScale);
        int triangleIdx = -1;
        Vector3 attachPoint = transform.rotation * (scaleAdjust * AttachmentHelpers.GetRandomAttachPoint(gameObject.GetComponent<MeshFilter>().mesh, out normal, out triangleIdx, attachedTriangles));
        if (triangleIdx == -1)
            return false;
        attachedTriangles.Add(triangleIdx);
        GameObject fan = AttachmentHelpers.AttachObjectToObject(gameObject, "Utopia/Propeller", attachPoint, transform.rotation * normal, true);
        if( !allowIntersectingFans && IntersectsOtherFans(fan))
        {
            Destroy(fan);
            return false;
        }

        // set repeat delay
        FanController fcontroller = fan.GetComponent<FanController>();
        if( fcontroller == null )
            fcontroller = fan.AddComponent<FanController>();
        fcontroller.repeatDelay = Random.Range((float)repeatDelayMin, (float)repeatDelayMax);
        fcontroller.durationPercent = Random.Range(0.05f, 0.95f);

        // force strength
        fan.GetComponent<ConstantForce>().relativeForce = Random.Range(forceStrengthMin, forceStrengthMax) * (-Vector3.right);

        if (incrementNumFans)
            ++numFans;
        return true;
    }

    bool IntersectsOtherFans(GameObject go)
    {
        BoxCollider[] childColliders = GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < childColliders.Length; ++i)
        {
            if (gameObject != childColliders[i].gameObject && go != childColliders[i].gameObject && go.GetComponent<Collider>().bounds.Intersects(childColliders[i].bounds))
                return true;
        }
        return false;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        PersistentGameManager.CaptureObjects.Remove(this);
    }
	
}
