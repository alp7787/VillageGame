using UnityEngine;
using System.Collections;

//directive to enforce that our parent Game Object has a Character Controller
[RequireComponent(typeof(CharacterController))]

public class Vehicle : Steering
{
	//The Character Controller on my parent GameObject
	protected CharacterController characterController;

	// The linear gravity factor. Made available in the Editor.
	public float gravity = 100.0f;
	
	// mass of vehicle
	public float mass = 1.0f;

	// The initial orientation.
	protected Quaternion initialOrientation;

	// The cummulative rotation about the y-Axis.
	protected float cummulativeRotation;

	// The rotation factor, this will control the speed we rotate at.
	public float rotationSensitvity = 2.0f;

	// The scout is used to mark the future position of the vehicle.
	// It is made visible as a debugging aid, but the point it is placed at is
	// used in alligment and in keeping the vehicle from leaving the terrain.
	public GameObject scout;

	//variables used to align the vehicle with the terrain surface 
	public float lookAheadDist = 2.0f; // How far ahead the scout is place
	protected Vector3 hitNormal; // Normal to the terrain under the vehicle
	private float halfHeight; // half the height of the vehicle
	protected Vector3 lookAtPt; // used to align the vehicle; marked by scout
	protected Vector3 rayOrigin; // point from which ray is cast to locate scout
	protected RaycastHit rayInfo; // struct to hold information returned by raycast
	protected int layerMask = 1 << 8; //mask for a layer containg the terrain

	//movement variables - exposed in inspector panel
	public float friction = 0.997f; // multiplier decreases speed
	
	//movement variables - updated by this component
	//protected float speed = 0.0f;  //current speed of vehicle
	protected Vector3 steeringForce; // force that accelerates the vehicle
	//protected Vector3 velocity; //change in position per second


	// Use this for initialization
	protected override void Start ()
	{
		//Use GetComponent to save a reference to the Character Controller. This 
		//generic method is avalable from the parent Game Object. The class in the  
		// angle brackets <  > is the type of the component we need a reference to.
		
		characterController = gameObject.GetComponent<CharacterController> ();
		
		//save the quaternion representing our initial orientation from the transform
		initialOrientation = transform.rotation;
		
		//set the cummulativeRotation to zero.
		cummulativeRotation = 0.0f;
		
		//half the height of vehicle bounding box
		halfHeight = renderer.bounds.extents.y;
		base.Start ();
	}

	// Update is called once per frame
	protected virtual void Update ()
	{
		//calculate steering - forces that change velocity
		ClampForces ();
		//forces must not exceed maxForce
		CalcVelocity ();
		//orient vehicle transform toward velocity
		if (velocity != Vector3.zero) {
			transform.forward = velocity;
			MoveAndAlign ();
		}
	}

	// if steering forces exceed maxForce they are set to maxForce
	private void ClampForces ()
	{
		if (steeringForce.magnitude > maxForce) {
			steeringForce.Normalize ();
			steeringForce *= maxForce;
		}
	}

	protected virtual void CalcSteeringForce ()
	{}
	protected virtual void ClampSteering()
	{}
	
	// acceleration and velocity are calculated
	protected virtual void CalcVelocity ()
	{
		Vector3 moveDirection = transform.forward;
		// move in forward direction
		speed *= friction;
		// speed is reduced to simulate friction
		velocity = moveDirection * speed;
		// movedirection is scaled to get velocity
		Vector3 acceleration = steeringForce / mass;
		// acceleration is force/mass
		velocity += acceleration * Time.deltaTime;
		// add acceleration to velocity
		speed = velocity.magnitude;
		// speed is altered by acceleration		
		if (speed > maxSpeed) {
			// clamp speed & velocity to maxspeed
			speed = maxSpeed;
			velocity = moveDirection * speed;
		}
	}

	//-----------------------------------MoveAndAlign------------------------------------		
	// Alignment permits our vehicle to tilt to climb hills and bank to follow the camber of the path.
	// It is done after we move and the transform is restored to its level state by the mouse steering
	// code at the beginning of the function as we prepare to orient and move again. 
	void MoveAndAlign ()
	{
		rayOrigin = transform.position + transform.forward * lookAheadDist;
		rayOrigin.y += 100;
		// A ray is cast from a position lookAheadDist ahead of the vehicle on its current path 
		// and high above the terrain. If the ray misses the terrain, we are likely to fall off, so
		// no move will take place.
		if (Physics.Raycast (rayOrigin, Vector3.down, out rayInfo, Mathf.Infinity, layerMask)) {
			//Apply net movement to character controller which keeps us from penetrating colliders.
			// Velocity is scaled by deltaTime to give the correct movement for the time elapsed
			// since the last update. Gravity keeps us grounded.
			characterController.Move (velocity * Time.deltaTime + Vector3.down * gravity);
			
			// Use lookat function to align vehicle with terrain and position scout
			lookAtPt = rayInfo.point;
			lookAtPt.y += halfHeight;
			transform.LookAt (lookAtPt, hitNormal);
			scout.transform.position = lookAtPt;
		}
	}

	// The hitNormal will give us a normal to the terrain under our vehicle
	// which we can use to align the vehicle with the terrain. It will be
	// called repeatedly when the collider on the character controller
	// of our vehicle contacts the collider on the terrain
	void OnControllerColliderHit (ControllerColliderHit hit)
	{	
		hitNormal = hit.normal;
	}
}

