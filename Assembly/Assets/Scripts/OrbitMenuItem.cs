using UnityEngine;
using System.Collections;

public class OrbitMenuItem : MonoBehaviour {

    public string title;
    public Font titleFont;

    public OrbitMenuItem pivotObject;
    public Vector2 pivot;
    public float orbitRadius;

    public Vector2 position;
    bool orbitCenter = false;
    public float rpm;
    public float angle;

    private float angleRunner;

    public Texture2D iconTexture;
    public float scale = 1;
    public float alsoScale = 1;
    private float currentScale = 1;

    private float fade;

    [HideInInspector] public bool held;
    [HideInInspector] public bool down;
    bool downCheck = false;


    void Start(){
	    scale *= alsoScale;
        if((pivotObject == null) && (pivot == Vector2.zero))
            orbitCenter = true;
    } // End of Start().


    void Update(){
	    if(pivotObject)
		    pivot = pivotObject.position;
	    else if(orbitCenter)
		    pivot = new Vector2(Screen.width / 2, Screen.height / 2);
	
	    angleRunner += 360 * (rpm / 60) * GameManager.Inst.deltaRealTime;
	    position.x = pivot.x + (Mathf.Cos((angleRunner + angle) * Mathf.Deg2Rad) * orbitRadius);
	    position.y = pivot.y + (Mathf.Sin((angleRunner + angle) * Mathf.Deg2Rad) * orbitRadius);


        if(down && !downCheck){
            down = false;
            downCheck = true;
        }

        if(held && !downCheck)
            down = true;

        if(!held)
            downCheck = false;
    } // End of Update().


    void OnGUI(){
	    float selectFadeTime = 0.15f;
	    held = false;
	
	    // Hover over button
	    if((Vector2.Distance(position, new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y)) <= (iconTexture.width * 0.5 * scale))){
		    fade = Mathf.MoveTowards(fade, 1, GameManager.Inst.deltaRealTime / selectFadeTime);
		
		    // Button pressed!
		    if(Input.GetMouseButton(0)){
			    held = true;
			    fade = 0;
		    }
	    }
	    else
		    fade = Mathf.MoveTowards(fade, 0, GameManager.Inst.deltaRealTime / selectFadeTime);

	
	    currentScale = Mathf.Lerp(scale, scale * 1.1f, fade);
	
	    GUI.color = Color.Lerp(new Color(1, 1, 1, 0.5f), Color.white, fade);
	    GUI.depth = -2000;
	    Rect iconRect = new Rect(position.x - (iconTexture.width * 0.5f * currentScale), position.y - (iconTexture.height * 0.5f * currentScale), iconTexture.width * currentScale, iconTexture.height * currentScale);
	    GUI.DrawTexture(iconRect, iconTexture, ScaleMode.ScaleToFit, true, 0);
	
	    GUI.depth = -2001;
	    GUI.skin.font = titleFont;
	    GUI.skin.label.alignment = TextAnchor.MiddleCenter;
	    GUI.color = Color.white;
	    GUI.Label(iconRect, title);
    } // End of OnGUI().
} // End of ODMenuItem.