using UnityEngine;
using System.Collections;

public class UnitsPanel : MonoBehaviour {//the panel with all the units, also used for selecting the army before attack

	public const int unitsNo = 12;  //correlate with MenuUnitBase.cs

	public UILabel[] 
		existingUnitsNo = new UILabel[unitsNo],
		battleUnitsNo = new UILabel[unitsNo];

	public UISprite[] existingUnitsPics = new UISprite[unitsNo];

	public GameObject[] 
		minusBt = new GameObject[unitsNo],
		allBt = new GameObject[unitsNo],
		noneBt = new GameObject[unitsNo];

	public GameObject startBt, commitAllBt, loadingLb;

	private Component transData, saveLoadMap, stats, statusMsg, soundFX;

	// Use this for initialization
	void Start () {

		stats = GameObject.Find("Stats").GetComponent <Stats>();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		transData = GameObject.Find("TransData").GetComponent<TransData>();
		saveLoadMap = GameObject.Find("SaveLoadMap").GetComponent<SaveLoadMap>();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		UpdateMinusButtons();
	}

	public void LoadMultiplayer0()
	{	
		bool unitsAssigned = false;

		for (int i = 0; i < ((Stats)stats).battleUnits.Length; i++) 
		{
			if(((Stats)stats).battleUnits[i]>0)
			{
				unitsAssigned = true;
				break;
			}
		}

		if(unitsAssigned && ((Stats)stats).gold >= 250)		
		{
			StartCoroutine(LoadMultiplayerMap(0)); 
		}
		else if(!unitsAssigned)
		{
			((Messenger)statusMsg).DisplayMessage("Assign units to the battle.");
		}
		else
		{
			((Messenger)statusMsg).DisplayMessage("You need more gold.");
		}
	
	}

	private IEnumerator LoadMultiplayerMap(int levelToLoad)				//building loot values = half the price
	{	
		((Stats)stats).gold -= 250;										//this is where the price for the battle is payed, before saving the game

		int[] battleUnits = ((Stats)stats).battleUnits;

		for (int i = 0; i < battleUnits.Length; i++) 
		{
			((Stats)stats).currentPopulationNo -= ((Stats)stats).populationWeights[i]*((Stats)stats).battleUnits[i];

		}

		((Stats)stats).UpdateUI ();// - optional- no element of the UI is visible at this time

		((SaveLoadMap)saveLoadMap).SaveGame ();							//local autosave at battle load

		startBt.SetActive (false);
		commitAllBt.SetActive (false);
		loadingLb.SetActive (true);
		((TransData)transData).populationWeights = ((Stats)stats).populationWeights;
		((TransData)transData).battleUnits = ((Stats)stats).battleUnits;
		((TransData)transData).leftBehindUnits = ((Stats)stats).existingUnits;
		((TransData)transData).tutorialBattleSeen = ((Stats)stats).tutorialBattleSeen;

		((TransData)transData).soundOn = ((SoundFX)soundFX).soundOn;
		((TransData)transData).musicOn = ((SoundFX)soundFX).musicOn;

		yield return new WaitForSeconds (0.2f);
		switch (levelToLoad) 
		{
			case 0:
			Application.LoadLevel("Map01");
			break;		
		}
	}

	public void Commit0()	{Commit (0);}
	public void Commit1()	{Commit (1);}
	public void Commit2()	{Commit (2);}
	public void Commit3()	{Commit (3);}
	public void Commit4()	{Commit (4);}
	public void Commit5()	{Commit (5);}
	public void Commit6()	{Commit (6);}
	public void Commit7()	{Commit (7);}
	public void Commit8()	{Commit (8);}
	public void Commit9()	{Commit (9);}
	public void Commit10() {Commit (10);}
	public void Commit11() {Commit (11);}
	
	public void Cancel0(){Cancel (0);}
	public void Cancel1(){Cancel (1);}
	public void Cancel2(){Cancel (2);}
	public void Cancel3(){Cancel (3);}
	public void Cancel4(){Cancel (4);}
	public void Cancel5(){Cancel (5);}
	public void Cancel6(){Cancel (6);}
	public void Cancel7(){Cancel (7);}
	public void Cancel8(){Cancel (8);}
	public void Cancel9(){Cancel (9);}
	public void Cancel10(){Cancel (10);}
	public void Cancel11(){Cancel (11);}
	
	public void All0()	{CommitAll (0, true);}
	public void All1()	{CommitAll (1, true);}
	public void All2()	{CommitAll (2, true);}
	public void All3()	{CommitAll (3, true);}
	public void All4()	{CommitAll (4, true);}
	public void All5()	{CommitAll (5, true);}
	public void All6()	{CommitAll (6, true);}
	public void All7()	{CommitAll (7, true);}
	public void All8()	{CommitAll (8, true);}
	public void All9()	{CommitAll (9, true);}
	public void All10() {CommitAll (10, true);}
	public void All11() {CommitAll (11, true);}
	
	public void None0()	{CommitAll (0, false);}
	public void None1()	{CommitAll (1, false);}
	public void None2()	{CommitAll (2, false);}
	public void None3()	{CommitAll (3, false);}
	public void None4()	{CommitAll (4, false);}
	public void None5()	{CommitAll (5, false);}
	public void None6()	{CommitAll (6, false);}
	public void None7()	{CommitAll (7, false);}
	public void None8()	{CommitAll (8, false);}
	public void None9()	{CommitAll (9, false);}
	public void None10() {CommitAll (10, false);}
	public void None11() {CommitAll (11, false);}



	public void CommitAllUnits()
	{
		for (int i = 0; i < ((Stats)stats).existingUnits.Length; i++) 
		{
			if (((Stats)stats).existingUnits [i] > 0) 
			{
				if (((Stats)stats).battleUnits [i] == 0) 
				{
					minusBt[i].SetActive(true);
				}
				((Stats)stats).battleUnits [i] += ((Stats)stats).existingUnits [i];
				((Stats)stats).existingUnits [i] = 0;
				CommitAllButtons(i, false);
			} 
		}	

		UpdateUnits ();
	}

	public void ActivateStartBt()
	{
		if(!startBt.activeSelf)
		{
			startBt.SetActive(true);
			commitAllBt.SetActive (true);
		}
	}

	public void DeactivateStartBt()
	{
		if(startBt.activeSelf)
		{
			startBt.SetActive(false);
			commitAllBt.SetActive (false);
		}
	}
	private void UpdateMinusButtons()
	{
		for (int i = 0; i < unitsNo; i++) 
		{
			if (((Stats)stats).battleUnits [i] > 0) 
			{
				minusBt[i].SetActive(true);
				CommitAllButtons(i, false);
			}

			if (((Stats)stats).existingUnits [i] > 0) 
			{
				CommitAllButtons(i, true);
			}
		}
	}

	private void CommitAllButtons(int i, bool b)
	{
		allBt [i].SetActive (b);
		noneBt [i].SetActive (!b);
		
	}
	private void CommitAll(int i, bool all)
	{	
		
		if(all)
		{
			if (((Stats)stats).existingUnits [i] > 0) 
			{
				if (((Stats)stats).battleUnits [i] == 0) 
				{
					minusBt[i].SetActive(true);
				}
				
				((Stats)stats).battleUnits [i]+=((Stats)stats).existingUnits [i];
				((Stats)stats).existingUnits [i] = 0;
				
				CommitAllButtons(i, false);				
			} 
			else 
			{
				return;
			}
		}
		else 
		{
			if (((Stats)stats).battleUnits [i] > 0) 
			{				
				((Stats)stats).existingUnits [i]+=((Stats)stats).battleUnits [i];
				((Stats)stats).battleUnits [i]=0;
				
				minusBt[i].SetActive(false);
				CommitAllButtons(i, true);
			} 
			else 
			{
				return;
			}
		}
		
		UpdateUnits ();
	}



	private void Commit(int i)
	{
		if (((Stats)stats).existingUnits [i] > 0) 
		{
			if (((Stats)stats).battleUnits [i] == 0) 
			{
				minusBt[i].SetActive(true);
			}
			((Stats)stats).existingUnits [i]--; 
			((Stats)stats).battleUnits [i]++;
			
			if (((Stats)stats).existingUnits[i] == 0) 
			{
				CommitAllButtons(i, false);
			}

		} 
		else 
		{
			return;
		}

		UpdateUnits ();
	}

	private void Cancel(int i)
	{
		if (((Stats)stats).battleUnits [i] > 0) 
		{
			if (((Stats)stats).battleUnits [i] == 1) 
			{
				minusBt[i].SetActive(false);
			}
			((Stats)stats).battleUnits [i]--;
			((Stats)stats).existingUnits [i]++;
		} 
		else 
		{
			return;
		}

		UpdateUnits ();
	}


	public void UpdateStats()
	{
		StartCoroutine (UpdateExistingUnits());//cannot send command directly, takes a while to update from stats
	}

	private IEnumerator UpdateExistingUnits()//building reselect
	{
		yield return new WaitForSeconds(0.25f);
		UpdateUnits ();
	}

	private void UpdateUnits()
	{
		for (int i = 0; i < existingUnitsNo.Length; i++) 
		{
			if(((Stats)stats).existingUnits[i]>0 )
			{
				existingUnitsNo[i].text =  ((Stats)stats).existingUnits[i].ToString();
				((UISprite)existingUnitsPics[i]).color = new Color(255,255,255);
			}
			else
			{
				existingUnitsNo[i].text = " ";
			}

			if(((Stats)stats).battleUnits[i]>0)
			{
				battleUnitsNo[i].text = ((Stats)stats).battleUnits[i].ToString();
				((UISprite)existingUnitsPics[i]).color = new Color(255,255,255);				//normal tint for existing units
			}
			else
			{
				battleUnitsNo[i].text = " ";
			}

			if(((Stats)stats).existingUnits[i] == 0 && ((Stats)stats).battleUnits[i] == 0)
			{
				((UISprite)existingUnitsPics[i]).color = new Color(0,0,0);						//black tint for non-existing units
			}
		}
		UpdateMinusButtons ();

	}

}
