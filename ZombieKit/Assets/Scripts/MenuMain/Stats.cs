using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour { //the resource bars on top of the screen

	private const int 
		noOfBuildings = 12,//correlate with BuildingCreator.cs; verify that the Inspector has 11 elements
		noOfUnits = 12;//correlate with MenuUnitBase.cs/ verify that the Inspector has 12 elements

	public GameObject GhostHelper;

	public bool 
		tutorialCitySeen = false, 
		tutorialBattleSeen = false,
		gameWasLoaded = false;

	public int 	//when user hard buys resources, storage capacity permanently increases 

		dobbitNo = 1, 
		occupiedDobbitNo = 0,
		currentPopulationNo = 1, 
		maxPopulationNo = 50,//for now, population is not factored in any way

		experience = 0,  
		crystals = 5,

		maxExperience = 1132570, 
		maxStorageGold = 5000, 
		maxStorageMana = 500, 
		maxCrystals = 5; 

	public float gold = 5000, mana = 500;
	public float[] productionRates;

	public int[] 
		productionBuildings,//0 gold 1 mana	
		populationWeights = new int[noOfUnits],
		existingUnits = new int[noOfUnits],
		battleUnits = new int[noOfUnits];

	public UIProgressBar experienceBar, dobbitsBar, goldBar,  manaBar, populationBar, crystalsBar;
	public UILabel populationLb, dobbitLb,  xpLb, goldLb, manaLb, crystalsLb, unitsLb;

	private Component transData, statusMsg;

	void Start () {

		transData = GameObject.Find ("TransData").GetComponent<TransData> ();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		StartCoroutine ("ReturnFromBattle");	
		StartCoroutine ("LaunchTutorial");
		productionBuildings = new int[2];
		productionRates = new float[2];
		InvokeRepeating ("Production", 1, 1);
	}

	private IEnumerator ReturnFromBattle()
	{
		yield return new WaitForSeconds (1.5f);

		if (((TransData)transData).battleOver) 
		{
			((TransData)transData).ReturnFromBattle();
			tutorialCitySeen = true;//since we have already been to battle, no tutorial 
		}	
	}

	private IEnumerator LaunchTutorial()
	{
		yield return new WaitForSeconds (10.0f);
		if (!tutorialCitySeen)
			GhostHelper.SetActive (true);//since this is a delayed function, we will activate the first time tutorial here 
	}

	private void Production()
	{
		bool isProducing = false;

		if (productionBuildings [0] > 0) 
		{
			if(gold<maxStorageGold)
			{
				gold += productionBuildings [0] * productionRates [0];
				isProducing=true;
			}
			else
			{
				((Messenger)statusMsg).DisplayMessage("Increase Gold storage capacity.");			
			}
		}
		if (productionBuildings [1] > 0) 
		{
			if(mana<maxStorageMana)
			{
				mana += productionBuildings [1] * productionRates [1];
				isProducing=true;
			}
			else
			{
				((Messenger)statusMsg).DisplayMessage("Increase Money storage capacity.");
			}
		}
		if (isProducing) UpdateUI();

	}

	public void ApplyMaxCaps()//cannot exceed storage+bought capacity
	{
		if (gold > maxStorageGold) { gold = maxStorageGold; }
		if (mana > maxStorageMana) { mana = maxStorageMana; }
		//if (experience > maxExperience) { experience = maxExperience; }
	}

	public void VerifyMaxReached()
	{
		if (gold == maxStorageGold) 
		{ 
			((Messenger)statusMsg).DisplayMessage("Increase Gold storage capacity.");	
		}
		if (mana == maxStorageMana) 
		{ 
			((Messenger)statusMsg).DisplayMessage("Increase Money storage capacity.");
		}
	}

	public void UpdateUI()//updates numbers and progress bars
	{
		((UISlider)experienceBar.GetComponent ("UISlider")).value = (float)experience/(float)maxExperience;
		((UISlider)dobbitsBar.GetComponent ("UISlider")).value = 1-((float)occupiedDobbitNo/(float)dobbitNo);
		((UISlider)goldBar.GetComponent ("UISlider")).value = (float)gold/(float)maxStorageGold;
		((UISlider)manaBar.GetComponent ("UISlider")).value = (float)mana/(float)maxStorageMana;
		((UISlider)crystalsBar.GetComponent ("UISlider")).value = (float)crystals/(float)maxCrystals;
		((UISlider)populationBar.GetComponent ("UISlider")).value = (float)currentPopulationNo/(float)maxPopulationNo;

		populationLb.text = currentPopulationNo.ToString () + " / " + maxPopulationNo.ToString (); 
		xpLb.text = experience.ToString ();
		dobbitLb.text = (dobbitNo-occupiedDobbitNo).ToString () + " / " + dobbitNo.ToString ();
		goldLb.text = ((int)gold).ToString ();
		manaLb.text = ((int)mana).ToString ();
		crystalsLb.text = crystals.ToString ();
	}

	public void UpdateUnitsNo()
	{	
		int remainingUnits = 0;
		for (int i = 0; i <  existingUnits.Length; i++) 
		{
			remainingUnits +=  existingUnits[i] + battleUnits[i];
		}		
		unitsLb.text = remainingUnits.ToString ();// update total units
	}
}
