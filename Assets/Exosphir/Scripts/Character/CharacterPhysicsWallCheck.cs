using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPhysicsWallCheck : MonoBehaviour {

	public CharacterPhysics player;

	List<GameObject> collidedObjects = new List<GameObject>();

	void Start () {
		if (player == null) {
			player = transform.parent.GetComponent<CharacterPhysics>();
		}
	}

	void OnTriggerEnter (Collider other) {
		collidedObjects.Add(other.gameObject);
	}

	void OnTriggerExit (Collider other) {
		for (int i = 0; i < collidedObjects.Count; i++) {
			if (collidedObjects[i] == other.gameObject) {
				collidedObjects.RemoveAt(i);
			}
		}
	}

	void Update () {
		if (collidedObjects.Count > 0) {
			player.isThereWall = true;
		} else {
			player.isThereWall = false;
		}
	}
}
