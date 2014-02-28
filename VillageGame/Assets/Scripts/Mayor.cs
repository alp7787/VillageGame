using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Mayor : Vehicle {

	//-----------------------------------steer with mouse------------------------------------		
	// In mouse steering, we keep track of the cumulative rotation on the y-axis which we can combine
	// with our initial orientation to get our current heading. We are keeping our transform level so that
	// right and left turning remains predictable even if our vehicle banks and climbs.	
	private void SteerWithMouse ()
	{
		//Get the left/right Input from the Mouse and use time along with a scaling factor 
		// to add a controlled amount to our cummulative rotation about the y-Axis.
		cummulativeRotation += Input.GetAxis ("Mouse X") * Time.deltaTime * rotationSensitvity;
		
		//Create a Quaternion representing our current cummulative rotation around the y-axis. 
		Quaternion currentRotation = Quaternion.Euler (0.0f, cummulativeRotation, 0.0f);
		
		//Use the quaternion to update the transform of our vehicle of the vehicles Game Object based on initial orientation 
		//and the currently applied rotation from the original orientation. 
		transform.rotation = initialOrientation * currentRotation;
	}

	protected override void Update()
	{
		SteerWithMouse ();
		CalcForces ();
		base.Update ();
	}


	// Calculate the forces that alter velocity
	private void CalcForces ()
	{
		steeringForce = Vector3.zero;
		steeringForce += KeyboardAcceleration ();
	}
	//----------------------------Accelerate with Arrow or WASD keys------------------------------------		
	// If the user is pressing the up-arrow or W key we will return a force to accelerate the vehicle
	// along its z-axis which is to say in the foward direction.
	private Vector3 KeyboardAcceleration ()
	{
		//Move 'forward' based on player input
		Vector3 force;
		Vector3 dv = Vector3.zero;
		//dv is desired velocity
		dv.z = Input.GetAxis ("Vertical");
		//forward is positive z 
		//Take the moveDirection from the vehicle's local space to world space 
		//using the transform of the Game Object this script is attached to.
		dv = transform.TransformDirection (dv);
		dv *= maxSpeed;
		force = dv - transform.forward * speed;
		return force;
	}
}
