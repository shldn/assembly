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
    
    List<Vector3> targets = new List<Vector3>();
    Vector3 originalScale = Vector3.one;

    BoxCollider boxCollider = null;
    int stage = 5000;
    bool invokeOnPositiveDelay = false; // helper for repeatDelay changes in editor

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        originalScale = boxCollider.size;
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
            Vector3 newSize = Vector3.MoveTowards(boxCollider.size, targets[stage], speed[stage] * Time.fixedDeltaTime);
            if (boxCollider.size == newSize)
                stage++;
            boxCollider.size = newSize;
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
            Vector3 target = originalScale + magnitude[i] * dirNormalized;
            if (targets.Count <= i)
                targets.Add(target);
            else
                targets[i] = target;
        }
    }

    int NumStages { get { return magnitude.Count; } }
     
}
