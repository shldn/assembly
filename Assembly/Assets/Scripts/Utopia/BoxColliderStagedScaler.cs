using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------
// BoxColliderStagedScaler
//
// Animates the scale of a box collider by given parameters
// Can setup as many stages to the animation as desired. 
// Example:
//   Spring goes in, then out, then back to start
//   magnitude will be negative, positive, then 1.
//----------------------------------------
[RequireComponent(typeof(BoxCollider))]
public class BoxColliderStagedScaler : MonoBehaviour
{

    public Vector3 dir = Vector3.forward;
    public float repeatDelay = 1.0f; // seconds

    public List<float> magnitude = new List<float>();
    public List<float> speed = new List<float>();
    public bool playAnimation = true;

    List<Vector3> offsetTargets = new List<Vector3>();
    Vector3 originalSize = Vector3.one;
    Vector3 originalCenter = Vector3.zero;
    Vector3 currentOffset = Vector3.zero;

    BoxCollider boxCollider = null;
    int stage = 5000;
    bool invokeOnPositiveDelay = false; // helper for repeatDelay changes in editor

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        originalSize = boxCollider.size;
        originalCenter = boxCollider.center;
    }

    void Start()
    {
        if (repeatDelay > 0)
            Invoke("Scale", repeatDelay);
    }

    void FixedUpdate()
    {
        if (invokeOnPositiveDelay && repeatDelay > 0)
        {
            Invoke("Scale", repeatDelay);
            invokeOnPositiveDelay = false;
        }
        if (stage < NumStages)
        {
            Vector3 newOffset = Vector3.MoveTowards(currentOffset, offsetTargets[stage], speed[stage] * Time.fixedDeltaTime);
            if (currentOffset == newOffset)
                stage++;
            currentOffset = newOffset;
            boxCollider.size = originalSize + newOffset;
            boxCollider.center = originalCenter + 0.5f * newOffset;
        }
    }

    void Scale()
    {
        SetupTargets();

        stage = 0;
        if (playAnimation && animation != null)
            animation.Play();
        if (repeatDelay > 0)
            Invoke("Scale", repeatDelay);
        else
            invokeOnPositiveDelay = true;
    }

    void SetupTargets()
    {
        Vector3 dirNormalized = dir.normalized;
        for (int i = 0; i < magnitude.Count; ++i)
        {
            Vector3 offsetTarget = magnitude[i] * dirNormalized;
            if (offsetTargets.Count <= i)
                offsetTargets.Add(offsetTarget);
            else
                offsetTargets[i] = offsetTarget;
        }
    }

    int NumStages { get { return magnitude.Count; } }

}
