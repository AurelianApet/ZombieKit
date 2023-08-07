using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuUnitBase : MonoBehaviour {

	public const int 
		maxTypes = 3,						// the 3 sockets for units under construction	
		numberOfUnits = 12;

	public int hours, minutes, seconds,	
		maxUnits = 10,						//maximum number of units of each type that can be trained	
		typesCounter = 0,					//keeps track of different types of units being constructed
		currentTrainingTime = 0;

											//Time.deltaTime = 0.016; 60*Time.deltaTime = 1s ; runs at 60fps
		
	public float 
		progTime = 0.57f, 					//current unit progress bars timer, also correction.57
		progCounter = 0,					//progress counter, reaches construction time, then is reset to 0
		currentSlidVal = 0, 				//current slider value
		currentTimeRemaining,				//seconds+minutes+hours, necessary for establishing hard price
		timeRemaining;						//full

	public int[] 
		trainingTimes = new int[numberOfUnits],				//time to finish training for each unit- values are loaded from XML
		populationWeights = new int[numberOfUnits],			//the unit+support personnel; number of soldiers != population
		trainingIndexes = new int[numberOfUnits];			//how many units are under construction - from 0 to 10 maximum 
			  
	public List<Vector3> queList = new List<Vector3>();		// qIndex, objIndex, trainingIndex

}
