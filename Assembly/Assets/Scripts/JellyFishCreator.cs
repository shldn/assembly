using UnityEngine;
using System.Collections;

public class JellyFishCreator : MonoBehaviour 
{
	
    [HideInInspector] public int headNum;
	public GameObject[] Heads;
    [HideInInspector] public int tailNum;
	public GameObject[] Tails;
    [HideInInspector] public int boballNum;
	public GameObject[] Boballs;
    [HideInInspector] public int wingNum;
	public GameObject[] smallTails;
	int num;
	public GUITexture myImg;

	
	// Update is called once per frame
	void Update () 
	{
        if (Input.GetKeyDown (KeyCode.A)) 
		{
			NextHead();
		}

		/*
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
		headNum = Random.Range(0, Heads.Length);
		changeHead(headNum);
	}

	public void TailChange()
	{
		tailNum = Random.Range(0, Tails.Length);
		changeTail(tailNum);
	}

	public void SmallTaillChange()
	{
		wingNum = Random.Range(0, smallTails.Length);
		smallTail(wingNum);
	}

	public void BoballChange()
	{
		boballNum = Random.Range(0, Boballs.Length);
		changeBoball(boballNum);
	}


    public void NextHead(){
        headNum = Mathf.FloorToInt(Mathf.Repeat(headNum + 1, Heads.Length));
		changeHead(headNum);
	}

	public void NextTail(){
		tailNum = Mathf.FloorToInt(Mathf.Repeat(tailNum + 1, Tails.Length));
		changeTail(tailNum);
	}

	public void NextWing(){
		wingNum = Mathf.FloorToInt(Mathf.Repeat(wingNum + 1, smallTails.Length));
		smallTail(wingNum);
	}

	public void NextBobble(){
		boballNum = Mathf.FloorToInt(Mathf.Repeat(boballNum + 1, Boballs.Length));
		changeBoball(boballNum);
	}

    public void changeProperty(string name, int number)
    {
        switch (name)
        {
            case "head":
                changeHead(number);
                break;
            case "tail":
                changeTail(number);
                break;
            case "wing":
                smallTail(number);
                break;
            case "bob":
            case "boball":
                changeBoball(number);
                break;
            default:
                Debug.LogError("Unknown property: " + name);
                break;
        }
    }


	public void changeHead(int number)
	{
        headNum = number;
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
        tailNum = number;
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
        boballNum = number;
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
        wingNum = number;
		for (int i = 0; i < smallTails.Length; i++) 
		{
			if(i == number)
				smallTails[i].SetActive(true);
			else
				smallTails[i].SetActive(false);
		};
	}


}


























