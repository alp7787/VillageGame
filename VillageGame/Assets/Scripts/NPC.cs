﻿using UnityEngine;
using System.Collections;

public class NPC : Vehicle {

	// Use this for initialization
	protected override void Start () {
		base.Start ();
	}
	
	// Update is called once per frame
	protected override void Update () {
	
	}

	public Vector3 Pursuit (Vector3 fwd)
	{
		// find dv, the desired velocity
		// sets the vector to the future position
		fwd = fwd * 1.1f;
		Vector3 dv = fwd - transform.position;
		dv.y = 0; //only steer in the x/z plane
		dv = dv.normalized * maxSpeed;//scale by maxSpeed
		dv -= transform.forward * speed;//subtract velocity to get vector in that direction
		return dv;  
	}
	
	public Vector3 Seek (Vector3 pos)
	{
		// find dv, the desired velocity
		Vector3 dv = pos - transform.position;
		//calculate distance between pos and position of seeker
		float dist = Vector3.Distance(pos,transform.position);
		dv.y = 0; //only steer in the x/z plane
		//if close slow down, else full speed seek
		
		if(dist > 5)
		{
			dv = dv.normalized * (maxSpeed * .7f);//scale by maxSpeed
		} 
		else
		{
			dv = dv.normalized * (maxSpeed * .5f);
		}
		
		dv -= transform.forward * speed;//subtract velocity to get vector in that direction
		return dv;
	}
	
	public Vector3 Arrival (Vector3 pos)
	{
		// find dv, the desired velocity
		Vector3 dv = pos - transform.position;
		//calculate distance between pos and position of seeker
		float dist = Vector3.Distance (pos, transform.position);
		dv.y = 0; //only steer in the x/z plane
		//if close slow down, else full speed seek
		
		if(dist > 10)
		{
			dv = dv.normalized * (maxSpeed);//scale by maxSpeed
		} 
		else
		{
			dv = Vector3.zero;
		}
		
		dv -= transform.forward * speed;//subtract velocity to get vector in that direction
		return dv;
	}
	
	// same as seek pos above, but parameter is game object
	public Vector3 Seek (GameObject gO)
	{
		return Seek(gO.transform.position);
	}
	
	public Vector3 Evasion (Vector3 fwd)
	{
		fwd = fwd * 2; // sets a future vector for level bobbing.
		Vector3 dv = transform.position - fwd;
		dv.y = 0; //only steer in the x/z plane
		dv = dv.normalized * maxSpeed;//scale by maxSpeed
		dv -= transform.forward * speed;//subtract velocity to get vector in that direction
		return dv;  
		
	}
	
	public Vector3 Flee (Vector3 pos)
	{
		Vector3 dv = transform.position - pos;//opposite direction from seek 
		dv.y = 0;
		dv = dv.normalized * maxSpeed;
		dv -= transform.forward * speed;
		return dv;
	}
	
	public Vector3 Flee (GameObject go)
	{
		Vector3 targetPos = go.transform.position;
		targetPos.y = transform.position.y;
		Vector3 dv = transform.position - targetPos;
		dv = dv.normalized * maxSpeed;
		dv.y = 0;
		return dv - transform.forward * speed;
	}
	
	public Vector3 AlignTo (Vector3 direction)
	{
		// useful for aligning with flock direction
		Vector3 dv = direction.normalized;
		dv = dv * maxSpeed - transform.forward * speed;
		dv.y = 0;
		return dv;
		
	}
	
	//Assumtions:
	// we can access radius of obstacle
	// we have CharacterController component
	public Vector3 AvoidObstacle (GameObject obst, float safeDistance)
	{
		Vector3 dv = Vector3.zero;
		//compute a vector from charactor to center of obstacle
		Vector3 vecToCenter = obst.transform.position - transform.position;
		//eliminate y component so we have a 2D vector in the x, z plane
		vecToCenter.y = 0;
		float dist = vecToCenter.magnitude;
		
		//return zero vector if too far to worry about
		if (dist > safeDistance + obst.GetComponent<Dimensions> ().Radius + GetComponent<Dimensions> ().Radius)
			return dv;
		
		//return zero vector if behind us
		if (Vector3.Dot (vecToCenter, transform.forward) < 0)
			return dv;
		
		//return zero vector if we can pass safely
		float rightDotVTC = Vector3.Dot (vecToCenter, transform.right);
		if (Mathf.Abs (rightDotVTC) > obst.GetComponent<Dimensions> ().Radius + GetComponent<Dimensions> ().Radius)
			return dv;
		
		//obstacle on right so we steer to left
		if (rightDotVTC > 0)
			dv = transform.right * -maxSpeed * safeDistance / dist;
		else
			//obstacle on left so we steer to right
			dv = transform.right * maxSpeed * safeDistance / dist;
		
		//stay in x/z plane
		dv.y = 0;
		
		//compute the force
		dv -= transform.forward * speed;
		return dv;
	}
}
