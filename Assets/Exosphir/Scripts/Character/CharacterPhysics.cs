using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPhysics : MonoBehaviour {

	[Header("Basic Movement")]
	public Vector2 movementForce = new Vector2(100.0f, 100.0f); // x = Horizontal/Sideways force, y = Forward force
	public Vector2 counteractForce = new Vector2(-50.0f, -50.0f); // x = Horizontal/Sideways force, y = Forward force
	public Vector2 maxSpeed = new Vector2(10.0f, 10.0f); // x = Horizontal/Sideways, y = Forward
	public Vector2 sprintMovementForce = new Vector2(100.0f, 100.0f); // x = Horizontal/Sideways force, y = Forward force
	public Vector2 sprintMaxSpeed = new Vector2(10.0f, 10.0f);
	public AnimationCurve forceCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

	[Header("Movement Multipliers")]
	public float strafeMultiplier = 0.7f;
	public float diagonalMultiplier = 0.7f;

	[Header("In-air Movement")]
	public Vector2 inAirMovementForce = new Vector2(10.0f, 10.0f);
	public Vector2 inAirMaxSpeed = new Vector2(10.0f, 10.0f);

	[Header("Jumping")]
	public float jumpSpeed = 3.0f;

	[Header("Collision Handling")]
	public float minColMagnitude = 7.0f;
	public float maxColMagnitude = 10.0f;
	public float afterColTime = 1.0f;
	public float newtonianMultiplier = 0.7f;
	public float maxPlatformVelocity = 7.0f;
	public float maxPlatformRotationVelocity = 7.0f;

	[Header("Ledge Hanging")]
	public float ledgeMoveVelocity = 1.0f;
	public float pushOffLedgeSpeed = 2.0f;
	public float pushUpForce = 3.0f;
	public Vector3 pushOffVector = new Vector3(0.0f, 1.0f, -1.0f);
	public Transform[] confirmLedgeHangingRaycasts;
	public Transform[] denyLedgeHangingRaycasts;
	public float checkerLengths = 1.0f;
	public CharacterLook look;

	[HideInInspector]
	public bool ledgeHanging;

	[HideInInspector]
	public Vector3 ledgeNormal;
	[HideInInspector]
	public Vector3 ledgeCharacterDir;
	[HideInInspector]
	public GameObject ledgeObject;

	Rigidbody body;
	Vector2 inputAxis;
	//CapsuleCollider capCollider;

	[HideInInspector]
	public bool grounded = false;

	[HideInInspector]
	public bool isThereWall = false;

	[HideInInspector]
	public bool pushOffLedge = false;
	
	bool colliding;
	float collisionStartTime;
	float collisionMagnitude;
	Vector3 collisionNormal;

	// Assigned by exterior scripts
	[HideInInspector]
	public Rigidbody currentPlatform;

	Vector3 oldPosPlatform;
	Quaternion oldRotPlatform;
	bool groundedFirstTime = true;
	bool hangingFirstTime = false;

	void Start () {
		body = GetComponent<Rigidbody>();
		//capCollider = GetComponent<CapsuleCollider>();
	}
	
	void FixedUpdate () {
		// Set current platform if platform has a rigidbody
		if (currentPlatform != null) {
			oldPosPlatform = currentPlatform.position;
		}

		inputAxis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;

		// Subtract platform velocity to the current relative velocity to make sure the character controller doesnt counteract while riding a platform
		Vector3 platformVelocity = (currentPlatform != null)? currentPlatform.velocity : Vector3.zero;
		Vector3 relativeVelocity = transform.InverseTransformDirection(body.velocity - platformVelocity);
		Vector3 relativeVelocityNorm = relativeVelocity.normalized;

		if (!ledgeHanging) {
			hangingFirstTime = true;

			if (grounded) {
				// Apply strafe multiplier
				inputAxis.x *= strafeMultiplier;

				// Change max speed based on sprinting or not
				Vector2 finalMaxSpeed = (Input.GetKey(KeyCode.LeftShift))? sprintMaxSpeed : maxSpeed;

				// Apply diagonal multiplier
				if (Mathf.Abs(inputAxis.x) > 0.0f && Mathf.Abs(inputAxis.y) > 0.0f) {
					inputAxis *= diagonalMultiplier;
				}

				// Move character with current platform
				if (currentPlatform != null) {
					// Set the oldPos platform on landing to advoid teleporting
					if (groundedFirstTime) {
						oldPosPlatform = currentPlatform.position;
						oldRotPlatform = currentPlatform.rotation;
					}

					Vector3 posDelta = currentPlatform.position - oldPosPlatform;
					Vector3 rotDelta = currentPlatform.rotation.eulerAngles - oldRotPlatform.eulerAngles;

					if (rotDelta.magnitude < maxPlatformRotationVelocity) {
						// Calculate the rotation realitive to origin, then translate it back to the players area
						Vector3 finalPlatformRot = RotatePoint(transform.position - currentPlatform.position, rotDelta) + currentPlatform.position;

						body.position = finalPlatformRot;
					}
					
					body.position += posDelta;

					// Set old values for calculating delta
					oldPosPlatform = currentPlatform.position;
					oldRotPlatform = currentPlatform.rotation;

					// If platform is moving over a certain speed, kick player off
					if (posDelta.magnitude >= maxPlatformVelocity) {
						currentPlatform = null;
					}
				}

				// When the user is applying direction, output is 0.0f.  When the user is not applying direction, output is 1.0f
				Vector2 inputAxisInverseMultiplier = new Vector2(Mathf.Abs(Mathf.Abs(inputAxis.x) - 1.0f), Mathf.Abs(Mathf.Abs(inputAxis.y) - 1.0f));

				// Compute counteracting force
				// When relativeVelocity is greater than max speed, always have counteracting force on no matter about the input
				// Also multiply the relativeVelocity.normalized to the final outputs to force the body to slow down faster
				Vector3 counteractingForce = new Vector3((((Mathf.Abs(relativeVelocity.x) >= finalMaxSpeed.x)? 1.0f : inputAxisInverseMultiplier.x) * counteractForce.x) * relativeVelocityNorm.x, 0.0f, (((Mathf.Abs(relativeVelocity.z) >= finalMaxSpeed.y)? 1.0f : inputAxisInverseMultiplier.y) * counteractForce.y) * relativeVelocityNorm.z);

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
				if (inputAxis.x < 1.0f && Mathf.Abs(relativeVelocity.x) < 1.0f) {
					counteractingForce.x = 0.0f;
				}
				if (inputAxis.y < 1.0f && Mathf.Abs(relativeVelocity.z) < 1.0f) {
					counteractingForce.z = 0.0f;
				}

				// Apply the counteracting force
				body.AddRelativeForce(counteractingForce, ForceMode.Acceleration);

				Vector2 finalMovementForce = (Input.GetKey(KeyCode.LeftShift))? sprintMovementForce : movementForce;

				// Compute force dir simply like a normal character controller
				Vector3 forceDir = new Vector3(inputAxis.x * finalMovementForce.x, 0.0f, inputAxis.y * finalMovementForce.y);

				Vector3 relativeColNormal = transform.InverseTransformDirection(collisionNormal);

				// Apply collision multiplier to forceDir when collided only when its not collision from ground
				if (colliding && Vector3Round(relativeColNormal) != Vector3.up) {
					forceDir *= collisionMultiplier;
				}

				// Attempt to cap the speed of the body at max speed by zeroing out the forceDir
				if (Mathf.Abs(relativeVelocity.x) >= finalMaxSpeed.x) {
					forceDir.x = 0.0f;
				}
				if (Mathf.Abs(relativeVelocity.z) >= finalMaxSpeed.y) {
					forceDir.z = 0.0f;
				}

				// Apply force curve based on current speed
				forceDir *= forceCurve.Evaluate(relativeVelocity.magnitude / ((finalMaxSpeed.x + finalMaxSpeed.y) / 2));

				// Apply main force
				body.AddRelativeForce(forceDir, ForceMode.Acceleration);

				// Handle jumping
				if (Input.GetButtonDown("Jump")) {
					Vector3 vel = body.velocity;
					vel.y = jumpSpeed;
					body.velocity = vel;

					if (currentPlatform != null) {
						// Apply force at position for a more realisitic force application
						// Multiply the mass into the force to simulate more force is needed to make a heavy character jump
						currentPlatform.AddForceAtPosition(new Vector3(0.0f, -jumpSpeed * body.mass * newtonianMultiplier, 0.0f), transform.position - (Vector3.down * (GetComponent<CapsuleCollider>().height / 2)), ForceMode.Impulse);
					}
				}

				groundedFirstTime = false;
			} else {
				// Make current platform null
				currentPlatform = null;

				groundedFirstTime = true;

				// Force on the z axis is only applied when the y inputAxis is negitive, to allow the user to slow down in mid air
				Vector3 forceDir = new Vector3(inputAxis.x * inAirMovementForce.x, 0.0f, inputAxis.y * inAirMovementForce.y);

				float dot = Vector2.Dot(new Vector2(relativeVelocity.x, relativeVelocity.z), inputAxis);

				if (Mathf.Sign(dot) * Mathf.Abs(relativeVelocity.x) >= inAirMaxSpeed.x) {
					forceDir.x = 0.0f;
				}
				if (Mathf.Sign(dot) * Mathf.Abs(relativeVelocity.z) >= inAirMaxSpeed.y) {
					forceDir.z = 0.0f;
				}

				// Completely zero out force if colliding in mid air
				if (isThereWall) {
					forceDir = Vector3.zero;
				}

				// Apply main in-air force
				body.AddRelativeForce(forceDir, ForceMode.Acceleration);

				// If pushing off ledge, apply force
				if (pushOffLedge) {
					Vector3 vel = body.velocity;
					vel.y += pushOffLedgeSpeed;
					body.velocity = vel;

					pushOffLedge = false;
				}
			}
		} else { // Ledge hanging = true
			look.enabled = false;

			// Set currentPlatform to ledgeObject if the ledgeObject has a rigidbody
			if (ledgeObject.GetComponent<Rigidbody>() != null) {
				currentPlatform = ledgeObject.GetComponent<Rigidbody>();
			} else {
				currentPlatform = null;
			}
			
			// Rotate character towards ledge
			transform.rotation = Quaternion.Euler(0.0f, Quaternion.LookRotation(-ledgeCharacterDir).eulerAngles.y, 0.0f);
			
			// Apply veloctiy to follow ledge
			Vector3 velocity = inputAxis.x * ledgeNormal * ledgeMoveVelocity;
			
			// Create and test the raycasts
			RaycastHit[] confirmHits = new RaycastHit[confirmLedgeHangingRaycasts.Length];
			int confirms = 0;
			
			for (int c = 0; c < confirmLedgeHangingRaycasts.Length; c++) {
				Transform confirmer = confirmLedgeHangingRaycasts[c];
				
				if (Physics.Raycast(confirmer.position, confirmer.rotation * Vector3.forward, out confirmHits[c], checkerLengths, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
					if (confirmHits[c].collider.gameObject == ledgeObject) {
						confirms++;
					}
				}
				
				Debug.DrawLine(confirmer.position, confirmer.position + ((confirmer.rotation * Vector3.forward) * checkerLengths), Color.cyan);
			}
			
			if (confirms == 0) {
				EjectFromWall();
			}

			// Move character with current platform
			if (currentPlatform != null) {
				// Set the oldPos platform on landing to advoid teleporting
				if (hangingFirstTime) {
					oldPosPlatform = currentPlatform.position;
					oldRotPlatform = currentPlatform.rotation;
				}

				Vector3 posDelta = currentPlatform.position - oldPosPlatform;
				Vector3 rotDelta = currentPlatform.rotation.eulerAngles - oldRotPlatform.eulerAngles;
				
				if (rotDelta.magnitude < maxPlatformRotationVelocity) {
					// Calculate the rotation realitive to origin, then translate it back to the players area
					Vector3 finalPlatformRot = RotatePoint(transform.position - currentPlatform.position, rotDelta) + currentPlatform.position;
					
					body.position = finalPlatformRot;
				}

				body.position += posDelta;
				
				// Set old values for calculating delta
				oldPosPlatform = currentPlatform.position;
				oldRotPlatform = currentPlatform.rotation;
				
				// If platform is moving over a certain speed, kick player off
				if (posDelta.magnitude >= maxPlatformVelocity) {
					currentPlatform = null;
				}
			}
			
			// Player can push off walls instead of pushing upwards always
			if (Input.GetButtonDown("Jump")) {
				velocity += Mathf.Clamp01(-inputAxis.y) * transform.TransformVector(pushOffVector);
				
				velocity += Mathf.Abs(Mathf.Abs(inputAxis.y) - 1.0f) * transform.TransformVector(Vector3.up * pushUpForce);
				velocity += Mathf.Clamp01(inputAxis.y) * transform.TransformVector(Vector3.up * pushUpForce);

				if (currentPlatform != null) {
					currentPlatform.AddForceAtPosition(-velocity * body.mass * newtonianMultiplier, transform.position - (Vector3.up * (GetComponent<CapsuleCollider>().height / 2)), ForceMode.Impulse);
				}
				
				EjectFromWall();
			}
			
			body.velocity = velocity;

			hangingFirstTime = false;
		}
	}

	void EjectFromWall () {
		body.useGravity = true;
		look.enabled = true;

		ledgeObject = null;
		ledgeHanging = false;
	}

	/*
	void OnGUI () {
		GUILayout.BeginArea(new Rect(10, 10, 400, Screen.height - 20.0f));

		GUILayout.Label(grounded.ToString());
		if (currentPlatform != null) {
			GUILayout.Label(currentPlatform.name.ToString());
		}

		GUILayout.EndArea();
	}
	*/

	void OnCollisionEnter (Collision collision) {
		collisionMagnitude = collision.relativeVelocity.magnitude;

		// Add all collision normals and normalize them
		Vector3 allCollisions = Vector3.zero;
		foreach (ContactPoint contact in collision.contacts) {
			allCollisions += contact.normal;
		}

		collisionNormal = allCollisions.normalized;

		if (collisionMagnitude >= minColMagnitude) {
			colliding = true;
			collisionStartTime = Time.time;
		}
	}

	/////////////////////////
	/// Utility stuff

	// Round vector3 to nearest 10th
	Vector3 Vector3Round (Vector3 a) {
		Vector3 finalVector;

		finalVector.x = Mathf.Round(a.x * 10.0f) / 10.0f;
		finalVector.y = Mathf.Round(a.y * 10.0f) / 10.0f;
		finalVector.z = Mathf.Round(a.z * 10.0f) / 10.0f;

		return finalVector;
	}

	// All the angles should be in degrees
	public Vector3 RotatePoint (Vector3 objectPosition, Vector3 rotations) {
		// Make all the matrices identity matrices so that we dont have to worry about the 1's in the final rotation matrix
		Matrix4x4 x = Matrix4x4.identity;
		Matrix4x4 y = Matrix4x4.identity;
		Matrix4x4 z = Matrix4x4.identity;
		
		// Get rid of that one 1 in the corner that nobody likes
		x[3, 3] = 0;
		y[3, 3] = 0;
		z[3, 3] = 0;
		
		// https://en.wikipedia.org/wiki/Rotation_matrix#In_three_dimensions
		
		/* Set the x rotation matrix
		 * | 1       0       0   |
		 * | 0    cos(x) -sin(x) |
		 * | 0    sin(x)  cos(x) |
		*/
		
		x[1, 1] = Mathf.Cos (rotations.x * Mathf.Deg2Rad);
		x[2, 1] = Mathf.Sin (rotations.x * Mathf.Deg2Rad);
		x[1, 2] = -Mathf.Sin (rotations.x * Mathf.Deg2Rad);
		x[2, 2] = Mathf.Cos(rotations.x * Mathf.Deg2Rad);
		
		/* Set the y rotation matrix
		 * | cos(y)  0    sin(y) |
		 * | 0       1      0    |
		 * | -sin(y) 0    cos(y) |
		*/
		
		y[0, 0] = Mathf.Cos (rotations.y * Mathf.Deg2Rad);
		y[0, 2] = Mathf.Sin (rotations.y * Mathf.Deg2Rad);
		y[2, 0] = -Mathf.Sin (rotations.y * Mathf.Deg2Rad);
		y[2, 2] = Mathf.Cos (rotations.y * Mathf.Deg2Rad);
		
		/* Set the y rotation matrix
		 * | cos(z) -sin(z)  0   |
		 * | sin(z) cos(z)   0   |
		 * | 0        0      1   |
		*/
		
		z[0, 0] = Mathf.Cos (rotations.z * Mathf.Deg2Rad);
		z[0, 1] = -Mathf.Sin (rotations.z * Mathf.Deg2Rad);
		z[1, 0] = Mathf.Sin (rotations.z * Mathf.Deg2Rad);
		z[1, 1] = Mathf.Cos (rotations.z * Mathf.Deg2Rad);
		
		// Multiply all the matrices
		Matrix4x4 finalRotation = x * y * z;
		
		// Rotate position around center
		Vector3 finalPosition = finalRotation.MultiplyPoint3x4(objectPosition);
		
		return finalPosition;
	}
}
