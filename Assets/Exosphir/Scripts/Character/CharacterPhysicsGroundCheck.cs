using UnityEngine;
using System.Collections;

public class CharacterPhysicsGroundCheck : MonoBehaviour {

	public CharacterPhysics player;

	GameObject lastGroundObject;

	void Start () {
		if (player == null) {
			player = transform.parent.GetComponent<CharacterPhysics>();
		}
	}

	void OnTriggerStay (Collider other) {
		if (other.transform != transform.parent) {
			player.grounded = true;
			lastGroundObject = other.gameObject;

			if (other.attachedRigidbody != null) {
				player.groundedObject = other.attachedRigidbody;
			}
		}
	}

	void OnTriggerExit (Collider other) {
		if (other.gameObject.GetInstanceID() == lastGroundObject.gameObject.GetInstanceID()) {
			player.grounded = false;

			player.groundedObject = null;
		}
	}
}
