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
    public Vector3 WorldOrigin { get; set; }
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
        if (Environment.Inst != null)
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

    public bool WithinBoundary(Vector3 worldPosition) {
        return !(Mathf.Sqrt(Mathf.Pow((worldPosition.x - WorldOrigin.x) / worldSize.x, 2f) + Mathf.Pow((worldPosition.y - WorldOrigin.y) / worldSize.y, 2f) + Mathf.Pow((worldPosition.z - WorldOrigin.z) / worldSize.z, 2f)) > 1f);
    }

    public float DistToBoundary(Vector3 position) {
        return Mathf.Abs(1f - Mathf.Sqrt(Mathf.Pow(position.x / worldSize.x, 2f) + Mathf.Pow(position.y / worldSize.y, 2f) + Mathf.Pow(position.z / worldSize.z, 2f)));
    }
}
