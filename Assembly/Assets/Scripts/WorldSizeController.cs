using UnityEngine;

public enum WorldAnimType
{
    capsule,
    sphere
}

public class WorldSizeController : MonoBehaviour {

    public static WorldSizeController Inst = null;

    Vector3 worldSize = new Vector3(150f, 150f, 150f);
    WorldAnimType worldAnim = WorldAnimType.capsule;
    float targetWorldSize = 150f;

    public Vector3 WorldSize { get { return worldSize; } set { worldSize = value; } }
    public float TargetWorldSize { set { targetWorldSize = value; } }
    public WorldAnimType WorldAnim { get { return worldAnim; } }

    void Awake () {
        Inst = this;
	}

    void Start() {
        if (PersistentGameManager.IsServer && !PersistentGameManager.EmbedViewer)
            ViewerData.Inst.messages.Add(new WorldSizeData(WorldSize));
    }
	
	void Update () {

        // World grows as food nodes are consumed.
        worldSize.z = Mathf.Lerp(worldSize.z, targetWorldSize, 0.1f * Time.deltaTime);

        // Once we get to a capsule, switch back to sphere.
        if (targetWorldSize >= 385f) {
            worldAnim = WorldAnimType.sphere;
        }

        // Once we get to sphere, switch back to capsule.
        if (targetWorldSize <= 150f) {
            worldAnim = WorldAnimType.capsule;
        }

        // Update Environment
        if (PersistentGameManager.EmbedViewer || PersistentGameManager.ViewerOnlyApp)
            Environment.Inst.WorldSize = worldSize;
    }

    void OnDestroy() {
        Inst = null;
    }

    // Moves the world animation forward in the animation cycle (called when a food node is depleted, etc.)
    public void AdvanceWorldTick() {
        if (worldAnim == WorldAnimType.capsule)
            targetWorldSize = Mathf.MoveTowards(targetWorldSize, 385f, 1f);
        else if (worldAnim == WorldAnimType.sphere)
            targetWorldSize = Mathf.MoveTowards(targetWorldSize, 150f, 1f);
        if (PersistentGameManager.IsServer && !PersistentGameManager.EmbedViewer)
            ViewerData.Inst.messages.Add(new TargetWorldSizeData(targetWorldSize));
    } // End of AdvanceWorldTick().
}
