﻿using UnityEngine;
using System.Collections;

public class CharacterPhysics : MonoBehaviour {

	[Header("Basic Movement")]
	public Vector2 movementForce = new Vector2(100.0f, 100.0f); // x = Horizontal/Sideways force, y = Forward force
	public Vector2 counteractForce = new Vector2(-50.0f, -50.0f); // x = Horizontal/Sideways force, y = Forward force
	public Vector2 maxSpeed = new Vector2(10.0f, 10.0f); // x = Horizontal/Sideways, y = Forward

	[Header("Movement Multipliers")]
	public float strafeMultiplier = 0.7f;
	public float diagonalMultiplier = 0.7f;

	[Header("In-air Movement")]
	public Vector2 inAirMovementForce = new Vector2(10.0f, 10.0f);
	public float inAirMaxStrafeSpeed = 3.0f;
	public float inAirminimumMaxReverseSpeed = 3.0f;

	[Header("Jumping")]
	public float jumpSpeed = 3.0f;

	[Header("Collision Handling")]
	public float minColMagnitude = 7.0f;
	public float maxColMagnitude = 10.0f;
	public float afterColTime = 1.0f;

	Rigidbody body;
	//CapsuleCollider capCollider;

	[HideInInspector]
	public bool grounded = false;
	[HideInInspector]
	public Rigidbody groundedObject;

	bool colliding;
	float collisionStartTime;
	float collisionMagnitude;
	//Vector3 collisionNormal;
	
	Rigidbody currentPlatform;
	Vector3 oldPosPlatform;

	void Start () {
		body = GetComponent<Rigidbody>();
		//capCollider = GetComponent<CapsuleCollider>();
	}
	
	void FixedUpdate () {
		// Set current platform if platform has a rigidbody
		if (groundedObject != null && currentPlatform == null) {
			currentPlatform = groundedObject;

			oldPosPlatform = currentPlatform.position;
		}

		Vector3 inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		// Subtract platform velocity to the current realitive velocity to make sure the character controller doesnt counteract while riding a platform
		Vector3 platformVelocity = (currentPlatform != null)? currentPlatform.velocity : Vector3.zero;
		Vector3 realitiveVelocity = transform.InverseTransformDirection(body.velocity - platformVelocity);
		Vector3 realitiveVelocityNorm = realitiveVelocity.normalized;

		if (grounded) {
			// Apply strafe multiplier
			Vector2 tempInputAxis = inputAxis;
			inputAxis.x *= diagonalMultiplier;
			inputAxis = tempInputAxis;

			// Apply diagonal multiplier
			if (inputAxis.x > 0.0f && inputAxis.y > 0.0f) {
				inputAxis *= diagonalMultiplier;
			}

			// Move character with current platform
			if (currentPlatform != null) {
				Vector3 posDelta = currentPlatform.position - oldPosPlatform;
				body.position += posDelta;

				oldPosPlatform = currentPlatform.position;
			}

			// When the user is applying direction, output is 0.0f.  When the user is not applying direction, output is 1.0f
			Vector2 inputAxisInverseMultiplier = new Vector2(Mathf.Abs(Mathf.Abs(inputAxis.x) - 1.0f), Mathf.Abs(Mathf.Abs(inputAxis.y) - 1.0f));

			// Compute counteracting force
			// When realitiveVelocity is greater than max speed, always have counteracting force on no matter about the input
			// Also multiply the realitiveVelocity.normalized to the final outputs to force the body to slow down faster
			Vector3 counteractingForce = new Vector3((((Mathf.Abs(realitiveVelocity.x) >= maxSpeed.x)? 1.0f : inputAxisInverseMultiplier.x) * counteractForce.x) * realitiveVelocityNorm.x, 0.0f, (((Mathf.Abs(realitiveVelocity.x) >= maxSpeed.x)? 1.0f : inputAxisInverseMultiplier.y) * counteractForce.y) * realitiveVelocityNorm.z);

			// Collision Magnitude >= max, then output = 1.  Collision Magnitude <= min, then output = 0
			float collisionMultiplier = Mathf.Abs(((collisionMagnitude - minColMagnitude) / maxColMagnitude) - 1.0f);
			if (colliding) {
				counteractingForce *= collisionMultiplier;
			}

			// Reset the colliding stuff after time has passed
			if (colliding && Time.time > (collisionStartTime + afterColTime)) {
				colliding = false;
			}

			// Zero out the counteracting force when the user isnt applying more force, and the body isnt moving, so the counteractingForce doesnt jitter/ping-pong
			if (inputAxis.x < 1.0f && Mathf.Abs(realitiveVelocity.x) < 1.0f) {
				counteractingForce.x = 0.0f;
			}
			if (inputAxis.y < 1.0f && Mathf.Abs(realitiveVelocity.z) < 1.0f) {
				counteractingForce.z = 0.0f;
			}

			// Apply the counteracting force
			body.AddRelativeForce(counteractingForce, ForceMode.Acceleration);

			// Compute force dir simply like a normal character controller
			Vector3 forceDir = new Vector3(inputAxis.x * movementForce.x, 0.0f, inputAxis.y * movementForce.y);

			// Apply collision multiplier to forceDir when collided
			if (colliding) {
				forceDir *= collisionMultiplier;
			}

			// Attempt to cap the speed of the body at max speed by zeroing out the forceDir
			if (Mathf.Abs(realitiveVelocity.x) >= maxSpeed.x) {
				forceDir.x = 0.0f;
			}
			if (Mathf.Abs(realitiveVelocity.z) >= maxSpeed.y) {
				forceDir.z = 0.0f;
			}

			// Apply main force
			body.AddRelativeForce(forceDir, ForceMode.Acceleration);

			// Handle jumping
			if (Input.GetButtonDown("Jump")) {
				Vector3 vel = body.velocity;
				vel.y = jumpSpeed;
				body.velocity = vel;
			}
		} else {
			// Make current platform null
			currentPlatform = null;

			// Force on the z axis is only applied when the y inputAxis is negitive, to allow the user to slow down in mid air
			Vector3 forceDir = new Vector3(inputAxis.x * inAirMovementForce.x, 0.0f, (Mathf.Sign (inputAxis.y) == -1.0f)? inputAxis.y * inAirMovementForce.y : 0.0f);

			// Caps strafing in mid air
			if (Mathf.Abs(realitiveVelocity.x) >= inAirMaxStrafeSpeed) {
				forceDir.x = 0.0f;
			}
			// Caps the force so the player cant keep accelerating backwards
			if (Mathf.Abs(realitiveVelocity.z) <= inAirminimumMaxReverseSpeed) {
				forceDir.z = 0.0f;
			}

			// Apply main in-air force
			body.AddRelativeForce(forceDir, ForceMode.Acceleration);
		}
	}

	/*
	void OnGUI () {
		GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20.0f));

		GUILayout.Label(grounded.ToString());

		GUILayout.EndArea();
	}
	*/

	void OnCollisionEnter (Collision collision) {
		collisionMagnitude = collision.relativeVelocity.magnitude;

		/*
		Vector3 allCollisions = Vector3.zero;
		foreach (ContactPoint contact in collision.contacts) {
			allCollisions += contact.normal;
		}

		collisionNormal = allCollisions.normalized;
		*/

		if (collisionMagnitude >= minColMagnitude) {
			colliding = true;
			collisionStartTime = Time.time;
		}
	}
}