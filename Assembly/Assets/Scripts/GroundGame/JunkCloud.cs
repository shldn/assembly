using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JunkCloud : MonoBehaviour {

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
        GameObject go = GameObject.Instantiate(rainPrefabs[NextPrefabIdx], RandomPointInCloud(), Random.rotation) as GameObject;
        if (createCreatures && creatureCount % skipCount == 0)
        {
            if (false)
            {
                SpringCreature creature = go.AddComponent<SpringCreature>();
                creature.numInitSprings = 5;
            }
            else
            {
                DelayedSpringCreature creature = go.AddComponent<DelayedSpringCreature>();
                creature.numSprings = 5;
            }

        }
        if (delayBetweenDrops > 0)
            Invoke("Rain", delayBetweenDrops);

        creatureCount++;
    }

    Vector3 RandomPointInCloud()
    {
        return transform.position + (new Vector3(Random.Range(cloudBounds.min.x, cloudBounds.max.x), Random.Range(cloudBounds.min.y, cloudBounds.max.y), Random.Range(cloudBounds.min.z, cloudBounds.max.z)));
    }


}
