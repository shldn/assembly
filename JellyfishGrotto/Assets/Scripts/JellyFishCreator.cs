using UnityEngine;
using System.Collections;

public class JellyFishCreator : MonoBehaviour 
{
	
	public GameObject[] Heads;
	public GameObject[] Tails;
	public GameObject[] Boballs;
	public GameObject[] smallTails;
	int num;
	public GUITexture myImg;

	// Use this for initialization
	void Start () 
	{
		Heads [0].SetActive (true);
		Tails [0].SetActive (true);
		Boballs [0].SetActive (true);
		smallTails [0].SetActive (true);

        Randomize();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.A)) 
		{
			num = Random.Range(0,Heads.Length);
			changeHead(num);
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			num = Random.Range(0,Tails.Length);
			changeTail(num);
		}

		if (Input.GetKeyDown(KeyCode.D))
		{
			num = Random.Range(0,smallTails.Length);
			smallTail(num);
		}

		if (Input.GetKeyDown(KeyCode.F))
		{
			num = Random.Range(0,Boballs.Length);
			changeBoball(num);
		}

		/*

		if  (Input.touches.Length <= 0)
		{

		}else 
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				if (myImg.HitTest(Input.GetTouch(i).position))
				{
					if(Input.GetTouch(i).phase==TouchPhase.Began)
					{
						num = Random.Range(0,Heads.Length);
						changeHead(num);
					}
					if(Input.GetTouch(i).phase==TouchPhase.Stationary)
					{

					}
					if(Input.GetTouch(i).phase==TouchPhase.Moved)
					{

					}
					if(Input.GetTouch(i).phase==TouchPhase.Ended)
					{

					}
				}
			}
		}
		*/

		/*
		if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
		{

		}
		*/
	}


	public void HeadChange()
	{
		num = Random.Range(0,Heads.Length);
		changeHead(num);
	}

	public void TailChange()
	{
		num = Random.Range(0,Tails.Length);
		changeTail(num);
	}

	public void SmallTaillChange()
	{
		num = Random.Range(0,smallTails.Length);
		smallTail(num);
	}

	public void BoballChange()
	{
		num = Random.Range(0,Boballs.Length);
		changeBoball(num);
	}

	public void changeHead(int number)
	{

		for (int i = 0; i < Heads.Length; i++) 
		{
			if(i == number)
				Heads[i].SetActive(true);
			else
				Heads[i].SetActive(false);
		};
	}

	public void changeTail(int number)
	{
		
		for (int i = 0; i < Tails.Length; i++) 
		{
			if(i == number)
				Tails[i].SetActive(true);
			else
				Tails[i].SetActive(false);
		};
	}

	public void changeBoball(int number)
	{
		
		for (int i = 0; i < Boballs.Length; i++) 
		{
			if(i == number)
				Boballs[i].SetActive(true);
			else
				Boballs[i].SetActive(false);
		};
	}

	public void smallTail(int number)
	{
		
		for (int i = 0; i < smallTails.Length; i++) 
		{
			if(i == number)
				smallTails[i].SetActive(true);
			else
				smallTails[i].SetActive(false);
		};
	}


    public void Randomize(){

		num = Random.Range(0,Heads.Length);
		changeHead(num);

		num = Random.Range(0,Tails.Length);
		changeTail(num);

		num = Random.Range(0,smallTails.Length);
		smallTail(num);

		num = Random.Range(0,Boballs.Length);
		changeBoball(num);

    } // End of Randomize().


}


























