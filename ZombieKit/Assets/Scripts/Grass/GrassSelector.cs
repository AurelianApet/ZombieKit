using UnityEngine;
using System.Collections;

public class GrassSelector : MonoBehaviour {
	
	public bool 
		isSelected = true,
	 	inCollision = false,
		isDestroyed = false;//the building on top has already been destroyed

	public int 
		grassIndex = -1,	
		grassType = 0;
			
	public void ReSelect()
	{
		isSelected = true;		
	}
	
}
