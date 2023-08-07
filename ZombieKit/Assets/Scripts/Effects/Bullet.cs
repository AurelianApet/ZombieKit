﻿using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour	//the yellow bullet building cannons are firing
{   
	public GameObject Vortex, Sparks, Grave, Ghost; //Effects prefabs

	private int 
		ghostZ = -9,
		explosionZ = -6,
		graveZ = 0;

    public int Speed = 600;
    public float LifeTime = 3.0f;    

	private Component soundFx, helios;

    void Start()
    {
		transform.parent = GameObject.Find("GroupEffects").transform;

        Destroy(gameObject, LifeTime);
		helios = GameObject.Find ("Helios").GetComponent<Helios>();
		soundFx = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
    }

    void FixedUpdate()
    {
        transform.position += 
			transform.forward * Speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider collider)//OnCollisionEnter(Collision collision)
    {
		if(collider.gameObject.tag == "Dobbit")
		{	       
			Vector3 pos = collider.gameObject.transform.position;

			if(collider.gameObject.GetComponent<FighterController>().life>30)
			{	
				Instantiate(Sparks, new Vector3(pos.x, pos.y, explosionZ), Quaternion.identity);//contact.point
				//((SoundFX)soundeFx).SoldierExplode();
				((SoundFX)soundFx).SoldierHit();
			}
				else
			{
				Instantiate(Vortex, new Vector3(pos.x, pos.y, explosionZ), Quaternion.identity);
				Instantiate(Grave, new Vector3(pos.x, pos.y, graveZ), Quaternion.identity);
				Instantiate(Ghost, new Vector3(pos.x, pos.y, ghostZ), Quaternion.identity);

				((Helios)helios).KillUnit(collider.gameObject.GetComponent<FighterController>().assignedToGroup,
				                                              collider.gameObject.GetComponent<Selector>().index);
				((SoundFX)soundFx).SoldierExplode();
			}

			collider.gameObject.GetComponent<FighterController>().Hit();
	        Destroy(gameObject);
		}
    }
}