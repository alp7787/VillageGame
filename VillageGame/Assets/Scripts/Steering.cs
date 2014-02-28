using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;


public class Steering : MonoBehaviour
{
	
	//movement variables - exposed in inspector panel
	public float maxSpeed = 50.0f;
	//maximum speed of vehicle
	public float maxForce = 15.0f;
	// maximimum force allowed
	
	//movement variables - updated by this component
	protected float speed = 0.0f;
	//current speed of vehicle
	protected Vector3 velocity;
	//change in position per second
	
	public Vector3 Velocity {
		get { return velocity; }
		set { velocity = value;}
	}
		
	public float Speed {
		get { return speed; }
		set { speed = Mathf.Clamp (value, 0, maxSpeed); }
	}
	
	protected virtual void Start ()
	{
		Velocity = Vector3.zero;
	}
	

}