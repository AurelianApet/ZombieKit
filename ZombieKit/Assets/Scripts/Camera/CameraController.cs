using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CameraController : MonoBehaviour {

	[HideInInspector]
	public bool movingBuilding = false;

	private bool 
		moveNb, 					//bools for button controlled moving Move North, East, South, West
		moveEb, 
		moveSb, 
		moveWb, 
		zoomInb, 
		zoomOutb,

		fadeMoveNb, 					//bools for fade out moving Move North, East, South, West
		fadeMoveSb, 
		fadeMoveEb, 
		fadeMoveWb, 
		fadeZoomInb, 
		fadeZoomOutb,

		fade = false,
		fadePan = false,
		fadeZoom = false,

		fadeTouch = false,
		fadeTouchPan = false,
		fadeTouchZoom = false,
		stillMovingTouch = false; 
		

	private float
		lastPanX,
		lastPanY,
		zoomMax = 1,						//caps for zoom
		zoomMin = 0.55f,
		zoom, 								// the current camera zoom factor
		currentFingerDistance,
		previousFingerDistance,
		camStepDefault,
		zoomStepDefault;

	public float
		touchPanSpeed = 4.0f,				
		camStep = 15,
		zoomStep = 0.01f,					

		camStepFade = 0.3f,
		zoomStepFade = 0.00005f;
			 							

	private Vector3 
		initPos = new Vector3(0,0,-10),
		RayHitPoint;
		
	private Vector2 
		velocity, 
		target;

	[HideInInspector]
	public int 								//camera bounds
		northMax = 3200,		
		southMin = -3300,  		
		eastMax = 4200,			
		westMin = -4200;		

	private Component relay;

	void Start () 
	{
		relay = GameObject.Find ("Relay").GetComponent<Relay>();
		camStepDefault = camStep;
		zoomStepDefault = zoomStep;
	}

	//to prevent selecting the buildint underneath the move buttons

	private void Delay() { ((Relay)relay).DelayInput(); }

	//Move
	public void MoveN()	{ moveSb = false; if(!moveNb) moveNb = true; else StopMove(); Delay (); }	//FadeOutPan(); 
	public void MoveE()	{ moveWb = false; if(!moveEb) moveEb = true; else StopMove(); Delay (); }
	public void MoveS()	{ moveNb = false; if(!moveSb) moveSb = true; else StopMove(); Delay (); }
	public void MoveW()	{ moveEb = false; if(!moveWb) moveWb = true; else StopMove(); Delay (); }

	//FadeMove
	private void FadeMoveN() { moveSb = false; moveNb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveS() { moveNb = false; moveSb = true; fade = true; fadePan = true; camStep = camStepDefault;}	
	private void FadeMoveE() { moveWb = false; moveEb = true; fade = true; fadePan = true; camStep = camStepDefault;}	 
	private void FadeMoveW() { moveEb = false; moveWb = true; fade = true; fadePan = true; camStep = camStepDefault;}   

	//Zoom
	public void ZoomIn() { zoomOutb = false; if(!zoomInb) zoomInb = true; else StopZoom(); Delay (); }//FadeOutZoom();
	public void ZoomOut() { zoomInb = false; if(!zoomOutb) zoomOutb = true;	else StopZoom(); Delay (); }

	//FadeZoom
	public void FadeZoomIn()  { zoomOutb = false; zoomInb = true;  fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }	
	public void FadeZoomOut() { zoomInb = false;  zoomOutb = true; fade = true; fadeZoom = true; zoomStep = zoomStepDefault; }

	// conditions keep the camera from going off-map, leaving a margin for zoom-in/out

	private void MoveCam(float speedX,float speedY){transform.position += new Vector3 (speedX, speedY, 0);}
		
	void Update()
	{	
		if(fade)
		{
			if(fadePan)
			{
				camStep-= camStepFade;
				if(camStep<=0)
				{
					StopMove();
					camStep = camStepDefault;
					fadePan = false;
				}
			}
			if(fadeZoom)
			{
				zoomStep-= zoomStepFade;
				if(zoomStep<=0.008)//0.002
				{
					StopZoom();
					zoomStep = zoomStepDefault;
					fadeZoom = false;
				}
			}

			if(!fadePan&&!fadeZoom)
				fade = false;
		}

		if(fadeTouch)
		{
			if(!stillMovingTouch)
			{
				lastPanX*=0.9f;
				lastPanY*=0.9f;
			}

			transform.Translate(lastPanX, lastPanY, 0);		// N
			
			if(Math.Abs(lastPanX)<0.1f||
			  (lastPanX<0 && transform.position.x < westMin)||
			  (lastPanX>0 && transform.position.x > eastMax))
			  lastPanX=0;

			if(Math.Abs(lastPanY)<0.1f||
			  (lastPanY>0 && transform.position.y > northMax)||
			  (lastPanY<0 && transform.position.y < southMin))
			  lastPanY=0;
			
			if(lastPanX==0&&lastPanY==0)
			{
				fadeTouch = false;
			}

			stillMovingTouch = false;			
		}

		TouchMoveZoom ();
		ButtonMoveZoom ();	
	}

	private void ButtonMoveZoom()
	{
		//NSEW distance bounds

		if(moveNb && transform.position.y < northMax){MoveCam(0,camStep); ResetFadePan(); fadeMoveNb = true;} 		// N
		else if(moveSb && transform.position.y > southMin){MoveCam(0,-camStep); ResetFadePan(); fadeMoveSb = true;}	// S
				
		if (moveEb && transform.position.x < eastMax){MoveCam(camStep,0); ResetFadePan(); fadeMoveEb = true;} 		// E
		else if (moveWb && transform.position.x > westMin){MoveCam(-camStep,0); ResetFadePan();fadeMoveWb = true;}	// W

		zoom = ((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor;		

		//zoom in/out

		if(zoomInb && zoom<zoomMax)
		{
			((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor += zoomStep;   //In
			fadeZoomOutb = false; fadeZoomInb = true; 
		}
		else if(zoomOutb && zoom>zoomMin)
		{
			((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor -= zoomStep;	//Out
			fadeZoomInb = false; fadeZoomOutb = true;
		}
	}

	private void TouchMoveZoom()
	{
		zoom = ((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor;

		if (Input.touchCount > 1 && Input.GetTouch(0).phase == TouchPhase.Moved//chech for 2 fingers on screen
				&& Input.GetTouch(1).phase == TouchPhase.Moved) 
		{
			if(!((Relay)relay).delay) Delay ();
			Vector2 touchPosition0 = Input.GetTouch(0).position;//positions for both fingers for pinch zoom in/out
			Vector2 touchPosition1 = Input.GetTouch(1).position;
				
			currentFingerDistance = Vector2.Distance(touchPosition0,touchPosition1);//distance between fingers
				
			//AUTO ZOOM - stopped with tap and brief hold
			/*
			if (currentFingerDistance>previousFingerDistance && zoom<zoomMax)
			{
				if(!ZoomInb)
					{
						ZoomOutb = false;
						ZoomInb = true;
					}
			}
			else if(zoom>zoomMin)
			{
				if(!ZoomOutb)
				{
					ZoomInb = false;
					ZoomOutb = true;			 
				}
			}
			*/

			//MANUAL ZOOM

			if (currentFingerDistance>previousFingerDistance)
			{
				if(zoom<zoomMax)
				{
					((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor += zoomStep;
					fadeZoomOutb = false; fadeZoomInb = true; FadeZoomIn();
				}
			}

			else if(currentFingerDistance<previousFingerDistance && zoom>zoomMin)
			{
				((tk2dCamera)this.GetComponent("tk2dCamera")).ZoomFactor -= zoomStep;
				fadeZoomInb = false; fadeZoomOutb = true; FadeZoomOut(); 
			}

			previousFingerDistance = currentFingerDistance;
		
			}
			
			//else, if one finger on screen - scroll
		else if (!movingBuilding && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) //drag
		{
			if(!((Relay)relay).delay) Delay ();//prevents selecting the buildings underneath 
            Vector2 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            
			//MANUAL MOVE

			float scrollCorrection = 1/ Math.Abs(zoom);

			if(touchDeltaPosition.y < 0)
			{
				if( transform.position.y < northMax)//MoveN
				{
					lastPanY = -touchDeltaPosition.y*touchPanSpeed*scrollCorrection;
				}
			}			
			else if( transform.position.y > southMin)//MoveS
			{
				lastPanY = -touchDeltaPosition.y*touchPanSpeed*scrollCorrection;
			}

			if(touchDeltaPosition.x > 0)
			{
				if( transform.position.x > westMin)//MoveE
				{
					lastPanX = -touchDeltaPosition.x*touchPanSpeed*scrollCorrection;
				}
			}
			else if( transform.position.x < eastMax)//MoveE
			{
				lastPanX = -touchDeltaPosition.x*touchPanSpeed*scrollCorrection;
			}

			stillMovingTouch = true;
			fadeTouch = true;
		}
		else if (Input.touchCount ==1 && Input.GetTouch(0).phase == TouchPhase.Stationary)// 
		{
			StopAll ();		
		}
	}

	private void ResetFadePan()
	{
		fadeMoveNb=false;
		fadeMoveSb=false;
		fadeMoveEb=false;
		fadeMoveWb=false;
	}

	private void FadeOutPan()
	{
		if(fadeMoveEb){FadeMoveE();fadeMoveEb=false;}
		else if(fadeMoveWb){FadeMoveW();fadeMoveWb=false;}

		if(fadeMoveNb){FadeMoveN();fadeMoveNb=false;}
		else if(fadeMoveSb){FadeMoveS();fadeMoveSb=false;}	
	}

	private void FadeOutPanTouch()
	{
		fadeTouch = true;
	}

	private void FadeOutZoom()
	{
		if(fadeZoomInb){FadeZoomIn();fadeZoomInb=false;} 
		else if(fadeZoomOutb){FadeZoomOut();fadeZoomOutb=false;}
	}

	private void StopAll()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
		zoomInb = false; zoomOutb = false;	
		fadeTouch = false;
	}

	private void StopMove()
	{
		moveNb=false; moveSb=false; moveEb=false; moveWb=false;
	}
	private void StopZoom()
	{
		zoomInb = false; zoomOutb = false;	
	}


								//          Fade Functions           \\
	
	//Developer section - testing
	/*
	void OnGUI()
	{
		if(GUI.Button(new Rect(15,Screen.height/2 -75,45,25),"Left"))
		{
			Delay ();
			if(!fadePan)
			{
				moveWb = true;
				ResetFadePan();
				fadeMoveWb = true;
				FadeMoveW();
			}
		}
		if(GUI.Button(new Rect(15,Screen.height/2 -45,45,25),"Right"))
		{
			Delay ();
			if(!fadePan)
			{
				moveEb = true;
				ResetFadePan();
				fadeMoveEb = true;
				FadeMoveE();
			}
		}
		if(GUI.Button(new Rect(15,Screen.height/2 -15,45,25),"Up"))
		{
			Delay ();
			if(!fadePan)
			{
				moveNb = true;
				ResetFadePan();
				fadeMoveNb = true;
				FadeMoveN();
			}
		}
		if(GUI.Button(new Rect(15,Screen.height/2 +15,45,25),"Down"))
		{
			Delay ();
			if(!fadePan)
			{
				moveSb = true;			
				ResetFadePan();
				fadeMoveSb = true;
				FadeMoveS();
			}
		}
		if(GUI.Button(new Rect(15,Screen.height/2 +45,45,25),"Zin"))
		{
			Delay ();
			if(!fadeZoom)
			{
				zoomOutb = false; 
				zoomInb = true;

				fadeZoomOutb = false;
				fadeZoomInb = true;

				FadeZoomIn();
			}
			
		}
		if(GUI.Button(new Rect(15,Screen.height/2 +75,45,25),"Zout"))
		{
			Delay ();
			if(!fadeZoom)
			{
				zoomInb = false; 
				zoomOutb = true;

				fadeZoomInb = false;
				fadeZoomOutb = true;

				FadeZoomOut();
			}
		}
	}
	*/

}