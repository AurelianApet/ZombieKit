using UnityEngine;
using System.Collections;

public class ConstructionSelector : MonoBehaviour {		//controls the behaviour of a "building under construction", from placement to completion

	private bool inConstruction = true;

	public bool 
		isSelected = true,								//for initial processing, right after the construction is instantiated
		battleMap = false;								//flag - some components only exist in hometown/battlemap

	public float progTime = 0.57f, progCounter = 0;		//for progress timer, one minute

	public int 
		buildingTime = 1,
		remainingTime = 1,
		priceno,										//price displayed for "finish now" button. based on remaining time
		storageIncrease,								//passes maxStorage to stats
		constructionIndex = -1;							//unique ID for constructions

	private int hours, minutes, seconds;				//for time remaining label
	public UILabel TimeCounterLb;

	public GameObject 
		Price,											//own child obj - has the price label
		ProgressBar;									//own child obj

	private GameObject GroupBuildings;					//to parent the building after it's finished

	private GameObject[] selectedGrassType;	

	public string buildingType; 						//what kind of building is hosting

	private Component soundFX, relay, statusMsg, stats;

	void Start () 
	{
		GroupBuildings = GameObject.Find("GroupBuildings");

		relay = GameObject.Find("Relay").GetComponent<Relay>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();

		if(!battleMap)
		{
			stats = GameObject.Find("Stats").GetComponent<Stats>();
		}

		//init price so user can't click fast on price 0
		remainingTime = buildingTime * (1 - (int)((UISlider)ProgressBar.GetComponent("UISlider")).value);
		UpdatePrice (remainingTime);
	}

	void FixedUpdate()
	{
		if(inConstruction)
		{
			ProgressBarUpdate();
		}
	}

	private void ProgressBarUpdate()
	{		
		progCounter += Time.deltaTime*0.5f;
		if(progCounter > progTime)
		{
			progCounter = 0;
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value += (float)(Time.deltaTime/buildingTime);				//update progress bars values
			
			((UISlider)ProgressBar.GetComponent("UISlider")).value=
			Mathf.Clamp(((UISlider)ProgressBar.GetComponent("UISlider")).value,0,1);

			remainingTime = (int)(buildingTime * (1 - ((UISlider)ProgressBar.GetComponent("UISlider")).value));

			UpdatePrice (remainingTime);
			UpdateTimeCounter(remainingTime);

			if(((UISlider)ProgressBar.GetComponent("UISlider")).value == 1)				//building finished - the progress bar has reached 1												
			{
				((SoundFX)soundFX).BuildingFinished();

				if(!battleMap)															//if this building is not finished on a battle map
				{
					((Stats)stats).occupiedDobbitNo--;									//the dobbit previously assigned becomes available
					((Stats)stats).UpdateUI();
				
					if(buildingType=="Barrel")											//increases total storage in Stats																	
					{
						((Stats)stats).maxStorageMana += storageIncrease;
					}
					else if(buildingType=="Forge")
					{
						((Stats)stats).productionBuildings[0]++;
						((Stats)stats).maxStorageGold += storageIncrease;
					}
					else if(buildingType=="InstitutionZ2")
					{
						((Stats)stats).productionBuildings[0]++;
						((Stats)stats).maxStorageGold += storageIncrease;
					}
					else if(buildingType=="InstitutionZ3")
					{
						((Stats)stats).productionBuildings[0]++;
						((Stats)stats).maxStorageGold += storageIncrease;
					}
					else if(buildingType=="Generator")
					{
						((Stats)stats).productionBuildings[1]++;
						((Stats)stats).maxStorageMana += storageIncrease;
					}
					else if(buildingType=="Vault")
					{
						((Stats)stats).maxStorageGold += storageIncrease;
					}
					((Stats)stats).UpdateUI();
				}

				foreach (Transform child in transform) 									//parenting and destruction of components no longer needed
				{					
					if(child.gameObject.tag == buildingType)
					{
						child.gameObject.SetActive(true);
						((BuildingSelector)child.gameObject.GetComponent("BuildingSelector")).inConstruction = false;

						if(battleMap)
							((BuildingSelector)child.gameObject.GetComponent("BuildingSelector")).battleMap = true;

						foreach (Transform childx in transform) 
						{	
							if(childx.gameObject.tag == "Grass")
							{									
								childx.gameObject.transform.parent = child.gameObject.transform;
								child.gameObject.transform.parent = GroupBuildings.transform;
								break;
							}
						}
					}
				}

				Destroy(this.gameObject);
				inConstruction = false;	

			}
			
		}	
		
	}

	private void UpdateTimeCounter(int remainingTime)				//calculate remaining time
	{
		hours = (int)remainingTime/60; 
		minutes = (int)remainingTime%60;
		seconds = (int)(60 - (((UISlider)ProgressBar.GetComponent("UISlider")).value*buildingTime*60)%60);	

		if (minutes == 60) minutes = 0;
		if (seconds == 60) seconds = 0;

		UpdateTimeLabel ();
	}

	private void UpdateTimeLabel()									//update the time labels on top
	{
		if(hours>0 && minutes >0 && seconds>=0 )
		{			

			((UILabel)TimeCounterLb).text = 
				hours.ToString() +" h " +
					minutes.ToString() +" m " +
					seconds.ToString() +" s ";			
		}
		else if(minutes > 0 && seconds >= 0)
		{
			((UILabel)TimeCounterLb).text = 
				minutes.ToString() +" m " +
					seconds.ToString() +" s ";
			
		}
		else if(seconds > 0 )
		{
			((UILabel)TimeCounterLb).text = 
				seconds.ToString() +" s ";
		}

	}


	private void UpdatePrice(int remainingTime)					//update the price label on the button, based on remaining time		
	{
		/*
		0		30		1
		30		60		3
		60		180		7
		180		600		15
		600		1440	30
		1440	2880	45
		2880	4320	70
		4320			150
		 */

		if (remainingTime >= 4320) { priceno = 150; }
		else if (remainingTime >= 2880) { priceno = 70; }
		else if (remainingTime >= 1440) { priceno = 45;	}
		else if (remainingTime >= 600)	{ priceno = 30;	}
		else if (remainingTime >= 180) { priceno = 15; }
		else if (remainingTime >= 60) { priceno = 7; }
		else if (remainingTime >= 30) {	priceno = 3; }
		else if(remainingTime >= 0) { priceno = 1; }

		((tk2dTextMesh)Price.GetComponent("tk2dTextMesh")).text = priceno.ToString();
	}

	public void Finish()									
	{
		//if(!battleMap) //no need to check, the finish button is not visible on the battle map
		//{
		if (!((Relay)relay).pauseInput && !((Relay)relay).delay)  //panels are open / buttons were just pressed 
		{
			((SoundFX)soundFX).Click();
			if (((Stats)stats).crystals >= priceno) 
			{
				((Stats)stats).crystals -= priceno;
				((Stats)stats).UpdateUI();
				((UISlider)ProgressBar.GetComponent ("UISlider")).value = 1;
			} 
			else 
			{				
				((Messenger)statusMsg).DisplayMessage("Insufficient crystals");
			}
		}
		//}
	}
}
