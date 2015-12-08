using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class BoxColliderScaler : MonoBehaviour {

    public Vector3 dir = Vector3.forward;
    public float magnitude = 1.0f;
    public float repeatDelay = 1.0f; // seconds

    public float speedOut = 2.0f;
    public float speedIn = 10.0f;

    public bool playAnimation = true;

    Vector3 scaleTarget = Vector3.one;
    Vector3 originalScale = Vector3.one;

    BoxCollider boxCollider = null;
    bool isAnimatingForward = false;
    bool isAnimatingBackward = false;
    bool invokeOnPositiveDelay = false; // helper for repeatDelay changes in editor

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        originalScale = boxCollider.size;
    }

    void Start()
    {
        scaleTarget = boxCollider.size;
        if( repeatDelay > 0 )
            Invoke("Scale", repeatDelay);
    }

	void FixedUpdate () {
        if (invokeOnPositiveDelay && repeatDelay > 0)
        {
            Invoke("Scale", repeatDelay);
            invokeOnPositiveDelay = false;
        }
        if (isAnimatingForward || isAnimatingBackward)
        {
            float speed = (isAnimatingForward) ? speedOut : speedIn;
            Vector3 newSize = Vector3.MoveTowards(boxCollider.size, scaleTarget, speed * Time.fixedDeltaTime);
            if (boxCollider.size == newSize)
            {
                if (isAnimatingForward)
                {
                    isAnimatingForward = false;
                    isAnimatingBackward = true;
                    scaleTarget = originalScale;
                }
                else
                    isAnimatingBackward = false;
            }
            boxCollider.size = newSize;
        }
	}

    void Scale()
    {
        scaleTarget = originalScale + magnitude * dir.normalized;
        isAnimatingForward = true;
        if (playAnimation && GetComponent<Animation>() != null)
            GetComponent<Animation>().Play();
        if (repeatDelay > 0)
            Invoke("Scale", repeatDelay);
        else
            invokeOnPositiveDelay = true;
    }
}
