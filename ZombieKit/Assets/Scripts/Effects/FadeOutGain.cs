using UnityEngine;
using System.Collections;

public class FadeOutGain : MonoBehaviour { //the gold coin / blue mana vial going up when the building is attacked

	public float alpha, fadeOutAfter, scale, deltaAlpha, deltaScale, ySpeed;

	private tk2dSprite sprite;
	private	tk2dTextMesh text;

	private bool fadeOut = false;
	private float fadeOutCounter = 0;

	void Start () 
	{
		transform.parent = GameObject.Find("GroupEffects").transform;
		sprite = gameObject.GetComponentInChildren<tk2dSprite> ();
		text = gameObject.GetComponentInChildren<tk2dTextMesh> ();
	}

	void FixedUpdate () {

		if (!fadeOut) 
		{
			fadeOutCounter += Time.deltaTime;
			if(fadeOutCounter > fadeOutAfter)
			{
				fadeOut=true;
			}
		}

		if (alpha > 0)
		{
			scale += deltaScale;	

			transform.position += new Vector3 (0, ySpeed,0);
			transform.localScale = new Vector3(scale,scale,1);

			if(fadeOut)
			{
				alpha += deltaAlpha;
				Color currentSpriteCollor = sprite.color;
				sprite.color = new Color(currentSpriteCollor.r, currentSpriteCollor.g, currentSpriteCollor.b, alpha);

				Color currentTextCollor = text.color;
				text.color = new Color(currentTextCollor.r, currentTextCollor.g, currentTextCollor.b, alpha);
			}

		}
		else
			Destroy(gameObject);			                                                        
	}
}