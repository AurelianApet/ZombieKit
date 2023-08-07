using UnityEngine;
using System.Collections;

public class BlackScreen : MonoBehaviour {

	private TweenAlpha fadeout;

	void Start () {
		fadeout = GetComponent<TweenAlpha> ();
	}

	public void Play()
	{
		((TweenAlpha)fadeout).Play ();
	}
	public void Disable()
	{
		gameObject.SetActive (false);
	}

}
