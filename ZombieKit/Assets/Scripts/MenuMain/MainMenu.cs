using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {											//manages major interface elements and panels
	
	private const int 
		noIFElements = 15,
		noPanels = 9;

	public GameObject[] 
		InterfaceElements = new GameObject[noIFElements],						//small interface elements to be deactivated when a panel is opened
		Panels = new GameObject[noPanels];										//shop options competition invite purchase upgrade units damage

	public GameObject UnitsPanelObj, ConfirmationPanelObj, StatsPadObj;			//GameObject.Find only retrieves active objects; it will not find a disabled panel

	public bool constructionGreenlit = true;

	private Component relay, unitsPanel, buildingCreator;

	void Start () {

		relay = GameObject.Find ("Relay").GetComponent<Relay> ();
		buildingCreator = GameObject.Find ("BuildingCreator").GetComponent<BuildingCreator>();
		unitsPanel = UnitsPanelObj.GetComponent<UnitsPanel> ();

	}

	private void Delay()//brief delay to prevent selecting the buildint underneath menus if button positions overlap - the "double command" 
	{
		((Relay)relay).DelayInput();
	}

	public void OnShop(){OnActivateButton (0);}
	public void OnCloseShop(){OnDeactivateButton (0);}
	public void OnCloseShopToBuild()
	{
		if (constructionGreenlit) 
		{
			Delay();
			Panels [0].SetActive (false);
			ActivateInterface ();
		}
	}

	public void OnOptions(){OnActivateButton (1);}
	public void OnCloseOptions(){OnDeactivateButton (1);}

	public void OnCompetition(){OnActivateButton (2);}
	public void OnCloseCompetition(){OnDeactivateButton (2);}

	public void OnInvite(){OnActivateButton (3);}
	public void OnCloseInvite(){OnDeactivateButton (3);}

	public void OnPurchase(){OnActivateButton (4);}
	public void OnClosePurchase(){OnDeactivateButton (4);}

	public void OnUpgrade(){OnActivateButton (5);}
	public void OnCloseUpgrade(){OnDeactivateButton (5);}

	public void OnUnits()
	{
		if(!((Relay)relay).pauseInput) 
		{
			OnActivateButton (6);
			((UnitsPanel)unitsPanel).UpdateStats();
		}
		
	}
	public void OnCloseUnits(){OnDeactivateButton (6);}

	public void OnDamage(){OnActivateButton (7);}
	public void OnCloseDamage(){OnDeactivateButton (7);}

	public void OnConfirmation()								//the destroy building confirmation
	{ 
		((Relay)relay).pauseMovement = true;
		ConfirmationPanelObj.SetActive (true); 
		StatsPadObj.SetActive (false);
	}
	public void OnCloseConfirmation() 
	{ 
		((Relay)relay).pauseMovement = false;
		ConfirmationPanelObj.SetActive (false); 
		Delay();
	}

	public void OnDestroyBuilding()
	{
		((BuildingCreator)buildingCreator).Cancel();
		ConfirmationPanelObj.SetActive (false);//Delay();		//delay deferred to buildingCreator
	}
	public void OnCancelDestroyBuilding()
	{
		((BuildingCreator)buildingCreator).OK();
		ConfirmationPanelObj.SetActive (false);//Delay(); 
	}

	void OnActivateButton(int panelno)
	{		
		bool pauseInput = ((Relay)relay).pauseInput;
		
		if (!pauseInput) 
		{
			Panels [panelno].SetActive (true);
			((Relay)relay).pauseInput = true;
			for (int i = 1; i < InterfaceElements.Length; i++) //skip the upgrade button
			{
				InterfaceElements [i].SetActive (false);
			}
		} 
	}

	void OnDeactivateButton(int scrno)
	{	
		((Relay)relay).pauseInput = false;
		Delay();
		Panels[scrno].SetActive(false);
		ActivateInterface();
	}

	private void ActivateInterface()
	{
		for (int i = 1; i < InterfaceElements.Length; i++) 
		{
			//if(i!=9)//to disable navigation buttons
			if(i!=4)//invite button remains deactivated
			InterfaceElements[i].SetActive(true);
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}	
}
