using UnityEngine;
using System.Collections;

public class AlphaCutoffControl : MonoBehaviour 
{

	Shader alphaControls;
	public Renderer rend;
	
	// Grab the renderer
	void Start () 
	{
		//Set the renderer to a variable for ease of typing		
        rend = GetComponent<Renderer>();
		
		//Grab the Alpha Cutoff Value from the shader		
        rend.material.shader = Shader.Find("Transparent/Cutout/Soft Edge Unlit");
    }
	
	//	On each update animate the alpha cutoff value to determin the cloudiness of the sky
    void Update() {
		
		//Assign animating math to cloudiness variable as well as a variable to control how it pingpongs.
        var smoothTime = 0.3;
		float cloudiness = Mathf.PingPong(0.2f, 1.0f);
		
		//Set cloudiness variable to Shader's cutoff value
        rend.material.SetFloat("_Cutoff", cloudiness);
    }
}

