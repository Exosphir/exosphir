using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterPhysicsSurroundingsCheck : MonoBehaviour {

	public enum AreaToCheck {
		Ground,
		Sides
	};

	public AreaToCheck checkArea = AreaToCheck.Ground;

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
		bool colliding = (collidedObjects.Count > 0);

		switch (checkArea) {
		case AreaToCheck.Ground:
			player.grounded = colliding;
			break;

		case AreaToCheck.Sides:
			player.isThereWall = colliding;
			break;
		}
	}
}
