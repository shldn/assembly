using UnityEngine;
using System.Collections;
using Vectrosity;

public class Node : MonoBehaviour
{
	public string nodeName = "Node";
    public int nodeKey;
    public int nodeLock;

    public Node[] connections = new Node[0];
    public VectorLine[] connectionLines = new VectorLine[0];
	
	public Sense sense = new Sense();
	public Control control = new Control();
	public Actuate actuate = new Actuate();

    public Material lineMaterial;

    public float signal;

	void Awake()
	{
        signal = Random.Range(0.5f, 1.0f);

        int keyLockNum = 50;
        nodeKey = Random.Range(0, keyLockNum);
        nodeLock = Random.Range(0, keyLockNum);	
	} // End of Awake().

    void Start()
    {
        // Set up connections with other nodes. (Debug-ish.)
        for (int j = 0; j < GameManager.allNodes.Length; j++)
        {
            Node otherNode = GameManager.allNodes[j];
            if ((otherNode != this) && (otherNode.nodeLock == nodeKey))
            {
                Node[] newConnections = new Node[connections.Length + 1];
                VectorLine[] newConnectionLines = new VectorLine[connections.Length + 1];
                for (int i = 0; i < connections.Length; i++)
                {
                    newConnections[i] = connections[i];
                    newConnectionLines[i] = connectionLines[i];
                }

                newConnections[connections.Length] = otherNode;

                Vector3[] linePoints = new Vector3[] { transform.position, otherNode.transform.position };

                newConnectionLines[connections.Length] = new VectorLine("ConnectionLine", linePoints, lineMaterial, 3.0f);
                newConnectionLines[connectionLines.Length].endCap = "ConnectionLineCaps";
                connections = newConnections;
                connectionLines = newConnectionLines;
            }
        }
    } // End of Start().
	
	void Update()
	{
        float totalSignalTransferred = 0;
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i])
            {
                VectorLine.Destroy(ref connectionLines[i]);
                continue;
            }

            Node otherNode = connections[i];
            VectorLine currentLine = connectionLines[i];

            // Attractive force
            Vector3 vectorToNode = (otherNode.transform.position - transform.position).normalized;
            float distToNode = (otherNode.transform.position - transform.position).magnitude;
            Vector3 attractiveVector = 100 * (vectorToNode / Mathf.Pow(distToNode, 2));
            float connectionStrength = Mathf.Clamp01(Mathf.Pow(attractiveVector.magnitude, 2));

            rigidbody.AddForce(attractiveVector);
            otherNode.rigidbody.AddForce(-attractiveVector);

            // Propogate signal
            float signalToTransfer = ((signal - otherNode.signal) / connections.Length) * attractiveVector.magnitude * 0.1f * Time.deltaTime;
            otherNode.signal += signalToTransfer;
            totalSignalTransferred += signalToTransfer;

            // Line offset from nodes.
            float lineOffsetDist = 0.9f;

            // Line direction shows direction of signal flow.
            bool lineFlip = signal > otherNode.signal;
            currentLine.points3[lineFlip ? 1 : 0] = otherNode.transform.position + (otherNode.transform.position - transform.position).normalized * -lineOffsetDist;
            currentLine.points3[lineFlip ? 0 : 1] = transform.position + (otherNode.transform.position - transform.position).normalized * lineOffsetDist;

            // Line color shows strength of connection.
            currentLine.SetColor(new Color(0.5f, 0.5f, 0.5f, connectionStrength));

            // Line width based on magnitude of signal flow.
            currentLine.SetWidths(new float[] {3.0f + ((5.0f * Mathf.Abs(signal - otherNode.signal)) * connectionStrength)});

            currentLine.Draw3D();
        }

        signal -= totalSignalTransferred;

        renderer.material.color = new Color(signal, signal, signal, 1f);

        //signal -= 0.01f * Time.deltaTime;

        if (signal <= 0.0f)
            DestroyNode();
	} // End of Update().

    void DestroyNode()
    {
        for (int i = 0; i < connections.Length; i++)
            VectorLine.Destroy(ref connectionLines[i]);

        Destroy(gameObject);
    } // End of DestroyNode().
}
