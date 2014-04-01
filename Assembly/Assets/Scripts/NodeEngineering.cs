using UnityEngine;
using System.Collections;

public class NodeEngineering : MonoBehaviour {

    public static NodeEngineering Inst = null;

    public Texture2D senseVectorHandle = null;
    public Texture2D actuateVectorHandle = null;
    public Texture2D dotTex = null;
    public Texture2D dotEmptyTex = null;

    ActiveButton senseHandle = null;
    ActiveButton actuateHandle = null;

    public bool uiLockout = false;

    public Vector3 senseVec = Vector3.zero;
    public Vector3 actuateVec = Vector3.zero;

    void Start(){

        Inst = this;

        senseHandle = new ActiveButton(senseVectorHandle);
        actuateHandle = new ActiveButton(actuateVectorHandle);

    } /// End of Start().
	
	void OnGUI(){
	
        Node selectedNode = MainCameraControl.Inst.selectedNode;
        Assembly selectedAssembly = MainCameraControl.Inst.selectedAssembly;
        if(selectedNode){

            


            GUI.skin.label.alignment = TextAnchor.MiddleCenter;

            float handleSize = 500f;

            Vector3 selectedNodeScreenPos = Camera.main.WorldToScreenPoint(selectedNode.worldPosition);

            // Sense
            senseVec = (selectedNode.assembly.physicsObject.transform.rotation * selectedNode.nodeProperties.senseVector) * Vector3.forward * 2f;
            Vector3 senseVecEnd = selectedNode.worldPosition + senseVec;
            Vector3 senseVecEndScreenPos = Camera.main.WorldToScreenPoint(senseVecEnd);

            senseHandle.rect = MathUtilities.CenteredSquare(senseVecEndScreenPos.x, senseVecEndScreenPos.y, handleSize / Vector3.Distance(Camera.main.transform.position, senseVecEnd));

            if(senseHandle.held){
                selectedNode.nodeProperties.senseVector = Quaternion.Lerp(selectedNode.nodeProperties.senseVector, Quaternion.Inverse(selectedNode.assembly.physicsObject.transform.rotation) * Camera.main.transform.rotation * Quaternion.LookRotation((Input.mousePosition - selectedNodeScreenPos).normalized, Camera.main.transform.up), 5f * (Time.deltaTime / Time.timeScale));
            }

            // Actuate
            actuateVec = (selectedNode.assembly.physicsObject.transform.rotation * selectedNode.nodeProperties.actuateVector) * Vector3.forward * 2f;
            Vector3 actuateVecEnd = selectedNode.worldPosition + actuateVec;
            Vector3 actuateVecEndScreenPos = Camera.main.WorldToScreenPoint(actuateVecEnd);

            actuateHandle.rect = MathUtilities.CenteredSquare(actuateVecEndScreenPos.x, actuateVecEndScreenPos.y, handleSize / Vector3.Distance(Camera.main.transform.position, actuateVecEnd));

            if(actuateHandle.held)
                selectedNode.nodeProperties.actuateVector = Quaternion.Lerp(selectedNode.nodeProperties.actuateVector, Quaternion.Inverse(selectedNode.assembly.physicsObject.transform.rotation) * Camera.main.transform.rotation * Quaternion.LookRotation((Input.mousePosition - selectedNodeScreenPos).normalized, Camera.main.transform.up), 5f * (Time.deltaTime / Time.timeScale));

            // Vector indications
            int numDots = 6;
            float dotSize = 150f;
            for(int i = 0; i < numDots; i++){
                Vector3 dotPos = MainCameraControl.Inst.selectedNode.worldPosition + (((NodeEngineering.Inst.senseVec) / (numDots + 1)) * (i + 1));
                Vector3 dotScreenPos = Camera.main.WorldToScreenPoint(dotPos);

                float thisDotSize = ((dotSize / Vector3.Distance(Camera.main.transform.position, dotPos)) / numDots) * (i + 1);

                if(Vector3.Angle(Camera.main.transform.forward, NodeEngineering.Inst.senseVec) >= 90f)
                    GUI.Label(MathUtilities.CenteredSquare(dotScreenPos.x, dotScreenPos.y, thisDotSize), NodeEngineering.Inst.dotTex);
                else
                    GUI.Label(MathUtilities.CenteredSquare(dotScreenPos.x, dotScreenPos.y, thisDotSize), NodeEngineering.Inst.dotEmptyTex);
            }
            for(int i = 0; i < numDots; i++){
                Vector3 dotPos = MainCameraControl.Inst.selectedNode.worldPosition + (((NodeEngineering.Inst.actuateVec) / (numDots + 1)) * (i + 1));
                Vector3 dotScreenPos = Camera.main.WorldToScreenPoint(dotPos);

                float thisDotSize = ((dotSize / Vector3.Distance(Camera.main.transform.position, dotPos)) / numDots) * (i + 1);

                if(Vector3.Angle(Camera.main.transform.forward, NodeEngineering.Inst.actuateVec) >= 90f)
                    GUI.Label(MathUtilities.CenteredSquare(dotScreenPos.x, dotScreenPos.y, thisDotSize), NodeEngineering.Inst.dotTex);
                else
                    GUI.Label(MathUtilities.CenteredSquare(dotScreenPos.x, dotScreenPos.y, thisDotSize), NodeEngineering.Inst.dotEmptyTex);
            }

            // Buttons
            senseHandle.Draw();
            actuateHandle.Draw();

            uiLockout = senseHandle.hovered || senseHandle.held || actuateHandle.hovered || actuateHandle.held;
        }
        else if(selectedAssembly){
            // Rotate assembly manually.
            selectedAssembly.physicsObject.transform.rotation *= Quaternion.Inverse(Quaternion.AngleAxis(WesInput.editHorizontalThrottle * 90f * (Time.deltaTime / Time.timeScale), Quaternion.Inverse(selectedAssembly.physicsObject.transform.rotation) * Camera.main.transform.up));
            selectedAssembly.physicsObject.transform.rotation *= Quaternion.Inverse(Quaternion.AngleAxis(WesInput.editVerticalThrottle * 90f * (Time.deltaTime / Time.timeScale), Quaternion.Inverse(selectedAssembly.physicsObject.transform.rotation) * -Camera.main.transform.right));
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
