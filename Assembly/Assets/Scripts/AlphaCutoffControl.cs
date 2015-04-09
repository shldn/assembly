using UnityEngine;
using System.Collections;

public class AlphaCutoffControl : MonoBehaviour 
{
	
	public enum WeatherType{clearSky, smallClouds, largeClouds, overcast};	
	public WeatherType weatherType = WeatherType.clearSky;
	Shader alphaControls;
	public Renderer rend;
	private float cloudiness = 0.3f;
	private float cloudinessSmoothVel = 0f;	
	
	void Start () 
	{
		//Set the renderer to a variable for ease of typing		
        rend = GetComponent<Renderer>();
		
		//Grab the Alpha Cutoff Value from the shader		
        rend.material.shader = Shader.Find("Transparent/Cutout/Soft Edge Unlit");
    }
	
	//	On each update animate the alpha cutoff value to determin the cloudiness of the sky
    void Update() {		
		
		
		float targetCloudiness = 0f;
		
		if (weatherType == WeatherType.clearSky)
			targetCloudiness = 0.99f;
		
		if (weatherType == WeatherType.smallClouds)
			targetCloudiness = 0.6f;
		
		if (weatherType == WeatherType.largeClouds)
			targetCloudiness = 0.2f;
		
		if (weatherType == WeatherType.overcast)
			targetCloudiness = 0f;
		
		//cloudiness = Mathf.MoveTowards(cloudiness, targetCloudiness, Time.deltaTime * 0.2f);
		cloudiness = Mathf.SmoothDamp (cloudiness, targetCloudiness, ref cloudinessSmoothVel, 2f);
		cloudiness = Mathf.MoveTowards(cloudiness, targetCloudiness, Time.deltaTime * 0.05f);
		rend.material.SetFloat ("_Cutoff", cloudiness);
        
		
		
    }
}

