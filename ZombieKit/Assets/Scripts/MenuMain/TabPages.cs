using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TabPages : MonoBehaviour {//controls the 2 pages and arrows for units, buildings, etc

	private int noPanel = 0;	
	private const int noPages = 2;
	public GameObject[] Pages = new GameObject[noPages];
	public GameObject ArrowLeft, ArrowRight;
	private Component relay;

	void Start () {
		
		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
	}

	private void Delay()//to prevent menu button commands from interfering with sensitive areas/buttons underneath
	{
		((Relay)relay).DelayInput();
	}

	public void OnArrowLeft()
	{
		Delay ();
		Pages[1].SetActive(false);
		Pages[0].SetActive(true);
		ArrowLeft.SetActive(false);
		ArrowRight.SetActive(true);
		noPanel--;
	}
	
	public void OnArrowRight()
	{	
		Delay ();
		Pages[0].SetActive(false);
		Pages[1].SetActive(true);
		ArrowRight.SetActive(false);
		ArrowLeft.SetActive(true);
		noPanel++;
	}	
	
}

