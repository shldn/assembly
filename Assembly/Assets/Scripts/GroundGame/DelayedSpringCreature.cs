using UnityEngine;
using System.Collections;

public class DelayedSpringCreature : MonoBehaviour {

    public float timeAfterBodyCreation = 5.0f;
    public float timeBetweenSprings = 1.0f;
    public int numSprings = 5;

    SpringCreature creature = null;


	// Use this for initialization
	void Start () {
        Invoke("CreateSpringCreature", timeAfterBodyCreation);
	}
	

    void CreateSpringCreature()
    {
        creature = gameObject.AddComponent<SpringCreature>();
        creature.numInitSprings = 1;
        Invoke("AddSpring", timeBetweenSprings);
    }

    void AddSpring()
    {
        creature.AddSpring();
        if (creature.SpringCount < numSprings)
            Invoke("AddSpring", timeBetweenSprings);
    }

}
