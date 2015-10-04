using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LedgeDetection : MonoBehaviour {
	
	public CharacterPhysics character;

	public Transform topCheck;
	public float topCheckLength = 2.0f;

	public float centerHeight = 0.5f;
	public float forwardStartDistance = 0.0f;
	public float forwardDistance = 2.0f;
	public float downwardHeight = 3.0f;
	public float downwardStartDistance = 2.0f;

	public float forwardDownOffset = 0.05f;

	public float maxAngle = 45.0f;

	void Start () {
		if (character == null) {
			character = transform.parent.GetComponent<CharacterPhysics>();
		}
	}

	void Update () {
		RaycastHit downwardHit;
		RaycastHit forwardHit;

		// The start point of this whole calcuation
		Vector3 downwardCheckPoint = character.transform.TransformPoint(new Vector3(0.0f, centerHeight + downwardHeight, downwardStartDistance));
		Vector3 forwardCheckPoint = character.transform.TransformPoint(new Vector3(0.0f, centerHeight, forwardStartDistance));

		if (!character.grounded) {
			if (Physics.Raycast(downwardCheckPoint, Vector3.down, out downwardHit, downwardHeight, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {

				Debug.DrawLine(forwardCheckPoint, forwardCheckPoint + (character.transform.forward * forwardDistance), Color.cyan);

				if (Physics.Raycast(forwardCheckPoint, character.transform.forward, out forwardHit, forwardDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {

					// Final normal to be used by the Character Physics
					Vector3 finalLedgeNormal = -Vector3.Cross(downwardHit.normal, forwardHit.normal);

					// Debug Stuff
					Debug.DrawLine(downwardCheckPoint, downwardHit.point, Color.red);

					Debug.DrawLine(downwardHit.point, downwardHit.point + (downwardHit.normal * 0.3f), Color.green);
					Debug.DrawLine(forwardHit.point, forwardHit.point + (forwardHit.normal * 0.3f), Color.magenta);

					Debug.DrawLine(forwardHit.point, forwardHit.point + (finalLedgeNormal * 1f), Color.white);
					// End Debug

					float angle = Vector3.Angle((downwardCheckPoint - downwardHit.point).normalized, downwardHit.normal);

					if (character.ledgeHanging) {
						character.ledgeNormal = finalLedgeNormal;
						character.ledgeCharacterDir = forwardHit.normal;
						character.ledgeObject = forwardHit.collider.gameObject;
					}

					if (angle <= maxAngle) {
						if (Input.GetAxis("Vertical") > 0.0f) {
							Rigidbody body = character.gameObject.GetComponent<Rigidbody>();
							Vector3 vel = character.transform.InverseTransformVector(body.velocity);

							if (Mathf.Sign(vel.y) > 0) {
								if (Input.GetButtonDown("Jump")) {
									character.pushOffLedge = true;
								}
							} else if (Mathf.Sign(vel.y) < 0) {
								RaycastHit topHit;
								if (Physics.Raycast(topCheck.position, topCheck.rotation * Vector3.forward, out topHit, topCheckLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) {
									Debug.DrawLine(topCheck.position, topCheck.position + (topCheck.rotation * Vector3.forward), Color.white);

									character.ledgeHanging = true;

									character.ledgeNormal = finalLedgeNormal;
									character.ledgeCharacterDir = forwardHit.normal;
									character.ledgeObject = topHit.collider.gameObject;

									body.useGravity = false;

									body.velocity = Vector3.zero;

									if (downwardHit.collider.attachedRigidbody != null) {
										character.currentPlatform = downwardHit.collider.attachedRigidbody;
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
