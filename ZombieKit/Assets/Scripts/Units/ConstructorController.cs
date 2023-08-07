using UnityEngine;
using System.Collections;

public class ConstructorController : MonoBehaviour {  //constrolls the dobbit during construction

	private int curPathIndex;

	public float speed = 55.0f, mass = 5.0f;
	private float curSpeed,	pathLength;
	private Vector3 targetPoint, velocity;

	//public bool isLooping = true;

	private enum DobbitState{Walk, Build}
	private DobbitState currentState = DobbitState.Walk;
	private DobbitAnimController dobbitAnimController;	

	public ConstructionPath path;

	void Start () {
		dobbitAnimController = GetComponent<DobbitAnimController>();
		pathLength = path.Length;
		curPathIndex = 0;

		Vector3 pos = transform.position;
		float zDepth = pos.z - 10 / (pos.y + 3300);	
		transform.position = new Vector3 (pos.x, pos.y, zDepth); //zDepth correction at instantiation
		//velocity = Vector3.zero;
	}

	void Update () {
		
		switch (currentState) 
		{
			case DobbitState.Walk:
			Walk();
			break;						
		}		
	}

	private void Walk()
	{	
		curSpeed = speed * Time.deltaTime;
		targetPoint = path.GetPoint(curPathIndex);

		//if path point reached, move to next point
		if (Vector3.Distance(transform.position, targetPoint) <
		    path.Radius) {
			currentState = DobbitState.Build;
			StartCoroutine("Build");

			if (curPathIndex < pathLength - 1) curPathIndex++;
			else curPathIndex = 0;
		}

		velocity += Steer(targetPoint);		
		transform.position += velocity;
	}

	IEnumerator Build()
	{

		ChangeAnimation("Build");
		yield return new WaitForSeconds(4);
		ChangeAnimation("Walk");
		currentState = DobbitState.Walk;
	}

	private void ChangeAnimation(string action)
	{
		string direction = " ";

		switch (action) {

		case "Walk":
			switch (curPathIndex) 
			{
			case 0:
				direction="SW";
				break;
			case 1:
				direction="NW";
				break;
			case 2:
				direction="NE";
				break;
			case 3:
				direction="SE";
				break;			
			}

		break;

		case "Build":
		switch (curPathIndex) 
			{
			case 0:
				direction="N";
				break;
			case 1:
				direction="E";
				break;
			case 2:
				direction="S";
				break;
			case 3:
				direction="W";
				break;				
			}
			break;
		}

		dobbitAnimController.ChangeAnim(action);
		dobbitAnimController.Turn(direction);
		dobbitAnimController.UpdateCharacterAnimation();
	}

	public Vector3 Steer(Vector3 target) 
	{
		Vector3 desiredVelocity = (target - transform.position);
		float dist = desiredVelocity.magnitude;

		desiredVelocity.Normalize();

		if (dist<10.0f) 
			desiredVelocity *= (curSpeed * (dist / 10.0f));
		else 
			desiredVelocity *= curSpeed;

		Vector3 steeringForce = desiredVelocity - velocity;
		Vector3 acceleration = steeringForce / mass;
						
		transform.position += velocity;

		return acceleration;

	}
}

