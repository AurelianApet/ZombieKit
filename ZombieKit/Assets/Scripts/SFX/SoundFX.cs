using UnityEngine;
using System.Collections;

public class SoundFX : MonoBehaviour {		//attached to the camera, generates sounds

	public GameObject 
		StartMusicBt, StopMusicBt, StartSoundBt, StopSoundBt;		//buttons

	public AudioClip 
		buildingFinished, move, click, close, bip, end,
		soldierFire, soldierisHit, soldierExplosion, 
		buildingExplosion, fireBurning;								//sounds

	public AudioSource 
		musicSource, soundSource;

	public AudioClip[] 
		hit = new AudioClip[10],
		die = new AudioClip[5],
		cannonFire = new AudioClip[2],
		vortex = new AudioClip[2],
		typeWriter = new AudioClip[7];

	private bool 
		soundsTimerb = true, fadeIn = false, fadeOut = false, busy = false, isPlaying = false;

	//[HideInInspector]
	public bool 
		musicOn = true, soundOn = true;

	private int maxSounds = 0;
	private float musicVoume = 1;

	// Use this for initialization
	void Start () {

		AudioSource[] sources = GetComponents <AudioSource>();
		musicSource = sources [0];
		soundSource = sources [1];
		musicSource.ignoreListenerVolume = true;
	}

	public void BattleMapSpecific()
	{
		InvokeRepeating ("SoundsTimer", 0.5f, 0.5f);
	}
	private void SoundsTimer()
	{
		soundsTimerb = !soundsTimerb;
	}

	public void MusicOn()
	{
		if (!fadeIn && !busy) 
		{
			fadeIn = true;
			busy = true;
			
			if (!isPlaying) 
			{	
				isPlaying=true;
				musicSource.Play();
			}
			musicOn = true;
			UpdateMusicBt ();
		}
	}
	
	public void MusicOff()
	{
		if (!fadeOut && !busy) 
		{
			fadeOut = true;
			busy = true;
			musicOn = false;
			UpdateMusicBt ();
		}
		
	}

	public void SoundOn() 
	{ 
		soundOn = true;
		soundSource.volume = 1;
		AudioListener.volume = 1;
		UpdateSoundBt ();
	}
	
	public void SoundOff()
	{
		soundOn = false;
		soundSource.volume = 0;
		AudioListener.volume = 0;
		UpdateSoundBt ();
	}

	private void UpdateMusicBt()
	{
		StartMusicBt.SetActive (!musicOn);
		StopMusicBt.SetActive (musicOn);
	}
	
	private void UpdateSoundBt()
	{
		StartSoundBt.SetActive (!soundOn);
		StopSoundBt.SetActive (soundOn);
	}

	public void ChangeMusic(bool b)//called from StatsBattle SaveLoadMap
	{
		if(b) MusicOn();
		else MusicOff(); 
	}
	
	public void ChangeSound(bool b)
	{		
		if (b) SoundOn();
		else SoundOff(); 
	}

	// Update is called once per frame
	void Update () {
		
		if(fadeIn)
		{
			if(musicVoume<1)
			{
				musicVoume += 0.01f;
				musicVoume = Mathf.Clamp(musicVoume,0,1);
				musicSource.volume = musicVoume;
			}
			else
			{
				fadeIn = false;
				busy = false;
			}
			
		}
		if(fadeOut)
		{
			if(musicVoume>0)
			{
				musicVoume -= 0.01f;
				musicVoume = Mathf.Clamp(musicVoume,0,1);
				musicSource.volume = musicVoume;
			}
			else
			{
				fadeOut = false;
				busy = false;
				musicSource.Pause();
				isPlaying = false;
			}			
		}
		
	}

	//called by buttons pressed in various 2dToolkit interfaces and played near the camera
	public void BuildingFinished()	{ soundSource.PlayOneShot(buildingFinished); }

	public void Move() { soundSource.PlayOneShot(move); }

	public void Click()	{ soundSource.PlayOneShot(click);	}

	public void Close()	{ soundSource.PlayOneShot(close);	}

	public void End()	{ soundSource.PlayOneShot(end);	}
	//Battle sounds

	public void CannonFire()	
	{
		int rand = Random.Range (0, 1);
		soundSource.PlayOneShot(cannonFire[rand]);	
	}

	public void BuildingBurn()
	{
		soundSource.loop = true; soundSource.clip = fireBurning; soundSource.Play();
		soundSource.PlayOneShot (fireBurning);
	}
	public void BuildingExplode() { soundSource.PlayOneShot(buildingExplosion);}

	public void SoldierPlace() { soundSource.PlayOneShot(bip);}

	public void SoldierFire()//this must be limited, otherwise the sound fades 	
	{ if(soundsTimerb) 
		{
			soundSource.PlayOneShot (soldierFire);
			maxSounds++;
			if(maxSounds > 2)//allow 3 sounds
			{
				maxSounds = 0;
				soundsTimerb = false;
			}
		}
	}

	public void SoldierHit()	
	{ 
		soundSource.PlayOneShot(soldierisHit);
		int rand = Random.Range (0, 9);
		soundSource.PlayOneShot(hit[rand]);
	}

	public void SoldierExplode() { soundSource.PlayOneShot(soldierExplosion);}

	public void SoldierDie()
	{
		int rand = Random.Range (0, 4);
		soundSource.PlayOneShot (die[rand]);
		soundSource.PlayOneShot (vortex[rand%2]);
	}
	public void TypeWriter()
	{
		int rand = Random.Range (0, 6);
		soundSource.PlayOneShot (typeWriter[rand]);
	}
}
