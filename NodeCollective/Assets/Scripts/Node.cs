using UnityEngine;
using System.Collections;

public class Node : MonoBehaviour
{
	public string nodeName = "Node";
	
	public Sense sense = new Sense();
	public Control control = new Control();
	public Actuate actuate = new Actuate();

    public float calories;
	public float caloriesDelta;
	
	public float signal;
	public float signalDecay = 1.0f;

	void Awake()
	{
        calories = Random.Range(0.5f, 1.0f);
	} // End of Awake().

	
	void Update()
	{
        calories += caloriesDelta * Time.deltaTime;
		//calories = Mathf.Clamp(calories, 0.0f, 10.0f);
		caloriesDelta = 0.0f;
		

        renderer.material.color = new Color(calories, calories, calories, 1.0f);
		renderer.material.color = Color.Lerp(renderer.material.color, Color.cyan * 2.0f, signal);
		
		signal -= Time.deltaTime;
		signal = Mathf.Clamp01(signal);
		
		signalDecay -= signal * Time.deltaTime;
		signalDecay += 0.1f * Time.deltaTime;
		signalDecay = Mathf.Clamp01(signalDecay);
		
		// Metabolism
        //calories -= 0.01f * Time.deltaTime;
		
		// If calories run out, node dies.
        //if (calories <= 0.0f)
        //    DestroyNode();
	} // End of Update().

    void DestroyNode()
    {
        Destroy(gameObject);
    } // End of DestroyNode().
}
