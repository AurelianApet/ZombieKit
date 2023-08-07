using UnityEngine;
using System.Collections;

public class CannonController : MonoBehaviour{		//cannons are instantiated in the middle of each building - simplified situation

    public GameObject Bullet;
	
    private Transform 
		turret,
    	bulletSpawnPoint;  

	public Vector3 target;
	public bool fire = false;

	private float turretRotSpeed = 0.5f;
  
    //Bullet shooting rate
    protected float 
		shootRate = 2.0f,
    	elapsedTime;

	private Component soundFx;

    void Start()
    {  
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
		soundFx = GameObject.Find ("SoundFX").GetComponent<SoundFX> ();
    }
	/*
    void OnEndGame()
    {
        // Don't allow any more control changes when the game ends
        this.enabled = false;
    }
	*/
    void Update()
	{
        UpdateWeapon();
    }
    
	private void Rotate()
	{
		Vector3 targetDir = gameObject.transform.position - target;

		if (targetDir != Vector3.zero)
		{
			float angle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg+10;

	        //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward); 						//instant rotation
			transform.rotation = Quaternion.Slerp(turret.transform.rotation, 
			                                      Quaternion.AngleAxis(angle, Vector3.forward), 
			                                      Time.deltaTime * turretRotSpeed);						//slow gradual rotation					
    	}
	}

    void UpdateWeapon()
    {
		if(fire)//Input.GetMouseButtonDown(0)
        {
			Rotate();
			elapsedTime += Time.deltaTime;
            if (elapsedTime >= shootRate)
            {
                //Reset the time
                elapsedTime = 0.0f;
				((SoundFX)soundFx).CannonFire();                
                Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            }
        }
    }
}

