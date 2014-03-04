using UnityEngine;
using System.Collections;

public class NodeEngineering : MonoBehaviour {

    public static NodeEngineering Inst = null;

    public Texture2D senseVectorHandle = null;
    public Texture2D actuateVectorHandle = null;
    public Texture2D dotTex = null;

    ActiveButton senseHandle = null;
    ActiveButton actuateHandle = null;

    public bool uiLockout = false;

    public Vector3 senseVecEnd = Vector3.zero;
    public Vector3 actuateVecEnd = Vector3.zero;

    void Start(){

        Inst = this;

        senseHandle = new ActiveButton(senseVectorHandle);
        actuateHandle = new ActiveButton(actuateVectorHandle);

    } /// End of Start().
	
	void OnGUI(){
	
        Node selectedNode = MainCameraControl.Inst.selectedNode;
        if(selectedNode){

            float handleSize = 500f;

            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(selectedNode.worldPosition);

            // Sense
            senseVecEnd = selectedNode.worldPosition + (selectedNode.assembly.physicsObject.transform.rotation * selectedNode.nodeProperties.senseVector) * Vector3.forward * 2f;
            Vector3 senseVecEndScreenPos = Camera.main.WorldToScreenPoint(senseVecEnd);

            senseHandle.rect = MathUtilities.CenteredSquare(senseVecEndScreenPos.x, senseVecEndScreenPos.y, handleSize / Vector3.Distance(Camera.main.transform.position, senseVecEnd));

            if(senseHandle.held){
                selectedNode.nodeProperties.senseVector = Quaternion.Inverse(selectedNode.assembly.physicsObject.transform.rotation) * Camera.main.transform.rotation * Quaternion.LookRotation((Input.mousePosition - selectedNodeScreenPos).normalized, Camera.main.transform.up);
            }


            // Actuate
            actuateVecEnd = selectedNode.worldPosition + (selectedNode.assembly.physicsObject.transform.rotation * selectedNode.nodeProperties.actuateVector) * Vector3.forward * 2f;
            Vector3 actuateVecEndScreenPos = Camera.main.WorldToScreenPoint(actuateVecEnd);

            actuateHandle.rect = MathUtilities.CenteredSquare(actuateVecEndScreenPos.x, actuateVecEndScreenPos.y, handleSize / Vector3.Distance(Camera.main.transform.position, actuateVecEnd));

            if(actuateHandle.held)
                selectedNode.nodeProperties.actuateVector = Quaternion.Inverse(selectedNode.assembly.physicsObject.transform.rotation) * Camera.main.transform.rotation * Quaternion.LookRotation((Input.mousePosition - selectedNodeScreenPos).normalized, Camera.main.transform.up);

            // Buttons
            senseHandle.Draw();
            actuateHandle.Draw();

            uiLockout = senseHandle.hovered || actuateHandle.hovered;
        }
	} // End of Update().
} // End of NodeEngineering.


public class ActiveButton {

    public Rect rect = new Rect();
    public Texture2D tex = null;

    public bool hovered = false;
    public bool clicked = false;
    public bool held = false;

    public ActiveButton(Rect newRect, Texture2D newTex){
        rect = newRect;
        tex = newTex;
    }
    public ActiveButton(Texture2D newTex){
        tex = newTex;
    }// End of ActiveButton().


    public void Draw(){


        for(int i = 0; i < 5; i++){
            Vector3 dotPos = MainCameraControl.Inst.selectedNode.worldPosition + ((NodeEngineering.Inst.senseVecEnd) / i);
            Vector3 dotScreenPos = Camera.main.WorldToScreenPoint(dotPos);
            GUI.Label(MathUtilities.CenteredSquare(dotScreenPos.x, dotScreenPos.y, 10f), NodeEngineering.Inst.dotTex);
        }


        hovered = rect.Contains(new Vector3(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 0f));

        clicked = false;
        if(hovered && Input.GetMouseButtonDown(0)){
            clicked = true;
            held = true;
        }
        else if(!Input.GetMouseButton(0))
            held = false;

        if(held)
            GUI.color = Color.green;
        else if(hovered)
            GUI.color = Color.white;
        else
            GUI.color = new Color(1f, 1f, 1f, 0.5f);

        GUI.Label(rect, tex);

    } // End of Draw().

} // End of ActiveButton.
