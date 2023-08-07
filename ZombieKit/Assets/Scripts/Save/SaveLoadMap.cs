using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Text;
//																	## This is for local saving / loading ##  

public struct ConStruct
{
	public string buildingTag;
	public int 
	buildingIndex,
	buildingTime,
	remainingTime,
	storageIncrease;
}

public class SaveLoadMap : MonoBehaviour {

	private List<ConStruct[]> conStruct = new List<ConStruct[]>();

	private const int 
		buildingTypesNo = 12,	//the building tags
		grassTypesNo = 3;

	private int 
		buildingZ = 1,	//layer depths for building, correlate with BuildingCreator.cs
		grassZ = 2;		//layer depths for grass, correlate with BuildingCreator.cs

	private string 
		filePath,
		fileNameLocal = "LocalSave",
		fileNameServer = "ServerSave",
		fileNameAttack = "ServerAttack",
		fileExt = ".txt";

	private string[] buildingTypes = new string[buildingTypesNo]{"HauntedPumpkin","HouseBlueZ1","HouseBrownZ1","InstitutionZ2","InstitutionZ3",
		"ModernBuildingZ4","ModernBuildingZ5","ModernTowerZ1","Store","TowerBlockZ1","WaterTowerZ1","WaterTowerZ2"};

	private int[] existingBuildings = new int[buildingTypesNo];//the entire array is transfered to BuildingCreator.cs; 
	//records how many buildings of each type, when they are built/game is loaded

	public GameObject[] 
		BuildingPrefabs = new GameObject[buildingTypesNo],
		GrassPrefabs = new GameObject[grassTypesNo];//three types of grass patches

	public GameObject 
		ConstructionPrefab,
		UnitProc, //the data for units under construction is extracted from unitproc, 
				  //that handles construction in a much simpler manner, having no graphics
		MenuUnit, //the object that holds the MenuUnit script; since it's disabled, must be linked manually
		DamagePanel, 
		GhostHelper;

	private List<GameObject[]> buildingList = new List<GameObject[]>();

	private GameObject[] Grass, Construction;

	private GameObject GroupBuildings;//object used to parent the buildings, once they are instantiated

	//lists for these elements - unknown number of elements
	private List<GameObject> 
		LoadedBuildings = new List<GameObject>(),
		LoadedConstructions = new List<GameObject>(),
		LoadedGrass = new List<GameObject>();

	private DateTime saveDateTime, loadDateTime;//saveTime, currentTime
	private TimeSpan timeDifference;

	public UILabel goldLostLb, manaLostLb, buildingsLostLb, unitsLostLb;

	private Component stats, statusMsg, soundFX, menuUnit, buildingCreator, relay, ghostHelper, unitProc;

	// Use this for initialization
	void Start () 
	{
		GroupBuildings = GameObject.Find("GroupBuildings");
		buildingCreator = GameObject.Find("BuildingCreator").GetComponent<BuildingCreator>();
		relay = GameObject.Find("Relay").GetComponent<Relay>();

		//C:\Users\user\AppData\LocalLow\AStarterKits\StrategyKit
		//filePath = Application.dataPath + "/";//windows - same folder as the project
		filePath = Application.persistentDataPath +"/";//iphone		                                    
		unitProc = UnitProc.GetComponent("UnitProc");//UnitProc.sc script
		menuUnit = MenuUnit.GetComponent("MenuUnit");//MenuUnit.sc - necessary when saving only
		ghostHelper = GhostHelper.GetComponent("GhostHelper");
		stats = GameObject.Find("Stats").GetComponent<Stats>();// Stats.cs for HUD data
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();//displays user messages
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();//audio options

		loadDateTime = System.DateTime.Now;//current time

		//LoadFromLocal ();	//automatic local load at startup - for local testing; for multiplayer, 
							//the user must both upload/download from server to receive the attack results
	}

	void OnApplicationQuit() {//autosave
		//if(!((Relay)relay).pauseInput)//check if the user is doing something, like moving buildings
		//SaveGame();
	}

	/* 	
	    //save file data structure
		Buildings: buildingType, buildingIndex, position.x, position.y
		Grass: grassType, grassIndex, position.x, position.y
		Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
		Units: currentSlidVal, currentTrainingTime
		Units: trainingTimes, existingUnits, battleUnits
		Units: queList = qIndex, objIndex, trainingIndex
		Stats: experience,dobbits,occupiedDobits,currentPopulation,maxPopulation,gold,mana,crystals,maxStorageGold,maxStorageMana,maxCrystals,productionRateGold,productionRateMana,tutorialCitySeen,tutorialBattleSeen,soundOn,musicOn
	*/

	public void SaveGame()
	{
		((Stats)stats).gameWasLoaded = true;//saving the game also prevents the user from loading on top
		print ("Save Game Path on your system: " + filePath.ToString ());
		StreamWriter sWriter = new StreamWriter (filePath + fileNameLocal + fileExt);
		ReadObjects ();//reads all buildings/grass/under construction


		sWriter.WriteLine ("###StartofFile###");

		sWriter.WriteLine ("###Buildings###");

		for (int i = 0; i < buildingList.Count; i++) 
		{
			GameObject[] buildingArray = buildingList[i];

			for (int j = 0; j < buildingArray.Length; j++) 
			{			
				Component BSel = buildingArray[j].GetComponent("BuildingSelector");
					
				sWriter.WriteLine(((BuildingSelector)BSel).buildingType+","+
				                  ((BuildingSelector)BSel).buildingIndex.ToString()+","+
				                  buildingArray[j].transform.position.x+","+
				                  buildingArray[j].transform.position.y
				                  );
			}
		}

		sWriter.WriteLine ("###Grass###");
		for (int i = 0; i < Grass.Length; i++) 
		{
			Component GSel = Grass[i].GetComponent("GrassSelector");
			sWriter.WriteLine(((GrassSelector)GSel).grassType+","+
			                  ((GrassSelector)GSel).grassIndex.ToString()+","+
			                  Grass[i].transform.position.x+","+
			                  Grass[i].transform.position.y
			                  );
		}

		sWriter.WriteLine ("###Construction###");
		for (int i = 0; i < Construction.Length; i++) 
		{
			Component CSel = Construction[i].GetComponent("ConstructionSelector");
			sWriter.WriteLine(((ConstructionSelector)CSel).buildingType+","+
			                  ((ConstructionSelector)CSel).constructionIndex.ToString()+","+
			                  ((ConstructionSelector)CSel).buildingTime+","+
			                  ((ConstructionSelector)CSel).remainingTime+","+
			                  ((ConstructionSelector)CSel).storageIncrease+","+
			                  Construction[i].transform.position.x+","+
			                  Construction[i].transform.position.y
			                  );
		}

		sWriter.WriteLine("###BuildingIndex###");//the unique id for buildings/grass patches

		sWriter.WriteLine(((BuildingCreator)buildingCreator).buildingIndex);

		sWriter.WriteLine (((UnitProc)unitProc).currentSlidVal.ToString("0.00")+","+
		                   (((UnitProc)unitProc).currentTrainingTime)		                            
		                   );
				   
		const int numberOfUnits = 12;
		int[] trainingTimes = new int[numberOfUnits];//an array that holds training times from all units - 
													//at first load, the XML will not have been read 
		int[] populationWeights = new int[numberOfUnits];
		int[] existingUnits = new int[numberOfUnits];//these units form the current population
		int[] battleUnits = new int[numberOfUnits];

		trainingTimes = ((UnitProc)unitProc).trainingTimes;//replace our empty array with the xml values, already in unitproc
		populationWeights = ((UnitProc)unitProc).populationWeights;
		existingUnits = ((Stats)stats).existingUnits;
		battleUnits = ((Stats)stats).battleUnits;

		sWriter.WriteLine (String.Join(",", new List<int>(trainingTimes).ConvertAll(i => i.ToString()).ToArray()));
		sWriter.WriteLine (String.Join(",", new List<int>(populationWeights).ConvertAll(i => i.ToString()).ToArray()));
		sWriter.WriteLine (String.Join(",", new List<int>(existingUnits).ConvertAll(i => i.ToString()).ToArray()));
		sWriter.WriteLine (String.Join(",", new List<int>(battleUnits).ConvertAll(i => i.ToString()).ToArray()));
		//qIndex, objIndex, trainingIndex  
		//0  5  10 
		// 0 = first position in queue ; 5 = object index - the fifth button/unit type ; 10 = number of units under construction

		List<Vector3> queList = new List<Vector3>();
		queList=((UnitProc)unitProc).queList;

		for (int i = 0; i < queList.Count; i++) 
		{
			sWriter.WriteLine(queList[i].ToString().Trim(new Char[] { ')', '(' }));
		}

		sWriter.WriteLine ("###Stats###");

		sWriter.WriteLine (((Stats)stats).experience+","+
		                   ((Stats)stats).dobbitNo+","+
		                   ((Stats)stats).occupiedDobbitNo+","+
		                   ((Stats)stats).currentPopulationNo+","+
		                   ((Stats)stats).maxPopulationNo+","+
		                   ((Stats)stats).gold+","+
		                   ((Stats)stats).mana+","+
		                   ((Stats)stats).crystals+","+
		                   ((Stats)stats).maxStorageGold+","+
		                   ((Stats)stats).maxStorageMana+","+
		                   ((Stats)stats).maxCrystals+","+
		                   ((Stats)stats).productionRates[0]+","+//forge gold production per second
		                   ((Stats)stats).productionRates[1]+","+//ModernBuildingZ4 mana
		                   ((Stats)stats).tutorialCitySeen+","+
		                   ((Stats)stats).tutorialBattleSeen+","+
		                   ((SoundFX)soundFX).soundOn+","+
		                   ((SoundFX)soundFX).musicOn
		                   );

		sWriter.WriteLine (System.DateTime.Now);

		sWriter.WriteLine ("###EndofFile###");

		sWriter.Flush ();
		sWriter.Close ();
		existingBuildings = new int[buildingTypesNo];//reset for next save - remove if automatic
		((Messenger)statusMsg).DisplayMessage("Game saved.");
	}

	private void ReadObjects()//reads all buildings/grass/under construction
	{ 
		buildingList.Clear ();
				
		for (int i = 0; i < buildingTypes.Length; i++) //find all buildings
		{
			buildingList.Add(GameObject.FindGameObjectsWithTag (buildingTypes[i]));			
		}

		Grass = GameObject.FindGameObjectsWithTag ("Grass");//finds all patches of grass from underneath the buildings
		Construction = GameObject.FindGameObjectsWithTag ("Construction");//find all buildings under construction
	}

	private bool CheckLocalSaveFile()
	{
		((Messenger)statusMsg).DisplayMessage("Checking for local save file...");
		bool localSaveExists = File.Exists(filePath + fileNameLocal + fileExt);
		return(localSaveExists);
	}

	private bool CheckLoadOnce()
	{
		bool gameWasLoaded = ((Stats)stats).gameWasLoaded;

		if(gameWasLoaded) //prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		{
			((Messenger)statusMsg).DisplayMessage("Only one load per session is allowed. Canceling...");
		}

		return gameWasLoaded;
	}

	public void LoadFromLocal()
	{
		if(CheckLoadOnce ()) return;

		if(CheckLocalSaveFile())
		{
			((Messenger)statusMsg).DisplayMessage("Local save file found.");
			StreamReader sReader = new StreamReader(filePath + fileNameLocal + fileExt);
			LoadGame (sReader);
		}
		else
		{
			((Messenger)statusMsg).DisplayMessage("No local save file found.");
			((SoundFX)soundFX).ChangeMusic (true);//first run, start music
		}
	}
	public void LoadFromServer()
	{
		if(CheckLoadOnce ()) return;

		StreamReader sReader = new StreamReader(filePath + fileNameServer + fileExt);
		LoadGame (sReader);

	}
	public void LoadAttackFromServer()
	{
		StreamReader sReader = new StreamReader(filePath + fileNameAttack + fileExt);
		AttackDamage (sReader);
	}

	private void AttackDamage(StreamReader sReader)
	{

		string currentLine = "";
		currentLine = sReader.ReadLine();//skip header

		currentLine = sReader.ReadLine();//read gold/mana

		string[] losses = currentLine.Split(","[0]);

		int goldLost = int.Parse(losses[0]);
		int manaLost = int.Parse(losses[1]);
		int buildingsLost = int.Parse(losses[2]);
		int unitsLost = int.Parse(losses[3]);

		if (goldLost == 0 && manaLost == 0) 
		{
			return;//the file exists, but has been loaded and reset
		}

		goldLostLb.text = goldLost.ToString ();
		manaLostLb.text = manaLost.ToString ();
		buildingsLostLb.text = buildingsLost.ToString ();
		unitsLostLb.text = unitsLost.ToString ();

		((Stats)stats).gold -= goldLost;
		((Stats)stats).mana -= manaLost;
		((Stats)stats).UpdateUI();
		StartCoroutine("ActivateDamagePanel");
	}

	private IEnumerator ActivateDamagePanel()//keeps trying to launch the damage panel, waiting fot the user to finish other tasks, if any
	{										 //otherwise the panel is superimposed on other panels	
		yield return new WaitForSeconds (5);
		if(!((Relay)relay).pauseInput)
		{
			((Relay)relay).pauseInput = true;
			if(GhostHelper.activeSelf){ ((GhostHelper)ghostHelper).ResetHelper();}
			DamagePanel.SetActive (true);
		}
		else
			StartCoroutine("ActivateDamagePanel");
	}

	public void LoadGame(StreamReader sReader)
	{
		((Messenger)statusMsg).DisplayMessage("Loading...");

		((Stats)stats).gameWasLoaded = true;

		LoadedBuildings.Clear ();
		LoadedConstructions.Clear ();
		LoadedGrass.Clear ();

		string currentLine = "";

		while (currentLine != "###Buildings###") 
		{
			currentLine = sReader.ReadLine();//goes through the headers, without doing anything, till it finds the ###Buildings### header
		}

		while (currentLine != "###Grass###") //Buildings - read till next header is found 
		{
		//Buildings: buildingType, buildingIndex, position.x, position.y
		currentLine = sReader.ReadLine();
		if(currentLine != "###Grass###") //if next category reached, skip
		{
			string[] currentBuilding = currentLine.Split(","[0]);

			string buildingTag = currentBuilding[0];
			int buildingIndex = int.Parse(currentBuilding[1]);

			float posX= float.Parse(currentBuilding[2]);
			float posY= float.Parse(currentBuilding[3]);

			switch (buildingTag) //based on tag
			{
			//"HauntedPumpkin","HouseBlueZ1","HouseBrownZ1","InstitutionZ2","InstitutionZ3","ModernBuildingZ4","ModernBuildingZ5","ModernTowerZ1","Store","TowerBlockZ1","WaterTowerZ1"
			//Buildings: buildingType, buildingIndex, position.x, position.y
							
			case "HauntedPumpkin":			
				GameObject HauntedPumpkin = (GameObject)Instantiate(BuildingPrefabs[0], new Vector3(posX,posY,buildingZ), Quaternion.identity);				
				existingBuildings[0]++;//a local array that holds how many buildings of each type
			break;
			
			case "HouseBlueZ1":
				GameObject HouseBlueZ1 = (GameObject)Instantiate(BuildingPrefabs[1], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[1]++;
				break;

			case "HouseBrownZ1":
				GameObject HouseBrownZ1 = (GameObject)Instantiate(BuildingPrefabs[2], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[2]++;
				break;

			case "InstitutionZ2":
			GameObject InstitutionZ2 = (GameObject)Instantiate(BuildingPrefabs[3], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[3]++;
				break;
			
			case "InstitutionZ3":
			GameObject InstitutionZ3 = (GameObject)Instantiate(BuildingPrefabs[4], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				((Stats)stats).productionBuildings[0]++;//sends the InstitutionZ3 to the production buildings array in stats; used at 1 second to produce resources
				existingBuildings[4]++;
				break;
			
			case "ModernBuildingZ4":
			GameObject ModernBuildingZ4 = (GameObject)Instantiate(BuildingPrefabs[5], new Vector3(posX,posY,buildingZ), Quaternion.identity);				
				((Stats)stats).productionBuildings[1]++;//sends the ModernBuildingZ4 to the production buildings array in stats; used at 1 second to produce resources
				existingBuildings[5]++;
				break;
			
			case "ModernBuildingZ5":
			GameObject ModernBuildingZ5 = (GameObject)Instantiate(BuildingPrefabs[6], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[6]++;
				break;
			
			case "ModernTowerZ1":
			GameObject ModernTowerZ1 = (GameObject)Instantiate(BuildingPrefabs[7], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[7]++;
				break;
			
			case "Store":
			GameObject Store = (GameObject)Instantiate(BuildingPrefabs[8], new Vector3(posX,posY,buildingZ), Quaternion.identity);				
				existingBuildings[8]++;
				break;
			
			case "TowerBlockZ1":
			GameObject TowerBlockZ1 = (GameObject)Instantiate(BuildingPrefabs[9], new Vector3(posX,posY,buildingZ), Quaternion.identity);				
				existingBuildings[9]++;
				break;
			
			case "WaterTowerZ1":
			GameObject WaterTowerZ1 = (GameObject)Instantiate(BuildingPrefabs[10], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[10]++;
				break;

			case "WaterTowerZ2":
			GameObject WaterTowerZ2 = (GameObject)Instantiate(BuildingPrefabs[11], new Vector3(posX,posY,buildingZ), Quaternion.identity);					
				existingBuildings[11]++;
				break;
			}

			ProcessBuilding(buildingTag, buildingIndex);//tag + building index
		}
		}

		while (currentLine != "###Construction###") //loads the grass patches, for both buildings and underconstruction
		{
			//Grass: grassType, grassIndex, position.x, position.y
			currentLine = sReader.ReadLine ();
			if (currentLine != "###Construction###") 
			{
				string[] currentGrass = currentLine.Split ("," [0]);//reads the line, values separated by ","

				int grassType = int.Parse (currentGrass [0]);
				int grassIndex = int.Parse (currentGrass [1]);

				float posX = float.Parse (currentGrass [2]);
				float posY = float.Parse (currentGrass [3]);	

				switch (int.Parse (currentGrass [0])) 
				{				
					case 2:
					GameObject Grass2x = (GameObject)Instantiate (GrassPrefabs [0], new Vector3 (posX, posY, grassZ), Quaternion.identity);					
					break;

					case 3:
					GameObject Grass3x = (GameObject)Instantiate (GrassPrefabs [1], new Vector3 (posX, posY, grassZ), Quaternion.identity);	
					break;
					
					case 4:
					GameObject Grass4x = (GameObject)Instantiate (GrassPrefabs [2], new Vector3 (posX, posY, grassZ), Quaternion.identity);	
					break;

					case 42:
					GameObject Grass4x2 = (GameObject)Instantiate (GrassPrefabs [3], new Vector3 (posX, posY, grassZ), Quaternion.identity);	
					break;

				}
				ProcessGrass (grassType, grassIndex);
			}
		}
		while (currentLine != "###BuildingIndex###") //Construction
		{
			//Construction: buildingType, constructionIndex, buildingTime, remainingTime, storageIncrease, position.x, position.y
			currentLine = sReader.ReadLine ();
			if (currentLine != "###BuildingIndex###") 
			{
				string[] currentConstruction = currentLine.Split ("," [0]);//reads the line, values separated by ","

				string buildingTag = currentConstruction[0];
				int buildingIndex = int.Parse(currentConstruction[1]);
				int buildingTime = int.Parse(currentConstruction [2]);
				int remainingTime = int.Parse(currentConstruction [3]); 
				int storageIncrease = int.Parse(currentConstruction [4]);

				float posX = float.Parse (currentConstruction [5]);
				float posY = float.Parse (currentConstruction [6]);


				GameObject Construction = (GameObject)Instantiate(ConstructionPrefab, new Vector3(posX,posY,buildingZ), Quaternion.identity);

				switch (buildingTag) 
				{
				//"HauntedPumpkin","HouseBlueZ1","HouseBrownZ1","InstitutionZ2","InstitutionZ3","ModernBuildingZ4","ModernBuildingZ5","ModernTowerZ1","Store","TowerBlockZ1","WaterTowerZ1"
				//Construction: buildingType, constructionIndex, buildingTime, remainingTime,storageincrease,  position.x, position.y
					
				case "HauntedPumpkin":		
					GameObject HauntedPumpkin = (GameObject)Instantiate(BuildingPrefabs[0], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[0]++;
				break;
					
				case "HouseBlueZ1":
					GameObject HouseBlueZ1 = (GameObject)Instantiate(BuildingPrefabs[1], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[1]++;
				break;
					
				case "HouseBrownZ1":
					GameObject HouseBrownZ1 = (GameObject)Instantiate(BuildingPrefabs[2], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[2]++;
					break;
					
				case "InstitutionZ2":
					GameObject InstitutionZ2 = (GameObject)Instantiate(BuildingPrefabs[3], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[3]++;
					break;
					
				case "InstitutionZ3":
					GameObject InstitutionZ3 = (GameObject)Instantiate(BuildingPrefabs[4], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[4]++;
					break;
					
				case "ModernBuildingZ4":
					GameObject ModernBuildingZ4 = (GameObject)Instantiate(BuildingPrefabs[5], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[5]++;
					break;
					
				case "ModernBuildingZ5":
					GameObject ModernBuildingZ5 = (GameObject)Instantiate(BuildingPrefabs[6], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[6]++;
					break;
					
				case "ModernTowerZ1":
					GameObject ModernTowerZ1 = (GameObject)Instantiate(BuildingPrefabs[7], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[7]++;
					break;
					
				case "Store":
					GameObject Store = (GameObject)Instantiate(BuildingPrefabs[8], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[8]++;
					break;
					
				case "TowerBlockZ1":
					GameObject TowerBlockZ1 = (GameObject)Instantiate(BuildingPrefabs[9], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
					existingBuildings[9]++;
					break;
					
				case "WaterTowerZ1":
					GameObject WaterTowerZ1 = (GameObject)Instantiate(BuildingPrefabs[10], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[10]++;
					break;

				case "WaterTowerZ2":
					GameObject WaterTowerZ2 = (GameObject)Instantiate(BuildingPrefabs[11], new Vector3(posX,posY,buildingZ), Quaternion.identity);
					existingBuildings[11]++;
					break;	
				}

				ProcessConstruction(buildingTag, buildingIndex, buildingTime, remainingTime, storageIncrease);
				//StartCoroutine(ProcessConstruction(buildingTag, buildingIndex, buildingTime, remainingTime, storageIncrease));

			}
		}

		ParentBuildings ();

		currentLine = sReader.ReadLine();
		((BuildingCreator)buildingCreator).buildingIndex = int.Parse(currentLine);


		//UNITS
		currentLine = sReader.ReadLine();//#Add verification for empty que
		UnitProc.SetActive (true);

		string[] currentUnitinProgress = currentLine.Split(","[0]);

		((UnitProc)unitProc).currentSlidVal = float.Parse (currentUnitinProgress [0]);
		((UnitProc)unitProc).currentTrainingTime = int.Parse (currentUnitinProgress [1]);

		currentLine = sReader.ReadLine();//load training times
		string[] trainingTimes = currentLine.Split(","[0]);

		for (int i = 0; i < trainingTimes.Length; i++) 
		{
			int value = int.Parse(trainingTimes[i]);
			((UnitProc)unitProc).trainingTimes[i] = value;
			((MenuUnit)menuUnit).trainingTimes[i] = value; //info is loaded late from xml; user could finish all units with one crystal
		}

		currentLine = sReader.ReadLine();//load training times
		string[] populationWeights = currentLine.Split(","[0]);
		
		for (int i = 0; i < populationWeights.Length; i++) 
		{
			int value = int.Parse(populationWeights[i]);
			((UnitProc)unitProc).populationWeights[i] = value;
			((MenuUnit)menuUnit).populationWeights[i] = value; //info is loaded late from xml; 
			((Stats)stats).populationWeights[i] = value;
		}

		currentLine = sReader.ReadLine();//load existing units
		string[] existingUnits = currentLine.Split(","[0]);

		for (int i = 0; i < existingUnits.Length; i++) 
		{
			((Stats)stats).existingUnits[i] = int.Parse(existingUnits[i]);
		}

		currentLine = sReader.ReadLine();//load battle units
		string[] battleUnits = currentLine.Split(","[0]);
		
		for (int i = 0; i < battleUnits.Length; i++) 
		{
			((Stats)stats).battleUnits[i] = int.Parse(battleUnits[i]);
		}

		((UnitProc)unitProc).queList.Clear ();

		while (currentLine != "###Stats###") 
		{
			currentLine = sReader.ReadLine ();
			if(currentLine != "###Stats###")
			{
				string[] unitQue = currentLine.Split(","[0]);

				if (currentLine != "###Stats###") 
				{
					((UnitProc)unitProc).queList.Add(new Vector3(
						float.Parse(unitQue[0]),float.Parse(unitQue[1]),
						float.Parse(unitQue[2])));
				}
			}
		}
		//((UnitProc)unitProc).start = true;
		((UnitProc)unitProc).SortList ();
		((MenuUnit)menuUnit).unitProc = unitProc;
		((MenuUnit)menuUnit).GetUnitsXML ();

		//Stats: experience,dobbits,occupiedDobbit,gold,mana,crystal,,maxStorageGold,maxStroageMana,maxCrystals,forgeRates,generatorRates
		currentLine = sReader.ReadLine ();

		string[] statsTx = currentLine.Split(","[0]);

		((Stats)stats).experience = int.Parse(statsTx[0]);
		((Stats)stats).dobbitNo = int.Parse(statsTx[1]);
		((Stats)stats).occupiedDobbitNo = int.Parse(statsTx[2]);

		((Stats)stats).currentPopulationNo = int.Parse(statsTx[3]);
		((Stats)stats).maxPopulationNo = int.Parse(statsTx[4]);

		((Stats)stats).gold = float.Parse(statsTx[5]);
		((Stats)stats).mana = float.Parse(statsTx[6]);
		((Stats)stats).crystals = int.Parse(statsTx[7]);	
			
		((Stats)stats).maxStorageGold = int.Parse(statsTx[8]);
		((Stats)stats).maxStorageMana = int.Parse(statsTx[9]);
		((Stats)stats).maxCrystals = int.Parse(statsTx[10]);	

		((Stats)stats).productionRates[0] = float.Parse(statsTx[11]);	
		((Stats)stats).productionRates[1] = float.Parse(statsTx[12]);	

		bool tutorialCitySeen = Boolean.Parse(statsTx [13]);//Convert.ToBoolean(statsTx [11]);
		((Stats)stats).tutorialCitySeen = tutorialCitySeen;
		if (tutorialCitySeen) GhostHelper.SetActive (false);

		bool tutorialBattleSeen = Boolean.Parse(statsTx [14]);
		((Stats)stats).tutorialBattleSeen = tutorialBattleSeen;

		bool soundOn= Boolean.Parse(statsTx [15]);
		bool musicOn= Boolean.Parse(statsTx [16]);

		((SoundFX)soundFX).ChangeSound (soundOn);
		((SoundFX)soundFX).ChangeMusic (musicOn);

		((Stats)stats).UpdateUI();

		currentLine = sReader.ReadLine ();
		saveDateTime = DateTime.Parse(currentLine);

		CalculateElapsedTime ();
		sReader.Close();
		((Messenger)statusMsg).DisplayMessage("Game loaded.");
		//print ("current time " + loadDateTime.ToString ());//to check if the time was calculated properly
		//print ("saved time " + saveDateTime.ToString ());
	}



	private void ProcessBuilding(string buildingTag, int buildingIndex)
	{
		GameObject[] selectedBuildingType = GameObject.FindGameObjectsWithTag(buildingTag);

		foreach (GameObject building in selectedBuildingType) 
		{
			if(((BuildingSelector)building.GetComponent("BuildingSelector")).isSelected)//isSelected is true at initialization				
			{
				Component buildingSelector = (BuildingSelector)building.GetComponent("BuildingSelector");

				((BuildingSelector)buildingSelector).buildingIndex = buildingIndex;//unique int to pair buildings and the grass underneath
				((BuildingSelector)buildingSelector).inConstruction = false;
				((BuildingSelector)buildingSelector).isSelected = false;

				LoadedBuildings.Add(building);//the list is sorted and then used to pair the buildings and the grass by index
				//all grass is recorded in the same list, for both buildings and constructions; after they are sorted by index, 
				//the first batch goes to finished buildings, the rest goes to underconstruction
				break;
			}
		}
	}

	private void ProcessGrass(int grassType, int grassIndex)
	{
		GameObject[] selectedGrassType = GameObject.FindGameObjectsWithTag("Grass");

		foreach (GameObject grass in selectedGrassType) 
		{
			if(((GrassSelector)grass.GetComponent("GrassSelector")).isSelected)				
			{
				Component grassSelector = (GrassSelector)grass.GetComponent("GrassSelector");

				((GrassSelector)grassSelector).grassIndex = grassIndex;
				((GrassCollider)grass.GetComponentInChildren<GrassCollider>()).isMoving = false;
				grass.GetComponentInChildren<GrassCollider>().enabled = false;

				((GrassSelector)grassSelector).isSelected = false;

				LoadedGrass.Add(grass);
				break;
			}
		}

	}

	//private IEnumerator  ProcessConstruction(string buildingTag, int buildingIndex, int buildingTime, int remainingTime, int storageIncrease)
	private void ProcessConstruction(string buildingTag, int buildingIndex, int buildingTime, int remainingTime, int storageIncrease)
	{
		GameObject[] selectedBuildingType = GameObject.FindGameObjectsWithTag(buildingTag);
		GameObject[] selectedConstructionType = GameObject.FindGameObjectsWithTag("Construction");

		foreach (GameObject construction in selectedConstructionType) 
		{
			if(((ConstructionSelector)construction.GetComponent("ConstructionSelector")).isSelected)		
			{
				Component constructionSelector = (ConstructionSelector)construction.GetComponent("ConstructionSelector");
				((ConstructionSelector)constructionSelector).buildingType = buildingTag;
				((ConstructionSelector)constructionSelector).constructionIndex = buildingIndex;
				((ConstructionSelector)constructionSelector).buildingTime = buildingTime;

				construction.GetComponentInChildren<UISlider>().value = 1-(float)remainingTime/buildingTime;
				((ConstructionSelector)constructionSelector).remainingTime = remainingTime;

				((ConstructionSelector)constructionSelector).storageIncrease = storageIncrease;
				((ConstructionSelector)constructionSelector).isSelected = false;
				LoadedConstructions.Add(construction);
				break;
			}
		}

		foreach (GameObject building in selectedBuildingType) 
		{
			if(((BuildingSelector)building.GetComponent("BuildingSelector")).isSelected)				
			{	
				Component buildingSelector = (BuildingSelector)building.GetComponent("BuildingSelector");

				((BuildingSelector)buildingSelector).buildingIndex = buildingIndex;
				((BuildingSelector)buildingSelector).isSelected = false;	

				LoadedBuildings.Add(building);
				break;
			}
		}
		//yield return new WaitForSeconds (0.1f);
	}

	private void ParentBuildings()
	{
		LoadedBuildings.Sort (delegate (GameObject g1, GameObject g2) {
			return ((BuildingSelector)g1.GetComponent ("BuildingSelector")).buildingIndex.CompareTo (((BuildingSelector)g2.GetComponent ("BuildingSelector")).buildingIndex);		
				});

		LoadedConstructions.Sort (delegate (GameObject g1, GameObject g2) {
			return ((ConstructionSelector)g1.GetComponent ("ConstructionSelector")).constructionIndex.CompareTo (((ConstructionSelector)g2.GetComponent ("ConstructionSelector")).constructionIndex);		
				});

		LoadedGrass.Sort (delegate (GameObject g1, GameObject g2) {
			return ((GrassSelector)g1.GetComponent ("GrassSelector")).grassIndex.CompareTo (((GrassSelector)g2.GetComponent ("GrassSelector")).grassIndex);		
		});

		int constructionIndex = 0;
		
		for (int i = 0; i < LoadedBuildings.Count; i++) 
		{
			if(!((BuildingSelector)LoadedBuildings[i].GetComponent("BuildingSelector")).inConstruction)
			{
				LoadedGrass[i].transform.parent = LoadedBuildings[i].transform;
				LoadedBuildings[i].transform.parent = GroupBuildings.transform;
			}
			else
			{
				LoadedGrass[i].transform.parent = LoadedConstructions[constructionIndex].transform;
				LoadedBuildings[i].transform.parent = LoadedConstructions[constructionIndex].transform;
				LoadedBuildings[i].SetActive(false);
				LoadedConstructions[constructionIndex].transform.parent = GroupBuildings.transform;
				constructionIndex++;
			}
		}
		((BuildingCreator)buildingCreator).existingBuildings = existingBuildings;
	}

	private void CalculateElapsedTime()
	{
				timeDifference = loadDateTime.Subtract (saveDateTime);

				//everything converted to minutes
				int elapsedTime = timeDifference.Days * 24 * 60 + timeDifference.Hours * 60 + timeDifference.Minutes; 

				//some production buildings have finished a while ago; needed for subsequent production amount
			
				List<int> finishTimesGold = new List<int> (); 
				List<int> finishTimesMana = new List<int> (); 
				finishTimesGold.Clear ();
				finishTimesMana.Clear ();

				GameObject[] constructionsInProgress = GameObject.FindGameObjectsWithTag ("Construction");

				for (int i = 0; i < constructionsInProgress.Length; i++) {
						int buildingTime = ((ConstructionSelector)constructionsInProgress [i].GetComponent ("ConstructionSelector")).buildingTime; 
						int remainingTime = ((ConstructionSelector)constructionsInProgress [i].GetComponent ("ConstructionSelector")).remainingTime;

						Component slider = constructionsInProgress [i].GetComponentInChildren (typeof(UISlider));

						if (elapsedTime >= remainingTime) {
								((UISlider)slider.GetComponent ("UISlider")).value = 1;
								((ConstructionSelector)constructionsInProgress [i].GetComponent ("ConstructionSelector")).progCounter = 1.1f;

								if (((ConstructionSelector)constructionsInProgress [i].GetComponent ("ConstructionSelector")).buildingType == "InstitutionZ3") {
										finishTimesGold.Add (elapsedTime - remainingTime);//add the time passed after the building was finished

								} else if (((ConstructionSelector)constructionsInProgress [i].GetComponent ("ConstructionSelector")).buildingType == "ModernBuildingZ4") {
										finishTimesMana.Add (elapsedTime - remainingTime);//add the time passed after the building was finished
								}
								//print("elapsedTime-remainingTime = " + (elapsedTime-remainingTime).ToString());
						} else {//everything under 1 minute will appear as finished at reload - int approximation, not an error
								((UISlider)slider.GetComponent ("UISlider")).value += (float)elapsedTime / (float)buildingTime;

						}
				}

				//Calculate the progession in unit construction que

		int substractTime = elapsedTime;

		List<Vector3> queList = new List<Vector3> ();	//qIndex, objIndex, trainingIndex

		queList = ((UnitProc)unitProc).queList;
		queList.Sort (delegate (Vector3 v1, Vector3 v2) { return v1.x.CompareTo (v2.x); });	

		const int numberOfUnits = 12;
		int[] trainingTimes = new int[numberOfUnits];
		trainingTimes = ((UnitProc)unitProc).trainingTimes;

		int currentTrainingTime;

		for (int i = 0; i < queList.Count; i++) 
		{
			if (substractTime > 0) 
			{
				currentTrainingTime = trainingTimes [(int)queList [i].y];
				int trainingIndex = (int)queList [i].z;

			while (trainingIndex > 0) 
				{
					if (substractTime > currentTrainingTime) 
					{
						substractTime -= currentTrainingTime;
						((Stats)stats).existingUnits[(int)queList [i].y]++;
						trainingIndex --;
						((UnitProc)unitProc).timeRemaining -= currentTrainingTime;
						queList [i] = new Vector3 (queList [i].x, queList [i].y, trainingIndex);
					} 
					else 					
					{
					((UnitProc)unitProc).currentTrainingTime = currentTrainingTime;
					((UnitProc)unitProc).currentSlidVal += (float)substractTime / (float)currentTrainingTime;

						if(currentTrainingTime - substractTime > 0)
						{
							((UnitProc)unitProc).timeRemaining = currentTrainingTime - substractTime;
						}
						else
						{
							((UnitProc)unitProc).timeRemaining = 1;
						}
										
					queList [i] = new Vector3 (queList [i].x, queList [i].y, trainingIndex);
					substractTime = 0;
					break;
					}
				}
			} 
			else 
			{ break;}
		}

				bool allZero = true;

				for (int i = 0; i < queList.Count; i++) {
						if ((int)queList [i].z != 0) {
								allZero = false;
								break;
						}
				}

				if (allZero) {
					((UnitProc)unitProc).queList.Clear ();
					UnitProc.SetActive(false);

				} else {
						((UnitProc)unitProc).queList = queList;						
						//((UnitProc)unitProc).SortList();//irrelevant - our qlist is already sorted
				}

				//elapsedTime - minutes
				//the existingBuildings array holds accurate finished/unfinished buildings number; substract unfinished 

		if(existingBuildings[4]-finishTimesGold.Count > 0)
		{
			((Stats)stats).gold += (existingBuildings[4]-finishTimesGold.Count) * ((Stats)stats).productionRates[0]*elapsedTime*60;
		}

		if (existingBuildings [5] - finishTimesMana.Count > 0) 
		{
			((Stats)stats).mana += (existingBuildings[5] - finishTimesMana.Count) * ((Stats)stats).productionRates[1]*elapsedTime*60;
		}

		for (int i = 0; i < finishTimesGold.Count; i++) 
		{
			((Stats)stats).gold += finishTimesGold[i]*((Stats)stats).productionRates[0]*60;
		}

		for (int i = 0; i < finishTimesMana.Count; i++) 
		{
			((Stats)stats).mana += finishTimesMana[i]*((Stats)stats).productionRates[1]*60;
		}

		StartCoroutine(LateApplyMaxCaps());//some data reaches stats with latency
	}
	
	private IEnumerator LateApplyMaxCaps()
	{				
		yield return new WaitForSeconds (0.50f);
		((Stats)stats).ApplyMaxCaps ();
		((Stats)stats).UpdateUI();//one-time interface update; called only after changes - once a second because of production
		((Stats)stats).UpdateUnitsNo ();
	}

}


