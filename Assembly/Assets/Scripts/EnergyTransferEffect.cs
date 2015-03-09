using UnityEngine;
using System.Collections;

public class EnergyTransferEffect : MonoBehaviour {

	public SenseNode receivingNode = null;
    public FoodPellet sendingPellet = null;
    
    public bool disableCheck = false;
    public LineRenderer absorbLineRenderer = null;
    public LineRenderer senseLineRenderer = null;


    float phaseOffset = 0f;
    float alpha = 0f;
    float alphaVel = 0f;
    Vector3 sendPos = Vector3.zero;
    Vector3 receivePos = Vector3.zero;


    void Awake(){
        renderer.enabled = false;
        phaseOffset = Random.RandomRange(0f, 1f);
    } // End of Awake().


    void Update(){

        float alphaTarget = 0f;

        if(sendingPellet != null)
            sendPos = sendingPellet.worldPosition;

        if(receivingNode != null)
            receivePos = receivingNode.worldPosition;

        if(sendPos.Equals(Vector3.zero) || receivePos.Equals(Vector3.zero))
            renderer.enabled = false;
        else
            renderer.enabled = true;

        float pointResolution = 1f;

        Vector3 vectorToNode = receivePos - sendPos;

        int numPoints = Mathf.CeilToInt(vectorToNode.magnitude * pointResolution);

        if(!disableCheck)
            alphaTarget = 1f - Mathf.Sqrt(vectorToNode.magnitude / SenseNode.consumeRange);

        absorbLineRenderer.SetVertexCount(numPoints);
        for(int i = 0; i < numPoints;i++){
            
            Vector3 truePoint = sendingPellet.worldPosition + (vectorToNode * ((float)i / numPoints));

            Quaternion spiralQuat = Quaternion.LookRotation(vectorToNode);
            spiralQuat *= Quaternion.AngleAxis(90, Vector3.up);

            float spiralRadius = 0.5f;
            float spiralStrength = 30f;
            float spiralSpeed = 500f;

            //spiralRadius *= 1f - ((float)i / (float)numPoints);
            spiralQuat *= Quaternion.AngleAxis((i * spiralStrength) - ((Time.time * spiralSpeed) + (spiralSpeed * phaseOffset)), Vector3.right);
            absorbLineRenderer.SetPosition(i, truePoint + (spiralQuat * Vector3.forward * spiralRadius));
        }

        /*
        absorbLineRenderer.SetPosition(0, sendPos);
        absorbLineRenderer.SetPosition(1, receivePos);

        absorbLineRenderer.material.mainTextureOffset = new Vector2(-Time.time * 5f + (5f * phaseOffset), 0f);
        absorbLineRenderer.material.mainTextureScale = new Vector2(vectorToNode.magnitude * 0.2f, 1f);
        */

        if(disableCheck){
            if(alpha <= 0.01f){
                Destroy(gameObject);
            }
        }

        alpha = Mathf.SmoothDamp(alpha, alphaTarget, ref alphaVel, 0.2f);
        absorbLineRenderer.SetColors(new Color(1f, 1f, 1f, alpha), new Color(1f, 1f, 1f, alpha));

        disableCheck = true;
    } // End of Update().

} // End of EnergyTransferEffect.
