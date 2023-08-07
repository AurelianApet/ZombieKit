using UnityEngine;
using System.Collections;

public class BuildingSelector : MonoBehaviour {//attached to each building as an invisible 2dtoolkit button
	
	public bool 
		isSelected = true,
		inConstruction = true,//only for load/save
		goldBased,
		battleMap = false;

	public int 
		buildingIndex = -1,	
		resourceValue;

	public string buildingType;

	private Component buildingCreator, relay, helios, soundFX, tween;

	// Use this for initialization
	void Start () {
		tween = GetComponent<BuildingTween> ();
		soundFX = GameObject.Find("SoundFX").GetComponent<SoundFX>();
		relay = GameObject.Find("Relay").GetComponent<Relay>();

		buildingCreator = GameObject.Find("BuildingCreator").GetComponent<BuildingCreator>();

		if (battleMap) 
		{				
			helios =  GameObject.Find("Helios").GetComponent<Helios>();
		}
	}

	public void ReSelect()
	{
		if(((Relay)relay).delay||((Relay)relay).pauseInput) return;

		((BuildingTween)tween).Tween();
		((SoundFX)soundFX).Click();

		if(!battleMap)
		{
		
		if(!((BuildingCreator)buildingCreator).isReselect &&
			!((Relay)relay).pauseInput)
		{
			isSelected = true;
			
			((BuildingCreator)buildingCreator).isReselect = true;

			switch (buildingType) //sends the reselect commands to BuildingCreator
			{
				case "HauntedPumpkin":
				((BuildingCreator)buildingCreator).OnReselect0();
				break;
				
				case "HouseBlueZ1":
				((BuildingCreator)buildingCreator).OnReselect1();
				break;
				
				case "HouseBrownZ1":
				((BuildingCreator)buildingCreator).OnReselect2();
				break;
				
				case "InstitutionZ2":
				((BuildingCreator)buildingCreator).OnReselect3();
				break;
				
				case "InstitutionZ3":
				((BuildingCreator)buildingCreator).OnReselect4();
				break;
				
				case "ModernBuildingZ4":
				((BuildingCreator)buildingCreator).OnReselect5();
				break;
		
				case "ModernBuildingZ5":
				((BuildingCreator)buildingCreator).OnReselect6();
				break;
				
				case "ModernTowerZ1":
				((BuildingCreator)buildingCreator).OnReselect7();
				break;			
				
				case "Store":
				((BuildingCreator)buildingCreator).OnReselect8();
				break;
				
				case "TowerBlockZ1":
				((BuildingCreator)buildingCreator).OnReselect9();
				break;
				
				case "WaterTowerZ1":
				((BuildingCreator)buildingCreator).OnReselect10();
				break;

				case "WaterTowerZ2":
				((BuildingCreator)buildingCreator).OnReselect11();
				break;
			}
		}
		}
		else //the target select on the battle map
		{
			((Helios)helios).selectedBuildingIndex = buildingIndex;
			if(((Helios)helios).DeployedUnits.Count == 0)return; //ignore if there are no units deployed
	
			int assignedToGroup = -1;
			bool userSelect = false;  //auto or user target select

			for (int i = 0; i <= ((Helios)helios).instantiationGroupIndex; i++) //((BattleProc)battleProcSc).userSelect.Length
			{			
				if(((Helios)helios).userSelect[i])
				{
					assignedToGroup = i;
					((Helios)helios).userSelect[i] = false;
					userSelect = true;
					break;
				}
			}

			if(!userSelect)
			{
				assignedToGroup = ((Helios)helios).FindNearestGroup(transform.position);//designate a group to attack this building
			}

			if(assignedToGroup == -1) return;

			if(((Helios)helios).targetBuildingIndex[assignedToGroup] != buildingIndex)//if this building is not already the target of the designated group
			{
				switch (assignedToGroup) 
				{
				case 0:
					((Helios)helios).Select0();
					break;

				case 1:
					((Helios)helios).Select1();
					break;

				case 2:
					((Helios)helios).Select2();
					break;

				case 3:
					((Helios)helios).Select3();
					break;
				}

				((Helios)helios).targetBuildingIndex[assignedToGroup] = buildingIndex;	//pass relevant info to BattleProc for this new target building		
				((Helios)helios).targetCenter[assignedToGroup] = transform.position;
				((Helios)helios).FindSpecificBuilding();
				((Helios)helios).updateTarget[assignedToGroup] = true;
				((Helios)helios).pauseAttack[assignedToGroup] = true;
			}

		}
	}

}
