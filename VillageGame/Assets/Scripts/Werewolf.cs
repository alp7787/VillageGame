using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CharacterController))]
//[RequireComponent(typeof(Steering))]


public class Werewolf : NPC {


	// Current node index, I believe. Don't quote me on that
	private int currentNodeIndex = 0;

	//weighting variables
	private int seekWeight = 20;
	private int fleeWeight = 30;
	private int pathWeight = 10;


	//variables for distances of steering behaviors
	public int seekDist = 30;
	public int fleeDist = 20;

	private int index = -1;
	public int Index 
	{
		get { return index; }
		set { index = value; }
	}
	
	//movement variables
	private Vector3 moveDirection;
	
	//steering variable
	private GameObject respawnPont;
	
	//Hunting variables
	private GameObject target;
	private int preyIndex;

	public NavMeshAgent myAgent;

	// Use this for initialization
	protected override void Start ()
	{
		//get component references
		characterController = gameObject.GetComponent<CharacterController> ();
		//steering = gameObject.GetComponent<Steering> ();
		
		respawnPont = GameObject.FindGameObjectWithTag("Respawn");
		
		gameManager = GameManager.Instance;
		
		preyIndex = 0;
		target = gameManager.Villagers[preyIndex];
		myAgent = (NavMeshAgent)this.GetComponent("NavMeshAgent");
		myAgent.SetDestination(gameManager.WerewolfPath[currentNodeIndex].transform.position);
		//target = gameManager.Villagers[0];
		//base.Start ();

	}
	
	public void OnCollisionEnter(Collision wCollision)
	{
		if(wCollision.gameObject.tag == "Villager")
		{
			Villager death = wCollision.gameObject.GetComponent<Villager>();
			gameManager.KillVillager(death);
			
			FindTarget();
		}
	}

	protected override Vector3 FollowPath()
	{
		// default so we don't break
//		if(gameManager.WerewolfPath.Length <= 0)
//			return Vector3.zero;
		//cycle the node if im too close
		//myAgent.Warp(gameManager.WerewolfPath[currentNodeIndex].transform.position);
		Debug.Log(myAgent.pathStatus);
		if(Vector3.Distance(transform.position, gameManager.WerewolfPath[currentNodeIndex].transform.position) <= 5.0f)
		{

			currentNodeIndex++;//go to next node
			if(currentNodeIndex >= gameManager.WerewolfPath.Length)
			{
				currentNodeIndex = 0;
			}
			myAgent.SetDestination(gameManager.WerewolfPath[currentNodeIndex].transform.position);
		}

		//return Seek(gameManager.WerewolfPath[currentNodeIndex].transform.position);//head for the next node in the path
		return Vector3.zero;
	}


	
	// Update is called once per frame
	protected void FixedUpdate () 
	{
		FollowPath();
		myAgent.updatePosition = true;
//		CalcSteeringForce ();
//		ClampSteering ();
//		
//		moveDirection = transform.forward * speed;
//		// movedirection equals velocity
//		//add acceleration
//		moveDirection += steeringForce * Time.deltaTime;
//		//update speed
//		speed = moveDirection.magnitude;
//		
//		//area of fix
//		if (speed != moveDirection.magnitude) {
//			moveDirection = moveDirection.normalized * speed;
//		}
//		
//		//orient transform
//		if (moveDirection != Vector3.zero)
//			transform.forward = moveDirection;
//		
//		// Apply gravity
//		moveDirection.y -= gravity;
//		
//		// the CharacterController moves us subject to physical constraints
//		characterController.Move (moveDirection * Time.deltaTime);
	}
	
	private void FindTarget()
	{

		
		GameObject prey;

		if(target == null)
		{
			target = gameManager.Villagers[0];
		}


		for (int i = 0; i < gameManager.Villagers.Count; i++)
		{	

			prey = gameManager.Villagers[i];

			if(((Villager)gameManager.Villagers[i].GetComponent("Villager")).MayorDist > fleeDist)
			{
			
				if(Vector3.Distance(this.transform.position, prey.transform.position) 
					< Vector3.Distance(this.transform.position, target.transform.position))
				{

					target = gameManager.Villagers[i];
				}
			}
		}
	}
		
	protected override void CalcSteeringForce ()
	{
		steeringForce = Vector3.zero;
		
		//Keeps werewolves away from Villager Spawn point
		steeringForce += gameManager.avoidWt * AvoidObstacle(respawnPont, 100f);


		// distance from me to mayor
		float mayDist = Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position);
		
		//Choose new villager to chase (closest villager)
		//target = gameManager.Villagers[0];
		FindTarget();
		
		float tarDist = Vector3.Distance(this.transform.position, target.transform.position);
		
		if(mayDist < fleeDist)
		{
			steeringForce += fleeWeight * Flee(gameManager.Mayor);
		}

		steeringForce += FollowPath() * pathWeight;
	
		if(((Villager)target.GetComponent("Villager")).MayorDist > fleeDist)
		{
			steeringForce += seekWeight * (seekDist/tarDist) * Seek(target);
		}



//		else
//		{
//			steeringForce += 5 * Evasion(gameManager.Mayor.transform.forward +
//				gameManager.Mayor.transform.position);
//		}
		
//		avoid close obstacles
		for(int i =0; i < gameManager.Obstacles.Length; i++)
		{
			if(Vector3.Distance(this.transform.position, gameManager.Obstacles[i].transform.position) < 60)
			{
				steeringForce += gameManager.avoidWt * AvoidObstacle(gameManager.Obstacles[i], gameManager.avoidDist);	
			}
		}
	}
	
	protected override void ClampSteering ()
	{
		if (steeringForce.magnitude > maxForce) {
			steeringForce.Normalize ();
			steeringForce *= maxForce;
		}
	}
	
	// tether type containment - not very good!
	private Vector3 StayInBounds ( float radius, Vector3 center)
	{
		steeringForce = Vector3.zero;
		
		if(transform.position.x > 750 || transform.position.x < 200 || 
			transform.position.z > 715 || transform.position.z < 205)
		{
			steeringForce += Seek(gameManager.gameObject);
		}
		
		return steeringForce;
	
	}

	
}
