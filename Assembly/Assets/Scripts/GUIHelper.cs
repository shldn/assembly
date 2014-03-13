using UnityEngine;
using System.Collections;

public class GUIHelper : MonoBehaviour {

	public Texture2D white = null;
    public static GUIHelper Inst = null;

    public void Awake(){
        Inst = this;
    }

    public void DrawRect(Rect rect){
        GUI.DrawTexture(rect, white);
    } // End of DrawSolidRect().

    public Rect CenteredRect(float x, float y, float width, float height){
        return new Rect(x - (width * 0.5f), y - (height * 0.5f), width, height);
    } // End of centeredRect().

    public Rect CenteredFillBar(float x, float y, float width, float height, float fill){
        return new Rect(x - (width * 0.5f), y - (height * 0.5f), width * fill, height);
    } // End of centeredRect().

    public void DrawCenteredRect(float x, float y, float width, float height){
        GUI.DrawTexture(CenteredRect(x, y, width, height), white);
    } // End of DrawSolidRect().

    public void DrawCenteredFillBar(float x, float y, float width, float height, float fill){
        GUI.DrawTexture(CenteredFillBar(x, y, width, height, fill), white);
    } // End of DrawSolidRect().

} // End of GUIHelper.
