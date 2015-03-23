﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JunkCloud : MonoBehaviour {

    // Limit the number of Junk pieces in the scene.
    private static int maxJunkObjects = 150;
    private static LinkedList<GameObject> allJunkObjects = new LinkedList<GameObject>();
    public static void Clear() { allJunkObjects = null; }

    public List<GameObject> rainPrefabs = new List<GameObject>();

	public Bounds cloudBounds = new Bounds(new Vector3(0,5,0), new Vector3(10,3,10));
	public float delayBetweenDrops = 1.0f;
    public bool createCreatures = true;
    public int skipCount = 6;
    private int creatureCount = 0;

    int prefabIdx = 0;
    int NextPrefabIdx { get { return prefabIdx++ % rainPrefabs.Count; } }

	void Start () {
        Invoke("Rain", delayBetweenDrops);	
	}

    void Rain()
    {
        if( UtopiaGameManager.Inst.enableJunkClouds )
        {
            // Remove one first if max junk level has been reached
            if (allJunkObjects.Count >= maxJunkObjects)
                RemoveOldestJunkNotVisible();

            GameObject go = GameObject.Instantiate(rainPrefabs[NextPrefabIdx], RandomPointInCloud(), Random.rotation) as GameObject;
            if (createCreatures && creatureCount % skipCount == 0)
            {
                if (false)
                {
                    SpringCreature creature = go.AddComponent<SpringCreature>();
                    creature.numSprings = 5;
                }
                else
                {
                    if( Random.Range(0.0f, 1.0f) > 0.5f)
                    {
                        FanCreature creature = go.AddComponent<FanCreature>();
                        creature.numFans = 5;
                    }
                    else
                    {
                        DelayedSpringCreature creature = go.AddComponent<DelayedSpringCreature>();
                        creature.numSprings = 5;
                    }
                }

            }
            creatureCount++;
            allJunkObjects.AddLast(go);
        }

        if (delayBetweenDrops > 0)
            Invoke("Rain", delayBetweenDrops);

    }

    Vector3 RandomPointInCloud()
    {
        return transform.position + (new Vector3(Random.Range(cloudBounds.min.x, cloudBounds.max.x), Random.Range(cloudBounds.min.y, cloudBounds.max.y), Random.Range(cloudBounds.min.z, cloudBounds.max.z)));
    }

    void RemoveOldestJunkNotVisible()
    {
        Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        // Find the oldest node that is not in the camera's view
        LinkedListNode<GameObject> current = allJunkObjects.First;
        while (current != null)
        {
            if (!GeometryUtility.TestPlanesAABB(cameraPlanes, current.Value.renderer.bounds))
            {
                allJunkObjects.Remove(current);
                Destroy(current.Value);
                return;
            }
            current = current.Next;
        }
    }


}
