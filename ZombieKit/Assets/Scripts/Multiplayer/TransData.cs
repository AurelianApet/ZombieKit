using UnityEngine;
using System.Collections;

public class TransData : MonoBehaviour {

	private const int 
		numberOfUnits = 12,								//coordinate with menu unit, etc
		numberOfBuildings = 11;

	public int[] 
		populationWeights = new int[numberOfUnits],
		battleUnits = new int[numberOfUnits],			//when we get back home, this will return undeployed units
		leftBehindUnits = new int[numberOfUnits],
		buildingValues = new int[numberOfBuildings];  	//value will be awarded to attackers

	public bool[] buildingGoldBased = new bool[numberOfBuildings];

	public int goldGained, manaGained;

	public bool battleOver = false, tutorialBattleSeen = false, soundOn = true, musicOn = true;
	private bool collectGained = false;

	private float addTime = 0.1f, addCounter = 0;
	private int messageCounter = 0;

	private Component stats, messenger;

	void Awake() {
		DontDestroyOnLoad(this);						// Do not destroy this game object:		
	} 

	public void ReturnFromBattle()
	{
		GameObject.Find ("SaveLoadMap").GetComponent<SaveLoadMap> ().LoadFromLocal ();
		CleanDuplicate ();

		stats = GameObject.Find ("Stats").GetComponent<Stats> ();
		messenger = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		for (int i = 0; i < numberOfUnits; i++) 
		{
			((Stats)stats).existingUnits[i] = leftBehindUnits[i];
			((Stats)stats).battleUnits[i] = battleUnits[i];	
			((Stats)stats).currentPopulationNo += populationWeights[i]*battleUnits[i];
		}
		((Stats)stats).tutorialBattleSeen = tutorialBattleSeen;
		collectGained = true;
	}

	private void CleanDuplicate()//since transdata is not destroyed at level load, now we have a duplicate
	{
		GameObject[] transDatas=GameObject.FindGameObjectsWithTag("TransData");

		if (transDatas.Length == 2) 
		{
			for (int i = 0; i < transDatas.Length; i++) 
			{
				if(!transDatas[i].GetComponent<TransData>().battleOver)
				{
					Destroy(transDatas[i]);
					break;
				}
			}
		}
	}
		
	// Update is called once per frame
	void Update () {

		if (collectGained) //increases the available gold and mana with the loot
		{
			addCounter += Time.deltaTime;

			if(addCounter>=addTime)
			{

				addCounter = 0;

				if(goldGained>10)
				{
					int substract = goldGained/10;

					goldGained-= substract;
					((Stats)stats).gold += substract;
				}
				else if(goldGained>0)
				{
					goldGained--;
					((Stats)stats).gold ++;
				}
				if(manaGained>10)
				{
					int substract = manaGained/10;

					manaGained-= substract;
					((Stats)stats).mana += substract;
				}
				else if(manaGained>0)
				{
					manaGained--;
					((Stats)stats).mana ++;
				}

				((Stats)stats).ApplyMaxCaps();

				((Stats)stats).UpdateUI();

				if(goldGained==0 && manaGained==0)
					collectGained=false;

				messageCounter++;

				if(messageCounter>20)
				{
					((Messenger)messenger).DisplayMessage("Adding loot to our resources");
					((Stats)stats).VerifyMaxReached();
					messageCounter=0;
				}
			}
		}

	}
}
