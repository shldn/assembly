using UnityEngine;
using System.Collections;

public class GuiKnob : MonoBehaviour {

    public Texture2D knobTex = null;
    public Texture2D handleTex = null;

    public string label = "Some Control";
    public float minValue = 0f;
    public float maxValue = 1f;
    public string stringFormat = "F2";
    public float initialValue = 0f;

    public float scale = 1f;
    [HideInInspector] public Vector2 pxlPos = Vector2.zero;

    float mapValue = 0f;
    float knobSize = 100f;

    float handleOffset = 40.5f;
    float handleScale = 18f;

    [HideInInspector] public bool clicked = false;

    public GuiKnob minLimitKnob = null;
    public GuiKnob maxLimitKnob = null;

    public float alpha = 1f;

    float yClickPos = 0f;
    bool clickPosCaught = false;

    public float Value{
        get{ return Mathf.Lerp(minValue, maxValue, mapValue); }
        set{ mapValue = Mathf.InverseLerp(minValue, maxValue, value); }
    }



    void Start(){
        Value = initialValue;
    }


    public void Draw(){

        Rect rect = new Rect(pxlPos.x - (scale * knobSize * 0.5f), pxlPos.y - (scale * knobSize * 0.5f), scale * knobSize, scale * knobSize);

        GUI.color = new Color(1f, 1f, 1f, 0.1f * alpha);
        if(clicked){{
            if(!clickPosCaught){
                yClickPos = Input.mousePosition.y;
                clickPosCaught = true;
            }

            mapValue += (Input.mousePosition.y - yClickPos) * 0.01f;
            yClickPos = Input.mousePosition.y;
            mapValue = Mathf.Clamp01(mapValue);

            if(minLimitKnob && (Value < minLimitKnob.Value))
                minLimitKnob.Value = Value;

            if(maxLimitKnob && (Value > maxLimitKnob.Value))
                maxLimitKnob.Value = Value;
        }
        } else if(Vector2.Distance(pxlPos, new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) <= (knobSize * scale * 0.5f)){
            GUI.color = new Color(1f, 1f, 1f, 0.15f * alpha);
            if(Input.GetMouseButtonDown(0))
                clicked = true;
        }


        if(!Input.GetMouseButton(0)){
            clicked = false;
            clickPosCaught = false;
        }

        GUI.DrawTexture(rect, knobTex);

        float zeroAngleOffset = 0.5f;
        Vector2 handlePos = pxlPos + new Vector2(Mathf.Cos((mapValue + zeroAngleOffset) * (Mathf.PI * 1.5f)) * handleOffset * scale, Mathf.Sin((mapValue + zeroAngleOffset) * (Mathf.PI * 1.5f)) * handleOffset * scale);
        Rect handleRect = new Rect(handlePos.x - (handleScale * scale * 0.5f), handlePos.y - (handleScale * scale * 0.5f), handleScale * scale, handleScale * scale);
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.DrawTexture(handleRect, handleTex);

        Rect labelRect = GUIHelper.Inst.CenteredRect(pxlPos.x, pxlPos.y, 500f, 500f);
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.fontSize = Mathf.RoundToInt(12f * scale);
        GUI.Label(labelRect, Value.ToString(stringFormat));
        labelRect.y -= knobSize * scale * 0.6f;
        GUI.Label(labelRect, label);


    } // End of OnGUI().
} // End of GuiKnob.