using UnityEngine;

public class TimedTrailRenderer : MonoBehaviour
{
 
   public bool emit = true;
   public float emitTime = 0.00f;
   public Material material;
   public float lifeTime = 1.00f;
 
   public Color[] colors;
   public float[] sizes;
   public float fade = 1f;
 
   public float uvLengthScale = 0.01f;
   public bool higherQualityUVs = true;
 
   public int movePixelsForRebuild = 6;
   public float maxRebuildTime = 0.1f;
   public float minVertexDistance = 0.10f; 
   public float maxVertexDistance = 10.00f;
   public float maxAngle = 3.00f;
   private int maxNumPts = 250;
 

    private bool render = true;
    public bool Render { get { return render; } set { render = value;  if(trailObj != null) { trailObj.GetComponent<TrailMesh>().render = value; } } }

    private GameObject trailObj = null;
 
   void Start()
   {
        trailObj = TrailMeshPool.Get();
        trailObj.transform.parent = transform;
        
        TrailMesh trailMesh = trailObj.GetComponent<TrailMesh>();
        trailMesh.objectToTrail = gameObject;
        trailMesh.SetTrailObject(gameObject);
        trailMesh.emit = emit;
        trailMesh.emitTime = emitTime;
        trailMesh.material = material;
        trailMesh.lifeTime = lifeTime;
        trailMesh.colors = colors;
        trailMesh.sizes = sizes;
        trailMesh.fade = fade;
        trailMesh.uvLengthScale = uvLengthScale;
        trailMesh.higherQualityUVs = higherQualityUVs;
        trailMesh.movePixelsForRebuild = movePixelsForRebuild;
        trailMesh.maxRebuildTime = maxRebuildTime;
        trailMesh.minVertexDistance = minVertexDistance;
        trailMesh.maxVertexDistance = maxVertexDistance;
        trailMesh.maxAngle = maxAngle;
   }
    

    void OnDestroy() {
        if(trailObj != null) {
            TrailMesh tmesh = trailObj.GetComponent<TrailMesh>();
            if (tmesh != null)
                tmesh.objectToTrail = null;
            TrailMeshPool.Release(trailObj);
            trailObj = null;
        }
    } // End of OnDestroy().

    
}