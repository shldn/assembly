using UnityEngine;
using System.Collections;

public class MaterialHueShift : MonoBehaviour {


    public Material material = null;
    public float brightness = 1f;
    public float loopTime = 1f;

    float hueRunner = 0f;
    public bool pause = false;
	
	void Update(){

        if(!pause)
            hueRunner += Time.deltaTime;

        float currentRun = Mathf.Repeat((hueRunner / loopTime) * 6f, 6f);

        float r = 0f;
        float g = 0f;
        float b = 0f;

        int step = Mathf.FloorToInt(currentRun);
        float rampUp = currentRun % 1f;
        float rampDown = 1f - (currentRun % 1f);

        switch(step){
            case 0:
                r = 1f;
                g = rampUp;
                break;
            case 1:
                g = 1f;
                r = rampDown;
                break;
            case 2:
                g = 1f;
                b = rampUp;
                break;
            case 3:
                b = 1f;
                g = rampDown;
                break;
            case 4:
                b = 1f;
                r = rampUp;
                break;
            case 5:
                r = 1f;
                b = rampDown;
                break;
        }

        material.SetColor("_TintColor", new Color(r * brightness, g * brightness, b * brightness, 1f));

	} // End of Update().
} // End of MaterialHueShift.
