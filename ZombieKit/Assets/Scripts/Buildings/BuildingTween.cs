using UnityEngine;
using System.Collections;

public class BuildingTween : MonoBehaviour {		//briefly scales up and down the spites parent object when the building is selected

	private GameObject sprites;

	public float 
		tweenSpeed = 0.02f,
		size = 1,			
		initSize = 1,  
		maxSize =  1.1f;

	private bool 
		tweenb, 
		scaleUpb;

	void Start () {

		sprites = transform.FindChild ("Sprites").gameObject; 	//find the sprites parent
	}

	public void Tween()
	{
		tweenb = true;
		scaleUpb = true;
	}

	void FixedUpdate()
	{
		if(tweenb)
		{
			if(scaleUpb)
			{
				size+= tweenSpeed;					//scale up
			}
			else
			{
				size-= tweenSpeed; 					//scale back down
			}

			if(size>maxSize) scaleUpb = false; 		//maximum size reached, time to scale down

			else if(size<initSize) 					//reached a size smaller than the initial size
			{
				tweenb = false;						//end the scale sequence 
				size = initSize; 					//reset the size to 1
			}

			sprites.transform.localScale = new Vector3(size,size,1);	//pass the scale values to the sprites parent
		}
	}
}