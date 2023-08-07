using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;
using System.Text;


public class SaveLoadBattle : MonoBehaviour {

	private const int 
		buildingTypesNo = 12,	//the building tags
		grassTypesNo = 3;

	private int 
		constructionZ = 0, 		//normally = 1, same as buildingZ, but creates a zdepth problem for the construction dobbits/soldiers
		buildingZ = 1,			//layer depths for building, correlate with BuildingCreator.cs
		grassZ = 2,				//layer depths for grass, correlate with BuildingCreator.cs
		cannonZ = -5,
		damageZ = -7,
		buildingIndexOverride = -1, //many lists of elements must be mirrored
		grassIndexOvveride = -1;		

	private string 
		filePath,
		fileNameLocal = "LocalSave",
		myMapID = "MyMapID",			//local - saves the map id from server IlXcUovzS6
		battleSaveFile = "BattleMap",
		fileExt = ".txt",
		myUseridFile,//gfdfghjke.txt
		myUseridCode,//gfdfghjke
		battleidCode,
		attackExt = "_results",//_attack
		nullUseridCode = "0000000000";//the server avoids loading own map; if there is no own map, an error is displayed in console

	public string //http://citybuildingkit.com/costin/admin.php
		serverAddress = "http://citybuildingkit.com/",//"http://inosfera.orgfree.com/",
		filesAddress = "get_match.php",//get_match.php?license=U125UNITYDEMO
		matchAddress = "finish_match.php",
		license = "U125UNITYDEMO";//"costin/finish_match.php"

	private WWW w2;

	public GameObject[] 
		BuildingPrefabs = new GameObject[buildingTypesNo],
		GrassPrefabs = new GameObject[grassTypesNo];//three types of grass patches

	private int[] existingBuildings = new int[buildingTypesNo];//the entire array is transfered to BuildingCreator.cs; 
	//records how many buildings of each type, when they are built/game is loaded

	private GameObject[] Grass, Construction;

	public GameObject BuildingCannon, ConstructionPrefab, DamageBar, GoHomeBt;

	private GameObject GroupBuildings, GroupCannons, GroupDamageBars;//objects used to parent the buildings/cannons , once they are instantiated

	//lists for these elements - unknown number of elements
	private List<GameObject> 
		LoadedBuildings = new List<GameObject>(),
		LoadedConstructions = new List<GameObject>(),
		LoadedGrass = new List<GameObject>(),//list is used to parent the grass to the buildings
		LoadedCannons = new List<GameObject>(), //list is used to parent the cannons to the cannon group
		LoadedDamageBars = new List<GameObject>();//list is used to parent the damagebars to the damagebar group

	private DateTime saveDateTime, loadDateTime;//saveTime, currentTime
	private TimeSpan timeDifference;

	private bool oneLoad = true;
	private Component buildingCreator, statsBattle, statusMsg, soundFX;

	// Use this for initialization
	void Start () 
	{
		GroupBuildings = GameObject.Find ("GroupBuildings");
		GroupCannons = GameObject.Find ("GroupCannons");
		GroupDamageBars = GameObject.Find ("GroupDamageBars");
		buildingCreator = GameObject.Find ("BuildingCreator").GetComponent<BuildingCreator> ();
		statsBattle = GameObject.Find ("StatsBattle").GetComponent<StatsBattle> ();
		statusMsg = GameObject.Find ("StatusMsg").GetComponent<Messenger> ();
		soundFX = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();

		//filePath = Application.dataPath + "/";//windows - same folder as the project
		filePath = Application.persistentDataPath +"/";//other platforms

		loadDateTime = System.DateTime.Now;//current time

		LoadGame(); //normal, server random load
	}

	private bool CheckServerSaveFile()//LOCAL recording of a previous save on server
	{
		bool serverSaveExists = File.Exists(filePath + myMapID + fileExt);
		return(serverSaveExists);
		//checks if the mapcode was saved locally, not if it is still available on server
	}

	public void ReadUserid()
	{
		StreamReader sReader = new StreamReader(filePath + myMapID + fileExt);
		myUseridCode = sReader.ReadLine();
	}

	private void SaveBattleMap()//saves a copy of the server map locally
	{
		StreamWriter sWriter = new StreamWriter (filePath + battleSaveFile + fileExt);
		sWriter.Write(w2.text);
		sWriter.Flush ();
		sWriter.Close ();
		LoadGameMap();
	}

	public void SaveAttack()
	{
		StartCoroutine("SendAttackToServer");

		//optional - write file locally
		/*
		StreamWriter sWriter = new StreamWriter (filePath + battleResultSaveFile + fileExt);

		sWriter.WriteLine ("###StartofFile###");
		//the battle results file must pass the losses to the attacked user id
		sWriter.WriteLine(
							((StatsBattle)StatsBattleSc).gold.ToString()+","+
		                  	((StatsBattle)StatsBattleSc).mana.ToString()
		                  );

		sWriter.WriteLine ("###EndofFile###");

		sWriter.Flush ();
		sWriter.Close ();
		StartCoroutine("SendResultsToServer");
		*/

	}

	IEnumerator SendAttackToServer()   
	{
		((Messenger)statusMsg).DisplayMessage("Uploading battle results.");
		//byte[] levelData = System.IO.File.ReadAllBytes(filePath + battleResultSaveFile + fileExt);//full local save file
		byte[] levelData = Encoding.ASCII.GetBytes("###StartofFile###\n" +// gold,mana,buildingsDestroyed,unitsLost
		                                           ((StatsBattle)statsBattle).gold.ToString () + "," +
		                                           ((StatsBattle)statsBattle).mana.ToString ()+  "," +
		                                           ((StatsBattle)statsBattle).buildingsDestroyed.ToString ()+  "," +
		                                           ((StatsBattle)statsBattle).unitsLost.ToString ()+
		                                           "\n###EndofFile###");
				
		WWWForm form = new WWWForm();

		form.AddField("savefile","file");
		
		form.AddBinaryData( "savefile", levelData, battleidCode+attackExt,"text/xml");//file

		//change the url to the url of the php file
		WWW w = new WWW(serverAddress + filesAddress +"?mapid=" + battleidCode+attackExt +"&license="+license, form);//myUseridFile 
			
		yield return w;
		if (w.error != null)
		{
			print("error");
			print ( w.error ); 
			((Messenger)statusMsg).DisplayMessage("Network error.");
		}
		else
		{
			//this part validates the upload, by waiting 5 seconds then trying to retrieve it from the web
			if(w.uploadProgress == 1 && w.isDone)
			{
				//print ( "Sent File " + myUseridCode + " Contents are: \n\n" + w.text);
				
				yield return new WaitForSeconds(5);
				//change the url to the url of the folder you want it the levels to be stored, the one you specified in the php file
				WWW w2 = new WWW(serverAddress + filesAddress +"?get_user_map=1&mapid=" + battleidCode+attackExt+"&license="+license);//returns a specific map
				
				yield return w2;
				if(w2.error != null)
				{
					print("error 2");
					print ( w2.error ); 
					((Messenger)statusMsg).DisplayMessage("Attack file check error.");
				}
				else
				{
					//then if the retrieval was successful, validate its content to ensure the level file integrity is intact
					if(w2.text != null && w2.text != "")
					{
						if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
						{
							//and finally announce that everything went well
							print ( "Received File " + battleidCode + attackExt + " Contents are: \n\n" + w2.text);//file
							((Messenger)statusMsg).DisplayMessage("Uploaded attack results to "  + battleidCode + attackExt );
						}
						else
						{
							print ( "Level File " + battleidCode+attackExt + " is Invalid");//file
							print ( "Response File " + battleidCode+attackExt + " Contents are: \n\n" + w2.text);//file although incorrect, prints the content of the retrieved file
							((Messenger)statusMsg).DisplayMessage("Uploaded attack failed.");
						}
					}
					else
					{
						print ( "Level File " + battleidCode+attackExt + " is Empty");//file
						((Messenger)statusMsg).DisplayMessage("Attack file is empty?");
					}
				}

			}     
		}
		GoHomeBt.SetActive(true);//display go home button regardles of upload success, so the user can go back to hometown/exit the game
	}


	public void LoadRandomFromServer()
	{
		StartCoroutine("DownloadRandomMap");//force the local level save before this
		((Messenger)statusMsg).DisplayMessage("Downloading random map.");
	}

	IEnumerator DownloadRandomMap()   
	{
		if(CheckServerSaveFile())
			ReadUserid();
		else 
			myUseridCode = nullUseridCode;

		//loads a map other than the user map; if there is only one map on the server - your own, loads your own map
		w2 = new WWW(serverAddress + filesAddress+"?get_random_map=1&mapid=" + myUseridCode+"&license="+license); //mapid with the get_random_map to prevent the user's map from being downloaded by accident

		yield return w2;
		
		if(w2.error != null)
		{
			print("Server load error" + w2.error);
			((Messenger)statusMsg).DisplayMessage("Map download error.");
		}
		
		else
		{
			//then if the retrieval was successful, validate its content to ensure the level file integrity is intact
			if(w2.text != null && w2.text != "")
			{
				if(w2.text.Contains("###StartofFile###") && w2.text.Contains("###EndofFile###"))
				{
					print ( "Random File "+ battleidCode + " Contents are: \n\n" + w2.text);
					SaveBattleMap();
					((Messenger)statusMsg).DisplayMessage("Map downloaded.");
				}
				
				else
				{			
					print ( "Random Level File is Invalid. Contents are: \n\n" + w2.text);
					//although incorrect, prints the content of the retrieved file
					((Messenger)statusMsg).DisplayMessage("Downloaded file corrupted.\n" +
					"If you have your own save file on server");
					((Messenger)statusMsg).DisplayMessage("delete local files or change your 10 character ID in:\n" +
					    "C:/Users/user/AppData/LocalLow/\n" +
					    "AStarterKits/StrategyKit/MyMapID.txt");
				}
			}
			else
			{
				print ( "Random Level File is Empty");
				((Messenger)statusMsg).DisplayMessage("Downloaded file empty?");
			}

		}
	}

	public void LoadGame()
	{
		if(!oneLoad) {return;}//prevents loading twice, since there are no safeties and the procedure should be automated at startup, not button triggered
		oneLoad = false;
		
		//StreamReader sReader = new StreamReader(filePath + fileName + fileExt);//load local file instead of server, same format
		
		LoadRandomFromServer();
	}


	public void LoadGameMap()
	{

		StreamReader sReader = new StreamReader(filePath + battleSaveFile + fileExt);//for loading random server map

		LoadedBuildings.Clear ();
		LoadedConstructions.Clear ();
		LoadedGrass.Clear ();
		LoadedCannons.Clear ();
		LoadedDamageBars.Clear ();

		/*

		//The random map returned from server includes the mapid  - this header:

		###StartofMapid###
		NJDCdFqQnM
		###EndofMapid###
		###StartofFile###
		###Buildings###
		InstitutionZ2,4,-896,-452.5
		InstitutionZ2,5,-512,-724
		InstitutionZ2,6,-128,-995.5
		
		*/

		string currentLine = "";

		currentLine = sReader.ReadLine ();//skip first line - ###StartofMapid###  
		battleidCode = sReader.ReadLine ();//read the mapid then skip ahead

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
			//"HauntedPumpkin","HouseBlueZ1","HouseBrownZ1","InstitutionZ2","InstitutionZ3","ModernBuildingZ4","ModernBuildingZ5","ModernTowerZ1","Store","TowerBlockZ1","WaterTowerZ1","WaterTowerZ2"
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
				//	((Stats)statsSc).productionBuildings[0]++;//sends the InstitutionZ3 to the production buildings array in stats; used at 1 second to produce resources
				existingBuildings[4]++;
				break;
			
			case "ModernBuildingZ4":
			GameObject ModernBuildingZ4 = (GameObject)Instantiate(BuildingPrefabs[5], new Vector3(posX,posY,buildingZ), Quaternion.identity);	
				//	((Stats)statsSc).productionBuildings[1]++;//sends the ModernBuildingZ4 to the production buildings array in stats; used at 1 second to produce resources
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

				//instantiates cannons with grass
				GameObject GrassCannon = (GameObject)Instantiate(BuildingCannon, new Vector3(posX,posY,cannonZ), Quaternion.identity);
				GameObject GrassDamageBar = (GameObject)Instantiate(DamageBar, new Vector3(posX,posY-200,damageZ), Quaternion.identity);

				switch (grassType) 
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


				GameObject Construction = (GameObject)Instantiate(ConstructionPrefab, new Vector3(posX,posY,constructionZ), Quaternion.identity);

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
			}
		}

		ParentBuildings ();

		currentLine = sReader.ReadLine();
		((BuildingCreator)buildingCreator).buildingIndex = int.Parse(currentLine);

		//UNITS
		/*
		currentLine = sReader.ReadLine();//#Add verification for empty que
		currentLine = sReader.ReadLine();//skip load training times
		currentLine = sReader.ReadLine();//skip load population weights
		currentLine = sReader.ReadLine();//skip load existing units
		currentLine = sReader.ReadLine();//skip load battle units
		*/
		while (currentLine != "###Stats###") 
		{
			currentLine = sReader.ReadLine ();
		
		}
			
		//Stats: experience,dobbits,occupiedDobbit,gold,mana,crystal,maxStorageGold,maxStroageMana,maxCrystals,forgeRates,generatorRates,tutorialSeen,soundOn,musicOn
		currentLine = sReader.ReadLine ();
		currentLine = sReader.ReadLine ();
		saveDateTime = DateTime.Parse(currentLine);

		CalculateElapsedTime ();
		sReader.Close();
		//print ("current time " + loadDateTime.ToString ());
		//print ("saved time " + saveDateTime.ToString ());

		GameObject tempHelios = GameObject.Find ("Helios");

		tempHelios.GetComponent<Helios>().Buildings = LoadedBuildings;
		tempHelios.GetComponent<Helios> ().Cannons = LoadedCannons;
		tempHelios.GetComponent<Helios> ().DamageBars = LoadedDamageBars;
		GameObject tempGridManager = GameObject.Find ("GridManager");
		tempGridManager.GetComponent<GridManager>().UpdateObstacles();
		tempHelios.GetComponent<Helios>().NetworkLoadReady();
		((SoundFX)soundFX).BattleMapSpecific ();//an invokerepeating for reducing the number of simultaneous weapon sounds
	}

	private void ProcessBuilding(string buildingTag, int buildingIndex)//int buildingValue, bool goldBased
	{
		GameObject[] selectedBuildingType = GameObject.FindGameObjectsWithTag(buildingTag);

		foreach (GameObject building in selectedBuildingType) 
		{
			if(((BuildingSelector)building.GetComponent("BuildingSelector")).isSelected)//isSelected is true at initialization				
			{
				Component buildingSelector = (BuildingSelector)building.GetComponent("BuildingSelector");

				((BuildingSelector)buildingSelector).buildingIndex = buildingIndex;//unique int to pair buildings and the grass underneath
				((BuildingSelector)buildingSelector).inConstruction = false;
				((BuildingSelector)buildingSelector).battleMap = true;
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
				((GrassSelector)grass.GetComponent("GrassSelector")).grassIndex = grassIndex;
				((GrassCollider)grass.GetComponentInChildren<GrassCollider>()).isMoving = false;
				grass.GetComponentInChildren<GrassCollider>().enabled = false;

				((GrassSelector)grass.GetComponent("GrassSelector")).isSelected = false;

				LoadedGrass.Add(grass);
				break;
			}
		}
		ProcessCannon (grassIndex);
		ProcessDamageBar (grassIndex);
	}

	private void ProcessCannon(int grassIndex)
	{
		GameObject[] selectedCannon = GameObject.FindGameObjectsWithTag("BuildingCannon");
		foreach (GameObject cannon in selectedCannon) 
		{
			if (((Selector)cannon.GetComponent ("Selector")).isSelected) //recurrent error? delayed load?
			{
				((Selector)cannon.GetComponent ("Selector")).index = grassIndex;
				((Selector)cannon.GetComponent ("Selector")).isSelected = false;
				LoadedCannons.Add(cannon);
				break;
			}
		}
	}

	private void ProcessDamageBar(int grassIndex)
	{
		GameObject[] selectedDamageBar = GameObject.FindGameObjectsWithTag("DamageBar");
		foreach (GameObject damageBar in selectedDamageBar) 
		{
			if (((Selector)damageBar.GetComponent ("Selector")).isSelected) //recurrent error? delayed load?
			{
				((Selector)damageBar.GetComponent ("Selector")).index = grassIndex;
				((Selector)damageBar.GetComponent ("Selector")).isSelected = false;
				LoadedDamageBars.Add(damageBar);
				break;
			}
		}
	}

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
				((ConstructionSelector)constructionSelector).battleMap = true;
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
			LoadedCannons[i].transform.parent = GroupCannons.transform;// since the lists for grass and cannons are identical, no need to sort the cannons
			LoadedDamageBars[i].transform.parent = GroupDamageBars.transform;

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
				//int[] unfinishedProductionBuildings = new int[2];
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
								constructionsInProgress [i].transform.Find("Progress Bar").gameObject.SetActive(false);//deactivates the progress barr/finish button
								//constructionsInProgress [i].transform.Find("TimeCounterLb").gameObject.SetActive(false);//you can deactivate the label, 
																														//but for now it is usefull to see if time was calculated correctly 
						}
				}			

	}
}


