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

	[Header("Ledge Hanging")]
	public float maxPlatformVelocity = 7.0f;
	public float ledgeMoveVelocity = 1.0f;
	public float pushOffLedgeSpeed = 2.0f;
	public float pushUpForce = 3.0f;
	public Vector3 pushOffVector = new Vector3(0.0f, 1.0f, -1.0f);
	public Transform[] confirmLedgeHangingRaycasts;
	public float checkerLengths = 1.0f;
	public MouseLook look;

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
					Vector3 posDelta = currentPlatform.position - oldPosPlatform;
					body.position += posDelta;

					oldPosPlatform = currentPlatform.position;

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
				}
			} else {
				// Make current platform null
				currentPlatform = null;

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
		} else {
			look.enabled = false;

			// Set currentPlatform to ledgeObject if the ledgeObject has a rigidbody
			if (ledgeObject.GetComponent<Rigidbody>() != null) {
				currentPlatform = ledgeObject.GetComponent<Rigidbody>();
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
				Vector3 posDelta = currentPlatform.position - oldPosPlatform;
				body.position += posDelta;
				
				oldPosPlatform = currentPlatform.position;
				
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
				
				EjectFromWall();
			}
			
			body.velocity = velocity;
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

	// Round vector3 to nearest 10th
	Vector3 Vector3Round (Vector3 a) {
		Vector3 finalVector;

		finalVector.x = Mathf.Round(a.x * 10.0f) / 10.0f;
		finalVector.y = Mathf.Round(a.y * 10.0f) / 10.0f;
		finalVector.z = Mathf.Round(a.z * 10.0f) / 10.0f;

		return finalVector;
	}
}
