using UnityEngine;
using System.Collections.Generic;

public class TrailMesh : MonoBehaviour
{
    public GameObject objectToTrail = null;
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

    private LinkedList<Point> points = new LinkedList<Point>();
    private Vector3 lastPosition;
    private Vector3 lastCameraPosition1;
    private Vector3 lastCameraPosition2;
    private float lastRebuildTime = 0.00f;
    private bool lastFrameEmit = true;
    private static readonly Color whiteAlpha = new Color(1f, 1f, 1f, 0f);

    // Bounds calcultation
    Bounds meshBounds = new Bounds();
    bool rebuildBounds = false;
    bool lastFrameVisible = true;

    public bool render = true;

    // Triangle List helper
    private static List<int> triangles = new List<int>();
    private static Dictionary<int, int[]> triangleListCache = new Dictionary<int, int[]>(); // using this helps cut down on garbage collection that was causing stutters

    // Member data structures to avoid garbage collection with their destruction upon each rebuild - helps reduce stutters
    Vector3[] newVertices = null;
    Vector2[] newUV = null;
    Color[] newColors = null;

    public class Point
    {
        public float timeCreated = 0.00f;
        public Vector3 position;
        public bool lineBreak = false;
    }

    void Start() {
        Init();
    }

    void Init() {
        if(objectToTrail != null)
            lastPosition = objectToTrail.transform.position;
        gameObject.layer = LayerMask.NameToLayer("TransparentFX");
        transform.parent = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        gameObject.AddComponent(typeof(MeshFilter));
        gameObject.AddComponent(typeof(MeshRenderer));
        gameObject.GetComponent<Renderer>().material = material;

        newVertices = new Vector3[maxNumPts * 2];
        newUV = new Vector2[maxNumPts * 2];
        newColors = new Color[maxNumPts * 2];
    }

    public void SetTrailObject(GameObject obj) {
        objectToTrail = obj;
        lastRebuildTime = 0.00f;
        lastPosition = objectToTrail.transform.position;
        emitTime = 0.00f;

        transform.parent = null;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        meshBounds.center = objectToTrail.transform.position;
        meshBounds.size = Vector3.one;

        // Clear mesh if it exists.
        MeshFilter meshFilter = gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter;
        if(meshFilter != null) {
            Mesh mesh = meshFilter.mesh;
            mesh.Clear();
        }

        points.Clear();
        render = true;
    }


    void Update() {
        if (objectToTrail == null)
            return;
        GetComponent<Renderer>().enabled = render;

        if (emit && emitTime != 0) {
            emitTime -= Time.deltaTime;
            if (emitTime == 0) emitTime = -1;
            if (emitTime < 0) emit = false;
        }

        // early out if there is no camera
        if (!Camera.main) return;

        bool re = false;
        bool dirtyBounds = false;

        // if we have moved enough, create a new vertex and make sure we rebuild the mesh
        float theDistance = (lastPosition - objectToTrail.transform.position).magnitude;
        if (emit) {
            if (theDistance > minVertexDistance) {
                bool make = false;
                if (points.Count < 3) {
                    make = true;
                }
                else {
                    Vector3 l1 = points.Last.Previous.Value.position - points.Last.Previous.Previous.Value.position;
                    Vector3 l2 = points.Last.Value.position - points.Last.Previous.Value.position;
                    if (Vector3.Angle(l1, l2) > maxAngle || theDistance > maxVertexDistance) make = true;
                }

                if (make) {
                    Point p = new Point();
                    p.position = objectToTrail.transform.position;
                    p.timeCreated = Time.time;
                    points.AddLast(p);
                    lastPosition = objectToTrail.transform.position;
                    dirtyBounds = !meshBounds.Contains(p.position + Vector3.one) || !meshBounds.Contains(p.position - Vector3.one);
                    if (dirtyBounds) {
                        meshBounds.Encapsulate(p.position + Vector3.one);
                        meshBounds.Encapsulate(p.position - Vector3.one);
                    }
                }
                else {
                    points.Last.Value.position = objectToTrail.transform.position;
                    points.Last.Value.timeCreated = Time.time;
                }
            }
            else if (points.Count > 0) {
                points.Last.Value.position = objectToTrail.transform.position;
                points.Last.Value.timeCreated = Time.time;
            }
        }

        if (!emit && lastFrameEmit && points.Count > 0) points.Last.Value.lineBreak = true;
        lastFrameEmit = emit;

        // approximate if we should rebuild the mesh or not
        if (points.Count > 1) {
            Vector3 cur1 = Camera.main.WorldToScreenPoint(points.First.Value.position);
            lastCameraPosition1.z = 0;
            Vector3 cur2 = Camera.main.WorldToScreenPoint(points.Last.Value.position);
            lastCameraPosition2.z = 0;

            float distance = (lastCameraPosition1 - cur1).magnitude;
            distance += (lastCameraPosition2 - cur2).magnitude;

            if (distance > movePixelsForRebuild || Time.time - lastRebuildTime > maxRebuildTime) {
                re = true;
                lastCameraPosition1 = cur1;
                lastCameraPosition2 = cur2;
            }
            if (!lastFrameVisible && IsTrailVisible())
                re = true;
        }
        else {
            re = true;
        }


        if (re) {
            lastRebuildTime = Time.time;

            LinkedListNode<Point> it = points.First;
            while (it != null) {
                if (Time.time - it.Value.timeCreated > lifeTime || points.Count > maxNumPts) {
                    LinkedListNode<Point> toRemove = it;
                    it = it.Next;
                    points.Remove(toRemove);
                    rebuildBounds = true;
                }
                else {
                    // the points are in order, so once we reach a point with valid lifetime, all points added after it will be valid as well.
                    it = null;
                }
            }

            if (points.Count > 1 && IsTrailVisible()) {

                int i = 0;
                float curDistance = 0.00f;

                it = points.First;
                while (it != null) {
                    float time = (Time.time - it.Value.timeCreated) / lifeTime;

                    Color color = Color.Lerp(Color.white, whiteAlpha, time);
                    if (colors != null && colors.Length > 0) {
                        float colorTime = time * (colors.Length - 1);
                        float min = Mathf.Floor(colorTime);
                        float max = Mathf.Clamp(Mathf.Ceil(colorTime), 1, colors.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, colorTime);
                        if (min >= colors.Length) min = colors.Length - 1; if (min < 0) min = 0;
                        if (max >= colors.Length) max = colors.Length - 1; if (max < 0) max = 0;
                        color = Color.Lerp(colors[(int)min], colors[(int)max], lerp).SetAlpha(fade);
                    }

                    float size = (sizes != null && sizes.Length == 1) ? sizes[0] : 1f;
                    if (sizes != null && sizes.Length > 1) {
                        float sizeTime = time * (sizes.Length - 1);
                        float min = Mathf.Floor(sizeTime);
                        float max = Mathf.Clamp(Mathf.Ceil(sizeTime), 1, sizes.Length - 1);
                        float lerp = Mathf.InverseLerp(min, max, sizeTime);
                        if (min >= sizes.Length) min = sizes.Length - 1; if (min < 0) min = 0;
                        if (max >= sizes.Length) max = sizes.Length - 1; if (max < 0) max = 0;
                        size = Mathf.Lerp(sizes[(int)min], sizes[(int)max], lerp);
                    }

                    Vector3 lineDirection = (i == 0) ? it.Value.position - it.Next.Value.position : it.Previous.Value.position - it.Value.position;

                    Vector3 vectorToCamera = Camera.main.transform.position - it.Value.position;
                    Vector3 perpendicular = Vector3.Cross(lineDirection, vectorToCamera).normalized;

                    newVertices[i * 2] = it.Value.position + (perpendicular * (size * 0.5f));
                    newVertices[(i * 2) + 1] = it.Value.position + (-perpendicular * (size * 0.5f));

                    newColors[i * 2] = newColors[(i * 2) + 1] = color;

                    newUV[i * 2] = new Vector2(curDistance * uvLengthScale, 0);
                    newUV[(i * 2) + 1] = new Vector2(curDistance * uvLengthScale, 1);

                    if (i > 0 && !(it.Previous.Value.lineBreak)) {
                        if (higherQualityUVs) curDistance += (it.Value.position - (it.Previous.Value.position)).magnitude;
                        else curDistance += (it.Value.position - (it.Previous.Value.position)).sqrMagnitude;
                    }

                    i++;
                    it = it.Next;
                }

                Mesh mesh = (GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
                mesh.Clear();
                mesh.vertices = newVertices;
                mesh.colors = newColors;
                mesh.uv = newUV;
                mesh.triangles = GetTriangles(points.Count);
            }
            else if (rebuildBounds && points.Count > 0) {
                meshBounds = new Bounds(points.First.Value.position, Vector3.one);
                foreach (Point p in points) {
                    meshBounds.Encapsulate(p.position + Vector3.one);
                    meshBounds.Encapsulate(p.position - Vector3.one);
                }
                rebuildBounds = false;
            }
        }
        else if (dirtyBounds) {
            Mesh mesh = (gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
            mesh.bounds = meshBounds;
        }

        if (lastFrameVisible && !IsTrailVisible()) {
            // Clear the mesh, or it could be left floating in space while the latest points are still invisible to the camera.
            Mesh mesh = (gameObject.GetComponent(typeof(MeshFilter)) as MeshFilter).mesh;
            mesh.Clear();
        }

        lastFrameVisible = IsTrailVisible();
    }

    private bool IsTrailVisible() {
        return render && GeometryUtility.TestPlanesAABB(CameraHelper.GetMainCameraFrustumPlanes(), meshBounds);
    }

    private static int[] GetTriangles(int numPoints) {
        UpdateTriangleList(numPoints);
        if (!triangleListCache.ContainsKey(numPoints))
            triangleListCache.Add(numPoints, triangles.GetRange(0, (numPoints - 1) * 6).ToArray());
        return triangleListCache[numPoints];
    }

    // Maintains a static triangle list
    // since the triangle indices never change across instances, they can pull from the same single static list.
    // pass in the ptSize needed, to make sure the triangle list is long enough for this instance.
    private static void UpdateTriangleList(int numPoints) {
        int size = (numPoints - 1) * 6;
        while (size > triangles.Count) {
            int i = (triangles.Count / 6) + 1;
            triangles.Add((i * 2) - 2);
            triangles.Add((i * 2) - 1);
            triangles.Add(i * 2);

            triangles.Add((i * 2) + 1);
            triangles.Add(i * 2);
            triangles.Add((i * 2) - 1);
        }
    }

}